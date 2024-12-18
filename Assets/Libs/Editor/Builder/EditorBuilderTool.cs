using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;
using System;
using UnityEditor.SceneManagement;
using UnityEditor.Compilation;

public class EditorBuilderTool
{
#if GOOGLE
    [MenuItem("Build/测试aab打包")]
    public static void TestBuildAAB()
    {
        EditorUserBuildSettings.buildAppBundle = true;
#if UNITY_IOS
        string outputPath = $"{HybridCLR.Editor.SettingsUtil.ProjectDir}/Release_Xcode";
#elif HybridCLR
        string outputPath = $"{HybridCLR.Editor.SettingsUtil.ProjectDir}/ReleaseBuilder";
#else
        string outputPath = $"ReleaseBuilderAAB";
#endif
        if (Directory.Exists(outputPath))
        {
            //Directory.Delete(outputPath, true);
        }
        var buildOptions = BuildOptions.CompressWithLz4;
#if UNITY_IOS
        string location = $"{outputPath}/nd_xcode";
#else
        string location = $"{outputPath}/{BuilderConfig.lang_id}_{DateTime.Now.ToString("yyMMdd_HHmmss")}.aab";
#endif
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions()
        {
            scenes = new string[] { "Assets/main.unity" },
            locationPathName = location,
            options = buildOptions,
            target = EditorUserBuildSettings.activeBuildTarget,
#if UNITY_ANDROID
            targetGroup = BuildTargetGroup.Android,
#elif UNITY_IOS
                        targetGroup = BuildTargetGroup.iOS,
#else
                        targetGroup = BuildTargetGroup.Standalone,
#endif
        };

        string asset_path = Application.dataPath.Replace("Assets", "assetbundles");
        Log.Log2File($"InstallTime asset_path={asset_path}");
        if (Directory.Exists(asset_path))
        {
            Directory.Delete(asset_path, true);
        }
#if HybridCLR
        GameBase.GameLoader.GenGameLogicAB();
#endif
        if (!Directory.Exists(asset_path))
        {
            Directory.CreateDirectory(asset_path);
        }
        bool b = GameBuilder.set_path_copy_assets(asset_path);
        if (!b)
        {
            Debug.LogError("资源复制失败");
            return;
        }
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

        if (Directory.Exists(asset_path))
        {
            PlayerSettings.Android.keyaliasPass = "android";
            PlayerSettings.Android.keystorePass = "android";
            var assetPackConfig = new Google.Android.AppBundle.Editor.AssetPackConfig();
            assetPackConfig.AddAssetsFolder("asset_base",
                                            asset_path,
                                            Google.Android.AppBundle.Editor.AssetPackDeliveryMode.InstallTime);
            Google.Android.AppBundle.Editor.AssetPacks.AssetPackConfigSerializer.SaveConfig(assetPackConfig);
            Google.Android.AppBundle.Editor.Bundletool.BuildBundle(buildPlayerOptions, assetPackConfig);
        }
        //PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, out string[] defines);
        //defines = GetAddDefine("HybridCLR", defines);
        //if (BuilderConfig.is_aab == 1)
        //{
        //    defines = GetAddDefine("GOOGLE", defines);
        //}
        //if (BuilderConfig.IsDebugBuild)
        //{
        //    defines = GetAddDefine("DEBUG", defines);
        //}
        //PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, defines);

        //AssetDatabase.SaveAssets();
        //BuilderApk();
    }
#endif

    public static void RequestScriptReload()
    {
        CompilationPipeline.RequestScriptCompilation(RequestScriptCompilationOptions.CleanBuildCache);

        string hotfixPath = "../hotfixtodll";
        string cmd = Application.dataPath.Replace("Assets", "") + hotfixPath + $"/mn_android.bat";
        string content = File.ReadAllText(cmd, System.Text.UTF8Encoding.UTF8);
        content = content.Replace("pause", "");
        File.WriteAllText(cmd, content, System.Text.UTF8Encoding.UTF8);
        System.Diagnostics.Process proc = System.Diagnostics.Process.Start(cmd);
        proc.WaitForExit();
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
    }

