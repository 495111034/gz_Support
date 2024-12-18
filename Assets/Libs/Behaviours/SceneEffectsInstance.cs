using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 特效中的MeshRender
/// </summary>
public class SceneEffectsInstance : MonoBehaviour
{    
    public struct MeshMaterial
    {
        public MeshRenderer renderer;
        public MaterialPropertyBlock block;
        public bool canntInstancing;
        //public Mesh mesh;
        //public Material material;
    };

    List<GameObject> meshRenderObj = new List<GameObject>();    
    MyBetterList<MeshMaterial> meshMatList = new MyBetterList<MeshMaterial>(10);
    MyBetterList<MyBetterList<Transform>> instanceTransformList = new MyBetterList<MyBetterList<Transform>>(10);

    MyBetterList<BoundingSphere> bundingSphereCache;
    CullingGroup cullingGroup = null;

    bool _isRending = false;

    public void AddEffect(GameObject go)
    {       
        if (!BuilderConfig.SupportsInstance) return;
        //#if UNITY_EDITOR && !UNITY_2019_2_OR_NEWER
        //        return;
        //#endif
        bool hasBadRenderer = false;
        var rl = MyListPool<MeshRenderer>.Get();
        go.GetComponentsEx(rl);
        for(int i = 0; i < rl.Count; ++i)
        {
            var r = rl[i];
            if (r.gameObject.GetComponent<SkinnedMeshRenderer>()) continue;
            //if (GPUSkinningPlayerMono.GetInstanceByGameobject( r.gameObject)) continue;
            if (!r.gameObject.GetComponent<MeshFilter>()) continue;
            if (meshRenderObj.Contains(r.gameObject)) continue;
            if (!r.sharedMaterial) continue;
            //if (r.material && r.material.name.EndsWith("(Instance)")) continue;
            if (!r.gameObject.GetComponent<MeshFilter>().sharedMesh) continue;
            if (MaterialAnimationBehaviour.GetInstanceByGameobject( r.gameObject) && MaterialAnimationBehaviour.GetInstanceByGameobject( r.gameObject).DisableInstance) continue;

            if(!r.sharedMaterial.enableInstancing)r.sharedMaterial.enableInstancing = true;

            r.gameObject.SetLayerRecursively((int)ObjLayer.Hidden);
            r.enabled = false;
            meshRenderObj.Add(r.gameObject);

            AddCulling(r);

            bool hasExists = false;
            for(int n = 0; n < meshMatList.size; ++n)
            {
                if (!meshMatList[n].renderer)
                {
                    hasBadRenderer = true;
                    continue;
                }

                var meshnameS = meshMatList[n].renderer.gameObject.GetComponent<MeshFilter>().sharedMesh.name.Replace("(Instance)", "").Trim().ToLower();
                var meshnameT = r.gameObject.GetComponent<MeshFilter>().sharedMesh.name.Replace("(Instance)", "").Trim().ToLower();
                var matnameS = meshMatList[n].renderer.sharedMaterial.name.Replace("(Instance)", "").Trim().ToLower();
                var matnameT = r.sharedMaterial.name.Replace("(Instance)", "").Trim().ToLower();

                if (meshnameS.CompareTo(meshnameT) == 0 && matnameS.CompareTo(matnameT) == 0)
                {                    
                    instanceTransformList[n].Add(r.gameObject.transform);                    

                    hasExists = true;
                    break;
                }
            }

            if(!hasExists)
            {
                MeshMaterial mm = new MeshMaterial()
                {
                    renderer = r,
                    block = new MaterialPropertyBlock(),
                    canntInstancing = Application.isMobilePlatform && SystemInfo.graphicsShaderLevel >= 50 && r && r.material && r.material.shader && r.material.shader.name.ToLower().Contains("tesselation"),
                    //mesh = r.gameObject.GetComponent<MeshFilter>().sharedMesh,
                    // material = r.material,
                };
               
                meshMatList.Add(mm);
                var tempList = new MyBetterList<Transform>(5);
                tempList.Add(r.gameObject.transform);
                instanceTransformList.Add(tempList);

            }
        }
        MyListPool<MeshRenderer>.Release(rl);
        if (hasBadRenderer)
            __clearBadRenderer();
    }

