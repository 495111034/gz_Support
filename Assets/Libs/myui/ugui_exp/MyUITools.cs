using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Object = UnityEngine.Object;
using System.Text.RegularExpressions;


public static class MyUITools
{
    public static IResourceLoader UIResPoolInstans;

    public static event Action<MonoBehaviour> OnStartShowTips;
    public static event Action<MonoBehaviour> OnHideTips;
    public static GameObject roomRoot;

    //以下两个值会在热更新代码LogicApp.Init中重新赋值
    public static int RefScreenWidth = 1920;
    public static int RefScreenHeight = 1080;

    public static CanvasScaler canvasScale;

    //以下对象由热更代码创建与设置
    public static RectTransform UIRoot { get; set; }
    public static RectTransform MainPanelRoot { get; set; }
    public static RectTransform ActivePanelRoot { get; set; }
    public static RectTransform ActivePanel3DRoot { get; set; }
    public static RectTransform DynamicPanelRoot { get;  set; }
    public static RectTransform TopPanelRoot { get;  set; }
    public static RectTransform MaskPanelRoot { get;  set; }
    public static RectTransform TalkPanelRoot { get; set; }
    public static RectTransform TalkMaskRoot { get; set; }
    public static RectTransform JuQingPanelRoot { get; set; }

    public static Camera MainPanelCamera { get; set; }
    public static Camera ActivePanelCamera { get; set; }
    public static Camera ActivePanel3DCamera { get; set; }
    public static Camera DynamicPanelCamera { get; set; }
    public static Camera TopPanelCamera { get; set; }
    public static Camera MaskPanelCamera { get; set; }
    public static Camera TalkPanelCamrea { get; set; }
    public static Camera TalkMaskCamrea { get; set; }
    public static Camera JuQingPanelCamrea { get; set; }


    //以上对象由热更代码创建与设置


    static public void ShowTips(MonoBehaviour obj)
    {
        OnStartShowTips?.Invoke(obj);
    }

    static public void HideTips(MonoBehaviour obj)
    {
        OnHideTips?.Invoke(obj);
    }

    /// <summary>
    /// 获得控件在屏幕上的位置(起点左下)
    /// </summary>
    /// <param name="uiObj"></param>
    /// <returns></returns>
    static public Vector3 GetUIPosition(this GameObject uiObj)
    {
        var canvas = GameObjectUtils.FindInParents<Canvas>(uiObj);
        if (!canvas)
        {
            Log.LogError($"uiObj:{uiObj.name} is not in Canvas");
            return Vector2.zero;
        }

        var camera = canvas.worldCamera;
        if (!camera)
        {
            Log.LogError($"uiObj:{uiObj.name} have not worldCamera");
            return Vector2.zero;
        }

        return camera.WorldToScreenPoint(uiObj.transform.position);

    }



    //
    public static Vector2 GetSize(this RectTransform rt)
    {
        var rc = rt.rect;
        return rc.size;
    }



