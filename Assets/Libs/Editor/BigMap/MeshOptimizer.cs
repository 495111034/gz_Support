using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// 模型优化
/// </summary>
public class MeshOptimizer
{
    // 三角形
    class Triangle
    {
        public Vertex[] vertexs;        // 顶点数组
        public Vector3 normal;          // 法线


        //
        public Triangle(Vertex v0, Vertex v1, Vertex v2)
        {
            vertexs = new Vertex[] { v0, v1, v2 };

            v0.triangles.Add(this);
            v1.triangles.Add(this);
            v2.triangles.Add(this);

            ComputeNormal();
        }

        // 重新计算法线
        public void ComputeNormal()
        {
            var p0 = vertexs[0].position;
            var p1 = vertexs[1].position;
            var p2 = vertexs[2].position;

            var side1 = p1 - p0;
            var side2 = p2 - p0;

            normal = Vector3.Cross(side1, side2);
        }

        // 是否包含某个顶点
        public bool HasVertex(Vertex v)
        {
            return vertexs[0] == v || vertexs[1] == v || vertexs[2] == v;
        }

        // 替换顶点
        public void ReplaceVertex(Vertex vold, Vertex vnew)
        {
            for (int i = 0; i < vertexs.Length; i++)
            {
                if (vertexs[i] == vold)
                {
                    // 替换顶点
                    vertexs[i] = vnew;

                    // 从顶点中删除
                    vold.triangles.Remove(this);
                    vnew.triangles.Add(this);

                    // 重算法线
                    ComputeNormal();
                }
            }
        }
    }

    // 顶点信息
    class Vertex
    {
        public int id;                      // 原始顶点ID
        public Vector3 position;            // 原始位置

        public List<Vertex> neighbor;       // 邻接顶点, 注意: 邻接关系由外层算法负责维护
        public List<Triangle> triangles;    // 三角形列表

        public bool is_edge;        // 是边界
        public float cost;      // 坍塌代价
        public Vertex collapse; // 该向哪个位置坍塌

        public int index;   // 在最终顶点列表中的索引

        //
        public Vertex(int id, Vector3 pos)
        {
            this.id = id;
            this.position = pos;

            neighbor = new List<Vertex>();
            triangles = new List<Triangle>();
        }

        public override string ToString()
        {
            return string.Format("id:{0}, pos:{1}, tris:{2}, neis:{3}", id, position, triangles.Count, neighbor.Count);
        }

        // 如果没有, 则加入该邻居, 单向
        public void AddNeighbor(Vertex v)
        {
            if (v != this && !neighbor.Contains(v))
            {
                neighbor.Add(v);
            }
        }
        public void RemoveNeighbor(Vertex v)
        {
            neighbor.Remove(v);
        }
    }

    // 原始 mesh
    Mesh _mesh;
    Vector3[] _vertexs;

    // 临时数据
    List<Vertex> _vertex_list;
    List<Triangle> _triangle_list;


    #region 构造信息

    // 构造信息
    void BuildInfo(Mesh mesh)
    {
        _mesh = mesh;
        _vertexs = _mesh.vertices;

        _vertex_list = new List<Vertex>();
        _triangle_list = new List<Triangle>();

        // 添加每个三角形
        var triangles = _mesh.triangles;
        var vertex_map = new Vertex[mesh.vertexCount];

        for (int i = 0; i < triangles.Length; )
        {
            var v0 = FindVertex(vertex_map, triangles[i++]);
            var v1 = FindVertex(vertex_map, triangles[i++]);
            var v2 = FindVertex(vertex_map, triangles[i++]);

            // 空三角形
            if (v0 == v1 || v0 == v2 || v1 == v2)
            {
                Debug.LogError("vertex same!");
                continue;
            }

            // 添加
            var t = new Triangle(v0, v1, v2);
            _triangle_list.Add(t);
        }
    }

    // 查找顶点
    Vertex FindVertex(Vertex[] vertex_map, int id)
    {
        // 从 map 中获取
        var info = vertex_map[id];
        if (info != null) return info;

        Vector3 pos = _vertexs[id];

        // 从列表中查找
        var error = 0.001f;
        var error2 = error * error;
        foreach (var vi in _vertex_list)
        {
            var diff = (vi.position - pos).sqrMagnitude;
            if (diff < error2)
            {
                //Log.LogError("FindVertex, use near, id:{0}, id2:{1}, diff:{2}", id, vi.id, diff);
                vertex_map[id] = vi;
                return vi;
            }
        }

        // 新建
        info = new Vertex(id, pos);

        vertex_map[id] = info;
        _vertex_list.Add(info);

        //
        return info;
    }

    #endregion

    #region 优化

    // 算法参考: http://dev.gameres.com/Program/Visual/3D/PolygonReduction.htm