    public void RemoveEffect(GameObject go)
    {
        if (!BuilderConfig.SupportsInstance) return;
        var rl = MyListPool<MeshRenderer>.Get();
        go.GetComponentsEx<MeshRenderer>(rl);

        bool hasBadRenderer = false;
        for (int i = 0; i < rl.Count; ++i)
        {
            var r = rl[i];
            if (meshRenderObj.Contains(r.gameObject))
            {
                r.gameObject.SetLayerRecursively((int)ObjLayer.SceneEffect);
                r.enabled = true;
                RemoveCulling(r.gameObject);
                meshRenderObj.Remove(r.gameObject);

                for (int n = 0; n < meshMatList.size; ++n)
                {
                    if (!meshMatList[n].renderer)
                    {
                        hasBadRenderer = true;
                        continue;
                    }

                    var meshnameS = meshMatList[n].renderer.gameObject.GetComponent<MeshFilter>().sharedMesh.name.Replace("(Instance)", "").Trim().ToLower();
                    var meshnameT = r.gameObject.GetComponent<MeshFilter>().sharedMesh.name.Replace("(Instance)", "").Trim().ToLower();
                    var matnameS = meshMatList[n].renderer.sharedMaterial.name.Replace("(Instance)", "").Trim().ToLower();
                    var matnameT = r.sharedMaterial.name.Replace("(Instance)", "").Trim().ToLower();

                    if (meshnameS.CompareTo(meshnameT) == 0 && matnameS.CompareTo(matnameT) == 0)
                    {
                        var idx = instanceTransformList[n].IndexOf(r.gameObject.transform);
                        if (idx >= 0)
                        {
                            instanceTransformList[n].RemoveAt(idx);
                        }
                        //else
                        //{
                        //    Log.LogError($"{r.gameObject.name} not in instanceTransformList");
                        //}

                        if (instanceTransformList[n].size == 0)
                        {
                            instanceTransformList.RemoveAt(n);
                            meshMatList[n].block.Clear();
                            meshMatList.RemoveAt(n);
                        }
                        break;

                    }
                }
            }
        }
        MyListPool<MeshRenderer>.Release(rl);
        if (hasBadRenderer)
            __clearBadRenderer();
    }

    void AddCulling(Renderer r)
    {
        if (bundingSphereCache == null)
        {
            bundingSphereCache = new MyBetterList<BoundingSphere>(100);            
        }      
        var bunds = r.bounds;
        BoundingSphere bounds = new BoundingSphere();
        bounds.position = bunds.center;
        bounds.radius = Mathf.Max(bunds.size.x, bunds.size.y, bunds.size.z) / 2f;
        bundingSphereCache.Add(bounds);

        SetCullingCache();
    }

    void SetCullingCache()
    {
        if (cullingGroup == null)
        {
            cullingGroup = new CullingGroup();
            cullingGroup.targetCamera = CameraUtils.MainCamera ? CameraUtils.MainCamera : Camera.main;
            cullingGroup.SetBoundingDistances(new float[] { 50 });
            cullingGroup.SetDistanceReferencePoint(cullingGroup.targetCamera.transform);

        }
        cullingGroup.SetBoundingSpheres(bundingSphereCache.buffer);
       // int count = Math.Min(SingleItemCount(), bundingSphereCache.buffer.Length);
        cullingGroup.SetBoundingSphereCount(meshRenderObj.Count);
    }

    void RemoveCulling(GameObject obj)
    {
        if(meshRenderObj.Contains(obj))
        {
            var idx = meshRenderObj.IndexOf(obj);            
            bundingSphereCache.RemoveAt(idx);
            SetCullingCache();
        }
    }




    public void StartRender()
    {
        _isRending = true;       
    }

    public void StopRender()
    {
        _isRending = false;

        if (matrix != null)
            matrix.Release();

    }

    private void OnDestroy()
    {
        StopRender();

        if (bundingSphereCache != null)
        {
            bundingSphereCache.Release();
        }
        bundingSphereCache = null;
        if (cullingGroup != null)
        {
            cullingGroup.Dispose();
        }
        cullingGroup = null;

       
    }

