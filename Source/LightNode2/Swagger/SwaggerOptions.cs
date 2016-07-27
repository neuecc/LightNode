using LightNode.Swagger.Schema;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace LightNode.Swagger
{
    public class SwaggerOptions
    {
        public string ApiBasePath { get; private set; }

        public Swagger.Schema.Info Info { get; set; }

        /// <summary>
        /// (FilePath, LoadedEmbeddedBytes) => CustomBytes)
        /// </summary>
        public Func<string, byte[], byte[]> ResolveCustomResource { get; set; }
        public Func<HttpContext, string> CustomHost { get; set; }
        public string XmlDocumentPath { get; set; }

        public bool IsEmitEnumAsString { get; set; }

        public SwaggerOptions(string title, string apiBasePath)
        {
            ApiBasePath = apiBasePath;
            Info = new Info { description = "", version = "1.0", title = title };
        }
    }
}
