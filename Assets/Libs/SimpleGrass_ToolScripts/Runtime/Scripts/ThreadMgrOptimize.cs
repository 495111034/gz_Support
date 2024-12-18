using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using System.Threading;

namespace SimpleGrass
{

   // [ExecuteInEditMode]
    public class ThreadMgrOptimize : MonoBehaviour
    {
        //根据需求设置默认并发数
        private static int  DefaultConcurrentCount = 2;
        //
        private static int SleepTimeWhenEmptyTaskInLoopMode = 130;
        //线程锁
        private static object _lock = new object();
        //默认静态串行队列对象
        private static TaskQueue _defaultSerial;
        //默认静态并发队列对象
        private static TaskQueue _defaultConcurrentLoop;
        
        /// <summary>
        /// our thread instance;
        /// </summary>       
        public static ThreadMgrOptimize instance
        {
            get
            {               
                return _instance;
            }
        }

        //提供默认串行队列
        public static TaskQueue DefaultSerailQueue
        {
            get
            {
                if (_defaultSerial == null)
                {
                    lock (_lock)
                    {
                        if (_defaultSerial == null)
                        {
                            _defaultSerial = new TaskQueue(1,false,0);
                        }
                    }
                }
                return _defaultSerial;
            }
        }

        //提供默认并发队列
        public static TaskQueue DefaultConcurrentLoopQueue
        {
            get
            {
                if (_defaultConcurrentLoop == null)
                {
                    lock (_lock)
                    {
                        if (_defaultConcurrentLoop == null)
                        {
                            _defaultConcurrentLoop = new TaskQueue(DefaultConcurrentCount,true, SleepTimeWhenEmptyTaskInLoopMode);
                        }
                    }
                }
                return _defaultConcurrentLoop;
            }
        }


        internal static ThreadMgrOptimize _instance;

        /// <summary>
        /// Is the multi-thread option enabled?
        /// </summary>
        public bool threadEnabled = true;

        /// <summary>
        /// Thread workers count
        /// </summary>
        int defMinWorkerThreads = 0;
        int defMinCompletionPortThreads = 0;
        int defMaxWorkerThreads = 0;
        int defMaxCompletionPortThreads = 0;

        //int minThreadWorkersCount = -1;
        //public int MinThreadWorkersCount
        //{
        //    get { return minThreadWorkersCount; }
        //    set
        //    {
        //        if(minThreadWorkersCount != value)
        //        {
        //           this.minThreadWorkersCount = value;
        //           int  newMinWorkerThreads = Math.Max(defMinWorkerThreads, this.minThreadWorkersCount);
        //           int newMinCompletionPortThreads = Math.Max(defMinCompletionPortThreads, this.minThreadWorkersCount);
        //           ThreadPool.SetMinThreads(newMinWorkerThreads, newMinCompletionPortThreads);
        //        }
        //    }
        //}

        //int maxThreadWorkersCount = -1;
        //public int MaxThreadWorkersCount
        //{
        //    get { return maxThreadWorkersCount; }
        //    set
        //    {
        //        if (maxThreadWorkersCount != value)
        //        {
        //            maxThreadWorkersCount = value;
        //            int newMaxWorkerThreads = Math.Max(defMaxWorkerThreads, this.maxThreadWorkersCount);
        //            int newMaxCompletionPortThreads = Math.Max(defMaxCompletionPortThreads, this.maxThreadWorkersCount);
        //            ThreadPool.SetMaxThreads(newMaxWorkerThreads, newMaxCompletionPortThreads);
        //        }
        //    }
        //}