    public static void GenHotDllAssetbundle()
    {
        string full_dll_path = Application.dataPath.Replace("Assets", "Assets/Plugins/dlls");
        string[] files = Directory.GetFiles(full_dll_path, "*", SearchOption.AllDirectories);
        foreach (var f in files)
        {
            if (f.EndsWith(".dll") || f.EndsWith(".dll.meta") || f.EndsWith(".bytes") || f.EndsWith(".bytes.meta"))
            {
                File.Delete(f);
            }
        }

        //CompilationPipeline.RequestScriptCompilation(RequestScriptCompilationOptions.CleanBuildCache);
        AssetDatabase.Refresh();
        Log.LogInfo("Refresh done");

        var type = typeof(MyComponent).Assembly.GetType("GameBase.GameLoader");
        if (type != null)
        {
            var method = type.GetMethod("GenGameLogic", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (method != null)
            {
                method.Invoke(type, null);
            }
        }

        if (type != null)
        {
            var method = type.GetMethod("GenMetadataForAOTAssembly", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (method != null)
            {
                method.Invoke(type, null);
            }
        }
    }

    public static void CollectAllShaderVariants() //收集变体
    {
        EditorSceneManager.OpenScene("Assets/scene/demo_1/demo_001.unity");
        AssetbundleBuilder.CollectAllShaderVariants();
    }

    public static void Builder()
    {
#if UNITY_ANDROID
        Dictionary<string, string> key_value = new Dictionary<string, string>();
        string[] args = System.Environment.GetCommandLineArgs();
        if (args != null && args.Length > 0)
        {
            foreach (var line0 in args)
            {
                string line = line0.Trim('"');
                if (line.Contains(';'))
                {
                    string[] te = line.Split(';');
                    foreach (var line1 in te)
                    {
                        if (line1.Contains('='))
                        {
                            string[] te1 = line1.Split('=');
                            key_value[te1[0].Trim()] = te1[1].Trim().Trim(' ').Trim('\n');
                        }
                    }
                }
                else if (line.Contains('='))
                {
                    string[] te = line.Split('=');
                    key_value[te[0].Trim()] = te[1].Trim().Trim(' ').Trim('\n');
                }
            }
            key_value["local_version"] = DateTime.Now.ToString("yyMMddHHmmss");

            string content = File.ReadAllText(Application.dataPath + "/Libs/utils/data/BuilderConfig.cs", System.Text.UTF8Encoding.UTF8);
            StringBuilder sb = new StringBuilder();
            StringReader sr = new StringReader(content);
            bool is_start = false;
            while (sr.Peek() > 1)
            {
                string line = sr.ReadLine();
                if (!is_start)
                {
                    if (line.Contains("#region auto"))
                    {
                        is_start = true;
                    }
                }
                else
                {
                    if (line.Contains("#endregion auto"))
                    {
                        is_start = false;
                    }
                    else
                    {
                        foreach (var kv in key_value)
                        {
                            int index = line.IndexOf(kv.Key);
                            if (index > -1)
                            {
                                int end = line.IndexOf(';');
                                if (line.Contains(" int "))
                                {
                                    float.TryParse(kv.Value, out float v);
                                    line = line.Substring(0, index + kv.Key.Length) + " = " + (int)v + line.Substring(end);
                                }
                                else if (line.Contains(" bool "))
                                {
                                    string v = "false";
                                    if (kv.Value.ToLower() == "true" || kv.Value == "1")
                                    {
                                        v = "true";
                                    }
                                    line = line.Substring(0, index + kv.Key.Length) + " = " + v + line.Substring(end);
                                }
                                else
                                {
                                    line = line.Substring(0, index + kv.Key.Length) + " = " + $"\"{kv.Value}\"" + line.Substring(end);
                                }
                                break;
                            }
                        }
                    }
                }
                sb.AppendLine(line);
            }
            sr.Close();
            sr.Dispose();
            File.WriteAllText(Application.dataPath + "/Libs/utils/data/BuilderConfig.cs", sb.ToString(), System.Text.UTF8Encoding.UTF8);
        }

        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        {
            string content = File.ReadAllText(Application.dataPath.Replace("Assets", "Packages/manifest.json"), System.Text.UTF8Encoding.UTF8);
            StringBuilder sb = new StringBuilder();
            StringReader sr = new StringReader(content);
            bool is_already = false;
            bool is_has = false;
            while (sr.Peek() > 1)
            {
                string line = sr.ReadLine();
                if (line.Contains("hybridclr_unity"))
                {
                    if (key_value.ContainsKey("patchmode") && key_value["patchmode"].Equals("huatuo"))
                    {
                        is_has = true;
                    }
                    else
                    {
                        continue;
                    }
                }
                if (!is_has && !is_already && line.Contains("com."))
                {
                    if (!line.Contains("hybridclr_unity") && key_value.ContainsKey("patchmode") && key_value["patchmode"].Equals("huatuo"))
                    {
                        is_already = true;
                        //"com.focus-creative-games.hybridclr_unity": "file:../HybridCLRData/hybridclr_unity",
                        sb.AppendLine("\"com.code-philosophy.hybridclr\": \"file:../HybridCLRData/hybridclr_unity\",");
                    }
                }
                if (line.Contains("com.unity.ads") || line.Contains("com.unity.analytics"))
                {
                    continue;
                }
                sb.AppendLine(line);
            }
            sr.Close();
            sr.Dispose();
            if (!is_has)
            {
                File.WriteAllText(Application.dataPath.Replace("Assets", "Packages/manifest.json"), sb.ToString(), System.Text.UTF8Encoding.UTF8);
            }
        }

        string qa_action = Application.streamingAssetsPath + "/qa_action";
        if (Directory.Exists(qa_action))
        {
            Directory.Delete(qa_action, true);
        }
        if (key_value.ContainsKey("qa_action") && key_value["qa_action"].Equals("1"))
        {
            Directory.CreateDirectory(qa_action);
            string[] files = Directory.GetFiles("../GameLibrary/QASuperMan", "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if (file.Contains(".bytes") && !file.Contains("QACodeApkDll"))
                {
                    continue;
                }
                File.Copy(file, qa_action + "/" + Path.GetFileName(file));
            }
        }

        PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, out string[] defines);
        if (key_value.ContainsKey("patchmode") && key_value["patchmode"].Equals("huatuo"))
        {
            defines = GetAddDefine("HybridCLR", defines);
        }
        else
        {
            defines = GetRemoveDefine("HybridCLR", defines);
        }
        if (key_value.ContainsKey("sdk_package") && key_value["sdk_package"] == "huawei")
        {
            defines = GetAddDefine("HUAWEI", defines);
        }
        else
        {
            defines = GetRemoveDefine("HUAWEI", defines);

            if (key_value.ContainsKey("is_aab") && key_value["is_aab"].Equals("1"))
            {
                defines = GetAddDefine("GOOGLE", defines);
            }
            else
            {
                defines = GetRemoveDefine("GOOGLE", defines);
            }
        }
        if (key_value.ContainsKey("debugBuild") && key_value["debugBuild"].Equals("1"))
        {
            defines = GetAddDefine("DEBUG", defines);
        }
        else
        {
            defines = GetRemoveDefine("DEBUG", defines);
        }
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, defines);

        AssetDatabase.SaveAssets();

        CompilationPipeline.RequestScriptCompilation(RequestScriptCompilationOptions.CleanBuildCache);

        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
#endif
    }

