using LightNode.Core;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace LightNode.Formatter
{
    public class JsonContentFormatter : LightNode.Formatter.ContentFormatterBase
    {
        readonly JsonSerializer serializer;

        public JsonContentFormatter(string mediaType = "application/json", string ext = "json")
            : this(new JsonSerializer(), mediaType, ext)
        {
        }

        public JsonContentFormatter(JsonSerializer serializer, string mediaType = "application/json", string ext = "json")
            : this(serializer, new UTF8Encoding(false), mediaType, ext)
        {
        }

        public JsonContentFormatter(Encoding encoding, string mediaType = "application/json", string ext = "json")
            : this(new JsonSerializer(), encoding, mediaType, ext)
        {
        }

        public JsonContentFormatter(JsonSerializer serializer, Encoding encoding, string mediaType = "application/json", string ext = "json")
            : base(mediaType, ext, encoding)
        {
            this.serializer = serializer;
        }

        public override void Serialize(System.IO.Stream stream, object obj)
        {
            using (var sw = new StreamWriter(stream, Encoding ?? new UTF8Encoding(false)))
            {
                serializer.Serialize(sw, obj);
            }
        }

        public override object Deserialize(Type type, System.IO.Stream stream)
        {
            using (var sr = new StreamReader(stream, Encoding ?? new UTF8Encoding(false)))
            {
                return serializer.Deserialize(sr, type);
            }
        }
    }

    public class JsonContentFormatterFactory : IContentFormatterFactory
    {
        public IContentFormatter CreateFormatter()
        {
            return new JsonContentFormatter();
        }
    }

    public class GZipJsonContentFormatter : LightNode.Formatter.ContentFormatterBase
    {
        readonly JsonSerializer serializer;

        public override string ContentEncoding
        {
            get
            {
                return "gzip";
            }
        }

        public GZipJsonContentFormatter(string mediaType = "application/json", string ext = "json")
            : this(new JsonSerializer(), mediaType, ext)
        {
        }

        public GZipJsonContentFormatter(JsonSerializer serializer, string mediaType = "application/json", string ext = "json")
            : this(serializer, new UTF8Encoding(false), mediaType, ext)
        {
        }

        public GZipJsonContentFormatter(Encoding encoding, string mediaType = "application/json", string ext = "json")
            : this(new JsonSerializer(), encoding, mediaType, ext)
        {
        }

        public GZipJsonContentFormatter(JsonSerializer serializer, Encoding encoding, string mediaType = "application/json", string ext = "json")
            : base(mediaType, ext, encoding)
        {
            this.serializer = serializer;
        }

        public override void Serialize(System.IO.Stream stream, object obj)
        {
            using (var gzip = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionLevel.Fastest))
            using (var sw = new StreamWriter(gzip, Encoding ?? new UTF8Encoding(false)))
            {
                serializer.Serialize(sw, obj);
            }
        }

        public override object Deserialize(Type type, System.IO.Stream stream)
        {
            using (var gzip = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress))
            using (var sr = new StreamReader(gzip, Encoding ?? new UTF8Encoding(false)))
            {
                return serializer.Deserialize(sr, type);
            }
        }
    }

    public class GZipJsonContentFormatterFactory : IContentFormatterFactory
    {
        public IContentFormatter CreateFormatter()
        {
            return new GZipJsonContentFormatter();
        }
    }
}