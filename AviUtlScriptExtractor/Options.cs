using CommandLine;

namespace AviUtlScriptExtractor
{
    internal class Options
    {
        [Value(0, Required = true, MetaName = "filename", HelpText = "aupファイルのパス")]
        public string Filename { get; set; } = string.Empty;

        [Option('o', "out", HelpText = "出力するcsvファイルのパス")]
        public string? OutputPath { get; set; }
    }
}
