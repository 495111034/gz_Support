using System.Collections.Generic;


/// <summary>
/// 数组缓存
/// </summary>
public class ArrayCache<T> where T : class, new()
{
    T[] _arr;
    List<T> _free_list;
    List<T> _used_list;

    // 创建
    public ArrayCache(int max_unit)
    {
        _arr = new T[max_unit];
        _free_list = new List<T>();
        _used_list = new List<T>();

        for (int i = 0; i < max_unit; i++)
        {
            _arr[i] = new T();
        }

        Reset();
    }

    public void Reset()
    {
        _free_list.Clear();
        _free_list.AddRange(_arr);

        _used_list.Clear();
    }

    public T Alloc()
    {
        T e = null;
        if (_free_list.Count > 0)
        {
            e = _free_list[0];
            _free_list.Remove(e);
            _used_list.Add(e);
        }
        return e;
    }

    public T[] Alloc(int count)
    {
        T[] arr = null;
        if (_free_list.Count >= count)
        {
            arr = new T[count];
            for (int i = 0; i < count; i++)
            {
                arr[i] = _free_list[i];
            }
            _free_list.RemoveRange(0, count);
            _used_list.AddRange(arr);
        }
        return arr;
    }

    public void Free(T e)
    {
        if (_used_list.Remove(e))
        {
            _free_list.Add(e);
        }
    }

    public void Free(T[] arr)
    {
        foreach (var e in arr)
        {
            if (_used_list.Remove(e))
            {
                _free_list.Add(e);
            }
        }
    }

    public List<T> GetUsedList()
    {
        return _used_list;
    }
}
