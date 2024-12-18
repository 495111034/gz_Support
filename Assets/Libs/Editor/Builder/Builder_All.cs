using BDFramework.Editor.AssetBundle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;


class MyAssetPostProcessor : AssetPostprocessor
{
    //public static List<string> unitys = new List<string>();
    static int camera_deg = 0;
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath)
    {
        var unitys_path = "tmp_unitys.txt";
        if (!File.Exists(unitys_path)) 
        {
            return;
        }
        var unitys = File.ReadAllLines(unitys_path);
        Log.LogInfo($"unitys={unitys.Length}");
        if (unitys.Length > 0)
        {
            UnityEditor.EditorApplication.delayCall = () =>
            {
                Log.LogError($"unitys={unitys.Length}, camera_deg={camera_deg}");
                if (camera_deg == 0)
                {
                    if (unitys.Length > 1)
                    {
                        var list = new string[unitys.Length - 1];
                        Array.Copy(unitys, 1, list, 0, list.Length);
                        File.WriteAllLines(unitys_path, list);
                    }
                    else 
                    {
                        File.Delete(unitys_path);
                    }
                    var u = unitys[0];
                    Log.LogError($"before open {u}");
                    UnityEditor.SceneManagement.EditorSceneManager.OpenScene(u, OpenSceneMode.Single);
                    Log.LogError($"after  open {u}");
                    camera_deg = 1;
                }
                else if (camera_deg == 1) 
                {
                    var camera = GameObject.FindObjectOfType<Camera>();
                    if (camera)
                    {
                        var go = camera.transform;
                        while (go)
                        {
                            go.localPosition = Vector3.zero;
                            go.localRotation = Quaternion.identity;
                            go = go.parent;
                        }
                        camera.transform.localPosition = Vector3.up * 150;
                        camera_deg = 2;
                    }
                    else 
                    {
                        camera_deg = 0;
                    }
                }
                else
                {
                    if (camera_deg >= 180)
                    {
                        camera_deg = 0;
                        //File.Delete(unitys_path);
                    }
                    else 
                    {
                        var camera = GameObject.FindObjectOfType<Camera>();
                        var eulerAngles = camera.transform.eulerAngles;
                        camera_deg += 20; 
                        eulerAngles.x = camera_deg;
                        eulerAngles.y = eulerAngles.x * 2;
                        camera.transform.eulerAngles = eulerAngles;
                    }
                }
                AssetbundleBuilder.ForceRefresh();
            };
        }
        else
        {
            File.Delete(unitys_path);
        }
    }
}

public static partial class Builder_All
{

    //static HashSet<string> unitys = new HashSet<string>();
    static AssetBuilderInfo _builderInfo;
    static bool _hasError = false;

    public static string CollectAllShaderVariants(bool force)
    {
        var output = PathDefs.ASSETS_PATH_BUILD_SHADERS;
        return output;
    }

