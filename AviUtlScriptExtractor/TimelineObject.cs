using System;
using System.Collections.Generic;
using System.Linq;

namespace AviUtlScriptExtractor
{
    class TimelineObject
    {
        public const uint BaseSize = 0x5C8;
        public const uint ExtSizeOffset = 0xF4;

        public uint Field1 { get; }
        public uint Field2 { get; }
        public uint StartFrame { get; }
        public uint EndFrame { get; }
        public string Preview { get; }
        public uint Keyframe { get; }
        public uint[] ObjectTypes { get; }

        public uint TrackbarNum { get; }
        public uint ExtSize { get; }

        public uint LayerIndex { get; }
        public uint SceneIndex { get; }

        public TrackbarData[] Trackbars { get; }

        public byte[][] ExtData { get; }

        public TimelineObject(byte[] data, ObjectType[] objectTypes)
        {
            if (data.Length < BaseSize)
            {
                throw new ArgumentException($"dataサイズは少なくとも{BaseSize}以上である必要があります");
            }
            Field1 = data.Take(4).ToArray().ParseUInt32();
            Field2 = data.Skip(4).Take(4).ToArray().ParseUInt32();
            StartFrame = data.Skip(8).Take(4).ToArray().ParseUInt32();
            EndFrame = data.Skip(12).Take(4).ToArray().ParseUInt32();
            Preview = data.Skip(0x10).Take(64).ToArray().ToSjisString();
            Keyframe = data.Skip(0x50).Take(4).ToArray().ParseUInt32();
            TrackbarNum = data.Skip(0xF0).Take(2).ToArray().ParseUInt16();
            ExtSize = data.Skip(0xF4).Take(4).ToArray().ParseUInt32();
            LayerIndex = data.Skip(0x5C0).Take(4).ToArray().ParseUInt32();
            SceneIndex = data.Skip(0x5C4).Take(4).ToArray().ParseUInt32();

            Trackbars = new TrackbarData[TrackbarNum];
            for (int i = 0; i < TrackbarNum; i++)
            {
                var current = data.Skip(0xF8 + i * 4).Take(4).ToArray().ParseUInt32();
                var next = data.Skip(0x1F8 + i * 4).Take(4).ToArray().ParseUInt32();
                var transition = data.Skip(0x2F8 + i * 4).Take(4).ToArray().ParseUInt32();
                var param = data.Skip(0x4C0 + i * 4).Take(4).ToArray().ParseUInt32();
                Trackbars[i] = new TrackbarData(current, next, transition, param);
            }

            var objTypes = new List<uint>();
            var objSizes = new List<uint>();
            var objOffsets = new List<uint>();
            for (int i = 0; i < 12; i++)
            {
                var objectType = data.Skip(0x54 + i * 12).Take(4).ToArray().ParseUInt32();
                if (objectType == 0xFFFF_FFFF)
                {
                    break;
                }
                objTypes.Add(objectType);
                objSizes.Add(objectTypes[objectType].ExtSize);
                var offset = data.Skip(0x54 + i * 12 + 8).Take(4).ToArray().ParseUInt32();
                objOffsets.Add(offset);
            }
            ObjectTypes = objTypes.ToArray();
            ExtData = new byte[ObjectTypes.Length][];
            for (int i = 0; i < ObjectTypes.Length; i++)
            {
                if (ExtSize > 0)
                {
                    ExtData[i] = data.Skip(0x5C8 + (int)objOffsets[i]).Take((int)objSizes[i]).ToArray();
                }
                else
                {
                    ExtData[i] = new byte[0];
                }
            }
        }
    }
}
