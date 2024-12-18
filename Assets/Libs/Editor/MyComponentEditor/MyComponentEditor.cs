using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using MyComponentExUtil;
using System.Reflection;
using System;
using UnityEngine.UI;

namespace MyComponentExUtil
{
    public class ConfigAttribute : System.Attribute
    {
        public string desc;
        public string fieldType;
        public string dialog;
        public string[] enumDesc;
        public ConfigAttribute(string desc, string dialog = null, string[] enumDesc = null)
        {
            this.desc = desc;
            this.dialog = dialog;
            this.enumDesc = enumDesc;
        }
    }
}

[CustomEditor(typeof(MyComponent), true)]
public class MyComponentEditor : Editor
{
    [System.Serializable]
    public class FieldJson
    {
        public string c = "";
        public string n = "";
        public string v = "";
    }

    [System.Serializable]
    public class FieldPackJson
    {
        public List<FieldJson> list = new List<FieldJson>();
    }


    MyComponent acomponent;
    int currLength;
    Component tempObj = null;
    FieldPackJson fieldPackJson = null;
    int currConfigModel
    {
        get { return acomponent.editorModel; } set { acomponent.editorModel = value; }
    }

    string[] configHeads = null;
    string[] configEnums = null;
    Dictionary<string, FieldJson> configDic = null;
    Dictionary<string, ConfigAttribute> configAttrDic = null;
    Dictionary<GameObject, string[]> targetCompNamesDic = null;
    Dictionary<int, int> targetCompIndexDic = null;

    private List<string> m_TempFiledNames = new List<string>();
    private List<string> m_TempComponentTypeNames = new List<string>();
    private IAutoBindRuleHelper m_RuleHelper = new DefaultAutoBindRuleHelper();
    private string search_text = "";
    private bool isCanvasComp = false;
    private string[] sort_types = new string[]
    {
        "排序",
        "按类型",
        "按字符串",
    };
    private string[] bind_types = new string[]
    {
        "自动绑定",
        "按命名",
        "按常用类型",
        "MyText",
        "MyButton",
        "MyToggle",
        "MyInputField",
        "MySlider",
        "ScrollRect",
    };
    private static List<System.Type> FindComps = new List<System.Type>()
    {
        typeof(MyComponent),
        typeof(MyText),
        typeof(MyImageText),
        typeof(MyButton),
        typeof(MyToggle),
        typeof(MyInputField),
        typeof(MySlider),
        typeof(Slider),
        typeof(NLoopHorizontalScrollRect),
        typeof(NLoopVerticalScrollRect),
        typeof(NLoopHorizontalScrollRectMulti),
        typeof(NLoopVerticalScrollRectMulti),
        typeof(ScrollRect),
        typeof(MySpriteImage),
        typeof(MyImage),
        typeof(RectTransform),
        typeof(Transform),
    };

    private List<string> type_name_s = new List<string>();
    private int curr_type_select = 0;
    void OnEnable()
    {
        configDic = null;
        configAttrDic = null;
        configEnums = null;
        isChange = false;
        acomponent = (MyComponent)target;
        if (acomponent.coms != null)
        {
            currLength = acomponent.coms.Length;
        }
        if (fieldPackJson == null)
        {
            fieldPackJson = new FieldPackJson();
        }

        isCanvasComp = true;// acomponent.gameObject.GetComponent<Canvas>() != null;

        type_name_s.Clear();
        type_name_s.Add("ALL");
        isRepaceField = false;
        for (int i = 0; i < acomponent.objExName.Length; i++)
        {
            if (acomponent.coms[i] != null)
            {
                string key = acomponent.coms[i].GetType().Name;
                if (!type_name_s.Contains(key))
                {
                    type_name_s.Add(key);
                }
            }
        }
    }
    bool isChange = false;
    bool isRepaceField = false;