        public static void InitializeIfNotAvailable(GameObject parentNode,int concurrentCount, int sleepTimeInLoopMode)//, int minThreadCount, int maxThreadCount)
        {
            if (instance == null)
            {
                _instance = parentNode.GetComponent<ThreadMgrOptimize>();

                if (_instance == null)
                {                                      
                    _instance = parentNode.AddComponent<ThreadMgrOptimize>();                    
                }               

                //Debug.Log("Threading Manager Has Been Initialized Successfully !!");
            }
            DefaultConcurrentCount = concurrentCount;
            SleepTimeWhenEmptyTaskInLoopMode = sleepTimeInLoopMode;
           // _instance.MinThreadWorkersCount = minThreadCount;
           // _instance.MaxThreadWorkersCount = maxThreadCount;
        }
       

        /// <summary>
        /// List of all queued unity thread actions
        /// </summary>
        static List<IThreadTask> UnityThreadQueuedActions = new List<IThreadTask>();

        public static bool inUnityThread
        {
            get
            {
                return Thread.CurrentThread == unityThreadIdentifier;
            }
        }

        

        /// <summary>
        /// Called when the object firstly enabled.
        /// </summary>
        void Awake()
        {
            //ThreadPool.GetMinThreads(out defMinWorkerThreads, out defMinCompletionPortThreads);
            //ThreadPool.GetMaxThreads(out defMaxWorkerThreads, out defMaxCompletionPortThreads);
        }

        /// <summary>
        /// Grab the unity thread to check if we are in it in the future.
        /// </summary>
        static Thread unityThreadIdentifier;

        /// <summary>
        /// Called when the object is enabled
        /// </summary>
        void OnEnable()
        {
            _instance = this;

            unityThreadIdentifier = Thread.CurrentThread;
        }

        /// <summary>
        /// Called when the object is disabled
        /// </summary>
        void OnDisable()
        {
        }

        void Destroy()
        {
            if (_defaultSerial != null)
            {
                _defaultSerial.Destroy();
                _defaultSerial = null;
            }

            if (_defaultConcurrentLoop != null)
            {
                _defaultConcurrentLoop.Destroy();
                _defaultConcurrentLoop = null;
            }
        }

        /// <summary>
        /// Access to unity thread
        /// </summary>
        void Update()
        {
            if (!Application.isPlaying) return;

            threadUpdate();
        }

        /// <summary>
        /// Update the unity thread
        /// </summary>
        private void threadUpdate()
        {
            IThreadTask queuedData;

            for (int i = 0; i < UnityThreadQueuedActions.Count; i++)
            {
                queuedData = UnityThreadQueuedActions[i];

                if (queuedData != null)
                {
                    queuedData.Invoke();
                }
            }

            UnityThreadQueuedActions.Clear();
        }


        /// <summary>
        /// Add an action to the unity thread
        /// </summary>
        /// <param name="action">the action</param>
        public void RunOnUnityThread(IThreadTask action)
        {
            if (action == null) return;

            if (inUnityThread)
            {
                action.Invoke();
            }
            else
            {
                UnityThreadQueuedActions.Add(action);
            }
        }

        /// <summary>
        /// Add an action to the UN thread
        /// </summary>
        /// <param name="action">the action</param>
        public void RunOnThreadAsync(IThreadTask action)
        {
            if (threadEnabled)
            {
                //ThreadPool.QueueUserWorkItem(new WaitCallback(OnThreadProcess), action);
                DefaultConcurrentLoopQueue.Run(action);
            }
            else
            {
                RunOnUnityThread(action);
            }
        }

        public void RunOnThreadSync(IThreadTask action)
        {
            if (threadEnabled)
            {
                //ThreadPool.QueueUserWorkItem(new WaitCallback(OnThreadProcess), action);
                DefaultSerailQueue.Run(action);
            }
            else
            {
                RunOnUnityThread(action);
            }
        }

        /// <summary>
        /// Called when the thread needs to process the task.
        /// </summary>
        /// <param name="processObject"></param>
        protected void OnThreadProcess(System.Object processObject)
        {
            IThreadTask task = processObject as IThreadTask;

            if (task != null)
            {
                try
                {
                    task.Invoke();
                }
                catch(UnityException ex)
                {
                    Debug.LogError("uNature Thread Manager : Error caught while running thread action : \n" + ex);
                }
            }
            else
            {
                Debug.LogError("uNature Thread Manager : Unrecognized thread process : " + processObject.ToString());
            }
        }

