#if  UNITY_EDITOR
#define _TRACE_TASK_RUN_LIST
#endif


using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Linq;
using System.Collections.Concurrent;
using System.Diagnostics;

public interface ITaskDoneable
{
    public bool isDone { get; }
}

/// <summary>
/// 任务(协程)管理器, 用作AI逻辑实现
///     . 时间单位无关, 可以用秒/毫秒等
///     . 调用 Call/Start 时, 协程会被首先执行一次. 同时, 加入判断检测死循环
///     . 增加任务退出时的析构操作
/// </summary>
public class MyTask
{

    class MyTaskDelay 
    {
        public int delay_ms;
    }
    static MyTaskDelay one = new MyTaskDelay();

    public static object Delay(float ms) 
    {
        one.delay_ms = (int)ms;
        return one;
    }

    //public static long TotalFrame = 0;
    public static Func<Exception, string, bool> ExceptionHandler;

    static int _time = 0;                   // 当前时刻
    static MyTask _cur_t;                     // 当前 Task
    static TaskUnit _cur_u;                 // 当前 TaskUnit
    static List<MyTask> _global_run_list = new List<MyTask>();    // 自动运行的任务列表
    public static int global_running => _global_run_list.Count;

    // debug
    static int _calls;                      // 连续 call 次数, 避免死循环
    static MyTask _last_t;                    // 记录上次执行的任务
    static TaskUnit _last_u;

