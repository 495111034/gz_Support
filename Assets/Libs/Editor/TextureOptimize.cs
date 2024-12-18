using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

//�����Ż�
static class TextureOptimize
{
    [MenuItem("Assets/�Ƴ�����Ҫalpha��Ϣ����ͼ")]
    static void RemoveTextUeslessAlphaInfo()
    {
        //��Ҫע��һ��AssetPostProcessor��
        //ti.alphaSource = TextureImporterAlphaSource.FromInput;��58�У�
        //ti.isReadable = _update(ti.isReadable, false, ref dirty); 77��82��
        //�����޸ĵ�ʱ��������Ϊ�����ʱ���ǻ�ȡ����ͼƬ�ģ�Ҳ����û�취�ж�ͼƬ��alpha��Ϣ
        try
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture", new string[] { "Assets/actor", "Assets/FX", "Assets/scene", });
            //string[] guids = AssetDatabase.FindAssets("t:Texture", new string[] { "Assets/scene/1001_kc", });
            int progress = 0;
            // ��ͣAssetPostprocessor�Ĵ���
            //AssetDatabase.StartAssetEditing(); ������
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                bool isCancel = EditorUtility.DisplayCancelableProgressBar("�Ƴ�����Ҫalpha��Ϣ����ͼ", assetPath, (float)progress / (float)guids.Length);
                progress++;
                Debug.Log("�����ļ���" + assetPath);
                TextureImporter textureImporter = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                if (textureImporter.textureType == TextureImporterType.Default && textureImporter.alphaSource != TextureImporterAlphaSource.None)
                {
                    textureImporter.isReadable = true;
                    AssetDatabase.ImportAsset(assetPath);
                    Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
                    if (texture != null)
                    {
                        Debug.Log("����ͼƬ��" + texture);
                        if (CheckTextureAlphaChannel(texture))
                        {
                            textureImporter.alphaSource = TextureImporterAlphaSource.None;
                            Debug.Log("�Ƴ�����Ҫalpha��Ϣ����ͼ��" + assetPath);
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
            // �ָ�AssetPostprocessor�Ĵ���
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

    [MenuItem("Assets/�Ƴ��ظ���ģ����ͼ/All")]
    static void RemoveReportTexture_All()
    {
        RemoveReportTexture(new string[] { "Assets/actor", "Assets/FX", "Assets/scene", });
    }

    [MenuItem("Assets/�Ƴ��ظ���ģ����ͼ/FX")]
    static void RemoveReportTexture_FX()
    {
        RemoveReportTexture(new string[] { "Assets/FX" });
    }

    [MenuItem("Assets/�Ƴ��ظ���ģ����ͼ/Actor")]
    static void RemoveReportTexture_Actor()
    {
        RemoveReportTexture(new string[] { "Assets/actor" });
    }

    [MenuItem("Assets/�Ƴ��ظ���ģ����ͼ/Scene")]
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
        //Debug.Log("��ͼ" + guids.Length);
        //1.����������ͼ ���ֵ仺�� �ҵ��ظ�����Դ 
        int progress = 0;
        foreach (var assetGuid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(assetGuid); // ��GUID�õ���Դ��·��
            bool isCancel = EditorUtility.DisplayCancelableProgressBar("�Ƴ��ظ�����ͼ", assetPath, (float)progress / (float)guids.Length);
            progress++;
            string md5 = GetMD5Hash(Path.Combine(Directory.GetCurrentDirectory(), assetPath)); //��ȡmd5
            string path;
            md5dic.TryGetValue(md5, out path);
            if (path == null)
            {
                md5dic[md5] = assetPath;
                // Debug.Log(assetPath);
            }
            else
            {
                Debug.LogFormat("��Դ�ظ�{0}��{1}", path, assetPath);
                if (matFiles == null)
                {
                    //2.�ҵ����в��ʣ����һ���
                    List<string> withoutExtensions = new List<string>() { ".mat" };
                    matFiles = Directory.GetFiles(Application.dataPath, "*.*", SearchOption.AllDirectories).Where(s => withoutExtensions.Contains(Path.GetExtension(s).ToLower())).ToArray();
                    matFilesContent = new string[matFiles.Length];
                    for (int i = 0; i < matFiles.Length; i++)
                    {
                        matFilesContent[i] = File.ReadAllText(matFiles[i]);
                    }
                }
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path); //������ͼ
                Texture2D deleteTex = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath); //ɾ����ͼ
                //3.�ҵ����ø���ͼ�����в���
                for (int startIndex = 0; startIndex < matFiles.Length; startIndex++)
                {
                    string file = GetRelativeAssetsPath(matFiles[startIndex]);

                    if (Regex.IsMatch(matFilesContent[startIndex], assetGuid))
                    {
                        Material material = AssetDatabase.LoadAssetAtPath<Material>(file);
                        bool isUseTex = false;
                        var textureNames = material.GetTexturePropertyNames();
                        //Debug.Log("����������Ҫ�޸��滻��ͼ�Ĳ��ʣ�" + file + ",��ͼ��:" + textureNames.Length);
                        //4.�жϸò�������������ͼ�ֶ� ���к��ظ���ͼ��ͬ ���滻
                        for (int j = 0; j < textureNames.Length; j++)
                        {
                            if (material.HasTexture(textureNames[j])) // �÷�����ȡ���������ڸ�shader����ͼ���л����ʺ�unity�ᱣ��֮ǰshader����Ϣ��
                            {
                                Texture texture = material.GetTexture(textureNames[j]); // ��ȡ�����ϵ���ͼ����
                                if (texture != null)
                                {
                                    //Debug.Log("��ȡ��ͼƬ���֣�" + texture.name);
                                    if (texture.name == deleteTex.name)
                                    {
                                        isUseTex = true;
                                        material.SetTexture(textureNames[j], tex);
                                        Debug.Log("�޸ĵĲ��ʣ�" + file + "����ͼ" + assetPath + ",assetGuid:" + assetGuid + ",�滻Ϊ��" + path + "�޸Ĳ���propertyName��" + textureNames[j]);
                                        EditorUtility.SetDirty(material);
                                    }
                                }
                            }
                        }
                    }
                }

                //5.�滻�����в��ʣ�ɾ�����ظ���ͼ
                //if(isChangeSucceedCount != referencesMatPath.Count)
                //{
                //    Debug.LogError("+++�޸�ʧ��+++isChangeSucceedCount��"+ isChangeSucceedCount+ "+++referencesMatPath.Count��" + referencesMatPath.Count);
                //}
                //else
                //{
                //    AssetDatabase.DeleteAsset(assetPath);
                //    Debug.LogError("+++�޸ĳɹ�+++");
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
                bool isCancel = EditorUtility.DisplayCancelableProgressBar("�������Ƿ�ȱ����ͼ", file, (float)checkProgress / (float)matFiles.Length);
                checkProgress++;

                Material material = AssetDatabase.LoadAssetAtPath<Material>(file);

                SerializedObject serializedMaterial = new SerializedObject(material);
                SerializedProperty texturesProperty = serializedMaterial.FindProperty("m_SavedProperties.m_TexEnvs");

                foreach (SerializedProperty textureProperty in texturesProperty)
                {
                    string propertyName = textureProperty.displayName;
                    SerializedProperty textureReference = textureProperty.FindPropertyRelative("second.m_Texture");

                    // �����ͼ�����Ƿ�ʧ
                    if (material.shader.FindPropertyIndex(propertyName) > 0 && textureReference.objectReferenceValue == null && deleteTexInsId.Contains(textureReference.objectReferenceInstanceIDValue))
                    {
                        Debug.LogError($"�Ƴ��ظ���ģ����ͼ���� Missing texture in material: {material.name}, Property: {propertyName}");
                    }
                }
            }
            EditorUtility.ClearProgressBar();
        }
    }

    //[MenuItem("Assets/���Լ����ʶ�ʧ��ͼ")]
    //static void TestCheckMatMissTex()
    //{
    //    Material material = AssetDatabase.LoadAssetAtPath<Material>("Assets/FX/Charcater/Berserker/mat/mat_fx_warrior_attack_1_14.mat");
    //    CheckMaterialTextures(material);
    //}

    //[MenuItem("Assets/������ͼobjectReferenceInstanceIDValue")]
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

    //        // �����ͼ�����Ƿ�ʧ
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

    [MenuItem("Assets/FindReferences/������ͼ����Щ����������", false, 10)]
    static private void FindTexBeDependOnMat()
    {
        EditorSettings.serializationMode = SerializationMode.ForceText;
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        string selectTexName = Selection.activeObject.name;
        if (Selection.activeObject.GetType() != typeof(Texture2D))
        {
            Debug.LogError("ѡ�����ʹ���");
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

                bool isCancel = EditorUtility.DisplayCancelableProgressBar("ƥ����Դ��", file, (float)startIndex / (float)files.Length);

                var relativePath = GetRelativeAssetsPath(file);
                Material material = AssetDatabase.LoadAssetAtPath<Material>(relativePath);
                var textureNames = material.GetTexturePropertyNames();
                //Debug.Log("����������Ҫ�޸��滻��ͼ�Ĳ��ʣ�" + file + ",��ͼ��:" + textureNames.Length);
                for (int j = 0; j < textureNames.Length; j++)
                {
                    if (material.HasTexture(textureNames[j])) // �÷�����ȡ���������ڸ�shader����ͼ���л����ʺ�unity�ᱣ��֮ǰshader����Ϣ��
                    {
                        Texture texture = material.GetTexture(textureNames[j]); // ��ȡ�����ϵ���ͼ����
                        if (texture != null)
                        {
                            //Debug.Log("��ȡ��ͼƬ���֣�" + texture.name);
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
                    Debug.Log("ƥ�����");
                }

            };
        }
    }

    [MenuItem("Assets/FindReferences/����shader����Щ����������", false, 10)]
    static private void FindShaderBeDependOnMat()
    {
        EditorSettings.serializationMode = SerializationMode.ForceText;
        string path = AssetDatabase.GetAssetPath(Selection.activeObject);
        string selectShaderName = Selection.activeObject.name;
        if(Selection.activeObject.GetType() != typeof(Shader))
        {
            Debug.LogError("ѡ�����ʹ���");
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

                bool isCancel = EditorUtility.DisplayCancelableProgressBar("ƥ����Դ��", file, (float)startIndex / (float)files.Length);

                var relativePath = GetRelativeAssetsPath(file);
                Material material = AssetDatabase.LoadAssetAtPath<Material>(relativePath);
                if(material.shader.name == selectShaderName)
                {
                    Debug.Log(relativePath);
                }
                //Debug.Log("����������Ҫ�޸��滻��ͼ�Ĳ��ʣ�" + file + ",��ͼ��:" + textureNames.Length);

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
                    Debug.Log("ƥ�����");
                }

            };
        }
    }

    static private string GetRelativeAssetsPath(string path)
    {
        return "Assets" + Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/');
    }
}
