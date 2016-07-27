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
using System.IO;

namespace AspNetCoreSample
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.Map("/api", builder =>
            {
                builder.UseLightNode(typeof(Startup));
            });

            app.Map("/swagger", builder =>
            {
                var xmlName = "AspNetCoreSample.xml";
                var xmlPath = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), xmlName);

                builder.UseLightNodeSwagger(new LightNode.Swagger.SwaggerOptions("AspNetCoreSample", "/api")
                {
                    XmlDocumentPath = xmlPath,
                    IsEmitEnumAsString = true
                });
            });
        }
    }

    public class Toriaezu : LightNodeContract
    {
        public string Echo(string x)
        {
            return x;
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