        #region CoroutinesUtility

        /// <summary>
        /// Run any action with a specific delay of seconds.
        /// </summary>
        /// <param name="task">the task you want to run after the specific amount of seconds</param>
        /// <param name="time">the specific amount of seconds to wait</param>
        public void DelayActionSeconds(IThreadTask task, float time)
        {
            StartCoroutine(DelayActionSecondsCoroutine(task, time));
        }

        /// <summary>
        /// Run any action after 1 frame
        /// </summary>
        /// <param name="task">the task you want to run after 1 frame</param>
        public void DelayActionFrames(int frames, IThreadTask task)
        {
            StartCoroutine(DelayActionFrameCoroutine(frames, task));
        }

        /// <summary>
        /// Run any action with a specific delay of seconds.
        /// </summary>
        /// <param name="task">the task you want to run after the specific amount of seconds</param>
        /// <param name="time">the specific amount of seconds to wait</param>
        private IEnumerator DelayActionSecondsCoroutine(IThreadTask task, float time)
        {
            yield return new WaitForSeconds(time);

            task.Invoke();
        }

        /// <summary>
        /// Run any action after 1 frame
        /// </summary>
        /// <param name="task">the task you want to run after 1 frame</param>
        private IEnumerator DelayActionFrameCoroutine(int frames, IThreadTask task)
        {
            for (int i = 0; i < frames; i++)
            {
                yield return new WaitForEndOfFrame();
            }

            task.Invoke();
        }

        #endregion
    }
    
    public class LimitedConcurrencyTaskScheduler
    {        
        //ThreadStatic 线程变量特性，表明是当前线程是否正在处理任务
        [ThreadStatic]
        private static bool _currentThreadIsProcessingItems;

        // 任务队列，使用链表比 List 和 Array 更方便执行插队出队操作（队列中不会出现空位）
        private readonly LinkedList<IThreadTask> _tasks = new LinkedList<IThreadTask>(); // 该队列由 lock(_tasks) 锁定

        // 最大并发数
        private readonly int _maxDegreeOfParallelism;

        // 当前已分配入队的任务数量 
        private int _delegatesQueuedOrRunning = 0;

        private int _threadSleep_WhenEmptyTask = 0;

        private bool _isLoopThread = false;

        private bool _tryTerminate = false;
        // 带并发数的构造方法
        public LimitedConcurrencyTaskScheduler(int maxDegreeOfParallelism,bool isLoopThread,int threadSleepWhenEmptyTask)
        {
            if (maxDegreeOfParallelism < 1) throw new ArgumentOutOfRangeException("maxDegreeOfParallelism");
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
            _threadSleep_WhenEmptyTask = threadSleepWhenEmptyTask;
            _isLoopThread = isLoopThread;
        }

        // 将 Task 放入调度队列
        public void QueueTask(IThreadTask task)
        {
            //将任务放入列表，检查当前执行数是否达到最大值，若未达到则分配线程执行，并计数+1
            lock (_tasks)
            {
                _tasks.AddLast(task);
                if (_delegatesQueuedOrRunning < _maxDegreeOfParallelism)
                {
                    ++_delegatesQueuedOrRunning;
                    NotifyThreadPoolOfPendingWork();
                }
            }
          }

        public void Destroy()
        {
            _tryTerminate = true;
        }

