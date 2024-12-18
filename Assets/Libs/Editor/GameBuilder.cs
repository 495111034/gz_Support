using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;
using StringDict = System.Collections.Generic.Dictionary<string, string>;
using System.Reflection;
using Object = UnityEngine.Object;
using UnityEditor.Build.Content;
using System;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;
using UnityEngine.Networking;
using System.Security.Policy;
using System.Text;
using UnityEditor.Build.Reporting;
using resource;

public static class GameBuilder
{
    static string[] SCENE_NAMES = new string[]
    {
        "Assets/main.unity",
    };


    static void SwitchTo(string mode)
    {
        Log.Log2File($"Switch to {mode} start");
#if UNITY_ANDROID
        var os = BuildTargetGroup.Android;
#else
        var os = BuildTargetGroup.iOS;
#endif
        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(os);

            if (mode == "mono")
            {
                ProcessUtils.ExecSystemComm("..\\SetEditorLink_mono.bat unity");
            }
            else 
            {
                ProcessUtils.ExecSystemComm("..\\SetEditorLink.bat unity");
            }
        
        PlayerSettings.SetScriptingDefineSymbolsForGroup(os, defines);
        AssetDatabase.Refresh();
        Log.Log2File($"Switch to {mode} done");
    }


    [MenuItem("Build/切换到 mono")]
    static void SwitchMono()
    {
        SwitchTo("mono");
    }

    [MenuItem("Build/切换到 il2cpp")]
    static void SwitchIL2CPP()
    {
        SwitchTo("il2cpp");
    }

    [MenuItem("Build/复制资源到工程")]
    static void CopyAssets() 
    {
        _copy_assets("E:\\long_new\\client\\GameBase\\Assets\\StreamingAssets\\assetbundles"); 
    }

    static bool _copy_assets(string outpath)
    {
        //var strAssetRoot = Path.GetFullPath(Path.Combine(outpath, "game_nd/src/main/assets/assetbundles"));
        return set_path_copy_assets(outpath);
    }

    public static bool set_path_copy_assets(string strAssetRoot)
    { 
        var configtxt = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "config.txt");
        Log.LogInfo($"打包apk, strAssetRoot={strAssetRoot}, configtxt={configtxt}");

        if (!File.Exists(configtxt))
        {
            Debug.LogError($"打包apk, {configtxt}文件不存在");
            throw new Exception($"打包apk, 错误！");
        }

        //var text = File.ReadAllText(configtxt);        
        //if (text != null) BuilderConfig.ParseStartupParams(text);
        //BuilderConfig.lang_id = lang_id;
        //var lang_id = BuilderConfig.lang_id;

#if !UNITY_IOS
        Log.LogInfo($"打包apk, BuilderConfig.include_assetbundles={BuilderConfig.include_assetbundles}");
        if (BuilderConfig.include_assetbundles == 0)
        {
            Log.LogInfo($"打包apk, BuilderConfig.include_assetbundles 不能设置为 0");
            return false;
        }
#endif

        var res_path_base = Path.GetFullPath((BuilderConfig.res_url).Replace("file://", "").TrimStart('/'));
        Log.LogInfo($"打包apk, lang_id={BuilderConfig.lang_id}, BuilderConfig.res_url={BuilderConfig.res_url}, res_path_base={res_path_base}");
        if (!Directory.Exists(res_path_base))
        {
            Debug.LogError($"打包apk, 资源文件夹{res_path_base}不存在");
            throw new Exception($"打包apk, 错误！");
        }

        Log.LogInfo($"打包apk, 创建目录{strAssetRoot}");
        if (Directory.Exists(strAssetRoot)) 
        {
            Directory.Delete(strAssetRoot, true);
        }
        Directory.CreateDirectory(strAssetRoot);
        var dirs = new string[] { "datas", BuilderConfig.os_name };
        foreach (var dir in dirs)
        {
            var subdir = System.IO.Path.Combine(strAssetRoot, dir);
            Log.LogInfo($"打包apk, 新建资源目录{subdir}");
            Directory.CreateDirectory(subdir);
        }

        var is_cn = BuilderConfig.lang_id == "cn";
        var lang_str = is_cn ? "" : "--l" + BuilderConfig.lang_id;
        var gamelogic_name = BuilderConfig.DLL_NAME;
        var this_filelist_name = $"filelist{lang_str}.txt";
        Log.LogInfo($"打包apk, gamelogic={gamelogic_name}, lang_str={lang_str}, this_filelist_name={this_filelist_name}");

