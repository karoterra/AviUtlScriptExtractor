using System;
using System.Linq;
using System.Text;

namespace AviUtlScriptExtractor
{
    static class ByteArrayExtensions
    {
        public static string ToSjisString(this byte[] bytes)
        {
            var sjis = Encoding.GetEncoding(932);
            return sjis.GetString(bytes.TakeWhile(c => c != 0).ToArray());
        }

        public static uint ParseUInt16(this byte[] bytes)
        {
            if (bytes.Length != 2)
            {
                throw new ArgumentException("2バイトのデータを指定してください");
            }
            return bytes[0] + ((uint)bytes[1] << 8);
        }

        public static uint ParseUInt32(this byte[] bytes)
        {
            if (bytes.Length != 4)
            {
                throw new ArgumentException("4バイトのデータを指定してください");
            }
            return bytes[0] + ((uint)bytes[1] << 8) + ((uint)bytes[2] << 16) + ((uint)bytes[3] << 24);
        }
    }
}
