using MsgPack.Serialization;
using System;

namespace LightNode.Formatter
{
    public class MsgPackContentFormatter : ContentFormatterBase
    {
        readonly MsgPack.Serialization.SerializationContext serializationContext;

        public MsgPackContentFormatter(string mediaType = "application/x-msgpack", string ext = "mpk")
            : this(SerializationContext.Default, mediaType, ext)
        {
        }

        public MsgPackContentFormatter(MsgPack.Serialization.SerializationContext serializationContext, string mediaType = "application/x-msgpack", string ext = "mpk")
            : base(mediaType, ext, null)
        {
            this.serializationContext = serializationContext;
        }

        public override void Serialize(System.IO.Stream stream, object obj)
        {
            using (var packer = MsgPack.Packer.Create(stream))
            {
                var serializer = serializationContext.GetSerializer(obj.GetType());
                serializer.PackTo(packer, obj);
            }
        }

        public override object Deserialize(Type type, System.IO.Stream stream)
        {
            using (var packer = MsgPack.Unpacker.Create(stream))
            {
                var serializer = serializationContext.GetSerializer(type);
                return serializer.UnpackFrom(packer);
            }
        }
    }
}