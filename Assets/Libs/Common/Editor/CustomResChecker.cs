using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
//using UnityEngine.UI;
//using UnityEditorInternal;

using UnityEditor;


public class MyData
{
    public string desc = "";
    public float memory = 0.0f;
    public string reason = "";
    public string file = "";
    public string file2 = "";
    public string file3 = "";
    public bool isreadwrite = false;
    public int tag = 0;
    //public float runmemory;
    public UnityEngine.Object obj;

    public MyData(string sFile, string sReason, float fMemory = 0.0f, bool breadwrite = false)
    {
        file = sFile;
        reason = sReason;
        memory = fMemory;
        isreadwrite = breadwrite;

        desc = "";
    }

    public MyData(string sFile, string sFile2, string sReason, float fMemory = 0.0f, bool breadwrite = false)
    {
        file = sFile;
        file2 = sFile2;
        reason = sReason;
        memory = fMemory;
        isreadwrite = breadwrite;

        desc = "";
    }
}

public class CustomResChecker
{


    protected string[] mScanDirs;
    protected string[] mScanAssets;

    //所有资源路径
    protected string[] mAllPath;

    public string mResultInfo = "";
    public Dictionary<string, MyData> mData = new Dictionary<string, MyData>();
    //static public Dictionary<string, string> mDescDic_CanWrite = new Dictionary<string, string>();

    public void SetScanDirs(string[] dirs)
    {
        mScanDirs = dirs;
        mScanAssets = null;
    }

    public void SetScanAssets(string[] assets)
    {
        mScanAssets = assets;
        mScanDirs = null;
    }
    public virtual void Begin()
    {

    }

    public virtual void Checking()
    {

    }

    public virtual void End()
    {

    }

    public void Execute()
    {
        this.Begin();
        EditorUtility.DisplayProgressBar("Analyze", GetType().Name, 0f);
        this.Checking();
        this.End();
        EditorUtility.ClearProgressBar();        
    }

   
       public static void SelectToPath(string path)
      {
          UnityEngine.Object obj = AssetDatabase.LoadMainAssetAtPath(path);
          if (obj == null)
              return;
          EditorGUIUtility.PingObject(obj);
          Selection.activeObject = obj;
      }


    ////////////////////////////////////////////////////////////////
       public  void DoAssetReimport(string path, ImportAssetOptions options)
       {
           try
           {
               AssetDatabase.StartAssetEditing();
               AssetDatabase.ImportAsset(path, options);
           }
           finally
           {
               AssetDatabase.StopAssetEditing();
           }
       }

       public static string[] GetAllGUIDBySelect(string filter, System.Func<UnityEngine.Object, string> func)
       {
           string[] guids = null;
           List<string> path = new List<string>();
           List<string> assets = new List<string>();
           UnityEngine.Object[] objs = Selection.GetFiltered(typeof(object), SelectionMode.Assets);
           if (objs.Length > 0)
           {
               for (int i = 0; i < objs.Length; i++)
               {
                   var guid = func(objs[i]);
                   if (!string.IsNullOrEmpty(guid))
                       assets.Add(guid);
                   else
                       path.Add(AssetDatabase.GetAssetPath(objs[i]));
               }
               if (path.Count > 0)
                   guids = AssetDatabase.FindAssets(filter, path.ToArray());
               else
                   guids = new string[] { };
           }
           for (int i = 0; i < guids.Length; i++)
           {
               assets.Add(guids[i]);
           }
           return assets.ToArray();
       }

       public static string[] GetAllPathsBySelect()
       {
           List<string> path = new List<string>();
           UnityEngine.Object[] objs = Selection.GetFiltered(typeof(object), SelectionMode.Assets);
           if (objs.Length > 0)
           {
               for (int i = 0; i < objs.Length; i++)
               {
                   string assetPath = AssetDatabase.GetAssetPath(objs[i]);

                   string fullPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/')) + "/" + assetPath;
                   DirectoryInfo info = new DirectoryInfo(fullPath);
                   if (info.Exists)
                       path.Add(assetPath);
                   //                   Debug.Log(Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/')) + "/" + AssetDatabase.GetAssetPath(aa[0]));                  
               }
           }
           return path.ToArray();
       }

