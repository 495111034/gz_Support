using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

/// <summary>
/// 内存泄露管理器
/// </summary>
public static class LeakManager
{
    static List<WeakReference> _obj_lists = new List<WeakReference>();
    static int _dirty_count = 0;

    // 添加监视
    [Conditional("ENABLE_PROFILER")]
    public static void AddWatch(object obj)
    {
#if UNITY_EDITOR
        var wr = new WeakReference(obj);
        _obj_lists.Add(wr);
        if (++_dirty_count > 100)
        {
            _dirty_count = 0;
            MiniSize();
        }
#endif
    }


    // 最小化
    [Conditional("ENABLE_PROFILER")]
    static void MiniSize()
    {
        var obj_lists = _obj_lists;
        for (var i = obj_lists.Count - 1; i>= 0; --i) 
        {
            var obj = obj_lists[i];
            if (!obj.IsAlive) 
            {
                obj_lists.swap_tail_and_fast_remove(i);
            }
        }
    }

    //[Conditional("ENABLE_PROFILER")]
    public static int GetCount()
    {
        int count = 0;
        foreach (var obj in _obj_lists)
        {
            if (obj.IsAlive)
            {
                count++;
            }
        }
        return count;
    }

    // 输出信息
    //[Conditional("ENABLE_PROFILER")]
    public static string Dump()
    {
        MiniSize();
        var cnts = new  Dictionary<Type, int>();
        foreach (var obj in _obj_lists)
        {
            if (obj.IsAlive)
            {
                var t = obj.Target.GetType();
                if (cnts.TryGetValue(t, out int cnt))
                {
                    cnts[t] = cnt + 1;
                }
                else
                {
                    cnts[t] = 1;
                }
            }
        }
        StringBuilder sb = new StringBuilder("LeakManager\n");
        foreach (var kv in cnts)
        {
            sb.Append(kv.Key);
            sb.Append(":");
            sb.Append(kv.Value);
            sb.Append("\n");
        }
        return sb.ToString();
    }
}
