using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace LightNode.Swagger.Schema
{
    [DataContract]
    public class SwaggerDocument
    {
        [DataMember(EmitDefaultValue = false)]
        public string swagger { get; set; }
        /// <summary>Required</summary>
        [DataMember(EmitDefaultValue = false)]
        public Info info { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string host { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string basePath { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string[] schemes { get; set; } // http, https. ws, wss

        /// <summary>Required</summary>
        [DataMember(EmitDefaultValue = false)]
        public IDictionary<string, PathItem> paths { get; set; }
    }

    [DataContract]
    public class Info
    {
        /// <summary>Required</summary>
        [DataMember(EmitDefaultValue = false)]
        public string title { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string description { get; set; }
        public Contact contact { get; set; }
        public License license { get; set; }
        /// <summary>Required</summary>
        [DataMember(EmitDefaultValue = false)]
        public string version { get; set; }
    }

    [DataContract]
    public class Contact
    {
        [DataMember(EmitDefaultValue = false)]
        public string name { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string url { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string email { get; set; }
    }

    [DataContract]
    public class License
    {
        /// <summary>Required</summary>
        [DataMember(EmitDefaultValue = false)]
        public string name { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string url { get; set; }
    }

    [DataContract]
    public class PathItem
    {
        // currently only supports post:)
        [DataMember(EmitDefaultValue = false)]
        public Operation post { get; set; }
    }

    [DataContract]
    public class Operation
    {
        [DataMember(EmitDefaultValue = false)]
        public string[] tags { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string summary { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string description { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public Parameter[] parameters { get; set; }

        // no response:) <- required!
        // public IDictionary<string, Response> responses { get; set; }
    }

    [DataContract]
    public class Parameter
    {
        [DataMember(EmitDefaultValue = false)]
        public string name { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string type { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string @in { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string description { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool required { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public object @default { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public Items items { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string collectionFormat { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public object[] @enum;
    }

    [DataContract]
    public class Items
    {
        [DataMember(EmitDefaultValue = false)]
        public string type { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public object[] @enum { get; set; }
    }

    [DataContract]
    public class Response
    {
        [DataMember(EmitDefaultValue = false)]
        public string description { get; set; }

        // other parameters not yet done
    }
}