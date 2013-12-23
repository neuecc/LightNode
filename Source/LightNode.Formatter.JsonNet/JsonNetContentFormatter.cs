using Newtonsoft.Json;
using System;
using System.IO;

namespace LightNode.Formatter
{
    public class JsonNetContentFormatter : LightNode.Formatter.ContentFormatterBase
    {
        public JsonNetContentFormatter(string mediaType = "application/json", string ext = "json")
            : base(mediaType, ext)
        {
        }

        public override void Serialize(System.IO.Stream stream, object obj)
        {
            using (var sw = new StreamWriter(stream))
            using (var jw = new JsonTextWriter(sw))
            {
                jw.Formatting = Formatting.None;

                var serializer = new JsonSerializer();
                serializer.Serialize(jw, obj);
            }
        }

        public override object Deserialize(Type type, System.IO.Stream stream)
        {
            using (var sr = new StreamReader(stream))
            using (var jr = new JsonTextReader(sr))
            {
                var serializer = new JsonSerializer();
                return serializer.Deserialize(jr, type);
            }
        }
    }
}