    public static void BuilderApk()
    {
        try
        {
            _doBuilderApk();
        }
        catch (Exception e)
        {
            var msg = $"{e.GetType().FullName}:BuilderApk, {e.Message}\n{e.StackTrace}";
            Log.LogError(msg);
            Console.WriteLine(msg);
            throw e;
        }
    }

    static void _doBuilderApk()
    {
        Debug.Log("前置设置完成，开始编译apk");
        //BuilderConfig.local_version = DateTime.Now.ToString("yyMMddHHmmss");
        string lang_id = BuilderConfig.lang_id;

        string[] args = System.Environment.GetCommandLineArgs();
        Dictionary<string, string> key_value = new Dictionary<string, string>();
        if (args != null && args.Length > 0)
        {
            foreach (var line in args)
            {
                if (line.Contains(';'))
                {
                    string[] te = line.Split(';');
                    foreach (var line1 in te)
                    {
                        if (line1.Contains('='))
                        {
                            string[] te1 = line1.Split('=');
                            key_value[te1[0].Trim()] = te1[1];
                        }
                    }
                }
                else if (line.Contains('='))
                {
                    string[] te = line.Split('=');
                    key_value[te[0].Trim()] = te[1];
                }
            }
        }

        string patch_model = BuilderConfig.patchmode;
        if (key_value.ContainsKey("patchmode"))
        {
            patch_model = key_value["patchmode"];
        }
        bool debugBuild = false;
        if (key_value.ContainsKey("debugBuild") && key_value["debugBuild"].Equals("1"))
        {
            debugBuild = true;
        }

        EditorUserBuildSettings.buildAppBundle = BuilderConfig.is_aab == 1;
        
        PlayerSettings.productName = BuilderConfig.app_name;
        PlayerSettings.bundleVersion = BuilderConfig.versionName;
        PlayerSettings.Android.bundleVersionCode = BuilderConfig.versionCode;
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, BuilderConfig.package_name);

