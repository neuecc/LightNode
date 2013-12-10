using System;
using System.Linq.Expressions;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace LightNode.Server
{
    internal class MessageContract
    {
        public string MethodName { get; set; }

        public ParameterInfo[] Arguments { get; set; }

        public Type ReturnType { get; set; }

        public MessageContractBodyType MessageContractBodyType { get; set; }

        public Func<object[], object> MethodFuncBody { get; set; } // 1

        public Func<object[], Task<object>> MethodAsyncFuncBody { get; set; } // 2

        public Action<object[]> MethodActionBody { get; set; } // 3
        public Func<object[], Task> MethodAsyncActionBody { get; set; } // 4
    }
    internal enum MessageContractBodyType
    {
        Func = 1,
        AsyncFunc = 2,
        Action = 3,
        AsyncAction = 4
    }


    public static class LightNodeServer
    {
        // {Class,Method} => MessageContract
        readonly static Dictionary<Tuple<string, string>, MessageContract> handlers = new Dictionary<Tuple<string, string>, MessageContract>();

        public static void RegisterHandler(Assembly[] hostAssemblies)
        {
            var contractTypes = hostAssemblies
                .SelectMany(x => x.GetTypes())
                .Where(x => typeof(ILightNodeContract).IsAssignableFrom(x));

            foreach (var classType in contractTypes)
            {
                var className = classType.Name;
                foreach (var methodInfo in classType.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                {
                    var contract = new MessageContract();

                    var methodName = methodInfo.Name;

                    contract.MethodName = methodName;
                    contract.Arguments = methodInfo.GetParameters();
                    contract.ReturnType = methodInfo.ReturnType;

                    if (contract.ReturnType == typeof(Task))
                    {
                        contract.MessageContractBodyType = MessageContractBodyType.AsyncAction;

                        var args = methodInfo.GetParameters().Select(x => Expression.Parameter(x.ParameterType, x.Name)).ToArray();
                        var objectArgs = args.Select(x => Expression.Parameter(typeof(object), x.Name)).ToArray();
                        var expr = Expression.Lambda<Func<object[], Task>>(
                            Expression.Call(
                                Expression.New(classType),
                                methodInfo,
                                objectArgs.Select((x, i) => Expression.Convert(x, args[i].Type)).ToArray()),
                            objectArgs);
                        

                        var v = expr.Compile();



                        var _ = v(new object[] { 10 });

                        //contract.MethodAsyncActionBody = expr.Compile();

                        //handlers.Add(Tuple.Create(className, methodName), contract);
                    }
                    else if (contract.ReturnType == typeof(void))
                    {
                        contract.MessageContractBodyType = MessageContractBodyType.Action;
                    }
                    else if (contract.ReturnType.IsGenericType && contract.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                    {
                        contract.MessageContractBodyType = MessageContractBodyType.AsyncFunc;
                    }
                    else
                    {
                        contract.MessageContractBodyType = MessageContractBodyType.Func;
                    }
                }
            }
        }

        public static async Task HandleRequest(IDictionary<string, object> environment)
        {
            var path = environment["owin.Request...."];
            // URL Trim

            // TODO:get path & classname
            var key = Tuple.Create("MyClass", "Hello");

            Func<object> handler;
            //if (handlers.TryGetV)
            //{
            // get handler

            // get parameter

            // invoker handler

            // set response

            //            }
            //          else
            //        {
            // throw exception
            //      }
        }
    }



    public interface ILightNodeContract
    {

    }


    public class ContractOptionAttribute : Attribute
    {
        public string Name { get; private set; }


    }

    public interface ISerializer
    {

    }



}