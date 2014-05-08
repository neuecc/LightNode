using LightNode.Server;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace LightNode.Server
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class LightNodeServerMiddleware
    {
        readonly LightNodeServer engine;
        readonly bool useOtherMiddleware;
        readonly AppFunc next;

        public LightNodeServerMiddleware(AppFunc next, LightNodeOptions options)
            : this(next, options, AppDomain.CurrentDomain.GetAssemblies())
        {
        }

        public LightNodeServerMiddleware(AppFunc next, LightNodeOptions options, Assembly[] hostAssemblies)
        {
            this.next = next;
            this.useOtherMiddleware = options.UseOtherMiddleware;
            this.engine = new LightNodeServer(options);
            this.engine.RegisterHandler(hostAssemblies);
        }

        public async Task Invoke(IDictionary<string, object> environment)
        {
            if (useOtherMiddleware)
            {
                await engine.ProcessRequest(environment).ConfigureAwait(true); // keep context
                await next(environment).ConfigureAwait(false);
            }
            else
            {
                await engine.ProcessRequest(environment).ConfigureAwait(false);
            }
        }
    }
}

namespace Owin
{
    public static class AppBuilderLightNodeMiddlewareExtensions
    {
        public static IAppBuilder UseLightNode(this IAppBuilder app)
        {
            return UseLightNode(app, new LightNodeOptions(AcceptVerbs.Get | AcceptVerbs.Post, new LightNode.Formatter.JavaScriptContentFormatter()));
        }

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