        foreach (var dir in dirs)
        {
            var res_path = System.IO.Path.Combine(res_path_base, dir);
            var StreamingAssetsPath = System.IO.Path.Combine(strAssetRoot, dir);
            Log.LogInfo($"打包apk, will copy dir {res_path} => {StreamingAssetsPath}");
            if (!Directory.Exists(res_path))
            {
                Debug.LogError($"打包apk, 资源子目录{res_path}不存在");
                throw new Exception($"打包apk, 错误！");
            }
            
            var filelist_names = new List<string>();
            var versionpath = Path.Combine(res_path, "version.txt");
            var dst_versionpath = Path.Combine(StreamingAssetsPath, "version.txt");
            var version_lines = File.ReadAllLines(versionpath);
            foreach (var version_line in version_lines)
            {
                if (
                    version_line.Contains("filelist") || version_line.Contains(gamelogic_name)
#if HybridCLR
                    || version_line.Contains("aotassembly")
#endif
                    )
                {
                    var line = version_line;
                    var filelist_name = line.Split('=')[0];
                    if (BuilderConfig.include_assetbundles != 2)
                    {
                        if (BuilderConfig.os_name == dir && version_line.Contains("filelist"))
                        {
                            //复制完整filelist，第一次运行时不用去cdn下载
                            File.AppendAllText( dst_versionpath, "remote_" + line + '\n');
                            Log.LogInfo($"打包apk, copy filelist {"remote_" + line}");
                            File.Copy(Path.Combine(res_path, filelist_name), Path.Combine(StreamingAssetsPath, "remote_" + filelist_name));

                            //只包含部分资源
                            line = line.Substring(0, line.Length - 2);
                            Log.LogInfo($"打包apk, change version {version_line} -> {line}");
                        }
                    }

                    File.AppendAllText(dst_versionpath, line + '\n');                    
                    //
                    if (version_line.Contains("filelist"))
                    {
                        filelist_names.Add(filelist_name);
                    }
                    else
                    {
                        Log.LogInfo($"打包apk, copy file {line}");
                        File.Copy(Path.Combine(res_path, filelist_name), Path.Combine(StreamingAssetsPath, filelist_name));
                    }
                }
            }
            
            HashSet<string> small_apk_filenames = null;
            if (BuilderConfig.os_name == dir && BuilderConfig.include_assetbundles == 1)
            {
                var lang_files = new Dictionary<string, string>();
                if (!is_cn) 
                {
                    var fileList = Path.Combine(res_path, this_filelist_name);
                    var files = File.ReadAllLines(fileList);
                    foreach (var file in files) 
                    {
                        var idx = file.IndexOf(lang_str);
                        if (idx > 0) 
                        {
                            var arr = file.Split(',');
                            var cn = arr[0].Remove(idx, lang_str.Length);
                            lang_files[cn] = arr[0];
                        }
                    }
                }

                small_apk_filenames = new HashSet<string>();
                var csv = "apk.res.csv";
                var lines = File.ReadAllLines(csv);
                for (int i = 0; i < lines.Length; ++i)
                {
                    var arr = lines[i].Split(',');//id,name,size
                    if (arr.Length > 1)
                    {
                        var file = arr[1];
                        if (lang_files.TryGetValue(file, out var lang_file))
                        {
                            file = lang_file;
                        }

                        var srcFile = Path.Combine(res_path, file);
                        if (File.Exists(srcFile))
                        {
                            small_apk_filenames.Add(file);
                        }
                        else 
                        {
                            Log.LogInfo($"打包apk, small_apk_filenames, file not found {i},{lines[i]} -> {file}");
                        }                        
                        {
                            //882 5940
                            var filename = Path.GetFileNameWithoutExtension(file);
                            if (filename.EndsWith("_lod"))
                            {
                                var nolod = file.Replace(filename, filename.Substring(0, filename.Length - 4));
                                srcFile = Path.Combine(res_path, nolod);
                                if (File.Exists(srcFile))
                                {
                                    small_apk_filenames.Add(nolod);
                                }
                            }
                            else
                            {
                                var lod = file.Replace(filename, filename + "_lod");
                                srcFile = Path.Combine(res_path, lod);
                                if (File.Exists(srcFile))
                                {
                                    small_apk_filenames.Add(lod);
                                }
                            }
                        }
                    }
                }
                small_apk_filenames.Add("abs/alldepts.txt");
                small_apk_filenames.Add("abs/mattexnames.txt");
                Log.LogInfo($"打包apk, read from {csv}, lines={lines.Length}, small_apk_filenames={small_apk_filenames.Count}");
            }

            filelist_names.Remove(this_filelist_name);
            filelist_names.Insert(0, this_filelist_name);

            foreach (var filelist_name in filelist_names)
            {
                //var is_cn = !filelist_name.Contains("--l");
                var fileList = Path.Combine(res_path, filelist_name);
                var exist = File.Exists(fileList);
                Log.LogInfo($"filelist={fileList}, exist={exist}");
                if (!exist)
                {
                    var files = Directory.GetFiles(res_path);
                    Log.LogInfo($"{res_path}, files={string.Join(',', files)}");
                }
                var dst_fileList = Path.Combine(StreamingAssetsPath, filelist_name);
                //
                var filelist_lines = new List<string>();
                //
                var thisfilelist = this_filelist_name == filelist_name;
                //
                using (var fl = File.OpenText(fileList))
                {
                    while (!fl.EndOfStream)
                    {
                        var line = fl.ReadLine().Trim();
                        var arr = line.Split(',');//filename,time,size
                        var filename = arr[0];

                        if (small_apk_filenames != null)
                        {
                            if (!small_apk_filenames.Contains(filename))
                            {
                                continue;
                            }
                        }
                        
                        if (BuilderConfig.os_name == dir)
                        {
                            if (thisfilelist)
                            {
                                if (filename.Contains("--l")) 
                                {
                                    if (is_cn || !filename.Contains(lang_str)) 
                                    {
                                        var log = $"{filelist_name}, 1skipfile {filename}";
                                        Log.LogError(log);
                                        throw new Exception(log);
                                    }
                                }
                            }
                            else
                            {
                                //只记录 默认 资源
                                if (filename.Contains("--l"))
                                {
                                    //Log.LogInfo($"{filelist_name}, 2skipfile {filename}");
                                    continue;
                                }
                            }
                        }

                        var dstName = Path.Combine(StreamingAssetsPath, filename);
                        var dstDir = Path.GetDirectoryName(dstName);
                        var srcFile = Path.Combine(res_path, filename);

                        var fileinfo = new FileInfo(srcFile);
                        if (!fileinfo.Exists)
                        {
                            var log = $"打包apk, 资源文件{srcFile}不存在,打包失败";
                            Log.LogError(log);
                            throw new Exception(log);
                        }

                        if (fileinfo.Length != int.Parse(arr[2]))
                        {
                            var log = $"打包apk, 资源文件{srcFile}大小{fileinfo.Length}和filelist的{arr[2]}不匹配";
                            Log.LogError(log);
                            throw new Exception(log);
                        }

                        if (!Directory.Exists(dstDir))
                        {
                            Directory.CreateDirectory(dstDir);
                        }
                        if (!File.Exists(dstName))
                        {
                            File.Copy(srcFile, dstName);
                        }                        
                        filelist_lines.Add(line);
                    }
                }
                Log.LogInfo($"打包apk, copy filelist {dst_fileList}, Count={filelist_lines.Count}/{small_apk_filenames?.Count}");
                File.WriteAllText(dst_fileList, string.Join("\n", filelist_lines));
            }
        }

        Log.LogInfo($"打包apk, copy files done");
        return true;
    }

#if UNITY_IOS
    [MenuItem("Build/Build iOS")]
#endif
    public static void BuildForIos()
    {
        Log.LogInfo($"打包apk, BuildForIos start");

        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);

        AssetDatabase.Refresh();

#if UNITY_IOS
        if (!System.IO.Directory.Exists(BuilderConfig.mBuildMachineRoot))
            System.IO.Directory.CreateDirectory(BuilderConfig.mBuildMachineRoot);
#endif
        PlayerSettings.SetStackTraceLogType(LogType.Error, StackTraceLogType.ScriptOnly);
        PlayerSettings.SetStackTraceLogType(LogType.Assert, StackTraceLogType.ScriptOnly);
        PlayerSettings.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
        PlayerSettings.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        PlayerSettings.SetStackTraceLogType(LogType.Exception, StackTraceLogType.ScriptOnly);


        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.stripEngineCode = false;
        PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.Disabled);//High为最大可能裁剪
        PlayerSettings.applicationIdentifier = BuilderConfig.package_name;
        PlayerSettings.iOS.scriptCallOptimization = ScriptCallOptimizationLevel.SlowAndSafe;
        PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
#if UNITY_IOS
        PlayerSettings.iOS.sdkVersion = BuilderConfig.mBuildDevice == "1" ? iOSSdkVersion.DeviceSDK : iOSSdkVersion.SimulatorSDK;
        PlayerSettings.iOS.applicationDisplayName = BuilderConfig.mDisplayName;
