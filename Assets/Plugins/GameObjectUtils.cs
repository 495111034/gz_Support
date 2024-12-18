using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
#if UNITY_ASYNC
using System.Threading;
using System.Threading.Tasks;
#endif
using Object = UnityEngine.Object;

/// <summary>
/// GameObject 辅助工具
/// </summary>

static public partial class GameObjectUtils
{

    #region 获取依赖项

    // 这个函数来自 BuilderUtils 类中, 必须保持一致
    /// <summary>
    /// 收集依赖的资源
    ///     包含: 
    ///         Material(Shader+Texture)
    ///         MeshFilter(Mesh)
    ///         TrailRenderer
    ///         SkinnedMeshRenderer(Mesh)
    ///         ParticleSystem(mesh)
    ///         Animation(AnimationClip)
    ///         AudioSource(AudioClip)
    ///         RuntimeAnimatorController
    /// </summary>
    public static MyBetterList<Object> GetDependAssetsList(this GameObject go)
    {

        MyBetterList<Object> list = new MyBetterList<Object>(10);
        if (go == null)
        {
            Log.LogError("GetDependAssetsList, go == null");
            return list;
        }

        Queue<GameObject> queue = new Queue<GameObject>();

        queue.Enqueue(go);
        while (queue.Count > 0)
        {
            go = queue.Dequeue();

            try
            {
                // material
                var renders = go.GetComponents<Renderer>();
                if (renders.Length > 0)
                {
                    // materials                
                    //foreach (var mat in render.sharedMaterials) AddDepList(list, mat);
                    for (int i = 0; i < renders.Length; ++i)
                        AddDepList(list, renders[i]);
                }
            }
            catch (System.Exception e)
            {
                Log.LogError($"{go.name},GetDependAssetsList get Renderer error:{e.Message},{e.StackTrace}");
            }

            //var ps = go.GetComponent<ParticleSystem>();
            //if (ps)
            //{
            //    AddDepList(list, ps);
            //} 

            try
            {
                // MeshFilter
                var mf = go.GetComponent<MeshFilter>();
                if (mf)
                {
                    AddDepList(list, mf);
                }
            }
            catch (System.Exception e)
            {
                Log.LogError($"{go.name},GetDependAssetsList get MeshFilter error:{e.Message}");
            }

            try
            {
                // animation
                var anim = go.GetComponent<Animation>();
                if (anim)
                {
                    AddDepList(list, anim);
                }
            }
            catch (System.Exception e)
            {
                Log.LogError($"{go.name},GetDependAssetsList get Animation error:{e.Message}");
            }

            try {

                // Animator
                var amt = go.GetComponent<Animator>();
                if (amt)
                {
                    AddDepList(list, amt);
                }
            }
            catch (System.Exception e)
            {
                Log.LogError($"{go.name},GetDependAssetsList get Animator error:{e.Message}");
            }

            try
            {
                var spray = go.GetComponent<MyParticle.Spray>();
                if (spray)
                {
                    AddDepList(list, spray);
                }
            }
            catch (System.Exception e)
            {
                Log.LogError($"{go.name},GetDependAssetsList get Spray error:{e.Message}");
            }



            // children
            foreach (Transform t in go.transform)
            {
                queue.Enqueue(t.gameObject);
            }
        }

        return list;
    }
    static void AddDepList(MyBetterList<Object> list, Object obj)
    {
        if (obj != null && list.IndexOf(obj) < 0) list.Add(obj);
    }

    public static List<Component> GetDependAssetsList2(this GameObject go)
    {
        var ret = MyListPool<Component>.Get();
        go.GetComponentsInChildren(ret);
        for (var i = ret.Count - 1; i >= 0; --i)
        {
            var com = ret[i];
            if (!(com is Renderer || com is MeshFilter || com is MyParticle.Spray))
            {
                ret.swap_tail_and_fast_remove(i);
            }
        }
        return ret;
    }


    #endregion
    public class EventObject
    {
        public GameObject obj;
        public object param;
    }

    public static bool Animator_IsName(this Animator animator, string name)
    {
        if (!animator)
        {
            return false;
        }
        for (var i = 0; i < animator.layerCount; ++i)
        {
            if (animator.GetCurrentAnimatorStateInfo(i).IsName(name))
            {
                return true;
            }
        }
        return false;
    }

    public static bool Animator_Next_IsName(this Animator animator, string name1, string name2)
    {
        if (!animator)
        {
            return false;
        }
        var state = animator.GetNextAnimatorStateInfo(0);
        return state.IsName(name1) || state.IsName(name2);
    }

    public static void PlayAnima(this Animator animator, string name)
    {
        if (animator != null)
        {
            if (!animator.enabled) animator.enabled = true;
            animator.Play(name, 0, 0);
        }
    }

    public static T Find<T>(this MyBetterList<Object> list, string name) where T : Object
    {
        // foreach (var obj in list)
        for (int i = 0; i < list.size; ++i)
        {
            var obj = list[i];
            if (obj.name.ToLower().CompareTo(name.ToLower()) == 0 && obj is T) return obj as T;
        }
        //Log.LogError("cant find obj, name:{0}, type:{1},count:{2},all names:{3}", name, typeof(T).Name, list.Count, (list.Select(i => (i.name + "(" + i.GetType().ToString() + ")").ToString(CultureInfo.InvariantCulture)).Aggregate((s1, s2) => s1 + ", " + s2)));
        return null;
    }

    public static MyBetterList<T> FindList<T>(this MyBetterList<Object> list, string name) where T : Object
    {
        MyBetterList<T> l = new MyBetterList<T>(5);
        for (int i = 0; i < list.size; ++i)
        {
            var obj = list[i];
            if (obj.name.CompareTo(name) == 0 && obj is T) l.Add(obj as T);
        }
        return l;
    }

