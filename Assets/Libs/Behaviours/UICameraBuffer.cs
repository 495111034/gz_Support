using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

//绘制UI特效部分
public class UICameraBuffer : MonoBehaviour
{
    private Camera _camera;
    private CommandBuffer mCommandBuf;

    MyBetterList<GameObject> uiEffects = new MyBetterList<GameObject>(5);

    MyBetterList<Renderer> effectOtherRenders = new MyBetterList<Renderer>(5);
    MyBetterList<Mesh> effectMeshs = new MyBetterList<Mesh>(5);
    MyBetterList<Material> effectMeshMats = new MyBetterList<Material>(5);
    MyBetterList<List<Transform>> effectMeshTransforms = new MyBetterList<List<Transform>>(5);

    string cameraName = "UICameraBuffer";
    string cameraName2 = "UICameraBufferCheck";
    public float update_interval = 0.1f;
    public float render_interval = 0.5f;
    
    public void AddEffect(GameObject obj)
    {
        if (!obj) return;
        var idx = uiEffects.IndexOf(obj);
        if (idx < 0)
        {
            uiEffects.Add(obj);
            __lastCheckRenders = 0;
        }
    }

    public void RemoveEffect(GameObject obj)
    {
        var idx = uiEffects.IndexOf(obj);
        if (idx >= 0)
        {
            uiEffects.RemoveAt(idx);
            __lastCheckRenders = 0;
        }
    }

    void Start()
    {
       
    }


    private float _last_udpate_time;
    MaterialPropertyBlock tmp;
    void Update()
    {
        if (Time.realtimeSinceStartup - _last_udpate_time >= update_interval)
        {
            _last_udpate_time = Time.realtimeSinceStartup;
        }
        else
        {
            return;
        }

        if (!_camera)
        {
            _camera = gameObject.GetComponent<Camera>();
            cameraName = $"UICameraBuffer_{gameObject.name}";
            cameraName2 = $"UICameraBufferCheck_{gameObject.name}";
        }

        if (!_camera)
            return;

        if (uiEffects.size == 0)
        {
            ClearData();
            return;
        }  
       
        ///低频检查renders
        if(Time.realtimeSinceStartup - __lastCheckRenders > render_interval)
        {
            UnityEngine.Profiling.Profiler.BeginSample(cameraName2);
            __checkRenderList();
            __lastCheckRenders = Time.realtimeSinceStartup;
            UnityEngine.Profiling.Profiler.EndSample();

        }
        if (tmp == null) tmp = new MaterialPropertyBlock();
        UnityEngine.Profiling.Profiler.BeginSample(cameraName);

        if (effectOtherRenders.size > 0 || effectMeshs.size > 0)
        {
            InitCommond();
           
            if (mCommandBuf != null)
            {
                mCommandBuf.Clear();
                UnityEngine.Profiling.Profiler.BeginSample("Draw Renders");
                for (int i = 0; i < effectOtherRenders.size; ++i)
                {
                    var r = effectOtherRenders[i];
                    if (!r || !r.material) continue;

                    if (true)
                    {                        
                        mCommandBuf.DrawRenderer(r, r.material);
                    }
                   
                }
                UnityEngine.Profiling.Profiler.EndSample();

                // #if !UNITY_EDITOR || UNITY_2019_2_OR_NEWER
                if (BuilderConfig.SupportsInstance )
                {
                    if (effectMeshs.size > 0 && effectMeshs.size == effectMeshMats.size && effectMeshTransforms.size == effectMeshs.size)
                    {
                        int Number = 0;
                        UnityEngine.Profiling.Profiler.BeginSample("Draw DrawMeshInstanceds");
                        for (int i = 0; i < effectMeshs.size; ++i)
                        {

                            if (effectMeshTransforms.size > i && effectMeshs[i] && effectMeshMats[i])
                            {

                                var m = effectMeshTransforms[i][0].GetComponent<MeshRenderer>().material;
                                var mm = effectMeshMats[i];

                                //if (!ReferenceEquals(m,mm))
                                //{
                                //    mCommandBuf.DrawRenderer(effectMeshTransforms[i][0].GetComponent<MeshRenderer>(), m);
                                //}
                                //else
                                {

                                    if (matrix == null) matrix = new MyBetterList<Matrix4x4>(5);

                                    Number = 0;

                                    for (int n = 0; n < effectMeshTransforms[i].Count; ++n)
                                    {
                                        if (!effectMeshTransforms[i][n]) continue;

                                        var matrixItem = Matrix4x4.TRS(effectMeshTransforms[i][n].position, effectMeshTransforms[i][n].rotation, effectMeshTransforms[i][n].lossyScale);

                                        if (matrix.size <= Number)
                                            matrix.Add(matrixItem);
                                        else
                                            matrix[Number] = matrixItem;

                                        Number++;
                                    }

                                    Number = Mathf.Min(Number, matrix.size);
                                    if (Number > 0 && matrix.size >= Number && mm && effectMeshs[i])
                                    {
                                        mCommandBuf.DrawMeshInstanced(effectMeshs[i], 0, mm, 0, matrix.buffer, Number, tmp);
                                    }
                                }
                            }
                        }

                        UnityEngine.Profiling.Profiler.EndSample();
                    }
                }
//#endif
            }

        }
        else
        {
            ClearData();
        }

        UnityEngine.Profiling.Profiler.EndSample();
    }
    MyBetterList<Matrix4x4> matrix;
    private void OnDestroy()
    {
        effectMeshs.Release();       
        effectOtherRenders.Release();
        effectMeshTransforms.Release();
        effectMeshMats.Release();
        ClearData();
    }

