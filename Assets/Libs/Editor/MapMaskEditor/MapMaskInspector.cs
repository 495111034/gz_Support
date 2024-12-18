using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Diagnostics;
using findpath;
using Object = UnityEngine.Object;
using Debug = UnityEngine.Debug;
using Mask = System.Byte;

/// <summary>
/// 地形掩码编辑器
/// </summary>
[CustomEditor(typeof(MapMaskBehaviour))]
class MapMaskInspector : Editor
{
    static readonly string[] size_names = new string[] { "x1", "x2", "x4", "x8", "x16", "x32" };
    static readonly int[] size_values = new int[] { 1, 2, 4, 8, 16, 32 };
    static readonly string[] mode_names = new string[] { "Edit Mask", "Find Path" };
    static string help = "1/2/3 切换掩码\nB/C 调整刷子尺寸\nV切换显示模式";

    // 编辑器信息
    MapMaskBehaviour _mmb;
    SerializedObject _mmo;
    MapMask _mm;
    int _want_width;
    int _want_height;
    MapMaskMesh _meshs;

    // 刷阻挡
    static Mask curMask = 1;
    static int size_idx = 2;
    static int curSize = 0;
    static int editMode = 0;

    // 自动设置
    static bool _auto_set_flag;
    static float _auto_set_degree = 45;

    // 寻径
    bool smooth = true;
    Vector3 start_pos;
    Vector3 end_pos;
    float path_error;
    Vector3[] path;
    Vector3[] line;
    Color line_color = Color.white;
    AStar astar;
    string path_desc;

    // 画刷信息
    class BrushInfo
    {
        public Vector3 pt;
        public int x1, x2, z1, z2;  // 包含 x2/z2
        public bool valid;
    }

    // 选中时
    void OnEnable()
    {
        // 编辑器信息
        _mmb = target as MapMaskBehaviour;
        _mmo = new SerializedObject(target);

        // 初始化 MapMask
        _mm = _mmb.mm;
        if (_mm == null || _mm.width == 0)
        {
            _mmb.mm = _mm = new MapMask();
            if (!LoadMask(_mm))
            {
                CreateMesh(64, 64);
            }
        }

        _want_width = _mm.width;
        _want_height = _mm.height;
        _meshs = new MapMaskMesh();

        //
        curSize = size_values[size_idx];

        //
        path = null;
        astar = null;

        //
        UpdateMesh();
    }

    // 取消选择时
    void OnDisable()
    {
        (target as MapMaskBehaviour).meshs = null;
        RepaintMask();
    }
    MyBetterList<float> findPathResult;
    public override void OnInspectorGUI()
    {
        // 设置尺寸
        EditorGUILayout.BeginHorizontal();
        _want_width = EditorGUILayout.IntField(_want_width);
        _want_height = EditorGUILayout.IntField(_want_height);
        if (GUILayout.Button("Set Size", EditorStyles.miniButtonMid))
        {
            CreateMesh(_want_width, _want_height);
            UpdateMesh();
        }
        EditorGUILayout.EndHorizontal();

        // 导出
        if (GUILayout.Button("Load Shield"))
        {
            if (!LoadMask(_mm))
            {
                CreateMesh(64, 64);
            }
            UpdateMesh();
        }
        if (GUILayout.Button("Save Shield"))
        {
            SaveMask(_mm);
        }

        // 自动设置阻挡
        _auto_set_flag = EditorGUILayout.BeginToggleGroup("Auto Set", _auto_set_flag);
        _auto_set_degree = EditorGUILayout.Slider("degree", _auto_set_degree, 0, 90);
        if (GUILayout.Button("Auto Set"))
        {
            _meshs.AutoSet(_auto_set_degree);
            RepaintMask();
        }
        EditorGUILayout.EndToggleGroup();
    }

