using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using System.IO;
using Object = UnityEngine.Object;
using System.Reflection;

/// <summary>
/// 
/// </summary>
class AssetPostProcessor : AssetPostprocessor
{
    //static int Shader_name_Ambilent_scale = -1;

    bool ispow2(int n)
    {
        return (n & (n - 1)) == 0;
        //return (n | (n - 1)) == n + (n - 1);
    }

    static object[] args = null;
    static MethodInfo mi = null;
    //static Int32 zero = 0;
    //static PropertyInfo forceMaximumCompressionQuality_BC6H_BC7;
    static T _update<T>(T a, T b, ref bool flag) where T : IComparable 
    {
        flag |= a.CompareTo(b) != 0;
        return b;
    }
    void OnPreprocessTexture()
    {
        TextureImporter ti = assetImporter as TextureImporter;
        var pathname = assetPath.ToLower();
        if (pathname.StartsWith("assets/libs/") || pathname.StartsWith("assets/thirdparty/") || pathname.StartsWith("assets/resources/") || pathname.StartsWith("assets/apkicon/"))
        {
            return;
        }
        if (pathname.StartsWith("packages/"))
        {
            return;
        }
        var name = Path.GetFileNameWithoutExtension(pathname);
        {
            bool dirty = false;            
            TextureImporterPlatformSettings tsAdnroid = ti.GetPlatformTextureSettings("Android");
            tsAdnroid.compressionQuality = _update(tsAdnroid.compressionQuality, 50, ref dirty);

            //var fix_formate = tsAdnroid.overridden ? TextureImporterFormat.ASTC_4x4 : TextureImporterFormat.ASTC_6x6;
            if (name.EndsWith("_n") || name.EndsWith("_ns") || name.EndsWith("_d") || name.EndsWith("_m"))
            {
                ti.textureShape = TextureImporterShape.Texture2D;
                if (name.EndsWith("_n") || name.EndsWith("_ns"))
                {
                    ti.textureType = _update(ti.textureType, TextureImporterType.NormalMap, ref dirty);
                }
                else
                {
                    ti.textureType = _update(ti.textureType, TextureImporterType.Default, ref dirty);
                    ti.alphaSource = _update(ti.alphaSource, TextureImporterAlphaSource.FromInput, ref dirty);
                }                
            }

            //
            if (pathname.StartsWith("assets/ui/") || pathname.StartsWith("assets/resources/"))
            {
                ti.mipmapEnabled = _update(ti.mipmapEnabled, false, ref dirty);
            }
            else
            {
                if (!tsAdnroid.overridden)
                {
                    ti.mipmapEnabled = _update(ti.mipmapEnabled, true, ref dirty);
                }
            }

            if (pathname.StartsWith(PathDefs.ASSETS_PATH_GUI_SPRITES))
            {
                ti.isReadable = _update(ti.isReadable, true, ref dirty);//图集合并
                ti.textureType = _update(ti.textureType, TextureImporterType.Sprite, ref dirty);
            }
            else
            {
                if (!name.EndsWith("_t4m") && !pathname.Contains("/t4m/"))
                {
                    ti.isReadable = _update(ti.isReadable, false, ref dirty);
                }
                if ((!tsAdnroid.overridden && pathname.StartsWith("assets/ui/asset/")) || pathname.StartsWith(PathDefs.PREFAB_PATH_UI_PACKERS) )
                {
                    ti.textureType = _update(ti.textureType, TextureImporterType.Default, ref dirty);
                    ti.sRGBTexture = _update(ti.sRGBTexture, false, ref dirty);
                    ti.alphaIsTransparency = _update(ti.alphaIsTransparency, true, ref dirty);
                }
            }

            if (pathname.StartsWith(PathDefs.ASSETS_PATH_GUI_SPRITES))
            {
                tsAdnroid.format = _update(tsAdnroid.format, TextureImporterFormat.RGBA32, ref dirty);
            }
            else
            {

                if (!tsAdnroid.overridden)
                {
                    if (tsAdnroid.maxTextureSize > 1024)
                    {
                        tsAdnroid.maxTextureSize = 1024;
                    }
                    if (ti.textureType == TextureImporterType.NormalMap || name.StartsWith("lightmap-") || !pathname.Contains("/scene/"))
                    {
                        tsAdnroid.format = TextureImporterFormat.ASTC_6x6;
                    }
                    else
                    {
                        tsAdnroid.format = TextureImporterFormat.ASTC_8x8;
                    }
                }

                if (!name.EndsWith("_t4m"))
                {
                    if (tsAdnroid.format == TextureImporterFormat.ARGB32 || tsAdnroid.format == TextureImporterFormat.RGB24 || tsAdnroid.format == TextureImporterFormat.RGBA32 || tsAdnroid.textureCompression == TextureImporterCompression.Uncompressed)
                    {
                        dirty = true;
                        tsAdnroid.format = TextureImporterFormat.ASTC_4x4;
                    }

                    if (!(tsAdnroid.format >= TextureImporterFormat.ASTC_4x4 && tsAdnroid.format <= TextureImporterFormat.ASTC_12x12) && !(tsAdnroid.format >= TextureImporterFormat.ASTC_HDR_4x4 && tsAdnroid.format <= TextureImporterFormat.ASTC_HDR_12x12))
                    {
                        dirty = true;
                        tsAdnroid.format = TextureImporterFormat.ASTC_6x6;
                    }
                }
            }

            //if (!pathname.StartsWith("assets/ui/") && tsAdnroid.format == TextureImporterFormat.ASTC_4x4) 
            //{
            //    dirty = true;
            //    tsAdnroid.format = TextureImporterFormat.ASTC_6x6;
            //}

            TextureImporterPlatformSettings tsIOS = ti.GetPlatformTextureSettings("iPhone");
            tsIOS.compressionQuality = _update(tsIOS.compressionQuality, tsAdnroid.compressionQuality, ref dirty);
            tsIOS.maxTextureSize = _update(tsIOS.maxTextureSize, tsAdnroid.maxTextureSize, ref dirty);
            tsIOS.format = _update(tsIOS.format, tsAdnroid.format, ref dirty);

            if (dirty || !tsAdnroid.overridden || !tsIOS.overridden)
            {
                tsAdnroid.overridden = true;
                tsIOS.overridden = true;
                ti.SetPlatformTextureSettings(tsAdnroid);
                ti.SetPlatformTextureSettings(tsIOS);
            }
        }
    }
    //static object[] args = null;
    //static MethodInfo mi = null;
    void OnPostprocessTexture(Texture tex)
    {
        //return;
    }

