using MessagePack;

namespace TessModel
{
    [MessagePackObject(keyAsPropertyName: true)]
    public sealed class Triangle
    {
        [SerializationConstructor]
        public Triangle(uint i, uint j, uint k)
        {
            I = i;
            J = j;
            K = k;
        }

        public uint I { get; }

        public uint J { get; }

        public uint K { get; }
    }
}
