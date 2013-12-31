using Newtonsoft.Json;
using System;
using System.IO;

namespace LightNode.Formatter
{
    public class JsonNetContentFormatter : LightNode.Formatter.ContentFormatterBase
    {
        readonly JsonSerializer serializer;

        public JsonNetContentFormatter(string mediaType = "application/json", string ext = "json")
            : this(new JsonSerializer(), mediaType, ext)
        {
        }
        public JsonNetContentFormatter(JsonSerializer serializer, string mediaType = "application/json", string ext = "json")
            : base(mediaType, ext)
        {
            this.serializer = serializer;
        }

        public override void Serialize(System.IO.Stream stream, object obj)
        {
            using (var sw = new StreamWriter(stream))
            {
                serializer.Serialize(sw, obj);
            }
        }

        public override object Deserialize(Type type, System.IO.Stream stream)
        {
            using (var sr = new StreamReader(stream))
            {
                return serializer.Deserialize(sr, type);
            }
        }
    }
}