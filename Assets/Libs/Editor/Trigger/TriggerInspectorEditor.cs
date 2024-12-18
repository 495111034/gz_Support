
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ScenarioBehaviour))]
public class TriggerInspectorEditor : Editor
{
    private SerializedObject obj;
    private SerializedProperty tiggerActions;
    private ScenarioBehaviour scenarioBehaviour;

    private void OnEnable()
    {
        obj = new SerializedObject(target);
        scenarioBehaviour = target as ScenarioBehaviour;

        tiggerActions = obj.FindProperty("triggerActions");

        Assembly a = Assembly.Load("Assembly-CSharp");
        var t = a.GetType("CameraTraceModule");
        if (t != null)
        {
            var inst = t.GetMethod("GetInstance");
            if (inst != null)
            {
                var tobj = inst.Invoke(null, null);
                var tfunc = t.GetMethod("cameraTraceUpdate");
                if (tfunc != null && tobj != null)
                {
                    PreviewEventDriver.updateEvent = () =>
                    {
                        tfunc.Invoke(tobj, null);
                    };
                }
            }
        }
    }

    /// <summary>
    /// inspector 面板绘制
    /// </summary>
    /// 
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        scenarioBehaviour.TriggerParams = MiniJSON.JsonDecode(scenarioBehaviour.TriggerParamsStr) as Hashtable;

        GUILayout.BeginHorizontal();
        GUILayout.Label("预览");
        if (GUILayout.Button("enter", GUILayout.MaxWidth(100)))
        {
            PreviewTriggerEnter();
        }

        if (GUILayout.Button("stay", GUILayout.MaxWidth(100)))
        {
            PreviewTriggerStay();
        }

        if (GUILayout.Button("exit", GUILayout.MaxWidth(100)))
        {
            PreviewTriggerExit();
        }
        GUILayout.EndHorizontal();


        if (scenarioBehaviour.TriggerParams == null)
        {
            scenarioBehaviour.TriggerParams = new Hashtable();

        }


