using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// 最大个数的列表，带线程保护
/// </summary>
/// <typeparam name="T"></typeparam>
public class MaxSizeList<T>
{
    int _total_count;
    int _max_count;
    ConcurrentQueue<T> _list;

    //
    public MaxSizeList(int max_count)
    {
        _max_count = max_count;
        _list = new ConcurrentQueue<T>();
    }

    public int MaxCount
    {
        get { return _max_count; }
        set
        {
            if (_max_count != value)
            {
                _max_count = value;
                RemoveExcess();
            }
        }
    }

    void RemoveExcess()
    {
        while (_list.Count - _max_count > 0)
        {
            T item;
            _list.TryDequeue(out item);
        }
    }

    public int TotalCount
    {
        get { return _total_count; }
    }

    public int CurCount
    {
        get { return _list.Count; }
    }

    public List<T> List
    {
        get { return _list.ToList(); }
    }

    public void Add(T value)
    {
        _list.Enqueue(value);
        ++_total_count;
        RemoveExcess();
    }

    public void Clear()
    {
        _list.Clear();
    }

}
