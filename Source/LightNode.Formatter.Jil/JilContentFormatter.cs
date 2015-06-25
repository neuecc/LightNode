using Jil;
using LightNode.Core;
using System;
using System.IO;
using System.Text;

namespace LightNode.Formatter
{
    public class JilContentFormatter : ContentFormatterBase
    {
        readonly Options options;

        public JilContentFormatter(string mediaType = "application/json", string ext = "json")
            : this(null, new UTF8Encoding(false), mediaType, ext)
        {

        }
        public JilContentFormatter(Options options, string mediaType = "application/json", string ext = "json")
            : this(options, new UTF8Encoding(false), mediaType, ext)
        {

        }

        public JilContentFormatter(Encoding encoding, string mediaType = "application/json", string ext = "json")
            : this(null, encoding, mediaType, ext)
        {

        }

        public JilContentFormatter(Options options, Encoding encoding, string mediaType = "application/json", string ext = "json")
            : base(mediaType, ext, encoding)
        {
            this.options = options;
        }

        public override void Serialize(System.IO.Stream stream, object obj)
        {
            using (var sw = new StreamWriter(stream, Encoding ?? new UTF8Encoding(false)))
            {
                JSON.Serialize(obj, sw);
            }
        }

        public override object Deserialize(Type type, System.IO.Stream stream)
        {
            using (var sr = new StreamReader(stream, Encoding ?? new UTF8Encoding(false)))
            {
                return JSON.Deserialize(sr, type, options);
            }
        }
    }

    public class JilContentFormatterFactory : IContentFormatterFactory
    {
        public IContentFormatter CreateFormatter()
        {
            return new JilContentFormatter();
        }
    }

    public class GZipJilContentFormatter : ContentFormatterBase
    {
        readonly Options options;

        public override string ContentEncoding
        {
            get
            {
                return "gzip";
            }
        }

        public GZipJilContentFormatter(string mediaType = "application/json", string ext = "json")
            : this(null, new UTF8Encoding(false), mediaType, ext)
        {

        }
        public GZipJilContentFormatter(Options options, string mediaType = "application/json", string ext = "json")
            : this(options, new UTF8Encoding(false), mediaType, ext)
        {

        }

        public GZipJilContentFormatter(Encoding encoding, string mediaType = "application/json", string ext = "json")
            : this(null, encoding, mediaType, ext)
        {

        }

        public GZipJilContentFormatter(Options options, Encoding encoding, string mediaType = "application/json", string ext = "json")
            : base(mediaType, ext, encoding)
        {
            this.options = options;
        }

        public override void Serialize(System.IO.Stream stream, object obj)
        {
            using (var gzip = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionLevel.Fastest))
            using (var sw = new StreamWriter(gzip, Encoding ?? new UTF8Encoding(false)))
            {
                JSON.Serialize(obj, sw, options);
            }
        }

        public override object Deserialize(Type type, System.IO.Stream stream)
        {
            using (var gzip = new System.IO.Compression.GZipStream(stream, System.IO.Compression.CompressionMode.Decompress))
            using (var sr = new StreamReader(gzip, Encoding ?? new UTF8Encoding(false)))
            {
                return JSON.Deserialize(sr, type, options);
            }
        }
    }

    public class GZipJilContentFormatterFactory : IContentFormatterFactory
    {
        public IContentFormatter CreateFormatter()
        {
            return new GZipJilContentFormatter();
        }
    }
}