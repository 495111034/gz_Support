using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEditor;

/// <summary>
/// 大地图编辑器
/// </summary>
class BigMapEditor
{
    const string GO_NAME = "__big_map_editor";
    const string BLOCK_GLOBAL = "_global";


    // 打开编辑器
    [MenuItem("Editor/Big Map/Select Editor")]
    static void OpenBigMapEditor()
    {
        var go = GameObject.Find(GO_NAME);
        if (go == null)
        {
            go = new GameObject(GO_NAME);
        }
        if (go.GetComponent<BigMapBehaviour>() == null)
        {
            go.AddComponent<BigMapBehaviour>();
        }
        Selection.activeGameObject = go;
    }

    // 打开编辑器
    [MenuItem("Editor/Big Map/Select Block")]
    public static void SelectBlock()
    {
        var sel = Selection.activeGameObject;
        if (sel == null) return;

        var bm_go = GameObject.Find(GO_NAME);
        if (bm_go == null) return;

        var bm = bm_go.GetComponent<BigMapBehaviour>();
        if (bm == null) return;

        var block = GetBestBlock(bm, sel);
        if (block == null) return;

        var objs = block.GetAllObjs();
        if (!objs.Contains(sel)) return;

        //
        Selection.activeGameObject = block.gameObject;
    }


    #region 创建地图, 划分物件

    // 创建大地图
    public static bool CreateMap(BigMapBehaviour bm, int map_width, int map_height, int block_width, int block_height)
    {
        // 验证合法性
        if (bm == null ||
            map_width <= 0 || map_width > 2048 ||
            map_height <= 0 || map_height > 2048 ||
            block_width <= 0 || block_width > map_width ||
            block_height <= 0 || block_height > map_height)
        {
            Debug.LogError("CreateMap, 参数错误!");
            return false;
        }

        // 清空
        {
            var blocks = bm.GetComponentsInChildren<MapBlockBehaviour>();
            ClearLightmap(blocks);

            GameObjectUtils.DestroyChildren(bm.gameObject);
        }

        // 设置根
        var bm_t = bm.transform;

        // 创建全局块
        {
            var go = new GameObject(BLOCK_GLOBAL);
            go.transform.parent = bm_t;

            var block = go.AddComponent<MapBlockBehaviour>();
            block.is_global = true;

            _lightmap_list.Clear();
        }

        // 创建每个 block
        for (int ix = 0, x = 0; x < map_width; ix++, x += block_width)
        {
            for (int iz = 0, z = 0; z < map_height; iz++, z += block_height)
            {
                var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.transform.parent = bm_t;

                var mr = go.GetComponentInChildren<MeshRenderer>();
                mr.castShadows = false;
                mr.receiveShadows = false;

                var block = go.AddComponent<MapBlockBehaviour>();
                block.ix = ix;
                block.iz = iz;
            }
        }

        // 保存信息
        bm.MapWidth = map_width;
        bm.MapHeight = map_height;
        bm.BlockWidth = block_width;
        bm.BlockHeight = block_height;
        bm.MaxObjSize = Mathf.Max(block_width, block_height) * 2;
        bm.LightmapWidth = CeilPowerOfTwo(block_width * 8);         // 32=>256, 64=>512
        bm.LightmapHeight = CeilPowerOfTwo(block_height * 8);         // 32=>256, 64=>512

        // 对齐
        AlignBlocks(bm);

        // 放置对象
        PutGo(bm);

        //
        return true;
    }

    //
    static int CeilPowerOfTwo(int value)
    {
        return Mathf.IsPowerOfTwo(value) ? value : Mathf.NextPowerOfTwo(value);
    }

    // 放置 go
    public static void PutGo(BigMapBehaviour bm)
    {
        var blocks = bm.GetComponentsInChildren<MapBlockBehaviour>();

        // 清空光照贴图
        //ClearLightmap(blocks);        // PutGo 在坐标变更时, 经常调用, 因此不能删除光照贴图

        // 清空
        foreach (var b in blocks)
        {
            b.tmp_list = new List<GameObject>();
        }

        // 获取所有 go, 并分配 block
        var gos = Builder_All.GetExportGameObjects();
        foreach (var go in gos)
        {
            // 获取 block
            var block = GetBestBlock(bm, go);
            if (block == null)
            {
                continue;
            }

            //
            block.tmp_list.Add(go);
        }

        // 更新 bound
        foreach (var b in blocks)
        {
            b.UpdateList();
        }
    }