    public override void OnInspectorGUI()
    {
        EditorGUILayout.BeginHorizontal();
        if (isCanvasComp)
        {
            if (EditorGUILayout.DropdownButton(new GUIContent($"切换配置"), FocusType.Keyboard, GUILayout.Width(150)))
            {
                currConfigModel = 1 - currConfigModel;
                if (currConfigModel == 0)
                {
                    currLength = acomponent.coms.Length;
                }
                else
                {
                    currLength = fieldPackJson.list.Count;
                }
            }
        }
        if (currConfigModel == 0 || !isCanvasComp)
        {
            //if (GUILayout.Button("排序"))
            //{
            //    SortBinds();
            //}
            //if (GUILayout.Button("自动绑定"))
            //{
            //    AutoBindComponent();
            //}
            //if (GUILayout.Button("全部删除"))
            //{
            //    DeleteAll();
            //}
            curr_type_select = EditorGUILayout.Popup(curr_type_select, type_name_s.ToArray());
            int index = EditorGUILayout.Popup(0, sort_types);
            if (index > 0)
            {
                SortBinds(index);
            }
            index = EditorGUILayout.Popup(0, bind_types);
            if (index > 0)
            {
                switch (index)
                {
                    case 1:
                        {
                            AutoBindComponent();
                        }
                        break;
                    case 2:
                        {
                            AutoBindComponent(FindComps.GetRange(0, 13));
                        }
                        break;
                    case 3:
                        {
                            AutoBindComponent(new List<Type>() { typeof(MyText), typeof(MyImageText) });
                        }
                        break;
                    case 4:
                        {
                            AutoBindComponent(new List<Type>() { typeof(MyButton), typeof(Button) });
                        }
                        break;
                    case 5:
                        {
                            AutoBindComponent(new List<Type>() { typeof(MyToggle) });
                        }
                        break;
                    case 6:
                        {
                            AutoBindComponent(new List<Type>() { typeof(MyInputField) });
                        }
                        break;
                    case 7:
                        {
                            AutoBindComponent(new List<Type>() { typeof(MySlider), typeof(Slider) });
                        }
                        break;
                    case 8:
                        {
                            AutoBindComponent(new List<Type>() { typeof(ScrollRect), typeof(NLoopHorizontalScrollRect), typeof(NLoopVerticalScrollRect), typeof(NLoopHorizontalScrollRectMulti), typeof(NLoopVerticalScrollRectMulti) });
                        }
                        break;
                }
            }
            //if (EditorGUILayout.DropdownButton(new GUIContent("排序"), FocusType.Keyboard))
            //{
            //    SortBinds();
            //}
            //if (EditorGUILayout.DropdownButton(new GUIContent("自动绑定"), FocusType.Keyboard))
            //{
            //    AutoBindComponent();
            //}
        }
        else
        {
            if (configEnums == null)
            {
                if (!string.IsNullOrEmpty(acomponent.configJson))
                {
                    fieldPackJson = JsonUtility.FromJson<FieldPackJson>(acomponent.configJson);
                }
                configAttrDic = new Dictionary<string, ConfigAttribute>();
                configDic = new Dictionary<string, FieldJson>();
                System.Type[] types = Assembly.GetExecutingAssembly().GetTypes();
                List<string> configHeadList = new List<string>();
                configHeadList.Add("添加全部");
                List<string> configList = new List<string>();
                configList.Add("+  Add");
                for (int i = 0; i < types.Length; i++)
                {
                    if (types[i].Namespace == "MyComponentExUtil")
                    {
                        ConfigAttribute configAttribute = types[i].GetCustomAttribute<ConfigAttribute>();
                        if (configAttribute != null)
                        {
                            configHeadList.Add(configAttribute.desc);
                            var fields = types[i].GetFields();
                            foreach (var field in fields)
                            {
                                ConfigAttribute fieldAttribute = field.GetCustomAttribute<ConfigAttribute>();
                                if (fieldAttribute != null)
                                {
                                    string name = configAttribute.desc + "/" + fieldAttribute.desc;
                                    configList.Add(name);
                                    object vobj = field.GetValue(System.Activator.CreateInstance(types[i]));
                                    string vstring = vobj != null ? vobj.ToString() : "";
                                    if (configAttribute.desc == "cfg")
                                    {
                                        fieldAttribute.fieldType = fieldAttribute.dialog;
                                    }
                                    else
                                    {
                                        fieldAttribute.fieldType = field.FieldType.Name;
                                    }
                                    configAttrDic[configAttribute.desc + field.Name] = fieldAttribute;
                                    configDic[name] = new FieldJson()
                                    {
                                        c = configAttribute.desc,
                                        n = field.Name,
                                        v = vstring
                                    };
                                }
                            }
                        }
                    }
                }
                configHeads = configHeadList.ToArray();
                configEnums = configList.ToArray();
            }

            int index = EditorGUILayout.Popup(0, configHeads);
            if (index > 0)
            {
                for (int i = 0; i < configEnums.Length; i++)
                {
                    if (configEnums[i].StartsWith(configHeads[index]))
                    {
                        var fieldConfig = configDic[configEnums[i]];
                        if (fieldConfig.c != "cfg")
                        {
                            for (int x = 0; x < fieldPackJson.list.Count; x++)
                            {
                                if (fieldPackJson.list[x].c == fieldConfig.c && fieldPackJson.list[x].n == fieldConfig.n)
                                {
                                    fieldConfig = null;
                                    break;
                                }
                            }
                        }
                        if (fieldConfig != null)
                        {
                            isChange = true;
                            fieldPackJson.list.Add(new FieldJson() { c = fieldConfig.c, n = fieldConfig.n, v = fieldConfig.v });
                        }
                    }
                }
            }
            index = EditorGUILayout.Popup(0, configEnums);
            if (index > 0)
            {
                var fieldConfig = configDic[configEnums[index]];
                if (fieldConfig.c != "cfg")
                {
                    for (int x = 0; x < fieldPackJson.list.Count; x++)
                    {
                        if (fieldPackJson.list[x].c == fieldConfig.c && fieldPackJson.list[x].n == fieldConfig.n)
                        {
                            fieldConfig = null;
                            break;
                        }
                    }
                }
                if (fieldConfig != null)
                {
                    isChange = true;
                    fieldPackJson.list.Add(new FieldJson() { c = fieldConfig.c, n = fieldConfig.n, v = fieldConfig.v });
                }
            }
        }
        EditorGUILayout.EndHorizontal();

        if (currConfigModel == 0 || !isCanvasComp)
        {
            OnGUICompConfig();
        }
        else
        {
            OnGUIJsonConfig();
        }
    }

