using CommandLine;

namespace AviUtlScriptExtractor
{
    internal class Options
    {
        [Value(0, Required = true, MetaName = "filename", HelpText = "aupファイルのパス")]
        public string Filename { get; set; } = string.Empty;

        [Option('o', "out", HelpText = "出力するcsvファイルのパス")]
        public string? OutputPath { get; set; }

        [Option("col", Separator = ',', HelpText = "出力する列名 (script|filename|type|author|nicoid|url|comment|count)")]
        public IEnumerable<ColumnType>? Columns { get; set; }

        [Option("header", HelpText = "ヘッダーの出力 (on|off|multi)")]
        public HeaderType? Header { get; set; }

        public bool Validate()
        {
            if (OutputPath == Filename)
            {
                Console.Error.WriteLine("入力ファイルと出力ファイルのパスが同じです。");
                return false;
            }
            return true;
        }
    }
}
