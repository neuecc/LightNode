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
        readonly static Dictionary<Type, Func<object, object>> taskResultExtractorCache = new Dictionary<Type, Func<object, object>>();

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

            // TODO:validation, duplicate entry, non support arguments, append attribute.

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
                    handler.Arguments = methodInfo.GetParameters();
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
                    var envArg = Expression.Parameter(typeof(IDictionary<string, object>), "Environment");
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

                            lock (taskResultExtractorCache)
                            {
                                if (!taskResultExtractorCache.ContainsKey(handler.ReturnType))
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

                                    taskResultExtractorCache[handler.ReturnType] = compiledResultLambda;
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
                        handlers.Add(new RequestPath(className, methodName), handler);
                    }
                }
            });
        }

        public static async Task ProcessRequest(IDictionary<string, object> environment)
        {
            try
            {
                var path = environment["owin.RequestPath"] as string;

                // TODO:extract "extension" for media type
                var keyBase = path.Trim('/').Split('/');
                if (keyBase.Length != 2)
                {
                    EmitNotFound(environment);
                    return;
                }

                // {ClassName, MethodName}
                var key = new RequestPath(keyBase[0], keyBase[1]);

                OperationHandler handler;
                if (handlers.TryGetValue(key, out handler))
                {
                    ILookup<string, string> requestParameter;
                    var queryString = environment["owin.RequestQueryString"] as string;
                    using (var sr = new StreamReader((environment["owin.RequestBody"] as Stream)))
                    {
                        var str = sr.ReadToEnd();
                        requestParameter = str.Split('&')
                            .Concat(queryString.Split('&'))
                            .Select(xs => xs.Split('='))
                            .Where(xs => xs.Length == 2)
                            .ToLookup(xs => xs[0], xs => xs[1]);
                    }

                    var methodParameters = new object[handler.Arguments.Length];
                    for (int i = 0; i < handler.Arguments.Length; i++)
                    {
                        var item = handler.Arguments[i];

                        var values = requestParameter[item.Name];
                        var count = values.Count();
                        if (count == 0)
                        {
                            if (item.IsOptional)
                            {
                                methodParameters[i] = item.DefaultValue;
                                continue;
                            }
                            else if (item.ParameterType.IsClass || (item.ParameterType.IsGenericType && item.ParameterType.GetGenericTypeDefinition() == typeof(Nullable<>)))
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
                        else if (count == 1)
                        {
                            if (!item.ParameterType.IsArray)
                            {
                                var conv = AllowRequestType.GetConverter(item.ParameterType);
                                if (conv == null) throw new InvalidOperationException("critical:register code is broken");

                                object pValue;
                                if (conv(values.First(), out pValue))
                                {
                                    methodParameters[i] = pValue;
                                    continue;
                                }
                                else if (item.ParameterType.IsClass || (item.ParameterType.IsGenericType && item.ParameterType.GetGenericTypeDefinition() == typeof(Nullable<>)))
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
                        }

                        // Array
                        if (!item.ParameterType.IsArray)
                        {
                            EmitBadRequest(environment);
                            await EmitStringMessage(environment, "Parameter Type is not array but accepted value is array:" + item.Name).ConfigureAwait(false);
                            return;
                        }
                        var arrayConv = AllowRequestType.GetArrayConverter(item.ParameterType);
                        if (arrayConv == null) throw new InvalidOperationException("critical:register code is broken");

                        methodParameters[i] = arrayConv(values);
                    }

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
                            var extractor = taskResultExtractorCache[funcTask.GetType()];
                            result = extractor(funcTask);
                            break;
                        default:
                            throw new InvalidOperationException("critical:register code is broken");
                    }

                    if (!isVoid)
                    {
                        var responseStream = environment["owin.ResponseBody"] as Stream;

                        // select formatter
                        options.DefaultFormatter.Serialize(responseStream, result);

                        // append header
                    }

                    EmitOK(environment);
                    return;
                }
                else
                {
                    EmitNotFound(environment);
                    return;
                }
            }
            catch
            {
                // TODO:exception handling
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

        static void EmitBadRequest(IDictionary<string, object> environment)
        {
            environment["owin.ResponseStatusCode"] = (int)System.Net.HttpStatusCode.BadRequest; // 400
        }

        static void EmitNotFound(IDictionary<string, object> environment)
        {
            environment["owin.ResponseStatusCode"] = (int)System.Net.HttpStatusCode.NotFound; // 404
        }

        static void EmitNotAcceptable(IDictionary<string, object> environment)
        {
            environment["owin.ResponseStatusCode"] = (int)System.Net.HttpStatusCode.NotAcceptable; // 406
        }

        static void EmitUnsupportedMediaType(IDictionary<string, object> environment)
        {
            environment["owin.ResponseStatusCode"] = (int)System.Net.HttpStatusCode.UnsupportedMediaType; // 415
        }
    }
}