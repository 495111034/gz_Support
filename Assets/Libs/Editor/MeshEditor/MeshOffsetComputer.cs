using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Globalization;
using System.Collections;


public class MeshOffsetComputerWizard : ScriptableWizard
{
    public Mesh source;
    public GameObject dest;

    void OnWizardUpdate()
    {
        helpString = "请将原形拖到source，将结果拖到dest";
        isValid = source   && (dest  && dest.GetComponentEx<MeshFilter>());
    }

    void OnWizardCreate()
    {     
        var source_mesh = (Mesh)Object.Instantiate(source);
        var dst_mesh = (Mesh)Object.Instantiate(((dest as GameObject).GetComponentEx<MeshFilter>()).sharedMesh);
        dst_mesh.name = dest.name.ToLower();
        source_mesh.name = source.name.ToLower();

        var fname = AssetDatabase.GetAssetPath(dest).ToLower();

        int textureWidth, textureHeight;
        int vertexCount = source_mesh.vertexCount < dst_mesh.vertexCount ? source_mesh.vertexCount : dst_mesh.vertexCount;
       

        textureWidth = textureHeight = (int)GetNearestPowerOfTwo(Mathf.Sqrt(vertexCount));

        Texture2D texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBAFloat, false);
        texture.filterMode = FilterMode.Point;
        Color[] pixels = texture.GetPixels();

        for(int i = 0; i < vertexCount; ++i)
        {
            var v1 = source_mesh.vertices[i];
            var v2 = dst_mesh.vertices[i];

            pixels[i].r = v2.x - v1.x;
            pixels[i].g = v2.y - v1.y;
            pixels[i].b = v2.z - v1.z;
            pixels[i].a = 0f;
            //Log.LogError($"{i}、r={pixels[i].r},g={pixels[i].g},b={pixels[i].b},,a={pixels[i].a}");

        }

        texture.SetPixels(pixels);
        texture.Apply();

        var savedPath = System.IO.Path.GetDirectoryName(fname) + dst_mesh.name + ".bytes";
        using (FileStream fileStream = new FileStream(savedPath, FileMode.Create))
        {
            MyProtobuf.BinByteBufWriter bw = new MyProtobuf.BinByteBufWriter();
            bw.WriteInt32(textureWidth);
            bw.WriteInt32(textureHeight);
            bw.WriteBytes(texture.GetRawTextureData()); 

            fileStream.Write(bw.GetBuffer(), 0, bw.GetLength());
            fileStream.Close();

        }

        DestroyImmediate(source_mesh);
        DestroyImmediate(dst_mesh);

       

        AssetDatabase.Refresh();
    }


    [MenuItem("GameObject/顶点偏移量")]
    static void RenderCubemap()
    {
        ScriptableWizard.DisplayWizard<MeshOffsetComputerWizard>(
            "顶点偏移量", "计算");       
    }

    static private void CalculateTextureSize(int numPixels, out int texWidth, out int texHeight)
    {
        texWidth = 1;
        texHeight = 1;
        while (true)
        {
            if (texWidth * texHeight >= numPixels) break;
            texWidth *= 2;
            if (texWidth * texHeight >= numPixels) break;
            texHeight *= 2;
        }
    }

    public static float GetNearestPowerOfTwo(float x)
    {
        return Mathf.Pow(2f, Mathf.Ceil(Mathf.Log(x) / Mathf.Log(2f)));
    }
}


