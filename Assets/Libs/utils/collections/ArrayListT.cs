using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// 带数组功能的 List
/// </summary>
public class ArrayListT<T> where T : class
{
    List<T> _list = new List<T>();
    T[] _array;

    // 获取数组
    public T[] Array
    {
        get
        {
            if (_array == null) _array = _list.ToArray();
            return _array;
        }
    }

    // 获取列表
    public List<T> List
    {
        get
        {
            _array = null;
            return _list;
        }
    }

    public T Find(Predicate<T> match)
    {
        return _list.Find(match);
    }

    public int IndexOf(T t)
    {
        return _list.IndexOf(t);
    }

    public T this[int index]
    {
        get
        {
            return _list[index];
        }
    }

    // 列表是否变更
    public bool IsChanged
    {
        get { return _array == null; }
    }

    public int Count
    {
        get { return _list.Count; }
    }

    public void Insert(int index, T item)
    {
        _list.Insert(index, item);
        _array = null;
    }

    public void RemoveAt(int index)
    {
        _list.RemoveAt(index);
        _array = null;
    }

    public void Add(T value)
    {
        _list.Add(value);
        _array = null;
    }

    public bool Remove(T value)
    {
        if (_list.Remove(value))
        {
            _array = null;
            return true;
        }
        return false;
    }

    public void Clear()
    {
        _list.Clear();
        _array = null;
    }

    public bool Contains(T value)
    {
        return _list.Contains(value);
    }
}
