/// 
/// 网格生成器，将需要喷发的粒子合并成一个大网格，达到手动实现粒子合批提交的目的
/// 每个网格所包含的粒子数最大上限为_maxCopyNumber，在构造函数中传入
/// 另外大网格顶点数最大为64K，以两者先到上限者为准
/// 
using UnityEngine;
namespace MyParticle
{
    public partial class Spray
    {

        [System.Serializable]
        class BulkMesh
        {
            #region 属性

            // 合并后的大网格
            Mesh _mesh;
            public Mesh mesh { get { return _mesh; } }

            // 粒子数量 
            int _copyCount;

            /// <summary>
            /// 粒子数量 
            /// </summary>
            public int copyCount { get { return _copyCount; } }

            #endregion

            #region 内部参数
            /// <summary>
            /// 最大复制数量，每个网格所包含的粒子数最大上限为此数，另外每个网格顶点数最大为64K，以两者先到上限者为准
            /// </summary>
            int _maxCopyNumber = 4096;
            #endregion

            #region 公共方法


            public BulkMesh(Mesh[] shapes,int maxCopy = 4096)
            {
                _maxCopyNumber = maxCopy;
                CombineMeshes(shapes);
            }

            public void Rebuild(Mesh[] shapes, int maxCopy = 4096)
            {
                Release();
                _maxCopyNumber = maxCopy;
                CombineMeshes(shapes);
            }

            public void Release()
            {
                if (_mesh)
                {
                    DestroyImmediate(_mesh);
                    _copyCount = 0;
                }
            }

            #endregion

            #region 内部方法

            // 网格信息缓冲区结构
            //如果网格为空，则定义一个默认的四边形
            struct ShapeCacheData
            {
                Vector3[] vertices;
                Vector3[] normals;
                Vector4[] tangents;
                Vector2[] uv;
                int[] indices;

                public ShapeCacheData(Mesh mesh)
                {
                    if (mesh)
                    {
                        vertices = mesh.vertices;
                        normals  = mesh.normals;
                        tangents = mesh.tangents;
                        uv       = mesh.uv;
                        indices  = mesh.GetIndices(0);
                    }
                    else
                    {
                        // 如果网格为空，则定义一个默认的四边形
                        // 生成默认的顶点、法线、切线和uv信息和索引表
                        vertices = new Vector3[] {
                            new Vector3 (-1, +1, 0), new Vector3 (+1, +1, 0),
                            new Vector3 (-1, -1, 0), new Vector3 (+1, -1, 0),
                            new Vector3 (+1, +1, 0), new Vector3 (-1, +1, 0),
                            new Vector3 (+1, -1, 0), new Vector3 (-1, -1, 0)
                        };
                        normals = new Vector3[] {
                             Vector3.forward,  Vector3.forward,
                             Vector3.forward,  Vector3.forward,
                            -Vector3.forward, -Vector3.forward,
                            -Vector3.forward, -Vector3.forward,
                        };
                        tangents = new Vector4[] {
                            new Vector4( 1, 0, 0, 1), new Vector4( 1, 0, 0, 1),
                            new Vector4( 1, 0, 0, 1), new Vector4( 1, 0, 0, 1),
                            new Vector4(-1, 0, 0, 1), new Vector4(-1, 0, 0, 1),
                            new Vector4(-1, 0, 0, 1), new Vector4(-1, 0, 0, 1)
                        };
                        uv = new Vector2[] {
                            new Vector2(0, 1), new Vector2(1, 1),
                            new Vector2(0, 0), new Vector2(1, 0),
                            new Vector2(1, 1), new Vector2(0, 1),
                            new Vector2(1, 0), new Vector2(0, 0)
                        };
                        indices = new int[] {0, 1, 2, 3, 2, 1, 4, 5, 6, 7, 6, 5};
                    }
                }

                public int VertexCount { get { return vertices.Length; } }
                public int IndexCount { get { return indices.Length; } }

                public void CopyVerticesTo(Vector3[] destination, int position)
                {
                    System.Array.Copy(vertices, 0, destination, position, vertices.Length);
                }

