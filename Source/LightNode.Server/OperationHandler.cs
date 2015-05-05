using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.IO;
using System.Linq.Expressions;
using LightNode.Core;
using LightNode.Diagnostics;

namespace LightNode.Server
{
    internal class OperationHandler
    {
        readonly static Dictionary<Type, Func<object, object>> taskResultExtractors = new Dictionary<Type, Func<object, object>>();

        public string ClassName { get; private set; }
        public string MethodName { get; private set; }

        public ParameterInfoSlim[] Arguments { get; private set; }
        public IReadOnlyList<string> ParameterNames { get; private set; }

        public Type ReturnType { get; private set; }

        public ILookup<Type, Attribute> AttributeLookup { get; private set; }

        // internal use
        internal AcceptVerbs AcceptVerb { get; private set; }
        internal IContentFormatter ForceUseFormatter { get; private set; }

        readonly LightNodeFilterAttribute[] filters;

        // formatter cache
        readonly IContentFormatter[] optionFormatters;
        readonly ILookup<string, IContentFormatter> formatterByExt;
        readonly ILookup<string, IContentFormatter> formatterByMediaType;
        readonly ILookup<string, IContentFormatter> formatterByContentEncoding;

        readonly HandlerBodyType handlerBodyType;

        // MethodCache Delegate => environment, arguments, returnType

        readonly Func<IDictionary<string, object>, object[], object> methodFuncBody;

        readonly Func<IDictionary<string, object>, object[], Task> methodAsyncFuncBody;

        readonly Action<IDictionary<string, object>, object[]> methodActionBody;

        readonly Func<IDictionary<string, object>, object[], Task> methodAsyncActionBody;

