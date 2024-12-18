using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using StringKeyValue = System.Collections.Generic.KeyValuePair<string, string>;


/// <summary>
/// 字符串工具
/// </summary>
public static class StringUtils
{

    private static StringBuilder m_StringBuilder = new StringBuilder(512);

    public static string ConcatString<T1,T2>(T1 s1, T2 s2)
    {
        m_StringBuilder.Length = 0;
        m_StringBuilder.Append(s1);
        m_StringBuilder.Append(s2);
        return m_StringBuilder.ToString();
    }
    /// <summary>
    /// 解析 "key=value" 数组
    /// </summary>
    public static List<StringKeyValue> ParseKeyValuePairs(string str)
    {
        List<StringKeyValue> list = new List<StringKeyValue>();
        foreach (var item in str.Split(' '))
        {
            int idx = item.IndexOf('=');
            if (idx > 0)
            {
                var key = item.Substring(0, idx).Trim();
                var val = item.Substring(idx + 1).Trim();
                if (key.Length > 0 && val.Length > 0)
                {
                    var kv = new StringKeyValue(key, val);
                    list.Add(kv);
                }
            }
        }
        return list;
    }

    public static string CharArray2String(char[] array, int start, int len)
    {
        char[] _strb = new char[len];
        Array.Copy(array, start, _strb, 0, len);
        var str = string.Join("", _strb);
        _strb = null;
        return str;
    }

    //
    public static bool StartsWith(string str, params string[] prefixs)
    {
        foreach (var prefix in prefixs)
        {
            if (str.StartsWith(prefix)) return true;
        }
        return false;
    }


    /// <summary>
    /// 把字符串用正则表达式拆分, 返回 字符串/模式 交合的数组
    /// </summary>
    public static List<string> SplitWithPattern(string source, string pattern)
    {
        List<string> list = new List<string>();

        int len = source.Length;
        int last = 0;
        var mc = Regex.Matches(source, pattern);

        foreach (Match m in mc)
        {
            if (m.Index > last)
            {
                list.Add(source.Substring(last, m.Index - last));
            }
            list.Add(m.Value);
            last = m.Index + m.Length;
        }
        if (len > last)
        {
            list.Add(source.Substring(last, len - last));
        }
        return list;
    }

    // 合并成字符串
    //public static string FormatArray<T>(T[] arr)
    //{
    //    if (arr == null) return "";
    //    int index = 0;
    //    string result = "";
    //    foreach (T t in arr)
    //    {
    //        result += t.ToString();
    //        if (index != arr.Length - 1)
    //            result += ", ";
    //        index++;
    //    }
    //    return result;
    //    //return string.Join(", ", Array.ConvertAll<T, string>(arr, (x) => { return x.ToString(); }));
    //}

    public static string FormatArray(float[] arr)
    {
        if (arr == null) return "";
        int index = 0;
        string result = "";
        foreach (float t in arr)
        {
            result += t.ToString();
            if (index != arr.Length - 1)
                result += ", ";
            index++;
        }
        return result;
    }

    public static string FormatArray(int[] arr)
    {
        if (arr == null) return "";
        int index = 0;
        string result = "";
        foreach (int t in arr)
        {
            result += t.ToString();
            if (index != arr.Length - 1)
                result += ", ";
            index++;
        }
        return result;
    }


    //格式化列表
    public static string FormatList<T>(List<T> arr)
    {
        if (arr == null) return "";
        int index = 0;
        string result = "";
        foreach (T t in arr)
        {
            result += t.ToString();
            if (index != arr.Count - 1)
                result += ",";
            index++;
        }
        return result;
    }

    // 接取 prefix - tail 之间的内容
    public static string Slice(string src, string prefix, string tail)
    {
        var a = src.IndexOf(prefix);
        var b = src.IndexOf(tail);

        a = a + prefix.Length;
        return src.Substring(a, b - a);
    }

    // 解析参数, 用空格分开, 如果个数为0, 则返回 null
    public static string[] SplitArgs(string cmd, char ch)
    {
        List<string> list = new List<string>();
        foreach (var str in cmd.Split(ch))
        {
            var str2 = str.Trim();
            if (!string.IsNullOrEmpty(str2)) list.Add(str2);
        }
        return list.Count > 0 ? list.ToArray() : null;
    }
    public static string[] SplitArgs(string cmd)
    {
        return SplitArgs(cmd, ' ');
    }


    public static int[] SplitAsNums(string str, char ch) 
    {
        var arr = str.Split(ch);
        var nums = new int[arr.Length];
        for (var i = 0; i < arr.Length; ++i) 
        {
            nums[i] = int.Parse( arr[i] );
        }
        return nums;
    }

    // 把 List 转成多行文本
    public static string ToLines(this List<string> list)
    {
        return string.Join("\r\n", list.ToArray());
    }
    public static string GetLastLines(this List<string> list, string filter, int max_line)
    {
        // 获取过滤器
        var has_filter = !string.IsNullOrEmpty(filter);
        string[] filters = null;
        if (has_filter)
        {
            if (filter.Contains(' '))
            {
                filters = SplitArgs(filter);
            }
        }

        // 过滤行
        var count = list.Count;
        List<string> list2 = new List<string>();
        for (int i = 0; i < count; i++)
        {
            var line = list[i];
            if (string.IsNullOrEmpty(line)) continue;

            var can_add = false;
            if (!has_filter) can_add = true;
            else if (filters != null)
            {
                foreach (var f in filters) if (line.Contains(f)) { can_add = true; break; }
            }
            else if (line.Contains(filter)) can_add = true;

            if (can_add)
            {
                list2.Add(line);
            }
        }

        // 删除多余行
        while (list2.Count > max_line)
        {
            list2.RemoveAt(0);
        }

        // 合并
        return list2.ToLines();
    }

