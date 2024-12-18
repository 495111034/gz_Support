using SimpleGrass;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class ProjectorShadow : MonoBehaviour
{
    //public float ProjectorSize => Mathf.Min(mProjectorSize, (parentCameraControl? parentCameraControl.distance : 0) + 4);

    public float mProjectorSize = 23;

    public int mRenderTexSize = 1024;

    public bool SetShadowMatrix = false;

    private int mShadowMatrixID;

    private int mObjectShadowMapID;

    private bool mInitCommandBuffer = false;

    private Projector mProjector;

    private Camera mShadowCam;

    private RenderTexture mShadowRT;
    private RenderTexture mColorRT;

    private CommandBuffer[] mCommandBufs;

    private Material mReplaceMat; // ShadowCaster

    //public static GameObject RoleGameobjectRoot;

    private LockViewCameraController parentCameraControl;

    private SimpleInstancingMgr grassMgr;

    private Matrix4x4 _worldToProjectorMatrix;

    public Matrix4x4 WorldToProjectorMatrix
    {
        get
        {
            return _worldToProjectorMatrix;
        }
    }

    public RenderTexture ShadowRT { get { return mShadowRT; } }

    //private GameObject sampleShadow;
    //private Mesh sampleShadowMesh;
    //private Material sampleShadowMat;//SampleShadowCaster

    //protected static Texture shadowTexture;

    [System.NonSerialized]
    public float AddProjectorSize = 0;

    #region 内置函数

    void Start()
    {
        matrix = new MyBetterList<Matrix4x4>(10);
        tmp = new MaterialPropertyBlock();
        mShadowMatrixID = Shader.PropertyToID("_ShadowMatrix");
        mObjectShadowMapID = Shader.PropertyToID("_ObjectShadowMap");
        ShadowOffset = Vector3.zero;
    }

    void InitSimpleGrassInstancingMgr()
    {
        Scene scene = SceneManager.GetActiveScene();
        grassMgr = GetGrassMgr(scene);
        if (grassMgr != null)
        {
            grassMgr.shadowProjector = this;
        }
        SceneManager.activeSceneChanged -= OnSceneChanged;
        SceneManager.activeSceneChanged += OnSceneChanged;
    }
    private void OnSceneChanged(Scene oldScene, Scene newScnen)
    {
        grassMgr = GetGrassMgr(newScnen);
        if (grassMgr != null)
        {
            grassMgr.shadowProjector = this;
            grassMgr.shadowIntensity = mProjector.material.GetFloat("_Intensity");
        }
    }

    private SimpleInstancingMgr GetGrassMgr(Scene scene)
    {
        SimpleInstancingMgr simpleInstancingMgr = null;
        foreach (var root in scene.GetRootGameObjects())
        {
            simpleInstancingMgr = root.GetComponentInChildren<SimpleInstancingMgr>(true);
            if (simpleInstancingMgr != null)
            {
                break;
            }
        }
        return simpleInstancingMgr;
    }

    //private float _last_udpate_time;
    void LateUpdate ()
    {
        if (!enabled) 
        {
            //Log.LogError($"!enabled");
            return; 
        }

        //if (!RoleGameobjectRoot) return;
        if (Graphics.activeTier != GraphicsTier.Tier3) 
        {
            //Log.LogError($"!Tier3");
            return; 
        }

        if (!mInitCommandBuffer)
        {
            Init();
            //Log.LogError($"!mInitCommandBuffer");
            return;
        }

        if (!parentCameraControl)
        {
            parentCameraControl = GameObject.FindObjectOfType<LockViewCameraController>();
        }

        //if(parentCameraControl)
        //{
        //    var mProjectorSize = Mathf.Min(parentCameraControl.real_distance + 3, 30) + AddProjectorSize;
        //    if (mProjectorSize != this.mProjectorSize)
        //    {
        //        this.mProjectorSize = mProjectorSize;
        //        mProjector.orthographicSize = mShadowCam.orthographicSize = mProjectorSize;
        //        mProjector.farClipPlane = mShadowCam.farClipPlane = mProjectorSize * (2 + (AddProjectorSize > 0 ? 1 : 0));
        //        //mProjector.farClipPlane = mProjectorSize + 4;
        //        //mShadowCam.fieldOfView = mProjectorSize * 2;
        //    }
        //}

        if (SetShadowMatrix)
        {
            Shader.SetGlobalMatrix(mShadowMatrixID, _worldToProjectorMatrix);
            Shader.SetGlobalTexture(mObjectShadowMapID, ShadowRT);
        }

        // 填充Commander Buffer
        //if (is_dirty)
        {
            //is_dirty = false;
            _worldToProjectorMatrix = mShadowCam.projectionMatrix * mShadowCam.worldToCameraMatrix;
            FillCommandBuffer();
        }
    }

    private void OnDisable()
    {
        //clearData();
        if(mProjector)
        {
            //mProjector.enabled = false;
        }
    }

    private void OnEnable()
    {
        if (mProjector)
        {
            mProjector.enabled = true;
        }
    }

    private void OnDestroy()
    {
        clearData();
    }



    #endregion

    #region 函数

    static int ShadowRtCount = 0;
    bool Init()
    {
        if (Graphics.activeTier != GraphicsTier.Tier3) return false;

        if (mInitCommandBuffer) return true;        

        UnityEngine.Profiling.Profiler.BeginSample("Projector Shadows Init");
        {
            gameObject.SetLayerRecursively((int)ObjLayer.Shadow);
            // 创建render texture
            if (mShadowRT)
            {
                RenderTexture.ReleaseTemporary(mShadowRT);
                mShadowRT = null;
            }
            if (!mShadowRT)
            {
                mColorRT = RenderTexture.GetTemporary(mRenderTexSize, mRenderTexSize, 0, RenderTextureFormat.R8);
                mShadowRT = RenderTexture.GetTemporary(mRenderTexSize, mRenderTexSize, 16, RenderTextureFormat.Depth);
                mShadowRT.name = $"ShadowRT {++ShadowRtCount}";
                mShadowRT.antiAliasing = 1;
                mShadowRT.filterMode = FilterMode.Bilinear;
                mShadowRT.wrapMode = TextureWrapMode.Clamp;
            }

            //projector初始化
            if (!mProjector)
            {
                mProjector = GetComponent<Projector>();
            }
            if (mProjector)
            {
                mProjector.orthographic = true;
                //mProjector.nearClipPlane = 0;
                mProjector.farClipPlane = mProjector.orthographicSize = mProjectorSize;
                mProjector.ignoreLayers = ~((int)ObjLayerMask.Terrain);
                //
                //mProjector.material.enableInstancing = true;
                //mProjector.material.renderQueue = 2000; 
                mProjector.material.SetTexture("_ShadowTex", mShadowRT);
                //if (shadowTexture)
                //{
                //    mProjector.material.SetTexture("_FalloffTex", shadowTexture);
                //}
            }
            //camera初始化
            mShadowCam = gameObject.GetComponentInChildren<Camera>();
            if (!mShadowCam)
            {
                var go = new GameObject("camera");
                go.transform.parent = transform;
                go.transform.localEulerAngles = Vector3.zero;
                mShadowCam = go.AddComponent<Camera>();
            }
            if (mShadowCam)
            {
                mShadowCam.clearFlags = CameraClearFlags.Color;
                mShadowCam.backgroundColor = Color.white;
                mShadowCam.orthographic = true;
                mShadowCam.orthographicSize = mProjectorSize;
                mShadowCam.depth = 0;
                mShadowCam.nearClipPlane = mProjector.nearClipPlane;
                mShadowCam.farClipPlane = mProjector.farClipPlane;
                mShadowCam.allowHDR = false;
                mShadowCam.allowMSAA = false;
                mShadowCam.allowDynamicResolution = false;
                mShadowCam.useOcclusionCulling = false;
                mShadowCam.cullingMask = 0;
                mShadowCam.RemoveAllCommandBuffers();

                //mShadowCam.targetTexture = mShadowRT;
                mShadowCam.SetTargetBuffers(mColorRT.colorBuffer, mShadowRT.depthBuffer);
                mShadowCam.enabled = true;
            }

            if (mCommandBufs == null)
            {
                mCommandBufs = new CommandBuffer[2];
                mCommandBufs[0] = CommandBufferPool.Get("skin阴影绘制");
                mCommandBufs[1] = CommandBufferPool.Get("mesh阴影绘制");
            }

            mShadowCam.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, mCommandBufs[0]);
            mShadowCam.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, mCommandBufs[1]);

            if (!mReplaceMat)
            {
                mReplaceMat = Resources.Load<Material>("small/replaceShadow");
                Log.Log2File($"mReplaceMat={mReplaceMat.shader}");
            }
            mInitCommandBuffer = true;
        }
        UnityEngine.Profiling.Profiler.EndSample();

        InitSimpleGrassInstancingMgr();
        return true;
    }
    MyBetterList<Matrix4x4> matrix;
    MaterialPropertyBlock tmp;

    //public Quaternion qt = Quaternion.identity;
    //[System.NonSerialized]
    public Vector3 ShadowOffset;

    private void FillCommandBuffer()
    {
        //if (!sampleShadow) return;
        if (mCommandBufs == null) return;

        UnityEngine.Profiling.Profiler.BeginSample("Projector Shadows Drawcalls");
        if (BuilderConfig.SupportsInstance)
        {
            if(is_dirty || AddProjectorSize == 0)
            {
                var mCommandBuf2 = this.mCommandBufs[1];
                mCommandBuf2.Clear();
                var objList2Mesh = this.objList2Mesh;
                var objList2Transforms = this.objList2Transforms;
                //var sampleShadowMesh = this.sampleShadowMesh;
                var matrix = this.matrix;

                //var sampleShadowMat = this.sampleShadowMat;
                var mReplaceMat = this.mReplaceMat;
                var tmp = this.tmp;

                var qt = Quaternion.identity;
                var eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
                var xscale = 0.975f + Mathf.Sin(Time.time * 2) * 0.025f;//模拟呼吸效果 (Time.time * 频率) * 幅度
                //显示简单阴影       
                for (int n = 0, objList2Mesh_size = objList2Mesh.size; n < objList2Mesh_size; ++n)
                {
                    var k = objList2Mesh[n];
                    if (!k) 
                    {
                        continue;
                    }
                    var v = objList2Transforms[n];
                    int Number = 0;
                    for (int i = 0, v_Count = v.Count; i < v_Count; ++i)
                    {
                        var t = v[i];
                        if (t)
                        {
                            {
                                var scale = xscale;
                                var layer = t.gameObject.layer;
                                if (layer == 0 || layer == 31 || layer == 18)
                                {
                                    scale = 1;
                                    qt = t.rotation;
                                }
                                else
                                {
                                    eulerAngles.z = - t.eulerAngles.y - 90;
                                    qt.eulerAngles = eulerAngles;
                                }
                                qt.eulerAngles += ShadowOffset;

                                if (matrix.size <= Number)
                                    matrix.Add(Matrix4x4.TRS(t.position, qt, t.lossyScale * scale));
                                else
                                    matrix[Number] = Matrix4x4.TRS(t.position, qt, t.lossyScale * scale);
                            }
                            Number++;
                        }
                    }

                    //Number = Mathf.Min(Number, matrix.size);
                    if (Number > 0)// && matrix.size >= Number)
                    {
                        var mat = mReplaceMat;
                        //if (k && mat)
                        {
                            if (mat.enableInstancing)//TODO
                            {
                                for (var idx = 0; idx < k.subMeshCount; ++idx)
                                {
                                    mCommandBuf2.DrawMeshInstanced(k, idx, mat, -1, matrix.buffer, Number, tmp);//,tmp
                                }
                                //Graphics.DrawMeshInstanced(k, 0, mat, matrix.buffer, Number, tmp, ShadowCastingMode.On, true, 0, mShadowCam);
                            }
                            else
                            {
                                var buffer = matrix.buffer;
                                for (var i = 0; i < Number; ++i)
                                {
                                    for (var idx = 0; idx < k.subMeshCount; ++idx)
                                    {
                                        mCommandBuf2.DrawMesh(k, buffer[i], mat, idx);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            //
            if(is_dirty)
            {
                var mCommandBuf1 = this.mCommandBufs[0];
                mCommandBuf1.Clear();

                is_dirty = false;
                var objList1 = this.objList1;
                var m = MyListPool<Material>.Get();
                for (int n = 0, objList1_size = objList1.size; n < objList1_size; ++n) 
                {
                    var rd = objList1[n];
                    if (rd)
                    {
                        rd.GetSharedMaterials(m);
                        for (int i = 0, m_size = m.Count; i < m_size; ++i)
                        {
                            if (m[i])
                            {
                                mCommandBuf1.DrawRenderer(rd, m[i], i);
                            }
                        }
                    }
                }
                //objList1.Clear();
                MyListPool<Material>.Release(m);
            }
        }
        UnityEngine.Profiling.Profiler.EndSample();

    }

    public void clearData()
    {
        if (mShadowCam)
        {
            mShadowCam.RemoveAllCommandBuffers();
            mShadowCam.targetTexture = null;
            mShadowCam.enabled = false;
        }

        //if(mProjector && mProjector.material)
        //    mProjector.material.SetTexture("_ShadowTex", null);

        if (mShadowRT)
        {
            if (mProjector && mProjector.material && mProjector.material.GetTexture("_ShadowTex") == mShadowRT)
                mProjector.material.SetTexture("_ShadowTex", null);

            RenderTexture.ReleaseTemporary(mShadowRT);
        }       
        mShadowRT = null;
        if (mColorRT)
        {
            RenderTexture.ReleaseTemporary(mColorRT);
        }
        mColorRT = null;

        if (mCommandBufs != null)
        {
            mCommandBufs[0].Clear();
            //mCommandBufs[0].Dispose();
            CommandBufferPool.Release(mCommandBufs[0]);

            mCommandBufs[1].Clear();
            CommandBufferPool.Release(mCommandBufs[1]);
        }
        mCommandBufs = null;

        if (matrix != null)
            matrix.Release();

        if (mReplaceMat)
        {
            //GameObject.Destroy(mReplaceMat);
            //Resources.UnloadAsset(mReplaceMat);
        }
        mReplaceMat = null;

        mInitCommandBuffer = false;

        if(objList2Transforms != null)
        {
            for(int i = 0; i < objList2Transforms.Count; ++i)
            {
                MyListPool<Transform>.Release(objList2Transforms[i]);
            }
            objList2Transforms.Clear();
        }
        objList2Mesh.Clear();
    }

    //需要显示完整阴影的角色
    //MyBetterList<GPUSkinningPlayerMono> objList1 = new MyBetterList<GPUSkinningPlayerMono>(5);
    MyBetterList<Renderer> objList1 = new MyBetterList<Renderer>(5);

    //主角的武器，和其它角色的简单阴影 
    MyBetterList<Mesh> objList2Mesh = new MyBetterList<Mesh>(10);
    List<List<Transform>> objList2Transforms = new List<List<Transform>>(10);
    bool is_dirty = false;

    public List<Renderer> FetCacheRendererList<T>(T obj) where T : MonoBehaviour, ISceneObject
    {
        var renderList = obj.CacheRendererList;
        if (renderList == null || renderList.Count == 0)
        {
            if (renderList == null)
            {
                renderList = obj.CacheRendererList = new List<Renderer>();
            }
            UnityEngine.Profiling.Profiler.BeginSample("GetComponentsInChildren<Renderer>");
            //有LOD就只渲染父对象的网格
            if (obj.gameObject.TryGetComponent<LODGroup>(out LODGroup temp))
            {
                renderList.AddRange(temp.GetLODs()[0].renderers);
            }
            else
            {
                obj.gameObject.GetComponentsInChildren(renderList);
            }
            UnityEngine.Profiling.Profiler.EndSample();
        }
        return renderList;
    }

    /// <summary>
    /// 更新需要显示阴影的角色列表，建议低频更新
    /// <param name="realShadows">需要显示实时阴影的角色</param>
    /// <param name="sampleShadows">需要显示简单阴影的角色</param>
    /// </summary>
    public void UpdateRoleList<T>(List<T> realShadows) where T : MonoBehaviour, ISceneObject
    {
        if (!enabled)
        {
            return;
        }

        //Log.LogInfo("UpdateRoleList");
        is_dirty = true;

        var objList1 = this.objList1;
        var objList2Mesh = this.objList2Mesh;
        var objList2Transforms = this.objList2Transforms;
        //var sampleShadowMesh = this.sampleShadowMesh;
        var is_scene = typeof(T) == typeof(SceneObjectShadow);

        objList1.Clear();
        objList2Mesh.Clear();
        for (int i = 0, objList2Transforms_Count = objList2Transforms.Count; i < objList2Transforms_Count; ++i)
        {
            MyListPool<Transform>.Release(objList2Transforms[i]);
        }
        objList2Transforms.Clear();

        UnityEngine.Profiling.Profiler.BeginSample("Projector Shadows UpdateRoleList");
        for (int j = 0, realShadows_Count = realShadows.Count; j < realShadows_Count; ++j)
        {
            var obj = realShadows[j];
            if (is_scene) 
            {
                if (!(obj as SceneObjectShadow).IsLoaded) 
                {
                    continue;
                }
            }
            var renderList = FetCacheRendererList(obj);
            for (int i = 0; i < renderList.Count; ++i)
            {
                var rd = renderList[i];
                if (!rd || rd.shadowCastingMode == ShadowCastingMode.Off) 
                    continue;

                var gameObject = rd.gameObject;
                if (gameObject.layer == (int)ObjLayer.RoleEffect) 
                    continue;

                if (true)
                {
                    if (BuilderConfig.SupportsInstance)
                    {
                        Mesh sharedMesh = null;
                        if (obj.iHightQualityShow)
                        {
                            objList1.Add(rd);
                        }
                        else
                        {
                            var skin = rd as SkinnedMeshRenderer;
                            if (skin)
                            {
                                sharedMesh = skin.sharedMesh;
                            }
                            else
                            {
                                UnityEngine.Profiling.Profiler.BeginSample("GetComponent<MeshFilter>");
                                var mf = gameObject.GetComponent<MeshFilter>();
                                UnityEngine.Profiling.Profiler.EndSample();
                                if (mf)
                                {
                                    sharedMesh = mf.sharedMesh;
                                }
                            }
                        }

                        //
                        if (sharedMesh)
                        {
                            List<Transform> l = null;
                            var mindx = objList2Mesh.IndexOf(sharedMesh);                            
                            if (mindx >= 0)
                            {
                                l = objList2Transforms[mindx];
                                //objList2Transforms[mindx].Add(m.gameObject.transform);
                            }
                            else
                            {
                                objList2Mesh.Add(sharedMesh);
                                l = MyListPool<Transform>.Get();
                                objList2Transforms.Add(l);
                                //objList2Transforms.Add(new List<Transform>() { m.gameObject.transform });
                            }

                            if (l != null)
                            {
                                l.Add(gameObject.transform);
                            }
                        }
                    }
                }
            }          
        }
        //
        //Log.Log2File($"realShadows={realShadows.Count}, objList1={objList1.size}, objList2Mesh={objList2Mesh.size}, objList2Transforms={objList2Transforms.Count}");
        UnityEngine.Profiling.Profiler.EndSample();
    }


    Vector3 _lastpos3;
    Plane[] _planes6 = new Plane[6];
    //List<Renderer> _renders = new List<Renderer>();
    public bool ObjectVisible(SceneObjectShadow obj)
    {
        if (_lastpos3 != mShadowCam.transform.position)
        {
            UnityEngine.Profiling.Profiler.BeginSample("CalculateFrustumPlanes");
            GeometryUtility.CalculateFrustumPlanes(mShadowCam, _planes6);
            UnityEngine.Profiling.Profiler.EndSample();
            _lastpos3 = mShadowCam.transform.position;
        }

        var _renders = FetCacheRendererList(obj);
        foreach(var render in _renders)
        {
            //GeometryUtility.TestPlanesAABB()方法，该方法可以检测一个AABB包围盒（即物体的边界框）是否在视锥体内，
            //返回一个bool值。如果返回值为true，则表示物体在视锥体内；如果返回值为false，则表示物体不在视锥体内。
            if (GeometryUtility.TestPlanesAABB(_planes6, render.bounds)) 
            {
                return true;
            }
        }
        return false;
    }

    public void SetDis(float dis)
    {
        var mProjectorSize = Mathf.Min(dis + 3, 30) + AddProjectorSize;
        if (mProjectorSize != this.mProjectorSize)
        {
            this.mProjectorSize = mProjectorSize;
            SetSize();
            //mProjector.farClipPlane = mProjectorSize + 4;
            //mShadowCam.fieldOfView = mProjectorSize * 2;
        }
    }

    public void ResetShadow()
    {
        this.enabled = true;
        this.LateUpdate();
        Scene scene = SceneManager.GetActiveScene();
        grassMgr = GetGrassMgr(scene);
        if (grassMgr != null)
        {
            grassMgr.shadowIntensity = mProjector.material.GetFloat("_Intensity");
        }
        SetSize();
    }

    private void SetSize()
    {
        float size = mProjectorSize * (2 + (AddProjectorSize > 0 ? 1 : 0));
        if (mProjector != null)
        {
            mProjector.orthographicSize = mProjectorSize;
            mProjector.farClipPlane = size;
        }
        if (mShadowCam != null)
        {
            mShadowCam.orthographicSize = mProjectorSize;
            mShadowCam.farClipPlane = size;
        }
    }


    #endregion
}