    // 获取 u-v 边上的三角形
    static List<Triangle> GetTriangleOnEdge(Vertex u, Vertex v)
    {
        var list = new List<Triangle>();
        foreach (var t in u.triangles)
        {
            if (t.HasVertex(v))
            {
                list.Add(t);
            }
        }
        return list;
    }

    // 计算坍塌代价, u->v
    static float ComputeEdgeCollapseCost(Vertex u, Vertex v)
    {
        // 计算 距离
        var edgelength = (u.position - v.position).magnitude;

        // 获得以 uv 为边的所有三角形
        var tris_on_uv = GetTriangleOnEdge(u, v);

        // 计算最大曲率
        var curvature = 0f;
        foreach (var ti in u.triangles)
        {
            //var mincurv = 1f;
            //foreach (var ti2 in tris_on_uv)
            //{
            //    var dotprod = Vector3.Dot(ti.normal, ti2.normal);       // 点积, 同向/逆向时为 [1, -1]
            //    var curv = (1 - dotprod) / 2f;                          // 曲率, 同向/逆向时为 [0, 1]
            //    mincurv = Mathf.Min(mincurv, curv);                     // 取最小曲率?
            //}
            //curvature = Mathf.Max(curvature, mincurv);

            // 改为使用最大值
            foreach (var ti2 in tris_on_uv)
            {
                var dotprod = Vector3.Dot(ti.normal, ti2.normal);       // 点积, 同向/逆向时为 [1, -1]
                var curv = (1 - dotprod) / 2f;                          // 曲率, 同向/逆向时为 [0, 1]
                curvature = Mathf.Max(curvature, curv);                 // 取最大值
            }
        }

        // 返回代价
        return edgelength * curvature;
    }

    // 计算 v 应该向哪个位置坍塌
    static void ComputeEdgeCostAtVertex(Vertex v)
    {
        // 如果没有邻居, 则是孤立点, 随时可以被删除
        if (v.neighbor.Count == 0)
        {
            v.collapse = null;
            v.cost = -0.01f;
            return;
        }

        //
        v.cost = float.MaxValue;
        v.collapse = null;

        if (v.is_edge) return;

        // 搜索邻居, 查找最小的代价
        foreach (var v2 in v.neighbor)
        {
            var c = ComputeEdgeCollapseCost(v, v2);
            if (c < v.cost)
            {
                v.collapse = v2;
                v.cost = c;
            }
        }
    }

    // 执行坍塌
    void Collapse(Vertex u, Vertex v)
    {
        // 如果 u 是独立的, 则删除 u
        if (v == null)
        {
            RemoveVertex(u);
            return;
        }

        // 保存 u 的所有邻居
        var tmp = new List<Vertex>();
        tmp.AddRange(u.neighbor);

        // 删除 uv 为边的三角形
        var tri_on_uv = GetTriangleOnEdge(u, v);
        foreach (var t in tri_on_uv)
        {
            RemoveTriangle(t);
        }

        // 把 u 中的三角形, 替换到 v 上
        foreach (var tri in u.triangles.ToArray())
        {
            tri.ReplaceVertex(u, v);
        }

        // 更新邻接关系
        u.neighbor.Clear();
        foreach (var t in tmp)
        {
            // 删除邻居 u
            t.RemoveNeighbor(u);

            // t, v 互相加入
            t.AddNeighbor(v);
            v.AddNeighbor(t);
        }

        // 删除 u
        RemoveVertex(u);

        // 重新计算邻居节点的坍塌
        foreach (var t in tmp)
        {
            ComputeEdgeCostAtVertex(t);
        }
    }

    // 获取最小代价的节点
    Vertex GetMinimumCostEdge()
    {
        if (_vertex_list.Count > 0)
        {
            SortVertexList();
            return _vertex_list[0];
        }
        return null;
    }
    void SortVertexList()
    {
        _vertex_list.Sort(CompareCost);
    }
    static int CompareCost(Vertex v1, Vertex v2)
    {
        var diff = v1.cost - v2.cost;
        return diff < 0 ? -1 : diff > 0 ? 1 : 0;
    }

    // 删除三角形
    void RemoveTriangle(Triangle ti)
    {
        // 从顶点中删除三角形
        foreach (var vi in ti.vertexs)
        {
            vi.triangles.Remove(ti);
        }

        // 从三角形中删除顶点
        ti.vertexs = null;

        // 从列表中删除
        _triangle_list.Remove(ti);
    }

    // 删除节点
    void RemoveVertex(Vertex v)
    {
        // 顶点必须是空的
        if (v.triangles.Count > 0 || v.neighbor.Count > 0)
        {
//            Log.LogInfo("Cant remove vertex! tri:{0}, nei:{1}", v.triangles.Count, v.neighbor.Count);
            return;
        }

        // 从列表删除
        _vertex_list.Remove(v);
    }

