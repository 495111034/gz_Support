
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// 排序工具
/// </summary>
public static class SortUtils
{
    /// <summary>
    ///  插入排序(稳定的)
    /// </summary>
    public static IList<T> InsertionSort<T>(this IList<T> list, Comparison<T> comparison)
    {
        if (list == null) throw new ArgumentNullException("list");
        if (comparison == null) throw new ArgumentNullException("comparison");

        var Count = list.Count;
        for (int j = 1; j < Count; j++)
        {
            T key = list[j];
            int i = j - 1;
            for (; i >= 0 && comparison(list[i], key) > 0; i--)
            {
                list[i + 1] = list[i];
            }
            list[i + 1] = key;
        }
        return list;
    }
}

