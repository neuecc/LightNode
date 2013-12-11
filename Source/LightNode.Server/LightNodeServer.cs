using System;
using System.Linq.Expressions;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.IO;

namespace LightNode.Server
{
    public static class LightNodeServer
    {
        // {Class,Method} => MessageContract
        readonly static Dictionary<Tuple<string, string>, OperationHandler> handlers = new Dictionary<Tuple<string, string>, OperationHandler>();
        readonly static Dictionary<Type, Func<object, object>> taskResultExtractorCache = new Dictionary<Type, Func<object, object>>();

        public static void RegisterHandler(Assembly[] hostAssemblies)
        {
            var contractTypes = hostAssemblies
                .SelectMany(x => x.GetTypes())
                .Where(x => typeof(ILightNodeContract).IsAssignableFrom(x));

            // TODO:validation, duplicate entry, non support arguments, append attribute.

            Parallel.ForEach(contractTypes, classType =>
            {
                var className = classType.Name;
                foreach (var methodInfo in classType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                {
                    var handler = new OperationHandler();

                    var methodName = methodInfo.Name;

                    handler.MethodName = methodName;
                    handler.Arguments = methodInfo.GetParameters();
                    handler.ReturnType = methodInfo.ReturnType;

                    if (handler.Arguments.All(x => AllowRequestType.IsAllowType(x.ParameterType)))
                    {
                        throw new InvalidOperationException(); // TODO:error message, parameter is not allow.
                    }

                    if (typeof(Task).IsAssignableFrom(handler.ReturnType))
                    {
                        // (object[] args) => new X().M((T1)args[0], (T2)args[1])...
                        var args = Expression.Parameter(typeof(object[]), "args");

                        var parameters = methodInfo.GetParameters()
                            .Select((x, i) => Expression.Convert(Expression.ArrayIndex(args, Expression.Constant(i)), x.ParameterType))
                            .ToArray();

                        var lambda = Expression.Lambda<Func<object[], Task>>(
                            Expression.Call(
                                Expression.New(classType),
                                methodInfo,
                                parameters),
                            args);

                        if (handler.ReturnType.IsGenericType && handler.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                        {
                            handler.HandlerBodyType = HandlerBodyType.AsyncFunc;
                            handler.MethodAsyncFuncBody = lambda.Compile();

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
                            lock (taskResultExtractorCache)
                            {
                                // safe duplicate entry
                                taskResultExtractorCache[handler.ReturnType] = compiledResultLambda;
                            }
                        }
                        else
                        {
                            handler.HandlerBodyType = HandlerBodyType.AsyncAction;
                            handler.MethodAsyncActionBody = lambda.Compile();
                        }
                    }
                    else if (handler.ReturnType == typeof(void))
                    {
                        // (object[] args) => { new X().M((T1)args[0], (T2)args[1])... }
                        var args = Expression.Parameter(typeof(object[]), "args");

                        var parameters = methodInfo.GetParameters()
                            .Select((x, i) => Expression.Convert(Expression.ArrayIndex(args, Expression.Constant(i)), x.ParameterType))
                            .ToArray();

                        var lambda = Expression.Lambda<Action<object[]>>(
                            Expression.Call(
                                Expression.New(classType),
                                methodInfo,
                                parameters),
                            args);

                        handler.HandlerBodyType = HandlerBodyType.Action;
                        handler.MethodActionBody = lambda.Compile();
                    }
                    else
                    {
                        // (object[] args) => (object)new X().M((T1)args[0], (T2)args[1])...
                        var args = Expression.Parameter(typeof(object[]), "args");

                        var parameters = methodInfo.GetParameters()
                            .Select((x, i) => Expression.Convert(Expression.ArrayIndex(args, Expression.Constant(i)), x.ParameterType))
                            .ToArray();

                        var lambda = Expression.Lambda<Func<object[], object>>(
                            Expression.Convert(
                                Expression.Call(
                                    Expression.New(classType),
                                    methodInfo,
                                    parameters)
                            , typeof(object)),
                            args);

                        handler.HandlerBodyType = HandlerBodyType.Func;
                        handler.MethodFuncBody = lambda.Compile();
                    }

                    lock (handlers)
                    {
                        // fail duplicate entry
                        handlers.Add(Tuple.Create(className, methodName), handler);
                    }
                }
            });
        }

        public static async Task HandleRequest(IDictionary<string, object> environment)
        {
            var path = environment["owin.RequestPath"] as string;

            // TODO:extract "extension" for media type
            var keyBase = path.Trim('/').Split('/');
            if (keyBase.Length != 2) throw new InvalidOperationException(); // TODO:Exception Handling

            // {ClassName, MethodName}
            var key = Tuple.Create(keyBase[0], keyBase[1]);

            OperationHandler handler;
            if (handlers.TryGetValue(key, out handler))
            {
                ILookup<string, string> requestParameter;
                // TODO:GET is from QueryString
                using (var sr = new StreamReader((environment["owin.RequestBody"] as Stream)))
                {
                    var str = await sr.ReadToEndAsync();
                    requestParameter = str.Split('&')
                        .Select(xs => xs.Split('='))
                        .Where(xs => xs.Length == 2)
                        .ToLookup(xs => xs[0], xs => xs[1]);
                }

                var methodParameters = handler.Arguments.Select(x =>
                {
                    var values = requestParameter[x.Name];
                    var count = values.Count();
                    if (count == 0)
                    {
                        if (x.IsOptional)
                        {
                            return x.DefaultValue;
                        }
                        else
                        {
                            throw new InvalidOperationException(); // TODO:Exception Handling
                        }
                    }
                    else if (count == 1)
                    {
                        var conv = AllowRequestType.GetConverter(x.ParameterType);
                        if (conv == null) throw new InvalidOperationException(); // TODO:Exception Handling
                        return conv(values.First());
                    }
                    else // Array
                    {
                        if (!x.ParameterType.IsArray) throw new InvalidOperationException(); // TODO:Exception Handling
                        var conv = AllowRequestType.GetArrayConverter(x.ParameterType);
                        if (conv == null) throw new InvalidOperationException(); // TODO:Exception Handling
                        return conv(values);
                    }
                })
                .ToArray();

                bool isVoid = true;
                object result = null;
                switch (handler.HandlerBodyType)
                {
                    case HandlerBodyType.Action:
                        handler.MethodActionBody(methodParameters);
                        break;
                    case HandlerBodyType.Func:
                        isVoid = false;
                        result = handler.MethodFuncBody(methodParameters);
                        break;
                    case HandlerBodyType.AsyncAction:
                        var actionTask = handler.MethodAsyncActionBody(methodParameters);
                        await actionTask;
                        break;
                    case HandlerBodyType.AsyncFunc:
                        isVoid = false;
                        var funcTask = handler.MethodAsyncFuncBody(methodParameters);
                        await funcTask;
                        var extractor = taskResultExtractorCache[funcTask.GetType()];
                        result = extractor(funcTask);
                        break;
                    default:
                        throw new InvalidOperationException("critical:register code is broken");
                }

                // TODO:
                // set response
                // exception handling
            }
            else
            {
                // TODO:return 404 Message
            }
        }
    }



    public interface ILightNodeContract
    {

    }


    public class ContractOptionAttribute : Attribute
    {
        public string Name { get; private set; }

        public AcceptVerbs AcceptVerb { get; set; }
    }

    [Flags]
    public enum AcceptVerbs
    {
        Get, Post
    }

    public interface ISerializer
    {

    }



}