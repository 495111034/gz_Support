

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class FileHelp : Editor
{
    [MenuItem("Assets/复制")]
    public static void FileCopy()
    {
        string path = "";
        foreach(int i in Selection.instanceIDs)
        {
            path += AssetDatabase.GetAssetPath(i)+"\n";
        }
        TextEditor text2Editor = new TextEditor();
        text2Editor.text = path;
        text2Editor.OnFocus();
        text2Editor.Copy();

    }

    [MenuItem("Assets/粘贴")]
    public static void FilePas()
    {
        string[] paths = GUIUtility.systemCopyBuffer.Split('\n');
        foreach (string path in paths)
        {
            if (path.StartsWith("Assets"))
            {
                string newPath = AssetDatabase.GetAssetPath(Selection.activeInstanceID);
                if (newPath.Contains("."))
                {
                    string[] file_name = newPath.Split('/');
                    newPath = newPath.Replace(file_name[file_name.Length - 1], "");
                }
                else
                {
                    newPath += "/";
                }
                string[] file_name2 = path.Split('/');

                AssetDatabase.CopyAsset(path, newPath + file_name2[file_name2.Length - 1]);

            }
        }
    }
}
