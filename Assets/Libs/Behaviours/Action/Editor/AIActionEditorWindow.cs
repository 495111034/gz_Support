
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ActionEditor
{
    public class AIActionEditorWindow : ActionEditorWindow
    {
        [MenuItem("Editor/行为树/打开行为树编辑器")]
        static void ShowEditor()
        {
            AIActionEditorWindow editor = EditorWindow.GetWindow<AIActionEditorWindow>();
            editor.minSize = new Vector2(1200, 800);
        }

        [MenuItem("Editor/行为树/生成所有行为树类型json")]
        private static void __GenActionJson()
        {
            string sb = GenActionJson("GameLogic.ActionTree");
            if (string.IsNullOrEmpty(sb))
            {
                return;
            }
            string dir = Application.dataPath + "/Libs/Behaviours/Action/Editor";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllText(dir + "/ActionMenuJson.txt", sb, System.Text.UTF8Encoding.UTF8);
        }

        protected override string ActionMenuJsonPath
        {
            get { return Application.dataPath + "/Libs/Behaviours/Action/Editor/ActionMenuJson.txt"; }
        }

        protected override string GetClientPath()
        {
            string uiSavePath = "../assetbundles";
            if (Application.dataPath.Contains("arts_projects") || !Directory.Exists(uiSavePath))
            {
                uiSavePath = "../../client/assetbundles";
                if (!Directory.Exists(uiSavePath))
                {
                    string config_path = "../config.txt";
                    if (!File.Exists(config_path))
                    {
                        EditorUtility.DisplayDialog("提示", "路径不正确，请检查你的项目和config.txt", "ok");
                        return "";
                    }
                    StringReader reader = new StringReader(File.ReadAllText(config_path, System.Text.UTF8Encoding.UTF8));
                    string line = reader.ReadLine();
                    line = line.Replace("file:///", "");
                    line = line.Replace("res_url=", "");
                    if (line.EndsWith("/"))
                    {
                        line = line + "datas/data/action";
                    }
                    else
                    {
                        line = line + "/datas/data/action";
                    }
                    uiSavePath = line;
                    reader.Close();
                    reader.Dispose();
                }
                else
                {
                    uiSavePath += "/datas/data/action";
                }
            }
            else
            {
                uiSavePath += "/datas/data/action";
            }
            return uiSavePath;
        }
    }
}