        PlayerSettings.gpuSkinning = true;
        if (patch_model != "mono")
        {
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7 | AndroidArchitecture.ARM64;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.Low);
            if (debugBuild)
            {
                PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.Android, Il2CppCompilerConfiguration.Debug);
            }
            else
            {
                PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.Android, Il2CppCompilerConfiguration.Release);
            }
        }
        else
        {
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.Mono2x);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARMv7;
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.Disabled);
            PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.Android, Il2CppCompilerConfiguration.Release);
        }


        EditorUserBuildSettings.development = debugBuild;
        EditorUserBuildSettings.connectProfiler = debugBuild;
        EditorUserBuildSettings.buildWithDeepProfilingSupport = debugBuild;
        EditorUserBuildSettings.switchHTCSScriptDebugging = debugBuild;

        PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android, ApiCompatibilityLevel.NET_Unity_4_8);
        PlayerSettings.gcIncremental = false;
        PlayerSettings.allowUnsafeCode = true;
        PlayerSettings.stripEngineCode = false;
        
        PlayerSettings.Android.useCustomKeystore = true;
        PlayerSettings.Android.keystoreName = Path.GetFullPath("..") + "\\android_builder\\keystory\\android.keystore";
        PlayerSettings.Android.keystorePass = "android";
        PlayerSettings.Android.keyaliasName = "android.keystore";
        PlayerSettings.Android.keyaliasPass = "android";

        PlayerSettings.SetStackTraceLogType(LogType.Error, StackTraceLogType.ScriptOnly);
        PlayerSettings.SetStackTraceLogType(LogType.Assert, StackTraceLogType.ScriptOnly);
        PlayerSettings.SetStackTraceLogType(LogType.Exception, StackTraceLogType.ScriptOnly);
        if (BuilderConfig.IsDebugBuild)
        {
            PlayerSettings.SetStackTraceLogType(LogType.Warning, StackTraceLogType.ScriptOnly);
            PlayerSettings.SetStackTraceLogType(LogType.Log, StackTraceLogType.ScriptOnly);
            Log.LogInfo($"打包apk, 打包测试版本");
        }
        else
        {
            PlayerSettings.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
            PlayerSettings.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Log.LogInfo($"打包apk, 打包正式版本, StackTraceLogType={PlayerSettings.GetStackTraceLogType(LogType.Error)}");
        }

        UnityEditor.Rendering.TierSettings ts = new UnityEditor.Rendering.TierSettings();
        ts.hdr = true;
        ts.hdrMode = UnityEngine.Rendering.CameraHDRMode.R11G11B10;
        ts.renderingPath = RenderingPath.Forward;
        ts.realtimeGICPUUsage = UnityEngine.Rendering.RealtimeGICPUUsage.Low;
        UnityEditor.Rendering.EditorGraphicsSettings.SetTierSettings(BuildTargetGroup.Android, UnityEngine.Rendering.GraphicsTier.Tier3, ts);



        string asset_path = Application.streamingAssetsPath + "/assetbundles";
        if (Directory.Exists(asset_path))
        {
            Directory.Delete(asset_path, true);
        }
        if (BuilderConfig.platType != PlatType.junhai)
        {
            var logs = PlayerSettings.SplashScreen.logos;
            var log = logs[0];
            log.logo = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/apkicon/LOGO.png");
            logs[0] = log;
            PlayerSettings.SplashScreen.logos = logs;
            ColorUtility.TryParseHtmlString("#231F20", out Color color);
            PlayerSettings.SplashScreen.backgroundColor = color;
            PlayerSettings.SplashScreen.unityLogoStyle = PlayerSettings.SplashScreen.UnityLogoStyle.LightOnDark;

            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
            if (BuilderConfig.is_aab == 1)
            {
                asset_path = Application.dataPath.Replace("Assets", "assetbundles");
                if (Directory.Exists(asset_path))
                {
                    Directory.Delete(asset_path, true);
                }
            }
            //var type = typeof(MyComponent).Assembly.GetType("GameBase.GameLoader");
            if (BuilderConfig.include_assetbundles != 0)
            {
                //if (key_value.ContainsKey("dll") && key_value["dll"].Equals("1"))
                //{
                //    if (type != null)
                //    {
                //        var method = type.GetMethod("GenGameLogicAB", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                //        if (method != null)
                //        {
                //            method.Invoke(type, null);
                //        }
                //    }
                //}
                if (!Directory.Exists(asset_path))
                {
                    Directory.CreateDirectory(asset_path);
                }
                bool b = GameBuilder.set_path_copy_assets(asset_path);
                if (!b)
                {
                    return;
                }
            }
        }
        else 
        {
            var logs = PlayerSettings.SplashScreen.logos;
            var log = logs[0];
            log.logo = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/apkicon/junhai_logo.png");
            logs[0] = log;
            PlayerSettings.SplashScreen.logos = logs;
            PlayerSettings.SplashScreen.backgroundColor = Color.white;
            PlayerSettings.SplashScreen.unityLogoStyle = PlayerSettings.SplashScreen.UnityLogoStyle.DarkOnLight;

            EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
        }
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

        GameBuilder.DelAppShaders();

        if (BuilderConfig.platType != PlatType.junhai)
        {
            string xml_path = Application.dataPath + "/Plugins/Android/launcherTemplate.gradle";
            if (File.Exists(xml_path))
            {
                string content = File.ReadAllText(xml_path, System.Text.UTF8Encoding.UTF8);
                StringBuilder sb = new StringBuilder();
                StringReader sr = new StringReader(content);
                //bool is_next_line = false;
                while (sr.Peek() > 1)
                {
                    string line = sr.ReadLine();
                    if (line.Contains("nd_analy_channel"))
                    {
                        //if (line.Contains("value="))
                        //{
                        //    //<meta-data tools:replace="android:value" android:name="ND_ANALY_CHANNEL" android:value="nd" />
                        //    line = $"<meta-data tools:replace=\"android:value\" android:name=\"ND_ANALY_CHANNEL\" android:value=\"{BuilderConfig.Channel}\" />";
                        //}
                        //else
                        //{
                        //    is_next_line = true;
                        //}
                        line = $"addManifestPlaceholders(\"nd_analy_channel\": \"{BuilderConfig.Channel}\")";
                    }
                    //else if (is_next_line)
                    //{
                    //    is_next_line = false;
                    //    line = $"android:value=\"{BuilderConfig.Channel}\" />";
                    //}
                    sb.AppendLine(line);
                }
                sr.Close();
                sr.Dispose();
                File.WriteAllText(xml_path, sb.ToString(), System.Text.UTF8Encoding.UTF8);
            }
        }
        //if (key_value.ContainsKey("dll") && key_value["dll"].Equals("1"))
        //{
        //    if (BuilderConfig.include_assetbundles == 0 && type != null)
        //    {
        //        var method = type.GetMethod("GenGameLogic", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        //        if (method != null)
        //        {
        //            method.Invoke(type, null);
        //        }
        //    }
        //}

