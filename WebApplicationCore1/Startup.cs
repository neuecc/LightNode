using System;
using System.Reflection;
using LightNode;
using LightNode.Server;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Glimpse;
using System.IO;

namespace WebApplicationCore1
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGlimpse();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseGlimpse();

            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Map("/api", builder =>
            {
                builder.UseLightNode(typeof(Startup));
            });

            app.Map("/swagger", builder =>
            {
                var xmlName = "WebApplicationCore1.xml";
                var xmlPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), xmlName);

                builder.UseLightNodeSwagger(new LightNode.Swagger.SwaggerOptions("WebApplicationCore1", "/api") // baseApi is LightNode's root
                {
                    XmlDocumentPath = xmlPath,
                    IsEmitEnumAsString = true
                });
            });

        }
    }

    /// <summary>
    /// aaa
    /// </summary>
    public class MyClass : LightNodeContract
    {
        /// <summary>
        /// HogeHoge
        /// </summary>
        public string Hoge(string i)
        {
            return "hogehogehoge!!!";
        }

        [Get]
        public int[] ArraySendTestGet(int[] xs)
        {
            return xs;
        }

        [Post]
        public int[] ArraySendTestPost(int[] xs)
        {
            return xs;
        }
    }
}
