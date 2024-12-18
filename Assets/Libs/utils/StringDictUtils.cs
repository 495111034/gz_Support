using StringDict = System.Collections.Generic.Dictionary<string, string>;

/// <summary>
/// 字符串字典工具
/// </summary>
public static class StringDictUtils
{
    public static string GetString(this StringDict dict, string key, string def_value = null)
    {
        string str = null;
        if (dict.TryGetValue(key, out str))
        {
            return str;
        }
        return def_value;
    }

    public static float GetFloat(this StringDict dict, string key, float def_value = 0)
    {
        string str = null;
        if (dict.TryGetValue(key, out str))
        {
            float value;
            if (float.TryParse(str, out value))
            {
                return value;
            }
        }
        return def_value;
    }

    public static int GetInt(this StringDict dict, string key, int def_value = 0)
    {
        string str;
        if (dict.TryGetValue(key, out str))
        {
            int ret;
            if (int.TryParse(str, out ret))
            {
                return ret;
            }
        }
        return def_value;
    }

    public static long GetLong(this StringDict dict, string key, long def_value = 0)
    {
        string str;
        if (dict.TryGetValue(key, out str))
        {
            long ret;
            if (long.TryParse(str, out ret))
            {
                return ret;
            }
        }
        return def_value;
    }

    public static ulong GetULong(this StringDict dict, string key, ulong def_value = 0)
    {
        string str;
        if (dict.TryGetValue(key, out str))
        {
            ulong ret;
            if (ulong.TryParse(str, out ret))
            {
                return ret;
            }
        }
        return def_value;
    }
}