    static List<float> s_pos_list = new List<float>();
    void OnSceneGUI()
    {
        //Log.LogInfo($"OnSceneGUI");

        // 操作界面
        Handles.BeginGUI();
        {
            GUILayout.BeginArea(new Rect(10, 10, 500, 200)); 
            {
                // 操作模式
                GUILayout.BeginHorizontal();
                //GUILayout.Label("Type");
                var prev_mode = editMode;
                editMode = GUILayout.Toolbar(editMode, mode_names);
                GUILayout.EndHorizontal();

                // 编辑模式
                if (editMode == 0)
                {
                    // 选择掩码
                    GUILayout.BeginHorizontal();
                    {
                        //GUILayout.Label("Map Mask");
                        curMask = (byte)GUILayout.Toolbar((int)curMask, MapMaskMesh.MASK_NAMES);
                    }
                    GUILayout.EndHorizontal();

                    // 选择尺寸
                    GUILayout.BeginHorizontal();
                    //GUILayout.Label("Brush Size");
                    var idx = GUILayout.Toolbar(size_idx, size_names);
                    if (idx != size_idx)
                    {
                        size_idx = idx;
                        curSize = size_values[size_idx];
                    }
                    GUILayout.EndHorizontal();

                    // 显示方式
                    GUILayout.BeginHorizontal();
                    _mmb.use_z = GUILayout.Toggle(_mmb.use_z, "Draw Mode");
                    GUILayout.EndHorizontal();

                    // help
                    GUILayout.Label(help, EditorStyles.whiteBoldLabel);
                }
                // 寻径模式
                else if (editMode == 1)
                {
                    // 第一次进入寻径模式, 清空寻径信息
                    if (prev_mode != editMode)
                    {
                        astar = null;
                    }
                    smooth = GUILayout.Toggle(smooth, "Smooth");
                    if (string.IsNullOrEmpty(path_desc)) path_desc = "";
                    GUILayout.TextArea(path_desc);
                    path_error = EditorGUILayout.FloatField("error", path_error);

                    // 寻径
                    if (GUILayout.Button("Test"))
                    {
                        start_pos.Set(1000, 1000, 1000);
                        end_pos.Set(1000, 1000, 1000);
                        path = null;
                    }
                }
            }
            GUILayout.EndArea();
        }
        Handles.EndGUI();

        // 如果鼠标位于掩码区域内
        var info = GetBrushInfo();
        if (info != null && info.valid)
        {
            // 绘制画刷
            if (editMode == 0)
            {
                var color = MapMaskMesh.MASK_COLORS[curMask];
                for (int x = info.x1; x <= info.x2; x++)
                {
                    for (int z = info.z1; z <= info.z2; z++)
                    {
                        var verts = new Vector3[]
                        {    
                            new Vector3(x, 0, z),
                            new Vector3(x, 0, z + 1),
                            new Vector3(x + 1, 0, z + 1),
                            new Vector3(x + 1, 0, z),
                        };
                        MapMaskMesh.UpdateHeight(verts);
                        Handles.DrawSolidRectangleWithOutline(verts, color, Color.white);
                    }
                }
            }

            // 鼠标位置信息
            {
                var gx = info.pt.x;
                var gy = info.pt.z;

                var posmask = "*";
                if (gx >=0 && gx < _mm.width && gy >=0 && gy < _mm.height) 
                {
                    posmask = _mm.masks[ (int)gx + (int)gy * _mm.width].ToString();
                }
                var h = MapMaskMesh.GetHeight(gx,gy);
                Handles.Label(info.pt, string.Format("    ({0:0.#},{1:0.#}) {2},{3:0.#}", gx, gy, posmask, h), EditorStyles.numberField);
                SceneView.RepaintAll();
            }

            // 刷阻挡
            switch (Event.current.type)
            {
                // 绘制掩码
                case EventType.MouseDown:
                case EventType.MouseDrag:
                    // 左键
                    if (Event.current.button == 0)
                    {
                        // 设置掩码
                        if (editMode == 0)
                        {
                            SetMask(info);
                            RepaintMask();
                            astar = null;
                            Event.current.Use();
                        }
                        // 设置寻径起点
                        else if (editMode == 1 && Event.current.type == EventType.MouseDown)
                        {
                            start_pos = info.pt;
                            start_pos.y = 0;
                            path = null;
                            Event.current.Use();
                        }
                    }
                    // 右键
                    else if (Event.current.button == 1 && Event.current.type == EventType.MouseDown)
                    {
                        // 设置寻径终点
                        if (editMode == 1)
                        {
                            end_pos = info.pt;
                            end_pos.y = 0;
                            path = null;
                            Event.current.Use();
                        }
                    }
                    break;

                // 保持被选中状态
                case EventType.Layout:
                    HandleUtility.AddDefaultControl(GUIUtility.GetControlID(GetHashCode(), FocusType.Passive));
                    break;
            }
        }

        // 快捷键
        if (Event.current.type == EventType.KeyDown)
        {
            switch (Event.current.keyCode)
            {
                case KeyCode.Alpha1:
                    curMask = (Mask)0;
                    break;

                case KeyCode.Alpha2:
                    curMask = (Mask)1;
                    break;

                case KeyCode.Alpha3:
                    curMask = (Mask)2;
                    break;

                case KeyCode.C:
                    if (curSize > 1) curSize--;
                    break;

                case KeyCode.B:
                    if (curSize < 100) curSize++;
                    break;

                case KeyCode.V:
                    _mmb.use_z = !_mmb.use_z;
                    RepaintMask();
                    break;

            }
        }

        // 绘制当前路径
        if (editMode == 1)
        {
            // 构造路径
            if (astar == null)
            {
                astar = new AStar();
                astar.LoadMask(_mm);
                path = null;
            }
            if (path == null)
            {
                var list = s_pos_list;
                int len = 0;
                var x0 = start_pos.x;
                var y0 = start_pos.z;
                var x1 = end_pos.x;
                var y1 = end_pos.z;
                Stopwatch sw = new Stopwatch();
                sw.Start();
                bool near = false;
                var ret = astar.Search(x0, y0, x1, y1, path_error, list, 0, ref near, false);
                line_color = ret == Result.Ok ? Color.white : Color.red;
                sw.Stop();
                if (ret == Result.Ok && list.Count >= 4)
                {
                    len = list.Count / 2;
                    path = new Vector3[len];
                    for (int i = 0, j = 0; i < len; i++, j += 2)
                    {
                        path[i] = new Vector3(list[j], 0, list[j + 1]);
                    }
                }
                else
                {
                    line = path = new Vector3[0];
                }

                // 统计信息
                var dist = Vector2.Distance(new Vector2(x0, y0), new Vector2(x1, y1));
                int total = _mm.width * _mm.height;
                path_desc = string.Format("Search result:{0}, ({1},{2})=>({3},{4}), dist:{5}, len:{6}, search:{7}/{8}={9}, time_used(ms):{10}",
                    ret, x0, y0, x1, y1, dist, len, astar.loop, total, (float)astar.loop / total, sw.ElapsedTicks / 10000f);

                // 直线
                astar.GetFarest(x0, y0, ref x1, ref y1, 0);
                line = new Vector3[]
                {
                    new Vector3(x0, 0, y0),
                    new Vector3(x1, 0, y1),
                };
            }

            // 绘制路径
            Handles.color = Color.green;
            Handles.DrawPolyLine(path);
            Handles.color = line_color;
            Handles.DrawPolyLine(line);

            // 绘制 开始/结束 图标
#if UNITY_2020_3_OR_NEWER
            Handles.color = Color.green;
            Handles.SphereHandleCap(0, start_pos, Quaternion.identity, HandleUtility.GetHandleSize(start_pos) * 0.1f, EventType.Repaint);
            Handles.color = Color.red;
            Handles.SphereHandleCap(0, end_pos, Quaternion.identity, HandleUtility.GetHandleSize(end_pos) * 0.1f, EventType.Repaint);
#else
            Handles.color = Color.green;
            Handles.SphereCap(0, start_pos, Quaternion.identity, HandleUtility.GetHandleSize(start_pos) * 0.1f);
            Handles.color = Color.red;
            Handles.SphereCap(0, end_pos, Quaternion.identity, HandleUtility.GetHandleSize(end_pos) * 0.1f);
#endif
        }
    }

