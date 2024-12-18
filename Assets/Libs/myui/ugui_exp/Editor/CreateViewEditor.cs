using UnityEditor;
using UnityEngine;
using System.Text;
using System.IO;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Collections;

public class CreateViewEditor
{

    //[MenuItem("Tools/测试图片转线性空间")]
    //private static void OpenSimplygon123()
    //{
    //    Texture2D scrTex = Selection.objects[0] as Texture2D;
    //    Texture2D temp = new Texture2D(scrTex.width, scrTex.height, TextureFormat.RGBA32, true);
    //    Color[] pixels = scrTex.GetPixels();
    //    for (int j = 0; j < pixels.Length; j++)
    //    {
    //        Color pixel = pixels[j];
    //        pixel.r = Mathf.Pow(pixel.r, 2.2f);
    //        pixel.g = Mathf.Pow(pixel.g, 2.2f);
    //        pixel.b = Mathf.Pow(pixel.b, 2.2f);
    //        pixels[j] = pixel;
    //    }
    //    temp.SetPixels(pixels);
    //    temp.Apply();
    //    var bytes = temp.EncodeToPNG();
    //    File.WriteAllBytes(AssetDatabase.GetAssetPath(Selection.objects[0]).Replace(".png", "_line.png"), bytes);
    //}


    //[MenuItem("Tools/检测所有的界面id")]
    //private static void CheckAllView()
    //{
    //    Dictionary<int, System.Type> viewTypeDic = new Dictionary<int, System.Type>();
    //    List<System.Type> auto_view_class = new List<System.Type>();
    //    System.Type[] types = typeof(Main).Assembly.GetTypes();
    //    for (int i = 0; i < types.Length; i++)
    //    {
    //        System.Type t = types[i];
    //        if (t.Namespace == "GameLogic.ViewAuto")
    //        {
    //            auto_view_class.Add(t);
    //        }
    //    }
    //    for (int i = 0; i < types.Length; i++)
    //    {
    //        System.Type t = types[i];
    //        if (t.BaseType != null && t.BaseType.Namespace == "GameLogic.ViewAuto")
    //        {
    //            if (auto_view_class.Contains(t.BaseType))
    //            {
    //                auto_view_class.Remove(t.BaseType);
    //            }
    //        }
    //    }
    //    for (int i = auto_view_class.Count - 1; i > -1; i--)
    //    {
    //        int viewId = (int)auto_view_class[i].GetField("viewId").GetValue(auto_view_class[i]);
    //        UnityEngine.Debug.LogError("没有被继承的界面：" + auto_view_class[i] + ", " + viewId);
    //    }

    //    string uiSavePath = "../GameLogic/script/view/auto";
    //    if (Application.dataPath.Contains("arts_projects"))
    //    {
    //        uiSavePath = "../../client/GameLogic/script/view/auto";
    //    }
    //    List<int> list = new List<int>();
    //    Dictionary<int, string> hashset = new Dictionary<int, string>();
    //    string[] files = Directory.GetFiles(uiSavePath, "*.cs", SearchOption.AllDirectories);
    //    for (int i = 0; i < files.Length; i++)
    //    {
    //        int viewId = GetViewId(files[i]);
    //        if (viewId == 0)
    //        {
    //            UnityEngine.Debug.LogError("未知UI脚本资源 viewId = 0：" + files[i]);
    //        }
    //        else
    //        {
    //            if (!hashset.ContainsKey(viewId))
    //            {
    //                hashset.Add(viewId, files[i]);
    //            }
    //            else
    //            {
    //                UnityEngine.Debug.LogError("viewId有重复：" + viewId + "  , " + hashset[viewId] + "  和  " + files[i]);
    //            }
    //            list.Add(viewId);
    //        }
    //    }

    //    list.Sort();
    //    for (int i = 0; i < list.Count; i++)
    //    {
    //        UnityEngine.Debug.LogError("viewId：" + list[i] + " , " + hashset[list[i]]);
    //    }
    //}

    public static int RunCommand(string workingDir, string program, string[] args)
    {
        using (Process p = new Process())
        {
            p.StartInfo.WorkingDirectory = workingDir;
            p.StartInfo.FileName = program;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            string argsStr = string.Join(" ", args.Select(arg => "\"" + arg + "\""));
            p.StartInfo.Arguments = argsStr;
            p.Start();
            p.WaitForExit();
            return p.ExitCode;
        }
    }

    public static bool ExistProgram(string prog)
    {
#if UNITY_EDITOR_WIN
        return RunCommand(".", "where", new string[] { prog }) == 0;
#elif UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
            return RunCommand(".", "which", new string[] {prog}) == 0;
#endif
    }

    static int baseViewId = 100;
    static bool isCheckSVN = false;
    static List<string> commit_list = null;
    static string all_view_path = "";

