using LightNode.Server;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace LightNode.Server
{
    using AppFunc = Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;

    public class LightNodeServerMiddleware
    {
        readonly AppFunc next;

        public LightNodeServerMiddleware(AppFunc next)
            : this(next, Assembly.GetEntryAssembly())
        {
        }

        public LightNodeServerMiddleware(AppFunc next, params Assembly[] hostAssemblies)
        {
            this.next = next;
            LightNodeServer.RegisterHandler(hostAssemblies);
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            await LightNodeServer.HandleRequest(environment);
            await next(environment);
        }
    }
}

namespace Owin
{
    public static class AppBuilderLightNodeMiddlewareExtensions
    {
        public static IAppBuilder UseLightNode(this IAppBuilder app)
        {
            return app.Use(typeof(LightNodeServerMiddleware));
        }
        public static IAppBuilder UseLightNode(this IAppBuilder app, params Assembly[] hostAssemblies)
        {
            return app.Use(typeof(LightNodeServerMiddleware), hostAssemblies);
        }
    }
}