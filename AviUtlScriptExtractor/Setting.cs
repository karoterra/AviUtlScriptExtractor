using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AviUtlScriptExtractor
{
    class Setting
    {
        [JsonPropertyName("authors")]
        public List<Author> Authors { get; set; }

        public void CompleteRead()
        {
            foreach (var author in Authors)
            {
                foreach (var script in author.Scripts)
                {
                    script.Author = author;
                }
            }
        }

        public Dictionary<string, Script> GetScripts()
        {
            var scripts = new Dictionary<string, Script>();

            foreach (var author in Authors)
            {
                foreach (var script in author.Scripts)
                {
                    scripts.Add(script.Name, script);
                }
            }

            return scripts;
        }
    }
}
