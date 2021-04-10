using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AviUtlScriptExtractor
{
    /// <summary>
    /// AviUtlのプロジェクトファイル(*.aup)を読み込むためのクラス
    /// 参考：https://scrapbox.io/ePi5131/aupファイルの構造
    /// </summary>
    class AviUtlProject
    {
        const string Header = "AviUtl ProjectFile version 0.18\0";
        const string FilterHeader = "FilterProject 0.1\0";
        readonly Encoding Sjis;

        public uint HandleSize { get; }
        public uint Flag { get; }
        public string EditFilename { get; }
        public string OutputFilename { get; }
        byte[] projectFilename;
        public string ProjectFilename
        {
            get
            {
                return projectFilename.ToSjisString();
            }
        }
        public int FrameNum { get; }
        public uint[] Video { get; }
        public uint[] Audio { get; }
        public uint[] Field1 { get; }
        public uint[] Field2 { get; }
        public byte[] Inter { get; }
        public byte[] Index24Fps { get; }
        public byte[] EditFlag { get; }
        public byte[] Config { get; }
        public byte[] Field3 { get; }
        public byte[] Vcm { get; }

        public Dictionary<string, byte[]> Filters { get; }

        public AviUtlProject(BinaryReader reader)
        {
            Sjis = Encoding.GetEncoding(932);
            var baseStream = reader.BaseStream;

            var header = Sjis.GetString(reader.ReadBytes(32));
            if (header != Header)
            {
                throw new FileFormatException("AviUtl ProjectFileヘッダが見つかりません");
            }

            HandleSize = reader.ReadUInt32();
            Flag = reader.ReadUInt32();
            EditFilename = reader.ReadBytes(260).ToSjisString();
            OutputFilename = reader.ReadBytes(260).ToSjisString();
            projectFilename = new byte[0x4C07DC];
            Decomp(reader, projectFilename);
            FrameNum = reader.ReadInt32();
            Video = new uint[FrameNum];
            Audio = new uint[FrameNum];
            Field1 = new uint[FrameNum];
            Field2 = new uint[FrameNum];
            Inter = new byte[FrameNum];
            Index24Fps = new byte[FrameNum];
            EditFlag = new byte[FrameNum];
            Config = new byte[FrameNum];
            Field3 = new byte[FrameNum];
            Vcm = new byte[FrameNum];
            SkipToFooter(reader);

            Filters = new Dictionary<string, byte[]>();
            while (baseStream.Position != baseStream.Length)
            {
                header = Sjis.GetString(reader.ReadBytes(18));
                if (header != FilterHeader)
                {
                    throw new FileFormatException("FilterProjectヘッダが見つかりません");
                }
                var nameLen = reader.ReadInt32();
                string name = reader.ReadBytes(nameLen).ToSjisString();
                var dataSize = reader.ReadInt32();
                var data = new byte[dataSize];
                Decomp(reader, data);
                Filters.Add(name, data);
            }
        }

        void SkipToFooter(BinaryReader reader)
        {
            int index = 0;
            byte[] footer = Sjis.GetBytes(Header);
            while (true)
            {
                byte data = reader.ReadByte();
                if (data == footer[index])
                {
                    index++;
                }
                else
                {
                    index = 0;
                }
                if (index == footer.Length)
                {
                    return;
                }
            }
        }

        public static void Decomp(BinaryReader reader, byte[] buf)
        {
            int index = 0;
            while (index < buf.Length)
            {
                byte size1 = reader.ReadByte();
                if ((size1 & 0x80) != 0)
                {
                    if ((size1 & 0x7F) != 0)
                    {
                        size1 &= 0x7F;
                        byte data = reader.ReadByte();
                        for (int i = 0; i < size1; i++)
                        {
                            buf[index + i] = data;
                        }
                        index += size1;
                    }
                    else
                    {
                        var _size2 = reader.ReadBytes(3);
                        int size2 = _size2[0] + (_size2[1] << 8) + (_size2[2] << 16);
                        byte data = reader.ReadByte();
                        for (int i = 0; i < size2; i++)
                        {
                            buf[index + i] = data;
                        }
                        index += size2;
                    }
                }
                else
                {
                    if (size1 != 0)
                    {
                        for (int i = 0; i < size1; i++)
                        {
                            buf[index + i] = reader.ReadByte();
                        }
                        index += size1;
                    }
                    else
                    {
                        var _size2 = reader.ReadBytes(3);
                        int size2 = _size2[0] + (_size2[1] << 8) + (_size2[2] << 16);
                        for (int i = 0; i < size2; i++)
                        {
                            buf[index + i] = reader.ReadByte();
                        }
                        index += size2;
                    }
                }
            }
        }
    }
}
