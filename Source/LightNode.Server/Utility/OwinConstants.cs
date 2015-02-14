using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LightNode.Server.Utility
{
    // Owin Spec 1.0.0
    // http://owin.org/spec/spec/owin-1.0.0.html
    internal static class OwinConstants
    {
        // 3.2.1 Request Data
        public const string RequestBody = "owin.RequestBody";
        public const string RequestHeaders = "owin.RequestHeaders";
        public const string RequestMethod = "owin.RequestMethod";
        public const string RequestPath = "owin.RequestPath";
        public const string RequestPathBase = "owin.RequestPathBase";
        public const string RequestProtocol = "owin.RequestProtocol";
        public const string RequestQueryString = "owin.RequestQueryString";
        public const string RequestScheme = "owin.RequestScheme";

        // 3.2.2 Response Data
        public const string ResponseBody = "owin.ResponseBody";
        public const string ResponseHeaders = "owin.ResponseHeaders";
        public const string ResponseStatusCode = "owin.ResponseStatusCode";
        public const string ResponseReasonPhrase = "owin.ResponseReasonPhrase";
        public const string ResponseProtocol = "owin.ResponseProtocol";

        // 3.2.3 Other Data
        public const string Version = "owin.Version";
        public const string CallCancelled = "owin.CallCancelled";
    }

    internal static class OwinConvertExtensions
    {
        // Request

        /// <summary>Required:Yes, A Stream with the request body, if any. Stream.Null MAY be used as a placeholder if there is no request body.</summary>
        public static Stream AsRequestBody(this IDictionary<string, object> environment)
        {
            return environment[OwinConstants.RequestBody] as Stream;
        }

        /// <summary>Required:Yes, An IDictionary of request headers.</summary>
        public static IDictionary<string, string[]> AsRequestHeaders(this IDictionary<string, object> environment)
        {
            return environment[OwinConstants.RequestHeaders] as IDictionary<string, string[]>;
        }

        /// <summary>Required:Yes, A string containing the HTTP request method of the request (e.g., "GET", "POST").</summary>
        public static string RequestMethod(this IDictionary<string, object> environment)
        {
            return environment[OwinConstants.RequestMethod] as string;
        }

        /// <summary>Required:Yes, A string containing the request path. The path MUST be relative to the "root" of the application delegate.</summary>
        public static string AsRequestPath(this IDictionary<string, object> environment)
        {
            return environment[OwinConstants.RequestPath] as string;
        }

        /// <summary>Required:Yes, A string containing the portion of the request path corresponding to the "root" of the application delegate.</summary>
        public static string AsRequestPathBase(this IDictionary<string, object> environment)
        {
            return environment[OwinConstants.RequestPathBase] as string;
        }

        /// <summary>Required:Yes, A string containing the protocol name and version (e.g. "HTTP/1.0" or "HTTP/1.1").</summary>
        public static string AsRequestProtocol(this IDictionary<string, object> environment)
        {
            return environment[OwinConstants.RequestProtocol] as string;
        }

        /// <summary>Required:Yes, A string containing the query string component of the HTTP request URI, without the leading “?” (e.g., "foo=bar&baz=quux"). The value may be an empty string.</summary>
        public static string AsRequestQueryString(this IDictionary<string, object> environment)
        {
            return environment[OwinConstants.RequestQueryString] as string;
        }

        /// <summary>Required:Yes, A string containing the URI scheme used for the request (e.g., "http", "https").</summary>
        public static string AsRequestScheme(this IDictionary<string, object> environment)
        {
            return environment[OwinConstants.RequestScheme] as string;
        }

        // Response

        /// <summary>Required:Yes, A Stream used to write out the response body, if any.</summary>
        public static Stream AsResponseBody(this IDictionary<string, object> environment)
        {
            return environment[OwinConstants.ResponseBody] as Stream;
        }

        /// <summary>Required:Yes, An IDictionary of response headers.</summary>
        public static IDictionary<string, string[]> AsResponseHeaders(this IDictionary<string, object> environment)
        {
            return environment[OwinConstants.ResponseHeaders] as IDictionary<string, string[]>;
        }

        /// <summary>Required:No, An optional int containing the HTTP response status code as defined in RFC 2616 section 6.1.1. The default is 200.</summary>
        public static int? AsResponseStatusCode(this IDictionary<string, object> environment)
        {
            object value;
            return environment.TryGetValue(OwinConstants.ResponseStatusCode, out value)
                ? (int?)value
                : null;
        }

        /// <summary>Required:No, An optional string containing the reason phrase associated the given status code. If none is provided then the server SHOULD provide a default as described in RFC 2616 section 6.1.1</summary>
        public static string AsResponseReasonPhrase(this IDictionary<string, object> environment)
        {
            object value;
            return environment.TryGetValue(OwinConstants.ResponseReasonPhrase, out value)
                ? (string)value
                : null;
        }

        /// <summary>Required:No, An optional string containing the protocol name and version (e.g. "HTTP/1.0" or "HTTP/1.1"). If none is provided then the “owin.RequestProtocol” key’s value is the default.</summary>
        public static string AsResponseProtocol(this IDictionary<string, object> environment)
        {
            object value;
            return environment.TryGetValue(OwinConstants.ResponseProtocol, out value)
                ? (string)value
                : null;
        }

        // Other

        /// <summary>Required:Yes, A CancellationToken indicating if the request has been cancelled/aborted.</summary>
        public static CancellationToken AsCallCancelled(this IDictionary<string, object> environment)
        {
            return (CancellationToken)environment[OwinConstants.CallCancelled];
        }

        /// <summary>Required:Yes, The string "1.0" indicating OWIN version.</summary>
        public static string AsVersion(this IDictionary<string, object> environment)
        {
            return environment[OwinConstants.Version] as string;
        }
    }
}