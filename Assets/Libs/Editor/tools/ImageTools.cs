using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;



public class ImageTools: Editor
{
    [MenuItem("Assets/Images/RemoveAlpha")]
    public static void RemoveAlpha()
    {
        foreach (var i in Selection.objects)
        {
            var tex = i as Texture2D;
            if (!tex)
                continue;

            var path = AssetDatabase.GetAssetPath(tex);
            var path2 = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory() ,  System.IO.Path.Combine(System.IO.Path.GetDirectoryName(path), tex.name + "_noAlpah.jpg"));
           // Log.LogError(path2);
            var textCopy = new Texture2D(tex.width, tex.height);
            for(int m = 0; m < tex.width; ++m)
                for(int n = 0; n < tex.height; ++n)
                {
                    var color = tex.GetPixel(m, n);
                    textCopy.SetPixel(m, n, new Color(color.r, color.g, color.b));
                }

            var datas = textCopy.EncodeToJPG(100);
            var fl = System.IO.File.Open(path2, System.IO.FileMode.CreateNew);
            fl.Write(datas, 0, datas.Length);
            fl.Close();
            fl.Dispose();
            AssetDatabase.Refresh();
           // AssetDatabase.CreateAsset(textCopy, path2);
           //AssetDatabase.SaveAssets();
           // Log.LogError(path);
        }
    }
}

