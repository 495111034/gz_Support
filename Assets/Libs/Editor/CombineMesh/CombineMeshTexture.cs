

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class CombineMeshTexture
{

    [MenuItem("GameObject/合并网格", false, 2)]
    public static void Combine()
    {
        GameObject g = Selection.gameObjects[0];
        if(g != null)
        {
            _Combine(g);
        }
    }

    private static void _Combine(GameObject g)
    {
        List<Texture2D> textures = new List<Texture2D>();
        MeshRenderer[] meshRenderers = g.GetComponentsInChildren<MeshRenderer>();
        foreach(MeshRenderer meshRender in meshRenderers)
        {
            textures.Add((Texture2D)meshRender.material.mainTexture);
        }

        Texture2D texture2D = new Texture2D(2048, 2048);
        Debug.Log(textures.Count);
        Rect[] rects = texture2D.PackTextures((textures.ToArray()), 0);
        texture2D.Apply();
        texture2D.name = g.name + "_texture";
        Texture2D reTexture2D = new Texture2D(texture2D.width, texture2D.height, TextureFormat.ARGB32, false);
        reTexture2D.name = g.name + "_texture";
        reTexture2D.SetPixels32(texture2D.GetPixels32());
        byte[] bytes = reTexture2D.EncodeToPNG();
        if(!Directory.Exists("assets/temp/combine/combine_texture")) Directory.CreateDirectory("assets/temp/combine/combine_texture");
        if (File.Exists("assets/temp/combine/combine_texture/" + texture2D.name + ".png")) File.Delete("assets/temp/combine/combine_texture/" + texture2D.name + ".png");

        File.WriteAllBytes("assets/temp/combine/combine_texture/" + texture2D.name + ".png",bytes);
        AssetDatabase.Refresh();
        //reTexture2D = AssetDatabase.LoadAssetAtPath<Texture2D>("assets/temp/combine/combine_texture/" + texture2D.name + ".png");
        _CombineMesh(g, textures,reTexture2D, rects);
        g.SetActive(false);
    }

    private static void _CombineMesh(GameObject g, List<Texture2D> textures, Texture2D combineTexture2D,Rect[] rects)
    {
        GameObject combineObject = new GameObject("CombineObject");
        combineObject.transform.SetParent(g.transform.parent, false);
        MeshFilter CombineMeshFilter =  combineObject.AddComponent<MeshFilter>();
        MeshRenderer CombineMeshRenderer = combineObject.AddComponent<MeshRenderer>();

        List<Vector2[]> uvs = new List<Vector2[]>(); 
        MeshFilter[] meshFilters = g.GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combineInstance = new CombineInstance[meshFilters.Length];
        for (int i=0;i < meshFilters.Length;i ++)
        {
            combineInstance[i].mesh = meshFilters[i].sharedMesh;
            combineInstance[i].transform = meshFilters[i].transform.localToWorldMatrix;
            uvs.Add(meshFilters[i].sharedMesh.uv);
        }
        CombineMeshFilter.sharedMesh = new Mesh();
        CombineMeshFilter.sharedMesh.CombineMeshes(combineInstance);
        Vector2[] uv = new Vector2[CombineMeshFilter.sharedMesh.vertices.Length];

        int count = 0;
        for(int i = 0; i < uvs.Count; i++)
        {
            float scaleX = ((float)textures[i].width / combineTexture2D.width);
            float scaleY = ((float)textures[i].height / combineTexture2D.height);
            for (int uv_index = 0; uv_index < uvs[i].Length; uv_index++)
            {
                uv[count] = new Vector2(uvs[i][uv_index].x* scaleX + rects[i].xMin, uvs[i][uv_index].y * scaleY + rects[i].yMin);
                count++;
            }
        }
        CombineMeshFilter.sharedMesh.uv = uv;
        AssetDatabase.CreateAsset(CombineMeshFilter.sharedMesh, "assets/temp/combine/combine_texture/" + g.name + "mesh.asset");        

        Material material = new Material(resource.ShaderManager.Find("Standard"));        
        material.mainTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("assets/temp/combine/combine_texture/" + combineTexture2D.name + ".png");
        CombineMeshRenderer.material = material;
        AssetDatabase.CreateAsset(material, "assets/temp/combine/combine_texture/" + combineTexture2D.name + ".mat");

        AssetDatabase.SaveAssets();

        string prefabPath = "assets/temp/combine/combine_texture/CombineObject.prefab";
        var mysper_bo = PrefabUtility.SaveAsPrefabAsset(combineObject, prefabPath);
        GameObject.DestroyImmediate(combineObject);

        var newobj = GameObject.Instantiate(mysper_bo);
        newobj.name = mysper_bo.name;
        

    }

}
