using System;
using System.Reflection;
using System.Threading.Tasks;

namespace LightNode.Server
{
    internal class OperationHandler
    {
        public string MethodName { get; set; }

        public ParameterInfo[] Arguments { get; set; }

        public Type ReturnType { get; set; }

        public HandlerBodyType HandlerBodyType { get; set; }

        public Func<object[], object> MethodFuncBody { get; set; } // 1

        public Func<object[], Task> MethodAsyncFuncBody { get; set; } // 2

        public Action<object[]> MethodActionBody { get; set; } // 3

        public Func<object[], Task> MethodAsyncActionBody { get; set; } // 4
    }

    internal enum HandlerBodyType
    {
        Func = 1,
        AsyncFunc = 2,
        Action = 3,
        AsyncAction = 4
    }
}