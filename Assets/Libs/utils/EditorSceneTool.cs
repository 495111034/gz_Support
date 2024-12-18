/****************************************************************************
*Copyright (c) 2018  All Rights Reserved.
*CLR版本： 4.0.30319.42000
*机器名称：ALI-UCD4E65
*公司名称：阿里巴巴
*命名空间：Assets.Plugins.Trigger
*文件名：  ProjectSceneManager
*版本号：  V1.0.0.0
*唯一标识：084317e1-e709-4bf8-8152-f6e3baf75ca7
*创建人：  mini_coco
*创建时间：2018/8/7 11:20:19
*****************************************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class EditorSceneTool
{
    public static void CleanHierarchy()
    {
        GameObject g = GameObject.Find("t_tmp");
        if (g != null) GameObject.DestroyImmediate(g);
    }

    public static void AddGameObject(GameObject p_gameObject)
    {
        GameObject g = GameObject.Find("t_tmp");
        if (g == null)
        {
            g = new GameObject();
            g.name = "t_tmp";
        }
        p_gameObject.transform.SetParent(g.transform,false);
    }

    public static void DestoryGameObject(GameObject p_GameObject)
    {
        if (p_GameObject != null) GameObject.DestroyImmediate(p_GameObject);
    }
}
