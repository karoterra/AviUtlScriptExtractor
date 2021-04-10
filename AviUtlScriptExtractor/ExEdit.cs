using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public ExEdit(byte[] data)
        {
            ObjectTypeNum = data.Skip(4).Take(4).ToArray().ParseUInt32();
            ObjectNum = data.Skip(8).Take(4).ToArray().ParseUInt32();
            SceneNum = data.Skip(0x68).Take(4).ToArray().ParseUInt32();
            LayerNum = data.Skip(0x6C).Take(4).ToArray().ParseUInt32();
            TrackbarNum = data.Skip(0x7C).Take(4).ToArray().ParseUInt32();

            int index = 0x100;
            Layers = new Layer[LayerNum];
            for (uint i = 0; i < LayerNum; i++)
            {
                Layers[i] = new Layer(data.Skip(index).Take(Layer.Size).ToArray());
                index += Layer.Size;
            }
            Scenes = new Scene[SceneNum];
            for (uint i = 0; i < SceneNum; i++)
            {
                Scenes[i] = new Scene(data.Skip(index).Take(Scene.Size).ToArray());
                index += Scene.Size;
            }
            Trackbars = new string[TrackbarNum];
            for (uint i = 0; i < TrackbarNum; i++)
            {
                Trackbars[i] = data.Skip(index).Take(128).ToArray().ToSjisString();
                index += 128;
            }
            ObjectTypes = new ObjectType[ObjectTypeNum];
            for (uint i = 0; i < ObjectTypeNum; i++)
            {
                ObjectTypes[i] = new ObjectType(data.Skip(index).Take(ObjectType.Size).ToArray());
                index += ObjectType.Size;
            }
            Objects = new TimelineObject[ObjectNum];
            for (uint i = 0; i < ObjectNum; i++)
            {
                var size = data.Skip(index + (int)TimelineObject.ExtSizeOffset).Take(4).ToArray().ParseUInt32();
                size += TimelineObject.BaseSize;
                Objects[i] = new TimelineObject(data.Skip(index).Take((int)size).ToArray());
                index += (int)size;
            }
        }
    }
}
