using LightNode.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace LightNode.Server
{
    public class ReturnStatusCodeException : Exception
    {
        public HttpStatusCode StatusCode { get; private set; }
        public string ReasonPhrase { get; private set; }

        object content;
        IContentFormatter contentFormatter;

        public ReturnStatusCodeException(HttpStatusCode statusCode, string reasonPhrase = null, object content = null, IContentFormatter contentFormatter = null)
        {
            this.StatusCode = statusCode;
            this.ReasonPhrase = reasonPhrase;
            this.content = content;
            this.contentFormatter = contentFormatter;
        }

        internal void EmitCode(ILightNodeOptions options, IDictionary<string, object> environment)
        {
            environment[OwinConstants.ResponseStatusCode] = (int)StatusCode;
            if (ReasonPhrase != null)
            {
                environment[OwinConstants.ResponseStatusCode] = ReasonPhrase;
            }
            if (content != null)
            {
                contentFormatter = contentFormatter ?? options.DefaultFormatter;
                var encoding = contentFormatter.Encoding;
                var responseHeader = environment.AsResponseHeaders();
                responseHeader["Content-Type"] = new[] { contentFormatter.MediaType + ((encoding == null) ? "" : "; charset=" + encoding.WebName) };

                var responseStream = environment.AsResponseBody();
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
                            // can't await in catch clouse at C# 5.0:)
                            // return buffer.CopyToAsync(responseStream);
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