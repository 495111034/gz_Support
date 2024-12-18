
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace thread
{
    public class ThreadTask
    {
        volatile bool _IsCompleted;
        public bool IsCompleted => _IsCompleted;
        
        volatile Action _action;
        
        volatile Action<object> _action2;
        object _param;

        internal volatile bool auto_release;

        internal void _run()
        {
            try
            {
                if (_action != null)
                {
                    _action.Invoke();
                }
                if (_action2 != null) 
                {
                    _action2.Invoke(_param);
                }
            }
            catch (Exception e)
            {
                Log.LogError($"call {_action?.Method},{_action2?.Method} {e.Message}\n{e.StackTrace}");
            }
            finally
            {
                _IsCompleted = true;
            }
        }
        internal ThreadTask(Action action)
        {
            _action = action;
        }

        internal ThreadTask(Action<object> action, object param)
        {
            _action2 = action;
            _param = param;
        }


        static public void Release(ref ThreadTask t) 
        {
            if (t._IsCompleted && _tasks.Count < 16)
            {
                t._IsCompleted = false;
                t._action = null;
                t._action2 = null;
                t._param = null;
                t.auto_release = false;
                _tasks.Push(t);
                t = null;                
            }        
        }

        static ConcurrentStack<ThreadTask> _tasks = new ConcurrentStack<ThreadTask>();
        static public ThreadTask New(Action action)
        {
            if (!_tasks.TryPop(out var t))
            {
                t = new ThreadTask(action);
            }
            else
            {
                t._action = action;
            }
            return t;
        }
        static public ThreadTask New(Action<object> action, object param) 
        {
            if (!_tasks.TryPop(out var t))
            {
                t = new ThreadTask(action, param);
            }
            else 
            {
                t._action2 = action;
                t._param = param;
            }
            return t;
        }
    }

    public class TaskThread
    {
        public int idx = 0;//from 1
        public bool running = false;
        public TaskThread(int idx)
        {
            this.idx = idx;
        }
    }

    public static class ThreadTaskPool
    {

        static ConcurrentQueue<ThreadTask> _io_xtasks = new ConcurrentQueue<ThreadTask>();
        static ConcurrentQueue<ThreadTask> _xtasks = new ConcurrentQueue<ThreadTask>();
        static ConcurrentQueue<ThreadTask> _vip_xtasks = new ConcurrentQueue<ThreadTask>();
        static AutoResetEvent _xtasks_cond = new AutoResetEvent(true);
        static TaskThread[] _threads;

        static int _processorCount = SystemInfo.processorCount;
        public static int processorCount 
        {
            get 
            {
                if (_processorCount <= 0)
                {
                    _processorCount = 2;//Òì³£
                }
                return _processorCount;
            }
        }


        public static void Init()
        {
            var num = ThreadTaskPool.processorCount - 1;
            if (num < 1)
            {
                num = 1;
            }
            Log.Log2File($"ThreadTaskPool create {num} tasks start");
            _threads = new TaskThread[num];
            for (var i = 1; i <= num; ++i)
            {
                var th = new Thread(_loop);
                th.Start(_threads[i - 1] = new TaskThread(i));
            }
            Log.Log2File($"ThreadTaskPool create {num} tasks done");
        }

        static ThreadTask _Run(ThreadTask task, int vip = 0)
        {
            if (vip < 0)
            {
                _io_xtasks.Enqueue(task);
            }
            else if (vip == 0)
            {
                _xtasks.Enqueue(task);
            }
            else
            {
                _vip_xtasks.Enqueue(task);
            }
            _xtasks_cond.Set();
            return task;
        }


        public static ThreadTask Run(Action action, int vip = 0)
        {
            var task = ThreadTask.New(action);
            return _Run(task,vip);
        }

        public static ThreadTask Run(Action<object> action, object param, int vip, bool auto_release)
        {
            var task = ThreadTask.New(action, param);
            task.auto_release = auto_release;
            return _Run(task, vip);
        }

        static volatile bool _exit;
        public static void OnApplicationQuit()
        {
            _exit = true;
            for (var i = 0; i < ThreadTaskPool.processorCount; ++i)
            {
                _xtasks_cond.Set();
            }
        }

        public static bool IsAllIdle()
        {
            if (!_vip_xtasks.IsEmpty || !_xtasks.IsEmpty || !_io_xtasks.IsEmpty)
            {
                return false;
            }

            for (var i = 0; i < _threads.Length; ++i)
            {
                if (_threads[i].running)
                {
                    return false;
                }
            }
            return true;
        }


        static void _loop(object p)
        {
            var thread = p as TaskThread;
            Log.Log2File($"pool thread {thread.idx} start");
            ThreadTask running;
            while (true)
            {
                running = null;
                if (thread.idx == 1) 
                {
                    _io_xtasks.TryDequeue(out running);
                }
                if (running == null && !_vip_xtasks.TryDequeue(out running) && !_xtasks.TryDequeue(out running))
                {
                    if (!_io_xtasks.IsEmpty)
                    {
                        if (!_threads[0].running)
                        {
                            //Log.LogError($"{thread.idx} wait up thread 0");
                            _xtasks_cond.Set();
                        }
                    }
                    else
                    {
                        if (_exit)
                        {
                            break;
                        }
                    }
                    //Log.LogInfo($"pool thread {thread.idx} WaitOne ..." );
                    _xtasks_cond.WaitOne();
                    //Log.LogInfo($"pool thread {thread.idx} WaitOne done");
                }
                else
                {
                    //
                    thread.running = true;
                    running._run();
                    thread.running = false;
                    //Log.LogInfo($"run task done, {running.GetHashCode()}");
                    if (running.auto_release) 
                    {
                        ThreadTask.Release(ref running);
                    }
                }
            }
            Log.Log2File($"pool thread {thread.idx} done");
        }
    }
}



