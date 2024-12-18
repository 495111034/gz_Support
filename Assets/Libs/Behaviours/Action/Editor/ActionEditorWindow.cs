using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.Reflection;
using System.IO;
using System.Collections;

namespace ActionEditor
{
    public class ActionEditorWindow : EditorWindow
    {
        public static string type_int = typeof(int).Name;
        public static string type_float = typeof(float).Name;
        public static string type_bool = typeof(bool).Name;
        public static string type_string = typeof(string).Name;
        public static string type_int_array = typeof(int[]).Name;
        public static string type_float_array = typeof(float[]).Name;
        public static string type_string_array = typeof(string[]).Name;
        public static string type_Vector2 = typeof(Vector2).Name;
        public static string type_Vector3 = typeof(Vector3).Name;
        public static string type_enum = "enum";

        private static Color[] colors = new Color[] 
        {
            new Color(0, 1, 0.602f, 1),
            new Color(0.149f, 1, 0, 1),
            new Color(0.91f, 1, 0, 1),
            new Color(1, 0.46f, 0, 1),
            new Color(1, 0, 0.1f, 1),
            new Color(1, 0, 0.619f, 1),
            new Color(0.66f, 0, 1, 1),
            new Color(0.33f, 0, 1, 1),
            new Color(0, 0.09f, 1, 1),
            new Color(0, 0.538f, 1, 1),
            new Color(0, 0.95f, 1, 1),
            new Color(0, 1, 0.65f, 1),
        };

        [Serializable]
        public class ActionMenuJson
        {
            public string class_name;
            public string node_title;
            public int order = 0;
            public bool is_container = false; //是否容器
            public bool is_have_parallel = false; //是否带可配置并行选项
            public List<ActionMenuField> fields = new List<ActionMenuField>();
        }

        [Serializable]
        public class ActionMenuField
        {
            public string desc;
            public string field;
            public string value;
            public string type;
            public string[] enums;
        }

        public Dictionary<string, ActionMenuJson> menu_dic = new Dictionary<string, ActionMenuJson>();
        public string[] container_keys = new string[0];
        public string[] node_type_keys = new string[0];

        public Vector2 default_size = new Vector2(100, 100);
        private Vector2 scroll_pos;

        public WindowNodeData windowNodeData = null;
        public WindowNodeData selectedWindow = null;

        private Dictionary<int, List<string>> undo_list_dic = new Dictionary<int, List<string>>();
        private Dictionary<int, int> undo_index_dic = new Dictionary<int, int>();

        public Dictionary<string, List<ActionConfig>> all_actions_dic = new Dictionary<string, List<ActionConfig>>();
        public Dictionary<int, ActionConfig> all_actions = new Dictionary<int, ActionConfig>();
        private int curr_select_config_id = -1;

        private bool is_refsh = false;
        public bool is_start_drag = false;
        private Vector2 mousePosition;
        private Vector2 mouse_drag;

        private GenericMenu menu = null;
        Color backgroundColor;
        Color gridColor;

        public int base_id = 0;
        private float temp_size = 0;
        private float scroll_size = 0;
        private float add_size = 0;
        private Vector2 global_size = Vector2.zero;
        private GUIStyle group_unSelectButtonGUIOp = null;
        private GUIStyle group_selectButtonGUIOp = null;
        private GUIStyle unSelectButtonGUIOp = null;
        private GUIStyle selectButtonGUIOp = null;
        private GUIStyle unSelectGroupButtonGUIOp = null;
        private GUIStyle selectGroupButtonGUIOp = null;
        protected string action_save_path = "";

        private Rect menu_left = new Rect(0, 0, 250, 500);
        public Rect menu_right = new Rect(0, 10, 200, 500);

        public bool is_delay_save = false;
        public bool is_undo_add = false;

        public string copy_json = "";

        public ActionMenuJson FindActionMenuJson(string class_name)
        {
            foreach (var kv in menu_dic)
            {
                if (kv.Value.class_name == class_name)
                {
                    return kv.Value;
                }
            }
            return null;
        }

        private bool is_regsis_LogicEvent = false;
        public static ActionEditorWindow instance = null;

        private void OnEnable()
        {
            instance = this;

            backgroundColor = new Color(0.1f, 0.1f, 0.1f);
            gridColor = new Color(0.2f, 0.2f, 0.2f);
        }

        private void OnDisable()
        {
            instance = null;
        }

