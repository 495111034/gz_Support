using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public class BuGuaiData
{
    public int sceneResID;
    public string fileName;
    public string sceneName;
    public string triggerName;
    public List<ObjNodeData> objs;
    //public List<Hashtable> triggers;
}

[Serializable]
public class ObjNodeData
{
    [HideInInspector]
    public int id = 0;
    public string gameobjectName = "obj_"; //策划用来看的
    public string title = "";
    public int type = 5;
    public int class_id = 0;
    //public GameObject yuLanGO;
    /// <summary>
    /// 波次id
    /// </summary>
    public int phase_id = 0;
    //public int scene_id = 0;
    public int aoi_group_id = 0;
    public int isTrigger = 0;//是否依赖触发器
    public float x = 0f;
    public float y = 0f;
    public float z = 0f;
    public float direction = 0f;

    /// <summary>
    /// 是否巡逻
    /// </summary>
    public bool isPatrol = false;
    /// <summary>
    /// 巡逻路径
    /// </summary>
    public List<Vector3> posi_patrols;

    /// <summary>
    /// 默认目标, 填aoi_group_id
    /// </summary>
    public int default_target = 0;

    /// <summary>
    /// 阵营
    /// </summary>
    public int zhenying = -1;

    /// <summary>
    /// 随机范围半径，用于出生点和复活点
    /// </summary>
    public float radius;

    /// <summary>
    /// 阻挡是否可通行
    /// </summary>
    public bool isPassable;
    public Vector3 blockScale;

    /// <summary>
    /// 阻挡特效
    /// </summary>
    public string block_effect;
    /// <summary>
    /// 阻挡消失特效
    /// </summary>
    public string block_dis_effect;

    /// <summary>
    /// 垂直方向偏移
    /// </summary>
    public float offset_h;
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append($"{gameobjectName}\t{aoi_group_id}\t{x}\t{y}\t{z}\t{direction}");
        return sb.ToString();
    }
}