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

                Debug.WriteLine("オブジェクト");
                foreach (var obj in exedit.Objects)
                {
                    Debug.WriteLine(" - " + (obj.Preview.Length > 0 ? obj.Preview : exedit.ObjectTypes[obj.ObjectType].Name));
                    if (exedit.ObjectTypes[obj.ObjectType].Name == "カスタムオブジェクト")
                    {
                        var name = obj.ExtData.Skip(4).ToArray().ToSjisString();
                        if (name.Length > 0)
                        {
                            Debug.WriteLine($"    カスタムオブジェクト: {name}");
                            objScripts.Add(name);
                        }
                    }
                    foreach (var trackbar in obj.Trackbars)
                    {
                        if (trackbar.Type == 0xF)
                        {
                            var name = exedit.Trackbars[trackbar.ScriptIndex];
                            Debug.WriteLine($"    トラックバー: {name}");
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
