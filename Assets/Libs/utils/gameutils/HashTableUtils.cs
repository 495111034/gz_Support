using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;


public static class HashTableUtils
{
    public static TKey[] GetKeysArray2<TKey, TValue>(this IDictionary<TKey, TValue> table)
    {
        if (table == null) return new TKey[0];

        var keys = table.Keys;
        TKey[] keyArray = new TKey[keys.Count];
        keys.CopyTo(keyArray, 0);
        return keyArray;
    }

    public static object[] GetKeysArray(this IDictionary table)
    {
        var keys = table.Keys;
        object[] keyArray = new object[keys.Count];
        keys.CopyTo(keyArray, 0);
        return keyArray;
    }

    /// <summary>
    /// 根据自定义排序，返回key List
    /// </summary>
    /// <param name="sort">排序方法，参数为key</param>
    /// <returns></returns>
    public static List<object> SortReturnKeyList(this Hashtable table, Comparison<object> sort)
    {
        var keyList = table.Keys.Cast<object>().ToList();
        keyList.Sort(sort);
        return keyList;
    }

    /// <summary>
    /// 如果 key 是int 型，此方法会获取失败， 请直接用 [] 获取
    /// </summary>
    /// <param name="ht"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    public static int GetInt(this Hashtable ht, string key)
    {
        try
        {
            return Convert.ToInt32(ht[key]);
        }
        catch (Exception)
        {
            return 0;
        }
    }

    public static long GetLong(this Hashtable ht, string key)
    {
        try
        {
            return Convert.ToInt64(ht[key]);
        }
        catch (Exception)
        {
            return 0;
        }
    }
    public static ulong GetULong(this Hashtable ht, string key)
    {
        try
        {
            return Convert.ToUInt64(ht[key]);
        }
        catch (Exception)
        {
            return 0;
        }
    }

    public static float GetFloat(this Hashtable ht, string key)
    {
        try
        {
            return Convert.ToSingle(ht[key]);
        }
        catch (Exception)
        {
            return 0;
        }
    }
    public static double GetDouble(this Hashtable ht, string key)
    {
        try
        {
            return Convert.ToDouble(ht[key]);
        }
        catch (Exception)
        {
            return 0;
        }
    }

    public static string GetString(this Hashtable ht, string key)
    {
        try
        {
            return ht[key]?.ToString();
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static Hashtable GetHashtable(this Hashtable ht, string key)
    {
        try
        {
            return (Hashtable)ht[key];
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static ArrayList GetArrayList(this Hashtable ht, string key)
    {
        try
        {
            return (ArrayList)ht[key];
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static System.Collections.Generic.List<DictionaryEntry> GetDEList(this Hashtable dt)
    {
        System.Collections.Generic.List<DictionaryEntry> deList = new System.Collections.Generic.List<DictionaryEntry>();

        foreach (DictionaryEntry kv in dt)
        {
            deList.Add(kv);
        }
        return deList;
    }

    public static bool IsEmpty(this Hashtable ht)
    {
        return ht == null || ht.Count == 0;
    }

    // 相加2个 ht, 遍历2个 ht 的第一层元素
    public static void AddOther(this Hashtable ht1, Hashtable ht2)
    {
        if (ht2.IsEmpty()) return;
        foreach (DictionaryEntry e2 in ht2)
        {
            var key = e2.Key;
            var v2 = e2.Value;
            if(v2 is ArrayList || v2 is Hashtable)
            {
                continue;
            }
            var v1 = ht1[key];

            if (v1 == null) v1 = v2;
            else v1 = MathUtils.Sum(v1, v2);

            ht1[key] = v1;
        }
    }

    //
    public static List<DictionaryEntry> SortedItems(this Hashtable ht, Comparison<DictionaryEntry> fn = null)
    {
        var list = new List<DictionaryEntry>();
        foreach (DictionaryEntry de in ht)
        {
            list.Add(de);
        }
        if (fn == null) fn = SortAsString;
        list.Sort(fn);
        return list;
    }

    public static int SortAsString(DictionaryEntry a, DictionaryEntry b)
    {
        return string.Compare(a.Key.ToString(), b.Key.ToString());
    }

    public static Hashtable CopyHtForFixValue(Hashtable ht, object fix_value)
    {
        var new_ht = new Hashtable();
        foreach (DictionaryEntry de in ht)
            new_ht[de.Key] = fix_value;
        return new_ht;
    }
}