    // 重置 go 上的动画状态
    public static void ResetAnim(GameObject go, bool bPlay)
    {
        var anim = go.GetComponent<Animation>();
        if (anim && anim.clip)
        {
            //Log.LogInfo("ResetAnim, go:{0}, anim:{1}, clip:{2}, bPlay:{3}", go, anim, anim.clip, bPlay);
            anim.Stop();
            anim.clip.SampleAnimation(go, 0);
            if (bPlay) anim.Play();
        }
    }

    public static void ResetAllAnim(GameObject go, bool bPlay)
    {
        var anims = GameObjectUtils.GetComponentsEx<Animation>(go, true);
        for (int i = 0; i < anims.Count; ++i)
        {
            ResetAnim(anims[i].gameObject, true);
        }
    }

    // 查找动画
    public static Animation FindAnimation(this GameObject go, string clip_name_filter)
    {
        if (clip_name_filter == null)
        {
            return go.GetComponentInChildren<Animation>();
        }
        foreach (var anim in go.GetComponentsInChildren<Animation>())
        {
            if (anim.clip != null && anim.clip.name.Contains(clip_name_filter))
            {
                return anim;
            }
        }
        return null;
    }

    /// <summary>
    /// 获得顶层gameobject
    /// </summary>
    /// <param name="go"></param>
    /// <returns></returns>
    public static GameObject GetTopGameobject(this GameObject go)
    {
        if (go.transform.parent != null)
        {
            return go.transform.parent.gameObject.GetTopGameobject();
        }
        else
        {
            return go;
        }
    }


    /// <summary>
    /// 获得所有上级gameobject
    /// </summary>
    /// <param name="go"></param>
    /// <param name="topGo"></param>
    /// <returns></returns>
    public static List<Transform> GetParentList(this GameObject go, GameObject topGo = null)
    {
        List<Transform> finds = new List<Transform>();
        if (go == null || (go && go == topGo)) return finds;

        finds.Add(go.transform);

        if (go.transform.parent)
        {
            var f = go.transform.parent.gameObject.GetParentList(topGo);
            for (int n = 0; n < f.Count; n++)
            {
                finds.Add(f[n]);
            }
        }

        return finds;
    }

    //// 查找多个组件(含子节点)
    public static void GetComponentsEx<T>(this GameObject go, List<T> list, bool includeInactive = false) where T : Component
    {
        go.GetComponentsInChildren(includeInactive, list);
    }

    // 查找多个组件(含子节点)
    public static List<T> GetComponentsEx<T>(this GameObject go, bool includeInactive = false) where T : Component
    {
        var list = new List<T>();
        go.GetComponentsInChildren(includeInactive, list);
        return list;
    }

    // 查找单个组件(含子节点)
    public static T GetComponentEx<T>(this GameObject go) where T : Component
    {
        if (!go) return null;
        var comp = go.GetComponent<T>();
        if (comp == null) comp = go.GetComponentInChildren<T>();
        return comp;
    }

    // 获取旗下所有材质
    public static List<Material> GetAllMaterials(GameObject go, bool includeInactive)
    {
        List<Material> list = new List<Material>();

        //
        foreach (var r in GetComponentsEx<Renderer>(go, includeInactive))
        {
            AddMaterial(list, r);
        }


        // 
        return list;
    }
    static void AddMaterial(List<Material> list, Renderer r)
    {
        if (r.sharedMaterial)
        {
            if (!list.Contains(r.sharedMaterial))
            {
                list.Add(r.sharedMaterial);
            }
        }
        if (r.sharedMaterials != null)
        {
            foreach (var mat in r.sharedMaterials)
            {
                if (mat && !list.Contains(mat))
                {
                    list.Add(mat);
                }
            }
        }
    }

    //  添加碰撞体
    public static void SetBoxCollider(GameObject go, Vector3 size)
    {
        AddRigidbody(go);

        //
        var bc = go.GetComponent<BoxCollider>();
        if (bc == null) bc = go.AddComponent<BoxCollider>();
        bc.center = new Vector3(0, size.y / 2, 0);
        bc.size = size;
    }

    // 复制碰撞体信息, 并设置刚体
    public static void CopyBoxCollider(GameObject from, GameObject to)
    {
        var box = from.GetComponent<BoxCollider>();
        if (box)
        {
            AddRigidbody(to);

            //
            var box2 = to.GetComponent<BoxCollider>();
            if (box2 == null) box2 = to.AddComponent<BoxCollider>();

            box2.center = box.center;
            box2.size = box.size;
        }
    }

    // 添加刚体
    public static void AddRigidbody(GameObject go)
    {
        var rb = go.GetComponent<Rigidbody>();
        if (rb == null) rb = go.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }


    // 查找场景内所有的 go
    public static List<GameObject> FindAllGosInScene()
    {
        List<GameObject> list = new List<GameObject>();
        foreach (GameObject go in Object.FindObjectsOfType(typeof(GameObject)))
        {
            if (go.transform.parent == null)
            {
                list.Add(go);
            }
        }
        return list;
    }

    // 递归设置对象的层次
    static GameObject[] _s_GameObject_tmps = new GameObject[2048];
    public static void SetLayerRecursively(this GameObject go, int layer, uint mask)
    {
        UnityEngine.Profiling.Profiler.BeginSample("SetLayerRecursively");
        var tmps = _s_GameObject_tmps;
        int n = 0;
        tmps[n++] = go;
        while (n > 0)
        {
            go = tmps[--n]; tmps[n] = null;
            if (((1 << go.layer) & mask) != 0 && go.layer != layer)
            {
                go.layer = layer;
            }
            var got = go.transform;
            for(int i=0, childn = got.childCount;i<childn;++i)
            {
                var t = got.GetChild(i);
                if (n == 2048)
                {
                    Log.LogError("SetLayerRecursively objs too large");
                    break;
                }
                tmps[n++] = t.gameObject;
            }
        }
        UnityEngine.Profiling.Profiler.EndSample();
    }
    public static void SetLayerRecursively(this GameObject go, int layer)
    {

        SetLayerRecursively(go, layer, 0xffffffff);
    }

