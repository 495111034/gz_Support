
using System;
using UnityEngine;

public static class TimeUtils
{
    public static float lasttime;           //Time.time
    public static float time;           //Time.time
    public static float deltaTime;      //Time.deltaTime
    public static int frameCount;
    public static float realFrameTime;

    public const long MsTicks = 10000L; //每毫秒
    public const long SecondTicks = 1000 * MsTicks;//每秒
    public const long HourTicks = 3600 * SecondTicks;//每小时
    public static int timezone => _timezone;
    public static long timestamp;        //服务器 当前帧 时间戳
    public static long timestamp_ms;    //服务器 当前帧 时间戳（毫秒）
    public static long real_timestamp_ms => _last_timestamp_ms + (int)((Time.realtimeSinceStartup - _last_realtime) * 1000);
    public static string server_lang => _server_lang;

    //static int _timezone_local;//本地时区
    static int _timezone;//服务器 时区
    static string _server_lang;//服务器 地区
    //static int _timezone_dt;

    static float _last_realtime;
    static long _last_timestamp_ms;
    static string _datetime;
    public static long _utc_1970_01_01_ticks = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;//服务器

    //static int _is_500ms_cnt2;
    public static int is_500ms_cnt;
    static float _is_500ms_tims;
    public static bool is_500ms;
    public static bool is_1_second;
    public static bool is_1_5_second;
    public static bool is_2_second;
    public static bool is_2_5_second;
    public static bool is_3_second;

    public static DateTime Now;//服务器 当前时间
    public static DateTime RealNow => GetDateTime(real_timestamp_ms);//服务器 当前时间

    public static DateTime GetDateTime(long timestamp_ms) //将 服务器时间戳 转换成 时间 
    {
        return new DateTime(_utc_1970_01_01_ticks + timestamp_ms * 10000L + _timezone * HourTicks, DateTimeKind.Utc);
    }

    public static string NowString  //服务器 当前帧 时间
    {
        get
        {
            if (_datetime == null)
            {
                _datetime = Now.ToString("MM/dd/yyyy HH:mm:ss");
            }
            return _datetime;
        }
    }


    public static void Update()
    {
        lasttime = time;
        time = Time.time;
        deltaTime = Time.deltaTime;
        frameCount = Time.frameCount;
        realFrameTime = Time.realtimeSinceStartup;
        //
        timestamp_ms = real_timestamp_ms;
        timestamp = (int)(timestamp_ms / 1000);
        Now = GetDateTime(timestamp_ms);
        _datetime = null;
        //
        if (time >= _is_500ms_tims)
        {
            var cnt = ++is_500ms_cnt;
            is_500ms = true;
            _is_500ms_tims += 0.5f;
            is_1_second = (cnt & 1) == 0;
            is_1_5_second = cnt % 3 == 0;
            is_2_second = (cnt & 3) == 0;
            is_2_5_second = cnt % 5 == 0;
            is_3_second = cnt % 6 == 0;
            //Log.LogError($"{cnt}, is_1_second={is_1_second}, is_1_5_second={is_1_5_second}, is_2_second={is_2_second}, is_2_5_second={is_2_5_second}, is_3_second={is_3_second}");
        }
        else if (is_500ms)
        {
            is_500ms = is_1_second = is_1_5_second = is_2_second = is_2_5_second = is_3_second = false;
        }        
    }

    public static void SetServerLang(string lang)
    {
        _server_lang = lang;
    }

    public static void SetTimestamp(long timestamp_ms, int timezone)
    {
        while (timezone > 12)
        {
            timezone -= 12;
        }
        while (timezone < -12)
        {
            timezone += 12;
        }
        _timezone = timezone;
        //_timezone_dt = _timezone - _timezone_local;
        _last_realtime = Time.realtimeSinceStartup;
        _last_timestamp_ms = TimeUtils.timestamp_ms = timestamp_ms;
        Log.ServerMsDT = timestamp_ms - (DateTime.UtcNow.Ticks - _utc_1970_01_01_ticks) / MsTicks + timezone * 3600 *1000;
        Update();
    }

    public static void Init()
    {
        var _timezone_local = TimeZoneInfo.Local.BaseUtcOffset.Hours;
        Log.Log2File($"local timezone={_timezone_local}, _utc_1970_01_01_ticks={_utc_1970_01_01_ticks}");
        SetTimestamp((DateTime.UtcNow.Ticks - _utc_1970_01_01_ticks) / MsTicks, _timezone_local);
    }

