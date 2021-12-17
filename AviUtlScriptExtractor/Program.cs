using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Karoterra.AupDotNet;
using Karoterra.AupDotNet.ExEdit;
using Karoterra.AupDotNet.ExEdit.Effects;
using CommandLine;

namespace AviUtlScriptExtractor
{
    class Program
    {
        static int Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var parser = new Parser(conf =>
            {
                conf.HelpWriter = Console.Out;
                conf.CaseInsensitiveEnumValues = true;
            });
            var res = parser.ParseArguments<Options>(args)
                .MapResult(opt => Run(opt), err => 1);
            return res;
        }

        static int Run(Options opt)
        {
            if (!opt.Validate())
            {
                return 1;
            }

            Setting setting = LoadSetting();
            if (!setting.Validate())
            {
                return 1;
            }
            setting.MergeOptions(opt);

            AviUtlProject? project = Utils.ReadAup(opt.Filename);
            if (project == null) return 1;

            ExEditProject? exedit = null;
            for (int i = 0; i < project.FilterProjects.Count; i++)
            {
                if (project.FilterProjects[i] is RawFilterProject filter && filter.Name == "拡張編集")
                {
                    exedit = new ExEditProject(filter);
                    break;
                }
            }

            if (exedit == null)
            {
                Console.Error.WriteLine("拡張編集のデータが見つかりません");
                return 1;
            }

            var usedScripts = ExtractScript(exedit, setting);
            var sorted = SortScript(usedScripts, setting);

            if (string.IsNullOrEmpty(opt.OutputPath))
            {
                opt.OutputPath = Path.Combine(
                    Path.GetDirectoryName(opt.Filename) ?? string.Empty,
                    $"{Path.GetFileNameWithoutExtension(opt.Filename)}_scripts.csv");
            }
            if (!OutputCsv(opt.OutputPath, sorted, setting))
            {
                return 1;
            }

            return 0;
        }

        static Setting LoadSetting()
        {
            var settingPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "setting.json");
            if (!File.Exists(settingPath))
            {
                return new Setting();
            }