    public static void Destroy(Object obj)
    {
        if (Application.isEditor && !Application.isPlaying)
        {
            Object.DestroyImmediate(obj, true);
        }
        else
        {
            Object.Destroy(obj);
        }
    }


    // 销毁碰撞体
    public static void DestroyCollider(this GameObject go)
    {
        List<Collider> list = null;
        foreach (var c in go.GetComponentsInChildren<Collider>())
        {
            if (list == null) list = new List<Collider>();
            list.Add(c);
        }
        if (list != null)
        {
            foreach (var c in list) Object.Destroy(c);
        }
    }

    static Transform[] s_tbuf = new Transform[1024];

    /// <summary>
    /// 递归搜索第一个匹配(深度优先)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="go"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    static public T FindInChild<T>(this GameObject go, string name = "") where T : Component
    {
        if (go == null) return null;

        var buf = s_tbuf;
        var max = buf.Length;

        int n = 0;
        buf[n++] = go.transform;
        while (n > 0)
        {
            Transform t = buf[--n]; buf[n] = null;
            if (name == null || name == "" || t.name.Contains(name))
            {
                var comp = t.GetComponent<T>();
                if (comp)
                {
                    while (n > 0)
                    {
                        buf[--n] = null;
                    }
                    return comp;
                }
            }
            for (var i = t.childCount - 1; i >= 0; --i)
            {
                if (n == max)
                {
                    Log.LogError($"FindInChild, gameobject too large");
                    break;
                }
                buf[n++] = t.GetChild(i);
            }
        }
        return null;
    }


    static public void FindsInChild<T>(this GameObject go, List<T> finds, string name = "", bool checkActive = false) where T : Component
    {
        if (go == null || (checkActive && !(go.IsActive()))) return;
        go.GetComponentsInChildren(finds);
        if(!string.IsNullOrEmpty(name)) 
        {
            for (int i = finds.Count - 1; i >= 0; i--) 
            {
                if (finds[i].name != name) 
                {
                    finds.swap_tail_and_fast_remove(i);
                }
            }
        }
    }
    /// <summary>
    /// 递归搜索所有匹配
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="go"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    static public List<T> FindsInChild<T>(this GameObject go, string name = "", bool checkActive = false) where T : Component
    {
        List<T> finds = new List<T>();
        if (go == null || (checkActive && !(go.IsActive()))) return finds;
        FindsInChild(go, finds, name);
        return finds;
    }

    // 删除某个组件
    public static void DestroyComponent(GameObject go, Type type)
    {
        Component c = go.GetComponent(type);
        if (c != null) Object.Destroy(c);
    }


