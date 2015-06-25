using Jil;
using LightNode.Core;
using LZ4;
using System;
using System.IO;
using System.Text;

namespace LightNode.Formatter
{
    public class LZ4JilContentFormatter : ContentFormatterBase
    {
        readonly Options options;

        public override string ContentEncoding
        {
            get
            {
                return "lz4";
            }
        }

        public LZ4JilContentFormatter(string mediaType = "application/json", string ext = "json")
            : this(null, new UTF8Encoding(false), mediaType, ext)
        {

        }
        public LZ4JilContentFormatter(Options options, string mediaType = "application/json", string ext = "json")
            : this(options, new UTF8Encoding(false), mediaType, ext)
        {

        }

        public LZ4JilContentFormatter(Encoding encoding, string mediaType = "application/json", string ext = "json")
            : this(null, encoding, mediaType, ext)
        {

        }

        public LZ4JilContentFormatter(Options options, Encoding encoding, string mediaType = "application/json", string ext = "json")
            : base(mediaType, ext, encoding)
        {
            this.options = options;
        }

        public override void Serialize(System.IO.Stream stream, object obj)
        {
            // default LZ4Stream buffer size is 1MB but it's too large on WebService serialization
            using (var lz4 = new LZ4Stream(stream, System.IO.Compression.CompressionMode.Compress, highCompression: false, blockSize: 1024 * 64))
            using (var sw = new StreamWriter(lz4, Encoding ?? new UTF8Encoding(false)))
            {
                JSON.Serialize(obj, sw, options);
            }
        }

        public override object Deserialize(Type type, System.IO.Stream stream)
        {
            using (var gzip = new LZ4Stream(stream, System.IO.Compression.CompressionMode.Decompress, highCompression: false, blockSize: 1024 * 64))
            using (var sr = new StreamReader(gzip, new UTF8Encoding(false)))
            {
                return JSON.Deserialize(sr, type, options);
            }
        }
    }

    public class LZ4JilContentFormatterFactory : IContentFormatterFactory
    {
        public IContentFormatter CreateFormatter()
        {
            return new LZ4JilContentFormatter();
        }
    }
}