    public static void CollectVariantsFromBat1()
    {
        var dir = Path.GetDirectoryName(PathDefs.ASSETS_PATH_BUILD_SHADERS) + '/';
        var ticks = System.DateTime.UtcNow.Ticks;
        var dels = Directory.GetFiles(dir);

        var blacks = new List<string>() 
        {
            "MyShaders/UI/Default",
            "BF/Scene/TerrainDefault",
        };

        var all = new ShaderVariantCollection();
        var shader_scene_svs = new Dictionary<Shader, List<ShaderVariantCollection.ShaderVariant>>();
        //var c = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>($"Assets/SceneShaderVariants2.shadervariants");
        var select = Selection.activeObject;
        Log.LogInfo($"select={AssetDatabase.GetAssetPath(select)}");
        var c = select as ShaderVariantCollection;
        if (!c) 
        {
            Log.LogError($"{select}，不是变体文件");
            return;
        }

        var s = new UnityEditor.SerializedObject(c);
        var m_Shaders = s.FindProperty("m_Shaders");
        //if (m_Shaders.isArray)
        {
            for (int i = 0; i < m_Shaders.arraySize; ++i)
            {
                var pair = m_Shaders.GetArrayElementAtIndex(i);
                var first = pair.FindPropertyRelative("first");
                var second = pair.FindPropertyRelative("second");//ShaderInfo
                var shader = first.objectReferenceValue as Shader;
                if (!shader)
                {
                    continue;
                }
                var path = AssetDatabase.GetAssetPath(shader);
                if (!File.Exists(path))
                {
                    continue;
                }
                if (blacks.Contains(shader.name))
                {
                    continue;
                }
                //
                if (!shader_scene_svs.TryGetValue(shader, out var sv))
                {
                    sv = shader_scene_svs[shader] = new List<ShaderVariantCollection.ShaderVariant>();
                }
                var variants = second.FindPropertyRelative("variants");
                if (variants.isArray)
                {
                    for (var vi = 0; vi < variants.arraySize; ++vi)
                    {
                        var variantInfo = variants.GetArrayElementAtIndex(vi);
                        var keywords = variantInfo.FindPropertyRelative("keywords").stringValue.Split(' ');
                        //if (keywords.Length > 0)
                        {
                            var passType = variantInfo.FindPropertyRelative("passType").intValue;
                            var v = new ShaderVariantCollection.ShaderVariant();
                            v.shader = shader;
                            v.passType = (UnityEngine.Rendering.PassType)passType;
                            v.keywords = keywords;
                            if (all.Add(v))
                            {
                                sv.Add(v);
                            }
                        }
                    }
                }
            }
        }

        //return;



        var mats = Directory.GetFiles("Assets/", "*.mat", SearchOption.AllDirectories);
        foreach (var mat in mats)
        {
            if (mat.StartsWith("Assets/Resources\\") || mat.StartsWith("Assets/Simplygon\\") || mat.StartsWith("Assets/Koenigz\\") || mat.StartsWith("Assets/Libs\\"))
            {
                continue;
            }
            var m = AssetDatabase.LoadAssetAtPath<Material>(mat);
            if (m && m.shader)
            {
                var shader = m.shader;
                if (!File.Exists(AssetDatabase.GetAssetPath(shader)))
                {
                    Log.LogError($"{mat} 使用了错误的材质{shader}");
                    continue;
                }
                if (!shader_scene_svs.ContainsKey(shader) && !blacks.Contains(shader.name))
                {
                    shader_scene_svs[shader] = new List<ShaderVariantCollection.ShaderVariant>();
                    Log.LogError($"{mat}的{shader} 没有搜集到变体");
                }
            }
        }

        AssetDatabase.Refresh();

        var names_map = new Hashtable();
        var names_ht = new Hashtable();
        var names_path = dir + "vcs_name.txt";
        if (File.Exists(names_path))
        {
            names_ht = MiniJSON.JsonDecode( File.ReadAllText(names_path) ) as Hashtable;
        }

        var AllShaders = new List<Shader>();
        var VCs = new List<ShaderVCs>();
        foreach (var kv in shader_scene_svs)
        {
            if (!AssetDatabase.GetAssetPath(kv.Key).StartsWith("Assets/Resources/"))
            {
                //kv.Value.Clear();
                continue;
            }
            if (kv.Value.Count == 0)
            {
                //continue;
            }
            var name = kv.Key.name.Replace(' ', '_').Replace('/', '_');
            var ss = new ShaderVariantCollection[kv.Value.Count];
            for (var i = 0; i < ss.Length; ++i)
            {
                ss[i] = new ShaderVariantCollection();

                Hashtable ht = null;
                if (names_ht.ContainsKey(name))
                {
                    ht = names_ht[name] as Hashtable;
                }
                else 
                {
                    names_ht[name] = ht = new Hashtable();   
                }

                var keywords = kv.Value[i].keywords.ToArray();
                Array.Sort(keywords);
                var key = kv.Value[i].passType + "-" + string.Join('-', kv.Value[i].keywords);
                var key2 = name + "-" + key;
                if (names_map.ContainsKey(key2))
                {
                    throw new Exception(key2);
                }
                names_map[key2] = true;

                string idx;
                if (ht.ContainsKey(key))
                {
                    idx = ht[key] as string;
                }
                else 
                {
                    ht[key] = idx = (ht.Count + 1000).ToString();
                }
                ss[i].name = name + "__" + idx;
                ss[i].Add(kv.Value[i]);
                var output4 = dir + ss[i].name + ".shadervariants";
                AssetDatabase.CreateAsset(ss[i], output4);
                ss[i] = AssetDatabase.LoadAssetAtPath<ShaderVariantCollection>(output4);
            }

            AllShaders.Add(kv.Key);

            var vcs = ScriptableObject.CreateInstance<ShaderVCs>();
            vcs.ShaderVariantCollections = ss;
            vcs.Shader = kv.Key;
            vcs.name = name + "__vcs";
            var output3 = dir + vcs.name + ".asset";
            AssetDatabase.CreateAsset(vcs, output3);
            VCs.Add(AssetDatabase.LoadAssetAtPath<ShaderVCs>(output3));
            if (kv.Value.Count > 0)
            {
                var x = new ShaderVariantCollection();
                x.name = vcs.name + "_view";
                foreach (var v in kv.Value)
                {
                    x.Add(v);
                }
                var outputx = dir + x.name + ".shadervariants";
                Log.Log2File($"create {outputx}");
                AssetDatabase.CreateAsset(x, outputx);
            }
        }

        var sb = new StringBuilder();
        sb.Append('{');
        var keys1 = names_ht.GetKeysArray();
        Array.Sort(keys1);
        foreach (var k1 in keys1) 
        {
            sb.Append("\n\t").Append('"').Append(k1).Append('"').Append(":\n\t{");
            var v1 = names_ht[k1] as Hashtable;
            var keys2 = v1.GetKeysArray();
            Array.Sort(keys2);
            foreach (var k2 in keys2) 
            {
                sb.Append("\n\t\t").Append('"').Append(k2).Append('"').Append(':').Append('"').Append(v1[k2]).Append('"').Append(',');
            }
            sb[sb.Length - 1] = '\n';
            sb.Append("\t},");
        }
        sb[sb.Length - 1] = '}';
        File.WriteAllText(names_path, sb.ToString());
        //File.WriteAllText( names_path, MiniJSON.JsonEncode(names_ht));

        VCs.Sort((a, b) =>
        {
            return a.ShaderVariantCollections.Length - b.ShaderVariantCollections.Length;
        });
        AllShaders.Sort((a, b) => { return a.name.CompareTo(b.name); });

        var fullShaderVaraint = ScriptableObject.CreateInstance<ShaderVariantCollections>();
        fullShaderVaraint.VCs = VCs.ToArray();
        fullShaderVaraint.AllShaders = AllShaders.ToArray();
        string fullpath = dir + "full_shader_variants.asset";
        Log.Log2File($"create {fullpath}");
        AssetDatabase.CreateAsset(fullShaderVaraint, fullpath);

        var empty = ScriptableObject.CreateInstance<ShaderVariantCollections>();
        empty.AllShaders = AllShaders.ToArray();
        var emptypath = dir + "empty_shader_variants.asset";
        Log.Log2File($"create {emptypath}");
        AssetDatabase.CreateAsset(empty, emptypath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        File.Copy(emptypath, PathDefs.ASSETS_PATH_BUILD_SHADERS, true);

        foreach (var del in dels)
        {
            if (del.EndsWith(".meta")) 
            {
                continue;
            }
            if (File.GetLastWriteTimeUtc(del).Ticks < ticks) 
            {
                Log.LogInfo($"del {del}");
                //File.Delete(del);
                //File.Delete(del + ".meta");
            }            
        }
        AssetDatabase.Refresh();
    }

    static void AddBuild(string save_name, string pathname, string dept_type, HashSet<string> depts)
    {
        AssetBundleBuild build = new AssetBundleBuild();
        if (save_name.EndsWith(".playable"))
        {
            save_name = save_name.Replace('.', '_') + ".playable";
        }

        build.assetBundleName = save_name;

        if (false && (depts != null && depts.Count > 0))
        {
            var name = Path.GetFileNameWithoutExtension(save_name);
            if (!name.StartsWith(dept_type))
            {
                name = dept_type + name;
            }
            var depts_path = $"{PathDefs.ASSETS_PATH_BUILD_TEMP}_{name}_depts.txt";
            _AddTempFile(depts_path, depts);
            build.addressableNames = new string[] { "depts", "main" };
            build.assetNames = new string[] { depts_path, pathname };
        }
        else
        {
            build.addressableNames = new string[] { "main" };
            build.assetNames = new string[] { pathname };
        }
        _builderInfo.AddNewAssetbundle(PathDefs.EXPORT_ROOT_OS + "abs", build);
    }

    static void _AddTempFile(string filePath, HashSet<string> depts)
    {
        var arr = AssetBuilderInfo.Sort(depts.ToArray_Ext()) as string[];
        //var txt = MiniJSON.JsonEncode(arr);
        var txt = string.Join(",", arr);
        _builderInfo.AddTempFile(filePath, txt, null);
    }

    static void _log_error(string error)
    {
        _hasError = true;
        Log.LogError("打包日志]" + error);
    }

    static string _get_save_ext_name(Object go)
    {
        if (go is GameObject)
        {
            var path = AssetDatabase.GetAssetPath(go);
            var ext = Path.GetExtension(path.ToLower());
            if (ext == ".fbx")
            {                
                return ext;
            }
        }

        if (go is Texture)
        {
            return ".tex";
        }

        if (go is Material)
        {
            return ".mat";
        }

        if (go is Mesh)
        {
            return ".mesh";
        }

        if (go is Shader)
        {
            return ".shader";
        }

        if (go is Font)
        {
            return ".otf";
        }

        if (go is Avatar)
        {
            return ".avatar";
        }

        if (go is TimelineData)
        {
            return ".asset";
        }

        if (go is UnityEngine.Timeline.TimelineAsset)
        {
            return ".playable";
        }

        if (go is AnimationClip)
        {
            throw new Exception($"此资源类型需要放到ani目录下 {go?.GetType()}, {AssetDatabase.GetAssetPath(go)} ");
        }
        else
        {
            throw new Exception($"此资源类型不允许打包 {go?.GetType()}, {AssetDatabase.GetAssetPath(go)} ");
        }
    }

    static List<string> _dept_get_files(string pathname)
    {
        var my_depts = new List<string>();
        var all_depts = AssetDatabase.GetDependencies(pathname);
        foreach (var dept in all_depts)
        {
            var lo = dept.ToLower();
            if (lo != pathname && !lo.EndsWith(".cs"))
            {
                var dir = Path.GetFileName(Path.GetDirectoryName(lo));
                if (dir != "ani" && dir != "ani_show")
                {
                    my_depts.Add(lo);
                }
            }
            if (lo.EndsWith(".fbx"))
            {
                Log.LogError($"不要依赖fbx文件！{pathname} -> {lo}");
            }
        }
        return my_depts;
    }


    static void _dept_add(HashSet<string> list, string dept)
    {
        if (!string.IsNullOrEmpty(dept) && !dept.StartsWith("__tmp_"))
        {
            list.Add(Path.GetFileNameWithoutExtension(dept));
        }
    }

    public static T[] ToArray_Ext<T>(this HashSet<T> set)
    {
        var arr = new T[set.Count];
        set.CopyTo(arr);
        return arr;
    }


    #region 语言包处理
    //static string language_file = null;
    static Dictionary<string, string> _ui_language_dic;

    static void _ui_OpenLanguageFile()
    {
        string path = PathDefs.EXPORT_PATH_DATA + "lang/";
        string langFile = path + "ui_lang.txt";
        Log.Log2File($"打包日志]read {langFile}");
        if (System.IO.File.Exists(langFile))
        {
            _ui_language_dic = TextParser.ParseIni(System.IO.File.ReadAllText(langFile));
        }
        else
        {
            _ui_language_dic = new Dictionary<string, string>();
        }
    }

    static void _ui_UpdateLanuage(string key, string text)
    {
        if (_ui_language_dic == null)
        {
            Debug.LogError("cannt opend language file");
            return;
        }
        text = text.Replace("\n", "\\n").Replace("\t", "\\t").Replace("\r", "\\r");
        _ui_language_dic[key] = text;
    }

    static void _ui_CloseLanguageFile()
    {
        if (_ui_language_dic != null)
        {
            string text = TextParser.SaveIni(_ui_language_dic);
            string path = PathDefs.EXPORT_PATH_DATA + "lang/";
            if (!System.IO.Directory.Exists(path))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            string langFile = path + "ui_lang.txt";
            Log.Log2File($"打包日志]save {langFile}");
            if (!File.Exists(langFile) || text != File.ReadAllText(langFile))
            {
                File.WriteAllText(langFile, text);
            }
        }
        _ui_language_dic = null;
    }
    #endregion


    public static void fix_all_ui_text(string[] files, bool forcesave) 
    {
        _ui_OpenLanguageFile();
        foreach (var f in files) 
        {
            //AssetDatabase.ImportAsset(f, ImportAssetOptions.ForceUpdate | ImportAssetOptions.DontDownloadFromCacheServer);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(f);
            fix_ui_text(true, go);
            if (forcesave)
            {
                UnityEditor.EditorUtility.SetDirty(go);
                //UnityEditor.PrefabUtility.SavePrefabAsset(go);
            }
        }
        _ui_CloseLanguageFile();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    static List<UnityEngine.EventSystems.UIBehaviour> _uicoms = new List<UnityEngine.EventSystems.UIBehaviour>();
    public static void fix_ui_text(bool save, GameObject go)
    {
        //var graphics = new List<MyText>();
        go.GetComponentsInChildren(true, _uicoms);
        //graphics.Sort((a, b) => { return a.name.CompareTo(b.name); });

        foreach (var graphic in _uicoms)
        {
            if (graphic is Text)
            {
                var text = graphic as MyText;
                var font = text.font;
                if (font)
                {
                    if (text.SaveLanguageID(out var k, out var v))
                    {
                        UnityEditor.EditorUtility.SetDirty(go);
                    }
                    if (!string.IsNullOrEmpty(k))
                    {
                        _ui_UpdateLanuage(k, v);
                    }
                }
                else
                {
                    Log.LogError($"{text.gameObject.GetLocation()}({graphic}),  font is null");
                }
            }
            else 
            {
                Texture tex = null;
                if (graphic is RawImage r)
                {
                    tex = r.texture;
                }
                else if (graphic is IMyTexture it)
                {
                    tex = it.iTexture;
                }

                if (tex)
                {
                    var path = AssetDatabase.GetAssetOrScenePath(tex);
                    var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer)
                    {
                        if (importer.textureType == TextureImporterType.Sprite)
                        {
                            
                            if (graphic is MySpriteImageBase sp)
                            {
                                if (importer.spriteImportMode != SpriteImportMode.Single)
                                {
                                    importer.spriteImportMode = SpriteImportMode.Single;
                                    importer.SaveAndReimport();
                                }

                                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                                Log.LogError($"{graphic.gameObject.GetLocation()}, 单图:{tex.name} 改成:{sprite}");
                                sp.SetSprite(sprite, null);
                                UnityEditor.EditorUtility.SetDirty(go);
                            }
                            else
                            {
                                Log.LogError($"{graphic.gameObject.GetLocation()} -> {path}, textureType 改成 Default, 或者把 {graphic} 改成 Sprite组件"); 
                                //importer.textureType = TextureImporterType.Default;
                                //importer.sRGBTexture = false;
                                //importer.alphaIsTransparency = true;
                                //importer.SaveAndReimport();
                            }
                        }
                    }
                }
            }
        }
    }

    static string _build_UIBundle(string pathname, GameObject go)
    {
        pathname = AssetDatabase.GetAssetPath(go).ToLower();

        var name = Path.GetFileNameWithoutExtension(pathname);
        var save_name = name + ".ab";
        if (_builderInfo.HasAdded(save_name, pathname))
        {
            return save_name;
        }

        //var dict = new Dictionary<string, string>();
        var depts = new HashSet<string>();
        var dept_files = _dept_get_files(pathname);
        foreach (var dept in dept_files)
        {
            var obj = AssetDatabase.LoadAssetAtPath<Object>(dept);
            if (!(obj is GameObject))
            {
                var ab_name = _build_SingleBundle(dept, obj, pathname);
                _dept_add(depts, ab_name);
            }
        }

        fix_ui_text(true, go);

        AddBuild(save_name, pathname, "panel_", depts);

        return save_name;
    }

    static string _build_Prefab(string pathname, GameObject go, string prefab_type)
    {
        pathname = AssetDatabase.GetAssetPath(go).ToLower();
        {
            var dept = go.GetComponent<PrefabDepends>();
            if (dept)
            {
                Log.LogWarning($"{pathname} has PrefabDepends");
                Object.DestroyImmediate(dept, true);
            }
        }

        var name = Path.GetFileNameWithoutExtension(pathname);
        var save_name = name + ".ab";
        //
        if (_builderInfo.HasAdded(save_name, pathname))
        {
            return save_name;
        }

        var depts = new HashSet<string>();
        var dept_files = _dept_get_files(pathname);
        foreach (var dept in dept_files)
        {
            _dept_add(depts, _build_SingleBundle(dept, null, pathname));
        }

        AddBuild(save_name, pathname, prefab_type, depts);

        return save_name;
    }


    static string _build_SingleBundle(string pathname, Object go, string from)
    {
        if (!go && pathname != null)
        {
            if (pathname.EndsWith(".prefab"))
            {
                return Build(pathname);
            }
            go = AssetDatabase.LoadAssetAtPath<Object>(pathname);
        }

        if (!go)
        {
            _log_error($"{pathname} not Object");
            return null;
        }

        pathname = AssetDatabase.GetAssetPath(go).ToLower();
        if (pathname == "resources/unity_builtin_extra")
        {
            return null;
        }

        var name = Path.GetFileNameWithoutExtension(pathname);
        var ext = _get_save_ext_name(go);
        var save_name = name + ext;
        //特殊规则 改名        
        if (PathDefs.IsAssetsResources(pathname))
        {
            if (go is Shader)
            {
                //shader
                save_name = "_shader_" + save_name;
                return null;
            }
            else if (go is Font)
            {
                //res 临时资源，避免冗余
                save_name = "__tmp_res_" + save_name;
            }
            else
            {
                Log.LogError($"resources下该类型的资源不允许导出, {go.GetType()}, {pathname}, from {from}");
                //_log_error($"resources下该类型的资源不允许导出, {go.GetType()}, {pathname}, from {from}");
                return null;
            }
        }
        else if (go is Texture)
        {
            //光照贴图
            if (pathname.StartsWith(PathDefs.ASSETS_PATH_UNITYSECNE))
            {
                save_name = Path.GetFileName(Path.GetDirectoryName(pathname)) + "-" + save_name;
            }
        }
        //
        if (!_builderInfo.HasAdded(save_name, pathname))
        {
            //
            if (pathname.StartsWith(PathDefs.ASSETS_PATH_GUI_SPRITES))
            {
                var dir = Path.GetDirectoryName(pathname);
                return _ui_gen_Packer(Path.GetFileName(dir), true);
            }
            else if (pathname.StartsWith(PathDefs.PREFAB_PATH_UI_PACKERS))
            {
                return _ui_gen_Packer(Path.GetFileNameWithoutExtension(pathname), true);
            }
            //
            HashSet<string> depts = null;
            if (go is Shader || go is Material || go is ShaderVariantCollection || go is TimelineData)
            {
                var dept_files = _dept_get_files(pathname);
                if (dept_files.Count > 0)
                {
                    if (go is ShaderVariantCollection)
                    {
                        AssetBundleBuild build = new AssetBundleBuild();
                        build.assetBundleName = save_name;

                        var names = new List<string>() { "main" };
                        foreach (var dept in dept_files)
                        {
                            names.Add(Path.GetFileNameWithoutExtension(dept));
                        }
                        dept_files.Insert(0, pathname);

                        build.addressableNames = names.ToArray();
                        build.assetNames = dept_files.ToArray();

                        _builderInfo.AddNewAssetbundle(PathDefs.EXPORT_ROOT_OS + "abs", build);
                        return save_name;
                    }
                    //
                    depts = new HashSet<string>();
                    foreach (var dept in dept_files)
                    {
                        _dept_add(depts, _build_SingleBundle(dept, null, pathname));
                    }
                }
            }
            //
            AddBuild(save_name, pathname, depts == null ? "" : ext.Substring(1) + '_', depts);
        }
        return save_name;
    }

    static string _ui_gen_Packer(string atlasName, bool build, bool log = false, bool force = false)
    {
        var atlasdir = PathDefs.ASSETS_PATH_GUI_SPRITES + atlasName;
        if (!Directory.Exists(atlasdir)) 
        {
            _log_error($"atlasdir={atlasdir} not found!");
            return null;
        }        

        var gen_atlasdir = PathDefs.PREFAB_PATH_UI_PACKERS + atlasName;
        if (!Directory.Exists(gen_atlasdir))
        {
            Directory.CreateDirectory(gen_atlasdir);
            AssetDatabase.Refresh();
        }
        var prefabPath = gen_atlasdir + $"/{atlasName}.prefab";
        var packerImagePath = gen_atlasdir + $"/{atlasName}.png";
        //
        var assetBundleName = atlasName + ".ab";
        if (build && _builderInfo.HasAdded(assetBundleName, prefabPath))
        {
            return assetBundleName;
        }

        var dirs = System.IO.Directory.GetDirectories(atlasdir);
        foreach (var dir in dirs)
        {
            var dirname = Path.GetFileName(dir);
            if (dirname == "." || dirname == "..")
            {
                continue;
            }
            Log.LogError($"图集目录[{atlasdir}]下不能存放文件夹[{dirname}]，请尽快删除！");
        }

        //
        // var atlasName = packerName;
        var images = System.IO.Directory.GetFiles(atlasdir, "*.png", SearchOption.TopDirectoryOnly);
        var modifyed = true;
        if (!force && File.Exists(prefabPath) && File.Exists(packerImagePath))
        {
            var last_datetime = File.GetLastWriteTimeUtc(packerImagePath);
            //modifyed = false;
            modifyed = Directory.GetLastWriteTimeUtc(atlasdir).CompareTo(last_datetime) > 0;
            if (!modifyed)
            {
                foreach (var image in images)
                {
                    if (File.GetLastWriteTimeUtc(image).CompareTo(last_datetime) > 0)
                    {
                        modifyed = true;
                        break;
                    }
                    if (File.GetLastWriteTimeUtc(image + ".meta").CompareTo(last_datetime) > 0)
                    {
                        modifyed = true;
                        break;
                    }
                }
            }

            //
            if (!modifyed)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                UnityEngine.UI.MySpritePacker mysper = prefab.AddMissingComponent<UnityEngine.UI.MySpritePacker>();
                
                if (!mysper || !mysper.PackerImage || mysper.PackerImageMD5 != StringUtils.md5(File.ReadAllBytes(packerImagePath)) || mysper.uvList == null || mysper.uvList.Length != images.Length)
                {
                    modifyed = true;
                }
            }
        }
        

        //modifyed = true;
        if (!modifyed) 
        {
            if (log) 
            {
                Log.LogInfo($"打包日志]图集：{atlasName} 已是最新!");
            }
        }else
        {
            //
#if DEBUG_PACKER
            {
                foreach (var image in images)
                {
                    //6 5 4
                    //7 8 3
                    //0 1 2
                    var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(image);
                    //第一列
                    tex.SetPixel(0, 0, Color.red);//0
                    tex.SetPixel(0, tex.height / 2, Color.red);//7
                    tex.SetPixel(0, tex.height - 1, Color.red);//6                

                    //第二列
                    tex.SetPixel(tex.width / 2, 0, Color.red);//1
                    tex.SetPixel(tex.width / 2, tex.height / 2, Color.red);//8
                    tex.SetPixel(tex.width / 2, tex.height - 1, Color.red);//5

                    //第三列
                    tex.SetPixel(tex.width - 1, 0, Color.red);//2
                    tex.SetPixel(tex.width - 1, tex.height / 2, Color.red);//3
                    tex.SetPixel(tex.width - 1, tex.height - 1, Color.red);//4

                    var tex_bytes = tex.EncodeToPNG();
                    File.WriteAllBytes(image, tex_bytes);
                }
                AssetDatabase.Refresh();
            }
#endif
            int areas = 0;
            //
            List<Sprite> spriteList = new List<Sprite>();
            List<Texture2D> textureList = new List<Texture2D>();
            foreach (var image in images)
            {
                var ti = AssetImporter.GetAtPath(image) as TextureImporter;
                var sp = AssetDatabase.LoadAssetAtPath(image, typeof(Sprite)) as Sprite;
                TextureImporterPlatformSettings tsAdnroid = ti.GetPlatformTextureSettings(PathDefs.os_name) ?? new TextureImporterPlatformSettings();
                if (!sp || !tsAdnroid.overridden || tsAdnroid.format != TextureImporterFormat.RGBA32) 
                {
                    ti.textureType = TextureImporterType.Sprite;
                    ti.isReadable = true;
                    ti.alphaIsTransparency = true;

                    tsAdnroid.name = PathDefs.os_name;
                    tsAdnroid.overridden = true;
                    tsAdnroid.maxTextureSize = 1024;
                    tsAdnroid.compressionQuality = 50;
                    tsAdnroid.format = TextureImporterFormat.RGBA32;
                    ti.SetPlatformTextureSettings(tsAdnroid);
                    AssetDatabase.Refresh();
                    sp = AssetDatabase.LoadAssetAtPath(image, typeof(Sprite)) as Sprite;
                }

                if (sp)
                {
                    spriteList.Add(sp);
                    textureList.Add(sp.texture);
                    areas += sp.texture.width * sp.texture.height;
                }
                else
                {
                    _log_error($"image={image} not a Sprite");
                }
            }
            Texture2D packerImage = new Texture2D(2048, 2048);
            Rect[] packingResult = packerImage.PackTextures(textureList.ToArray(), 2);
            if (packingResult.Length != spriteList.Count)
            {
                _log_error($"atlasdir={atlasdir} packingResult.Length:{packingResult.Length} != spriteList.Count:{spriteList.Count}");
                return null;
            }
            //var go3 = GameObject.Find("RawImage3");
            //var raw = go3.GetComponent<RawImage>();
            //raw.texture = packerImage;

            var rgba = packerImage;
            if(rgba.format != TextureFormat.RGBA32)
            {
                Log.LogError($"打包日志]导出的图集贴图format={rgba.format} -> rgba32, 可能会出现紫色底，重启unity重试");
                var pixels = packerImage.GetPixels32();
                rgba = new Texture2D(packerImage.width, packerImage.height);                
                rgba.alphaIsTransparency = true;
                rgba.SetPixels32(pixels);                
            }
            //
            rgba.Apply();
            Log.LogInfo($"打包日志]生成图集：{atlasName}, force={force}, format={rgba.format}");
            var bytes = rgba.EncodeToPNG();
            File.WriteAllBytes(packerImagePath, bytes);

            UnityEngine.UI.MySpriteInfo[] UVDic = new UnityEngine.UI.MySpriteInfo[textureList.Count];
            for (int i = 0; i < textureList.Count; ++i)
            {
                var tex = textureList[i];
                var sp = spriteList[i];

                var info = new UnityEngine.UI.MySpriteInfo();
                info.name = tex.name;
                info.rect = packingResult[i];
                if (info.rect == Rect.zero) 
                {
                    Log.LogError($"erro rect for {tex}");
                }
                info.size = new Vector2(tex.width, tex.height);
                info.pivot = sp.pivot;
                info.border = sp.border;
                //info.padding = UnityEngine.Sprites.DataUtility.GetPadding(sp);
                info.pixelsPerUnit = sp.pixelsPerUnit;
                info.mainTextureSize = new Vector2(packerImage.width, packerImage.height);
                UVDic[i] = info;
            } 

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            //Log.LogError($"{packerImagePath},{prefabPath}");
            if (!prefab)
            {
                var go = new GameObject();
                prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
                GameObject.DestroyImmediate(go);
            }
            AssetDatabase.Refresh();
            //
            UnityEngine.UI.MySpritePacker mysper = prefab.AddMissingComponent<UnityEngine.UI.MySpritePacker>();
            mysper.PackerImage = AssetDatabase.LoadAssetAtPath<Texture2D>(packerImagePath);
            mysper.PackerImageMD5 = StringUtils.md5(File.ReadAllBytes(packerImagePath));
            mysper.SerializePackInfo(UVDic);
            //
            EditorUtility.SetDirty(prefab);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Object.DestroyImmediate(packerImage);
            Object.DestroyImmediate(rgba);

            //UnityEditor.EditorGUIUtility.PingObject(prefab);
        }
        if (build)
        {
            return _ui_build_Packer(prefabPath, AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath));
        }
        return null;
    }
    static string _ui_build_Packer(string pathname, GameObject go)
    {
        pathname = AssetDatabase.GetAssetPath(go).ToLower();

        var name = Path.GetFileNameWithoutExtension(pathname);
        _ui_gen_Packer(name, false);
        
        var assetBundleName = name + ".ab";
        if (_builderInfo.HasAdded(assetBundleName, pathname))
        {
            return assetBundleName;
        }
        var pack = go.GetComponent<MySpritePacker>();
        AssetBundleBuild build = new AssetBundleBuild();
        build.assetBundleName = assetBundleName;
        build.assetNames = new string[] { pathname, AssetDatabase.GetAssetPath(pack.PackerImage).ToLower() };
        build.addressableNames = new string[] { "main", "packer" };
        _builderInfo.AddNewAssetbundle(PathDefs.EXPORT_ROOT_OS + "abs", build);
        return assetBundleName;
    }

    public static string GenPacker(string dir, bool log, bool force = false)
    {
        if (dir.StartsWith(PathDefs.ASSETS_PATH_GUI_SPRITES))
        {
            return _ui_gen_Packer(Path.GetFileName(dir), false, log, force);
        }
        //
        Log.LogError($"打包日志]{dir} 不是图集目录");
        return null;
    }

    static long _time_start;
    public static void BeforeBuild()
    {
        Log.Log2File($"打包日志]开始构建打包信息 ...");

        _time_start = DateTime.Now.Ticks;
        _scene_build_paths.Clear();
        _builderInfo = new AssetBuilderInfo();
        _hasError = false;
        _ui_OpenLanguageFile();
    }

    public static string Build(string pathname)
    {
        pathname = pathname.ToLower().Replace('\\', '/');

        if (Directory.Exists(pathname))
        {
            return _ui_gen_Packer(Path.GetFileName(pathname) , true);
        }

        if (pathname.EndsWith(".unity"))
        {
            return _build_SceneBundle(pathname, true);
        }

        var obj = AssetDatabase.LoadAssetAtPath(pathname, typeof(Object));
        if (!obj)
        {
            _log_error($"{pathname} not Object");
            return null; 
        }

        var go = obj as GameObject;
        if (!go)
        {
            return _build_SingleBundle(pathname, obj, pathname);
        }

        if (PathDefs.IsAssetsResources(pathname)) 
        {
            _log_error($"不能导出resouces 下的 gameobject，{pathname}");
            return null;
        }

        {
            var dept = go.GetComponent<PrefabDepends>();
            if (dept)
            {
                Log.LogWarning($"{pathname} has PrefabDepends");
                Object.DestroyImmediate(dept, true);
            }
        }

        AssetbundleBuilder.check_mat_mesh(pathname, go);

        if (pathname.StartsWith(PathDefs.PREFAB_PATH_UI_PACKERS))
        {
            return _ui_build_Packer(pathname, go);
        }

        if (pathname.StartsWith(PathDefs.PREFAB_PATH_GUI_PANEL))
        {
            return _build_UIBundle(pathname, go);
        }

        if (go.GetComponentInChildren<ParticleSystemRenderer>())
        {
            return _build_Prefab(pathname, go, "fx_");
        }

        if (go.GetComponent<Animator>())
        {
            return _build_Prefab(pathname, go, "actor_");
        }

        return _build_Prefab(pathname, go, "prefab_");
    }

    public static string fix_scene(string unity, bool is_fix_error)
    {
        //_build_SceneBundle(unity, false);
        return _fix_scene(unity, is_fix_error);
    }

    public static string empty_scene_path(string unity) 
    {
        var scenename = Path.GetFileNameWithoutExtension(unity);
        var emptypath = (PathDefs.PREFAB_PATH_SCENES_TEMP_EMPTY + scenename + "_empty.unity").ToLower();
        return emptypath;
    }

    static void del_hide_child(Transform t) 
    {
        for (var i = t.childCount - 1; i >= 0; --i)
        {
            var go = t.GetChild(i).gameObject;
            if (!go.activeInHierarchy && PrefabUtility.GetCorrespondingObjectFromSource(go))
            {
                GameObject.DestroyImmediate(go);
            }
            else 
            {
                del_hide_child(t.GetChild(i));
            }
        }
    }


    public static void del_hide_child(string unity)
    {
        var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(unity);
        var roots = scene.GetRootGameObjects();
        foreach (var root in roots)
        {
            del_hide_child( root.transform );
        }
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
    }

    static void _remove_missing_mono(Transform t)
    {
        var cnt = UnityEditor.GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
        if (cnt > 0)
        {
            Log.LogError($"checkprefab {t.gameObject.GetLocation()} 存在错误的mono={cnt}");
        }
        foreach (Transform c in t)
        {
            _remove_missing_mono(c);
        }
    }


    static List<MeshFilter> filters_go = new List<MeshFilter>();
    static List<MeshFilter> filters_prefab = new List<MeshFilter>();
    static List<Renderer> renderers_go = new List<Renderer>();
    static List<Renderer> renderers_prefab = new List<Renderer>();
    static Transform fix_errors_root;
    static void transform2tree(int childi, Transform t, Dictionary<GameObject, int> childs_prefabs)
    {
        if (PrefabUtility.IsPrefabAssetMissing(t.gameObject))
        {
            Log.LogError($"checkprefab {childi} {t.gameObject.GetLocation()} 丢失prefab");
            t.SetParent(fix_errors_root, true);
            return;
        }
        //else
        {
            var prefab = PrefabUtility.GetCorrespondingObjectFromSource(t.gameObject);
            if (prefab)
            {
                //if (t.gameObject.name.StartsWith("1002_perfectculling"))
                //{
                //    Log.Log2File("here");
                //}
                //else
                //{
                //    return null;
                //}
                //if (is_fix_error)
                {
                    var path = AssetDatabase.GetAssetPath(prefab).ToLower();
                    if (!path.EndsWith(".prefab"))
                    {
                        Log.LogError($"checkprefab {childi} {t.gameObject.GetLocation()} prefab后缀名错误，{path}");
                        t.SetParent(fix_errors_root, true);
                        return;
                    }

                    //if (is_fix_error)
                    {
                        var name = Path.GetFileNameWithoutExtension(path);
                        if (childs_prefabs.TryGetValue(prefab, out var cnt))
                        {
                            //name += $" ({cnt})";                            
                        }
                        if (cnt > 0)
                        {
                            name += $" ({childi},{cnt})";
                        }
                        if (t.gameObject.name != name)
                        {
                            //Log.LogError($"change {t.gameObject.GetLocation()} -> {name}");
                            t.gameObject.name = name;
                        }
                        childs_prefabs[prefab] = cnt + 1;
                    }
                    var meshcollider = t.gameObject.GetComponentInChildren<MeshCollider>();
                    if (meshcollider && meshcollider.convex && meshcollider.sharedMesh is Mesh mesh)
                    {
                        if (!mesh.isReadable)
                        {
                            //Log.LogError($"MeshCollider的mesh 需要勾选读写, {meshcollider.gameObject.GetLocation()}");        
                        }
                    }
                    var valids = new List<PropertyModification>();
                    var modifys = PrefabUtility.GetPropertyModifications(t.gameObject);
                    //PropertyModification[] newmodifys = null;
                    foreach (var m in modifys)
                    {
                        if (m.target)
                        {
                            if (m.target is Renderer prefab_rd && m.objectReference is Material mat)
                            {
                                var go_target = prefab_rd.transform;
                                if (go_target.parent)
                                {
                                    var rdpath = prefab_rd.gameObject.GetLocation();
                                    var go_rdpath = rdpath.Substring(rdpath.IndexOf('/') + 1);
                                    go_target = t.Find(go_rdpath);
                                }
                                var go_rd = go_target.GetComponent<Renderer>();

                                var prefab_sharedMaterials = prefab_rd.sharedMaterials;
                                var go_sharedMaterials = go_rd.sharedMaterials;
                                if (go_rd.sharedMaterials.Length == prefab_rd.sharedMaterials.Length)
                                {
                                    var same = true;
                                    for (var i = 0; i < go_sharedMaterials.Length; ++i)
                                    {
                                        if (go_sharedMaterials[i] != prefab_sharedMaterials[i])
                                        {
                                            same = false;
                                            break;
                                        }
                                    }
                                    if (same)
                                    {
                                        Log.LogInfo($"checkprefab {childi} {t.gameObject.GetLocation()} 1drop PropertyModification, {m.objectReference} at {m.propertyPath} on {m.target}, value={m.value}");
                                        continue;
                                    }
                                }
                                if (Array.IndexOf(go_sharedMaterials, mat) < 0)
                                {
                                    Log.LogInfo($"checkprefab {childi} {t.gameObject.GetLocation()} 2drop PropertyModification, {m.objectReference} at {m.propertyPath} on {m.target}, value={m.value}");
                                    continue;
                                }
                                //
                                Log.LogError($"checkprefab {childi} {go_rd.gameObject.GetLocation()}.{go_rd.GetType().Name}的材质{mat.name} 需要apply");
                            }
                            valids.Add(m);
                        }
                        else
                        {
                            Log.LogInfo($"checkprefab {childi} {t.gameObject.GetLocation()} drop PropertyModification, objectReference={m.objectReference} at propertyPath={m.propertyPath}, value={m.value}");
                        }
                    }
                    //
                    //
                    if (valids.Count != modifys.Length)
                    {
                        PrefabUtility.SetPropertyModifications(t.gameObject, valids.ToArray());
                        var newmodifys = PrefabUtility.GetPropertyModifications(t.gameObject);
                        Log.LogError($"checkprefab {childi} {t.gameObject.GetLocation()} SetPropertyModifications from {modifys.Length} -> {valids.Count} -> {newmodifys.Length}");
                        if (newmodifys.Length != valids.Count)
                        {
                            Log.LogError($"checkprefab {childi} {t.gameObject.GetLocation()} SetPropertyModifications 失败");
                        }
                    }

                    {
                        var coms = PrefabUtility.GetAddedComponents(t.gameObject);
                        if (coms != null)
                        {
                            foreach (var com in coms)
                            {
                                if (com.instanceComponent is not Collider && com.instanceComponent is not SceneObjectShadow)
                                {
                                    //if (is_fix_error)
                                    {
                                        Log.LogInfo($"ApplyAddedComponent {path} <- {com.instanceComponent} at {t.gameObject.GetLocation()}");
                                        try
                                        {
                                            PrefabUtility.ApplyAddedComponent(com.instanceComponent, path, InteractionMode.AutomatedAction);
                                        }
                                        catch (Exception e)
                                        {
                                            Log.LogError(e.Message);
                                            Object.DestroyImmediate(com.instanceComponent);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    {
                        var coms = PrefabUtility.GetRemovedComponents(t.gameObject);
                        if (coms != null)
                        {
                            foreach (var com in coms)
                            {
                                if (com.assetComponent is not Collider)
                                {
                                    //if (is_fix_error)
                                    {
                                        Log.LogInfo($"ApplyRemovedComponent {path} -> {com.assetComponent}");
                                        PrefabUtility.ApplyRemovedComponent(com.containingInstanceGameObject, com.assetComponent, InteractionMode.AutomatedAction);
                                    }
                                }
                            }
                        }
                    }

                    var overrides = PrefabUtility.GetObjectOverrides(t.gameObject);
                    foreach (var ovd in overrides)
                    {
                        //if (ovd.instanceObject != prefab.transform)
                        if (
                                !(ovd.instanceObject is Transform) && !(ovd.instanceObject is GameObject) && !(ovd.instanceObject is LODGroup) &&
                                !(ovd.instanceObject is Collider) && !(ovd.instanceObject is WeatherSystem.WeatherSystemScript) && !(ovd.instanceObject is Renderer) &&
                                !(ovd.instanceObject is SceneObjectShadow)
                           )
                        {
                            //if (is_fix_error)
                            {
                                var com = (ovd.instanceObject as Component)?.gameObject;
                                Log.LogInfo($"ApplyObjectOverride {path} -> {ovd.instanceObject}, activeSelf={t.gameObject.activeSelf}, {(com ?? t.gameObject).GetLocation()}");
                                PrefabUtility.ApplyObjectOverride(ovd.instanceObject, path, InteractionMode.AutomatedAction);
                                //Log.LogInfo($"activeSelf={t.gameObject.activeSelf}");
                                //ovd.Apply(path, InteractionMode.AutomatedAction);
                            }
                        }
                    }
                    _remove_missing_mono(t);

                    prefab.transform.GetComponentsInChildren(true, filters_prefab);
                    prefab.transform.GetComponentsInChildren(true, renderers_prefab);
                    t.TryGetComponent<Animator>(out var animator_prefab);

                    t.GetComponentsInChildren(true, filters_go);
                    t.GetComponentsInChildren(true, renderers_go);
                    t.TryGetComponent<Animator>(out var animator_go);

                    if (animator_prefab && !animator_go)
                    {
                        Log.LogError($"checkprefab {childi} {t.gameObject.GetLocation()} Animator 丢失");
                        t.SetParent(fix_errors_root, true);
                    }
                    if (!animator_prefab && animator_go)
                    {
                        Log.LogError($"checkprefab {childi} {t.gameObject.GetLocation()} Animator 没有 applay 到 prefab");
                        t.SetParent(fix_errors_root, true);
                    }
                    if (animator_prefab && !animator_prefab.runtimeAnimatorController)
                    {
                        Log.LogError($"checkprefab {childi} {t.gameObject.GetLocation()} prefab 丢失 Animator.runtimeAnimatorController");
                        t.SetParent(fix_errors_root, true);
                    }
                    if (animator_go && !animator_go.runtimeAnimatorController)
                    {
                        Log.LogError($"checkprefab {childi} {t.gameObject.GetLocation()} 丢失 Animator.runtimeAnimatorController");
                        t.SetParent(fix_errors_root, true);
                    }

                    if (renderers_prefab.Count != renderers_go.Count)
                    {
                        Log.LogError($"checkprefab {childi} {t.gameObject.GetLocation()} MeshRenderer数量 和prefab不一致");
                    }
                    else
                    {
                        for (int i = 0; i < renderers_prefab.Count; ++i)
                        {
                            if (!renderers_prefab[i].sharedMaterial)
                            {
                                Log.LogError($"checkprefab {renderers_prefab[i].gameObject.GetLocation()}的{renderers_prefab[i].GetType().Name} 丢失材质");
                            }
                            else
                            {
                                var matpath = AssetDatabase.GetAssetPath(renderers_prefab[i].sharedMaterial);
                                if (string.IsNullOrEmpty(matpath))
                                {
                                    Log.LogError($"checkprefab {renderers_prefab[i].gameObject.GetLocation()}的{renderers_prefab[i].GetType().Name} 无法获取材质文件的路径");
                                }
                                else if (matpath.ToLower().EndsWith(".fbx"))
                                {
                                    Log.LogError($"checkprefab {renderers_prefab[i].gameObject.GetLocation()}的{renderers_prefab[i].GetType().Name} 引用了fbx内部材质，会导致材质冗余和泄漏");
                                }
                            }

                            if (renderers_prefab[i].sharedMaterial != renderers_go[i].sharedMaterial)
                            {
                                Log.LogError($"checkprefab {childi} {t.gameObject.GetLocation()} 材质({renderers_go[i].sharedMaterial?.name})和prefab({renderers_go[i].sharedMaterial?.name})不一致");
                            }
                        }
                    }

                    if (filters_prefab.Count != filters_go.Count)
                    {
                        Log.LogError($"checkprefab {childi} {t.gameObject.GetLocation()} MeshFilter数量 和prefab不一致");
                    }
                    else
                    {
                        for (int i = 0; i < filters_prefab.Count; ++i)
                        {
                            if (!filters_prefab[i].sharedMesh)
                            {
                                Log.LogError($"checkprefab {filters_prefab[i].gameObject.GetLocation()}的{filters_prefab[i].GetType().Name} 丢失模型");
                            }
                            if (filters_prefab[i].sharedMesh != filters_go[i].sharedMesh)
                            {
                                Log.LogError($"checkprefab {childi} {t.gameObject.GetLocation()} 模型({filters_go[i].sharedMesh?.name})和prefab({filters_go[i].sharedMesh?.name})不一致");
                            }
                        }
                    }
                }
                return;
            }
            else
            {
                if (t.gameObject.activeInHierarchy && t.root.name != "[Light]")
                {
                    var Renderer = t.GetComponent<Renderer>();
                    if (Renderer)
                    {
                        Log.LogError($"非prefab节点，配置了Renderer组件。{t.gameObject.GetLocation()}");
                    }
                    var MeshFilter = t.GetComponent<MeshFilter>();
                    if (MeshFilter)
                    {
                        Log.LogError($"非prefab节点，配置了MeshFilter组件。{t.gameObject.GetLocation()}");
                    }
                }
                if (t.childCount > 0)
                {
                    for (int i = t.childCount - 1; i >= 0; --i)
                    {
                        transform2tree(i, t.GetChild(i), childs_prefabs);
                    }
                }
                return;
            }
        }
    }


    static object transform2tree2(string tab, Transform t)
    {
        if (!t.gameObject.activeSelf && t.root.name != "[Effect]")
        {
            return null;
        }
        if (PrefabUtility.IsPrefabAssetMissing(t.gameObject))
        {
            return null;
        }
        var prefab = PrefabUtility.GetCorrespondingObjectFromSource(t.gameObject);
        if (prefab)
        {
            return prefab.name.ToLower();
        }
        if (t.childCount > 0)
        {
            var tab2 = tab + "        ";
            var allnull = true;
            var list = new ArrayList();
            for (int i = 0; i < t.childCount; ++i)
            {
                var c = t.GetChild(i);
                var path = c.gameObject.GetLocation();
                var e = transform2tree2(tab2, c);
                if (e is ArrayList al)
                {
                    Debug.Log($"{tab}{i} {path} + {al.Count}");
                }
                else if (e is string s) 
                {
                    if (c.name.StartsWith(s, StringComparison.CurrentCultureIgnoreCase))
                    {
                        Debug.Log($"{tab}{i} {path} <- {s}");
                    }
                    else 
                    {
                        Debug.LogError($"{tab}{i} {path} <- {s}");
                    }                    
                }
                else
                {
                    Debug.Log($"{tab}{i} {path} <- null");
                }
                list.Add(e);
                if (e != null)
                {
                    allnull = false;
                }
            }
            if (!allnull)
            {
                return list;
            }
        }
        return null;
    }



    static string _fix_scene(string unity, bool is_fix_error)
    {
        fix_errors_root = null;
        if (unity.StartsWith(PathDefs.PREFAB_PATH_SCENES_TEMP_EMPTY))
        {
            if (!is_fix_error)
            {
                var xjsonpath = PathDefs.PREFAB_PATH_SCENES_TEMP_EMPTY + Path.GetFileNameWithoutExtension(unity).Replace("_empty","") + ".json";
                _check_xml(unity, xjsonpath);
            }
            throw new Exception( unity );
        }

        var emptypath = empty_scene_path(unity);
        var scenename = Path.GetFileNameWithoutExtension(unity);
        var jsonpath = PathDefs.PREFAB_PATH_SCENES_TEMP_EMPTY + scenename + ".json";
        var md5path = PathDefs.PREFAB_PATH_SCENES_TEMP_EMPTY + scenename + ".md5.txt";
        var md5_str = "";
        {
            var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            md5.ComputeHash(File.OpenRead(unity));
            foreach (var b in md5.Hash)
            {
                md5_str += b.ToString("X2");
            }
            md5.Clear();
            md5.Dispose();
        }
        
        if (!File.Exists(emptypath) || !File.Exists(md5path) || File.ReadAllText(md5path) != md5_str || !File.Exists(jsonpath) || is_fix_error)
        {
            var childs_prefabs = new Dictionary<GameObject, int>();
            Log.Log2File($"打包日志] 清空场景， path={emptypath}, {md5_str}, is_fix_error={is_fix_error}");

            var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            if (scene.path.ToLower() != unity)
            {
                scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(unity);
            }

            //fix_errors_root = null;
            if (is_fix_error)
            {
                var go = GameObject.Find("fix_errors_root");
                if (go && go.transform.parent != null)
                {
                    go.name += "_old";
                    go = null;
                }
                if (!go)
                {
                    go = new GameObject("fix_errors_root");
                }
                fix_errors_root = go.transform;
            }

            //var childCount = fix_errors_root ? fix_errors_root.childCount : 0;
            //Hashtable tree = new Hashtable();
            var roots = scene.GetRootGameObjects();
            var n = 0;
            foreach (var root in roots)
            {
                if (root.name[0] != '[' || root.transform.childCount == 0)
                {
                    if (!is_fix_error)
                    {
                        GameObject.DestroyImmediate(root);
                    }
                }
                else
                {
                    var rt = root.transform;
                    if (!is_fix_error && root.name != "[Effect]")
                    {
                        var islight = root.name == "[Light]";
                        for (var i = rt.childCount - 1; i >= 0; --i)
                        {
                            var go = rt.GetChild(i).gameObject;
                            if (!go.activeInHierarchy || (islight && !go.GetComponent<Light>()))
                            {
                                GameObject.DestroyImmediate(go);
                            }
                        }
                    }

                    if (is_fix_error)
                    {
                        transform2tree(n++, rt, childs_prefabs);
                    }
                    {
                        var colliders = root.GetComponentsInChildren<MeshCollider>(true);
                        foreach (var c in colliders)
                        {
                            var mf = c.gameObject.GetComponent<MeshFilter>();
                            if (!mf)
                            {
                                Log.LogError($"碰撞盒对象 缺少 MeshFilter 组件, {c.gameObject.GetLocation()}");
                                c.sharedMesh = null;
                            }
                            else if (!c.sharedMesh)
                            {
                                Log.LogError($"碰撞盒对象 mesh 丢失, {c.gameObject.GetLocation()}");
                            }
                            else
                            {
                                if (!c.sharedMesh.isReadable)
                                {
                                    var path_c = AssetDatabase.GetAssetOrScenePath(c.sharedMesh);
                                    var model = AssetImporter.GetAtPath(path_c) as ModelImporter;
                                    if (model && !model.isReadable)
                                    {
                                        Log.LogError($"自动勾上读写, fbx={path_c}");
                                        model.isReadable = true;
                                        model.SaveAndReimport();
                                    }
                                    else
                                    {
                                        Log.LogError($"碰撞盒的 {c.sharedMesh.name} 需要勾上读写, {c.gameObject.GetLocation()}, fbx={path_c}");
                                    }
                                }

                                if (c.sharedMesh != mf.sharedMesh)
                                {
                                    var path_c = AssetDatabase.GetAssetOrScenePath(c.sharedMesh);
                                    if (!c.sharedMesh.name.EndsWith("_collider"))
                                    {
                                        var path_mf = AssetDatabase.GetAssetOrScenePath(mf.sharedMesh);
                                        Log.LogError($"碰撞盒模型({c.sharedMesh}) 和 MeshFilter模型({mf.sharedMesh}) 不一致, {c.gameObject.GetLocation()}\nMeshCollider={path_c}\nMeshFilter={path_mf}");
                                    }
                                }
                                else
                                {
                                    if (!is_fix_error)
                                    {
                                        c.sharedMesh = null;
                                        if (!c.enabled)
                                        {
                                            Object.DestroyImmediate(c);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (!is_fix_error)
                    {
                        //
                        if (true)
                        {
                            foreach (Transform c in root.transform)
                            {
                                if (c.name != "__statics")
                                {
                                    c.GetComponentsInChildren(true, renderers_go);
                                    foreach (var rd in renderers_go)
                                    {
                                        //if (rd is not ParticleSystemRenderer)
                                        {
                                            rd.enabled = false;
                                        }
                                        if (rd is SkinnedMeshRenderer skin)
                                        {
                                            skin.sharedMesh = null;
                                        }
                                    }
                                    renderers_go.Clear();
                                    //
                                    c.GetComponentsInChildren(true, filters_go);
                                    foreach (var ft in filters_go)
                                    {
                                        ft.sharedMesh = null;
                                    }
                                    filters_go.Clear();
                                }
                            }
                        }
                        //
                        var lights = root.GetComponentsInChildren<Light>(true);
                        foreach (var light in lights)
                        {
                            if (light.lightmapBakeType == LightmapBakeType.Baked)
                            {
                                GameObject.DestroyImmediate(light.gameObject);
                            }
                        }
                    }
                }
            }

            if (is_fix_error)
            {
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);
                AssetbundleBuilder.ForceRefresh();
                //UnityEditor.SceneManagement.EditorSceneManager.CloseScene(scene, false);
                Log.Log2File($"打包日志] 保存当前场景");
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            else
            {
                Log.Log2File($"打包日志] 保存到空场景 {emptypath}");
                UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, emptypath, true);

                UnityEditor.AssetDatabase.Refresh();
                scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(emptypath);
                roots = scene.GetRootGameObjects();
                var tree = new Hashtable();
                foreach (var root in roots)
                {
                    tree[root.name] = transform2tree2("", root.transform);
                }
                var values = new HashSet<string>();
                foreach (DictionaryEntry kv in tree)
                {
                    _collect_array_r(kv.Value as ArrayList, values);
                }
                var arr = values.ToArray();
                Array.Sort(arr);
                foreach (DictionaryEntry kv in tree)
                {
                    _replace_array_r(kv.Value as ArrayList, arr);
                }
                tree["values"] = arr;
                Log.Log2File($"打包日志] 生成xml:{jsonpath}, md5:{md5path}");
                File.WriteAllText(jsonpath, MiniJSON.JsonEncode(tree, ""));
                File.WriteAllText(md5path, md5_str);
            }
        }
        else if(false)
        {
            _check_xml(emptypath, jsonpath);
        }
        return emptypath;
    }

    static void _check_xml(string emptypath, string jsonpath) 
    {
        Debug.Log($"check start, {emptypath}");
        var ht = MiniJSON.JsonDecode(File.ReadAllText(jsonpath)) as Hashtable;
        var values = ht.GetArrayList("values");
        foreach (DictionaryEntry kv in ht)
        {
            _recover_array_r(kv.Value as ArrayList, values);
        }
        UnityEditor.AssetDatabase.Refresh();
        var scene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(emptypath);
        var roots = scene.GetRootGameObjects();
        foreach (var r in roots)
        {
            _check_array_r("",r.transform, ht[r.name], 0);
        }
        Debug.Log($"check done, {emptypath}");
    }

    static void _check_array_r(string tab, Transform t, object e, int i)
    {
        if (e is string s)
        {
            if (!t.name.Equals(s, StringComparison.CurrentCultureIgnoreCase))
            {
                if (!t.name.StartsWith(s, StringComparison.CurrentCultureIgnoreCase))
                {
                    Debug.LogError($"{tab}{i} {t.gameObject.GetLocation()} <-> {s}");
                }
                else if (!t.name.StartsWith(s + $" ({i},", StringComparison.CurrentCultureIgnoreCase))
                {
                    Debug.Log($"{tab}{i} {t.gameObject.GetLocation()} <-> {s} ({i},");
                }
            }
        }
        else if (e is ArrayList al)
        {
            if (t.childCount == al.Count)
            {
                var tab2 = tab + "        ";
                for (var n = 0; n < al.Count; ++n)
                {
                    _check_array_r(tab2,t.GetChild(n), al[n], n);
                }
            }
            else
            {
                Debug.LogError($"{i} {t.gameObject.GetLocation()}, {t.childCount} <-> {al.Count}");
            }
        }
    }

    static void _recover_array_r(ArrayList arr, ArrayList values) 
    {
        if (arr is null)
        {
            return;
        }
        for (var i=0;i <arr.Count;++i) 
        {
            var e = arr[i];
            if (e is int n)
            {
                arr[i] = values[n];
            }
            else if (e is ArrayList al) 
            {
                _recover_array_r(al, values);
            }
        }
    }

    static void _collect_array_r(ArrayList arr, HashSet<string> values) 
    {
        if (arr != null) 
        {
            foreach (var o in arr) 
            {
                if (o is string s)
                {
                    values.Add(s);
                }
                else if (o is ArrayList a) 
                {
                    _collect_array_r(a, values);
                }
            }
        }
    }

    static void _replace_array_r(ArrayList arr, string[] values)
    {
        if (arr != null)
        {
            for(int i=0, cnt = arr.Count; i< cnt; ++i)
            {
                var o = arr[i];
                if (o is string s)
                {
                    arr[i] = Array.IndexOf(values, s);
                }
                else if (o is ArrayList a)
                {
                    _replace_array_r(a, values);
                }
            }
        }
    }

    public static void Flush(bool log, bool all)
    {
        var need_variants = false;
        foreach (var _kv in _builderInfo.builderDic)
        {
            foreach (var ab in _kv.Value)
            {
                if (ab.assetBundleName.EndsWith(".mat"))
                {
                    need_variants = true;                    
                    break;
                }
            }
            if (need_variants) 
            {
                break;
            }
        }
        if (need_variants) 
        {
            var variants = Builder_All.CollectAllShaderVariants(false);
            Build(variants);
        }

        Log.Log2File($"打包日志]构建耗时{(DateTime.Now.Ticks - _time_start) / 10000000}秒。");

        _ui_CloseLanguageFile();
        
        AssetDatabase.Refresh();
        //
        var opt = BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.StrictMode;
        //
        var scene_build_names = new HashSet<string>();
        var scenepath = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().path;
        if (_scene_build_paths.Count > 0)
        {
            UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene);
            foreach (var unity in _scene_build_paths)
            {
                scene_build_names.Add(Path.GetFileNameWithoutExtension(unity));
                var empty = _fix_scene(unity, false);
                AddBuild(Path.GetFileNameWithoutExtension(empty) + "_unity.ab", empty, null, null);                
            }
        }        
        
        AssetBundle.UnloadAllAssetBundles(true);
        if (all)
        {
            var dir = PathDefs.EXPORT_ROOT_OS + "xmls";
            var files = Directory.GetFiles(dir);
            foreach (var file in files)
            {
                var name = Path.GetFileNameWithoutExtension(file);
                if (!scene_build_names.Contains(name))
                {
                    Log.Log2File($"打包日志]删除场景配置文件:{file}");
                    File.Delete(file);
                }
            }
        }

        //var shader_names = new Dictionary<string, string>();
        string old_txt = null;
        var depts_sort_list = new List<string>();
        var depts_map = new Dictionary<string, string>();
        var depts_path = PathDefs.EXPORT_PATH_SCENE + "/alldepts.txt";
        if (File.Exists(depts_path)) 
        {
            old_txt = File.ReadAllText(depts_path);
            if (!all)
            {
                var lines = old_txt.Split('\n');
                foreach (var line in lines)
                {
                    var kv = line.Split('=');
                    depts_map[kv[0]] = kv[1];
                }
            }
        }

        var shaders_name = Path.GetFileName(PathDefs.ASSETS_PATH_BUILD_SHADERS);
        string old_txt2 = null;
        var matshadernames = new Dictionary<string, string>();
        var matshadernames_path = PathDefs.EXPORT_PATH_SCENE + "/matshadernames.txt";
        if (File.Exists(matshadernames_path))
        {
            old_txt2 = File.ReadAllText(matshadernames_path);
            if (!all)
            {
                var lines = old_txt2.Split('\n');
                foreach (var line in lines)
                {
                    var kv = line.Split('=');
                    matshadernames[kv[0]] = kv[1];
                }
            }
        }

        foreach (var _kv in _builderInfo.builderDic)
        {
            var Key = _kv.Key;
            var Value = _kv.Value;
            var dir = Path.GetFileName(Key);
            try
            {
                if (Value.Count == 0)
                {
                    continue;
                }
                if (log)
                {
                    var logs = new List<string>();
                    foreach (var b in Value)
                    {
                        logs.Add($"{b.assetBundleName}, names={string.Join(", ", b.addressableNames)}, files={string.Join(", ", b.assetNames)}");
                    }
                    Log.Log2File($"打包日志]will build to {Key}, builds={Value.Count}\n{string.Join("\n", logs)}");
                }

                Dictionary<string, long> output_files = null;
                if (all)
                {
                    var files = Directory.GetFiles(Key);
                    output_files = new Dictionary<string, long>();
                    foreach (var file in files)
                    {
                        output_files.Add(Path.GetFileName(file), File.GetLastWriteTimeUtc(file).Ticks);
                    }
                    //
                    Log.Log2File($"打包日志][{Key}]目录下的文件数量：{output_files.Count}");
                    //以下文件不是打包输出的资源，不能对比删除
                    {
                        //unity 自带依赖
                        output_files.Remove(dir);
                        output_files.Remove(dir + ".manifest");
                        //依赖信息
                        output_files.Remove("alldepts.txt");
                        output_files.Remove("matshadernames.txt");
                        //
                        foreach (var b in scene_build_names)
                        {
                            var xml = b + ".xml";
                            output_files.Remove(xml);
                            //
                            //var scene = b + "_unity.ab";
                            //output_files.Remove(scene);
                            //output_files.Remove(scene + ".meta");
                        }
                    }                    
                }

                Log.Log2File($"打包日志]打包开始，opt={opt} ...");
                var t1 = DateTime.Now.Ticks;
                var mfset = BuildPipeline.BuildAssetBundles(Key, Value.ToArray(), opt, PathDefs.PlatformName); 
                var t2 = DateTime.Now.Ticks;
                if (mfset == null)
                {
                    _log_error("3打包错误！BuildPipeline.BuildAssetBundles = return null!");
                }
                else
                {
                    var Bundles = mfset.GetAllAssetBundles();
                    Log.Log2File($"打包日志]打包结束, 导出数量={Bundles.Length}，耗时{(t2 - t1) / 10000000}秒。开始校验打包结果 ...\n{string.Join("\n", Bundles)}");
                    if (Bundles.Length != Value.Count)
                    {
                        _log_error($"to {Key}, out Bundles={Bundles.Length} != in builds={Value.Count},");
                    }
                    //
                    //var mf_assets = AssetBundle.LoadFromFile(Key + "/" + Path.GetFileName(Key));
                    //var mfs = mf_assets.LoadAllAssets<AssetBundleManifest>();                    
                    //Log.Log2File($"mf_assets={mfs.Length}");                    
                    //                    
                    AssetDatabase.SaveAssets();     // 打包过程中, 可能会修改某些资源(如角色打包时, 会添加碰撞体等), 所以要保存
                    AssetDatabase.Refresh();
                    //
                    foreach (var build in Value)
                    {
                        var gen = Key + "\\" + build.assetBundleName;// + ".manifest";
                        //
                        if (!File.Exists(gen))
                        {
                            _log_error($"results: 没有生成 {gen}");
                        }
                        else
                        {

                            // 检查 addressableNames
                            {                                
                                var asset = AssetBundle.LoadFromFile(gen);
                                var names = asset.GetAllAssetNames();
                                names.InsertionSort(string.Compare);
                                asset.Unload(true);
                                //
                                if (gen.EndsWith("_unity.ab"))
                                {
                                    if (names.Length != 0)
                                    {
                                        _log_error($"results: path={gen}, GetAllAssetNames:{names.Length}={string.Join(",", names)} must empty");
                                    }
                                }
                                else
                                {
                                    var addressableNames = build.addressableNames;
                                    addressableNames.InsertionSort(string.Compare);
                                    if (names.Length != addressableNames.Length)
                                    {
                                        _log_error($"results: path={gen}, GetAllAssetNames:{names.Length}={string.Join(",", names)} != addressableNames:{addressableNames.Length}={string.Join(",", addressableNames)}");
                                    }
                                    else
                                    {
                                        for (var i = 0; i < names.Length; ++i)
                                        {
                                            if (names[i] != addressableNames[i])
                                            {
                                                _log_error($"results: path={gen}, GetAllAssetNames[{i}]:{names[i]} != addressableNames[{i}]:{addressableNames[i]}");
                                            }
                                        }
                                    }
                                }
                            }
                            //手动维护依赖
                            {
                                var assetBundleName = Path.GetFileNameWithoutExtension(build.assetBundleName);
                                var depts = mfset.GetDirectDependencies(build.assetBundleName);
                                if (depts == null)
                                {
                                    _log_error($"results: path={gen}, GetDirectDependencies({build.assetBundleName}) return null");
                                }
                                else
                                {
                                    //Log.Log2File($"{build.assetBundleName} -> depts = {string.Join(",", depts)}");
                                    if (depts.Length == 0)
                                    {
                                        if (!all)
                                        {
                                            depts_map.Remove(assetBundleName);
                                        }
                                    }
                                    else if (!assetBundleName.StartsWith("__tmp_"))
                                    {                                        
                                        depts_sort_list.Clear();
                                        for (var i = 0; i < depts.Length; ++i)
                                        {
                                            var dept = depts[i];
                                            if (!dept.StartsWith("__tmp_") && !dept.EndsWith(shaders_name))
                                            {
                                                depts_sort_list.Add(Path.GetFileNameWithoutExtension(dept));
                                            }
                                        }
                                        depts_sort_list.Sort();
                                        if (gen.EndsWith(".mat"))
                                        {
                                            var mat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(build.assetNames[0]);
                                            matshadernames[mat.name] = mat.shader.name;
                                        }
                                        if (depts_sort_list.Count > 0)
                                        {
                                            depts_map[assetBundleName] = string.Join(",", depts_sort_list);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    //
                    var t3 = DateTime.Now.Ticks;
                    Log.Log2File($"打包日志]校验结束，耗时{(t3 - t2) / 10000000}秒。");
                    //
                    if (all && !_hasError)
                    {
                        Log.Log2File($"打包日志]检测资源更新情况 ...");
                        var adds = new List<string> ();
                        var updates = new List<string>();
                        foreach (var b in Bundles)
                        {
                            if (output_files.TryGetValue(b, out var oldtime))
                            {
                                output_files.Remove(b);
                                output_files.Remove(b + ".manifest");
                                if (File.GetLastWriteTimeUtc(Key + '\\' + b).Ticks != oldtime) 
                                {
                                    updates.Add(b);
                                }
                            }
                            else
                            {
                                adds.Add(b);
                            }
                        }
                        Log.Log2File($"打包日志][新增]资源数量={adds.Count}\n{string.Join("\n", adds)}");
                        Log.Log2File($"打包日志][更新]资源数量={updates.Count}\n{string.Join("\n", updates)}");
                        Log.Log2File($"打包日志][删除]资源数量={output_files.Count}\n{string.Join("\n", output_files)}");
                        if (output_files.Count > 0)
                        {
                            foreach (var b in output_files.Keys)
                            {
                                File.Delete(Key + "/" + b);
                            }
                            var t4 = DateTime.Now.Ticks;
                            Log.Log2File($"打包日志]删除资源完成，耗时{(t4 - t3) / 10000000}秒。");
                        }
                    }
                }
            }
            catch (Exception err)
            {
                _log_error("5打包错误！" + Key + ":" + err.Message + "\n" + err.StackTrace);
            }
        }

        AssetDatabase.Refresh();
        // 输出结果
        if (!_hasError && (_builderInfo.builderDic.Count > 0))
        {
            depts_sort_list.Clear();
            foreach (var kv in depts_map) 
            {
                depts_sort_list.Add( $"{kv.Key}={kv.Value}" );
            }
            depts_sort_list.Sort();
            var txt = string.Join("\n", depts_sort_list);
            if (old_txt != txt) 
            {
                Log.Log2File($"update {depts_path}");
                File.WriteAllText(depts_path, txt);
            }

            depts_sort_list.Clear();
            foreach (var kv in matshadernames)
            {
                depts_sort_list.Add($"{kv.Key}={kv.Value}");
            }
            depts_sort_list.Sort();
            var txt2 = string.Join("\n", depts_sort_list);
            if (old_txt2 != txt2)
            {
                Log.Log2File($"update {matshadernames_path}");
                File.WriteAllText(matshadernames_path, txt2);
            }

            Log.Log2File($"打包日志]打包成功! 场景工程的临时目录{PathDefs.PREFAB_PATH_SCENES_TEMP_EMPTY}记得提交svn");
            ProcessUtils.ExecPython(PathDefs.EXPORT_ROOT + PathDefs.os_name + "/" + "update.py");
        }        
    }
}



