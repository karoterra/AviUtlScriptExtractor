using System;

namespace AviUtlScriptExtractor
{
    class Scene
    {
        public const int Size = 220;

        public uint SceneIndex { get; }
        public uint Flag { get; }
        public string Name { get; }

        public Scene(ReadOnlySpan<byte> data)
        {
            if (data.Length != Size)
            {
                throw new ArgumentException($"dataサイズは{Size}である必要があります");
            }
            SceneIndex = data.Slice(0, 4).ParseUInt32();
            Flag = data.Slice(4, 4).ParseUInt32();
            Name = data.Slice(8).ToSjisString();
        }
    }
}
