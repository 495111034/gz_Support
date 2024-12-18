using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IniDict = System.Collections.Generic.Dictionary<string, string>;

public static class IniDictUtils
{
    public static string[] GetStrings(this IniDict dict, string key, int min_size)
    {
        string str;
        if (!dict.TryGetValue(key, out str)) return null;

        var arr = str.Split(',');

        if (arr.Length < min_size)
        {
            var arr2 = new string[min_size];
            Array.Copy(arr, arr2, arr.Length);
            arr = arr2;
        }

        return arr;
    }

    public static void SetStrings(this IniDict dict, string key, string[] values)
    {
        dict[key] = string.Join(",", values);
    }

    public static T[] GetValues<T>(this IniDict dict, string key, int min_size)
    {
        var arr = GetStrings(dict, key, min_size);
        if (arr == null) return null;

        T[] ret = new T[arr.Length];
        for (int i = 0; i < arr.Length; i++)
        {
            var str = arr[i];
            if (string.IsNullOrEmpty(str))
                ret[i] = default(T);
            else
                ret[i] = (T)Convert.ChangeType(str, typeof(T));
        }
        return ret;
    }

    //public static void SetValues(this IniDict dict, string key, int index, params object[] values)
    //{
    //    var min_size = index + values.Length;
    //    var arr = GetStrings(dict, key, min_size);

    //    if (arr == null)
    //    {
    //        arr = new string[min_size];
    //    }
    //    for (int i = 0; i < values.Length; i++)
    //    {
    //        arr[index + i] = values[i].ToString();
    //    }

    //    SetStrings(dict, key, arr);
    //}
}
