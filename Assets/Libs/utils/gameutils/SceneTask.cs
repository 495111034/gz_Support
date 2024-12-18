//#define ENABLE_TRACE
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;


/// <summary>
/// 场景任务, 切换场景后丢失
/// </summary>
public static class SceneTask
{
    static MyTaskRunner _runner = new MyTaskRunner("SceneTask");

    // 启动一个任务
    public static MyTask Run(IEnumerator e)
    {
        var t = _runner.AddTask(e.ToString());
        t.Start(e);
        return t;
    }

    // 停止
    public static void Stop()
    {
        _runner.Stop();
    }

    // 更新
    public static void Update()
    {
        _runner.Update();
    }

}
