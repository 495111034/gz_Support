#if UNITY_IOS
#define HybridCLR
#endif

using System;
using System.IO;
using UnityEngine;

/// <summary>
/// 编译选项, 由打包脚本自动设置
/// </summary>
public static class BuilderConfig
{

    #region auto
    public static int platform_id = 1090;
    public static int game_id = 305301;
    public static string lang_id = "cn";//运行时 客户端的语言id，初始值在打包时设置，后续可以在设置在切换
    public static string app_name = "game_nd";
    public static string package_name = "";

    public static string patchmode = "mono";       //热更新方式：so, mono
    public static int versionCode = 2;            //android 版本号
    public static string versionName = "1.0.2";   //版本名称
    public static int include_x86bit_so = 0;         //包含x86版本
    public static string debugBuild = "0";
    
    public static string asset_version = null;
    public static string logic_version = null;
    public const string local_version = "2211091035";
    public static int uwa_sdk_build = 0;
    
    public static string ly_name = "233";//ly_neophyte

    public static string Channel = "miracland";
    public static string ChannelIdList = "1,2";
    public static string ChannelNameList = "cn,sgp";
    public static string MoneyKey = "062543e0-c098-4d68-ba55-7cedb1afbdc8";

    public static int include_assetbundles = 0;     //是否包含所有assetbundle资源
    public static int IsFastLogin = 0; //1自动创建角色， 2手动输入uid， 3 压缩贴图， 4 内网包， 5 内网帐服， 6 uwa
    public static string cdn_url = "1";//"http://192.168.1.192:8080/assetbundles/";

    //public static string IsTest = "0"; //0可见外网，1可见内网

    public static int is_aab = 0; //是否使用aab打包方式 
    public static bool IsGWHServer = false;

    public static string sdk_package = "";

    public static int sdk_type = 0;

    #endregion auto

    public static PlatType platType = (PlatType)(sdk_type);

    public static bool IsDebugBuild = debugBuild == "1";
    public static string build_app_lang_id { get; } = lang_id; //打包时确定

    public static bool build_assetbundle_bat = true;
    public static bool offline_mode = false;
    //jmana
    //public static bool is_use_local_res_url = false;
#if UNITY_EDITOR
    public static string res_url = Path.GetFullPath("../assetbundles").Replace('\\','/') + "/";
#else
    public static string res_url = "";
#endif

#if HybridCLR
    public const string DLL_NAME = "hybridclr.ab";
#else
    public const string DLL_NAME = "mono.ab";
#endif

    public static bool is_huidu_res_url;
    public static string change_huidu_res_url(string res_url)
    {
        if (is_huidu_res_url)
        {
            if (res_url.IndexOf("/huidu/") < 0)
            {
                res_url = res_url.Replace("/assetbundles", "/huidu/assetbundles");
            }
        }
        else
        {
            res_url = res_url.Replace("/huidu/", "/");
        }
        Log.Log2File($"huidu={is_huidu_res_url}, change to huidu res_url -> {res_url}");
        return res_url;
    }

    public static void load_hudu() 
    {
        var path = Application.persistentDataPath + "/huidu.txt";
        is_huidu_res_url = File.Exists(path) && File.ReadAllText(path) == "1";
        Log.Log2File($"huidu path={path}, huidu={is_huidu_res_url}");
        res_url = change_huidu_res_url(res_url);
    }
    public static void set_huidu(bool huidu) 
    {
        var path = Application.persistentDataPath + "/huidu.txt";
        is_huidu_res_url = huidu;        
        File.WriteAllText( path, huidu ? "1" : "0" );
        res_url = change_huidu_res_url(res_url);
    }

    public const string svnbranch = "0";
    
    
    public static bool SupportsInstance
    {
        get
        {
            return Application.isEditor || SystemInfo.supportsInstancing;
        }
    }

    public static bool IsMobileDevice => Application.isMobilePlatform;