            var json = File.ReadAllText(settingPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                Converters =
                {
                    new JsonStringEnumConverter(),
                }
            };
            try
            {
                return JsonSerializer.Deserialize<Setting>(json, options) ?? new Setting();
            }
            catch (JsonException e)
            {
                Console.Error.WriteLine("設定ファイルの読込時にエラーが発生しました。");
                Console.Error.WriteLine(e.Message);
                Console.Error.WriteLine("デフォルトの設定で動作します。");
                return new Setting();
            }
        }

        /// <summary>
        /// 拡張編集で使ったスクリプトを抽出する
        /// </summary>
        /// <param name="exedit">拡張編集のデータ</param>
        /// <param name="setting">アプリの設定</param>
        /// <returns></returns>
        static List<ScriptData> ExtractScript(ExEditProject exedit, Setting setting)
        {
            var knownScripts = setting.GetScripts();
            var usedScripts = new Dictionary<UsedScriptKey, ScriptData>();

            foreach (var obj in exedit.Objects)
            {
                if (obj.Chain) continue;

                foreach (var effect in obj.Effects)
                {
                    ScriptType type = ScriptType.Other;
                    string scriptName = "";
                    switch (effect)
                    {
                        case AnimationEffect anm:
                            type = ScriptType.Anm;
                            scriptName = string.IsNullOrEmpty(anm.Name)
                                ? AnimationEffect.Defaults[anm.ScriptId]
                                : anm.Name;
                            break;
                        case CustomObjectEffect coe:
                            type = ScriptType.Obj;
                            scriptName = string.IsNullOrEmpty(coe.Name)
                                ? CustomObjectEffect.Defaults[coe.ScriptId]
                                : coe.Name;
                            break;
                        case CameraEffect cam:
                            type = ScriptType.Cam;
                            scriptName = string.IsNullOrEmpty(cam.Name)
                                ? CameraEffect.Defaults[cam.ScriptId]
                                : cam.Name;
                            break;
                        case SceneChangeEffect scn when scn.Params != null:
                            type = ScriptType.Scn;
                            scriptName = scn.Name;
                            break;
                    }
                    if (!string.IsNullOrEmpty(scriptName))
                    {
                        AddUsedScript(knownScripts, usedScripts, scriptName, type);
                    }
                    foreach (var trackbar in effect.Trackbars)
                    {
                        if (trackbar.Type == TrackbarType.Script)
                        {
                            scriptName = exedit.TrackbarScripts[trackbar.ScriptIndex].Name;
                            AddUsedScript(knownScripts, usedScripts, scriptName, ScriptType.Tra);
                        }
                    }
                }
            }

            return usedScripts.Values.ToList();
        }

        /// <summary>
        /// usedに使ったスクリプトを登録する
        /// </summary>
        /// <param name="known">settingにある既知のスクリプト。キー:ファイル名</param>
        /// <param name="used">使ったスクリプト。キー:UsedScriptKey</param>
        /// <param name="name">スクリプト名</param>
        /// <param name="type">スクリプトの種類</param>
        static void AddUsedScript(
            Dictionary<string, ScriptData> known,
            Dictionary<UsedScriptKey, ScriptData> used,
            string name,
            ScriptType type)
        {
            var filename = Utils.GetScriptFilename(name, type);
            var key = new UsedScriptKey { Name = name, Type = type };
            if (used.ContainsKey(key))
            {
                used[key].Increment();
                foreach (var d in used[key].Dependencies)
                    d.Increment();
                return;
            }
            else if (known.ContainsKey(filename))
            {
                used[key] = known[filename].Clone();
                used[key].Name = name;
                foreach (var d in known[filename].Dependencies)
                {
                    var dkey = new UsedScriptKey { Name = d.Name, Type = type };
                    if (used.ContainsKey(dkey))
                    {
                        used[key].Dependencies.Add(used[dkey]);
                        used[dkey].Increment();
                    }
                    else
                    {
                        used[dkey] = d.Clone();
                        used[key].Dependencies.Add(used[dkey]);
                    }
                }
            }
            else
            {
                used[key] = new ScriptData
                {
                    Name = name,
                    Filename = filename,
                };
            }
        }

        static IEnumerable<ScriptData> SortScript(IEnumerable<ScriptData> scripts, Setting setting)
        {
            if (setting.Sort.Count == 0)
                return scripts;

            var sorted = setting.Sort[0].Order
                ? scripts.OrderBy(s => s.GetValue(setting.Sort[0].Column))
                : scripts.OrderByDescending(s => s.GetValue(setting.Sort[0].Column));
            foreach (var item in setting.Sort.Skip(1))
            {
                sorted = item.Order
                    ? sorted.ThenBy(s => s.GetValue(item.Column))
                    : sorted.ThenByDescending(s => s.GetValue(item.Column));
            }
            return sorted;
        }

        static bool OutputCsv(string path, IEnumerable<ScriptData> scripts, Setting setting)
        {
            try
            {
                using var sw = new StreamWriter(path);

                if (setting.Header == HeaderType.On
                    || (setting.Header == HeaderType.Multi && setting.Columns.Count > 1))
                {
                    sw.WriteLine(string.Join(',', setting.Columns).ToLower());
                }

                foreach (var script in scripts)
                {
                    if (script == null) continue;
                    var elements = new List<string>(setting.Columns.Count);
                    foreach (var column in setting.Columns)
                    {
                        string elem = column switch
                        {
                            ColumnType.Script => script.Name,
                            ColumnType.Filename => script.Filename,
                            ColumnType.Type => script.Type.ToString().ToLower(),
                            ColumnType.Author => script.Author ?? string.Empty,
                            ColumnType.NicoId => script.NicoId ?? string.Empty,
                            ColumnType.Url => script.Url ?? string.Empty,
                            ColumnType.Comment => script.Comment ?? string.Empty,
                            ColumnType.Count => script.Count.ToString(),
                            _ => string.Empty,
                        };
                        elem = elem.Replace("\"", "\"\"");
                        if (elem.Contains(',') || elem.Contains('"'))
                            elem = $"\"{elem}\"";
                        elements.Add(elem);
                    }
                    sw.WriteLine(string.Join(',', elements));
                }
            }
            catch (UnauthorizedAccessException)
            {
                Console.Error.WriteLine($"{path} へのアクセス許可がありません。");
                return false;
            }
            catch (PathTooLongException)
            {
                Console.Error.WriteLine("出力パスが長すぎます。");
                return false;
            }
            catch (Exception e) when (
                e is ArgumentException or
                ArgumentNullException or
                DirectoryNotFoundException or
                NotSupportedException)
            {
                Console.Error.WriteLine("有効な出力パスを指定してください。");
                return false;
            }
            catch (IOException)
            {
                Console.Error.WriteLine("ファイル作成中にIOエラーが発生しました。");
                return false;
            }
            return true;
        }
    }
}
