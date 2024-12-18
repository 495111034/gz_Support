using UnityEngine;
using UnityEditor;
using UnityEditor.Macros;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SimpleGrass
{


    public class SimpleEditorCommon
    {
       public static string PrefabPath = "Assets/SimpleGrass/Prefabs/";
       public static string ProfilePath = "Assets/SimpleGrass/Profiles/";
       public static string OldProfilePath = "Assets/SimpleGrass/Editor/Profiles/";
        //public static string SaveDataPath = "Assets/SimpleGrass/SaveData/";
        public static string SaveColliderNodeName = "SimpleGrass_Colliders";
       public static string ColliderParentPrefix = "Layer_";
        public static string SaveWorldDataPath = "Assets/SimpleGrass/WorldDatas/";

        public static float Def_CullingMaxDist = 400;
       public static bool Def_CastShadows = false;
       public static bool Def_ReceiveShadows = false;
       public static int Def_Layer = 0;
       public static float Def_MergeChunkDistance = 10;
        public static int MAX_GRASS_NUMLIMIT = 8000;
        public static int MAX_GRASS_SHOWLIMIT = 3000;
        public static int MAX_GRASS_REALSHOWLIMIT = 500;

        public static void GetChunkPrefabs(out List<GUIContent> contents, out List<GameObject> values,out List<string> prefabsPaths)
        {
            List<GUIContent> outObjsName = new List<GUIContent>();
            List<GameObject> outObjs = new List<GameObject>();
            List<string> outPrefabsPaths = new List<string>();
            List<string> prefabs_names = new List<string>();
            //获取指定路径下面的所有资源文件
            if (Directory.Exists(PrefabPath))
            {
                DirectoryInfo direction = new DirectoryInfo(PrefabPath);
                FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);
                for (int i = 0; i < files.Length; i++)
                {
                    if (files[i].Name.EndsWith(".prefab"))
                    {
                        string ab_name = PrefabPath;
                        if(files[i].Directory.Name == "Prefabs" )               
                          prefabs_names.Add(ab_name + files[i].Name);
                        else if(files[i].Directory.Parent.Name == "Prefabs")
                            prefabs_names.Add(ab_name + files[i].Directory.Name + "/" + files[i].Name);
                    }
                }

                for (int i = 0; i < prefabs_names.Count; i++)
                {
                    GameObject go = AssetDatabase.LoadAssetAtPath(prefabs_names[i], typeof(System.Object)) as GameObject;
                    if (go != null)
                    {
                        SimpleGrassChunk cmp = go.GetComponentInChildren<SimpleGrassChunk>();
                        if (cmp != null && cmp.grassPrefab != null)
                        {
                            outObjs.Add(go);
                            outObjsName.Add(new GUIContent(go.name));
                            outPrefabsPaths.Add(prefabs_names[i]);
                        }
                    }
                }
            }else
            {
                //创建目录
                 Directory.CreateDirectory(PrefabPath);
            }

            values = outObjs;
            contents = outObjsName;
            prefabsPaths = outPrefabsPaths;
        }


        public static void GetProfiles(out List<GUIContent> contents, out List<string> filepaths)
        {
            List<GUIContent> outObjsName = new List<GUIContent>();
            List<string> outObjs = new List<string>();
            //获取指定路径下面的所有资源文件
            if (Directory.Exists(ProfilePath))
            {
                DirectoryInfo direction = new DirectoryInfo(ProfilePath);
                FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);
                for (int i = 0; i < files.Length; i++)
                {
                    if (files[i].Name.EndsWith(".asset"))
                    {
                        string ab_name = ProfilePath;
                        if (files[i].Directory.Name == "Profiles")
                            outObjs.Add(ab_name + files[i].Name);
                        else if (files[i].Directory.Parent.Name == "Profiles")
                            outObjs.Add(ab_name + files[i].Directory.Name + "/" + files[i].Name);

                        string filename = files[i].Name.Split('.')[0];
                        outObjsName.Add(new GUIContent(filename));
                    }
                }
            }
            else
            {
                //创建目录
                Directory.CreateDirectory(PrefabPath);

            }

            filepaths = outObjs;
            contents = outObjsName;
        }


        public static bool DeleteAllFile(string prefix,string fullPath)
        {
            //获取指定路径下面的所有资源文件  然后进行删除
            if (Directory.Exists(fullPath))
            {
                DirectoryInfo direction = new DirectoryInfo(fullPath);
                //FileInfo[] files = direction.GetFiles("*", SearchOption.AllDirectories);
                FileInfo[] files = direction.GetFiles(prefix+"*", SearchOption.AllDirectories);

                Debug.Log(files.Length);

                for (int i = 0; i < files.Length; i++)
                {
                    //if (files[i].Name.EndsWith(".meta"))
                    //{
                    //    continue;
                    //}
                    string FilePath = fullPath + "/" + files[i].Name;                  
                    File.Delete(FilePath);
                }
                return true;
            }
            return false;
        }

    }


  }

