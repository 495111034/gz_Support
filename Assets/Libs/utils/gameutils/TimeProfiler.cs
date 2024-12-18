using System.Diagnostics;
using UnityEngine;

/// <summary>
/// 时间监视
/// 
///     . 支持嵌套调用 
///     
/// </summary>
public static class TimeProfiler
{
    static Stopwatch _sw = new Stopwatch();
    static long[] _time_arr = new long[256];
    static int _time_idx = 0;


    // 重置状态, 避免错误 
    public static void ResetState()
    {
        _sw.Stop();
        _sw.Reset();
        _time_idx = 0;
    }

    // 开始检测
    public static void Begin()
    {
        if (_time_idx == 0)
        {
            _sw.Reset();
            _sw.Start();
        }
        _time_arr[_time_idx++] = _sw.ElapsedMilliseconds;
    }

    // 结束检测
    public static void End(long time_limit, string fmt, params object[] args)
    {
        if (_time_idx <= 0)
        {
            Log.LogError("没有调用 Begin");
            return;
        }

        var time_start = _time_arr[--_time_idx];
        var time_now = _sw.ElapsedMilliseconds;
        if (_time_idx == 0) _sw.Stop();

        var time_used = time_now - time_start;
        if (time_used >= time_limit)
        {
            if(Application.isEditor) Log.LogWarning("TimeCheck, limit:{0}, used:{1}, msg:{2}", time_limit, time_used, string.Format(fmt, args));
        }
    }
}
