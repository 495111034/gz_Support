using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.Callbacks;
using Namotion;
using System.IO;
using System.Text;

[CustomEditor(typeof(RedPointComponent), true)]
public class RedPointCompEditor : Editor
{
    RedPointComponent redPointComp;
    private GameObject tempObj = null;
    private bool isChange = false;
    private int currLength = 0;
    private int currShowIndex = -1;
    private int deleteIndex = 0;

    public List<NamotionReflection> namotionReflections = null;

    public Dictionary<int, RedPointInfo> redPointPackDic = new Dictionary<int, RedPointInfo>();
    public Dictionary<int, string> redPointPackDescDic = new Dictionary<int, string>();

    public string savePath;

    public int currPackBaseId = 1000000;

    private RedPointCompWindow redPointCompWindow = null;

    private bool isRefshConfig = false;

    void OnEnable()
    {
        redPointComp = (RedPointComponent)target;
        if (redPointComp.redPointInfos == null)
        {
            redPointComp.redPointInfos = new List<RedPointInfo>();
        }
        currLength = redPointComp.redPointInfos.Count;
    }

    private void ReadyPackConfig()
    {
        if (isRefshConfig)
        {
            return;
        }
        isRefshConfig = true;
        redPointPackDic = new Dictionary<int, RedPointInfo>();

        string redpointpath = "../GameLibrary/Libs/myui/runtime/Editor/E_RedPointType.txt";
        if (Application.dataPath.Contains("arts_projects"))
        {
            redpointpath = "../../client/GameLibrary/Libs/myui/runtime/Editor/E_RedPointType.txt";
        }
        namotionReflections = NamotionReflection.GetNamotionsSimple(redpointpath);

        if (!string.IsNullOrEmpty(BuilderConfig.res_url))
        {
            var res_path = (BuilderConfig.res_url).Replace("file:///", "");
            EditorPathUtils.InitPaths(res_path);
        }

        savePath = PathDefs.EXPORT_PATH_DATA + "redpointconfig.txt";

        if (File.Exists(savePath))
        {
            string content = File.ReadAllText(savePath, System.Text.UTF8Encoding.UTF8);
            if (!string.IsNullOrEmpty(content))
            {
                string[] te = content.Split(';');
                for (int i = 0; i < te.Length; i++)
                {
                    if (!string.IsNullOrEmpty(te[i]))
                    {
                        string[] tte = te[i].Split(':');
                        if (tte.Length == 3)
                        {
                            int.TryParse(tte[0], out int packId);
                            if (packId > currPackBaseId)
                            {
                                currPackBaseId = packId;
                            }
                            if (!redPointPackDic.TryGetValue(packId, out RedPointInfo list))
                            {
                                list = new RedPointInfo();
                                redPointPackDic.Add(packId, list);
                            }
                            redPointPackDescDic[packId] = tte[1];
                            if (!string.IsNullOrEmpty(tte[2]))
                            {
                                string[] tte2 = tte[2].Split(',');
                                for (int j = 0; j < tte2.Length; j++)
                                {
                                    if (int.TryParse(tte2[j], out int v))
                                    {
                                        Add(list, v, false);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private void OnDisable()
    {
        if (redPointCompWindow != null)
        {
            redPointCompWindow.Close();
            redPointCompWindow = null;
        }
    }

    private bool is_show_gui = false;
    public override void OnInspectorGUI()
    {
        if (Application.isPlaying)
        {
            if (EditorGUILayout.DropdownButton(new GUIContent("显示首位红点配置"), FocusType.Keyboard))
            {
                is_show_gui = true;
            }
        }
        else { is_show_gui = true; }

        tempObj = EditorGUILayout.ObjectField(null, typeof(GameObject), true, GUILayout.Height(35)) as GameObject;
        if (tempObj != null)
        {
            for (int j = 0; j < redPointComp.redPointInfos.Count; j++)
            {
                if (redPointComp.redPointInfos[j].go == tempObj)
                {
                    tempObj = null;
                    break;
                }
            }
            if (tempObj != null)
            {
                currLength++;
                isChange = true;
            }
        }
        if (currLength != redPointComp.redPointInfos.Count)
        {
            isChange = true;
            List<RedPointInfo> tempComs = redPointComp.redPointInfos;
            redPointComp.redPointInfos = new List<RedPointInfo>();
            for (int i = 0; i < currLength; i++)
            {
                if (i < tempComs.Count)
                {
                    redPointComp.redPointInfos.Add(tempComs[i]);
                }
                else
                {
                    redPointComp.redPointInfos.Add(new RedPointInfo() { go = tempObj, bind_types = new List<int>() });
                }
            }
        }
        deleteIndex = -1;

        for (int i = 0; i < redPointComp.redPointInfos.Count; i++)
        {
            RedPointInfo redPointInfo = redPointComp.redPointInfos[i];
            EditorGUILayout.BeginHorizontal();
            if (currShowIndex != i)
            {
                if (EditorGUILayout.Toggle("", false, GUILayout.Height(20), GUILayout.Width(20)))
                {
                    currShowIndex = i;
                    if (redPointCompWindow != null)
                    {
                        redPointCompWindow.redPointInfo = redPointInfo;
                        redPointCompWindow.redPointCompEditor = this;
                        redPointCompWindow.Repaint();
                    }
                }
            }
            else if (!EditorGUILayout.Toggle("", true, GUILayout.Height(20), GUILayout.Width(20)))
            {
                currShowIndex = -1;
                if (redPointCompWindow != null)
                {
                    redPointCompWindow.Close();
                    redPointCompWindow = null;
                }
            }
            EditorGUILayout.LabelField(i.ToString(), GUILayout.Width(20));
            GameObject go = EditorGUILayout.ObjectField(redPointInfo.go, typeof(GameObject), true) as GameObject;
            if (go != redPointInfo.go)
            {
                redPointInfo.go = go;
                isChange = true;
            }
            if (is_show_gui && currShowIndex != i)
            {
                string desc = GetHelpBoxDesc(redPointInfo, true);
                EditorGUILayout.PrefixLabel(desc);
            }
            if (EditorGUILayout.DropdownButton(new GUIContent("打开配置"), FocusType.Keyboard))
            {
                ReadyPackConfig();
                isChange = true;
                currShowIndex = i;
                if (redPointCompWindow == null)
                {
                    redPointCompWindow = EditorWindow.GetWindow<RedPointCompWindow>();
                }
                else
                {
                    redPointCompWindow.Show();
                }
                redPointCompWindow.redPointCompEditor = this;
                redPointCompWindow.redPointInfo = redPointInfo;
                redPointCompWindow.Repaint();
            }
            if (EditorGUILayout.DropdownButton(new GUIContent("删除"), FocusType.Keyboard))
            {
                deleteIndex = i;
            }
            EditorGUILayout.EndHorizontal();
            if (currShowIndex == i)
            {
                string desc = GetHelpBoxDesc(redPointInfo, false);
                EditorGUILayout.HelpBox(desc, MessageType.Info);
            }
        }

        if (deleteIndex > -1)
        {
            List<RedPointInfo> tempComs = redPointComp.redPointInfos;
            currLength--;
            redPointComp.redPointInfos.RemoveAt(deleteIndex);
            isChange = true;
        }

        if (isChange)
        {
            isChange = false;
            if (!Application.isPlaying)
            {
                EditorUtility.SetDirty(target);
            }
        }
    }

    public string GetHelpBoxDesc(RedPointInfo redPointInfo, bool is_first)
    {
        var getLog_Dic = redPointInfo.GetLog_Dic;
        string desc = "";
        for (int j = 0; j < redPointInfo.bind_packs.Count; j++)
        {
            int v = redPointInfo.bind_packs[j];
            string t = GetEnumDesc(v, true);
            if (!string.IsNullOrEmpty(t))
            {
                desc += j == 0 ? t + "," + v : "\n" + t + "," + v;
                if (is_first)
                {
                    return desc;
                }
            }
        }
        for (int j = 0; j < redPointInfo.bind_types.Count; j++)
        {
            int v = redPointInfo.bind_types[j];
            string t = GetEnumDesc(v);
            if (!string.IsNullOrEmpty(t))
            {
                getLog_Dic.TryGetValue(v, out int num);
                desc += (string.IsNullOrEmpty(desc) ? t + "," + v : "\n" + t + "," + v) + ": " + num;
                if (is_first)
                {
                    return desc;
                }
            }
        }
        return desc;
    }

    public string GetEnumDesc(int v, bool isPack = false)
    {
        ReadyPackConfig();
        if (namotionReflections == null) return "";
        if (isPack)
        {
            if (redPointPackDescDic.ContainsKey(v))
            {
                return "包：" + redPointPackDescDic[v];
            }
        }
        else
        {
            for (int i = 0; i < namotionReflections.Count; i++)
            {
                if (namotionReflections[i].value == v)
                {
                    return namotionReflections[i].summaryDesc;
                }
            }
        }
        return "";
    }

    public void Add(RedPointInfo redPoint, int v, bool isPack)
    {
        int[] array = isPack ? redPoint.bind_packs.ToArray() : redPoint.bind_types.ToArray();
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] == v)
            {
                return;
            }
        }
        var temp = array;
        array = new int[temp.Length + 1];
        for (int i = 0; i < array.Length; i++)
        {
            if (i < temp.Length)
            {
                array[i] = temp[i];
            }
            else
            {
                array[i] = v;
            }
        }
        if (isPack)
        {
            redPoint.bind_packs = new List<int>(array);
        }
        else
        {
            redPoint.bind_types = new List<int>(array);
        }
    }

    public void Remove(RedPointInfo redPoint, int v, bool isPack)
    {
        int[] array = isPack ? redPoint.bind_packs.ToArray() : redPoint.bind_types.ToArray();
        if (array.Length == 0)
        {
            return;
        }
        for (int x = 0; x < array.Length; x++)
        {
            if (array[x] == v)
            {
                var temp = array;
                array = new int[temp.Length - 1];
                int index = 0;
                for (int i = 0; i < temp.Length; i++)
                {
                    if (i != x)
                    {
                        array[index++] = temp[i];
                    }
                }
                if (isPack)
                {
                    redPoint.bind_packs = new List<int>(array);
                }
                else
                {
                    redPoint.bind_types = new List<int>(array);
                }
                return;
            }
        }
    }
}


public class RedPointCompWindow : EditorWindow
{

    public EditorWindow editorWindow;
    public RedPointCompEditor redPointCompEditor;
    public RedPointInfo redPointInfo;

    private List<NamotionReflection> namotionReflections { get { return redPointCompEditor.namotionReflections; } set { redPointCompEditor.namotionReflections = value; } }

    private int startHorCount = 50;
    private bool isStartHor = true;
    private int index = 0;

    private Vector2 srollve2 = new Vector2();

    private Dictionary<int, RedPointInfo> redPointPackDic { get { return redPointCompEditor.redPointPackDic; } set { redPointCompEditor.redPointPackDic = value; } }
    private Dictionary<int, string> redPointPackDescDic { get { return redPointCompEditor.redPointPackDescDic; } set { redPointCompEditor.redPointPackDescDic = value; } }

    private int currPackBaseId { get { return redPointCompEditor.currPackBaseId; } set { redPointCompEditor.currPackBaseId = value; } }
    private int currEditPackId = 0;
    private bool isOpenEdit = false;
    private bool isInit = false;
    private HashSet<int> foldouts = new HashSet<int>();
    private HashSet<int> foldoutPacks = new HashSet<int>();
    private string savePath { get { return redPointCompEditor.savePath; } }

    private int ShowType = 2;

    private void Init()
    {
        isInit = true;
        if (redPointInfo.go == null)
        {
            ShowType = 2;
        }

    }

    private void OnDisable()
    {
        SavePackConfig();
    }

    private void OnGUI()
    {
        if (redPointInfo == null)
        {
            return;
        }
        else if (!isInit)
        {
            Init();
        }

        startHorCount = (int)(this.position.height / 5);

        if (redPointInfo.go != null)
        {
            EditorGUILayout.ObjectField(redPointInfo.go, typeof(GameObject), true);
        }
        EditorGUILayout.Space();
        if (redPointInfo.go != null)
        {
            EditorGUILayout.BeginHorizontal();
            if (ShowType == 2)
            {
                if (!EditorGUILayout.ToggleLeft("节点 " + redPointInfo.bind_types.Count, true, GUILayout.Width(100)))
                {
                    ShowType = 0;
                }
            }
            else
            {
                if (EditorGUILayout.ToggleLeft("节点 " + redPointInfo.bind_types.Count, false, GUILayout.Width(100)))
                {
                    ShowType = 2;
                }
            }
            if (ShowType == 1)
            {
                EditorGUILayout.BeginHorizontal();
                if (!EditorGUILayout.ToggleLeft("红点包 " + redPointInfo.bind_packs.Count, true, GUILayout.Width(100)))
                {
                    ShowType = 0;
                }
                if (EditorGUILayout.DropdownButton(new GUIContent("新建红点包"), FocusType.Passive, GUILayout.MaxWidth(80)))
                {
                    int id = ++currPackBaseId;
                    redPointPackDescDic[id] = "新红点包";
                    redPointPackDic.Add(id, new RedPointInfo());
                }
                if (isOpenEdit)
                {
                    if (EditorGUILayout.DropdownButton(new GUIContent("关闭配置"), FocusType.Passive, GUILayout.MaxWidth(80)))
                    {
                        isOpenEdit = false;
                    }
                }
                else if (EditorGUILayout.DropdownButton(new GUIContent("打开配置"), FocusType.Passive, GUILayout.MaxWidth(80)))
                {
                    isOpenEdit = true;
                }
                EditorGUILayout.EndHorizontal();
            }
            else
            {
                if (EditorGUILayout.ToggleLeft("红点包 " + redPointInfo.bind_packs.Count, false, GUILayout.Width(100)))
                {
                    ShowType = 1;
                    currEditPackId = 0;
                    isOpenEdit = false;
                }
            }

            EditorGUILayout.EndHorizontal();
        }
        index = 0;

        isStartHor = true;

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        srollve2 = EditorGUILayout.BeginScrollView(srollve2);
        if (ShowType == 1)
        {
            int iIndex = 0;
            foreach (var kv in redPointPackDic)
            {
                if (isStartHor)
                {
                    isStartHor = false;
                    GUILayout.BeginArea(new Rect(iIndex / startHorCount * 250 + 20, 0, 200, this.position.height));
                    EditorGUILayout.BeginVertical();
                }
                iIndex++;
                EditorGUILayout.BeginHorizontal();
                if (currEditPackId == kv.Key)
                {
                    if (IsSelectByPack(kv.Key))
                    {
                        if (!EditorGUILayout.ToggleLeft("", true, GUILayout.Width(20)))
                        {
                            redPointCompEditor.Remove(redPointInfo, kv.Key, true);
                            SelectSortPack();
                        }
                    }
                    else
                    {
                        if (EditorGUILayout.ToggleLeft("", false, GUILayout.Width(20)))
                        {
                            redPointCompEditor.Add(redPointInfo, kv.Key, true);
                            SelectSortPack();
                        }
                    }
                    redPointPackDescDic[kv.Key] = EditorGUILayout.TextField(redPointPackDescDic[kv.Key], GUILayout.MaxWidth(100));
                    if (EditorGUILayout.DropdownButton(new GUIContent("确定"), FocusType.Passive, GUILayout.MaxWidth(40)))
                    {
                        currEditPackId = 0;
                    }
                    if (EditorGUILayout.DropdownButton(new GUIContent("配置"), FocusType.Passive, GUILayout.MaxWidth(40)))
                    {
                        RedPointCompSubWindow pointCompWindow = EditorWindow.GetWindow<RedPointCompSubWindow>();
                        pointCompWindow.editorWindow = this;
                        pointCompWindow.redPointCompEditor = redPointCompEditor;
                        pointCompWindow.redPointInfo = kv.Value;
                    }
                }
                else
                {
                    if (IsSelectByPack(kv.Key))
                    {
                        if (!EditorGUILayout.ToggleLeft("", true, GUILayout.Width(20)))
                        {
                            redPointCompEditor.Remove(redPointInfo, kv.Key, true);
                            SelectSortPack();
                        }
                    }
                    else
                    {
                        if (EditorGUILayout.ToggleLeft("", false, GUILayout.Width(20)))
                        {
                            redPointCompEditor.Add(redPointInfo, kv.Key, true);
                            SelectSortPack();
                        }
                    }
                    if (isOpenEdit)
                    {
                        if (EditorGUILayout.DropdownButton(new GUIContent("编辑"), FocusType.Passive, GUILayout.MaxWidth(40)))
                        {
                            currEditPackId = kv.Key;
                        }
                    }
                    bool isFoldout = EditorGUILayout.Foldout(foldoutPacks.Contains(kv.Key), redPointPackDescDic[kv.Key] + $" ({kv.Value.bind_types.Count})", true);
                    if (!foldoutPacks.Contains(kv.Key) && isFoldout)
                    {
                        foldoutPacks.Add(kv.Key);
                    }
                    else if (!isFoldout)
                    {
                        foldoutPacks.Remove(kv.Key);
                    }
                }
                EditorGUILayout.EndHorizontal();

                if (foldoutPacks.Contains(kv.Key))
                {
                    if (kv.Value.bind_types != null && kv.Value.bind_types.Count > 0)
                    {
                        string desc = "";
                        for (int j = 0; j < kv.Value.bind_types.Count; j++)
                        {
                            int v = kv.Value.bind_types[j];
                            desc += j == 0 ? redPointCompEditor.GetEnumDesc(v) : "\n" + redPointCompEditor.GetEnumDesc(v);
                        }
                        EditorGUILayout.HelpBox(desc, MessageType.Info);
                    }
                }

                index++;
                if (index >= startHorCount)
                {
                    index = 0;
                    EditorGUILayout.EndVertical();
                    GUILayout.EndArea();
                    isStartHor = true;
                }
            }
        }
        else if(ShowType == 2)
        {
            int foldOutIndex = 0;
            for (int i = 0; i < namotionReflections.Count; i++)
            {
                if (isStartHor)
                {
                    isStartHor = false;
                    GUILayout.BeginArea(new Rect(i / startHorCount * 250 + 20, 0, 230, this.maxSize.y));
                }
                if (namotionReflections[i].value == 0)
                {
                    foldOutIndex = i;
                    EditorGUILayout.BeginHorizontal();
                    int num = 0;
                    for (int x = i + 1; x < namotionReflections.Count; x++)
                    {
                        if (namotionReflections[x].value == 0)
                        {
                            break;
                        }
                        else
                        {
                            if (IsSelect(namotionReflections[x].value))
                            {
                                num++;
                            }
                        }
                    }
                    bool isFoldout = EditorGUILayout.Foldout(foldouts.Contains(foldOutIndex), namotionReflections[i].summaryDesc + " (" + num + ")", true);
                    if (isFoldout && EditorGUILayout.DropdownButton(new GUIContent("全选/反选"), FocusType.Passive, GUILayout.Width(60)))
                    {
                        for (int x = i + 1; x < namotionReflections.Count; x++)
                        {
                            if (namotionReflections[x].value == 0)
                            {
                                break;
                            }
                            else
                            {
                                if (num > 0)
                                {
                                    redPointCompEditor.Remove(redPointInfo, namotionReflections[x].value, false);
                                }
                                else
                                {
                                    redPointCompEditor.Add(redPointInfo, namotionReflections[x].value, false);
                                }
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    if (!foldouts.Contains(foldOutIndex) && isFoldout)
                    {
                        foldouts.Add(foldOutIndex);
                    }
                    else if(!isFoldout)
                    {
                        foldouts.Remove(foldOutIndex);
                    }
                    
                    if (isFoldout)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("", GUILayout.Width(10));
                        EditorGUILayout.BeginVertical();
                    }
                }
                else if(foldouts.Contains(foldOutIndex))
                {
                    if (IsSelect(namotionReflections[i].value))
                    {
                        if (!EditorGUILayout.ToggleLeft(namotionReflections[i].summaryDesc + $":{namotionReflections[i].value}", true, GUILayout.Width(200)))
                        {
                            redPointCompEditor.Remove(redPointInfo, namotionReflections[i].value, false);
                            if (editorWindow != null)
                            {
                                editorWindow.Repaint();
                            }
                        }
                    }
                    else
                    {
                        if (EditorGUILayout.ToggleLeft(namotionReflections[i].summaryDesc + $":{namotionReflections[i].value}", false, GUILayout.Width(200)))
                        {
                            redPointCompEditor.Add(redPointInfo, namotionReflections[i].value, false);

                            if (editorWindow != null)
                            {
                                editorWindow.Repaint();
                            }
                        }
                    }
                }
                if (foldouts.Contains(foldOutIndex))
                {
                    if (i + 1 < namotionReflections.Count)
                    {
                        if (namotionReflections[i + 1].value == 0)
                        {
                            EditorGUILayout.EndVertical();
                            EditorGUILayout.EndHorizontal();
                        }
                    }
                    else
                    {
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                    }
                }
                index++;
                if (index >= startHorCount)
                {
                    index = 0;
                    GUILayout.EndArea();
                    isStartHor = true;
                }
            }
        }
        if (!isStartHor)
        {
            if (ShowType == 1)
            {
                EditorGUILayout.EndVertical();
            }
            GUILayout.EndArea();
            isStartHor = true;
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndHorizontal();
        
    }

    private void SavePackConfig()
    {
        if (string.IsNullOrEmpty(savePath) || redPointInfo.go == null)
        {
            return;
        }
        StringBuilder sb = new StringBuilder();
        foreach (var kv in redPointPackDic)
        {
            string list = "";
            for (int i = 0; i < kv.Value.bind_types.Count; i++)
            {
                if (i == 0)
                {
                    list += kv.Value.bind_types[i];
                }
                else
                {
                    list += "," + kv.Value.bind_types[i];
                }
            }
            sb.Append(kv.Key.ToString()).Append(":").Append(redPointPackDescDic[kv.Key]).Append(":").Append(list).Append(";");
        }
        File.WriteAllText(savePath, sb.ToString(), System.Text.UTF8Encoding.UTF8);
    }

    
    private bool IsSelectByPack(int v)
    {
        for (int i = 0; i < redPointInfo.bind_packs.Count; i++)
        {
            if (redPointInfo.bind_packs[i] == v)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsSelect(int v)
    {
        for (int i = 0; i < redPointInfo.bind_types.Count; i++)
        {
            if (redPointInfo.bind_types[i] == v)
            {
                return true;
            }
        }
        return false;
    }

    //private int SelectSort(NamotionReflection a, NamotionReflection b)
    //{
    //    int a1 = 0;
    //    int b1 = 0;
    //    if (IsSelect(a.value))
    //    {
    //        a1 = 100 + a.value;
    //    }
    //    if (IsSelect(b.value))
    //    {
    //        b1 = 100 + b.value;
    //    }
    //    return b1.CompareTo(a1);
    //}

    private void SelectSortPack()
    {
        List<int> list = new List<int>(redPointPackDic.Keys);
        list.Sort(SelectSortPack);
        var temp = redPointPackDic;
        redPointPackDic = new Dictionary<int, RedPointInfo>();
        for (int i = 0; i < list.Count; i++)
        {
            redPointPackDic[list[i]] = temp[list[i]];
        }
    }


    private int SelectSortPack(int a, int b)
    {
        int a1 = 0;
        int b1 = 0;
        if (IsSelectByPack(a))
        {
            a1 = 100 + a;
        }
        if (IsSelectByPack(b))
        {
            b1 = 100 + b;
        }
        return b1.CompareTo(a1);
    }
}

public class RedPointCompSubWindow : RedPointCompWindow  { }

public class ListenScriptLoad
{
    [DidReloadScripts]
    public static void Listen()
    {
        string scrPath = Application.dataPath + "/GameLogic/game/RedPoint/E_RedPointType.cs";
        if (File.Exists(scrPath))
        {
            string dstPath = Application.dataPath + "/Libs/myui/runtime/Editor/E_RedPointType.txt";
            File.WriteAllBytes(dstPath, File.ReadAllBytes(scrPath));
        }
    }
}