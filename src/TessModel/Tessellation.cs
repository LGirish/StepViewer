using System;
using MessagePack;

namespace TessModel
{
    [MessagePackObject(keyAsPropertyName: true)]
    public sealed class Tessellation
    {
        [SerializationConstructor]
        public Tessellation(
            Point[] points, Triangle[] triangles)
        {
            Points = points;
            Triangles = triangles;
        }

        public Point[] Points { get; private set; } = Array.Empty<Point>();

        public Triangle[] Triangles { get; private set; } = Array.Empty<Triangle>();
    }
}
