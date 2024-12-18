using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>
/// 路径工具
/// </summary>
class EditorPathUtils
{

    #region 打包路径设置

    // 设置所有路径
    public static bool InitPaths(string root)
    {
        root = Path.GetFullPath(root).Replace('\\','/').Replace("//","/");
        if (!root.EndsWith("/")) root = root + "/";

        // 检查 root_os
        var root_os = root + PathDefs.os_name + "/";
        if (!Directory.Exists(root_os))
        {
            Debug.LogError("Cant find path root_os:" + root_os);
            return false;
        }

        //
        PathDefs.EXPORT_ROOT = root;
        PathDefs.EXPORT_ROOT_OS = root_os;
        root = root_os;
        PathDefs.EXPORT_PATH_COMMTEXTURE = CheckPath(root + "abs/");
        PathDefs.EXPORT_PATH_SCENE = CheckPath(root + "abs/");
        PathDefs.EXPORT_PATH_SKYBOX = CheckPath(root + "abs/");
        PathDefs.EXPORT_PATH_EFFECT = CheckPath(root + "abs/");
        PathDefs.EXPORT_PATH_SCENE_PREFAB = CheckPath(root + "abs/"); 
        PathDefs.EXPORT_PATH_ANIMATION = CheckPath(root + "abs/");
        PathDefs.EXPORT_PATH_CHARACTERS = CheckPath(root + "abs/");
        PathDefs.EXPORT_PATH_MISC = CheckPath(root + "abs/");
        PathDefs.EXPORT_PATH_SCENE_SMALL_MAP = CheckPath(root + "abs/");
        PathDefs.EXPORT_PATH_GUI_ATLAS = CheckPath(root + "abs/");
        PathDefs.EXPORT_PATH_GUI_IMAGES = CheckPath(root + "abs/");
        PathDefs.EXPORT_PATH_GUI_PANEL = CheckPath(root + "abs/");        
        PathDefs.EXPORT_PATH_PACKERS = CheckPath(root + "abs/");
        PathDefs.EXPORT_PATH_ASSETDATA = CheckPath(root + "abs/");
        PathDefs.EXPORT_PATH_SOUND = CheckPath(root + "abs/");        
        PathDefs.EXPORT_PATH_SHADER = CheckPath(root + "abs/");
        PathDefs.EXPORT_PATH_DATA = CheckPath(root + "../datas/data/");
        PathDefs.EXPORT_PATH_SHIELD = CheckPath(root + "../datas/data/shield/");
        //
        CheckPath(PathDefs.ASSETS_PATH_BUILD_TEMP);

        //
        return true;
    }

    // 检测路径, 如果不存在, 则新建
    static string CheckPath(string path)
    {
        path = Path.GetFullPath(path);
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        return path;
    }

    static bool LoadPathSettings()
    {
        string pathText =  "";
        if (File.Exists(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "config.txt")))
        {
            pathText = File.ReadAllText(System.IO.Path.Combine(Directory.GetCurrentDirectory(), "config.txt"));
            var lang_id = BuilderConfig.lang_id;
            if (pathText != null) BuilderConfig.ParseStartupParams(pathText);
            BuilderConfig.lang_id = lang_id;
        }
        else
        {
            Log.LogError("config.txt文件不存在");
            return false;
        }

        
        if (!string.IsNullOrEmpty(BuilderConfig.res_url))
        {
            var res_path = (BuilderConfig.res_url).Replace("file:///", "");
            return InitPaths(res_path);
        }
        return false;
    }

    /// <summary>
    /// 检测路径设置, 如果没有设置, 则弹出对话框选择
    /// </summary>
    public static bool CheckPathSettings()
    {
        AssetDatabase.Refresh();
        LoadPathSettings();
        if (PathDefs.EXPORT_ROOT != null)
        {
            return true;
        }
        else
        {
            Log.LogError("没有设置导出路径，请检查config.txt文件");
            return false;
        }
    }



    #endregion


    // 是否是可识别的资源
    static List<string> _asset_path_list = null;
    public static bool IsKnownAsset(string pathname)
    {
        // 找出 "PathDefs.ASSETS_PATH_" 为前缀的目录定义
        if (_asset_path_list == null)
        {
            _asset_path_list = new List<string>();
            var fields = typeof(PathDefs).GetFields();
            foreach (var field in fields)
            {
                if (field.IsPublic && field.IsStatic &&( field.Name.StartsWith("ASSETS_PATH_") || field.Name.StartsWith("PREFAB_PATH_")))
                {
                    var value = field.GetValue(null);
                    _asset_path_list.Add(value.ToString().ToLower());
                }
            }
        }
        // 如果 pathname 以 "PathDefs.ASSETS_PATH_" 为前缀, 则合法
        foreach (var asset_path in _asset_path_list)
        {
            //Log.LogError($"pathname = {pathname},asset_path={asset_path},{pathname.StartsWith(asset_path)}");
            if (pathname.StartsWith(asset_path)) return true;
        }
        return false;
    }
    


   
}

