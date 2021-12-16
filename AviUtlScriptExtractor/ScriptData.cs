namespace AviUtlScriptExtractor
{
    class ScriptData
    {
        public string Name { get; set; } = string.Empty;
        public string Filename { get;set; } = string.Empty;
        public ScriptType Type => Utils.FilenameToType(Filename);
        public string? Author { get; set; }
        public string? NicoId { get; set; }
        public string? Url { get; set; }
        public string? Comment { get; set; }
        public List<ScriptData> Dependencies { get; } = new List<ScriptData>();

        public ScriptData Clone()
        {
            return new ScriptData
            {
                Name = Name,
                Filename = Filename,
                Author = Author,
                NicoId = NicoId,
                Url = Url,
                Comment = Comment
            };
        }
    }
}
