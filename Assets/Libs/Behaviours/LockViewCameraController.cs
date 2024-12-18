//#define TEST_INPUT
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
using UnityEditor; 
#endif


#if UNITY_EDITOR
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class ShowAsFlagsAttribute : PropertyAttribute
{
}
[CustomPropertyDrawer(typeof(ShowAsFlagsAttribute))]
public class ShowAsFlagsDrawer : PropertyDrawer
{
    private MethodInfo _miIntToEnumFlags;
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // 若是不是枚举，则按默认显示
        if (property.propertyType != SerializedPropertyType.Enum)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        //property.intValue = EditorGUI.MaskField(position, label, property.intValue, property.enumDisplayNames);
        if (_miIntToEnumFlags == null)
        {
            var type = Type.GetType("UnityEditor.EnumDataUtility,UnityEditor");
            _miIntToEnumFlags = type.GetMethod("IntToEnumFlags", BindingFlags.Static | BindingFlags.NonPublic);
        }

        // 复杂的转换问题，让Unity来解决（参考EditorGUI.EnumFlagsField()方法的反编译结果）
        Enum currentEnum = (Enum)_miIntToEnumFlags.Invoke(null, new object[] { fieldInfo.FieldType, property.intValue });
        EditorGUI.BeginProperty(position, label, property);
        Enum newEnum = EditorGUI.EnumFlagsField(position, label, currentEnum);
        property.intValue = Convert.ToInt32(newEnum);
        EditorGUI.EndProperty();

        // 备注：
        // 不能使用如下方式获取枚举值：
        // Enum currentEnum = (Enum)fieldInfo.GetValue(property.serializedObject.targetObject);
        // 使用如下方式时，若是ScriptableObject中包含一个某类型的数组，该类型中包含了Flags枚举，将会致使Editor抛出ArgumentException：
        // ArgumentException: Field <enum_flags> defined on type <host_type> is not a field on the target object which is of type <unity_object>.
    }
}
#endif

/// <summary>
/// 锁定视角的相机控制器
/// </summary>
public class LockViewCameraController : MonoBehaviour
{
    public Transform target;// { set; private get; }
    [NonSerialized]
    public Vector3 target_pos = Vector3.zero;
    
    
    public float procector_offset = 0;

    [NonSerialized]
    public float PauseTime;

    public bool stop_follow = false;
    [Range(1, 90)]
    public float angle_y = 55.4f;       // Y轴 角度
    [Range(0, 360)]
    public float angle_xz = 232f;      // XZ平面 轴旋转角度
    [Range(1, 100)]
    public float distance = 14.4f;      // 和目标的距离
    [Range(0, 2)]
    public float distance_angle = 1.6f;

    [Range(0, 5)]
    public float procector_freq = 0.5f;

    public float distance_default { get; set; } = 40;
    public float distance_min_clamp { get; set; } = 1;
    public float distance_max_clamp { get; set; } = 100;

    public float real_distance { get; private set; }

    //[range(1, 100)]
    //public float ProjectorShadow_dist = 12;
    public float FollowFastEndTime;

    [Range(1f, 100f)]
    public float follow_speed = 40f;   // 移动相机的速度, 米/秒
    public float offset_y = 1f;       // 相机对准脚还是对准屁股
    float _start_time;
    float _need_time;
    Vector3 _last_tp;
    float _offset_y, _angle_y, _angle_xz, _distance, _distance_angle;
    Vector3 _dt;

    [Range(1, 100)]
    public float follow_back_time = 15;
    [Range(0, 90)]
    public float follow_min_angle = 40;
    static float _follow_y = 0;

    [Range(0.5f, 5)]
    public float FollowFightTimeCD = 1;         //战斗背追时间
    [Range(40, 150)]
    public float FollowFightAngleStart = 150;    //战斗背追起始角度
    //[Range(1, 240)]
    //public int fps = -1;

    [System.Flags]
    public enum Shock_Type
    {
        shock_none = 0,
        shock_x = 1,
        shock_y = 2,
        shock_z = 4,
    }
    [Range(1, 300f)]
    public float shock_speed = 40;
    [Range(0, 1f)]
    public float shock_time = 0.15f;
    [Range(0, 1f)]
    public float shock_intensity = 0.5f;
#if UNITY_EDITOR && !UNITY_IPHONE
    [ShowAsFlags]
#endif
    public Shock_Type shock_type = Shock_Type.shock_y | Shock_Type.shock_z;

