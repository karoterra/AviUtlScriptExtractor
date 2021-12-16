using System.Text.Json.Serialization;

namespace AviUtlScriptExtractor
{
    public class ScriptSetting
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("nicoid")]
        public string? NicoId { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("dependencies")]
        public List<string?> Dependencies { get; set; } = new List<string?>();

        [JsonPropertyName("comment")]
        public string? Comment { get; set; }
    }
}
