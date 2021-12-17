using Karoterra.AupDotNet;
using Karoterra.AupDotNet.ExEdit;
using Karoterra.AupDotNet.ExEdit.Effects;

namespace AviUtlScriptExtractor
{
    internal static class Utils
    {
        /// <summary>
        /// スクリプトファイル名からスクリプトの種類を取得
        /// </summary>
        /// <param name="name">スクリプトファイル名</param>
        /// <returns></returns>
        public static ScriptType FilenameToType(string name)
            => Path.GetExtension(name) switch
            {
                ".anm" => ScriptType.Anm,
                ".obj" => ScriptType.Obj,
                ".scn" => ScriptType.Scn,
                ".cam" => ScriptType.Cam,
                ".tra" => ScriptType.Tra,
                _ => ScriptType.Other,
            };

        /// <summary>
        /// スクリプト名とスクリプトの種類からスクリプトファイル名を取得
        /// </summary>
        /// <param name="name">スクリプト名</param>
        /// <param name="type">スクリプトの種類</param>
        /// <returns></returns>
        public static string GetScriptFilename(string name, ScriptType type)
        {
            switch (type)
            {
                case ScriptType.Anm when AnimationEffect.Defaults.Contains(name):
                    return "exedit.anm";
                case ScriptType.Obj when CustomObjectEffect.Defaults.Contains(name):
                    return "exedit.obj";
                case ScriptType.Scn when SceneChangeEffect.DefaultScripts.Contains(name):
                    return "exedit.scn";
                case ScriptType.Cam when CameraEffect.Defaults.Contains(name):
                    return "exedit.cam";
                case ScriptType.Tra when TrackbarScript.Defaults.Any(t => t.Name == name):
                    return "exedit.tra";
                case ScriptType.Other:
                    return name;
            }
            string ext = type.ToString().ToLower();
            int index = name.IndexOf('@');
            if (index >= 0)
            {
                name = name[index..];
            }
            return $"{name}.{ext}";
        }

        /// <summary>
        /// aupファイルを読み込む
        /// </summary>
        /// <param name="path">aupファイルのパス</param>
        /// <returns></returns>
        public static AviUtlProject? ReadAup(string path)
        {
            AviUtlProject? aup = null;
            try
            {
                aup = new AviUtlProject(path);
            }
            catch (FileNotFoundException)
            {
                Console.Error.WriteLine($"\"{path}\" が見つかりません。");
            }
            catch (UnauthorizedAccessException)
            {
                Console.Error.WriteLine($"\"{path}\" はディレクトリであるか、アクセス許可がありません。");
            }
            catch (PathTooLongException)
            {
                Console.Error.WriteLine("パスが長すぎます。");
            }
            catch (Exception e) when (
                e is ArgumentException or
                ArgumentNullException or
                DirectoryNotFoundException or
                NotSupportedException)
            {
                Console.Error.WriteLine("有効なパスを指定してください。");
            }
            catch (FileFormatException e)
            {
                Console.Error.WriteLine($"\"{path}\" は AviUtl プロジェクトファイルでないか破損している可能性があります。");
                Console.Error.WriteLine(e.Message);
            }
            catch (EndOfStreamException)
            {
                Console.Error.WriteLine($"\"{path}\" は AviUtl プロジェクトファイルでないか破損している可能性があります。");
                Console.Error.WriteLine("ファイルの読み込み中に終端に達しました。");
            }
            catch (IOException)
            {
                Console.Error.WriteLine("IOエラーが発生しました。");
            }

            return aup;
        }
    }
}
