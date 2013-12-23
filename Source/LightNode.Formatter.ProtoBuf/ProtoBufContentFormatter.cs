using System;

namespace LightNode.Formatter
{
    public class ProtoBufContentFormatter : ContentFormatterBase
    {
        public ProtoBufContentFormatter(string mediaType = "application/x-protobuf", string ext = "proto")
            : base(mediaType, ext)
        {
        }

        public override void Serialize(System.IO.Stream stream, object obj)
        {
            ProtoBuf.Serializer.NonGeneric.Serialize(stream, obj);
        }

        public override object Deserialize(Type type, System.IO.Stream stream)
        {
            return ProtoBuf.Serializer.NonGeneric.Deserialize(type, stream);
        }
    }
}