using System;
using System.IO;
using System.Text;

namespace LightNode.Core
{
    public interface IContentFormatter
    {
        string MediaType { get; }
        string Ext { get; }
        Encoding Encoding { get; }
        void Serialize(Stream stream, object obj);
        object Deserialize(Type type, Stream stream);
    }

    public interface IContentFormatterFactory
    {
        IContentFormatter CreateFormatter();
    }
}