    private void OnDisable()
    {
        StopRender();
    }
    private float _last_udpate_time;
    MyBetterList<Matrix4x4> matrix;
    private void Update()
    {
        if (!BuilderConfig.SupportsInstance) return;

        if (!_isRending) return;

        //if (Time.realtimeSinceStartup - _last_udpate_time >= 0.1f)
        //{
        //    _last_udpate_time = Time.realtimeSinceStartup;
        //}
        //else
        //{
        //    return;
        //}

        UnityEngine.Profiling.Profiler.BeginSample("Scene Effects update pos ");

       // List<GameObject> __waitForDel = MyListPool<GameObject>.Get();

        for(int i = 0; i < meshRenderObj.Count; ++i)
        {
            if (meshRenderObj[i])
            {
                var bounds = meshRenderObj[i].GetComponent<Renderer>().bounds;
                bundingSphereCache.buffer[i].position = bounds.center;
                bundingSphereCache.buffer[i].radius = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z) / 2f;
            }
            else
            {
                //__waitForDel.Add(meshRenderObj[i]);
            }
        }


        //MyListPool<GameObject>.Release(__waitForDel);
        UnityEngine.Profiling.Profiler.EndSample();

        bool hasBadRenderer = false;
        UnityEngine.Profiling.Profiler.BeginSample("Scene Effects update list ");
        for (int n = 0; n < meshMatList.size; ++n)
        {
            if (matrix == null) matrix = new MyBetterList<Matrix4x4>(10);
           
            var mm = meshMatList[n];

            if(!mm.renderer || !mm.renderer.gameObject || !mm.renderer.gameObject.GetComponent<MeshFilter>().sharedMesh || !mm.renderer.material)
            {
                hasBadRenderer = true;               
                continue;
            }           

           MaterialPropertyBlock b = null; 

            int Number = 0;
           // List<Transform> waitRemoveList = MyListPool<Transform>.Get();
            if (instanceTransformList.size > n)
            {
                
                for (int i = 0; i < instanceTransformList[n].size; ++i)
                {
                    if (!instanceTransformList[n][i].gameObject || !instanceTransformList[n][i].gameObject.activeInHierarchy) 
                    {
                        /*waitRemoveList.Add(instanceTransformList[n][i]);*/ continue; 
                    }

                    var itemobj = instanceTransformList[n][i];

                    if (BuilderConfig.IsDebugBuild) UnityEngine.Profiling.Profiler.BeginSample("GetComponent 11 ");
                    var mab = MaterialAnimationBehaviour.GetInstanceByGameobject(itemobj.gameObject);//.GetComponent<MaterialAnimationBehaviour>();
                    if (BuilderConfig.IsDebugBuild) UnityEngine.Profiling.Profiler.EndSample();
                    //不更新材质
                    mab?.SetLogicActive(false);                   

                    var spIdx = meshRenderObj.IndexOf(itemobj.gameObject);
                    if (spIdx < 0 ) 
                    { 
                        Log.LogError($"{itemobj.gameObject.name} is not in meshRenderObj"); /*waitRemoveList.Add(instanceTransformList[n][i])*/;                       
                        continue; 
                    }                    
                    
                    var bVisible = cullingGroup != null ? cullingGroup.IsVisible(spIdx) : false;
                    

                    if (bVisible)
                    {
                        //更新transform
                        mab?.SetVisibleInScene(true);
                       
                        if (matrix.size <= Number)
                            matrix.Add(Matrix4x4.TRS(itemobj.position, itemobj.rotation, itemobj.lossyScale));
                        else
                            matrix[Number] = Matrix4x4.TRS(itemobj.position, itemobj.rotation, itemobj.lossyScale);
                        
                        Number++;
                    }
                    else
                    {
                        //不更新transform
                        mab?.SetVisibleInScene(false);
                    }
                }
                
                if (Number > 0)
                {
                    var mab = MaterialAnimationBehaviour.GetInstanceByGameobject(instanceTransformList[n][0].gameObject);
                    mab?.DoUpdateByTimeline(Time.deltaTime);//调用第一个的材质更新
                   
                    if (mab && mab.materialPropertys != null)
                    {
                        b = mab.materialPropertys;
                    }
                    else
                    {                        
                        mm.renderer.GetPropertyBlock(mm.block);
                        b = mm.block;                        
                    }

                    UnityEngine.Profiling.Profiler.BeginSample("Scene Effects DrawCalls ");
                    DrawCall(mm, Number, b);
                    UnityEngine.Profiling.Profiler.EndSample();
                }
            }

            //MyListPool<Transform>.Release(waitRemoveList);


        }
        UnityEngine.Profiling.Profiler.EndSample();