    // 获取最佳的 block, 如果尺寸太大, 则不适合
    static MapBlockBehaviour GetBestBlock(BigMapBehaviour bm, GameObject go)
    {
        // 获取渲染尺寸
        var bound = GameObjectUtils.GetRenderBounds(go, true);
        var size = bound.size;

        // 获取块名字
        string name = null;

        // 如果尺寸太大, 则放入全局块
        var obj_size = Mathf.Max(size.x, size.z);
        if (obj_size > bm.MaxObjSize)
        {
            name = BLOCK_GLOBAL;
        }
        else
        {
            // 根据位置, 判断所属的 ix/iz
            var pos = bound.center;

            var x = Mathf.Clamp(pos.x, 0, bm.MapWidth - 1);
            var z = Mathf.Clamp(pos.z, 0, bm.MapHeight - 1);

            var ix = (int)(x / bm.BlockWidth);
            var iz = (int)(z / bm.BlockHeight);

            // 根据 ix/iz, 确定块名字
            name = GetBlockName(ix, iz);
        }

        // 根据名字查找
        var t = bm.transform.Find(name);
        if (t == null)
        {
            Log.LogError("Cant find block: {0}", name);
            return null;
        }

        //
        var block = t.gameObject.GetComponent<MapBlockBehaviour>();
        return block;
    }

    // 对齐块
    public static void AlignBlocks(BigMapBehaviour bm)
    {
        foreach (var block in bm.GetComponentsInChildren<MapBlockBehaviour>())
        {
            AlignBlock(bm, block);
        }
    }
    public static void AlignBlock(BigMapBehaviour bm, MapBlockBehaviour block)
    {
        if (block.name == BLOCK_GLOBAL) return;

        // 获取 bm 信息
        if (bm == null)
        {
            bm = GetBigMap(block);
            if (bm == null) return;
        }

        var half_width = bm.BlockWidth / 2;
        var half_height = bm.BlockHeight / 2;
        var scale = new Vector3(bm.BlockWidth, 1, bm.BlockHeight);

        // 名字
        var ix = block.ix;
        var iz = block.iz;
        block.name = GetBlockName(ix, iz);

        // layer
        block.gameObject.layer = (int)ObjLayer.Terrain;

        // 位置
        var t = block.transform;
        t.localPosition = new Vector3(ix * bm.BlockWidth + half_width, 0, iz * bm.BlockHeight + half_height);
        t.localScale = scale;
        t.localRotation = Quaternion.identity;
    }

    #endregion

    //
    static string GetBlockName(int ix, int iz)
    {
        return "block " + GetBlockId(ix, iz);
    }

    static string GetBlockTerrainName(int ix, int iz)
    {
        return "terrain " + GetBlockId(ix, iz);
    }

    static string GetBlockId(int ix, int iz)
    {
        return string.Format("{0:D2}-{1:D2}", ix, iz);
    }

    static string GetLightmapName(MapBlockBehaviour block, int index)
    {
        var block_id = block.is_global ? "global" : GetBlockId(block.ix, block.iz);
        return string.Format("LightmapFar-{0}-{1}.exr", block_id, index);
    }

    // 获取地形资源文件名
    static string GetTerrainAssetPathname(TerrainData td, MapBlockBehaviour block, string asset_type, string ext)
    {
        var path = ExportTerrain.GetTerrainMeshPath(td);
        var id = GetBlockId(block.ix, block.iz);
        return string.Format("{0}terrain_{1}_{2}{3}", path, asset_type, id, ext);
    }

    //
    public static BigMapBehaviour GetBigMap(MapBlockBehaviour block)
    {
        return block.gameObject.transform.parent.GetComponent<BigMapBehaviour>();
    }

