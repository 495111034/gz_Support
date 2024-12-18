using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 任务运行器
/// </summary>
public class MyTaskRunner
{
    public string name { get; set; }
    List<MyTask> _run_list = new List<MyTask>();
    private int _max_running = 0;
    public MyTaskRunner(string name)
    {
        this.name = name;
    }

    public MyTaskRunner(string name, int max_running)
    {
        this.name = name;
        _max_running = max_running;
    }


    // 获取运行中的任务
    public List<MyTask> GetRunningTasks()
    {
        return _run_list;
    }

    // 是否完成
    public bool IsDone
    {
        get
        {
            return _run_list.Count == 0;
        }
    }

    // 添加
    public MyTask AddTask(string name = "")
    {
        return new MyTask(_run_list, name);
    }

    // 停止
    public void Stop()
    {
        var _run_list = this._run_list;
        for (int i = 0, Count = _run_list.Count; i < Count; ++i )
        {
            _run_list[i].Stop();
        }
        //_run_list.Clear();
    }

    // 更新
    //int _next_update_i = 0;
    public void Update()
    {
        var _run_list = this._run_list;
        for (int i = _run_list.Count - 1; i >= 0; --i)
        {
            UnityEngine.Profiling.Profiler.BeginSample(_run_list[i].name);
            _run_list[i].Update(i);
            UnityEngine.Profiling.Profiler.EndSample();
        }
    }

    public int Count { get { return _run_list.Count; } }

    // 开始
    public void Run(IEnumerator e, string name = null)
    {
        var t = MyTask.GetTask(_run_list, name ?? e.ToString());
        t.can_push_to_cache = true;//阅后即焚
        t.Start(e);
    }
}