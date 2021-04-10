using System;
using System.Linq;

namespace AviUtlScriptExtractor
{
    class ObjectType
    {
        public const int Size = 112;
        public byte[] Header { get; }
        public string Name { get; }

        public ObjectType(byte[] data)
        {
            if (data.Length != Size)
            {
                throw new ArgumentException($"dataサイズは{Size}である必要があります");
            }
            Header = data.Take(16).ToArray();
            Name = data.Skip(16).ToArray().ToSjisString();
        }
    }
}
