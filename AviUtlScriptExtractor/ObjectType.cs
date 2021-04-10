using System;

namespace AviUtlScriptExtractor
{
    class ObjectType
    {
        public const int Size = 112;

        public uint Field1 { get; }
        public uint Field2 { get; }
        public uint Field3 { get; }
        public uint ExtSize { get; }
        public string Name { get; }

        public ObjectType(ReadOnlySpan<byte> data)
        {
            if (data.Length != Size)
            {
                throw new ArgumentException($"dataサイズは{Size}である必要があります");
            }
            Field1 = data.Slice(0, 4).ParseUInt32();
            Field2 = data.Slice(4, 4).ParseUInt32();
            Field3 = data.Slice(8, 4).ParseUInt32();
            ExtSize = data.Slice(12, 4).ParseUInt32();
            Name = data.Slice(16).ToSjisString();
        }
    }
}