#if UNITY_IOS
        string outputPath = Application.dataPath.Replace("Assets", "Release_Xcode");
#else
        string outputPath = Application.dataPath.Replace("Assets", "ReleaseBuilder");
#endif

        if (Directory.Exists(outputPath))
        {
            Directory.Delete(outputPath, true);
        }

        var buildOptions = BuildOptions.CompressWithLz4;
        bool is_aab = false;
#if UNITY_IOS
        string location = $"{outputPath}/nd_xcode";
#else
        string location = $"{outputPath}/{lang_id}_{DateTime.Now.ToString("yyMMdd_HHmmss")}.apk";
        if ((key_value.ContainsKey("is_aab") && key_value["is_aab"].Equals("1")) || BuilderConfig.is_aab == 1)
        {
            location = location.Replace(".apk", ".aab");
            is_aab = true;
        }
#endif

        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions()
        {
            scenes = new string[] { "Assets/main.unity" },
            locationPathName = location,
            options = buildOptions,
            target = EditorUserBuildSettings.activeBuildTarget,
#if UNITY_ANDROID
            targetGroup = BuildTargetGroup.Android,
#elif UNITY_IOS
                targetGroup = BuildTargetGroup.iOS,
#else
                targetGroup = BuildTargetGroup.Standalone,
#endif
        };
        Directory.CreateDirectory(outputPath);

#if HybridCLR
        var setting = HybridCLR.Editor.Settings.HybridCLRSettings.Instance;
        setting.hotUpdateAssemblies = new string[] { "hybridclr" };
        setting.externalHotUpdateAssembliyDirs = new string[] { "../hotfixtodll/bin/Release/net4.7.2" };
        setting.enable = true;
        setting.patchAOTAssemblies = new string[]
        {
            "mscorlib",
            "System",
            "System.Core",
            "Google.Protobuf"
        };
        HybridCLR.Editor.Settings.HybridCLRSettings.Save();
        AssetDatabase.SaveAssets();

        var scenes = EditorBuildSettings.scenes;
        EditorBuildSettings.scenes = new EditorBuildSettingsScene[1] { new EditorBuildSettingsScene("Assets/main.unity", true) };

        HybridCLR.Editor.Installer.InstallerController _controller = new HybridCLR.Editor.Installer.InstallerController();
        bool hasInstall = _controller.HasInstalledHybridCLR();
        if (!hasInstall)
        {
            _controller.InstallDefaultHybridCLR();
        }

        HybridCLR.Editor.Commands.PrebuildCommand.GenerateAll();
        var typeGameLoader = typeof(MyComponent).Assembly.GetType("GameBase.GameLoader");
        if (typeGameLoader != null)
        {
            var method = typeGameLoader.GetMethod("ReplaceGenMetadataForAOTAssembly", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            if (method != null)
            {
                method.Invoke(typeGameLoader, null);
            }
        }
        EditorBuildSettings.scenes = new EditorBuildSettingsScene[0];
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
#endif
        var apkdir = "";
        if (BuilderConfig.platType == PlatType.junhai && buildPlayerOptions.targetGroup == BuildTargetGroup.Android)
        {
            apkdir = GameBuilder.ExportAndroidProject(outputPath, is_aab);
        }
        else
        {
#if GOOGLE || HUAWEI
            if (is_aab)
            {
                string _path = Application.dataPath.Replace("Assets", "assetbundles");
                if (Directory.Exists(_path))
                {
                    var assetPackConfig = new Google.Android.AppBundle.Editor.AssetPackConfig();
                    assetPackConfig.AddAssetsFolder("asset_base",
                                                    _path,
                                                    Google.Android.AppBundle.Editor.AssetPackDeliveryMode.InstallTime);
                    Google.Android.AppBundle.Editor.AssetPacks.AssetPackConfigSerializer.SaveConfig(assetPackConfig);
                    Google.Android.AppBundle.Editor.Bundletool.BuildBundle(buildPlayerOptions, assetPackConfig);
                }
                else
                {
                    var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
                    if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                    {
                        UnityEngine.Debug.LogError("打包失败");
                        return;
                    }
                }
            }
            else
            {
#endif
                var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
                if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
                {
                    UnityEngine.Debug.LogError("打包失败");
                    return;
                }
#if GOOGLE || HUAWEI
            }
#endif
        }
        UnityEngine.Debug.LogError("打包成功");
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);