    private static void GetClientPath()
    {
        string uiSavePath = "../GameLogic/script/view/auto";
        if (Application.dataPath.Contains("arts_projects") || !Directory.Exists(uiSavePath))
        {
            uiSavePath = "../../client/GameLogic/script/view/auto";
            if (!Directory.Exists(uiSavePath))
            {
                string config_path = "../config.txt";
                if (!File.Exists(config_path))
                {
                    EditorUtility.DisplayDialog("提示", "路径不正确，请检查你的项目和config.txt", "ok");
                    return;
                }
                StringReader reader = new StringReader(File.ReadAllText(config_path, System.Text.UTF8Encoding.UTF8));
                string line = reader.ReadLine();
                line = line.Replace("file:///", "");
                line = line.Replace("res_url=", "");
                if (line.EndsWith("/"))
                {
                    line = line.Replace("assetbundles/", "GameLogic/script/view/auto");
                }
                else
                {
                    line = line.Replace("assetbundles", "GameLogic/script/view/auto");
                }
                uiSavePath = line;
                reader.Close();
                reader.Dispose();
            }
        }
        all_view_path = uiSavePath;
    }

    //[MenuItem("Tools/给所有自动化UI加guid")]
    //public static void FixAllAutoView()
    //{
    //    Dictionary<string, string> ui_name_to_guid_dic = new Dictionary<string, string>();
    //    string rootPath = "Assets/ui/prefab/myuipanel";
    //    string[] resFiles = AssetDatabase.FindAssets("t:Prefab", new string[] { rootPath });
    //    foreach (var guid in resFiles)
    //    {
    //        string path = AssetDatabase.GUIDToAssetPath(guid);
    //        if (path.Contains("/Resources/"))
    //        {
    //            path = path.Substring(0, path.LastIndexOf('.')).ToLower();
    //        }
    //        else
    //        {
    //            path = Path.GetFileNameWithoutExtension(path);
    //        }
    //        ui_name_to_guid_dic[path] = guid;
    //    }

    //    GetClientPath();
    //    string[] files = Directory.GetFiles(all_view_path, "*.cs", SearchOption.AllDirectories);
    //    for (int i = 0; i < files.Length; i++)
    //    {
    //        string name = Path.GetFileNameWithoutExtension(files[i]);
    //        string guid = GetGUID(files[i]);
    //        if (string.IsNullOrEmpty(guid) && ui_name_to_guid_dic.TryGetValue(name, out string g1))
    //        {
    //            StringBuilder sb = new StringBuilder();
    //            string content = File.ReadAllText(files[i]);
    //            StringReader sr = new StringReader(content);
    //            while (sr.Peek() > 1)
    //            {
    //                string line = sr.ReadLine();
    //                sb.AppendLine(line);
    //                if (line.Contains("public static int viewId"))
    //                {
    //                    sb.AppendLine("        public static string resGUID = \"" + g1 + "\";");
    //                }
    //            }
    //            sr.Close();
    //            sr.Dispose();
    //            File.WriteAllText(files[i], sb.ToString(), System.Text.UTF8Encoding.UTF8);
    //        }
    //        else if (ui_name_to_guid_dic.TryGetValue(name, out string g))
    //        {
    //            if (g.Equals(guid))
    //            {
    //                UnityEngine.Debug.LogError("GUID 匹配失败：" + name + " , " + guid  + " , 新的：" + g);
    //            }
    //        }
    //        else 
    //        {
    //            UnityEngine.Debug.LogError("GUID 匹配失败，本地没有找到prefab：" + name + " , " + guid);
    //        }
    //    }
    //}

    //[MenuItem("Tools/使用GUID重新匹配UI资源名字")]
    //public static void FixAllAutoView2()
    //{
    //    Dictionary<string, string> ui_guid_to_name_dic = new Dictionary<string, string>();
    //    string rootPath = "Assets/ui/prefab/myuipanel";
    //    string[] resFiles = AssetDatabase.FindAssets("t:Prefab", new string[] { rootPath });
    //    foreach (var guid in resFiles)
    //    {
    //        string path = AssetDatabase.GUIDToAssetPath(guid);
    //        if (path.Contains("/Resources/"))
    //        {
    //            path = path.Substring(0, path.LastIndexOf('.')).ToLower();
    //        }
    //        else
    //        {
    //            path = Path.GetFileNameWithoutExtension(path);
    //        }
    //        ui_guid_to_name_dic[guid.Trim()] = path;
    //    }
    //    GetClientPath();
    //    string[] files = Directory.GetFiles(all_view_path, "*.cs", SearchOption.AllDirectories);
    //    for (int i = 0; i < files.Length; i++)
    //    {
    //        string guid = GetGUID(files[i]).Replace("\"", "");
    //        if (!string.IsNullOrEmpty(guid) && ui_guid_to_name_dic.TryGetValue(guid, out string name1))
    //        {
    //            StringBuilder sb = new StringBuilder();
    //            string content = File.ReadAllText(files[i]);
    //            StringReader sr = new StringReader(content);
    //            while (sr.Peek() > 1)
    //            {
    //                string line = sr.ReadLine();
    //                if (line.Contains("protected override string resAsset"))
    //                {
    //                    sb.AppendLine($"        protected override string resAsset => \"{name1}\";");
    //                }
    //                else
    //                {
    //                    sb.AppendLine(line);
    //                }
    //            }
    //            sr.Close();
    //            sr.Dispose();
    //            File.WriteAllText(files[i], sb.ToString(), System.Text.UTF8Encoding.UTF8);
    //        }
    //    }
    //}

