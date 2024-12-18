using System;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// 缓存字典
///		按 LRU 算法管理固定数量上限的元素, 多余的内容会被直接释放
///		该类用作代替 Dictionary 类的功能
///			
/// </summary>
public class CachedDictionary<Key, Value> where Value : class
{
    public event Action<Value> OnRemoveEvent;		// 当对象过期/并被删除时调用

    //
    List<Key> _list = new List<Key>();
    Dictionary<Key, Value> _dict = new Dictionary<Key, Value>();
    int _max_count;


    // 初始化 cache
    public CachedDictionary(int max_count)
    {
        this._max_count = max_count;
    }

    public int MaxCount { get { return _max_count; } }
    public int Count { get { return _list.Count; } }

    // 获取所有的 key
    public Key[] GetAllKeys()
    {
        return _list.ToArray();
    }

    // 获取所有的 value
    public Value[] GetAllValues()
    {
        Value[] arr = new Value[_dict.Count];
        var idx = 0;
        foreach (var kv in _dict)
        {
            arr[idx++] = kv.Value;
        }
        return arr;
    }

    // 获取一个值, 会修改 LRU 优先级
    public bool TryGetValue(Key key, out Value value)
    {
        value = null;
        if (key == null)
        {
            Log.LogError($"TryGetValue,thie key is null");
            return false;
        }
        if (_dict.ContainsKey(key))
        {
            if (_dict.TryGetValue(key, out value))
            {
                Use(key);
                return true;
            }
        }
       
        value = default(Value);
        return false;
    }

    // 获取
    public Value Get(Key key)
    {
        Value v;
        TryGetValue(key, out v);
        return v;
    }

    // 使用一个 key, 修改上次使用时间
    public void Use(Key key)
    {
        if (_list.Remove(key))
        {
            _list.Insert(0, key);
        }
    }

    // 添加一个值
    public void Add(Key key, Value value)
    {
        if(value == null)
        {
            Log.LogError($"key:{key}; value == null");
            return;
        }
        if (!_dict.ContainsKey(key))
        {
            _dict.Add(key, value);
            _list.Insert(0, key);

            // 删除多余
            if (_list.Count > _max_count)
            {
                key = _list[_list.Count - 1];
                Remove(key);
            }
        }
    }

    // 是否包含
    public bool ContainsKey(Key key)
    {
        return _dict.ContainsKey(key);
    }

    // 删除对象
    public void Remove(Key key)
    {
        Value value;
        if (_dict.TryGetValue(key, out value))
        {
            _list.Remove(key);
            _dict.Remove(key);
            if (OnRemoveEvent != null) OnRemoveEvent(value);
        }
    }

    // 清空缓存
    public void Clear()
    {
        if (OnRemoveEvent != null)
        {
            foreach (var v in _dict.Values)
            {
                OnRemoveEvent(v);
            }
        }

        //
        _list.Clear();
        _dict.Clear();
    }
}

