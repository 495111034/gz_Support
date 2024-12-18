using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace UnityEngine.UI
{
    /// <summary>
    /// 字符串列表构造器
    /// </summary>
    public class StringListBuilder
    {
        string[] _arr;
        int _count;


        //
        public StringListBuilder(int max_num)
        {
            _arr = new string[max_num];
            _count = 0;

            Reset();
        }

        // 重置
        public void Reset()
        {
            _count = 0;
        }

        public int Count
        {
            get { return _count; }
        }

        public string GetString(int index)
        {
            return _arr[index];
        }

        // 转换全部
        public List<T> ConvertAll<T>(Func<string, T> conv_handler)
        {
            List<T> list = new List<T>(_count);
            for (int i = 0; i < _count; i++)
            {
                T v = conv_handler(_arr[i]);
                if (v != null) list.Add(v);
            }
            return list;
        }

        // 添加字符串
        public void AppendString(string str)
        {
            _arr[_count++] = str;
        }

        // 添加字符
        public void AppendChar(string fmt, char ch)
        {
            var str = string.Format(fmt, StringUtils.EncodeUrl(ch));
            AppendString(str);
        }

        // 添加数字
        public void AppendNumber(string fmt, bool has_plus, int number)
        {
            if (has_plus && number > 0)
            {
                AppendChar(fmt, '+');
            }

            var char_list = number.ToString();
            AppendCharList(fmt, char_list);
        }

        // 添加字符串
        public void AppendCharList(string fmt, string char_list)
        {
            foreach (var ch in char_list)
            {
                AppendChar(fmt, ch);
            }
        }
    }

}