    // 限制长度
    public static string Limit(this string str, int max_chars)
    {
        if (str != null && str.Length > max_chars)
        {
            str = str.Substring(0, max_chars);
        }
        return str;
    }

    // 拷贝到缓冲区, 如果缓冲区太小则自动增长
    public static void CopyToBuff(this string str, ref char[] buff, bool append_end)
    {
        var len = str.Length;
        if (append_end) len++;
        if (buff == null || buff.Length < len) buff = new char[len];
        str.CopyTo(0, buff, 0, str.Length);
        if (append_end) buff[str.Length] = '\0';
    }

    /// <summary>
    /// 字符串中是否有中文
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static bool HasChinese(this string str)
    {
        // return Regex.IsMatch(str, @"[\u4e00-\u9fa5]");
        return (Regex.IsMatch(str, @"[\u4e00-\u9fa5]")) || (str.Length != Encoding.Default.GetByteCount(str)) ;
    }

    // url 编码
    public static string EncodeUrl(char ch)
    {
        if (char.IsLetterOrDigit(ch)) return "" + ch;

        // URL 编码表参考: http://www.w3school.com.cn/tags/html_ref_urlencode.html
        // 常用:
        //      +       %2b
        //      -       %2d
        //      /       %2f
        //      :       %3a
        return "%" + ((int)ch).ToString("x2");
    }
    public static string EncodeUrl(string str)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var ch in str)
        {
            sb.Append(EncodeUrl(ch));
        }
        return sb.ToString();
    }

    public static T GetLast<T>(this List<T> list)
    {
        if (list == null) return default(T);
        var count = list.Count;
        if (count == 0) return default(T);
        return list[count - 1];
    }

    public static T GetFirst<T>(this List<T> list)
    {
        if (list == null) return default(T);
        if (list.Count == 0) return default(T);
        return list[0];
    }

    // 获取插值, 允许 rate 超出 [0,1] 范围, 超出时根据 extend 决定是否估算
    public static float EvaluateEx(this AnimationCurve curve, float rate, bool extend)
    {
        if (rate < 0)
        {
            var v0 = curve.Evaluate(0);
            if (!extend) return v0;

            var vx = curve.Evaluate(0.1f);
            return v0 + (vx - v0) / 0.1f * rate;
        }
        if (rate > 1)
        {
            var v1 = curve.Evaluate(1);
            if (!extend) return v1;

            var vx = curve.Evaluate(0.9f);
            return v1 + (v1 - vx) / 0.1f * (rate - 1);
        }
        return curve.Evaluate(rate);
    }

    public static string ToSBC(string input)//single byte charactor
    {
        char[] c = input.ToCharArray();
        for (int i = 0; i < c.Length; i++)
        {
            if (c[i] == 12288)//全角空格为12288，半角空格为32
            {
                c[i] = (char)32;
                continue;
            }
            if (c[i] > 65280 && c[i] < 65375)//其他字符半角(33-126)与全角(65281-65374)的对应关系是：均相差65248
                c[i] = (char)(c[i] - 65248);
        }
        return new string(c);
    }
    public static  string MyReplace(string text)
    {
        const string s1 = "。；，？！、“”‘’";
        const string s2 = @".;,?!\""""''";
        char[] c = text.ToCharArray();
        for (int i = 0; i < c.Length; i++)
        {
            int n = s1.IndexOf(c[i]);
            if (n != -1) c[i] = s2[n];
        }
        return new string(c);
    }

    /// <summary>
    /// 从字符串中获取数字
    /// </summary>
    /// <param name="text"></param>
    /// <returns>返回剔除到非数字的字符，小数点也会被剔除</returns>
    public static string GetNumberFromString(this string text)
    {
        return Regex.Replace(text, @"[^\d.\d]", "");
    }

    public static bool IsInt(this string value)
    {
        return Regex.IsMatch(value, @"^\-?\d+$");
    }

    public static bool IsBoolean(this string value)
    {
        return value.Contains("true")|value.Contains("false")?true:false;
    }

    public static bool IsNumber(this string value)
    {
        return Regex.IsMatch(value, @"^[+-]?\d*[.]?\d*$");
    }

    public static bool IsVector3(this string value)
    {
        string number = @"[+-]?\d*[.]?\d*";
        return Regex.IsMatch(value, $"^{number}\\|{number}\\|{number}$");
    }
    public static bool IsVector2(this string value)
    {
        string number = @"[+-]?\d*[.]?\d*";
        return Regex.IsMatch(value, $"^{number}\\|{number}$");
    }

    //比 string.EndsWith 快很多
    public static bool EndsWithEx(this string str, string value)
    {
        if (object.ReferenceEquals(str, value))
        {
            return true;
        }

        if (value == null) 
        {
            return false;
        }

        if (value.Length == 0) 
        {
            return true;
        }

        var len = value.Length;
        var offset = str.Length - len;
        if (offset < 0)
        {
            return false;
        }

        for (var i = 0; i < len; ++i)
        {
            if (str[i + offset] != value[i])
            {
                return false;
            }
        }

        return true;
    }
    public static string md5(string str)
    {
        return md5(System.Text.Encoding.UTF8.GetBytes(str));
    }

    public static string md5_len(byte[] bytes, int offset, int count)
    {
        var md5_str = "";
        var md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        md5.ComputeHash(bytes, offset, count);
        foreach (var b in md5.Hash)
        {
            md5_str += b.ToString("X2");
        }
        md5.Clear();
        md5.Dispose();
        return md5_str;
    }
    public static string md5(byte[] bytes) 
    {
        return md5_len(bytes, 0, bytes.Length);
    }
}