    /// <summary>
    /// 初始信息获取地址
    /// </summary>
    public static string platform_url
    {
        get
        {
            if (_platform_url == null)
            {
                if (IsGWHServer)
                {
                    _platform_url = "http://long-center-demon.99.com/platinfo_demo.php";
                }
                else
                {
                    if (Application.isEditor || os_name == "standalone" || IsFastLogin > 0)
                    {
                        _platform_url = "http://game.nd91.top:8088/platinfo.php";
                    }
                    else 
                    {
                        if (platType == PlatType.junhai)
                        {
                            _platform_url = "https://dhljh-center.99.com/platinfo.php";
                        }
                        else 
                        {
                            _platform_url = "https://long-center.99.com/platinfo.php";
                        }                        
                    }
                }
            }            
            return _platform_url;
        }
    }


    public static void ReplaceHostByIP(string ip) 
    {
        _platform_url_base = null;
        var uri = new Uri(_platform_url );
        _platform_url = _platform_url.Replace( uri.Host, ip);
    }


    static string _platform_url, _platform_url_base;
    public static string platform_url_base
    {
        get
        {
            if (_platform_url_base == null)
            {
                var idx = platform_url.LastIndexOf('/');
                _platform_url_base = platform_url.Substring(0, idx);
            }
            return _platform_url_base;
        }
    }


    //public const string os_name = PathDefs.os_name + "_trees";
    public const string os_name = PathDefs.os_name;
#if !UNITY_2021_3_43
#error 需要升级unity 到 2021_3_43
#endif

#if UNITY_ANDROID
    public const string mJavaPath="E:/long_new/client//android_builder/game_nd";
#elif UNITY_IPHONE
    public const string mAppleDevTeamId = "JAG3938AT7";           // teamID
    public const string mBuildMachineRoot = "E:/233game_new/client/bin/";    
    public static string mBuildDevice="1";
    public static string mDisplayName = "My233Demo2";
#endif

    public static string mProjPath="E:/long_new/client//android_builder/bin/cn_233_2211091035_mono/";

    
    //public const bool IsPublish = false;

    public static bool IsOpenGLES3
    {
        get
        {
            return Application.isEditor || SystemInfo.graphicsShaderLevel > 30;
        }
    }
    public static string BundleId
    {
        get
        {
            return Application.identifier;
        }
    }

    public static void LoadConfig()
    {
        if (Application.isPlaying)
        {
            var lang_id = PlayerPrefs.GetString("setting_lang_id", "");
            if (lang_id != "")
            {
                BuilderConfig.lang_id = lang_id;
            }
        }
#if UNITY_EDITOR
        var text = File.ReadAllText(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "config.txt"));
        ParseStartupParams(text);
#endif
    }
    // 解析参数
    public static void ParseStartupParams(string text)
    {
        Type type = typeof(BuilderConfig);
        var dict = TextParser.ParseIni(text);
        foreach (var item in dict)
        {
            var fi = type.GetField(item.Key);
            if (fi != null)
            {
                var Value = item.Value;
                if (item.Key == "res_url")
                {
                    Value = Value.Replace("file://", "").TrimStart('/');
                }
                fi.SetValue(null, Convert.ChangeType(Value, fi.FieldType));
            }
        }
        Log.Log2File($"BuilderConfig.res_url={BuilderConfig.res_url}, build_app_lang_id={BuilderConfig.build_app_lang_id}, lang_id={BuilderConfig.lang_id}");
    }
    public static bool IsPlayingGameBase = false;

#if UNITY_EDITOR
    static float _ScreenScale = 0;
    public static float ScreenScale
    {
        get
        {
            
            if (_ScreenScale == 0)
            {
                if (Application.isPlaying)
                {
                    var t = Type.GetType("UnityEditor.GameView,UnityEditor");
                    var w = UnityEditor.EditorWindow.GetWindow(t);
                    var f = w.GetType().GetField("m_defaultScale", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    _ScreenScale = (float)(f.GetValue(w));
                }
                else 
                {
                    _ScreenScale = 1;
                }
            }
            return _ScreenScale;
        }
    }
#else
    public static float ScreenScale => 1f;
#endif

    public static int ScreenHeight = (int)(Screen.height * ScreenScale);
    public static int ScreenWidth = (int)(Screen.width * ScreenScale);

}