        if(hasBadRenderer)
        {
            //UnityEngine.Profiling.Profiler.BeginSample("Scene Effects __clearBadRenderer ");
            __clearBadRenderer();
            //UnityEngine.Profiling.Profiler.EndSample();
        }
    }

    void __clearBadRenderer()
    {
        bool hasBadRenderer = false;
        int badIdx = -1;
        for (int n = 0; n < meshMatList.size; ++n)
        {
            var mm = meshMatList[n];

            if (!mm.renderer || !mm.renderer.gameObject || !mm.renderer.gameObject.GetComponent<MeshFilter>().sharedMesh || !mm.renderer.material)
            {
                hasBadRenderer = true;
                badIdx = n;
                break;
            }

            int badTransform = -1;
            for(int i = 0; i < instanceTransformList[n].size; ++i)
            {
                var obj = instanceTransformList[n][i];
                if(!obj || !obj.gameObject )
                {
                    badTransform = i;
                    break;
                }
            }

            if(badTransform >= 0)
            {
                instanceTransformList[n].RemoveAt(badTransform);
            }

            if(instanceTransformList[n].size == 0)
            {
                hasBadRenderer = true;
                badIdx = n;
                break;
            }
        }

        if(hasBadRenderer && badIdx >= 0)
        {
            if (meshMatList[badIdx].block != null)
                meshMatList[badIdx].block.Clear();            
            meshMatList.RemoveAt(badIdx);
            instanceTransformList[badIdx].Clear();
            instanceTransformList.RemoveAt(badIdx);

            //不递归调用，以免死循环风险
            //__clearBadRenderer();
        }
    }
    MaterialPropertyBlock tmp;
    /// <summary>
    /// 不要在shader中使用UNITY_ACCESS_INSTANCED_PROP，UNITY_ACCESS_INSTANCED_PROP只适合unity自动调用instance的时候，手动instance时会导致只有第一个实例才生效
    /// </summary>
    /// <param name="mm"></param>
    /// <param name="number"></param>
    /// <param name="block"></param>
    public void DrawCall(MeshMaterial mm,int number, MaterialPropertyBlock block)
    {        
        if (number <= 0) return;

        if (tmp == null) tmp = new MaterialPropertyBlock();
        MaterialPropertyBlock _block = block;
        if (_block == null)
            _block = tmp;
        if (!mm.renderer.gameObject.GetComponent<MeshFilter>().sharedMesh || !mm.renderer.material || !mm.renderer.material.enableInstancing || matrix.size < number) return;
        
        if(mm.canntInstancing)
        {
            for (int i = 0; i < number; ++i)
            {
                Graphics.DrawMesh(mm.renderer.gameObject.GetComponent<MeshFilter>().sharedMesh, matrix[i], mm.renderer.material, (int)ObjLayer.SceneEffect);
            }
        }
        else
        {
            Graphics.DrawMeshInstanced(mm.renderer.gameObject.GetComponent<MeshFilter>().sharedMesh, 0, mm.renderer.material, matrix.buffer, number, _block, UnityEngine.Rendering.ShadowCastingMode.Off, false, (int)ObjLayer.SceneEffect /*gameObject.layer*/);
        }
        
    }

    int SingleItemCount()
    {
        int count = 0;
        for(int i = 0; i < meshMatList.size; ++i)
        {
            count += instanceTransformList[i].size;
        }
        return count;
    }
}

