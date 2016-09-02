using LightNode.Core;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Net;

namespace LightNode.Server
{
    public class ReturnStatusCodeException : Exception
    {
        public int StatusCode { get; private set; }

        object content;
        IContentFormatter contentFormatter;
        Action<HttpContext> contextEmitter;

        public ReturnStatusCodeException(int statusCode, object content = null, IContentFormatter contentFormatter = null, Action<HttpContext> contextEmitter = null)
        {
            this.StatusCode = statusCode;
            this.content = content;
            this.contentFormatter = contentFormatter;
            this.contextEmitter = contextEmitter;
        }

        public ReturnStatusCodeException(HttpStatusCode statusCode, object content = null, IContentFormatter contentFormatter = null, Action<HttpContext> contextEmitter = null)
        {
            this.StatusCode = (int)statusCode;
            this.content = content;
            this.contentFormatter = contentFormatter;
            this.contextEmitter = contextEmitter;
        }

        internal void EmitCode(ILightNodeOptions options, HttpContext httpContext)
        {
            httpContext.Response.StatusCode = (int)StatusCode;
   
            if (content != null)
            {
                contentFormatter = contentFormatter ?? options.DefaultFormatter;
                var encoding = contentFormatter.Encoding;
                var responseHeader = httpContext.Response.Headers;
                responseHeader["Content-Type"] = new[] { contentFormatter.MediaType + ((encoding == null) ? "" : "; charset=" + encoding.WebName) };

                contextEmitter?.Invoke(httpContext);

                var responseStream = httpContext.Response.Body;
                if (options.StreamWriteOption == StreamWriteOption.DirectWrite)
                {
                    contentFormatter.Serialize(new UnclosableStream(responseStream), content);
                }
                else
                {
                    using (var buffer = new MemoryStream())
                    {
                        contentFormatter.Serialize(new UnclosableStream(buffer), content);
                        responseHeader["Content-Length"] = new[] { buffer.Position.ToString() };
                        buffer.Position = 0;
                        if (options.StreamWriteOption == StreamWriteOption.BufferAndWrite)
                        {
                            buffer.CopyTo(responseStream); // not CopyToAsync
                        }
                        else
                        {
                            // EmitCode is void:)
                            buffer.CopyTo(responseStream);
                        }
                    }
                }
            }
        }

        public override string ToString()
        {
            return "ReturnStatusCode:" + (int)StatusCode + " " + StatusCode.ToString();
        }
    }
}