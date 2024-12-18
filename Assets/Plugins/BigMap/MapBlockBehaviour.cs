using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class MapBlockBehaviour : MonoBehaviour
{
    [Serializable]
    class GameObjInfo
    {
        // go 信息
        public GameObject go;

        // 包围盒
        public Vector3 center;
        public Vector3 size;

        // 光照贴图, 1个 go 仅支持1个 Render
        public int lm_index;
        public Vector4 lm_attrib;
    }

    //
    public static bool ShowBounds1 = false;
    public static bool ShowBounds2 = true;

    // 外部使用
    [HideInInspector]
    public bool is_global;

    [HideInInspector]
    public int ix;

    [HideInInspector]
    public int iz;

    [HideInInspector]
    public List<GameObject> tmp_list;

    //
    [SerializeField]
    GameObjInfo[] go_infos;

    [HideInInspector]
    public Vector3 center;

    [HideInInspector]
    public Vector3 size;

    //
    const int INVALID_INDEX = 255;

    public Texture2D[] lightmaps;
    public int LightmapWidth = 0;
    public int LightmapHeight = 0;

    //
    public GameObject terrain;


    // 更新列表
    public void UpdateList()
    {
        var count = tmp_list.Count;

        //
        Bounds b2 = new Bounds();
        var pref_infos = go_infos;
        go_infos = new GameObjInfo[count];
        for (int i = 0; i < count; i++)
        {
            var go = tmp_list[i];

            var info = new GameObjInfo();
            go_infos[i] = info;

            // go
            info.go = go;

            // bound
            var b = GameObjectUtils.GetRenderBounds(go, true);
            info.center = b.center;
            info.size = b.size;

            if (i == 0) b2 = b;
            else b2.Encapsulate(b);

            // lightmap
            info.lm_index = INVALID_INDEX;

            // 保存之前的光照贴图信息
            if (pref_infos != null)
            {
                foreach (var info0 in pref_infos)
                {
                    if (info0.go == go)
                    {
                        info.lm_index = info0.lm_index;
                        info.lm_attrib = info0.lm_attrib;
                        break;
                    }
                }
            }
        }
        center = b2.center;
        size = b2.size;

        //
        tmp_list = null;
    }

    // 获取所有 go
    public GameObject[] GetAllObjs()
    {
        var list = new List<GameObject>();
        foreach (var info in go_infos) list.Add(info.go);
        return list.ToArray();
    }

    // 保存光照贴图信息
    public void SaveLightmap(Texture2D[] texs)
    {
        // 保存贴图数组
        lightmaps = texs;

        // 设置 光照贴图属性
        foreach (var info in go_infos)
        {
            var go = info.go;

            //
            var mr = go.GetComponentInChildren<MeshRenderer>();
            if (mr == null)
            {
                info.lm_index = INVALID_INDEX;
                continue;
            }

            //
            info.lm_index = mr.lightmapIndex;
            if (info.lm_index < 0) info.lm_index = INVALID_INDEX;
            info.lm_attrib = mr.lightmapScaleOffset;
        }
    }

    // 获取光照贴图面积
    public float GetLightmapSize()
    {
        float size = 0;
        foreach (var info in go_infos)
        {
            if (info.lm_index != INVALID_INDEX)
            {
                size += (info.lm_attrib.w * info.lm_attrib.z);
            }
        }
        return size;
    }

    // 清空光照贴图信息, 但不会删除 texture2d 资源
    public void ClearLightmapInfo()
    {
        // 删除 贴图数组
        lightmaps = null;

        // 删除信息
        foreach (var info in go_infos)
        {
            var go = info.go;

            //
            var mr = go.GetComponentInChildren<MeshRenderer>();
            if (mr != null)
            {
                mr.lightmapIndex = INVALID_INDEX;
            }

            //
            info.lm_index = INVALID_INDEX;
        }
    }

    // 是否有光照贴图
    public int LightmapCount
    {
        get { return lightmaps != null ? lightmaps.Length : 0; }
    }

    public Texture2D[] Lightmaps
    {
        get { return lightmaps; }
    }

    // 显示光照贴图
    public void ShowLightmap(int base_index, LightmapData[] datas)
    {
        if (lightmaps == null) return;

        //
        int len = lightmaps.Length;
        if (base_index + len >= INVALID_INDEX) return;

        // 设置 tex
        for (int i = 0; i < len; i++)
        {
            var data = datas[base_index + i];
            data.lightmapColor = lightmaps[i];
            data.lightmapDir = null;
        }

        // 设置各 go 
        foreach (var info in go_infos)
        {
            if (info.lm_index != INVALID_INDEX)
            {
                var mr = info.go.GetComponentInChildren<MeshRenderer>();
                if (mr)
                {
                    mr.lightmapIndex = base_index + info.lm_index;
                    mr.lightmapScaleOffset = info.lm_attrib;
                }
            }
        }
    }

    // 隐藏
    public void HideLightmap()
    {
        foreach (var info in go_infos)
        {
            if (info.lm_index != INVALID_INDEX)
            {
                var mr = info.go.GetComponentInChildren<MeshRenderer>();
                if (mr)
                {
                    mr.lightmapIndex = INVALID_INDEX;
                }
            }
        }
    }

    //
    public void UpdateTerrain(GameObject go)
    {
        ClearTerrain();

        terrain = go;

        //
        go.transform.parent = transform;
    }

    public void ClearTerrain()
    {
        if (terrain)
        {
            GameObject.DestroyImmediate(terrain);
        }
    }

    //
    void OnDrawGizmosSelected()
    {
        if (ShowBounds1 || ShowBounds2)
        {
            if (go_infos != null)
            {
                Gizmos.color = new Color(0, 1, 0, 0.2f);

                // 绘制每个包围盒
                if (ShowBounds1)
                {
                    foreach (var info in go_infos)
                    {
                        Gizmos.DrawCube(info.center, info.size);
                    }
                }

                // 绘制外部包围盒
                if (ShowBounds2)
                {
                    Gizmos.DrawCube(center, size);
                }
            }
        }
    }
}
