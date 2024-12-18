/****************************************************************************
*Copyright (c) 2018  All Rights Reserved.
*CLR版本： 4.0.30319.42000
*机器名称：ALI-UCD4E65
*公司名称：阿里巴巴
*命名空间：Assets.scripts.core.utils
*文件名：  SerializableDictionary
*版本号：  V1.0.0.0
*唯一标识：e0af1134-3c19-4ced-811b-ef6a8f852fc9
*创建人：  mini_coco
*创建时间：2018/8/2 10:50:22
*****************************************************************************/


using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 可序列化字典
/// 
/// 用法:
///      [System.Serializable]
///      public class MyDictionary : SerializableDictionary<int, GameObject> { }
///      public MyDictionary dic;
/// </summary>
/// <typeparam name="TKey"></typeparam>
/// <typeparam name="TValue"></typeparam>
[System.Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    // We save the keys and values in two lists because Unity does understand those.
    [SerializeField]
    private List<TKey> _keys;
    [SerializeField]
    private List<TValue> _values;

    // Before the serialization we fill these lists
    public void OnBeforeSerialize()
    {
        
    }

    // After the serialization we create the dictionary from the two lists
    public void OnAfterDeserialize()
    {
        this.Clear();
        int count = Mathf.Min(_keys.Count, _values.Count);
        for (int i = 0; i < count; ++i)
        {
            this.Add(_keys[i], _values[i]);
        }
    }
}