#if UNITY_ANDROID
        if (key_value.ContainsKey("outpath"))
        {
            string outpath = key_value["outpath"].Replace('\\', '/');
            if (!Directory.Exists(outpath))
            {
                Directory.CreateDirectory(outpath);
            }
            UnityEngine.Debug.LogError($"outpath={outpath}, apkdir={apkdir}");
            if (apkdir == "")
            {
                string[] files = Directory.GetFiles("./ReleaseBuilder", is_aab ? "*.aab" : "*.apk", SearchOption.TopDirectoryOnly);
                Log.LogInfo($"outpath={outpath}, is_aab={is_aab}, files={files.Length}");
                for (int i = 0; i < files.Length; i++)
                {
                    var dst = outpath + "/" + patch_model + "_" + Path.GetFileName(files[i]);
                    Log.LogInfo($"copy {files[i]} -> {dst}");
                    File.Copy(files[i], dst);
                }
            }
            else
            {
                string[] files = Directory.GetFiles(apkdir, is_aab ? "*.aab" : "*.apk", SearchOption.AllDirectories);
                Log.LogInfo($"apkdir={apkdir}, is_aab={is_aab}, files={files.Length}");
                for (int i = 0; i < files.Length; i++)
                {
                    var basedir = files[i].Substring(0, files[i].IndexOf("\\launcher")).TrimEnd('\\', '/');
                    basedir = Path.GetFileName(basedir);
                    var dst = outpath + "/" + patch_model + "_" + lang_id + "_" + basedir + "_" + Path.GetFileName(files[i]);
                    Log.LogInfo($"copy {files[i]} -> {dst}");
                    File.Copy(files[i], dst);
                }
            }
        }
        else 
        {
            UnityEngine.Debug.LogError("key_value not ContainsKey outpath");
        }
