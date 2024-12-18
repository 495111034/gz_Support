
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GameSupportEditor
{
    public class PathUtils
    {
        static public string AssetPath_To_FullPath(string assetPath)
        {
            const string startStr = "Assets";
            string applicationPath = Application.dataPath;
            int applicationPathLength = applicationPath.Length - 7;
            if (!assetPath.StartsWith(startStr))
            {
                return "";
            }
            string dir = applicationPath.Remove(applicationPathLength);
            return dir + "/" + assetPath;
        }

        static public string FullPath_To_AssetPath(string fullPath)
        {
            string applicationPath = Application.dataPath;
            int applicationPathLength = applicationPath.Length - 6;
            if (fullPath.Length <= applicationPathLength)
            {
                return "";
            }
            if (!fullPath.StartsWith(applicationPath))
            {
                return "";
            }
            return fullPath.Remove(0, applicationPathLength);
        }

        /// <summary>
        /// 获取选中的路径
        /// </summary>
        /// <param name="isNeedDefaultPath">true选中的路径只能为Assets/gameres的子路径</param>
        /// <returns></returns>
        static public List<string> GetSelectionPath(bool isNeedDefaultPath = true)
        {
            List<string> retPaths = new List<string>();
            string defualtPath = "Assets/gameres";
            foreach (var str in Selection.assetGUIDs)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(str);
                if (isNeedDefaultPath)
                {
                    if (!assetPath.Contains(defualtPath))
                    {
                        continue;
                    }
                }
                string fullPath = AssetPath_To_FullPath(assetPath);
                if (string.IsNullOrEmpty(fullPath))
                {
                    continue;
                }
                //去除文件名,只保留路径
                if (!Directory.Exists(fullPath))
                {
                    int lastSlash = assetPath.LastIndexOf("/");
                    assetPath = assetPath.Remove(lastSlash);
                }

                if (!retPaths.Contains(assetPath))
                {
                    retPaths.Add(assetPath);
                }
            }
            if (retPaths.Count == 0 && isNeedDefaultPath)
            {
                retPaths.Add(defualtPath);
            }
            return retPaths;
        }
    }
}
