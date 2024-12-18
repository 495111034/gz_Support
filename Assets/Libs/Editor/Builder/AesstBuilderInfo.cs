
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class AssetBuilderInfo : IDisposable
{
    public Dictionary<string, List<AssetBundleBuild>> builderDic;
    public HashSet<string> tempFileList;
    public List<string> tempLegacyAnimclips;

    Dictionary<string, string> outputs = new Dictionary<string, string>();
    Dictionary<string, AssetBundleBuild> inputs = new Dictionary<string, AssetBundleBuild>();

    public AssetBuilderInfo()
    {
        builderDic = new Dictionary<string, List<AssetBundleBuild>>();
        tempFileList = new HashSet<string>();
        tempLegacyAnimclips = new List<string>();
    }

    public bool HasAdded(string output, string pathname)
    {
        if (outputs.TryGetValue(output.ToLower(), out var pathname2))
        {
            if (pathname != pathname2)
            {
                Log.LogError($"资源名字冲突！{output}:{pathname}<=>{pathname2}");
            }
            return true;
        }
        return false;
    }

    public bool AddNewAssetbundle(string path, AssetBundleBuild assetbund)
    {
        var output = assetbund.assetBundleName = assetbund.assetBundleName.ToLower();
        if (HasAdded(output, assetbund.assetNames[0]))
        {
            return false;
        }
        outputs[output] = assetbund.assetNames[0];

        foreach (var input in assetbund.assetNames)
        {
            if (inputs.TryGetValue(input,out var old))
            {
                throw new Exception($"output=[{output}], 与[{old.assetBundleName}]导出重复使用了[{input}]");
            }
            inputs[input] = assetbund;
        }

        if (assetbund.assetNames.Length != assetbund.addressableNames.Length)
        {
            throw new Exception($"output=[{output}], assetNames={string.Join(",", assetbund.assetNames)} 与 addressableNames={string.Join(",", assetbund.addressableNames)} 不一致");
        }
        else
        {
            for (int i=0; i< assetbund.assetNames.Length; ++i) 
            {
                if (string.IsNullOrEmpty(assetbund.assetNames[i])) 
                {
                    throw new Exception($"output=[{output}], assetNames[{i}]为空！{string.Join(",", assetbund.assetNames)}");
                }
                if (string.IsNullOrEmpty(assetbund.addressableNames[i]))
                {
                    throw new Exception($"output=[{output}], addressableNames[{i}]为空！{string.Join(",", assetbund.addressableNames)}");
                }
            }
        }

        path = Path.GetFullPath(path).TrimEnd('/', '\\');
        if (!builderDic.TryGetValue(path, out var list))
        {
            list = builderDic[path] = new List<AssetBundleBuild>();
        }
        list.Add(assetbund);
        return true;
    }


    public static ICollection Sort(ICollection c)
    {
        if (c is IDictionary)
        {
            var d = c as IDictionary;
            //
            var kvs = new List<DictionaryEntry>();
            foreach (DictionaryEntry kv in d)
            {
                kvs.Add(kv);
            }
            kvs.Sort((a, b) => { return a.Key.ToString().CompareTo(b.Key.ToString()); });

            var copy = Activator.CreateInstance(c.GetType()) as IDictionary;
            foreach (var kv in kvs)
            {
                if (kv.Value is ICollection)
                {
                    copy[kv.Key] = Sort(kv.Value as ICollection);
                }
                else
                {
                    copy[kv.Key] = kv.Value;
                }
            }
            return copy;
        }
        else if (c is Array)
        {
            var L = c as Array;
            for (var i = 0; i < L.Length; ++i)
            {
                var Value = L.GetValue(i);
                if (Value is ICollection)
                {
                    L.SetValue(Sort(Value as ICollection), i);
                }
            }
        }
        else
        {
            var L = c as IList;
            for (var i = 0; i < L.Count; ++i)
            {
                var Value = L[i];
                if (Value is ICollection)
                {
                    L[i] = Sort(Value as ICollection);
                }
            }
        }
        return c;
    }

    public void AddTempFile(string filePath, string txt, byte[] bytes)
    {
        //Log.LogError($"add temp file:{filePath}");
        if (!tempFileList.Add(filePath))
        {
            throw new Exception($"重复使用临时文件{filePath}");
        }

        WiriteIfDif(filePath,txt, bytes);
    }
    public static bool WiriteIfDif(string filePath, string txt, byte[] bytes) 
    { 
    
        string file_txt = null;
        byte[] file_bytes = null;

        if (File.Exists(filePath) && (txt != null || bytes != null)) 
        {
            if (txt != null)
            {
                file_txt = File.ReadAllText(filePath);
            }
            else 
            {
                file_bytes = File.ReadAllBytes(filePath);
            }
        }

        if (txt != null && txt != file_txt) 
        {
            File.WriteAllText(filePath, txt);
            return true;
        }
        if (bytes != null && !array_eq(bytes, file_bytes))
        {            
            File.WriteAllBytes(filePath, bytes);
            return true;
        }
        return false;
    }

    static bool array_eq(byte[] a, byte[] b)
    {
        if (a == b) 
        {
            return true;
        }

        if (a == null || b == null) 
        {
            return false;
        }

        if (a.Length != b.Length) 
        {
            return false;
        }

        for (var i=0;i<a.Length;++i) 
        {
            if (a[i] != b[i]) 
            {
                return false;
            }
        }
        return true;
    }

    public void AddTempLegacyAnimclip(string filepath)
    {
        if (!tempLegacyAnimclips.Contains(filepath))
            tempLegacyAnimclips.Add(filepath);
    }

    public void Dispose()
    {
        builderDic.Clear();
        outputs.Clear();
        inputs.Clear();
        tempFileList.Clear();

        for (int i = 0; i < tempLegacyAnimclips.Count; ++i)
        {
            string fileName = tempLegacyAnimclips[i];
            var animClip = AssetDatabase.LoadAssetAtPath(fileName, typeof(AnimationClip)) as AnimationClip;
            if (animClip)
            {
                animClip.legacy = false;
            }
        }
        tempLegacyAnimclips.Clear();
    }

}
