using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;
using System.Reflection;
using static UnityEngine.Application;
using System.Collections;
using System.Collections.Concurrent;
#if UNITY_EDITOR
using UnityEditor;
#endif
/// <summary>
/// 日志管理
/// </summary>
public class Log
{
    public static LogLevel showLogLevel = LogLevel.None;    //显示日志的级别
    public static LogLevel fileLogLevel = LogLevel.Exception;    //写入文件日志级别
    public static bool isLogGameState = false;               //是否记录游戏状态
    public static bool isLogNetworkMsg = true;              //是否记录网络日志
    public static Action<string, string> OnLogErrorEvent;

    const string INFO_COLOR = "#909090";
    const string WARNING_COLOR = "orange";
    const string ERROR_COLOR = "red";

    public enum LogLevel : byte
    {
        None = 0,
        Warning = 1,
        Info = 2,
        Exception = 3,
        Error = 4,
    }

    static string[] LOG_COLORS = new string[]
    {
        INFO_COLOR,
        WARNING_COLOR,
        INFO_COLOR,
        ERROR_COLOR,
        ERROR_COLOR
    };

    //static bool _is_init = false;
#if UNITY_EDITOR
    static WeakReference _logcallback;
    public static void CheckInit() 
    {
        if (_logcallback == null || !_logcallback.IsAlive)
        {
            Init();
        }
    }
#endif

    static object[] reports = new object[10];
    static long[] reports_times = new long[10];
    static int repoerts_idx = 0;
    public static string LastError;
    public static void Init()
    {
        //Debug.Log($"Log.Init");
        //_is_init = true;
        Application.lowMemory += () =>
        {
            Log.LogWarning($"lowMemory");
        };
        AppDomain.CurrentDomain.UnhandledException += (sender, args) => 
        {
            var e = args?.ExceptionObject as Exception;
            if (e != null)
            {
                Log.LogError($"sender={sender}, {e.GetType()}:{e.Message}\n{e.StackTrace}");
            }
        };
        //
        Application.SetStackTraceLogType(LogType.Error, StackTraceLogType.ScriptOnly);
        Application.SetStackTraceLogType(LogType.Assert, StackTraceLogType.ScriptOnly);
        Application.SetStackTraceLogType(LogType.Exception, StackTraceLogType.ScriptOnly);
        if (Application.isEditor || Debug.isDebugBuild)
        {
            Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.ScriptOnly);
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.ScriptOnly);
        }
        else
        {
            Application.SetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
            Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
        }

        LogCallback callback = (string condition, string stackTrace, LogType type) =>
        {
            if (type == LogType.Log || string.IsNullOrEmpty(condition))
            {
                return;
            }
#if UNITY_EDITOR
            if (condition.StartsWith("GUID [") && condition.IndexOf(" conflicts ") > 0) 
            { 
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    EditorUtility.DisplayDialog("GUID冲突", condition, "确定");
                };
            }
#endif            
            if (type != LogType.Warning || (condition[0] != '[' && condition[0] != '<'))
            {
                int idx = -1;
                if (string.IsNullOrEmpty(stackTrace) || (idx = stackTrace.IndexOf('\n')) < 0 || idx == stackTrace.Length - 1)
                {
                    if (Application.isEditor)
                    {
                        stackTrace = "new StackTrace(2, true) -> \n" + new StackTrace(2, true).ToString();
                    }
                    else
                    {
                        stackTrace = "new StackTrace(7, true) -> \n" + new StackTrace(7, true).ToString();
                    }
                }
            }

            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            {
                var old = condition;
                if (OnLogErrorEvent != null)
                {
                    if (!string.IsNullOrEmpty(old) && (old[0] == '[' || old[0] == '<'))
                    {
                        var subidx = old.IndexOf(") ");
                        if (subidx > 0)
                        {
                            old = old.Substring(subidx + 1);
                        }
                    }
                }
                //
                condition += $"\nstackTrace={stackTrace}\n\n";
                LastError = condition;
                //
                if (OnLogErrorEvent != null && condition.Length < 10 * 1024)
                {
                    var idx = Array.IndexOf(reports, old);
                    var time0 = reports_times[repoerts_idx];
                    var time1 = System.DateTime.UtcNow.Ticks / 10000000;
                    reports_times[repoerts_idx] = time1;
                    reports[repoerts_idx] = old;
                    if (time1 - time0 >= 10)//控制上报频率，10秒内最多上传 reports_times.Length
                    {
                        repoerts_idx = (repoerts_idx + 1) % reports_times.Length;
                        if (idx < 0)
                        {
                            var ht = new Hashtable()
                            {
                                ["userId"] = report_uid,
                                ["roleId"] = report_cid,
                                ["serverId"] = report_serverid,
                                ["msg"] = condition,
                            };
                            var log = MiniJSON.JsonEncode(ht);
                            //Debug.Log("xxxx do report");
                            //报告服务器
                            if (System.Threading.Thread.CurrentThread.ManagedThreadId == _mainthreadid)
                            {
                                OnLogErrorEvent.Invoke(log, "");
                            }
                            else 
                            {
                                delay_reports.Enqueue(log);
                            }                            
                        }
                        else 
                        {
                            //Debug.Log("xxxx had reported");
                        }
                    }
                    else
                    {
                        //Debug.Log("xxxx too fast");
                    }
                }
            }
            //var c = condition[0];
            Log2File(condition, type);
        };

        //Application.logMessageReceived += callback;
        Application.logMessageReceivedThreaded -= callback;
        Application.logMessageReceivedThreaded += callback;
        
