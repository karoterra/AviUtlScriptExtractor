using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            var sjis = Encoding.GetEncoding(932);
            string inputPath = args[0];
            if (!File.Exists(inputPath))
            {
                Console.Error.WriteLine($"\"{inputPath}\" が見つかりません");
                Console.WriteLine("終了するにはEnterを押してください...");
                Console.ReadLine();
                return 1;
            }

            var anmScripts = new HashSet<string>();
            var objScripts = new HashSet<string>();
            var scnScripts = new HashSet<string>();
            var camScripts = new HashSet<string>();
            var traScripts = new HashSet<string>();

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
                    switch (effect)
                    {
                        case AnimationEffect anm:
                            if (anm.Name.Length > 0)
                            {
                                anmScripts.Add(anm.Name);
                            }
                            break;
                        case CustomObjectEffect coe:
                            if (coe.Name.Length > 0)
                            {
                                objScripts.Add(coe.Name);
                            }
                            break;
                        case CameraEffect cam:
                            if (cam.Name.Length > 0)
                            {
                                camScripts.Add(cam.Name);
                            }
                            break;
                        case SceneChangeEffect scn:
                            if (scn.Name.Length > 0)
                            {
                                scnScripts.Add(scn.Name);
                            }
                            break;
                    }
                    foreach (var trackbar in effect.Trackbars)
                    {
                        if (trackbar.Flag.HasFlag(TrackbarFlag.Script))
                        {
                            traScripts.Add(exedit.TrackbarScripts[trackbar.ScriptIndex].Name);
                        }
                    }
                }
            }

            var outputDir = Path.GetDirectoryName(inputPath);
            var outputFilename = $"{Path.GetFileNameWithoutExtension(inputPath)}_scripts.txt";
            var outputPath = Path.Combine(outputDir, outputFilename);
            using (var sw = new StreamWriter(outputPath, false, sjis))
            {
                sw.WriteLine("# アニメーション効果");
                foreach (var x in anmScripts)
                {
                    sw.WriteLine(x);
                }
                sw.WriteLine("\n# カスタムオブジェクト");
                foreach (var x in objScripts)
                {
                    sw.WriteLine(x);
                }
                sw.WriteLine("\n# シーンチェンジ");
                foreach (var x in scnScripts)
                {
                    sw.WriteLine(x);
                }
                sw.WriteLine("\n# カメラ効果");
                foreach (var x in camScripts)
                {
                    sw.WriteLine(x);
                }
                sw.WriteLine("\n# トラックバースクリプト");
                foreach (var x in traScripts)
                {
                    sw.WriteLine(x);
                }
            }

            return 0;
        }
    }
}
