using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;

namespace AviUtlScriptExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Count() != 1)
            {
                Console.Error.WriteLine("ファイル名を指定してください");
                Environment.Exit(1);
            }
            var sjis = Encoding.GetEncoding(932);
            string inputPath = args[0];
            if (!File.Exists(inputPath))
            {
                Console.Error.WriteLine($"\"{inputPath}\"が見つかりません");
                Environment.Exit(1);
            }

            var anmScripts = new HashSet<string>();
            var objScripts = new HashSet<string>();
            var scnScripts = new HashSet<string>();
            var camScripts = new HashSet<string>();
            var traScripts = new HashSet<string>();

            AviUtlProject project;
            using (BinaryReader br = new BinaryReader(File.OpenRead(inputPath), sjis))
            {
                project = new AviUtlProject(br);
            }
            if (project.Filters.ContainsKey("拡張編集"))
            {
                var exedit = new ExEdit(project.Filters["拡張編集"]);
                var animEffectType = exedit.ObjectTypes.ToList().FindIndex(x => x.Name == "アニメーション効果");
                var customObjectType = exedit.ObjectTypes.ToList().FindIndex(x => x.Name == "カスタムオブジェクト");
                var camEffectType = exedit.ObjectTypes.ToList().FindIndex(x => x.Name == "カメラ効果");
                var sceneChangeType = exedit.ObjectTypes.ToList().FindIndex(x => x.Name == "シーンチェンジ");

                Debug.WriteLine("オブジェクト");
                foreach (var obj in exedit.Objects)
                {
                    Debug.WriteLine("- " + (obj.Preview.Length > 0
                        ? obj.Preview.Replace("\r\n", " ")
                        : exedit.ObjectTypes[obj.ObjectTypes[0]].Name));
                    // アニメーション効果、カメラ効果、シーンチェンジ
                    for (int i = 0; i < obj.ObjectTypes.Length; i++)
                    {
                        if (obj.ObjectTypes[i] == animEffectType && obj.ExtData[i].Length > 0)
                        {
                            var name = obj.ExtData[i].Skip(4).Take(256).ToArray().ToSjisString();
                            if (name.Length > 0)
                            {
                                Debug.WriteLine($"  アニメーション効果: {name}");
                                anmScripts.Add(name);
                            }
                        }
                        if (obj.ObjectTypes[i] == camEffectType && obj.ExtData[i].Length > 0)
                        {
                            var name = obj.ExtData[i].Skip(4).Take(256).ToArray().ToSjisString();
                            if (name.Length > 0)
                            {
                                Debug.WriteLine($"  カメラ効果: {name}");
                                camScripts.Add(name);
                            }
                        }
                        if (obj.ObjectTypes[i] == sceneChangeType && obj.ExtData[i].Length > 0)
                        {
                            var name = obj.ExtData[i].Skip(4).Take(256).ToArray().ToSjisString();
                            if (name.Length > 0)
                            {
                                Debug.WriteLine($"  シーンチェンジ: {name}");
                                scnScripts.Add(name);
                            }
                        }
                    }
                    // カスタムオブジェクト
                    if (obj.ObjectTypes[0]== customObjectType && obj.ExtData[0].Length > 0)
                    {
                        var name = obj.ExtData[0].Skip(4).Take(256).ToArray().ToSjisString();
                        if (name.Length > 0)
                        {
                            Debug.WriteLine($"  カスタムオブジェクト: {name}");
                            objScripts.Add(name);
                        }
                    }
                    // トラックバー
                    foreach (var trackbar in obj.Trackbars)
                    {
                        if (trackbar.Type == 0xF)
                        {
                            var name = exedit.Trackbars[trackbar.ScriptIndex];
                            Debug.WriteLine($"  トラックバー: {name}");
                            traScripts.Add(name);
                        }
                    }
                }
            }

            Console.WriteLine("# アニメーション効果");
            foreach (var x in anmScripts)
            {
                Console.WriteLine(x);
            }
            Console.WriteLine("\n# カスタムオブジェクト");
            foreach (var x in objScripts)
            {
                Console.WriteLine(x);
            }
            Console.WriteLine("\n# シーンチェンジ");
            foreach (var x in scnScripts)
            {
                Console.WriteLine(x);
            }
            Console.WriteLine("\n# カメラ効果");
            foreach (var x in camScripts)
            {
                Console.WriteLine(x);
            }
            Console.WriteLine("\n# トラックバースクリプト");
            foreach (var x in traScripts)
            {
                Console.WriteLine(x);
            }

            Console.ReadLine();
        }
    }
}
