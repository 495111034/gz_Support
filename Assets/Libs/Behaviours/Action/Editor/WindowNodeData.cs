using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace ActionEditor
{
    public class WindowNodeData
    {
        public Rect rect;
        public bool is_line_selected;
        public bool selected;
        public bool is_press = false;
        public bool is_trigger_move = false;
        public int id
        {
            get { return actionConfig.id; }
            set { actionConfig.id = value; }
        }
        public ActionEditorWindow actionEditor = null;
        public WindowNodeData parent;

        public List<WindowNodeData> windowNodeDatas = new List<WindowNodeData>();

        private Vector2 temp_rect;

        public ActionConfig actionConfig;
        public ActionEditorWindow.ActionMenuJson menu_json;

        private GUIContent close_content = null;
        private Rect top_rect = new Rect();
        private Rect full_select = new Rect();

        public bool is_remove = false;
        private List<WindowNodeData> removes = new List<WindowNodeData>();
        public bool is_parallel = false;

        public Color top_color = Color.white;
        public Color line1_color = Color.yellow;
        //public Color line2_color = Color.green;

        private Vector2 temp_pos;
        private int container_index = 0;
        private int node_index = 0;

        private string logs_text = "";
        private float runtime_time = 0;
        private float log_color_time = 0;
        public WindowNodeData(ActionEditorWindow _, ActionConfig __)
        {
            close_content = new GUIContent("x");

            actionEditor = _;
            this.actionConfig = __;
            menu_json = actionEditor.FindActionMenuJson(actionConfig.class_name);

            if (menu_json.is_have_parallel)
            {
                foreach (var v in actionConfig.fields)
                {
                    if (v.f == "isParallel")
                    {
                        is_parallel = v.v == "1" || v.v == "True" || v.v == "TRUE";
                    }
                }
            }

            if (menu_json.is_container)
            {
                for (int i = 0; i < actionEditor.container_keys.Length; i++)
                {
                    if (actionEditor.container_keys[i] == menu_json.node_title)
                    {
                        container_index = i;
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < actionEditor.node_type_keys.Length; i++)
                {
                    if (actionEditor.node_type_keys[i] == menu_json.node_title)
                    {
                        node_index = i;
                        break;
                    }
                }
            }

            rect = new Rect(actionConfig.x, actionConfig.y, actionEditor.default_size.x, actionEditor.default_size.y);

            if (actionConfig.childs.Count > 0)
            {
                line1_color = actionEditor.GetColor();
                windowNodeDatas = new List<WindowNodeData>();
                for (int i = 0; i < actionConfig.childs.Count; i++)
                {
                    WindowNodeData data = new WindowNodeData(actionEditor, actionConfig.childs[i]);
                    data.line1_color = line1_color;
                    windowNodeDatas.Add(data);
                }
                FixChildParent();
            }
        }

        string temp_log = "";
        public void UpdateLogData(int id, int state, string log)
        {
            if (this.id == id)
            {
                string tlog = (state == 0 ? "完成" : "执行") + ": " + log + "\n";
                if (!tlog.Equals(temp_log))
                {
                    temp_log = tlog;
                    logs_text += temp_log;
                }
                runtime_time = 1;
            }
            for (int i = 0; i < windowNodeDatas.Count; i++)
            {
                windowNodeDatas[i].UpdateLogData(id, state, log);
            }
        }

        public bool IsSequence()
        {
            if (menu_json.class_name == "Sequence")
            {
                return true;
            }
            else if (menu_json.is_have_parallel)
            {
                if (!is_parallel)
                {
                    return true;
                }
            }
            return false;
        }

        public void MoveTarget(WindowNodeData curr, bool isUp)
        {
            bool is_have = false;
            for (int i = 0; i < windowNodeDatas.Count; i++)
            {
                if (windowNodeDatas[i].id == curr.id)
                {
                    actionEditor.OnNodeSelected(curr, false);
                    is_have = true;
                    WindowNodeData temp = null;
                    if (isUp)
                    {
                        if (i > 0)
                        {
                            temp = windowNodeDatas[i - 1];
                            windowNodeDatas[i - 1] = curr;
                            actionConfig.childs[i - 1] = curr.actionConfig;
                        }
                    }
                    else if (i < windowNodeDatas.Count - 1)
                    {
                        temp = windowNodeDatas[i + 1];
                        windowNodeDatas[i + 1] = curr;
                        actionConfig.childs[i + 1] = curr.actionConfig;
                    }
                    if (temp != null)
                    {
                        Vector2 used_pos1 = curr.rect.position;
                        Vector2 used_pos2 = temp.rect.position;

                        actionConfig.childs[i] = temp.actionConfig;
                        windowNodeDatas[i] = temp;

                        MoveAllChild(temp, used_pos1 - temp.rect.position);
                        MoveAllChild(curr, used_pos2 - curr.rect.position);
                    }
                    break;
                }
            }
            if (!is_have)
            {
                if (this.parent != null)
                {
                    parent.MoveTarget(curr, isUp);
                }
                return;
            }
            FixChildParent();

            actionEditor.OnNodeSelected(curr, true);
        }

        public void AddChild(ActionEditorWindow.ActionMenuJson config)
        {
            if (windowNodeDatas.Count == 0)
            {
                line1_color = actionEditor.GetColor();
            }
            ActionConfig action = new ActionConfig();
            Rect temp_rect = rect;
            if (windowNodeDatas.Count > 0)
            {
                temp_rect = windowNodeDatas[windowNodeDatas.Count - 1].rect;
            }
            temp_rect.y += temp_rect.height;
            temp_rect.y += 20;
            action.x = temp_rect.x;
            action.y = temp_rect.y;
            action.desc = config.node_title;
            action.id = ++actionEditor.base_id;
            if (action.desc.IndexOf('/') > -1)
            {
                action.desc = action.desc.Substring(action.desc.IndexOf('/') + 1);
            }
            action.class_name = config.class_name;
            actionConfig.childs.Add(action);
            WindowNodeData data = new WindowNodeData(actionEditor, action);
            data.line1_color = line1_color;
            windowNodeDatas.Add(data);

            FixChildParent();
        }

        public void Draw(Vector2 mouse_drag, Vector2 global_size)
        {
            rect.position += mouse_drag;
            if (parent != null && global_size != temp_rect)
            {
                if (temp_rect == Vector2.zero)
                {
                    temp_rect = rect.size;
                }
                Vector2 dir = global_size - temp_rect;
                temp_rect = global_size;
                rect.position -= (parent.rect.position - rect.position).normalized * dir.magnitude * Mathf.Sign(dir.x);
            }
            rect.size = global_size;
            rect = GUILayout.Window(id, rect, DrawNodeWindow, menu_json.node_title);
            if (temp_pos.x != rect.x || temp_pos.y != rect.y)
            {
                actionEditor.is_delay_save = !Mathf.Approximately(actionConfig.x, rect.x) || !Mathf.Approximately(actionConfig.y, rect.y);
                if (actionEditor.is_delay_save)
                {
                    is_trigger_move = true;
                }
                if (is_press && windowNodeDatas != null && windowNodeDatas.Count > 0)
                {
                    Vector2 dir = rect.position - new Vector2(actionConfig.x, actionConfig.y);
                    for(int i = 0; i < windowNodeDatas.Count; i++)
                    {
                        windowNodeDatas[i].MoveSelfAndAllChildPosition(dir);
                    }
                }
                actionConfig.x = rect.x;
                actionConfig.y = rect.y;
            }
            temp_pos.x = rect.x;
            temp_pos.y = rect.y;

            if (parent != null)
            {
                var line_start = parent.rect;
                if (parent.menu_json.class_name == "Parallel")
                {
                    DrawNodeCurve(line_start, rect, line1_color);
                }
                else if (parent.menu_json.is_have_parallel)
                {
                    if (parent.is_parallel)
                    {
                        DrawNodeCurve(line_start, rect, line1_color);
                    }
                    else
                    {
                        DrawNodeCurve(line_start, rect, line1_color);
                    }
                }
                else
                {
                    DrawNodeCurve(line_start, rect, line1_color);
                }
            }

            if (windowNodeDatas.Count > 0)
            {
                for (int i = 0; i < windowNodeDatas.Count; i++)
                {
                    windowNodeDatas[i].Draw(mouse_drag, global_size);
                    if (windowNodeDatas[i].is_remove)
                    {
                        if (i < windowNodeDatas.Count - 1)
                        {
                            if (windowNodeDatas[i + 1].parent == windowNodeDatas[i])
                            {
                                if (i > 0)
                                {
                                    windowNodeDatas[i + 1].parent = windowNodeDatas[i - 1];
                                }
                                else
                                {
                                    windowNodeDatas[i + 1].parent = this;
                                }
                            }
                        }
                        actionConfig.childs.Remove(windowNodeDatas[i].actionConfig);
                        removes.Add(windowNodeDatas[i]);
                    }
                }
                if (removes.Count > 0)
                {
                    actionEditor.is_undo_add = true;
                    actionEditor.is_delay_save = true;
                    for (int i = 0; i < removes.Count; i++)
                    {
                        windowNodeDatas.Remove(removes[i]);
                    }
                    removes.Clear();
                    actionEditor.UnSelectState();
                }
            }

            
            if (this == actionEditor.selectedWindow)
            {
                float pos_y = rect.y;
                float pos_x = rect.x + rect.width;
                if (parent != null)
                {
                    GUI.color = Color.red;
                    if (GUI.Button(new Rect(pos_x, pos_y, 20, 20), close_content))
                    {
                        if (EditorUtility.DisplayDialog("提示", "确定要删除吗？", "ok", "cancel"))
                        {
                            is_remove = true;
                        }
                    }
                    pos_y += 20;
                    GUI.color = Color.white;

                    if (IsParentSequence(this))
                    {
                        if (GUI.Button(new Rect(pos_x, pos_y, 30, 20), "↑"))
                        {
                            parent.MoveTarget(this, true);
                        }
                        pos_y += 20;
                        if (GUI.Button(new Rect(pos_x, pos_y, 30, 20), "↓"))
                        {
                            parent.MoveTarget(this, false);
                        }
                        pos_y += 20;
                    }
                }
                if (GUI.Button(new Rect(pos_x, pos_y, 30, 20), "复"))
                {
                    actionEditor.copy_json = JsonUtility.ToJson(this.actionConfig);
                }
                if (GUI.Button(new Rect(pos_x, pos_y + 20, 30, 20), "粘"))
                {
                    if (menu_json.is_container)
                    {
                        if (!string.IsNullOrEmpty(actionEditor.copy_json))
                        {
                            ActionConfig action = JsonUtility.FromJson<ActionConfig>(actionEditor.copy_json);
                            RestAllId(action);
                            Rect temp_rect = rect;
                            if (windowNodeDatas.Count > 0)
                            {
                                temp_rect = windowNodeDatas[windowNodeDatas.Count - 1].rect;
                            }
                            temp_rect.y += temp_rect.height;
                            temp_rect.y += 20;

                            Vector2 dir = temp_rect.position - new Vector2(action.x, action.y);
                            MoveAllChild(action, dir);

                            actionConfig.childs.Add(action);
                            WindowNodeData data = new WindowNodeData(actionEditor, action);
                            data.line1_color = line1_color;
                            if (windowNodeDatas.Count > 0 && IsSequence())
                            {
                                data.parent = windowNodeDatas[windowNodeDatas.Count - 1];
                            }
                            else
                            {
                                data.parent = this;
                            }
                            windowNodeDatas.Add(data);
                        }

                        FixChildParent();
                    }
                }
            }
        }

        private bool IsParentSequence(WindowNodeData nodeData)
        {
            if (nodeData.parent != null)
            {
                if (nodeData.parent.windowNodeDatas != null)
                {
                    if (nodeData.parent.windowNodeDatas.Contains(nodeData))
                    {
                        return nodeData.parent.IsSequence();
                    }
                }
                return IsParentSequence(nodeData.parent);
            }
            return false;
        }

        private void RestAllId(ActionConfig action)
        {
            action.id = ++actionEditor.base_id;
            if (action.childs != null)
            {
                for (int i = 0; i < action.childs.Count; i++)
                {
                    RestAllId(action.childs[i]);
                }
            }
        }

        private void MoveAllChild(WindowNodeData action, Vector2 dir)
        {
            action.rect.position = action.rect.position + dir;
            if (action.windowNodeDatas != null)
            {
                for (int i = 0; i < action.windowNodeDatas.Count; i++)
                {
                    MoveAllChild(action.windowNodeDatas[i], dir);
                }
            }
        }

        private void MoveAllChild(ActionConfig action, Vector2 dir)
        {
            var pos = new Vector2(action.x, action.y) + dir;
            action.x = pos.x;
            action.y = pos.y;
            if (action.childs != null)
            {
                for (int i = 0; i < action.childs.Count; i++)
                {
                    MoveAllChild(action.childs[i], dir);
                }
            }
        }

        void DrawNodeWindow(int id)
        {
            if (id == this.id)
            {
                //这里是渲染窗口的，和获取鼠标事件的
                top_rect.width = rect.width;
                top_rect.height = 20;
                if (this.parent == null)
                {
                    top_color = Color.red;
                    top_color.a = 0.3f;
                    EditorGUI.DrawRect(top_rect, top_color);
                }
                else if (menu_json.class_name == "Parallel")
                {
                    top_color = new Color(0, 1, 0, 0.3f);
                    EditorGUI.DrawRect(top_rect, top_color);
                }
                else if (menu_json.is_have_parallel && menu_json.class_name != "Condition")
                {
                    if (is_parallel)
                    {
                        top_color = new Color(0, 1, 0, 0.3f);
                        EditorGUI.DrawRect(top_rect, top_color);
                    }
                    else
                    {
                        top_color = Color.green;
                        top_color.a = 0.3f;
                        EditorGUI.DrawRect(top_rect, top_color);
                    }
                }
                else if (menu_json.class_name == "Condition" || menu_json.node_title.StartsWith("条件"))
                {
                    top_color = Color.yellow;
                    top_color.a = 0.3f;
                    EditorGUI.DrawRect(top_rect, top_color);
                }
                else if (menu_json.is_container)
                {
                    top_color = Color.green;
                    top_color.a = 0.3f;
                    EditorGUI.DrawRect(top_rect, top_color);
                }
                else
                {
                    top_color = Color.blue;
                    top_color.a = 0.3f;
                    EditorGUI.DrawRect(top_rect, top_color); 
                }

                full_select.size = rect.size;

                if (runtime_time > 0)
                {
                    runtime_time -= Time.deltaTime;
                    top_color = Color.Lerp(Color.white, Color.red, log_color_time);
                    EditorGUI.DrawRect(full_select, top_color);
                    log_color_time += Time.deltaTime;
                    if (log_color_time > 1) log_color_time = 0;
                }

                if (selected)
                {
                    top_color = new Color(0, 1, 0, 0.2f);
                    EditorGUI.DrawRect(full_select, top_color);
                }
                var curEvt = Event.current;
                if (selected)
                {
                    if (curEvt.type == EventType.MouseDown && curEvt.button == 0)
                    {
                        actionEditor.is_start_drag = false;
                        is_press = true;
                    }
                    else if(curEvt.type == EventType.MouseUp && curEvt.button == 0)
                    {
                        if (is_trigger_move)
                        {
                            actionEditor.is_undo_add = true;
                        }
                        is_trigger_move = false;
                        actionEditor.FixWindownPosition(actionEditor.windowNodeData, this);
                        is_press = false;
                    }
                }
                else
                {
                    if (curEvt.type == EventType.MouseDown && curEvt.button == 0)
                    {
                        is_press = true;
                        if (rect.x + rect.width > actionEditor.position.width - actionEditor.menu_right.width)
                        { }
                        else 
                        { 
                            actionEditor.OnNodeSelected(this, true);
                        }
                    }
                }
                if (menu_json.is_container)
                {
                    if (curEvt.type == EventType.MouseDown && curEvt.button == 1)
                    {
                        actionEditor.ShowMenu(this);
                        curEvt.Use();
                    }
                }

                GUI.DragWindow(full_select);
                //以下是绘制控件的
                GUILayout.Label(id.ToString());
                GUILayout.TextArea(actionConfig.desc, GUILayout.MaxWidth(rect.width));
            }
        }

        void DrawNodeCurve(Rect start, Rect end, Color color)
        {
            Vector3 startPos = new Vector3(start.x + start.width * 0.5f, start.y + 100 * 0.5f, 0);
            Vector3 endPos = new Vector3(end.x + end.width * 0.5f, end.y + 100 * 0.5f, 0);
            Vector3 startTan = startPos + Vector3.down * 10;
            Vector3 endTan = endPos + Vector3.down * 10;
            Color shadowCol = new Color(1, 1, 1, is_line_selected ? 0.5f : 0.1f);
            for (int i = 0; i < 3; i++)
            {
                Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * 5);
            }
            Handles.DrawBezier(startPos, endPos, startTan, endTan, color, null, 2);
        }


        private Vector2 field_scroll_ve2 = Vector2.zero;

        public void DrawField()
        {
            field_scroll_ve2 = EditorGUILayout.BeginScrollView(field_scroll_ve2);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("id:", GUILayout.Width(20));
            EditorGUILayout.IntField(actionConfig.id);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("简介:", GUILayout.Width(40));
            if (parent == null)
            {
                EditorGUILayout.LabelField(actionConfig.desc);
            }
            else
            {
                actionConfig.desc = EditorGUILayout.TextField(actionConfig.desc);
            }
            EditorGUILayout.EndHorizontal();

            if (menu_json.is_container)
            {
                int index = EditorGUILayout.Popup(container_index, actionEditor.container_keys);
                if (container_index != index)
                {
                    container_index = index;
                    menu_json = actionEditor.menu_dic[actionEditor.container_keys[index]];
                    actionConfig.class_name = menu_json.class_name;
                    actionConfig.fields = new List<ActionField>();
                    is_parallel = false;

                    FixChildParent();
                }
            }
            else
            {
                int index = EditorGUILayout.Popup(node_index, actionEditor.node_type_keys);
                if (node_index != index)
                {
                    node_index = index;
                    menu_json = actionEditor.menu_dic[actionEditor.node_type_keys[index]];
                    actionConfig.class_name = menu_json.class_name;
                    actionConfig.fields = new List<ActionField>();
                    is_parallel = false;
                }
                //EditorGUILayout.LabelField(menu_json.node_title);
            }
            EditorGUILayout.Space();

            if (menu_json.fields != null)
            {
                if (menu_json.fields.Count != actionConfig.fields.Count)
                {
                    FixRestJsonConfig();
                }
                for (int x = 0; x < menu_json.fields.Count; x++)
                {
                    var field_config = menu_json.fields[x];
                    var curr_json_config = actionConfig.fields[x];
                    if (!field_config.field.Equals(curr_json_config.f))
                    {
                        FixRestJsonConfig();
                        curr_json_config = actionConfig.fields[x];
                    }
                    if (field_config.type.Contains("BaseActionNode"))
                    {
                        continue;
                    }

                    if (field_config.field == "ret_un_opt" || field_config.field == "ret_finish_opt" || field_config.field == "ret_opt")
                    {
                        EditorGUILayout.LabelField(field_config.desc);
                    }
                    else
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField(field_config.desc, GUILayout.Width(140));
                        if (GUILayout.Button("copy", GUILayout.Width(40)))
                        {
                            GUIUtility.systemCopyBuffer = field_config.field;
                        }
                        EditorGUILayout.EndHorizontal();
                    }
                    var temp_v = curr_json_config.v;
                    if (field_config.type == ActionEditorWindow.type_int)
                    {
                        int.TryParse(curr_json_config.v, out int v);
                        v = EditorGUILayout.IntField(v);
                        curr_json_config.v = v.ToString();
                    }
                    else if (field_config.type == ActionEditorWindow.type_float)
                    {
                        float.TryParse(curr_json_config.v, out float v);
                        v = EditorGUILayout.FloatField(v);
                        curr_json_config.v = v.ToString();
                    }
                    else if (field_config.type == ActionEditorWindow.type_bool)
                    {
                        bool v = EditorGUILayout.Toggle(curr_json_config.v == "1" || curr_json_config.v == "True" || curr_json_config.v == "TRUE");
                        curr_json_config.v = v.ToString();
                    }
                    else if (field_config.type == ActionEditorWindow.type_string || field_config.type == ActionEditorWindow.type_int_array || field_config.type == ActionEditorWindow.type_float_array || field_config.type == ActionEditorWindow.type_string_array)
                    {
                        curr_json_config.v = EditorGUILayout.TextField(curr_json_config.v);
                    }
                    else if (field_config.type == ActionEditorWindow.type_Vector2)
                    {
                        Vector2 v = Vector2.zero;
                        if (!string.IsNullOrEmpty(curr_json_config.v))
                        {
                            string[] te = curr_json_config.v.Split(',');
                            float.TryParse(te[0], out v.x);
                            float.TryParse(te[1], out v.y);
                        }
                        v = EditorGUILayout.Vector2Field("", v);
                        curr_json_config.v = $"{v.x},{v.y}";
                    }
                    else if (field_config.type == ActionEditorWindow.type_Vector3)
                    {
                        Vector3 v = Vector3.zero;
                        if (!string.IsNullOrEmpty(curr_json_config.v))
                        {
                            string[] te = curr_json_config.v.Split(',');
                            float.TryParse(te[0], out v.x);
                            float.TryParse(te[1], out v.y);
                            float.TryParse(te[2], out v.z);
                        }
                        v = EditorGUILayout.Vector3Field("", v);
                        curr_json_config.v = $"{v.x},{v.y},{v.z}";
                    }
                    else if (field_config.type == ActionEditorWindow.type_enum)
                    {
                        int.TryParse(curr_json_config.v, out int v);
                        v = EditorGUILayout.Popup(v, field_config.enums);
                        curr_json_config.v = v.ToString();
                    }
                    if (temp_v != curr_json_config.v)
                    {
                        actionEditor.is_undo_add = true;
                        actionEditor.is_delay_save = true;
                        if (menu_json.is_have_parallel)
                        {
                            if (field_config.field == "isParallel")
                            {
                                is_parallel = curr_json_config.v == "1" || curr_json_config.v == "True" || curr_json_config.v == "TRUE";
                                FixChildParent();
                            }
                        }
                    }
                }
            }

            if (logs_text != "")
            {
                EditorGUILayout.TextArea(logs_text);
            }

            EditorGUILayout.EndScrollView();
        }

        private void FixRestJsonConfig()
        {
            var temp = new List<ActionField>(actionConfig.fields);
            actionConfig.fields.Clear();
            for (int x = 0; x < menu_json.fields.Count; x++)
            {
                var c1 = menu_json.fields[x];
                var json_field = new ActionField();
                json_field.f = c1.field;
                json_field.v = c1.value;
                foreach (var v in temp)
                {
                    if (v.f.Equals(c1.field))
                    {
                        json_field.v = v.v;
                        break;
                    }
                }
                actionConfig.fields.Add(json_field);
            }
        }

        private void FixChildParent()
        {
            WindowNodeData temp = this;
            for (int i = 0; i < windowNodeDatas.Count; i++)
            {
                windowNodeDatas[i].parent = temp;
                windowNodeDatas[i].line1_color = line1_color;
                if (IsSequence())
                {
                    temp = windowNodeDatas[i];
                }
            }
        }

        public void MoveSelfAndAllChildPosition(Vector2 dir)
        {
            this.rect.position += dir;
            if (windowNodeDatas != null)
            {
                for (int i = 0; i < windowNodeDatas.Count; i++)
                {
                    windowNodeDatas[i].MoveSelfAndAllChildPosition(dir);
                }
            }
        }
    }
}