    //[MenuItem("Tools/检测并规则化命名UI资源名字")]
    //private static void RenameUIPrefab()
    //{
    //    if (EditorUtility.DisplayDialog("提示", "即将开始批量给UI资源加_panel后缀，是否已做过GUID绑定？", "ok", "cancel"))
    //    {
    //        string rootPath = "Assets/ui/prefab/myuipanel";
    //        string[] resFiles = AssetDatabase.FindAssets("t:Prefab", new string[] { rootPath });
    //        foreach (var guid in resFiles)
    //        {
    //            string file_path = AssetDatabase.GUIDToAssetPath(guid);
    //            string file_name = Path.GetFileNameWithoutExtension(file_path);
    //            if (!file_name.EndsWith("_panel"))
    //            {
    //                //修改文件名
    //                AssetDatabase.RenameAsset(file_path, file_name + "_panel");
    //            }
    //        }

    //        AssetDatabase.Refresh();
    //    }
    //}

    //[MenuItem("Tools/把所有继承的UI名字匹配到资源名字")]
    //private static void RenameAllInheritUIPrefab()
    //{
    //    GetClientPath();
    //    Dictionary<string, string> old_name_to_new_name_dic = new Dictionary<string, string>();
    //    string[] files = Directory.GetFiles(all_view_path, "*.cs", SearchOption.AllDirectories);
    //    for (int i = 0; i < files.Length; i++)
    //    {
    //        string content = File.ReadAllText(files[i]);
    //        StringReader sr = new StringReader(content);
    //        string asset_name = "";
    //        while (sr.Peek() > 1)
    //        {
    //            string line = sr.ReadLine();
    //            if (line.Contains("protected override string resAsset"))
    //            {
    //                line = line.Replace("protected override string resAsset => ", "");
    //                line = line.Replace("\"", "");
    //                asset_name = line.Replace(";", "").Trim();
    //                break;
    //            }
    //        }
    //        if (asset_name.Contains("/"))
    //        {
    //            asset_name = Path.GetFileNameWithoutExtension(asset_name);
    //        }
    //        string old_name = "";
    //        sr = new StringReader(content);
    //        StringBuilder sb = new StringBuilder();
    //        while (sr.Peek() > 1)
    //        {
    //            string line = sr.ReadLine();
    //            if (line.Contains("View.BaseView"))
    //            {
    //                old_name = line.Replace("public class ", "").Replace(" : View.BaseView", "").Trim();
    //                sb.AppendLine($"    public class {asset_name} : View.BaseView");
    //            }
    //            else
    //            {
    //                sb.AppendLine(line);
    //            }
    //        }
    //        sr.Close();
    //        sr.Dispose();
    //        File.Delete(files[i]);
    //        File.WriteAllText(all_view_path + "/" + asset_name + ".cs", sb.ToString(), System.Text.UTF8Encoding.UTF8);
    //        old_name_to_new_name_dic.Add(old_name.Trim(), asset_name);
    //    }
    //    all_view_path = all_view_path.Replace("/view/auto", "");
    //    files = Directory.GetFiles(all_view_path, "*.cs", SearchOption.AllDirectories);
    //    for (int i = 0; i < files.Length; i++)
    //    {
    //        StringBuilder sb = new StringBuilder();
    //        string content = File.ReadAllText(files[i]);
    //        StringReader sr = new StringReader(content);
    //        bool is_view = false;
    //        while (sr.Peek() > 1)
    //        {
    //            string line = sr.ReadLine();
    //            if (line.Contains("class") && line.Contains("ViewAuto."))
    //            {
    //                int index = line.IndexOf("ViewAuto");
    //                string temp = "";
    //                for (int x = index; x < line.Length; x++)
    //                {
    //                    if (line[x] == ' ' || line[x] == ',')
    //                    {
    //                        break;
    //                    }
    //                    temp += line[x];
    //                }
    //                string ui_name = temp.Replace("ViewAuto.", "");
    //                if (old_name_to_new_name_dic.TryGetValue(ui_name, out string newName))
    //                {
    //                    line = line.Replace(temp, "ViewAuto." + newName);
    //                    is_view = true;
    //                }
    //                sb.AppendLine(line);
    //            }
    //            else
    //            {
    //                sb.AppendLine(line);
    //            }
    //        }
    //        sr.Close();
    //        sr.Dispose();
    //        if (is_view)
    //        {
    //            File.WriteAllText(files[i], sb.ToString(), System.Text.UTF8Encoding.UTF8);
    //        }
    //    }
    //}

    //private static string GetGUID(string uiPath)
    //{
    //    string guid = "";
    //    if (File.Exists(uiPath))
    //    {
    //        string content = File.ReadAllText(uiPath);
    //        string findStr = "public static string resGUID = ";
    //        int index = content.IndexOf(findStr);
    //        if (index > -1)
    //        {
    //            content = content.Substring(index + findStr.Length);
    //            guid = content.Substring(0, content.IndexOf(';'));
    //        }
    //    }
    //    return guid;
    //}