    void OnPreprocessModel()
    {
        var modelImporter = assetImporter as ModelImporter;
        string path = modelImporter.assetPath.ToLower();
        if (path.EndsWith(".fbx"))
        {
            //Debug.LogError("path:" + path);
            if (modelImporter.importSettingsMissing)
            {
                modelImporter.globalScale = 1;
                modelImporter.isReadable = false;
                modelImporter.materialImportMode = ModelImporterMaterialImportMode.None;
                if (!path.StartsWith(PathDefs.ASSETS_PATH_CHARACTER))
                {
                    modelImporter.optimizeMeshPolygons = true;
                    modelImporter.optimizeMeshVertices = true;
                    modelImporter.optimizeGameObjects = true;

                    modelImporter.importAnimation = false;
                    modelImporter.importVisibility = false;
                    modelImporter.importCameras = false;
                    modelImporter.importLights = false;
                }
            }
        }
    }

    static bool Refresh2 = false;
    void OnPostprocessModel(GameObject go)
    {
        //var modelImporter = assetImporter as ModelImporter;
        //modelImporter.materialImportMode = ModelImporterMaterialImportMode.None;
    }

    private void OnPostprocessPrefab(GameObject go)
    {
        if (!string.IsNullOrEmpty(assetImporter.assetPath) && assetImporter.assetPath.ToLower().Contains(PathDefs.PREFAB_PATH_GUI_PANEL))
        {
            var file_name = Path.GetFileNameWithoutExtension(assetImporter.assetPath);
            if (!file_name.EndsWith("_panel"))
            {
                Debug.LogError("UI Prefab 需要以_panel 结尾，请重命名");
                EditorUtility.DisplayDialog("提示", "UI Prefab 需要以_panel 结尾，请重命名", "ok");
            }
        }
    }

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromPath)
    {
        //Debug.Log($"OnPostprocessAllAssets");
        Log.CheckInit();
        if (importedAssets.Length < 10) 
        {
            foreach (var p in importedAssets)
            {
                if (p.ToLower().EndsWith(".prefab"))
                {
                    var go = AssetDatabase.LoadAssetAtPath<GameObject>(p);
                    if (go)
                    {
                        var pss = go.GetComponentsInChildren<ParticleSystem>(true);
                        foreach (var ps in pss)
                        {
                            for (var i = 0; i < ps.subEmitters.subEmittersCount; ++i)
                            {
                                var sub = ps.subEmitters.GetSubEmitterSystem(i);
                                if (sub && sub.transform.parent != ps.transform)
                                {
                                    Log.LogError($"{ps.gameObject.GetLocation()} subEmitters[{i}] -> {sub.gameObject.GetLocation()}");
                                }
                            }
                        }
                    }
                }
            }
        }
        //
        //AssetDatabase.SaveAssets();
        //Log.LogError($"OnPostprocessAllAssets");
        //Shader.DisableKeyword("__MAPDEBUG");
        //Shader.EnableKeyword("__NO_MAPDEBUG");
        return;
        foreach (var imported_path in importedAssets)
        {
            //Debug.Log(imported_path);
            var pathname = imported_path.ToLower();
            //if (pathname.StartsWith(PathDefs.ASSETS_PATH_SCENE_ASSETS) || pathname.StartsWith(PathDefs.ASSETS_PATH_COMPLEX_OBJECT) || pathname.StartsWith(PathDefs.ASSETS_PATH_CHARACTER))
            //if (true)
            //{
            //    var obj = AssetDatabase.LoadAssetAtPath(pathname, typeof(Object));
            //    if (obj is GameObject)
            //    {
            //        var curFile = AssetDatabase.GetAssetPath(obj);
            //        var curPath = Path.GetDirectoryName(curFile);
            //        if (Path.GetExtension(curFile).ToLower() == ".fbx")
            //        {
            //            var tmpMeshs = (obj as GameObject).GetComponentsEx<MeshFilter>();
            //            for (int i = 0; i < tmpMeshs.Count; ++i)
            //            {
            //                var tmpMesh = (Mesh)Object.Instantiate(tmpMeshs[i].sharedMesh);
            //                tmpMesh.name = tmpMeshs[i].sharedMesh.name;
            //                var meshFile = "Assets/temp/.mesh/" + tmpMesh.name.ToLower() + ".asset";
            //                if (File.Exists(meshFile))
            //                {
            //                    File.Delete(meshFile);
            //                }
            //                AssetDatabase.CreateAsset(tmpMesh, meshFile);
            //                AssetDatabase.SaveAssets();
            //            }

            //        }
            //    }
            //}
            //else
            if (pathname.StartsWith(PathDefs.ASSETS_PATH_CHARACTER) && pathname.Contains("02face"))
            {
                var obj = AssetDatabase.LoadAssetAtPath(pathname, typeof(Object));
                if (obj is GameObject)
                {
                    var curFile = AssetDatabase.GetAssetPath(obj);
                    var curPath = Path.GetDirectoryName(curFile);

                    if (Path.GetExtension(curFile).ToLower() == ".fbx")
                    {
                        var tmpMeshs = (obj as GameObject).GetComponentsEx<SkinnedMeshRenderer>();
                        for (int i = 0; i < tmpMeshs.Count; ++i)
                        {
                            var tmpMesh = (Mesh)Object.Instantiate(tmpMeshs[i].sharedMesh);
                            tmpMesh.name = tmpMeshs[i].sharedMesh.name;
                            var meshFile = curPath + "/" + tmpMesh.name.ToLower() + ".asset";

                            if (File.Exists(meshFile))
                            {
                                File.Delete(meshFile);
                            }
                            AssetDatabase.CreateAsset(tmpMesh, meshFile);
                            if (!AssetbundleBuilder.BuilderIsSaving)
                            {
                                AssetDatabase.SaveAssets();
                            }
                        }

                    }
                }
            }
           
            else if(pathname.StartsWith(PathDefs.ASSETS_PATH_SCENE_ASSETS))
            {
                var obj = AssetDatabase.LoadAssetAtPath(pathname, typeof(Object));
                if (obj is GameObject)
                {
                    var curFile = AssetDatabase.GetAssetPath(obj);

                    if (Path.GetExtension(curFile).ToLower() == ".prefab")
                    {
                        var tmpMeshs = (obj as GameObject).GetComponentsEx<MeshFilter>();
                        for (int i = 0; i < tmpMeshs.Count; ++i)
                        {
                            var meshFile = AssetDatabase.GetAssetPath(tmpMeshs[i].sharedMesh);
                            if(!string.IsNullOrEmpty(meshFile) && meshFile.ToLower().EndsWith(".fbx"))
                            {
                                var meshPath = PathDefs.ASSETS_PATH_SCENE_ASSETS + "mesh/";
                                var meshDataFile = meshPath + "/" + tmpMeshs[i].sharedMesh.name.ToLower() + ".asset";
                                if (File.Exists(meshDataFile))
                                {
                                    tmpMeshs[i].sharedMesh = AssetDatabase.LoadAssetAtPath(meshDataFile, typeof(Mesh)) as Mesh;
                                }
                               
                            }
                        }
                        if (!AssetbundleBuilder.BuilderIsSaving)
                        {
                            AssetDatabase.SaveAssets();
                        }
                    }
                    else if (Path.GetExtension(curFile).ToLower() == ".fbx")
                    {
                        var tmpMeshs = (obj as GameObject).GetComponentsEx<MeshFilter>();
                        for (int i = 0; i < tmpMeshs.Count; ++i)
                        {
                            var tmpMesh = (Mesh)Object.Instantiate(tmpMeshs[i].sharedMesh);
                            tmpMesh.name = tmpMeshs[i].sharedMesh.name;
                            var meshFile = "Assets/temp/.mesh/" + tmpMesh.name.ToLower() + ".asset";
                            if (File.Exists(meshFile))
                            {
                                File.Delete(meshFile);
                            }
                            AssetDatabase.CreateAsset(tmpMesh, meshFile);
                            if (!AssetbundleBuilder.BuilderIsSaving)
                            {
                                AssetDatabase.SaveAssets();
                            }
                        }
                    }

                }
            }
        }
    }


    void OnPostprocessAudio(AudioClip clip) 
    {
        var importer = assetImporter as AudioImporter;
        if (importer.importSettingsMissing)
        {
            importer.loadInBackground = true;
        }
    }
}
