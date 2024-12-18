using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

/// <summary>
/// 转换 Terrain 为 Mesh
/// </summary>
class Terrain2MeshEditor
{
    /// <summary>
    /// 把 Terrain 转换为 Mesh
    /// </summary>
    [MenuItem("Editor/Terrain/Create Mesh Terrain")]
    static void CreateMeshTerrain()
    {
        var t = ExportTerrain.GetTargetTerrain();
        if (t == null) return;
        var td = t.terrainData;
        var path = ExportTerrain.GetTerrainMeshPath(td);

        // 获取 mesh
        var mesh_pathname = ExportTerrain.GetTerrainMeshPathName(td);
        var mesh = AssetDatabase.LoadAssetAtPath(mesh_pathname, typeof(Mesh)) as Mesh;
        if (mesh == null)
        {
            Debug.LogError(string.Format("Can not found obj file:{0}. Please run 'Export To Obj...' first!", mesh_pathname));
            return;
        }

        // 获取 splats
        var alphas = GetSplatAlphas(td);

        // 创建模型对象
        var go_name = t.name.ToLower() + "(mesh)";
        var mesh_go = new GameObject(go_name);
        mesh_go.transform.position = t.transform.position;
        mesh_go.layer = t.gameObject.layer;
        mesh_go.isStatic = true;

        // 每4个一组, 为所有 splatPrototypes 添加子对象
        var splats_len = td.splatPrototypes.Length;
        for (int splat_idx = 0; splat_idx < splats_len; splat_idx += 4)
        {
            var obj_idx = splat_idx / 4;

            // 创建 mat
            var mat_pathname = path + string.Format("terrain_material {0}.asset", obj_idx);
            string sdr_name;
            if (splat_idx == 0)
            {
                sdr_name = "Yixin/Terrain/FirstPass";
                if (splats_len == 2) sdr_name += "2";
                else if (splats_len == 3) sdr_name += "3";
            }
            else
            {
                sdr_name = "Yixin/Terrain/AddPass";
            }
            var main_tex = alphas[obj_idx];
            var mat = CreateMaterial(sdr_name, td, main_tex, splat_idx);
            AssetDatabase.CreateAsset(mat, mat_pathname);

            // 创建 obj
            var obj_name = string.Format("splat {0}", obj_idx);
            var go = CreateMeshObject(obj_name, mesh, mat);

            // 挂接对象
            go.transform.parent = mesh_go.transform;
            go.transform.localPosition = Vector3.zero;
            go.layer = t.gameObject.layer;
            go.isStatic = true;
            go.tag = ObjTag.CheckShader;        // 发布为安卓平台后, shader 在编辑器中不正常, 因此需后期检测
        }

        // 创建 prefab
        {
            var scene_name = Path.GetFileNameWithoutExtension(EditorApplication.currentScene).ToLower();
            var prefab_name = path + scene_name + "-" + go_name + ".prefab";
            var prefab = PrefabUtility.CreateEmptyPrefab(prefab_name);
            PrefabUtility.ReplacePrefab(mesh_go, prefab, ReplacePrefabOptions.ConnectToPrefab);
        }

        //
        Debug.Log(string.Format("CreateMeshTerrain done!, obj:{0}", mesh_go.name));
    }

    // 获取 TerrainData 中的贴图
    static List<Texture2D> GetSplatAlphas(TerrainData td)
    {
        List<Texture2D> list = new List<Texture2D>();
        var deps = EditorUtility.CollectDependencies(new Object[] { td });
        foreach (var obj in deps)
        {
            if (obj is Texture2D && obj.name.StartsWith("SplatAlpha"))
            {
                list.Add(obj as Texture2D);
            }
        }
        list.Sort((a, b) => { return string.Compare(a.name, b.name); });
        return list;
    }

    // 创建模型对象
    static GameObject CreateMeshObject(string obj_name, Mesh mesh, Material mat)
    {
        var go = new GameObject(obj_name);

        var mf = go.AddComponent<MeshFilter>();
        mf.sharedMesh = mesh;

        var mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = mat;

        var mc = go.AddComponent<MeshCollider>();
        mc.sharedMesh = mesh;

        return go;
    }

    // 创建材质
    static Material CreateMaterial(string sdr_name, TerrainData td, Texture2D main_tex, int splat_idx)
    {
        var mat = new Material(resource.ShaderManager.Find(sdr_name));

        // contrl
        mat.SetTexture("_Control", main_tex);

        // splat
        var splats = td.splatPrototypes;
        for (int i = 0; i < 4; i++)
        {
            var idx = splat_idx + i;
            if (idx >= splats.Length) break;

            var splat = splats[idx];
            if (splat == null) continue;

            mat.SetTexture("_Splat" + i, splat.texture);

            var tx = td.size.x / splat.tileSize.x;
            var ty = td.size.z / splat.tileSize.y;
            mat.SetVector("_Tiling" + i, new Vector4(tx, ty, 1, 1));
        }

        //
        return mat;
    }

