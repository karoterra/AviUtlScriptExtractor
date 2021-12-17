using System.Text.Json.Serialization;

namespace AviUtlScriptExtractor
{
    public enum ColumnType
    {
        Script = 0,
        Filename,
        Type,
        Author,
        NicoId,
        Url,
        Comment,
        Count,
    }

    public enum HeaderType
    {
        On,
        Off,
        Multi,
    }

    public class Setting
    {
        private static readonly ColumnType[] defaultColumns =
        {
            ColumnType.Script,
            ColumnType.Filename,
            ColumnType.Type,
            ColumnType.Author,
            ColumnType.NicoId,
            ColumnType.Url,
            ColumnType.Comment,
            ColumnType.Count,
        };

        [JsonPropertyName("authors")]
        public List<AuthorSetting> Authors { get; set; } = new List<AuthorSetting>();

        [JsonPropertyName("columns")]
        public List<ColumnType> Columns { get; set; } = defaultColumns.ToList();

        [JsonPropertyName("header")]
        public HeaderType Header { get; set; } = HeaderType.On;

        public bool Validate()
        {
            if (Columns.Count == 0)
            {
                Columns = defaultColumns.ToList();
            }

            return true;
        }

        internal void MergeOptions(Options opt)
        {
            if (opt.Columns != null)
            {
                var cols = opt.Columns.ToList();
                if (cols.Count > 0)
                {
                    Columns = cols;
                }
            }

            if (opt.Header != null)
            {
                Header = opt.Header.Value;
            }
        }

        /// <summary>
        /// スクリプトの抽出時に使う既知のスクリプトの辞書の作成
        /// </summary>
        /// <returns>キー:ファイル名,値:ScriptData</returns>
        internal Dictionary<string, ScriptData> GetScripts()
        {
            var scripts = new Dictionary<string, ScriptData>();
            var exedit = new ScriptData
            {
                Filename = "exedit.anm",
                Author = "ＫＥＮくん",
                NicoId = "im1696493",
                Url = "http://spring-fragrance.mints.ne.jp/aviutl/",
                Comment = "標準スクリプト",
            };
            scripts["exedit.anm"] = exedit.Clone();
            exedit.Filename = "exedit.obj";
            scripts["exedit.obj"] = exedit.Clone();
            exedit.Filename = "exedit.scn";
            scripts["exedit.scn"] = exedit.Clone();
            exedit.Filename = "exedit.cam";
            scripts["exedit.cam"] = exedit.Clone();
            exedit.Filename = "exedit.tra";
            scripts["exedit.tra"] = exedit;

            foreach (var author in Authors)
            {
                if (author == null) continue;

                foreach (var script in author.Scripts)
                {
                    if (script == null || string.IsNullOrEmpty(script.Name))
                        continue;

                    var data = new ScriptData
                    {
                        Name = script.Name,
                        Filename = script.Name,
                        Author = author.Name,
                        NicoId = script.NicoId,
                        Url = script.Url,
                        Comment = script.Comment,
                    };
                    scripts[script.Name] = data;
                }
            }

            foreach (var author in Authors)
            {
                if (author == null) continue;
                foreach (var script in author.Scripts)
                {
                    if (script == null || string.IsNullOrEmpty(script.Name))
                        continue;
                    foreach (var d in script.Dependencies)
                    {
                        if (string.IsNullOrEmpty(d)) continue;

                        if (scripts.ContainsKey(d))
                        {
                            scripts[script.Name].Dependencies.Add(scripts[d]);
                        }
                        else
                        {
                            var data = new ScriptData
                            {
                                Name = d,
                                Filename = d,
                            };
                            scripts[d] = data;
                            scripts[script.Name].Dependencies.Add(data);
                        }
                    }
                }
            }

            return scripts;
        }
    }
}
