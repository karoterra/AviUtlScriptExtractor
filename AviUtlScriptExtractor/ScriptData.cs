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

        private int _count = 1;
        public int Count => _count;

        public void Increment() => _count++;

        public object GetValue(ColumnType column)
        {
            return column switch
            {
                ColumnType.Script => Name,
                ColumnType.Filename => Filename,
                ColumnType.Type => Type,
                ColumnType.Author => Author ?? string.Empty,
                ColumnType.NicoId => NicoId ?? string.Empty,
                ColumnType.Url => Url ?? string.Empty,
                ColumnType.Comment => Comment ?? string.Empty,
                ColumnType.Count => Count,
                _ => 0,
            };
        }

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