        public OperationHandler(ILightNodeOptions options, Type classType, MethodInfo methodInfo)
        {
            this.ClassName = classType.Name;
            this.MethodName = methodInfo.Name;
            this.Arguments = methodInfo.GetParameters()
                .Select(x => new ParameterInfoSlim(x))
                .ToArray();
            this.ParameterNames = Arguments.Select(x => x.Name).ToList().AsReadOnly();
            this.ReturnType = methodInfo.ReturnType;

            this.filters = options.Filters
                .Concat(classType.GetCustomAttributes<LightNodeFilterAttribute>(true))
                .Concat(methodInfo.GetCustomAttributes<LightNodeFilterAttribute>(true))
                .OrderBy(x => x.Order)
                .ToArray();

            var operationOption = methodInfo.GetCustomAttributes<OperationOptionAttribute>(true).FirstOrDefault();
            this.AcceptVerb = (operationOption != null && operationOption.AcceptVerbs != null)
                ? operationOption.AcceptVerbs.Value
                : options.DefaultAcceptVerb;

            var verbSpecifiedAttr = methodInfo.GetCustomAttributes<HttpVerbAttribtue>(true).FirstOrDefault();
            if (verbSpecifiedAttr != null)
            {
                this.AcceptVerb = verbSpecifiedAttr.AcceptVerbs;
            }

            this.ForceUseFormatter = (operationOption != null && operationOption.ContentFormatter != null)
                ? operationOption.ContentFormatter
                : null;
            var formatterChoiceBase = new[] { options.DefaultFormatter }.Concat(options.SpecifiedFormatters).Where(x => x != null).ToArray();
            this.optionFormatters = formatterChoiceBase;
            this.formatterByExt = formatterChoiceBase.SelectMany(x => (x.Ext ?? "").Split('|'), (fmt, ext) => new { fmt, ext }).ToLookup(x => x.ext, x => x.fmt, StringComparer.OrdinalIgnoreCase);
            this.formatterByMediaType = formatterChoiceBase.ToLookup(x => x.MediaType, StringComparer.OrdinalIgnoreCase);
            this.formatterByContentEncoding = formatterChoiceBase.ToLookup(x => x.ContentEncoding, StringComparer.OrdinalIgnoreCase);

            this.AttributeLookup = classType.GetCustomAttributes(true)
                .Concat(methodInfo.GetCustomAttributes(true))
                .Cast<Attribute>()
                .ToLookup(x => x.GetType());

            foreach (var argument in this.Arguments)
            {
                if (!TypeBinder.IsAllowType(argument.ParameterType))
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

        // Accept, Accept-Encoding flow
        internal IContentFormatter NegotiateFormat(IDictionary<string, object> environment, string ext, ILightNodeOptions options, IOperationCoordinator coorinator)
        {
            var requestHeader = environment["owin.RequestHeaders"] as IDictionary<string, string[]>;

            string[] accepts;
            if (ForceUseFormatter != null) return ForceUseFormatter;
            if (!string.IsNullOrWhiteSpace(ext))
            {
                // Ext match -> ContentEncoding match
                var selectedFormatters = formatterByExt[ext] as ICollection<IContentFormatter> ?? formatterByExt[ext].ToArray();
                if (!selectedFormatters.Any())
                {
                    coorinator.OnProcessInterrupt(options, environment, InterruptReason.NegotiateFormatFailed, "Ext:" + ext);
                    options.Logger.NegotiateFormatFailed(OperationMissingKind.NegotiateFormatFailed, ext);
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

                return SelectAcceptEncodingFormatter(requestHeader, selectedFormatters);
            }
            else if (requestHeader.TryGetValue("Accept", out accepts))
            {
                if (optionFormatters.Length == 1) return options.DefaultFormatter; // optimize path, defaultFormatter only

                // MediaType match -> ContentEncoding match
                var acceptsValues = GetDescendingQualityHeaderValues(accepts);
                var formatters = acceptsValues.SelectMany(x => formatterByMediaType[x.Item3]).ToArray();

                if (formatters.Length == 0)
                {
                    // only accept-encoding
                    goto CONTENT_ENCODING_MATCH;
                }

                return SelectAcceptEncodingFormatter(requestHeader, formatters);
            }

            // ContentEncoding match
            CONTENT_ENCODING_MATCH:
            {
                if (optionFormatters.Length == 1) return options.DefaultFormatter; // optimize path, defaultFormatter only

                // ContentEncoding match
                string[] rawAcceptEncoding;
                if (!requestHeader.TryGetValue("Accept-Encoding", out rawAcceptEncoding))
                {
                    return options.DefaultFormatter;
                }

                var acceptEncodings = GetDescendingQualityHeaderValues(rawAcceptEncoding);
                var formatter = acceptEncodings
                    .Select(kvp => formatterByContentEncoding[kvp.Item3].FirstOrDefault())
                    .FirstOrDefault(x => x != null);

                if (formatter == null) return options.DefaultFormatter;
                return formatter;
            }
        }

        static IContentFormatter SelectAcceptEncodingFormatter(IDictionary<string, string[]> requestHeader, ICollection<IContentFormatter> selectedFormatters)
        {
            if (selectedFormatters.Count == 1) return selectedFormatters.First();

            string[] rawAcceptEncoding;
            if (!requestHeader.TryGetValue("Accept-Encoding", out rawAcceptEncoding))
            {
                return selectedFormatters.First();
            }

            var acceptEncodings = GetDescendingQualityHeaderValues(rawAcceptEncoding);
            var formatter = acceptEncodings
                .Select(kvp => selectedFormatters.FirstOrDefault(x => kvp.Item3.Equals(x.ContentEncoding, StringComparison.OrdinalIgnoreCase)))
                .FirstOrDefault(x => x != null);

            if (formatter == null) return selectedFormatters.First();
            return formatter;
        }

        // Tuple<index, q, name>
        static List<Tuple<int, double, string>> GetDescendingQualityHeaderValues(string[] rawHeaders)
        {
            var list = new List<Tuple<int, double, string>>(rawHeaders.Length);
            var index = 0;
            foreach (var item in rawHeaders)
            {
                var splitted = item.Split(';');
                if (splitted.Length == 1)
                {
                    list.Add(Tuple.Create(index++, 1.0, splitted[0].Trim()));
                }
                else if (splitted.Length == 2)
                {
                    var name = splitted[0].Trim();
                    var rawQ = splitted[1];
                    var q = 1.0;
                    var qSplitted = rawQ.Split('=');
                    if (qSplitted.Length == 2 && qSplitted[0].Trim().Equals("q", StringComparison.InvariantCultureIgnoreCase) && double.TryParse(qSplitted[1], out q))
                    {
                        list.Add(Tuple.Create(index++, q, name));
                    }
                    else
                    {
                        list.Add(Tuple.Create(index++, 1.0, name));
                    }
                }
            }

            // needs stable sort, compare quality first, second compare index
            list.Sort((a, b) =>
            {
                int c = b.Item2.CompareTo(a.Item2);
                if (c != 0) return c;

                return a.Item1.CompareTo(b.Item1);
            });

            return list;
        }

        public Task Execute(ILightNodeOptions options, OperationContext context, IOperationCoordinator coordinator)
        {
            var targetFilters = coordinator.GetFilters(options, context, filters);

            return InvokeRecursive(-1, targetFilters, options, context, coordinator);
        }

        Task InvokeRecursive(int index, IReadOnlyList<LightNodeFilterAttribute> filters, ILightNodeOptions options, OperationContext context, IOperationCoordinator coordinator)
        {
            index += 1;
            if (filters.Count != index)
            {
                // chain next filter
                return filters[index].Invoke(context, () => InvokeRecursive(index, filters, options, context, coordinator));
            }
            else
            {
                // execute operation
                return coordinator.ExecuteOperation(options, context, ExecuteOperation);
            }
        }

        async Task<object> ExecuteOperation(ILightNodeOptions options, OperationContext context)
        {
            // prepare
            var handler = this;
            var environment = context.Environment;
            var methodParameters = (object[])context.Parameters;

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
                if (!string.IsNullOrWhiteSpace(context.ContentFormatter.ContentEncoding))
                {
                    responseHeader["Content-Encoding"] = new[] { context.ContentFormatter.ContentEncoding };
                }
                environment.EmitOK();

                var responseStream = environment["owin.ResponseBody"] as Stream;
                if (options.StreamWriteOption == StreamWriteOption.DirectWrite)
                {
                    context.ContentFormatter.Serialize(new UnclosableStream(responseStream), result);
                }
                else
                {
                    using (var buffer = new MemoryStream())
                    {
                        context.ContentFormatter.Serialize(new UnclosableStream(buffer), result);
                        responseHeader["Content-Length"] = new[] { buffer.Position.ToString() };
                        buffer.Position = 0;
                        if (options.StreamWriteOption == StreamWriteOption.BufferAndWrite)
                        {
                            buffer.CopyTo(responseStream); // not CopyToAsync
                        }
                        else
                        {
                            await buffer.CopyToAsync(responseStream).ConfigureAwait(false);
                        }
                    }
                }

                return result;
            }
            else
            {
                environment.EmitNoContent();
                return null;
            }
        }
    }

    public class ParameterInfoSlim
    {
        public Type ParameterType { get; private set; }
        public bool ParameterTypeIsArray { get; private set; }
        public bool ParameterTypeIsClass { get; private set; }
        public bool ParameterTypeIsString { get; private set; }
        public bool ParameterTypeIsNullable { get; private set; }

        public string Name { get; private set; }
        public bool IsOptional { get; private set; }
        public object DefaultValue { get; private set; }

        internal ParameterInfoSlim(ParameterInfo parameterInfo)
        {
            Name = parameterInfo.Name;
            DefaultValue = parameterInfo.DefaultValue;
            IsOptional = parameterInfo.IsOptional;
            ParameterType = parameterInfo.ParameterType;
            ParameterTypeIsArray = parameterInfo.ParameterType.IsArray;
            ParameterTypeIsClass = parameterInfo.ParameterType.IsClass;
            ParameterTypeIsString = parameterInfo.ParameterType == typeof(string);
            ParameterTypeIsNullable = parameterInfo.ParameterType.IsNullable();
        }
    }

    internal enum HandlerBodyType
    {
        // 0 is invalid

        Func = 1,
        AsyncFunc = 2,
        Action = 3,
        AsyncAction = 4
    }

    public class OperationInfo
    {
        public string ClassName { get; private set; }
        public string MethodName { get; private set; }
        public AcceptVerbs AcceptVerbs { get; private set; }
        public ParameterInfoSlim[] Parameters { get; private set; }
        public Type ReturnType { get; private set; }
        internal IContentFormatter ForceUseFormatter { get; private set; }

        internal OperationInfo(OperationHandler handler)
        {
            this.ClassName = handler.ClassName;
            this.MethodName = handler.MethodName;
            this.AcceptVerbs = handler.AcceptVerb;
            this.Parameters = handler.Arguments;
            this.ReturnType = handler.ReturnType;
            this.ForceUseFormatter = handler.ForceUseFormatter;
        }
    }
}