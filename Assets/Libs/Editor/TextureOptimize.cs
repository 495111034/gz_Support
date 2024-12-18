using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

//纹理优化
static class TextureOptimize
{
    [MenuItem("Assets/移除不需要alpha信息的贴图")]
    static void RemoveTextUeslessAlphaInfo()
    {
        //需要注释一下AssetPostProcessor的
        //ti.alphaSource = TextureImporterAlphaSource.FromInput;（58行）
        //ti.isReadable = _update(ti.isReadable, false, ref dirty); 77，82行
        //不在修改的时候处理是因为导入的时候是获取不到图片的，也就是没办法判断图片的alpha信息
        try
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture", new string[] { "Assets/actor", "Assets/FX", "Assets/scene", });
            //string[] guids = AssetDatabase.FindAssets("t:Texture", new string[] { "Assets/scene/1001_kc", });
            int progress = 0;
            // 暂停AssetPostprocessor的处理
            //AssetDatabase.StartAssetEditing(); 不管用
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                bool isCancel = EditorUtility.DisplayCancelableProgressBar("移除不需要alpha信息的贴图", assetPath, (float)progress / (float)guids.Length);
                progress++;
                Debug.Log("遍历文件：" + assetPath);
                TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (textureImporter.textureType == TextureImporterType.Default && textureImporter.alphaSource != TextureImporterAlphaSource.None)
                {
                    textureImporter.isReadable = true;
                    AssetDatabase.ImportAsset(assetPath);
                    Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                    if (texture != null)
                    {
                        Debug.Log("遍历图片：" + texture);
                        if (CheckTextureAlphaChannel(texture))
                        {
                            textureImporter.alphaSource = TextureImporterAlphaSource.None;
                            Debug.Log("移除不需要alpha信息的贴图：" + assetPath);
                            //EditorUtility.SetDirty(texture);
                        }
                    }
                    textureImporter.isReadable = false;
                    AssetDatabase.ImportAsset(assetPath);
                }
            }
            AssetDatabase.SaveAssets();
            EditorUtility.ClearProgressBar();
        }
        finally
        {
            // 恢复AssetPostprocessor的处理
            //AssetDatabase.StopAssetEditing();
        }
    }

    public static bool CheckTextureAlphaChannel(Texture2D texture)
    {
        Color32[] pixels = texture.GetPixels32();

        for (int i = 0; i < pixels.Length; i++)
        {
            byte alpha = pixels[i].a;

            if (alpha != 0 && alpha != 255)
            {
                return false;
            }
        }
        return true;
    }

    [MenuItem("Assets/移除重复的模型贴图/All")]
    static void RemoveReportTexture_All()
    {
        RemoveReportTexture(new string[] { "Assets/actor", "Assets/FX", "Assets/scene", });
    }

    [MenuItem("Assets/移除重复的模型贴图/FX")]
    static void RemoveReportTexture_FX()
    {
        RemoveReportTexture(new string[] { "Assets/FX" });
    }

    [MenuItem("Assets/移除重复的模型贴图/Actor")]
    static void RemoveReportTexture_Actor()
    {
        RemoveReportTexture(new string[] { "Assets/actor" });
    }

    [MenuItem("Assets/移除重复的模型贴图/Scene")]
    static void RemoveReportTexture_Scene()
    {
        RemoveReportTexture(new string[] { "Assets/scene" });
    }

    static void RemoveReportTexture(string[] searchInFolders)
    {
        Dictionary<string, string> md5dic = new Dictionary<string, string>();
        HashSet<int> deleteTexInsId = new HashSet<int>();
        //string[] guids = AssetDatabase.FindAssets("t:Texture", new string[] { "Assets/actor", "Assets/FX", "Assets/scene", });
        string[] guids = AssetDatabase.FindAssets("t:Texture", searchInFolders);
        string[] matFiles = null;
        string[] matFilesContent = null;
        //Debug.Log("贴图" + guids.Length);
        //1.遍历所有贴图 用字典缓存 找到重复的资源 
        int progress = 0;
        foreach (var assetGuid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetGuid); // 从GUID拿到资源的路径
            bool isCancel = EditorUtility.DisplayCancelableProgressBar("移除重复的贴图", assetPath, (float)progress / (float)guids.Length);
            progress++;
            string md5 = GetMD5Hash(Path.Combine(Directory.GetCurrentDirectory(), assetPath)); //获取md5
            string path;
            md5dic.TryGetValue(md5, out path);
            if (path == null)
            {
                md5dic[md5] = assetPath;
                // Debug.Log(assetPath);
            }
            else
            {
                Debug.LogFormat("资源重复{0}，{1}", path, assetPath);
                if (matFiles == null)
                {
                    //2.找到所有材质，并且缓存
                    List<string> withoutExtensions = new List<string>() { ".mat" };
                    matFiles = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories).Where(s => withoutExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
                    matFilesContent = new string[matFiles.Length];
                    for (int i = 0; i < matFiles.Length; i++)
                    {
                        matFilesContent[i] = File.ReadAllText(matFiles[i]);
                    }
                }
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path); //保留的图
                Texture2D deleteTex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath); //删除的图
                //3.找到引用该贴图的所有材质
                for (int startIndex = 0; startIndex < matFiles.Length; startIndex++)
                {
                    string file = GetRelativeAssetsPath(matFiles[startIndex]);

                    if (Regex.IsMatch(matFilesContent[startIndex], assetGuid))
                    {
                        Material material = AssetDatabase.LoadAssetAtPath<Material>(file);
                        bool isUseTex = false;
                        var textureNames = material.GetTexturePropertyNames();
                        //Debug.Log("遍历所有需要修改替换贴图的材质：" + file + ",贴图数:" + textureNames.Length);
                        //4.判断该材质所有引用贴图字段 如有和重复贴图相同 则替换
                        for (int j = 0; j < textureNames.Length; j++)
                        {
                            if (material.HasTexture(textureNames[j])) // 该方法获取不到不属于该shader的贴图（切换材质后unity会保留之前shader的信息）
                            {
                                Texture texture = material.GetTexture(textureNames[j]); // 获取材质上的贴图引用
                                if (texture != null)
                                {
                                    //Debug.Log("获取到图片名字：" + texture.name);
                                    if (texture.name == deleteTex.name)
                                    {
                                        isUseTex = true;
                                        material.SetTexture(textureNames[j], tex);
                                        Debug.Log("修改的材质：" + file + "的贴图" + assetPath + ",assetGuid:" + assetGuid + ",替换为：" + path + "修改材质propertyName：" + textureNames[j]);
                                        EditorUtility.SetDirty(material);
                                    }
                                }
                            }
                        }
                    }
                }

                //5.替换完所有材质，删除该重复贴图
                //if(isChangeSucceedCount != referencesMatPath.Count)
                //{
                //    Debug.LogError("+++修改失败+++isChangeSucceedCount："+ isChangeSucceedCount+ "+++referencesMatPath.Count：" + referencesMatPath.Count);
                //}
                //else
                //{
                //    AssetDatabase.DeleteAsset(assetPath);
                //    Debug.LogError("+++修改成功+++");
                //}
                Debug.Log("+++DeleteAsset+++:" + assetPath);
                deleteTexInsId.Add(deleteTex.GetInstanceID());
                AssetDatabase.DeleteAsset(assetPath);
            }
        }
        AssetDatabase.SaveAssets();
        EditorUtility.ClearProgressBar();

        if (deleteTexInsId.Count > 0)
        {
            int checkProgress = 0;
            for (int startIndex = 0; startIndex < matFiles.Length; startIndex++)
            {
                string file = GetRelativeAssetsPath(matFiles[startIndex]);
                bool isCancel = EditorUtility.DisplayCancelableProgressBar("检查材质是否缺少贴图", file, (float)checkProgress / (float)matFiles.Length);
                checkProgress++;

                Material material = AssetDatabase.LoadAssetAtPath<Material>(file);

                SerializedObject serializedMaterial = new SerializedObject(material);
                SerializedProperty texturesProperty = serializedMaterial.FindProperty("m_SavedProperties.m_TexEnvs");

                foreach (SerializedProperty textureProperty in texturesProperty)
                {
                    string propertyName = textureProperty.displayName;
                    SerializedProperty textureReference = textureProperty.FindPropertyRelative("second.m_Texture");

                    // 检查贴图引用是否丢失
                    if (material.shader.FindPropertyIndex(propertyName) > 0 && textureReference.objectReferenceValue == null && deleteTexInsId.Contains(textureReference.objectReferenceInstanceIDValue))
                    {
                        Debug.LogError($"移除重复的模型贴图导致 Missing texture in material: {material.name}, Property: {propertyName}");
                    }
                }
            }
            EditorUtility.ClearProgressBar();
        }
    }

    //[MenuItem("Assets/测试检查材质丢失贴图")]
    //static void TestCheckMatMissTex()
    //{
    //    Material material = AssetDatabase.LoadAssetAtPath<Material>("Assets/FX/Charcater/Berserker/mat/mat_fx_warrior_attack_1_14.mat");
    //    CheckMaterialTextures(material);
    //}

    //[MenuItem("Assets/测试贴图objectReferenceInstanceIDValue")]
    //static void TestTexReferenceIns()
    //{
    //    Texture2D texture2D = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/FX/Charcater/Berserker/tex/tex_fx_warrior_attack_1_7.png");
    //    Debug.LogError("texture2D.GetInstanceID()" + texture2D.GetInstanceID() + "texture2D.GetHashCode()" + texture2D.GetHashCode());
    //}

    //private static void CheckMaterialTextures(Material material)
    //{
    //    SerializedObject serializedMaterial = new SerializedObject(material);
    //    SerializedProperty texturesProperty = serializedMaterial.FindProperty("m_SavedProperties.m_TexEnvs");

    //    foreach (SerializedProperty textureProperty in texturesProperty)
    //    {
    //        string propertyName = textureProperty.displayName;
    //        SerializedProperty textureReference = textureProperty.FindPropertyRelative("second.m_Texture");

    //        // 检查贴图引用是否丢失
    //        if (material.shader.FindPropertyIndex(propertyName) > 0 && textureReference.objectReferenceValue == null)
    //        {
    //            string texturePath = textureReference.objectReferenceInstanceIDValue.ToString();
    //            Debug.LogError($"Missing texture in material: {material.name}, Property: {propertyName}, Texture Path: {texturePath}");
    //        }
    //    }
    //}

    static string GetMD5Hash(string filePath)
    {
        MD5 md5 = new MD5CryptoServiceProvider();
        return BitConverter.ToString(md5.ComputeHash(File.ReadAllBytes(filePath))).Replace("-", "").ToLower();
    }

    [MenuItem("Assets/FindReferences/查找贴图被哪些材质依赖了", false, 10)]
    static private void FindTexBeDependOnMat()
    {
        EditorSettings.serializationMode = SerializationMode.ForceText;
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        string selectTexName = Selection.activeObject.name;
        if (Selection.activeObject.GetType() != typeof(Texture2D))
        {
            Debug.LogError("选中类型错误");
            return;
        }

        if (!string.IsNullOrEmpty(path))
        {
            string guid = AssetDatabase.AssetPathToGUID(path);
            List<string> withoutExtensions = new List<string>() { ".mat" };
            string[] files = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories).Where(s => withoutExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
            int startIndex = 0;

            EditorApplication.update = delegate ()
            {
                string file = files[startIndex];

                bool isCancel = EditorUtility.DisplayCancelableProgressBar("匹配资源中", file, (float)startIndex / (float)files.Length);

                var relativePath = GetRelativeAssetsPath(file);
                Material material = AssetDatabase.LoadAssetAtPath<Material>(relativePath);
                var textureNames = material.GetTexturePropertyNames();
                //Debug.Log("遍历所有需要修改替换贴图的材质：" + file + ",贴图数:" + textureNames.Length);
                for (int j = 0; j < textureNames.Length; j++)
                {
                    if (material.HasTexture(textureNames[j])) // 该方法获取不到不属于该shader的贴图（切换材质后unity会保留之前shader的信息）
                    {
                        Texture texture = material.GetTexture(textureNames[j]); // 获取材质上的贴图引用
                        if (texture != null)
                        {
                            //Debug.Log("获取到图片名字：" + texture.name);
                            if (texture.name == selectTexName)
                            {
                                Debug.Log(relativePath);
                            }
                        }
                    }
                }
                //if (Regex.IsMatch(File.ReadAllText(file), guid))
                //{
                //    Debug.Log(GetRelativeAssetsPath(file));
                //}

                startIndex++;
                if (isCancel || startIndex >= files.Length)
                {
                    EditorUtility.ClearProgressBar();
                    EditorApplication.update = null;
                    startIndex = 0;
                    Debug.Log("匹配结束");
                }

            };
        }
    }

    [MenuItem("Assets/FindReferences/查找shader被哪些材质依赖了", false, 10)]
    static private void FindShaderBeDependOnMat()
    {
        EditorSettings.serializationMode = SerializationMode.ForceText;
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        string selectShaderName = Selection.activeObject.name;
        if(Selection.activeObject.GetType() != typeof(Shader))
        {
            Debug.LogError("选中类型错误");
            return;
        }

        if (!string.IsNullOrEmpty(path))
        {
            string guid = AssetDatabase.AssetPathToGUID(path);
            List<string> withoutExtensions = new List<string>() { ".mat" };
            string[] files = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories).Where(s => withoutExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
            int startIndex = 0;

            EditorApplication.update = delegate ()
            {
                string file = files[startIndex];

                bool isCancel = EditorUtility.DisplayCancelableProgressBar("匹配资源中", file, (float)startIndex / (float)files.Length);

                var relativePath = GetRelativeAssetsPath(file);
                Material material = AssetDatabase.LoadAssetAtPath<Material>(relativePath);
                if(material.shader.name == selectShaderName)
                {
                    Debug.Log(relativePath);
                }
                //Debug.Log("遍历所有需要修改替换贴图的材质：" + file + ",贴图数:" + textureNames.Length);

                //if (Regex.IsMatch(File.ReadAllText(file), guid))
                //{
                //    Debug.Log(GetRelativeAssetsPath(file));
                //}

                startIndex++;
                if (isCancel || startIndex >= files.Length)
                {
                    EditorUtility.ClearProgressBar();
                    EditorApplication.update = null;
                    startIndex = 0;
                    Debug.Log("匹配结束");
                }

            };
        }
    }

    static private string GetRelativeAssetsPath(string path)
    {
        return "Assets" + Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/');
    }
}
