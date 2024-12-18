using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

public class UIEditorPrefabHelper : EditorWindow
{
    [MenuItem("Tools/UI图集整理工具")]
    public static void OpenUIEditorPrefabHelper()
    {
        EditorWindow.GetWindow<UIEditorPrefabHelper>("UI图集整理工具");
    }

    [MenuItem("Tools/导出所有ui")]
    public static void BuildSelect()
    {
        if (!EditorPathUtils.CheckPathSettings()) return;
        string rootPath = "Assets/ui/prefab/myuipanel";
        string[] resFiles = AssetDatabase.FindAssets("t:Prefab", new string[] { rootPath });
        List<string> paths = new List<string>();
        foreach (var file in resFiles)
        {
            string path = AssetDatabase.GUIDToAssetPath(file);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            MyComponent comp = go.GetComponent<MyComponent>();
            if (comp != null)
            {
                paths.Add(path);
            }
        }
        AssetbundleBuilder._ExportSelects(paths, false);
    }

    //[MenuItem("Tools/导出所有前缀atlas的图集")]
    public static void BuildAllAtlas()
    {
        if (!EditorPathUtils.CheckPathSettings()) return;
        string rootPath = "Assets/ui/prefab/MyPacker";
        string[] resFiles = AssetDatabase.FindAssets("t:Prefab", new string[] { rootPath });
        List<string> paths = new List<string>();
        foreach (var file in resFiles)
        {
            string path = AssetDatabase.GUIDToAssetPath(file);
            if (path.StartsWith("atlas_"))
            {
                paths.Add(path);
            }
        }
        AssetbundleBuilder._ExportSelects(paths, false);
    }

    private class UIPrefabInfo
    {
        public Object prefab = null;
        public GameObject curr_gameobj = null;
        public Dictionary<Component, string> comp_package_path_dic = new Dictionary<Component, string>();
        public Dictionary<Object, List<Component>> package_to_comps_dic = new Dictionary<Object, List<Component>>();

        public bool need_refsh_group = true;
        private UIEditorPrefabHelper prefabHelper2;
        public UIPrefabInfo(UIEditorPrefabHelper uIEditorPrefab, Object obj)
        {
            prefabHelper2 = uIEditorPrefab;
            prefab = obj;
            curr_gameobj = PrefabUtility.InstantiatePrefab(obj) as GameObject;
            var mySpriteImages = curr_gameobj.GetComponentsInChildren<MySpriteImage>(true);
            var myImages = curr_gameobj.GetComponentsInChildren<MyImage>(true);
            foreach (var sp in mySpriteImages)
            {
                if (!string.IsNullOrEmpty(sp.PackerName) && sp.mainTexture != null)
                {
                    string path = "Assets/ui/asset/sprites/" + sp.mainTexture.name + "/" + sp.PackerSpriteName + ".png";
                    if (prefabHelper2.sprites_dic.ContainsKey(path))
                    {
                        comp_package_path_dic[sp] = path;
                    }
                    if (!uIEditorPrefab.image_to_component_dic.TryGetValue(path, out var list))
                    {
                        list = new List<ImageToComponentInfo>();
                        uIEditorPrefab.image_to_component_dic.Add(path, list);
                    }
                    list.Add(new ImageToComponentInfo() { prefab = curr_gameobj, image = sp });
                }
            }
            foreach (var sp in myImages)
            {
                if (!string.IsNullOrEmpty(sp.PackerName) && sp.mainTexture != null)
                {
                    string path = "Assets/ui/asset/sprites/" + sp.mainTexture.name + "/" + sp.PackerSpriteName + ".png";
                    if (prefabHelper2.sprites_dic.ContainsKey(path))
                    {
                        comp_package_path_dic[sp] = path;
                    }
                    if (!uIEditorPrefab.image_to_component_dic.TryGetValue(path, out var list))
                    {
                        list = new List<ImageToComponentInfo>();
                        uIEditorPrefab.image_to_component_dic.Add(path, list);
                    }
                    list.Add(new ImageToComponentInfo() { prefab = curr_gameobj, image = sp });
                }
            }

            RefshPackageGroup();
        }

