using System.Text;
using System.Text.Json;
using Karoterra.AupDotNet;
using Karoterra.AupDotNet.ExEdit;
using Karoterra.AupDotNet.ExEdit.Effects;
using CommandLine;

namespace AviUtlScriptExtractor
{
    class Program
    {
        public static readonly string[] columns = { "script", "filename", "type", "author", "nicoid", "url", "comment", "count" };

        static int Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            var res = Parser.Default.ParseArguments<Options>(args)
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

            if (string.IsNullOrEmpty(opt.OutputPath))
            {
                opt.OutputPath = Path.Combine(
                    Path.GetDirectoryName(opt.Filename) ?? string.Empty,
                    $"{Path.GetFileNameWithoutExtension(opt.Filename)}_scripts.csv");
            }
            OutputCsv(opt.OutputPath, usedScripts, setting);

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
            };
            return JsonSerializer.Deserialize<Setting>(json, options) ?? new Setting();
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

        static bool OutputCsv(string path, List<ScriptData> scripts, Setting setting)
        {
            try
            {
                using var sw = new StreamWriter(path);
                sw.WriteLine(string.Join(',', setting.Columns));
                foreach (var script in scripts)
                {
                    if (script == null) continue;
                    var elements = new List<string>(setting.Columns.Count);
                    foreach (var column in setting.Columns)
                    {
                        string elem = column switch
                        {
                            "script" => script.Name,
                            "filename" => script.Filename,
                            "type" => script.Type.ToString().ToLower(),
                            "author" => script.Author ?? string.Empty,
                            "nicoid" => script.NicoId ?? string.Empty,
                            "url" => script.Url ?? string.Empty,
                            "comment" => script.Comment ?? string.Empty,
                            "count" => script.Count.ToString(),
                            _ => string.Empty,
                        };
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
            catch (Exception e) when(
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
