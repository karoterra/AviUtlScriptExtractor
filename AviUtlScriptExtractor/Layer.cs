using System;
using System.Linq;

namespace AviUtlScriptExtractor
{
    class Layer
    {
        public const int Size = 76;

        public uint SceneIndex { get; }
        public uint LayerIndex { get; }
        public uint Flag { get; }
        public string Name { get; }

        public Layer(byte[] data)
        {
            if (data.Length != Size)
            {
                throw new ArgumentException($"dataサイズは{Size}である必要があります");
            }
            SceneIndex = data.Take(4).ToArray().ParseUInt32();
            LayerIndex = data.Skip(4).Take(4).ToArray().ParseUInt32();
            Flag = data.Skip(8).Take(4).ToArray().ParseUInt32();
            Name = data.Skip(12).ToArray().ToSjisString();
        }
    }
}
