using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.IO;

namespace AviUtlScriptExtractor
{
    class Script
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("nicoid")]
        public string NicoId { get; set; }

        [JsonPropertyName("url")]
        public string Url { get; set; }

        [JsonPropertyName("dependencies")]
        public List<string> Dependencies { get; set; }

        [JsonIgnore]
        public Author Author { get; set; }

        [JsonIgnore]
        public ScriptType Type
        {
            get
            {
                switch (Path.GetExtension(Name))
                {
                    case ".anm":
                        return ScriptType.Anm;
                    case ".obj":
                        return ScriptType.Obj;
                    case ".scn":
                        return ScriptType.Scn;
                    case ".cam":
                        return ScriptType.Cam;
                    case ".tra":
                        return ScriptType.Tra;
                }
                return ScriptType.Other;
            }
        }
    }
}
