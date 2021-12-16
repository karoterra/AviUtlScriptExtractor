using System.Text.Json.Serialization;
using System.Collections.Generic;
using System.IO;

namespace AviUtlScriptExtractor
{
    class Script
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("nicoid")]
        public string? NicoId { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("dependencies")]
        public List<string> Dependencies { get; } = new List<string>();

        [JsonPropertyName("comment")]
        public string? Comment { get; set; }

        [JsonIgnore]
        public Author Author { get; set; }

        [JsonIgnore]
        public ScriptType Type => Path.GetExtension(Name) switch
        {
            ".anm" => ScriptType.Anm,
            ".obj" => ScriptType.Obj,
            ".scn" => ScriptType.Scn,
            ".cam" => ScriptType.Cam,
            ".tra" => ScriptType.Tra,
            _ => ScriptType.Other,
        };
    }
}
