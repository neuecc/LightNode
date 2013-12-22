using LightNode.Core;
using System;
using System.IO;
using System.Text;

namespace LightNode.Formatters
{
    public class JavaScriptContentTypeFormatter : ContentFormatterBase
    {
        public JavaScriptContentTypeFormatter(string mediaType = "application/json", string ext = "json")
            : base(mediaType, ext)
        {

        }

        public override void Serialize(Stream stream, object obj)
        {
            var json = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(obj);
            var data = Encoding.UTF8.GetBytes(json);
            stream.Write(data, 0, data.Length);
        }

        public override object Deserialize(Type type, Stream stream)
        {
            using (var sr = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true))
            {
                return new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize(sr.ReadToEnd(), type);
            }
        }
    }
}