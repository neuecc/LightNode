using LightNode.Core;
using System;
using System.IO;
using System.Text;

namespace LightNode.Formatter
{
    public class JavaScriptContentFormatter : ContentFormatterBase
    {
        public JavaScriptContentFormatter(string mediaType = "application/json", string ext = "json")
            : this(null, mediaType, ext)
        {

        }
        public JavaScriptContentFormatter(Encoding encoding, string mediaType = "application/json", string ext = "json")
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

    public class JavaScriptContentFormatterFactory : IContentFormatterFactory
    {
        public IContentFormatter CreateFormatter()
        {
            return new JavaScriptContentFormatter();
        }
    }

    public class GZipJavaScriptContentFormatter : ContentFormatterBase
    {
        public override string ContentEncoding
        {
            get
            {
                return "gzip";
            }
        }

        public GZipJavaScriptContentFormatter(string mediaType = "application/json", string ext = "json")
            : this(null, mediaType, ext)
        {

        }
        public GZipJavaScriptContentFormatter(Encoding encoding, string mediaType = "application/json", string ext = "json")
            : base(mediaType, ext, encoding ?? Encoding.UTF8)
        {

        }

        public override void Serialize(Stream stream, object obj)
        {
            var json = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(obj);
            var data = this.Encoding.GetBytes(json);
            using (var gzip = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionLevel.Fastest))
            {
                gzip.Write(data, 0, data.Length);
            }
        }

        public override object Deserialize(Type type, Stream stream)
        {
            using (var gzip = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress))
            using (var sr = new StreamReader(gzip, this.Encoding))
            {
                return new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize(sr.ReadToEnd(), type);
            }
        }
    }

    public class GZipJavaScriptContentFormatterFactory : IContentFormatterFactory
    {
        public IContentFormatter CreateFormatter()
        {
            return new GZipJavaScriptContentFormatter();
        }
    }
}