        public void RefshPackageGroup()
        {
            if (need_refsh_group)
            {
                need_refsh_group = false;
                package_to_comps_dic.Clear();
                foreach (var kv in comp_package_path_dic)
                {
                    string dir = Path.GetDirectoryName(prefabHelper2.sprites_dic[kv.Value]).Replace('\\', '/');
                    string pack_name = Path.GetFileName(dir);
                    Object package = AssetDatabase.LoadAssetAtPath<Object>(dir.Replace("asset/sprites", "prefab/MyPacker") + "/" + pack_name + ".prefab");
                    if (!package_to_comps_dic.TryGetValue(package, out var list))
                    {
                        list = new List<Component>();
                        package_to_comps_dic.Add(package, list);
                    }
                    list.Add(kv.Key);
                }
            }
        }

        public void ChangeReplacePrefab()
        {
            bool isHave = false;
            foreach (var kv in comp_package_path_dic)
            {
                if (kv.Value != prefabHelper2.sprites_dic[kv.Value])
                {
                    isHave = true;
                    var obj = kv.Key;
                    string sprite_name = Path.GetFileNameWithoutExtension(prefabHelper2.sprites_dic[kv.Value]);
                    string dir = Path.GetDirectoryName(prefabHelper2.sprites_dic[kv.Value]).Replace('\\', '/');
                    string pack_name = Path.GetFileName(dir);
                    Object package_obj = AssetDatabase.LoadAssetAtPath<Object>("Assets/ui/prefab/MyPacker/" + pack_name + "/" + pack_name + ".prefab");
                    MySpritePacker spritePacker = (package_obj as GameObject).GetComponent<MySpritePacker>();
                    if (obj is MyImage)
                    {
                        MyImage myImage = obj as MyImage;
                        myImage.Editor_SetPacker_sprite(spritePacker, sprite_name);
                    }
                    else if (obj is MySpriteImage)
                    {
                        MySpriteImage myImage = obj as MySpriteImage;
                        myImage.Editor_SetPacker_sprite(spritePacker, sprite_name);
                    }
                }
            }
            if (isHave)
            {
                PrefabUtility.ApplyPrefabInstance(curr_gameobj, InteractionMode.AutomatedAction);
            }
        }

        public void Finish()
        {
            if (curr_gameobj != null)
            {
                GameObject.DestroyImmediate(curr_gameobj);
            }
            //PrefabUtility.UnloadPrefabContents(curr_gameobj);
            curr_gameobj = null;
        }
    }

    public class ImageToComponentInfo
    {
        public Object prefab;
        public Component image;
    }

    private Vector2 scroll1;
    private Vector2 scroll2;
    public List<string> delete_list = new List<string>();
    public Dictionary<string, string> sprites_dic = new Dictionary<string, string>();
    public Dictionary<string, Object> sprite_images_dic = new Dictionary<string, Object>();
    public Dictionary<string, List<string[]>> package_image_paths = new Dictionary<string, List<string[]>>(); //[图集名]=List<[原路径，新路径]>
    private Dictionary<Object, List<Object>> all_dir_objs = new Dictionary<Object, List<Object>>();
    private Dictionary<Object, UIPrefabInfo> all_uiprefab_info_dic = new Dictionary<Object, UIPrefabInfo>();
    public Dictionary<string, List<ImageToComponentInfo>> image_to_component_dic = new Dictionary<string, List<ImageToComponentInfo>>();
    private Object curr_select_dir, curr_select_ui;
    private List<Component> select_all_comps = new List<Component>();
    private List<Object> select_ui_packers = new List<Object>();
    private UIPrefabInfo curr_prefab_info;
    private string[] package_opts = new string[0];

    public void RefshPackageOpts()
    {
        List<string> packages = new List<string>();
        packages.Add("重定向到指定图集");
        package_image_paths.Clear();
        foreach (var kv in sprites_dic)
        {
            string package_name = Path.GetFileName(Path.GetDirectoryName(kv.Value));
            if (!package_image_paths.TryGetValue(package_name, out var list))
            {
                list = new List<string[]>();
                package_image_paths.Add(package_name, list);
                packages.Add(package_name);
            }
            list.Add(new string[] { kv.Key, kv.Value });
        }
        package_opts = packages.ToArray();
    }

    public void ChangeReplacePrefab()
    {
        foreach (var v in all_uiprefab_info_dic.Values)
        {
            v.ChangeReplacePrefab();
        }

        this.Close();
    }

    public void RestAllUIPrefab()
    {
        foreach (var v in all_uiprefab_info_dic.Values)
        {
            v.need_refsh_group = true;
        }
    }

