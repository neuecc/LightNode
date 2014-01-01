using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LightNode.Server
{
    public static class LightNodeServer
    {
        readonly static Dictionary<RequestPath, OperationHandler> handlers = new Dictionary<RequestPath, OperationHandler>();
        readonly static Dictionary<Type, Func<object, object>> taskResultExtractors = new Dictionary<Type, Func<object, object>>();

        static LightNodeOptions options;

        static int alreadyRegistered = -1;

        public static void RegisterOptions(LightNodeOptions options)
        {
            LightNodeServer.options = options;
        }

        public static void RegisterHandler(Assembly[] hostAssemblies)
        {
            if (Interlocked.Increment(ref alreadyRegistered) != 0) return;

            var contractTypes = hostAssemblies
                .SelectMany(x => x.GetTypes())
                .Where(x => typeof(LightNodeContract).IsAssignableFrom(x));

            Parallel.ForEach(contractTypes, classType =>
            {
                var className = classType.Name;
                foreach (var methodInfo in classType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (methodInfo.IsSpecialName && (methodInfo.Name.StartsWith("set_") || methodInfo.Name.StartsWith("get_"))) continue; // as property

                    var methodName = methodInfo.Name;

                    // ignore default methods
                    if (methodName == "Equals"
                     || methodName == "GetHashCode"
                     || methodName == "GetType"
                     || methodName == "ToString")
                    {
                        continue;
                    }

                    var handler = new OperationHandler();

                    handler.MethodName = methodName;
                    handler.Arguments = methodInfo.GetParameters()
                        .Select(x => new ParameterInfoSlim
                        {
                            Name = x.Name,
                            DefaultValue = x.DefaultValue,
                            IsOptional = x.IsOptional,
                            ParameterType = x.ParameterType,
                            ParameterTypeIsArray = x.ParameterType.IsArray,
                            ParameterTypeIsClass = x.ParameterType.IsClass,
                            ParameterTypeIsString = x.ParameterType == typeof(string),
                            ParameterTypeIsNullable = x.ParameterType.IsNullable()
                        })
                        .ToArray();
                    handler.ReturnType = methodInfo.ReturnType;

                    foreach (var argument in handler.Arguments)
                    {
                        if (!AllowRequestType.IsAllowType(argument.ParameterType))
                        {
                            throw new InvalidOperationException(string.Format("parameter is not allowed, class:{0} method:{1} paramName:{2} paramType:{3}",
                                className, methodName, argument.Name, argument.ParameterType.FullName));
                        }
                    }

                    // prepare lambda parameters
                    var envArg = Expression.Parameter(typeof(IDictionary<string, object>), "environment");
                    var envBind = Expression.Bind(typeof(LightNodeContract).GetProperty("Environment"), envArg);
                    var args = Expression.Parameter(typeof(object[]), "args");
                    var parameters = methodInfo.GetParameters()
                        .Select((x, i) => Expression.Convert(Expression.ArrayIndex(args, Expression.Constant(i)), x.ParameterType))
                        .ToArray();

                    // Task or Task<T>
                    if (typeof(Task).IsAssignableFrom(handler.ReturnType))
                    {
                        // (object[] args) => new X().M((T1)args[0], (T2)args[1])...
                        var lambda = Expression.Lambda<Func<IDictionary<string, object>, object[], Task>>(
                            Expression.Call(
                                Expression.MemberInit(Expression.New(classType), envBind),
                                methodInfo,
                                parameters),
                            envArg, args);

                        if (handler.ReturnType.IsGenericType && handler.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                        {
                            handler.HandlerBodyType = HandlerBodyType.AsyncFunc;
                            handler.MethodAsyncFuncBody = lambda.Compile();

                            lock (taskResultExtractors)
                            {
                                if (!taskResultExtractors.ContainsKey(handler.ReturnType))
                                {
                                    // (object task) => (object)((Task<>).Result)
                                    var taskParameter = Expression.Parameter(typeof(object), "task");
                                    var resultLambda = Expression.Lambda<Func<object, object>>(
                                        Expression.Convert(
                                            Expression.Property(
                                                Expression.Convert(taskParameter, handler.ReturnType),
                                                "Result"),
                                            typeof(object)),
                                        taskParameter);

                                    var compiledResultLambda = resultLambda.Compile();

                                    taskResultExtractors[handler.ReturnType] = compiledResultLambda;
                                }
                            }
                        }
                        else
                        {
                            handler.HandlerBodyType = HandlerBodyType.AsyncAction;
                            handler.MethodAsyncActionBody = lambda.Compile();
                        }
                    }
                    else if (handler.ReturnType == typeof(void)) // of course void
                    {
                        // (object[] args) => { new X().M((T1)args[0], (T2)args[1])... }
                        var lambda = Expression.Lambda<Action<IDictionary<string, object>, object[]>>(
                            Expression.Call(
                                Expression.MemberInit(Expression.New(classType), envBind),
                                methodInfo,
                                parameters),
                            envArg, args);

                        handler.HandlerBodyType = HandlerBodyType.Action;
                        handler.MethodActionBody = lambda.Compile();
                    }
                    else // return T
                    {
                        // (object[] args) => (object)new X().M((T1)args[0], (T2)args[1])...
                        var lambda = Expression.Lambda<Func<IDictionary<string, object>, object[], object>>(
                            Expression.Convert(
                                Expression.Call(
                                    Expression.MemberInit(Expression.New(classType), envBind),
                                    methodInfo,
                                    parameters)
                            , typeof(object)),
                            envArg, args);

                        handler.HandlerBodyType = HandlerBodyType.Func;
                        handler.MethodFuncBody = lambda.Compile();
                    }

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
                }
            });
        }

        // TODO:very long method, refactoring...
        public static async Task ProcessRequest(IDictionary<string, object> environment)
        {
            try
            {
                var path = environment["owin.RequestPath"] as string;

                // verb check
                var method = environment["owin.RequestMethod"];
                var verb = AcceptVerbs.Get;
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
                    EmitMethodNotAllowed(environment);
                    return;
                }

                var keyBase = path.Trim('/').Split('/');
                if (keyBase.Length != 2)
                {
                    EmitNotFound(environment);
                    return;
                }

                // extract "extension" for media type
                var extStart = keyBase[1].LastIndexOf(".");
                var ext = "";
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
                    if (!options.DefaultAcceptVerb.HasFlag(verb))
                    {
                        EmitMethodNotAllowed(environment);
                        return;
                    }

                    // Extract parameter
                    ILookup<string, string> requestParameter;
                    var queryString = environment["owin.RequestQueryString"] as string;
                    using (var sr = new StreamReader((environment["owin.RequestBody"] as Stream)))
                    {
                        var str = sr.ReadToEnd();
                        requestParameter = str.Split('&')
                            .Concat(queryString.Split('&'))
                            .Select(xs => xs.Split('='))
                            .Where(xs => xs.Length == 2)
                            .ToLookup(xs => Uri.UnescapeDataString(xs[0]), xs => Uri.UnescapeDataString(xs[1]), StringComparer.OrdinalIgnoreCase);
                    }

                    // Parameter binding
                    var methodParameters = new object[handler.Arguments.Length];
                    for (int i = 0; i < handler.Arguments.Length; i++)
                    {
                        var item = handler.Arguments[i];

                        var values = requestParameter[item.Name];
                        var count = values.Count();
                        if (count == 0 && !item.ParameterTypeIsArray)
                        {
                            if (item.IsOptional)
                            {
                                methodParameters[i] = item.DefaultValue;
                                continue;
                            }
                            else if ((!item.ParameterTypeIsString || options.ParameterStringAllowsNull) && (item.ParameterTypeIsClass || item.ParameterTypeIsNullable))
                            {
                                methodParameters[i] = null;
                                continue;
                            }
                            else
                            {
                                EmitBadRequest(environment);
                                await EmitStringMessage(environment, "Lack of Parameter:" + item.Name).ConfigureAwait(false);
                                return;
                            }
                        }
                        else if (!item.ParameterTypeIsArray)
                        {
                            var conv = AllowRequestType.GetConverter(item.ParameterType);
                            if (conv == null) throw new InvalidOperationException("critical:register code is broken");

                            object pValue;
                            if (conv(values.First(), out pValue))
                            {
                                methodParameters[i] = pValue;
                                continue;
                            }
                            else if (item.IsOptional)
                            {
                                methodParameters[i] = item.DefaultValue;
                                continue;
                            }
                            else if ((!item.ParameterTypeIsString || options.ParameterStringAllowsNull) && (item.ParameterTypeIsClass || item.ParameterTypeIsNullable))
                            {
                                methodParameters[i] = null;
                                continue;
                            }
                            else
                            {
                                EmitBadRequest(environment);
                                await EmitStringMessage(environment, "Mismatch Parameter Type:" + item.Name).ConfigureAwait(false);
                                return;
                            }
                        }

                        var arrayConv = AllowRequestType.GetArrayConverter(item.ParameterType);
                        if (arrayConv == null) throw new InvalidOperationException("critical:register code is broken");

                        methodParameters[i] = arrayConv(values);
                        continue;
                    }

                    // Operation execute
                    bool isVoid = true;
                    object result = null;
                    switch (handler.HandlerBodyType)
                    {
                        case HandlerBodyType.Action:
                            handler.MethodActionBody(environment, methodParameters);
                            break;
                        case HandlerBodyType.Func:
                            isVoid = false;
                            result = handler.MethodFuncBody(environment, methodParameters);
                            break;
                        case HandlerBodyType.AsyncAction:
                            var actionTask = handler.MethodAsyncActionBody(environment, methodParameters);
                            await actionTask.ConfigureAwait(false);
                            break;
                        case HandlerBodyType.AsyncFunc:
                            isVoid = false;
                            var funcTask = handler.MethodAsyncFuncBody(environment, methodParameters);
                            await funcTask.ConfigureAwait(false);
                            var extractor = taskResultExtractors[funcTask.GetType()];
                            result = extractor(funcTask);
                            break;
                        default:
                            throw new InvalidOperationException("critical:register code is broken");
                    }

                    if (!isVoid)
                    {
                        var requestHeader = environment["owin.RequestHeaders"] as IDictionary<string, string[]>;
                        string[] accepts;

                        // select formatter
                        var formatter = options.DefaultFormatter;
                        if (ext != "")
                        {
                            formatter = new[] { options.DefaultFormatter }.Concat(options.SpecifiedFormatters)
                                .FirstOrDefault(x => x.Ext == ext);

                            if (formatter == null)
                            {
                                EmitNotAcceptable(environment);
                                return;
                            }
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

                        // append header
                        var responseHeader = environment["owin.ResponseHeaders"] as IDictionary<string, string[]>;
                        responseHeader["Content-Type"] = new[] { formatter.MediaType };
                        EmitOK(environment);

                        var responseStream = environment["owin.ResponseBody"] as Stream;
                        formatter.Serialize(new UnflushableStream(responseStream), result);

                        return;
                    }
                    else
                    {
                        EmitNoContent(environment);
                        return;
                    }
                }
                else
                {
                    EmitNotFound(environment);
                    return;
                }
            }
            catch (ReturnStatusCodeException statusException)
            {
                statusException.EmitCode(environment);
                return;
            }
            catch
            {
                EmitInternalServerError(environment);
                throw;
            }
        }

        static Task EmitStringMessage(IDictionary<string, object> environment, string message)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            return (environment["owin.ResponseBody"] as Stream).WriteAsync(bytes, 0, bytes.Length);
        }

        static void EmitOK(IDictionary<string, object> environment)
        {
            environment["owin.ResponseStatusCode"] = (int)System.Net.HttpStatusCode.OK; // 200
        }

        static void EmitNoContent(IDictionary<string, object> environment)
        {
            environment["owin.ResponseStatusCode"] = (int)System.Net.HttpStatusCode.NoContent; // 204
        }

        static void EmitBadRequest(IDictionary<string, object> environment)
        {
            environment["owin.ResponseStatusCode"] = (int)System.Net.HttpStatusCode.BadRequest; // 400
        }

        static void EmitNotFound(IDictionary<string, object> environment)
        {
            environment["owin.ResponseStatusCode"] = (int)System.Net.HttpStatusCode.NotFound; // 404
        }

        static void EmitMethodNotAllowed(IDictionary<string, object> environment)
        {
            environment["owin.ResponseStatusCode"] = (int)System.Net.HttpStatusCode.MethodNotAllowed; // 405
        }

        static void EmitNotAcceptable(IDictionary<string, object> environment)
        {
            environment["owin.ResponseStatusCode"] = (int)System.Net.HttpStatusCode.NotAcceptable; // 406
        }

        static void EmitUnsupportedMediaType(IDictionary<string, object> environment)
        {
            environment["owin.ResponseStatusCode"] = (int)System.Net.HttpStatusCode.UnsupportedMediaType; // 415
        }

        static void EmitInternalServerError(IDictionary<string, object> environment)
        {
            environment["owin.ResponseStatusCode"] = (int)System.Net.HttpStatusCode.InternalServerError; // 500
        }
    }
}