    // 判断 V 是否是边顶点
    static bool IsVertexEdge(Vertex v)
    {
        return v.triangles.Count != v.neighbor.Count;
    }

    // 执行优化
    void Optimize()
    {
        // 构造邻接
        foreach (var v in _vertex_list)
        {
            foreach (var t in v.triangles)
            {
                foreach (var v2 in t.vertexs)
                {
                    v.AddNeighbor(v2);
                }
            }
        }

        // 识别边界
        foreach (var v in _vertex_list)
        {
            v.is_edge = IsVertexEdge(v);
        }

        // 计算坍塌
        foreach (var v in _vertex_list)
        {
            ComputeEdgeCostAtVertex(v);
        }

        //SortVertexList();
        //for (int i = 0; i < _vertex_list.Count / 2; i++)
        //{
        //    var v = _vertex_list[i];
        //    if (v.is_edge) break;
        //    Collapse(v, v.collapse);
        //}

        // 如果未满足目标, 则持续优化
        var dst_count = _triangle_list.Count / 2;
        while (_triangle_list.Count > dst_count)
        {
            // 获取最小代价
            var v = GetMinimumCostEdge();
            if (v == null) break;
            if (v.is_edge) break;

            // 执行坍塌
            Collapse(v, v.collapse);
        }
    }

    #endregion

    // 构造模型
    Mesh BuildMesh()
    {
        // 更新顶点索引
        var vcount = _vertex_list.Count;
        var index2id = new int[vcount];
        for (int i = 0; i < vcount; i++)
        {
            var v = _vertex_list[i];
            index2id[i] = v.id;
            v.index = i;
        }

        var mesh2 = new Mesh();
        mesh2.name = "Optimize Mesh";

        var vtx = new Vector3[vcount];
        for (int i = 0; i < vcount; i++)
        {
            vtx[i] = _vertex_list[i].position;
        }
        mesh2.vertices = vtx;

        var tcount = _triangle_list.Count;
        var tris = new int[tcount * 3];
        var tid = 0;
        for (int i = 0; i < tcount; i++)
        {
            var ti = _triangle_list[i];
            tris[tid++] = ti.vertexs[0].index;
            tris[tid++] = ti.vertexs[1].index;
            tris[tid++] = ti.vertexs[2].index;
        }
        mesh2.triangles = tris;

        var color = _mesh.colors;
        var color2 = color;
        if (color != null && color.Length > 0)
        {
            color2 = new Color[vcount];
            for (int i = 0; i < vcount; i++) color2[i] = color[index2id[i]];
            mesh2.colors = color2;
        }

        var uv = _mesh.uv;       
        var uv2 = uv;
        if (uv != null && uv.Length > 0)
        {
            uv2 = new Vector2[vcount];
            for (int i = 0; i < vcount; i++) uv2[i] = uv[index2id[i]];
            mesh2.uv = uv2;
        }

        uv = _mesh.uv2;
        if (uv != null && uv.Length > 0)
        {
            uv2 = new Vector2[vcount];
            for (int i = 0; i < vcount; i++) uv2[i] = uv[index2id[i]];
            mesh2.uv2 = uv2;
        }

        uv = _mesh.uv3;
        if (uv != null && uv.Length > 0)
        {
            uv2 = new Vector2[vcount];
            for (int i = 0; i < vcount; i++) uv2[i] = uv[index2id[i]];
            mesh2.uv3 = uv2;
        }

        var nor = _mesh.normals;
        var nor2 = nor;
        if (nor != null && nor.Length > 0)
        {
            nor2 = new Vector3[vcount];
            for (int i = 0; i < vcount; i++) nor2[i] = nor[index2id[i]];
            mesh2.normals = nor2;
        }

        var bs = _mesh.boneWeights;       
        if(bs != null && bs.Length > 0)
        {
            var bs2 = new BoneWeight[vcount];
            for (int i = 0; i < vcount; i++)
            {
                bs2[i].weight0 = bs[index2id[i]].weight0;
                bs2[i].weight1 = bs[index2id[i]].weight1;
                bs2[i].weight2 = bs[index2id[i]].weight2;
                bs2[i].weight3 = bs[index2id[i]].weight3;
                bs2[i].boneIndex0 = bs[index2id[i]].boneIndex0;
                bs2[i].boneIndex1 = bs[index2id[i]].boneIndex1;
                bs2[i].boneIndex2 = bs[index2id[i]].boneIndex2;
                bs2[i].boneIndex3 = bs[index2id[i]].boneIndex3;

                //Log.LogInfo($"boneWeights[{i}]=weight0:{bs2[i].weight0},weight1:{bs2[i].weight1},weight2:{bs2[i].weight2},weight3:{bs2[i].weight3},boneIndex0={bs2[i].boneIndex0},boneIndex1={bs2[i].boneIndex1},boneIndex2={bs2[i].boneIndex2},boneIndex3={bs2[i].boneIndex3}");
            }
            mesh2.boneWeights = bs2;
        }

        mesh2.bindposes = _mesh.bindposes;
        //if(bindposes != null && bindposes.Length > 0)
        //{
        //    var bp2 = new Matrix4x4[vcount];
        //    for (int i = 0; i < vcount; i++)
        //    {
        //        bp2[i] = bindposes[index2id[i]];
        //    }
        //    mesh2.bindposes = bp2;
        //}

        //
        return mesh2;
    }