    public static BigMapBehaviour GetBigMap(MapBlockBehaviour[] blocks)
    {
        if (blocks == null || blocks.Length == 0) return null;
        var bm = GetBigMap(blocks[0]);
        return bm;
    }

    #region 光照贴图

    static List<MapBlockBehaviour> _lightmap_list = new List<MapBlockBehaviour>();  // 当前位于光照贴图中的块

    // 烘培光照贴图
    public static void BakeLightmap(MapBlockBehaviour[] blocks, bool bakeNew, bool bShow)
    {
        // 烘培
        var worker = new LightmapBakeWorker();
        worker.Start(blocks, bakeNew, bShow);
    }

    // 清空光照贴图
    public static void ClearLightmap(MapBlockBehaviour[] blocks)
    {
        // 隐藏
        HideLightmap(blocks);

        // 清空
        foreach (var block in blocks)
        {
            // 删除贴图
            var texs = block.Lightmaps;
            if (texs != null)
            {
                foreach (var tex in texs)
                {
                    if (tex != null)
                    {
                        var pathname = AssetDatabase.GetAssetPath(tex);
                        AssetDatabase.DeleteAsset(pathname);
                    }
                }
            }

            // 清空属性
            block.ClearLightmapInfo();
        }
    }

    // 保存光照贴图
    public static void AssignLightmap(MapBlockBehaviour block)
    {
        if (Lightmapping.isRunning) return;

        // 重命名
        var datas = LightmapSettings.lightmaps;
        var texs = new Texture2D[datas.Length];
        for (int i = 0; i < datas.Length; i++)
        {
            var tex = datas[i].lightmapColor;

            // 重命名
            var pathname1 = AssetDatabase.GetAssetPath(tex);
            var path = Path.GetDirectoryName(pathname1);
            var name = GetLightmapName(block, i);
            var pathname2 = Path.Combine(path, name);
            AssetDatabase.MoveAsset(pathname1, pathname2);

            //
            var tex2 = AssetDatabase.LoadAssetAtPath(pathname2, typeof(Texture2D)) as Texture2D;
            texs[i] = tex2;
        }

        // 保存信息
        block.SaveLightmap(texs);

        // 清空当前
        Lightmapping.Clear();
    }

    // 显示
    public static void ShowLightmap(MapBlockBehaviour[] blocks)
    {
        // 添加列表
        foreach (var block in blocks)
        {
            if (!_lightmap_list.Contains(block)) _lightmap_list.Add(block);
        }

        // 更新
        UpdateLightmap();
    }

    // 隐藏
    public static void HideLightmap(MapBlockBehaviour[] blocks)
    {
        // 删除列表
        foreach (var block in blocks)
        {
            _lightmap_list.Remove(block);
            block.HideLightmap();
        }

        // 更新
        UpdateLightmap();
    }

    // 更新显示
    static void UpdateLightmap()
    {
        if (Lightmapping.isRunning) return;

        // 清空
        Lightmapping.Clear();

        // 创建 data 数组
        var datas = new LightmapData[255];
        for (int i = 0; i < datas.Length; i++)
        {
            datas[i] = new LightmapData();
        }

        //
        int base_index = 0;
        foreach (var block in _lightmap_list)
        {
            if (block.LightmapCount > 0)
            {
                block.ShowLightmap(base_index, datas);
                base_index += block.LightmapCount;
            }
        }

        // 赋值
        LightmapSettings.lightmaps = datas;
    }

    #endregion

    #region 地形