    //[MenuItem("Tools/检查所有UI自动化勾上UI语言包")]
    //private static void CheckAllUIPrefabSelectSaveLange()
    //{
    //    string rootPath = "Assets/ui/prefab/myuipanel";
    //    string[] resFiles = AssetDatabase.FindAssets("t:Prefab", new string[] { rootPath });
    //    foreach (var guid in resFiles)
    //    {
    //        string file_path = AssetDatabase.GUIDToAssetPath(guid);
    //        string file_name = Path.GetFileNameWithoutExtension(file_path);
    //        if (file_name.EndsWith("_panel"))
    //        {
    //            GameObject go = PrefabUtility.LoadPrefabContents(file_path);
    //            var comps = go.GetComponentsEx<MyComponent>(true);
    //            if (comps != null)
    //            {
    //                HashSet<Component> values = new HashSet<Component>();
    //                foreach (var c in comps)
    //                {
    //                    foreach (var cp in c.coms)
    //                    {
    //                        if (!values.Contains(cp)) values.Add(cp);
    //                    }
    //                }
    //                var mytexts = go.GetComponentsEx<MyText>(true);
    //                foreach (var t in mytexts)
    //                {
    //                    if (!values.Contains(t) && IsContainsChinese(t.text))
    //                    {
    //                        t.SaveToAB = true;
    //                    }
    //                }

    //                var my_image_texts = go.GetComponentsEx<MyImageText>(true);
    //                foreach (var t in my_image_texts)
    //                {
    //                    if (!values.Contains(t) && IsContainsChinese(t.text))
    //                    {
    //                        t.SaveToAB = true;
    //                    }
    //                }
    //            }
    //            PrefabUtility.SaveAsPrefabAsset(go, file_path);
    //            PrefabUtility.UnloadPrefabContents(go);
    //        }
    //    }

    //    AssetDatabase.Refresh();
    //}


    //[MenuItem("Tools/检查所有UI是否有Slider和Button父子级关系")]
    //private static void CheckAllUIPrefabSelectSaveLange123()
    //{
    //    string rootPath = "Assets/ui/prefab/myuipanel";
    //    string[] resFiles = AssetDatabase.FindAssets("t:Prefab", new string[] { rootPath });
    //    foreach (var guid in resFiles)
    //    {
    //        string file_path = AssetDatabase.GUIDToAssetPath(guid);
    //        string file_name = Path.GetFileNameWithoutExtension(file_path);
    //        if (file_name.EndsWith("_panel"))
    //        {
    //            GameObject go = PrefabUtility.LoadPrefabContents(file_path);
    //            var comps = go.GetComponentsEx<MyComponent>(true);
    //            if (comps != null)
    //            {
    //                //HashSet<Component> values = new HashSet<Component>();
    //                //foreach (var c in comps)
    //                //{
    //                //    foreach (var cp in c.coms)
    //                //    {
    //                //        if (!values.Contains(cp)) values.Add(cp);
    //                //    }
    //                //}
    //                var mytexts = go.GetComponentsEx<MySlider>(true);
    //                foreach (var t in mytexts)
    //                {
    //                    var buts = t.gameObject.GetComponentsInChildren<MyButton>(true);
    //                    foreach (var b in buts)
    //                    {
    //                        UnityEngine.Debug.LogError("but: " + b);
    //                    }
    //                    //var buts1 = t.gameObject.GetComponentsInChildren<Button>(true);
    //                    //foreach (var b1 in buts1)
    //                    //{
    //                    //    UnityEngine.Debug.LogError("but: " + b1);
    //                    //}
    //                }

    //                var my_image_texts = go.GetComponentsEx<Slider>(true);
    //                foreach (var t in my_image_texts)
    //                {
    //                    var buts = t.gameObject.GetComponentsInChildren<MyButton>(true);
    //                    foreach (var b in buts)
    //                    {
    //                        UnityEngine.Debug.LogError("but: " + b);
    //                    }
    //                    var buts1 = t.gameObject.GetComponentsInChildren<Button>(true);
    //                    foreach (var b1 in buts1)
    //                    {
    //                        UnityEngine.Debug.LogError("but: " + b1);
    //                    }
    //                }
    //            }
    //            //PrefabUtility.SaveAsPrefabAsset(go, file_path);
    //            PrefabUtility.UnloadPrefabContents(go);
    //        }
    //    }

    //    AssetDatabase.Refresh();
    //}


