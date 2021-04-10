using System;
using System.Linq;

namespace AviUtlScriptExtractor
{
    class Scene
    {
        public const int Size = 220;

        public uint SceneIndex { get; }
        public uint Flag { get; }
        public string Name { get; }

        public Scene(byte[] data)
        {
            if (data.Length != Size)
            {
                throw new ArgumentException($"dataサイズは{Size}である必要があります");
            }
            SceneIndex = data.Take(4).ToArray().ParseUInt32();
            Flag = data.Skip(4).Take(4).ToArray().ParseUInt32();
            Name = data.Skip(8).ToArray().ToSjisString();
        }
    }
}
