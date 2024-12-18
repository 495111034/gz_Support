using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;
using Mask = System.Byte;

/// <summary>
/// 用于地图掩码绘制的 Mesh
/// </summary>
public class MapMaskMesh
{
    public const int GRID_PER_MESH = 100;
    public const int SIZE_PER_GRID = 1;

    public static readonly Color COLOR_OUT = new Color32(0, 0, 0, 0);
    public static readonly Color COLOR_BLOCK = new Color32(200, 0, 0, 128);

    public static readonly Color COLOR_PASS1 = new Color32(255, 255, 0, 128);
    public static readonly Color COLOR_PASS2 = new Color32(255, 255, 0, 255);

    public static readonly Color COLOR_PASS3 = new Color32(0, 255, 0, 128);
    public static readonly Color COLOR_PASS4 = new Color32(0, 255, 0, 255);

    public static readonly Color COLOR_PASS5 = new Color32(0, 255, 255, 128);
    public static readonly Color COLOR_PASS6 = new Color32(0, 255, 255, 255);

    public static readonly Color COLOR_PASS7 = new Color32(0, 0, 255, 128);
    public static readonly Color COLOR_PASS8 = new Color32(0, 0, 255, 255);

    public static readonly string[] MASK_NAMES = new string[] { " 红 0", "黄 1", "黄 2", "绿 3", "绿 4", "青 5", "青 6", "紫 7", "紫 8" };
    public static readonly Color[] MASK_COLORS = new Color[] { COLOR_BLOCK, COLOR_PASS1, COLOR_PASS2, COLOR_PASS3, COLOR_PASS4, COLOR_PASS5, COLOR_PASS6, COLOR_PASS7, COLOR_PASS8 };

    // 单个网格数据, 整个地图由多个网格组成
    class MeshInfo
    {
        public Mesh mesh;
        public Color[] colors;
        public bool dirty;
    }

    Mesh[,] _meshs;                 // [z,x], 网格数组
    MeshInfo[,] _infos;             // info 数组
    int _x_width;                   // 尺寸信息
    int _z_height;
    Mask[] _mask;           // 总体掩码


    // 清空
    public void Clear()
    {
        if (_meshs != null)
        {
            foreach (var mesh in _meshs)
            {
                Object.DestroyImmediate(mesh);
            }
        }

        _meshs = null;
        _infos = null;
        _x_width = 0;
        _z_height = 0;
        _mask = null;
    }

    // 读入掩码
    public void LoadMask(int x_width, int z_height, Mask[] mask)
    {
        // 清空
        Clear();

        // 保存数据
        int z_count = Mathf.CeilToInt((float)z_height / GRID_PER_MESH);
        int x_count = Mathf.CeilToInt((float)x_width / GRID_PER_MESH);

        _meshs = new Mesh[z_count, x_count];
        _infos = new MeshInfo[z_count, x_count];
        _x_width = x_width;
        _z_height = z_height;
        _mask = mask;

        // 创建网格
        float offset = SIZE_PER_GRID * 0.05f;
        for (int zc = 0; zc < z_count; zc++)
        {
            float zc0 = zc * GRID_PER_MESH * SIZE_PER_GRID;
            for (int xc = 0; xc < x_count; xc++)
            {
                float xc0 = xc * GRID_PER_MESH * SIZE_PER_GRID;

                var verts = new Vector3[GRID_PER_MESH * GRID_PER_MESH * 4];
                var tris = new int[GRID_PER_MESH * GRID_PER_MESH * 6];
                var colors = new Color[GRID_PER_MESH * GRID_PER_MESH * 4];
                int iv = 0, it = 0;
                for (int zg = 0; zg < GRID_PER_MESH; zg++)
                {
                    float zg0 = zc0 + zg * SIZE_PER_GRID;
                    for (int xg = 0; xg < GRID_PER_MESH; xg++)
                    {
                        float xg0 = xc0 + xg * SIZE_PER_GRID;

                        // 3 2
                        // 0 1
                        verts[iv + 0] = new Vector3(xg0 + offset, 0, zg0 + offset);
                        verts[iv + 1] = new Vector3(xg0 - offset + SIZE_PER_GRID, 0, zg0 + offset);
                        verts[iv + 2] = new Vector3(xg0 + offset, 0, zg0 + SIZE_PER_GRID - offset);
                        verts[iv + 3] = new Vector3(xg0 - offset + SIZE_PER_GRID, 0, zg0 + SIZE_PER_GRID - offset);

                        // 023, 031
                        tris[it + 0] = iv + 0;
                        tris[it + 1] = iv + 2;
                        tris[it + 2] = iv + 3;
                        tris[it + 3] = iv + 0;
                        tris[it + 4] = iv + 3;
                        tris[it + 5] = iv + 1;

                        //
                        colors[iv + 0] = COLOR_OUT;
                        colors[iv + 1] = COLOR_OUT;
                        colors[iv + 2] = COLOR_OUT;
                        colors[iv + 3] = COLOR_OUT;

                        //
                        iv += 4;
                        it += 6;
                    }
                }

                // 计算高度值
                UpdateHeight(verts);

                // 
                var mesh = _meshs[zc, xc] = new Mesh();
                mesh.hideFlags = HideFlags.HideAndDontSave;
                mesh.vertices = verts;
                mesh.colors = colors;
                mesh.triangles = tris;
                mesh.MarkDynamic();
                mesh.RecalculateBounds();

                var info = _infos[zc, xc] = new MeshInfo();
                info.mesh = mesh;
                info.colors = colors;
                info.dirty = false;
            }
        }

        // 更新掩码到 mesh/color
        UpdateMeshAndColor();
    }

