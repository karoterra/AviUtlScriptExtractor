using System;

namespace AviUtlScriptExtractor
{
    class ExEdit
    {
        public uint ObjectTypeNum { get; }
        public uint ObjectNum { get; }
        public uint SceneNum { get; }
        public uint LayerNum { get; }
        public uint TrackbarNum { get; }

        public Layer[] Layers { get; }
        public Scene[] Scenes { get; }
        public string[] Trackbars { get; }
        public ObjectType[] ObjectTypes { get; }
        public TimelineObject[] Objects { get; }

        public ExEdit(ReadOnlySpan<byte> data)
        {
            ObjectTypeNum = data.Slice(4, 4).ParseUInt32();
            ObjectNum = data.Slice(8, 4).ParseUInt32();
            SceneNum = data.Slice(0x68, 4).ParseUInt32();
            LayerNum = data.Slice(0x6C, 4).ParseUInt32();
            TrackbarNum = data.Slice(0x7C, 4).ParseUInt32();

            int index = 0x100;
            Layers = new Layer[LayerNum];
            for (uint i = 0; i < LayerNum; i++)
            {
                Layers[i] = new Layer(data.Slice(index, Layer.Size));
                index += Layer.Size;
            }
            Scenes = new Scene[SceneNum];
            for (uint i = 0; i < SceneNum; i++)
            {
                Scenes[i] = new Scene(data.Slice(index, Scene.Size));
                index += Scene.Size;
            }
            Trackbars = new string[TrackbarNum];
            for (uint i = 0; i < TrackbarNum; i++)
            {
                Trackbars[i] = data.Slice(index, 128).ToSjisString();
                index += 128;
            }
            ObjectTypes = new ObjectType[ObjectTypeNum];
            for (uint i = 0; i < ObjectTypeNum; i++)
            {
                ObjectTypes[i] = new ObjectType(data.Slice(index, ObjectType.Size));
                index += ObjectType.Size;
            }
            Objects = new TimelineObject[ObjectNum];
            for (uint i = 0; i < ObjectNum; i++)
            {
                var size = data.Slice(index + (int)TimelineObject.ExtSizeOffset, 4).ParseUInt32();
                size += TimelineObject.BaseSize;
                Objects[i] = new TimelineObject(data.Slice(index, (int)size), ObjectTypes);
                index += (int)size;
            }
        }
    }
}
