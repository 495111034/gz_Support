using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

/// <summary>
/// 场景编辑工具
/// </summary>
class SceneEditor
{
    /// <summary>
    /// 场景编辑器
    /// </summary>
    [MenuItem("Editor/Map Mask Editor")]
    public static void MapMaskEditor()
    {
        var go = GameObject.Find("MapMask");
        if (go == null)
        {
            go = new GameObject("MapMask");
        }
        if (go.GetComponent<MapMaskBehaviour>() == null)
        {
            go.AddComponent<MapMaskBehaviour>();
        }
        Selection.activeGameObject = go;
    }

    /// <summary>
    /// AOI编辑器
    /// </summary>
    [MenuItem("Editor/Aoi Editor")]
    public static void AoiEditor()
    {
        var go = GameObject.Find("AoiEditor");
        if (go == null)
        {
            go = new GameObject("AoiEditor");
        }
        if (go.GetComponent<AoiBehaviour>() == null)
        {
            go.AddComponent<AoiBehaviour>();
        }
        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;
        Selection.activeGameObject = go;
    }


    /// <summary>
    /// 修改地形分辨率
    /// </summary>
    [MenuItem("Editor/Terrain/Change Alphamap Resolution/To 512")]
    public static void ChangeTerrainAlphamapResolution512()
    {
        ChangeTerrainAlphamapResolution(512);
    }
    [MenuItem("Editor/Terrain/Change Alphamap Resolution/To 256")]
    public static void ChangeTerrainAlphamapResolution256()
    {
        ChangeTerrainAlphamapResolution(256);
    }
    [MenuItem("Editor/Terrain/Change Alphamap Resolution/To 128")]
    public static void ChangeTerrainAlphamapResolution128()
    {
        ChangeTerrainAlphamapResolution(128);
    }
    [MenuItem("Editor/Terrain/Change Alphamap Resolution/To 64")]
    public static void ChangeTerrainAlphamapResolution64()
    {
        ChangeTerrainAlphamapResolution(64);
    }
    [MenuItem("Editor/Terrain/Change Alphamap Resolution/To 32")]
    public static void ChangeTerrainAlphamapResolution32()
    {
        ChangeTerrainAlphamapResolution(32);
    }
    [MenuItem("Editor/Terrain/Change Alphamap Resolution/To 16")]
    public static void ChangeTerrainAlphamapResolution16()
    {
        ChangeTerrainAlphamapResolution(16);
    }

