using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightNode.Server
{
    internal class ParameterBinder
    {
        public static ParameterBinder Default = new ParameterBinder();

        internal object[] BindParameter(IDictionary<string, object> environment, LightNodeOptions options, ParameterInfoSlim[] arguments)
        {
            // Extract parameter
            ILookup<string, string> requestParameter;
            var queryString = environment["owin.RequestQueryString"] as string;
            using (var sr = new StreamReader((environment["owin.RequestBody"] as Stream)))
            {
                var str = sr.ReadToEnd();
                requestParameter = str.Split('&')
                    .Concat(queryString.Split('&'))
                    .Select(xs => xs.Split('='))
                    .Where(xs => xs.Length == 2)
                    .ToLookup(xs => Uri.UnescapeDataString(xs[0]), xs => Uri.UnescapeDataString(xs[1]), StringComparer.OrdinalIgnoreCase);
            }

            // Parameter binding
            var methodParameters = new object[arguments.Length];
            for (int i = 0; i < arguments.Length; i++)
            {
                var item = arguments[i];

                var values = requestParameter[item.Name];
                var count = values.Count();
                if (count == 0 && !item.ParameterTypeIsArray)
                {
                    if (item.IsOptional)
                    {
                        methodParameters[i] = item.DefaultValue;
                        continue;
                    }
                    else if ((!item.ParameterTypeIsString || options.ParameterStringImplicitNullAsDefault) && (item.ParameterTypeIsClass || item.ParameterTypeIsNullable))
                    {
                        methodParameters[i] = null;
                        continue;
                    }
                    else
                    {
                        environment.EmitBadRequest();
                        environment.EmitStringMessage("Lack of Parameter:" + item.Name);
                        return null;
                    }
                }
                else if (!item.ParameterTypeIsArray)
                {
                    var conv = TypeBinder.GetConverter(item.ParameterType, !options.ParameterEnumAllowsFieldNameParse);
                    if (conv == null) throw new InvalidOperationException("critical:register code is broken");

                    object pValue;
                    if (conv(values.First(), out pValue))
                    {
                        methodParameters[i] = pValue;
                        continue;
                    }
                    else if (item.IsOptional)
                    {
                        methodParameters[i] = item.DefaultValue;
                        continue;
                    }
                    else if ((!item.ParameterTypeIsString || options.ParameterStringImplicitNullAsDefault) && (item.ParameterTypeIsClass || item.ParameterTypeIsNullable))
                    {
                        methodParameters[i] = null;
                        continue;
                    }
                    else
                    {
                        environment.EmitBadRequest();
                        environment.EmitStringMessage("Mismatch Parameter Type:" + item.Name);
                        return null;
                    }
                }

                var arrayConv = TypeBinder.GetArrayConverter(item.ParameterType, !options.ParameterEnumAllowsFieldNameParse);
                if (arrayConv == null) throw new InvalidOperationException("critical:register code is broken");

                methodParameters[i] = arrayConv(values);
                continue;
            }

            return methodParameters;
        }
    }
}