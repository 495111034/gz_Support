using UnityEditor;
using Object = UnityEngine.Object;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

/// <summary>
/// 特效测试程序 打包用工具 by yqb
/// </summary>
public class CreateAssetBundles : Editor
{
    //编辑器扩展,添加菜单选项,要求必须是静态方法
    [MenuItem("Assets/特效测试程序/打包AssetBundles(请选中特效Prefab)")]
    static void BuildAllAssetBundles()
    {
        var selectNames = GetFiltered();
        int selectCount = selectNames.Count;
        List<AssetBundleBuild> builds = new List<AssetBundleBuild>();
        for (int i = 0; i < selectCount; i++)
        {
            string pathname = selectNames[i];
            AssetBundleBuild build = new AssetBundleBuild
            {
                addressableNames = new string[] { "main" },
                assetNames = new string[] { pathname }
            };
            var name = Path.GetFileNameWithoutExtension(pathname);
            var save_name = name + ".ab";
            build.assetBundleName = save_name;
            builds.Add(build);
        }
        //监测目录，没有则创建
        string dir = "Assets/StreamingAssets";
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        //打包
        BuildPipeline.BuildAssetBundles(dir, builds.ToArray(),
        BuildAssetBundleOptions.None, BuildTarget.Android);
        //刷新
        AssetDatabase.Refresh();
    }

    public static List<string> GetFiltered()
    {
        AssetDatabase.Refresh();
        List<string> list = new List<string>();
        foreach (var obj in Selection.objects)
        {
            if (obj == null) continue;
            var fname = AssetDatabase.GetAssetPath(obj).ToLower();
            list.Add(fname);
        }

        return list;
    }
    
    public static string GetSelectNames()
    {
        AssetDatabase.Refresh();
        List<string> list = new List<string>();
        StringBuilder stringBuilder = new StringBuilder();
        foreach (var obj in Selection.objects)
        {
            if (obj == null) continue;
            var fname = AssetDatabase.GetAssetPath(obj).ToLower();
            if (fname.EndsWithEx(".ab"))
            {
                stringBuilder.Append(obj.name + ".ab,");
            }
        }
        return stringBuilder.ToString().TrimEnd(',');
    }

    static string streamingPath = Application.streamingAssetsPath + "/TxtFile/onebyone.txt";
    static string persistentPath = Application.persistentDataPath + "/onebyone.txt";

    static string streamingPathOnce = Application.streamingAssetsPath + "/TxtFile/once.txt";
    static string persistentPathOnce = Application.persistentDataPath + "/once.txt";

    static string TxtFilePath = Application.streamingAssetsPath + "/TxtFile";

    [MenuItem("Assets/特效测试程序/修改onebyone.txt（完全覆盖）请选中StreamingAssets文件夹下的")]
    static void WritrOneByOneAll()
    {
        WriteAll();
    }

    [MenuItem("Assets/特效测试程序/修改onebyone.txt（末尾添加）")]
    static void WritrOneByOneEnd()
    {
        WriteEnd();
    }

    [MenuItem("Assets/特效测试程序/修改once.txt（完全覆盖）")]
    static void WritrOnceAll()
    {
        WriteAll(isOnce: true);
    }

    [MenuItem("Assets/特效测试程序/修改once.txt（末尾添加）")]
    static void WritrOnceEnd()
    {
        WriteEnd(isOnce: true);
    }

    static void WriteAll(bool isOnce = false)
    {
        if (!Directory.Exists(TxtFilePath))
        {
            Directory.CreateDirectory(TxtFilePath);
        }
        string allNames = GetSelectNames();
        if (string.IsNullOrEmpty(allNames)) return;
        StreamWriter sw = new StreamWriter(isOnce ? streamingPathOnce : streamingPath);
        sw.Write(allNames);
        sw.Close();

        sw = new StreamWriter(isOnce ? persistentPathOnce : persistentPath);
        sw.Write(allNames);
        sw.Close();
        sw.Dispose();
    }

    static void WriteEnd(bool isOnce = false)
    {
        if (!Directory.Exists(TxtFilePath))
        {
            Directory.CreateDirectory(TxtFilePath);
        }
        string allNames = GetSelectNames();
        if (string.IsNullOrEmpty(allNames)) return;
        StreamWriter sw;
        FileInfo t = new FileInfo(isOnce ? streamingPathOnce : streamingPath);
        if (!t.Exists)
        {
            sw = t.CreateText();
            sw.Write(allNames);
        }
        else
        {
            sw = t.AppendText();
            sw.Write(',');
            sw.Write(allNames);
        }
        sw.Close();

        t = new FileInfo(isOnce ? persistentPathOnce : persistentPath);
        if (!t.Exists)
        {
            sw = t.CreateText();
            sw.Write(allNames);
        }
        else
        {
            sw = t.AppendText();
            sw.Write(',');
            sw.Write(allNames);
        }
        sw.Close();

        sw.Dispose();
    }
}