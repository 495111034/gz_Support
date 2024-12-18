using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// AlignEditor用于UI编辑的批量对齐。
/// </summary>
public class AlignEditor : EditorWindow
{
    public string alignX = "0";
    public string alignY = "0";
    public string alignZ = "0";

    [MenuItem("Window/AlignEditor")]
    public static void Init()
    {
        // Init Editor Window
        var alignEditor = EditorWindow.GetWindow(typeof(AlignEditor));
        alignEditor.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("Distance Align Options");
        alignX = EditorGUILayout.TextField("X", alignX);
        alignY = EditorGUILayout.TextField("Y", alignY);
        alignZ = EditorGUILayout.TextField("Z", alignZ);
        if (GUILayout.Button("Align正向"))
        {
            DoAlign(true);
        }
        if (GUILayout.Button("Align反向"))
        {
            DoAlign(false);
        }
        GUILayout.Label("Other Align");

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Left/Right Align"))
        {
            this.PositionSelectionObjects(AlignType.LeftAlign);
        }

        if (GUILayout.Button("Top/Bottom Align"))
        {
            this.PositionSelectionObjects(AlignType.TopAlign);
        }

        GUILayout.EndHorizontal();
    }

    void DoAlign(bool is_add)
    {
        GameObject[] gameObjects = this.getSortedGameObjects();

        Vector3 firstObjectVec = Vector3.zero;
        for (int i = 0; i < gameObjects.Length; i++)
        {
            if (i == 0)
            {
                firstObjectVec = gameObjects[i].transform.localPosition;
                continue;
            }

            float x = 0;
            if (is_add)
            {
                x = firstObjectVec.x + Convert.ToSingle(alignX) * i;
            }
            else
            {
                x = firstObjectVec.x - Convert.ToSingle(alignX) * i;
            }

            gameObjects[i].transform.localPosition = new Vector3(
                x,
                firstObjectVec.y + -Convert.ToSingle(alignY) * i,
                firstObjectVec.z + Convert.ToSingle(alignZ) * i);
        }
    }

    private enum AlignType
    {
        TopAlign,
        LeftAlign,
        RightAlign,
        BottomAlign
    }


    private int CompareGameObjectsByName(GameObject a, GameObject b)
    {
        return a.name.CompareTo(b.name);
    }


    private GameObject[] getSortedGameObjects()
    {
        List<GameObject> gameObjects = new List<GameObject>(Selection.gameObjects);

        gameObjects.Sort(this.CompareGameObjectsByName);
        Debug.Log("sort:" + string.Join(",", gameObjects));
        return gameObjects.ToArray();
    }

    private void PositionSelectionObjects(AlignType alignType)
    {
        GameObject[] gameObjects = this.getSortedGameObjects();


        if (gameObjects.Length > 0)
        {
            if (alignType == AlignType.TopAlign)
            {
                float firstY = gameObjects[0].transform.localPosition.y;
                for (int i = 0; i < gameObjects.Length; i++)
                {
                    var obj = gameObjects[i];
                    float selfX = obj.transform.localPosition.x;
                    float selfZ = obj.transform.localPosition.z;

                    obj.transform.localPosition = new Vector3(selfX, firstY, selfZ);
                }
            }
            else if (alignType == AlignType.LeftAlign)
            {
                float fisrtX = gameObjects[0].transform.localPosition.x;

                for (int i = 0; i < gameObjects.Length; i++)
                {
                    var obj = gameObjects[i];
                    float selfY = obj.transform.localPosition.y;
                    float selfZ = obj.transform.localPosition.z;

                    obj.transform.localPosition = new Vector3(fisrtX, selfY, selfZ);
                }
            }
        }
    }

    [MenuItem("GameObject/Copy Path #C", false, 2)]
    public static void CopyPath()
    {
        if (Selection.gameObjects == null) return;
        string path = "";
        for (int i = 0; i < Selection.gameObjects.Length; i++)
        {
            GameObject t = Selection.gameObjects[i];
            if (t != null)
            {
                path += GetPath(t.transform) + "\n";
            }
        }
        TextEditor editor = new TextEditor();
        editor.content = new GUIContent(path);
        editor.SelectAll();
        editor.Copy();
    }

    // 获取路径, 用于调试
    public static string GetPath(Transform t)
    {
        List<string> list = new List<string>();
        while (t != null)
        {
            list.Add(t.name);
            t = t.parent;
        }

        list.Reverse();
        return string.Join("/", list.ToArray());
    }
    
    [MenuItem("Editor/Open PersistentDataPath", false, 2)]
    public static void OpenPersistentDataPath()
    {
        Process.Start(Application.persistentDataPath);
    }
}