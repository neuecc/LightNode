using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace LightNode.Server
{
    public class ReturnStatusCodeException : Exception
    {
        public string Content { get; set; }
        public string ReasonPhrase { get; set; }

        public HttpStatusCode StatusCode { get; private set; }

        public ReturnStatusCodeException(HttpStatusCode statusCode)
        {
            this.StatusCode = statusCode;
        }

        internal void EmitCode(IDictionary<string, object> environment)
        {
            environment["owin.ResponseStatusCode"] = (int)StatusCode;
            if (ReasonPhrase != null)
            {
                var responseHeader = environment["owin.ResponseHeaders"] as IDictionary<string, string[]>;
                responseHeader["Reason-Phrase"] = new[] { ReasonPhrase };
            }
            if (Content != null)
            {
                var responseStream = environment["owin.ResponseBody"] as Stream;
                var bytes = System.Text.Encoding.UTF8.GetBytes(Content);
                responseStream.Write(bytes, 0, bytes.Length);
            }
        }
    }
}