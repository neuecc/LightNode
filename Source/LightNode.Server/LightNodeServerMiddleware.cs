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

        public LightNodeServerMiddleware(AppFunc next, LightNodeOptions options)
            : this(next, options, new[] { Assembly.GetEntryAssembly() })
        {
        }

        public LightNodeServerMiddleware(AppFunc next, LightNodeOptions options, Assembly[] hostAssemblies)
        {
            this.next = next;
            LightNodeServer.RegisterOptions(options);
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
        public static IAppBuilder UseLightNode(this IAppBuilder app, LightNodeOptions options)
        {
            return app.Use(typeof(LightNodeServerMiddleware), options);
        }
        public static IAppBuilder UseLightNode(this IAppBuilder app, LightNodeOptions options, params Assembly[] hostAssemblies)
        {
            return app.Use(typeof(LightNodeServerMiddleware), options, hostAssemblies);
        }
    }
}