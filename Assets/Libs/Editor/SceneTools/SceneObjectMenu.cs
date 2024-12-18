using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;
using System.IO;

static internal class SceneObjectMenu
{
    [MenuItem("GameObject/SceneTools/替换fbx模型为独立mesh文件", false, 1)]
    static public void ReplaceFBXMesh(MenuCommand menuCommand)
    {
        var tmpMeshs = (menuCommand.context as GameObject).GetComponentsEx<MeshFilter>();
        for (int i = 0; i < tmpMeshs.Count; ++i)
        {
            var mesh = tmpMeshs[i].sharedMesh;
            var curFile = AssetDatabase.GetAssetPath(mesh);

            if (!string.IsNullOrEmpty(curFile) && Path.GetExtension(curFile).ToLower() == ".fbx")
            {
                var meshPath = Path.GetDirectoryName(curFile);
                var meshDataFile = meshPath + "/" + tmpMeshs[i].sharedMesh.name.ToLower() + ".asset";
                if (File.Exists(meshDataFile))
                {
                    tmpMeshs[i].sharedMesh = AssetDatabase.LoadAssetAtPath(meshDataFile, typeof(Mesh)) as Mesh;
                }
            }

        }
        AssetDatabase.SaveAssets();
    }

    [MenuItem("Scenes/检查合批，场景对象缩放是否为负", false, 1)]
    static public void CheckObjNegative()
    {
        var gos = GameObject.FindObjectsOfType<GameObject>();
        foreach (var go in gos)
        {
            if (go.transform.localScale.x < 0 || go.transform.localScale.y < 0 || go.transform.localScale.z < 0)
            {
                Log.LogError($"{go.name} scale is 负的");
            }
        }
    }
}

public class MoveObjectOffsetSet : ScriptableWizard
{
    public Vector3 offset;

    static protected GameObject currentParent = null;

    void OnWizardUpdate()
    {
        helpString = "请输入要调整的偏移量";
        isValid = offset != Vector3.zero;
    }

    void OnWizardCreate()
    {
        if (!currentParent) return;
        for(int i = 0; i < currentParent.transform.childCount; ++i)
        {
            var child = currentParent.transform.GetChild(i);
            child.localPosition = child.localPosition + offset;
        }
        currentParent = null;
    }

    [MenuItem("GameObject/SceneTools/移动所有孩子指定偏移量", false, 999)]
    static public void MoveAllChildOffset(MenuCommand menuCommand)
    {
        currentParent = null;
        if (!(menuCommand.context is GameObject)) return;
        if ((menuCommand.context as GameObject).transform.position != Vector3.zero) return;

        currentParent = menuCommand.context as GameObject;
        ScriptableWizard.DisplayWizard<MoveObjectOffsetSet>(
            "移动孩子", "移动");
    }
}