    static Stack<string> _names = new Stack<string>();
    /// <summary>
    /// 获取路径信息 获取一次600B的GC
    /// </summary>
    /// <param name="go">当前gameobject</param>
    /// <param name="topParent">最顶级(达到此gameobject停止向上搜索)</param>
    /// <returns></returns>
    public static string GetLocation(this GameObject go)
    {
        var names = _names;
        names.Clear();
        var t = go.transform;
        while (t)
        {
            names.Push(t.name);
            t = t.parent;
        }
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (names.Peek() == "Canvas (Environment)") 
            {
                names.Pop();
            }
        }
#endif
        return string.Join("/", names);
    }

    static Dictionary<GameObject, WeakReference> pathBuilder = new Dictionary<GameObject, WeakReference>();
    /// <summary>
    /// 获取路径信息 用StringBuilder优化 经测试比string少240B 即便ToString 也要少100B
    /// </summary>
    /// <param name="go">当前gameobject</param>
    /// <param name="topParent">最顶级(达到此gameobject停止向上搜索)</param>
    /// <returns></returns>
    public static string GetLocationByBuilder(this GameObject go)
    {
        if (pathBuilder.TryGetValue(go, out var wk) && wk.IsAlive) 
        {
            return (string)wk.Target;
        }
        
        if (pathBuilder.Count > 16) 
        {
            GameObject del = null;
            foreach (var kv in pathBuilder) 
            {
                del = kv.Key;
                if (!kv.Value.IsAlive) 
                {
                    break;
                }
            }
            pathBuilder.Remove(del);
        }

        var path = GetLocation(go);
        pathBuilder[go] = new WeakReference(path);
        return path;
    }

    /// <summary>
    /// 通过路径返回查找到的GameObject,允许有多个是因为允许有同名的GameObject
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static List<GameObject> GetObjectsByLocation(this GameObject root, string path)
    {
        Transform parent = root.transform;
        List<Transform> chirdren = new List<Transform>() { parent };

        if (string.IsNullOrEmpty(path.Trim()))
            return new List<GameObject>();

        string[] p = path.Split('/');
        int i = 0;
        for (i = p.Length - 2; i > 0; --i)
        {
            List<Transform> chirdren_child = new List<Transform>();
            chirdren_child.Clear();
            foreach (var c in chirdren)
            {
                for (int j = 0; j < c.childCount; ++j)
                {
                    var fc = c.GetChild(j);
                    if (fc.name.CompareTo(p[i]) == 0)
                    {
                        chirdren_child.Add(fc);
                    }
                }
            }

            if (chirdren_child.Count == 0)
                return null;

            chirdren = chirdren_child;
        }
        if (chirdren.Count == 0)
            return null;

        List<GameObject> findResult = new List<GameObject>();
        string objName = p[0];
        foreach (var c in chirdren)
        {
            for (int j = 0; j < c.childCount; ++j)
            {
                var fc = c.GetChild(j);
                if (fc.name.CompareTo(p[i]) == 0)
                {
                    findResult.Add(fc.gameObject);
                }
            }
        }

        return findResult;
    }

    // 输出资源信息
    static public void dumpResource(GameObject go)
    {
        List<string> msgs = new List<string>();
        msgs.Add("dumo of '" + go.name + "'");

        // 输出骨骼信息
        Transform[] transforms = go.GetComponentsInChildren<Transform>();
        msgs.Add("Bones: " + transforms.Length);
        foreach (Transform tran in transforms)
        {
            msgs.Add("   " + tran.name);
        }

        SkinnedMeshRenderer smr = go.GetComponentInChildren<SkinnedMeshRenderer>();
        if (smr != null)
        {
            Mesh mesh = smr.sharedMesh;
            msgs.Add("vertexCount: " + mesh.vertexCount);
            msgs.Add("triangles: " + mesh.triangles.Length / 3);

            BoneWeight[] bws = mesh.boneWeights;
            msgs.Add("bws: " + bws.Length);
            foreach (BoneWeight bw in bws)
            {
                msgs.Add(string.Format("  {0}A={1}, {2}B={3}, {4}C={5}, {6}D={7}",
                                        bw.boneIndex0, bw.weight0,
                                        bw.boneIndex1, bw.weight1,
                                        bw.boneIndex2, bw.weight2,
                                        bw.boneIndex3, bw.weight3));
            }

            if (msgs.Count > 200)
            {
                Log.LogInfo(string.Join("\n", msgs.ToArray()));
                msgs.Clear();
            }
        }

        //
        Log.LogInfo(string.Join("\n", msgs.ToArray()));
    }

    // 获取碰撞体的包围范围
    public static Bounds GetColliderBounds(GameObject go)
    {
        Bounds b = new Bounds(go.transform.position, Vector3.zero);
        foreach (var c in go.GetComponentsInChildren<Collider>())
        {
            b.Encapsulate(c.bounds);
        }
        return b;
    }
    public static Bounds GetFirstColliderBounds(GameObject go)
    {
        var c = go.GetComponentInChildren<Collider>();
        return c != null ? c.bounds : new Bounds(go.transform.position, Vector3.one);
    }

    // 获取 Y 旋转角度
    public static float GetRotateY(Vector3 forward)
    {
        return Mathf.Atan2(-forward.z, forward.x) * Mathf.Rad2Deg + 90;
    }

    // 获取或创建组件
    public static T GetOrCreateComponent<T>(this GameObject go) where T : Component
    {
        var t = go.GetComponent<T>();
        if (t == null)
        {
            t = go.AddComponent<T>();
        }
        return t;
    }


    /// <summary>
    /// 查找对象
    /// </summary>
    public static List<T> FindBehaviourByScreenPos<T>(GameObject mainCameraObject, Vector2 spos, int layerMask) where T : MonoBehaviour
    {
        if (!mainCameraObject)
        {
            return null;
        }

        var c = mainCameraObject.GetComponent<Camera>();
        if (!c)
        {
            if(Application.isEditor) Log.LogWarning("相机未初始化");        // 全屏时
            return null;
        }

        var r = c.ScreenPointToRay(new Vector2(spos.x, Screen.height - spos.y));
        var hits = Physics.RaycastAll(r, 1000, layerMask);
        List<T> list = new List<T>();
        foreach (var hit in hits)
        {
            var tt = hit.transform;
            while (tt != null)
            {
                var b = tt.gameObject.GetComponent<T>();
                if (b != null)
                {
                    list.Add(b as T);
                    break;
                }
                tt = tt.parent;
            }
        }
        return list;
    }

    public static Vector2 GetClickScreenPos() 
    {
        if (Input.touchCount > 0)
        {
            var touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                return touch.position;
            }
        }
        else if(Input.mousePresent && Input.GetMouseButtonDown(0)) 
        {
            return Input.mousePosition;
        }
        return Vector2.zero;
    }

    public static Transform GetFirstClickGameObject(Camera camera, int layerMask, out Vector3 clicked)
    {
        return GetFirstClickGameObject(camera, layerMask, Vector2.one, out clicked);
    }

    public static Transform GetFirstClickGameObject(Camera camera, int layerMask, Vector2 scene_to_world_ratio, out Vector3 clicked) 
    {
        var pos2 = GetClickScreenPos();
        pos2 *= scene_to_world_ratio;
        clicked = pos2;
        if (clicked != Vector3.zero) 
        {
            var r = camera.ScreenPointToRay(pos2);
            if (Physics.Raycast(r, out var hit, camera.fieldOfView * 2, layerMask)) 
            {
                clicked = hit.point;
                return hit.transform;
            }
        }
        return null;
    }


    public static void RayAllGameObject<T>(Camera camera, int layerMask, Vector2 scene_to_world_ratio, List<T> list) where T : Component
    {
        list.Clear();
        var pos2 = GetClickScreenPos();
        pos2 *= scene_to_world_ratio;
        if (pos2 != Vector2.zero)
        {
            var r = camera.ScreenPointToRay(pos2);
            var hits = Physics.RaycastAll(r, camera.fieldOfView * 2, layerMask);
            if (hits != null && hits.Length > 0)
            {
                for (int i = 0; i < hits.Length; i++)
                {
                    T t = hits[i].transform.GetComponent<T>();
                    if (t != null)
                    {
                        list.Add(t);
                    }
                }
            }
        }
    }

    // 获取渲染范围
    public static Bounds GetRenderBounds(this GameObject go, bool includeInactive)
    {
        Bounds b = new Bounds(go.transform.position, Vector3.one);
        var rs = GameObjectUtils.GetComponentsEx<Renderer>(go, includeInactive);
        foreach (var r in rs)
        {
            var b2 = r.bounds;
            b.Encapsulate(b2);
        }
        return b;
    }

    // 删除孩子
    public static void DestroyChildren(GameObject go)
    {
        if(!go)
        {
            Log.LogError($"DestroyChildren : null");
            return;
        }
        List<GameObject> list = MyListPool<GameObject>.Get();
        foreach (Transform t in go.transform)
        {
            list.Add(t.gameObject);
        }
        foreach (var go2 in list)
        {
            if(go2)
                Destroy(go2);
        }
        MyListPool<GameObject>.Release(list);
    }

    public static void SetActiveAndResert(this GameObject go)
    {
        if (!go) return;
        UnityEngine.Profiling.Profiler.BeginSample("SetActiveAndReset");
        go.SetActive(true);
        List<EffectBehaviour> _list = MyListPool<EffectBehaviour>.Get();
        go.GetComponentsInChildren(_list);
        //go.FindsInChild(_list);
        for (int i = 0; i < _list.Count; ++i)
        {
            _list[i].Resert();
        }
        MyListPool<EffectBehaviour>.Release(_list);

        List<MyParticle.Spray> _list2 = MyListPool<MyParticle.Spray>.Get();
        go.GetComponentsInChildren(_list2);
        //go.FindsInChild(_list2);
        for (int i = 0; i < _list2.Count; ++i)
        {
            _list2[i].Reset();
        }
        MyListPool<MyParticle.Spray>.Release(_list2);

        List<MaterialAnimationBehaviour> _list3 = MyListPool<MaterialAnimationBehaviour>.Get();
        go.GetComponentsInChildren(_list3);
        //go.FindsInChild(_list3);
        for (int i = 0; i < _list3.Count; ++i)
        {
            _list3[i].Resert();
        }
        MyListPool<MaterialAnimationBehaviour>.Release(_list3);
        UnityEngine.Profiling.Profiler.EndSample();
    }