    // 返回画刷信息
    BrushInfo GetBrushInfo()
    {
        RaycastHit hit;
        var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        if (Physics.Raycast(ray, out hit, 1000, (int)ObjLayerMask.HeightTest))
        {
            var pt = hit.point;
            return GetBrushInfo(pt);
        }
        return null;
    }
    BrushInfo GetBrushInfo(Vector3 pt)
    {
        var size = curSize;

        var info = new BrushInfo();
        info.pt = pt;
        info.x1 = (int)pt.x - (size / 2);
        info.z1 = (int)pt.z - (size / 2);
        info.x2 = info.x1 + size - 1;
        info.z2 = info.z1 + size - 1;
        info.valid = info.x2 >= 0 && info.x1 < _mm.width && info.z2 >= 0 && info.z1 < _mm.height;

        return info;
    }

    // 设置掩码
    void SetMask(BrushInfo info)
    {
        var m = curMask;
        var c = MapMaskMesh.MASK_COLORS[m];
        _meshs.SetColor(info.x1, info.x2, info.z1, info.z2, m, c);
        SetDirty();
    }

    // 创建掩码
    void CreateMesh(int width, int height)
    {
        width = Mathf.Clamp(width, 1, 1000);
        height = Mathf.Clamp(height, 1, 1000);

        _mm.width = width;
        _mm.height = height;
        _mm.masks = new Mask[height * width];
        for (int i = 0; i < _mm.masks.Length; i++) _mm.masks[i] = 1;
        SetDirty();

        astar = null;
    }

