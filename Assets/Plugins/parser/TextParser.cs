using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using StringDict = System.Collections.Generic.Dictionary<string, string>;

/// <summary>
/// 通用文本解析器
/// </summary>
public static class TextParser
{
    // 是否是注释行
    static bool IsCommentLine(string line)
    {
        //return line == "" || line.StartsWith("//") || line.StartsWith("#"); ;
        return line.Length == 0 ||
            line[0] == '#' ||
            line[0] == '/' && line[1] == '/';
    }

    /// <summary>
    /// 解析 URL 字符串
    /// </summary>
    /// <param name="text">可以包含多行, 每行为 url 编码格式, 不支持空格</param>
    public static StringDict ParseUrl(string text, char split, char equal)
    {
        var dict = new StringDict();
        using (StringReader sr = new StringReader(text))
        {
            while (true)
            {
                string line = sr.ReadLine();
                if (line == null) break;
                if (IsCommentLine(line)) continue;
                string[] kvs = line.Split(split);
                foreach (var kv in kvs)
                {
                    int epos = kv.IndexOf(equal);
                    if (epos > 0)
                    {
                        string key = kv.Substring(0, epos);
                        string val = kv.Substring(epos + 1);
                        dict[key] = val;
                    }
                }
            }
        }
        return dict;
    }
    public static StringDict ParseUrl(string text)
    {
        return ParseUrl(text, '&', '=');
    }

    // 编码 URL 
    public static string EncodeUrl(this StringDict dict, char split = '&', char equal = '=')
    {
        StringBuilder sb = new StringBuilder();
        foreach (var kv in dict)
        {
            if (sb.Length > 0) sb.Append(split);
            sb.AppendFormat("{0}{1}{2}", kv.Key, equal, kv.Value);
        }
        return sb.ToString();
    }

    /// <summary>
    /// 解析 INI 字符串, 支持 \r \n \t 转义
    /// </summary>
    public static StringDict ParseIni(string text, StringDict dict = null, char split_char = '=')
    {
        if (dict == null) dict = new StringDict();
        using (StringReader sr = new StringReader(text))
        {
            while (true)
            {
                var line = sr.ReadLine();
                if (line == null) break;
                if (IsCommentLine(line)) continue;
                int index = line.IndexOf(split_char);
                if (index > 0)
                {
                    var key = line.Substring(0, index);
                    var val = line.Substring(index + 1);
                    val = DecodeNewLine(val);
                    dict[key] = val;
                }
            }
            return dict;
        }
    }
    // 解码换行
    public static string DecodeNewLine(string str)
    {
        if (str.IndexOf('\\') >= 0)
        {
            str = str.Replace("\\r", "\r");
            str = str.Replace("\\n", "\n");
            str = str.Replace("\\t", "\t");
        }
        return str;
    }

    // 保存 ini
    public static string SaveIni(StringDict dict)
    {
        List<string> lines = new List<string>();
        foreach (var kv in dict)
        {
            lines.Add(string.Format("{0}={1}", kv.Key, kv.Value.Replace("\n", "\\n").Replace("\r", "\\r")));
        }
        lines.Sort();
        return string.Join("\n", lines.ToArray());
    }

    //移除文件名中的语言标记
    public static string RemoveLanguageID(string filename)
    {
        string value = filename;
        var ll_id = value.IndexOf("--l");

        if (ll_id > 0)
        { 
            var s_id = ll_id + 3;
            var le_id = value.IndexOf('-', s_id, 1);
            if (le_id < 0)
                le_id = value.IndexOf('.', s_id, 1);           
            if (le_id < 0)
                le_id = value.Length - 1;
            value = value.Remove(ll_id, le_id - ll_id + 1);
        }
        return value;
    }

    //移除语言中的品质标记
    public static string RemoveQualityId(string filename)
    {
        string value = filename;
        var ll_id = value.IndexOf("--q");
        if (ll_id > 0)
        {
            var s_id = ll_id + 3;
            var le_id = value.IndexOf('-', s_id, 1);
            if (le_id < 0)
                le_id = value.IndexOf('.', s_id, 1);
            if (le_id < 0)
                le_id = value.Length - 1;
            value = value.Remove(ll_id, le_id - ll_id + 1);
        }
        return value;
    }

    public static string GetLanguageID(string res_name)
    {
        string lang_id = "";
        var ll_id = res_name.IndexOf("--l");
        if (ll_id > 0)
        {
            ll_id = ll_id + 3;

            var le_id = res_name.IndexOf('-', ll_id, 1);
            if (le_id < 0)
                le_id = res_name.IndexOf('.', ll_id, 1);
            if (le_id < 0)
                le_id = res_name.Length - 1;

            lang_id = res_name.Substring(ll_id, le_id - ll_id + 1);
            if (lang_id == "cn")
                return "";
        }
        return lang_id;
    }
}
