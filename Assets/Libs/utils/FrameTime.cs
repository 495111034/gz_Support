using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using UnityEngine;

/// <summary>
/// 帧时间控制
/// </summary>
public static class FrameTime
{
    //static Stopwatch _sw = new Stopwatch();
    static float StartFrameTime;

    // 帧开始
    public static void StartFrame(int targetFps)
    {
        StartFrameTime = Time.realtimeSinceStartup;

        //_sw.Reset();
        //_sw.Start();
    }

    // 本帧开始经过的时间
    public static int TimeMsSinceFrame
    {
        get { return (int)((Time.realtimeSinceStartup - StartFrameTime)*1000); }
    }

    // 帧时间 是否溢出, 即
    //public static bool IsFrameTimeout
    //{
    //    get { return TimeMsSinceFrame > 40; }
    //}

}