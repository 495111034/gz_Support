// Converted from UnityScript to C# at http://www.M2H.nl/files/js_to_c.php - by Mike Hergaarden
// C # manual conversion work by Yun Kyu Choi

// 2013.12.25, hproof.
// obj 文件格式参考: http://blog.csdn.net/szchtx/article/details/8628265

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.IO;
using System.Text;

enum SaveFormat { Triangles, Quads }
enum SaveResolution { Full = 0, Half, Quarter, Eighth, Sixteenth }

class ExportTerrain : EditorWindow
{
    SaveFormat saveFormat = SaveFormat.Triangles;
    SaveResolution saveResolution = SaveResolution.Full;

    //static TerrainData terrain;
    //static Vector3 terrainPos;

    int tCount;
    int counter;
    int totalCount;
    int progressUpdateInterval = 10000;

    [MenuItem("Editor/Terrain/Export To Obj...")]
    static void ExportToObj()
    {
        EditorWindow.GetWindow<ExportTerrain>().Show();
    }

    void OnGUI()
    {
        saveFormat = (SaveFormat)EditorGUILayout.EnumPopup("Export Format", saveFormat);
        saveResolution = (SaveResolution)EditorGUILayout.EnumPopup("Resolution", saveResolution);
        if (GUILayout.Button("Export"))
        {
            Export();
        }
    }

