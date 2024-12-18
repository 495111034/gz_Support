using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


/// <summary>
/// 单个对象信息
/// </summary>
public class AoiObjBehaviour : MonoBehaviour
{
    public int id = 0;
    public int type = 0;
    public int class_id = 0;
    public int phase_id = 0;
    public Vector2[] patro_ways; // 巡逻路径 {{x,y},{x,y},{x,y}}  type:ArrayList

    //
    public string GetName()
    {
        //return string.Format("i{0}_t{1}_c{2}_p{3}", id, type, class_id, phase_id);
        return string.Format("{0}_{1}_{2}_{3}", id, type, class_id, phase_id);
    }
}
