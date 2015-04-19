using LightNode.Server;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using LightNode.Diagnostics;
using System.Diagnostics;
using System.Linq;

namespace LightNode.Server
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class LightNodeServerMiddleware
    {
        static readonly object runningHandlerLock = new object();
        static ILookup<string, IReadOnlyCollection<KeyValuePair<string, OperationInfo>>> runningHandlers =
            Enumerable.Empty<IReadOnlyCollection<KeyValuePair<string, OperationInfo>>>().ToLookup(x => "", x => x);

        /// <summary>
        /// Get all registered handlers. Key is ILightNodeOptions.ServerEngineId.
        /// </summary>
        public static ILookup<string, IReadOnlyCollection<KeyValuePair<string, OperationInfo>>> GetRegisteredHandlersInfo()
        {
            return runningHandlers;
        }

        readonly LightNodeServer engine;
        readonly bool useOtherMiddleware;
        readonly AppFunc next;

        public LightNodeServerMiddleware(AppFunc next, ILightNodeOptions options)
            : this(next, options, AppDomain.CurrentDomain.GetAssemblies())
        {
        }

        public LightNodeServerMiddleware(AppFunc next, ILightNodeOptions options, Assembly[] hostAssemblies)
        {
            this.next = next;
            this.useOtherMiddleware = options.UseOtherMiddleware;
            this.engine = new LightNodeServer(options);

            var sw = Stopwatch.StartNew();
            var registeredHandler = this.engine.RegisterHandler(hostAssemblies);
            LightNodeEventSource.Log.InitializeComplete(sw.Elapsed.TotalMilliseconds);

            lock (runningHandlerLock)
            {
                runningHandlers = runningHandlers.SelectMany(g => g, (g, xs) => new { g.Key, xs })
                    .Concat(new[] { new { Key = options.ServerEngineId, xs = registeredHandler } })
                    .ToLookup(x => x.Key, x => x.xs);
            }
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
            return UseLightNode(app, new LightNodeOptions());
        }

        public static IAppBuilder UseLightNode(this IAppBuilder app, ILightNodeOptions options)
        {
            return app.Use(typeof(LightNodeServerMiddleware), options);
        }

        public static IAppBuilder UseLightNode(this IAppBuilder app, ILightNodeOptions options, params Assembly[] hostAssemblies)
        {
            return app.Use(typeof(LightNodeServerMiddleware), options, hostAssemblies);
        }
    }
}