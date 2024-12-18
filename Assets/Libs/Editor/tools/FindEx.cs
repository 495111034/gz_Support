using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// 扩展选中右键菜单
/// </summary>
[ExecuteInEditMode]
public class FindEx
{
    private const string find_menu = "GameObject/Find/";

    [MenuItem(find_menu + "打印选中节点下所有的MyText", false, 2)]
    public static void FindAllText()
    {
        GameObject go = Selection.gameObjects[0];
        if (go != null)
        {
            var children = go.GetComponentsInChildren<MyText>(false);
            Log.LogInfo($"{go.name}下所有Text{children.Length}个");
        }
        else
        {
            Log.LogInfo("请先选中Gameobject");
        }
    }

    [MenuItem(find_menu + "打印选中节点下所有active的MyText", false, 2)]
    public static void FindAllText2()
    {
        GameObject go = Selection.gameObjects[0];
        if (go != null)
        {
            var children = go.GetComponentsInChildren<MyText>(true);
            var count = 0;
            foreach (var child in children)
            {
                if (child.gameObject.activeInHierarchy) count++;
            }

            Log.LogInfo($"{go.name}下所有active=true的Text{count}个");
        }
        else
        {
            Log.LogInfo("请先选中Gameobject");
        }
    }

    [MenuItem(find_menu + "打印选中节点下所有MySpriteImage", false, 2)]
    public static void FindAllMySpriteImage()
    {
        GameObject go = Selection.gameObjects[0];
        if (go != null)
        {
            var children = go.GetComponentsInChildren<MySpriteImage>();
            Log.LogInfo($"{go.name}下所有MySpriteImage{children.Length}个");
        }
        else
        {
            Log.LogInfo("请先选中Gameobject");
        }
    }


    [MenuItem(find_menu + "打印选中节点下所有Gameobject", false, 2)]
    public static void FindAllGo()
    {
        GameObject go = Selection.gameObjects[0];
        if (go != null)
        {
            var children = go.GetComponentsInChildren<Transform>(false);
            Log.LogInfo($"{go.name}下所有的gameobject{children.Length}个");
        }
        else
        {
            Log.LogInfo("请先选中Gameobject");
        }
    }


    [MenuItem(find_menu + "打印选中节点下所有active的Gameobject", false, 2)]
    public static void FindAllGo2()
    {
        GameObject go = Selection.gameObjects[0];
        if (go != null)
        {
            var children = go.GetComponentsInChildren<Transform>(true);
            var count = 0;
            foreach (var child in children)
            {
                if (child.gameObject.activeInHierarchy) count++;
            }

            Log.LogInfo($"{go.name}下所有active=true的gameobject{count}个");
        }
        else
        {
            Log.LogInfo("请先选中Gameobject");
        }
    }
}