    ////[MenuItem("Tools/检查所有UI自动化替换paneltype")]
    //private static void CheckAllUIPrefabSelectSaveLange()
    //{
    //    string rootPath = "Assets/ui/prefab/myuipanel";
    //    string[] resFiles = AssetDatabase.FindAssets("t:Prefab", new string[] { rootPath });
    //    foreach (var guid in resFiles)
    //    {
    //        string file_path = AssetDatabase.GUIDToAssetPath(guid);
    //        string file_name = Path.GetFileNameWithoutExtension(file_path);
    //        if (file_name.EndsWith("_panel"))
    //        {
    //            GameObject go = PrefabUtility.LoadPrefabContents(file_path);
    //            var comp = go.GetComponent<MyComponent>();
    //            if (comp != null)
    //            {
    //                var fieldPackJson = JsonUtility.FromJson<MyComponentEditor.FieldPackJson>(comp.configJson);
    //                if (fieldPackJson != null && fieldPackJson.list != null)
    //                {
    //                    bool is_has = false;
    //                    bool is_already = false;
    //                    for (int i = 0; i < fieldPackJson.list.Count; i++)
    //                    {
    //                        if (fieldPackJson.list[i].n.Equals("f12"))
    //                        {
    //                            if (fieldPackJson.list[i].v.Equals("1"))
    //                            {
    //                                is_has = true;
    //                                fieldPackJson.list[i].v = "2";
    //                            }
    //                            else if (fieldPackJson.list[i].v.Equals("2"))
    //                            {
    //                                is_has = true;
    //                                fieldPackJson.list[i].v = "1";
    //                            }
    //                        }
    //                        else if (fieldPackJson.list[i].n.Equals("ct"))
    //                        {
    //                            is_already = true;
    //                        }
    //                    }
    //                    if (!is_already && is_has)
    //                    {
    //                        fieldPackJson.list.Add(new MyComponentEditor.FieldJson() { c = "UI", n = "ct" });
    //                        comp.configJson = JsonUtility.ToJson(fieldPackJson);
    //                    }
    //                }
    //            }
    //            PrefabUtility.SaveAsPrefabAsset(go, file_path);
    //            PrefabUtility.UnloadPrefabContents(go);
    //        }
    //    }

    //    AssetDatabase.Refresh();
    //}

    private static bool IsContainsChinese(string text)
    {
        for (int i = 0; i < text.Length; i++)
        {
            if ((int)text[i] > 127)
            {
                return true;
            }
        }
        return false;
    }

    [MenuItem("Assets/创建UI类")]
    public static void CreateAllView()
    {
        if (!ExistProgram("svn"))
        {
            EditorUtility.DisplayDialog("提示", "请安装SVN！如果安装了请设置环境变量!", "ok");
            return;
        }
        GetClientPath();
        commit_list = new List<string>();
        isCheckSVN = false;
        Object[] objects = Selection.objects;
        for (int i = 0; i < objects.Length; i++)
        {
            Object obj = objects[i];
            if (obj is GameObject)
            {
                MyComponent refObj = (obj as GameObject).GetComponent<MyComponent>();
                if (refObj != null)
                {
                    string path = AssetDatabase.GetAssetPath(obj);
                    if (path.Contains("/Resources/"))
                    {
                        CreateView(obj.name, refObj, path.Substring(0, path.LastIndexOf('.')).ToLower(), path);
                    }
                    else
                    {
                        CreateView(obj.name, refObj, null, path);
                    }
                }
            }
        }
        if (isCheckSVN || commit_list.Count > 0)
        {
            //string concat = "";
            //foreach (var path in commit_list)
            //{
            //    if (string.IsNullOrEmpty(concat))
            //    {
            //        concat = path;
            //    }
            //    else
            //    {
            //        concat += "*" + path;
            //    }
            //}
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            SVNCommand(COMMIT, all_view_path);
        }

        //AssetDatabase.Refresh();
    }

