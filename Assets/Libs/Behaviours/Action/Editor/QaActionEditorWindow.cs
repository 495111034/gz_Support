using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace ActionEditor
{
    public class QaActionEditorWindow : ActionEditorWindow
    {
        [MenuItem("Editor/行为树/打开QA自动化行为编辑器")]
        static void ShowEditor1()
        {
            QaActionEditorWindow editor = EditorWindow.GetWindow<QaActionEditorWindow>();
            editor.minSize = new Vector2(1200, 800);
        }

        [MenuItem("Editor/行为树/生成QA自动化行为树类型json")]
        private static void __GenActionJson1()
        {
            string sb = GenActionJson("GameLogic.ActionTreeQA");
            if (string.IsNullOrEmpty(sb))
            {
                return;
            }
            string dir = "../GameLibrary/QASuperMan";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllText(dir + "/ActionMenuJsonQA.txt", sb, System.Text.UTF8Encoding.UTF8);
        }

        protected override string ActionMenuJsonPath
        {
            get { return GetClientPath() + "/ActionMenuJsonQA.txt"; }
        }

        protected override string GetClientPath()
        {
            return "../GameLibrary/QASuperMan";
        }

        protected override void ExcuteAction()
        {
            var type = typeof(MyComponent).Assembly.GetType("GameBase.GameLoader");
            if (type != null)
            {
                var evt = type.GetMethod("CallLogic", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                if (evt != null)
                {
                    string json = JsonUtility.ToJson(windowNodeData.actionConfig);
                    string full_path = Path.GetFullPath(GetClientPath());
                    var hash = new Hashtable() { ["id"] = windowNodeData.id, ["json"] = MiniJSON.JsonDecode(json), ["path"] = full_path };
                    evt.Invoke(type, new object[] { -999997, hash });
                }
            }
        }

        protected override void ExtendOptionGUI()
        {
            if (GUILayout.Button("停止行为", GUILayout.Width(100)))
            {
                var type = typeof(MyComponent).Assembly.GetType("GameBase.GameLoader");
                if (type != null)
                {
                    var evt = type.GetMethod("CallLogic", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                    if (evt != null)
                    {
                        evt.Invoke(type, new object[] { -999996, new Hashtable() });
                    }
                }
            }
        }
    }
}
