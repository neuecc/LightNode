using System;
using System.IO;
using System.Text;

namespace LightNode.Server
{
    public interface IContentFormatter
    {
        string MediaType { get; }
        string Ext { get; }
        void Serialize(Stream stream, object obj);
        object Deserialize(Type type, Stream stream);
    }

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

    public class TextContentTypeFormatter : ContentFormatterBase
    {
        public Encoding Encoding { get; private set; }

        public TextContentTypeFormatter(string mediaType = "text/plain", string ext = "txt")
            : this(Encoding.UTF8, mediaType, ext)
        {

        }

        public TextContentTypeFormatter(Encoding encoding, string mediaType = "text/plain", string ext = "txt")
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
            using (var sr = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true))
            {
                return sr.ReadToEnd();
            }
        }
    }

    public class RawOctetStreamContentTypeFormatter : ContentFormatterBase
    {
        public RawOctetStreamContentTypeFormatter(string mediaType = "application/octet-stream", string ext = "")
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

    public class XmlContentTypeFormatter : ContentFormatterBase
    {
        public XmlContentTypeFormatter(string mediaType = "application/xml", string ext = "xml")
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
}