#endif

            var ResourcesDir = Path.GetFullPath($"..\\GameLibrary\\Resources").Replace('/', '\\');
            ProcessUtils.ExecSystemComm($"svn up {ResourcesDir}");
        }

    private static string[] GetAddDefine(string define, string[] defines)
    {
        bool is_has_hybrid = false;
        for (int i = 0; i < defines.Length; i++)
        {
            if (defines[i].Equals(define))
            {
                is_has_hybrid = true;
                break;
            }
        }
        if (!is_has_hybrid)
        {
            List<string> list = new List<string>(defines);
            list.Add(define);
            defines = list.ToArray();
        }
        return defines;
    }

    private static string[] GetRemoveDefine(string define, string[] defines)
    {
        List<string> list = new List<string>();
        for (int i = 0; i < defines.Length; i++)
        {
            if (!defines[i].Equals(define))
            {
                list.Add(defines[i]);
            }
        }
        return list.ToArray();
    }



    public static void ReadyBuildPC()
    {
        Dictionary<string, string> key_value = new Dictionary<string, string>();
        string[] args = System.Environment.GetCommandLineArgs();
        if (args != null && args.Length > 0)
        {
            foreach (var line in args)
            {
                if (line.Contains(';'))
                {
                    string[] te = line.Split(';');
                    foreach (var line1 in te)
                    {
                        if (line1.Contains('='))
                        {
                            string[] te1 = line1.Split('=');
                            key_value[te1[0].Trim()] = te1[1].Trim().Trim(' ').Trim('\n');
                        }
                    }
                }
                else if (line.Contains('='))
                {
                    string[] te = line.Split('=');
                    key_value[te[0].Trim()] = te[1].Trim().Trim(' ').Trim('\n');
                }
            }

            key_value["local_version"] = DateTime.Now.ToString("yyMMddHHmmss");
            string content = File.ReadAllText(Application.dataPath + "/Libs/utils/data/BuilderConfig.cs", System.Text.UTF8Encoding.UTF8);
            StringBuilder sb = new StringBuilder();
            StringReader sr = new StringReader(content);
            bool is_start = false;
            while (sr.Peek() > 1)
            {
                string line = sr.ReadLine();
                if (!is_start)
                {
                    if (line.Contains("#region auto"))
                    {
                        is_start = true;
                    }
                }
                else
                {
                    if (line.Contains("#endregion auto"))
                    {
                        is_start = false;
                    }
                    else
                    {
                        foreach (var kv in key_value)
                        {
                            int index = line.IndexOf(kv.Key);
                            if (index > -1)
                            {
                                int end = line.IndexOf(';');
                                if (line.Contains(" int "))
                                {
                                    float.TryParse(kv.Value, out float v);
                                    line = line.Substring(0, index + kv.Key.Length) + " = " + (int)v + line.Substring(end);
                                }
                                else if (line.Contains(" bool "))
                                {
                                    string v = "false";
                                    if (kv.Value.ToLower() == "true" || kv.Value == "1")
                                    {
                                        v = "true";
                                    }
                                    line = line.Substring(0, index + kv.Key.Length) + " = " + v + line.Substring(end);
                                }
                                else
                                {
                                    line = line.Substring(0, index + kv.Key.Length) + " = " + $"\"{kv.Value}\"" + line.Substring(end);
                                }
                                break;
                            }
                        }
                    }
                }
                sb.AppendLine(line);
            }
            sr.Close();
            sr.Dispose();
            File.WriteAllText(Application.dataPath + "/Libs/utils/data/BuilderConfig.cs", sb.ToString(), System.Text.UTF8Encoding.UTF8);
        }

        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        {
            string content = File.ReadAllText(Application.dataPath.Replace("Assets", "Packages/manifest.json"), System.Text.UTF8Encoding.UTF8);
            StringBuilder sb = new StringBuilder();
            StringReader sr = new StringReader(content);
            bool is_has = false;
            while (sr.Peek() > 1)
            {
                string line = sr.ReadLine();
                if (line.Contains("hybridclr_unity"))
                {
                    continue;
                }
                if (line.Contains("com.unity.ads") || line.Contains("com.unity.analytics"))
                {
                    continue;
                }
                sb.AppendLine(line);
            }
            sr.Close();
            sr.Dispose();
            if (!is_has)
            {
                File.WriteAllText(Application.dataPath.Replace("Assets", "Packages/manifest.json"), sb.ToString(), System.Text.UTF8Encoding.UTF8);
            }
        }

        string qa_action = Application.streamingAssetsPath + "/qa_action";
        if (Directory.Exists(qa_action))
        {
            Directory.Delete(qa_action, true);
        }
        if (key_value.ContainsKey("qa_action") && key_value["qa_action"].Equals("1"))
        {
            Directory.CreateDirectory(qa_action);
            string[] files = Directory.GetFiles("../GameLibrary/QASuperMan", "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                if (file.Contains(".bytes") && !file.Contains("QACodeApkDll"))
                {
                    continue;
                }
                File.Copy(file, qa_action + "/" + Path.GetFileName(file));
            }
        }

        PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, out string[] defines);
        defines = GetRemoveDefine("HybridCLR", defines);

        if (key_value.ContainsKey("is_aab") && key_value["is_aab"].Equals("1"))
        {
            defines = GetAddDefine("GOOGLE", defines);
        }
        else
        {
            defines = GetRemoveDefine("GOOGLE", defines);
        }
        if (key_value.ContainsKey("debugBuild") && key_value["debugBuild"].Equals("1"))
        {
            defines = GetAddDefine("DEBUG", defines);
        }
        else
        {
            defines = GetRemoveDefine("DEBUG", defines);
        }
        PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, defines);

        AssetDatabase.SaveAssets();

        CompilationPipeline.RequestScriptCompilation(RequestScriptCompilationOptions.CleanBuildCache);

        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
    }

    public static void BuildPC()
    {
        Debug.Log("前置设置完成，开始编译PC版本");
        //BuilderConfig.local_version = DateTime.Now.ToString("yyMMddHHmmss");
        string lang_id = BuilderConfig.lang_id;

        string[] args = System.Environment.GetCommandLineArgs();
        Dictionary<string, string> key_value = new Dictionary<string, string>();
        if (args != null && args.Length > 0)
        {
            foreach (var line in args)
            {
                if (line.Contains(';'))
                {
                    string[] te = line.Split(';');
                    foreach (var line1 in te)
                    {
                        if (line1.Contains('='))
                        {
                            string[] te1 = line1.Split('=');
                            key_value[te1[0].Trim()] = te1[1];
                        }
                    }
                }
                else if (line.Contains('='))
                {
                    string[] te = line.Split('=');
                    key_value[te[0].Trim()] = te[1];
                }
            }
        }

        string patch_model = BuilderConfig.patchmode;
        if (key_value.ContainsKey("patchmode"))
        {
            patch_model = key_value["patchmode"];
        }
        bool debugBuild = false;
        if (key_value.ContainsKey("debugBuild") && key_value["debugBuild"].Equals("1"))
        {
            debugBuild = true;
        }

        EditorUserBuildSettings.buildAppBundle = false;

        PlayerSettings.productName = BuilderConfig.app_name;
        PlayerSettings.bundleVersion = BuilderConfig.versionName;
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Standalone, BuilderConfig.package_name);

        PlayerSettings.gpuSkinning = true;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Standalone, ScriptingImplementation.Mono2x);
        PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Standalone, ManagedStrippingLevel.Disabled);
        PlayerSettings.SetIl2CppCompilerConfiguration(BuildTargetGroup.Standalone, Il2CppCompilerConfiguration.Release);


        EditorUserBuildSettings.development = debugBuild;
        EditorUserBuildSettings.connectProfiler = debugBuild;
        EditorUserBuildSettings.buildWithDeepProfilingSupport = debugBuild;
        EditorUserBuildSettings.switchHTCSScriptDebugging = debugBuild;

        PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Standalone, ApiCompatibilityLevel.NET_Unity_4_8);
        PlayerSettings.gcIncremental = false;
        PlayerSettings.allowUnsafeCode = true;
        PlayerSettings.stripEngineCode = false;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.defaultScreenWidth = 768;
        PlayerSettings.defaultScreenHeight = 432;
        PlayerSettings.resizableWindow = true;

        PlayerSettings.SetStackTraceLogType(LogType.Error, StackTraceLogType.ScriptOnly);
        PlayerSettings.SetStackTraceLogType(LogType.Assert, StackTraceLogType.ScriptOnly);
        PlayerSettings.SetStackTraceLogType(LogType.Exception, StackTraceLogType.ScriptOnly);
        if (BuilderConfig.IsDebugBuild)
        {
            PlayerSettings.SetStackTraceLogType(LogType.Warning, StackTraceLogType.ScriptOnly);
            PlayerSettings.SetStackTraceLogType(LogType.Log, StackTraceLogType.ScriptOnly);
            Log.LogInfo($"打包apk, 打包测试版本");
        }
        else
        {
            PlayerSettings.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
            PlayerSettings.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
            Log.LogInfo($"打包apk, 打包正式版本, StackTraceLogType={PlayerSettings.GetStackTraceLogType(LogType.Error)}");
        }

        UnityEditor.Rendering.TierSettings ts = new UnityEditor.Rendering.TierSettings();
        ts.hdr = true;
        ts.hdrMode = UnityEngine.Rendering.CameraHDRMode.R11G11B10;
        ts.renderingPath = RenderingPath.Forward;
        ts.realtimeGICPUUsage = UnityEngine.Rendering.RealtimeGICPUUsage.Low;
        UnityEditor.Rendering.EditorGraphicsSettings.SetTierSettings(BuildTargetGroup.Standalone, UnityEngine.Rendering.GraphicsTier.Tier3, ts);


        string asset_path = Application.streamingAssetsPath + "/assetbundles";
        if (Directory.Exists(asset_path))
        {
            Directory.Delete(asset_path, true);
        }
        if (BuilderConfig.is_aab == 1)
        {
            asset_path = Application.dataPath.Replace("Assets", "assetbundles");
            if (Directory.Exists(asset_path))
            {
                Directory.Delete(asset_path, true);
            }
        }

        var type = typeof(MyComponent).Assembly.GetType("GameBase.GameLoader");

        if (BuilderConfig.include_assetbundles != 0)
        {
            if (!Directory.Exists(asset_path))
            {
                Directory.CreateDirectory(asset_path);
            }
            bool b = GameBuilder.set_path_copy_assets(asset_path);
            if (!b)
            {
                return;
            }
        }
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        var buildOptions = BuildOptions.CompressWithLz4;
        string location = key_value["outpath"].Replace('\\', '/');
        if (Directory.Exists(location))
        {
            Directory.Delete(location, true);
        }
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions()
        {
            scenes = new string[] { "Assets/main.unity" },
            locationPathName = location + ".exe",
            options = buildOptions,
            target = EditorUserBuildSettings.activeBuildTarget,
#if UNITY_ANDROID
            targetGroup = BuildTargetGroup.Android,
#elif UNITY_IOS
                targetGroup = BuildTargetGroup.iOS,
#else
                targetGroup = BuildTargetGroup.Standalone,
#endif
        };
        
        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            UnityEngine.Debug.LogError("打包失败");
            return;
        }

        UnityEngine.Debug.LogError("打包成功");
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
    }

}
