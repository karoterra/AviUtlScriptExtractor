using System;
using System.Collections.Generic;

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

        public TimelineObject(ReadOnlySpan<byte> data, ObjectType[] objectTypes)
        {
            if (data.Length < BaseSize)
            {
                throw new ArgumentException($"dataサイズは少なくとも{BaseSize}以上である必要があります");
            }
            Field1 = data.Slice(0, 4).ParseUInt32();
            Field2 = data.Slice(4, 4).ParseUInt32();
            StartFrame = data.Slice(8, 4).ParseUInt32();
            EndFrame = data.Slice(12, 4).ParseUInt32();
            Preview = data.Slice(0x10, 64).ToSjisString();
            Keyframe = data.Slice(0x50, 4).ParseUInt32();
            TrackbarNum = data.Slice(0xF0, 2).ParseUInt16();
            ExtSize = data.Slice(0xF4, 4).ParseUInt32();
            LayerIndex = data.Slice(0x5C0, 4).ParseUInt32();
            SceneIndex = data.Slice(0x5C4, 4).ParseUInt32();

            Trackbars = new TrackbarData[TrackbarNum];
            for (int i = 0; i < TrackbarNum; i++)
            {
                var current = data.Slice(0xF8 + i * 4, 4).ParseUInt32();
                var next = data.Slice(0x1F8 + i * 4, 4).ParseUInt32();
                var transition = data.Slice(0x2F8 + i * 4, 4).ParseUInt32();
                var param = data.Slice(0x4C0 + i * 4, 4).ParseUInt32();
                Trackbars[i] = new TrackbarData(current, next, transition, param);
            }

            var objTypes = new List<uint>();
            var objSizes = new List<uint>();
            var objOffsets = new List<uint>();
            for (int i = 0; i < 12; i++)
            {
                var objectType = data.Slice(0x54 + i * 12, 4).ParseUInt32();
                if (objectType == 0xFFFF_FFFF)
                {
                    break;
                }
                objTypes.Add(objectType);
                objSizes.Add(objectTypes[objectType].ExtSize);
                var offset = data.Slice(0x54 + i * 12 + 8, 4).ParseUInt32();
                objOffsets.Add(offset);
            }
            ObjectTypes = objTypes.ToArray();
            ExtData = new byte[ObjectTypes.Length][];
            for (int i = 0; i < ObjectTypes.Length; i++)
            {
                if (ExtSize > 0)
                {
                    ExtData[i] = data.Slice(0x5C8 + (int)objOffsets[i], (int)objSizes[i]).ToArray();
                }
                else
                {
                    ExtData[i] = new byte[0];
                }
            }
        }
    }
}
