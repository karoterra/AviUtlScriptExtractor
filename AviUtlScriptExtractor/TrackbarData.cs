using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AviUtlScriptExtractor
{
    class TrackbarData
    {
        public readonly Dictionary<int, string> TypeNames = new Dictionary<int, string>()
        {
            {0, "移動無し"},
            {1, "直線移動"},
            {2, "曲線移動"},
            {3, "瞬間移動"},
            {4, "中間点無視"},
            {5, "移動量指定"},
            {6, "ランダム移動"},
            {7, "加減速"},
            {8, "反復移動"},
            {0xf, "スクリプト"},
        };

        public uint Current { get; }
        public uint Next { get; }
        public int Flag { get; }
        public int Type { get; }
        public int ScriptIndex { get; }
        public uint Parameter { get; }

        public TrackbarData(uint current, uint next, uint transition, uint param)
        {
            Current = current;
            Next = next;
            Parameter = param;
            Flag = (int)(transition & 0xF0) >> 4;
            Type = (int)(transition & 0xF);
            ScriptIndex = (int)(transition >> 16);
        }
    }
}
