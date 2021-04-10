using System;
using System.Linq;

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

        public ObjectType(byte[] data)
        {
            if (data.Length != Size)
            {
                throw new ArgumentException($"dataサイズは{Size}である必要があります");
            }
            Field1 = data.Take(4).ToArray().ParseUInt32();
            Field2 = data.Skip(4).Take(4).ToArray().ParseUInt32();
            Field3 = data.Skip(8).Take(4).ToArray().ParseUInt32();
            ExtSize = data.Skip(12).Take(4).ToArray().ParseUInt32();
            Name = data.Skip(16).ToArray().ToSjisString();
        }
    }
}