    private void DeleteAll()
    {
        currLength = 0;
    }

    private void AutoBindComponent(List<Type> find_comps)
    {
        for (int i = 0; i < find_comps.Count; i++)
        {
            var comps = acomponent.gameObject.GetComponentsInChildren(find_comps[i]);
            foreach (var comp in comps)
            {
                if (i == 0)
                {
                    if (acomponent != comp)
                    {
                        Add(comp.name, comp);
                    }
                }
                else
                {
                    Add(comp.name, comp);
                }
            }
        }
        isChange = true;
    }

    private void Add(string key, Component obj)
    {
        if (acomponent.coms != null)
        {
            bool is_has = false;
            foreach(var comp in acomponent.coms)
            {
                if (comp == obj)
                {
                    is_has = true;
                    break;
                }
            }
            if (!is_has)
            {
                foreach (var name in acomponent.objExName)
                {
                    if (name == key)
                    {
                        is_has = true;
                        break;
                    }
                }
            }
            if (is_has)
            {
                return;
            }
        }

        currLength = acomponent.coms.Length + 1;
        Component[] tempComs = acomponent.coms;
        string[] tempStrs = acomponent.objExName;
        acomponent.coms = new Component[currLength];
        acomponent.objExName = new string[currLength];
        for (int i = 0; i < currLength; i++)
        {
            if (i < tempComs.Length)
            {
                acomponent.coms[i] = tempComs[i];
                acomponent.objExName[i] = tempStrs[i];
            }
            else
            {
                acomponent.coms[i] = obj;
                acomponent.objExName[i] = key;
            }
        }
    }

