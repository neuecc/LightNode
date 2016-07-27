using LightNode.Diagnostics;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightNode.Server
{
    internal static class ParameterBinder
    {
        internal static object[] BindParameter(HttpContext httpContext, ILightNodeOptions options,IOperationCoordinator coordinator,  ValueProvider valueProvider, ParameterInfoSlim[] arguments)
        {
            var methodParameters = new object[arguments.Length];
            for (int i = 0; i < arguments.Length; i++)
            {
                var item = arguments[i];

                var _values = valueProvider.GetValue(item.Name);
                var value = _values as string;
                var values = _values as List<string>;
                var isEmpty = _values == null;

                if (isEmpty && !item.ParameterTypeIsArray)
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
                        coordinator.OnProcessInterrupt(options, httpContext, InterruptReason.ParameterBindMissing, "Lack of Parameter:" + item.Name);
                        options.Logger.ParameterBindMissing(OperationMissingKind.LackOfParameter, item.Name);
                        if (options.OperationMissingHandlingPolicy == OperationMissingHandlingPolicy.ThrowException)
                        {
                            throw new ParameterMissingException(OperationMissingKind.LackOfParameter, item.Name);
                        }
                        else
                        {
                            httpContext.EmitBadRequest();
                            if (options.OperationMissingHandlingPolicy == OperationMissingHandlingPolicy.ReturnErrorStatusCodeIncludeErrorDetails)
                            {
                                httpContext.EmitStringMessage("Lack of Parameter:" + item.Name);
                            }
                            return null;
                        }
                    }
                }
                else if (!item.ParameterTypeIsArray)
                {
                    var conv = TypeBinder.GetConverter(item.ParameterType, !options.ParameterEnumAllowsFieldNameParse);
                    if (conv == null) throw new InvalidOperationException("critical:register code is broken");

                    object pValue;
                    if (conv(value ?? values[0], out pValue))
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
                        coordinator.OnProcessInterrupt(options, httpContext, InterruptReason.ParameterBindMissing, "Mismatch ParameterType:" + item.Name);
                        options.Logger.ParameterBindMissing(OperationMissingKind.MissmatchParameterType, item.Name);
                        if (options.OperationMissingHandlingPolicy == OperationMissingHandlingPolicy.ThrowException)
                        {
                            throw new ParameterMissingException(OperationMissingKind.MissmatchParameterType, item.Name);
                        }
                        else
                        {
                            httpContext.EmitBadRequest();
                            if (options.OperationMissingHandlingPolicy == OperationMissingHandlingPolicy.ReturnErrorStatusCodeIncludeErrorDetails)
                            {
                                httpContext.EmitStringMessage("Mismatch ParameterType:" + item.Name);
                            }
                            return null;
                        }
                    }
                }

                var arrayConv = TypeBinder.GetArrayConverter(item.ParameterType, !options.ParameterEnumAllowsFieldNameParse);
                if (arrayConv == null) throw new InvalidOperationException("critical:register code is broken");

                methodParameters[i] = arrayConv((values != null) ? values : (value != null) ? new[] { value } : (IList<string>)new string[0]);
                continue;
            }

            return methodParameters;
        }
    }
}