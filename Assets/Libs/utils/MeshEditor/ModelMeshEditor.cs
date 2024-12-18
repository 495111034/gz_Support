# if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;
using System.IO;
using UnityEditor;

public class ModelMeshEditor : MonoBehaviour
{

    //控制点的大小
    public float pointScale = 1.0f;
    private float lastPointScale = 1.0f;

    Mesh dst_mesh = null;
    Mesh source_mesh = null;

    //顶点列表
    List<Vector3> positionList = new List<Vector3>();

    //顶点控制物体列表
    List<GameObject> positionObjList = new List<GameObject>();

    /// <summary>
    /// key:顶点字符串
    /// value:顶点在列表中的位置
    /// </summary>
    Dictionary<string, List<int>> pointmap = new Dictionary<string, List<int>>();

    // Use this for initialization
    void Start()
    {
        lastPointScale = pointScale;

        if(gameObject.GetComponentEx<MeshFilter>())
        {
            var meshFilter = gameObject.GetComponentEx<MeshFilter>();
            source_mesh = meshFilter.sharedMesh;

            dst_mesh = GameObject.Instantiate(source_mesh);
            dst_mesh.name = source_mesh.name;
            meshFilter.sharedMesh = dst_mesh;
        }
        else
        {
            if(gameObject.GetComponentEx<SkinnedMeshRenderer>())
            {
                var meshRender = gameObject.GetComponentEx<SkinnedMeshRenderer>();
                source_mesh = meshRender.sharedMesh;

                dst_mesh = GameObject.Instantiate(source_mesh);
                dst_mesh.name = source_mesh.name;
                meshRender.sharedMesh = dst_mesh;
            }
        }
        
        if(!source_mesh)
        {
            Log.LogError("当前gameobject没有mesh");
            return;
        }
        
      
        CreateEditorPoint();
    }

#if UNITY_EDITOR
    private void OnGUI()
    {
        if(GUI.Button(new Rect(0,0, GUIScale.Scale(100),GUIScale.Scale(50)),new GUIContent("保存")))
        {
            SaveMeshOffsetToPic();
        }
    }
#endif
    //创建控制点
    public void CreateEditorPoint()
    {
        positionList = new List<Vector3>(dst_mesh.vertices);

        for (int i = 0; i < dst_mesh.vertices.Length; i++)
        {
            string vstr = Vector2String(dst_mesh.vertices[i]);

            if (!pointmap.ContainsKey(vstr))
            {
                pointmap.Add(vstr, new List<int>());
            }
            pointmap[vstr].Add(i);
        }

        foreach (string key in pointmap.Keys)
        {
            GameObject editorpoint = (GameObject)UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/prefabsdata/MeshEditorPoint.prefab");
            editorpoint = Instantiate(editorpoint);
            editorpoint.transform.parent = transform;
            editorpoint.transform.localPosition = String2Vector(key);
            editorpoint.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);

            MeshEditorPoint editorPoint = editorpoint.GetComponent<MeshEditorPoint>();
            editorPoint.onMove = PointMove;
            editorPoint.pointid = key;

            positionObjList.Add(editorpoint);
        }
    }

    //顶点物体被移动时调用此方法
    public void PointMove(string pointid, Vector3 position)
    {
        if (!pointmap.ContainsKey(pointid))
        {
            return;
        }

        List<int> _list = pointmap[pointid];

        for (int i = 0; i < _list.Count; i++)
        {
            positionList[_list[i]] = position;
        }

        dst_mesh.vertices = positionList.ToArray();
        dst_mesh.RecalculateNormals();
    }

    // Update is called once per frame
    void Update()
    {
        //检测控制点尺寸是否改变
        if (Math.Abs(lastPointScale - pointScale) > 0.1f)
        {
            lastPointScale = pointScale;
            for (int i = 0; i < positionObjList.Count; i++)
            {
                positionObjList[i].transform.localScale = new Vector3(pointScale, pointScale, pointScale);
            }
        }
    }

    string Vector2String(Vector3 v)
    {
        StringBuilder str = new StringBuilder();
        str.Append(v.x).Append(",").Append(v.y).Append(",").Append(v.z);
        return str.ToString();
    }

    Vector3 String2Vector(string vstr)
    {
        try
        {
            string[] strings = vstr.Split(',');
            return new Vector3(float.Parse(strings[0]), float.Parse(strings[1]), float.Parse(strings[2]));
        }
        catch (Exception e)
        {
            Debug.LogError(e.ToString());
            return Vector3.zero;
        }
    }

    /// <summary>
    /// 保存顶点变化信息到图片
    /// </summary>
    void SaveMeshOffsetToPic()
    {
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

        var fname = AssetDatabase.GetAssetPath(source_mesh).ToLower();
        string savedPath = System.IO.Path.GetDirectoryName(fname) + dst_mesh.name + ".bytes";
        using (FileStream fileStream = new FileStream(savedPath, FileMode.Create))
        {
            MyProtobuf.BinByteBufWriter bw = new MyProtobuf.BinByteBufWriter();
            bw.WriteInt32(textureWidth);
            bw.WriteInt32(textureHeight);
            bw.WriteBytes(texture.GetRawTextureData());

            fileStream.Write(bw.GetBuffer(), 0, bw.GetLength());
            fileStream.Close();

        }
    }

    float GetNearestPowerOfTwo(float x)
    {
        return Mathf.Pow(2f, Mathf.Ceil(Mathf.Log(x) / Mathf.Log(2f)));
    }
}

#endif