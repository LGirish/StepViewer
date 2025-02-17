using MessagePack;

// ReSharper disable UnusedMember.Global
namespace TessModel
{
    [MessagePackObject(keyAsPropertyName: true)]
    public sealed class Point
    {
        [SerializationConstructor]
        public Point(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double X { get; }

        public double Y { get; }

        public double Z { get; }
    }
}
