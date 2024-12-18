

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;
using SearchOption = System.IO.SearchOption;

class AssetbundleBuilderNew
{
    public static HashSet<string> shaders_in_shadervariants;
    public static bool export_variants = false;
    static string[] _tmp = new string[1];
    static List<string> _get_depts(string path, ref bool has_shader)
    {
        var list = new List<string>();
        if (path.EndsWith("postprocessresources.asset") || path.EndsWith(".shadervariants"))
        {
            return list;
        }
        //        
        if (shaders_in_shadervariants == null)
        {
            shaders_in_shadervariants = new HashSet<string>();
            var ab = AssetDatabase.LoadAssetAtPath<ShaderVariantCollections>(PathDefs.ASSETS_PATH_BUILD_SHADERS);
            foreach (var shader in ab.AllShaders)
            {
                var low = AssetDatabase.GetAssetPath(shader).ToLower();
                if (File.Exists(low))
                {
                    shaders_in_shadervariants.Add(low);
                }
            }
        }
        bool keep_shader = path == PathDefs.ASSETS_PATH_BUILD_SHADERS;
        if (export_variants) 
        {
            if (keep_shader)
            {
                keep_shader = false;
            }
            else 
            {
                keep_shader = path.StartsWith(PathDefs.ASSETS_PATH_BUILD_SHADERS_DIR);
            }
        }
        _tmp[0] = path;
        var depts = AssetDatabase.GetDependencies(_tmp, false);
        foreach (var _ in depts)
        {
            var low = _.ToLower();
            if (low == path)
            {
                continue;
            }
            if (low.EndsWith(".cs"))
            {
                continue;
            }
            if (low.EndsWith(".unity"))
            {
                continue;
            }
            if (low.EndsWith(".giparams"))
            {
                continue;
            }
            if (low.EndsWith(".pbm"))
            {
                continue;
            }
            if (low.EndsWith(".lighting"))
            {
                continue;
            }
            if (path.EndsWith(".unity")) 
            {
                if (low.EndsWith(".fbx"))
                {
                    //Log.LogError($"地编不要直接把fbx:{low}当做prefab使用 {path}");
                    continue;
                }
                if (low.EndsWith(".mat"))
                {
                    Log.LogError($"场景存在材质泄漏{low} in {path}");
                    continue;
                }
            }
            if (low.EndsWith(".shader"))
            {                
                if (!shaders_in_shadervariants.Contains(low))
                {
                    Log.LogError($"[{path}]依赖的[{low}]不在变体中");
                }
                else
                {
                    has_shader = true;
                    if (!keep_shader)
                    {
                        continue;
                    }
                }
            }
            if (low.EndsWith("lightingdata.asset"))
            {
                list.AddRange(_get_depts(low, ref has_shader));
                continue;
            }
            list.Add(low);
        }
        return list;
    }

    static System.Text.RegularExpressions.Regex preg = new System.Text.RegularExpressions.Regex(@"[ \u4e00-\u9fa5]", System.Text.RegularExpressions.RegexOptions.Singleline);
    static string get_ab_filename(string realpath)
    {
        var path = preg.Replace(realpath, "_");
        if (path != realpath) 
        {
            Log.LogError($"{realpath} 存在空格或者中文");
        }
        if (path.EndsWith('x'))
        {
            path += 's';
        }
        else
        {
            path += 'x';
        }
        if (path.StartsWith("assets/resources/"))
        {
            return path;
        }

        //重命名规则
        var dir = path;

        path = Path.GetFileName(path);
        if (realpath.EndsWith(".unity"))
        {
            var name = Path.GetFileNameWithoutExtension(path);
            path = path.Replace(name + '.', name + "_unity.");
        }

        if (export_variants)
        {
            if (realpath != PathDefs.ASSETS_PATH_BUILD_SHADERS && realpath.StartsWith(PathDefs.ASSETS_PATH_BUILD_SHADERS_DIR))
            {
                path = "assets/resources/vcs/" + path;
            }
        }

        //fbx 
        if (realpath.EndsWith(".fbx")) 
        {
            path = "fbx_" + path;
        }

        //光照贴图 重命名
        if (dir.StartsWith(PathDefs.ASSETS_PATH_UNITYSECNE) && (path.StartsWith("lightmap-") || path.StartsWith("reflectionprobe-")))
        {
            dir = Path.GetDirectoryName(dir);
            path = Path.GetFileName(dir) + '-' + path;
        }
        //动作 重命名
        if (realpath.EndsWith(".anim")) 
        {
            //var name = Path.GetFileNameWithoutExtension(path);
            if (IsSplitAni(realpath)) 
            {                
                var ctl = Directory.GetFiles(Path.GetDirectoryName(realpath), "*.controller");
                if (ctl.Length == 1)
                {
                    var ctr_name = Path.GetFileNameWithoutExtension(ctl[0]).ToLower();
                    return ctr_name + "-" + path;
                }
                else 
                {
                    Log.LogError($"{realpath} 同目录下没有找到唯一的控制器");
                }
            }
        }

        while (realpath.EndsWith(".anim"))
        {
            dir = Path.GetDirectoryName(dir);
            var dir_name = Path.GetFileName(dir);
            if (!dir_name.Contains("ani"))
            {
                if (!path.Contains(dir_name)) 
                {
                    path = dir_name + '-' + path;
                }                
                break;
            }
        }        
        return path;
    }

