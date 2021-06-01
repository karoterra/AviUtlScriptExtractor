using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AviUtlScriptExtractor
{
    class Author
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("scripts")]
        public List<Script> Scripts { get; set; }
    }
}