        string remove_index = null;
        for (int i = 1; i <= scenarioBehaviour.TriggerParams.Count; i++)
        {
            if (!scenarioBehaviour.TriggerParams.ContainsKey(i.ToString()))
            {
                if (Application.isEditor) Debug.LogWarning("no find action");
                return;
            }

            Hashtable action = scenarioBehaviour.TriggerParams[i.ToString()] as Hashtable;

            if (!action.ContainsKey("command"))
            {
                action.Add("command", TriggerParamsTypeDefine.TriggerParamsType.Keys.ToArray()[0]);
            }

            //EditorGUILayout.LabelField(""); //换行
            EditorGUILayout.LabelField(i.ToString()); //换行

            //所有类别
            int classIndex = 0;
            if (action.ContainsKey("trigger_class"))
            {
                classIndex = Array.IndexOf(TriggerParamsTypeDefine.TriggerParamsType.Keys.ToArray(), (string)action["trigger_class"]);
            }
            int newClassIndex = EditorGUILayout.Popup("类别", classIndex, TriggerParamsTypeDefine.TriggerParamsType.Keys.ToArray());
            if (newClassIndex != classIndex)
            {
                action.Clear();
            }
            action["trigger_class"] = TriggerParamsTypeDefine.TriggerParamsType.Keys.ToArray()[newClassIndex];

            //所有事件
            int eventIndex = 0;
            if (action.ContainsKey("command"))
            {
                eventIndex = Mathf.Clamp(Array.IndexOf(TriggerParamsTypeDefine.TriggerParamsType[action["trigger_class"] as string].Keys.ToArray(), (string)action["command"]), 0, int.MaxValue);
            }
            List<string> eventStr = new List<string>();
            foreach (var kv in TriggerParamsTypeDefine.TriggerParamsType[action["trigger_class"] as string])
            {
                eventStr.Add(kv.Value[0]);
            }
            int newEventIndex = EditorGUILayout.Popup("目的", eventIndex, eventStr.ToArray());
            if (newEventIndex != eventIndex)
            {
                var _tmp = action["trigger_class"];
                action.Clear();
                action["trigger_class"] = _tmp;
            }
            action["command"] = TriggerParamsTypeDefine.TriggerParamsType[action["trigger_class"] as string].Keys.ToArray()[newEventIndex];


            int fire_type_index = Mathf.Clamp(Array.IndexOf(TriggerParamsTypeDefine.fire_types, action["fire_type"]), 0, int.MaxValue);
            action["fire_type"] = TriggerParamsTypeDefine.fire_types[EditorGUILayout.Popup("触发类型", fire_type_index, TriggerParamsTypeDefine.fire_types_chinese)];

            string[] triggerParams = TriggerParamsTypeDefine.TriggerParamsType[action["trigger_class"] as string][(string)action["command"]];

            int index = 1;
            while (index < triggerParams.Length)
            {
                if (triggerParams[index] == "phase_id")
                {
                    if (fire_type_index != 6)
                    {
                        index += 3;
                        continue;
                    }
                }
                if (!action.ContainsKey(triggerParams[index]))
                {
                    action.Add(triggerParams[index], "");
                }

                action[triggerParams[index]] = AddItem(triggerParams[index + 2], action[triggerParams[index]] as string, triggerParams[index + 1]);
                index += 3;
            }
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("移除"))
            {
                remove_index = i.ToString();
            }
            GUILayout.EndHorizontal();

        }

        //移除事件
        if (remove_index != null)
        {
            scenarioBehaviour.TriggerParams.Remove(remove_index);
            try
            {
                int _tmp = int.Parse(remove_index);
                _tmp++;
                while (scenarioBehaviour.TriggerParams.ContainsKey(_tmp.ToString()))
                {
                    scenarioBehaviour.TriggerParams[(_tmp - 1).ToString()] = scenarioBehaviour.TriggerParams[_tmp.ToString()];
                    _tmp++;
                }

                scenarioBehaviour.TriggerParams.Remove((_tmp - 1).ToString());
            }
            catch (Exception e)
            {

            }
        }

        EditorGUILayout.LabelField(""); //换行

        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("添加"))
        {
            scenarioBehaviour.TriggerParams.Add((scenarioBehaviour.TriggerParams.Count + 1).ToString(), new Hashtable());
        }
        if (GUILayout.Button("预览Json"))
        {
            string json = MiniJSON.JsonEncode(scenarioBehaviour.TriggerParams);
            if (EditorUtility.DisplayDialog("结果", json, "yes", "No"))
            {
                TextEditor text2Editor = new TextEditor();
                text2Editor.text = json;
                text2Editor.OnFocus();
                text2Editor.Copy();
            }
        }
        GUILayout.EndHorizontal();
        string params_str = MiniJSON.JsonEncode(scenarioBehaviour.TriggerParams);
        if (params_str != scenarioBehaviour.TriggerParamsStr)
        {
            EditorUtility.SetDirty(target);
        }
        scenarioBehaviour.TriggerParamsStr = params_str;
        scenarioBehaviour.nodeEffect = EditorGUILayout.TextField("特效(不填不显示)", scenarioBehaviour.nodeEffect);
        scenarioBehaviour.funcOpenID = EditorGUILayout.IntField("FuncOpenID(默认0无限制)", scenarioBehaviour.funcOpenID);
    }

    /// <summary>
    /// 预览场景中的trigger(enter)
    /// </summary>
    public void PreviewTriggerEnter()
    {
        ; GameObject gameObject = Selection.gameObjects[0];

        Assembly a = Assembly.Load("Assembly-CSharp");
        var t = a.GetType("ScenarioTriggerEventCenter");
        if (t != null)
        {
            var inst = t.GetMethod("GetInstance");
            if (inst != null)
            {
                var tobj = inst.Invoke(null, null);
                var tfunc = t.GetMethod("OnTriggerEnter");
                if (tfunc != null && tobj != null)
                {
                    object[] parems = { gameObject.GetComponent<ScenarioBehaviour>() };
                    tfunc.Invoke(tobj, parems);

                }
            }
        }
    }

    /// <summary>
    /// 预览场景中的trigger(stay)
    /// </summary>
    public void PreviewTriggerStay()
    {
        ; GameObject gameObject = Selection.gameObjects[0];

        Assembly a = Assembly.Load("Assembly-CSharp");
        var t = a.GetType("ScenarioTriggerEventCenter");
        if (t != null)
        {
            var inst = t.GetMethod("GetInstance");
            if (inst != null)
            {
                var tobj = inst.Invoke(null, null);
                var tfunc = t.GetMethod("OnTriggerStay");
                if (tfunc != null && tobj != null)
                {
                    object[] parems = { gameObject.GetComponent<ScenarioBehaviour>() };
                    tfunc.Invoke(tobj, parems);

                }
            }
        }
    }

    /// <summary>
    /// 预览场景中的trigger(exit)
    /// </summary>
    public void PreviewTriggerExit()
    {
        ; GameObject gameObject = Selection.gameObjects[0];

        Assembly a = Assembly.Load("Assembly-CSharp");
        var t = a.GetType("ScenarioTriggerEventCenter");
        if (t != null)
        {
            var inst = t.GetMethod("GetInstance");
            if (inst != null)
            {
                var tobj = inst.Invoke(null, null);
                var tfunc = t.GetMethod("OnTriggerExit");
                if (tfunc != null && tobj != null)
                {
                    object[] parems = { gameObject.GetComponent<ScenarioBehaviour>() };
                    tfunc.Invoke(tobj, parems);

                }
            }
        }
    }

    private static string AddItem(string name, string value, string type)
    {
        switch (type)
        {
            case "int":
                return AddIntItem(name, value);
            case "float":
                return AddFloatItem(name, value);
            case "bool":
                return AddBooleanItem(name, value);
            case "string":
                return AddTextItem(name, value);
            case "vector3":
                return AddVector3ItemWithGameObjetc(name, value);
            case "vector2":
                return AddVector2ItemWithGameObjetc(name, value);
            case "object_name":
                return AddObjetcItem(name, value);
            case "game_object":
                return GameObjectObjetcItem(name, value);
            case "trigger_object":
                return TriggerItem(name, value);
            default:
                Debug.LogError("no find type:" + type);
                return null;
        }
    }

    private static string AddIntItem(string name, string value)
    {
        if (value == "")
        {
            value = "0";
        }
        return EditorGUILayout.IntField(name, int.Parse(value)).ToString();
    }

    private static string AddFloatItem(string name, string value)
    {
        if (value == "")
        {
            value = "0";
        }
        return EditorGUILayout.FloatField(name, float.Parse(value)).ToString();
    }

    private static string AddBooleanItem(string name, string value)
    {
        if (value == "")
        {
            value = "true";
        }
        return EditorGUILayout.Toggle(name, Boolean.Parse(value)).ToString();
    }

    private static string AddTextItem(string name, string value)
    {
        return EditorGUILayout.TextField(name, value);
    }

    private static string AddVector3Item(string name, string value)
    {
        if (value == "")
        {
            value = "0|0|0";
        }
        return DataParse.ToStringL(EditorGUILayout.Vector3Field(name, DataParse.GetVector3L(value)));
    }

    private static string AddVector3ItemWithGameObjetc(string name, string value)
    {
        GameObject gameObject = null;
        gameObject = EditorGUILayout.ObjectField(name, gameObject, typeof(GameObject)) as GameObject;
        if (gameObject != null)
        {
            value = DataParse.ToStringL(gameObject.transform.position);
        }
        if (value == "")
        {
            value = "0|0|0";
        }
        return DataParse.ToStringL(EditorGUILayout.Vector3Field(name, DataParse.GetVector3L(value)));
    }

    private static string AddVector2ItemWithGameObjetc(string name, string value)
    {
        GameObject gameObject = null;
        gameObject = EditorGUILayout.ObjectField(name, gameObject, typeof(GameObject)) as GameObject;
        if (gameObject != null)
        {
            value = $"{gameObject.transform.position.x}|{gameObject.transform.position.z}";
        }
        if (value == "")
        {
            value = "0|0";
        }

        return DataParse.ToStringL(EditorGUILayout.Vector2Field(name, DataParse.GetVector2L(value)));
    }

    private static string AddObjetcItem(string name, string value)
    {
        UnityEngine.Object uobjetc = null;
        uobjetc = EditorGUILayout.ObjectField(name, uobjetc, typeof(UnityEngine.Object)) as UnityEngine.Object;
        if (uobjetc != null)
        {
            value = uobjetc.name;
        }
        return EditorGUILayout.TextField(name, value);
    }

    private static string GameObjectObjetcItem(string name, string value)
    {
        UnityEngine.GameObject uobjetc = null;
        uobjetc = EditorGUILayout.ObjectField(name, uobjetc, typeof(UnityEngine.GameObject)) as UnityEngine.GameObject;
        if (uobjetc != null)
        {
            value = uobjetc.transform.name;
            Transform parent = uobjetc.transform.parent;
            while (parent != null && parent.name != "prefabroot")
            {
                value = parent.name + "/" + value;

                parent = parent.parent;
            }
        }
        return EditorGUILayout.TextField(name, value);
    }

    private static string TriggerItem(string name, string value)
    {
        UnityEngine.GameObject uobjetc = null;
        uobjetc = EditorGUILayout.ObjectField(name, uobjetc, typeof(UnityEngine.GameObject)) as UnityEngine.GameObject;
        if (uobjetc != null)
        {
            Transform parent = uobjetc.transform.parent;
            if (parent.name == "trigger")
            {
                value = uobjetc.transform.name;
            }
        }
        return EditorGUILayout.TextField(name, value);
    }

}

