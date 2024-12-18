#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;
using System.IO;
using UnityEditor;

/// <summary>
/// 采样顶点动画
/// </summary>
public class MeshAnimSample : MonoBehaviour
{   
    Mesh source_mesh = null;
    Animation anim;

    bool isRead = false;

    public string defaultAnim;

    void Start()
    {
        if(gameObject.GetComponentEx<SkinnedMeshRenderer>())
        {         
            source_mesh = GameObject.Instantiate(gameObject.GetComponentEx<SkinnedMeshRenderer>().sharedMesh);
            source_mesh.name = System.IO.Path.GetFileNameWithoutExtension(AssetDatabase.GetAssetPath(gameObject.GetComponentEx<SkinnedMeshRenderer>().sharedMesh)).ToLower();
        }
        
        if(source_mesh == null)
        {
            Log.LogError("错误：找不到模型");
            isRead = false;
            return;
        }

        anim = gameObject.GetComponentEx<Animation>();
        if(!anim)
        {
            Log.LogError("错误：找不到动画");
            isRead = false;
            return;
        }

        if (string.IsNullOrEmpty(defaultAnim))
        {
            Log.LogError("错误：请输入动画名");
            isRead = false;
            return;
        }

        if(!anim.GetClip(defaultAnim))
        {
            Log.LogError($"错误：请输入动画名{defaultAnim}不存在");
            isRead = false;
            return;
        }

        isRead = true;

        anim.Play(defaultAnim);

        
    }

    void SameMesh(string animName)
    {
        if (!isRead) return;


        StartCoroutine(SaveMeshOffsetToPic(animName));
    }


    /// <summary>
    /// 保存顶点变化信息到图片
    /// </summary>
    IEnumerator SaveMeshOffsetToPic(string animName)
    {
        yield return new WaitForEndOfFrame();

        var _mesh = gameObject.GetComponentEx<SkinnedMeshRenderer>().sharedMesh;

        Mesh dst_mesh = new Mesh();
        gameObject.GetComponentEx<SkinnedMeshRenderer>().BakeMesh(dst_mesh);
        dst_mesh.name = source_mesh.name + "_" + animName;

        

        int textureWidth, textureHeight;
        int vertexCount = source_mesh.vertexCount < dst_mesh.vertexCount ? source_mesh.vertexCount : dst_mesh.vertexCount;

        textureWidth = textureHeight = (int)GetNearestPowerOfTwo(Mathf.Sqrt(vertexCount));

        Texture2D texture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBAFloat, false);
        texture.filterMode = FilterMode.Point;
        Color[] pixels = texture.GetPixels();

        for (int i = 0; i < vertexCount; ++i)
        {
            var v1 = source_mesh.vertices[i];
            var v2 = dst_mesh.vertices[i];

            pixels[i].r = v2.x - v1.x;
            pixels[i].g = v2.y - v1.y;
            pixels[i].b = v2.z - v1.z;
            pixels[i].a = 0f;

        }

        texture.SetPixels(pixels);
        texture.Apply();

        var fname = AssetDatabase.GetAssetPath(_mesh).ToLower();      
       

       

        string savedPath =$"{ System.IO.Path.GetDirectoryName(fname)}{source_mesh.name}_{animName}.bytes";
        using (FileStream fileStream = new FileStream(savedPath, FileMode.Create))
        {
            MyProtobuf.BinByteBufWriter bw = new MyProtobuf.BinByteBufWriter();
            bw.WriteInt32(textureWidth);
            bw.WriteInt32(textureHeight);
            bw.WriteBytes(texture.GetRawTextureData());

            fileStream.Write(bw.GetBuffer(), 0, bw.GetLength());
            fileStream.Close();

        }

        UnityEditor.AssetDatabase.Refresh();
    }

    float GetNearestPowerOfTwo(float x)
    {
        return Mathf.Pow(2f, Mathf.Ceil(Mathf.Log(x) / Mathf.Log(2f)));
    }
}


#endif
