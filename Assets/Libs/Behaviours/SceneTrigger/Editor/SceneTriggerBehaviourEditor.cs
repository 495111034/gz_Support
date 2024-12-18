using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.IO;
using System.Text;

/// <summary>
/// 场景通用触发器编辑扩展
/// </summary>
[CustomEditor(typeof(SceneTriggerBehaviour), true)]
public class SceneTriggerBehaviourEditor : Editor
{
    [MenuItem("Editor/场景触发器/强制刷新所有触发器类型json")]
    private static void ForceRefsh()
    {
        SceneTriggerBehaviourEditor.triggerConfigDic.Clear();
    }

    [MenuItem("Editor/场景触发器/生成所有触发器类型json")]
    private static void GenTriggerEventJson()
    {
        StringBuilder sb = new StringBuilder();
        StringBuilder sb2 = new StringBuilder();
        Assembly assembly = typeof(SceneTriggerBehaviour).Assembly;
        var classname = "GameLogic.SceneTrigger.BaseSceneTrigger";
        var type = assembly.GetType(classname);
        if (type == null)
        {
            return;
            //assembly = Assembly.LoadFile(Application.dataPath.Replace("Assets", $"Assets/Plugins/dlls/mn_android/GameLogic.bytes"));
        }
        var types = assembly.GetTypes();
        foreach (var _type in types)
        {
            if (_type.Namespace == "GameLogic.SceneTrigger")
            {
                var attributes = _type.GetCustomAttributes(true);
                if (attributes != null && attributes.Length > 0)
                {
                    var attri = attributes[0];
                    if (attri.GetType().Name == "TriggerConfig")
                    {
                        string desc = (string)attri.GetType().GetField("desc").GetValue(attri);
                        int event_id = (int)attri.GetType().GetField("event_id").GetValue(attri);
                        TriggerConfig triggerConfig = new TriggerConfig();
                        triggerConfig.desc = desc;
                        triggerConfig.eventId = event_id;
                        object tobj = System.Activator.CreateInstance(_type);
                        var fields = _type.GetFields();
                        foreach (var field in fields)
                        {
                            var fieldAttris = field.GetCustomAttributes(true);
                            if (fieldAttris != null && fieldAttris.Length > 0 && fieldAttris[0].GetType().Name == "TriggerConfig")
                            {
                                FieldConfig config = new FieldConfig();
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
                                    else if (config.type == type_Vector2)
                                    {
                                        config.value = obj.ToString().TrimStart('(').TrimEnd(')');
                                    }
                                    else if (config.type == type_Vector3)
                                    {
                                        config.value = obj.ToString().TrimStart('(').TrimEnd(')');
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
                                triggerConfig.fieldConfigs.Add(config);
                            }
                        }
                        sb.AppendLine(desc + "@" + JsonUtility.ToJson(triggerConfig));
                    }
                    else if (attri.GetType().Name == "TriggerTypeAttribute")
                    {
                        string desc = (string)attri.GetType().GetField("desc").GetValue(attri);
                        int event_id = (int)attri.GetType().GetField("type").GetValue(attri);
                        TriggerConfig triggerConfig = new TriggerConfig();
                        triggerConfig.desc = desc;
                        triggerConfig.eventId = event_id;
                        object tobj = System.Activator.CreateInstance(_type);
                        var fields = _type.GetFields();
                        foreach (var field in fields)
                        {
                            var fieldAttris = field.GetCustomAttributes(true);
                            if (fieldAttris != null && fieldAttris.Length > 0 && fieldAttris[0].GetType().Name == "TriggerConfig")
                            {
                                FieldConfig config = new FieldConfig();
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
                                triggerConfig.fieldConfigs.Add(config);
                            }
                        }
                        sb2.AppendLine(desc + "@" + JsonUtility.ToJson(triggerConfig));
                    }
                }
            }
        }

        File.WriteAllText(Application.dataPath + "/Libs/Behaviours/SceneTrigger/Editor/SceneTriggerBehaviourJson.txt", sb.ToString(), System.Text.UTF8Encoding.UTF8);
        File.WriteAllText(Application.dataPath + "/Libs/Behaviours/SceneTrigger/Editor/SceneTriggerTypeJson.txt", sb2.ToString(), System.Text.UTF8Encoding.UTF8);
    }

    [System.Serializable]
    public class FieldConfig
    {
        public string desc;
        public string field;
        public string type;
        public string value;
        public string[] enums;
    }

    [System.Serializable]
    public class TriggerConfig
    {
        public int eventId;
        public string desc;
        public List<FieldConfig> fieldConfigs = new List<FieldConfig>();
    }


    [System.Serializable]
    public class JsonField
    {
        public string f;
        public string v;
    }

    [System.Serializable]
    public class JsonConfig
    {
        public int id = 0;
        public int eventId = 0;
        public List<JsonField> fields = new List<JsonField>();
        public List<TriggerTypeJson> triggers = new List<TriggerTypeJson>();
    }

    [System.Serializable]
    public class TriggerTypeJson
    {
        public int eventId = 0;
        public bool is_group = false;
        public List<JsonField> fields = new List<JsonField>();
    }

    [System.Serializable]
    public class JsonConfigTemp
    {
        public int id = 0;
        public int eventId = 0;
        public List<JsonField> fields = new List<JsonField>();

        public int trigger_type = 0;
        public List<JsonField> type_fields = new List<JsonField>();
    }

    private static Dictionary<string, TriggerConfig> triggerConfigDic = new Dictionary<string, TriggerConfig>();
    private static string[] trigger_menus = new string[0];
    private static Dictionary<string, TriggerConfig> triggerTypeConfigDic = new Dictionary<string, TriggerConfig>();
    private static string[] triggerType_menus = new string[0];

    private static string type_int = typeof(int).Name;
    private static string type_float = typeof(float).Name;
    private static string type_bool = typeof(bool).Name;
    private static string type_string = typeof(string).Name;
    private static string type_int_array = typeof(int[]).Name;
    private static string type_float_array = typeof(float[]).Name;
    private static string type_string_array = typeof(string[]).Name;
    private static string type_Vector2 = typeof(Vector2).Name;
    private static string type_Vector3 = typeof(Vector3).Name;
    private static string type_enum = "enum";

    private SceneTriggerBehaviour behaviour;
    private List<JsonConfig> json_configs = null;
    private List<int> event_indexs = null;
    private List<bool> toggles = null;
    private static System.DateTime stamDateTime = new System.DateTime(2022, 11, 2);

    private GUIContent triggerContent = null;

    private static int base_id = 0;

    private void LoadConfig()
    {
        triggerContent = new GUIContent("触发方式：");
        triggerContent.tooltip = "勾选变绿表示所有绿色的要同时达成，否则任意达成一个就会触发";

        if (triggerConfigDic == null || triggerConfigDic.Count == 0)
        {
            string path = Application.dataPath + "/Libs/Behaviours/SceneTrigger/Editor/SceneTriggerBehaviourJson.txt";
            if (!File.Exists(path))
            {
                Debug.LogError("没有SceneTriggerBehaviourJson文件: " + path);
                return;
            }
            triggerConfigDic.Clear();
            trigger_menus = new string[0];
            StringReader sr = new StringReader(File.ReadAllText(path, System.Text.UTF8Encoding.UTF8));
            while (sr.Peek() > 1)
            {
                string line = sr.ReadLine().Trim();
                line = line.Trim(new char[] { '\r', '\n', '\t' });
                string[] te = line.Split('@');
                triggerConfigDic[te[0]] = JsonUtility.FromJson<TriggerConfig>(te[1]);
            }
            sr.Close();
            sr.Dispose();
            trigger_menus = new List<string>(triggerConfigDic.Keys).ToArray();

            path = Application.dataPath + "/Libs/Behaviours/SceneTrigger/Editor/SceneTriggerTypeJson.txt";
            if (!File.Exists(path))
            {
                Debug.LogError("没有SceneTriggerTypeJson文件: " + path);
                return;
            }
            triggerTypeConfigDic.Clear();
            triggerType_menus = new string[0];
            sr = new StringReader(File.ReadAllText(path, System.Text.UTF8Encoding.UTF8));
            while (sr.Peek() > 1)
            {
                string line = sr.ReadLine().Trim();
                line = line.Trim(new char[] { '\r', '\n', '\t' });
                string[] te = line.Split('@');
                triggerTypeConfigDic[te[0]] = JsonUtility.FromJson<TriggerConfig>(te[1]);
            }
            sr.Close();
            sr.Dispose();
            triggerType_menus = new List<string>(triggerTypeConfigDic.Keys).ToArray();

            InitData();
        }
    }

    private void OnEnable()
    {
        if (base_id == 0)
        {
            base_id = (int)(System.DateTime.Now - stamDateTime).TotalSeconds;
        }

        LoadConfig();

        behaviour = (SceneTriggerBehaviour)target;

        InitData();
    }

    private void InitData()
    {
        if (behaviour == null)
        {
            return;
        }
        if (behaviour.parmas_jsons == null || behaviour.parmas_jsons.Length == 0)
        {
            behaviour.parmas_jsons = new string[1];
        }
        int len = behaviour.parmas_jsons.Length;
        event_indexs = new List<int>();
        toggles = new List<bool>();
        json_configs = new List<JsonConfig>();
        for (int i = 0; i < len; i++)
        {
            event_indexs.Add(0);
            toggles.Add(false);
            json_configs.Add(null);
            InitJson(i);
        }
    }

    private void InitJson(int index, int event_index = -1)
    {
        JsonConfig json_config = null;
        string parmas_json = index < behaviour.parmas_jsons.Length ? behaviour.parmas_jsons[index] : null;
        if (event_index != -1 || string.IsNullOrEmpty(parmas_json))
        {
            bool change = true;
            if (event_index == -1)
            {
                change = false;
                event_index = 0;
            }
            var trigger_config = triggerConfigDic[trigger_menus[event_index]];
            var triggerType_config = triggerTypeConfigDic[triggerType_menus[0]];
            json_config = new JsonConfig();
            json_config.eventId = trigger_config.eventId;
            if (!change)
            {
                json_config.triggers = new List<TriggerTypeJson>();
                json_config.id = ++base_id;
            }
            else
            {
                json_config.triggers = json_configs[index].triggers;
                json_config.id = json_configs[index].id;
            }
            json_config.fields = new List<JsonField>();
            for (int i = 0; i < trigger_config.fieldConfigs.Count; i++)
            {
                var c1 = trigger_config.fieldConfigs[i];
                var json_field = new JsonField();
                json_field.f = c1.field;
                json_field.v = c1.value;
                json_config.fields.Add(json_field);
            }
            if (!change)
            {
                var trigger = new TriggerTypeJson();
                trigger.eventId = triggerType_config.eventId;
                trigger.fields = new List<JsonField>();
                for (int i = 0; i < triggerType_config.fieldConfigs.Count; i++)
                {
                    var c1 = triggerType_config.fieldConfigs[i];
                    var json_field = new JsonField();
                    json_field.f = c1.field;
                    json_field.v = c1.value;
                    trigger.fields.Add(json_field);
                }
                json_config.triggers.Add(trigger);
            }
            parmas_json = JsonUtility.ToJson(json_config);
        }
        else
        {
            Hashtable hash = MiniJSON.JsonDecode(parmas_json) as Hashtable;
            if (!hash.Contains("triggers"))
            {
                var temp = JsonUtility.FromJson<JsonConfigTemp>(parmas_json);
                json_config = new JsonConfig();
                json_config.id = temp.id;
                json_config.eventId = temp.eventId;
                json_config.fields = temp.fields;
                json_config.triggers = new List<TriggerTypeJson>();
                json_config.triggers.Add(new TriggerTypeJson());
                json_config.triggers[0].eventId = temp.trigger_type;
                json_config.triggers[0].fields = temp.type_fields;
                parmas_json = JsonUtility.ToJson(json_config);
            }
            else
            {
                json_config = JsonUtility.FromJson<JsonConfig>(parmas_json);
            }
        }
        if (triggerConfigDic.Count != 0)
        {
            for (int i = 0; i < trigger_menus.Length; i++)
            {
                if (triggerConfigDic[trigger_menus[i]].eventId == json_config.eventId)
                {
                    event_indexs[index] = i;
                    break;
                }
            }
        }
        if (index < behaviour.parmas_jsons.Length)
        {
            behaviour.parmas_jsons[index] = parmas_json;
        }
        json_configs[index] = json_config;
    }

    private static int child_count = 0;

    int deleteIndex = -1;

    public override void OnInspectorGUI()
    {
        deleteIndex = -1;
        for (int i = 0; i < json_configs.Count; i++)
        {
            GUIDraw(i);
            EditorGUILayout.Space();
        }

        if (deleteIndex > -1)
        {
            json_configs.RemoveAt(deleteIndex);
            event_indexs.RemoveAt(deleteIndex);
            toggles.RemoveAt(deleteIndex);
            SetSetDirty(-1);
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("新增"))
        {
            int end_id = 0;
            if (json_configs.Count > 0)
            {
                end_id = json_configs[json_configs.Count - 1].id;
            }
            event_indexs.Add(0);
            toggles.Add(false);
            json_configs.Add(null);
            InitJson(json_configs.Count - 1);

            if (json_configs[json_configs.Count - 1].id == end_id)
            {
                json_configs[json_configs.Count - 1].id = end_id + 1;
            }

            SetSetDirty(-1);
        }

        if (GUILayout.Button("刷新触发器"))
        {
            triggerConfigDic.Clear();
            LoadConfig();
        }
    }

    private void GUIDraw(int i)
    {
        bool is_change = false;
        var json_config = json_configs[i];
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(40));
        if (behaviour.trigger_counts != null && behaviour.trigger_counts.Count > 0 && i < behaviour.trigger_counts.Count)
        {
            GUI.color = Color.green;
            EditorGUILayout.LabelField("已触发" + behaviour.trigger_counts[i] + "次");
            GUI.color = Color.white;
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        int edit_id = EditorGUILayout.IntField("触发器id: ", json_config.id);
        if (edit_id != json_config.id)
        {
            json_config.id = edit_id;
            is_change = true;
        }
        if (GUILayout.Button("移除"))
        {
            deleteIndex = i;
        }
        EditorGUILayout.EndHorizontal();
        int index = EditorGUILayout.Popup("事件类型", event_indexs[i], trigger_menus);
        if (event_indexs[i] != index)
        {
            event_indexs[i] = index;
            InitJson(i, index);
            json_config = json_configs[i];
            is_change = true;
        }
        EditorGUILayout.Space();
        DrawConfig(triggerConfigDic[trigger_menus[index]], json_config.fields, ref is_change);

        EditorGUILayout.BeginHorizontal();
        toggles[i] = EditorGUILayout.BeginFoldoutHeaderGroup(toggles[i], triggerContent);
        if (toggles[i] && GUILayout.Button("新增", GUILayout.ExpandWidth(true)))
        {
            is_change = true;
            var triggerType_config = triggerTypeConfigDic[triggerType_menus[0]];
            var trigger = new TriggerTypeJson();
            trigger.eventId = triggerType_config.eventId;
            trigger.fields = new List<JsonField>();
            for (int i0 = 0; i0 < triggerType_config.fieldConfigs.Count; i0++)
            {
                var c1 = triggerType_config.fieldConfigs[i0];
                var json_field = new JsonField();
                json_field.f = c1.field;
                json_field.v = c1.value;
                trigger.fields.Add(json_field);
            }
            json_config.triggers.Add(trigger);
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.EndHorizontal();
        if (toggles[i])
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("", GUILayout.Width(10));
            EditorGUILayout.BeginVertical();
            for (int x = 0; x < json_config.triggers.Count; x++)
            {
                var json_type = json_config.triggers[x];
                index = 0;
                if (triggerTypeConfigDic.Count != 0)
                {
                    for (int i0 = 0; i0 < triggerType_menus.Length; i0++)
                    {
                        if (triggerTypeConfigDic[triggerType_menus[i0]].eventId == json_type.eventId)
                        {
                            index = i0;
                            break;
                        }
                    }
                }
                EditorGUILayout.BeginHorizontal();
                if (json_type.is_group)
                {
                    GUI.color = Color.green;
                }
                bool is_group = EditorGUILayout.ToggleLeft("", json_type.is_group, GUILayout.Width(20));
                if (is_group != json_type.is_group)
                {
                    is_change = true;
                    json_type.is_group = is_group;
                }
                int new_index = EditorGUILayout.Popup(index, triggerType_menus, GUILayout.Width(200));
                GUI.color = Color.white;
                EditorGUILayout.Space();
                if (GUILayout.Button("移除", GUILayout.Width(60)))
                {
                    json_config.triggers.RemoveAt(x);
                    is_change = true;
                    SetSetDirty(i);
                    return;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("", GUILayout.Width(30));
                EditorGUILayout.BeginVertical();
                if (index != new_index)
                {
                    index = new_index;
                    var triggerType_config = triggerTypeConfigDic[triggerType_menus[index]];
                    json_type.eventId = triggerType_config.eventId;
                    json_type.fields = new List<JsonField>();
                    for (int x1 = 0; x1 < triggerType_config.fieldConfigs.Count; x1++)
                    {
                        var c1 = triggerType_config.fieldConfigs[x1];
                        var json_field = new JsonField();
                        json_field.f = c1.field;
                        json_field.v = c1.value;
                        json_type.fields.Add(json_field);
                    }
                    is_change = true;
                }
                DrawConfig(triggerTypeConfigDic[triggerType_menus[index]], json_type.fields, ref is_change);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
        else
        {
            string desc = "";
            for (int x = 0; x < json_config.triggers.Count; x++)
            {
                var json_type = json_config.triggers[x];
                index = 0;
                if (triggerTypeConfigDic.Count != 0)
                {
                    for (int i0 = 0; i0 < triggerType_menus.Length; i0++)
                    {
                        if (triggerTypeConfigDic[triggerType_menus[i0]].eventId == json_type.eventId)
                        {
                            index = i0;
                            break;
                        }
                    }
                }
                desc += triggerType_menus[index] + "\n";
            }
            EditorGUILayout.HelpBox(desc, MessageType.Info);
        }

        if (is_change)
        {
            SetSetDirty(i);
        }
    }

    private void DrawConfig(TriggerConfig trigger_config, List<JsonField> fields, ref bool is_change)
    {
        if (trigger_config.fieldConfigs != null)
        {
            if (trigger_config.fieldConfigs.Count != fields.Count)
            {
                FixRestJsonConfig(trigger_config, fields);
            }
            for (int x = 0; x < trigger_config.fieldConfigs.Count; x++)
            {
                var field_config = trigger_config.fieldConfigs[x];
                var curr_json_config = fields[x];
                if (!field_config.field.Equals(curr_json_config.f))
                {
                    FixRestJsonConfig(trigger_config, fields);
                    curr_json_config = fields[x];
                }
                var temp_v = curr_json_config.v;
                if (field_config.type == type_int)
                {
                    int.TryParse(curr_json_config.v, out int v);
                    v = EditorGUILayout.IntField(field_config.desc, v);
                    curr_json_config.v = v.ToString();
                }
                else if (field_config.type == type_float)
                {
                    float.TryParse(curr_json_config.v, out float v);
                    v = EditorGUILayout.FloatField(field_config.desc, v);
                    curr_json_config.v = v.ToString();
                }
                else if (field_config.type == type_bool)
                {
                    bool v = EditorGUILayout.Toggle(field_config.desc, curr_json_config.v == "1" || curr_json_config.v == "True" || curr_json_config.v == "TRUE");
                    curr_json_config.v = v.ToString();
                }
                else if (field_config.type == type_string || field_config.type == type_int_array || field_config.type == type_float_array || field_config.type == type_string_array)
                {
                    curr_json_config.v = EditorGUILayout.TextField(field_config.desc, curr_json_config.v);
                }
                else if (field_config.type == type_Vector2)
                {
                    Vector2 v = Vector2.zero;
                    if (!string.IsNullOrEmpty(curr_json_config.v))
                    {
                        string[] te = curr_json_config.v.Split(',');
                        float.TryParse(te[0], out v.x);
                        float.TryParse(te[1], out v.y);
                    }
                    v = EditorGUILayout.Vector2Field(field_config.desc, v);
                    curr_json_config.v = $"{v.x},{v.y}";
                }
                else if (field_config.type == type_Vector3)
                {
                    Vector3 v = Vector3.zero;
                    if (!string.IsNullOrEmpty(curr_json_config.v))
                    {
                        string[] te = curr_json_config.v.Split(',');
                        float.TryParse(te[0], out v.x);
                        float.TryParse(te[1], out v.y);
                        float.TryParse(te[2], out v.z);
                    }
                    v = EditorGUILayout.Vector3Field(field_config.desc, v);
                    curr_json_config.v = $"{v.x},{v.y},{v.z}";
                }
                else if (field_config.type == type_enum)
                {
                    int.TryParse(curr_json_config.v, out int v);
                    v = EditorGUILayout.Popup(field_config.desc, v, field_config.enums);
                    curr_json_config.v = v.ToString();
                }

                if (temp_v != curr_json_config.v)
                {
                    is_change = true;
                }
            }
        }
    }

    private void FixRestJsonConfig(TriggerConfig trigger_config, List<JsonField> fields)
    {
        var temp = new List<JsonField>(fields);
        fields.Clear();
        for (int x = 0; x < trigger_config.fieldConfigs.Count; x++)
        {
            var c1 = trigger_config.fieldConfigs[x];
            var json_field = new JsonField();
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
            fields.Add(json_field);
        }
    }

    private void SetSetDirty(int index)
    {
        EditorUtility.SetDirty(target);

        if (index == -1)
        {
            int len = json_configs.Count;
            behaviour.parmas_jsons = new string[len];
            for (int i = 0; i < len; i++)
            {
                behaviour.parmas_jsons[i] = JsonUtility.ToJson(json_configs[i]);
            }
        }
        else
        {
            behaviour.parmas_jsons[index] = JsonUtility.ToJson(json_configs[index]);
        }

        behaviour.ExcuteDataUpdate(index);
    }
}
