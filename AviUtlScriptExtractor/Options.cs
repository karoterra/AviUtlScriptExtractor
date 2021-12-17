using CommandLine;
using CommandLine.Text;

namespace AviUtlScriptExtractor
{
    internal class Options
    {
        [Value(0, Required = true, MetaName = "filename", HelpText = "aupファイルのパス。")]
        public string Filename { get; set; } = string.Empty;

        [Option('o', "out", HelpText = "出力するcsvファイルのパス。")]
        public string OutputPath { get; set; } = string.Empty;

        [Option("header", HelpText = "ヘッダーの出力指定(on|off|multi)。on: ヘッダーを出力する。off: ヘッダーを出力しない。multi: 複数列出力する場合のみヘッダーを出力する。")]
        public HeaderType? Header { get; set; }

        [Option("col", Separator = ',', HelpText = "出力する列名をカンマ区切りで指定(script|filename|type|author|nicoid|url|comment|count)。")]
        public IEnumerable<ColumnType> Columns { get; set; } = Array.Empty<ColumnType>();

        [Option("sort", Separator = ',', HelpText = "ソートする列をカンマ区切りで指定。大文字にすると降順。")]
        public IEnumerable<OrderingItem> Sort { get; set; } = Array.Empty<OrderingItem>();

        public bool Validate()
        {
            if (OutputPath == Filename)
            {
                Console.Error.WriteLine("入力ファイルと出力ファイルのパスが同じです。");
                return false;
            }
            return true;
        }

        [Usage]
        public static IEnumerable<Example> Examples
        {
            get
            {
                yield return new Example("通常", new Options { Filename = @"C:\path\to\project.aup" });
                yield return new Example("出力先を指定", new Options { Filename = @"C:\path\to\project.aup", OutputPath = @"C:\path\to\scripts.csv" });
                yield return new Example("ヘッダーを出力しない", new Options { Filename = @"C:\path\to\project.aup", Header = HeaderType.Off });
                yield return new Example("スクリプト名と使用回数のみ出力", new Options
                {
                    Filename = @"C:\path\to\project.aup",
                    Columns = new[] { ColumnType.Script, ColumnType.Count }
                });
                yield return new Example("使用回数(降順)、スクリプト名(昇順)でソート", new Options
                {
                    Filename = @"C:\path\to\project.aup",
                    Sort = new OrderingItem[]
                    {
                        new(){ Column = ColumnType.Count, Order = false },
                        new(){ Column = ColumnType.Script, Order = true },
                    }
                });
            }
        }
    }
}