    // 烘培地形
    public static void BakeTerrain(MapBlockBehaviour[] blocks)
    {
        var bm = GetBigMap(blocks);
        if (bm == null || bm.terrain == null) return;

        //
        foreach (var block in blocks)
        {
            var ret = BakeTerrain(bm, block);
            if (ret == 0)
            {
                continue;
            }
            else if (ret == 1)
            {
                UpdateTerrain(bm.terrain, block);
            }
            else
            {
                break;
            }
        }
    }
    // 烘培地形, 返回 0=忽略, 1=正确, 2=错误
    static int BakeTerrain(BigMapBehaviour bm, MapBlockBehaviour block)
    {
        if (block.is_global) return 0;

        //
        int ix = block.ix;
        int iz = block.iz;

        var t = bm.terrain;
        var td = t.terrainData;

        StringBuilder sb = new StringBuilder();
        sb.AppendFormat("BakeTerrain, td:{0}, block:{1}\n", td, block);

        // 地形 坐标/尺寸
        var t_pos = t.transform.position;
        var t_size = td.size;
        sb.AppendFormat("t_pos:{0}, t_size:{1}\n", t_pos, t_size);

        // 高度图尺寸
#if UNITY_2020_3_OR_NEWER
        var h_width = td.heightmapResolution - 1;
        var h_height = td.heightmapResolution - 1;
#else
        var h_width = td.heightmapWidth - 1;
        var h_height = td.heightmapHeight - 1;
#endif
        var h_size = new Vector3(h_width, 1, h_height);
        sb.AppendFormat("h_width:{0}, h_height:{1}, h_size:{2}\n", h_width, h_height, h_size);

        // 地形坐标 -> 高度图坐标 互转
        var t2h = new Vector3(h_size.x / t_size.x, h_size.y / t_size.y, h_size.z / t_size.z);
        var h2t = new Vector3(t_size.x / h_size.x, t_size.y / h_size.y, t_size.z / h_size.z);
        sb.AppendFormat("t2h:{0}, h2t:{1}\n", t2h, h2t);

        // 计算块范围
        var b_x0 = ix * bm.BlockWidth;
        var b_z0 = iz * bm.BlockHeight;
        var b_x1 = b_x0 + bm.BlockWidth;
        var b_z1 = b_z0 + bm.BlockHeight;
        var b_pos = new Vector3(b_x0, 0, b_z0);
        sb.AppendFormat("b range:{0},{1},{2},{3}\n", b_x0, b_x1, b_z0, b_z1);

        // 转换成高度图内的范围
        var h_x0 = Mathf.Clamp((int)((b_x0 - t_pos.x) * t2h.x), 0, h_width);
        var h_x1 = Mathf.Clamp((int)((b_x1 - t_pos.x) * t2h.x), 0, h_width);
        var h_z0 = Mathf.Clamp((int)((b_z0 - t_pos.z) * t2h.z), 0, h_height);
        var h_z1 = Mathf.Clamp((int)((b_z1 - t_pos.z) * t2h.z), 0, h_height);
        var h_w = h_x1 - h_x0;
        var h_h = h_z1 - h_z0;
        if (h_w <= 0 || h_h <= 0) return 0;   // 无可见地形
        sb.AppendFormat("h range:{0},{1},{2},{3}, h_w:{4}, h_h:{5}\n", h_x0, h_x1, h_z0, h_z1, h_w, h_h);

        // 高度度开始位置 的世界坐标
        var w_x0 = h_x0 * h2t.x + t_pos.x;
        var w_z0 = h_z0 * h2t.z + t_pos.z;

        // 获取高度图
        var heights = td.GetHeights(h_x0, h_z0, h_w + 1, h_h + 1);      // 考虑到边界, 高度图的尺寸总是额外 +1 的
        var h_line = h_w + 1;       // 行长

        // 转换成 mesh
        var num_vertexs = (h_w + 1) * (h_h + 1);        // 顶点个数
        var num_uvs = num_vertexs;                      // uv 个数
        var num_triangles = h_w * h_h * 2;              // 三角形个数 = 矩形个数 * 2 
        sb.AppendFormat("num_vertexs:{0}, num_uvs:{1}, num_triangles:{2}\n", num_vertexs, num_uvs, num_triangles);

        if (num_triangles * 3 >= 65535)
        {
            Debug.LogError("too many triangles!");
            return 2;
        }

        var vertexs = new Vector3[num_vertexs];
        var uvs = new Vector2[num_uvs];
        var tris = new int[num_triangles * 3];          // 每个三角形需要 3个索引值

        //var h2b = new Vector3(b_x0 - w_x0, 0, b_z0 - w_z0); // 高度图内坐标 => 块内坐标

        // 填充 顶点/UV
        for (int z = 0; z < h_h + 1; z++)
        {
            for (int x = 0; x < h_w + 1; x++)
            {
                var height = heights[z, x];

                // 高度图内的坐标
                //float tmp_x = h_x0 + x;
                float tmp_x = h_x0 - x;         // 测试发现 X 轴是反的, 不知道原因(可能是右手坐标系). 因此这里取负值
                float tmp_y = height;
                float tmp_z = h_z0 + z;

                // 转换为地形内坐标
                tmp_x = tmp_x * h2t.x;
                tmp_y = tmp_y * h2t.y;
                tmp_z = tmp_z * h2t.z;
                var pos1 = new Vector3(tmp_x, tmp_y, tmp_z);

                // 转换 世界坐标, 块内坐标
                var pos2 = pos1 + t_pos - b_pos;

                // 保存该顶点
                var id = z * h_line + x;
                vertexs[id] = pos2;

                var u = (float)x / h_w;
                var v = (float)z / h_h;
                uvs[id] = new Vector2(u, v);
            }
        }

        // 填充三角形
        var tri_id = 0;
        for (int z = 0; z < h_h; z++)
        {
            for (int x = 0; x < h_w; x++)
            {
                // 起始顶点ID
                var id0 = z * h_line + x;

                // 右手坐标系
                tris[tri_id++] = id0;
                tris[tri_id++] = id0 + h_line + 1;
                tris[tri_id++] = id0 + h_line;

                tris[tri_id++] = id0;
                tris[tri_id++] = id0 + 1;
                tris[tri_id++] = id0 + h_line + 1;
            }
        }

        // 保存 mesh 文件
        var mesh_pathname = GetTerrainAssetPathname(td, block, "mesh", ".obj");
        ExportTerrain.SaveMesh(mesh_pathname, vertexs, uvs, tris);
        sb.AppendFormat("SaveMesh, pathname:{0}\n", mesh_pathname);

        // 获取通道图尺寸
        var a_width = td.alphamapWidth;
        var a_height = td.alphamapHeight;
        var a_layers = td.alphamapLayers;
        var h2a = (float)a_width / h_width;         // 高度图 => 通道图 缩放值
        sb.AppendFormat("a_width:{0}, a_height:{1}, a_layers:{2}, h2a:{3}\n", a_width, a_height, a_layers, h2a);

        // 块范围 -> 通道图范围
        var a_x0 = Mathf.FloorToInt(h_x0 * h2a);
        var a_x1 = Mathf.FloorToInt(h_x1 * h2a);
        var a_z0 = Mathf.FloorToInt(h_z0 * h2a);
        var a_z1 = Mathf.FloorToInt(h_z1 * h2a);
        var a_w = a_x1 - a_x0;
        var a_h = a_z1 - a_z0;
        sb.AppendFormat("a range:{0},{1},{2},{3}, a_w:{4}, a_h:{5}\n", a_x0, a_x1, a_z0, a_z1, a_w, a_h);

        // 大地图中 layers 可以很多(超过4个), 但同一个块内不能超过4个, 因此需要做 layer -> splat_id 的转换
        int next_splat_id = 0;                            // 下一个可用的 splat_id
        int[] layer_to_splat_id = new int[a_layers];        // 保存 layer -> splat_id 映射
        for (int i = 0; i < a_layers; i++) layer_to_splat_id[i] = -1;

        // 遍历 alphamap, 填充 tex, 并分配 splat_id
        var alphas = td.GetAlphamaps(a_x0, a_z0, a_w, a_h);
        var tex = new Texture2D(a_w, a_h);
        float[] rgba = new float[4];
        for (int z = 0; z < a_h; z++)
        {
            for (int x = 0; x < a_w; x++)
            {
                // 计算该位置的颜色值
                rgba[0] = rgba[1] = rgba[2] = rgba[3] = 0;
                for (int layer = 0; layer < a_layers; layer++)
                {
                    var value = alphas[z, x, layer];
                    if (value > 0)
                    {
                        // 获取该 layer 对应的 splat_id
                        var splat_id = layer_to_splat_id[layer];
                        if (splat_id == -1)
                        {
                            // id 不能超过4, 因为每个 terrain_mesh 不能拥有超过4个地表贴图
                            if (next_splat_id >= 4)
                            {
                                Debug.LogError(string.Format("too many layers! block:{0}", block));
                                Selection.activeGameObject = block.gameObject;
                                return 2;
                            }

                            // 新分配 id
                            layer_to_splat_id[layer] = splat_id = next_splat_id++;
                        }

                        // 保存
                        rgba[splat_id] = value;
                    }
                }

                // 保存颜色
                var color = new Color(rgba[0], rgba[1], rgba[2], rgba[3]);
                tex.SetPixel(x, z, color);
            }
        }

        var splat_count = Mathf.Clamp(next_splat_id, 1, 4);
        sb.AppendFormat("splat_count:{0}, layer_to_splat_id:{1}\n", splat_count, string.Join(",", Array.ConvertAll<int, string>(layer_to_splat_id, (n) => n.ToString())));

        // 处理 alpha 贴图
        var alpha_pathname = GetTerrainAssetPathname(td, block, "alpha", ".png");
        if (splat_count <= 1)
        {
            // 删除 alpha 贴图
            if (File.Exists(alpha_pathname))
            {
                File.Delete(alpha_pathname);
                sb.AppendFormat("Delete Alpha Tex, pathname:{0}\n", alpha_pathname);
            }
        }
        else
        {
            // 保存 alpha 贴图
            File.WriteAllBytes(alpha_pathname, tex.EncodeToPNG());
            sb.AppendFormat("Save Alpha Tex, pathname:{0}\n", alpha_pathname);
        }

        // 处理材质球
        var mat_pathname = GetTerrainAssetPathname(td, block, "mat", ".mat");
        if (splat_count == 0)
        {
            // 删除材质球
            if (File.Exists(mat_pathname))
            {
                File.Delete(mat_pathname);
                sb.AppendFormat("Delete Material, pathname:{0}\n", mat_pathname);
            }
        }
        else
        {
            // 根据 splat_id 获取 splat
            Func<int, SplatPrototype> GetSplatById = (splat_id) =>
                {
                    for (int layer = 0; layer < layer_to_splat_id.Length; layer++)
                    {
                        if (layer_to_splat_id[layer] == splat_id)
                        {
                            return td.splatPrototypes[layer];
                        }
                    }
                    return null;
                };

            // 获取该 splat 在 mat 中的 offset 属性
            Func<SplatPrototype, Vector2> GetTextureOffset = (splat) =>
                {
                    var x_offset = (b_x0 % splat.tileSize.x) / splat.tileSize.x;     // (32 % 10) / 10 = 2 / 10 = 0.2, 则偏移 0.2
                    var y_offset = (b_z0 % splat.tileSize.y) / splat.tileSize.y;
                    return new Vector2(x_offset, y_offset);
                };

            // 获取该 splat 在 mat 中的 scale 属性
            Func<SplatPrototype, Vector2> GetTextureScale = (splat) =>
                {
                    var x_times = bm.BlockWidth / splat.tileSize.x;     // 32 / 16 = 2, 则显示2次
                    var y_times = bm.BlockHeight / splat.tileSize.y;
                    return new Vector2(x_times, y_times);
                };

            // 创建材质
            var sdr_name = terrain_shader_names[splat_count - 1];
            var mat = new Material(resource.ShaderManager.Find(sdr_name));
            if (splat_count == 1)   // diffuse
            {
                var splat = GetSplatById(0);

                mat.SetTexture(resource.ShaderNameHash.MainTex, splat.texture);
                mat.SetTextureScale(resource.ShaderNameHash.MainTex, GetTextureScale(splat));
                mat.SetTextureOffset(resource.ShaderNameHash.MainTex, GetTextureOffset(splat));
            }
            else // terrain shader
            {
                AssetDatabase.Refresh();

                // 设置 alpha 贴图
                var alpha_tex = AssetDatabase.LoadAssetAtPath(alpha_pathname, typeof(Texture2D)) as Texture2D;
                mat.SetTexture("_Control", alpha_tex);

                // 设置 splat 贴图
                for (int splat_id = 0; splat_id < splat_count; splat_id++)
                {
                    var splat = GetSplatById(splat_id);
                    var tex_name = "_Splat" + splat_id;

                    mat.SetTexture(tex_name, splat.texture);
                    mat.SetTextureScale(tex_name, GetTextureScale(splat));
                    mat.SetTextureOffset(tex_name, GetTextureOffset(splat));
                }
            }

            // 保存材质球
            AssetDatabase.CreateAsset(mat, mat_pathname);
            sb.AppendFormat("Save Material, pathname:{0}\n", mat_pathname);
        }

        // 创建 prefab
        var prefab_pathname = GetTerrainAssetPathname(td, block, "prefab", ".prefab");
        if (splat_count == 0)
        {
            // 删除 prefab
            if (File.Exists(prefab_pathname))
            {
                File.Delete(prefab_pathname);
                sb.AppendFormat("Delete Prefab, pathname:{0}\n", prefab_pathname);
            }
        }
        else
        {
            AssetDatabase.Refresh();

            // 创建 prefab
            var go = new GameObject();

            var mf = go.AddComponent<MeshFilter>();
            mf.sharedMesh = AssetDatabase.LoadAssetAtPath(mesh_pathname, typeof(Mesh)) as Mesh;

            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = AssetDatabase.LoadAssetAtPath(mat_pathname, typeof(Material)) as Material;

            var mc = go.AddComponent<MeshCollider>();

            // 保存
            if (File.Exists(prefab_pathname))
            {
                var prefab = AssetDatabase.LoadAssetAtPath(prefab_pathname, typeof(GameObject)) as GameObject;
                PrefabUtility.ReplacePrefab(go, prefab, ReplacePrefabOptions.ConnectToPrefab);
            }
            else
            {
                PrefabUtility.CreatePrefab(prefab_pathname, go, ReplacePrefabOptions.ConnectToPrefab);
            }
            GameObject.DestroyImmediate(go);
        }

        // done
        sb.AppendFormat("done!\n");
        Debug.Log(sb.ToString());
        return 1;
    }
    static string[] terrain_shader_names = new string[] {
        "Mobile/Diffuse",
        "Kunlun/Terrain/Pass2",
        "Kunlun/Terrain/Pass3",
        "Kunlun/Terrain/Pass4",
    };