    static string __tmp_fbxs_full = "__tmp_fbxs_full";
    static List<AssetBundleBuild> _gen_fbx_abs(List<string> fbxs, Dictionary<string, List<string>> d2masters, bool all, List<AssetBundleBuild> abs1)
    {
        var tmp_dir = PathDefs.ASSETS_PATH_BUILD_TEMP_FBX_PREFABS;
        if (!Directory.Exists(tmp_dir))
        {
            Directory.CreateDirectory(tmp_dir);
        }
        ProcessUtils.ExecSystemComm("svn up " + tmp_dir + " --accept theirs-full");
        bool flag_dirty = false;

        var SkinnedMeshRenderers = new List<SkinnedMeshRenderer>();
        var MeshFilters = new List<MeshFilter>();
        var ParticleSystemRenderers = new List<ParticleSystemRenderer>();
        var Animators = new List<Animator>();
        var arr_meshs = new Mesh[4];

        Dictionary<string, AssetBundleBuild> fbx_abs = new Dictionary<string, AssetBundleBuild>();
        var all_fbxobjs = new HashSet<Object>();
        foreach (var fbxpath in fbxs)
        {
            var filename = Path.GetFileNameWithoutExtension(fbxpath);
            var fbxobjs = new HashSet<Object>();
            var mesh_prefabs = d2masters[fbxpath].ToList();
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
            var tmp_prefab = tmp_dir + "fbx_" + filename + ".asset";
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
                    Log.LogError($"不引用fbx的mesh吗？{fbxpath} -> {string.Join(',', mesh_prefabs)}, fbxobjs={fbxobjs.Count}={string.Join(',', from a in fbxobjs select a.name)}");
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
                    ProcessUtils.ExecSystemComm("svn add " + tmp_prefab + "* --force");
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
                    if (cmp == 0)
                    {
                        cmp = AssetDatabase.GetAssetOrScenePath(a).CompareTo(AssetDatabase.GetAssetOrScenePath(b));
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
                    flag_dirty = true;
                    mono.FbxObjects = objlist.ToArray();
                    EditorUtility.SetDirty(mono);
                    Log.Log2File($"SetDirty {tmp_prefab}");
                }

                {
                    //1次打包，临时资源
                    var ab = new AssetBundleBuild();
                    ab.assetBundleName = __tmp_fbxs_full + "/" + get_ab_filename(fbxpath);//
                    ab.assetNames = new string[] { tmp_prefab, fbxpath };//将整个fbx 打进临时assetbundle中，让其他assetbundle 引用此fbx里面资源
                    ab.addressableNames = new string[] { "main" };
                    abs1.Add(ab);

                    //2次打包，必须保持 assetBundleName 不变
                    ab.assetNames = new string[] { tmp_prefab };//只将需要的fbx资源（剔除了 gameobject，shader 等耗内存的资源） 打进assetbundle中，欺骗其他assetbundle引用它
                    fbx_abs.Add(fbxpath, ab);
                    //

                    //2次打包，临时资源，tmp_prefab 避免mesh打包时被unity 过度优化，引用了其他的prefab的MeshFilter，
                    foreach (var o in objlist)
                    {
                        var prefab = AssetDatabase.GetAssetOrScenePath(o).ToLower();
                        if (File.Exists(prefab))
                        {
                            if (prefab.EndsWith(".fbx"))
                            {
                                all_fbxobjs.Add(o);
                            }
                            else if (!fbx_abs.ContainsKey(prefab))
                            {
                                ab = new AssetBundleBuild();
                                ab.assetBundleName = get_ab_filename(prefab);
                                ab.assetNames = new string[] { prefab };
                                ab.addressableNames = new string[] { "main" };
                                fbx_abs[prefab] = ab;
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

        if (all)
        {
            ObjectsRefForFBX mono = null;
            var tmp_prefab = tmp_dir + "fbx_test_all_fbx_objs.asset";
            if (File.Exists(tmp_prefab))
            {
                mono = AssetDatabase.LoadAssetAtPath<ObjectsRefForFBX>(tmp_prefab);
            }
            if (!mono) 
            {
                mono = ScriptableObject.CreateInstance<ObjectsRefForFBX>();
                AssetDatabase.CreateAsset(mono, tmp_prefab);
                ProcessUtils.ExecSystemComm("svn add " + tmp_prefab + "* --force");
            }
            if (!all && mono.FbxObjects != null)
            {
                foreach (var o in mono.FbxObjects)
                {
                    if (o)
                    {
                        all_fbxobjs.Add(o);
                    }
                }
            }            

            var FbxObjects = all_fbxobjs.ToArray();
            Array.Sort(FbxObjects, (a, b) => 
            { 
                var cmp = a.name.CompareTo(b.name);
                if (cmp == 0)
                {
                    cmp = a.GetType().FullName.CompareTo(b.GetType().FullName);
                }
                if (cmp == 0) 
                {
                    cmp = AssetDatabase.GetAssetOrScenePath(a).CompareTo(AssetDatabase.GetAssetOrScenePath(b));
                }
                return cmp;
            });

            var dirty = mono.FbxObjects == null || mono.FbxObjects.Length != FbxObjects.Length;
            if (!dirty) 
            {
                for (var i = 0; i < FbxObjects.Length; ++i)
                {
                    if (FbxObjects[i] != mono.FbxObjects[i])
                    {
                        dirty = true;
                        break;
                    }
                }
            }
            if (dirty)
            {
                flag_dirty = true;
                mono.FbxObjects = FbxObjects;
                EditorUtility.SetDirty(mono);
                Log.Log2File($"SetDirty {tmp_prefab}");
            }
            var ab = new AssetBundleBuild();
            ab.assetBundleName = get_ab_filename(tmp_prefab);
            ab.assetNames = new string[] { tmp_prefab };
            ab.addressableNames = new string[] { "main" };
            abs1.Add(ab);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        foreach (var fbxpath in fbxs)
        {
            var filename = Path.GetFileNameWithoutExtension(fbxpath);
            var tmp_prefab = tmp_dir + "fbx_" + filename + ".asset";
            var has_shader = false;
            var depts = _get_depts(tmp_prefab, ref has_shader);
            foreach (var d in depts)
            {
                if (!fbx_abs.ContainsKey(d))
                {
                    Log.LogError($"{tmp_prefab} not found dept {d}");
                }
            }
        }

        if (flag_dirty)
        {
            ProcessUtils.ExecSystemComm("svn ci  " + tmp_dir + " -m\"fbx临时文件 force_commit_meta\"");
        }
        return fbx_abs.Values.ToList();
    }

    static void _svn_ci(string dir) 
    {
        //del
        var cur = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(dir);
            var strlines = ProcessUtils.ExecSystemComm("svn status");
            var lines = strlines.Trim().Split('\n');
            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                var trim = line.Trim();
                if (trim.Length > 1)
                {
                    if (trim[0] == '!')
                    {
                        trim = trim.Substring(2).Trim();
                        //ProcessUtils.ExecSystemComm($"svn delete \"{trim}\"");
                        sb.Append(' ').Append('"').Append(trim).Append('"');
                        if (sb.Length > 2048) 
                        {
                            sb.Insert(0, "svn delete ");
                            ProcessUtils.ExecSystemComm(sb.ToString());
                            sb.Clear();
                        }
                    }
                }
            }
            if (sb.Length > 0) 
            {
                sb.Insert(0, "svn delete ");
                ProcessUtils.ExecSystemComm(sb.ToString());
                sb.Clear();
            }
            ProcessUtils.ExecSystemComm($"svn add . --force");
            ProcessUtils.ExecSystemComm($"svn revert *.VERSION abs/*.VERSION");
            ProcessUtils.ExecSystemComm("svn ci . -m\"一键打包 force_commit_meta\"");
            //ProcessUtils.ExecPython(dir + "/" + "update.py");
        }
        catch (Exception e)
        {
            Log.LogError($"{e.GetType().Name}:{e.Message}\n{e.StackTrace}");
        }
        Directory.SetCurrentDirectory(cur);        
    }

    public static bool _has_export_ab(string output2, string path) 
    {
        var ab = get_ab_filename(path);
        if (!File.Exists(output2 + ab))
        {
            return false;
        }
        var manifest = output2 + ab + ".manifest";
        if (!File.Exists(manifest))
        {
            return false;
        }

        if (path.EndsWith(".fbx"))
        {
            return true;
        }
        var txt = File.ReadAllText(manifest);
        if (!txt.Contains(path, StringComparison.CurrentCultureIgnoreCase))
        {
            return false;
        }

        return true;
    }
    public static bool IsSplitPath(string path) 
    {
        //武器
        if (path.Contains("/ani_w/"))
        {
            return false;
        }
        //是否角色
        if (path.Contains("/berserker_") || path.Contains("/gladiator_") || path.Contains("/priest_") || path.Contains("/starmage_"))
        {
            //角色
            return true;
        }
        return false;
    }

    public static bool IsSplitAni(string path)
    {
        var name = Path.GetFileNameWithoutExtension(path);
        if (name != path)
        {
            if (!IsSplitPath(path))
            {
                return false;
            }
        }
        if (name.StartsWith("xempty_"))
        {
            return false;
        }
        //钓鱼
        if (name.StartsWith("fish_"))
        {
            return true;
        }
        //采集
        if (name.StartsWith("collect_"))
        {
            return true;
        }
        //剧情
        if (name.StartsWith("juqing_"))
        {
            return true;
        }
        //牵引
        if (name.StartsWith("qianyin_"))
        {
            return true;
        }
        //跳舞
        if (name.Equals("tiaowu"))
        {
            return true;
        }
        //创角
        if (name.Equals("create") || name.Equals("create_sxt_1"))
        {
            return true;
        }
        if (name.Equals("idle_create") || name.Equals("idle_create_sxt_1"))
        {
            return true;
        }
        //死亡
        if (name.Equals("die"))
        {
            return true;
        }
        return false;
    }

    public static void _Export(List<string> list, bool is_all, bool is_force)
    {
        //ProcessUtils.ExecSystemComm("asdasdf"); return;
        var output_abs = (PathDefs.EXPORT_ROOT_OS + "abs").Replace("//", "/");
        var output_abs2 = output_abs + '/';
        var dir_os = Path.GetDirectoryName(output_abs);
        var langs = new string[] { "--lsgp" };

        if (!is_all)
        {
            //自动导出lod
            var lods = new List<string>();
            foreach (var prefab in list)
            {
                if (prefab.StartsWith(PathDefs.ASSETS_PATH_CHARACTER) && prefab.EndsWith(".prefab"))
                {
                    var name = Path.GetFileNameWithoutExtension(prefab);
                    var lod = prefab.Replace("/prefab/", "/prefab_lod/").Replace(name + ".prefab", name + "_lod.prefab");
                    if (!list.Contains(lod) && File.Exists(lod))
                    {
                        lods.Add(lod);
                    }
                }
            }
            if (lods.Count > 0)
            {
                list.AddRange(lods);
            }
        }

        var Count = list.Count;
        if (!is_all && !is_force)
        {
            for (var i = list.Count - 1; i >= 0; --i)
            {
                if (list[i].EndsWith(".prefab") || list[i].EndsWith(".unity") || list[i].StartsWith(PathDefs.ASSETS_PATH_ASSETDATA) || list[i].Contains("__alone."))
                {
                    continue;
                }
                //
                if (!_has_export_ab(output_abs2, list[i]))
                {
                    list.RemoveAt(i);
                    continue;
                }
            }
        }
        Log.LogInfo($"list={Count} -> {list.Count}");
        //return;
        if (list.Count == 0)
        {
            return;
        }

        //筛选出 场景 和 UI界面
        var unitys = new List<string>();
        var panels = new List<string>();
        foreach (var e in list)
        {
            if (e.EndsWith(".unity"))
            {
                unitys.Add(e);
            }
            else if (e.EndsWith("_panel.prefab"))
            {
                panels.Add(e);
            }
        }

        if (unitys.Count > 0 || EditorSceneManager.GetActiveScene().path.ToLower().StartsWith("assets/skill/"))
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
        }

        if (unitys.Count > 0)
        {
            if (unitys.Count == 1)
            {
                var unity_name = Path.GetFileNameWithoutExtension(unitys[0]);
                ProcessUtils.ExecSystemComm("svn up " + PathDefs.PREFAB_PATH_SCENES_TEMP_EMPTY + $"/{unity_name}* --accept theirs-full");
            }
            else 
            {
                ProcessUtils.ExecSystemComm("svn up " + PathDefs.PREFAB_PATH_SCENES_TEMP_EMPTY + " --accept theirs-full");
            }
            foreach (var e in unitys)
            {
                var e2 = Builder_All.fix_scene(e, false);
                list.Remove(e);
                list.Add(e2);
            }
            if (unitys.Count == 1)
            {
                var unity_name = Path.GetFileNameWithoutExtension(unitys[0]);
                ProcessUtils.ExecSystemComm("svn add " + PathDefs.PREFAB_PATH_SCENES_TEMP_EMPTY + $"/{unity_name}* --force");
            }
            else 
            {
                ProcessUtils.ExecSystemComm("svn add " + PathDefs.PREFAB_PATH_SCENES_TEMP_EMPTY + "/*.* --force");
            }            
            ProcessUtils.ExecSystemComm("svn ci  " + PathDefs.PREFAB_PATH_SCENES_TEMP_EMPTY + " -m\"场景临时文件force_commit_meta\"");
        }

        if (panels.Count > 0)
        {
            //UI 文本 多语言处理
            if (is_all)
            {
                ProcessUtils.ExecSystemComm("svn up " + PathDefs.EXPORT_ROOT + "/datas/data/lang/ui_lang.txt --accept theirs-full");
            }
            if(!is_all)
            {
                Builder_All.fix_all_ui_text(panels.ToArray(), !is_all);
            }
        }

        var m2depts = new Dictionary<string, List<string>>();
        var d2masters = new Dictionary<string, List<string>>();
        var tops = new HashSet<string>();
        var lang_texs = new HashSet<string>();

        //var name_ids = new List<string>();

        //依赖处理
        var has_shader = false;
        var stack = new Stack<string>(list);
        while (stack.Count > 0)
        {
            var pop = stack.Pop();
            if (m2depts.ContainsKey(pop))
            {
                continue;
            }
            if (pop.EndsWith(".controller") && IsSplitPath(pop))
            {
                var controller = AssetDatabase.LoadAssetAtPath<UnityEditor.Animations.AnimatorController>(pop);
                var dirty = false;
                //foreach (var lay in controller.layers)
                for (var i = 0; i < controller.layers.Length; ++i)
                {
                    var states = controller.layers[i].stateMachine.states;
                    //foreach (var st in lay.stateMachine.states) 
                    //Log.LogInfo($"{controller} lay:{i} -> states={states.Length}");
                    for (var j = 0; j < states.Length; ++j)
                    {
                        var clip = states[j].state.motion as AnimationClip;
                        if (clip)
                        {
                            var clippath = AssetDatabase.GetAssetPath(clip);
                            if (!string.IsNullOrEmpty(clippath)) 
                            {
                                var fname = Path.GetFileNameWithoutExtension(clippath);
                                if (fname != clip.name) 
                                {
                                    Log.LogError($"{controller} fix {clip} name to {fname} at {clippath}");
                                    clip.name = fname;
                                    UnityEditor.EditorUtility.SetDirty(clip);
                                    UnityEditor.AssetDatabase.SaveAssetIfDirty(clip);
                                }
                            }

                            if (IsSplitAni(states[j].state.name))
                            {
                                var clipname = "xempty_" + states[j].state.name;
                                if (clip.name != clipname)
                                {
                                    dirty = true;
                                    Log.LogInfo($"{controller} layer:{i} -> {j}:{states[j].state.name}:{clip} -> {clipname}");
                                    var xempty = Path.GetDirectoryName(pop) + "/" + clipname + ".anim";
                                    if (!File.Exists(xempty))
                                    {
                                        AssetDatabase.CreateAsset(new AnimationClip() { name = clipname }, xempty);
                                    }
                                    var ani = AssetDatabase.LoadAssetAtPath<AnimationClip>(xempty);
                                    states[j].state.motion = ani;
                                }
                            }
                        }
                    }
                }
                if (dirty)
                {
                    UnityEditor.EditorUtility.SetDirty(controller);
                    //UnityEditor.AssetDatabase.SaveAssetIfDirty(controller);
                    UnityEditor.AssetDatabase.SaveAssets();
                }
                var anis = Directory.GetFiles(Path.GetDirectoryName(pop), "*.anim", SearchOption.TopDirectoryOnly);
                foreach (var ani in anis)
                {
                    var k = ani.ToLower().Replace('\\', '/');
                    if (!m2depts.ContainsKey(k) && IsSplitAni(k))
                    {
                        stack.Push(k);
                    }
                }
            }


            var depts = _get_depts(pop, ref has_shader);
            m2depts[pop] = depts;

            //贴图 多语言处理 //好的方式是使用变体
            if (pop.EndsWith(".png"))
            {
                var prepath = pop.Substring(0, pop.Length - 4);
                var sgp = prepath + "--lsgp.png";
                if (File.Exists(sgp))
                {
                    tops.Add(pop);
                    lang_texs.Add(pop);
                    if (!m2depts.ContainsKey(sgp))
                    {
                        tops.Add(sgp);
                        m2depts[sgp] = new List<string>();
                    }
                    for (var i = 1; i < langs.Length; ++i)
                    {
                        sgp = prepath + "--l" + langs[i] + ".png";
                        if (File.Exists(sgp) && !m2depts.ContainsKey(sgp))
                        {
                            tops.Add(sgp);
                            m2depts[sgp] = new List<string>();
                        }
                    }
                }
            }

            //
            if (false && pop.EndsWith(".mat"))
            {
                var mat = AssetDatabase.LoadAssetAtPath<Material>(pop);
                var s = new UnityEditor.SerializedObject(mat);
                var m_TexEnvs = s.FindProperty("m_SavedProperties").FindPropertyRelative("m_TexEnvs");
                foreach (SerializedProperty kv in m_TexEnvs)
                {
                    var m_Texture = kv.FindPropertyRelative("second").FindPropertyRelative("m_Texture");
                    if (m_Texture.objectReferenceInstanceIDValue > 0 && m_Texture.objectReferenceValue is not Texture)
                    {
                        Log.LogError($"{mat} 丢失贴图:{kv.displayName},{m_Texture.objectReferenceValue}");
                    }
                }
            }

            //场景依赖 需要独立打包，减少切换场景的资源加载 消耗
            var is_unity = pop.EndsWith(".unity");
            //
            foreach (var e in depts)
            {
                if (is_unity)
                {
                    tops.Add(e);
                }
                if (!m2depts.ContainsKey(e))
                {
                    stack.Push(e);
                }
                if (!d2masters.TryGetValue(e, out var masters))
                {
                    d2masters[e] = masters = new List<string>();
                }
                masters.Add(pop);
            }
        }

        //因为不是使用变体，需要在运行时替换 多语言
        foreach (var lang in lang_texs)
        {
            if (d2masters.TryGetValue(lang, out var masters))
            {
                foreach (var m in masters)
                {
                    tops.Add(m);
                }
            }
        }

        //shader 单独打包
        if (has_shader)
        {
            var shaders = PathDefs.ASSETS_PATH_BUILD_SHADERS;
            var from = shaders.Replace("all_", (is_all || (list.Count == 1 && list[0] == shaders)) ? "full_" : "empty_");//增量打包时，不打包变体
            //var from = shaders.Replace("all_", "full_");
            File.Copy(from, shaders, true);
            AssetDatabase.Refresh();
            if (!m2depts.ContainsKey(shaders))
            {
                var depts = _get_depts(shaders, ref has_shader);
                m2depts.Add(shaders, depts);
            }
        }

        var csv_hash = _get_csv_res(is_all);
        var icon_path = PathDefs.ASSETS_PATH_GUI_IMAGES + "icon/";
        var common_tex = PathDefs.ASSETS_PATH_GUI_IMAGES + "common_tex/";

        var abs_paths = new HashSet<string>();
        foreach (var kv in m2depts)
        {
            var path = kv.Key;
            if (path.EndsWith(".shader") && !shaders_in_shadervariants.Contains(path))
            {
                abs_paths.Add(path);
                continue;
            }
            if (export_variants && path.StartsWith(PathDefs.ASSETS_PATH_BUILD_SHADERS_DIR) && path.EndsWith(".asset"))
            {
                abs_paths.Add(path);
                continue;
            }
            var name = Path.GetFileNameWithoutExtension(path);
            if (name.EndsWith("_t4m"))
            {
                abs_paths.Add(path);
                continue;
            }
            if (path.EndsWith(".prefab") || path.StartsWith(icon_path) || path.StartsWith(common_tex) || name.EndsWith("__alone") || !d2masters.TryGetValue(path, out var masters1) || masters1.Count > 1 || tops.Contains(path) || csv_hash.Contains(name))
            {
                abs_paths.Add(path);
                continue;
            }
            if (!is_all)
            {
                if (_has_export_ab(output_abs2, path))
                {
                    abs_paths.Add(path);
                    continue;
                }
            }
        }

        Log.LogInfo($"abs_paths={abs_paths.Count}");
        if (unitys.Count > 0)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
        }

        var fbxs = new List<string>();
        var abname2path = new Dictionary<string, string>();
        var addressableNames = new string[] { "main" };
        var tmp_paths = new List<string>();
        var abs1 = new List<AssetBundleBuild>();
        var sorted_paths = abs_paths.ToList();
        sorted_paths.Sort();
        foreach (var m in sorted_paths)
        {
            //fbx 需要打包2次，第2次过滤掉不需要的资源
            if (m.EndsWith(".fbx"))
            {
                fbxs.Add(m);
                continue;
            }

            tmp_paths.Clear();
            tmp_paths.Add(m);
            var assetBundleName = get_ab_filename(m);
            //场景是特殊的ab，不能是复合包，特殊处理
            if (!m.EndsWith(".unity"))
            {
                var depts = m2depts[m];
                foreach (var e in depts)
                {
                    if (abs_paths.Contains(e))
                    {
                        //e 是独立的ab，依赖加载
                        continue;
                    }
                    if (e.EndsWith(".fbx"))
                    {
                        //fbx 一定是独立打包的
                        continue;
                    }
                    tmp_paths.Add(e);
                }
            }


            //同名错误检测
            var dir = m;
            while (abname2path.TryGetValue(assetBundleName, out var old))
            {
                Log.LogError($"同名资源：{m} -> {old}");
                dir = Path.GetDirectoryName(dir);
                //if (!dir.EndsWith("ani"))
                {
                    assetBundleName = Path.GetFileName(dir) + '-' + assetBundleName;
                }
            }
            abname2path.Add(assetBundleName, m);
            var ab = new AssetBundleBuild();
            ab.assetBundleName = assetBundleName;
            ab.assetNames = tmp_paths.ToArray();
            ab.addressableNames = addressableNames;
            abs1.Add(ab);
        }

        //fbx 打包优化
        Log.LogInfo($"abs1={abs1.Count}, fbxs={fbxs.Count}");
        var fbx_abs = fbxs.Count > 0 ? _gen_fbx_abs(fbxs, d2masters, is_all, abs1) : new List<AssetBundleBuild>();
        //return;
        //打包
        _build_abs(unitys, dir_os, output_abs, output_abs2, abs1, fbx_abs, abname2path, lang_texs, is_all, is_force, true);

        var xdir_os = dir_os.Replace("\\" + PathDefs.os_name, "\\" + PathDefs.os_name + "_trees");
        var xoutput_abs = output_abs.Replace("/" + PathDefs.os_name, "/" + PathDefs.os_name + "_trees");
        var xoutput_abs2 = output_abs2.Replace("/" + PathDefs.os_name, "/" + PathDefs.os_name + "_trees");
        //_build_abs(unitys, xdir_os, xoutput_abs, xoutput_abs2, abs1, fbx_abs, abname2path, lang_texs, is_all, is_force, false);
    }
    
    static void _build_abs(List<string> unitys, string dir_os, string output_abs, string output_abs2, List<AssetBundleBuild> abs1, List<AssetBundleBuild> fbx_abs, Dictionary<string,string> abname2path, HashSet<string> lang_texs, bool is_all, bool is_force, bool Disable_trees)
    {
        if (!Directory.Exists(dir_os))
        {
            Directory.CreateDirectory(dir_os);
        }
        if (!Directory.Exists(output_abs2))
        {
            Directory.CreateDirectory(output_abs2);
        }
        //
        if (is_all || unitys.Count > 0)
        {
            ProcessUtils.ExecSystemComm("svn up " + dir_os + " --accept theirs-full");
        }
        if (is_all) 
        {
            AssetbundleBuilder.ForceRefresh();
        }
        var opts = BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DeterministicAssetBundle;
        if (PathDefs.PlatformName != BuildTarget.StandaloneWindows && PathDefs.PlatformName != BuildTarget.StandaloneWindows64)
        {
            opts |= BuildAssetBundleOptions.StrictMode;
        }
        if (Disable_trees) 
        {
            opts |= BuildAssetBundleOptions.DisableWriteTypeTree;
        }
        if (is_force)
        {
            opts |= BuildAssetBundleOptions.ForceRebuildAssetBundle;
        }


        foreach (var e in unitys)
        {
            var name = Path.GetFileNameWithoutExtension(e).Replace("_empty","");
            var json_path = PathDefs.PREFAB_PATH_SCENES_TEMP_EMPTY + name + ".json";
            if (File.Exists(json_path))
            {
                var export_json = output_abs2 + name + ".json";
                File.Copy(json_path, export_json, true);
            }
        }

        try
        {
            var depts_path = output_abs2 + "alldepts.txt";
            var mattex_path = output_abs2 + "mattexnames.txt";

            var files_map = new Dictionary<string, long>();
            if (is_all)
            {
                var files = Directory.GetFiles(output_abs2, "*.*", SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                {
                    if (file.EndsWith(".VERSION"))
                    {
                        continue;
                    }
                    if (file.EndsWith(".manifest"))
                    {
                        var xfile = file.Substring(0, file.Length - 9);
                        if (!File.Exists(xfile))
                        {
                            Log.LogError($"not found {xfile}, del {file}");
                            File.Delete(file);
                            //ProcessUtils.ExecSystemComm($"svn delete {file}");
                            files_map.Remove(Path.GetFileName(xfile));
                        }                        
                    }
                    else
                    {
                        files_map[Path.GetFileName(file)] = File.GetLastWriteTimeUtc(file).Ticks;
                    }
                }
                files_map.Remove(Path.GetFileName(output_abs));
                Log.LogInfo($"files_map={files_map.Count}, in {output_abs2}");
            }
            else
            {
                files_map[Path.GetFileName(depts_path)] = File.GetLastWriteTimeUtc(depts_path).Ticks;
                files_map[Path.GetFileName(mattex_path)] = File.GetLastWriteTimeUtc(mattex_path).Ticks;
            }
            //
            var map_depts = new Dictionary<string, string>();
            var map_mats = new Dictionary<string, string>();

            if (!is_all)
            {
                if (File.Exists(depts_path))
                {
                    var lines = File.ReadAllLines(depts_path);
                    foreach (var l in lines)
                    {
                        var arr = l.Split('=');
                        map_depts[arr[0]] = l;
                    }
                }
                if (File.Exists(mattex_path))
                {
                    var lines = File.ReadAllLines(mattex_path);
                    foreach (var l in lines)
                    {
                        var arr = l.Split('=');
                        map_mats[arr[0]] = l;
                    }
                }
            }
            //
            var gen_abs = new List<string>();
            var gen_txts = new List<string>();
            gen_txts.Add(Path.GetFileName(depts_path));
            gen_txts.Add(Path.GetFileName(mattex_path));
            //
            foreach (var abs in new List<AssetBundleBuild>[] { abs1, fbx_abs })
            {
                if (abs.Count == 0)
                {
                    continue;
                }

                var fbxs_mesh = 0;
                foreach (var ab in abs)
                {
                    if (ab.assetBundleName.EndsWith(".fbxs"))
                    {
                        ++fbxs_mesh;
                    }
                }
                var isfirst = abs == abs1;
                var output = isfirst ? output_abs : output_abs + "_tmp";
                Log.LogInfo($"output={output}, abs={abs.Count}, fbxs_mesh={fbxs_mesh}");
                if (!Directory.Exists(output))
                {
                    Directory.CreateDirectory(output);
                }

                string tmp_fbxs_full = null;
                if (!isfirst)
                {
                    tmp_fbxs_full = output + "/" + __tmp_fbxs_full;
                    if (Directory.Exists(tmp_fbxs_full))
                    {
                        Directory.Delete(tmp_fbxs_full);
                    }
                    //创建目录链接
                    var cmd = "mklink /j " + $"{tmp_fbxs_full} {output_abs}".Replace('/', '\\');
                    var ret = ProcessUtils.ExecSystemComm(cmd);
                    if (!Directory.Exists(tmp_fbxs_full))
                    {
                        throw new Exception($"场景目录链接失败！cmd={cmd}, ret={ret}");
                    }
                }

                var other_map = new HashSet<string>();
                var other_dir = output;
                if (isfirst)
                {
                    other_dir += '/' + __tmp_fbxs_full;
                }
                if (is_all)
                {
                    if (Directory.Exists(other_dir))
                    {
                        var other_files = Directory.GetFiles(other_dir + '/', "*.*", SearchOption.TopDirectoryOnly);
                        foreach (var f in other_files)
                        {
                            if (f.EndsWith(".VERSION"))
                            {
                                continue;
                            }
                            if (f.EndsWith(".manifest"))
                            {
                                var xfile = f.Substring(0, f.Length - 9);
                                if (!File.Exists(xfile))
                                {
                                    Log.LogError($"not found {xfile}, del {f}");
                                    File.Delete(f);
                                    other_map.Remove(Path.GetFileName(xfile));
                                }
                            }
                            else
                            {
                                other_map.Add(Path.GetFileName(f));
                            }
                        }
                        var d = Path.GetFileNameWithoutExtension(other_dir);
                        other_map.Remove(d);
                    }
                    Log.LogInfo($"other_map={other_map.Count}, path={other_dir}");
                }
                else
                {
                    foreach (var ab in abs)
                    {
                        Log.LogInfo($"{ab.assetBundleName}={string.Join('\n', ab.assetNames)}");
                        if (ab.assetBundleName.EndsWith(".fbxs") != isfirst)
                        {
                            var filename = Path.GetFileName(ab.assetBundleName);
                            var file = output_abs2 + filename;
                            //Log.LogInfo($"ticks1 {file}");
                            if (File.Exists(file))
                            {
                                files_map[filename] = File.GetLastWriteTimeUtc(file).Ticks;
                            }
                        }
                    }
                }

                Log.LogInfo($"BuildAssetBundles start, output={output}, abs={abs.Count}");
                var Ticks = DateTime.UtcNow.Ticks;
                var mf1 = BuildPipeline.BuildAssetBundles(output, abs.ToArray(), opts, PathDefs.PlatformName);
                var bundles = mf1.GetAllAssetBundles();
                Log.LogInfo($"abs={abs.Count}, bundles={bundles.Length}, cost={(DateTime.UtcNow.Ticks - Ticks) / 10000000}s");
                if (bundles.Length != abs.Count)
                {
                    throw new Exception($"bundles.Length error");
                }
                else
                {
                    var depts_list = new List<string>();
                    var packs_list = new List<string>();
                    var depts_comps = new List<Component>();
                    foreach (var ab in abs)
                    {
                        var abname = ab.assetBundleName;
                        if (abname.StartsWith("assets/"))
                        {
                            continue;
                        }
                        var name = ab.assetNames[0];
                        //
                        if (abname.EndsWith(".fbxs") != isfirst)
                        {
                            gen_abs.Add(Path.GetFileName(abname));
                            if (is_all && name.EndsWith(".unity"))
                            {
                                var gen = Path.GetFileNameWithoutExtension(name).Replace("_empty", "") + ".json";
                                gen_txts.Add(gen);
                            }
                            if (isfirst)
                            {
                                var bname = Path.GetFileNameWithoutExtension(abname);
                                var depts = mf1.GetDirectDependencies(abname);
                                var is_scene = name.EndsWith(".unity");
                                var is_panel = name.EndsWith("_panel.prefab");
                                depts_list.Clear();
                                foreach (var d in depts)
                                {
                                    if (d.StartsWith("assets/") || d == "all_shader_variants.assetx" || d == "postprocessresources.assetx")
                                    {
                                        continue;
                                    }
                                    if (is_scene && d.EndsWith(".prefabx") && !d.StartsWith("fx_") && !d.EndsWith("_dianti_skin"))
                                    {
                                        var need = false;
                                        var path = abname2path[d];
                                        var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                                        go.GetComponentsInChildren(true, depts_comps);
                                        foreach (var c in depts_comps)
                                        {
                                            if (c is ParticleSystemRenderer || c is ParticleSystem)
                                            {
                                                need = true;
                                                break;
                                            }
                                            if (c is Transform || c is Renderer || c is MeshFilter || c is Animator || c is LODGroup || c is Collider
                                                || c is RotationBehaviour || c is GameObjectRotationBhv || c is AnimaEvent_SetActive || c is UpdateFollowTargetPosition)
                                            {
                                                continue;
                                            }
                                            need = true;
                                            //Log.LogInfo($"{b} need {c} at {c.gameObject.GetLocation()}");
                                            break;
                                        }
                                        if (!need)
                                        {
                                            continue;
                                        }
                                    }
                                    depts_list.Add(Path.GetFileNameWithoutExtension(d));
                                }

                                packs_list.Clear();
                                if (is_panel && ab.assetNames.Length > 1)
                                {
                                    for (var i = 1; i < ab.assetNames.Length; ++i)
                                    {
                                        if (ab.assetNames[i].StartsWith(PathDefs.ASSETS_PATH_GUI_IMAGES))
                                        {
                                            packs_list.Add(Path.GetFileNameWithoutExtension(ab.assetNames[i]));
                                        }
                                    }
                                }

                                if (depts_list.Count > 0 || packs_list.Count > 0)
                                {
                                    depts_list.Sort();
                                    map_depts[bname] = bname + '=' + string.Join(',', depts_list);
                                    if (packs_list.Count > 0)
                                    {
                                        packs_list.Sort();
                                        map_depts[bname] += '|' + string.Join(',', packs_list);
                                    }
                                }
                                else if (!is_all)
                                {
                                    map_depts.Remove(bname);
                                }
                            }
                        }
                        else if (is_all)
                        {
                            var b = Path.GetFileName(abname);
                            other_map.Remove(b);
                        }
                    }

                    if (isfirst)
                    {
                        var mat_props = new List<string>();
                        foreach (var a in abs)
                        {
                            var ismat = a.assetNames[0].EndsWith(".mat");
                            if (ismat)
                            {
                                mat_props.Clear();
                                var mat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(a.assetNames[0]);
                                var names = mat.GetTexturePropertyNames();
                                foreach (var name in names)
                                {
                                    var tex = mat.GetTexture(name);
                                    if (tex)
                                    {
                                        var texpath = AssetDatabase.GetAssetPath(tex).ToLower();
                                        if (lang_texs.Contains(texpath))
                                        {
                                            mat_props.Add($"{name}:{tex.name.ToLower()}");
                                        }
                                    }
                                }
                                var assetBundleName = Path.GetFileNameWithoutExtension(a.assetBundleName);
                                if (mat_props.Count == 0)
                                {
                                    if (!is_all)
                                    {
                                        map_mats.Remove(assetBundleName);
                                    }
                                }
                                else
                                {
                                    mat_props.Sort();
                                    map_mats[assetBundleName] = assetBundleName + '=' + string.Join(',', mat_props);
                                }
                            }
                        }
                    }

                    if (is_all)
                    {
                        Log.LogInfo($"total gens={gen_abs.Count}");
                        Log.LogInfo($"del other_map={other_map.Count}, path={other_dir}\n{string.Join(',', other_map)}");
                        foreach (var d in other_map)
                        {
                            var delpath = other_dir + '/' + d;
                            if (File.Exists(delpath))
                            {
                                File.Delete(delpath);
                            }
                            delpath += ".manifest";
                            if (File.Exists(delpath))
                            {
                                File.Delete(delpath);
                            }
                        }
                    }
                }
                //
                if (tmp_fbxs_full != null)
                {
                    Directory.Delete(tmp_fbxs_full);
                }
            }

            //
            {
                var lines = map_depts.Values.ToList();
                lines.Sort();
                var txt = string.Join('\n', lines);
                if (!File.Exists(depts_path) || File.ReadAllText(depts_path) != txt)
                {                    
                    File.WriteAllText(depts_path, txt);
                }
            }

            //
            {
                var lines = map_mats.Values.ToList();
                lines.Sort();
                var txt = string.Join('\n', lines);
                if (!File.Exists(mattex_path) || File.ReadAllText(mattex_path) != txt)
                {
                    File.WriteAllText(mattex_path, txt);
                }
            }

            //
            int keep = 0, update = 0, add = 0, del = 0;
            var sbu = new StringBuilder();
            var sba = new StringBuilder();
            var sbd = new StringBuilder();

            gen_abs.AddRange(gen_txts);
            foreach (var gen in gen_abs)
            {
                if (files_map.TryGetValue(gen, out var ticks))
                {
                    files_map.Remove(gen);
                    var file = output_abs2 + gen;
                    //Log.LogInfo($"ticks2 {file}");//
                    if (ticks == File.GetLastWriteTimeUtc(file).Ticks)
                    {
                        //keep
                        ++keep;
                    }
                    else
                    {
                        //update
                        //Log.LogInfo($"update {gen}");
                        sbu.Append(gen).Append(',');
                        ++update;
                    }
                }
                else
                {
                    //add
                    //Log.LogInfo($"add {gen}");
                    sba.Append(gen).Append(',');
                    ++add;
                }
            }
            if (is_all)
            {
                del = files_map.Count;
                foreach (var d in files_map)
                {
                    //delete
                    //Log.LogInfo($"del {d.Key}");
                    sbd.Append(d.Key).Append(',');
                    try
                    {
                        File.Delete(output_abs2 + d.Key);
                        File.Delete(output_abs2 + d.Key + ".manifest");
                    }
                    catch { }
                }
            }
            Log.LogInfo($"summary, keep = {keep}, update = {update}, add = {add}, del = {del}");
            if (update > 0)
            {
                Log.LogInfo($"updates={sbu}");
            }
            if (add > 0)
            {
                Log.LogInfo($"adds={sba}");
            }
            if (del > 0)
            {
                Log.LogInfo($"dels={sbd}");
            }
            if (is_all)
            {
                _svn_ci(dir_os);
            }

            if (is_all || add > 0)
            {
                if (!is_all && unitys.Count == 0) 
                {
                    ProcessUtils.ExecSystemComm("svn up " + dir_os + " --accept theirs-full");
                }
                ProcessUtils.ExecPython(dir_os + "/" + "update.py");
            }
        }
        catch (Exception e)
        {
            Log.LogError($"{e.GetType().Name}:{e.Message}\n{e.StackTrace}");
        }
    }


    static List<string> _get_ab_assets(string manifest)
    {
        if (!Path.GetFileNameWithoutExtension(manifest).Contains('.')) 
        {
            return null;
        }

        var paths = new List<string>();
        bool flag = false;
        foreach (var l in File.ReadAllLines(manifest))
        {
            if (!flag)
            {
                if (l.StartsWith("Assets:"))
                {
                    flag = true;
                }
                continue;
            }
            if (!l.StartsWith("- "))
            {
                break;
            }
            var path = l.Substring(2).ToLower();
            if (File.Exists(path))
            {
                paths.Add(path);
            }
            else
            {
                Log.LogError($"{path} file not found, from {manifest}");
            }
        }

        var hit = false;
        var name = Path.GetFileNameWithoutExtension(manifest);
        var is_unityx = name.EndsWith(".unityx");
        for (var i = 0; i < paths.Count; ++i)
        {
            string need = Path.GetFileName(paths[i]);
            if (need.StartsWith("fbx_") && need.EndsWith(".asset"))
            {
                need = Path.GetFileNameWithoutExtension(need);
            }
            need = preg.Replace(need, "_");
            if (name.Contains(need) || (is_unityx && need.EndsWith(".unity")))
            {
                if (i != 0)
                {
                    var a = paths[0];
                    paths[0] = paths[i];
                    paths[i] = a;
                }
                hit = true;
                break;
            }
        }
        if (!hit)
        {
            if (paths.Count > 0 || !flag)
            {
                Log.LogError($"drop {manifest} not hit in paths={string.Join(',', paths)}");
            }
            return null;
        }
        return paths;
    }

    static void _append_list_paths(string path, Dictionary<string,string> all_paths, HashSet<string> redundances, Dictionary<string, List<string>> all_list) 
    {
        //var needfix = false;
        var paths = _get_ab_assets(path);
        if (paths != null)
        {
            all_list.Add(path, paths);
            foreach (var p in paths)
            {
                if (all_paths.TryGetValue(p, out var other))
                {
                    Log.LogInfo($"{p} found in other assetbundle {other} <-> {path}");
                    redundances.Add( p );
                }
                else 
                {
                    all_paths[p] = path;
                }                
            }
        }
    }

    static void _fix_list_paths(Dictionary<string, string> paths, HashSet<string> redundances, Dictionary<string, List<string>> all_list) 
    {
        HashSet<string> set = new HashSet<string>();
        foreach (var kv in all_list) 
        {
            var list = kv.Value;
            set.Add( list[0] );
            for (var i = list.Count - 1; i > 0; --i) 
            {
                if (redundances.Contains(list[i])) 
                {
                    //Log.LogInfo($"remove {list[i]} from {list[0]} by {paths[list[i]]}");
                    list.RemoveAt( i );
                }
            }
        }

        foreach (var k in redundances) 
        {
            if (!set.Contains(k)) 
            {
                //Log.LogInfo($"append {k}");
                all_list.Add(k, new List<string>() { k });
            }
        }
    }

    static List<AssetBundleBuild> _list_to_abs(Dictionary<string, List<string>> list_paths)
    {
        var set = new Dictionary<string, int>(); 
        var abs = new List<AssetBundleBuild>();
        foreach (var kv in list_paths) 
        {
            string assetBundleName;
            if (kv.Key.StartsWith("assets/") || kv.Key.Contains("/assets/"))
            {
                assetBundleName = get_ab_filename(kv.Key);
                if (assetBundleName.StartsWith("fbx_") && assetBundleName.EndsWith(".assetx"))
                {
                    assetBundleName = Path.GetFileNameWithoutExtension(assetBundleName) + ".fbxs";
                }
            }
            else 
            {
                assetBundleName = Path.GetFileNameWithoutExtension(kv.Key);//aaa.x.manifest
            }

            if (set.TryGetValue(assetBundleName, out var cnt))
            {
                set[assetBundleName] = ++cnt;
                assetBundleName = Path.GetFileNameWithoutExtension(assetBundleName) + "__" + cnt + Path.GetExtension(assetBundleName);
                Log.LogError($"remane {assetBundleName}");
            }
            else 
            {
                set[assetBundleName] = 0;
            }            

            if (assetBundleName.EndsWith(".fbxs"))
            {
                assetBundleName = __tmp_fbxs_full + "/" + assetBundleName;
            }
            var ab = new AssetBundleBuild()
            {
                assetBundleName = assetBundleName,
                assetNames = kv.Value.ToArray(),
            };
            if (!kv.Value[0].EndsWith(".unity"))
            {
                ab.addressableNames = new string[] { "main" };
            }
            abs.Add(ab);            
        }
        return abs;
    }

    //[MenuItem("Export/* 同步打包 trees *")]
    static void _copy_build_abs()
    {
        if (!EditorPathUtils.CheckPathSettings()) return;
        var output_abs = (PathDefs.EXPORT_ROOT_OS + "abs").Replace("//", "/");
        var output_abs2 = output_abs + '/';
        var dir_os = Path.GetDirectoryName(output_abs);
        var output_tmp = output_abs + "_tmp";//mesh
        var abs__tmp_fbxs_full = output_abs2 + __tmp_fbxs_full; //fbx

        //
        var all = Directory.GetFiles(output_abs, "*.manifest", SearchOption.AllDirectories);
        //
        var fbxs = Directory.GetFiles(abs__tmp_fbxs_full, "*.manifest", SearchOption.TopDirectoryOnly);
        //
        var meshs = Directory.GetFiles(output_tmp, "*.manifest", SearchOption.AllDirectories);

        HashSet<string> fbx_names = new HashSet<string>();
        foreach (var fbx in fbxs)
        {
            fbx_names.Add(Path.GetFileName(fbx));
        }

        Dictionary<string, string> abname2path = new Dictionary<string, string>();
        HashSet<string> lang_texs = new HashSet<string>();
        List<string> unitys = new List<string>();
        List<AssetBundleBuild> abs1 = null, abs2 = null;
        {
            var redundances = new HashSet<string>();
            var all_list = new Dictionary<string, List<string>>();
            var all_paths = new Dictionary<string, string>();
            foreach (var f in all)
            {
                var fname = Path.GetFileName(f);
                if (fbx_names.Contains(fname) && !f.Contains("__tmp_fbxs_full"))
                {
                    continue;
                }
                _append_list_paths(f, all_paths, redundances, all_list);
            }
            if (redundances.Count > 0)
            {
                _fix_list_paths(all_paths, redundances, all_list);
            }
            abs1 = _list_to_abs(all_list);
            foreach (var ab in abs1)
            {
                var path = ab.assetNames[0];
                abname2path[ab.assetBundleName] = path;
                if (path.EndsWith(".unity"))
                {
                    unitys.Add(path);
                }
                if (path.EndsWith(".png"))
                {
                    var idx = path.IndexOf("--");
                    if (idx > 0)
                    {
                        var tex = path.Remove(idx, path.LastIndexOf('.') - idx);
                        lang_texs.Add(tex);
                    }
                }
            }
        }

        {
            var redundances = new HashSet<string>();
            var all_list = new Dictionary<string, List<string>>();
            var all_paths = new Dictionary<string, string>();
            foreach (var path in meshs)
            {
                _append_list_paths(path, all_paths, redundances, all_list);                
            }
            foreach (var fn in fbx_names)
            {
                var path = output_abs2 + fn;
                if (!File.Exists(path))
                {
                    Log.LogError($"{path} not found");
                    continue;
                }
                _append_list_paths(path, all_paths, redundances, all_list);                
            }
            if (redundances.Count > 0)
            {
                _fix_list_paths(all_paths, redundances, all_list);
            }
            abs2 = _list_to_abs(all_list);
        }
        //_build_abs(unitys, dir_os, output_abs, output_abs2, abs1, abs2, abname2path, lang_texs, true, false, true);
        var xdir_os = dir_os.Replace("\\" + PathDefs.os_name, "\\" + PathDefs.os_name + "_trees");
        var xoutput_abs = output_abs.Replace("/" + PathDefs.os_name, "/" + PathDefs.os_name + "_trees");
        var xoutput_abs2 = output_abs2.Replace("/" + PathDefs.os_name, "/" + PathDefs.os_name + "_trees");
        _build_abs(unitys, xdir_os, xoutput_abs, xoutput_abs2, abs1, abs2, abname2path, lang_texs, true, false, false);
    }

    static HashSet<string> _get_csv_res(bool is_all)
    {        
        var csv_hash = new HashSet<string>();
        if (is_all)
        {
            var time = System.DateTime.Now.Ticks;
            var xpreg = new System.Text.RegularExpressions.Regex(@"[0-9a-zA-Z,_ -]", System.Text.RegularExpressions.RegexOptions.Singleline);
            var headers = new List<string>();
            var bools = new List<int>();
            var cells = new List<string>();
            var csv_root = PathDefs.EXPORT_ROOT + "/datas/data/csv/";
            //QualityData
            //FuncOpenBase.func_title
            //SkillBase.pic,icon,res,_tex
            var csvs = Directory.GetFiles(csv_root, "*.csv", SearchOption.TopDirectoryOnly);
            foreach (var csv in csvs)
            {
                if (csv.Contains("--l"))
                {
                    continue;
                }
                var lines = File.ReadAllLines(csv);
                if (lines.Length <= 2) 
                {
                    continue;
                }
                var name = Path.GetFileNameWithoutExtension(csv);
                if (name.StartsWith("DownloadRes")) 
                {
                    continue;
                }
                //
                {
                    bools.Clear();
                    var isQualityData = name == "QualityData";
                    _csv_line_2_cells(lines[1], headers);
                    //bool flag = false;
                    for (var i = 0; i < headers.Count; ++i)
                    {
                        var header = headers[i];
                        //if (header.StartsWith("resist_") || header.StartsWith("resolve_") || header.StartsWith("progress_") || header.EndsWith("_text__s")) 
                        //{
                        //    continue;
                        //}
                        var b = isQualityData || header.Contains("pic") || header.Contains("icon") || header.Contains("res") || header.Contains("_tex") || header == "func_title__s";
                        if (b)
                        {
                            bools.Add(i);
                        }
                    }
                    if (bools.Count == 0)
                    {
                        continue;
                    }
                    //
                    for (var i2 = 2; i2 < lines.Length; ++i2)
                    {
                        var line = lines[i2];
                        _csv_line_2_cells(line, cells);
                        var min = cells.Count;
                        foreach (var n in bools)
                        {
                            if (n >= min)
                            {
                                break;
                            }
                            var cell = cells[n];
                            var header = headers[n];
                            if (cell.Length <= 3 || cell.Length >= 64 || cell.Contains('/'))
                            {
                                continue;
                            }
                            if (cell[0] == '{')
                            {
                                cell = cell.Substring(1, cell.Length - 2);
                            }
                            cell = cell.Replace("\"","");
                            if (xpreg.Matches(cell).Count != cell.Length) 
                            {
                                continue;
                            }
                            if (cell.Contains(','))
                            {
                                foreach (var c in cell.Split(','))
                                {
                                    csv_hash.Add(c);
                                }
                            }
                            else
                            {
                                csv_hash.Add(cell);
                            }
                        }
                    }
                }
            }
            Log.Log2File($"csv_hash={csv_hash.Count}, cost={(System.DateTime.Now.Ticks - time) / 10000}ms");
        }        
        return csv_hash;
    }

    static void _csv_line_2_cells(string line, List<string> cells)
    {
        cells.Clear();
        var idx1 = -1;
        while (true)
        {
            if (idx1 + 1 == line.Length)
            {
                cells.Add(string.Empty);
                break;
            }
            var flag = line[idx1 + 1] == '"';
            if (flag)
            {
                var idx2 = idx1 + 2;
            goto_lable:
                var idx3 = line.IndexOf("\",", idx2);
                var cnt = 0;
                for (var i = idx3 < 0 ? line.Length - 1 : idx3 - 1; i > idx2; --i)
                {
                    if (line[i] == '"')
                    {
                        ++cnt;
                    }
                    else
                    {
                        break;
                    }
                }
                if (idx3 < 0)
                {
                    var len = line.Length - (idx1 + 2);
                    if (cnt % 2 == 1)
                    {
                        --len;
                    }
                    cells.Add(line.Substring(idx1 + 2, len).Replace("\"\"", "\""));
                    break;
                }
                else
                {
                    if (cnt % 2 == 1)
                    {
                        idx2 = idx3 + 1;
                        goto goto_lable;
                    }
                    var len = idx3 - idx1 - 2;
                    if (len > 0)
                    {
                        cells.Add(line.Substring(idx1 + 2, len).Replace("\"\"", "\""));
                    }
                    else
                    {
                        cells.Add(string.Empty);
                    }
                    idx1 = idx3 + 1;
                }
            }
            else
            {
                var idx2 = line.IndexOf(',', idx1 + 1);
                if (idx2 < 0)
                {
                    cells.Add(line.Substring(idx1 + 1));
                    break;
                }
                else
                {
                    var len = idx2 - idx1 - 1;
                    if (len > 0)
                    {
                        cells.Add(line.Substring(idx1 + 1, len));
                    }
                    else
                    {
                        cells.Add(string.Empty);
                    }
                    idx1 = idx2;
                }
            }
        }
    }



    //[MenuItem("Export/复制怪物")]
    static void CopyMonsters() 
    {
        var dir = "Assets/actor/assets/monster/monster_001";
        var dir_copys = dir + "/copys";
        if (!Directory.Exists(dir_copys)) 
        {
            Directory.CreateDirectory(dir_copys);
        }
        var dir_prefabs = dir_copys + "/prefabs";
        if (!Directory.Exists(dir_prefabs))
        {
            Directory.CreateDirectory(dir_prefabs);
        }
        var dir_mats = dir_copys + "/mats";
        if (!Directory.Exists(dir_mats))
        {
            Directory.CreateDirectory(dir_mats);
        }
        var dir_texs = dir_copys + "/texs";
        if (!Directory.Exists(dir_texs))
        {
            Directory.CreateDirectory(dir_texs);
        }


        var copy = 500;
        if (false)
        {
            //复制贴图
            var texs = new string[] { "_d", "_m", "_n" };
            foreach (var t in texs)
            {
                var from = dir + "/tex/tex_monster_001" + t + ".png";
                for (var i = 1; i <= copy; ++i)
                {
                    var to = dir_texs + "/tex_monster_" + (i + 1000) + t + ".png";
                    File.Copy(from, to, true);
                }
            }

            //复制材质
            {
                var from = dir + "/mat/mat_monster_001.mat";
                for (var i = 1; i <= copy; ++i)
                {
                    var to = dir_mats + "/mat_monster_" + (i + 1000) + ".mat";
                    File.Copy(from, to, true);

                    to = dir_mats + "/mat_monster_" + (i + 2000) + ".mat";
                    File.Copy(from, to, true);

                    to = dir_mats + "/mat_monster_" + (i + 3000) + ".mat";
                    File.Copy(from, to, true);
                }
            }
            //复制 prefab
            {
                var from = dir + "/prefab/part_mesh_monster_001.prefab";
                for (var i = 1; i <= copy; ++i)
                {
                    var to = dir_prefabs + "/part_mesh_monster_" + (i + 1000) + ".prefab";//1000+ mat   abs = (prefab+mat+tex)*500
                    File.Copy(from, to, true);

                    to = dir_prefabs + "/part_mesh_monster_" + (i + 2000) + ".prefab";//2000+ mat abs = (prefab+mat)*500 + tex*3
                    File.Copy(from, to, true);

                    to = dir_prefabs + "/part_mesh_monster_" + (i + 3000) + ".prefab";//3000+ mat abs = prefab * 500 + mat * 500 + tex * 3
                    File.Copy(from, to, true);

                    to = dir_prefabs + "/part_mesh_monster_" + (i + 4000) + ".prefab";//3000+ mat abs = prefab * 500 + mat * 500 + tex * 3
                    File.Copy(from, to, true);

                    to = dir_prefabs + "/part_mesh_monster_" + (i + 5000) + ".prefab";//mat   abs = (prefab*500 + mat + tex*3
                    File.Copy(from, to, true);
                }
            }
            AssetDatabase.Refresh();
        }

        if (false)
        {
            //设置材质的贴图
            {
                for (var n = 1; n <= 3; ++n)
                {
                    for (var i = 1; i <= copy; ++i)
                    {
                        var mat = AssetDatabase.LoadAssetAtPath<Material>(dir_mats + "/mat_monster_" + (i + 1000 * n) + ".mat");
                        mat.name = "mat_monster_" + (i + 1000 * n);
                        if (n == 1)
                        {
                            var d = AssetDatabase.LoadAssetAtPath<Texture2D>(dir_texs + "/tex_monster_" + (i + 1000 * n) + "_d.png");
                            mat.SetTexture("_MainTex", d);

                            var b = AssetDatabase.LoadAssetAtPath<Texture2D>(dir_texs + "/tex_monster_" + (i + 1000 * n) + "_n.png");
                            mat.SetTexture("_BumpMap", b);

                            var m = AssetDatabase.LoadAssetAtPath<Texture2D>(dir_texs + "/tex_monster_" + (i + 1000 * n) + "_m.png");
                            mat.SetTexture("_MetallicTex", m);
                        }
                        EditorUtility.SetDirty(mat);
                    }
                }
            }

            //设置prefa的材质
            {
                for (var i = 1; i <= copy; ++i)
                {
                    //1000+ mat   abs = (prefab+mat+tex)*500 + mesh*1 + controller * 1
                    {
                        var go1 = AssetDatabase.LoadAssetAtPath<GameObject>(dir_prefabs + "/part_mesh_monster_" + (i + 1000) + ".prefab");
                        var skin1 = go1.GetComponentInChildren<SkinnedMeshRenderer>();
                        skin1.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(dir_mats + "/mat_monster_" + (i + 1000) + ".mat");
                        EditorUtility.SetDirty(go1);
                    }

                    //2000+ mat  abs = (prefab+mat)*500 + tex*3 + mesh*1 + controller * 1
                    {
                        var go2 = AssetDatabase.LoadAssetAtPath<GameObject>(dir_prefabs + "/part_mesh_monster_" + (i + 2000) + ".prefab");
                        var skin2 = go2.GetComponentInChildren<SkinnedMeshRenderer>();
                        skin2.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(dir_mats + "/mat_monster_" + (i + 2000) + ".mat");
                        EditorUtility.SetDirty(go2);
                    }

                    //3000+ mat abs = prefab * 500 + mat * 500 + tex * 3 + mesh*1 + controller * 1
                    {
                        var go3 = AssetDatabase.LoadAssetAtPath<GameObject>(dir_prefabs + "/part_mesh_monster_" + (i + 3000) + ".prefab");
                        var skin3 = go3.GetComponentInChildren<SkinnedMeshRenderer>();
                        skin3.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(dir_mats + "/mat_monster_" + (i + 3000) + ".mat");
                        EditorUtility.SetDirty(go3);

                        var go4 = AssetDatabase.LoadAssetAtPath<GameObject>(dir_prefabs + "/part_mesh_monster_" + (i + 4000) + ".prefab");
                        var skin4 = go4.GetComponentInChildren<SkinnedMeshRenderer>();
                        skin4.sharedMaterial = skin3.sharedMaterial;
                        EditorUtility.SetDirty(go4);
                    }
                }
            }

            AssetDatabase.SaveAssets();
        }
    }

}