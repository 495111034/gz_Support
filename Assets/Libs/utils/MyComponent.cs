using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MyComponent : MonoBehaviour
{
    #region CacheValue
    class CacheValue
    {

    }
    class CacheValue<T> : CacheValue
    {
        public T value;
    }
    #endregion
    Transform _trans;
    public Transform trans
    {
        get
        {
            if (_trans == null)
            {
                _trans = transform;
            }
            return _trans;
        }
    }
    RectTransform trRectform;
    public RectTransform rectTrans
    {
        get
        {
            if (trRectform == null)
            {
                trRectform = transform as RectTransform;
            }
            return trRectform;
        }
    }

    public int editorModel = 0;
    public string configJson = "";

    private Dictionary<string, string> configDic { set; get; }

    public string[] objExName = new string[0];
    public Component[] coms = new Component[0];
    private Dictionary<string, Component> m_coms { set; get; }
    private Dictionary<string, CacheValue> m_caches { set; get; }

    private System.Action config_change_ev = null;

    private void InitComponent()
    {
        m_coms = new Dictionary<string, Component>();
        int count = this.GetLength();
        for (byte i = 0; i < count; i++)
        {
            string key = this.GetObjName(i);
            if (!string.IsNullOrEmpty(key))
            {
                m_coms[key] = this.GetObj(i);
            }
        }
    }

    private void InitConfig()
    {
        configDic = new Dictionary<string, string>();
        if (!string.IsNullOrEmpty(configJson))
        {
            var json = MiniJSON.JsonDecode(configJson) as Hashtable;
            var list = json["list"] as ArrayList;
            for (int i = 0; i < list.Count; i++)
            {
                Hashtable j1 = list[i] as Hashtable;
                string v = j1["v"].ToString();
                if (v.IndexOf('@') > -1)
                {
                    string[] te = v.Split('@');
                    configDic[te[0]] = te[1];
                }
                else
                {
                    configDic[j1["n"].ToString()] = v;
                }
            }
        }
    }

    public Dictionary<string, string> GetConfigDic()
    {
        if (configDic == null) InitConfig();
        return configDic;
    }

    public void SetConfigDic(Dictionary<string, string> dic)
    {
        configDic = dic;
    }

    public void SetConfigChangeEvent(System.Action action)
    {
        config_change_ev = action;
    }

    public void TriggerConfigChange()
    {
        InitConfig();
        config_change_ev?.Invoke();
    }

    public int GetLength()
    {
        return coms.Length;
    }
    public Component GetObj(int index)
    {
        return coms[index];
    }
    public string GetObjName(int index)
    {
        return objExName[index];
    }

    public bool ContainsKey(string key)
    {
        if (m_coms == null) InitComponent();
        return m_coms.ContainsKey(key);
    }

    public T GetComp<T>(string key) where T : Component
    {
        if (m_coms == null) InitComponent();
        if (m_coms.TryGetValue(key, out Component com))
        {
            return GetComp<T>(com);
        }
        return default(T);
    }

    public T GetComp<T>(int index) where T : Component
    {
        return GetComp<T>(this.GetObj(index));
    }

    private T GetComp<T>(Component comp) where T : Component
    {
        if (comp != null)
        {
            if (comp is T)
            {
                return (T)comp;
            }
            else
            {
                return comp.gameObject.GetComponent<T>();
            }
        }
        return default(T);
    }

    public void AddComp(Component comp, string name = "")
    {
        if (m_coms == null) InitComponent();
        if (string.IsNullOrEmpty(name))
        {
            name = comp.name;
        }
        m_coms[name] = comp;
    }

    public GameObject GetGameObject(string key)
    {
        if (m_coms == null) InitComponent();
        if (m_coms.TryGetValue(key, out Component com))
        {
            return com.gameObject;
        }
        return null;
    }

    public GameObject GetGameObject(int index)
    {
        Component comp = this.GetObj(index);
        if (comp != null)
        {
            return comp.gameObject;
        }
        return null;
    }

    public bool ContainsKeyByConfig(string key)
    {
        if(configDic == null) InitConfig();
        return configDic.ContainsKey(key);
    }

    public T GetConfig<T>(string key)
    {
        if (configDic == null) InitConfig();
        if (configDic.TryGetValue(key, out string v))
        {
            return (T)System.Convert.ChangeType(v, typeof(T));
        }
        return default(T);
    }

    public string GetConfig(string key)
    {
        if (configDic == null) InitConfig();
        if (configDic.TryGetValue(key, out string v))
        {
        }
        return v;
    }

    public void SetConfig(string key, string value)
    {
        if (configDic == null) InitConfig();
        configDic[key] = value;
    }

    public void RemoveConfig(string key)
    {
        if (configDic == null) InitConfig();
        configDic.Remove(key);
    }
    #region CacheValue

    public bool HasValue(string key)
    {
        if (m_caches != null)
        {
            return m_caches.ContainsKey(key);
        }
        return false;
    }

    public bool RemoveValue(string key)
    {
        if (m_caches != null)
        {
            return m_caches.Remove(key);
        }
        return false;
    }

    public void SetValue<T>(string key, T t)
    {
        if (m_caches == null) m_caches = new Dictionary<string, CacheValue>();
        if (!m_caches.TryGetValue(key, out CacheValue cache))
        {
            cache = new CacheValue<T>();
            m_caches.Add(key, cache);
        }
        (cache as CacheValue<T>).value = t;
    }

    public T GetValue<T>(string key)
    {
        if (m_caches != null && m_caches.TryGetValue(key, out CacheValue cache))
        {
            return (cache as CacheValue<T>).value;
        }
        return default(T);
    }

    public T GetValue<T>(string key, T defaultValue)
    {
        if (m_caches != null && m_caches.TryGetValue(key, out CacheValue cache))
        {
            return (cache as CacheValue<T>).value;
        }
        return defaultValue;
    }

    #endregion

#if UNITY_EDITOR
    [ContextMenu("复制MyComponent关联")]
    private void CopyCompField()
    {
        if (UnityEditor.Selection.activeObject is GameObject)
        {
            MyComponent aComponent = this;// (UnityEditor.Selection.activeObject as GameObject).GetComponent<MyComponent>();
            if (aComponent != null)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                for (int i = 0; i < aComponent.objExName.Length; i++)
                {
                    if (aComponent.coms[i] != null)
                    {
                        string comName = aComponent.coms[i].GetType().Name;
                        string fileName = aComponent.objExName[i].Replace(" ", "");
                        sb.Append(comName + " " + fileName + " = ").AppendLine("comp.GetComp<" + comName + ">(\"" + aComponent.objExName[i] + "\");");
                    }
                    else
                    {
                        Debug.LogError("第" + i + "个组件引用丢失");
                    }
                }
                GUIUtility.systemCopyBuffer = sb.ToString();
            }
        }
    }

    [ContextMenu("复制MyComponent关联成员变量用")]
    private void CopyCompFieldInClass()
    {
        if (UnityEditor.Selection.activeObject is GameObject)
        {
            MyComponent aComponent = this;// (UnityEditor.Selection.activeObject as GameObject).GetComponent<MyComponent>();
            if (aComponent != null)
            {
                System.Text.StringBuilder sb0 = new System.Text.StringBuilder();
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                for (int i = 0; i < aComponent.objExName.Length; i++)
                {
                    if (aComponent.coms[i] != null)
                    {
                        string comName = aComponent.coms[i].GetType().Name;
                        string fileName = aComponent.objExName[i].Replace(" ", "");
                        sb0.AppendLine("       private " + comName + " " + fileName + " = null;");
                        if (i == aComponent.objExName.Length - 1)
                        {
                            sb.Append(fileName + " = ").Append("comp.GetComp<" + comName + ">(\"" + aComponent.objExName[i] + "\");");
                        }
                        else
                        {
                            sb.Append(fileName + " = ").AppendLine("comp.GetComp<" + comName + ">(\"" + aComponent.objExName[i] + "\");");
                        }
                    }
                    else
                    {
                        Debug.LogError("第" + i + "个组件引用丢失");
                    }
                }

                sb0.AppendLine("public void InitComp(MyComponent comp)");
                sb0.AppendLine("{");
                sb0.AppendLine(sb.ToString());
                sb0.AppendLine("}");
                GUIUtility.systemCopyBuffer = sb0.ToString();
            }
        }
    }

    [ContextMenu("打开UI代码文件", false, 1000)]
    public void TryOpenProgramFile()
    {
        //var activeObj = UnityEditor.Selection.activeObject;
        //if (activeObj == null) return;
        MyComponent aComponent = this;
        if (aComponent == null || aComponent.gameObject == null) return;

        string objName = aComponent.gameObject.name;
        if (string.IsNullOrEmpty(objName)) return;
        if (!objName.Contains("_panel")) return;
        string[] splits = objName.Split("(");
        string finalClassName = splits[0];
        finalClassName = finalClassName.Split(" ")[0];
        string finalClassPath = System.IO.Path.GetFullPath($"../GameLogic/script/view/auto/{finalClassName}.cs");
        if (System.IO.File.Exists(finalClassPath))
        {
            //System.Diagnostics.Process.Start(finalClassPath);
            string class_name = finalClassName;
            System.Type[] types = typeof(MyComponent).Assembly.GetTypes();
            for (int i = 0; i < types.Length; i++)
            {
                System.Type t = types[i];
                if (t.BaseType != null && t.BaseType.Namespace == "GameLogic.ViewAuto")
                {
                    if (t.BaseType.Name.Equals(finalClassName))
                    {
                        class_name = t.Name;
                        break;
                    }
                }
            }
            class_name += ".cs";
            var files = UnityEditor.AssetDatabase.FindAssets($"t:Script", new string[] { $"Assets/GameLogic" });
            foreach (var f in files)
            {
                string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(f); // 从GUID拿到资源的路径
                if (System.IO.Path.GetFileName(assetPath).Equals(class_name))
                {
                    var obj = UnityEditor.AssetDatabase.LoadMainAssetAtPath(assetPath);
                    if (obj != null)
                    {
                        UnityEditor.AssetDatabase.OpenAsset(obj);
                    }
                    else
                    {
                        Debug.LogError($"文件不存在：{finalClassPath}");
                    }
                    break;
                }
            }
        }
        else
        {
            Debug.LogError($"文件不存在：{finalClassPath}");
        }
    }
#endif
}
