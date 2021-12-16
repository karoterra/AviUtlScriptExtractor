using System.Text.Json.Serialization;

namespace AviUtlScriptExtractor
{
    public class AuthorSetting
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("scripts")]
        public List<ScriptSetting?> Scripts { get; set; } = new List<ScriptSetting?>();
    }
}
