using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightNode.Server
{
    internal class ValueProvider
    {
        // object is List[String] or String.
        // optimize way, value is single in many cases.
        Dictionary<string, object> values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public ValueProvider(HttpContext context, AcceptVerbs verb)
        {
            var queryString = context.Request.QueryString;
            AppendValues(queryString.Value.TrimStart('?'));

            if (verb != AcceptVerbs.Get)
            {
                var requestHeader = context.Request.Headers;
                StringValues contentType;
                if (requestHeader.TryGetValue("Content-Type", out contentType))
                {
                    if (contentType.Any(x => x.Contains("application/x-www-form-urlencoded")))
                    {
                        var requestStream = context.Request.Body;
                        using (var sr = new StreamReader(new UnclosableStream(requestStream)))
                        {
                            var formUrlEncoded = sr.ReadToEnd();
                            AppendValues(formUrlEncoded);
                        }
                        requestStream.Position = 0; // rewind for custom use
                    }
                }
            }
        }

        void AppendValues(string urlEncodedString)
        {
            foreach (var amp in urlEncodedString.Split('&'))
            {
                var item = amp.Split('=');
                if (item.Length == 2)
                {
                    var key = System.Net.WebUtility.UrlDecode(item[0]);
                    var value = System.Net.WebUtility.UrlDecode(item[1]);

                    object result;
                    if (values.TryGetValue(key, out result))
                    {
                        if (result is string)
                        {
                            // second
                            values[key] = new List<string>() { (string)result, value };
                        }
                        else
                        {
                            // third
                            ((List<string>)result).Add(value);
                        }
                    }
                    else
                    {
                        // first
                        values[key] = value;
                    }
                }
            }
        }

        /// <summary>Returns List[String] or String or Null</summary>
        public object GetValue(string key)
        {
            object result;
            return values.TryGetValue(key, out result)
                ? result
                : null;
        }
    }
}
