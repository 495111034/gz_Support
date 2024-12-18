using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SmallMapBehaviour))]
class SmallMapInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("重新截屏"))
        {
            TakeScreenshot(target as SmallMapBehaviour);
        }
    }
    static Vector2 spos = Vector2.zero;

    //
    static void TakeScreenshot(SmallMapBehaviour smb)
    {
        var width = smb.tex_width;
        var height = smb.tex_height;
        var camera = smb.GetComponent<Camera>();

        // 屏幕选项
        //camera.cullingMask = (int)ObjLayerMask.ViewAll;

        // render
        var rt = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);

        camera.targetTexture = rt;
        camera.Render();
        camera.targetTexture = null;

        // read
        var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        RenderTexture.active = rt;
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        RenderTexture.active = null;

        // save
        var bytes = tex.EncodeToPNG();
        var fname = GetSmallMapSavePathname();
        File.WriteAllBytes(fname, bytes);

        // destroy
        RenderTexture.DestroyImmediate(rt);
        Texture2D.DestroyImmediate(tex);

        //
        AssetDatabase.Refresh();

        //
        smb.tex = AssetDatabase.LoadAssetAtPath(fname, typeof(Texture2D)) as Texture2D;
        Debug.Log("TakeScreenshot, save to: " + fname);
    }


    // 获取保存文件名
    static string GetSmallMapSavePathname()
    {
        var scene_name = Path.GetFileNameWithoutExtension(EditorApplication.currentScene).ToLower();

        var path = PathDefs.ASSETS_PATH_SMALL_MAP;
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);

        var save_name = scene_name + ".png";

        var save_pathname = path + save_name;

        //
        return save_pathname;
    }
}