    void ClearData()
    {
        if(_camera && mCommandBuf != null)
        {
            _camera.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, mCommandBuf);
        }
      
        if (mCommandBuf != null)
        {
            CommandBufferPool.Release(mCommandBuf);
        }
        mCommandBuf = null;

       
    }

    void InitCommond()
    {
        if (mCommandBuf == null)
        {
            mCommandBuf = CommandBufferPool.Get($"UI特效_{cameraName}");           
            _camera.AddCommandBuffer(CameraEvent.BeforeImageEffects, mCommandBuf);
        }
    }

    float __lastCheckRenders = 0;
    void __checkRenderList()
    {
        if (uiEffects.size == 0) { return; }

        effectMeshs.Clear();
        effectOtherRenders.Clear();
        effectMeshTransforms.Clear();
        effectMeshMats.Clear();

        var waitRemove = MyListPool<int>.Get();

        for (int i = 0; i < uiEffects.size; ++i)
        {
            if (!uiEffects[i])
            {
                waitRemove.Add(i);
                continue;
            }
            var rl = MyListPool<Renderer>.Get();
            uiEffects[i].GetComponentsEx(rl);
            for(int n = 0; n < rl.Count; ++n)
            {
                var r = rl[n];

                if (!r.enabled || !r.gameObject.IsActive())
                    continue;
                //r.material.DisableKeyword("__USING_FOG");

                if ((r is MeshRenderer && r.gameObject.GetComponent<MeshFilter>()) /*|| r is SkinnedMeshRenderer*/)
                {
                    Material mat = null;
                    Mesh mesh = null;
                   
                    //r.sharedMaterial.DisableKeyword("__USING_FOG");
                    if (r is MeshRenderer)
                    {
                        mat = r.material;
                        if (!mat) continue;
                        mesh = r.gameObject.GetComponent<MeshFilter>().sharedMesh;
                    }
                    else if (r is SkinnedMeshRenderer)
                    {
                        var sr = r as SkinnedMeshRenderer;
                        if (!sr) continue;
                        mat = sr.sharedMaterial;
                        mesh = sr.sharedMesh;
                    }                   

                    if (!mat || !mesh) continue;

                    if (mat.name.EndsWith("(Instance)"))
                    {
                        //Log.LogError(mat.name);
                        effectOtherRenders.Add(r);
                    }
                    else if(MaterialAnimationBehaviour.GetInstanceByGameobject(r.gameObject) && MaterialAnimationBehaviour.GetInstanceByGameobject( r.gameObject).DisableInstance )
                    {
                        effectOtherRenders.Add(r);
                    }
                    else
                    {
                        //Log.LogError(r.gameObject.ToString());

                        //#if UNITY_EDITOR && !UNITY_2019_2_OR_NEWER
                        if (!BuilderConfig.SupportsInstance)
                        {
                            effectOtherRenders.Add(r);
                        }
                        //#else
                        else
                        {

                            if (!mat.enableInstancing)
                                mat.enableInstancing = true;


                            if (mesh)
                            {
                                var meshidx = effectMeshs.IndexOf(mesh);
                                if (meshidx >= 0)
                                {
                                    //  var idx = effectMeshs.IndexOf(mesh);
                                    if (effectMeshTransforms.size < meshidx)
                                    {
                                        Log.LogError($"error:effectMeshTransforms count is {effectMeshTransforms.size},effectMeshs count is {effectMeshs.size}");
                                        goto ErrorConfig;
                                    }

                                    effectMeshTransforms[meshidx].Add(r.gameObject.transform);
                                }
                                else
                                {
                                    effectMeshs.Add(mesh);
                                    effectMeshMats.Add(mat);

                                    if (effectMeshTransforms.buffer != null && effectMeshTransforms.buffer.Length >= effectMeshs.size)
                                    {
                                        effectMeshTransforms.size = effectMeshs.size;
                                        var tl = effectMeshTransforms[effectMeshs.size - 1];
                                        if (tl != null && tl.Count > 0) tl.Clear();
                                        tl.Add(r.gameObject.transform);
                                    }
                                    else
                                    {
                                        var tl = new List<Transform>();
                                        tl.Add(r.gameObject.transform);
                                        effectMeshTransforms.Add(tl);
                                    }
                                }
                            }
                        }
//#endif
                    }
                }
                
                else
                {
                    if(r is SkinnedMeshRenderer)
                    {
                        if (r.GetComponentInParent<Animator>())
                        {                           
                            r.GetComponentInParent<Animator>().cullingMode = AnimatorCullingMode.AlwaysAnimate;                           
                        }
                    }
                    effectOtherRenders.Add(r);
                }
            }
            MyListPool<Renderer>.Release(rl);
        }

        if(waitRemove.Count > 0)
        {
            for(int i = 0; i < waitRemove.Count; ++i)
            {
                uiEffects.RemoveAt(waitRemove[i]);
            }
            waitRemove.Clear();
        }

        MyListPool<int>.Release(waitRemove);

        return;

    ErrorConfig:
        effectMeshs.Clear();       
        effectMeshTransforms.Clear();
        effectMeshMats.Clear();
        return;
    }
}