#endif
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.colorSpace = ColorSpace.Gamma;// ColorSpace.Linear;
        PlayerSettings.gpuSkinning = true;
        PlayerSettings.SetGraphicsAPIs(BuildTarget.iOS, new UnityEngine.Rendering.GraphicsDeviceType[] { UnityEngine.Rendering.GraphicsDeviceType.Metal });

        UnityEditor.Rendering.TierSettings ts = new UnityEditor.Rendering.TierSettings();
        ts.hdr = true;
        ts.hdrMode = UnityEngine.Rendering.CameraHDRMode.R11G11B10;
        ts.renderingPath = RenderingPath.Forward;
        ts.realtimeGICPUUsage = UnityEngine.Rendering.RealtimeGICPUUsage.Low;
        UnityEditor.Rendering.EditorGraphicsSettings.SetTierSettings(BuildTargetGroup.iOS, UnityEngine.Rendering.GraphicsTier.Tier3, ts);

        AssetDatabase.Refresh();

        //BuilderConfig.scriptmode = "il2cpp";

        string dllres = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Assets/Res");
        string dllres_bak = dllres + "/../resbak";
        if (System.IO.Directory.Exists(dllres))
        {
            try
            {
                if (System.IO.Directory.Exists(dllres_bak))
                {
                    System.IO.Directory.Delete(dllres_bak, true);
                }
                System.IO.Directory.Move(dllres, dllres_bak);
            }
            catch (System.IO.IOException e1)
            {
                Log.LogError($"打包apk, Move {dllres} error:{e1.Message}");
                throw new Exception($"打包apk, 错误！");
            }
        }
        //ProcessUtils.ExecSystemComm($"rm -rf {dllres} ");

        //BuilderConfig.include_assetbundles = 1;
        

        try
        {
            var mProjPath = "/Users/hanchong/products/" + BuilderConfig.lang_id  + "_" + BuilderConfig.patchmode + "_" + DateTime.Now.ToString("yyyyMMddHHmm");
            Debug.Log($"打包apk, start Build target to {mProjPath}");
            var buildResult = BuildPipeline.BuildPlayer(SCENE_NAMES, mProjPath, BuildTarget.iOS, BuildOptions.None);
            Debug.Log($"打包apk, Build target complete:{buildResult}");
            if (buildResult.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.LogError($"build result:{buildResult.summary.result}");
                throw new Exception($"打包apk, 错误！");
            }
            else 
            {
                _copy_assets(mProjPath);
            }
            // __updateVerstionFile();
        }
        catch (System.Exception e)
        {
            Debug.LogError("打包apk, build error:" + e.Message);
            throw new Exception($"打包apk, 错误！");
        }
        finally
        {
            if (System.IO.Directory.Exists(dllres_bak))
            {
                System.IO.Directory.Move(dllres_bak, dllres);
            }
        }

        AssetDatabase.Refresh();
    }
    /// <summary>
    /// 编译前处理
    /// </summary>
    public static void BuildAndroidBefore(bool by_ide)
    {
        Log.LogInfo($"打包apk, BuildAndroidBefore start");
        AssetDatabase.Refresh();

        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        EditorUserBuildSettings.development = BuilderConfig.debugBuild == "1";
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
        if (EditorUserBuildSettings.development)
        {
            EditorUserBuildSettings.allowDebugging = true;
            EditorUserBuildSettings.connectProfiler = true;
        }
        {
            string aarfile = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Assets\\Plugins\\Android\\233-debug.aar");
            string androidXmlFile = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "Assets\\Plugins\\Android\\AndroidManifest.xml");
            try
            {

                File.Delete(aarfile);
            }
            catch (System.Exception err2)
            {
                Log.LogError($"打包apk, delete debug sdk error,{aarfile}:{err2.Message}");
                throw new Exception($"打包apk, 错误！");
            }
            try
            {

                File.Delete(androidXmlFile);
            }
            catch (System.Exception err2)
            {
                Log.LogError($"打包apk, delete debug sdk error,{androidXmlFile}:{err2.Message}");
            }
        }
        if (EditorUserBuildSettings.development)
        {
            EditorUserBuildSettings.allowDebugging = true;
            PlayerSettings.enableInternalProfiler = true;
            EditorUserBuildSettings.connectProfiler = true;
            PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.Android, Il2CppCompilerConfiguration.Debug);
            EditorUserBuildSettings.exportAsGoogleAndroidProject = true;

            PlayerSettings.SetStackTraceLogType(LogType.Error, StackTraceLogType.ScriptOnly);
            PlayerSettings.SetStackTraceLogType(LogType.Assert, StackTraceLogType.ScriptOnly);
            PlayerSettings.SetStackTraceLogType(LogType.Warning, StackTraceLogType.ScriptOnly);
            PlayerSettings.SetStackTraceLogType(LogType.Log, StackTraceLogType.ScriptOnly);
            PlayerSettings.SetStackTraceLogType(LogType.Exception, StackTraceLogType.ScriptOnly);

            Log.LogInfo($"打包apk, 打包测试版本");
        }
        else
        {
            EditorUserBuildSettings.allowDebugging = false;
            PlayerSettings.enableInternalProfiler = false;
            EditorUserBuildSettings.connectProfiler = false;
            PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.Android, Il2CppCompilerConfiguration.Release);
            EditorUserBuildSettings.exportAsGoogleAndroidProject = true;

            PlayerSettings.SetStackTraceLogType(LogType.Error, StackTraceLogType.ScriptOnly);
            PlayerSettings.SetStackTraceLogType(LogType.Assert, StackTraceLogType.ScriptOnly);
            PlayerSettings.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
            PlayerSettings.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            PlayerSettings.SetStackTraceLogType(LogType.Exception, StackTraceLogType.ScriptOnly);

            Log.LogInfo($"打包apk, 打包正式版本, StackTraceLogType={PlayerSettings.GetStackTraceLogType(LogType.Error)}");

        }


        AssetDatabase.Refresh();

        Log.LogInfo($"打包apk, BuilderConfig.patchmode={BuilderConfig.patchmode}");

        var script_define = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
        if (EditorUserBuildSettings.development)
        {
#if !DEBUG
            script_define += ";DEBUG";
#endif
        }
        else
        {
#if DEBUG
            script_define = script_define.Replace(";DEBUG", ";");
#endif
        }

        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, $"{script_define.Replace(',', ';')}".TrimEnd(';'));
        if (BuilderConfig.IsDebugBuild)
        {
            //debug 模式不能使用dll
            SwitchTo("il2cpp");
        }
        else
        {
            if (!by_ide)
            {
                SwitchTo(BuilderConfig.patchmode);
            }
        }

        AssetDatabase.Refresh();

        PlayerSettings.productName = BuilderConfig.app_name;

        PlayerSettings.MTRendering = true;         //多线程渲染

        PlayerSettings.applicationIdentifier = BuilderConfig.package_name;
        PlayerSettings.Android.forceSDCardPermission = true;
        PlayerSettings.Android.forceInternetPermission = true;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