    private void AutoBindComponent()
    {
        isChange = true;
        List<Tuple<Component, string>> tempAddDatas = new List<Tuple<Component, string>>();

        Transform[] childs = acomponent.gameObject.GetComponentsInChildren<Transform>(true);

        for (int tindex = 0; tindex < childs.Length; tindex++)
        {
            var child = childs[tindex];
            // 跳过自己
            if (child == acomponent.transform) continue;
            // 跳过带MyComponent的，附带跳过其childCount
            var mycomp = child.GetComponent<MyComponent>();
            if (mycomp != null)
            {
                var ccount = mycomp.gameObject.GetComponentsInChildren<Transform>(true).Length;
                if (ccount > 0) tindex += ccount - 1;
                continue;
            }

            m_TempFiledNames.Clear();
            m_TempComponentTypeNames.Clear();

            if (m_RuleHelper.IsValidBind(child, m_TempFiledNames, m_TempComponentTypeNames))
            {
                for (int i = 0; i < m_TempFiledNames.Count; i++)
                {
                    Component com = child.GetComponent(m_TempComponentTypeNames[i]);
                    if (com == null)
                    {
                        Debug.LogError($"{child.name}上不存在{m_TempComponentTypeNames[i]}的组件");
                        continue;
                    }
                    bool isRepeated = false;
                    for (int x = 0; x < acomponent.objExName.Length; x++)
                    {
                        if (m_TempFiledNames[i] == acomponent.objExName[x])
                        {
                            Debug.LogError($"{m_TempFiledNames[i]} 字段名重复");
                            isRepeated = true;
                            break;
                        }
                    }
                    if (isRepeated) continue;
                    tempAddDatas.Add(Tuple.Create(com, m_TempFiledNames[i]));
                }
            }

            if (m_TempFiledNames.Count == 0)
            {
                Component com = child.gameObject.GetComponent<MyComponent>();
                if (com != null)
                {
                    tempAddDatas.Add(Tuple.Create(com, child.name));
                }
            }
        }

        if (tempAddDatas.Count == 0) return;
        
        int originCount = acomponent.coms.Length;
        int nowCount = originCount + tempAddDatas.Count;
        currLength = nowCount;
        Component[] tempComs = acomponent.coms;
        string[] tempStrs = acomponent.objExName;
        acomponent.coms = new Component[nowCount];
        acomponent.objExName = new string[nowCount];

        tempComs.CopyTo(acomponent.coms, 0);
        tempStrs.CopyTo(acomponent.objExName, 0);
        for (int i = 0; i < tempAddDatas.Count; i++)
        {
            int index = originCount + i;
            acomponent.coms[index] = tempAddDatas[i].Item1;
            acomponent.objExName[index] = tempAddDatas[i].Item2;
        }
    }

    /// <summary>
    /// 排序
    /// </summary>
    private void SortBinds(int type)
    {
        if (acomponent.coms == null || acomponent.coms.Length == 0) return;
        if (acomponent.coms.Length != acomponent.objExName.Length) return;
        int tmplength = acomponent.coms.Length;

        isChange = true;
        Component[] tempComs = acomponent.coms;
        string[] tempStrs = acomponent.objExName;

        List<Tuple<Component, string>> tempDatas = new List<Tuple<Component, string>>();
        for (int i = 0; i < tempComs.Length; i++)
        {
            tempDatas.Add(Tuple.Create(tempComs[i], tempStrs[i]));
        }
        tempDatas.Sort((x, y) =>
        {
            if (type == 1)
            {
                int idx1 = 9999;
                int idx2 = 9999;
                for (int i = 0; i < FindComps.Count; i++)
                {
                    if (x.Item1.GetType() == FindComps[i])
                    {
                        idx1 = i;
                    }
                    if (y.Item1.GetType() == FindComps[i])
                    {
                        idx2 = i;
                    }
                }
                if (idx1 == idx2)
                {
                    return string.Compare(x.Item2, y.Item2, StringComparison.Ordinal);
                }
                return idx1.CompareTo(idx2);
            }
            else
            {
                return string.Compare(x.Item2, y.Item2, StringComparison.Ordinal);
            }
        });

        acomponent.coms = new Component[tmplength];
        acomponent.objExName = new string[tmplength];
        for (int i = 0; i < tmplength; i++)
        {
            if (i < tempComs.Length)
            {
                acomponent.coms[i] = tempDatas[i].Item1;
                acomponent.objExName[i] = tempDatas[i].Item2;
            }
        }

        targetCompNamesDic = new Dictionary<GameObject, string[]>();
        targetCompIndexDic = new Dictionary<int, int>();
    }

