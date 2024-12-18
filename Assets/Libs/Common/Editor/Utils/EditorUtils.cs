
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace UnityEngine.Rendering.Common
{
    public static class EditorUtils
    {
        public const string MySupportResRootPath = "Assets/plugins_camera/SupportRes/";
        public static void ModifySerializedObjectProperty(UnityEngine.Object obj, string propertyName, bool propertyValue)
        {
            SerializedObject serializedObject = new SerializedObject(obj);

            var property = serializedObject.FindProperty(propertyName);
            if (property != null)
            {
                property.boolValue = propertyValue;
            }
            serializedObject.ApplyModifiedProperties();
        }

        public static T[] GetAssetBySelectedDirectory<T>(string searchPattern, Predicate<string> pathMatch, Predicate<T> assetMatch)
            where T : Object
        {
            if (pathMatch == null)
            {
                pathMatch = obj => true;
            }
            if (assetMatch == null)
            {
                assetMatch = obj => true;
            }
            UnityEngine.Object[] arr = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets);
            List<T> result = new List<T>();
            string path;
            for (int i = 0; i < arr.Length; i++)
            {
                string tempPath = AssetDatabase.GetAssetPath(arr[i]);
                if (tempPath.Length == 0)
                {
                    continue;
                }
                if (Directory.Exists(tempPath))
                {
                    DirectoryInfo direction = new DirectoryInfo(tempPath);
                    FileInfo[] files = direction.GetFiles(searchPattern, SearchOption.AllDirectories);
                    foreach (var file in files)
                    {
                        path = file.FullName;
                        path = path.Replace('\\', '/');
                        path = "Assets" + path.Substring(Application.dataPath.Length);
                        if (pathMatch(path))
                        {
                            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
                            if (assetMatch(asset))
                            {
                                result.Add(asset);
                            }

                        }
                        
                    }
                }
                else
                {
                    var asset = AssetDatabase.LoadAssetAtPath<T>(tempPath);
                    if (asset != null && pathMatch(tempPath) && assetMatch(asset))
                    {
                        result.Add(asset);
                    }
                }
            }

            return result.ToArray();
        }
    }
}