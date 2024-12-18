using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// 
public interface ISafeDictionary
{
    void Add(object key, object value);
    void Clear();
}

/// <summary>
/// 安全字典, key 不存在时不抛异常, 输出错误信息 
/// </summary>
public class SafeDictionary<K, V> : ISafeDictionary
    where V : class
{
    Dictionary<K, V> _dict = new Dictionary<K, V>();

    // 获取
    public V this[K key]
    {
        get
        {
            if (!_dict.ContainsKey(key))
            {
                Log.LogError("Key not exist, key:{0}, type:{1}", key, typeof(V).Name);
                return null;
            }
            return _dict[key];
        }
        set
        {
            _dict[key] = value;
        }
    }

    // 是否存在
    public bool HasKey(K key)
    {
        return _dict.ContainsKey(key);
    }

    // 添加
    public void Add(object tkey, object value)
    {
        var key = (K)Convert.ChangeType(tkey, typeof(K));
        if (_dict.ContainsKey(key))
        {
            Log.LogError("Key aleardy exist, key:{0}, type:{1}, value:{2}", key, typeof(V).Name, value);
            return;
        }
        _dict.Add(key, (V)value);
    }

    //
    public void Clear()
    {
        _dict.Clear();
    }
}
