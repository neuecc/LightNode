using System;

namespace LightNode.Formatter
{
    public class MsgPackContentFormatter : ContentFormatterBase
    {
        public MsgPackContentFormatter(string mediaType = "application/x-msgpack", string ext = "mpk")
            : base(mediaType, ext)
        {
        }

        public override void Serialize(System.IO.Stream stream, object obj)
        {
            using (var packer = MsgPack.Packer.Create(stream))
            {
                var serializer = MsgPack.Serialization.MessagePackSerializer.Create(obj.GetType());
                serializer.PackTo(packer, obj);
            }
        }

        public override object Deserialize(Type type, System.IO.Stream stream)
        {
            using (var packer = MsgPack.Unpacker.Create(stream))
            {
                var serializer = MsgPack.Serialization.MessagePackSerializer.Create(type);
                return serializer.UnpackFrom(packer);
            }
        }
    }
}