    public bool force_refresh = false;

    public bool show_follow_log;

    static LockViewCameraController Instance;

    public float slow_follow_distance_sqr = 100; //超出该距离，则直接设置位置

    public bool DontUpdateProjectorPos = false;

    public bool LockAngle = false;

    private void Awake()
    {
        Instance = this;
        procector_offset = 0;
        real_distance = distance;
        //fps = Application.targetFrameRate;
    }

    ProjectorShadow ProjectorShadow_aoi, ProjectorShadow_scene;
    public void SetProjectorShadow(ProjectorShadow aoi, ProjectorShadow scene)
    {
        var dis = distance_min_clamp * (1f - (angle_y / 180) * distance_angle);

        ProjectorShadow_aoi = aoi;
        ProjectorShadow_aoi.SetDis(dis);
        ProjectorShadow_scene = scene;
        ProjectorShadow_scene.SetDis(dis);

        var DL = GameObject.Find("DL");
        if (DL == null)
        {
            DL = GameObject.Find("[Light]");
            if (DL != null)
            {
                var DL_trans = DL.transform.Find("DL");
                if (DL_trans != null)
                {
                    DL = DL_trans.gameObject;
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }
        }
        aoi.transform.forward = DL.transform.forward;
        scene.transform.forward = DL.transform.forward;
    }
    void _check_dirty()
    {
        if (_offset_y != offset_y || _angle_y != angle_y || _angle_xz != angle_xz || _distance != distance || distance_angle != _distance_angle || force_refresh)
        {
            force_refresh = false;
            if (distance < 0.1f)
            {
                distance = 0.1f;
            }
            //ui.panel.BasePanel.__dy_root.localScale = Vector3.one * (distance > 15? 15 / distance : 1);
            if (offset_y != _offset_y)
            {
                if (offset_y > _offset_y)
                {
                    float step = Mathf.Max(0.05f, (offset_y - _offset_y) / 10);
                    _offset_y = Mathf.Min(_offset_y + step, offset_y);
                }
                else
                {
                    float step = Mathf.Max(0.05f, (_offset_y - offset_y) / 10);
                    _offset_y = Mathf.Max(_offset_y - step, offset_y);
                }
            }
             _angle_y = angle_y; _angle_xz = angle_xz; _distance = distance; _distance_angle = distance_angle;
             //Log.Log2File($"LockViewCameraController: _offset_y = {offset_y}; _angle_y = {angle_y}; _angle_xz = {angle_xz}; _distance = {distance};");

             var targetpos = this.target ? this.target.position : this.target_pos;

            real_distance = distance * (1f - 0.28f * distance_angle);
            //var fixed_distance = distance * (1f - 2 * Mathf.Pow(angle_y / 180, 2) * distance_angle);

            OnDistanceChagne?.Invoke(real_distance);

            var off_y = real_distance * Mathf.Cos(angle_y * Mathf.Deg2Rad);
            var radius = real_distance * Mathf.Sin(angle_y * Mathf.Deg2Rad);
            var off_x = radius * Mathf.Cos(angle_xz * Mathf.Deg2Rad);
            var off_z = radius * Mathf.Sin(angle_xz * Mathf.Deg2Rad);
            _dt = new Vector3(off_x, off_y, off_z);

            transform.position = targetpos + _dt;
            //摄像机对准 坐标
            targetpos.y += _offset_y;
            transform.forward = targetpos - transform.position;
            ChangeCameraFrame = TimeUtils.frameCount;
            if (false)
            {
                var dlplay = GameObject.Find("dlplay");
                if (dlplay)
                {
                    dlplay.transform.forward = transform.forward;
                    dlplay.transform.position = new Vector3(0, 50, 0);
                }
            }
        }
    }
    //float _move_ProjectorShadow_time;
    //float _touch_y = -1;
    MyTask _task = new MyTask(true, "LockViewCameraController");

    List<Renderer> hit_objs = new List<Renderer>();
    List<Renderer> hide_objs = new List<Renderer>();
    RaycastHit[] hits = new RaycastHit[32];

    float hit_dirty = 0f;

    //分支等APK全替换后才能调用
    public void SetHitDirty(float dirty)
    {
        this.hit_dirty = dirty;
    }

    private void Update()
    {
        _ApplyUpdate();
    }

    Vector3 _follow_back_pos;
    void _follow_back()
    {
        if (target)
        {
            if (!LockAngle)
            {
                var isfight = Time.time - _FollowFightTime <= FollowFightTimeCD;
                if (isfight || ((target.position != _follow_back_pos) && (_FollowDirection || Time.time - _FollowDirectionTime < 0.5f)))
                {
                    if (isfight)
                    {
                        FollowBackUpdate(true);
                    }
                    else
                    {
                        var ret = Func_FollowDirection != null ? Func_FollowDirection() : -999;
                        var b = ret > 0;
                        if (show_follow_log)
                        {
                            Log.LogInfo($"xlock, b={b} <- {_FollowDirection}, isfight={isfight}, Func_FollowDirection={Func_FollowDirection} return {ret}");
                        }
                        if (b)
                        {
                            FollowBackUpdate(false);
                        }
                        else
                        {
                            if (Time.time - _FollowDirectionTime > 0.5f)
                            {
                                _FollowDirection = false;
                                _FollowDirectionTime = 0;
                            }
                        }
                    }
                }
                else
                {
                    if (show_follow_log)
                    {
                        Log.LogInfo($"xlock, isfight={isfight}, target.position={target.position}, _follow_back_pos={_follow_back_pos}, _FollowDirection={_FollowDirection}, Time.time - _FollowDirectionTime={Time.time - _FollowDirectionTime}");
                    }
                }
            }
            //
            _follow_back_pos = target.position;
        }
        else 
        {
            if (show_follow_log)
            {
                Log.LogInfo($"xlock, no target");
            }
        }
    }

    public void FollowBackUpdate(bool isfight, bool isImmediately = false)
    {
        while (angle_xz < 0)
        {
            angle_xz += 360;
        }
        while (angle_xz >= 360)
        {
            angle_xz -= 360;
        }

        var y = target.eulerAngles.y;
        if (isfight)
        {
            y = _FollowFightDir;
        }
        while (y < 0)
        {
            y += 360;
        }
        while (y >= 360)
        {
            y -= 360;
        }

        if (!isfight && !isImmediately && _follow_y != y)
        {
            //防抖, 主角正在转身，等待转身稳定下来再背追
            if (show_follow_log)
            {
                Log.LogInfo($"xlock, 防抖, _follow_y={_follow_y}, y={y}, isfight={isfight}, _FollowDirection={_FollowDirection}, _FollowDirectionTime={_FollowDirectionTime}");
            }
            _follow_y = y;
            if (_FollowDirection)
            {
                _FollowDirectionTime = Time.time;
            }
        }
        else
        {
            var expect_angle_xz = 270 - y;
            while (expect_angle_xz < 0)
            {
                expect_angle_xz += 360;
            }
            while (expect_angle_xz >= 360)
            {
                expect_angle_xz -= 360;
            }
            var e1 = expect_angle_xz - 360;
            var e2 = expect_angle_xz + 360;
            if (Mathf.Abs(e1 - angle_xz) < Mathf.Abs(expect_angle_xz - angle_xz))
            {
                expect_angle_xz = e1;
            }
            if (Mathf.Abs(e2 - angle_xz) < Mathf.Abs(expect_angle_xz - angle_xz))
            {
                expect_angle_xz = e2;
            }

            bool toosmall = false;
            if (Time.time - _FollowDirectionTime < 0.1f || isfight)
            {
                var xz_dt = Mathf.Abs(expect_angle_xz - angle_xz);
                if (xz_dt < follow_min_angle)
                {
                    toosmall = true;
                }
            }
            if (!toosmall)
            {
                var follow_back_time = Mathf.Max(1, this.follow_back_time);
                if (isfight)
                {
                    follow_back_time /= 3f;
                }
                if (angle_xz > expect_angle_xz)
                {
                    follow_back_time /= 1 + Mathf.Pow((angle_xz - expect_angle_xz) / 80f, 3);
                    if (show_follow_log)
                    {
                        Log.LogInfo($"xlock, --angle_xz={angle_xz} -> {expect_angle_xz}, y={y}");
                    }
                    angle_xz -= (isImmediately ? 1 : Time.deltaTime) * 360 / follow_back_time;
                    if (angle_xz < expect_angle_xz)
                    {
                        angle_xz = expect_angle_xz;
                    }
                }
                if (angle_xz < expect_angle_xz)
                {
                    follow_back_time /= 1 + Mathf.Pow((expect_angle_xz - angle_xz) / 80f, 3);

                    if (show_follow_log)
                    {
                        Log.LogInfo($"xlock, ++angle_xz={angle_xz} -> {expect_angle_xz}, y={y}");
                    }
                    angle_xz += (isImmediately ? 1 : Time.deltaTime) * 360 / follow_back_time;
                    if (angle_xz > expect_angle_xz)
                    {
                        angle_xz = expect_angle_xz;
                    }
                }
            }
            else
            {
                if (show_follow_log)
                {
                    Log.LogInfo($"xlock, toosmall, Time.time={Time.time}, _FollowDirectionTime={_FollowDirectionTime}, isfight={isfight}, expect_angle_xz={expect_angle_xz}, angle_xz={angle_xz}, follow_min_angle={follow_min_angle}");
                }
                _FollowDirectionTime = Time.time;
            }
        }
    }

    public void ImmediatelyFollowBack()
    {
        float temp_back_time = follow_back_time;
        follow_back_time = 2;
        FollowBackUpdate(false, true);
        follow_back_time = temp_back_time;
    }

    //Vector3 _last_pos1;
    public void ApplyUpdate()
    {
        if (enabled)
        {
            return;
        }
        _ApplyUpdate();
    }
    void _ApplyUpdate() 
    { 
        //if (fps != Application.targetFrameRate)
        //{
        //    Application.targetFrameRate = fps;
        //    Log.Log2File($"change Application.targetFrameRate={fps}");
        //}
        if (target && target.position.x >= 10000) 
        {
            target = null;
        }
        var tp = target_pos = target ? target.position : target_pos;

        if (!DontUpdateProjectorPos)
        {
            //if (this.ProjectorShadow_aoi && this.ProjectorShadow_aoi.enabled)
            {
                var aoi = this.ProjectorShadow_aoi;
                if (aoi && aoi.enabled && aoi.transform.childCount > 0)
                {
                    var cam = aoi.transform.GetChild(0);
                    if (cam.localPosition != Vector3.zero)
                    {
                        aoi.transform.position = cam.position;
                        cam.localPosition = Vector3.zero;
                    }
                    var p = tp - aoi.transform.forward * (aoi.mProjectorSize * (1f + procector_offset));
                    if ((cam.position - p).sqrMagnitude > 2)
                    {
                        cam.position = p;
                    }
                }
            }
            {
                var scene = this.ProjectorShadow_scene;
                if (scene && scene.enabled && scene.transform.childCount > 0)
                {
                    var cam = scene.transform.GetChild(0);
                    if (cam.localPosition != Vector3.zero)
                    {
                        scene.transform.position = cam.position;
                        cam.localPosition = Vector3.zero;
                    }
                    //
                    var p = tp - scene.transform.forward * (scene.mProjectorSize * (2f + procector_offset));
                    if ((cam.position - p).sqrMagnitude > 4)
                    {
                        cam.position = p;
                    }
                }
            }
        }

        if (stop_follow)// || _task.IsRunning
        {
            if (show_follow_log) 
            {
                Log.LogInfo("xlock, stop_follow");
            }
#if UNITY_EDITOR
            tp.y += offset_y;
            Debug.DrawLine(transform.position, tp, Color.green);
#endif
            return;
        }

        _follow_back();

        _check_dirty();
        //var dirty = false;

        var dst = tp + _dt;
        if (transform.position != dst)
        {
            if (this.target)
            {
                //target_pos = dst;
                if (_last_tp != tp)
                {
                    _last_tp = tp;
                    _start_time = Time.time - Time.deltaTime;
                    if (!target || FollowFastEndTime > Time.time || (transform.position - dst).sqrMagnitude > slow_follow_distance_sqr)
                    {
                        _need_time = 0;
                    }
                    else
                    {
                        _need_time = (dst - transform.position).magnitude / Mathf.Max(follow_speed, 0.01f);
                        if (_need_time > 0.5f)
                        {
                            _need_time = 0.5f;
                        }
                    }
                }
                var passed = (Time.time - _start_time) / Mathf.Max(_need_time, 0.01f);
                //Log.LogError($"tp={tp}, _need_time={_need_time}, passed={passed}");
                if (passed < 1)
                {
                    dst = Vector3.Lerp(transform.position, dst, passed);
                }
            }
            else 
            {
                //Log.Log2File($"ctrl set pos={dst}, target_pos={target_pos}");
            }

            transform.position = dst;
            ChangeCameraFrame = TimeUtils.frameCount;

            if (hit_dirty == 0)
            {
                hit_dirty = Time.time;
            }
        }

#if UNITY_EDITOR
        tp.y += offset_y;
        Debug.DrawLine(transform.position, tp, Color.green);
#endif
        //Log.LogInfo($"check HasTouchGameObject={UnityEngine.EventSystems.MyStandaloneInputModule.CurrentFoceObject}");
        //if (!UnityEngine.EventSystems.MyStandaloneInputModule.CurrentDragGameObject && Time.time - PauseTime > 0.5f)
        {
            if (ChangeCameraView()) 
            {
                if (hit_dirty == 0)
                {
                    hit_dirty = Time.time;
                }
            }
        }

        if (hit_dirty > 0 && Time.time - hit_dirty > 0.5f)
        {
            hit_dirty = 0f;

            var hit_objs = this.hit_objs;
            var hide_objs = this.hide_objs;
            var hits = this.hits;
            var dir = tp - transform.position;

            hit_objs.Clear();
            //var num0 = Physics.RaycastNonAlloc(transform.position, -dir, hits, dir.magnitude + 3f, (int)(ObjLayerMask.HeightTest | ObjLayerMask.Default));
            var num = Physics.RaycastNonAlloc(transform.position, dir, hits, dir.magnitude - 0.1f, (int)(ObjLayerMask.SceneBaseObj | ObjLayerMask.Default));
            if (num == 0) 
            {
                num = Physics.RaycastNonAlloc(tp, -dir, hits, dir.magnitude - 0.1f, (int)(ObjLayerMask.SceneBaseObj | ObjLayerMask.Default));
            }
            //Log.LogError($"num={num}");
            for (var i = 0; i < num; ++i)
            {
                var rd = hits[i].transform.GetComponent<Renderer>();
                if (rd)
                {
                    rd.enabled = false;
                    hit_objs.Add(rd);
                }
            }
            foreach (var hide in hide_objs)
            {
                if (hide && !hit_objs.Contains(hide))
                {
                    hide.enabled = true;
                }
            }
            hide_objs.Clear();

            this.hide_objs = hit_objs;
            this.hit_objs = hide_objs;
        }

#if TEST_INPUT
        test_input();
#endif


    }


#if TEST_INPUT
    string[] names = new string[] { "Mouse X", "Mouse Y", "Mouse ScrollWheel", "Vertical", "Horizontal" };
    void test_input()
    {
        if (Input.touchCount > 0)
        {
            Log.Log2File($"Input.touchCount={Input.touchCount}, mouse={Input.mousePresent},{Input.mousePosition},{Input.mouseScrollDelta},ctrl={Input.GetKey(KeyCode.LeftControl)},{Input.GetKey(KeyCode.Z)}");
        }
        for (var i = 0; i <= 5; i++)
        {
            if (Input.GetMouseButtonDown(i))
            {
                Log.Log2File($"Input.GetMouseButtonDown({i})");
            }
            if (Input.GetMouseButton(i))
            {
                Log.Log2File($"Input.GetMouseButton({i})");
            }
            if (Input.GetMouseButtonUp(i))
            {
                Log.Log2File($"Input.GetMouseButtonUp({i})");
            }
        }
        foreach (var name in names)
        {
            var f = Input.GetAxis(name);
            if (f != 0)
            {
                Log.Log2File($"Input.GetAxis({name})={f}");
            }
        }
    }
#endif

    [HideInInspector]
    [NonSerialized]
    public int ChangeCameraFrame;

    float magnitude;
    Vector2 mousepos;
    //
    public static Dictionary<int, PointerEventData> active_drag_ids = new Dictionary<int, PointerEventData>();
    public static List<int> touche_ids = new List<int>();
    public static event Action<float> OnDistanceChagne;
    public static Func<int> Func_FollowDirection;
    static float _FollowDirectionTime = 0f;
    static bool _FollowDirection = false;   //是否 背追 角色的朝向
    static float _FollowFightTime = 0;      //战斗 背追 时间
    static float _FollowFightDir = 0;       //战斗 背追 朝向


    public static void AddRemovePointerData(PointerEventData eventData, bool is_add)
    {
        if (is_add)
        {
            active_drag_ids[eventData.pointerId] = eventData;
            if (!touche_ids.Contains(eventData.pointerId))
            {
                touche_ids.Add(eventData.pointerId);
            }
        }
        else
        {
            if (active_drag_ids.ContainsKey(eventData.pointerId))
            {
                active_drag_ids.Remove(eventData.pointerId);
            }
            if (touche_ids.Contains(eventData.pointerId))
            {
                touche_ids.Remove(eventData.pointerId);
            }
        }
    }

    public static void SetFollowDirection(bool b, float face_dir)
    {
        if (Instance.show_follow_log)
        {
            Log.LogInfo($"xlock, b={b} <- {_FollowDirection}, face_dir={face_dir}");
        }

        if (face_dir >= 0)
        {
            var expect_angle_xz = 270 - face_dir;
            //夹角
            var dt = Instance.angle_xz - expect_angle_xz;
            var dt0 = dt;

            while (dt < 0)
            {
                dt += 360;
            }
            while (dt > 360)
            {
                dt -= 360;
            }
            //锐角
            if (dt > 180)
            {
                dt = 360 - dt;
            }
            //
            if (dt < Instance.FollowFightAngleStart)
            {
                b = _FollowDirection;
                face_dir = -1;
            }
            if (Instance.show_follow_log)
            {
                Log.LogInfo($"xlock, b={b}, face_dir={face_dir}, expect_angle_xz={expect_angle_xz}, angle_xz={Instance.angle_xz}, dt0={dt0}, dt={dt}");
            }
        }

        if (_FollowDirection != b)
        {
            _follow_y = -1;
            _FollowDirection = b;
            _FollowDirectionTime = Time.time;
        }
        if (face_dir >= 0)
        {
            _FollowFightTime = Time.time;
            _FollowFightDir = face_dir;
        }
    }

    bool ChangeCameraView()
    {

        bool flag = false;

#if UNITY_EDITOR
        if (!Input.GetKey(KeyCode.Space))
#else
        if (Input.touchCount == 0)
#endif
        {
#if !(UNITY_STANDALONE || UNITY_EDITOR)
            if (active_drag_ids.Count > 0 || touche_ids.Count > 0)
            {
                active_drag_ids.Clear();
                touche_ids.Clear();
            }
#endif
            float mouseWheel = Input.GetAxis("Mouse ScrollWheel");
            if (mouseWheel != 0)
            {
                if (Screen.safeArea.Contains(Input.mousePosition))
                {
                    flag = true;
                    distance -= mouseWheel * (2 + distance / 4);
                    //flag = CalcMouseY(mouseWheel * (2 + distance / 4));
                }
            }
            else
            {
                if (active_drag_ids.Count == 1)
                {
                    if (Input.GetMouseButton(0))
                    {
                        float mouseY = Input.GetAxis("Mouse Y");
                        flag = CalcMouseY(mouseY);
                    }
                    else
                    {
                        active_drag_ids.Clear();
                    }
                }
            }
        }
        else if (touche_ids.Count > 0)
        {
            var touch = active_drag_ids[touche_ids[touche_ids.Count - 1]];
            if (touch.IsPointerMoving() || (touche_ids.Count > 1 && active_drag_ids[touche_ids[0]].IsPointerMoving()))
            {
                if (touche_ids.Count > 1 || Input.GetKey(KeyCode.Space))
                {
                    flag = true;
                    float mouseY;
                    if (touche_ids.Count == 1)
                    {
                        mouseY = 50 * (active_drag_ids[touche_ids[0]].delta.y / Screen.height);
                    }
                    else
                    {
                        var t2 = active_drag_ids[touche_ids[touche_ids.Count - 2]];
                        var magnitude = (touch.position - t2.position).magnitude;
                        if (this.magnitude > 0)
                        {
                            mouseY = 30 * ((magnitude - this.magnitude) / Screen.height);
                        }
                        else
                        {
                            mouseY = 0;
                        }
                        this.magnitude = magnitude;
                    }
                    distance -= mouseY * (1 + distance / 10);
                }
                else
                {
                    flag = true;
                    var mouseY = 90 * (touch.delta.y / Screen.height);
                    angle_y += mouseY;

                    var mouseX = 360 * (touch.delta.x / Screen.width);
                    angle_xz -= mouseX;// * (touch.position.y > Screen.height / 2 ? 1 : -1);
                }
            }
        }
        //
        if (flag)
        {
            //
            //angle_xz = Mathf.Clamp(angle_xz, -1, 361);
            if (angle_xz < 0) angle_xz = 360;
            if (angle_xz > 360) angle_xz = 0;

            if (angle_y < 40) angle_y = 40;
            if (angle_y > 80) angle_y = 80;

            //if (angle_y > 80)
            //{
            //    distance -= angle_y - 80;
            //    angle_y = 80;
            //}
            //else if (distance < distance_default)
            //{
            //    distance += 80 - angle_y;
            //    if (distance > distance_default)
            //    {
            //        distance = distance_default;
            //    }
            //    angle_y = 80;
            //}

            //if (angle_y < 50)
            //{
            //    distance += 50 - angle_y;
            //    angle_y = 50;
            //}
            //else if (distance > distance_default)
            //{
            //    distance -= angle_y - 50;
            //    if (distance < distance_default)
            //    {
            //        distance = distance_default;
            //    }
            //    angle_y = 50;
            //}

            distance = Mathf.Clamp(distance, distance_min_clamp, distance_max_clamp);
        }
        else 
        {
            this.mousepos = Vector2.zero;
            this.magnitude = 0;
        }
        return flag;
    }

    private bool CalcMouseY(float mouseY)
    {
        bool flag = false;
        if (mouseY != 0)
        {
            flag = true;
            angle_y += mouseY * 2;
        }

        float mouseX = Input.GetAxis("Mouse X");
        if (mouseX != 0)
        {
            flag = true;
            angle_xz -= mouseX * 2;// * (Input.mousePosition.y > Screen.height / 2 ? 1 : -1);
        }

        if (!flag)
        {
            var mousepos = this.mousepos;
            this.mousepos = Input.mousePosition;
            if (mousepos != Vector2.zero)
            {
                var dt = this.mousepos - mousepos;
                if (dt != Vector2.zero)
                {
                    flag = true;
                    angle_y += dt.y / 3;
                    angle_xz -= dt.x / 3;
                }
            }
        }
        return flag;
    }

    public void StopTask()
    {
        _task.Stop();
    }

    //点头
    public void Play_diantou()
    {
        StopTask();
        IEnumerator _Play()
        {
            var angle_y = this.angle_y;
            var time = 3f;
            var end = TimeUtils.time + time;

            while (TimeUtils.time < end)
            {
                this.angle_y = angle_y + 0.2f;
                _check_dirty();
                yield return 30;
                //
                this.angle_y = angle_y - 0.2f;
                _check_dirty();
                yield return 30;
            }
            this.angle_y = angle_y;
        }
        _task.Start(_Play());
    }

    //转圈
    public void Play_zhuanquan()
    {
        StopTask();
        IEnumerator _Play()
        {
            var angle_xz = this.angle_xz;
            var time = 3f;
            var end = TimeUtils.time + time;
            var dt = 360 / time;
            while (TimeUtils.time < end)
            {
                this.angle_xz += TimeUtils.deltaTime * dt;
                _check_dirty();
                yield return null;
            }
            this.angle_xz = angle_xz;
        }
        _task.Start(_Play());
    }

    public void Play_zhenping()
    {

        StopTask();

        if (transform.childCount == 0) return;

        IEnumerator _Play()
        {
            var nor = Vector3.zero;
            if ((shock_type & Shock_Type.shock_x) != Shock_Type.shock_none)
            {
                nor.x = Mathf.Sign(gameObject.transform.eulerAngles.x);
            }
            if ((shock_type & Shock_Type.shock_y) != Shock_Type.shock_none)
            {
                nor.y = Mathf.Sign(gameObject.transform.eulerAngles.y);
            }
            if ((shock_type & Shock_Type.shock_z) != Shock_Type.shock_none)
            {
                nor.z = Mathf.Sign(gameObject.transform.eulerAngles.z);
            }
            nor = nor.normalized;

            var cam_gameObject = gameObject.GetComponentInChildren<Camera>();
            var start = Time.time;
            var end = Time.time + shock_time;
            while (Time.time < end)
            {
                var s = Mathf.Sin((Time.time - start) * shock_speed);
                cam_gameObject.transform.localPosition += nor * s * shock_intensity;
                yield return MyTask.Delay(35);
            }
            cam_gameObject.transform.localPosition = Vector3.zero;
        }
        _task.Start(_Play());
    }
}
