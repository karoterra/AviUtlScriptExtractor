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
            if (args.Count() != 1)
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
            Setting setting = JsonSerializer.Deserialize<Setting>(json);
            setting.CompleteRead();
            var knownScripts = setting.GetScripts();
            var usedScripts = new Dictionary<string, Script>();

            AviUtlProject project;
            try
            {
                using (BinaryReader br = new BinaryReader(File.OpenRead(inputPath), sjis))
                {
                    project = new AviUtlProject(br);
                }
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

            ExEditProject exedit = null;
            for (int i = 0; i < project.FilterProjects.Count; i++)
            {
                RawFilterProject filter = project.FilterProjects[i] as RawFilterProject;
                if (filter != null && filter.Name == "拡張編集")
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
                foreach (var effect in obj.Effects)
                {
                    ScriptType type = ScriptType.Other;
                    string scriptName = "";
                    switch (effect)
                    {
                        case AnimationEffect anm:
                            type = ScriptType.Anm;
                            scriptName = anm.Name;
                            break;
                        case CustomObjectEffect coe:
                            type = ScriptType.Obj;
                            scriptName = coe.Name;
                            break;
                        case CameraEffect cam:
                            type = ScriptType.Cam;
                            scriptName = cam.Name;
                            break;
                        case SceneChangeEffect scn:
                            type = ScriptType.Scn;
                            scriptName = scn.Name;
                            break;
                    }
                    if (scriptName.Length > 0)
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

            var outputDir = Path.GetDirectoryName(inputPath);
            var outputFilename = $"{Path.GetFileNameWithoutExtension(inputPath)}_scripts.csv";
            var outputPath = Path.Combine(outputDir, outputFilename);
            using (var sw = new StreamWriter(outputPath, false, sjis))
            {
                sw.WriteLine("script,filename,type,author,nicoid,url,comment");
                foreach (var x in usedScripts)
                {
                    if (x.Value != null)
                    {
                        sw.WriteLine($"{x.Key},{x.Value.Name},{x.Value.Type.ToString().ToLower()},{x.Value.Author.Name},{x.Value.NicoId},{x.Value.Url},{x.Value.Comment}");
                    }
                    else
                    {
                        sw.WriteLine($"{x.Key},,,,,,");
                    }
                }
            }

            return 0;
        }

        static string GetScriptFilename(string name, ScriptType type)
        {
            string ext = type.ToString().ToLower();
            int index = name.IndexOf('@');
            if (index >= 0)
            {
                name = name.Substring(index);
            }
            return $"{name}.{ext}";
        }

        static void AddUsedScript(Dictionary<string, Script> known, Dictionary<string, Script> used, string name, ScriptType type)
        {
            var filename = GetScriptFilename(name, type);
            if (known.ContainsKey(filename))
            {
                used[name] = known[filename];
                if (known[filename].Dependencies != null)
                {
                    foreach (var d in known[filename].Dependencies)
                    {
                        if (known.ContainsKey(d))
                        {
                            used[d] = known[d];
                        }
                        else
                        {
                            used[d] = null;
                        }
                    }
                }
            }
            else
            {
                used[name] = null;
            }
        }
    }
}
