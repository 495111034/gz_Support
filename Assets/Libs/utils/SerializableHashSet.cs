/****************************************************************************
*Copyright (c) 2018  All Rights Reserved.
*CLR版本： 4.0.30319.42000
*机器名称：ALI-UCD4E65
*公司名称：阿里巴巴
*命名空间：Assets.scripts.core.utils
*文件名：  SerializableHashSet
*版本号：  V1.0.0.0
*唯一标识：a2502079-08ce-4008-97d2-5c4d83a842f2
*创建人：  mini_coco
*创建时间：2018/8/17 9:41:12
*****************************************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public class SerializableHashSet<Value> : HashSet<Value>, ISerializationCallbackReceiver
{
    [SerializeField]
    private List<Value> _value = new List<Value>();


    public void OnBeforeSerialize()
    {
        _value.Clear();
        _value.Capacity = this.Count;
        foreach (var kvp in this)
        {
            _value.Add(kvp);
        }
    }

    public void OnAfterDeserialize()
    {
        //this.Clear();
        foreach (var v in _value)
        {
            this.Add(v);
        }
    }
}