    // 优化模型
    public Mesh OptimizeMesh(Mesh mesh)
    {
        BuildInfo(mesh);
        Optimize();
        var mesh2 = BuildMesh();
       // Log.LogInfo("OptimizeMesh, vertexs:{0}->{1}, tris:{2}->{3}", mesh.vertexCount, mesh2.vertexCount, mesh.triangles.Length / 3, mesh2.triangles.Length / 3);
        return mesh2;
    }


    // 模型减面
    public static void OptimizeMesh(GameObject go)
    {
        Mesh mesh = null;
        var mf = go.GetComponentInChildren<MeshFilter>();
        if (mf)
        {
            mesh = mf.sharedMesh;
        }

        if(!mesh)
        {
            var smr = go.GetComponentEx<SkinnedMeshRenderer>();
            if(smr)
            {
                mesh = smr.sharedMesh;
            }
        }
        if (!mesh)
        {
            Log.LogError($"当前物体{go.name}没有网格");
            return;
        }
       
        var mesh2 = new MeshOptimizer().OptimizeMesh(mesh);
        var oldFull = UnityEditor.AssetDatabase.GetAssetPath(mesh);
        var dstFileName = System.IO.Path.GetFileNameWithoutExtension(oldFull);
        string destPath = System.IO.Path.GetDirectoryName(oldFull);
        int quality_level = 1;
        if (dstFileName.Contains("--q"))
        {
            quality_level = int.Parse(dstFileName.Substring(dstFileName.IndexOf("--q"), 1));
            dstFileName = dstFileName.Remove(dstFileName.IndexOf("--q"), 4);
        }

        if (quality_level == 0) return;
        var low_mesh = $"{destPath.Replace('\\', '/')}/{dstFileName}--q0.asset";
        Log.LogInfo($"生成低模：{low_mesh}");

        UnityEditor.AssetDatabase.CreateAsset(mesh2, low_mesh);
        UnityEditor.AssetDatabase.SaveAssets();
        UnityEditor.AssetDatabase.Refresh();
        //mf.sharedMesh = UnityEditor.AssetDatabase.LoadAssetAtPath<Mesh>($"{PathDefs.ASSETS_PATH_TERRAIN_MESH}{go.name}.asset");
    }

    //
}

public static class MeshAssetUtils
{
    //[MenuItem("Assets/生成低模")]
    public static void OptimizeMesh()
    {
        Object[] objs = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
        foreach (var obj in objs)
        {
            Mesh mesh = null;
            if(obj is Mesh)
            {
                mesh = obj as Mesh;
            }
            else if(obj is GameObject)
            {
                var go = obj as GameObject;
                var mf = go.GetComponentEx<MeshFilter>();
                if (mf)
                {
                    mesh = mf.sharedMesh;
                }

                if (!mesh)
                {
                    var smr = go.GetComponentEx<SkinnedMeshRenderer>();
                    if (smr)
                    {
                        mesh = smr.sharedMesh;
                    }
                }
            }

            if (mesh)
            {
                var mesh2 = new MeshOptimizer().OptimizeMesh(mesh);
               // var mesh2 = new MeshOptimizer().OptimizeMesh(mesh_tmp);
                var oldFull = UnityEditor.AssetDatabase.GetAssetPath(mesh);
                var dstFileName = System.IO.Path.GetFileNameWithoutExtension(oldFull);
                string destPath = System.IO.Path.GetDirectoryName(oldFull);
                int quality_level = 1;
                if (dstFileName.Contains("--q"))
                {
                    quality_level = int.Parse(dstFileName.Substring(dstFileName.IndexOf("--q"), 1));
                    dstFileName = dstFileName.Remove(dstFileName.IndexOf("--q"), 4);
                }

                if (quality_level == 0) return;
                var low_mesh = $"{destPath.Replace('\\', '/')}/{dstFileName}--q0.asset";
                Log.LogInfo($"生成低模：{low_mesh}");

                UnityEditor.AssetDatabase.CreateAsset(mesh2, low_mesh);
                UnityEditor.AssetDatabase.SaveAssets();
            }
        }
        UnityEditor.AssetDatabase.Refresh();
    }
}
