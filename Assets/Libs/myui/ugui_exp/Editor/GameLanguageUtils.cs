using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;


namespace UnityEditor.UI
{

    public static class Language
    {
        static Dictionary<string, string> _dict;


        public static string GetString(string id,out bool found, params object[] args)
        {
            found = false;
            if (_dict == null) RefreshLanguage();
            if(_dict == null)
            {
                Log.LogError("无法找到游戏语言包，请先设置输出路径");
                return "";
            }
            string str = null;
            if (_dict.TryGetValue(id, out str))
            {
                found = true;

                return args != null ? string.Format(str, args) : str;
            }
            Log.LogError("GetString, lost id:{0}, {1}", id, new System.Diagnostics.StackTrace(true));
            return "{" + id + "}";
        }

        public static void RefreshLanguage()
        {
            _dict = null;
            if (string.IsNullOrEmpty(PathDefs.EXPORT_PATH_DATA))
            {
                if(!EditorPathUtils.CheckPathSettings())
                {
                    Log.LogError("调协输出路径失败");
                    return;
                }
            }

            string path = PathDefs.EXPORT_PATH_DATA + "lang/";
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            string langFile = path + "lang.txt";

            if (System.IO.File.Exists(langFile))
            {
                var text = File.ReadAllText(langFile);
                _dict = TextParser.ParseIni(text);
            }
            else
            {
                Log.LogError($"{langFile}文件不存在:root:{PathDefs.EXPORT_ROOT}");
            }
            
        }

    }

   
}