    /// <summary>
    /// 把多个地图模型, 合并成一个大地图
    /// </summary>
    [MenuItem("Editor/Terrain/Merge Mesh Group")]
    static void MergeMeshGroup()
    {
        var t_go = Selection.activeGameObject;
        if (t_go == null)
        {
            Log.LogError("please select a terrain-gameobject");
            return;
        }

        var t_prefab = PrefabUtility.GetCorrespondingObjectFromSource(t_go);
        if (t_prefab == null)
        {
            Log.LogError("cant find terrain-prefab");
            return;
        }

        var pathname_prefab = AssetDatabase.GetAssetPath(t_prefab).ToLower();
        if (!pathname_prefab.StartsWith(PathDefs.ASSETS_PATH_TERRAIN_MESH))
        {
            Log.LogError("terrain-prefab path error!");
            return;
        }

        //
        var path = Path.GetDirectoryName(pathname_prefab);

        // 读入材质
        var mat0_fname = Path.Combine(path, "terrain_material 0.asset");
        var mat0 = AssetDatabase.LoadAssetAtPath(mat0_fname, typeof(Material)) as Material;
        if (mat0 == null)
        {
            Log.LogError("cant find material at:{0}", mat0_fname);
            return;
        }

        // 读入贴图
        var tex0 = mat0.GetTexture("_Control") as Texture2D;
        {
            // 优先使用 PNG
            var pathname = AssetDatabase.GetAssetPath(tex0);
            pathname = Path.ChangeExtension(pathname, ".png");
            if (File.Exists(pathname))
            {
                tex0 = AssetDatabase.LoadAssetAtPath(pathname, typeof(Texture2D)) as Texture2D;
            }
        }

        // 读入 obj 文件列表
        var files = Directory.GetFiles(path, "terrain_mesh_*.obj", SearchOption.TopDirectoryOnly);
        Array.Sort(files);

        // 获取个数
        var x_count = 0;
        var z_count = 0;
        {
            var name = Path.GetFileNameWithoutExtension(files[files.Length - 1]);
            var ext = name.Replace("terrain_mesh_", null);
            x_count = ext[0] - '0' + 1;
            z_count = ext[1] - '0' + 1;
        }
        var tex_width = tex0.width / x_count;
        var tex_height = tex0.height / z_count;

        //
        foreach (var file in files)
        {
            var name = Path.GetFileNameWithoutExtension(file);
            var ext = name.Replace("terrain_mesh_", null);
            var x_i = ext[0] - '0';
            var z_i = ext[1] - '0';

            // 创建 go
            GameObject go;
            var t = t_go.transform.Find(name);
            if (t != null)
            {
                go = t.gameObject;
            }
            else
            {
                go = new GameObject(name);
                go.layer = t_go.layer;
                go.transform.parent = t_go.transform;
                go.transform.localPosition = Vector3.zero;
                go.transform.localScale = Vector3.one;
                go.transform.localRotation = Quaternion.identity;
            }

            // 创建 mat
            var mat_name = name.Replace("_mesh_", "_mat_") + ".asset";
            var mat_fname = Path.Combine(path, mat_name);
            if (File.Exists(mat_fname))
            {
                AssetDatabase.DeleteAsset(mat_fname);
            }
            AssetDatabase.CopyAsset(mat0_fname, mat_fname);

            // 创建 tex
            var tex_name = name.Replace("_mesh_", "_tex_") + ".png";
            var tex_fname = Path.Combine(path, tex_name);
            if (File.Exists(tex_fname))
            {
                File.Delete(tex_fname);
            }
            var tex_x = x_i * tex_width;
            var tex_z = z_i * tex_height;
            var colors = tex0.GetPixels(tex_x, tex_z, tex_width, tex_height);
            var tex = new Texture2D(tex_width, tex_height, TextureFormat.ARGB32, false);
            tex.SetPixels(colors);
            File.WriteAllBytes(tex_fname, tex.EncodeToPNG());

            // 读入 mat
            AssetDatabase.Refresh();
            var mat = AssetDatabase.LoadAssetAtPath(mat_fname, typeof(Material)) as Material;
            tex = AssetDatabase.LoadAssetAtPath(tex_fname, typeof(Texture2D)) as Texture2D;
            mat.SetTexture("_Control", tex);
            for (int i = 0; i < 4; i++)
            {
                var tiling_name = "_Tiling" + i;
                if (mat.HasProperty(tiling_name))
                {
                    var v = mat.GetVector(tiling_name);
                    v.x /= x_count;
                    v.y /= z_count;
                    mat.SetVector(tiling_name, v);
                }
            }

            // 创建 mf/mr
            var mesh = AssetDatabase.LoadAssetAtPath(file, typeof(Mesh)) as Mesh;
            var mf = GameObjectUtils.AddMissingComponent<MeshFilter>(go);
            mf.sharedMesh = mesh;

            var mr = GameObjectUtils.AddMissingComponent<MeshRenderer>(go);
            mr.sharedMaterial = mat;

            // 加入碰撞体
            var mc = GameObjectUtils.AddMissingComponent<MeshCollider>(go);
        }
    }
}
