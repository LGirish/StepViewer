using MessagePack;
using MessagePack.Formatters;

namespace TessModel
{
    public sealed class TriangleFormatter : IMessagePackFormatter<Triangle>
    {
        public void Serialize(ref MessagePackWriter writer, Triangle value, MessagePackSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNil();
                return;
            }

            writer.WriteArrayHeader(3);
            writer.Write(value.I);
            writer.Write(value.J);
            writer.Write(value.K);
        }

        public Triangle Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                return null;
            }

            var count = reader.ReadArrayHeader();
            if (count != 3)
            {
                throw new MessagePackSerializationException("Invalid Triangle data: expected array of size 3");
            }

            uint i = reader.ReadUInt32();
            uint j = reader.ReadUInt32();
            uint k = reader.ReadUInt32();

            return new Triangle(i, j, k);
        }
    }
}