    [MenuItem("Editor/Dump Selection Info")]
    public static void DumpSelectionInfo()
    {
        var go = Selection.activeGameObject;
        if (go == null)
        {
            Debug.Log("No selection!");
            return;
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendFormat("Dump Selection, go:{0}\n", go);

        // 地形
        var t = go.GetComponent<Terrain>();
        if (t)
        {
            var td = t.terrainData;
            sb.AppendFormat("Terrain:{0}, pos:{1}\n", t, t.transform.position);
#if UNITY_2020_3_OR_NEWER
            sb.AppendFormat("heightmapWidth:{0}, heightmapHeight:{1}, heightmapResolution:{2},heightmapScale:{3}\n", td.heightmapResolution, td.heightmapResolution, td.heightmapResolution, td.heightmapScale);
#else
            sb.AppendFormat("heightmapWidth:{0}, heightmapHeight:{1}, heightmapResolution:{2},heightmapScale:{3}\n", td.heightmapWidth, td.heightmapHeight, td.heightmapResolution, td.heightmapScale);
#endif
            sb.AppendFormat("alphamapWidth:{0}, alphamapHeight:{1}, alphamapResolution:{2},alphamapLayers:{3}\n", td.alphamapWidth, td.alphamapHeight, td.alphamapResolution, td.alphamapLayers);
            sb.AppendFormat("size:{0}, baseMapResolution:{1}\n", td.size, td.baseMapResolution);
        }

        //
        Debug.Log(sb.ToString());
    }

    // 修改地形分辨率
    static void ChangeTerrainAlphamapResolution(int resolution)
    {
        TerrainData td = null;

        // 获取 td
        var obj = Selection.activeObject;
        if (obj is TerrainData)
        {
            td = obj as TerrainData;
        }
        else if (obj is GameObject)
        {
            var t = (obj as GameObject).GetComponent<Terrain>();
            if (t != null)
            {
                td = t.terrainData;
            }
        }
        if (td == null)
        {
            Debug.LogError("请选择 Terrain 后重新操作");
            return;
        }
        if (td.alphamapResolution == resolution)
        {
            return;
        }

        // 
        int width = td.alphamapWidth;
        int height = td.alphamapHeight;
        var maps = td.GetAlphamaps(0, 0, width, height);
        var layer_count = maps.GetLength(2);

        td.alphamapResolution = resolution;

        var width2 = td.alphamapWidth;
        var height2 = td.alphamapHeight;
        if (width == width2 && height == height2) return;
        var maps2 = td.GetAlphamaps(0, 0, width2, height2);

        // 
        for (var y2 = 0; y2 < height2; y2++)
        {
            for (var x2 = 0; x2 < width2; x2++)
            {
                var x = GetValue2(x2, width2, width);
                var y = GetValue2(y2, height2, height);
                for (int i = 0; i < layer_count; i++)
                {
                    var a = maps[x, y, i];
                    maps2[x2, y2, i] = a;
                }
            }
        }
        td.SetAlphamaps(0, 0, maps2);
    }

    //
    static int GetValue2(int value1, int length1, int length2)
    {
        var value2 = Mathf.RoundToInt((float)value1 / length1 * length2);
        return Mathf.Clamp(value2, 0, length2 - 1);
    }

    /// <summary>
    /// 转换 PNG
    /// </summary>
    [MenuItem("Editor/Texture/Save To PNG")]
    public static void SaveToPng()
    {
        LoadOrSavePng(true, true);
    }
    [MenuItem("Editor/Texture/Load From PNG")]
    public static void LoadFromPng()
    {
        LoadOrSavePng(false, true);
    }
    static void LoadOrSavePng(bool bSave, bool fill_alpha)
    {
        var tex = Selection.activeObject as Texture2D;
        if (tex == null)
        {
            Debug.LogError("must select a Texture2D");
            return;
        }

        var pathname = AssetDatabase.GetAssetPath(tex);
        var path = Path.GetDirectoryName(pathname);
        var name = Path.GetFileNameWithoutExtension(pathname);
        var pathname2 = Path.Combine(path, name) + ".png";

        if (bSave)
        {
            // get bytes
            byte[] bytes = null;
            if (fill_alpha)
            {
                var tex2 = new Texture2D(tex.width, tex.height, tex.format, false);
                var pixs = tex.GetPixels();
                AlignPixs(pixs, 1);
                tex2.SetPixels(pixs);
                bytes = tex2.EncodeToPNG();
                Texture2D.DestroyImmediate(tex2);
            }
            else
            {
                bytes = tex.EncodeToPNG();
            }

            // save
            File.WriteAllBytes(pathname2, bytes);
            AssetDatabase.Refresh();

            //
            Debug.Log(string.Format("Save Png {0} => {1}", pathname, pathname2));
        }
        else
        {
            var tex2 = AssetDatabase.LoadAssetAtPath(pathname2, typeof(Texture2D)) as Texture2D;
            if (tex2 == null)
            {
                Log.LogError("cant load texture:{0}", pathname2);
                return;
            }
            var pixs = tex2.GetPixels();
            AlignPixs(pixs, 0);
            tex.SetPixels(pixs);
        }
    }
    [MenuItem("Editor/Texture/Align PNG")]
    public static void AlignPng()
    {
        var tex = Selection.activeObject as Texture2D;
        if (tex == null)
        {
            Log.LogError("must select texture!");
            return;
        }

        var pixs = tex.GetPixels();
        AlignPixs(pixs, 0);
        tex.SetPixels(pixs);

        var pathname = AssetDatabase.GetAssetPath(tex);
        File.WriteAllBytes(pathname, tex.EncodeToPNG());

        AssetDatabase.Refresh();
    }

    //
    static void AlignPixs(Color[] pixs, float alpha)
    {
        for (int i = 0; i < pixs.Length; i++)
        {
            var c = pixs[i];
            var r = c.r;
            var g = c.g;
            var sum = r + g;
            pixs[i] = new Color(r / sum, g / sum, 0, alpha);
        }
    }


    /// <summary>
    /// 小地图相机
    /// </summary>
    [MenuItem("Editor/Small Map Camera")]
    public static void SmallMapCamera()
    {
        string name = "Small Map Camera";
        var go = GameObject.Find(name);
        if (go == null)
        {
            go = new GameObject(name, typeof(Camera), typeof(SmallMapBehaviour));
        }
        Selection.activeGameObject = go;
    }


#region CreateTerrainMeshPrefab

    // 把 TerrainMesh 构建成 prefab
    //[MenuItem("Assets/CreateTerrainMeshPrefab")]
    public static void CreateTerrainMeshPrefab()
    {
        ForeachScene(_CreateTerrainMeshPrefab);
    }
    static void _CreateTerrainMeshPrefab()
    {
        List<GameObject> list = new List<GameObject>();
        var gos = GameObjectUtils.FindAllGosInScene();
        foreach (var go in gos)
        {
            if (PrefabUtility.GetCorrespondingObjectFromSource(go) != null) continue;
            if (IsTerrainMesh(go))
            {
                list.Add(go);
            }
        }
        if (list.Count == 0) return;

        var scene_name = Path.GetFileNameWithoutExtension(EditorApplication.currentScene).ToLower();
        foreach (var go in list)
        {
            var mf = go.GetComponentInChildren<MeshFilter>();
            var pathname = AssetDatabase.GetAssetPath(mf.sharedMesh).ToLower();
            var path = Path.GetDirectoryName(pathname);

            var prefab_name = path + "/" + scene_name + "-" + go.name.ToLower() + ".prefab";
            var prefab = PrefabUtility.CreateEmptyPrefab(prefab_name);
            PrefabUtility.ReplacePrefab(go, prefab, ReplacePrefabOptions.ConnectToPrefab);
            Debug.Log(string.Format("create terrain_mesh, scene_name:{0}, prefab:{1}", scene_name, prefab_name));
        }

        EditorApplication.SaveScene();
    }

    // 判断是否是 地形Mesh
    static bool IsTerrainMesh(GameObject go)
    {
        if (!go.name.Contains("(mesh)")) return false;

        var t = go.transform;
        if (t.parent != null) return false;

        var mf = go.GetComponentInChildren<MeshFilter>();
        if (mf == null) return false;

        var pathname = AssetDatabase.GetAssetPath(mf.sharedMesh).ToLower();
        if (!pathname.StartsWith(PathDefs.ASSETS_PATH_TERRAIN_MESH)) return false;

        return true;
    }

#endregion


    // 删除碰撞体
    [MenuItem("Assets/Remove Collider")]
    static void RemoveCollider()
    {
        Object[] objs = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
        Debug.Log("RemoveCollider, count:" + objs.Length);

        //int mask = (int)(ObjLayerMask.Building | ObjLayerMask.Terrain | ObjLayerMask.Box);
        int mask = 0;
        foreach (var obj in objs)
        {
            var go = obj as GameObject;
            if (go == null) continue;

            if (((1 << go.layer) & mask) == 0)
            {
                bool changed = false;
                foreach (var c in GameObjectUtils.GetComponentsEx<Collider>(go, true))
                {
                    changed = true;
                    GameObject.DestroyImmediate(c, true);
                }
                if (changed)
                {
                    Debug.Log("Disable collider:" + AssetDatabase.GetAssetPath(go));
                }
            }
        }

        //
        AssetDatabase.SaveAssets();
    }




    // 对每个场景(或当前场景)执行 callback
    static void ForeachScene(Action callback)
    {
        bool found = false;
        Object[] objs = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
        foreach (var obj in objs)
        {
            var pathname = AssetDatabase.GetAssetPath(obj).ToLower();
            if (pathname.EndsWith(".unity"))
            {
                found = true;
                EditorApplication.OpenScene(pathname);
                callback();
            }
        }
        if (!found)
        {
            callback();
        }
    }

}
