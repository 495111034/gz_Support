using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// 带数组功能的 Dictionary
/// </summary>
public class ArrayDictionaryT<K, V> 
{
    Dictionary<K, V> _dict = null;
    List<K> _keys = null;
    List<V> _values = null;


    public ArrayDictionaryT()
    {
        _dict = new Dictionary<K, V>();
    }
    public ArrayDictionaryT(Dictionary<K, V> dict)
    {
        _dict = new Dictionary<K, V>(dict);
    }
    //
    public void Add(K key, V value)
    {
        _resetArrays();
        _dict.Add(key, value);
    }

    public bool Remove(K key)
    {
        if (_dict.Remove(key))
        {
            _resetArrays();
            return true;
        }
        return false;
    }

    public void Clear()
    {
        _resetArrays();
        _dict.Clear();
    }

    public void Release()
    {
        Clear();
    }

    void _resetArrays()
    {
        if (_keys != null)
        {
            _keys.Clear();
            _keys = null;
        }
        if (_values != null)
        {
            _values.Clear();
            _values = null;
        }
    }

    public List<K> Keys
    {
        get
        {
            if (_keys == null) _keys = _dict.Keys.ToList();
            return _keys;
        }
    }

    public List<V> Values
    {
        get
        {
            if (_values == null) _values = _dict.Values.ToList();
            return _values;
        }
    }

    #region 只读访问

    public bool TryGetValue(K key, out V value)
    {
        return _dict.TryGetValue(key, out value);
    }

    public bool ContainsKey(K key)
    {
        return _dict.ContainsKey(key);
    }

    public V this[K key]
    {
        get
        {
            return _dict[key];
        }
        set
        {
            _dict[key] = value;
            _resetArrays();
        }
    }

    public int Count
    {
        get { return _dict.Count; }
    }

    #endregion

}