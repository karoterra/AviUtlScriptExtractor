using System;

namespace AviUtlScriptExtractor
{
    class Layer
    {
        public const int Size = 76;

        public uint SceneIndex { get; }
        public uint LayerIndex { get; }
        public uint Flag { get; }
        public string Name { get; }

        public Layer(ReadOnlySpan<byte> data)
        {
            if (data.Length != Size)
            {
                throw new ArgumentException($"dataサイズは{Size}である必要があります");
            }
            SceneIndex = data.Slice(0, 4).ParseUInt32();
            LayerIndex = data.Slice(4, 4).ParseUInt32();
            Flag = data.Slice(8, 4).ParseUInt32();
            Name = data.Slice(12).ToSjisString();
        }
    }
}
