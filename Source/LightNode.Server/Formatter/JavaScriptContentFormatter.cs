using LightNode.Core;
using System;
using System.IO;
using System.Text;

namespace LightNode.Formatter
{
    public class JavaScriptContentFormatter : ContentFormatterBase
    {
        public JavaScriptContentFormatter(string mediaType = "application/json; charset=utf-8", string ext = "json")
            : this(null, mediaType, ext)
        {

        }
        public JavaScriptContentFormatter(Encoding encoding, string mediaType = "application/json; charset=utf-8", string ext = "json")
            : base(mediaType, ext, encoding ?? Encoding.UTF8)
        {

        }

        public override void Serialize(Stream stream, object obj)
        {
            var json = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(obj);
            var data = this.Encoding.GetBytes(json);
            stream.Write(data, 0, data.Length);
        }

        public override object Deserialize(Type type, Stream stream)
        {
            using (var sr = new StreamReader(stream, this.Encoding))
            {
                return new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize(sr.ReadToEnd(), type);
            }
        }
    }
}