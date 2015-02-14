using LightNode.Core;
using LightNode.Diagnostics;
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

        readonly LightNodeOptions options;

        int alreadyRegistered = -1;

        public LightNodeServer(LightNodeOptions options)
        {
            this.options = options;
        }

        // cache all methods
        public void RegisterHandler(Assembly[] hostAssemblies)
        {
            if (Interlocked.Increment(ref alreadyRegistered) != 0) return;

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
                .Where(x => !x.IsAbstract);

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
                    LightNodeEventSource.Log.RegisiterOperation(handler.ClassName, handler.MethodName, sw.Elapsed.TotalMilliseconds);
                }
            });
        }

        OperationHandler SelectHandler(IDictionary<string, object> environment, out AcceptVerbs verb, out string ext)
        {
            // out default
            verb = AcceptVerbs.Get;
            ext = "";
            var path = environment["owin.RequestPath"] as string;
            var method = environment["owin.RequestMethod"] as string;

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
            LightNodeEventSource.Log.MethodNotAllowed(OperationMissingKind.MethodNotAllowed, path, method);
            if (options.OperationMissingHandlingPolicy == OperationMissingHandlingPolicy.ThrowException)
            {
                throw new MethodNotAllowdException(OperationMissingKind.MethodNotAllowed, path, method);
            }
            else
            {
                environment.EmitMethodNotAllowed();
                if (options.OperationMissingHandlingPolicy == OperationMissingHandlingPolicy.ReturnErrorStatusCodeIncludeErrorDetails)
                {
                    environment.EmitStringMessage("MethodNotAllowed:" + method);
                }
                return null;
            }

        NOT_FOUND:
            LightNodeEventSource.Log.OperationNotFound(OperationMissingKind.OperationNotFound, path);
            if (options.OperationMissingHandlingPolicy == OperationMissingHandlingPolicy.ThrowException)
            {
                throw new OperationNotFoundException(OperationMissingKind.MethodNotAllowed, path);
            }
            else
            {
                environment.EmitNotFound();
                if (options.OperationMissingHandlingPolicy == OperationMissingHandlingPolicy.ReturnErrorStatusCodeIncludeErrorDetails)
                {
                    environment.EmitStringMessage("OperationNotFound:" + path);
                }
                return null;
            }
        }

        IContentFormatter NegotiateFormat(IDictionary<string, object> environment, string ext)
        {
            var requestHeader = environment["owin.RequestHeaders"] as IDictionary<string, string[]>;
            string[] accepts;

            var formatter = options.DefaultFormatter;
            if (ext != "")
            {
                // TODO:need performance improvement
                var selectedFormatter = new[] { options.DefaultFormatter }.Concat(options.SpecifiedFormatters)
                    .SelectMany(x => (x.Ext ?? "").Split('|'), (fmt, xt) => new { fmt, xt })
                    .FirstOrDefault(x => x.xt == ext);

                if (selectedFormatter == null)
                {
                    LightNodeEventSource.Log.NegotiateFormatFailed(OperationMissingKind.NegotiateFormatFailed, ext);
                    if (options.OperationMissingHandlingPolicy == OperationMissingHandlingPolicy.ThrowException)
                    {
                        throw new NegotiateFormatFailedException(OperationMissingKind.NegotiateFormatFailed, ext);
                    }
                    else
                    {
                        environment.EmitNotAcceptable();
                        if (options.OperationMissingHandlingPolicy == OperationMissingHandlingPolicy.ReturnErrorStatusCodeIncludeErrorDetails)
                        {
                            environment.EmitStringMessage("NegotiateFormat failed, ext:" + ext);
                        }
                    }
                    return null;
                }
                formatter = selectedFormatter.fmt;
            }
            else if (requestHeader.TryGetValue("Accept", out accepts))
            {
                // TODO:parse accept header q, */*, etc...
                var contentType = accepts[0];
                formatter = new[] { options.DefaultFormatter }.Concat(options.SpecifiedFormatters)
                    .FirstOrDefault(x => contentType.Contains(x.MediaType));

                if (formatter == null)
                {
                    formatter = options.DefaultFormatter; // through...
                }
            }

            return formatter;
        }

        // Routing -> ParameterBinding -> Execute
        public async Task ProcessRequest(IDictionary<string, object> environment)
        {
            AcceptVerbs verb;
            string ext;
            var handler = SelectHandler(environment, out verb, out ext);
            if (handler == null) return;

            // Parameter binding
            var valueProvider = new ValueProvider(environment, verb);
            var methodParameters = ParameterBinder.BindParameter(environment, options, valueProvider, handler.Arguments);
            if (methodParameters == null) return;

            // select formatter
            var formatter = handler.ForceUseFormatter;
            if (formatter == null)
            {
                formatter = NegotiateFormat(environment, ext);
                if (formatter == null) return;
            }

            try
            {
                // Operation execute
                var context = new OperationContext(environment, handler.ClassName, handler.MethodName, verb)
                {
                    Parameters = methodParameters,
                    ParameterNames = handler.ParameterNames,
                    ContentFormatter = formatter,
                    Attributes = handler.AttributeLookup
                };
                await handler.Execute(options, context).ConfigureAwait(false);
                return;
            }
            catch (ReturnStatusCodeException statusException)
            {
                statusException.EmitCode(environment);
                return;
            }
            catch (Exception ex)
            {
                switch (options.ErrorHandlingPolicy)
                {
                    case ErrorHandlingPolicy.ReturnInternalServerError:
                        environment.EmitInternalServerError();
                        environment.EmitStringMessage("500 InternalServerError");
                        return;
                    case ErrorHandlingPolicy.ReturnInternalServerErrorIncludeErrorDetails:
                        environment.EmitInternalServerError();
                        environment.EmitStringMessage(ex.ToString());
                        return;
                    case ErrorHandlingPolicy.ThrowException:
                    default:
                        throw;
                }
            }
        }
    }
}