using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Globalization;
using UnityEditor.SceneManagement;
using System.Reflection;
using UnityEngine.UI;
using System.Collections;
using GameSupport.EffectToolBox;

/// <summary>
/// 打包脚本
/// </summary>
static class AssetbundleBuilder
{
    [MenuItem("Scenes/导出UI中文")]
    static void ExportChineseUI() 
    {
        var preg = new System.Text.RegularExpressions.Regex(@"[\u4e00-\u9fa5]", System.Text.RegularExpressions.RegexOptions.Singleline);
        var lines = new List<string>();
        var prefabs = Directory.GetFiles( PathDefs.PREFAB_PATH_GUI_PANEL, "*.prefab", SearchOption.AllDirectories);
        foreach (var prefab in prefabs) 
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(prefab);
            var texts = go.GetComponentsInChildren<MyText>(true);
            foreach (var text in texts) 
            {
                if (!text.SaveToAB && preg.IsMatch(text.text)) 
                {
                    lines.Add($"{text.gameObject.GetLocation()}={text.text.Replace("\n","\\n").Replace("\r", "\\r")}");
                }
            }
        }
        File.WriteAllText("uilang.txt", string.Join("\n",lines));
    }

    [MenuItem("Scenes/设置场景物件材质队列")]
    static void CalcSceneMatsRendererQueue()
    {
        if (!Application.isPlaying)
        {
            throw new Exception("只能在playing模式下使用");
        }

        var __gpu_instance = GameObject.Find("__gpu_instance")?.transform;
        if (!__gpu_instance)
        {
            throw new Exception("没有找到 __gpu_instance 节点");
        }
        Dictionary<Material, Material> _instanceing_mats = new Dictionary<Material, Material>();
        HashSet<Material> _mats = new HashSet<Material>();
        Dictionary<int, int> _queue_sort = new Dictionary<int, int>();

        var rds = GameObject.FindObjectsOfType<Renderer>();
        foreach (var mr in rds)
        {
            var prefab_mat = mr.sharedMaterial;
            if (!prefab_mat) 
            {
                continue;
            }

            Transform root2 = null;
            var root = mr.transform;
            while (root.parent)
            {
                root2 = root;
                root = root.parent;
            }
            if (root2 == __gpu_instance)
            {                
                if (prefab_mat.name.EndsWith("(Clone)"))
                {
                    Log.LogError(mr.gameObject.GetLocation());
                    throw new Exception($"请重新播放后再执行");
                }
                //
                if (!_instanceing_mats.TryGetValue(prefab_mat, out var ins_mat) || !ins_mat || ins_mat.mainTexture != prefab_mat.mainTexture)
                {
                    if (ins_mat)
                    {
                        Log.Log2File($"destroy {ins_mat} and clone again from {prefab_mat}");
                        GameObject.Destroy(ins_mat);
                    }
                    ins_mat = _instanceing_mats[prefab_mat] = GameObject.Instantiate(prefab_mat);
                    ins_mat.enableInstancing = true;
                }
                if (_mats.Add(prefab_mat))
                {
                    int q = prefab_mat.renderQueue;
                    if (_queue_sort.TryGetValue(q, out int add))//不同材质不要用同一个队列
                    {
                        ins_mat.renderQueue += add;
                    }
                    //Log.LogError($"mat={mat}, mat.renderQueue = {mat.renderQueue} = {q} + {add}, mat.enableInstancing={mat.enableInstancing}, enableInstancing={enableInstancing}");
                    _queue_sort[q] = ++add;
                }
                mr.sharedMaterial = ins_mat;
            }
        }
    }

    static void _check_project()
    {
        var project = Path.GetFileName(Path.GetFullPath("."));
        if (project != "scene")
        {
            throw new Exception($"只能在 scene 工程打包资源！");
        }
    }

    public static bool check_mat_mesh(string pathname, GameObject go)
    {
        bool ret = true;
        var rds = go.GetComponentsInChildren<Renderer>();
        foreach (var rd in rds)
        {
            if (rd.enabled)
            {
                if (!rd.sharedMaterial)
                {
                    Log.LogError($"{pathname}, {rd.gameObject.GetLocation()} lost sharedMaterial！");
                    ret = false;
                }

                var skin = rd as SkinnedMeshRenderer;
                if (skin && !skin.sharedMesh)
                {
                    Log.LogError($"{pathname}, {rd.gameObject.GetLocation()} lost sharedMesh！");
                    ret = false;
                }
            }
        }
        var mfs = go.GetComponentsInChildren<MeshFilter>();
        foreach (var mf in mfs)
        {
            if (!mf.sharedMesh)
            {
                Log.LogError($"{pathname}, {mf.gameObject.GetLocation()} lost sharedMesh！");
                ret = false;
            }
        }
        return ret;
    }

    class filecmp
    {
        public string file;
        public int size;
    }

    static List<string> _get_depts(string path, bool all)
    {
        if (path.EndsWith("postprocessresources.asset"))
        {
            return new List<string>();
        }
        //
        var depts = AssetDatabase.GetDependencies(path, all);
        var list = new List<string>();
        foreach (var _ in depts)
        {
            var low = _.ToLower();
            if (low == path || low.EndsWith(".cs"))
            {
                continue;
            }
            //
            if (low.EndsWith(".giparams"))
            {
                //Log.LogInfo($"{path} skip {low}");
            }
            else
            {
                list.Add(low);
            }
        }
        return list;
    }

    static void _set_addressableNames(List<AssetBundleBuild> abs)
    {
        for (var i = 0; i < abs.Count; i++)
        {
            var ab = abs[i];
            if (!ab.assetBundleName.StartsWith("fbxs_with_"))
            {
                var names = ab.assetNames;
                //for (var n=0;n< names.Length;++n) 
                //{
                //    names[n] = new FileInfo(names[n]).FullName; //Path.GetFullPath(names[n]);
                //}
                if (!names[0].EndsWith(".unity"))
                {
                    var addressableNames = new string[names.Length];
                    addressableNames[0] = "main";
                    for (var n = 1; n < names.Length; ++n)
                    {
                        addressableNames[n] = Path.GetFileName(names[n]);
                    }
                    ab.addressableNames = addressableNames;
                }
                else
                {
                    ab.addressableNames = new string[] { };
                }
                abs[i] = ab;
            }
        }
    }
    [MenuItem("Scenes/*检测场景prefab*")]
    static void FixScenePrefabs()
    {
        if (!Application.isPlaying)
        {            
            var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            Log.LogInfo($"{scene.name}, isDirty={scene.isDirty}");
            if (scene.isDirty)
            {
                AssetbundleBuilder.ForceRefresh();
                UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            }
            var path = scene.path.ToLower();
            //UnityEditor.SceneManagement.EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
            Builder_All.fix_scene(path, true);
        }
    }

    //[MenuItem("Scenes/*生成场景xml*")]
    static void GenSceneXML()
    {
        if (!Application.isPlaying)
        {
            var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            Log.LogInfo($"{scene.name}, isDirty={scene.isDirty}");
            if (scene.isDirty)
            {
                AssetbundleBuilder.ForceRefresh();
                UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            }
            Builder_All.fix_scene(scene.path.ToLower(), false);
        }
    }

    //[MenuItem("Scenes/*检测所有场景prefab*")]
    static void FixScenesPrefabs()
    {
        if (!Application.isPlaying)
        {            
            var scene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
            Log.LogInfo($"{scene.name}, isDirty={scene.isDirty}");
            if (scene.isDirty)
            {                
                UnityEditor.EditorUtility.DisplayDialog("错误", $"请先保存当前场景", "知道了");
                return;
            }
            AssetbundleBuilder.ForceRefresh();
            UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();
            UnityEditor.SceneManagement.EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
            var scenes = Directory.GetFiles(PathDefs.ASSETS_PATH_UNITYSECNE, "*.unity", SearchOption.AllDirectories);
            Log.LogInfo($"fix scenes");
            foreach (var s in scenes)
            {
                Builder_All.fix_scene(s.Replace('\\','/').ToLower(), true);
            }
            Log.LogInfo($"gen scenes and xmls");
            foreach (var s in scenes)
            {
                Builder_All.fix_scene(s.Replace('\\', '/').ToLower(), false);
            }
        }
    }

    [MenuItem("Scenes/查找GUID")]
    static void FindFileByGUID()
    {
        //UnityEngine.Object[] array = Resources.FindObjectsOfTypeAll(typeof(EditorWindow));
        var t = Type.GetType("UnityEditor.ProjectBrowser,UnityEditor");
        var w = UnityEditor.EditorWindow.GetWindow(t);
        if (w == null)
        {
            Log.LogError($"没有找到 ProjectBrowser 窗口");
        }
        else
        {
            var f = t.GetField("m_SearchFieldText", BindingFlags.NonPublic | BindingFlags.Instance);
            var guid = f.GetValue(w) as string;
            var path = AssetDatabase.GUIDToAssetPath(guid);
            Log.LogInfo($"guid={guid}");
            Log.LogInfo($"path={path}");
            if (!string.IsNullOrEmpty(path))
            {
                var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                if (obj)
                {
                    EditorGUIUtility.PingObject(obj);
                    f.SetValue(w, guid);
                }
            }
        }
    }



    //[MenuItem("Export/*导出测试贴图*")]
    static void ExportTestsTexs()
    {
        if (!EditorPathUtils.CheckPathSettings()) return;
        var list = new List<string>();
        var rootdir = "Assets\\ui\\asset\\myuiimage\\tests".ToLower().Replace('\\', '/');
        var template = rootdir + "/template.png";
        //var what = -1;    //复制图片
        //var what = 0;       //设置rgba32
        //var what = 1;     //红线
        //var what = 2;     //设置 指定 格式
        var what = 3;     //打包
        var dirs = Directory.GetDirectories(rootdir);
        for (var i = 0; i < dirs.Length; ++i)
        {
            var dir = dirs[i].ToLower().Replace('\\', '/');
            var dirname = Path.GetFileName(dir);
            var max = 70;
            for (var n = 1; n <= max; n++)
            {
                var path = $"{dir}/tex_{dirname}_{n}.png";
                list.Add(path);
                if (what < 3)
                {
                    Log.LogInfo($"path={path}");
                    if (what == -1)
                    {
                        File.Copy(template, path, true);
                    }
                    else
                    {
                        var imp = AssetImporter.GetAtPath(path) as TextureImporter;
                        var android = imp.GetPlatformTextureSettings("android");

                        if (what == 0)
                        {
                            android.format = TextureImporterFormat.RGBA32;
                            android.overridden = true;
                            imp.mipmapEnabled = false;
                            imp.isReadable = true;
                            imp.textureType = TextureImporterType.GUI;
                            imp.npotScale = TextureImporterNPOTScale.ToNearest;
                            imp.SetPlatformTextureSettings(android);
                            imp.SaveAndReimport();
                        }

                        if (what == 1)
                        {
                            var p = (float)n / max;
                            //      
                            var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
                            var tex = obj as Texture2D;
                            var x2 = (int)(tex.width * p);
                            for (var x = 0; x < x2; ++x)
                            {
                                for (var y = -2; y <= 2; ++y)
                                {
                                    tex.SetPixel(x, tex.height / 2 + y, Color.red);
                                }
                            }
                            var bytes = tex.EncodeToPNG();
                            File.WriteAllBytes(path, bytes);
                        }

                        if (what == 2)
                        {
                            var format = TextureImporterFormat.RGBA32;
                            switch (dirname)
                            {
                                case "astc4": format = TextureImporterFormat.ASTC_4x4; break;
                                case "astc8": format = TextureImporterFormat.ASTC_8x8; break;
                                case "astc12": format = TextureImporterFormat.ASTC_12x12; break;
                                case "etc8": format = TextureImporterFormat.ETC2_RGBA8; break;
                                default: break;
                            }
                            android.format = format;
                            imp.mipmapEnabled = false;
                            imp.npotScale = TextureImporterNPOTScale.ToNearest;
                            imp.SetPlatformTextureSettings(android);
                            imp.SaveAndReimport();
                        }
                    }
                }
            }
        }
        AssetDatabase.Refresh();
        if (what == 3)
        {
            list.Add(rootdir + "/UICanvas.prefab".ToLower());
            _ExportSelects(list, false, true);
        }
    }

    //[MenuItem("Export/*清理shader*")]

    public static void DelNoUsedShaders()
    {
        var shaders = Directory.GetFiles("Assets/Resources/shader", "*.shader", SearchOption.AllDirectories);

        var mats = Directory.GetFiles("Assets/", "*.mat", SearchOption.AllDirectories);
        var used_shaders = new HashSet<string>();
        foreach (var mat in mats)
        {
            var m = AssetDatabase.LoadAssetAtPath<Material>(mat);
            if (m)
            {
                used_shaders.Add(AssetDatabase.GetAssetPath(m.shader));
            }
        }

        var keep_shader_names = new string[]
        {
                "Hidden/CameraEffects/ColorSuite",
                "Hidden/CameraEffects/Bloom",
                "Hidden/CameraEffects/Motion/Reconstruction",
                "Hidden/CameraEffects/Motion/FrameBlending",
                "Hidden/CameraEffects/DepthField",
                "Hidden/CPSSSSSShader",
                "ProjectorShadow/ShadowCaster",
                "ProjectorShadow/SampleShadowCaster",
                "one",
                "MyShaders/UI/Default",
                "MyShaders/UI/Default_bg",
                "MyShaders/UI/WareAlpha",
                "MyShaders/others/ImgFadeAlpha",
                "MyShaders/others/ImgFade",
                "MyShaders/others/Gaussian Blur",
        };
        foreach (var name in keep_shader_names)
        {
            var shader = Shader.Find(name);
            var path = AssetDatabase.GetAssetPath(shader);
            if (string.IsNullOrEmpty(path))
            {
                Log.LogError($"shader=[{name}] not found");
            }
            else
            {
                used_shaders.Add(path);
            }
        }

        foreach (var v in used_shaders.ToList())
        {
            var depts = AssetDatabase.GetDependencies(v, true);
            foreach (var dept in depts)
            {
                used_shaders.Add(dept);
            }
        }

        Log.Log2File($"shaders={shaders.Length}, used_shaders={used_shaders.Count}");
        //
        foreach (var win_path in shaders)
        {
            var path = win_path.Replace('\\', '/');
            if (!used_shaders.Contains(path))
            {
                Log.Log2File($"noused shader={path}");
                File.Delete(path);
            }
        }
        AssetDatabase.Refresh();
    }