    // 全局更新
    public static void UpdateAll(int time)
    {
        if (time < _time) return;   // 必须 >=0 

        _time = time;
        _cur_t = null;
        _cur_u = null;
        _calls = 0;
        _last_t = null;
        _last_u = null;

        MyTask _curTask = null;
        try
        {
            var Array = _global_run_list;
            for (int i = Array.Count - 1; i >=0;--i)
            {
                _curTask = Array[i];
                UnityEngine.Profiling.Profiler.BeginSample(_curTask.name);
                _curTask.Update(i);
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }
        catch (System.Exception err)
        {
            Log.LogError($"MyTask.UpdateAll,task:{_curTask.name} error:{err.Message},{err.StackTrace}");
        }
    }

    static public Stack<IEnumerator> Last_tasks = new Stack<IEnumerator>();
    static public string _Last_opcode { get; private set; }
    static public string _Last_opcode2 { get; private set; }
    static public int IsChangingScene = 2;//0没有加载，1预加载，2全力加载
    static public float ChangeSceneDoneTime;
    static public string Last_opcode
    {
        set
        {
            _Last_opcode2 = _Last_opcode; _Last_opcode = value;
        }
    }
    public static long run_cnt { get; private set; }

    // 任务单元, 代表一个协程函数
    class TaskUnit
    {
#if UNITY_EDITOR2
        public string _call_stackstrace;
        public TaskUnit _prev;
        string CallStackStrace
        {
            get
            {
                return " \n  ---- called from ---->\n" + _call_stackstrace  + (_prev?.CallStackStrace ?? "");
            }
        }
#endif

        public IEnumerator e;       // 协程函数

        public string e_name;

        //public bool isDebug = false;
        public object data;         // 外部数据
        public int time;            // 唤醒时间
        public ITaskDoneable time2;   //
        public event Action leave;  // 析构函数
        //public List<MyTask> sub_tasks;
        bool _has_invoke_leave;
        public int dirty_cnt = 0;

        //public long _push_frame = 0;
        //public string _push_task = null;
        //public string _push_unit = null;

        public void __reset()
        {
            e = null;
            //isDebug = false;
            data = null;
            time = 0;
            time2 = null;
            leave = null;
            //sub_tasks = null;
            _has_invoke_leave = false;
            ++dirty_cnt;
            e_name = null;

            //_push_frame = Log.TotalFrame;
            //_push_task = _cur_t != null ? _cur_t.name : "null";
            //_push_unit = _cur_u != null ? $"{_cur_u.GetHashCode()}:{_cur_u.e}" : "null";

#if UNITY_EDITOR2
            _call_stackstrace = null;
            _prev = null;
#endif
        }


        // 调用 leave
        public void InvokeLeave()
        {
            _has_invoke_leave = true;
            if (leave != null)
            {
                try
                {
                    UnityEngine.Profiling.Profiler.BeginSample($"task leave {e_name}");
                    leave();
                    UnityEngine.Profiling.Profiler.EndSample();
                }
                catch (Exception err)
                {
                    var calls = "";
#if UNITY_EDITOR2
                    calls = CallStackStrace;
#endif
                    Log.LogError($"TaskUnit InvokeLeave, {err.GetType()}:{err.Message}\n{err.StackTrace}{calls}");
                }
                finally
                {
                    leave = null;
                }
            }
        }


        static long checked_max = 200;
        //static string _debug_logs = null;
        // 执行一次更新, 返回是否存活
        public bool Update()
        {
            //UnityEngine.Profiling.Profiler.BeginSample("TaskUnit.Update");

            ++run_cnt;
            // 执行
            var e = this.e;
            

            var old_u = _cur_u;
            _cur_u = _last_u = this;
            var alive = false;
            try
            {
                //UnityEngine.Profiling.Profiler.BeginSample("Last_tasks.Push");
                Last_tasks.Push(e);
                //UnityEngine.Profiling.Profiler.EndSample();

                var my_cnt = run_cnt;                
                var K41 = Log.GetMonoMemInKB();
                var m51 = Log.GetMonoMemInMB2();
                var _t1 = Time.realtimeSinceStartup;
                //
#if ENABLE_PROFILER
                if (e_name == null) 
                {
                    UnityEngine.Profiling.Profiler.BeginSample("new Profiling name");
                    e_name = e.ToString();
                    UnityEngine.Profiling.Profiler.EndSample();
                }
#endif
                MyTask.Last_opcode = _cur_t.name;
                UnityEngine.Profiling.Profiler.BeginSample(e_name);
                alive = e.MoveNext();
                UnityEngine.Profiling.Profiler.EndSample();
                MyTask.Last_opcode = null;
                var time_dt = Time.realtimeSinceStartup - _t1;
                //
                var cnt_dt = run_cnt - my_cnt;
                var K42 = Log.GetMonoMemInKB();
                var m52 = Log.GetMonoMemInMB2();


                if (IsChangingScene == 0 && (K42 - K41 > 512 || m52 > m51))
                {
                    if (!BuilderConfig.IsDebugBuild)
                    {
                        Log.Log2File($"mem slow, cost={K42 - K41}={K41}->{K42}KB,{m52 - m51}={m51}->{m52}MB, dt={cnt_dt}, parents={MyTask.Last_tasks.Count},MoveNext={alive},{e.Current}, {_cur_t.name} -> {e}");
                    }
                }

                if (time_dt > 1f || cnt_dt >= checked_max || (IsChangingScene == 0 && time_dt > 0.01f))
                {
                    if (cnt_dt > checked_max)
                    {
                        checked_max = cnt_dt;
                    }
                    if (!BuilderConfig.IsDebugBuild)
                    {
                        Log.Log2File($"time slow, cost={(int)(time_dt * 1000)}ms, dt={cnt_dt}, parents={MyTask.Last_tasks.Count},MoveNext={alive},{e.Current}, {_cur_t.name} -> {e}");
                    }
                }
            }
            catch (Exception err)
            {
                alive = false;
                var calls = "";
#if UNITY_EDITOR2
                calls = CallStackStrace;
#endif
                Log.LogError($"TaskUnit {e}, {this.GetHashCode()}, {err.GetType()}:{err.Message}\n{err.StackTrace}{calls}");
            }

            // 结束
            if (!alive)
            {
                InvokeLeave();
            }
            // 暂停
            else
            {
                
                UnityEngine.Profiling.Profiler.BeginSample("get e.Current");
                var obj = e.Current;
                UnityEngine.Profiling.Profiler.EndSample();
                var www = obj as ITaskDoneable;
                if (www == null)
                {
                    UnityEngine.Profiling.Profiler.BeginSample("ConvertToInt");
                    var wait_time = ConvertToInt(e, obj);
                    UnityEngine.Profiling.Profiler.EndSample();
                    time = _time + wait_time;
                }
                else 
                {
                    time2 = www;
                }                
            }
            _cur_u = old_u;
            //UnityEngine.Profiling.Profiler.BeginSample("Last_tasks.Pop");
            Last_tasks.Pop();
            //UnityEngine.Profiling.Profiler.EndSample();

            //UnityEngine.Profiling.Profiler.EndSample();            
            return alive && !_has_invoke_leave;
        }

        // 转换为 int
        static int ConvertToInt(IEnumerator e, object obj)
        {
            var ret = 0;
            if (obj != null)
            {
                if (obj is MyTaskDelay)
                {
                    ret = (obj as MyTaskDelay).delay_ms;
                }
                else
                {
                    Log.LogError($"请使用 yield return MyTask.Delay(delay_ms); 避免产生封箱gc！{e}");
                    if (obj is int)
                    {
                        ret = (int)obj;
                    }
                    else if (obj is float)
                    {
                        ret = (int)(float)obj;
                    }
                    else if (!int.TryParse(obj.ToString(), out ret))
                    {
                        ret = 0;
                        UnityEngine.Debug.LogError($"{e}, Cant convert yield {obj} to int");
                    }
                }
                if (ret < 0) ret = 0;
            }
            return ret;
        }
    }

    //end TaskUnit
    //
    public string name;     // 名字, 外部识别
    Stack<TaskUnit> _stacks = new Stack<TaskUnit>();   // 任务队列, 后进的最先被执行
    //bool _isDebug = false;

    List<MyTask> _run_list;    // 自动执行列表
    bool _is_in_run_list = false;
    //bool _is_in_list;
    public bool can_push_to_cache = false;     //是否可以缓存池
    //bool _is_stoped = false;
    float dirty_cnt = 0;

    void __reset()
    {
        if (this._run_list == null)
        {
            Log.LogError($"----------------_run_list == null on __reset, {this.GetHashCode()}");
        }
        else
        {
#if _TRACE_TASK_RUN_LIST
            if (this._run_list.Contains(this))
            {
                Log.LogError($"----------------Contains on __reset, {this.GetHashCode()}");
            }
#endif
        }
        this._run_list = null;

        if (_stacks.Count > 0)
        {
            Log.LogError($"----------------_stacks.Count > 0 on __reset, {this.GetHashCode()}");
            this._stacks.Clear();
        }
        this.name = null;
        this.can_push_to_cache = false;
        //this._is_stoped = false;
        this.dirty_cnt += 1;
    }


    static Stack<MyTask> _task_caches = new Stack<MyTask>();
    public static MyTask GetTask(List<MyTask> run_list, string name)
    {
        run_list = run_list ?? _global_run_list;

        MyTask t;
        if (_task_caches.Count > 0)
        {
            t = _task_caches.Pop();
            if (t == _cur_t)
            {
                Log.LogError($"pop task == _cur_t");
            }
            t.name = string.IsNullOrEmpty(name) ? "" : "task:" + name;
            t.BindAutoRunList(run_list);
            //Log.LogError($"pop task {t.name},{t.GetHashCode()}");
        }
        else
        {
            t = new MyTask(run_list, name);
        }
        return t;
    }

    public static void _push_task(MyTask t)
    {
        if (t != null)
        {
            //Log.LogError($"push task {t.name},{t.GetHashCode()}");
            t.__reset();
            if (_task_caches.Count < 200)
            {
                _task_caches.Push(t);
            }
        }
    }

    public static void CacheTask(MyTask t)
    {
        if (t != null)
        {
            t.can_push_to_cache = true;
        }
    }



    //
    public MyTask(bool autoRun, string name = "")
    {
        //Log.LogError($"new task1, {_task_caches.Count} {name},{GetHashCode()}");
        this.name = string.IsNullOrEmpty(name) ? "" : "task:" + name;
        LeakManager.AddWatch(this);

        BindAutoRunList(_global_run_list);
    }
    public MyTask(List<MyTask> run_list, string name = "")
    {
        //Log.LogError($"new task2, {_task_caches.Count} {name},{GetHashCode()}");
        this.name = string.IsNullOrEmpty(name) ? "" : "task:" + name;
        LeakManager.AddWatch(this);

        BindAutoRunList(run_list ?? _global_run_list);
    }

    // 绑定到自动运行列表
    void BindAutoRunList(List<MyTask> run_list)
    {
        if (Application.isEditor && !string.IsNullOrEmpty(name))
        {
            var cnt = 0;
            var array = run_list;
            foreach (var t in array)
            {
                if (t.IsRunning && t.name == this.name)
                {
                    ++cnt;
                }
            }
            if (cnt >= 50)
            {
                Log.Log2File($"检测到很多同名协程: task name={name}, cnt={cnt}, run_list.Length={array.Count}, is_global_run_list={run_list == _global_run_list}");
            }
        }
        _run_list = run_list;
#if _TRACE_TASK_RUN_LIST
        if (this._run_list.Contains(this))
        {
            Log.LogError($"----------------Contains on BindAutoRunList, {this.GetHashCode()}");
        }
#endif
    }

    public static int AllUnitCount()
    {
        var _global_run_list = MyTask._global_run_list;
        int c = 0;
        for (int i = _global_run_list.Count - 1; i >=0;  --i)
        {
            c += _global_run_list[i].UnitCount;
        }
        return c;
    }
    public int UnitCount { get { return _stacks.Count; } }


    public bool CanUpdate() 
    {
        if (_stacks.Count == 0) 
        {
            return false;
        }
        return _stacks.Peek().time < Time.time * 1000;
    }

    // 执行更新
    public void Update(int idx)
    {
        // 结束
        if (_stacks.Count == 0)
        {
            if (_run_list == null)
            {
                Log.LogError($"----------------_run_list is null on Update {this.GetHashCode()}");
            }
            else
            {
                var List = _run_list;
                var Count = List.Count;
                if (idx >= 0 && idx < Count && List[idx] == this)
                {
                    //Log.LogError($"1remove at {idx}, _run_list.Count={Count}");
                    if (idx == 0)
                    {
                        List.RemoveAt(idx);
                    }
                    else
                    {
                        List[idx] = List[Count-1];
                        List.RemoveAt(Count - 1);//删除最后一个性能比较好
                    }
                }
                else
                {
                    if(Application.isEditor) Log.LogWarning($"remove MyTask:{name} at {idx}, {GetHashCode()}");
                    if (!List.Remove(this))
                    {
                        Log.LogError($"----------------Remove fail on Update, {this.GetHashCode()}");
                    }
                }
                _is_in_run_list = false;
                if (this.can_push_to_cache)
                {
                    _push_task(this);
                }
            }
            return;
        }

        // 获取第1个执行单元
        //UnityEngine.Profiling.Profiler.BeginSample("_stacks.First()");
        var u = _stacks.Peek();
        //UnityEngine.Profiling.Profiler.EndSample();
        if (u.time > _time)
        {
            return;
        }

        if (u.time2 != null && !u.time2.isDone) 
        {
            return;
        }

        //
        var e = u.e;
        //var e_name = e.ToString();
        var dirty_cnt = u.dirty_cnt;
        var olddirty_cnt = this.dirty_cnt;
        var oldname = this.name;

        // 执行协程, 如果结束, 则删除
        var old_t = _cur_t;
        _cur_t = _last_t = this;

        //UnityEngine.Profiling.Profiler.BeginSample(oldname);
        var result = u.Update();
        //UnityEngine.Profiling.Profiler.EndSample();        

        if (!result)
        {
            // u 执行完毕
            if (dirty_cnt == u.dirty_cnt)
            {
                //UnityEngine.Profiling.Profiler.BeginSample("_stacks.Pop()");
                var pop = _stacks.Count > 0 ? _stacks.Pop() : null;
                //UnityEngine.Profiling.Profiler.EndSample();
                if (pop != null && pop != u)
                {
                    _stacks.Push(pop);
                    Log.LogError($"task={oldname},{olddirty_cnt} expect u={u.GetHashCode()},u.e={u.e}, e={e}, but now task {name},{dirty_cnt} got pop={pop.GetHashCode()}, pop.e={pop.e}");
                }
                else
                {
                    add_taskunit_to_caches(u);
                }
            }
            else
            {
                Log.LogError($"dirty_cnt not match, task={name},{GetHashCode()} -> old u={e} -> now u={u.e},{u.GetHashCode()}");//,({u._push_frame},{u._push_task},{u._push_unit})
            }
        }
        _cur_t = old_t;
    }

    // 停止所有任务, 并调用 leave
    public void Stop()
    {
        if (_stacks.Count > 0)
        {
            dirty_cnt += 0.01f;
            //Log.LogWarning($"Stop {name},{GetHashCode()}");
            var arr = _stacks.ToArray();
            _stacks.Clear();
            // 调用 leave
            foreach (var u in arr)
            {
                if (u != _cur_u)
                {
                    u.InvokeLeave();
                    add_taskunit_to_caches(u);
                }
            }
        }

    }

    // 执行任务
    public void Start(IEnumerator e)
    {
        _Start(e, true);
    }

    static int _max_task_count = 0;
    void _Start(IEnumerator e, bool bStopAll)
    {
        if (string.IsNullOrEmpty(name))
        {
            name = "task:" + e.ToString();
        }

        if (bStopAll) Stop();

        // 执行第一次
        var old_t = _cur_t;
        _cur_t = _last_t = this;
        UnityEngine.Profiling.Profiler.BeginSample(name);
        Call(e);
        UnityEngine.Profiling.Profiler.EndSample();
        _cur_t = old_t;


        if (_run_list == null)
        {
            Log.LogError($"----------------_run_list == null on _Start, {this.GetHashCode()}");
        }
        else
        {
            // 添加到自动执行队列
            if (!_is_in_run_list)
            {
                //Log.LogError($"_Start, {this.GetHashCode()}");
#if _TRACE_TASK_RUN_LIST
                if (this._run_list.Contains(this))
                {
                    Log.LogError($"----------------Contains on _Start, {this.GetHashCode()}");
                }
#endif
                _is_in_run_list = true;
                _run_list.Add(this);
                if (_run_list.Count >= 200 && _run_list.Count % 20 == 0)
                //if(_run_list.Count > _max_task_count)
                {
                    //_max_task_count = _run_list.Count;
                    Log.LogWarning($"_run_list.Count={_run_list.Count}, is_global_run_list={_run_list == _global_run_list}, task name={name}\n{new System.Diagnostics.StackTrace(true)}");
                }
            }
            else
            {
#if _TRACE_TASK_RUN_LIST
                if (!this._run_list.Contains(this))
                {
                    Log.LogError($"----------------not Contains on _Start, {this.GetHashCode()}");
                }
#endif
            }
        }
    }


    // 中断当前任务, 执行新任务 e
    public void Interrupt(IEnumerator e, CanInterruptHandler handler)
    {
        if (this == _cur_t && this._stacks.Count > 0)
        {
            Log.LogError($"MyTask, can't Interrupt self!");
            return;
        }
        // 中断之前的任务
        if (handler != null)
        {
            _Interrupt(handler);
        }
        //
        _Start(e, false);
    }
    void _Interrupt(CanInterruptHandler handler)
    {
        while (_stacks.Count > 0)
        {
            var u = _stacks.Peek();
            if (!handler(u.data))
            {                
                break;
            }
            u.InvokeLeave();
            _stacks.Pop();
        }
    }
    public delegate bool CanInterruptHandler(object data);      // 返回某个函数释放可被中断

    // 任务状态
    public bool IsDone
    {
        get { return _stacks.Count == 0; }
    }
    public bool IsRunning
    {
        get { return _stacks.Count > 0; }
    }
    public bool IsCurrent
    {
        get { return MyTask._cur_t == this; }
    }

    // 返回所有的 data 数组, 堆栈顺序, [0]为最新, [n-1]为最老
    public List<object> GetTmpDatas()
    {
        if (_data_list == null) _data_list = new List<object>();
        _data_list.Clear();
        foreach (var u in _stacks)
        {
            _data_list.Add(u.data);
        }
        return _data_list;
    }
    static List<object> _data_list;

    // 返回所有的 data 数组, 堆栈顺序, [0]为最新, [n-1]为最老
    public object[] GetDatas()
    {
        var arr = from x in _stacks select x.data;
        return arr.ToArray();
    }

    #region 静态方法

    // 运行一个协程
    public static void Run(IEnumerator e)
    {
        var t = MyTask.GetTask(null, e.ToString());//new MyTask(true,e.ToString());
        t.can_push_to_cache = true;
        t.Start(e);
    }

    public static MyTask RunTask(IEnumerator e)
    {
        var t = new MyTask(true, e.ToString());
        t.Start(e);
        return t;
    }

    static Stack<TaskUnit> _taskunit_caches = new Stack<TaskUnit>(200);
    static void add_taskunit_to_caches(TaskUnit u)
    {
        //Log.LogError($"push {_taskunit_caches.Count} {u.e},{u.GetHashCode()}");
        u.__reset();
        if (_taskunit_caches.Count < 200)
        {
            _taskunit_caches.Push(u);
        }
    }

    // 调用一个指函数 e, 等它结束后再返回自己
    // 注意: 一定要用 yield return Task.Call 方式调用, 否则执行结果不可预测
    public static object Call(IEnumerator e)
    {
        if (++_calls > 1000) throw new Exception("检测到死循环!");

        // 添加任务
        TaskUnit u;
        if (_taskunit_caches.Count > 0)
        {
            u = _taskunit_caches.Pop();
            u.e = e;
            //Log.LogError($"pop {_taskunit_caches.Count} {u.e},{u.GetHashCode()}");
        }
        else
        {
            u = new TaskUnit()
            {
                e = e,
#if UNITY_EDITOR2
            _call_stackstrace = new StackTrace(true).ToString(),
            _prev = _cur_t._stack.Count > 0 ? _cur_t._stack[0] : null,
#endif
            };
            //Log.LogError($"new {u.e},{u.GetHashCode()} at {_cur_t.name},{_cur_t.GetHashCode()}");
        }
        _cur_t._stacks.Push(u);     // 添加到头部
        var Count = _cur_t._stacks.Count;
        if (Count >= 200 && Count % 20 == 0)
        {
            Log.LogError($"{_cur_t.name} add {e}, _stack.Count={Count}\n{new System.Diagnostics.StackTrace(true)}");
        }

        // 执行一次
        _cur_t.Update(-1);

        --_calls;
        return null;
    }

    // 结束当前函数, 并跳转到新函数 e. 类似状态机中的状态迁移, 或者函数尾调用
    // 注意: 一定要用 yield return Task.Goto 方式调用, 否则执行结果不可预测
    public static object Goto(IEnumerator e)
    {
        // 结束当前函数
        _cur_u.InvokeLeave();

        // 调用新函数
        return Call(e);
    }
    // 终止当前 task, 因为 yield break 只能终止当前协程, 无法终止整个 task
    public static void StopCurrent()
    {
        _cur_t.Stop();
    }

    // 设置当前函数的析构函数
    public static void SetLeave(Action handler)
    {
        _cur_u.leave += handler;
    }
    public static void InvokeLeave()
    {
        _cur_u.InvokeLeave();
    }

    // 设置当前函数私有数据
    public static void SetData(object data)
    {
        _cur_u.data = data;
    }
    public static object GetData()
    {
        return _cur_u.data;
    }



    #endregion

    #region 其它

    // 延迟调用, key 用于避免多次调用, 可以为 null
    public static MyTask DelayCall(string key, int wait_time, Action callback)
    {
        // 获取 task
        MyTask task = null;
        if (key != null)
        {
            _delay_call_tasks.TryGetValue(key, out task);
            if (task == null)
            {
                task = new MyTask(true);
                _delay_call_tasks.Add(key, task);
            }
        }
        else
        {
            task = new MyTask(true);
        }

        // 开始协程
        task.Start(_DelayCall(key, wait_time, callback));

        //
        return task;
    }
    static IEnumerator _DelayCall(string key, int wait_time, Action callback)
    {
        yield return Delay(wait_time);
        if (key != null) _delay_call_tasks.Remove(key);
        callback();
    }
    static Dictionary<string, MyTask> _delay_call_tasks = new Dictionary<string, MyTask>();

    // 取消延迟调用, 如果已经开始, 则会被终止
    public static void DelayCallCancel(string key)
    {
        MyTask task = null;
        if (_delay_call_tasks.TryGetValue(key, out task))
        {
            _delay_call_tasks.Remove(key);
            task.Stop();
        }
    }


    //
    public override string ToString()
    {
        return $"name:{name}, units:{_stacks.Count}";
    }



    #endregion
}

