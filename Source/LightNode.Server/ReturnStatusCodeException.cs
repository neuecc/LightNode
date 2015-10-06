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
        Action<IDictionary<string, object>> environmentEmitter;

        public ReturnStatusCodeException(HttpStatusCode statusCode, string reasonPhrase = null, object content = null, IContentFormatter contentFormatter = null, Action<IDictionary<string, object>> environmentEmitter = null)
        {
            this.StatusCode = statusCode;
            this.ReasonPhrase = reasonPhrase;
            this.content = content;
            this.contentFormatter = contentFormatter;
            this.environmentEmitter = environmentEmitter;
        }

        internal void EmitCode(ILightNodeOptions options, IDictionary<string, object> environment)
        {
            environment[OwinConstants.ResponseStatusCode] = (int)StatusCode;
            if (ReasonPhrase != null)
            {
                environment[OwinConstants.ResponseReasonPhrase] = ReasonPhrase;
            }
            if (content != null)
            {
                contentFormatter = contentFormatter ?? options.DefaultFormatter;
                var encoding = contentFormatter.Encoding;
                var responseHeader = environment.AsResponseHeaders();
                responseHeader["Content-Type"] = new[] { contentFormatter.MediaType + ((encoding == null) ? "" : "; charset=" + encoding.WebName) };

                if (environmentEmitter != null)
                {
                    environmentEmitter(environment);
                }

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