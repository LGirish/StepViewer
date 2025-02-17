using MessagePack;
using MessagePack.Formatters;

namespace TessModel
{
    public sealed class PointFormatter : IMessagePackFormatter<Point>
    {
        public void Serialize(ref MessagePackWriter writer, Point value, MessagePackSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNil();
                return;
            }

            writer.WriteArrayHeader(3);
            writer.Write(value.X);
            writer.Write(value.Y);
            writer.Write(value.Z);
        }

        public Point Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
            {
                return null;
            }

            var count = reader.ReadArrayHeader();
            if (count != 3)
            {
                throw new MessagePackSerializationException("Invalid Point data: expected array of size 3");
            }

            double i = reader.ReadDouble();
            double j = reader.ReadDouble();
            double k = reader.ReadDouble();

            return new Point(i, j, k);
        }
    }
}
