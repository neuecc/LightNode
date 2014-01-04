using LightNode.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LightNode.Formatter
{
    public abstract class ContentFormatterBase : IContentFormatter
    {
        readonly string mediaType;
        readonly string ext;

        public ContentFormatterBase(string mediaType, string ext)
        {
            this.mediaType = mediaType;
            this.ext = ext;
        }

        public string MediaType
        {
            get { return mediaType; }
        }

        public string Ext
        {
            get { return ext; }
        }

        public abstract void Serialize(Stream stream, object obj);

        public abstract object Deserialize(Type type, Stream stream);
    }

    public class TextContentFormatter : ContentFormatterBase
    {
        public Encoding Encoding { get; protected set; }

        public TextContentFormatter(string mediaType = "text/plain; charset=utf-8", string ext = "txt")
            : this(Encoding.UTF8, mediaType, ext)
        {

        }

        public TextContentFormatter(Encoding encoding, string mediaType = "text/plain", string ext = "txt")
            : base(mediaType, ext)
        {
            this.Encoding = encoding;
        }

        public override void Serialize(Stream stream, object obj)
        {
            var str = obj as string;
            if (str != null)
            {
                var bytes = Encoding.GetBytes(str);
                stream.Write(bytes, 0, bytes.Length);
                return;
            }
            throw new InvalidOperationException();
        }

        public override object Deserialize(Type type, Stream stream)
        {
            using (var sr = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true))
            {
                return sr.ReadToEnd();
            }
        }
    }

    public class HtmlContentFormatter : TextContentFormatter
    {
        public HtmlContentFormatter(string mediaType = "text/html; charset=utf-8", string ext = "htm|html")
            : this(Encoding.UTF8, mediaType, ext)
        {

        }

        public HtmlContentFormatter(Encoding encoding, string mediaType = "text/html", string ext = "htm|html")
            : base(mediaType, ext)
        {
            this.Encoding = encoding;
        }
    }

    public class RawOctetStreamContentFormatter : ContentFormatterBase
    {
        public RawOctetStreamContentFormatter(string mediaType = "application/octet-stream", string ext = "")
            : base(mediaType, ext)
        {

        }

        public override void Serialize(Stream stream, object obj)
        {
            var bytes = obj as byte[];
            if (bytes != null)
            {
                stream.Write(bytes, 0, bytes.Length);
                return;
            }
            throw new InvalidOperationException();
        }

        public override object Deserialize(Type type, Stream stream)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }

    public class XmlContentFormatter : ContentFormatterBase
    {
        public XmlContentFormatter(string mediaType = "application/xml; charset=utf-8", string ext = "xml")
            : base(mediaType, ext)
        {

        }

        public override void Serialize(Stream stream, object obj)
        {
            new System.Xml.Serialization.XmlSerializer(obj.GetType()).Serialize(stream, obj);
        }

        public override object Deserialize(Type type, Stream stream)
        {
            return new System.Xml.Serialization.XmlSerializer(type).Deserialize(stream);
        }
    }

    public class DataContractContentFormatter : ContentFormatterBase
    {
        public DataContractContentFormatter(string mediaType = "application/xml; charset=utf-8", string ext = "xml")
            : base(mediaType, ext)
        {

        }

        public override void Serialize(Stream stream, object obj)
        {
            var serializer = new System.Runtime.Serialization.DataContractSerializer(obj.GetType());
            serializer.WriteObject(stream, obj);
        }

        public override object Deserialize(Type type, Stream stream)
        {
            var serializer = new System.Runtime.Serialization.DataContractSerializer(type);
            return serializer.ReadObject(stream);
        }
    }

    public class DataContractJsonContentFormatter : ContentFormatterBase
    {
        public DataContractJsonContentFormatter(string mediaType = "application/json; charset=utf-8", string ext = "json")
            : base(mediaType, ext)
        {

        }

        public override void Serialize(Stream stream, object obj)
        {
            var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(obj.GetType());
            serializer.WriteObject(stream, obj);
        }

        public override object Deserialize(Type type, Stream stream)
        {
            var serializer = new System.Runtime.Serialization.Json.DataContractJsonSerializer(type);
            return serializer.ReadObject(stream);
        }
    }
}