    public static void SetSize(this RectTransform rt, Vector2 value)
    {
        if (rt.anchorMin == rt.anchorMax)
        {
            if (rt.sizeDelta != value)
            {
                rt.sizeDelta = value;
            }
        }
        else
        {
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value.x);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value.y);
        }
    }

    public static RectTransform GetRectTransform(this GameObject obj)
    {
        //if (!obj.IsUIObject())
        //{
        //    Log.LogError($"gameobject:{obj.name} is not ui object");
        //    return null;
        //}
        if (!obj)
        {
            return null;
        }

        return obj.transform as RectTransform;
    }

    /// <summary>
    /// 填充满
    /// </summary>
    /// <param name="rt"></param>
    public static void SetFullByParent(this RectTransform rt)
    {
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 1);
    }

    /// <summary>
    /// 填充至两边,固定高度,
    /// 例如固定按16:9显示 :FillToBothSide(Mathf.RoundToInt((float)Screen.width / 1.7777778f));
    /// </summary>
    /// <param name="rt"></param>
    /// <param name="height"></param>
    public static void FillToBothSide(this RectTransform rt, float height)
    {
        rt.anchorMin = new Vector2(0, 0.5f);//left,bottom;
        rt.anchorMax = new Vector2(1, 0.5f);//right,top

        if (Screen.height > GetCurrentScreenWidth())
        {
            height = height * (GetCurrentScreenWidth() / (float)Screen.width);
        }

        rt.sizeDelta = new Vector2(0, height);



        //rt.SetSize(new Vector2( GetCurrentScreenWidth(), height));
    }

    /// <summary>
    /// 固定宽度,填充上下
    /// </summary>
    public static void FillToBothUD(this RectTransform rt, float width)
    {
        rt.anchorMin = new Vector2(0.5f, 0);//left,bottom;
        rt.anchorMax = new Vector2(0.5f, 1);//right,top

        if (Screen.height > GetCurrentScreenHeight())
        {
            width = width * (GetCurrentScreenHeight() / (float)Screen.height);
        }

        rt.sizeDelta = new Vector2(width, 0);
    }

    /// <summary>
    /// 设置加载背景图为适当的尺寸
    /// </summary>
    public static void SetBgLoadingToSuiSize(this RectTransform rc)
    {
        if ((int)((Screen.width / (float)Screen.height) * 100) > (int)((RefScreenWidth / (float)MyUITools.RefScreenHeight) * 100))
        {
            rc.FillToBothUD(Mathf.RoundToInt(Screen.height * 1.7777778f));
        }
        else
        {
            rc.FillToBothSide(Mathf.RoundToInt(Screen.width / 1.7777776f));
        }
    }


    /// <summary>
    /// 获取当前的屏幕宽度的UI基准值,在移动设备上此值并不等于实际分辨率
    /// </summary>
    /// <returns></returns>
    public static int GetCurrentScreenWidth()
    {
        if (BuilderConfig.IsMobileDevice)
        {
            return Screen.width > RefScreenWidth ? RefScreenWidth : Screen.width;
        }
        else
        {
            return Screen.width;
        }
    }


    public static int GetCurrentScreenHeight()
    {
        if (BuilderConfig.IsMobileDevice)
        {
            var h = Screen.height > RefScreenHeight ? RefScreenHeight : Screen.height;
            return h;           
        }
        else
        {
            return Screen.height;
        }
    }


    static int __CurrentMathWidthOrHeight = -1;

    public static int GetCurrentMatchWidthOrHeight()
    {
        if (__CurrentMathWidthOrHeight >= 0)
            return __CurrentMathWidthOrHeight;

        if ((int)(((float)Screen.width / (float)Screen.height) * 100) > (int)(((float)RefScreenWidth / (float)RefScreenHeight) * 100))
        {
            __CurrentMathWidthOrHeight = 1;
        }
        else
        {
            __CurrentMathWidthOrHeight = 0;
        }

        return __CurrentMathWidthOrHeight;
    }

    /// <summary>
    /// 将屏幕坐标(起点左下)转换为UI坐标（起点中中）
    /// </summary>
    /// <param name="screenPos">屏幕坐标（基于当前分辨率），起点左下</param>
    /// <returns>UI坐标，基于当前UI设置的缩放，起点中中</returns>
    public static Vector2 GetUIPositionByScreenPos(Vector2 screenPos)
    {
        return GetUIPositionByScreenPos(screenPos.x, screenPos.y);
    }

    public static Vector2 GetUIPositionByScreenPos(float x,float y)
    {
        Vector2 outVec;
        //if (RectTransformUtility.ScreenPointToLocalPointInRectangle(DynamicPanelRoot, new Vector2(Screen.width, Screen.height), DynamicPanelCamera, out outVec))
        //{
        //    Log.LogError($"screen=x={Screen.width},y={Screen.height} uiscreen = {outVec}");
        //}
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(TopPanelRoot, new Vector2(x,y), TopPanelCamera, out outVec))
        {          
            //Log.LogError($"outVec={outVec}");
            return outVec;
        }
        else
        {
            Log.LogError($"Get UI Position error:x={x},y={y}");
        }
        return Vector2.zero;
    }

    static Vector2 uiFullScreenSize = new Vector2(RefScreenWidth, RefScreenHeight);
    static bool HasGetedUIScreenSize = false;
    public static Vector2 GetUIFullScreenSize()
    {
        if (HasGetedUIScreenSize)
        {
            return uiFullScreenSize;
        }
        else
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(DynamicPanelRoot, new Vector2(Screen.width, Screen.height), DynamicPanelCamera, out uiFullScreenSize))
            {
                HasGetedUIScreenSize = true;
                return uiFullScreenSize;
            }
            else
            {
                HasGetedUIScreenSize = false;
            }
        }

        return new Vector2(RefScreenWidth, RefScreenHeight);
    }


    /// <summary>
    /// 标准UI坐标(1024 * 573环境下的坐标)转换为实际缩放坐标，锚点为右下角
    /// </summary>
    /// <param name="standardPos"></param>
    /// <returns></returns>
    public static Vector2 StandardPosToScalePos(Vector2 standardPos, CanvasScaler canvasScaler = null)
    {
        if (canvasScaler == null)
        {
            canvasScaler = canvasScale;
        }

        float offect = 1f;

        offect = (Screen.width / canvasScaler.referenceResolution.x);


        return new Vector2(standardPos.x * offect, standardPos.y * offect);
    }

    /// <summary>
    /// 底部对齐
    /// </summary>
    /// <param name="rt"></param>
    public static void AlignToUnder(this RectTransform rt, float offset = 0, float sizeX = 0, float sizeY = 0)
    {
        rt.anchorMin = new Vector2(0.5f, 0);
        rt.anchorMax = new Vector2(0.5f, 0);

        if (sizeX > 0 && sizeY > 0)
        {
            rt.SetSize(new Vector2(sizeX, sizeY));
        }

        rt.anchoredPosition = new Vector2(0, rt.GetSize().y / 2 + offset);
    }

    public static void AlignToTop(this RectTransform rt, float offset = 0, float sizeX = 0, float sizeY = 0)
    {
        rt.anchorMin = new Vector2(0.5f, 1);
        rt.anchorMax = new Vector2(0.5f, 1);

        if (sizeX > 0 && sizeY > 0)
        {
            rt.SetSize(new Vector2(sizeX, sizeY));
        }

        //var parentHeight = rt.parent ? (rt.parent as RectTransform).GetSize().y : Screen.height;
        rt.anchoredPosition = new Vector2(0, -rt.GetSize().y / 2 - offset);
    }

    /// <summary>
    /// 左对齐
    /// </summary>
    /// <param name="rt"></param>
    public static void AlignToLeft(this RectTransform rt, float offset = 0, float sizeX = 0, float sizeY = 0)
    {
        rt.anchorMin = new Vector2(0, 0.5f);
        rt.anchorMax = new Vector2(0, 0.5f);

        if (sizeX > 0 && sizeY > 0)
        {
            rt.SetSize(new Vector2(sizeX, sizeY));
        }

        rt.anchoredPosition = new Vector2(rt.GetSize().x / 2 + offset, 0);
    }

    /// <summary>
    /// 右对齐
    /// </summary>
    /// <param name="rt"></param>
    public static void AlignToRight(this RectTransform rt, float offset = 0, float sizeX = 0, float sizeY = 0)
    {
        rt.anchorMin = new Vector2(1, 0.5f);
        rt.anchorMax = new Vector2(1, 0.5f);

        if (sizeX > 0 && sizeY > 0)
        {
            rt.SetSize(new Vector2(sizeX, sizeY));
        }

        rt.anchoredPosition = new Vector2(-rt.GetSize().x / 2 - offset, 0);
    }


    static Font _font;

    public static void SetDefaultFont(Font font)
    {
        if (font)
        {
            _font = font;
        }
    }
    public static Font DefaultFont
    {
        get
        {
            if (!_font)
            {
                _font = Resources.Load<Font>("fonts/arial");
            }

            return _font;
        }
    }

    static Texture2D _defaultBigBg;
    public static Texture2D DefaultBigBg
    {
        get
        {
            if (!_defaultBigBg) _defaultBigBg = Resources.Load<Texture2D>("bg/tex_login_bg01");
            if (_defaultBigBg) return _defaultBigBg;
            return null;

        }
    }

    /// <summary>
    /// 获取本地坐标
    /// </summary>
    /// <param name="data"></param>
    /// <param name="t"></param>
    /// <returns></returns>
    public static Vector2 GetLocalPosPress(this PointerEventData data, Transform t)
    {
        var camera = data.pressEventCamera;
        if (camera == null)
        {
            return Vector2.zero;
        }

        var world_pos = camera.ScreenToWorldPoint(data.position);
        var local_pos = t.worldToLocalMatrix.MultiplyPoint3x4(world_pos);
        return local_pos;
    }

    /// <summary>
    /// 获取 Y 向下的坐标
    /// </summary>
    /// <param name="eventData"></param>
    /// <returns></returns>
    public static Vector2 GetPositionYDown(this PointerEventData eventData)
    {
        var pos = eventData.position;
        pos.y = Screen.height - pos.y;
        return pos;
    }


    #region 查找

    /// <summary>
    /// 查找孩子
    ///     . 使用 广度优先 遍历算法
    ///     . 有一定性能开销, 为提高性能:
    ///         . 目标节点层次不要太深
    ///         . 尽量提供详细的路径名
    ///         . 查找多个节点时, 可先查找它们的公共父节点, 然后在该公共节点下查找
    ///     . id 格式
    ///         . "name"                    -- 单个名字
    ///         . "name/name/.../name"      -- 多个名字组合的路径, 越详细性能越好
    /// </summary>
    public static GameObject FindChild(this GameObject go, string id, bool check_visible = false, bool raise_error = true)
    {
        if (string.IsNullOrEmpty(id)) 
        {
            return null;
        }        
        if (go == null)
        {
            if (raise_error)
            {
                Log.LogError("FindChild, go is null");
            }
            return null;
        }
        var t = FindChild(go.transform, id, check_visible, raise_error);
        //Log.LogInfo($"{go.GetLocation()} xFindChild({id}) -> {t}");
        return t != null ? t.gameObject : null;
    }
    public static GameObject FindChild(this RectTransform bhv, string id, bool check_visible = false, bool raise_error = true)
    {
        return bhv.gameObject.FindChild(id, check_visible, raise_error);
    }

    // 查找孩子
    public static T FindChild<T>(this GameObject go, string id, bool check_visible = false, bool raise_error = true) where T : Component
    {
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }
        var t = FindChild(go.transform, id, check_visible, raise_error);
        //Log.LogInfo($"{go.GetLocation()} xFindChild<T>({id}) -> {t}");
        return t != null ? t.GetComponent<T>() : null;
    }
    public static T FindChild<T>(this RectTransform bhv, string id, bool check_visible = false, bool raise_error = true) where T : Component
    {
        return bhv.gameObject.FindChild<T>(id, check_visible, raise_error);
    }

    //判断节点的子节点中有无某个节点（只搜索一层）
    public static bool HaveChild(this GameObject go, string id)
    {
        for (int i = 0; i < go.transform.childCount; i++)
        {
            if (go.transform.GetChild(i).name == id)
            {
                return true;
            }
        }
        return false;
    }
    //删除一个子节点（只搜索一层）
    public static void DelChild(this GameObject go, string id)
    {
        var trans = go.transform;
        for (int i = 0; i < trans.childCount; i++)
        {
            var child = trans.GetChild(i);
            if (child.name == id)
            {
                GameObjectUtils.Destroy(child.gameObject);
            }
        }
    }

    // 根据 ID 查找
    static Transform FindChild(Transform t, string id, bool check_visible, bool raise_error)
    {
        
        //Transform root = t;
        if (string.IsNullOrEmpty(id))
        {
            return null;
        }

        if (check_visible && !t.IsActive())
        {
            return null; // 不可见
        }

        if (id == ".")
        {
            return t;        // 自己
        }

        if (t.name.StartsWith('[') && t.TryGetComponent<SceneRootUniqueChild>(out var scenevar))
        {
            if (scenevar.UniqueChild.TryGetValue(id, out var c))
            {
                return c;
            }
        }

        var t1 = t.Find(id);
        if (t1)
        {
            return t1;
        }
        // 用 / 分割路径
        if (id.Contains('/'))
        {
            var arr = id.Split('/');
            var p1 = FindChildRecursively(t.gameObject, arr[0], check_visible);
            if (p1)
            {
                var left = arr.Length == 2 ? arr[1] : string.Join('/', arr, 1, arr.Length - 1);
                p1 = p1.transform.Find(left)?.gameObject;
            }
            if (!p1)
            {
                //if (raise_error)
                {
                    if (Application.isEditor)
                    {
                        Log.LogError($"xFindChild({id}) failed, root={t.gameObject.GetLocation()}");
                    }
                    else
                    {
                        Log.LogWarning($"xFindChild({id}) failed, root={t.gameObject.GetLocation()}");
                    }
                }
                return null;
            }
            return p1.transform;
        }

        // 递归查找
        UnityEngine.Profiling.Profiler.BeginSample("FindChildRecursively");
        var go = FindChildRecursively(t.gameObject, id, check_visible);
        UnityEngine.Profiling.Profiler.EndSample();
        if (go == null)
        {
            if (raise_error)
            {
                if (Application.isEditor)
                {
                    Log.LogError($"xFindChild({id}) failed, root={t.gameObject.GetLocation()}");
                }
                else 
                {
                    Log.LogWarning($"xFindChild({id}) failed, root={t.gameObject.GetLocation()}");
                }
            }
            return null;
        }
        return go.transform;
    }

    public static Transform FindChildTopLevel(Transform go, string name, bool check_active = false)
    {
        if (!go || check_active && !go.gameObject.activeInHierarchy) return null;
        //if (go.name == name)
        //{
        //    return go;
        //}        
        for (int i = 0, count = go.childCount; i < count; ++i)
        {
            var c = go.GetChild(i);
            if (c.name == name && (!check_active || c.gameObject.activeInHierarchy))
            {
                return c;
            }
        }
        return null;
    }

    public static Func<string, bool> FindChildUseCachePath;
    static Dictionary<string, string> _id2fullpath = new Dictionary<string, string>();
    static Transform[] s_tbuf = new Transform[1024];
    // 递归搜索名字(例如查找骨骼名)
    public static GameObject FindChildRecursively(this GameObject go, string name, bool check_active = false)
    {
        if (!go || check_active && !go.activeInHierarchy) return null;
        var root = go;
        {
            var got = go.transform;
            if (got.childCount == 0)
            {
                return null;
            }
            if (got.GetChild(0).name == "_meshs")
            {
                got = got.GetChild(0);
                go = got.gameObject;
                if (got.childCount == 0)
                {
                    return null;
                }
            }
        } 

        var goname = go.name;
        if (goname == "_meshs")
        {
            go = go.transform.GetChild(0).gameObject;
            goname = go.name;
        }

        UnityEngine.Profiling.Profiler.BeginSample("FindChild ObjRefBehaviour");
        var bret = go.TryGetComponent<ObjRefBehaviour>(out var refs);
        var dict = (bret && refs) ? refs.GetRef<Dictionary<string, GameObject>>() : null;
        if (bret && refs)
        {
            if (dict == null)
            {
                dict = new Dictionary<string, GameObject>();
                refs.AddRef(dict);
            }
            if (dict.TryGetValue(name, out var ret))
            {
                UnityEngine.Profiling.Profiler.EndSample();
                return ret;
            }
        }
        UnityEngine.Profiling.Profiler.EndSample();
        //
        string cache_id = null;
        if (!check_active && (FindChildUseCachePath == null || FindChildUseCachePath(goname)))
        {
            //从ab实例化的 go，具有固定的节点，使用缓存
            if (goname.StartsWith("fx_") || goname.StartsWith("part_") || goname.StartsWith("prefab_") || goname.EndsWith("_panel") || (goname.EndsWith(')') && goname.Contains("_panel")))
            {
                UnityEngine.Profiling.Profiler.BeginSample("FindChild cachepath");
                UnityEngine.Profiling.Profiler.BeginSample("gen cache_id key");
                //if (goname.StartsWith("part_") && goname.EndsWith("_lod"))
                //{
                //    goname = goname.Substring(0, goname.Length - 4);
                //}
                if (goname.EndsWith(')'))
                {
                    var idx = goname.LastIndexOf('(');
                    if (idx > 0)
                    {
                        if (goname[idx - 1] == ' ')
                        {
                            --idx;
                        }
                        goname = goname.Substring(0, idx);
                    }
                }
                cache_id = $"{goname}|{name}";
                UnityEngine.Profiling.Profiler.EndSample();
                if (_id2fullpath.TryGetValue(cache_id, out var fullpath))
                {
                    UnityEngine.Profiling.Profiler.BeginSample("use cache_id");
                    var g1 = fullpath == null ? null : go.transform.Find(fullpath)?.gameObject;
                    UnityEngine.Profiling.Profiler.EndSample();
                    if (g1 || fullpath == null)
                    {
                        if (dict != null)
                        {
                            dict[name] = g1;
                        }
                        UnityEngine.Profiling.Profiler.EndSample();
                        return g1;
                    }
                    Log.LogError($"{root.name} -> {go.GetLocation()} -> Error xFindChild fail by fullpath {fullpath} ");
                }
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }
        UnityEngine.Profiling.Profiler.BeginSample("FindChild Recursively");
        var buf = s_tbuf;
        var max = buf.Length;

        int n = 0;
        buf[n++] = go.transform;
        while (n > 0)
        {
            Transform t = buf[--n]; buf[n] = null;            
            for (var i = t.childCount - 1; i >= 0; --i)
            {
                var c = t.GetChild(i);
                if (!check_active || c.gameObject.activeInHierarchy)
                {
                    if (c.name == name)
                    {
                        if (n > 0)
                        {
                            Array.Clear(buf, 0, n);
                        }
                        UnityEngine.Profiling.Profiler.EndSample();
                        if (dict != null)
                        {
                            dict[name] = c.gameObject;
                        }
                        if (cache_id != null) 
                        {
                            UnityEngine.Profiling.Profiler.BeginSample("gen cache_id");
                            var paths = MyListPool<string>.Get();
                            var tmp = c;
                            while (tmp.gameObject != go)
                            {
                                paths.Add(tmp.name);
                                tmp = tmp.parent;
                            }
                            paths.Reverse();
                            var fullpath = string.Join('/', paths);
                            MyListPool<string>.Release(paths);
                            UnityEngine.Profiling.Profiler.EndSample();
#if UNITY_EDITOR
                            UnityEngine.Profiling.Profiler.BeginSample("check cache_id");
                            var check = go.transform.Find(fullpath);
                            UnityEngine.Profiling.Profiler.EndSample();
                            if (check != c)
                            {
                                Log.LogError($"{root.name} -> {go.GetLocation()} Error xFindChild {fullpath} got={c.gameObject.GetLocation()} not match check={(check ? check.gameObject.GetLocation() : "")}");
                            }
                            if (_id2fullpath.TryGetValue(cache_id, out var old)) 
                            {
                                Log.LogError($"{root.GetLocation()} Error xFindChild change fullpath from {old} -> {fullpath}");
                            }
                            Log.LogInfo($"{go.GetLocation()} xFindChild {cache_id} -> {fullpath}");
#endif
                            _id2fullpath[cache_id] = fullpath;                            
                        }
                        return c.gameObject;
                    }
                    if (n == max)
                    {
                        Log.LogError($"gameobject too large");
                        break;
                    }
                    buf[n++] = c;
                }
            }
        }
        UnityEngine.Profiling.Profiler.EndSample();
        Log.LogInfo($"{root.name} -> {go.GetLocation()} xFindChildW({name}) not found");
        if (dict != null)
        {
            dict[name] = null;
        }
        if (cache_id != null) 
        {
            _id2fullpath[cache_id] = null;
        }
        return null;
    }

    #endregion


    /// <summary>
    /// 模拟点击按钮
    /// </summary>
    /// <param name="go"></param>
    public static void PointerClick(GameObject go, string pointer_name = "OnPointerClick")
    {
        if (go == null)
        {
            Debug.LogError("PointerClick is null");
            return;
        }
        var data = new PointerEventData(EventSystem.current);
        var canvs = go.GetComponentInParent<Canvas>();
        if (canvs != null && canvs.worldCamera != null)
        {
            data.position = RectTransformUtility.WorldToScreenPoint(canvs.worldCamera, go.transform.position);
        }
        else if (Camera.main != null)
        {
            data.position = RectTransformUtility.WorldToScreenPoint(Camera.main, go.transform.position);
        }
        go.SendMessage(pointer_name, new PointerEventData(EventSystem.current));
    }

    /// <summary>
    /// 通过输入屏幕坐标完全模拟按下或抬起操作
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="pressed"></param>
    public static void PointerClickEvent(Vector2 pos, bool pressed)
    {
        var inputModule = EventSystem.current.currentInputModule as MyStandaloneInputModule;
        Input.simulateMouseWithTouches = true;

        var methodGetTouch = inputModule.GetType().GetMethod("GetTouchPointerEventData", BindingFlags.Instance | BindingFlags.NonPublic);

        var touch = new Touch()
        {
            position = pos,
        };

        var pointerData = methodGetTouch.Invoke(inputModule, new object[] { touch, false, false });

        var methodProcess = inputModule.GetType().GetMethod("ProcessTouchPress", BindingFlags.Instance | BindingFlags.NonPublic);

        methodProcess.Invoke(inputModule, new object[] { pointerData, pressed, !pressed });
    }

    /// <summary>
    /// 获取当前鼠标位置射线的第一个对象
    /// </summary>
    /// <returns></returns>
    public static GameObject GetCurrentRayGameObject()
    {
        var data = new PointerEventData(EventSystem.current);
        data.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);
        if (results.Count == 0) return null;
        return results[0].gameObject;
    }

    /// <summary>
    /// 获取当前屏幕坐标射线第一个可点击对象
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public static GameObject GetPointerObjByScenePos(Vector3 position)
    {
        var data = new PointerEventData(EventSystem.current);
        data.position = position;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);
        if (results.Count == 0) return null;
        return results[0].gameObject;
    }

    public static void PointerClickByScenePos(Vector3 position, string pointer_name = "OnPointerClick")
    {
        var data = new PointerEventData(UnityEngine.EventSystems.EventSystem.current);
        data.position = position;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);
        if (results.Count == 0) return;
        PointerClick(results[0].gameObject, pointer_name);
    }

    public static void SetGray(GameObject go, bool isGray)
    {
        var texts = go.GetComponentsInChildren<MyText>();
        for (int i = 0; i < texts.Length; i++)
        {
            texts[i].IsFade = isGray;
        }
        var imgs = go.GetComponentsInChildren<MySpriteImage>();
        for (int i = 0; i < imgs.Length; i++)
        {
            imgs[i].IsFade = isGray;
        }
    }


    public static Regex zhn = new Regex("[^ ]*[\u4e00-\u9fa5]+[^ ]*");
    public static string BetterChineseString(string text) 
    {
        //return text.Replace(' ', '\u00A0') + "XXX";
        if (!text.Contains(' '))
        {
            return text;
        }
        var matchs = zhn.Matches(text);
        if (matchs == null && matchs.Count == 0) 
        {
            return text;
        }
        string newstr = null;
        unsafe
        {
            int lasti = -1;
            char* ptr = null;
            foreach (Match match in matchs)
            {
                var i = text.LastIndexOf(' ', match.Index, match.Index - lasti);
                if (i > lasti)
                {
                    lasti = i;
                    if (newstr == null)
                    {
                        newstr = new string(text);
                        fixed (char * ptr2 = newstr) 
                        {
                            ptr = ptr2;
                        }
                    }
                    ptr[i] = '\u00A0';
                }
            }
        }
        return newstr ?? text;
    }

    //文本过长，最后用"…"代替
    public static void SetTextWithEllipsis(MyText textComponent, string value)
    {
        value = value.Replace(' ', '\u00A0');
        var generator = new TextGenerator();
        var rectTransform = textComponent.GetComponent<RectTransform>();
        var settings = textComponent.GetGenerationSettings(rectTransform.rect.size);
        generator.Populate(value, settings);
        var characterCountVisible = generator.characterCountVisible;
        var updatedText = value;
        if (value.Length > characterCountVisible)
        {
            updatedText = value.Substring(0, characterCountVisible - 1);
            updatedText += "…";
        }
        textComponent.text = updatedText;
    }
}