    // 更新掩码
    void UpdateMesh()
    {
        _meshs.LoadMask(_mm.width, _mm.height, _mm.masks);
        (target as MapMaskBehaviour).meshs = _meshs.Meshs;
        RepaintMask();
    }

    // 重绘场景
    void RepaintMask()
    {
        if (target) (target as MonoBehaviour).transform.position = Vector3.zero;
    }

    // 设置为脏
    new void SetDirty()
    {
        if (target) EditorUtility.SetDirty(target);
    }

    #region 文件IO

    // 保存掩码
    static void SaveMask(MapMask mm)
    {
        if (!EditorPathUtils.CheckPathSettings()) return;

        var data = mm.Save();

        // 导出 shield
        var save_shield_name = PathDefs.EXPORT_PATH_SHIELD + GetMaskSaveName() + ".shield";
        File.WriteAllBytes(save_shield_name, data);
        Debug.Log(string.Format("SaveMask: {0}, length:{1:N}", save_shield_name, data.Length));

        // 导出 ab 文件
        //bool ret = false;
        //var save_ab_name = PathUtils.EXPORT_PATH_SCENE + GetSceneName() + ".mask";
        //BuildHelper.CreateTempTextAsset(GetSceneName(), data, (ta) =>
        //    {
        //        ret = BuildPipeline.BuildAssetBundle(ta, null, save_ab_name, BuildAssetBundleOptions.CompleteAssets, BuildHelper.buildTarget);
        //        BuildHelper.ShowBuildLog("SaveMask", ret, save_ab_name);
        //    });
    }

    // 读入掩码
    static bool LoadMask(MapMask mm)
    {
        if (!EditorPathUtils.CheckPathSettings()) return false;

        // 读入 shield
        var save_shield_name = PathDefs.EXPORT_PATH_SHIELD + GetMaskSaveName() + ".shield";
        if (File.Exists(save_shield_name))
        {
            try
            {
                var data = File.ReadAllBytes(save_shield_name);
                if (mm.Load(save_shield_name, data))
                {
                    Debug.Log("LoadMask: " + save_shield_name);
                    return true;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return false;
            }
        }

        //
        return false;
    }

    // 获取 地图掩码 保存的文件名
    public static string GetMaskSaveName()
    {
        var re = new System.Text.RegularExpressions.Regex(@"[0-9]+");
        var scenename = re.Match(GetSceneName()).Value;
        scenename = int.Parse(scenename).ToString();
        return scenename;
    }
    static string GetSceneName()
    {
        return Path.GetFileNameWithoutExtension(EditorApplication.currentScene);
    }

    #endregion

}
