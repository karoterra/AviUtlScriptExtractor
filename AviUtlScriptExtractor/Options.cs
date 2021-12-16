using CommandLine;

namespace AviUtlScriptExtractor
{
    internal class Options
    {
        [Value(0, Required = true, MetaName = "filename", HelpText = "aupファイルのパス")]
        public string Filename { get; set; } = string.Empty;

        [Option('o', "out", HelpText = "出力するcsvファイルのパス")]
        public string? OutputPath { get; set; }

        [Option("col", Separator = ',', HelpText = "出力する列名")]
        public IEnumerable<string>? Columns { get; set; }

        public bool Validate()
        {
            if (OutputPath == Filename)
            {
                Console.Error.WriteLine("入力ファイルと出力ファイルのパスが同じです。");
                return false;
            }
            if (Columns != null)
            {
                foreach (var column in Columns)
                {
                    if (!Program.columns.Contains(column))
                    {
                        Console.Error.WriteLine("不明な列名が指定されました。");
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