    private static void CreateView(string className, MyComponent aComponent, string resPath, string assetPath)
    {
        if (string.IsNullOrEmpty(resPath))
        {
            resPath = className;
        }
        if (!Directory.Exists(all_view_path))
        {
            Directory.CreateDirectory(all_view_path);
        }
        string uiPath = $"{all_view_path}/{className}.cs";
        int viewId = GetViewId(uiPath);

        string t = "    ";
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("using UnityEngine;");
        sb.AppendLine("using UnityEngine.UI;");
        sb.AppendLine("namespace GameLogic.ViewAuto");
        sb.AppendLine("{");
        sb.Append(t).Append("public class ").Append(className).AppendLine(" : View.BaseView");
        sb.Append(t).AppendLine("{");

        if (aComponent != null)
        {
            if (viewId == 0)
            {
                if (!isCheckSVN)
                {
                    isCheckSVN = true;
                    CheckGetBaseViewId();
                }
                viewId = ++baseViewId;
                commit_list.Add(uiPath);
            }

            List<Hashtable> cfg_hashs = new List<Hashtable>();
            bool is_has_cfg_need = false;
            var configDic = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(aComponent.configJson))
            {
                var json = MiniJSON.JsonDecode(aComponent.configJson) as Hashtable;
                var list = json["list"] as ArrayList;
                for (int i = 0; i < list.Count; i++)
                {
                    Hashtable j1 = list[i] as Hashtable;
                    string v = j1["v"].ToString();
                    if (v.IndexOf('@') > -1)
                    {
                        string[] te = v.Split('@');
                        configDic[te[0]] = te[1];
                        is_has_cfg_need = true;
                        cfg_hashs.Add(j1);
                    }
                    else
                    {
                        configDic[j1["n"].ToString()] = v;
                    }
                }
            }
            sb.Append(t + t).AppendLine($"public static int viewId = {viewId};");
            sb.Append(t + t).AppendLine($"public static string resGUID = \"{AssetDatabase.AssetPathToGUID(assetPath)}\";");
            sb.Append(t + t).AppendLine($"protected override bool isSyncLoad => {configDic.ContainsKey("f20").ToString().ToLower()};");
            sb.Append(t + t).AppendLine($"protected override bool _IsAlphaFadeIn => {configDic.ContainsKey("f21").ToString().ToLower()};");
            sb.Append(t + t).AppendLine($"protected override string resAsset => \"{resPath}\";");
            for (int i = 0; i < aComponent.objExName.Length; i++)
            {
                if (aComponent.coms[i] != null)
                {
                    string __namespace = aComponent.coms[i].GetType().Namespace;
                    if (!string.IsNullOrEmpty(__namespace) && __namespace.Contains("UnityEngine"))
                    {
                        __namespace = "";
                    }
                    else if(!string.IsNullOrEmpty(__namespace))
                    {
                        __namespace += ".";
                    }
                    string comName = aComponent.coms[i].GetType().Name;
                    string fileName = aComponent.objExName[i].Replace(" ", "");
                    sb.Append(t + t).AppendLine("protected " + __namespace + comName + " " + fileName + " = null;");
                }
                else
                {
                    UnityEngine.Debug.LogError("第" + i + "个组件引用丢失");
                }
            }

            StringBuilder read_config_sb = new StringBuilder();
            if (is_has_cfg_need)
            {
                for (int i = 0; i < cfg_hashs.Count; i++)
                {
                    Hashtable j1 = cfg_hashs[i];
                    string type_name = "";
                    string def_value = "";
                    string n = j1["n"].ToString();
                    switch (n)
                    {
                        case "f01":
                            {
                                type_name = "int";
                                def_value = "0";
                            }
                            break;
                        case "f02":
                            {
                                type_name = "float";
                                def_value = "0";
                            }
                            break;
                        case "f03":
                            {
                                type_name = "bool";
                                def_value = "false";
                            }
                            break;
                        case "f04":
                            {
                                type_name = "string";
                                def_value = "\"\"";
                            }
                            break;
                        case "f05":
                            {
                                type_name = "Color";
                                def_value = "Color.white";
                            }
                            break;
                        case "f06":
                            {
                                type_name = "Vector2";
                                def_value = "Vector2.zero";
                            }
                            break;
                        case "f07":
                            {
                                type_name = "Vector3";
                                def_value = "Vector3.zero";
                            }
                            break;

                    }
                    string v = j1["v"].ToString();
                    if (v.IndexOf('@') > -1)
                    {
                        string[] te = v.Split('@');
                        sb.Append(t + t).AppendLine("protected " + type_name + " " + te[0] + " = " + def_value + ";");
                        //this._myComp.GetConfig<float>("up_speed");
                        if (type_name == "Color")
                        {
                            //ColorUtility.TryParseHtmlString("#{field_value}", out Color color);
                            read_config_sb.Append(t + t + t).AppendLine("string color_" + te[0] + " = " + $"this._myComp.GetConfig(\"{te[0]}\");");
                            read_config_sb.Append(t + t + t).AppendLine("if(ColorUtility.TryParseHtmlString($\"#{" + ("color_" + te[0]) + "}\", out Color c_" + te[0] + "))");
                            read_config_sb.Append(t + t + t).AppendLine("{");
                            read_config_sb.Append(t + t + t + t).AppendLine("this." + te[0] + " = " + "c_" + te[0] + ";");
                            read_config_sb.Append(t + t + t).AppendLine("}");
                        }
                        else if (type_name == "string")
                        {
                            read_config_sb.Append(t + t + t).AppendLine("this." + te[0] + " = " + $"this._myComp.GetConfig(\"{te[0]}\");");
                        }
                        else if (type_name == "Vector2")
                        {
                            read_config_sb.Append(t + t + t).AppendLine("string ve2_" + te[0] + " = " + $"this._myComp.GetConfig(\"{te[0]}\");");
                            read_config_sb.Append(t + t + t).AppendLine($"if (!string.IsNullOrEmpty(ve2_{te[0]}))");
                            read_config_sb.Append(t + t + t).AppendLine("{");
                            read_config_sb.Append(t + t + t + t).AppendLine($"Vector2 ve2 = Vector2.zero;");
                            read_config_sb.Append(t + t + t + t).AppendLine($"string[] te_{te[0]} = ve2_{te[0]}.Split(',');");
                            read_config_sb.Append(t + t + t + t).AppendLine($"if (te_{te[0]}.Length > 0) float.TryParse(te_{te[0]}[0], out ve2.x);");
                            read_config_sb.Append(t + t + t + t).AppendLine($"if (te_{te[0]}.Length > 1) float.TryParse(te_{te[0]}[1], out ve2.y);");
                            read_config_sb.Append(t + t + t + t).AppendLine($"{te[0]} = ve2;");
                            read_config_sb.Append(t + t + t).AppendLine("}");
                        }
                        else if (type_name == "Vector3")
                        {
                            read_config_sb.Append(t + t + t).AppendLine("string ve3_" + te[0] + " = " + $"this._myComp.GetConfig(\"{te[0]}\");");
                            read_config_sb.Append(t + t + t).AppendLine($"if (!string.IsNullOrEmpty(ve3_{te[0]}))");
                            read_config_sb.Append(t + t + t).AppendLine("{");
                            read_config_sb.Append(t + t + t + t).AppendLine($"Vector3 ve3 = Vector3.zero;");
                            read_config_sb.Append(t + t + t + t).AppendLine($"string[] te_{te[0]} = ve3_{te[0]}.Split(',');");
                            read_config_sb.Append(t + t + t + t).AppendLine($"if (te_{te[0]}.Length > 0) float.TryParse(te_{te[0]}[0], out ve3.x);");
                            read_config_sb.Append(t + t + t + t).AppendLine($"if (te_{te[0]}.Length > 1) float.TryParse(te_{te[0]}[1], out ve3.y);");
                            read_config_sb.Append(t + t + t + t).AppendLine($"if (te_{te[0]}.Length > 2) float.TryParse(te_{te[0]}[2], out ve3.z);");
                            read_config_sb.Append(t + t + t + t).AppendLine($"{te[0]} = ve3;");
                            read_config_sb.Append(t + t + t).AppendLine("}");
                        }
                        else
                        {
                            read_config_sb.Append(t + t + t).AppendLine("this." + te[0] + " = " + $"this._myComp.GetConfig<{type_name}>(\"{te[0]}\");");
                        }
                    }
                }
            }
            sb.Append(t + t).AppendLine("protected override void Load()");
            sb.Append(t + t).AppendLine("{");
            
            for (int i = 0; i < aComponent.objExName.Length; i++)
            {
                if (aComponent.coms[i] != null)
                {
                    string __namespace = aComponent.coms[i].GetType().Namespace;
                    if (!string.IsNullOrEmpty(__namespace) && __namespace.Contains("UnityEngine"))
                    {
                        __namespace = "";
                    }
                    else if (!string.IsNullOrEmpty(__namespace))
                    {
                        __namespace += ".";
                    }
                    string comName = aComponent.coms[i].GetType().Name;
                    string fileName = aComponent.objExName[i].Replace(" ", "");
                    sb.Append(t + t + t).AppendLine("this." + fileName + " = " + "this._myComp.GetComp<" + __namespace + comName + ">(\"" + aComponent.objExName[i] + "\");");
                }
            }
            sb.Append(t + t + t).AppendLine("base.Load();");
            if (is_has_cfg_need)
            {
                sb.Append(t + t + t).AppendLine("this._myComp.SetConfigChangeEvent(OnConfigChangeEvent);");
                sb.Append(t + t + t).AppendLine("OnConfigChangeEvent();");
            }
            sb.Append(t + t).AppendLine("}");

            sb.Append(t + t).AppendLine("public override void Destroy()");
            sb.Append(t + t).AppendLine("{");
            sb.Append(t + t).AppendLine("    base.Destroy();");
            for (int i = 0; i < aComponent.objExName.Length; i++)
            {
                if (aComponent.coms[i] != null)
                {
                    string fileName = aComponent.objExName[i].Replace(" ", "");
                    sb.Append(t + t + t).AppendLine("this." + fileName + " = null;");
                }
            }
            sb.Append(t + t).AppendLine("}");

            if (is_has_cfg_need)
            {
                sb.Append(t + t).AppendLine("protected virtual void OnConfigChangeEvent()");
                sb.Append(t + t).AppendLine("{");
                sb.AppendLine(read_config_sb.ToString());
                sb.Append(t + t).AppendLine("}");
            }
        }