    void Export()
    {
        //string fileName = EditorUtility.SaveFilePanel("Export .obj file", "", "Terrain", "obj");

        // 获取地形
        var t = GetTargetTerrain();
        if (!t) return;
        var td = t.terrainData;
        var fileName = GetTerrainMeshPathName(td);

        //
#if UNITY_2020_3_OR_NEWER
        int w = td.heightmapResolution;
        int h = td.heightmapResolution;
#else
        int w = td.heightmapWidth;
        int h = td.heightmapHeight;
#endif
        Vector3 meshScale = td.size;
        int tRes = (int)Mathf.Pow(2, (int)saveResolution);  // 精度: 0=full, 1=half, tRes: 0=1=full, 1=2=half; tRes 是除数
        meshScale = new Vector3(meshScale.x / (w - 1) * tRes, meshScale.y, meshScale.z / (h - 1) * tRes);
        Vector2 uvScale = new Vector2(1.0f / (w - 1), 1.0f / (h - 1));
        float[,] tData = td.GetHeights(0, 0, w, h);

        w = (w - 1) / tRes + 1;
        h = (h - 1) / tRes + 1;
        Vector3[] tVertices = new Vector3[w * h];
        Vector2[] tUV = new Vector2[w * h];

        int[] tPolys;

        if (saveFormat == SaveFormat.Triangles)
        {
            tPolys = new int[(w - 1) * (h - 1) * 6];
        }
        else
        {
            tPolys = new int[(w - 1) * (h - 1) * 4];
        }

        // Build vertices and UVs
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                //tVertices[y * w + x] = Vector3.Scale(meshScale, new Vector3(x, tData[x * tRes, y * tRes], y)) + terrainPos;
                tVertices[y * w + x] = Vector3.Scale(meshScale, new Vector3(-x, tData[y * tRes, x * tRes], y));      // by hproof
                tUV[y * w + x] = Vector2.Scale(new Vector2(x * tRes, y * tRes), uvScale);
            }
        }

        int index = 0;
        if (saveFormat == SaveFormat.Triangles)
        {
            // Build triangle indices: 3 indices into vertex array for each triangle
            for (int y = 0; y < h - 1; y++)
            {
                for (int x = 0; x < w - 1; x++)
                {
                    // For each grid cell output two triangles
                    tPolys[index++] = (y * w) + x;
                    tPolys[index++] = (y * w) + x + 1;
                    tPolys[index++] = ((y + 1) * w) + x;
                    

                    tPolys[index++] = ((y + 1) * w) + x;
                    tPolys[index++] = (y * w) + x + 1;
                    tPolys[index++] = ((y + 1) * w) + x + 1;
                    
                }
            }
        }
        else
        {
            // Build quad indices: 4 indices into vertex array for each quad
            for (int y = 0; y < h - 1; y++)
            {
                for (int x = 0; x < w - 1; x++)
                {
                    // For each grid cell output one quad
                    tPolys[index++] = (y * w) + x;
                    tPolys[index++] = (y * w) + x + 1;
                    tPolys[index++] = ((y + 1) * w) + x + 1;
                    tPolys[index++] = ((y + 1) * w) + x;
                }
            }
        }

        // Export to .obj
        StreamWriter sw = new StreamWriter(fileName);
        bool ok = false;
        try
        {

            sw.WriteLine("# Unity terrain OBJ File");

            // Write vertices
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            counter = tCount = 0;
            totalCount = (tVertices.Length * 2 + (saveFormat == SaveFormat.Triangles ? tPolys.Length / 3 : tPolys.Length / 4)) / progressUpdateInterval;
            for (int i = 0; i < tVertices.Length; i++)
            {
                UpdateProgress();
                StringBuilder sb = new StringBuilder("v ", 20);
                // StringBuilder stuff is done this way because it's faster than using the "{0} {1} {2}"etc. format
                // Which is important when you're exporting huge terrains.
                sb.Append(tVertices[i].x.ToString()).Append(" ").
                   Append(tVertices[i].y.ToString()).Append(" ").
                   Append(tVertices[i].z.ToString());
                sw.WriteLine(sb);
            }
            // Write UVs
            for (int i = 0; i < tUV.Length; i++)
            {
                UpdateProgress();
                StringBuilder sb = new StringBuilder("vt ", 22);
                sb.Append(tUV[i].x.ToString()).Append(" ").
                   Append(tUV[i].y.ToString());
                sw.WriteLine(sb);
            }
            if (saveFormat == SaveFormat.Triangles)
            {
                // Write triangles
                for (int i = 0; i < tPolys.Length; i += 3)
                {
                    UpdateProgress();
                    StringBuilder sb = new StringBuilder("f ", 43);
                    sb.Append(tPolys[i] + 1).Append("/").Append(tPolys[i] + 1).Append(" ").
                       Append(tPolys[i + 1] + 1).Append("/").Append(tPolys[i + 1] + 1).Append(" ").
                       Append(tPolys[i + 2] + 1).Append("/").Append(tPolys[i + 2] + 1);
                    sw.WriteLine(sb);
                }
            }
            else
            {
                // Write quads
                for (int i = 0; i < tPolys.Length; i += 4)
                {
                    UpdateProgress();
                    StringBuilder sb = new StringBuilder("f ", 57);
                    sb.Append(tPolys[i] + 1).Append("/").Append(tPolys[i] + 1).Append(" ").
                       Append(tPolys[i + 1] + 1).Append("/").Append(tPolys[i + 1] + 1).Append(" ").
                       Append(tPolys[i + 2] + 1).Append("/").Append(tPolys[i + 2] + 1).Append(" ").
                       Append(tPolys[i + 3] + 1).Append("/").Append(tPolys[i + 3] + 1);
                    sw.WriteLine(sb);
                }
            }
            ok = true;
        }
        catch (Exception err)
        {
            Debug.Log("Error saving file: " + err.Message);
        }
        sw.Close();

        td = null;
        EditorUtility.ClearProgressBar();
        EditorWindow.GetWindow<ExportTerrain>().Close();

        if (ok)
        {
            AssetDatabase.Refresh();
            Debug.Log("Export obj:" + fileName);
        }
    }

    void UpdateProgress()
    {
        if (counter++ == progressUpdateInterval)
        {
            counter = 0;
            EditorUtility.DisplayProgressBar("Saving...", "", Mathf.InverseLerp(0, totalCount, ++tCount));
        }
    }

    // 保存 Mesh 
    public static void SaveMesh(string fname, Vector3[] vertexs, Vector2[] uvs, int[] tris)
    {
        // Export to .obj
        StreamWriter sw = new StreamWriter(fname);
        sw.WriteLine("# Unity terrain OBJ File");

        // Write vertices
        for (int i = 0; i < vertexs.Length; i++)
        {
            StringBuilder sb = new StringBuilder("v ", 20);
            // StringBuilder stuff is done this way because it's faster than using the "{0} {1} {2}"etc. format
            // Which is important when you're exporting huge terrains.
            sb.Append(vertexs[i].x.ToString()).Append(" ").
               Append(vertexs[i].y.ToString()).Append(" ").
               Append(vertexs[i].z.ToString());
            sw.WriteLine(sb);
        }

        // Write UVs
        for (int i = 0; i < uvs.Length; i++)
        {
            StringBuilder sb = new StringBuilder("vt ", 22);
            sb.Append(uvs[i].x.ToString()).Append(" ").
               Append(uvs[i].y.ToString());
            sw.WriteLine(sb);
        }

        // Write triangles
        for (int i = 0; i < tris.Length; i += 3)
        {
            StringBuilder sb = new StringBuilder("f ", 43);
            sb.Append(tris[i] + 1).Append("/").Append(tris[i] + 1).Append(" ").
               Append(tris[i + 1] + 1).Append("/").Append(tris[i + 1] + 1).Append(" ").
               Append(tris[i + 2] + 1).Append("/").Append(tris[i + 2] + 1);
            sw.WriteLine(sb);
        }

        //
        sw.Close();
    }

    // 获取目标地形
    public static Terrain GetTargetTerrain()
    {
        Terrain t = null;
        var go = Selection.activeObject as GameObject;
        if (go != null)
        {
            t = go.GetComponent<Terrain>();
        }
        if (!t)
        {
            t = Terrain.activeTerrain;
        }
        if (!t)
        {
            Debug.LogError("select a terrain!");
        }
        return t;
    }

    // 获取地形目录
    public static string GetTerrainMeshPath(TerrainData td)
    {
        var path = PathDefs.ASSETS_PATH_TERRAIN_MESH + td.name + "/";
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        return path;
    }

    // 获取地形模型的名字
    public static string GetTerrainMeshPathName(TerrainData td)
    {
        return GetTerrainMeshPath(td) + "terrain_mesh.obj";
    }
}