        // 使用 ThreadPool 将 Task 分配到工作线程
        private void NotifyThreadPoolOfPendingWork()
        {
            ThreadPool.UnsafeQueueUserWorkItem(_ =>
            {
                //标记当前线程正在执行任务，当有 Task 想插入此线程执行时会检查该状态
                _currentThreadIsProcessingItems = true;
                try
                {
                    // 死循环处理所有队列中 Task
                    while (true)
                    {
                        //指示强制结束
                        if (_tryTerminate)
                        {
                            --_delegatesQueuedOrRunning;
                            break;
                        }

                        IThreadTask item = null;
                        bool isEmptySleep = false;
                        lock (_tasks)
                        {
                            // 任务队列执行完后退出循环，并将占用标记置为 false
                            if (_tasks.Count == 0)
                            {
                                if (_isLoopThread)
                                {
                                    isEmptySleep = true;
                                }
                                else
                                {
                                    --_delegatesQueuedOrRunning;
                                    break;
                                }
                            }
                            else
                            {

                                // 若还有 Task 则获取第一个，并出队
                                item = _tasks.First.Value;
                                _tasks.RemoveFirst();
                            }
                        }                        
                        // 执行 Task
                        TryExecuteTask(item);

                        if (isEmptySleep)
                            Thread.Sleep(_threadSleep_WhenEmptyTask);
                    }
                }
                // 线程占用标记置为 false
                finally { _currentThreadIsProcessingItems = false; }
            }, null);
        }

        // 尝试在当前线程执行指定任务
        protected bool TryExecuteTaskInline(IThreadTask task, bool taskWasPreviouslyQueued)
        {
            // 若当前线程没有在执行任务则无法执行插队操作
            if (!_currentThreadIsProcessingItems) return false;

            // 若该任务已在队列中，则出队
            if (taskWasPreviouslyQueued)
                // 尝试执行 Task
                if (TryDequeue(task))
                    return TryExecuteTask(task);
                else
                    return false;
            else
                return TryExecuteTask(task);
        }

        protected bool TryExecuteTask(IThreadTask task)
        {
            if(task == null)
            {
                return false;
            }
            task.Invoke();
            return true;
        }

        // 尝试将已调度的 Task 移出调度队列
        protected bool TryDequeue(IThreadTask task)
        {
            lock (_tasks) return _tasks.Remove(task);
        }

        // 获取最大并发数
        public int MaximumConcurrencyLevel { get { return _maxDegreeOfParallelism; } }