#if UNITY_EDITOR
        _logcallback = new WeakReference(callback);
#endif

    }

    public static string report_uid = "0", report_cid = "0", report_serverid = "0", report_level = "0", report_scene_id="0";
    static ConcurrentQueue<string> delay_reports = new ConcurrentQueue<string>();
    public static void CheckUplaodLog() 
    {
        if (delay_reports.TryDequeue(out var condition))
        {
            OnLogErrorEvent.Invoke(condition, "");
        }
    }


    #region 日志接口
    //-----------------------------------------------------------------------------//
    // 普通信息输出
    [Conditional("ENABLE_PROFILER")]
    public static void LogInfo(string fmt, params object[] args)
    {
        if (args.Length == 0)
            LogLevelFormat(2, LogLevel.Info, fmt);
        else
            LogLevelFormat(2, LogLevel.Info, string.Format(fmt, args));
    }
    [Conditional("ENABLE_PROFILER")]
    public static void LogInfo(string fmt)
    {
        LogLevelFormat(2, LogLevel.Info, fmt);
    }
    // 输出 警告
    public static void LogWarning(string fmt, params object[] args)
    {
        if (args.Length == 0)
            LogLevelFormat(2, LogLevel.Warning, fmt);
        else
            LogLevelFormat(2, LogLevel.Warning, string.Format(fmt, args));
    }
    public static void LogWarning(string fmt)
    {
        LogLevelFormat(2, LogLevel.Warning, fmt);
    }

    // 输出错误
    public static void LogError(string fmt, params object[] args)
    {
        if (args.Length == 0)
            LogLevelFormat(2, LogLevel.Error, fmt);
        else
            LogLevelFormat(2, LogLevel.Error, string.Format(fmt, args));
    }
    public static void LogError(string fmt)
    {
        LogLevelFormat(2, LogLevel.Error, fmt);
    }

    //-----------------------------------------------------------------------------//
    //带调用者
    //-----------------------------------------------------------------------------//

    // 输出 警告
    public static void LogWarningWithObj(string sender, string fmt, params object[] args)
    {
        LogLevelFormat(3, LogLevel.Warning, string.Format(fmt, args), sender);
    }

    public static void LogExceptionWithObj(string sender, string fmt, params object[] args)
    {
        LogLevelFormat(4, LogLevel.Exception, string.Format(fmt, args), sender);
    }

    // 输出错误
    public static void LogErrorWithObj(string sender, string fmt, params object[] args)
    {
        LogLevelFormat(3, LogLevel.Error, string.Format(fmt, args), sender);
    }
    #endregion

    public static long ServerMsDT = 0;
    static string[] _levelFormats = new string[10];
    #region 日志输出
    /// <summary>
    /// 输出日志和记录到文件
    /// </summary>
    /// <param name="p_level">日志级别</param>
    /// <param name="p_message">日志消息</param>
    /// <param name="p_sender">发送者</param>
    private static void LogLevelFormat(int p_stackDeep, LogLevel p_level, string p_message, string p_sender = "")
    {
        if (string.IsNullOrEmpty(p_message)) 
        {
            return;
        }

#if UNITY_EDITOR
        CheckInit();
#endif

        string _level = _levelFormats[(int)p_level];
        if (_level == null)
        {
            _level = p_level.ToString().ToUpper().Substring(0, 4);
            _levelFormats[(int)p_level] = _level;
        }

        DateTime now = DateTime.Now;
        DateTime servnow = DateTime.UtcNow.AddMilliseconds(ServerMsDT);

        string _showLogString = null;
        //在console中显示日志
        if (p_level >= showLogLevel)
        {
            var m2 = GetMonoMemInMB();
#if UNITY_EDITOR
            string fmt = "<color=" + LOG_COLORS[(int)p_level % 5] + ">[{0}]</color> {1}'{2:D3}({3}'{4:D3})({5},{6},{7}) {8}{9}";
#else
            string fmt = "[{0}] {1}'{2:D3}({3}'{4:D3})({5},{6},{7}) {8}{9}";
#endif
            var enter = p_message[p_message.Length - 1] == '\n' ? "" : "\n";
            var threadid = System.Threading.Thread.CurrentThread.ManagedThreadId;
            _showLogString = string.Format(fmt, _level, now.ToLongTimeString(), now.Millisecond, servnow.ToLongTimeString(), servnow.Millisecond, threadid, m2, TotalFrame, p_message, enter);

            switch (p_level)
            {
                case LogLevel.Exception:
                case LogLevel.Error:
                    Debug.LogError(_showLogString);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(_showLogString);
                    break;
                default:
                    if (!CloseLog)
                    {
                        Debug.Log(_showLogString);
                    }
                    break;
            }
        }
    }
    #endregion

    #region 网络日志

    //    static MaxSizeList<string> _net_logs = new MaxSizeList<string>(4096);

    //    //
    //    public static List<string> NetLogList
    //    {
    //        get { return _net_logs.List; }
    //    }

    //    // 添加网络收发日志
    //    public static void AddNetLog(string fmt, params object[] args)
    //    {
    //        if (IsLogNetworkMsg)
    //        {
    //            var str = args.Length == 0 ? fmt : string.Format(fmt, args);
    //            str = DateTime.Now.ToLongTimeString() + " " + str;


    //#if UNITY_IPHONE
    //            Console.WriteLine(str);
    //#else
    //#if UNITY_EDITOR
    //            Debug.Log(str);
    //#endif
    //#endif
    //            {

    //                _net_logs.Add(str);
    //            }


    //        }

    //    }

    #endregion

    #region 记录到文件

    //static string _log_file_url;    
    static byte[] logs_w = new byte[256 * 1024];
    static byte[] logs_r = new byte[logs_w.Length];
    static FileStream log_file_fd;
    public volatile static int CacheLogsLength, CacheLogsLength2;
    public static int CacheLogsLengthMax = logs_w.Length;
    /// <summary>
    /// 初始化日志路径
    /// </summary>
    static void InitLog2File()
    {
        if (log_file_fd != null) return;
        lock (logs_w)
        {
            if (log_file_fd == null)
            {
                var logpath = "";
                if (Application.isEditor)
                {
                    logpath = "logs";
                }
                else
                {
#if UNITY_STANDALONE
                    //string fullpath = System.Windows.Forms.Application.ExecutablePath;
                    string fullpath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                    var name = Path.GetFileNameWithoutExtension(fullpath);
                    logpath = Path.GetFullPath(Application.dataPath + "/../../logs_" + name + "/").Replace("\\", "/");
#else
                    logpath = Path.Combine(Application.temporaryCachePath, "logs/");
#endif
                }

                // 获取日志目录

                if (!Directory.Exists(logpath)) Directory.CreateDirectory(logpath);

                try
                {
                    Func<int, string> getName = (id) =>
                        {
                            return Path.Combine(logpath, string.Format("log{0}.txt", id));
                        };

                    // 保留最近10个日志文件
                    var name9 = getName(9);
                    if (File.Exists(name9)) File.Delete(name9);
                    for (int i = 8; i >= 0; i--)
                    {
                        var name1 = getName(i);
                        if (File.Exists(name1))
                        {
                            var name2 = getName(i + 1);
                            File.Move(name1, name2);
                        }
                    }

                    log_file_fd = new FileStream(getName(0), FileMode.Append);
                    logs_w[CacheLogsLength++] = 0xef;
                    logs_w[CacheLogsLength++] = 0xbb;
                    logs_w[CacheLogsLength++] = 0xbf;
                }
                catch (Exception e)
                {
                    Debug.Log($"{e.GetType().Name}:{e.Message}\n{e.StackTrace}");
                }
            }
        }
    }

    static int _mainthreadid = System.Threading.Thread.CurrentThread.ManagedThreadId;

    public static int GetMonoMemInMB()
    {
        var ismain = _mainthreadid == System.Threading.Thread.CurrentThread.ManagedThreadId;
        var m2 = (ismain ? UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong() : GC.GetTotalMemory(false)) / 1024 / 1024;
        return (int)m2;
    }

    public static long GetMonoMemInKB()
    {
        var m2 = UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong() / 1024;
        return m2;
    }

    public static int GetMonoMemInMB2()
    {
        var mb = UnityEngine.Profiling.Profiler.GetMonoHeapSizeLong() / 1024 / 1024;
        return (int)mb;
    }

    public static bool CloseLog = false;
    //static StringBuilder sb = new StringBuilder();
    static string[] _log_level_names = new string[8];

    static bool is_log_info = Application.GetStackTraceLogType(LogType.Log) != StackTraceLogType.None;
    public static long TotalFrame = 0;
    /// <summary>
    /// 将日志记录到文件
    /// </summary>
    /// <param name="msg"></param>
    public static void Log2File(string msg, LogType callback = LogType.Log)
    {

#if UNITY_EDITOR
        CheckInit();
#endif

        if (Application.platform == RuntimePlatform.WebGLPlayer || string.IsNullOrEmpty(msg))
        {
            return;
        }

        try
        {
            if (log_file_fd == null)
            {
                InitLog2File();
            }

            var is_callback = callback != LogType.Log;
            if (!is_callback || (msg[0] != '[' && msg[0] != '<'))
            {
                DateTime now = DateTime.Now;
                DateTime servnow = DateTime.UtcNow.AddMilliseconds(ServerMsDT);
                var m2 = GetMonoMemInMB();
                var logname = is_callback ? (_log_level_names[(int)callback] ?? (_log_level_names[(int)callback] = callback.ToString())) : "FILE";
                var threadid = System.Threading.Thread.CurrentThread.ManagedThreadId;
                //var space = _mainthreadid == threadid ? ' ' : '+';
                var enter = msg[msg.Length - 1] == '\n' ? "" : "\n";
                msg = string.Format("[{0}] {1}'{2:D3}({3}'{4:D3})({5},{6},{7}) {8}{9}", logname, now.ToLongTimeString(), now.Millisecond, servnow.ToLongTimeString(), servnow.Millisecond, threadid, m2, TotalFrame, msg, enter);
            }

            if (!is_callback && is_log_info)
            {
                if (!CloseLog)
                {
                    Debug.Log(msg);
                }
            }

            var need = Encoding.UTF8.GetByteCount(msg);
            lock (logs_w)
            {                
                if (!ApplicationQuit || CacheLogsLength > 0)
                {
                    if (CacheLogsLength + need < CacheLogsLengthMax)
                    {
                        var need2 = Encoding.UTF8.GetBytes(msg, 0, msg.Length, logs_w, CacheLogsLength);
                        CacheLogsLength += need2;
                        //Debug.Log($"{System.Threading.Thread.CurrentThread.ManagedThreadId} CacheLogsLength + {need}/{need2} -> {CacheLogsLength}");
                    }
                    else
                    {
                        //Debug.Log($"{System.Threading.Thread.CurrentThread.ManagedThreadId} 1flush CacheLogsLength={CacheLogsLength}");
                        if (CacheLogsLength > 0)
                        {
                            while (CacheLogsLength2 > 0)
                            {
                                System.Threading.Thread.Sleep(1);
                                Debug.Log($"---------------------------------------- waiting Flush2File {CacheLogsLength2}--------------------------------------------------");
                                --CacheLogsLength2;
                            }
                            //
                            lock (log_file_fd)
                            {
                                log_file_fd.Write(logs_w, 0, CacheLogsLength);
                            }
                            CacheLogsLength = 0;
                        }
                        if (need < CacheLogsLengthMax)
                        {
                            CacheLogsLength = Encoding.UTF8.GetBytes(msg, 0, msg.Length, logs_w, 0);
                            //Debug.Log($"{System.Threading.Thread.CurrentThread.ManagedThreadId} CacheLogsLength = {need}/{CacheLogsLength}");
                        }
                        else
                        {
                            //File.AppendAllText(log_file_url, msg, Encoding.UTF8);
                            Debug.Log($"{System.Threading.Thread.CurrentThread.ManagedThreadId} skip1 log {need}");
                        }
                    }
                }
                else
                {
                    //File.AppendAllText(log_file_url, msg, Encoding.UTF8);
                    if (need < CacheLogsLengthMax)
                    {
                        need = Encoding.UTF8.GetBytes(msg, 0, msg.Length, logs_w, 0);
                        //Debug.Log($"{System.Threading.Thread.CurrentThread.ManagedThreadId} 3flush {need}");
                        lock (log_file_fd)
                        {
                            log_file_fd.Write(logs_w, 0, need);
                            log_file_fd.Flush();
                        }
                    }
                    else
                    {
                        Debug.Log($"{System.Threading.Thread.CurrentThread.ManagedThreadId} skip2 log {need}");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log($"{e.Message}\n{e.StackTrace}");
        }
    }


    static bool ApplicationQuit = false;
    public static void Flush2File(bool quit)
    {
        ApplicationQuit = quit;
        var log_file_fd = Log.log_file_fd;
        if (log_file_fd != null)
        {
            byte[] logs = null;
            int logs_len = 0;
            lock (logs_w)
            {
                if (CacheLogsLength > 0)
                {
                    logs_len = CacheLogsLength2 = CacheLogsLength;
                    logs = logs_w;
                    logs_w = logs_r;
                    logs_r = logs;
                    CacheLogsLength = 0;               
                }
            }
            //
            if (logs != null)
            {
                lock (log_file_fd)
                {
                    log_file_fd.Write(logs, 0, logs_len);
                    log_file_fd.Flush();
                    logs_len = CacheLogsLength2 = 0;
                }
            }
        }
    }
    #endregion

    #region 双击跳转到调用代码
#if UNITY_EDITOR
    private static int s_InstanceID;
    //private static List<StackFrame> s_LogStackFrameList = new List<StackFrame>();
    //ConsoleWindow  
    private static EditorWindow s_ConsoleWindow;
    private static FieldInfo m_ActiveText;
    //private static Type consoleWindowType;
    //private static object s_LogListView;
    //private static FieldInfo s_LogListViewTotalRows;
    //private static FieldInfo s_LogListViewCurrentRow;
    //LogEntry  
    //private static MethodInfo s_LogEntriesGetEntry;
    //private static object s_LogEntry;
    //instanceId 非UnityEngine.Object的运行时 InstanceID 为零所以只能用 LogEntry.Condition 判断  
    //private static FieldInfo s_LogEntryInstanceId;
    //private static FieldInfo s_LogEntryLine;
    //private static FieldInfo s_LogEntryCondition;
    static Log()
    {
        //Type t = typeof(Log);
        s_InstanceID = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/Plugins/Log.cs").GetInstanceID();
        //s_LogStackFrameList.Clear();
        // GetConsoleWindowListView();
    }

    private static void GetConsoleWindowListView()
    {
        if (s_ConsoleWindow == null)
        {
            Assembly unityEditorAssembly = Assembly.GetAssembly(typeof(EditorWindow));
            var consoleWindowType = unityEditorAssembly.GetType("UnityEditor.ConsoleWindow");
            FieldInfo fieldInfo = consoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
            s_ConsoleWindow = fieldInfo.GetValue(null) as EditorWindow;
            m_ActiveText = consoleWindowType.GetField("m_ActiveText", BindingFlags.NonPublic | BindingFlags.Instance);
            //FieldInfo listViewFieldInfo = consoleWindowType.GetField("m_ListView", BindingFlags.Instance | BindingFlags.NonPublic);
            //s_LogListView = listViewFieldInfo.GetValue(s_ConsoleWindow);
            //s_LogListViewTotalRows = listViewFieldInfo.FieldType.GetField("totalRows", BindingFlags.Instance | BindingFlags.Public);
            //s_LogListViewCurrentRow = listViewFieldInfo.FieldType.GetField("row", BindingFlags.Instance | BindingFlags.Public);
            //LogEntries  
            //Type logEntriesType = unityEditorAssembly.GetType("UnityEditor.LogEntries");
            //s_LogEntriesGetEntry = logEntriesType.GetMethod("GetEntryInternal", BindingFlags.Static | BindingFlags.Public);
            //Type logEntryType = unityEditorAssembly.GetType("UnityEditor.LogEntry");
            //s_LogEntry = Activator.CreateInstance(logEntryType);
            //s_LogEntryInstanceId = logEntryType.GetField("instanceID", BindingFlags.Instance | BindingFlags.Public);
            //s_LogEntryLine = logEntryType.GetField("line", BindingFlags.Instance | BindingFlags.Public);
            //s_LogEntryCondition = logEntryType.GetField("message", BindingFlags.Instance | BindingFlags.Public);
        }
    }

    [UnityEditor.Callbacks.OnOpenAssetAttribute(0)]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        if (Application.isPlaying && instanceID == s_InstanceID)
        {
            GetConsoleWindowListView();
            if (UnityEditor.EditorWindow.focusedWindow == s_ConsoleWindow)
            {
                //Debug.LogError(string.Format("{0}   {1}", instanceID, line));            
                var message = m_ActiveText.GetValue(s_ConsoleWindow) as string;
                //Debug.Log($"message={message}");
                if (message != null)
                {
                    var flag = "PingObjectID:<";
                    var ping = message.IndexOf(flag);
                    if (ping >= 0)
                    {
                        ping += flag.Length;
                        var pingend = message.IndexOf('>', ping);
                        var id = message.Substring(ping, pingend - ping);
                        var go = UnityEditor.EditorUtility.InstanceIDToObject(int.Parse(id));
                        if (go)
                        {
                            UnityEditor.EditorGUIUtility.PingObject(go);
                            return true;
                        }
                    }
                }
            }
        }
        return false;
    }
#endif
    #endregion
}



namespace UnityEngine.Profiling
{
    public sealed class ProfilerX
    {

        public static Stack<string> samples = null;
        public static List<string> all_samples = null;


        [Conditional("ENABLE_PROFILER")]
        public static void BeginSample(string name)
        {
            if (samples != null) 
            {
                samples.Push(name);
                var sf = new StackFrame(1, true);
                all_samples.Add($"+{samples.Count} + {name} + {sf.GetMethod().Name} at {sf.GetFileName()}:{sf.GetFileLineNumber()}");
            }
            UnityEngine.Profiling.Profiler.BeginSample(name);
        }

        [Conditional("ENABLE_PROFILER")]
        public static void EndSample()
        {
            if (samples != null)
            {
                var sf = new StackFrame(1, true);
                //var sf = new StackTrace(true).GetFrame(1);                
                if (samples.Count > 0)
                {
                    var name = samples.Pop();
                    all_samples.Add($"-{samples.Count} - {name} - {sf.GetMethod().Name} at {sf.GetFileName()}:{sf.GetFileLineNumber()}");
                }
                else 
                {
                    Log.LogError(all_samples[all_samples.Count - 1]);
                }
            }
            UnityEngine.Profiling.Profiler.EndSample();
            //last_name = null;
        }

        [Conditional("ENABLE_PROFILER")]
        public static void Clear()
        {
            if (samples == null)
            {
                samples = new Stack<string>();
                all_samples = new List<string>();
            }
            else
            {
                samples.Clear();
                all_samples.Clear();
            }
        }

        class MemNode
        {
            public MemNode(string name, MemNode parent)
            {
                this.name = name;
                this.parent = parent;
            }
            public string name;
            public int cnt;
            public long bytes;

            public int _gc;
            public long _before;

            public MemNode parent;
            public List<MemNode> childs;

        }

        static MemNode _root = new MemNode("_root", null);
        static MemNode _cur = null;
        static MemNode _last_debug = null;
        public static void BeginSampleMem(string name)
        {
            if (_cur == null)
            {
                _cur = _root;
            }
            if (_cur.childs == null)
            {
                _cur.childs = new List<MemNode>();
            }
            MemNode child = null;
            foreach (var c in _cur.childs)
            {
                if (c.name == name)
                {
                    child = c;
                    break;
                }
            }
            if (child == null)
            {
                child = new MemNode(name, _cur);
                _cur.childs.Add(child);
            }
            _cur = child;
            _cur._gc = GC.CollectionCount(0);
            _cur._before = GC.GetTotalMemory(false);

        }
        public static void EndSampleMem()
        {
            if (_cur == null)
            {
                Log.LogError($"_last_debug={_last_debug?.name}");
                return;
            }

            _last_debug = _cur;

            if (_cur._gc == GC.CollectionCount(0))
            {
                var bytes = GC.GetTotalMemory(false) - _cur._before;
                if (bytes >= 0)
                {
                    ++_cur.cnt;
                    _cur.bytes += bytes;
                }
            }
            _cur = _cur.parent;
        }
    }
}

