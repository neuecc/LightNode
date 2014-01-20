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

        public ValueProvider(IDictionary<string, object> environment, AcceptVerbs verb)
        {
            var queryString = environment["owin.RequestQueryString"] as string;
            AppendValues(queryString);

            if (verb != AcceptVerbs.Get)
            {
                using (var sr = new StreamReader((environment["owin.RequestBody"] as Stream)))
                {
                    var formUrlEncoded = sr.ReadToEnd();
                    AppendValues(formUrlEncoded);
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
                    var key = Uri.UnescapeDataString(item[0]);
                    var value = Uri.UnescapeDataString(item[1]);

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