    // 更新 掩码到 mesh/color
    void UpdateMeshAndColor()
    {
        for (int z = 0; z < _z_height; z++)
        {
            int z0 = z * _x_width;
            for (int x = 0; x < _x_width; x++)
            {
                var m = _mask[z0 + x];
                SetColor(x, z, m, MASK_COLORS[m]);
            }
        }
        UpdateColor();
    }

    // 更新颜色
    void UpdateColor()
    {
        foreach (var info in _infos)
        {
            if (info.dirty)
            {
                info.dirty = false;
                info.mesh.colors = info.colors;
            }
        }
    }

    // 自动设置掩码
    public void AutoSet(float degree)
    {
        if (_mask == null)
        {
            return;
        }

        var disableds = new List<Collider>();
        var colliders = GameObject.FindObjectsOfType<Collider>();
        foreach (var c in colliders)
        {
            if (c.gameObject.layer == (int)ObjLayer.Terrain && c.enabled && !c.gameObject.name.EndsWith("_nav") && c.transform.parent?.name != "block_nav") 
            {
                c.enabled = false;
                disableds.Add(c);
                Log.LogError($"错误的碰撞体，{c.gameObject.GetLocation()}, 不应该有 {c.GetType().Name}");
            }
        }

        var _ns = new float[][]
        {
            //new int[] { -1, -1 }, new int[] { -1,  1 },new int[] {  1, -1 },
            new float[] {  1,  1 },
            new float[] {  0,  1 },
            new float[] {  1,  0 },
            //
            new float[] {  1.05f,  1.05f },
            new float[] {  -0.05f,  1.05f },
            new float[] { 1.05f,  -0.05f },
            new float[] { -0.05f,  -0.05f },
        };

        var c0 = MapMaskMesh.MASK_COLORS[0];
        var c1 = MapMaskMesh.MASK_COLORS[1];
        for (int z = 0; z < _z_height; z++)
        {
            for (int x = 0; x < _x_width; x++)
            {
                var min = GetHeight(x, z);
                var max = min;
                foreach (var v2 in _ns)
                {
                    var h = GetHeight(x + v2[0] * 1f, z + v2[1] * 1f);
                    min = Mathf.Min(min, h);
                    max = Mathf.Max(max, h);
                }
                var a = Mathf.Atan2(max - min, 1) * Mathf.Rad2Deg;
                if (a >= degree || min <= -100 || x == 0 || x == _x_width - 1 || z ==0 || z == _z_height - 1)
                {
                    SetColor(x, z, 0, c0);
                }
                else if(_mask[z * _x_width + x] == 0)
                {
                    SetColor(x, z, 1, c1);
                }
            }
        }

        foreach (var c in disableds) 
        {
            c.enabled = true;
        }
        UpdateColor();       
    }

    // 设置范围颜色
    public void SetColor(int x0, int x1, int z0, int z1, Mask m, Color c)
    {
        for (int x = x0; x <= x1; x++)
            for (int z = z0; z <= z1; z++)
                SetColor(x, z, m, c);
        UpdateColor();
    }

    // 设置某个位置的颜色值
    void SetColor(int x, int z, Mask m, Color c)
    {
        if (x < 0 || x >= _x_width || z < 0 || z >= _z_height) return;

        int x1 = x / GRID_PER_MESH;
        int x2 = x % GRID_PER_MESH;
        int z1 = z / GRID_PER_MESH;
        int z2 = z % GRID_PER_MESH;

        var info = _infos[z1, x1];
        info.dirty = true;

        var colors = info.colors;
        var pid = x2 * 4 + z2 * GRID_PER_MESH * 4;
        colors[pid + 0] = c;
        colors[pid + 1] = c;
        colors[pid + 2] = c;
        colors[pid + 3] = c;

        if (_mask != null)
        {
            _mask[z * _x_width + x] = m;
        }
    }

    // 获取网格
    public Mesh[,] Meshs
    {
        get { return _meshs; }
    }


    // 更新高度值
    public static void UpdateHeight(Vector3[] verts)
    {
        for (int i = 0; i < verts.Length; i++)
        {
            var v = verts[i];
            v.y = GetHeight(v.x, v.z) + 0.3f;
            verts[i] = v;
        }
    }

    // 获取高度值
    public static float GetHeight(float x, float z)
    {
        RaycastHit hit;
        if (Physics.Raycast(new Vector3(x, 500, z), Vector3.down, out hit, 1000, (int)ObjLayerMask.HeightTest))
        {
            if (hit.transform.name == "block_nav" || (hit.transform.parent && hit.transform.parent.name == "block_nav")) 
            {
                return -100;
            }
            return hit.point.y;
        }
        return -100;
    }
    public static Vector3 GetPos(float x, float z, float add_height)
    {
        var y = GetHeight(x, z) + add_height;
        return new Vector3(x, y, z);
    }
}