    static string time_ret = "";
    //minleftnum 代表左边显示最小分段数，如传3 则返回 00:00:00样式，传2 则如果不足显示时分位则返回 00:00，传1 则如果时分秒不足显示时，则返回00样式，传 0 则前面都不满足返回 0，默认为1
    public static string ParseTimeDesc(long second, int minleftnum = 1, bool is_add_end_s = false)
    {
        long hour = second / 3600;
        long min = second % 3600 / 60;
        second = second % 60;

        time_ret = "";
        if (hour != 0)
        {
            time_ret += (hour > 9 ? hour.ToString() : "0" + hour) + ":";
        }
        else if (minleftnum == 3) { time_ret += "00:"; }

        if (min != 0)
        {
            time_ret += (min > 9 ? min.ToString() : "0" + min) + ":";
        }
        else if (minleftnum >= 2) { time_ret += "00:"; }

        if (second != 0)
        {
            time_ret += second > 9 ? second.ToString() : "0" + second;
        }
        else if (minleftnum >= 1) { time_ret += "00"; } else { time_ret += "0"; }

        if (is_add_end_s)
        {
            if (minleftnum < 2 && hour == 0 && min == 0)
            {
                time_ret += "s";
            }
        }
        return time_ret;
    }

    /// <summary>
    /// Debug.LogError(TimeUtils.ParseMinuteDesc(0));           // ''
    /// Debug.LogError(TimeUtils.ParseMinuteDesc(1));           // 1秒
    /// Debug.LogError(TimeUtils.ParseMinuteDesc(60));          // 1分钟
    /// Debug.LogError(TimeUtils.ParseMinuteDesc(61));          // 1分钟1秒
    /// Debug.LogError(TimeUtils.ParseMinuteDesc(3600));        // 1小时
    /// Debug.LogError(TimeUtils.ParseMinuteDesc(3660));        // 1小时1分钟
    /// Debug.LogError(TimeUtils.ParseMinuteDesc(3661));        // 1小时1分钟1秒
    /// Debug.LogError(TimeUtils.ParseMinuteDesc(86400));       // 1天
    /// Debug.LogError(TimeUtils.ParseMinuteDesc(86401));       // 1天1秒
    /// Debug.LogError(TimeUtils.ParseMinuteDesc(999999999));   // 11574天1小时46分钟39秒
    /// </summary>
    public static string ParseSecondDesc(long seconds, string color_html = null)
    {
        long day = seconds / 86400;
        long hour = seconds % 86400 / 3600;
        long min = seconds % 3600 / 60;
        long leftSeconds = seconds % 60;
        // "{0:#天;;\"\"}{1:#小时;;\"\"}{2:#分钟;;\"\"}"
        if (string.IsNullOrEmpty(color_html))
        {
            return MyUITools.UIResPoolInstans.LangFromId("common_time_fmt1", new object[] { day, hour, min, leftSeconds });
        }
        else
        {
            string d = MyUITools.UIResPoolInstans.LangFromId("common_time_fmt2", new object[] { day, hour, min, leftSeconds });
            return d.Replace("html", color_html);
        }
    }

    //显示格式：x天x小时
    //当活动小于1天时显示：x小时x分
    //当活动小于1小时显示: X分: X秒,并且字体会变成红色
    public static string GetActCountDown(long cdTime, string color_html = null)
    {
        string cdFormat = "";
        if (cdTime > 86400)
        {
            long minTime = cdTime % 3600;
            cdTime = cdTime - minTime;
        }
        else if (cdTime > 3600)
        {
            long minTime = cdTime % 60;
            cdTime = cdTime - minTime;
        }
        else
        {
            cdFormat = color_html;
        }

        return ParseSecondDesc(cdTime, cdFormat);
    }

    public static long GetCurrDayTimestamp(string time_str)
    {
        string[] te = time_str.Split(':');
        int.TryParse(te[0], out int hour);
        int.TryParse(te[1], out int min);
        int.TryParse(te[2], out int sec);
        return GetCurrDayTimestamp(hour, min, sec);
    }

    public static long GetCurrDayTimestamp(int hour, int min, int sec)
    {
        var curr = Now.AddHours(hour - Now.Hour).AddMinutes(min - Now.Minute).AddSeconds(sec - Now.Second);
        var seconds = timestamp + (curr - Now).TotalSeconds;
        return (long)seconds;
    }
}
