using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.IO;
using System.Linq.Expressions;
using LightNode.Core;

namespace LightNode.Server
{
    internal class OperationHandler
    {
        readonly static Dictionary<Type, Func<object, object>> taskResultExtractors = new Dictionary<Type, Func<object, object>>();

        public string ClassName { get; private set; }
        public string MethodName { get; private set; }

        public ParameterInfoSlim[] Arguments { get; private set; }

        public Type ReturnType { get; private set; }

        public ILookup<Type, Attribute> AttributeLookup { get; private set; }

        readonly LightNodeFilterAttribute[] filters;

        readonly HandlerBodyType handlerBodyType;

        // MethodCache Delegate => environment, arguments, returnType

        readonly Func<IDictionary<string, object>, object[], object> methodFuncBody;

        readonly Func<IDictionary<string, object>, object[], Task> methodAsyncFuncBody;

        readonly Action<IDictionary<string, object>, object[]> methodActionBody;

        readonly Func<IDictionary<string, object>, object[], Task> methodAsyncActionBody;

        public OperationHandler(LightNodeOptions options, Type classType, MethodInfo methodInfo)
        {
            this.ClassName = classType.Name;
            this.MethodName = methodInfo.Name;
            this.Arguments = methodInfo.GetParameters()
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
            this.ReturnType = methodInfo.ReturnType;

            this.filters = options.Filters
                .Concat(classType.GetCustomAttributes<LightNodeFilterAttribute>(true))
                .Concat(methodInfo.GetCustomAttributes<LightNodeFilterAttribute>(true))
                .OrderBy(x => x.Order)
                .ToArray();

            this.AttributeLookup = classType.GetCustomAttributes(true)
                .Concat(methodInfo.GetCustomAttributes(true))
                .Cast<Attribute>()
                .ToLookup(x => x.GetType());

            foreach (var argument in this.Arguments)
            {
                if (!AllowRequestType.IsAllowType(argument.ParameterType))
                {
                    throw new InvalidOperationException(string.Format("parameter is not allowed, class:{0} method:{1} paramName:{2} paramType:{3}",
                        classType.Name, methodInfo.Name, argument.Name, argument.ParameterType.FullName));
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
            if (typeof(Task).IsAssignableFrom(this.ReturnType))
            {
                // (object[] args) => new X().M((T1)args[0], (T2)args[1])...
                var lambda = Expression.Lambda<Func<IDictionary<string, object>, object[], Task>>(
                    Expression.Call(
                        Expression.MemberInit(Expression.New(classType), envBind),
                        methodInfo,
                        parameters),
                    envArg, args);

                if (this.ReturnType.IsGenericType && this.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    this.handlerBodyType = HandlerBodyType.AsyncFunc;
                    this.methodAsyncFuncBody = lambda.Compile();

                    lock (taskResultExtractors)
                    {
                        if (!taskResultExtractors.ContainsKey(this.ReturnType))
                        {
                            // (object task) => (object)((Task<>).Result)
                            var taskParameter = Expression.Parameter(typeof(object), "task");
                            var resultLambda = Expression.Lambda<Func<object, object>>(
                                Expression.Convert(
                                    Expression.Property(
                                        Expression.Convert(taskParameter, this.ReturnType),
                                        "Result"),
                                    typeof(object)),
                                taskParameter);

                            var compiledResultLambda = resultLambda.Compile();

                            taskResultExtractors[this.ReturnType] = compiledResultLambda;
                        }
                    }
                }
                else
                {
                    this.handlerBodyType = HandlerBodyType.AsyncAction;
                    this.methodAsyncActionBody = lambda.Compile();
                }
            }
            else if (this.ReturnType == typeof(void)) // of course void
            {
                // (object[] args) => { new X().M((T1)args[0], (T2)args[1])... }
                var lambda = Expression.Lambda<Action<IDictionary<string, object>, object[]>>(
                    Expression.Call(
                        Expression.MemberInit(Expression.New(classType), envBind),
                        methodInfo,
                        parameters),
                    envArg, args);

                this.handlerBodyType = HandlerBodyType.Action;
                this.methodActionBody = lambda.Compile();
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

                this.handlerBodyType = HandlerBodyType.Func;
                this.methodFuncBody = lambda.Compile();
            }
        }

        public Task Execute(LightNodeOptions options, OperationContext context)
        {
            int index = -1;
            Func<Task> invokeRecursive = null;
            invokeRecursive = () =>
            {
                index += 1;
                if (filters.Length != index)
                {
                    // chain next filter
                    return filters[index].Invoke(context, invokeRecursive);
                }
                else
                {
                    // execute operation
                    return ExecuteOperation(options, context);
                }
            };
            return invokeRecursive();
        }

        async Task ExecuteOperation(LightNodeOptions options, OperationContext context)
        {
            // prepare
            var handler = this;
            var environment = context.Environment;
            var methodParameters = context.Parameters;


            bool isVoid = true;
            object result = null;
            switch (handler.handlerBodyType)
            {
                case HandlerBodyType.Action:
                    handler.methodActionBody(environment, methodParameters);
                    break;
                case HandlerBodyType.Func:
                    isVoid = false;
                    result = handler.methodFuncBody(environment, methodParameters);
                    break;
                case HandlerBodyType.AsyncAction:
                    var actionTask = handler.methodAsyncActionBody(environment, methodParameters);
                    await actionTask.ConfigureAwait(false);
                    break;
                case HandlerBodyType.AsyncFunc:
                    isVoid = false;
                    var funcTask = handler.methodAsyncFuncBody(environment, methodParameters);
                    await funcTask.ConfigureAwait(false);
                    var extractor = taskResultExtractors[funcTask.GetType()];
                    result = extractor(funcTask);
                    break;
                default:
                    throw new InvalidOperationException("critical:register code is broken");
            }

            if (!isVoid)
            {
                // append header
                var responseHeader = environment["owin.ResponseHeaders"] as IDictionary<string, string[]>;
                var encoding = context.ContentFormatter.Encoding;
                responseHeader["Content-Type"] = new[] { context.ContentFormatter.MediaType + ((encoding == null) ? "" : "; charset=" + encoding.WebName) };
                environment.EmitOK();

                var responseStream = environment["owin.ResponseBody"] as Stream;
                if (options.BufferContentBeforeWrite)
                {
                    using (var buffer = new MemoryStream())
                    {
                        context.ContentFormatter.Serialize(new UnclosableStream(buffer), result);
                        responseHeader["Content-Length"] = new[] { buffer.Position.ToString() };
                        buffer.Position = 0;
                        await buffer.CopyToAsync(responseStream).ConfigureAwait(false);
                    }
                }
                else
                {
                    context.ContentFormatter.Serialize(new UnclosableStream(responseStream), result);
                }

                return;
            }
            else
            {
                environment.EmitNoContent();
                return;
            }
        }
    }

    internal class ParameterInfoSlim
    {
        public Type ParameterType { get; set; }
        public bool ParameterTypeIsArray { get; set; }
        public bool ParameterTypeIsClass { get; set; }
        public bool ParameterTypeIsString { get; set; }
        public bool ParameterTypeIsNullable { get; set; }

        public string Name { get; set; }
        public bool IsOptional { get; set; }
        public object DefaultValue { get; set; }
    }

    internal enum HandlerBodyType
    {
        // 0 is invalid

        Func = 1,
        AsyncFunc = 2,
        Action = 3,
        AsyncAction = 4
    }
}