#if UNITY_2021_1_OR_NEWER
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel22;
#else
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel19;
#endif
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.Android.renderOutsideSafeArea = true;
        PlayerSettings.Android.startInFullscreen = true;


        if (BuilderConfig.patchmode == "mono")
        {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.X86;
        }
        else
        {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
        }

        PlayerSettings.stripEngineCode = false;
        PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.Disabled);//High为最大可能裁剪

        if (BuilderConfig.include_x86bit_so == 1)
        {
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.All;
        }

        PlayerSettings.colorSpace = ColorSpace.Linear;// ColorSpace.Gamma;// ColorSpace.Linear;
        //PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new UnityEngine.Rendering.GraphicsDeviceType[] { UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3 });
        //PlayerSettings.openGLRequireES31 = false;
        //PlayerSettings.openGLRequireES31AEP = false; 
        PlayerSettings.gpuSkinning = true;
        PlayerSettings.Android.keyaliasPass = "android";
        PlayerSettings.Android.keystorePass = "android";
        PlayerSettings.Android.keyaliasName = "androiddebugkey";
        PlayerSettings.Android.keystoreName = Path.GetFullPath("..") + "\\android_builder\\keystory\\debug.keystore";

        UnityEditor.Rendering.TierSettings ts = new UnityEditor.Rendering.TierSettings();
        ts.hdr = true;
        ts.hdrMode = UnityEngine.Rendering.CameraHDRMode.R11G11B10;
        ts.renderingPath = RenderingPath.Forward;
        ts.realtimeGICPUUsage = UnityEngine.Rendering.RealtimeGICPUUsage.Low;
        UnityEditor.Rendering.EditorGraphicsSettings.SetTierSettings(BuildTargetGroup.Android, UnityEngine.Rendering.GraphicsTier.Tier3, ts);

        AssetDatabase.Refresh();

        Log.LogInfo($"打包apk, BuildAndroidBefore done");
    }
    public static void BuildForAndroid_IDE()
    {
        _BuildForAndroid(true);
    }
    public static void BuildForAndroid() 
    {
        _BuildForAndroid(false);
    }

    static void _del_shaders()
    {
        //var keep_shaders = new HashSet<string>();

        void _del_by_path(string path) 
        {
            var del = path;
            //var dels = AssetDatabase.GetDependencies(path, true);
            //foreach (var del in dels)
            if (del.StartsWith("assets/resources/shader/"))
            {
                var dirname = Path.GetFileName(Path.GetDirectoryName(del));
                if (dirname == "projectorshadow")
                {
                    return;
                }
                if (File.Exists(del))
                {
                    Log.Log2File($"del {del}");
                    File.Delete(del);
                }
                var meta = del + ".meta";
                if (File.Exists(meta))
                {
                    Log.Log2File($"del {meta}");
                    File.Delete(meta);
                }
            }
        }

        void _del_by_name(string name) 
        {
            var shader = Shader.Find(name);
            if (shader)
            {
                var path = UnityEditor.AssetDatabase.GetAssetPath(shader).ToLower();
                _del_by_path(path);
            }
        }
        //
        _del_by_name("BF/Scene/TerrainDefault");
        _del_by_name("BF/Scene/UVAniEffect_Quility_Shadow");
        _del_by_name("BF/Scene/Effect");
        _del_by_name("BF/Effect/EffectPacket");
        _del_by_name("KK/Scene/Diffuse");
        _del_by_name("BF/Common/Albedo");
        {
            var path = Path.GetFullPath($"../assetbundles/{BuilderConfig.os_name}/abs/all_shader_variants.assetx.manifest");
            if (!File.Exists(path)) 
            {
                path = path.Replace("\\assetbundles\\", "\\assetbundles2\\");
            }
            var lines = File.ReadAllLines( path );
            foreach (var line in lines) 
            {
                var l = line.Trim().ToLower();
                if (l.EndsWith(".shader")) 
                {
                    _del_by_path(l.Substring(2));
                }
            }
        }
        Resources.UnloadUnusedAssets();
        AssetDatabase.Refresh();        
    }

    [MenuItem("Build/删除apkshader")]
    public static void DelAppShaders()
    {
        _del_shaders();
        PlayerSettings.Android.keystoreName = (Path.GetFullPath("..") +  "/android_builder/keystory/android.keystore").Replace('\\','/');
        PlayerSettings.Android.keyaliasPass = "android";
        PlayerSettings.Android.keystorePass = "android";        
    }
    public static void _BuildForAndroid(bool by_ide)
    {
        AssetDatabase.Refresh();
        try
        {
            string outpath = BuilderConfig.mProjPath.TrimEnd('/');
            if (by_ide)
            {
                BuilderConfig.mProjPath = outpath = (Path.GetDirectoryName(outpath) + "\\long_" + DateTime.Now.ToString("yyMMdd_HHmmss")).Replace('/', '\\');
                BuilderConfig.mProjPath = outpath = "E:\\long_new\\client\\android_builder\\buildscript\\bin\\long_220915_105608";
            }
            Log.LogInfo($"打包apk, BuilderConfig.IsDebugBuild:{BuilderConfig.debugBuild}，输出目录:{outpath}, by_ide={by_ide}");

            BuildAndroidBefore(by_ide);
            _del_shaders();
            //
            var opt = BuildOptions.None;// BuildOptions.AcceptExternalModificationsToPlayer;
            if (BuilderConfig.debugBuild.CompareTo("1") == 0)
            {
                opt = opt | BuildOptions.Development;
            }
            Log.LogInfo($"打包apk, 输出完成, opt={opt}, outpath={outpath}, by_ide={by_ide}");
            var buildResult = BuildPipeline.BuildPlayer(SCENE_NAMES, outpath, BuildTarget.Android, opt);
            if (buildResult.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.LogError($"打包apk, 失败, build result:{buildResult.summary.result}, totalErrors={buildResult.summary.totalErrors}");
            }
            else
            {
                Log.LogInfo($"打包apk, 输出完成。");
                if (!by_ide)
                {
                    _copy_assets(outpath);
                }
            }
            //
            //if (!by_ide)
            {
                var ResourcesDir = Path.GetFullPath($"..\\GameLibrary\\Resources").Replace('/', '\\');
                ProcessUtils.ExecSystemComm($"svn up {ResourcesDir}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("打包apk, build error:" + e.Message + "\n" + e.StackTrace);
        }

        PlayerSettings.SetStackTraceLogType(LogType.Warning, StackTraceLogType.ScriptOnly);
        PlayerSettings.SetStackTraceLogType(LogType.Log, StackTraceLogType.ScriptOnly);

        AssetDatabase.Refresh();
    }

    public static void BuildAndoidPatchFile()
    {
        if (!EditorUserBuildSettings.exportAsGoogleAndroidProject)
        {
            return;
        }

        if (File.Exists(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "config.txt")))
        {
            var text = File.ReadAllText(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "config.txt"));
            var lang_id = BuilderConfig.lang_id;
            if (text != null) BuilderConfig.ParseStartupParams(text);
            BuilderConfig.lang_id = lang_id;
        }
        else
        {
            Debug.LogError("config.txt文件不存在");
            return;
        }

        string outpath = BuilderConfig.mProjPath;
        if (outpath.EndsWith("/"))
            outpath = outpath.Remove(outpath.Length - 1);


        Log.LogInfo($"原目录:{outpath}");

        BuildAndroidAfter(outpath);
    }


    /// <summary>
    /// 编译后处理
    /// </summary>
    /// <param name="outpath"></param>
    public static void BuildAndroidAfter(string outpath)
    {
    }



    [MenuItem("Build/导出第三方android工程")]
    static void MenuExportAndroidProject() 
    {
        ExportAndroidProject(null, true);
    }
    public static string ExportAndroidProject(string outpath, bool is_aab)
    {
        if (BuilderConfig.sdk_type != (int)PlatType.junhai) 
        {
            Log.LogError("BuilderConfig.sdk_type 需要配置为:" + (int)PlatType.junhai);
            return null;
        }

        DelAppShaders();

        void remove_file(string file)
        {
            try
            {
                File.Delete(file);
            }
            catch { }

            if (!file.EndsWith(".meta"))
            {
                try
                {
                    File.Delete(file + ".meta");
                }
                catch { }
            }
        }
        void remove_dir(string dir)
        {
            try
            {
                Directory.Delete(dir, true);
            }
            catch { }
            remove_file(dir + ".meta");
        }

        var client = Path.GetFullPath("..");

        remove_dir(client + "\\GameLibrary\\Plugins\\Android\\androidx");
        remove_dir(client + "\\GameLibrary\\Plugins\\Android\\jar");
        remove_dir(client + "\\GameLibrary\\Plugins\\Android\\ndshare_libs");
        remove_dir(client + "\\GameLibrary\\Plugins\\Android\\ndunisdk_libs");
        remove_dir(client + "\\GameLibrary\\Plugins\\Android\\ndunity_libs");
        remove_dir(client + "\\GameLibrary\\Plugins\\Android\\my_aars");


        ProcessUtils.ExecSystemComm($"del {client}\\GameLibrary\\Plugins\\Android\\*.DISABLED");
        ProcessUtils.ExecSystemComm($"del {client}\\GameLibrary\\Plugins\\Android\\*.DISABLED.meta");

        ProcessUtils.ExecSystemComm($"del {client}\\GameLibrary\\Plugins\\Android\\*.gradle");
        ProcessUtils.ExecSystemComm($"del {client}\\GameLibrary\\Plugins\\Android\\*.gradle.meta");

        ProcessUtils.ExecSystemComm($"del {client}\\GameLibrary\\Plugins\\Android\\*.properties");
        ProcessUtils.ExecSystemComm($"del {client}\\GameLibrary\\Plugins\\Android\\*.properties.meta");

        ProcessUtils.ExecSystemComm($"del {client}\\GameLibrary\\Plugins\\Android\\*.xml");
        ProcessUtils.ExecSystemComm($"del {client}\\GameLibrary\\Plugins\\Android\\*.xml.meta");

        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

        var opt = BuildOptions.UncompressedAssetBundle;
        if (EditorUserBuildSettings.development)
        {
            opt |= BuildOptions.Development;
        }
        PlayerSettings.Android.useCustomKeystore = false;
        PlayerSettings.applicationIdentifier = "com.dhl.en.google";
        EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
        EditorUserBuildSettings.buildAppBundle = true;
        PlayerSettings.SetIl2CppCompilerConfiguration( BuildTargetGroup.Android, Il2CppCompilerConfiguration.Master); 

        if (string.IsNullOrEmpty(outpath))
        {
            outpath = client + "\\android_builder";
        }
        outpath += "\\prj_" + System.DateTime.Now.ToString("yyMMdd_HHmmss");
        //outpath += "\\prj_test";

        Log.LogInfo($"打包apk, opt={opt}, outpath={outpath}");
        var buildResult = BuildPipeline.BuildPlayer(SCENE_NAMES, outpath, BuildTarget.Android, opt);
        Log.LogInfo($"打包apk, result={buildResult.summary.result}, outpath={outpath}");


        var gradlePath = outpath + "\\launcher/build.gradle";
        string ndkPath = null, versionCode = null, versionName = null, fanti_googleImplementation = null;
        foreach (var line in File.ReadAllLines(gradlePath))
        {
            if (ndkPath == null && line.Contains("ndkPath \""))
            {
                ndkPath = line;
            }
            if (versionCode == null && line.Contains(" versionCode "))
            {
                versionCode = line;
            }
            if (versionName == null && line.Contains(" versionName '"))
            {
                versionName = line;
            }
        }
        Log.LogInfo($"ndkPath -> {ndkPath}");
        Log.LogInfo($"versionCode -> {versionCode}");
        Log.LogInfo($"versionName -> {versionName}");

        //复制配置文件 和 命令行脚本
        string[] copys = new string[]
        {
            "gradle/wrapper/gradle-wrapper.jar",

            "gradlew",
            "gradlew.bat",
            "cleanapk_sample.bat",
            "buildapk_sample.bat",
            "buildaab_sample.bat",

            "build.gradle",
            "settings.gradle",

            "launcher/build.gradle",
            "launcher/src/main/AndroidManifest.xml",

            //"unityLibrary/build.gradle",
            "unityLibrary/src/main/AndroidManifest.xml",
            "unityLibrary/src/main/java/com/unity3d/player/UnityPlayerActivity.java",
        };
        outpath += "\\";

        var from = client + "\\android_builder\\AndroidProject\\";
        foreach (var cp in copys)
        {
            File.Copy(from + cp, outpath + cp, true);
        }
        
        var lines = File.ReadAllLines(gradlePath);
        for (var i = 0; i < lines.Length; ++i)
        {
            var line = lines[i];
            if (ndkPath != null && line.Contains("ndkPath \""))
            {
                lines[i] = ndkPath;
                ndkPath = null;
            }
            if (versionCode != null && line.Contains(" versionCode "))
            {
                lines[i] = versionCode;
                versionCode = null;
            }
            if (versionName != null && line.Contains(" versionName '"))
            {
                lines[i] = versionName;
                versionName = null;
            }
            if (fanti_googleImplementation == null && line.Contains("fanti_googleImplementation"))
            {
                fanti_googleImplementation = line;
            }
        }        
        File.WriteAllLines(gradlePath, lines);


        Log.LogInfo($"fanti_googleImplementation -> {fanti_googleImplementation}");

        var gradlePath2 = outpath + "\\unityLibrary/build.gradle";
        var text = File.ReadAllText( gradlePath2 );
        var idx = text.IndexOf("implementation");
        text = text.Insert(idx, "implementation 'androidx.core:core:1.13.1'\n    ");
        text = text.Replace("mergeDebugJniLibFolders", "mergeFanti_googleDebugJniLibFolders");
        text = text.Replace("mergeReleaseJniLibFolders", "mergeFanti_googleReleaseJniLibFolders");
        text += @"
android {
    flavorDimensions ""versionCode""
    productFlavors {
        fanti_google {
            dependencies {
" + fanti_googleImplementation + @"
            }
        }
    }
}

";
        File.WriteAllText(gradlePath2, text );

        //复制 keystore 和 fanti_google 和  asset_base
        string[] copy_dirs = new string[]
        {
            "keystore",
            "launcher\\src\\fanti_google",
            "asset_base",
        };

        void CreateDir(string full)
        {
            if (Directory.Exists(full))
            {
                return;
            }
            CreateDir(Path.GetDirectoryName(full));
            Directory.CreateDirectory(full);
        }

        foreach (var d in copy_dirs)
        {
            var fromd = from + d;
            var files = Directory.GetFiles(fromd, "*", SearchOption.AllDirectories);
            foreach (var f in files)
            {
                var dt = f.Replace(fromd, "");
                var to = outpath + d + dt;
                CreateDir(Path.GetDirectoryName(to));
                if (!f.EndsWith("1.txt"))
                {
                    File.Copy(f, to, true);
                }
            }
        }
        if (BuilderConfig.include_assetbundles > 0)
        {
            if (is_aab)
            {
                set_path_copy_assets(outpath + "asset_base\\src\\main\\assets\\assetpack");
            }
            else 
            {
                set_path_copy_assets(outpath + "unityLibrary\\src\\main\\assets\\assetbundles");
            }            
        }
        if (is_aab)
        {
            ProcessUtils.ExecSystemComm(outpath + "buildaab_sample.bat");
        }
        else 
        {
            ProcessUtils.ExecSystemComm(outpath + "buildapk_sample.bat");
        }        

        Log.LogInfo($"打包apk, {outpath} done");

        return outpath + "\\launcher\\build\\outputs";
    }


    static void buildSoGzips(string soPath, string targetPath, string targetFile)
    {
        var arr = Directory.GetFiles(soPath, "*.so", SearchOption.TopDirectoryOnly);

        Dictionary<string, byte[]> datas = new Dictionary<string, byte[]>();
        DirectoryInfo di = new DirectoryInfo(soPath);

        foreach (FileInfo info in di.GetFiles("*.*", SearchOption.TopDirectoryOnly))
        {
            if (info.Name.Contains("libil2cpp.so"))
                continue;

            datas.Add(info.Name, File.ReadAllBytes(info.FullName));
        }

        var bytes = ZipUtils.Compress(datas);

        File.WriteAllBytes(Path.Combine(targetPath, targetFile), bytes);
    }