#if UNITY_EDITOR
    static void DelayDestroyMono(MonoBehaviour mono) 
    {
        UnityEditor.EditorApplication.delayCall += () => { GameObject.DestroyImmediate(mono); };        
    }
#endif

    static public void SetParam(this GameObject obj, object param)
    {
        if (!obj) return;
        if(param is Delegate)
        {
            Log.LogError($"禁止将Action赋值给GameObject");
            return;
        }
        if (param != null)
        {
            var paramBehaviour = obj.AddMissingComponent<MonoBehaviourParam>();
            paramBehaviour.Param = param;
        }
        else
        {
            //var paramBehaviour = obj.GetComponent<MonoBehaviourParam>();
            if (obj.TryGetComponent<MonoBehaviourParam>(out var paramBehaviour))
            {
#if UNITY_EDITOR
                if (!UnityEngine.Application.isPlaying)
                {
                    DelayDestroyMono(paramBehaviour);
                }
                else
                {
                    paramBehaviour.Param = null;
                    //GameObject.Destroy(paramBehaviour);
                }
#else
                 paramBehaviour.Param = null;
                //GameObject.Destroy(paramBehaviour);
#endif
            }
        }

    }
    static public void SetParam(this MonoBehaviour obj, object param)
    {
        if (!obj) return;
        obj.gameObject.SetParam(param);
    }

    static public object GetParam(this MonoBehaviour obj)
    {
        if (!obj) return null;
        return obj.gameObject.GetParam();
    }

    static public object GetParam(this GameObject obj)
    {
        if (!obj.TryGetComponent<MonoBehaviourParam>(out var paramBehaviour)) 
        {
            return null;
        }
        return paramBehaviour ? paramBehaviour.Param : null;
    }

    static public T AddMissingComponent<T>(this GameObject go) where T : Component
    {
        if (!go)
        {
            if(go == null)
                Log.LogError($"AddMissingComponent<{typeof(T)}> error, go is null ");
            else
                Log.LogError($"AddMissingComponent<{typeof(T)}> error, go is alread destory ");

            return null;
        }
        //T comp = go.GetComponent<T>();
        if (!go.TryGetComponent<T>(out var comp))
        {
            comp = go.AddComponent<T>();
        }
        return comp;
    }



    //static public T AddChild<T>(GameObject parent) where T : Component
    //{
    //    GameObject go = AddChild(parent);
    //    go.name = GetTypeName<T>();
    //    return go.AddComponent<T>();
    //}

    /// <summary>
    /// Add a child object to the specified parent and attaches the specified script to it.
    /// </summary>

    static public T AddChild<T>(GameObject parent, bool undo = true) where T : Component
    {
        GameObject go = AddChild(parent, undo);
        go.name = GetTypeName<T>();
        return go.AddComponent<T>();
    }

    /// <summary>
    /// Add a new child game object.
    /// </summary>

    //static public GameObject AddChild(GameObject parent) { return AddChild(parent, true); }

    /// <summary>
    /// Add a new child game object.
    /// </summary>

    static public GameObject AddChild(GameObject parent, bool undo = true)
    {
        GameObject go = new GameObject();
#if UNITY_EDITOR
        if (undo) UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Create Object");
#endif
        if (parent != null)
        {
            Transform t = go.transform;
            t.SetParent(parent.transform);
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
            go.layer = parent.layer;
        }
        return go;
    }

    static public GameObject AddChild(GameObject parent, GameObject prefab)
    {
        GameObject go = GameObject.Instantiate(prefab) as GameObject;

#if UNITY_EDITOR && !UNITY_3_5 && !UNITY_4_0 && !UNITY_4_1 && !UNITY_4_2
        UnityEditor.Undo.RegisterCreatedObjectUndo(go, "Create Object");
#endif

        if (go != null && parent != null)
        {
            Transform t = go.transform;
            t.SetParent(parent.transform);
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
            go.layer = parent.layer;
        }
        return go;
    }


    /// <summary>
    /// Helper function that returns the string name of the type.
    /// </summary>

    static public string GetTypeName<T>()
    {
        string s = typeof(T).ToString();
        if (s.StartsWith("My")) s = s.Substring(2);
        else if (s.StartsWith("UnityEngine.")) s = s.Substring(12);
        return s;
    }

    /// <summary>
    /// Helper function that returns the string name of the type.
    /// </summary>

    static public string GetTypeName(UnityEngine.Object obj)
    {
        if (obj == null) return "Null";
        string s = obj.GetType().ToString();
        if (s.StartsWith("My")) s = s.Substring(2);
        else if (s.StartsWith("UnityEngine.")) s = s.Substring(12);
        return s;
    }

    public static bool IsActive(this Transform t)
    {
        return t.gameObject.activeInHierarchy;
    }
    public static bool IsActive(this GameObject go)
    {
        if (!go) return false;
        return go.activeInHierarchy;
    }

    /// <summary>
    /// Finds the specified component on the game object or one of its parents.
    /// </summary>
    static public T FindInParents<T>(Transform trans) where T : Component
    {
        if (trans == null) return null;
        return _FindInParents1<T>(trans, true);
    }
    static public T FindInParents<T>(this GameObject go, bool IncludingMyself = true) where T : Component
    {
        if (go == null) return null;
        return _FindInParents1<T>(go.transform, IncludingMyself);
    }
    static T _FindInParents1<T>(Transform go, bool IncludingMyself) where T : Component
    {
        if (!IncludingMyself)
        {
            go = go.parent;
            if (go == null)
            {
                return null;
            }
        }
        return go.GetComponentInParent<T>();
    }    

    // 输出树状结构
    public static void DumpTree(Transform t, StringBuilder sb = null, string prefix = null)
    {
        var is_mine = false;
        if (sb == null)
        {
            sb = new StringBuilder();
            prefix = "";
            is_mine = true;
        }

        // 名字
        sb.AppendFormat("{0}name:{1}\n", prefix, t.name);

        // 其它组件
        var arr = t.GetComponents<Component>();
        foreach (var c in arr)
        {
            if (c != null) sb.AppendFormat("{0}{1}\n", prefix, c.GetType().Name);
            else sb.AppendFormat("{0}null\n", prefix);
        }

        // 孩子
        sb.AppendFormat("{0}child:\n", prefix);
        prefix += "  ";
        for (int i = 0; i < t.childCount; i++)
        {
            var t2 = t.GetChild(i);
            DumpTree(t2, sb, prefix);
        }

        // 输出
        if (is_mine)
        {
            Log.LogInfo(sb.ToString());
        }
    }

    /// <summary>
    /// 设置 active, 会避免重复设置, 用在频繁调用的地方
    /// </summary>
    /// <param name="go"></param>
    /// <param name="active"></param>
    public static void SetActive(this GameObject go, bool active)
    {
        if (go && go.activeSelf != active)
        {
            UnityEngine.Profiling.Profiler.BeginSample($"{go.name} SetActive({active})");
            go.SetActive(active);
            UnityEngine.Profiling.Profiler.EndSample();
        }
    }
    public static void SetActive(this Component go, bool active)
    {
        if (go && go.gameObject.activeSelf != active)
        {
            UnityEngine.Profiling.Profiler.BeginSample($"{go.name} SetActive({active})");
            go.gameObject.SetActive(active);
            UnityEngine.Profiling.Profiler.EndSample();
        }
    }

    public static void SetActiveX(this GameObject go, bool active)
    {
        if (go && go.activeSelf != active) 
        {
            UnityEngine.Profiling.Profiler.BeginSample($"{go.name} SetActive({active})");
            go.SetActive(active);
            UnityEngine.Profiling.Profiler.EndSample();
        }
    }

    public static void PlayAnima(this GameObject go)
    {
        Animator anim = go.GetComponent<Animator>();
        if (anim != null && anim.runtimeAnimatorController != null && anim.runtimeAnimatorController.animationClips.Length > 0)
        {
            anim.PlayAnima(anim.runtimeAnimatorController.animationClips[0].name);
        }
    }

    public static void PlayAnima(this GameObject go, string ani_name)
    {
        Animator anim = go.GetComponent<Animator>();
        if (anim != null)
        {
            anim.PlayAnima(ani_name);
        }
    }

    public static void SetToggleActive(this Component go, bool active)
    {
        if (go && go.gameObject)
        {
            go.gameObject.SetActive(!active);
            go.gameObject.SetActive(active);
        }
    }
    
    public static void SetAlpha(this CanvasGroup go, float alpha)
    {
        if (go) go.alpha = alpha;
    }
    
    public static GameObject GetDontDestoryObject(string name, Type tp)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        if (Application.isPlaying)
            GameObject.DontDestroyOnLoad(go);
        return go;
    }

    /// <summary>
    /// 当前场景的基准高度
    /// </summary>
    static float LastSceneBaseHeight;
    public static float SceneBaseHeight;
    /// <summary>
    /// 获取当前高度
    /// </summary>
    /// <returns></returns>
    public static float GetHeight(Vector3 curPos)
    {
        RaycastHit hit;

        if (curPos.y > 0)
        {
            var r = new Ray(new Vector3(curPos.x, curPos.y + 1, curPos.z), Vector3.down);
            //UnityEngine.Profiling.Profiler.BeginSample("1Physics.Raycast");
            var b = Physics.Raycast(r, out hit, 2, (int)ObjLayerMask.HeightTest);
            //UnityEngine.Profiling.Profiler.EndSample();
            if (b)
            {
                return LastSceneBaseHeight = hit.point.y;
            }
        }
        if (curPos.y == 0) 
        {
            curPos.y = LastSceneBaseHeight;
        }

        var r2 = new Ray(new Vector3(curPos.x, curPos.y + 30, curPos.z), Vector3.down);
        //UnityEngine.Profiling.Profiler.BeginSample("2Physics.Raycast");
        var b2 = Physics.Raycast(r2, out hit, 60, (int)ObjLayerMask.HeightTest);
        //UnityEngine.Profiling.Profiler.EndSample();
        if (b2)
        {
            return LastSceneBaseHeight = hit.point.y;
        }

        //Log.LogInfo($"GetHeight {curPos} try {y} -> {CurrentSceneBaseHeight} + 20");
        r2 = new Ray(new Vector3(curPos.x, curPos.y + 500, curPos.z), Vector3.down);
        //UnityEngine.Profiling.Profiler.BeginSample("3Physics.Raycast");
        b2 = Physics.Raycast(r2, out hit, 1000, (int)ObjLayerMask.HeightTest);
        //UnityEngine.Profiling.Profiler.EndSample();
        if (b2)
        {
            return LastSceneBaseHeight = hit.point.y;
        }
        if (Application.isEditor)
        {
            //Log.LogError($"GetHeight fail, curPos={curPos}");
        }
        return curPos.y;
    }

    public static float GetHeight(float x, float y, float z) 
    {
        return GetHeight( new Vector3(x,y,z));
    }
    public static Vector3 Pos2ToPos3(Vector2 pos2) 
    {
        var pos3 = new Vector3( pos2.x, LastSceneBaseHeight, pos2.y);
        pos3.y = GetHeight(pos3);
        return pos3;
    }

    public static string GetLightmapKey(MeshRenderer mr, Dictionary<Transform, int> t_ids)
    {
        var t = mr.transform;
        while (!t_ids.ContainsKey(t))
        {
            if (!t.parent) return null;
            t = t.parent;
        }
        var id = t_ids[t];
        var key = id.ToString();
        if (t != mr.transform) key += "|" + mr.name;
        return key;
    }
    public static void SetChildsActive(this Component t, bool active)
    {
        if (!t) return;
        for (int i = 0; i < t.transform.childCount; ++i)
        {
            t.transform.GetChild(i).SetActive(active);
        }
    }

    public static void SetChildsActive(this GameObject t, bool active)
    {
        if (!t) return;
        for (int i = 0; i < t.transform.childCount; ++i)
        {
            t.transform.GetChild(i).SetActive(active);
        }
    }

    //散射参数
    public static int scatteringMapID { get; private set; } = -1;
    public static int scatteringColorID { get; private set; } = -1;    
    /// <summary>
    /// 是否带有散射功能
    /// </summary>
    /// <param name="material"></param>
    /// <returns></returns>
    public static bool HasMaterialSSSParameter(this Material material)
    {
        if (!material) return false;
        if (scatteringMapID <= 0 || scatteringColorID<= 0)
        {
            scatteringMapID = resource.ShaderNameHash.ShaderNameId("_ScatteringMap");
            scatteringColorID = resource.ShaderNameHash.ShaderNameId("_ScatteringColor");
        }

        if (!material.HasProperty(scatteringColorID) && !material.HasProperty(scatteringMapID))
            return false;

#if UNITY_EDITOR
        if (Application.isPlaying)
        {
            var tex = material.GetTexture(scatteringMapID);
            if (!tex) return false;  //没有遮罩的不处理
        }
#else
        var tex = material.GetTexture(scatteringMapID);
        if (!tex) return false;  //没有遮罩的不处理
#endif

        var color = material.GetColor(scatteringColorID);

        if (color == Color.black || (color.r < 0.1f && color.b < 0.1f && color.g < 0.1f)) return false; //次表面颜色为黑色的不处理
        //透明材质不支持散射
        if (material.renderQueue >= (int)UnityEngine.Rendering.RenderQueue.Transparent) return false;

        return true;
    }

    static int __ShaderID__TintColor = -1;
    public static int ShaderID__TintColor {
        get
        {
            if (__ShaderID__TintColor < 0)
            {
                __ShaderID__TintColor = resource.ShaderNameHash.ShaderNameId("_TintColor");
            }
            return __ShaderID__TintColor;
        }
    }
    
    static public void SetScale(this GameObject go, Vector3 scale)
    {
        if (go) go.transform.localScale = scale;
    }



    //private const int COMBINE_TEXTURE_MAX = 512;
    //private const string COMBINE_DIFFUSE_TEXTURE = "_MainTex";

    static readonly Dictionary<string, Mesh> _CombineMeshCache = new Dictionary<string, Mesh>();
    static Dictionary<string, Transform>  _transforms_map = new Dictionary<string, Transform>();

    /// <summary>
    /// Combine SkinnedMeshRenderers together and share one skeleton.
    /// Merge materials will reduce the drawcalls, but it will increase the size of memory. 
    /// </summary>
    /// <param name="skeleton">combine meshes to this skeleton(a gameobject)</param>
    /// <param name="meshes">meshes need to be merged</param>
    public static void CombineObject(GameObject skeleton, string key, List<SkinnedMeshRenderer> meshes)
    {
        //
        _CombineMeshCache.TryGetValue(key, out var _Combined);
        if (!_Combined || key == "")
        {
            UnityEngine.Profiling.Profiler.BeginSample("CombineMeshes");
            var combineInstances = MyListPool<CombineInstance>.Get();// new List<CombineInstance>();//the list of meshes
            for (int i = 0, Length = meshes.Count; i < Length; i++)
            {
                SkinnedMeshRenderer smr = meshes[i];
                for (int sub = 0, subMeshCount = smr.sharedMesh.subMeshCount; sub < subMeshCount; sub++)
                {
                    CombineInstance ci = new CombineInstance();
                    ci.mesh = smr.sharedMesh;
                    ci.subMeshIndex = sub;
                    combineInstances.Add(ci);
                }
            }
            _CombineMeshCache[key] = _Combined = new Mesh();
            _Combined.name = key;
            _Combined.CombineMeshes(combineInstances.ToArray(), false, false);// Combine meshes
            MyListPool<CombineInstance>.Release(combineInstances);
            UnityEngine.Profiling.Profiler.EndSample();
        }

        var isprefab = !(skeleton.activeInHierarchy || skeleton.scene.IsValid());

        SkinnedMeshRenderer skeleton_smr = null;
        var cs = skeleton.transform.Find("_bones");
        var c = cs?.Find( key );
        c?.TryGetComponent(out skeleton_smr);
        if (!Application.isEditor && !skeleton_smr)
        {
            Log.LogInfo($"CombineObject {skeleton} -> {key}, cache={skeleton_smr != null}, isprefab={isprefab}");
        }
        List<Transform> bones = MyListPool<Transform>.Get();// new List<Transform>();
        var transforms_map = _transforms_map;
        transforms_map.Clear();
        if (!skeleton_smr)
        {
            UnityEngine.Profiling.Profiler.BeginSample("transforms_map");
            // Fetch all bones of the skeleton        
            skeleton.GetComponentsInChildren(true, bones);
            foreach (var t in bones)
            {
                var name = t.name;
                if (!transforms_map.ContainsKey(name))
                {
                    transforms_map[name] = t;
                }
            }
            UnityEngine.Profiling.Profiler.EndSample();
        }
        bones.Clear();//the list of bones

        var fet_transfters = transforms_map.Count > 0;

        UnityEngine.Profiling.Profiler.BeginSample("Collect bones");
        var sharedMaterials = new Material[meshes.Count];
        for (int i = 0, Length = meshes.Count; i < Length; i++)
        {
            SkinnedMeshRenderer smr = meshes[i];
            // Collect materials
            sharedMaterials[i] = smr.sharedMaterial;
            if (fet_transfters)
            {
                // Collect bones
                var smrbones = smr.bones;
                for (int j = 0, bl = smrbones.Length; j < bl; j++)
                {
                    if (transforms_map.TryGetValue(smrbones[j].name, out var b))
                    {
                        bones.Add(b);
                    }
                }
            }
        }
        transforms_map.Clear();
        UnityEngine.Profiling.Profiler.EndSample();

        if (skeleton_smr && fet_transfters) 
        {
            //测试 代码
            var old_bones = skeleton_smr.bones;
            Log.LogError($"old_bones={old_bones.Length}, bones={bones.Count}");
            for (var i=0;i<old_bones.Length;++i) 
            {
                if (ReferenceEquals(old_bones[i], bones[i]))
                {
                    Log.LogInfo($"{i}, ReferenceEquals");
                }
                else 
                {
                    Log.LogError($"{i}, old_bones={old_bones[i].gameObject.GetLocation()},{old_bones[i].GetHashCode()}, exist={bones.Contains(old_bones[i])}");
                    Log.LogError($"{i}, new_bones={bones[i].gameObject.GetLocation()},{bones[i].GetHashCode()}");
                }
            }
        }

        if (!fet_transfters)
        {
            bones.AddRange(skeleton_smr.bones);
        }

        if (!cs || !isprefab)
        {
            if (cs)
            {
                GameObject.DestroyImmediate(cs.gameObject);
            }
            UnityEngine.Profiling.Profiler.BeginSample("add SkinnedMeshRenderer");
            // Create a new SkinnedMeshRenderer
            //GameObject.DestroyImmediate(skeleton_smr);
            skeleton_smr = skeleton.AddMissingComponent<SkinnedMeshRenderer>();
            skeleton_smr.sharedMaterials = sharedMaterials;
            skeleton_smr.bones = bones.ToArray();// Use new bones
            skeleton_smr.sharedMesh = _Combined;
            UnityEngine.Profiling.Profiler.EndSample();
        }
        else
        {
            UnityEngine.Profiling.Profiler.BeginSample("add SkinnedMeshRenderer");
            if (!c)
            {
                c = new GameObject(key).transform;
                c.parent = cs;
            }
            skeleton_smr = c.gameObject.AddMissingComponent<SkinnedMeshRenderer>();
            skeleton_smr.bones = bones.ToArray();
            UnityEngine.Profiling.Profiler.EndSample();
        }
        //
        //r.gameObject.AddMissingComponent<AutoDestroyBhv>().obj = r.sharedMesh;
        MyListPool<Transform>.Release(bones);
    }

    /// <summary>
    /// 继承 base_sortingOrder 的渲染层级，用于特效显示在UI上时传入界面层级，使特效显示在UI上层
    /// </summary>
    /// <param name="go"></param>
    /// <param name="base_sortingOrder"></param>
    public static void InheritSortingOrder(GameObject go, int base_sortingOrder)
    {
        if (go != null)
        {
            var rendes = go.GetComponentsInChildren<Renderer>();
            if (rendes != null)
            {
                for (int i = 0; i < rendes.Length; i++)
                {
                    rendes[i].sortingOrder += base_sortingOrder;
                }
            }
        }
    }
}