    private void OnDisable()
    {
        foreach (var kv in all_uiprefab_info_dic)
        {
            kv.Value.Finish();
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("重新加载", GUILayout.Width(200)))
        {
            sprites_dic = new Dictionary<string, string>();
            foreach (var file in AssetDatabase.FindAssets("t:Texture", new string[] { "Assets/ui/asset/sprites" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(file);
                sprites_dic[path] = path;
                sprite_images_dic[path] = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
            RefshPackageOpts();

            var all_par = AssetDatabase.FindAssets("t:Prefab", new string[] { "Assets/ui/prefab/MyPacker" });
            for (int i = 0; i < all_par.Length; i++)
            {
                all_par[i] = AssetDatabase.GUIDToAssetPath(all_par[i]);
            }
            all_dir_objs.Clear();
            string rootPath = "Assets/ui/prefab/myuipanel";
            string[] resFiles = AssetDatabase.FindAssets("t:Prefab", new string[] { rootPath });
            foreach (var file in resFiles)
            {
                string path = AssetDatabase.GUIDToAssetPath(file);
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                MyComponent comp = go.GetComponent<MyComponent>();
                if (comp != null)
                {
                    string dir = Path.GetDirectoryName(path).Replace('\\', '/');
                    var dirobj = AssetDatabase.LoadAssetAtPath<DefaultAsset>(dir);
                    if (!all_dir_objs.TryGetValue(dirobj, out var list))
                    {
                        list = new List<Object>();
                        all_dir_objs.Add(dirobj, list);
                    }
                    list.Add(go);
                    all_uiprefab_info_dic[go] = new UIPrefabInfo(this, go);
                }
            }
        }
        if (sprites_dic.Count > 0)
        {
            if (GUILayout.Button("打开图集管理", GUILayout.Width(200)))
            {
                EditorWindow.GetWindow<AtlasEditorWindow>("图集管理").uIEditorPrefab = this;
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();

        scroll1 = EditorGUILayout.BeginScrollView(scroll1, GUILayout.Width(250));
        foreach (var kv in all_dir_objs)
        {
            EditorGUILayout.BeginHorizontal();
            if (curr_select_dir == kv.Key)
            {
                if (!EditorGUILayout.ToggleLeft("", true, GUILayout.Width(20)))
                {
                    curr_select_dir = null;
                    curr_select_ui = null;
                    curr_prefab_info = null;
                    select_all_comps.Clear();
                    select_ui_packers.Clear();
                }
            }
            else
            {
                if (EditorGUILayout.ToggleLeft("", false, GUILayout.Width(20)))
                {
                    curr_select_ui = null;
                    curr_prefab_info = null;
                    curr_select_dir = kv.Key;
                    select_all_comps.Clear();
                    select_ui_packers.Clear();
                }
            }
            EditorGUILayout.ObjectField(kv.Key, typeof(Object), false, GUILayout.Width(200));
            EditorGUILayout.EndHorizontal();
            if (curr_select_dir == kv.Key)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("", GUILayout.Width(20));
                EditorGUILayout.BeginVertical();
                foreach (var v in kv.Value)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (curr_select_ui == v)
                    {
                        if (!EditorGUILayout.ToggleLeft("", true, GUILayout.Width(20)))
                        {
                            curr_select_ui = null;
                            curr_prefab_info = null;
                            select_all_comps.Clear();
                            select_ui_packers.Clear();
                        }
                    }
                    else
                    {
                        if (EditorGUILayout.ToggleLeft("", false, GUILayout.Width(20)))
                        {
                            curr_select_ui = v;
                            curr_prefab_info = all_uiprefab_info_dic[v];
                            curr_prefab_info.need_refsh_group = true;
                            select_all_comps.Clear();
                            select_ui_packers.Clear();
                        }
                    }
                    EditorGUILayout.ObjectField(v, typeof(Object), false, GUILayout.Width(200));
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
        }

        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        //图集列表
        if (curr_select_ui != null && curr_prefab_info != null)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("以下是使用的所有图集: ");
            curr_prefab_info.RefshPackageGroup();
            scroll2 = EditorGUILayout.BeginScrollView(scroll2);
            if (curr_prefab_info.package_to_comps_dic != null && curr_prefab_info.package_to_comps_dic.Count > 0)
            {
                foreach (var kv in curr_prefab_info.package_to_comps_dic)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (select_ui_packers.Contains(kv.Key))
                    {
                        if (!EditorGUILayout.ToggleLeft("", true, GUILayout.Width(20)))
                        {
                            if (select_ui_packers.Contains(kv.Key)) select_ui_packers.Remove(kv.Key);
                        }
                    }
                    else
                    {
                        if (EditorGUILayout.ToggleLeft("", false, GUILayout.Width(20)))
                        {
                            if (!select_ui_packers.Contains(kv.Key))
                            {
                                select_ui_packers.Add(kv.Key);
                            }
                        }
                    }
                    EditorGUILayout.ObjectField(kv.Key, typeof(Object), false, GUILayout.Width(200));
                    EditorGUILayout.LabelField(kv.Value.Count.ToString(), GUILayout.Width(20));
                    if (GUILayout.Button("全选/反选", GUILayout.Width(200)))
                    {
                        if (kv.Value.Count > 0)
                        {
                            if (select_all_comps.Contains(kv.Value[0]))
                            {
                                foreach (var obj in kv.Value)
                                {
                                    if (select_all_comps.Contains(obj))
                                    {
                                        select_all_comps.Remove(obj);
                                    }
                                }
                            }
                            else
                            {
                                foreach (var obj in kv.Value)
                                {
                                    if (!select_all_comps.Contains(obj))
                                    {
                                        select_all_comps.Add(obj);
                                    }
                                }
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    if (select_ui_packers.Contains(kv.Key))
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("", GUILayout.Width(20));
                        EditorGUILayout.BeginVertical();
                        foreach (var obj in kv.Value)
                        {
                            EditorGUILayout.BeginHorizontal();
                            if (select_all_comps.Contains(obj))
                            {
                                if (!EditorGUILayout.ToggleLeft("", true, GUILayout.Width(20)))
                                {
                                    if (select_all_comps.Contains(obj)) select_all_comps.Remove(obj);
                                }
                            }
                            else
                            {
                                if (EditorGUILayout.ToggleLeft("", false, GUILayout.Width(20)))
                                {
                                    if (!select_all_comps.Contains(obj))
                                    {
                                        select_all_comps.Add(obj);
                                    }
                                }
                            }
                            EditorGUILayout.ObjectField(obj, typeof(Object), true, GUILayout.Width(200));
                            sprite_images_dic.TryGetValue(curr_prefab_info.comp_package_path_dic[obj], out var image);
                            EditorGUILayout.ObjectField(image, typeof(Object), false, GUILayout.Width(200));
                            EditorGUILayout.EndHorizontal();
                        }
                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndHorizontal();
                    }
                }
            }

            EditorGUILayout.LabelField("操作选项: " + select_all_comps.Count, GUILayout.Width(200));
            foreach (var obj in select_all_comps)
            {
                EditorGUILayout.ObjectField(obj, typeof(Object), true, GUILayout.Width(200));
            }
            EditorGUILayout.BeginHorizontal();

            if (select_all_comps.Count > 0)
            {
                int atlas_index = EditorGUILayout.Popup(0, package_opts, GUILayout.Width(200));
                if (atlas_index != 0)
                {
                    string atlas_name = package_opts[atlas_index];
                    foreach (var obj in select_all_comps) //把当前选中的Image所用的图集和路径重新定向
                    {
                        string lost_path = curr_prefab_info.comp_package_path_dic[obj];
                        string file_name = Path.GetFileName(lost_path);
                        sprites_dic[lost_path] = "Assets/ui/asset/sprites/" + atlas_name + "/" + file_name;
                    }
                    select_all_comps.Clear();

                    curr_prefab_info.need_refsh_group = true;

                    RefshPackageOpts();
                }

                //if (GUILayout.Button("清除选中组件的图集", GUILayout.Width(200)))
                //{
                //    if (select_all_comps.Count > 0)
                //    {
                //        foreach (var obj in select_all_comps)
                //        {
                //            if (obj is MyImage)
                //            {
                //                SetField(obj, "_sp_packer", null);
                //                SetField(obj, "_packerName", "");
                //                SetField(obj, "_sp_name", "");
                //                SetField(obj, "_spriterName", "");
                //                SetField(obj as Image, "sprite", null);
                //                SetField(obj, "mainTexture", null);
                //                (obj as MyImage).sprite = null;
                //            }
                //            else if (obj is MySpriteImage)
                //            {
                //                SetField(obj, "_sp_packer", null);
                //                SetField(obj as MySpriteImageBase, "_packerName", "");
                //                SetField(obj, "_sp_name", "");
                //                SetField(obj, "_spriterName", "");
                //                SetField(obj, "_textureName", "");
                //                SetField(obj, "_tex", null);
                //                SetField(obj, "_sprite", null);
                //                SetField(obj, "mainTexture", null);
                //            }
                //        }
                //        curr_prefab_info.Init();
                //        select_all_comps.Clear();
                //    }
                //}
                //if (GUILayout.Button("保存到Prefab", GUILayout.Width(200)))
                //{
                //    PrefabUtility.SaveAsPrefabAsset(curr_prefab_info.curr_gameobj, AssetDatabase.GetAssetPath(curr_select_ui));
                //    AssetDatabase.SaveAssets();
                //    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                //}
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
        else if (curr_select_dir != null)
        {
            //if (all_dir_packges.TryGetValue(curr_select_dir, out var list))
            //{
            //    EditorGUILayout.BeginVertical();
            //    EditorGUILayout.Space();
            //    EditorGUILayout.Space();
            //    EditorGUILayout.LabelField("以下是此文件夹使用的所有图集: ");
            //    scroll2 = EditorGUILayout.BeginScrollView(scroll2);
            //    foreach (var obj in list)
            //    {
            //        EditorGUILayout.ObjectField(obj, typeof(Object), false, GUILayout.Width(200));
            //    }
            //    EditorGUILayout.EndScrollView();
            //    EditorGUILayout.EndVertical();
            //}
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();


    }
}

public class AtlasEditorWindow : EditorWindow
{
    public UIEditorPrefabHelper uIEditorPrefab = null;

    private Vector2 scroll1;
    private string curr_package = "";
    private string rename_package = "";
    private int show_type = 0;
    private Dictionary<string, Vector2> package_size_dic = new Dictionary<string, Vector2>();
    private List<string> select_image_paths = new List<string>();
    private List<string> select_show_ui = new List<string>();
    private Dictionary<string, string> rename_dic = new Dictionary<string, string>();
    private Dictionary<Object, Vector2> image_size_dic = new Dictionary<Object, Vector2>();
    private Dictionary<string, List<Object>> package_ui_list = new Dictionary<string, List<Object>>();
    private bool isinit = true;
    private void OnDisable()
    {
        uIEditorPrefab.RestAllUIPrefab();
    }

    private void OnEnable()
    {
        isinit = true;
    }

    private void OnGUI()
    {
        if (uIEditorPrefab == null)
        {
            return;
        }
        if (isinit)
        {
            isinit = false;
            package_ui_list.Clear();
            foreach (var kv in uIEditorPrefab.image_to_component_dic)
            {
                var list = kv.Value;
                int count = list.Count;
                for (int i = 0; i < count - 1; i++)
                {
                    for (int j = 0; j < count - 1 - i; j++)
                    {
                        int a = list[j].prefab.GetInstanceID();
                        int b = list[j + 1].prefab.GetInstanceID();

                        if (a > b)
                        {
                            var temp = list[j];
                            list[j] = list[j + 1];
                            list[j + 1] = temp;
                        }
                    }
                }
                string package_name = Path.GetFileName(Path.GetDirectoryName(kv.Key));
                if (!package_ui_list.TryGetValue(package_name, out var ui_list))
                {
                    ui_list = new List<Object>();
                    package_ui_list.Add(package_name, ui_list);
                }
                foreach (var v in kv.Value)
                {
                    if (!ui_list.Contains(v.prefab)) ui_list.Add(v.prefab);
                }
            }
        }

        EditorGUILayout.BeginHorizontal();
        
        if (show_type == 1)
        {
            if (GUILayout.Button("显示图片", GUILayout.Width(200)))
            {
                show_type = 0;
            }
        }
        else if (show_type == 0)
        {
            if (GUILayout.Button("显示UI", GUILayout.Width(200)))
            {
                show_type = 1;
            }
        }

        if (GUILayout.Button("一键整理图集", GUILayout.Width(200)))
        {
            if (EditorUtility.DisplayDialog("tips", "确定应用所有变化重新生成图集？", "ok", "cancel"))
            {

                uIEditorPrefab.RefshPackageOpts();

                List<string> need_rebuild_packages = new List<string>();

                foreach (var kv in uIEditorPrefab.package_image_paths)
                {
                    string pacageName = kv.Key;
                    if (!Directory.Exists(Application.dataPath.Replace("Assets", "Assets/ui/asset/sprites/" + pacageName)))
                    {
                        AssetDatabase.CreateFolder("Assets/ui/asset/sprites", pacageName);
                    }
                    foreach (var paths in kv.Value)
                    {
                        if (paths[0] != paths[1]) 
                        {
                            if (!need_rebuild_packages.Contains(pacageName))
                            {
                                need_rebuild_packages.Add(pacageName);
                            }
                            break;
                        }
                    }
                }

                foreach (var kv in uIEditorPrefab.sprites_dic)
                {
                    if (kv.Key != kv.Value)
                    {
                        AssetDatabase.MoveAsset(kv.Key, kv.Value);
                    }
                }

                foreach (var path in uIEditorPrefab.delete_list)
                {
                    AssetDatabase.DeleteAsset(path);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                ConsoleUtils.CleanConsole();
                foreach (var kv in uIEditorPrefab.package_image_paths)
                {
                    if (kv.Value.Count > 0 && need_rebuild_packages.Contains(kv.Key))
                    {
                        Builder_All.GenPacker(Path.GetDirectoryName(kv.Value[0][1]).Replace('\\', '/').ToLower(), true);
                    }
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                uIEditorPrefab.ChangeReplacePrefab();
                this.Close();
            }
        }

        if (select_image_paths.Count > 0)
        {
            if (GUILayout.Button("新建图集(复制)", GUILayout.Width(200)))
            {
                foreach (var lost_path in select_image_paths)
                {
                    string pacageName = "new_packager_" + uIEditorPrefab.package_image_paths.Count;
                    string file_name = "Assets/ui/asset/sprites/" + pacageName + "/" + Path.GetFileName(lost_path);
                    //if (!Directory.Exists(Application.dataPath.Replace("Assets", "Assets/ui/asset/sprites/" + pacageName)))
                    //{
                    //    AssetDatabase.CreateFolder("Assets/ui/asset/sprites", pacageName);
                    //}
                    //AssetDatabase.CopyAsset(lost_path, file_name);
                    uIEditorPrefab.sprites_dic[file_name] = file_name;
                    uIEditorPrefab.sprite_images_dic[file_name] = AssetDatabase.LoadAssetAtPath<Texture2D>(lost_path);
                }
                uIEditorPrefab.RefshPackageOpts();
                select_image_paths.Clear();
                curr_package = null;
                return;
            }
            if (GUILayout.Button("新建图集(移动)", GUILayout.Width(200)))
            {
                foreach (var lost_path in select_image_paths)
                {
                    string pacageName = "new_packager_" + uIEditorPrefab.package_image_paths.Count;
                    string file_name = "Assets/ui/asset/sprites/" + pacageName + "/" + Path.GetFileName(lost_path);
                    //if (!Directory.Exists(Application.dataPath.Replace("Assets", "Assets/ui/asset/sprites/" + pacageName)))
                    //{
                    //    AssetDatabase.CreateFolder("Assets/ui/asset/sprites", pacageName);
                    //}
                    uIEditorPrefab.sprites_dic[lost_path] = file_name;
                }
                uIEditorPrefab.RefshPackageOpts();
                select_image_paths.Clear();
                curr_package = null;
                return;
            }
            if (GUILayout.Button("删除", GUILayout.Width(200)))
            {
                foreach (var lost_path in select_image_paths)
                {
                    uIEditorPrefab.sprites_dic.Remove(lost_path);
                    if(!uIEditorPrefab.delete_list.Contains(lost_path)) uIEditorPrefab.delete_list.Add(lost_path);
                }
                uIEditorPrefab.RefshPackageOpts();
                select_image_paths.Clear();
                curr_package = null;
                return;
            }
        }
        EditorGUILayout.EndHorizontal();

        scroll1 = EditorGUILayout.BeginScrollView(scroll1);

        foreach (var image_kv in uIEditorPrefab.package_image_paths)
        {
            string package_name = image_kv.Key;
            List<string[]> image_paths = image_kv.Value;
            EditorGUILayout.BeginHorizontal();
            if (curr_package == package_name)
            {
                if (!EditorGUILayout.ToggleLeft("", true, GUILayout.Width(20)))
                {
                    curr_package = null;
                    rename_dic.Clear();
                    select_show_ui.Clear();
                }
            }
            else
            {
                if (EditorGUILayout.ToggleLeft("", false, GUILayout.Width(20)))
                {
                    select_show_ui.Clear();
                    rename_dic.Clear();
                    curr_package = package_name;
                    rename_package = package_name;
                    foreach (var p in image_paths)
                    {
                        rename_dic[p[0]] = p[1];
                    }
                }
            }
            if (curr_package == package_name)
            {
                rename_package = EditorGUILayout.TextField(rename_package, GUILayout.Width(300));
                if (rename_package != package_name)
                {
                    if (GUILayout.Button("替换新名字", GUILayout.Width(100)))
                    {
                        List<string> keys = new List<string>(uIEditorPrefab.sprites_dic.Keys);
                        for (int i = 0; i < keys.Count; i++)
                        {
                            string v = uIEditorPrefab.sprites_dic[keys[i]];
                            if (v.Contains("/" + package_name + "/"))
                            {
                                uIEditorPrefab.sprites_dic[keys[i]] = uIEditorPrefab.sprites_dic[keys[i]].Replace("/" + package_name + "/", "/" + rename_package + "/");
                            }
                        }
                        rename_package = package_name;
                        uIEditorPrefab.RefshPackageOpts();
                        select_image_paths.Clear();
                        select_show_ui.Clear();
                        curr_package = null;
                        return;
                    }
                }
            }
            else
            {
                EditorGUILayout.TextField(package_name, GUILayout.Width(300));
            }

            if (!package_size_dic.TryGetValue(package_name, out var size))
            {
                Texture2D t = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/ui/prefab/MyPacker/" + package_name + "/" + package_name + ".png");
                if (t != null)
                {
                    size = new Vector2(t.width, t.height);
                }
                package_size_dic.Add(package_name, size);
            }

            if (size.x > 1024 || size.y > 1024)
            {
                GUI.color = Color.red;
                EditorGUILayout.LabelField(size.x + "*" + size.y, GUILayout.Width(200));
                GUI.color = Color.white;
            }
            else
            {
                EditorGUILayout.LabelField(size.x + "*" + size.y, GUILayout.Width(200));
            }

            EditorGUILayout.LabelField(image_paths.Count.ToString(), GUILayout.Width(100));

            if (select_image_paths.Count > 0 && curr_package != package_name)
            {
                if (GUILayout.Button("复制到", GUILayout.Width(100)))
                {
                    List<string> new_paths = new List<string>(uIEditorPrefab.sprites_dic.Values);
                    foreach (var lost_path in select_image_paths)
                    {
                        string file_name = "Assets/ui/asset/sprites/" + package_name + "/" + Path.GetFileName(lost_path);
                        if (uIEditorPrefab.sprites_dic.ContainsKey(file_name) || new_paths.Contains(file_name))
                        {
                            file_name = "Assets/ui/asset/sprites/" + package_name + "/icon_" + image_paths.Count + "_" + Path.GetFileName(lost_path);
                        }
                        uIEditorPrefab.sprites_dic[file_name] = file_name;
                        uIEditorPrefab.sprite_images_dic[file_name] = AssetDatabase.LoadAssetAtPath<Texture2D>(lost_path);
                    }
                    uIEditorPrefab.RefshPackageOpts();
                    select_image_paths.Clear();
                    curr_package = null;
                    return;
                }
                if (GUILayout.Button("移动到", GUILayout.Width(100)))
                {
                    List<string> new_paths = new List<string>(uIEditorPrefab.sprites_dic.Values);
                    foreach (var lost_path in select_image_paths)
                    {
                        string file_name = "Assets/ui/asset/sprites/" + package_name + "/" + Path.GetFileName(lost_path);
                        if (uIEditorPrefab.sprites_dic.ContainsKey(file_name) || new_paths.Contains(file_name))
                        {
                            file_name = "Assets/ui/asset/sprites/" + package_name + "/icon_" + image_paths.Count + "_" + Path.GetFileName(lost_path);
                        }
                        uIEditorPrefab.sprites_dic[lost_path] = file_name;
                    }
                    uIEditorPrefab.RefshPackageOpts();
                    select_image_paths.Clear();
                    curr_package = null;
                    return;
                }
            }

            EditorGUILayout.EndHorizontal();

            if (select_image_paths.Count > 0)
            {
                EditorGUILayout.LabelField("------------------------------------------------------------------------------------------------------------------------------------");
            }

            if (curr_package == package_name)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("", GUILayout.Width(20));
                EditorGUILayout.BeginVertical();
                if (show_type == 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("全选/反选", GUILayout.Width(200)))
                    {
                        if (select_image_paths.Count > 0) { select_image_paths.Clear(); }
                        else
                        {
                            select_image_paths.Clear();
                            foreach (var image_path in image_paths)
                            {
                                select_image_paths.Add(image_path[0]);
                            }
                        }
                    }
                    if (GUILayout.Button("展开/缩进", GUILayout.Width(200)))
                    {
                        if (select_show_ui.Count > 0) { select_show_ui.Clear(); }
                        else
                        {
                            select_show_ui.Clear();
                            foreach (var image_path in image_paths)
                            {
                                select_show_ui.Add(image_path[0]);
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    for (int i = 0; i < image_paths.Count; i++)
                    {
                        var image_path = image_paths[i];

                        EditorGUILayout.BeginHorizontal();

                        if (select_image_paths.Contains(image_path[0]))
                        {
                            if (!EditorGUILayout.ToggleLeft("", true, GUILayout.Width(20)))
                            {
                                if (select_image_paths.Contains(image_path[0])) select_image_paths.Remove(image_path[0]);
                            }
                        }
                        else
                        {
                            if (EditorGUILayout.ToggleLeft("", false, GUILayout.Width(20)))
                            {
                                if (!select_image_paths.Contains(image_path[0]))
                                {
                                    select_image_paths.Add(image_path[0]);
                                }
                            }
                        }
                        EditorGUILayout.ObjectField(uIEditorPrefab.sprite_images_dic[image_path[0]], typeof(Object), false, GUILayout.Width(200));
                        if (select_show_ui.Contains(image_path[0]))
                        {
                            if (!EditorGUILayout.ToggleLeft("", true, GUILayout.Width(20)))
                            {
                                if (select_show_ui.Contains(image_path[0])) select_show_ui.Remove(image_path[0]);
                            }
                        }
                        else
                        {
                            if (EditorGUILayout.ToggleLeft("", false, GUILayout.Width(20)))
                            {
                                if (!select_show_ui.Contains(image_path[0]))
                                {
                                    select_show_ui.Add(image_path[0]);
                                }
                            }
                        }
                        if (!image_size_dic.TryGetValue(uIEditorPrefab.sprite_images_dic[image_path[0]], out var size2))
                        {
                            Texture2D t = uIEditorPrefab.sprite_images_dic[image_path[0]] as Texture2D;
                            if (t != null)
                            {
                                size2 = new Vector2(t.width, t.height);
                            }
                            image_size_dic.Add(uIEditorPrefab.sprite_images_dic[image_path[0]], size2);
                        }
                        EditorGUILayout.LabelField(size2.x + "*" + size2.y, GUILayout.Width(80));
                        rename_dic[image_path[0]] = EditorGUILayout.TextField(rename_dic[image_path[0]], GUILayout.Width(400));
                        if (rename_dic[image_path[0]] != uIEditorPrefab.sprites_dic[image_path[0]])
                        {
                            if (GUILayout.Button("替换重命名", GUILayout.Width(200)))
                            {
                                uIEditorPrefab.sprites_dic[image_path[0]] = rename_dic[image_path[0]];
                                uIEditorPrefab.RefshPackageOpts();
                            }
                            return;
                        }
                        EditorGUILayout.EndHorizontal();
                        if (select_show_ui.Contains(image_path[0]))
                        {
                            if (uIEditorPrefab.image_to_component_dic.TryGetValue(image_path[0], out var infos))
                            {
                                foreach(var info in infos)
                                {
                                    EditorGUILayout.BeginHorizontal();
                                    EditorGUILayout.LabelField("", GUILayout.Width(20));
                                    EditorGUILayout.ObjectField(info.prefab, typeof(Object), true, GUILayout.Width(240));
                                    EditorGUILayout.ObjectField(info.image, typeof(Object), true);
                                    EditorGUILayout.EndHorizontal();
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (package_ui_list.TryGetValue(package_name, out var ui_list))
                    {
                        foreach (var image_path in ui_list)
                        {
                            EditorGUILayout.ObjectField(image_path, typeof(Object), false, GUILayout.Width(200));
                        }
                    }
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndScrollView();
    }
}