        sb.Append(t).AppendLine("}");
        sb.AppendLine("}");


        File.WriteAllText(uiPath, sb.ToString(), System.Text.UTF8Encoding.UTF8);
    }

    [MenuItem("Assets/复制ViewID")]
    private static void CopyViewID()
    {
        Object obj = Selection.activeObject;
        GetClientPath();
        string uiPath = $"{all_view_path}/{obj.name}.cs";
        int viewId = GetViewId(uiPath);
        GUIUtility.systemCopyBuffer = viewId.ToString();
        UnityEngine.Debug.Log(viewId);
    }

    private static int GetViewId(string uiPath)
    {
        int viewId = 0;
        if (File.Exists(uiPath))
        {
            string content = File.ReadAllText(uiPath);
            string findStr = "public static int viewId = ";
            int index = content.IndexOf(findStr);
            if (index > -1)
            {
                content = content.Substring(index + findStr.Length);
                int.TryParse(content.Substring(0, content.IndexOf(';')), out viewId);
            }
        }
        return viewId;
    }

    private static void CheckGetBaseViewId()
    {
        SVNCommand(UPDATE, all_view_path);
        //Dictionary<int, string> uipaths = new Dictionary<int, string>();
        //string[] files = Directory.GetFiles(uiSavePath, "*.cs", SearchOption.AllDirectories);
        //for (int i = 0; i < files.Length; i++)
        //{
        //    if (File.Exists(files[i]))
        //    {
        //        string content = File.ReadAllText(files[i]);
        //        string findStr = "public static int viewId = ";
        //        int index = content.IndexOf(findStr);
        //        if (index > -1)
        //        {
        //            content = content.Substring(index + findStr.Length);
        //            int.TryParse(content.Substring(0, content.IndexOf(';')), out int viewId);
        //            uipaths[viewId] = files[i];
        //        }
        //    }
        //}

        //List<int> keys = new List<int>(uipaths.Keys);
        //keys.Sort((int a, int b)=> { return a.CompareTo(b); });
        //for (int i = 0; i < keys.Count; i++)
        //{
        //    string filePath = uipaths[keys[i]];
        //    if (File.Exists(filePath))
        //    {
        //        string content = File.ReadAllText(filePath);
        //        string findStr = "public static int viewId = ";
        //        int index = content.IndexOf(findStr);
        //        if (index > -1)
        //        {
        //            string viewText = content.Substring(index + findStr.Length);
        //            content = content.Replace(viewText.Substring(0, viewText.IndexOf(';')), (++baseViewId).ToString());
        //            File.WriteAllText(filePath, content, System.Text.UTF8Encoding.UTF8);
        //        }
        //    }
        //}
        //return;
        //if (File.Exists(allviewDefsPath))
        //{
        //    baseViewId = 0;
        //    string content = File.ReadAllText(allviewDefsPath, System.Text.UTF8Encoding.UTF8);
        //    string[] te = content.Split('\n');
        //    for (int i = 0; i < te.Length; i++)
        //    {
        //        string[] te2 = te[i].Split('=');
        //        int.TryParse(te2[0].Trim(' '), out int v);
        //        if (v > baseViewId)
        //        {
        //            baseViewId = v;
        //        }
        //    }
        //}

        HashSet<int> hashset = new HashSet<int>();
        baseViewId = 0;
        string[] files = Directory.GetFiles(all_view_path, "*.cs", SearchOption.AllDirectories);
        for (int i = 0; i < files.Length; i++)
        {
            int viewId = GetViewId(files[i]);
            if (viewId == 0)
            {
                UnityEngine.Debug.LogError("未知UI脚本资源 viewId = 0：" + files[i]);
            }
            else
            {
                if (!hashset.Contains(viewId))
                {
                    hashset.Add(viewId);
                }
                else
                {
                    UnityEngine.Debug.LogError("viewId有重复：" + viewId + "  ," + files[i]);
                }
                if (viewId > baseViewId)
                {
                    baseViewId = viewId;
                }
            }
        }
    }

    private const string COMMIT = "commit";
    private const string UPDATE = "update";
    /// <summary>  
    /// 创建一个SVN的cmd命令  
    /// </summary>  
    /// <param name="command">命令(可在help里边查看)</param>  
    /// <param name="path">命令激活路径</param>  
    public static void SVNCommand(string command, string path)
    {
        //closeonend 2 表示假设提交没错，会自动关闭提交界面返回原工程，详细描述可在  
        //TortoiseSVN/help/TortoiseSVN/Automating TortoiseSVN里查看  
        string c = "";
        if (COMMIT == command)
        {
            c = "/c tortoiseproc.exe /command:{0} /path:\"{1}\" /logmsg:\"更新自动化view，需要点击ok进行提交\" /closeonend 2";
        }
        else
        {
            c = "/c tortoiseproc.exe /command:{0} /path:\"{1}\" /logmsg:\"更新自动化view\" /closeonend 2";
        }
        c = string.Format(c, command, path);
        ProcessStartInfo info = new ProcessStartInfo("cmd.exe", c);
        info.WindowStyle = ProcessWindowStyle.Hidden;
        Process.Start(info).WaitForExit();
    }



    [MenuItem("Tools/移动被多个common_tex")]
    private static void MoveToCommonTex()
    {
        string path = "F:/trunk/main/client/GameBase/common_tex.log";
        string find_root = "F:/trunk/main/arts_projects/ui/Assets/ui/asset/myuiimage/freesize";
        string target_root = "F:/trunk/main/arts_projects/ui/Assets/ui/asset/myuiimage/common_tex";
        string[] files = Directory.GetFiles(find_root, "*", SearchOption.AllDirectories);
        Dictionary<string, string> tex_path_dic = new Dictionary<string, string>();
        foreach (string f in files)
        {
            if ((f.Contains(".png") || f.Contains(".jpg")) && !f.Contains(".meta"))
            {
                string ex_f = Path.GetFileNameWithoutExtension(f);
                tex_path_dic[ex_f] = f.Replace('\\', '/');
            }
        }
        string content = File.ReadAllText(path, System.Text.UTF8Encoding.UTF8);
        StringReader reader = new StringReader(content);
        while (reader.Peek() > 1)
        {
            string line = reader.ReadLine();
            string file_name = line.Trim().Trim(' ').Trim('\r');
            file_name = file_name.Split(',')[0];
            if (tex_path_dic.TryGetValue(file_name, out string file_path))
            {
                string source_file = Path.GetFileName(file_path);
                File.Move(file_path, target_root + "/" + source_file);
                File.Move(file_path + ".meta", target_root + "/" + source_file + ".meta");

                tex_path_dic.Remove(file_name);
            }
        }

        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
    }
}
