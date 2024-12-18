#if UNITY_ANDROID && !UNITY_EDITOR 
using System.Runtime.InteropServices;
#endif
using UnityEngine;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using System;

public static class MyPatch
{
#if UNITY_ANDROID && !UNITY_EDITOR 
    const string DLL_NAME = "mypatch";

    [DllImport(DLL_NAME)]
    public static extern string get_patch_path();

    [DllImport(DLL_NAME)]
    public static extern string get_abi();

    [DllImport(DLL_NAME)]
    public static extern string GetBundleId();
#else
    public static string get_patch_path()   //"/data/data/{bundle-id}/files/patch";
    {
        return $"/data/data/{Application.identifier}/files/patch";
    }
    public static string get_abi() //"x86, armeabi-v7a or arm64-v8a";
    {
        return "armeabi-v7a";
    }
#endif

    public static void Del_il2cpp_Cache()
    {
        var dirs = new string[] { "il2cpp", "Unity", "UnityShaderCache" };
        for(var i=0;i<dirs.Length;++i)
        {
            var dir = dirs[i];
            var basepath = i < 2 ? Application.persistentDataPath : Application.temporaryCachePath;
            var il2cpp = Path.Combine(basepath, dir);
            Log.Log2File($"try del {il2cpp}");
            if (Directory.Exists(il2cpp))
            {                
                try
                {
                    Directory.Delete(il2cpp, true);
                }
                catch (Exception e)
                {
                    Log.LogError($"{e.Message}");
                }
            }
        }
    }

    // 删除旧补丁
    public static void DeleteOldPath(string filter)
    {
        var path3 = get_patch_path();
        if (Directory.Exists(path3))
        {
            var dirs = Directory.GetDirectories(path3);
            foreach (var dir in dirs)
            {
                if (Path.GetFileName(dir) != filter)
                {
                    var d = dir;// Path.Combine(path3,dir);
                    Log.Log2File($"delete {d}");
                    try
                    {
                        Directory.Delete(d, true);
                    }
                    catch (System.Exception err)
                    {
                        Log.LogError($"{err.Message}");
                    }
                }
            }

            if (string.IsNullOrEmpty(filter))
            {
                var cfg_pathname = Path.Combine(path3, "version.txt");
                if (File.Exists(cfg_pathname))
                {
                    Log.Log2File($"delete {cfg_pathname}");
                    try
                    {
                        File.Delete(cfg_pathname);
                    }
                    catch (System.Exception err)
                    {
                        Log.LogError($"{err.Message}");
                    }
                }

                Del_il2cpp_Cache();
            }           
        }
    }

    // 获取当前补丁版本, 失败返回 null
    public static string GetCurrentPatchVer()
    {
        var path3 = get_patch_path();
        var cfg_pathname = Path.Combine(path3, "version.txt");
        if (File.Exists(cfg_pathname))
        {
            return File.ReadAllText(cfg_pathname);
        }
        return null;
    }

    // 安装补丁文件
    //  . path2     -- 补丁目录, 例如: /storage/0/Android/data/{包名}/files/assets/patch
    //  . ver       -- 版本号, 例如: "v1.0"
    public static void InstallPatch(string ver, byte[] apk)
    {
        Del_il2cpp_Cache();

        var abi = get_abi();
        var path3 = get_patch_path();
        Log.Log2File($"InstallPatch {path3}, abi={abi}, ver={ver}");
        if (!Directory.Exists(path3))
        {
            Directory.CreateDirectory(path3);
        }

        // 建立版本目录
        var varpath = Path.Combine(path3, ver);
        if (!Directory.Exists(varpath))
        {
            Directory.CreateDirectory(varpath);
        }

        var so_pathname = Path.Combine(varpath, "libil2cpp.so");
        using (var zipStream = new ZipFile(new MemoryStream(apk)))
        {
            var entry = zipStream.GetEntry($"lib/{abi}/libil2cpp.so");
            using (var source = zipStream.GetInputStream(entry))
            {
                var so = new byte[entry.Size];
                source.Read(so, 0, so.Length);

                Log.Log2File($"InstallPatch {so_pathname}, so.Length={so.Length}");
                File.WriteAllBytes(so_pathname, so);
            }
        }

        var apk_pathname = Path.Combine(varpath, "base.apk");
        Log.Log2File($"InstallPatch {apk_pathname}, apk.Length={apk.Length}");
        File.WriteAllBytes(apk_pathname, apk);

        var cfg_pathname = Path.Combine(path3, "version.txt");
        File.WriteAllText(cfg_pathname, ver);

        Log.Log2File("InstallPatch, done");
    }
}

