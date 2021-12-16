using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using Karoterra.AupDotNet;
using Karoterra.AupDotNet.ExEdit;
using Karoterra.AupDotNet.ExEdit.Effects;

namespace AviUtlScriptExtractor
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.Error.WriteLine("ファイル名を指定してください");
                Console.WriteLine("終了するにはEnterを押してください...");
                Console.ReadLine();
                return 1;
            }
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var sjis = Encoding.GetEncoding(932);
            string inputPath = args[0];
            if (!File.Exists(inputPath))
            {
                Console.Error.WriteLine($"\"{inputPath}\" が見つかりません");
                Console.WriteLine("終了するにはEnterを押してください...");
                Console.ReadLine();
                return 1;
            }

            var settingPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "setting.json");
            if (!File.Exists(settingPath))
            {
                Console.Error.WriteLine("setting.json が見つかりません");
                Console.WriteLine("終了するにはEnterを押してください...");
                Console.ReadLine();
                return 1;
            }
            var json = File.ReadAllText(settingPath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            Setting setting = JsonSerializer.Deserialize<Setting>(json, options) ?? new Setting();
            var knownScripts = setting.GetScripts();
            var usedScripts = new Dictionary<UsedScriptKey, ScriptData>();

            AviUtlProject project;
            try
            {
                project = new(inputPath);
            }
            catch (FileFormatException ex)
            {
                Console.Error.WriteLine($"\"{inputPath}\" はAviUtlプロジェクトファイルではないか破損している可能性があります。");
                Console.Error.WriteLine($"詳細: {ex.Message}");
                Console.WriteLine("終了するにはEnterを押してください...");
                Console.ReadLine();
                return 1;
            }
            catch (EndOfStreamException)
            {
                Console.Error.WriteLine($"\"{inputPath}\" はAviUtlプロジェクトファイルではないか破損している可能性があります。");
                Console.Error.WriteLine($"詳細: ファイルの読み込み中に終端に達しました");
                Console.WriteLine("終了するにはEnterを押してください...");
                Console.ReadLine();
                return 1;
            }

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
                Console.WriteLine("終了するにはEnterを押してください...");
                Console.ReadLine();
                return 1;
            }

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

            var outputDir = Path.GetDirectoryName(inputPath) ?? "";
            var outputFilename = $"{Path.GetFileNameWithoutExtension(inputPath)}_scripts.csv";
            var outputPath = Path.Combine(outputDir, outputFilename);
            using (var sw = new StreamWriter(outputPath, false, sjis))
            {
                sw.WriteLine("script,filename,type,author,nicoid,url,comment");
                foreach (var x in usedScripts.Values)
                {
                    if (x == null) continue;
                    var elements = new string[]
                    {
                        x.Name,
                        x.Filename,
                        x.Type.ToString().ToLower(),
                        x.Author ?? string.Empty,
                        x.NicoId ?? string.Empty,
                        x.Url ?? string.Empty,
                        x.Comment ?? string.Empty,
                    };
                    sw.WriteLine(string.Join(',', elements));
                }
            }

            return 0;
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
                return;
            }
            else if (known.ContainsKey(filename))
            {
                used[key] = known[filename].Clone();
                used[key].Name = name;
                foreach (var d in known[filename].Dependencies)
                {
                    var dkey = new UsedScriptKey { Name = d.Name, Type = type };
                    if (!used.ContainsKey(dkey))
                    {
                        used[dkey] = d.Clone();
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
    }
}
