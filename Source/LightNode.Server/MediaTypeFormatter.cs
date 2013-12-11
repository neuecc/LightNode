using System;
using System.IO;
using System.Text;

namespace LightNode.Server
{
    public interface IMediaTypeFormatter
    {
        string MediaType { get; }
        string Ext { get; }
        void Serialize(Stream stream, object obj);
        object Deserialize(Type type, Stream stream);
    }

    public class RawOctetStreamMediaTypeFormatter : IMediaTypeFormatter
    {
        public virtual string MediaType
        {
            get { return "application/octet-stream"; }
        }

        public virtual string Ext
        {
            get { return ""; }
        }

        public void Serialize(Stream stream, object obj)
        {
            var bytes = obj as byte[];
            if (bytes != null)
            {
                stream.Write(bytes, 0, bytes.Length);
                return;
            }
            throw new InvalidOperationException();
        }

        public object Deserialize(Type type, Stream stream)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }
    }

    public class JavaScriptMediaTypeFormatter : IMediaTypeFormatter
    {
        public virtual string MediaType
        {
            get { return "application/json"; }
        }

        public virtual string Ext
        {
            get { return "json"; }
        }

        public void Serialize(Stream stream, object obj)
        {
            var json = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(obj);
            var data = Encoding.UTF8.GetBytes(json);
            stream.Write(data, 0, data.Length);
        }

        public object Deserialize(Type type, Stream stream)
        {
            using (var sr = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true))
            {
                return new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize(sr.ReadToEnd(), type);
            }
        }
    }

    public class XmlMediaTypeFormatter : IMediaTypeFormatter
    {
        public virtual string MediaType
        {
            get { return "application/xml"; }
        }

        public virtual string Ext
        {
            get { return "xml"; }
        }

        public void Serialize(Stream stream, object obj)
        {
            new System.Xml.Serialization.XmlSerializer(obj.GetType()).Serialize(stream, obj);
        }

        public object Deserialize(Type type, Stream stream)
        {
            return new System.Xml.Serialization.XmlSerializer(type).Deserialize(stream);
        }
    }
}