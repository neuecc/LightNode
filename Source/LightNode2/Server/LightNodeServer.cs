using LightNode.Core;
using LightNode.Diagnostics;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LightNode.Server
{
    internal class LightNodeServer
    {
        readonly Dictionary<RequestPath, OperationHandler> handlers = new Dictionary<RequestPath, OperationHandler>();

        readonly ILightNodeOptions options;

        int alreadyRegistered = -1;

        public LightNodeServer(ILightNodeOptions options)
        {
            this.options = options;
        }

        // cache all methods
        public IReadOnlyCollection<KeyValuePair<string, OperationInfo>> RegisterHandler(Assembly[] hostAssemblies)
        {
            if (Interlocked.Increment(ref alreadyRegistered) != 0) return new KeyValuePair<string, OperationInfo>[0];

            var contractTypes = hostAssemblies
                .SelectMany(x =>
                {
                    try
                    {
                        return x.GetTypes();
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        return ex.Types.Where(t => t != null);
                    }
                })
                .Where(x => typeof(LightNodeContract).IsAssignableFrom(x))
                .Where(x => !x.IsAbstract());

            Parallel.ForEach(contractTypes, classType =>
            {
                var className = classType.Name;
                if (!classType.GetConstructors().Any(x => x.GetParameters().Length == 0))
                {
                    throw new InvalidOperationException(string.Format("Type needs parameterless constructor, class:{0}", classType.FullName));
                }
                if (classType.GetCustomAttribute<IgnoreOperationAttribute>(true) != null) return; // ignore

                foreach (var methodInfo in classType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (methodInfo.IsSpecialName && (methodInfo.Name.StartsWith("set_") || methodInfo.Name.StartsWith("get_"))) continue; // as property
                    if (methodInfo.GetCustomAttribute<IgnoreOperationAttribute>(true) != null) continue; // ignore

                    var methodName = methodInfo.Name;

                    // ignore default methods
                    if (methodName == "Equals"
                     || methodName == "GetHashCode"
                     || methodName == "GetType"
                     || methodName == "ToString")
                    {
                        continue;
                    }

                    var sw = Stopwatch.StartNew();

                    // create handler
                    var handler = new OperationHandler(options, classType, methodInfo);
                    lock (handlers)
                    {
                        // fail duplicate entry
                        var path = new RequestPath(className, methodName);
                        if (handlers.ContainsKey(path))
                        {
                            throw new InvalidOperationException(string.Format("same class and method is not allowed, class:{0} method:{1}", className, methodName));
                        }
                        handlers.Add(path, handler);
                    }

                    sw.Stop();
                    options.Logger.RegisiterOperation(handler.ClassName, handler.MethodName, sw.Elapsed.TotalMilliseconds);
                }
            });

            // return readonly operation info
            return handlers.Select(x => new KeyValuePair<string, OperationInfo>(x.Key.ToString(), new OperationInfo(x.Value))).ToList().AsReadOnly();
        }

        OperationHandler SelectHandler(HttpContext httpContext, IOperationCoordinator coorinator, out AcceptVerbs verb, out string ext)
        {
            // out default
            verb = AcceptVerbs.Get;
            ext = "";
            var path = httpContext.Request.Path.Value;
            var method = httpContext.Request.Method;

            // extract path
            var keyBase = path.Trim('/').Split('/');
            if (keyBase.Length != 2)
            {
                goto NOT_FOUND;
            }

            // extract "extension" for media type
            var extStart = keyBase[1].LastIndexOf(".");
            if (extStart != -1)
            {
                ext = keyBase[1].Substring(extStart + 1);
                keyBase[1] = keyBase[1].Substring(0, keyBase[1].Length - ext.Length - 1);
            }

            // {ClassName, MethodName}
            var key = new RequestPath(keyBase[0], keyBase[1]);

            OperationHandler handler;
            if (handlers.TryGetValue(key, out handler))
            {
                // verb check
                if (StringComparer.OrdinalIgnoreCase.Equals(method, "GET"))
                {
                    verb = AcceptVerbs.Get;
                }
                else if (StringComparer.OrdinalIgnoreCase.Equals(method, "POST"))
                {
                    verb = AcceptVerbs.Post;
                }
                else if (StringComparer.OrdinalIgnoreCase.Equals(method, "PUT"))
                {
                    verb = AcceptVerbs.Put;
                }
                else if (StringComparer.OrdinalIgnoreCase.Equals(method, "DELETE"))
                {
                    verb = AcceptVerbs.Delete;
                }
                else if (StringComparer.OrdinalIgnoreCase.Equals(method, "PATCH"))
                {
                    verb = AcceptVerbs.Patch;
                }
                else
                {
                    goto VERB_MISSING;
                }

                if (!handler.AcceptVerb.HasFlag(verb))
                {
                    goto VERB_MISSING;
                }

                return handler; // OK
            }
            else
            {
                goto NOT_FOUND;
            }

            VERB_MISSING:
            coorinator.OnProcessInterrupt(options, httpContext, InterruptReason.MethodNotAllowed, "MethodName:" + method);
            options.Logger.MethodNotAllowed(OperationMissingKind.MethodNotAllowed, path, method);
            if (options.OperationMissingHandlingPolicy == OperationMissingHandlingPolicy.ThrowException)
            {
                throw new MethodNotAllowedException(OperationMissingKind.MethodNotAllowed, path, method);
            }
            else
            {
                httpContext.EmitMethodNotAllowed();
                if (options.OperationMissingHandlingPolicy == OperationMissingHandlingPolicy.ReturnErrorStatusCodeIncludeErrorDetails)
                {
                    httpContext.EmitStringMessage("MethodNotAllowed:" + method);
                }
                return null;
            }

            NOT_FOUND:
            coorinator.OnProcessInterrupt(options, httpContext, InterruptReason.OperationNotFound, "SearchedPath:" + path);
            options.Logger.OperationNotFound(OperationMissingKind.OperationNotFound, path);
            if (options.OperationMissingHandlingPolicy == OperationMissingHandlingPolicy.ThrowException)
            {
                throw new OperationNotFoundException(OperationMissingKind.MethodNotAllowed, path);
            }
            else
            {
                httpContext.EmitNotFound();
                if (options.OperationMissingHandlingPolicy == OperationMissingHandlingPolicy.ReturnErrorStatusCodeIncludeErrorDetails)
                {
                    httpContext.EmitStringMessage("OperationNotFound:" + path);
                }
                return null;
            }
        }

        // Routing -> ParameterBinding -> Execute
        public async Task ProcessRequest(HttpContext httpContext)
        {
            options.Logger.ProcessRequestStart(httpContext.Request.Path);

            MemoryStream bufferedRequestStream = null;
            var originalRequestStream = httpContext.Request.Body;
            if (!originalRequestStream.CanSeek)
            {
                bufferedRequestStream = new MemoryStream();
                if (options.StreamWriteOption == StreamWriteOption.BufferAndAsynchronousWrite)
                {
                    await originalRequestStream.CopyToAsync(bufferedRequestStream); // keep context
                }
                else
                {
                    originalRequestStream.CopyTo(bufferedRequestStream);
                }
                bufferedRequestStream.Position = 0;
                httpContext.Request.Body = bufferedRequestStream;
            }
            try
            {
                var coordinator = options.OperationCoordinatorFactory.Create();
                if (!coordinator.OnStartProcessRequest(options, httpContext))
                {
                    return;
                }

                AcceptVerbs verb;
                string ext;
                var handler = SelectHandler(httpContext, coordinator, out verb, out ext);
                if (handler == null) return;

                // Parameter binding
                var valueProvider = new ValueProvider(httpContext, verb);
                var methodParameters = ParameterBinder.BindParameter(httpContext, options, coordinator, valueProvider, handler.Arguments);
                if (methodParameters == null) return;

                // select formatter
                var formatter = handler.NegotiateFormat(httpContext, ext, options, coordinator);
                if (formatter == null)
                {
                    if (formatter == null) return;
                }

                try
                {
                    // Operation execute
                    var context = new OperationContext(httpContext, handler.ClassName, handler.MethodName, verb)
                    {
                        Parameters = methodParameters,
                        ParameterNames = handler.ParameterNames,
                        ContentFormatter = formatter,
                        Attributes = handler.AttributeLookup
                    };
                    var executionPath = context.ToString();
                    options.Logger.ExecuteStart(executionPath);
                    var sw = Stopwatch.StartNew();
                    var interrupted = false;
                    try
                    {
                        await handler.Execute(options, context, coordinator).ConfigureAwait(false);
                    }
                    catch
                    {
                        interrupted = true;
                        throw;
                    }
                    finally
                    {
                        sw.Stop();
                        options.Logger.ExecuteFinished(executionPath, interrupted, sw.Elapsed.TotalMilliseconds);
                    }
                    return;
                }
                catch (ReturnStatusCodeException statusException)
                {
                    try
                    {
                        var code = (int)statusException.StatusCode;
                        for (int i = 0; i < options.PassThroughWhenStatusCodesAre.Length; i++)
                        {
                            if (code == (int)options.PassThroughWhenStatusCodesAre[i])
                            {
                                httpContext.Response.StatusCode = code;
                                return;
                            }
                        }

                        statusException.EmitCode(options, httpContext);
                    }
                    catch (Exception ex)
                    {
                        if (IsRethrowOrEmitException(coordinator, options, httpContext, ex))
                        {
                            throw;
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (IsRethrowOrEmitException(coordinator, options, httpContext, ex))
                    {
                        throw;
                    }
                }
            }
            finally
            {
                if (bufferedRequestStream != null)
                {
                    bufferedRequestStream.Dispose();
                }
                httpContext.Request.Body = originalRequestStream;
            }
        }

        static bool IsRethrowOrEmitException(IOperationCoordinator coordinator, ILightNodeOptions options, HttpContext httpContext, Exception ex)
        {
            var exString = ex.ToString();
            coordinator.OnProcessInterrupt(options, httpContext, InterruptReason.ExecuteFailed, exString);
            switch (options.ErrorHandlingPolicy)
            {
                case ErrorHandlingPolicy.ReturnInternalServerError:
                    httpContext.EmitInternalServerError();
                    httpContext.EmitStringMessage("500 InternalServerError");
                    return false;
                case ErrorHandlingPolicy.ReturnInternalServerErrorIncludeErrorDetails:
                    httpContext.EmitInternalServerError();
                    httpContext.EmitStringMessage(exString);
                    return false;
                case ErrorHandlingPolicy.ThrowException:
                default:
                    httpContext.EmitInternalServerError();
                    return true;
            }
        }
    }
}