using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using System.IO;
using Object = UnityEngine.Object;

/// <summary>
/// 转换贴图
/// </summary>
class ChangeTexture
{
    class MaterialInfo
    {
        public string pathname;     // 路径
        public Material mat;            // 材质对象
        public Dictionary<string, Texture2D> deps;  // 依赖的贴图
    }

    // 获取 shader 上的贴图属性名列表
    public static List<string> GetShaderTextureNames(Shader sdr)
    {
        var id = sdr.GetInstanceID();
        List<string> names = null;
        if (!_sdr_infos.TryGetValue(id, out names))
        {
            names = new List<string>();
            var count = ShaderUtil.GetPropertyCount(sdr);
            for (int i = 0; i < count; i++)
            {
                if (ShaderUtil.GetPropertyType(sdr, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                {
                    var name = ShaderUtil.GetPropertyName(sdr, i);
                    names.Add(name);
                }
            }
            //Debug.Log(" get sdr tex names, sdr:" + sdr.name + ", names:" + string.Join(",", names.ToArray()));
            _sdr_infos.Add(id, names);
        }
        return names;
    }
    static Dictionary<int, List<string>> _sdr_infos = new Dictionary<int, List<string>>();

    // 获取所有材质信息
    static List<MaterialInfo> GetMaterialList()
    {
        _sdr_infos.Clear();

        //
        List<MaterialInfo> mat_list = new List<MaterialInfo>();
        List<string> files = new List<string>();
        {
            files.AddRange(Directory.GetFiles("assets", "*.mat", SearchOption.AllDirectories));
            files.AddRange(Directory.GetFiles("assets", "*.asset", SearchOption.AllDirectories));
        }
        Debug.Log("get mat:" + files.Count);
        foreach (var fname in files)
        {
            // 加载材质
            var mat = AssetDatabase.LoadAssetAtPath(fname, typeof(Material)) as Material;
            if (mat == null) continue;

            // 获取该材质中的所有贴图
            var sdr = mat.shader;
            if (sdr == null) continue;

            // 获取贴图名字列表
            var names = GetShaderTextureNames(sdr);
            if (names.Count == 0) continue;

            // 获取贴图列表
            Dictionary<string, Texture2D> deps = new Dictionary<string, Texture2D>();
            foreach (var name in names)
            {
                var tex = mat.GetTexture(name) as Texture2D;
                if (tex != null)
                {
                    var path = AssetDatabase.GetAssetPath(tex).ToLower();
                    //if (sdr.name.Contains("FirstPass2")) Debug.Log("222:" + path);
                    if (path.EndsWith(".dds"))
                    {
                        deps.Add(name, tex);
                    }
                }
            }
            if (deps.Count == 0) continue;

            // 保存信息
            var info = new MaterialInfo();
            info.pathname = fname.ToLower();
            info.mat = mat;
            info.deps = deps;
            mat_list.Add(info);
        }

        //
        Debug.Log("get mat list:" + mat_list.Count);
        return mat_list;
    }


    // 为 DDS 创建 PNG
    [MenuItem("ChangeTexture/Create Png")]
    static void CreatePng()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine(@"path=D:\Program Files\ImageMagick-6.8.8-Q8");
        var files = Directory.GetFiles("assets", "*.dds", SearchOption.AllDirectories);
        foreach (var fname in files)
        {
            var fname2 = Path.ChangeExtension(fname, ".png");
            if (!File.Exists(fname2))
            {
                sb.AppendFormat("convert \"{0}\" \"{1}\"", fname, fname2);
                sb.AppendLine();
            }
        }

        //
        var bat_name = "create_png.bat";
        File.WriteAllText(bat_name, sb.ToString());
        Debug.Log("Run " + bat_name);
    }

    /// <summary>
    /// 替换 DDS 为其它同名贴图
    /// </summary>
    [MenuItem("ChangeTexture/Replace DDS")]
    static void ReplaceDDS()
    {
        // 获取所有材质信息
        var mat_list = GetMaterialList();
        if (mat_list.Count == 0) return;

        // 遍历所有 dds 
        var dds_names = Directory.GetFiles("assets", "*.dds", SearchOption.AllDirectories);
        foreach (var dds_name in dds_names)
        {
            var tex = AssetDatabase.LoadAssetAtPath(dds_name, typeof(Texture2D)) as Texture2D;
            if (tex == null) continue;

            // 获取 png
            var png_name = Path.ChangeExtension(dds_name, ".png");
            var tex2 = AssetDatabase.LoadAssetAtPath(png_name, typeof(Texture2D)) as Texture2D;
            if (tex2 == null) continue;

            // 替换贴图
            Debug.Log(string.Format("replace dds, {0} => {1}", dds_name, png_name));
            foreach (var info in mat_list)
            {
                // 替换该材质
                foreach (var dep in info.deps)
                {
                    if (dep.Value == tex)
                    {
                        Debug.Log(string.Format("change material:{0}, prop:{1}", info.pathname, dep.Key));
                        info.mat.SetTexture(dep.Key, tex2);
                    }
                }
            }
        }
    }
}