#if UNITY_EDITOR_WIN
    [MenuItem("Window/打开日志目录")]
    static void OpenLogDir() 
    {
        var p = new System.Diagnostics.Process();
        p.StartInfo.FileName = "explorer.exe";
        p.StartInfo.Arguments = '"' + Path.GetDirectoryName(Application.consoleLogPath) + '"';
        p.Start();
    }
#endif

    //[MenuItem("Export/*测试变体打包*")]

    public static void ExportVarient()
    {
        var a1 = new AssetBundleBuild()
        {
            assetBundleName = "test_varient_panel.ab",
            assetNames = new string[] { "Assets/ui/prefab/myuipanel/test_varient/test_varient_panel.prefab" },
            addressableNames = new string[] { "main" },
        };

        var a2 = new AssetBundleBuild()
        {
            assetBundleName = "10001_pic",
            assetBundleVariant = "cn",
            assetNames = new string[] { "Assets/ui/prefab/myuipanel/test_varient/10001_pic.png" },
            addressableNames = new string[] { "main" },
        };

        var a3 = new AssetBundleBuild()
        {
            assetBundleName = "10001_pic",
            assetBundleVariant = "sgp",
            assetNames = new string[] { "Assets/ui/prefab/myuipanel/test_varient/sgp/10001_pic.png" },
            addressableNames = new string[] { "main" },
        };
        var abs = new AssetBundleBuild[]
        {
            a1,a2,a3
        };

        var output = "E:/long_new/client/assetbundles/android/abs_tmp";
        //if (false)
        {
            var mf = BuildPipeline.BuildAssetBundles(output, abs, BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DisableWriteTypeTree, PathDefs.PlatformName);
            var abs1 = mf.GetAllAssetBundles();
            var abs2 = mf.GetAllAssetBundlesWithVariant();
            foreach (var ab in abs1)
            {
                var depts = mf.GetAllDependencies(ab);
                Log.LogInfo($"1 {ab} -> {string.Join(',', depts)}");
            }
            foreach (var ab in abs2)
            {
                var depts = mf.GetAllDependencies(ab);
                Log.LogInfo($"2 {ab} -> {string.Join(',', depts)}");
            }
        }

        AssetBundle.UnloadAllAssetBundles(true);

        {
            var ab2 = AssetBundle.LoadFromFile(output + "/" + a2.assetBundleName + '.' + a2.assetBundleVariant);
            Log.LogInfo($"ab2={ab2}");
            ab2.LoadAllAssets();
            

            var ab1 = AssetBundle.LoadFromFile(output + "/" + a1.assetBundleName + '.' + a1.assetBundleVariant);
            Log.LogInfo($"ab1={ab1}");
            Object.Instantiate(ab1.LoadAsset("main"));

            ab1.Unload(false);
            ab2.Unload(false);
        }

        {
            var ab3 = AssetBundle.LoadFromFile(output + "/" + a3.assetBundleName + '.' + a3.assetBundleVariant);
            Log.LogInfo($"ab3={ab3}");
            ab3.LoadAllAssets();

            var ab1 = AssetBundle.LoadFromFile(output + "/" + a1.assetBundleName + '.' + a1.assetBundleVariant);
            Log.LogInfo($"ab1={ab1}");
            Object.Instantiate(ab1.LoadAsset("main"));

            ab1.Unload(false);
            ab3.Unload(false);
        }
    }

    //[MenuItem("Export/*打开所有场景*")]
    public static void OpenAllScenes()
    {
        var unitys = System.IO.Directory.GetFiles(PathDefs.ASSETS_PATH_UNITYSECNE, "*.unity", SearchOption.AllDirectories);
        File.WriteAllLines("tmp_unitys.txt", unitys);
        ForceRefresh();
    }


    [MenuItem("Export/*2生成 shader变体*")]
    public static void CollectAllShaderVariants()
    {
        ForceRefresh();
        Builder_All.CollectVariantsFromBat1();
    }

    [MenuItem("Export/*3打包 shader变体*")]
    public static void ExportAllShaderVariants()
    {
        if (!EditorPathUtils.CheckPathSettings()) return;

        AssetbundleBuilderNew.export_variants = true;
        AssetbundleBuilderNew.export_variants = false;
        File.Copy("assets/collect_shaders/full_shader_variants.asset", PathDefs.ASSETS_PATH_BUILD_SHADERS, true);
        _ExportSelects(new List<string>() { PathDefs.ASSETS_PATH_BUILD_SHADERS }, false, true);
    }

    [MenuItem("Export/*检查特效材质*")]
    public static void CheckFxMaterial()
    {
        var paths = Directory.GetFiles("Assets/Fx", "*.prefab", SearchOption.AllDirectories);
        foreach (var pathname in paths)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(pathname);
            check_mat_mesh(pathname, go);
        }
    }

    [MenuItem("Export/*检查字体*")]
    public static void CheckTextFont()
    {
        var paths = Directory.GetFiles("Assets/ui/prefab/myuipanel", "*.prefab", SearchOption.AllDirectories);
        foreach (var pathname in paths)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(pathname);
            var txts = go.GetComponentsInChildren<Text>();
            foreach (var txt in txts)
            {
                var font = txt.font;
                if (!font)
                {
                    Log.LogError($"{pathname}, {txt.gameObject.GetLocation()}.{txt.GetType().Name} lost font");
                }
                else
                {
                    var fontpath = AssetDatabase.GetAssetPath(font).ToLower();
                    if (!PathDefs.IsAssetsResources(fontpath))
                    {
                        Log.LogError($"{pathname}, {txt.gameObject.GetLocation()}.{txt.GetType().Name}, error fontpath={fontpath}");
                        var filename = Path.GetFileName(fontpath).ToLower();
                        var resource_font = $"Assets\\Resources\\fonts\\" + filename;
                        if (!File.Exists(fontpath))
                        {
                            resource_font = $"Assets\\Resources\\fonts\\arial.otf";
                        }
                        if (File.Exists(resource_font))
                        {
                            Log.LogError($"Fix font as {resource_font}");
                            resource_font = resource_font.Replace("Assets\\Resources\\", "").Replace(".otf", "");
                            txt.font = Resources.Load<Font>(resource_font);
                            PrefabUtility.SavePrefabAsset(go);
                        }
                    }
                }
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"CheckTextFont done");
    }


    [MenuItem("Export/*检查所有Prefabs单图*")]

    static void FixUIPrefabs()
    {
        if (!Application.isPlaying)
        {
            if (!EditorPathUtils.CheckPathSettings()) return;
            var prefabs = Directory.GetFiles(PathDefs.PREFAB_PATH_GUI_PANEL, "*.prefab", SearchOption.AllDirectories);
            Log.Log2File($"prefabs={prefabs.Length}");
            Builder_All.fix_all_ui_text(prefabs, true);
            Log.Log2File("done");
        }
    }
    [MenuItem("Assets/*检查Prefab单图*")]

    static void FixUIPrefab()
    {
        if (!Application.isPlaying)
        {
            if (!EditorPathUtils.CheckPathSettings()) return;
            var select = AssetDatabase.GetAssetPath(Selection.activeObject).ToLower();
            Log.Log2File($"prefab={select}");
            if (select.StartsWith(PathDefs.PREFAB_PATH_GUI_PANEL))
            {
                Builder_All.fix_all_ui_text(new string[] { select }, true);
                Log.Log2File("done");
            }
        }
    }


    [MenuItem("Export/*检查贴图配置*")]
    static void CheckPngSize()
    {
        var paths = new string[] { "assets/scene/", "assets/other_assets/", "assets/fx/", "assets/actor/assets/", "assets/ui/asset/myuiimage/", "assets/ui/prefab/myuipanel/", "assets/ui/prefab/mypacker/" };
        var pats = new string[] { "*.png", "*.exr" };
        int i = 0;
        foreach (var path in paths)
        {
            ++i;
            foreach (var p in pats)
            {
                var files = Directory.GetFiles(path, p, SearchOption.AllDirectories);
                foreach (var wf in files)
                {
                    var f = wf.ToLower();
                    var teximport = AssetImporter.GetAtPath(f) as TextureImporter;
                    if (teximport)
                    {
                        var dirty = false;
                        var android = teximport.GetPlatformTextureSettings("android");
                        var maxTextureSize = android.maxTextureSize;
                        var format = android.format;
                        var textureCompression = android.textureCompression;
                        if (i < 5 && maxTextureSize > 1024 && !f.Contains("_t4m."))
                        {
                            var tex = AssetDatabase.LoadAssetAtPath<Texture>(f);
                            if (tex.width > 1024 || tex.height > 1024)
                            {
                                dirty = true;
                                android.maxTextureSize = 1024;
                            }
                        }
                        TextureImporterFormat best;
                        //var best = (teximport.textureType == TextureImporterType.NormalMap || f.Contains("_t4m.") || f.Contains("tex_ui")) ? TextureImporterFormat.ASTC_4x4 : ( i >= 5 ? TextureImporterFormat.ASTC_5x5 : ((f.Contains("lightmap-") || i >= 3) ? TextureImporterFormat.ASTC_6x6 : TextureImporterFormat.ASTC_8x8));
                        if (teximport.textureType == TextureImporterType.NormalMap || f.Contains("_t4m.") || f.Contains("_skybox"))
                        {
                            //法线、t4m
                            best = TextureImporterFormat.ASTC_4x4;
                        }
                        else if (i >= 5 || f.Contains("tex_ui"))
                        {
                            //UI
                            best = TextureImporterFormat.ASTC_5x5;
                            if ( i < 7 && maxTextureSize >= 2048) 
                            {
                                var tex = AssetDatabase.LoadAssetAtPath<Texture>(f);
                                if (tex.width >= 2048 || tex.height >= 2048)
                                {
                                    best = TextureImporterFormat.ASTC_6x6;
                                }
                            }
                        }
                        else if (i >= 3 || f.Contains("lightmap-"))
                        {
                            //角色、光照贴图
                            best = TextureImporterFormat.ASTC_6x6;
                        }
                        else 
                        {
                            //场景
                            best = TextureImporterFormat.ASTC_8x8;
                        }

                        if (!(format >= best && format <= TextureImporterFormat.ASTC_12x12))
                        {
                            dirty = true;
                            android.format = best;
                        }
                        if (textureCompression == TextureImporterCompression.Uncompressed)
                        {
                            dirty = true;
                            android.textureCompression = TextureImporterCompression.Compressed;
                        }
                        if (dirty)
                        {
                            Log.LogInfo($"{i} up {maxTextureSize}->{android.maxTextureSize}, {format}->{android.format}, {f}");
                            teximport.SetPlatformTextureSettings(android);
                            teximport.SaveAndReimport(); 
                        }
                    }
                }
            }
        }
    }


    [MenuItem("Export/**  更新全部图集  **")]
    static void GenAllPackers()
    {
        //生成 图集
        AssetDatabase.Refresh();
        var arr_atlas = Directory.GetDirectories(PathDefs.ASSETS_PATH_GUI_SPRITES);
        foreach (var atlas in arr_atlas)
        {
            Builder_All.GenPacker(atlas, true);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    //[MenuItem("Export/**  更新prefab到 2021.3.18  **")]
    static void UpgradePrefabTo2020()
    {
        AssetDatabase.Refresh();
        var arr_atlas = Directory.GetFiles("assets/ui/prefab/myuipanel/team", "*team_invite_main_tips_panel.prefab", SearchOption.AllDirectories);
        Log.Log2File($"arr_atlas={arr_atlas.Length}");
        foreach (var atlas in arr_atlas)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(atlas);
            //AssetDatabase.OpenAsset(go);
            EditorUtility.SetDirty(go);
            PrefabUtility.SavePrefabAsset(go);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Log.Log2File($"done");
    }

    [MenuItem("Export/**  优化导出  特效  **")]
    public static void FixFxMotionVectors()
    {
        AssetDatabase.Refresh();
        var fxs = Directory.GetFiles(PathDefs.PREFAB_PATH_COMPLEX_OBJECT, "*.prefab", SearchOption.AllDirectories);
        if (false)
        {
            foreach (var fx in fxs)
            {
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(fx);
                ArtToolsEffectChecker.BatchOptimizationRes(go);
                PrefabUtility.SavePrefabAsset(go);
            }
            AssetDatabase.SaveAssets();
        }
        else
        {
            if (!EditorPathUtils.CheckPathSettings()) return;
            _ExportSelects(new List<string>(fxs), false);
        }
    }


    [MenuItem("Export/**  资源一键打包  **")]
    public static void ExportAll()
    {

        if (!EditorPathUtils.CheckPathSettings()) return;
        var build_assetbundle_bat = BuilderConfig.build_assetbundle_bat;

        ConsoleUtils.CleanConsole();
        _check_project();
        AssetDatabase.Refresh();

        var t1 = DateTime.Now.Ticks;
        Log.Log2File($"打包日志]一键打包开始, 收集资源文件列表 ...");

        var Searchs = new Dictionary<string, string[]>();
        //ui
        {
            //打包 timeline
            Searchs.Add(PathDefs.ASSETS_PATH_ASSETDATA, new string[] { "*.asset" });

            //打包 单图
            Searchs.Add(PathDefs.ASSETS_PATH_GUI_IMAGES, new string[] { "*.png", "*.jpeg" });
            Searchs.Add(PathDefs.ASSETS_PATH_COMMTEX, new string[] { "*.png", "*.jpeg" });
            Searchs.Add(PathDefs.ASSETS_PATH_COMMALONE, new string[] { "*.*" });

            //打包 图集
            Searchs.Add(PathDefs.PREFAB_PATH_UI_PACKERS, new string[] { "*.prefab" });

            //打包 panel
            Searchs.Add(PathDefs.PREFAB_PATH_GUI_PANEL, new string[] { "*.prefab" });
        }

        //fx
        {
            Searchs.Add(PathDefs.PREFAB_PATH_COMPLEX_OBJECT, new string[] { "*.prefab" });
            Searchs.Add(PathDefs.PREFAB_PATH_COMPLEX_OBJECT + "mats/", new string[] { "*.mat" });
        }

        //actor
        {
            Searchs.Add(PathDefs.PREFAB_PATH_CHARACTER, new string[] { "*.prefab" });
            Searchs.Add(PathDefs.ASSETS_PATH_CHARACTER, new string[] { "*.prefab", "*_sxt.mat", "*_hair_d.png" });
        }

        //scene
        {
            Searchs.Add(PathDefs.ASSETS_PATH_UNITYSECNE, new string[] { "*.unity" });
            Searchs.Add(PathDefs.ASSETS_PATH_TRIGGERDATA, new string[] { "*.prefab" });
            //Searchs.Add(PathDefs.ASSETS_PATH_SCENEOBJS, new string[] { "*.prefab" });
            Searchs.Add(PathDefs.ASSETS_PATH_SOUND, new string[] { "*.wav", "*.mp3", "*.ogg", "*.mp4" });
            Searchs.Add(PathDefs.ASSETS_PATH_VCAMS, new string[] { "*.prefab" });
        }

        //
        {
            Searchs.Add(PathDefs.ASSETS_PATH_OTHERASSETS, new string[] { "*.prefab", "*.anim" });
        }

        //
        var list = new List<string>();
        list.Add(PathDefs.ASSETS_PATH_BUILD_SHADERS);//变体
        foreach (var kv in Searchs)
        {
            if (Directory.Exists(kv.Key))
            {
                var is_images = kv.Key == PathDefs.ASSETS_PATH_GUI_IMAGES;
                var exclude = kv.Key + "test\\";
                var is_panel = kv.Key == PathDefs.PREFAB_PATH_GUI_PANEL;

                foreach (var p in kv.Value)
                {
                    var files = Directory.GetFiles(kv.Key, p, SearchOption.AllDirectories);
                    Log.Log2File($"打包日志]{kv.Key}{p}, files={files?.Length}");
                    if (files != null && files.Length > 0)
                    {                        
                        var is_other_ani = kv.Key == PathDefs.ASSETS_PATH_OTHERASSETS && p == "*.anim";
                        foreach (var f in files)
                        {
                            if (is_other_ani)
                            {
                                var ani = Path.GetDirectoryName(f);
                                var dirdir_name = Path.GetFileName(Path.GetDirectoryName(ani));
                                if (!Path.GetFileName(f).StartsWith(dirdir_name)) 
                                {
                                    continue;
                                }
                            }
                            else if (is_images)
                            {
                                if (f.StartsWith(exclude))
                                {
                                    continue;
                                }
                            }

                            if (is_panel && false) 
                            {                                
                                AssetDatabase.ImportAsset(f, ImportAssetOptions.ForceUpdate);
                                var panel = AssetDatabase.LoadAssetAtPath<GameObject>(f);
                                EditorUtility.SetDirty(panel);
                                PrefabUtility.SavePrefabAsset(panel);
                            }
                            //
                            var filename = Path.GetFileNameWithoutExtension(f);
                            if (pathreg.IsMatch(filename))
                            {
                                list.Add(f.ToLower().Replace('\\','/'));
                            }
                        }
                    }
                }
            }
            else
            {
                Log.LogError($"打包日志]{kv.Key} 目录不存在! ");
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        var t2 = DateTime.Now.Ticks;
        Log.Log2File($"打包日志]完成收集资源文件列表，资源数量：{list.Count}，耗时:{(t2 - t1) / 10000000}秒。");
        ForceRefresh();
        //return;
        if (build_assetbundle_bat)
        {
            _ExportSelects(list, true);
        }
        else
        {
            BuildByList(list, true, true);
        }
    }

    public static void ExportAll_Scenes() 
    {
        if (!EditorPathUtils.CheckPathSettings()) return;
        var files = Directory.GetFiles(PathDefs.ASSETS_PATH_UNITYSECNE , "*.unity", SearchOption.AllDirectories);
        _ExportSelects(new List<string>(files), false);
    }

    static System.Text.RegularExpressions.Regex pathreg = new System.Text.RegularExpressions.Regex("^[_0-9a-zA-Z-]+$");
    static System.Text.RegularExpressions.Regex pathreg2 = new System.Text.RegularExpressions.Regex("([_0-9a-zA-Z-]+)");
    static string get_ab_filename(string path)
    {
        throw new Exception("方法过期");

        var filename = Path.GetFileNameWithoutExtension(path);
        if (!pathreg.IsMatch(filename))
        {
            var Matches = pathreg2.Matches(filename);
            var arr = new string[Matches.Count];
            for (int i = 0; i < Matches.Count; ++i)
            {
                arr[i] = Matches[i].Value;
            }
            filename = "__tmp_" + string.Join("_", arr) + "_" + StringUtils.md5(filename).ToLower();
            Log.LogError($"非法的资源名[{path}], 导出资源名字修正为[{filename}]");
        }
        if (path.EndsWith("postprocessresources.asset"))
        {
            return "postprocessresources.postasset";
        }

        var ext = Path.GetExtension(path);
        if (PathDefs.IsAssetsResources(path))
        {
            if (ext != ".shader")
            {
                filename = "__tmp_res_" + path.Substring(0, path.LastIndexOf('.')).Replace('/', '$');
            }
        }

        if (ext == ".unity")
        {
            return filename + "_unity.ab";
        }

        if (ext == ".playable")
        {
            return filename + "_playable.playable";
        }

        if (path.EndsWith(".fbx")) 
        {
            return filename + ".fab";
        }

        var obj = AssetDatabase.LoadAssetAtPath<Object>(path);
        if (obj is GameObject)
        {
            return filename + ".ab";
        }

        if (ext == ".shader") 
        {
            filename = obj.name.Replace(' ','_').Replace('/','_');
            return filename + ext;
        }

        if (obj is Texture)
        {
            if (scene_depts.Contains(path))
            {
                filename = Path.GetFileName(Path.GetDirectoryName(path)) + "-" + filename;
            }
            return filename + ".tex";
        }

        if (obj is AnimationClip)
        {
            var dir = Path.GetDirectoryName(path);
            var dirname = Path.GetFileName(dir);
            if (dirname == "ani")
            {
                dirname = Path.GetFileName(Path.GetDirectoryName(dir));
            }else if (dirname.Contains("ani"))
            {
                dirname = Path.GetFileName(Path.GetDirectoryName(dir)) + "_" + dirname + "_";
            }
            if (!filename.Contains(dirname))
            {
                filename = dirname + '_' + filename;
            }
        }

        if (ext == ".asset")
        {
            ext = ".ab";
        }
        return filename + ext;
    }






    static HashSet<string> scene_depts = new HashSet<string>();
    public static void _ExportSelects(List<string> list, bool all, bool force = false)
    {

        AssetbundleBuilderNew._Export(list, all, force);
        return;

        var langs = new string[] { "--lsgp" };

        var curscene = EditorSceneManager.GetActiveScene();
        if (curscene.path.ToLower().StartsWith("assets/skill/"))
        {
            Log.LogError($"不能在timeline场景下导出资源");
            return;
        }

        if (list.Count == 0) 
        {
            return;
        }

        if (all) 
        {
            ProcessUtils.ExecSystemComm("svn revert " + PathDefs.EXPORT_ROOT + PathDefs.os_name + " -R");
        }
        {
            var ret = ProcessUtils.ExecSystemComm("svn up " + PathDefs.EXPORT_ROOT + PathDefs.os_name + " --accept theirs-full");
            Log.Log2File($"svn up done, ret={ret}");
        }

        var shaders_path = PathDefs.ASSETS_PATH_BUILD_SHADERS;
        if (!all)
        {
            if (list.Count != 1 || list[0] != shaders_path)
            {
                File.Copy("assets/collect_shaders/empty_shader_variants.asset", shaders_path, true);
            }
        }
        else 
        {
            File.Copy("assets/collect_shaders/full_shader_variants.asset", shaders_path, true);
        }
        //File.Copy("assets/collect_shaders/full_shader_variants.asset", shaders_path, true); 
        var shaders_dir = Path.GetDirectoryName(shaders_path).Replace('\\', '/');
        var shaders_name = Path.GetFileName(shaders_path);
        AssetDatabase.Refresh();

        scene_depts = new HashSet<string>();

        var t1 = DateTime.Now.Ticks;

        var unitys = new List<string>();
        var panels = new List<string>();
        //var panels_names = new HashSet<string>();

        for (var i = list.Count - 1; i >= 0; --i)
        {
            var file = list[i] = list[i].ToLower().Replace('\\', '/');
            if (file.StartsWith(PathDefs.ASSETS_PATH_UNITYSECNE) && file.EndsWith(".unity"))
            {
                unitys.Add(file);
                list.swap_tail_and_fast_remove(i);
            }
            else if (file.StartsWith(PathDefs.PREFAB_PATH_GUI_PANEL) && file.EndsWith(".prefab"))
            {
                panels.Add(file);
                //panels_names.Add(Path.GetFileNameWithoutExtension(file));
            }
        }

        var t2 = DateTime.Now.Ticks;
        Log.Log2File($"打包日志]资源分类，资源数量：list={list.Count}，panels={panels.Count}, unitys={unitys.Count}, 耗时:{(t2 - t1) / 10000000}秒。");
        long t3 = 0;


        if (unitys.Count > 0)
        {
            UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.EmptyScene);
        }

        var output = PathDefs.EXPORT_ROOT_OS + "abs";

        //var fbxs_depts = new Dictionary<string, Tuple<string, List<string>>>();
        var deptpath = output + "/" + "alldepts.txt";
        var xalldepts = new Dictionary<string, string[]>();

        var mattexnames_path = output + "/" + "mattexnames.txt";
        var mattexnames = new Dictionary<string, string>();

        var all_depts = new Dictionary<string, List<string>>();
        var depted_cnt = new Dictionary<string, List<string>>();
        var in_main_ab = new Dictionary<string, string>();

        if (!all)
        {
            if (File.Exists(deptpath))
            {
                var lines = File.ReadAllLines(deptpath);
                foreach (var line in lines)
                {
                    //ab = dept1,dept2,dept3|name2,name3,name4
                    var arr = line.Split('=');
                    xalldepts[arr[0]] = arr[1].Split('|');
                }

                //var dirtys = new Dictionary<string, string[]>();
                foreach (var kv in xalldepts)
                {
                    if (kv.Value.Length > 1)
                    {
                        var dirty = false;
                        var arr3 = kv.Value[1].Split(',');
                        for (int i = 0; i < arr3.Length; ++i)
                        {
                            var name = arr3[i];
                            if (xalldepts.ContainsKey(name) || in_main_ab.ContainsKey(name))
                            {
                                dirty = true;
                                arr3[i] = null;
                            }
                            else
                            {
                                in_main_ab[name] = kv.Key;
                            }
                        }
                        if (dirty)
                        {
                            kv.Value[1] = string.Join(',', from s in arr3 where s != null select s);
                        }
                    }
                }
            }

            if (File.Exists(mattexnames_path))
            {
                var lines = File.ReadAllLines(mattexnames_path);
                foreach (var line in lines)
                {
                    var arr = line.Split('=');
                    mattexnames[arr[0]] = arr[1];
                }
            }
            t3 = DateTime.Now.Ticks;
            Log.Log2File($"打包日志]加载依赖文件，耗时:{(t3 - t2) / 10000000}秒。");
        }

        //HashSet<string> in_empty_scene = new HashSet<string>();
        try
        {

            if (unitys.Count > 0)
            {                
                foreach (var scene in unitys)
                {
                    var json_path = PathDefs.PREFAB_PATH_SCENES_TEMP_EMPTY + Path.GetFileNameWithoutExtension(scene) + ".json";
                    var export_json = PathDefs.EXPORT_PATH_SCENE + Path.GetFileNameWithoutExtension(scene) + ".json";
                    if (force)
                    {
                        if (File.Exists(json_path))
                        {
                            File.Delete(json_path);
                        }
                    }
                    var empty = Builder_All.fix_scene(scene, false);
                    if (File.Exists(json_path))
                    {
                        File.Copy(json_path, export_json, true);
                    }
                    //将prefab 提取出来，用于流逝加载
                    var assetNames = _get_depts(empty, false);
                    foreach (var file in assetNames)
                    {
                        //Log.LogInfo(file);                        
                        if (file.EndsWith("lightingdata.asset"))
                        {
                            var lightmaps = _get_depts(file, false);
                            foreach (var lightmap in lightmaps)
                            {
                                //var filename = Path.GetFileName(lightmap);
                                if (AssetDatabase.LoadAssetAtPath<Texture>(lightmap))
                                {
                                    list.Add(lightmap);
                                    scene_depts.Add(lightmap);
                                }
                            }
                        }
                        else if (file.EndsWith(".lighting"))
                        {
                            //Log.LogInfo($"{empty} skip {file}");
                        }
                        else
                        {
                            if (!file.EndsWith(".prefab") && !file.Contains("occlusion_") && !AssetDatabase.LoadAssetAtPath<Texture>(file))
                            {
                                Log.LogError($"场景依赖[{file}]不是prefab, in {empty}");
                            }
                            list.Add(file);
                            scene_depts.Add(file);
                        }
                    }
                    all_depts[empty] = new List<string>();
                    depted_cnt[empty] = new List<string>();
                    //throw new Exception("test");
                }

                UnityEditor.SceneManagement.EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
                AssetDatabase.Refresh();

                var t4 = DateTime.Now.Ticks;
                Log.Log2File($"打包日志]分析场景依赖，耗时:{(t4 - t3) / 10000000}秒。");
                //return;
            }

            if (panels.Count > 0)
            {
                //
                var t4 = DateTime.Now.Ticks;
                Builder_All.fix_all_ui_text(panels.ToArray(), false);
                var t50 = DateTime.Now.Ticks;
                Log.Log2File($"打包日志]分析ui，耗时:{(t50 - t4) / 10000000}秒。");
                //return;
            }



            if (!all)
            {
                //foreach (var file in list)
                for(var i=list.Count-1;i>=0;--i)
                {
                    var file = list[i];
                    var is_prefab = file.EndsWith(".prefab");
                    if (!scene_depts.Contains(file))
                    {
                        var gen = get_ab_filename(file);
                        if (!File.Exists(output + '/' + gen))
                        {
                            if (is_prefab)
                            {
                                if (file.StartsWith(PathDefs.ASSETS_PATH_UNITYSECNE) || file.StartsWith(PathDefs.ASSETS_PATH_SCENE_ASSETS))
                                {
                                    throw new Exception($"{file} 不能选中导出，请选择场景文件导出！");
                                }
                            }
                            else if (!file.StartsWith(PathDefs.ASSETS_PATH_ASSETDATA) && !file.StartsWith(PathDefs.ASSETS_PATH_COMMALONE))
                            {
                                if (!force && !File.Exists(output + '/' + gen))
                                {
                                    Log.Log2File($"{file} 无法选中导出，未找到[{output}/{gen}]，请使用【一键导出】或者选择依赖它的资源文件");
                                    list.swap_tail_and_fast_remove(i);
                                    continue;
                                }
                            }
                        }
                    }
                    //
                    if (is_prefab)
                    {
                        depted_cnt[file] = new List<string>() { };
                    }
                    else
                    {
                        depted_cnt[file] = new List<string>() { "", "" };
                    }

                    var name = Path.GetFileNameWithoutExtension(file);
                    xalldepts.Remove(name);
                }
            }
            else
            {
                foreach (var file in list)
                {
                    depted_cnt[file] = new List<string>();
                }
            }

            var export_pngs = new HashSet<string>();
            bool export_vcs = false; 
            bool export_shader = false;
            var need_shader = unitys.Count > 0;
            var t5 = DateTime.Now.Ticks;
            var stack = new Stack<string>(list.Distinct()); 
            Log.Log2File($"打包日志] stack={stack.Count}");
            while (stack.Count > 0)
            {
                var file = stack.Pop();
                if (!need_shader && file.EndsWith(".shader"))
                {
                    need_shader = true;
                }
                if (!all_depts.ContainsKey(file))
                {
                    var depts = _get_depts(file, false);
                    all_depts[file] = depts;
                    foreach (var dept in depts)
                    {
                        if (!depted_cnt.TryGetValue(dept, out var cnt))
                        {
                            depted_cnt[dept] = cnt = new List<string>();
                        }
                        cnt.Add(file);
                        if (dept.EndsWith(".shader"))
                        {
                            need_shader = true;
                            if (export_shader) 
                            {
                                stack.Push(dept);
                            }
                        }
                        else
                        {
                            stack.Push(dept);
                        }
                    }
                    
                    if (!export_pngs.Contains(file)) 
                    {
                        var tex = AssetDatabase.LoadAssetAtPath<Texture>(file);
                        if (tex) 
                        {
                            var ext = Path.GetExtension(file);
                            var sgppath = file.Replace(ext, "--lsgp" + ext);
                            if (File.Exists(sgppath)) 
                            {
                                export_pngs.Add(file);
                                //
                                export_pngs.Add(sgppath);
                                if (!all_depts.ContainsKey(sgppath)) 
                                {
                                    stack.Push(sgppath);
                                    if (!depted_cnt.ContainsKey(sgppath)) 
                                    {
                                        depted_cnt[sgppath] = new List<string>();
                                    }
                                }
                                //
                                for (var i = 1; i < langs.Length; ++i) 
                                {
                                    sgppath = file.Replace(ext, langs[i] + ext);
                                    if (File.Exists(sgppath)) 
                                    {
                                        export_pngs.Add(sgppath);
                                        if (!all_depts.ContainsKey(sgppath))
                                        {
                                            stack.Push(sgppath);
                                            if (!depted_cnt.ContainsKey(sgppath))
                                            {
                                                depted_cnt[sgppath] = new List<string>();
                                            }
                                        }
                                    }
                                }
                            }
                        }                        
                    }
                }
            }

            if (all || need_shader)
            {
                if (!all_depts.ContainsKey(shaders_path))
                {
                    list.Add(shaders_path);
                    all_depts[shaders_path] = _get_depts(shaders_path, false);
                    depted_cnt[shaders_path] = new List<string>();
                }
            }

            var t6 = DateTime.Now.Ticks;
            Log.Log2File($"打包日志]依赖分析，耗时:{(t6 - t5) / 10000000}秒。all_depts={all_depts.Count}, depted_cnt={depted_cnt.Count}");

            var equip_icon = PathDefs.ASSETS_PATH_GUI_IMAGES + "icon/equip_icon/";
            var item_icon = PathDefs.ASSETS_PATH_GUI_IMAGES + "icon/item_icon/";

            //bool export_vcs = false;
            //var add_texs = new List<string>();
            int tex_cnt = 0;
            var dels = new List<string>();
            foreach (var kv in all_depts)
            {
                var file = kv.Key;
                //if (file.EndsWith(".mat"))
                //{
                //    var mat = AssetDatabase.LoadAssetAtPath<Material>(file);
                //    matshadernames[mat.name] = mat.shader.name;
                //}
                if (!depted_cnt.ContainsKey(file))
                {
                    Log.LogError("depted_cnt not found " + file);
                }

                if (file.EndsWith(".mat"))
                {
                    var arr_tex_names = new List<string>();
                    var merge = true;
                    var mat = AssetDatabase.LoadAssetAtPath<Material>(file);
                    var tex_names = mat.GetTexturePropertyNames();
                    foreach (var tex_name in tex_names)
                    {
                        var tex = mat.GetTexture(tex_name);
                        if (tex)
                        {
                            var texpath = AssetDatabase.GetAssetPath(tex).ToLower();
                            if (!string.IsNullOrEmpty(texpath))
                            {
                                var ext = Path.GetExtension(texpath);
                                var sgppath = texpath.Replace(ext, "--lsgp" + ext);
                                if (File.Exists(sgppath))
                                {
                                    arr_tex_names.Add(tex_name + ':' + tex.name.ToLower());
                                    merge = false;
                                }
                            }
                        }
                    }
                    if (arr_tex_names.Count > 0)
                    {
                        arr_tex_names.Sort();
                        mattexnames[mat.name.ToLower()] = string.Join(',', arr_tex_names);
                    }
                    if (!merge)
                    {
                        continue;
                    }
                }

                if (depted_cnt[file].Count != 1)
                {
                    continue;
                }

                if (file.EndsWith(".prefab") || scene_depts.Contains(file) || export_pngs.Contains(file))
                {
                    continue;
                }

                if (file.StartsWith(equip_icon) || file.StartsWith(item_icon))
                {
                    continue;
                }
                if (file.EndsWith(".shader"))
                {
                    continue;
                }

                if (export_vcs && file.EndsWith("__vcs.asset")) 
                {
                    continue;
                }

                if (!all)
                {
                    if (File.Exists(output + '/' + get_ab_filename(file)))
                    {
                        continue;
                    }
                }

                if (!file.StartsWith(PathDefs.PREFAB_PATH_UI_PACKERS))
                {
                    var ext = Path.GetExtension(file);
                    var sgppath = file.Replace(ext, "--lsgp" + ext);
                    if (File.Exists(sgppath))
                    {
                        continue;
                    }
                }
                dels.Add(file);
            }
            foreach (var del in dels)
            {
                all_depts.Remove(del);
            }

            var t7 = DateTime.Now.Ticks;
            Log.Log2File($"打包日志]分包分析，耗时:{(t7 - t6) / 10000000}秒。dels={dels.Count}, tex_cnt={tex_cnt}");

            //bool build_scene_fbx_mesh_twice = true;
            List<AssetBundleBuild> abs_all = new List<AssetBundleBuild>();
            List<AssetBundleBuild> abs_fbxs = new List<AssetBundleBuild>();
            //Dictionary<string, List<string>> merge_prefabs = new Dictionary<string, List<string>>();
            //List<string> has_multy_fbxs = new List<string>();
            //bool merge_all_fbx = true;
            //var all_shaders = new HashSet<string>();
            
            var fbxs = new Dictionary<string, HashSet<string>>();            
            foreach (var kv in all_depts)
            {
                var file = kv.Key;
                if (!export_shader && file.EndsWith(".shader"))
                {
                    //特殊处理 shader 打进 一个复合包
                    //all_shaders.Add(file);
                    continue;
                }

                if (!export_vcs)
                {
                    if (file.EndsWith("__vcs.asset"))
                    {
                        //特殊处理 vcs 打进 一个复合包
                        continue;
                    }
                }

                if (file.EndsWith(".fbx"))
                {
                    if (kv.Value.Count > 0)
                    {
                        Log.LogError($"error fbx={file}, depts={kv.Value.Count}={string.Join(",", kv.Value)}");
                    }
                    continue;
                }

                //var build_delay = false;
                var depts = kv.Value;
                //特殊处理 shader 打成一个复合包
                //if (file != shaders_path || export_vcs || export_shader) 
                {
                    int fbx_dept_cnt = 0;//多个 prefab 引用多个 fbx，复合包可能会很大
                    //for (var i = depts.Count - 1; i >= 0; --i)
                    //{
                    //    var dept = depts[i];
                    //    if (dept.EndsWith(".fbx") && dept.StartsWith(PathDefs.ASSETS_PATH_UNITYSECNE))
                    //    {
                    //        ++fbx_dept_cnt;
                    //    }
                    //}

                    for (var i = depts.Count - 1; i >= 0; --i)
                    {
                        var dept = depts[i];
                        if (export_shader && dept.EndsWith(".shader")) 
                        {
                            depts.swap_tail_and_fast_remove(i);
                            continue;
                        }

                        if (export_vcs && dept.EndsWith("__vcs.asset"))
                        {
                            depts.swap_tail_and_fast_remove(i);
                            continue;
                        }                        

                        //dept 被多个其他资源依赖
                        //dept 已经是个独立的资源
                        if (depted_cnt[dept].Count != 1 || all_depts.ContainsKey(dept))
                        {
                            //通过依赖加载
                            depts.swap_tail_and_fast_remove(i);
                            if (fbx_dept_cnt <= 1 && dept.EndsWith(".fbx"))
                            {
                                //var is_scene_fbx = dept.StartsWith(PathDefs.ASSETS_PATH_UNITYSECNE);
                                //if (all || is_scene_fbx)
                                {
                                    //build_delay |= is_scene_fbx;
                                    if (!fbxs.TryGetValue(dept, out var set))
                                    {
                                        set = fbxs[dept] = new HashSet<string>();
                                    }
                                    set.Add(file);
                                }
                            }
                        }
                        else
                        {
                            if (dept.EndsWith(".fbx"))
                            {
                                //fbx不需要 address，这样打包会过滤 fbx 里面的那些没被引用的资源
                                depts.swap_tail_and_fast_remove(i);
                            }
                            else
                            {
                                //复合包，多个资源打进1个ab，方便查看manifest中的资源来源
                            }
                        }
                    }
                }

                //if (!build_delay)
                {
                    depts.Insert(0, file);
                    var ab = new AssetBundleBuild();
                    ab.assetBundleName = get_ab_filename(file);
                    ab.assetNames = depts.ToArray();
                    abs_all.Add(ab);
                }
            } 
            //if (all_shaders.Count > 0) 
            //{
            //    var ab = new AssetBundleBuild();
            //    ab.assetBundleName = "all_shaders.shaders";
            //    ab.assetNames = all_shaders.ToArray();
            //    abs_all.Add(ab);
            //}
            var t8 = DateTime.Now.Ticks;
            Log.Log2File($"打包日志]分包分析，构建打包，耗时:{(t8 - t7) / 10000000}秒。"); 

            foreach (var kv in depted_cnt)
            {
                if (kv.Value.Count != 1 && !all_depts.ContainsKey(kv.Key))
                {
                    if (!kv.Key.EndsWith(".shader"))
                    {
                        Log.LogError($"fix build {kv.Key}");
                        var ab = new AssetBundleBuild();
                        ab.assetBundleName = get_ab_filename(kv.Key);
                        ab.assetNames = new string[] { kv.Key };
                        abs_all.Add(ab);
                    }
                }
            }
            //
            var t9 = DateTime.Now.Ticks;
            Log.Log2File($"打包日志]构建打包，耗时:{(t9 - t8) / 10000000}秒。");
            //return;
            Log.Log2File($"fbxs={fbxs.Count}");
            if (fbxs.Count > 0)
            {
                if (!Directory.Exists(PathDefs.ASSETS_PATH_BUILD_TEMP_FBX_PREFABS))
                {
                    Directory.CreateDirectory(PathDefs.ASSETS_PATH_BUILD_TEMP_FBX_PREFABS);
                }

                ProcessUtils.ExecSystemComm("svn up " + PathDefs.ASSETS_PATH_BUILD_TEMP_FBX_PREFABS + " --accept theirs-full");

                var prefabs = Directory.GetFiles(PathDefs.ASSETS_PATH_BUILD_TEMP_FBX_PREFABS, "*.asset", SearchOption.TopDirectoryOnly).ToList();
                for (var i = 0; i < prefabs.Count; ++i)
                {
                    prefabs[i] = prefabs[i].ToLower().Replace('\\', '/');
                }

                var check_set = new Dictionary<string, string>();
                var SkinnedMeshRenderers = new List<SkinnedMeshRenderer>();
                var MeshFilters = new List<MeshFilter>();
                var ParticleSystemRenderers = new List<ParticleSystemRenderer>();
                var Animators = new List<Animator>();
                var arr_meshs = new Mesh[4];

                var set = new HashSet<string>(); 
                var all_fbxobjs = new HashSet<Object>();
                foreach (var kv in fbxs)
                {
                    var fbxpath = kv.Key;
                    var filename = Path.GetFileNameWithoutExtension(fbxpath);
                    if (false && fbxpath.StartsWith(PathDefs.ASSETS_PATH_UNITYSECNE))
                    {
                        var names = new List<string>();
                        foreach (var p in kv.Value)
                        {
                            names.Add(p);
                            names.AddRange(all_depts[p]);
                        }

                        var ab = new AssetBundleBuild();
                        ab.assetBundleName = "fbxs_with_" + filename + ".ab";
                        ab.assetNames = names.ToArray();
                        var addressableNames = ab.addressableNames = (from f in ab.assetNames select Path.GetFileNameWithoutExtension(f)).ToArray();

                        var k = Path.GetFileNameWithoutExtension(ab.assetBundleName);
                        if (unitys.Count == 1)
                        {
                            if (xalldepts.ContainsKey(k))
                            {
                                k += "_" + Path.GetFileNameWithoutExtension(unitys[0]);
                                ab.assetBundleName = k + ".ab";
                            }
                        }
                        abs_all.Add(ab);
                        if (!all)
                        {
                            xalldepts.Remove(k);
                            foreach (var name in addressableNames)
                            {
                                if (in_main_ab.TryGetValue(name, out var main))
                                {
                                    if (xalldepts.TryGetValue(main, out var arr))
                                    {
                                        var arr2 = arr[1].Split(',');
                                        var idx = Array.IndexOf(arr2, name);
                                        if (idx >= 0)
                                        {
                                            arr2[idx] = null;
                                            arr[1] = string.Join(',', from s in arr2 where s != null select s);
                                        }
                                    }
                                }
                                in_main_ab[name] = k;
                                xalldepts.Remove(name);
                            }
                            foreach (var d in ab.assetNames)
                            {
                                var path = output + '/' + get_ab_filename(d);
                                if (File.Exists(path))
                                {
                                    Log.Log2File($"fbxs -> del {path}");
                                    File.Delete(path);
                                }
                                path += ".manifest";
                                if (File.Exists(path))
                                {
                                    Log.Log2File($"fbxs -> del {path}");
                                    File.Delete(path);
                                }
                            }
                        }
                        continue;
                    }
                    //if (filename == "mesh_fx_ssr_009_attack_001_piaodai001")
                    //{
                    //    Log.LogError($"here {fbxpath} -> {string.Join(',', kv.Value)}");
                    //}
                    if (check_set.TryGetValue(filename, out var oldfbx))
                    {
                        Log.LogError($"资源同名，{fbxpath} -> {oldfbx}");
                        continue;
                    }
                    check_set[filename] = fbxpath;

                    //Renderer rd = null;
                    //MeshFilter mf = null;

                    var fbxobjs = new HashSet<Object>();
                    //var fbxprefabs = new HashSet<string>(kv.Value);
                    var mesh_prefabs = kv.Value.ToList();
                    mesh_prefabs.Sort();
                    foreach (var prefab in mesh_prefabs)
                    {
                        var go = AssetDatabase.LoadAssetAtPath<GameObject>(prefab);
                        if (go)
                        {
                            {
                                go.GetComponentsInChildren(true, SkinnedMeshRenderers);
                                foreach (var sk in SkinnedMeshRenderers)
                                {
                                    if (sk.sharedMesh && AssetDatabase.GetAssetOrScenePath(sk.sharedMesh).ToLower() == fbxpath)
                                    {
                                        fbxobjs.Add(sk.sharedMesh);
                                        if (sk.sharedMaterial && File.Exists(AssetDatabase.GetAssetPath(sk.sharedMaterial.shader)) && fbxobjs.Add(sk.sharedMaterial.shader))
                                        {
                                            fbxobjs.Add(sk);
                                        }
                                    }
                                }
                            }
                            //
                            {
                                go.GetComponentsInChildren(true, MeshFilters);
                                foreach (var sk in MeshFilters)
                                {
                                    if (sk.sharedMesh && AssetDatabase.GetAssetOrScenePath(sk.sharedMesh).ToLower() == fbxpath)
                                    {
                                        fbxobjs.Add(sk.sharedMesh);
                                        var mr = sk.gameObject.GetComponent<MeshRenderer>();
                                        if (mr && mr.sharedMaterial && File.Exists(AssetDatabase.GetAssetPath(mr.sharedMaterial.shader)) && fbxobjs.Add(mr.sharedMaterial.shader))
                                        {
                                            fbxobjs.Add(sk);
                                        }
                                    }
                                }
                            }
                            //
                            {
                                go.GetComponentsInChildren(true, ParticleSystemRenderers);
                                foreach (var sk in ParticleSystemRenderers)
                                {
                                    Array.Clear(arr_meshs, 0, arr_meshs.Length);
                                    int meshcnt = sk.GetMeshes(arr_meshs);
                                    var hit = false;
                                    for (var i = 0; i < meshcnt; ++i)
                                    {
                                        if (arr_meshs[i] && AssetDatabase.GetAssetOrScenePath(arr_meshs[i]).ToLower() == fbxpath)
                                        {
                                            fbxobjs.Add(arr_meshs[i]);
                                            hit = true;
                                        }
                                    }
                                    if (hit && sk.sharedMaterial && File.Exists(AssetDatabase.GetAssetPath(sk.sharedMaterial.shader)) && fbxobjs.Add(sk.sharedMaterial.shader))
                                    {
                                        fbxobjs.Add(sk);
                                    }
                                }
                            }
                            {
                                go.GetComponentsInChildren(true, Animators);
                                foreach (var animator in Animators)
                                {
                                    if (animator.avatar && AssetDatabase.GetAssetOrScenePath(animator.avatar).ToLower() == fbxpath)
                                    {
                                        //Log.Log2File($"prefab={prefab}, animator 不要引用fbx的Avatar, {fbxpath}");
                                        if (animator.gameObject == go)
                                        {
                                            animator.avatar = null;
                                            EditorUtility.SetDirty(go);
                                        }
                                        else
                                        {
                                            fbxobjs.Add(animator.avatar);
                                        }
                                    }
                                    if (animator.runtimeAnimatorController && AssetDatabase.GetAssetOrScenePath(animator.runtimeAnimatorController).ToLower() == fbxpath)
                                    {
                                        Log.LogError($"prefab={prefab}, animator 不要引用fbx的Controller, {fbxpath}");
                                        fbxobjs.Add(animator.runtimeAnimatorController);
                                    }
                                }
                            }
                        }
                    }
                    //
                    //var objlist = fbxobjs.ToList();
                    var tmp_prefab = PathDefs.ASSETS_PATH_BUILD_TEMP_FBX_PREFABS + "fbx_" + filename + ".asset";
                    prefabs.Remove(tmp_prefab);
                    if (fbxobjs.Count == 0)
                    {
                        Log.LogError($"没有找到引用对象, fbx {fbxpath} -> {string.Join(',', kv.Value)}");
                        if (File.Exists(tmp_prefab))
                        {
                            File.Delete(tmp_prefab);
                        }
                    }
                    else
                    {
                        bool used_mesh = false;
                        foreach (var obj in fbxobjs)
                        {
                            if (obj is Mesh)
                            {
                                used_mesh = true;
                                break;
                            }
                        }
                        if (!used_mesh)
                        {
                            Log.LogError($"不引用fbx的mesh吗？？？fbx {fbxpath} -> {string.Join(',', kv.Value)} ");
                        }


                        ObjectsRefForFBX mono = null;
                        if (File.Exists(tmp_prefab))
                        {
                            mono = AssetDatabase.LoadAssetAtPath<ObjectsRefForFBX>(tmp_prefab);
                            if (!all && mono)
                            {
                                if (mono.FbxObjects != null)
                                {
                                    foreach (var o in mono.FbxObjects)
                                    {
                                        if (o)
                                        {
                                            fbxobjs.Add(o);
                                        }
                                    }
                                }
                            }
                        }
                        //
                        if (!mono)
                        {
                            mono = ScriptableObject.CreateInstance<ObjectsRefForFBX>();
                            AssetDatabase.CreateAsset(mono, tmp_prefab);
                            Log.Log2File($"CreateAsset {tmp_prefab}");
                        }
                        //

                        var objlist = fbxobjs.ToList();
                        objlist.Sort((a, b) =>
                        {
                            var cmp = a.name.CompareTo(b.name);
                            if (cmp == 0)
                            {
                                cmp = a.GetType().FullName.CompareTo(b.GetType().FullName);
                            }
                            return cmp;
                        });
                        var dirty = mono.FbxObjects == null || mono.FbxObjects.Length != objlist.Count;
                        if (!dirty)
                        {
                            for (var i = 0; i < objlist.Count; ++i)
                            {
                                if (objlist[i] != mono.FbxObjects[i])
                                {
                                    dirty = true;
                                    break;
                                }
                            }
                        }
                        if (dirty)
                        {
                            mono.FbxObjects = objlist.ToArray();
                            EditorUtility.SetDirty(mono);
                            Log.Log2File($"SetDirty {tmp_prefab}");
                        }

                        {
                            //
                            var ab = new AssetBundleBuild();
                            ab.assetBundleName = "__tmp_fbxs_full/" + get_ab_filename(tmp_prefab);//
                            ab.assetNames = new string[] { tmp_prefab, fbxpath };//将整个fbx 打进临时assetbundle中，欺骗其他assetbundle 引用此fbx里面资源
                            abs_all.Add(ab);

                            //必须保持 assetBundleName 不变
                            ab.assetNames = new string[] { tmp_prefab };//二次打包，只将需要的fbx资源（剔除了 gameobject，shader 等耗内存的资源） 打进assetbundle中，欺骗其他assetbundle引用它
                            abs_fbxs.Add(ab);
                            //
                            
                            foreach (var o in objlist)
                            {
                                var prefab = AssetDatabase.GetAssetOrScenePath(o).ToLower();
                                if (File.Exists(prefab))
                                {
                                    if (prefab.EndsWith(".fbx"))
                                    {
                                        all_fbxobjs.Add(o);
                                    }
                                    else if (set.Add(prefab))
                                    {
                                        ab = new AssetBundleBuild();
                                        ab.assetBundleName = get_ab_filename(prefab);
                                        ab.assetNames = new string[] { prefab };
                                        abs_fbxs.Add(ab);
                                    }
                                }
                                else 
                                {
                                    throw new Exception($"{o} -> {prefab}, file not found");
                                }
                            }
                        }
                    }
                    //break;
                }

                //if (all)
                {
                    var tmp_prefab = PathDefs.ASSETS_PATH_BUILD_TEMP_FBX_PREFABS + "fbx_test_all_fbx_objs.asset";
                    prefabs.Remove(tmp_prefab);

                    if (!File.Exists(tmp_prefab))
                    {
                        var mono = ScriptableObject.CreateInstance<ObjectsRefForFBX>();
                        mono.FbxObjects = all_fbxobjs.ToArray();
                        AssetDatabase.CreateAsset(mono, tmp_prefab);
                    }
                    else
                    {
                        var mono = AssetDatabase.LoadAssetAtPath<ObjectsRefForFBX>(tmp_prefab);
                        if (!all && mono.FbxObjects != null)
                        {
                            var set_fbxobjs = new HashSet<Object>(mono.FbxObjects);
                            foreach (var o in all_fbxobjs)
                            {
                                if (o)
                                {
                                    set_fbxobjs.Add(o);
                                }
                            }
                            mono.FbxObjects = set_fbxobjs.ToArray();
                        }
                        else
                        {
                            mono.FbxObjects = all_fbxobjs.ToArray();
                        }

                        var listx = new List<Object>();
                        foreach (var a in mono.FbxObjects)
                        {
                            if (a)
                            {
                                listx.Add(a);
                            }
                        }
                        listx.Sort((a, b) => { return a.name.CompareTo(b.name); });
                        mono.FbxObjects = listx.ToArray();
                        EditorUtility.SetDirty(mono);
                    }

                    var ab = new AssetBundleBuild();
                    ab.assetBundleName = get_ab_filename(tmp_prefab);
                    ab.assetNames = new string[] { tmp_prefab };
                    abs_all.Add(ab);

                    if (all)
                    {
                        Log.Log2File($"delete assets={prefabs.Count}, {string.Join(",", prefabs)}");
                        foreach (var p in prefabs)
                        {
                            //File.Delete(p);
                        }
                    }
                }
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                ProcessUtils.ExecSystemComm("svn add " + PathDefs.ASSETS_PATH_BUILD_TEMP_FBX_PREFABS + "*.* --force");
                ProcessUtils.ExecSystemComm("svn ci  " + PathDefs.ASSETS_PATH_BUILD_TEMP_FBX_PREFABS + " -m\"auto add assets\"");
            }
            //return;

            _set_addressableNames(abs_all);
            _set_addressableNames(abs_fbxs);

            var tmp_sort_strings = new List<string>();
            var opts = BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DisableWriteTypeTree;
            if (force)
            {
                opts |= BuildAssetBundleOptions.ForceRebuildAssetBundle;
            }
            var test_opts = opts | BuildAssetBundleOptions.StrictMode | BuildAssetBundleOptions.DryRunBuild;
            //
            List<AssetBundleBuild[]> arr_abs = new List<AssetBundleBuild[]>();
            arr_abs.Add(abs_all.ToArray());
            if (abs_fbxs.Count > 0)
            {
                arr_abs.Add(abs_fbxs.ToArray());
            }
            var add_files = new List<string>();
            var update_files = new List<string>();
            var keep_files = new List<string>();
            Dictionary<string, long> delete_files = new Dictionary<string, long>();

            if (all)
            {
                var files = Directory.GetFiles(output, "*", SearchOption.AllDirectories);
                foreach (var f in files)
                {
                    if (!f.EndsWith(".VERSION"))
                    {
                        delete_files[f.Substring(output.Length + 1).Replace('\\', '/')] = File.GetLastWriteTimeUtc(f).Ticks;
                    }
                }
                //
                foreach (var scene in unitys)
                {
                    delete_files.Remove(Path.GetFileNameWithoutExtension(scene) + ".json");
                }
                var outdir = Path.GetFileNameWithoutExtension(output);
                delete_files.Remove(outdir);
                delete_files.Remove(outdir + ".manifest");

                delete_files.Remove(Path.GetFileName(deptpath));
                delete_files.Remove(Path.GetFileName(mattexnames_path));

                Log.Log2File($"all files={delete_files.Count}");
                //return;
            }
            else
            {
                for (var i = 0; i < arr_abs.Count; ++i)
                {
                    var abs = arr_abs[i];
                    for (var n = 0; n < abs.Length; ++n)
                    {
                        var f = output + '/' + abs[n].assetBundleName;
                        if (File.Exists(f))
                        {
                            delete_files[abs[n].assetBundleName] = File.GetLastWriteTimeUtc(f).Ticks;
                        }
                    }
                }
            }
            
            AssetBundle.UnloadAllAssetBundles(true);

            List<Component> coms = new List<Component>();
            HashSet<Type> types = new HashSet<Type>();
            GameObject[] gos = new GameObject[1];
            int AssetBundleCnt = 0;
            //
            bool has_error = false;
            //
            //AssetBundle.LoadFromStream();
            var depts_logs = new Dictionary<string, string>();
            for (var i = 0; i < arr_abs.Count; ++i)
            {
                var abs = arr_abs[i];   
                if (abs.Length == 0)
                {
                    continue;
                }

                if (!all)
                {
                    var logs = new List<string>();
                    foreach (var ab in abs)
                    {
                        logs.Add($"{ab.assetBundleName} = {string.Join("\n\t", ab.assetNames)}");
                    }
                    if (logs.Count > 0)
                    {
                        Log.Log2File($"打包日志]打包输出：{logs.Count}个\n{string.Join("\n", logs)}");
                    }
                }

                var xoutput = output;
                string __tmp_fbxs_full = null;

                //PlayerSettings.stripUnusedMeshComponents = i == 0;
                if (i == 1)
                {
                    // fbx 资源 二次打包
                    //assetbundle 依赖引用 使用 assetBundleName + guid 计算哈希
                    //必须保持 assetBundleName 不变， 输出目录可以改变，不影响对依赖的资源引用
                    //修改输出目录可以避免二次打包 覆盖 前次的打包结果
                    //将输出目录改成父级目录， 二次打包的结果将存放在 父级的 __tmp_fbxs_full 目录， 利用windows的 目录链接，定向到 abs，方便abs下的资源统一管理。
                    //相当于将二次打包的结果打包到 abs，并且欺骗unity 获得正确的依赖资源
                    //xoutput = Path.GetDirectoryName(xoutput).Replace('\\', '/');
                    xoutput += "_tmp";
                    if (!Directory.Exists(xoutput)) 
                    {
                        Directory.CreateDirectory(xoutput);
                    }
                    //
                    __tmp_fbxs_full = xoutput + "/__tmp_fbxs_full";
                    if (!Directory.Exists(__tmp_fbxs_full))
                    {
                        //创建目录链接
                        var cmd = "mklink /j " + $"{__tmp_fbxs_full} {output}".Replace('/','\\');
                        var ret = ProcessUtils.ExecSystemComm(cmd);
                        if (!Directory.Exists(__tmp_fbxs_full))
                        {
                            throw new Exception($"场景目录链接失败！ret={ret}, cmd={cmd}");
                        }
                    }
                }
                var t10 = DateTime.Now.Ticks;
                Log.Log2File($"打包日志]{i} xoutput={xoutput}, abs.Length={abs.Length}, test_opts={test_opts}, opts={opts}");

                var set = new Dictionary<string, AssetBundleBuild>();
                foreach (var ab in abs)
                {
                    if (set.TryGetValue(ab.assetBundleName, out var old))
                    {
                        Log.LogError($"打包日志]{i} 资源同名，assetBundleName={ab.assetBundleName}, assetNames={string.Join(",", ab.assetNames)}, old assetNames={string.Join(",", old.assetNames)}");
                        //EditorUtility.DisplayDialog("资源同名", $"存在同名资源:{ab.assetBundleName}, 打包失败！", "确定");
                        has_error = true;
                    }
                    else
                    {
                        set[ab.assetBundleName] = ab;
                    }
                }

                var mf1 = BuildPipeline.BuildAssetBundles(xoutput, abs, test_opts, PathDefs.PlatformName);
                if (mf1 == null)
                {
                    throw new Exception($"打包日志], {i}, test_opts={test_opts}, BuildAssetBundles, return is null");
                }
                var bundles = mf1.GetAllAssetBundles();
                if (bundles.Length != abs.Length)
                {
                    Log.Log2File($"set={set.Count},{string.Join("\n", set.Keys)}");
                    Log.Log2File($"bundles={bundles.Length},{string.Join("\n", bundles)}");
                    foreach (var ab in bundles)
                    {
                        if (!set.Remove(ab))
                        {
                            Log.Log2File($"error bundle={ab}");
                        }
                    }
                    Log.Log2File($"left abs={set.Count}\n{string.Join("\n", set)}");
                    throw new Exception($"打包日志], {i}, opts={opts}, BuildAssetBundles, bundles.Length={bundles.Length} != abs.Length={abs.Length}");
                }
                var t11 = DateTime.Now.Ticks;
                Log.Log2File($"打包日志]{i} 测试打包, opts={opts}，耗时:{(t11 - t10) / 10000000}秒。abs={abs.Length}, Bundles={mf1.GetAllAssetBundles().Length}");
                mf1 = BuildPipeline.BuildAssetBundles(xoutput, abs, opts, PathDefs.PlatformName);
                if (mf1 == null)
                {
                    throw new Exception($"打包日志], {i}, opts={opts}, mf1 is null");
                }
                bundles = mf1.GetAllAssetBundles();
                if (bundles.Length != abs.Length)
                {
                    Log.Log2File($"set={set.Count},{string.Join("\n", set.Keys)}");
                    Log.Log2File($"bundles={bundles.Length},{string.Join("\n", bundles)}");
                    foreach (var ab in bundles)
                    {
                        if (!set.Remove(ab))
                        {
                            Log.Log2File($"error bundle={ab}");
                        }
                    }
                    Log.Log2File($"left abs={set.Count}\n{string.Join("\n", set)}");
                    throw new Exception($"打包日志], {i}, opts={opts}, BuildAssetBundles, bundles.Length={bundles.Length} != abs.Length={abs.Length}");
                }
                var t12 = DateTime.Now.Ticks;
                Log.Log2File($"打包日志]{i} 真实打包，xoutput={xoutput}, opts={opts}, 耗时:{(t12 - t11) / 10000000}秒。abs={abs.Length}, Bundles={mf1.GetAllAssetBundles().Length}");

                if (__tmp_fbxs_full != null)
                {
                    Directory.Delete(__tmp_fbxs_full);
                    //Directory.Delete(xoutput, true);
                    foreach (var abb in abs)
                    {
                        var xb = Path.GetFileName(abb.assetBundleName);
                        delete_files.Remove(xb);
                        delete_files.Remove(xb + ".manifest");
                    }
                }
                else
                {
                    var shaders_nameab = Path.GetFileNameWithoutExtension(shaders_name) + ".ab";
                    foreach (var abb in abs)
                    {
                        var assetBundleName = abb.assetBundleName;
                        var name = Path.GetFileNameWithoutExtension(assetBundleName);

                        var gen = xoutput + '/' + assetBundleName;
                        //if (all)
                        {
                            var xb = xoutput == output ? assetBundleName : Path.GetFileName(assetBundleName);
                            if (delete_files.TryGetValue(xb, out var ticks))
                            {
                                if (File.GetLastWriteTimeUtc(gen).Ticks == ticks)
                                {
                                    keep_files.Add(xb);
                                }
                                else
                                {
                                    update_files.Add(xb);
                                }
                                delete_files.Remove(xb);
                                delete_files.Remove(xb + ".manifest");
                            }
                            else
                            {
                                add_files.Add(assetBundleName);
                            }
                        }

                        if (in_main_ab.TryGetValue(name, out var main))
                        {
                            if (xalldepts.TryGetValue(main, out var arr))
                            {
                                if (arr.Length == 1)
                                {
                                    Log.LogError($"name={name} not in main={main}");
                                }
                                else
                                {
                                    var arr2 = arr[1].Split(',');
                                    var idx = Array.IndexOf(arr2, name);
                                    if (idx >= 0)
                                    {
                                        arr2[idx] = null;
                                        arr[1] = string.Join(',', from s in arr2 where s != null select s);
                                    }
                                }
                            }
                        }

                        var depts = mf1.GetDirectDependencies(assetBundleName);
                        if (depts?.Length > 0 && !assetBundleName.StartsWith("__tmp_"))
                        {
                            var is_unity = abb.assetNames[0].EndsWith(".unity") && !abb.assetNames[0].StartsWith("ui_");
                            tmp_sort_strings.Clear();

                            foreach (var dept in depts)
                            {
                                //__tmp_fbxs_full
                                if (dept.EndsWith(shaders_nameab) || dept.EndsWith("postprocessresources.postasset") || dept.StartsWith("__tmp_fbx_"))
                                {
                                    continue;
                                }
                                if (dept.EndsWith("__vcs.ab") && !assetBundleName.EndsWith(shaders_nameab))
                                {
                                    continue;
                                }

                                if (is_unity)
                                {
                                    if (!dept.EndsWith(".tex") && !dept.EndsWith(".ab"))
                                    {
                                        Log.LogWarning($"检测到场景[{abb.assetBundleName}]直接依赖了 {dept}, 需要检测使用该资源的prefab是否忘记 apply！");
                                        //continue;
                                    }
                                    //

                                    if (
                                        dept.EndsWith(".ab")
                                            && !dept.Contains("weathersystemprefab")
                                            && !dept.StartsWith("fx_")
                                            && !dept.StartsWith("prefab_fx")
                                            && !dept.Contains("_fx_")
                                            && !dept.Contains("occlusion_")
                                            && !dept.EndsWith("_perfectculling.ab")
                                            && !dept.EndsWith("_dianti_skin.ab")
                                            && !dept.StartsWith("fbxs_with_")
                                        )
                                    {
                                        var need_dept = false;
                                        if (!all && false)
                                        {
                                            //Log.LogInfo(dept + " at " + AssetBundleCnt);
                                            if (++AssetBundleCnt % 1000 == 0)
                                            {
                                                //GC.Collect();
                                                //Resources.UnloadUnusedAssets();
                                            }
                                            var deptab = AssetBundle.LoadFromFile(xoutput + '/' + dept);
                                            var objs = gos;
                                            if (deptab.Contains("main"))
                                            {
                                                //Log.LogInfo("1");
                                                gos[0] = deptab.LoadAsset<GameObject>("main");
                                                //Log.LogInfo("2");
                                            }
                                            else
                                            {
                                                //Log.LogInfo("3");
                                                objs = deptab.LoadAllAssets<GameObject>();
                                                //Log.LogInfo("4");
                                            }
                                            foreach (var obj in objs)
                                            {
                                                if (obj.transform.parent)
                                                {
                                                    continue;
                                                }
                                                obj.GetComponentsInChildren(coms);
                                                foreach (var c in coms)
                                                {
                                                    if (c is Transform || c is Renderer || c is MeshFilter || c is Animator || c is LODGroup || c is Collider || c is RotationBehaviour || c is GameObjectRotationBhv)
                                                    {
                                                        //支持 流逝加载
                                                    }
                                                    else
                                                    {
                                                        Log.LogError($"{dept} 存在无法流逝加载的组件 {c.gameObject.GetLocation()} -> {c}, ");
                                                        need_dept = true;
                                                        //break;
                                                    }
                                                }
                                                if (need_dept)
                                                {
                                                    break;
                                                }
                                            }
                                            deptab.Unload(true);
                                        }
                                        if (!need_dept)
                                        {
                                            continue;
                                        }
                                    }
                                }
                                //
                                //if (!dept.EndsWith(shaders_nameab) && !dept.EndsWith("postprocessresources.postasset"))
                                {
                                    if (dept.StartsWith("__tmp_res_"))
                                    {
                                        var _dept = dept.Replace("__tmp_res_", "").Replace('$', '/');
                                        tmp_sort_strings.Add(_dept.Substring(0, _dept.LastIndexOf('.')));
                                    }
                                    else if (!dept.StartsWith("__tmp_") || dept.StartsWith("__tmp_fbxs_full"))
                                    {
                                        tmp_sort_strings.Add(Path.GetFileNameWithoutExtension(dept));
                                    }
                                }
                            }

                            //var k = Path.GetFileNameWithoutExtension(assetBundleName);
                            if (tmp_sort_strings.Count > 0 || assetBundleName.StartsWith("fbxs_with_") || (assetBundleName.EndsWith("_panel.ab") && abb.addressableNames.Length > 1))
                            {
                                tmp_sort_strings.Sort();
                                var dept_names = string.Join(",", tmp_sort_strings);
                                if (assetBundleName.StartsWith("fbxs_with_"))
                                {
                                    var packs = abb.addressableNames.InsertionSort(string.Compare);
                                    foreach (var pack in packs)
                                    {
                                        xalldepts.Remove(pack);
                                    }
                                    xalldepts[name] = new string[] { dept_names, string.Join(",", packs) };
                                }
                                else if (assetBundleName.EndsWith("_panel.ab") && abb.addressableNames.Length > 1)
                                {
                                    var assetNames = abb.assetNames;
                                    var pngs = from s in assetNames.InsertionSort(string.Compare) where s.EndsWith(".png") && s.StartsWith(PathDefs.ASSETS_PATH_GUI_IMAGES) select Path.GetFileNameWithoutExtension(s);
                                    foreach (var png in pngs)
                                    {
                                        xalldepts.Remove(png);
                                    }
                                    if (pngs.Count() > 0)
                                    {
                                        xalldepts[name] = new string[] { dept_names, string.Join(",", pngs) };
                                    }
                                    else
                                    {
                                        xalldepts[name] = new string[] { dept_names };
                                    }
                                }
                                else
                                {
                                    xalldepts[name] = new string[] { dept_names };
                                }
                            }
                            else
                            {
                                xalldepts.Remove(name);
                            }
                        }
                        else
                        {
                            xalldepts.Remove(name);
                        }
                        var names = string.Join(",", abb.addressableNames.InsertionSort(string.Compare));
                        //if(false)
                        {
                            if (++AssetBundleCnt % 1000 == 0)
                            {
                                //GC.Collect();
                                //Resources.UnloadUnusedAssets();
                            }
                            var ab = AssetBundle.LoadFromFile(gen);
                            var gen_names = string.Join(",", ab.GetAllAssetNames().InsertionSort(string.Compare));
                            ab.Unload(true);
                            Object.DestroyImmediate(ab);
                            if (gen_names != names)
                            {
                                has_error = true;
                                Log.LogError($"{abb.assetBundleName}, files={string.Join(",", abb.assetNames)}, addressableNames=[{names}] != ab.GetAllAssetNames=[{gen_names}], gen={gen}");
                            }
                        }
                        //if (!abb.assetBundleName.StartsWith("__tmp_"))
                        {
                            depts_logs[abb.assetBundleName] = names;
                        }
                    }
                }

                var t13 = DateTime.Now.Ticks;
                Log.Log2File($"打包日志]{i} 校验打包，耗时:{(t13 - t12) / 10000000}秒。abs={abs.Length}");
                Object.DestroyImmediate(mf1);                
            }

            if (has_error)
            {
                throw new Exception("出现错误");
            }

            {
                StringBuilder sb = new StringBuilder();
                foreach (var kv in depts_logs)
                {
                    sb.Append(kv.Key).Append('=').Append(kv.Value).Append('\n');
                }
                File.WriteAllText("depts_logs.txt", sb.ToString());
            }

            //if (Error == null)
            {
                var lines = new List<string>();
                var keys = xalldepts.Keys.ToList();
                keys.Sort();
                foreach (var key in keys)
                {
                    var arr = xalldepts[key];
                    var line = arr[0];
                    if (arr.Length > 1 && arr[1].Length > 0) 
                    {
                        var line2 = string.Join(',', from s in arr[1].Split(',') where !string.IsNullOrEmpty(s) select s);
                        if (!string.IsNullOrEmpty(line2))
                        {
                            line += '|' + line2;
                        }
                    }
                    if (!string.IsNullOrEmpty(line))
                    {
                        lines.Add(key + '=' + line);
                    }
                }
                var text = string.Join("\n", lines);
                if (!File.Exists(deptpath) || File.ReadAllText(deptpath) != text)
                {
                    Log.Log2File($"打包日志], update {deptpath}");
                    File.WriteAllText(deptpath, text);
                }
            }

            //if (Error == null)
            {
                var lines = new List<string>();
                var keys = mattexnames.Keys.ToList();
                keys.Sort();
                foreach (var key in keys)
                {
                    lines.Add(key + '=' + mattexnames[key]);
                }
                var text = string.Join("\n", lines);
                if (!File.Exists(mattexnames_path) || File.ReadAllText(mattexnames_path) != text)
                {
                    Log.Log2File($"打包日志], update {mattexnames_path}");
                    File.WriteAllText(mattexnames_path, text);
                }
            }

            //if (all)
            {
                Log.Log2File($"打包日志], add={add_files.Count}\n{string.Join(",", add_files)}");
                Log.Log2File($"打包日志], update={update_files.Count}\n{string.Join(",", update_files)}");
                Log.Log2File($"打包日志], keep={keep_files.Count}\n{string.Join(",", keep_files)}");
                if (all)
                {
                    Log.Log2File($"打包日志], del={delete_files.Count}\n{string.Join(",", delete_files.Keys)}");
                    foreach (var key in delete_files.Keys)
                    {
                        File.Delete(output + '/' + key); 
                    }
                }
            }
            Log.Log2File("打包日志], done!");
            ProcessUtils.ExecPython(PathDefs.EXPORT_ROOT + PathDefs.os_name + "/" + "update.py");
            if(all) 
            {
                ProcessUtils.ExecPython(PathDefs.EXPORT_ROOT + "datas/" + "update.py");
                var cur = Directory.GetCurrentDirectory();
                try
                {
                    Directory.SetCurrentDirectory(PathDefs.EXPORT_ROOT);
                    var strlines = ProcessUtils.ExecSystemComm("svn status");

                    //var bytes = System.Text.ASCIIEncoding.Convert(System.Text.ASCIIEncoding.ASCII, System.Text.ASCIIEncoding.UTF8, System.Text.ASCIIEncoding.ASCII.GetBytes(strlines));
                    //var strline2 = System.Text.ASCIIEncoding.UTF8.GetString(bytes);

                    var need_add = false;
                    var lines = strlines.Trim().Split('\n');
                    foreach (var line in lines)
                    {
                        var trim = line.Trim();
                        if (trim.Length > 1)
                        {
                            if (trim[0] == '!')
                            {
                                trim = trim.Substring(1).Trim();
                                ProcessUtils.ExecSystemComm($"svn delete \"{trim}\"");
                            }
                            else if (trim[0] == '?')
                            {
                                trim = trim.Substring(1).Trim();
                                if (trim != "." && !trim.EndsWith(".VERSION"))
                                {
                                    var ret = ProcessUtils.ExecSystemComm($"svn -q add \"{trim}\"");
                                    if (!string.IsNullOrWhiteSpace(ret.Trim('\r','\n'))) 
                                    {
                                        need_add = true;
                                    }
                                }
                            }
                        }
                    }
                    if (need_add)
                    {
                        ProcessUtils.ExecSystemComm($"svn add {PathDefs.os_name} --force");
                    }
                    ProcessUtils.ExecSystemComm("svn ci . -m\"一键打包提交\"");                    
                }
                catch (Exception e) 
                {
                    Log.LogError($"{e.GetType().Name}:{e.Message}\n{e.StackTrace}");
                }
                Directory.SetCurrentDirectory(cur);
            }
        }
        catch (Exception e)
        {
            Log.LogError($"{e.Message}\n{e.StackTrace}");
            EditorUtility.DisplayDialog("打包失败", e.Message, "确定");
        }
        AssetDatabase.Refresh();
    }


    //[MenuItem("Export/ab变体打包")]
    static void ExportVariant()
    {
        if (false)
        {
            var abs = new AssetBundleBuild[]
            {
                new AssetBundleBuild()
                {
                    assetBundleName = "jingjichang_art_0001.tex",
                    assetBundleVariant = "--lcn",
                    assetNames = new string[]{ "Assets/variant_ab/jingjichang_art_0001.png" },
                    addressableNames = new string[]{ "main", "sprite"},
                },

                new AssetBundleBuild()
                {
                    assetBundleName = "jingjichang_art_0001.tex",
                    assetBundleVariant = "--lsgp",
                    assetNames = new string[]{ "Assets/variant_ab/sgp/jingjichang_art_0001.png" },
                    addressableNames = new string[]{ "main", "sprite"},
                },

                new AssetBundleBuild()
                {
                    assetBundleName = "jingjichang_art_0001.tex",
                    assetBundleVariant = "--ltw",
                    assetNames = new string[]{ "Assets/variant_ab/tw/jingjichang_art_0001.png" },
                    addressableNames = new string[]{ "main", "sprite"},
                },



                new AssetBundleBuild()
                {
                    assetBundleName = "pack_change_job_system.ab",
                    assetBundleVariant = "--lcn",
                    assetNames = new string[]{ "Assets/variant_ab/atlas_change_job_system/pack_change_job_system.asset" },
                    addressableNames = new string[]{ "main"},
                },

                new AssetBundleBuild()
                {
                    assetBundleName = "pack_change_job_system.ab",
                    assetBundleVariant = "--lsgp",
                    assetNames = new string[]{ "Assets/variant_ab/sgp/atlas_change_job_system/pack_change_job_system.asset" },
                    addressableNames = new string[]{ "main"},
                },


                new AssetBundleBuild()
                {
                    assetBundleName = "test_panel.ab",
                    assetNames = new string[]{ "Assets/variant_ab/test_panel.prefab" },
                    addressableNames = new string[]{ "main"},
                },
            };

            var mf1 = BuildPipeline.BuildAssetBundles("abs", abs, BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DisableWriteTypeTree | BuildAssetBundleOptions.ForceRebuildAssetBundle | BuildAssetBundleOptions.StrictMode, BuildTarget.Android);
            var str1 = string.Join(",", mf1.GetAllAssetBundles());
            var str2 = string.Join(",", mf1.GetAllAssetBundlesWithVariant());
            Log.Log2File($"str1={str1}, str2={str2}");

            AssetDatabase.Refresh();
        }
        else
        {
            AssetBundle.UnloadAllAssetBundles(true);
            UnityEngine.Resources.UnloadUnusedAssets();

            Log.LogInfo($"1AssetBundles={AssetBundle.GetAllLoadedAssetBundles().Count()}");

            var texpath = "abs/jingjichang_art_0001.tex.--ltw";
            var ab = AssetBundle.LoadFromFile(texpath);
            ab.LoadAsset("main");

            var atlaspath = "abs/pack_change_job_system.ab.--lcn";
            var atlasab = AssetBundle.LoadFromFile(atlaspath);
            var gos = atlasab.LoadAllAssets();

            var prefabpath = "abs/test_panel.ab";
            var ab2 = AssetBundle.LoadFromFile(prefabpath);
            var go = ab2.LoadAsset<GameObject>("main");
            var coms = go.GetComponentsInChildren<MonoBehaviour>();
            GameObject.Instantiate(go);
            Log.LogInfo($"2AssetBundles={AssetBundle.GetAllLoadedAssetBundles().Count()}");
        }

    }

    [MenuItem("Assets/查看一级依赖")]
    static void ShowSelectionDepts() 
    {
        var logs = new List<string>();
        Object[] objs = Selection.GetFiltered(typeof(Object), SelectionMode.TopLevel);
        foreach (var obj in objs) 
        {
            var path = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(path)) 
            {
                logs.Add(path);

                var depts = AssetDatabase.GetDependencies(path, false);
                var orders = new List<string>(depts);
                orders.Sort((a,b)=> 
                {
                    var ret = Path.GetExtension(a).CompareTo( Path.GetExtension(b) );
                    if (ret != 0) 
                    {
                        return ret;
                    }
                    return (a).CompareTo((b));
                });
                foreach (var dept in orders) 
                {
                    logs.Add("\t\t" + dept);
                }
            }
        }
        Log.LogInfo(string.Join('\n',logs));
    }


    static void _list_fiter2(List<string> list) 
    {
        for (var i = list.Count - 1; i >= 0; --i) 
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>( list[i] );
            if (go && (
                    go.GetComponentInChildren<My3DRoomImage>(true)
                ))
            {
                continue;
            }       
            list.RemoveAt(i);
        }
    }

    /*
    [MenuItem("Assets/生成图集容器")]
    static void GenSpritePackerContainer()
    {
        var go = Selection.activeGameObject;
        if (go && go.TryGetComponent<MySpritePacker>(out var packer))
        {
            var dir =  Path.GetDirectoryName(AssetDatabase.GetAssetPath(go).ToLower());
            var path = dir + "/" + go.name.Replace("atlas_","pack_") + ".asset";
            var c = ScriptableObject.CreateInstance<MySpritePackerContainer>();
            c.PackerImage = packer.PackerImage;
            c.UVList = packer.uvList;
            AssetDatabase.CreateAsset(c, path);
        }
    }
    */

    /// <summary>
    /// 导出选中资源
    /// </summary>
    [MenuItem("Export/导出选中项")]
    [MenuItem("Assets/导出选中项")]
    static void ExportSelectionWithLog()
    {
        if (!EditorPathUtils.CheckPathSettings()) return;
        var list = GetFiltered();
        if (false)
        {
            Log.LogInfo($"list1={list.Count}");
            Log.LogInfo(string.Join('\n', list));
            _list_fiter2(list);
            Log.LogInfo($"list2={list.Count}");
            Log.LogInfo(string.Join('\n', list));
        }
        _ExportSelects(list, false);
    }


    [MenuItem("Assets/强制导出选中项")]
    static void ExportSelectionForce()
    {
        if (!EditorPathUtils.CheckPathSettings()) return;
        var list = GetFiltered();
        _ExportSelects(list, false, true);
    }

    [MenuItem("Assets/-- 更新图集 --")]
    static void GenPacker() 
    {
        _GenPacker(false);
    }
    [MenuItem("Assets/-- 强制 - 更新图集 --")]
    static void GenPackerForce()
    {
        _GenPacker(true);
    }

    static void _GenPacker(bool force)
    {
        List<string> dirList = new List<string>();
        foreach (var obj in Selection.objects)
        {
            if (obj is DefaultAsset)
            {
                string dirPath = AssetDatabase.GetAssetPath(obj);
                if (Directory.GetFiles(dirPath, "*.png", SearchOption.TopDirectoryOnly).Length > 0)
                {
                    var fname = dirPath.ToLower();
                    if (fname.StartsWith(PathDefs.ASSETS_PATH_GUI_SPRITES))
                    {
                        string dir = fname.Replace('\\', '/');
                        if (!dirList.Contains(dir))
                        {
                            dirList.Add(dir);
                        }
                    }
                }
            }
            else if (obj is Texture2D)
            {
                var fname = AssetDatabase.GetAssetPath(obj).ToLower();
                if (fname.StartsWith(PathDefs.ASSETS_PATH_GUI_SPRITES))
                {
                    string dir = Path.GetDirectoryName(fname).Replace('\\', '/');
                    if (!dirList.Contains(dir))
                    {
                        dirList.Add(dir);
                    }
                }
            }
        }
        if (dirList.Count > 0)
        {
            foreach(var dir in dirList)
            {
                if (!Path.GetFileNameWithoutExtension(dir).StartsWith("atlas_"))
                {
                    EditorUtility.DisplayDialog("提示", "图集文件夹需要以atlas_开头的前缀命名", "ok");
                    return;
                }
            }
            AssetDatabase.Refresh();
            ConsoleUtils.CleanConsole();
            for (int i = 0; i < dirList.Count; i++)
            {
                Builder_All.GenPacker(dirList[i], true, force);
            }
            ForceRefresh();
        }
        else
        {
            Log.LogError("没有选中图集文件");
        }
    }


    [MenuItem("Assets/-- 刷新资源 --")]
    public static void ForceRefresh()
    {
        Log.LogInfo("ForceRefresh");
        var path = "Assets/Libs/Editor/Builder/AssetbundleBuilder.cs";
        var text = File.ReadAllText(path, Encoding.UTF8);
        //Log.LogError($"text={text.Length}");
        File.WriteAllText(path, text + "\r\npublic class __My_Test {} \r\n", Encoding.UTF8);
        AssetDatabase.Refresh();
        File.WriteAllText(path, text, Encoding.UTF8);
        AssetDatabase.Refresh();
    }


    [MenuItem("Assets/检测动画控制器")]
    static void CheckController() 
    {
        var active = Selection.activeObject as RuntimeAnimatorController;
        if (!active) 
        {
            return;
        }
        var has_error = false;
        var names = new HashSet<string>();
        var anis = active.animationClips;
        for (var i = 0; i < anis.Length; ++i) 
        {
            if (!anis[i])
            {
                has_error = true;
                Log.LogError($"第{i}个 丢失了");
            }
            else 
            {
                if (!names.Add(anis[i].name))
                {
                    has_error = true;
                    Log.LogError($"第{i}个名字[{anis[i].name}]重复了");
                }
                else 
                {
                    Log.LogInfo($"{i} -> {anis[i].name}");
                }
            }
        }

        if (has_error)
        {
            var path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(active));
            var files = Directory.GetFiles(path,"*.anim", SearchOption.TopDirectoryOnly);
            foreach (var f in files) 
            {
                var n = Path.GetFileNameWithoutExtension(f);
                if (!names.Contains(n)) 
                {
                    Log.LogError($"{n} 没有被使用");
                }
            }
        }
    }

    [MenuItem("Assets/检查多语言*图集*")]
    static void CheckLangPackers() 
    {
        var dirs = Directory.GetDirectories(PathDefs.PREFAB_PATH_UI_PACKERS);
        foreach (var dir in dirs) 
        {
            var idx = dir.IndexOf("--l");
            if ( idx > 0) 
            {
                var cn = dir.Substring(0, idx);
                var cnname = Path.GetFileName( cn );
                var cn_prefab = cn + "/" + cnname + ".prefab";
                var cn_go = AssetDatabase.LoadAssetAtPath<GameObject>(cn_prefab);
                if (!cn_go) 
                {
                    Log.LogError($"缺少中文图集：{cn_prefab}");
                    continue;
                }
                var cn_packer = cn_go.GetComponent<MySpritePacker>();                

                var name = Path.GetFileName( dir );
                var prefab = dir + "/" + name + ".prefab";
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(prefab);
                if (!go)
                {
                    Log.LogError($"缺少图集：{prefab}");
                    continue;
                }
                Log.LogInfo($"检测图集：{name}");

                var packer = go.GetComponent<MySpritePacker>();
                var uvList = packer.uvList;
                var dict = new HashSet<string>();
                foreach (var uv in uvList)
                {
                    dict.Add(uv.name);
                }
                if (cn_packer.uvList.Length != uvList.Length) 
                {
                    Log.LogError($"图集[{name}]长度{uvList.Length}错误， 图集[{cnname}]的长度是{cn_packer.uvList.Length}");
                }
                foreach (var uv in cn_packer.uvList) 
                {
                    if (!dict.Contains(uv.name)) 
                    {
                        Log.LogError($"图集[{name}]丢失[{uv.name}]");
                    }
                }
            }
        }


    }
    // 获取选中的特定类型列表
    public static List<string> GetFiltered()
    {
        AssetDatabase.Refresh();

        var activeObject = Selection.activeObject;
        var path = AssetDatabase.GetAssetPath(activeObject).ToLower() + '/';
        var is_dir = Directory.Exists(path);
        var is_scene = is_dir && path.StartsWith(PathDefs.ASSETS_PATH_UNITYSECNE);
        var is_actor = is_dir && path.StartsWith(PathDefs.ASSETS_PATH_CHARACTER);
        var is_fx = is_dir && path.StartsWith(PathDefs.PREFAB_PATH_COMPLEX_OBJECT);
        var is_panel = is_dir && path.StartsWith("assets/ui/prefab/");

        Log.Log2File($"path={path}, is_scene={is_scene}, is_actor={is_actor}, is_fx={is_fx}");
        List<string> list = new List<string>();
        Object[] objs = Selection.GetFiltered(typeof(Object), SelectionMode.DeepAssets);
        foreach (var obj in objs)
        {
            if (obj)
            {
                var fname = AssetDatabase.GetAssetPath(obj).ToLower();
                if(File.Exists(fname))
                {
                    if (is_scene)
                    {
                        if (fname.EndsWith(".unity"))
                        {
                            list.Add(fname);
                        }
                    }
                    else if (is_actor || is_fx || is_panel)
                    {
                        if (fname.EndsWith(".prefab"))
                        {
                            list.Add(fname);
                        }
                    }
                    else 
                    {
                        list.Add(fname);
                    }
                }
            }
        }
        Log.Log2File($"list={list.Count}");
        //throw new Exception("test"); 
        return list;
    }

    /// <summary>
    /// 根据列表打包资源
    ///     BuildByList 是所有打包的总入口, 它会派发到各 Builder_XXX 中去
    ///     
    /// </summary>
    /// <param name="list">资源路径名列表</param>
    /// <param name="force_build">是否强制重新打包, 否则仅打包变更的部分</param>
    public static bool BuildByList(List<string> list, bool log, bool all)
    {
        if (!all)
        {
            ConsoleUtils.CleanConsole();
            Log.Log2File($"选中的文件列表: {string.Join(",", list)}");
        }

        if (!EditorPathUtils.CheckPathSettings()) return false;

        if (Application.isPlaying)
        {
            EditorUtility.DisplayDialog("错误", "请先停止运行, 然后打包", "确认");
            return false;
        }

        BuilderIsSaving = true;
        try
        {
            Builder_All.BeforeBuild();
            foreach (var p in list)
            {
                Builder_All.Build(p);
            }
            Builder_All.Flush(log, all);
        }
        catch (Exception e)
        {
            Log.LogError($"打包日志]打包错误, {e.GetType()}:{e.Message}\n{e.StackTrace}");
        }
        BuilderIsSaving = false;

        return true;
    }

    public static bool BuilderIsSaving;
}