#if UNITY_ANDROID
    //[MenuItem("Build/加密 GameLogic.bytes mono")]
#endif
    public static void BuildJiamiGameLogicBytesForMono_menu()
    {
        BuildJiamiGameLogicBytesForMono();
    }
    static void BuildJiamiGameLogicBytesForMono()
    {
        var ori_dllpath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), $"Assets/Res/dlls/android/GameLogic.bytes");
        var datetime = File.GetLastWriteTime(ori_dllpath);
        if (datetime == null)
        {
            throw new Exception("没有找到原始dll文件：" + ori_dllpath);
        }
        bool b1, b2;
        var v1 = _get_dll_version(ori_dllpath, out b1);
        if (string.IsNullOrEmpty(v1) || v1 == "^will_replace_after_gendll$")
        {
            throw new Exception("原始dll文件，错误的PluginMain.dllversion=" + v1 + ", in " + ori_dllpath);
        }
        if (b1)
        {
            throw new Exception("原始dll文件：" + ori_dllpath + " 已经加密过");
        }

        var usedll = ori_dllpath;

        //加密dll文件
        var securedll = System.IO.Path.Combine(Directory.GetCurrentDirectory(), $"Assets/Res/dlls/android/GameLogic_Secure/GameLogic.bytes");
        var xdatetime = File.GetLastWriteTime(securedll);
        if (xdatetime != null && xdatetime.CompareTo(datetime) > 0)
        {
            if (v1 != _get_dll_version(securedll, out b2))
            {
                throw new Exception("混淆dll与原始dll的PluginMain.dllversion不一致！请重新混淆！");
            }
            if (b2)
            {
                throw new Exception("混淆dll文件：" + securedll + " 已经加密过, 请重新混淆！");
            }
            usedll = securedll;
        }

        if (usedll == ori_dllpath)
        {
            Log.LogError("警告：使用没有混淆的原始dll来加密");
        }
        //
        var loader = System.IO.Path.Combine(Directory.GetCurrentDirectory(), $"Assets/Res/dlls/android/GameLogic_Secure/loader.bytes");
        //loader = "F:\\client\\loader\\bin\\Release\\loader_Secure\\loader.dll";
        if (!File.Exists(loader))
        {
            throw new Exception("找不到加密辅助文件: " + loader);
        }
        var new_dllbytes = _gen_logic_bytes(loader, usedll);
        File.WriteAllBytes(ori_dllpath, new_dllbytes);
        UnityEngine.Debug.Log("Make " + ori_dllpath + ", size=" + new_dllbytes.Length + " by " + usedll);
    }

    //[MenuItem("Build/生成 gamelogic.ab mono")]
    public static void BuildGameLogicDllAssetbundsForAndroid_menu()
    {
        BuildGameLogicAb();
    }
    static void BuildGameLogicAb()
    {

#if UNITY_ANDROID
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
#else
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
#endif
        AssetDatabase.Refresh();

        var configpath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), "config.txt");
        Log.LogInfo($"configpath={configpath}");
        if (!File.Exists(configpath))
        {
            Log.LogError("config.txt文件不存在");
            return;
        }
        var lang_id = BuilderConfig.lang_id;
        var text = File.ReadAllText(configpath);
        if (text != null) BuilderConfig.ParseStartupParams(text);
        //keep lang_id
        BuilderConfig.lang_id = lang_id;        

        Log.LogInfo($"lang_id={lang_id}, res_url={BuilderConfig.res_url}");
        var res_path = System.IO.Path.Combine(BuilderConfig.res_url.Replace("file:///", "").Replace('/', '\\'), BuilderConfig.os_name);
        res_path = System.IO.Path.Combine(Directory.GetCurrentDirectory(), res_path);
        Log.LogInfo($"res_path={res_path}");
        if (!Directory.Exists(res_path))
        {
            Log.LogError($"目标文件夹{res_path}不存在");
            return;
        }

        var prefabpath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), $"Assets/Res/prefabs/{BuilderConfig.os_name}/GameLogic.prefab");
        Log.LogInfo($"prefabpath={prefabpath}");

        if (!File.Exists(prefabpath))
        {
            Log.LogError($"{prefabpath} 不存在");
            return;
        }

        //
        var dllpath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), $"Assets/Res/dlls/{BuilderConfig.os_name}/GameLogic.bytes");
        Log.LogInfo($"dllpath={dllpath}");
        if (!File.Exists(dllpath))
        {
            throw new Exception("没有找到dll文件：" + dllpath);
        }

        ////检测dll版本号
        bool b;
        var ori_dllversion = _get_dll_version(dllpath, out b);
        //Log.LogInfo($"原始dllversion={ori_dllversion}");
        if (string.IsNullOrEmpty(ori_dllversion) || ori_dllversion == "^will_replace_after_gendll$")
        {
            throw new Exception("错误的PluginMain.dllversion=" + ori_dllversion + ", in " + dllpath);
        }
        //if (!b)
        //{
        //    //TODO
        //    //throw new Exception("dll还没有加密，请先运行生成加密GameLogic.bytes");
        //}
        var gamelogic = $"gamelogic.ab";
        var logicPath = res_path + "/" + gamelogic;
        if (File.Exists(logicPath))
        {
            Log.LogInfo($"检查当前存在的ab版本号");
            var old_dllversion = _get_ab_version(logicPath, out b);
            //Log.LogInfo($"当前ab文件dllversion={old_dllversion}");
            if (old_dllversion == ori_dllversion)
            {
                Log.LogError("dll和ab文件的版本相同，该dll已经打包过！");
            }
        }

        AssetDatabase.Refresh();

        AssetBundleBuild build = new AssetBundleBuild();
        build.assetBundleName = gamelogic;
        build.assetNames = new string[] { $"Assets/Res/prefabs/{BuilderConfig.os_name}/GameLogic.prefab" };
        build.addressableNames = new string[] { "GameLogic" };
        //
        Log.LogInfo($"build {res_path}/{build.assetBundleName} with {build.assetNames[0]}");
        var opt = BuildAssetBundleOptions.DeterministicAssetBundle;
