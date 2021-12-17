namespace AviUtlScriptExtractor
{
    public class OrderingItem
    {
        public ColumnType Column { get; set; }
        public bool Order { get; set; } = true;

        public OrderingItem()
        {
        }

        public OrderingItem(string column)
        {
            Column = column.ToLower() switch
            {
                "script" => ColumnType.Script,
                "filename" => ColumnType.Filename,
                "type" => ColumnType.Type,
                "author" => ColumnType.Author,
                "nicoid" => ColumnType.NicoId,
                "url" => ColumnType.Url,
                "comment" => ColumnType.Comment,
                "count" => ColumnType.Count,
                _ => throw new ArgumentException("invalid column"),
            };
            Order = !column.Any(c => 'A' <= c && c <= 'Z');
        }

        public override string ToString()
        {
            return Order ? Column.ToString().ToLower() : Column.ToString().ToUpper();
        }
    }
}
