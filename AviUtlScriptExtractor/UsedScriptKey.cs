using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AviUtlScriptExtractor
{
    internal class UsedScriptKey
    {
        /// <summary>
        /// スクリプト名
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// スクリプトの種類
        /// </summary>
        public ScriptType Type { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is UsedScriptKey other &&
                Name == other.Name && Type == other.Type;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Type.GetHashCode();
        }
    }
}