    // 显示地形
    public static void ShowTerrain(MapBlockBehaviour[] blocks)
    {
        var bm = GetBigMap(blocks);
        if (bm == null || bm.terrain == null) return;

        foreach (var block in blocks)
        {
            ShowTerrain(bm, block);
        }
    }
    static void ShowTerrain(BigMapBehaviour bm, MapBlockBehaviour block)
    {
        if (block.is_global) return;

        // 获取名字
        var name = GetBlockTerrainName(block.ix, block.iz);

        // 查找之前
        var t = bm.gameObject.transform.Find(name);
        if (t)
        {
            var mr = t.GetComponent<MeshRenderer>().enabled = true;
            t.localPosition = new Vector3(block.ix * bm.BlockWidth, 0, block.iz * bm.BlockHeight);
            return;
        }

        // 读入 prefab
        var td = bm.terrain.terrainData;
        var prefab_pathname = GetTerrainAssetPathname(td, block, "prefab", ".prefab");
        var prefab = AssetDatabase.LoadAssetAtPath(prefab_pathname, typeof(GameObject));
        if (prefab == null) return;

        // 创建新的
        var go = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
        go.name = name;
        go.transform.parent = bm.transform;
        go.transform.localPosition = new Vector3(block.ix * bm.BlockWidth, 0, block.iz * bm.BlockHeight);
    }

    public static void HideTerrain(MapBlockBehaviour[] blocks)
    {
    }


    // 更新地形
    static void UpdateTerrain(Terrain t, MapBlockBehaviour block)
    {
        AssetDatabase.Refresh();

        //
        var td = t.terrainData;
        //var pathname = GetTerrainAssetPathname(td, block, ");

        block.ClearTerrain();
    }

    //
    public static void ClearTerrain(MapBlockBehaviour[] blocks)
    {
        foreach (var block in blocks)
        {
            block.ClearTerrain();
        }
    }

#endregion

}