#if UNITY_ANDROID
        BuildPipeline.BuildAssetBundles(res_path, new AssetBundleBuild[] { build }, opt, UnityEditor.BuildTarget.Android);
#else
        BuildPipeline.BuildAssetBundles(res_path, new AssetBundleBuild[] { build }, opt, UnityEditor.BuildTarget.iOS);
#endif
        if (!File.Exists(logicPath))
        {
            throw new Exception($"生成{logicPath} 失败");
        }

        //Log.LogInfo($"检查ab版本号");
        var ab_dllversion = _get_ab_version(logicPath, out b);
        if (ab_dllversion != ori_dllversion)
        {
            throw new Exception($"生成ab文件的PluginMain.dllversion={ab_dllversion}与dll的PluginMain.dllversion={ori_dllversion}不一致！请运行游戏然后关闭重试！");
        }

        var fileinfo = new FileInfo(logicPath);
        
        Log.LogInfo($"{logicPath}, filesize={fileinfo.Length}, modify={fileinfo.LastWriteTime}, done!");
        //ProcessUtils.ExecPython(System.IO.Path.Combine(res_path, "update.py"));
    }


    static int _idx = 0;
    private static string _get_dll_version(string path, out bool is_encodedll, byte[] bytes = null)
    {       
        if (bytes == null)
        {
            bytes = File.ReadAllBytes(path);
        }
        //
        is_encodedll = _is_encode_logic_bytes(bytes);
        Log.LogInfo($"_get_dll_version, {path}, Length={bytes.Length}, is_encodedll={is_encodedll}");
        if (is_encodedll)
        {
            bytes = _decode_logic_bytes(true, bytes, typeof(UnityEngine.Time), null);
        }

        var path2 = "d:\\tmp.gamelogic.ab.bytes";
        File.WriteAllBytes(path2, bytes);

        var cs_dll = "Assembly-CSharp.dll";
        File.Copy("Library/ScriptAssemblies/" + cs_dll, cs_dll, true);
        var ret = ProcessUtils.ExecSystemComm($"dllversion.exe {path2}");
        Log.LogInfo($"{path}, PluginMain.dllversion={ret}");

        File.Delete(cs_dll);
        File.Delete(path2);
        
        return ret;
    }

    private static string _get_ab_version(string path, out bool is_encodedll)
    {
        string version = null;
        try
        {
            var assetBundle = AssetBundle.LoadFromFile(path);
            Log.LogInfo($"www.assetBundle={assetBundle}, {path}");
            var dllObj = assetBundle.LoadAsset("GameLogic") as GameObject;
            var _ReferenceCollector = dllObj.GetComponent<ReferenceCollector>();
            var GameLogic = _ReferenceCollector.Get<TextAsset>("GameLogic");
            var ab_bytes = GameLogic.bytes;
            //File.WriteAllBytes("d://tmp.GameLogic.bytes", ab_bytes);
            version = _get_dll_version(path, out is_encodedll, ab_bytes);
            assetBundle.Unload(true);
        }
        catch (Exception e)
        {
            Log.LogError($"读取{path}失败！请运行游戏然后关闭重试！");
            throw new Exception(e.Message + "\n" + e.StackTrace);
        }
        return version;
    }

    static string tips = "Hello World!";
    private static bool _is_encode_logic_bytes(byte[] bytes)
    {
        var len = bytes[0];
        return len > 0 && tips == Encoding.ASCII.GetString(bytes, 1, len);
    }

    private static byte[] _decode_logic_bytes(bool is_builder, byte[] bytes, Type type_time, List<object> logs)
    {
        var tips_len = bytes[0];
        var self_idx = 1 + tips_len;
        var _decode_byte = bytes[self_idx++];
        var self_len = BitConverter.ToUInt16(bytes, self_idx);
        var asm_idx = self_idx + 2 + self_len;

        var asm_bytes = new byte[bytes.Length - asm_idx];
        Array.Copy(bytes, asm_idx, asm_bytes, 0, asm_bytes.Length);

        var seed_pos = 913020191220233L;
        var time_pos = 913020191221233L;
        var tick_pos = 913020191222233L;
        //
        var decode__ = 913020191223233L;
        var checksum = 913020191224233L;

        int checksum_local = 0;
        var decode = (byte)decode__;
        if (is_builder)
        {
            decode = _decode_byte;
            Log.LogInfo($"_decode_logic_bytes, seed={decode}");
        }
        for (var i = 1000; i < asm_bytes.Length; ++i)
        {
            asm_bytes[i] ^= decode++;
            checksum_local += asm_bytes[i];
        }
        if (!is_builder)
        {
            if (checksum != checksum_local)
            {
                throw new Exception("1");
            }
            var seed = new System.Random().Next() * 31415L;
            var seeds = BitConverter.GetBytes(seed + 31415926);
            Array.Copy(seeds, 0, asm_bytes, seed_pos, seeds.Length);

            var time = seed ^ (long)((float)type_time.GetProperty("realtimeSinceStartup").GetValue(null) * 1000);
            var times = BitConverter.GetBytes((long)(time));
            Array.Copy(times, 0, asm_bytes, time_pos, times.Length);

            var tick = seed ^ System.DateTime.UtcNow.Ticks;
            var ticks = BitConverter.GetBytes(tick);
            Array.Copy(ticks, 0, asm_bytes, tick_pos, ticks.Length);
        }
        return asm_bytes;
    }

    static private byte[] _gen_logic_bytes(string loader, string usedll)
    {
        var loader_bytes = File.ReadAllBytes(loader);
        var logic_bytes = File.ReadAllBytes(usedll);

        if (loader_bytes.Length >= UInt16.MaxValue)
        {
            throw new Exception($"loader {loader} too large");
        }

        var seed = UnityEngine.Random.Range(99, 255);

        for (var i = 0; i < 5; i++)
        {
            var find = 913020191220233L + i * 1000;
            var pos1 = _findlong(loader_bytes, find, $"loader_bytes {i}");

            int pos2 = 0;
            if (i < 3)
            {
                pos2 = _findlong(logic_bytes, find, $"logic_bytes {i}");
                _setlong(logic_bytes, pos2, 2339130 * 10 + i, $"logic_bytes {i}");
            }
            else if (i == 3)
            {
                pos2 = seed;
            }
            else
            {
                byte decodeb = (byte)seed;
                for (int n = 1000; n < logic_bytes.Length; ++n)
                {
                    pos2 += logic_bytes[n];
                    logic_bytes[n] ^= decodeb++;
                }
            }
            _setlong(loader_bytes, pos1, pos2, $"loader_bytes {i}");
        }


        var tips_bytes = Encoding.ASCII.GetBytes(tips);

        //tipslen, tips, seed, loaderlen, loader, dllbytes
        var bytes = new byte[1 + tips_bytes.Length + 1 + 2 + loader_bytes.Length + logic_bytes.Length];

        //write tips_bytes
        var idx = 0;
        bytes[idx++] = (byte)tips.Length;
        Array.Copy(tips_bytes, 0, bytes, idx, tips_bytes.Length);
        idx += tips_bytes.Length;

        //
        bytes[idx++] = (byte)seed;
        //Log.LogInfo($"seed={seed}");
        Log.LogInfo($"_gen_logic_bytes, seed={seed}");

        //write loader_bytes
        var lbytes = BitConverter.GetBytes((UInt16)loader_bytes.Length);
        Array.Copy(lbytes, 0, bytes, idx, lbytes.Length);
        idx += lbytes.Length;
        Array.Copy(loader_bytes, 0, bytes, idx, loader_bytes.Length);
        idx += loader_bytes.Length;

        //write logic_bytes
        Array.Copy(logic_bytes, 0, bytes, idx, logic_bytes.Length);
        idx += logic_bytes.Length;

        return bytes;
    }

    static void _setlong(byte[] bytes, int idx, long find, string memo)
    {
        //Log.LogInfo("at idx={0} set {1}, {2}", idx, find, memo);
        byte[] finds = BitConverter.GetBytes(find);
        Array.Copy(finds, 0, bytes, idx, finds.Length);
    }

    static int _findlong(byte[] bytes, long find, string memo)
    {
        int idx = -1;
        byte[] finds = BitConverter.GetBytes(find);
        for (int n = 0; n < bytes.Length - finds.Length; ++n)
        {
            int i = 0;
            for (; i < finds.Length; ++i)
            {
                if (bytes[n + i] != finds[i])
                {
                    break;
                }
            }
            if (i == finds.Length)
            {
                if (idx >= 0)
                {
                    throw new Exception($"{memo} find {find} not unique");
                }
                idx = n;
            }
        }
        if (idx < 0)
        {
            throw new Exception($"{memo} not find {find}");
        }
        return idx;
    }


    [MenuItem("Build/生成 My Obb文件")]
    public static void BuildAndroidMyObbFile_menu()
    {
        BuildAndroidMyObbFile();
    }

    static ArrayList _obb_infos = new ArrayList();
    static byte[] _obb_buffer = null;
    static int _obb_buffer_offset = 0;
    static int AddFileToZipStream(string filePath, string entryName, int mtime, Stream outputStream)
    {
        if (_obb_buffer == null)
        {
            _obb_buffer = new byte[1024 * 1024 * 32]; 
        }
        using (var fd = File.OpenRead(filePath))
        {
            int filesize = (int)fd.Length;
            _obb_infos.Add(new ArrayList() { entryName, outputStream.Position + _obb_buffer_offset, filesize, mtime });
            //outputStream.Write(bytes,0,bytes.Length);

            Debug.Assert(_obb_buffer.Length > filesize, $"max={_obb_buffer.Length}, file too large, size={filesize}, {filePath}");

            if (_obb_buffer_offset + filesize >= _obb_buffer.Length)
            {
                Flush_obb_buffer(outputStream);
            }

            //var t1 = DateTime.UtcNow.Ticks;
            fd.Read(_obb_buffer, _obb_buffer_offset, filesize);
            //var t2 = DateTime.UtcNow.Ticks;
            //if (t2 - t1 > 10000)
            //{
            //    Log.LogInfo($"read={filesize}, cost={(t2 - t1) / 10000f}ms, {filePath}");
            //}
            _obb_buffer_offset += filesize;
            return filesize;
        }        
    }
    static void Flush_obb_buffer(Stream outputStream)
    {
        if (_obb_buffer_offset > 0)
        {
            //var t1 = DateTime.UtcNow.Ticks;
            outputStream.Write(_obb_buffer, 0, _obb_buffer_offset);
            //var t2 = DateTime.UtcNow.Ticks;
            //Log.LogInfo($"flush={_obb_buffer_offset}, cost={(t2-t1)/10000f}ms, {outputStream.Position/1024/1024}");
            _obb_buffer_offset = 0;
        }
    }
    public static void BuildAndroidMyObbFile()
    {
        return;
        AssetDatabase.Refresh();

        var res_url = BuilderConfig.res_url;
        var target_zip_file = "";
        if (File.Exists(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "config.txt")))
        {
            var text = File.ReadAllText(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "config.txt"));
            var build_app_lang_id = BuilderConfig.build_app_lang_id;
            if (text != null) BuilderConfig.ParseStartupParams(text);
            BuilderConfig.lang_id = build_app_lang_id;
            if (!Directory.Exists(BuilderConfig.res_url))
            {
                BuilderConfig.res_url = res_url;
            }
        }

        var res_path_base = BuilderConfig.res_url.Replace("file:///", "");
        Log.LogInfo($"res_path={res_path_base}, BuilderConfig.lang_id={BuilderConfig.lang_id}");

        if (!Directory.Exists(res_path_base))
        {
            Debug.LogError($"资源文件夹{res_path_base}不存在");
            return;
        }

        var lang_str = BuilderConfig.lang_id == "cn" ? "" : "--l" + BuilderConfig.lang_id;

        

        Log.LogInfo($"目标文件：{target_zip_file}");
        if (File.Exists(target_zip_file))
        {
            Log.LogInfo($"删除已有目标文件");
            File.Delete(target_zip_file);
        }
        using (var outputStream_ = File.OpenWrite(target_zip_file))
        {
            _obb_infos.Clear();

            var _obbsize = new byte[8];
            outputStream_.Write(_obbsize, 0, _obbsize.Length);

            var dirs = new string[] { "android", "datas" };
            foreach (var dir in dirs)
            {
                var res_path = Path.Combine(res_path_base, dir);
                var versionpath = Path.Combine(res_path, "version.txt");
                var lines = File.ReadAllLines(versionpath);
                var versions = new Dictionary<string, int>();
                foreach (var line in lines) 
                {
                    var arr = line.Split('=');
                    versions[arr[0]] = int.Parse( arr[1] );
                }

                AddFileToZipStream(versionpath, $"{dir}/version.txt", 0, outputStream_);
                //
                var filelistname = $"filelist{lang_str}.txt";
                var fileList = Path.Combine(res_path, filelistname);
                Log.LogInfo($"add fileList={fileList}");
                AddFileToZipStream(fileList, $"{dir}/{filelistname}", versions[filelistname], outputStream_);

                var gamelogics =  new string[] { "mono.ab", "aotassembly.ab", "hybridclr.ab" };
                foreach (var gamelogic in gamelogics)
                {
                    var gamelogicpath = Path.Combine(res_path, gamelogic);
                    if (File.Exists(gamelogicpath))
                    {
                        Log.LogInfo($"add {gamelogic}");
                        AddFileToZipStream(gamelogicpath, $"{dir}/{gamelogic}", versions[gamelogic], outputStream_);
                    }
                }

                //AddFileToZipStream(Path.Combine(res_path, $"privatedata{lang_str}.ab"), "privatedata.ab", outputStream_);
                Flush_obb_buffer(outputStream_);

                var position = outputStream_.Position;
                var appendtotal = 0L;
                using (var fl = File.OpenText(Path.Combine(res_path, fileList)))
                {
                    while (!fl.EndOfStream)
                    {
                        var line = fl.ReadLine();
                        var fileinfo = line.Split(',');
                        var filename = fileinfo[0];
                        var mtime = fileinfo[1];
                        var filesize = int.Parse(fileinfo[2]);

                        var srcFile = Path.Combine(res_path, filename);
                        //if (filename.EndsWith(".gz"))
                        //{
                        //    continue;
                        //}
                        if (!File.Exists(srcFile))
                        {
                            Log.LogError($"{srcFile} 文件不存在,打包失败");
                            return;
                        }
                        var realsize = AddFileToZipStream(srcFile, filename, int.Parse(mtime), outputStream_);
                        if (realsize != filesize)
                        {
                            Log.LogError($"{srcFile}, filelist文件大小={filesize}, 实际文件大小={realsize}");
                            return;
                        }
                        appendtotal += realsize;
                    }
                }

                Flush_obb_buffer(outputStream_);
                var realappend = outputStream_.Position - position;
                Log.LogInfo($"filelist资源文件大小={appendtotal}, 实际资源文件大小={realappend}");
                if (realappend != appendtotal)
                {
                    Log.LogError("写入错误");
                    return;
                }
            }

            long obbsize = outputStream_.Position - _obbsize.Length;
            _obbsize = BitConverter.GetBytes(obbsize);

            var json = MiniJSON.JsonEncode(_obb_infos);
            var json_bytes = Encoding.UTF8.GetBytes(json);
            outputStream_.Write(json_bytes, 0, json_bytes.Length);

            outputStream_.Seek(0, SeekOrigin.Begin);
            outputStream_.Write(_obbsize, 0, _obbsize.Length);

            outputStream_.Flush();
        }
        Log.LogInfo($"done2");
    }

    public static void Build_IOS()
    {
        BuildTarget target = BuildTarget.iOS;
        BuildTarget activeTarget = EditorUserBuildSettings.activeBuildTarget;
        if (activeTarget != BuildTarget.iOS)
        {
            Debug.LogError("请先切到iOS平台再打包");
            return;
        }
        // Get filename.
        var buildOptions = BuildOptions.None;

        string location = "/Users/nd/Desktop/SVNProject/long_xcode";
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions()
        {
            scenes = SCENE_NAMES,
            locationPathName = location,
            options = buildOptions,
            target = target,
            targetGroup = BuildTargetGroup.iOS,
        };

        BuildPipeline.BuildPlayer(buildPlayerOptions);
    }
}
