using LightNode.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace LightNode
{
    public static class ApplicationBuilderLightNodeMiddlewareExtensions
    {
        public static IApplicationBuilder UseLightNode(this IApplicationBuilder app, Type hostAssemblyIncludingType)
        {
            return UseLightNode(app, new LightNodeOptions(), new[] { hostAssemblyIncludingType.GetTypeInfo().Assembly });
        }

        public static IApplicationBuilder UseLightNode(this IApplicationBuilder app, Assembly[] hostAssembly)
        {
            return UseLightNode(app, new LightNodeOptions(), hostAssembly);
        }

        public static IApplicationBuilder UseLightNode(this IApplicationBuilder app, ILightNodeOptions options, Assembly[] hostAssemblies)
        {
            return app.UseMiddleware<LightNodeServerMiddleware>(options, hostAssemblies);
        }
    }
}

namespace LightNode.Server
{
    public class RegisteredHandlersInfo
    {
        public string EngineId { get; private set; }
        public ILightNodeOptions Options { get; private set; }
        public IReadOnlyCollection<KeyValuePair<string, OperationInfo>> RegisteredHandlers { get; private set; }

        public RegisteredHandlersInfo(string engineId, ILightNodeOptions options, IReadOnlyCollection<KeyValuePair<string, OperationInfo>> registeredHandlers)
        {
            this.EngineId = engineId;
            this.Options = options;
            this.RegisteredHandlers = registeredHandlers;
        }
    }

    public class LightNodeServerMiddleware
    {
        static readonly object runningHandlerLock = new object();
        static ILookup<string, RegisteredHandlersInfo> runningHandlers =
            Enumerable.Empty<RegisteredHandlersInfo>().ToLookup(x => "", x => x);

        /// <summary>
        /// Get all registered handlers. Key is ILightNodeOptions.ServerEngineId.
        /// </summary>
        public static ILookup<string, RegisteredHandlersInfo> GetRegisteredHandlersInfo()
        {
            return runningHandlers;
        }

        readonly LightNodeServer engine;
        readonly bool useOtherMiddleware;
        readonly RequestDelegate next;

        public LightNodeServerMiddleware(RequestDelegate next, ILightNodeOptions options, Assembly[] hostAssemblies)
        {
            this.next = next;
            this.useOtherMiddleware = options.UseOtherMiddleware;
            this.engine = new LightNodeServer(options);

            var sw = Stopwatch.StartNew();
            var registeredHandler = this.engine.RegisterHandler(hostAssemblies);
            options.Logger.InitializeComplete(sw.Elapsed.TotalMilliseconds);

            lock (runningHandlerLock)
            {
                runningHandlers = runningHandlers.SelectMany(g => g, (g, xs) => new { g.Key, xs })
                    .Concat(new[] { new { Key = options.ServerEngineId, xs = new RegisteredHandlersInfo(options.ServerEngineId, options, registeredHandler) } })
                    .ToLookup(x => x.Key, x => x.xs);
            }
        }

        public async Task Invoke(HttpContext context)
        {
            if (useOtherMiddleware)
            {
                await engine.ProcessRequest(context).ConfigureAwait(true); // keep context
                await next(context).ConfigureAwait(false);
            }
            else
            {
                await engine.ProcessRequest(context).ConfigureAwait(false);
            }
        }
    }
}