        // 获取已调度任务队列迭代器
        protected IEnumerable<IThreadTask> GetScheduledTasks()
        {
            bool lockTaken = false;
            try
            {
                // Monitor.TryEnter 作用为线程锁，其语法糖为 lock (_tasks)
                lockTaken = Monitor.TryEnter(_tasks);
                if (lockTaken) return _tasks;
                else throw new NotSupportedException();
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_tasks);
            }
        }
    }

    public class TaskQueue
    {
        //持有的调度器
        private LimitedConcurrencyTaskScheduler _scheduler;

        public TaskQueue(int concurrentCount, bool isLoopThread, int threadSleepWhenEmptyTask)
        {
            _scheduler = new LimitedConcurrencyTaskScheduler(concurrentCount, isLoopThread, threadSleepWhenEmptyTask);
        }

        //执行
        public void Run(IThreadTask task)
        {
            task.Invoke(_scheduler);
        }

        public void Destroy()
        {
            _scheduler.Destroy();
        }
    }

        #region Tasks
        /// <summary>
        /// A thread task that takes no parameters.
        /// </summary>
    public class ThreadTask : IThreadTask
    {
        System.Action action;
        int frame;

        public int creationFrame
        {
            get
            {
                return frame;
            }
        }

        public ThreadTask(System.Action _action)
        {
            action = _action;

            if (ThreadMgrOptimize.inUnityThread)
            {
                frame = Time.frameCount;
            }
        }

        public void Invoke()
        {
            action();
        }

        public void Invoke(LimitedConcurrencyTaskScheduler scheduler)
        {
            scheduler.QueueTask(this);
        }
    }
    /// <summary>
    /// A thread task that takes 1 parameter.
    /// <typeparam name="T">Type 1</typeparam>
    /// </summary>
    public class ThreadTask<T> : IThreadTask
    {
        System.Action<T> action;
        T data;
        int frame;

        System.Action<ThreadTask<T>> onDone;

        public int creationFrame
        {
            get
            {
                return frame;
            }
        }
        
        public ThreadTask(System.Action<T> _action, T _data)
        {
            action = _action;
            data = _data;

            if (ThreadMgrOptimize.inUnityThread)
            {
                frame = Time.frameCount;
            }
        }

        public void Invoke()
        {
            action(data);
        }

        public void Invoke(LimitedConcurrencyTaskScheduler scheduler)
        {
            scheduler.QueueTask(this);
        }
    }
    /// <summary>
    /// A thread task that takes 2 parameters.
    /// <typeparam name="T">Type 1</typeparam>
    /// <typeparam name="T1">Type 2</typeparam>
    /// </summary>
    public class ThreadTask<T, T1> : IThreadTask
    {
        System.Action<T, T1> action;
        T data1;
        T1 data2;
        int frame;

        public int creationFrame
        {
            get
            {
                return frame;
            }
        }

        public ThreadTask(System.Action<T, T1> _action, T _data1, T1 _data2)
        {
            action = _action;
            data1 = _data1;
            data2 = _data2;

            if (ThreadMgrOptimize.inUnityThread)
            {
                frame = Time.frameCount;
            }
        }

        public void Invoke()
        {
            action(data1, data2);
        }

        public void Invoke(LimitedConcurrencyTaskScheduler scheduler)
        {
            scheduler.QueueTask(this);
        }
    }
    /// <summary>
    /// A thread task that takes 3 parameters.
    /// <typeparam name="T">Type 1</typeparam>
    /// <typeparam name="T1">Type 2</typeparam>
    /// <typeparam name="T2">Type 3</typeparam>
    /// </summary>
    public class ThreadTask<T, T1, T2> : IThreadTask
    {
        System.Action<T, T1, T2> action;
        T data1;
        T1 data2;
        T2 data3;
        int frame;

        public int creationFrame
        {
            get
            {
                return frame;
            }
        }

        public ThreadTask(System.Action<T, T1, T2> _action, T _data1, T1 _data2, T2 _data3)
        {
            action = _action;
            data1 = _data1;
            data2 = _data2;
            data3 = _data3;

            if (ThreadMgrOptimize.inUnityThread)
            {
                frame = Time.frameCount;
            }
        }

        public void Invoke()
        {
            action(data1, data2, data3);
        }

        public void Invoke(LimitedConcurrencyTaskScheduler scheduler)
        {
            scheduler.QueueTask(this);
        }
    }
    /// <summary>
    /// A thread task that takes 4 parameters.
    /// <typeparam name="T">Type 1</typeparam>
    /// <typeparam name="T1">Type 2</typeparam>
    /// <typeparam name="T2">Type 3</typeparam>
    /// <typeparam name="T3">Type 4</typeparam>
    /// </summary>
    public class ThreadTask<T, T1, T2, T3> : IThreadTask
    {
        System.Action<T, T1, T2, T3> action;

        T data1;
        T1 data2;
        T2 data3;
        T3 data4;
        int frame;

        public int creationFrame
        {
            get
            {
                return frame;
            }
        }

        public ThreadTask(System.Action<T, T1, T2, T3> _action, T _data1, T1 _data2, T2 _data3, T3 _data4)
        {
            action = _action;
            data1 = _data1;
            data2 = _data2;
            data3 = _data3;
            data4 = _data4;

            if (ThreadMgrOptimize.inUnityThread)
            {
                frame = Time.frameCount;
            }
        }

        public void Invoke()
        {
            action(data1, data2, data3, data4);
        }

        public void Invoke(LimitedConcurrencyTaskScheduler scheduler)
        {
            scheduler.QueueTask(this);
        }
    }

    /// <summary>
    /// A thread task interface.
    /// Implement on any customely created thread task.
    /// </summary>
    public interface IThreadTask
    {
        void Invoke();
        void Invoke(LimitedConcurrencyTaskScheduler scheduler);
        int creationFrame { get; }
    }
    #endregion

}