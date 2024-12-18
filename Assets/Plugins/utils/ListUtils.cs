using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class ListUtils
{
    public static void swap_tail_and_fast_remove(this System.Collections.IList list, int i)
    {
        var tail = list.Count - 1;
        if (i != tail)
        {
            list[i] = list[tail];
        }
        list.RemoveAt(tail);
    }
    public static T AddT<T>(this List<T> list, T e) 
    {
        list.Add(e);
        return e;
    }

    public static List<T> AddElements<T>(this List<T> list, T e, int num)
    {
        for (int i = 0; i < num; i++)
        {
            list.Add(e);
        }
        return list;
    }
    //
    public static int SafeGetCount<T>(this List<T> list)
    {
        return list != null ? list.Count : 0;
    }

    // 删除重复
    public static void SortAndRemoveDuplicate<T>(this List<T> list) where T : IEquatable<T>
    {
        list.Sort();
        for (int i = 1; i < list.Count; i++)
        {
            if (list[i].Equals(list[i - 1]))
            {
                list.RemoveAt(i);
                --i;
            }
        }
    }

    // List -> Array
    public static T2[] ToArray<T1, T2>(this List<T1> list, Func<T1, T2> conveter)
    {
        T2[] arr = new T2[list.Count];
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = conveter(list[i]);
        }
        return arr;
    }

    // 合并2个列表, 排除重复
    public static void Merge<T>(this List<T> list, List<T> other)
    {
        foreach (var obj in other) if (!list.Contains(obj)) list.Add(obj);
    }
    public static void Merge<T>(this List<T> list, T obj)
    {
        if (!list.Contains(obj)) list.Add(obj);
    }

    // 添加列表, 如果 list=null 则创建
    public static void SafeAdd<T>(this List<T> list, T obj)
    {
        if (list == null) list = new List<T>();
        list.Add(obj);
    }

    //
    public static T SafeGet<T>(this List<T> list, int index)
    {
        if (list == null || index < 0 || index >= list.Count) return default(T);
        return list[index];
    }

    public static List<T> ToMyList<T>(this List<object> from, List<T> to)
    {
        to.Clear();
        for (int i = 0; i < from.Count; ++i)
        {
            T item = (T)(from[i]);
            to.Add(item);
        }
        return to;
    }

    public static T GetAndRemoveAt<T>(this List<T> list, int index)
    {
        var obj = list[index];
        list.Remove(obj);
        return obj;
    }

    //把第一个from替换为to
    public static int ReplaceFirst<T>(this List<T> list, T from, T to) where T : IEquatable<T>
    {
        int pos = -1;
        if (list != null)
        {
            for (int i = 0; i < list.Count; ++i)
            {
                if (list[i].Equals(from))
                {
                    pos = i;
                    break;
                }
            }
            if (pos > 0)
            {
                list[pos] = to;
            }
        }
        return pos;
    }

    // 获取切片
    public static List<T> Slice<T>(this List<T> list, int start, int end)
    {
        var ret = new List<T>();
        for (int i = start; i < end; i++)
        {
            ret.Add(list[i]);
        }
        return ret;
    }
}
