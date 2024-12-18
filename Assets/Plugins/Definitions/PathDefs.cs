using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


/// <summary>
/// 路径定义
/// </summary>
public static class PathDefs
{
    public const string AssetsResources = "assets/resources/";
    public static bool IsAssetsResources(string path) 
    {
        return path.StartsWith(AssetsResources);
    }

#if UNITY_EDITOR
    // 资源路径   
    public const string ASSETS_PATH_UNITYSECNE = "assets/scene/";
    public const string ASSETS_PATH_SMALL_MAP = "assets/scene_smallmap/";
    //public const string PREFAB_PATH_SCENES_TEMP = "assets/temp/";
    public const string PREFAB_PATH_SCENES_TEMP_EMPTY = "assets/temp/empty_scenes/";
    public const string ASSETS_PATH_SCENE_ASSETS = "assets/scene_res/";
    public const string PREFAB_PATH_CHARACTER = "assets/actor/prefabs/";
    public const string ASSETS_PATH_CHARACTER = "assets/actor/assets/";
    public const string ASSETS_PATH_COMMTEX = "assets/commom/";
    public const string ASSETS_PATH_COMMALONE = "assets/commom/alone/";
    // public const string ASSETS_PATH_CHARACTER_COMMON = "assets/actor/commom";
    public const string PREFAB_PATH_COMPLEX_OBJECT = "assets/fx/";
    public const string PREFAB_PATH_COMPLEX_UI = "assets/fx/common/ui";
    public const string PREFAB_PATH_ITEMS = "assets/equips/";
    public const string ASSETS_PATH_BUILD_TEMP = "assets/temp/";
    public const string ASSETS_PATH_BUILD_TEMP_FBX_PREFABS = "assets/temp/fbx_prefabs/";
    public const string ASSETS_PATH_BUILD_SHADERS_DIR = "assets/collect_shaders/";
    public const string ASSETS_PATH_BUILD_SHADERS = ASSETS_PATH_BUILD_SHADERS_DIR + "all_shader_variants.asset";
    //public const string ASSETS_PATH_BUILD_SHADERS2 = "assets/collect_shaders/yl_shader_variants.asset";
    //
    public const string ASSETS_PATH_GUI_IMAGES = "assets/ui/asset/myuiimage/";
    //public const string ASSETS_PATH_GUI_SPRITE = "assets/ui/asset/sprite_single/";
    public const string ASSETS_PATH_GUI_SPRITES = "assets/ui/asset/sprites/";
    public const string PREFAB_PATH_GUI_PANEL = "assets/ui/prefab/myuipanel/";
    public const string PREFAB_PATH_UI_PACKERS = "assets/ui/prefab/mypacker/";

    public const string ASSETS_PATH_GUI_IMAGES_FREE = "assets/ui/asset/myuiimage/freesize/";
    public const string ASSETS_PATH_GUI_IMAGES_FREE2 = "assets/ui/asset/preview_images";
    //
    public const string ASSETS_PATH_ASSETDATA = "assets/prefabsdata/assetdata/";
    public const string ASSETS_PATH_TRIGGERDATA = "assets/triggerdata/";
    public const string ASSETS_PATH_OTHERASSETS = "assets/other_assets/";
    public const string ASSETS_PATH_SOUND = "assets/sound/";
    public const string ASSETS_PATH_TERRAIN_MESH = "assets/scene/unity_terrain_mesh/";
    public const string ASSETS_PATH_SCENEOBJS = "assets/scene_objs/";
    public const string ASSETS_PATH_VCAMS = "assets/prefabsdata/assetdata/vcams/";


    // 导出路径变量
    public static string EXPORT_ROOT;
    public static string EXPORT_ROOT_OS;
    public static string EXPORT_PATH_COMMTEXTURE;               //公用纹理
    public static string EXPORT_PATH_SCENE;                     //场景
    public static string EXPORT_PATH_SKYBOX;                    //天空盒
    public static string EXPORT_PATH_EFFECT;                    //特效
    public static string EXPORT_PATH_SCENE_PREFAB;              //场景预制体
    public static string EXPORT_PATH_SCENE_SMALL_MAP;           //小地图
    public static string EXPORT_PATH_SHIELD;                    //地图阻挡数据
    public static string EXPORT_PATH_ANIMATION;                 //动画
    public static string EXPORT_PATH_CHARACTERS;                //角色
    public static string EXPORT_PATH_MISC;                      //杂项
    public static string EXPORT_PATH_DATA;                      //数据    
    public static string EXPORT_PATH_ASSETDATA;                 //Asset数据文件
    public static string EXPORT_PATH_SOUND;                     //声音fmod库
    public static string EXPORT_PATH_SHADER;                    //shader文件

    // GUI
    public static string EXPORT_PATH_GUI_PANEL;
    public static string EXPORT_PATH_GUI_ATLAS;
    public static string EXPORT_PATH_GUI_IMAGES;
    public static string EXPORT_PATH_PACKERS;               //预先合并的图集

#endif
    // 参考: http://docs.unity3d.com/Documentation/Manual/PlatformDependentCompilation.html

#if UNITY_ANDROID
    public const string os_name = "android";
#if UNITY_EDITOR
    public const UnityEditor.BuildTarget PlatformName = UnityEditor.BuildTarget.Android;
#endif
#elif UNITY_IPHONE
    public const string os_name = "ios";
#if UNITY_EDITOR
    public const UnityEditor.BuildTarget PlatformName = UnityEditor.BuildTarget.iOS;
#endif
#elif UNITY_STANDALONE
	public const string os_name = "standalone";
#if UNITY_EDITOR
    public const UnityEditor.BuildTarget PlatformName = UnityEditor.BuildTarget.StandaloneWindows;
#endif
#endif

}