    void OnGUICompConfig()
    { 
        if (acomponent.coms == null)
        {
            isChange = true;
            acomponent.coms = new Component[0];
            acomponent.objExName = new string[0];
        }
        //if (EditorGUILayout.DropdownButton(new GUIContent("-   Delete"), FocusType.Keyboard))
        //{
        //    currLength--;
        //    if (currLength < 0)
        //    {
        //        currLength = 0;
        //    }
        //}
        EditorGUILayout.BeginHorizontal();
        tempObj = EditorGUILayout.ObjectField(null, typeof(Component), true, GUILayout.Height(36)) as Component;
        if (acomponent.coms.Length > 20)
        {
            EditorGUILayout.BeginVertical();
            if (GUILayout.Button("搜索(x)", GUILayout.Height(16)))
            {
                search_text = "";
            }
            search_text = EditorGUILayout.TextField(search_text, GUILayout.Height(20));
            EditorGUILayout.EndVertical();
        }
        if (GUILayout.Button("清空", GUILayout.Width(40), GUILayout.Height(36)))
        {
            if (EditorUtility.DisplayDialog("温馨提示", "是否要清空所有组件？", "OK", "点错了"))
            {
                isChange = true;
                acomponent.coms = new Component[0];
                acomponent.objExName = new string[0];
                targetCompNamesDic = null;
                targetCompIndexDic = null;
                currLength = 0;
            }
        }
        if (GUILayout.Button("粘贴", GUILayout.Width(40), GUILayout.Height(36)))
        {
            Transform baseParent = GetBaseParent(acomponent.transform);
            string content = GUIUtility.systemCopyBuffer;
            GUIUtility.systemCopyBuffer = null;
            if (!string.IsNullOrEmpty(content))
            {
                string[] subs = content.Split('\n');
                for (int i = 0; i < subs.Length; i++)
                {
                    if (!string.IsNullOrEmpty(subs[i]))
                    {
                        string path = subs[i].TrimEnd(' ');
                        if (!string.IsNullOrEmpty(path))
                        {
                            GameObject findObj = GameObject.Find(path);
                            if (findObj == null)
                            {
                                Transform t = null;
                                if (path.IndexOf('/') > -1)
                                {
                                    t = baseParent.Find(path.Substring(baseParent.name.Length + 1));
                                }
                                else
                                {
                                    t = baseParent;
                                }
                                if (t != null)
                                {
                                    findObj = t.gameObject;
                                }
                            }
                            if (findObj != null)
                            {
                                tempObj = findObj.GetComponent<Component>();
                                if (tempObj != null)
                                {
                                    bool isHave = false;
                                    for (int x = 0; x < acomponent.objExName.Length; x++)
                                    {
                                        if (acomponent.coms[x] == tempObj)
                                        {
                                            isHave = true;
                                            break;
                                        }
                                    }
                                    if (!isHave)
                                    {
                                        currLength++;
                                        isChange = true;
                                        NewAddObject();
                                    }
                                    tempObj = null;
                                }
                            }
                        }
                    }
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        if (tempObj != null)
        {
            currLength++;
            isChange = true;
        }
        NewAddObject();
        int deleteIndex = -1;
        for (int i = 0; i < acomponent.coms.Length; i++)
        {
            string tempExName = acomponent.objExName[i];
            if (curr_type_select > 0)
            {
                if (acomponent.coms[i] == null || acomponent.coms[i].GetType().Name != type_name_s[curr_type_select])
                {
                    continue;
                }
            }
            if (!string.IsNullOrEmpty(search_text))
            {
                if (string.IsNullOrEmpty(tempExName) || !tempExName.ToLower().Contains(search_text))
                {
                    if (acomponent.coms[i] == null || (!acomponent.coms[i].name.ToLower().Contains(search_text) && !acomponent.coms[i].GetType().Name.ToLower().Contains(search_text)))
                    {
                        continue;
                    }
                }
            }
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(i.ToString(), GUILayout.MaxWidth(20));
            var comTemp = acomponent.coms[i];
            acomponent.objExName[i] = EditorGUILayout.TextField(acomponent.objExName[i], GUILayout.Width(120));
            acomponent.coms[i] = EditorGUILayout.ObjectField(acomponent.coms[i], typeof(Component), true, GUILayout.Width(100)) as Component;
            if (targetCompNamesDic == null)
            {
                targetCompNamesDic = new Dictionary<GameObject, string[]>();
                targetCompIndexDic = new Dictionary<int, int>();
            }
            if (acomponent.coms[i] != null)
            {
                if (comTemp == null)
                {
                    AutoSetComp(i, acomponent.coms[i]);
                    targetCompIndexDic.Remove(i);
                }
                if (!targetCompNamesDic.TryGetValue(acomponent.coms[i].gameObject, out string[] popups))
                {
                    Component[] cms = acomponent.coms[i].gameObject.GetComponents<Component>();
                    string[] fileNames = new string[cms.Length];
                    for (int x = 0; x < cms.Length; x++)
                    {
                        fileNames[x] = cms[x].GetType().Name;
                    }
                    popups = fileNames;
                    targetCompNamesDic.Add(acomponent.coms[i].gameObject, popups);
                }
                if (!targetCompIndexDic.TryGetValue(i, out int popupIndex))
                {
                    for (int x = 0; x < popups.Length; x++)
                    {
                        if (popups[x] == acomponent.coms[i].GetType().Name)
                        {
                            popupIndex = x;
                            break;
                        }
                    }
                    targetCompIndexDic.Add(i, popupIndex);
                }
                int curr = EditorGUILayout.Popup(popupIndex, popups);
                if (curr != popupIndex)
                {
                    popupIndex = curr;
                    targetCompIndexDic[i] = popupIndex;
                    Component[] cms = acomponent.coms[i].gameObject.GetComponents<Component>();
                    for (int x = 0; x < cms.Length; x++)
                    {
                        if (popups[popupIndex] == cms[x].GetType().Name)
                        {
                            acomponent.coms[i] = cms[x];
                            break;
                        }
                    }
                }
            }

            if (tempExName != acomponent.objExName[i] || comTemp != acomponent.coms[i])
            {
                bool isHave = false;
                for (int c = 0; c < acomponent.objExName.Length; c++)
                {
                    if (i != c)
                    {
                        if (acomponent.objExName[c] == acomponent.objExName[i])
                        {
                            isHave = true;
                            break;
                        }
                    }
                }
                if (isHave)
                {
                    acomponent.objExName[i] = tempExName;
                }
                if (acomponent.objExName[i] == acomponent.gameObject.name)
                {
                    acomponent.objExName[i] = "_" + acomponent.objExName[i];
                }
                isChange = true;
            }

            if (GUILayout.Button("↑"))
            {
                SetReplace(i - 1, i);
            }
            if (GUILayout.Button("↓"))
            {
                SetReplace(i + 1, i);
            }
            if (GUILayout.Button("×"))
            {
                deleteIndex = i;
            }
            EditorGUILayout.EndHorizontal();
        }
        if (deleteIndex > -1)
        {
            Component[] tempComs = acomponent.coms;
            string[] tempStrs = acomponent.objExName;
            currLength--;
            acomponent.coms = new Component[currLength];
            acomponent.objExName = new string[currLength];
            int index1 = 0;
            for (int x = 0; x < tempComs.Length; x++)
            {
                if (x != deleteIndex)
                {
                    acomponent.coms[index1] = tempComs[x];
                    acomponent.objExName[index1] = tempStrs[x];
                    index1++;
                }
            }

            targetCompNamesDic = new Dictionary<GameObject, string[]>();
            targetCompIndexDic = new Dictionary<int, int>();

            isChange = true;
        }

        if (isChange)
        {
            type_name_s.Clear();
            type_name_s.Add("ALL");
            isRepaceField = false;
            for (int i = 0; i < acomponent.objExName.Length; i++)
            {
                string temp = acomponent.objExName[i];
                if (acomponent.coms[i] != null)
                {
                    string key = acomponent.coms[i].GetType().Name;
                    if (!type_name_s.Contains(key))
                    {
                        type_name_s.Add(key);
                    }
                }
                for (int x = 0; x < acomponent.objExName.Length; x++)
                {
                    if (x != i && temp == acomponent.objExName[x])
                    {
                        isRepaceField = true;
                        break;
                    }
                }
            }
            isChange = false;
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(target);
            }
        }
        if (isRepaceField)
        {
            EditorGUILayout.HelpBox("有字段重复，请重命名", MessageType.Error);
        }
    }

    void NewAddObject()
    {
        if (currLength != acomponent.coms.Length)
        {
            isChange = true;
            Component[] tempComs = acomponent.coms;
            string[] tempStrs = acomponent.objExName;
            acomponent.coms = new Component[currLength];
            acomponent.objExName = new string[currLength];
            for (int i = 0; i < currLength; i++)
            {
                if (i < tempComs.Length)
                {
                    acomponent.coms[i] = tempComs[i];
                    acomponent.objExName[i] = tempStrs[i];
                }
                else
                {
                    AutoSetComp(i, tempObj);
                }
            }
        }
    }

    void AutoSetComp(int i, Component _Obj)
    {
        acomponent.coms[i] = _Obj;
        bool is_auto_set = false;
        if (m_RuleHelper.IsValidBind(_Obj.transform, m_TempFiledNames, m_TempComponentTypeNames))
        {
            for (int x = 0; x < m_TempFiledNames.Count; x++)
            {
                var _tempObj = _Obj.gameObject.GetComponent(m_TempComponentTypeNames[x]);
                if (_tempObj == null)
                {
                    continue;
                }
                acomponent.coms[i] = _tempObj;
                is_auto_set = true;
                break;
            }
        }
        if (!is_auto_set)
        {
            Component com = _Obj.transform.GetComponent<MyComponent>();
            if (com != null)
            {
                acomponent.coms[i] = com;
            }
        }
        bool isHave = false;
        for (int j = 0; j < acomponent.objExName.Length; j++)
        {
            if (j != i && acomponent.objExName[j] == _Obj.name)
            {
                isHave = true;
                break;
            }
        }
        if (isHave)
        {
            acomponent.objExName[i] = _Obj.name + currLength;
        }
        else
        {
            acomponent.objExName[i] = _Obj.name;
        }
        if (acomponent.objExName[i] == acomponent.gameObject.name)
        {
            acomponent.objExName[i] = "_" + acomponent.objExName[i];
        }
    }

    void SetReplace(int i0, int i)
    {
        if (i0 < 0 || i0 >= acomponent.coms.Length)
        {
            return;
        }
        var tCom = acomponent.coms[i0];
        acomponent.coms[i0] = acomponent.coms[i];
        acomponent.coms[i] = tCom;
        var tStr = acomponent.objExName[i0];
        acomponent.objExName[i0] = acomponent.objExName[i];
        acomponent.objExName[i] = tStr;

        targetCompNamesDic = new Dictionary<GameObject, string[]>();
        targetCompIndexDic = new Dictionary<int, int>();

        isChange = true;
    }


    void OnGUIJsonConfig()
    {
        if (fieldPackJson != null)
        {
            bool is_cfg = false;
            int deleteIndex = -1;
            for (int i = 0; i < fieldPackJson.list.Count; i++)
            {
                FieldJson fieldJson = fieldPackJson.list[i];
                EditorGUILayout.BeginHorizontal();
                if (configAttrDic.ContainsKey(fieldJson.c + fieldJson.n))
                {
                    ConfigAttribute configAttribute = configAttrDic[fieldJson.c + fieldJson.n];
                    if (fieldJson.c != "cfg" && !string.IsNullOrEmpty(configAttribute.dialog))
                    {
                        EditorGUILayout.LabelField(configAttribute.desc);
                        EditorGUILayout.PrefixLabel(configAttribute.dialog);
                    }
                    else
                    {
                        string field_name = fieldJson.n;
                        string field_value = fieldJson.v;
                        if (fieldJson.c == "cfg")
                        {
                            is_cfg = true;
                            EditorGUILayout.LabelField(configAttribute.desc, GUILayout.Width(100));
                            if (!string.IsNullOrEmpty(fieldJson.v))
                            {
                                string[] te = fieldJson.v.Split('@');
                                if (te != null && te.Length > 1)
                                {
                                    field_name = te[0];
                                    field_value = te[1];
                                }
                            }
                            field_name = EditorGUILayout.TextField(field_name);
                        }
                        else
                        {
                            EditorGUILayout.LabelField(configAttribute.desc);
                        }
                        switch (configAttribute.fieldType)
                        {
                            case "Int32":
                                {
                                    int.TryParse(field_value, out int tv);
                                    var temp = tv;
                                    if (configAttribute.enumDesc != null)
                                    {
                                        tv = EditorGUILayout.Popup(tv, configAttribute.enumDesc);
                                    }
                                    else
                                    {
                                        tv = EditorGUILayout.IntField(tv);
                                    }
                                    field_value = tv.ToString();
                                    if (temp != tv)
                                    {
                                        isChange = true;
                                    }
                                }
                                break;
                            case "Single":
                                {
                                    float.TryParse(field_value, out float tv);
                                    var temp = tv;
                                    tv = EditorGUILayout.FloatField(tv);
                                    field_value = tv.ToString();
                                    if (temp != tv)
                                    {
                                        isChange = true;
                                    }
                                }
                                break;
                            case "Boolean":
                                {
                                    bool.TryParse(field_value, out bool tv);
                                    var temp = tv;
                                    tv = EditorGUILayout.Toggle(tv);
                                    field_value = tv.ToString();
                                    if (temp != tv)
                                    {
                                        isChange = true;
                                    }
                                }
                                break;
                            case "String":
                                {
                                    string temp = field_value;
                                    field_value = EditorGUILayout.TextField(field_value);
                                    if (temp != field_value)
                                    {
                                        isChange = true;
                                    }
                                }
                                break;
                            case "Color":
                                {
                                    var temp = field_value;
                                    ColorUtility.TryParseHtmlString($"#{field_value}", out Color color);
                                    field_value = ColorUtility.ToHtmlStringRGBA(EditorGUILayout.ColorField(color));
                                    if (temp != field_value)
                                    {
                                        isChange = true;
                                    }
                                }
                                break;
                            case "Vector2":
                                {
                                    string temp = field_value;
                                    Vector2 v2 = new Vector2();
                                    if (!string.IsNullOrEmpty(field_value))
                                    {
                                        string[] te = field_value.Split(',');
                                        if (te.Length > 0) float.TryParse(te[0], out v2.x);
                                        if (te.Length > 1) float.TryParse(te[1], out v2.y);
                                    }
                                    v2 = EditorGUILayout.Vector2Field("", v2);
                                    field_value = v2.x + "," + v2.y;
                                    if (temp != field_value)
                                    {
                                        isChange = true;
                                    }
                                }
                                break;
                            case "Vector3":
                                {
                                    string temp = field_value;
                                    Vector3 v3 = new Vector3();
                                    if (!string.IsNullOrEmpty(field_value))
                                    {
                                        string[] te = field_value.Split(',');
                                        if (te.Length > 0) float.TryParse(te[0], out v3.x);
                                        if (te.Length > 1) float.TryParse(te[1], out v3.y);
                                        if (te.Length > 2) float.TryParse(te[2], out v3.z);
                                    }
                                    v3 = EditorGUILayout.Vector3Field("", v3);
                                    field_value = v3.x + "," + v3.y + "," + v3.z;
                                    if (temp != field_value)
                                    {
                                        isChange = true;
                                    }
                                }
                                break;

                        }
                        if (fieldJson.c == "cfg")
                        {
                            string v = field_name + "@" + field_value;
                            if (v != fieldJson.v)
                            {
                                isChange = true;
                            }
                            fieldJson.v = v;
                        }
                        else
                        {
                            fieldJson.v = field_value;
                        }
                    }
                }
                if (GUILayout.Button("×"))
                {
                    deleteIndex = i;
                }
                EditorGUILayout.EndHorizontal();
            }
            if (deleteIndex > -1)
            {
                fieldPackJson.list.RemoveAt(deleteIndex);
                isChange = true;
            }
            if (isChange)
            {
                acomponent.configJson = JsonUtility.ToJson(fieldPackJson);
                isChange = false;
                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(target);
                }
                else
                {
                    if (is_cfg)
                    {
                        acomponent.TriggerConfigChange();
                    }
                }
            }
        }
    }

    // 获取路径, 用于调试
    public static Transform GetBaseParent(Transform t)
    {
        while (t != null)
        {
            if (t.parent == null)
            {
                break;
            }
            else
            {
                t = t.parent;
            }
        }
        return t;
    }
}