                public void CopyNormalsTo(Vector3[] destination, int position)
                {
                    System.Array.Copy(normals, 0, destination, position, normals.Length);
                }

                public void CopyTangentsTo(Vector4[] destination, int position)
                {
                    System.Array.Copy(tangents, 0, destination, position, tangents.Length);
                }

                public void CopyUVTo(Vector2[] destination, int position)
                {
                    System.Array.Copy(uv, 0, destination, position, uv.Length);
                }

                public void CopyIndicesTo(int[] destination, int position, int offset)
                {
                    for (var i = 0; i < indices.Length; i++)
                        destination[position + i] = offset + indices[i];
                }

                public void CopyColorTo(Color[] colors,int position)
                {
                    for(int i = 0; i < VertexCount; ++i)
                    {
                        var dis = Vector3.Distance(vertices[i], Vector3.zero);
                        colors[position + i] = new Color(dis, 0, 0, 0);
                    }
                }
            }

            // 生成大网格
            void CombineMeshes(Mesh[] shapes)
            {
                ShapeCacheData[] cache;

                if (shapes == null || shapes.Length == 0)
                {
                    // 默认数据
                    cache = new ShapeCacheData[1];
                    cache[0] = new ShapeCacheData(null);
                }
                else
                {
                    // 网格存储到shape缓冲区
                    cache = new ShapeCacheData[shapes.Length];
                    for (var i = 0; i < shapes.Length; i++)
                        cache[i] = new ShapeCacheData(shapes[i]);
                }

                // 得到缓冲区的顶点数和索引数
                var vc_shapes = 0;
                var ic_shapes = 0;
                foreach (var s in cache) {
                    vc_shapes += s.VertexCount;
                    ic_shapes += s.IndexCount;
                }

                
                if (vc_shapes == 0) return;

                // 确定本实例中的粒子数量
                // - 顶点数不超过65535
                // - 粒子数量上限为_maxCopyNumber
                // - 以两者谁先达到为准
                var vc = 0;
                var ic = 0;
                for (_copyCount = 0; _copyCount < _maxCopyNumber; _copyCount++)
                {
                    var s = cache[_copyCount % cache.Length];
                    if (vc + s.VertexCount > 65535) break;
                    vc += s.VertexCount;
                    ic += s.IndexCount;
                }

                // 顶点数组
                var vertices = new Vector3[vc];
                //var colors = new Color[vc];
                var normals  = new Vector3[vc];
                var tangents = new Vector4[vc];
                var uv       = new Vector2[vc];
                var uv2      = new Vector2[vc];
                var indicies = new int[ic];

                for (int v_i = 0, i_i = 0, e_i = 0; v_i < vc; e_i++)
                {
                    var s = cache[e_i % cache.Length];

                    s.CopyVerticesTo(vertices, v_i);
                    s.CopyNormalsTo (normals,  v_i);
                    s.CopyTangentsTo(tangents, v_i);
                    s.CopyUVTo      (uv,       v_i);
                    s.CopyIndicesTo (indicies, i_i, v_i);
                    //s.CopyColorTo(colors, v_i);


                    var coord = new Vector2((float)e_i / _copyCount, 0);
                    for (var i = 0; i < s.VertexCount; i++) uv2[v_i + i] = coord;

                    v_i += s.VertexCount;
                    i_i += s.IndexCount;
                }

                // 生成模型网格
                _mesh = new Mesh();

                _mesh.vertices = vertices;
                
                _mesh.normals  = normals;
                _mesh.tangents = tangents;
                _mesh.uv       = uv;
                _mesh.uv2      = uv2;

                _mesh.SetIndices(indicies, MeshTopology.Triangles, 0);
                ;

                // 临时使用，所以选DontSave
                _mesh.hideFlags = HideFlags.DontSave;

                // 生成包围区信息，避免被裁剪
                _mesh.bounds = new Bounds(Vector3.zero, Vector3.one * 100);
            }

            #endregion
        }
    }
}
