using LightNode.Server;
using LightNode.Swagger;
using LightNode.Swagger.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;

namespace LightNode.Swagger
{
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public class LightNodeSwaggerMiddleware
    {
        readonly AppFunc next;
        readonly SwaggerOptions options;

        public LightNodeSwaggerMiddleware(AppFunc next, SwaggerOptions options)
        {
            this.next = next;
            this.options = options;
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            // reference embedded resouces
            const string prefix = "LightNode.Swagger.SwaggerUI.";

            var path = environment.AsRequestPath().Trim('/');
            if (path == "") path = "index.html";
            var filePath = prefix + path.Replace("/", ".");

            if (path.Split('.').Last() == "json")
            {
                var engineId = Path.GetFileNameWithoutExtension(path);
                // only use first item
                var target = (engineId == "api-default")
                    ? LightNodeServerMiddleware.GetRegisteredHandlersInfo().First().First()
                    : LightNodeServerMiddleware.GetRegisteredHandlersInfo()[engineId].First();

                var bytes = BuildSwaggerJson(options, environment, target);
                environment.AsResponseBody().Write(bytes, 0, bytes.Length);
                environment.AsResponseHeaders()["Content-Type"] = new[] { "application/json" };
                environment[OwinConstants.ResponseStatusCode] = 200;
                return Task.FromResult(0);
            }

            var myAssembly = typeof(LightNodeSwaggerMiddleware).Assembly;

            using (var stream = myAssembly.GetManifestResourceStream(filePath))
            {
                if (options.ResolveCustomResource == null)
                {
                    if (stream == null) return next(environment);

                    var response = environment.AsResponseBody();
                    stream.CopyTo(response);
                }
                else
                {
                    byte[] bytes;
                    if (stream == null)
                    {
                        bytes = options.ResolveCustomResource(path, null);
                    }
                    else
                    {
                        using (var ms = new MemoryStream())
                        {
                            stream.CopyTo(ms);
                            bytes = options.ResolveCustomResource(path, ms.ToArray());
                        }
                    }

                    if (bytes == null) return next(environment);

                    var response = environment.AsResponseBody();
                    response.Write(bytes, 0, bytes.Length);
                }
            }

            var mediaType = GetMediaType(filePath);
            environment.AsResponseHeaders()["Content-Type"] = new[] { mediaType };
            environment[OwinConstants.ResponseStatusCode] = 200;

            return Task.FromResult(0);
        }


        static byte[] BuildSwaggerJson(SwaggerOptions options, IDictionary<string, object> environment, RegisteredHandlersInfo handlersInfo)
        {
            var xDocLookup = (options.XmlDocumentPath != null)
                ? BuildXmlCommentStructure(options.XmlDocumentPath)
                : null;

            var doc = new SwaggerDocument();
            doc.swagger = "2.0";
            doc.info = options.Info;
            doc.host = (options.CustomHost != null) ? options.CustomHost(environment) : environment.AsRequestHeaders()["Host"][0];
            doc.basePath = options.ApiBasePath;
            doc.schemes = new[] { environment.AsRequestScheme() };
            doc.paths = new Dictionary<string, PathItem>();

            foreach (var item in handlersInfo.RegisteredHandlers)
            {
                XmlCommentStructure xmlComment = null;
                if (xDocLookup != null)
                {
                    xmlComment = xDocLookup[Tuple.Create(item.Value.ClassName, item.Value.MethodName)].FirstOrDefault();
                }

                var parameters = item.Value.Parameters
                    .Select(x =>
                    {
                        var parameterXmlComment = x.ParameterType.Name;
                        if (xmlComment != null)
                        {
                            xmlComment.Parameters.TryGetValue(x.Name, out parameterXmlComment);
                            parameterXmlComment = x.ParameterType.Name + " " + parameterXmlComment;
                        }

                        var defaultValue = x.DefaultValue;
                        if (defaultValue != null && x.ParameterType.IsEnum)
                        {
                            defaultValue = (options.IsEmitEnumAsString)
                                ? defaultValue.ToString()
                                : Convert.ChangeType(defaultValue, Enum.GetUnderlyingType(defaultValue.GetType()));
                        }

                        var items = (x.ParameterTypeIsArray)
                            ? new Items { type = TypeToType(x.ParameterType.GetElementType()) }
                            : null;

                        object[] enums = null;
                        if (x.ParameterType.IsEnum || (x.ParameterType.IsArray && x.ParameterType.GetElementType().IsEnum))
                        {
                            var enumType = (x.ParameterType.IsEnum) ? x.ParameterType : x.ParameterType.GetElementType();

                            var enumValues = Enum.GetValues(enumType).Cast<object>()
                                .Select(v =>
                                {
                                    return (options.IsEmitEnumAsString)
                                        ? v.ToString()
                                        : Convert.ChangeType(v, Enum.GetUnderlyingType(enumType));
                                })
                                .ToArray();

                            if (x.ParameterType.IsArray)
                            {
                                items.@enum = enumValues;
                            }
                            else
                            {
                                enums = enumValues;
                            }
                        }

                        return new Parameter
                        {
                            name = x.Name,
                            @in = "formData",
                            type = TypeToType(x.ParameterType),
                            description = parameterXmlComment,
                            required = !x.IsOptional,
                            @default = (x.IsOptional) ? defaultValue : null,
                            items = items,
                            @enum = enums,
                            collectionFormat = "multi"
                        };
                    })
                    .ToArray();

                var operation = new Operation
                {
                    tags = new[] { item.Value.ClassName },
                    summary = (xmlComment != null) ? xmlComment.Summary : "",
                    description = (xmlComment != null) ? xmlComment.Remarks : "",
                    parameters = parameters
                };

                doc.paths.Add(item.Key, CreatePathItem(item.Value.AcceptVerbs, operation));
            }

            using (var ms = new MemoryStream())
            {
                var serializer = new DataContractJsonSerializer(typeof(SwaggerDocument), new DataContractJsonSerializerSettings
                {
                    SerializeReadOnlyTypes = true,
                    UseSimpleDictionaryFormat = true
                });
                serializer.WriteObject(ms, doc);
                return ms.ToArray();
            }
        }

        static PathItem CreatePathItem(AcceptVerbs acceptVerbs, Operation operation)
        {
            if (acceptVerbs.HasFlag(AcceptVerbs.Get))
            {
                return new PathItem { get = operation };
            }
            if (acceptVerbs.HasFlag(AcceptVerbs.Post))
            {
                return new PathItem { post = operation };
            }
            if (acceptVerbs.HasFlag(AcceptVerbs.Put))
            {
                return new PathItem { put = operation };
            }
            if (acceptVerbs.HasFlag(AcceptVerbs.Delete))
            {
                return new PathItem { delete = operation };
            }
            if (acceptVerbs.HasFlag(AcceptVerbs.Patch))
            {
                return new PathItem { patch = operation };
            }
            throw new ArgumentOutOfRangeException();
        }

        static string GetMediaType(string path)
        {
            var extension = path.Split('.').Last();

            switch (extension)
            {
                case "css":
                    return "text/css";
                case "js":
                    return "text/javascript";
                case "json":
                    return "application/json";
                case "gif":
                    return "image/gif";
                case "png":
                    return "image/png";
                case "eot":
                    return "application/vnd.ms-fontobject";
                case "woff":
                    return "application/font-woff";
                case "woff2":
                    return "application/font-woff2";
                case "otf":
                    return "application/font-sfnt";
                case "ttf":
                    return "application/font-sfnt";
                case "svg":
                    return "image/svg+xml";
                case "ico":
                    return "image/x-icon";
                default:
                    return "text/html";
            }
        }

        static string TypeToType(Type type)
        {
            if (type.IsArray)
            {
                return "array";
            }

            if (type.IsNullable())
            {
                type = Nullable.GetUnderlyingType(type);
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    return "boolean";
                case TypeCode.Decimal:
                case TypeCode.Single:
                case TypeCode.Double:
                    return "number";
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                    return "integer";
                default:
                    return "string";
            }
        }

        static ILookup<Tuple<string, string>, XmlCommentStructure> BuildXmlCommentStructure(string xmlDocumentPath)
        {
            var file = File.ReadAllText(xmlDocumentPath);
            var xDoc = XDocument.Parse(file);
            var xDocLookup = xDoc.Descendants("member")
                .Where(x => x.Attribute("name").Value.StartsWith("M:"))
                .Select(x =>
                {
                    var match = Regex.Match(x.Attribute("name").Value, @"(\w+)\.(\w+)?(\(.+\)|$)");

                    var summary = ((string)x.Element("summary")) ?? "";
                    var returns = ((string)x.Element("returns")) ?? "";
                    var remarks = ((string)x.Element("remarks")) ?? "";
                    var parameters = x.Elements("param")
                        .Select(e => Tuple.Create(e.Attribute("name").Value, e))
                        .Distinct(new Item1EqualityCompaerer<string, XElement>())
                        .ToDictionary(e => e.Item1, e => e.Item2.Value);

                    return new XmlCommentStructure
                    {
                        ClassName = match.Groups[1].Value,
                        MethodName = match.Groups[2].Value,
                        Summary = summary,
                        Remarks = remarks,
                        Parameters = parameters,
                        Returns = returns
                    };
                })
                .ToLookup(x => Tuple.Create(x.ClassName, x.MethodName));

            return xDocLookup;
        }

        class Item1EqualityCompaerer<T1, T2> : EqualityComparer<Tuple<T1, T2>>
        {
            public override bool Equals(Tuple<T1, T2> x, Tuple<T1, T2> y)
            {
                return x.Item1.Equals(y.Item1);
            }

            public override int GetHashCode(Tuple<T1, T2> obj)
            {
                return obj.Item1.GetHashCode();
            }
        }

        class XmlCommentStructure
        {
            public string ClassName { get; set; }
            public string MethodName { get; set; }
            public string Summary { get; set; }
            public string Remarks { get; set; }
            public Dictionary<string, string> Parameters { get; set; }
            public string Returns { get; set; }
        }
    }
}

namespace Owin
{
    public static class AppBuilderLightNodeSwaggerMiddlewareExtensions
    {
        public static IAppBuilder UseLightNodeSwagger(this IAppBuilder app, SwaggerOptions options)
        {
            return app.Use(typeof(LightNodeSwaggerMiddleware), options);
        }
    }
}