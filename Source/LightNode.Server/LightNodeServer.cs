using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace LightNode.Server
{
    public static class LightNodeServer
    {
        static bool registered = false;

        // {Class,Method} => Func<object>
        readonly static ConcurrentDictionary<Tuple<string, string>, Func<object>> handlers = new ConcurrentDictionary<Tuple<string, string>, Func<object>>();
        readonly static ConcurrentDictionary<Tuple<string, string>, Func<Task<object>>> asyncHandlers = new ConcurrentDictionary<Tuple<string, string>, Func<Task<object>>>();

        // initialize at once.
        public static void RegisterHandler(Assembly[] hostAssemblies)
        {
            if (registered)
            {
                // TODO: throw exception
            }
            else
            {
                registered = true;
            }

            //hostAssemblies.SelectMany(x => x.GetTypes())
            //    .Where(x => x.IsAssignableFrom(typeof(ILightNodeContract)))
            //    .Select(x =>
            //    {


            //    });
        }

        public static async Task HandleRequest(IDictionary<string, object> environment)
        {
            var path = environment["owin.Request...."];
            // URL Trim

            // TODO:get path & classname
            var key = Tuple.Create("MyClass", "Hello");

            Func<object> handler;
            if (handlers.TryGetValue(key, out handler))
            {
                // get handler

                // get parameter

                // invoker handler

                // set response

            }
            else
            {
                // throw exception
            }
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