        private void OnInspectorUpdate()
        {
            if (Application.isPlaying)
            {
                instance = this;
                if (!is_regsis_LogicEvent)
                {
                    is_regsis_LogicEvent = true;
                    var type = typeof(MyComponent).Assembly.GetType("GameBase.GameLoader");
                    if (type != null)
                    {
                        var evt = type.GetEvent("OnCallLogicEvent", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                        if (evt != null)
                        {
                            MethodInfo handler = typeof(ActionEditorWindow).GetMethod("OnCallLogicEvent", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            Delegate del = Delegate.CreateDelegate(evt.EventHandlerType, null, handler);
                            evt.AddEventHandler(this, del);
                        }
                    }
                }
            }
            else
            {
                if (is_regsis_LogicEvent)
                {
                    is_regsis_LogicEvent = false;
                    var type = typeof(MyComponent).Assembly.GetType("GameBase.GameLoader");
                    if (type != null)
                    {
                        var evt = type.GetEvent("OnCallLogicEvent", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                        if (evt != null)
                        {
                            MethodInfo handler = typeof(ActionEditorWindow).GetMethod("OnCallLogicEvent", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                            Delegate del = Delegate.CreateDelegate(evt.EventHandlerType, null, handler);
                            evt.RemoveEventHandler(this, del);
                        }
                    }
                }
            }
        }

        private void OnLostFocus()
        {
            is_start_drag = false;
        }

        private void OnCallLogicEvent(int eventId, Hashtable hash)
        {
            if (eventId == -999999)
            {
                UpdateLogEvent(hash.GetInt("id"), hash.GetInt("state"), hash.GetString("log"));
            }
        }

        private void UpdateLogEvent(int id, int state, string log)
        {
            if (instance == null)
            {
                return;
            }
            instance.curr_trigger_id = id;
            if (instance.windowNodeData != null)
            {
                instance.windowNodeData.UpdateLogData(id, state, log);

                instance.Repaint();
            }
        }

        public void ParseFindMaxBaseId(ActionConfig action)
        {
            if (action.id > base_id)
            {
                base_id = action.id;
            }
            if (action.childs.Count > 0)
            {
                foreach (var c in action.childs)
                {
                    ParseFindMaxBaseId(c);
                }
            }
        }

        private int color_index = 0;
        public Color GetColor()
        {
            if (color_index >= colors.Length)
            {
                color_index = 0;
            }
            return colors[color_index++];
        }

        protected virtual string ActionMenuJsonPath
        {
            get { return Application.dataPath + "/Libs/Behaviours/Action/Editor/ActionMenuJson.txt"; }
        }

        protected virtual string GetClientPath() { return ""; }


        private void GroupSortAllActionToDic()
        {
            all_actions_dic = new Dictionary<string, List<ActionConfig>>();
            foreach(ActionConfig action in all_actions.Values)
            {
                string group_desc = "0";
                int group_index = action.desc.IndexOf('/');
                if (group_index > -1)
                {
                    group_desc = action.desc.Substring(0, group_index);
                }
                if (!all_actions_dic.TryGetValue(group_desc, out var list))
                {
                    list = new List<ActionConfig>();
                    all_actions_dic.Add(group_desc, list);
                }
                list.Add(action);
            }

            foreach (var v in all_actions_dic.Values)
            {
                v.Sort(SortAction);
            }
        }

        private int SortAction(ActionConfig a, ActionConfig b)
        {
            return a.sort.CompareTo(b.sort);
        }

        private float save_invetavl = 0;
        public int curr_trigger_id = -1;
        public string curr_select_group_type = "";
        private float left_menu_offset_x = 200;
        void OnGUI()
        {
            if (menu_dic == null || menu_dic.Count == 0)
            {
                string path = ActionMenuJsonPath;
                if (!File.Exists(path))
                {
                    Debug.LogError("没有ActionMenuJson文件: " + path);
                    return;
                }
                menu = new GenericMenu();
                menu_dic.Clear();

                List<ActionMenuJson> actionMenuJsons = new List<ActionMenuJson>();
                List<string> container_key_list = new List<string>();
                List<string> node_type_key_list = new List<string>();
                StringReader sr = new StringReader(File.ReadAllText(path, System.Text.UTF8Encoding.UTF8));
                while (sr.Peek() > 1)
                {
                    string line = sr.ReadLine().Trim();
                    line = line.Trim(new char[] { '\r', '\n', '\t' });
                    var config = JsonUtility.FromJson<ActionMenuJson>(line);
                    if (config.order == 0) config.order = 999999999;
                    actionMenuJsons.Add(config);
                }

                actionMenuJsons.Sort((ActionMenuJson a, ActionMenuJson b) => { return a.order.CompareTo(b.order); });

                for (int i = 0; i < actionMenuJsons.Count; i++)
                {
                    menu_dic[actionMenuJsons[i].node_title] = actionMenuJsons[i];
                    menu.AddItem(new GUIContent(actionMenuJsons[i].node_title), false, Callback, actionMenuJsons[i].node_title);
                    if (actionMenuJsons[i].node_title.StartsWith("容器"))
                    {
                        container_key_list.Add(actionMenuJsons[i].node_title);
                    }
                    else
                    {
                        node_type_key_list.Add(actionMenuJsons[i].node_title);
                    }
                }

                container_keys = container_key_list.ToArray();
                node_type_keys = node_type_key_list.ToArray();
                sr.Close();
                sr.Dispose();

                if (all_actions.Count == 0)
                {
                    base_id = 0;
                    all_actions = new Dictionary<int, ActionConfig>();

                    action_save_path = GetClientPath();
                    if (Directory.Exists(action_save_path))
                    {
                        string[] files = Directory.GetFiles(action_save_path, "*.json", SearchOption.AllDirectories);
                        for (int i = 0; i < files.Length; i++)
                        {
                            string json = File.ReadAllText(files[i], System.Text.UTF8Encoding.UTF8);
                            ActionConfig action = JsonUtility.FromJson<ActionConfig>(json);
                            all_actions[action.id] = action;
                            ParseFindMaxBaseId(action);
                        }
                    }

                    GroupSortAllActionToDic();
                }
            }

            if (windowNodeData == null)
            {
                curr_select_config_id = -1;

                temp_size = default_size.x;
                scroll_size = temp_size;

                global_size = default_size;

                group_unSelectButtonGUIOp = new GUIStyle("flow node 2");
                group_unSelectButtonGUIOp.fixedHeight = 30;
                group_unSelectButtonGUIOp.stretchWidth = true;

                group_selectButtonGUIOp = new GUIStyle("flow node 2 on");
                group_selectButtonGUIOp.fixedHeight = 30;
                group_selectButtonGUIOp.stretchWidth = true;


                unSelectButtonGUIOp = new GUIStyle("flow node 0");
                unSelectButtonGUIOp.fixedHeight = 30;
                unSelectButtonGUIOp.stretchWidth = true;

                selectButtonGUIOp = new GUIStyle("flow node 1 on");
                selectButtonGUIOp.fixedHeight = 30;
                selectButtonGUIOp.stretchWidth = true;

                unSelectGroupButtonGUIOp = new GUIStyle("flow node 0");
                unSelectGroupButtonGUIOp.fixedHeight = 30;
                unSelectGroupButtonGUIOp.stretchWidth = true;
                unSelectGroupButtonGUIOp.margin.left = 10;

                selectGroupButtonGUIOp = new GUIStyle("flow node 1 on");
                selectGroupButtonGUIOp.fixedHeight = 30;
                selectGroupButtonGUIOp.stretchWidth = true;
                selectGroupButtonGUIOp.margin.left = 10;
            }

            DrawBackground();
            DrawGrid(10, 0.2f);
            DrawGrid(50, 0.4f);
            mouse_drag = Vector2.zero;
            add_size = 0;
            var curEvt = Event.current;
            if (curEvt.mousePosition.x > left_menu_offset_x && curEvt.mousePosition.x < position.width - menu_right.width && curEvt.mousePosition.y < position.height - 30)
            {
                if (curEvt.isScrollWheel)
                {
                    is_refsh = true;
                    scroll_size -= curEvt.delta.y * 3.5f;
                    if (scroll_size < 50)
                    {
                        scroll_size = 50;
                    }
                    if (scroll_size > 200)
                    {
                        scroll_size = 200;
                    }
                    add_size = scroll_size - temp_size;
                    temp_size = scroll_size;

                    global_size.x += add_size;
                    global_size.y += add_size;
                }
                else if (curEvt.button == 0)
                {
                    if (!is_start_drag)
                    {
                        if (curEvt.type == EventType.MouseDown)
                        {
                            is_start_drag = true;
                            mousePosition = curEvt.mousePosition;
                        }
                    }
                    else if (curEvt.type == EventType.MouseUp)
                    {
                        is_start_drag = false;
                    }
                    else if(curEvt.type == EventType.MouseDrag)
                    {
                        mouse_drag = curEvt.mousePosition - mousePosition;
                        mousePosition = curEvt.mousePosition;
                    }
                }
                else
                {
                    is_refsh = false;
                }
            }
            else if (is_start_drag)
            {
                if (curEvt.type == EventType.MouseUp)
                {
                    is_start_drag = false;
                }
                else if (curEvt.type == EventType.MouseDrag)
                {
                    mouse_drag = curEvt.mousePosition - mousePosition;
                    mousePosition = curEvt.mousePosition;
                }
            }

            if (windowNodeData != null)
            {
                BeginWindows();
                windowNodeData.Draw(mouse_drag, global_size);
                EndWindows();
            }

            if (is_start_drag || is_refsh)
            {
                Repaint();
            }

            left_menu_offset_x = EditorGUILayout.Slider(left_menu_offset_x, 0, 200, GUILayout.Width(200));

            menu_left.width = 250;
            menu_left.height = position.height - 10;
            menu_left.x = -200 + left_menu_offset_x;
            menu_left.y = 20;
            GUILayout.BeginArea(menu_left);
            EditorGUILayout.Space();
            scroll_pos = EditorGUILayout.BeginScrollView(scroll_pos, GUILayout.Width(210));
            foreach (var kv in all_actions_dic)
            {
                if (kv.Key == curr_select_group_type || kv.Key == "0")
                {
                    GUIStyle select = selectButtonGUIOp;
                    GUIStyle unselect = unSelectButtonGUIOp;

                    float width = 200;
                    if (kv.Key == "0")
                    {
                        selectButtonGUIOp.margin.left = 0;
                        unSelectButtonGUIOp.margin.left = 0;
                    }
                    else
                    {
                        if (GUILayout.Button(kv.Key, group_selectButtonGUIOp, GUILayout.Width(200), GUILayout.Height(32)))
                        {
                            curr_select_group_type = "";
                        }
                        else
                        {
                            select = selectGroupButtonGUIOp;
                            unselect = unSelectGroupButtonGUIOp;
                            width = 180;
                        }
                    }
                    for (int i = 0; i < kv.Value.Count; i++)
                    {
                        if (curr_trigger_id == kv.Value[i].id)
                        {
                            GUI.color = Color.red;
                        }
                        if (curr_select_config_id == kv.Value[i].id)
                        {
                            GUILayout.Button(kv.Value[i].desc.Replace(kv.Key + "/", ""), select, GUILayout.Width(width), GUILayout.Height(32));
                        }
                        else
                        {
                            if (GUILayout.Button(kv.Value[i].desc.Replace(kv.Key + "/", ""), unselect, GUILayout.Width(width), GUILayout.Height(32)))
                            {
                                curr_select_config_id = kv.Value[i].id;
                                color_index = 0;
                                windowNodeData = new WindowNodeData(this, kv.Value[i]);
                                if (!undo_list_dic.TryGetValue(windowNodeData.id, out List<string> undo_list) || undo_list.Count == 0)
                                {
                                    is_undo_add = true;
                                }
                            }
                        }
                        GUI.color = Color.white;
                    }
                }
                else
                {
                    if (GUILayout.Button(kv.Key, group_unSelectButtonGUIOp, GUILayout.Width(200), GUILayout.Height(32)))
                    {
                        curr_select_group_type = kv.Key;
                    }
                }
            }

            if (GUILayout.Button("+", GUILayout.Width(200), GUILayout.Height(32)))
            {
                ActionConfig action = new ActionConfig();
                action.id = ++base_id;
                action.x = this.position.width * 0.5f - default_size.x * 0.5f;
                action.y = 50;
                action.class_name = "Sequence";
                action.desc = "新增行为";
                curr_select_config_id = action.id;
                windowNodeData = new WindowNodeData(this, action);
                all_actions[action.id] = action;
                is_delay_save = true;
                is_undo_add = true;

                GroupSortAllActionToDic();
            }
            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();

            if (selectedWindow != null)
            {
                menu_right.x = position.width - menu_right.width - 10;
                menu_right.height = position.height;
                EditorGUI.DrawRect(menu_right, new Color(0.5f, 0.5f, 0.5f, 0.6f));
                GUILayout.BeginArea(menu_right);
                selectedWindow.DrawField();
                GUILayout.EndArea();
            }

            if (all_actions.ContainsKey(curr_select_config_id))
            {
                menu_left.x = menu_left.width - 40;
                menu_left.y = 10;
                menu_left.width = position.width;
                menu_left.height = 80;

                if (curEvt.control && curEvt.type == EventType.KeyUp)
                {
                    if (curEvt.keyCode == KeyCode.Z || curEvt.keyCode == KeyCode.X)
                    {
                        if (!undo_list_dic.TryGetValue(windowNodeData.id, out List<string> undo_list))
                        {
                            undo_list = new List<string>();
                            undo_list_dic.Add(windowNodeData.id, undo_list);
                        }
                        if (undo_list.Count > 0)
                        {
                            undo_index_dic.TryGetValue(windowNodeData.id, out int index);
                            if (curEvt.keyCode == KeyCode.Z)
                            {
                                index--;
                            }
                            else
                            {
                                index++;
                            }
                            if (index < 0) index = 0;
                            if (index >= undo_list.Count) index = undo_list.Count - 1;

                            string undo_json = undo_list[index];
                            undo_index_dic[windowNodeData.id] = index;
                            ActionConfig action = JsonUtility.FromJson<ActionConfig>(undo_json);
                            all_actions[windowNodeData.id] = action;
                            bool is_ok = false;
                            foreach (var kv in all_actions_dic)
                            {
                                for (int i = 0; i < kv.Value.Count; i++)
                                {
                                    if (kv.Value[i].id == windowNodeData.id)
                                    {
                                        kv.Value[i] = action;
                                        is_ok = true;
                                        break;
                                    }
                                }
                                if (is_ok)
                                {
                                    break;
                                }
                            }
                            color_index = 0;
                            windowNodeData = new WindowNodeData(this, action);
                            if (selectedWindow != null)
                            {
                                SelectActionWindowById(windowNodeData, selectedWindow.id);
                                if (selectedWindow != null)
                                {
                                    OnNodeSelected(selectedWindow, false);
                                }
                            }
                            is_undo_add = false;
                            is_delay_save = true;
                            this.Repaint();
                        }
                    }
                }

                //undo_index_dic

                GUILayout.BeginArea(menu_left);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(action_save_path + "/action_" + windowNodeData.id + ".json", GUILayout.Width(300));
                if (GUILayout.Button("自动排列", GUILayout.Width(100)))
                {
                    AutoSortFormWinDataLayout(windowNodeData, windowNodeData.rect.position);
                }
                if (is_delay_save && windowNodeData != null)
                {
                    if (GUILayout.Button("保存", GUILayout.Width(100)) || (curEvt.control && curEvt.keyCode == KeyCode.S))
                    {
                        save_invetavl = 0.8f;
                        is_delay_save = false;
                        string file = "action_" + windowNodeData.id;
                        File.WriteAllText(action_save_path + "/" + file + ".json", JsonUtility.ToJson(windowNodeData.actionConfig));
                        GroupSortAllActionToDic();
                    }
                }

                if (!string.IsNullOrEmpty(copy_json))
                {
                    if (GUILayout.Button("粘贴", GUILayout.Width(100)))
                    {
                        ActionConfig action = JsonUtility.FromJson<ActionConfig>(copy_json);
                        action.id = curr_select_config_id;
                        all_actions[curr_select_config_id] = action;
                        windowNodeData = new WindowNodeData(this, action);
                        GroupSortAllActionToDic();
                    }
                }

                if (Application.isPlaying)
                {
                    if (GUILayout.Button("应用行为", GUILayout.Width(100)))
                    {
                        ExcuteAction();
                    }
                    ExtendOptionGUI();
                    if (GUILayout.Button("点击复制坐标", GUILayout.Width(100)))
                    {
                        var type = typeof(MyComponent).Assembly.GetType("GameBase.GameLoader");
                        if (type != null)
                        {
                            var evt = type.GetMethod("CallLogic", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                            if (evt != null)
                            {
                                evt.Invoke(type, new object[] { -999995, new Hashtable() });
                            }
                        }
                    }
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                string ddes = all_actions[curr_select_config_id].desc;
                string group_desc = "";
                int group_index = ddes.IndexOf('/');
                if (group_index > -1)
                {
                    group_desc = ddes.Substring(0, group_index);
                    ddes = ddes.Substring(group_index + 1);
                }
                group_desc = EditorGUILayout.TextField(group_desc, GUILayout.Width(60));
                ddes = EditorGUILayout.TextField(ddes, GUILayout.Width(200));
                if (!string.IsNullOrEmpty(group_desc))
                {
                    group_desc = group_desc + "/" + ddes;
                }
                else
                {
                    group_desc = ddes;
                }
                if (group_desc != all_actions[curr_select_config_id].desc)
                {
                    all_actions[curr_select_config_id].desc = group_desc;
                    is_delay_save = true;
                    is_undo_add = true;
                }
                EditorGUILayout.LabelField("排序：", GUILayout.Width(30));
                int ssort = EditorGUILayout.IntField(all_actions[curr_select_config_id].sort, GUILayout.Width(40));
                if (ssort != all_actions[curr_select_config_id].sort)
                {
                    all_actions[curr_select_config_id].sort = ssort;
                    is_delay_save = true;
                    is_undo_add = true;
                }

                if (windowNodeData != null && undo_list_dic.TryGetValue(windowNodeData.id, out List<string> undo_list1))
                {
                    if (undo_list1.Count > 0)
                    {
                        undo_index_dic.TryGetValue(windowNodeData.id, out int index);
                        EditorGUILayout.LabelField($"{index},{undo_list1.Count - 1}", GUILayout.Width(50));
                    }
                }

                EditorGUILayout.EndHorizontal();
                GUILayout.EndArea();
            }

            if (is_delay_save && windowNodeData != null)
            {
                
                //if (save_invetavl < 0)
                //{
                //    save_invetavl = 0.8f;
                //    is_delay_save = false;
                //    string file = "action_" + windowNodeData.id;
                //    File.WriteAllText(action_save_path + "/" + file + ".json", JsonUtility.ToJson(windowNodeData.actionConfig));
                //}
                //save_invetavl -= Time.deltaTime;
            }

            if (is_undo_add && windowNodeData != null)
            {
                is_undo_add = false;
                if(!undo_list_dic.TryGetValue(windowNodeData.id, out List<string> undo_list)) 
                {
                    undo_list = new List<string>();
                    undo_list_dic.Add(windowNodeData.id, undo_list);
                }
                undo_list.Add(JsonUtility.ToJson(windowNodeData.actionConfig));

                if (undo_list.Count > 50)
                {
                    undo_list.RemoveAt(0);
                }

                if (!undo_index_dic.ContainsKey(windowNodeData.id))
                {
                    undo_index_dic.Add(windowNodeData.id, undo_list.Count);
                }
                else
                {
                    undo_index_dic[windowNodeData.id] = undo_list.Count;
                }
            }
        }

        private void SelectActionWindowById(WindowNodeData nodeData, int id)
        {
            if (nodeData != null)
            {
                if (nodeData.id == id)
                {
                    selectedWindow = nodeData;
                    return;
                }
                if (nodeData.windowNodeDatas != null && nodeData.windowNodeDatas.Count > 0)
                {
                    for (int i = 0; i < nodeData.windowNodeDatas.Count; i++)
                    {
                        SelectActionWindowById(nodeData.windowNodeDatas[i], id);
                    }
                }
            }
        }

        protected virtual void ExcuteAction()
        {
            var type = typeof(MyComponent).Assembly.GetType("GameBase.GameLoader");
            if (type != null)
            {
                var evt = type.GetMethod("CallLogic", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                if (evt != null)
                {
                    string json = JsonUtility.ToJson(windowNodeData.actionConfig);
                    var hash = new Hashtable() { ["id"] = windowNodeData.id, ["json"] = MiniJSON.JsonDecode(json) };
                    evt.Invoke(type, new object[] { -999998, hash });
                }
            }
        }

        protected virtual void ExtendOptionGUI()
        {
           
        }

        public void ShowMenu(WindowNodeData nodeData)
        {
            if (selectedWindow != null)
            {
                selectedWindow.selected = false;
            }
            selectedWindow = nodeData;
            selectedWindow.selected = true;
            menu.ShowAsContext();
        }

        private void Callback(object userData)
        {
            selectedWindow.AddChild(menu_dic[userData.ToString()]);

            Repaint();
        }

        public static void DrawConnectLine(Vector3 startPos, Vector3 endPos, Color color)
        {
            var startTangent = new Vector3(startPos.x + Mathf.Sign(endPos.x - startPos.x) * 10, startPos.y, startPos.z);

            var endTangent = new Vector3(endPos.x + Mathf.Sign(endPos.y - startPos.y) * 10, endPos.y, endPos.z);

            Handles.DrawBezier(startPos, endPos, startTangent, endTangent, color, null, 1);
        }

        public void UnSelectState()
        {
            if (selectedWindow != null)
            {
                ParseSelectLineNodeParent(selectedWindow, false);
                //ParseSelectLineNodeChild(selectedWindow, false);
                selectedWindow.is_line_selected = false;
                selectedWindow.selected = false;
            }
            selectedWindow = null;
        }

        public void OnNodeSelected(WindowNodeData data, bool is_repaint)
        {
            is_start_drag = false;
            UnSelectState();
            ParseSelectLineNodeParent(data, true);
            //ParseSelectLineNodeChild(data, true);

            data.is_line_selected = true;
            data.selected = true;
            selectedWindow = data;

            if (is_repaint)
            {
                Repaint();

                GUIUtility.keyboardControl = 0;
            }
        }

        private void ParseSelectLineNodeParent(WindowNodeData data, bool selected)
        {
            if (data.parent != null)
            {
                data.parent.is_line_selected = selected;
                ParseSelectLineNodeParent(data.parent, selected);
                //if (data.parent.IsSequence())
                //{
                //    ParseSelectLineNodeChild(data.parent, selected);
                //}
            }
        }

        private void ParseSelectLineNodeChild(WindowNodeData data, bool selected)
        {
            data.is_line_selected = selected;
            if (data.windowNodeDatas.Count > 0)
            {
                for (int i = 0; i < data.windowNodeDatas.Count; i++)
                {
                    ParseSelectLineNodeChild(data.windowNodeDatas[i], selected);
                }
            }
        }

        private void DrawBackground()
        {
            EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), backgroundColor);
        }

        Vector2 offset;

        private void DrawGrid(float gridSpacing, float gridOpacity)
        {
            int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
            int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);
            Handles.BeginGUI();
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);
            Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);
            for (int i = 0; i < widthDivs; i++)
            {
                Handles.DrawLine(new Vector3(gridSpacing * i, -gridSpacing, 0) + newOffset, new Vector3(gridSpacing * i, position.height, 0f) + newOffset);
            }
            for (int j = 0; j < heightDivs; j++)
            {
                Handles.DrawLine(new Vector3(-gridSpacing, gridSpacing * j, 0) + newOffset, new Vector3(position.width, gridSpacing * j, 0f) + newOffset);
            }
            Handles.color = Color.white;
            Handles.EndGUI();
        }

        public void FixWindownPosition(WindowNodeData curr, WindowNodeData windowNode)
        {
            if (windowNode == null)
            {
                return;
            }
            if (curr == null)
            {
                curr = windowNodeData;
            }
            if (curr != null && windowNode != curr)
            {
                if (Mathf.Abs(windowNode.rect.x - curr.rect.x) < 10)
                {
                    windowNode.rect.x = curr.rect.x;
                }
                if (Mathf.Abs(windowNode.rect.y - curr.rect.y) < 10)
                {
                    windowNode.rect.y = curr.rect.y;
                }
                if (curr.windowNodeDatas != null)
                {
                    for (int i = 0; i < curr.windowNodeDatas.Count; i++)
                    {
                        FixWindownPosition(curr.windowNodeDatas[i], windowNode);
                    }
                }
            }
        }

        //自动排列行为树
        public Vector2 AutoSortFormWinDataLayout(WindowNodeData nodeData, Vector2 pos)
        {
            if (nodeData.windowNodeDatas != null && nodeData.windowNodeDatas.Count > 0)
            {
                if (nodeData.parent != null)
                {
                    if (nodeData.IsSequence())
                    {
                        pos.y += 50;
                    }
                    else
                    {
                        pos.y += nodeData.rect.height + 10;
                    }
                }
                for (int i = 0; i < nodeData.windowNodeDatas.Count; i++)
                {
                    var node = nodeData.windowNodeDatas[i];
                    pos.x += nodeData.rect.width + 20;
                    node.rect.position = pos;
                    pos.x = AutoSortFormWinDataLayout(node, pos).x;
                }
            }
            return pos;
        }

        public static string GenActionJson(string _namespace)
        {
            Assembly assembly = typeof(UnityEngine.UI.MyImage).Assembly;
            var classname = _namespace + ".BaseActionNode";
            var type = assembly.GetType(classname);
            if (type == null)
            {
                return null;
            }
            StringBuilder sb = new StringBuilder();
            var types = assembly.GetTypes();
            foreach (var _type in types)
            {
                if (_type.Namespace == _namespace)
                {
                    var attributes = _type.GetCustomAttributes(true);
                    if (attributes != null && attributes.Length > 0)
                    {
                        var attri = attributes[0];
                        if (attri.GetType().Name == "ActionConfigAttribute")
                        {
                            string desc = (string)attri.GetType().GetField("desc").GetValue(attri);
                            ActionMenuJson triggerConfig = new ActionMenuJson();
                            triggerConfig.node_title = desc;
                            triggerConfig.class_name = _type.Name;
                            triggerConfig.order = (int)attri.GetType().GetField("order").GetValue(attri);
                            triggerConfig.is_have_parallel = (bool)attri.GetType().GetField("is_have_parallel").GetValue(attri);
                            object tobj = System.Activator.CreateInstance(_type);
                            var fields = _type.GetFields();
                            foreach (var field in fields)
                            {
                                var fieldAttris = field.GetCustomAttributes(true);
                                if (fieldAttris != null && fieldAttris.Length > 0 && fieldAttris[0].GetType().Name == "ActionConfigAttribute")
                                {
                                    ActionMenuField config = new ActionMenuField();
                                    config.desc = (string)fieldAttris[0].GetType().GetField("desc").GetValue(fieldAttris[0]);
                                    config.field = field.Name;
                                    config.type = field.FieldType.Name;
                                    var obj = field.GetValue(tobj);
                                    if (obj != null)
                                    {
                                        if (config.type == type_int_array)
                                        {
                                            config.value = string.Join(",", (int[])obj);
                                        }
                                        else if (config.type == type_float_array)
                                        {
                                            config.value = string.Join(",", (float[])obj);
                                        }
                                        else if (config.type == type_string_array)
                                        {
                                            config.value = string.Join(",", (string[])obj);
                                        }
                                        else if (config.type == "BaseActionNode[]" || config.type == "BaseActionNode")
                                        {
                                            config.value = "";
                                            triggerConfig.is_container = true;
                                        }
                                        else
                                        {
                                            config.value = obj.ToString();
                                        }
                                    }
                                    config.enums = (string[])fieldAttris[0].GetType().GetField("enum_types").GetValue(fieldAttris[0]);
                                    if (config.enums != null && config.enums.Length > 0)
                                    {
                                        config.type = type_enum;
                                    }
                                    triggerConfig.fields.Add(config);
                                }
                            }
                            sb.AppendLine(JsonUtility.ToJson(triggerConfig));
                        }
                    }
                }
            }
            return sb.ToString();
        }
    }

    //namespace DC.DCIMGUIBox
    //{
    //    public class WindowNodeData
    //    {
    //        public Rect rect;

    //        public bool selected;
    //    }

    //    public class DragWindow : EditorWindow
    //    {
    //        [MenuItem("DC/IMGUI/DragWindow")]
    //        public static void Open()
    //        {
    //            var window = GetWindow<DragWindow>();
    //            window.minSize = new Vector2(800, 600);
    //        }

    //        public List<WindowNodeData> nodeDataList = new List<WindowNodeData>();

    //        List<Tuple<WindowNodeData, WindowNodeData>> lines = new List<Tuple<WindowNodeData, WindowNodeData>>();

    //        private WindowNodeData lastSelectedWindow = null;

    //        public void OnGUI()
    //        {
    //            GUILayout.BeginHorizontal();
    //            if (GUILayout.Button("add"))
    //            {
    //                nodeDataList.Add(new WindowNodeData() { rect = new Rect(100, 100, 200, 100) });
    //            }

    //            GUILayout.EndHorizontal();

    //            var col = new Color(0.8f, 0.3f, 0.3f);
    //            for (int i = 0; i < lines.Count; i++)
    //            {
    //                var line = lines[i];
    //                var startPos = line.Item1.rect.center;
    //                var endPos = line.Item2.rect.center;
    //                DrawConnectLine(new Vector3(startPos.x, startPos.y), new Vector3(endPos.x, endPos.y), col);
    //            }

    //            BeginWindows();

    //            for (int i = 0; i < nodeDataList.Count; i++)
    //            {
    //                nodeDataList[i].rect = GUILayout.Window(i, nodeDataList[i].rect, WindowNode, i.ToString());
    //            }

    //            EndWindows();
    //        }

    //        public static void DrawConnectLine(Vector3 startPos, Vector3 endPos, Color color)
    //        {
    //            var startTangent = new Vector3(startPos.x + Mathf.Sign(endPos.x - startPos.x) * 10, startPos.y, startPos.z);

    //            var endTangent = new Vector3(endPos.x + Mathf.Sign(endPos.y - startPos.y) * 10, endPos.y, endPos.z);

    //            Handles.DrawBezier(startPos, endPos, startTangent, endTangent, color, null, 1);
    //        }

    //        public void OnNodeSelected(WindowNodeData data)
    //        {
    //            for (int i = 0; i < nodeDataList.Count; i++)
    //            {
    //                if (nodeDataList[i] == data)
    //                {
    //                    continue;
    //                }

    //                nodeDataList[i].selected = false;
    //            }

    //            Repaint();

    //            if (lastSelectedWindow != null)
    //            {
    //                lines.Add(new Tuple<WindowNodeData, WindowNodeData>(lastSelectedWindow, data));
    //            }

    //            lastSelectedWindow = data;
    //        }

    //        public void WindowNode(int windowId)
    //        {
    //            var data = nodeDataList[windowId];

    //            GUI.DragWindow(new Rect(0, 0, data.rect.width, 16));
    //            var curEvt = Event.current;

    //            if (curEvt.type == EventType.MouseDown && curEvt.button == 0 &&
    //                new Rect(0, 16, data.rect.width, data.rect.height - 16).Contains(curEvt.mousePosition))
    //            {
    //                data.selected = true;
    //                OnNodeSelected(data);
    //            }

    //            if (data.selected)
    //            {
    //                EditorGUI.DrawRect(new Rect(0, 0, data.rect.width, data.rect.height), new Color(0.6f, 0, 0.4f, 0.3f));
    //            }

    //            GUILayout.Label(windowId.ToString());
    //        }
    //    }
    //}
}