        //获取当前场景中所有使用的材质贴图
       public static bool FindAllTextureInCurScene(string[] addAssetGuids, out List<string> outres)
        {            
            Dictionary<string, string> res = new Dictionary<string, string>();
            List<string> resPaths = new List<string>();
            foreach (GameObject go in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
            {
                if(go == null)
                    continue;

                #region 找到材质中的texture
                Renderer[] renders = go.GetComponentsInChildren<Renderer>(true);
                foreach (var render in renders)
                {
                    foreach (var mat in render.sharedMaterials)
                    {
                        if (!mat) continue;
                        //判断shader用的贴图
                        for (int i = 0; i < ShaderUtil.GetPropertyCount(mat.shader); i++)
                        {
                            if (ShaderUtil.GetPropertyType(mat.shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
                            {
                                string propertyname = ShaderUtil.GetPropertyName(mat.shader, i);
                                Texture t = mat.GetTexture(propertyname);
                                if (t)
                                {
                                    string assetPath = AssetDatabase.GetAssetPath(t);                                
                                    if (! res.Keys.Contains(assetPath))
                                    {
                                        res.Add(assetPath, assetPath);
                                    }                                           
                                }
                            }
                        }
                    }
                }
                #endregion

                /*
                #region 找到Image
                Image[] images = go.GetComponentsInChildren<Image>(true);
                foreach (var img in images)
                {
                   // if (AssetDatabase.GetAssetPath(img.sprite).Contains(builtin))
                    {
                        if (img)
                        {
                            string assetPath = AssetDatabase.GetAssetPath(img);
                            if (!res.Keys.Contains(assetPath))
                            {
                                res.Add(assetPath, assetPath);
                            }
                        }
                    }
                }

                #endregion
                #region 找到RawImage
                            RawImage[] rawimgs = go.GetComponentsInChildren<RawImage>(true);
                            foreach (var rawimg in rawimgs)
                            {
                                if (rawimg.texture )
                                {
                                    string assetPath = AssetDatabase.GetAssetPath(rawimg.texture);
                                    if (!res.Keys.Contains(assetPath))
                                    {
                                        res.Add(assetPath, assetPath);
                                    }                                           
                                }
                            }
                            #endregion                            
                

                 //特定资源
                */
                    if (addAssetGuids != null && addAssetGuids.Length > 0)
                    {

                        for (int i = 0; i < addAssetGuids.Length; ++i)
                        {
                            string assetPath = AssetDatabase.GUIDToAssetPath(addAssetGuids[i]);
                            if (!res.Keys.Contains(assetPath))
                            {
                                res.Add(assetPath, assetPath);
                            }
                        }
                    }
                    // if(addPaths != null && addPaths.Length > 0)
                    //{
                    //    string[] strTextPaths = AssetDatabase.FindAssets("t:texture", addPaths);
                    //    if (strTextPaths != null && strTextPaths.Length > 0)
                    //    {

                    //        for (int i = 0; i < strTextPaths.Length; ++i)
                    //        {
                    //            string assetPath = AssetDatabase.GUIDToAssetPath(strTextPaths[i]);
                    //            if (!res.Keys.Contains(assetPath))
                    //            {
                    //                res.Add(assetPath, assetPath);
                    //            }
                    //        }
                    //    }
                    //}
                    

                }

            //Material[] Materials = GameObject.FindObjectsOfType<Material>();
            //foreach (var mat in Materials)
            //{
            //    for (int i = 0; i < ShaderUtil.GetPropertyCount(mat.shader); i++)
            //    {
            //        if (ShaderUtil.GetPropertyType(mat.shader, i) == ShaderUtil.ShaderPropertyType.TexEnv)
            //        {
            //            string propertyname = ShaderUtil.GetPropertyName(mat.shader, i);
            //            Texture t = mat.GetTexture(propertyname);
            //            if (t)
            //            {
            //                string assetPath = AssetDatabase.GetAssetPath(t);
            //                if (!res.Keys.Contains(assetPath))
            //                {
            //                    res.Add(assetPath, assetPath);
            //                }
            //            }
            //        }
            //    }
            //}

            foreach (KeyValuePair<string, string> item in res)
            {
               // string fullPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/')) + "/" + item.Key;
                string fullPath = item.Key;
                string guid = AssetDatabase.AssetPathToGUID(fullPath);
                resPaths.Add(guid);                
            }

            

            outres = resPaths;
            return true;
        }

}
