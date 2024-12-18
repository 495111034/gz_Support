using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

public class PluginApiTools
{

	public class Callback : AndroidJavaProxy
	{
		public Callback()
			: base("com.game.apitools.CompletionReceiver$Callback")
		{ }

		void downloadCompleted(string data)
		{
			Log.Log2File($"downloadCompleted, data={data}");
		}
	}


	private static AndroidJavaObject _javaObject = null;
	public static AndroidJavaObject javaObject
	{
		get
		{
			if (_javaObject == null)
			{
				AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
				AndroidJavaObject unityObj = unity.GetStatic<AndroidJavaObject>("currentActivity");
				AndroidJavaClass launcher = new AndroidJavaClass("com.game.apitools.helper");
				_javaObject = launcher.CallStatic<AndroidJavaObject>("Instance");
				_javaObject.Call("Init", unityObj, "PluginApiTools");
			}
			return _javaObject;
		}
	}

	private static AndroidJavaObject _javaDownLoadObject = null;
	public static AndroidJavaObject javaDownLoadObject
	{
		get
		{
			if (_javaDownLoadObject == null)
			{
				AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
				AndroidJavaObject unityObj = unity.GetStatic<AndroidJavaObject>("currentActivity");
				AndroidJavaClass launcher = new AndroidJavaClass("com.game.downloadservice.downloadhelper");
				_javaDownLoadObject = launcher.CallStatic<AndroidJavaObject>("Instance");
				_javaDownLoadObject.Call("Init", unityObj, "PluginApiTools");
			}
			return _javaDownLoadObject;
		}
	}

#if UNITY_EDITOR
	/// <summary>
	/// 重启app
	/// </summary>
	public static void RestartAndroidApp()
	{
		UnityEditor.EditorApplication.isPlaying = false;
		////记录主线程
		//SynchronizationContext sc = SynchronizationContext.Current;

		//Thread t = new Thread(() =>
		//{
		//	Thread.Sleep(500);

		//	//传递到主线程调用isPlaying
		//	sc.Post((object o) =>
		//	{
		//		UnityEditor.EditorApplication.isPlaying = true;
		//	}, null);
		//});
		//t.Start();
	}

	/// <summary>
	/// 退出app
	/// </summary>
	public static void QuitAndroidApp()
	{
		UnityEditor.EditorApplication.isPlaying = false;
	}
#elif UNITY_ANDROID
	/// <summary>
	/// 重启app
	/// </summary>
	public static void RestartAndroidApp()
	{
		javaObject.Call("RestartAndroidApp");
	}

	/// <summary>
	/// 退出app
	/// </summary>
	public static void QuitAndroidApp()
	{
		// 调用Android原生代码
        AndroidJavaObject activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer")
                                        .GetStatic<AndroidJavaObject>("currentActivity");
        activity.Call("finish");

        // Kill the process
        AndroidJavaClass process = new AndroidJavaClass("android.os.Process");
        int pid = process.CallStatic<int>("myPid");
        process.CallStatic("killProcess", pid);

        // Exit the application using System.exit(0);
        AndroidJavaClass system = new AndroidJavaClass("java.lang.System");
        system.CallStatic("exit", 0);
	}
#else
	public static void RestartAndroidApp()
	{
		Application.Quit();
	}

	/// <summary>
	/// 退出app
	/// </summary>
	public static void QuitAndroidApp()
	{
		Application.Quit();
	}
#endif

	public static bool IsEmulator()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		try
		{
			return javaObject.Call<bool>("IsEmulator");
		}
		catch (System.Exception e)
		{
			Debug.Log(e.ToString());
		}
#endif

		return false;
	}

	private static readonly Regex _regnum = new Regex(@"[0-9]+", RegexOptions.Multiline);
	public static string GetCPU()
	{
		string cpu = SystemInfo.processorType;
#if UNITY_ANDROID && !UNITY_EDITOR
		try
		{
			cpu = javaObject.Call<string>("GetCPU");
		}
		catch (System.Exception e)
		{
			Debug.Log(e.ToString());
		}
#endif
		//没有数字 尝试使用 cpuinfo
		if (!_regnum.IsMatch(cpu))
		{
			try
			{
				var file = "/proc/cpuinfo";
				if (System.IO.File.Exists(file))
				{
					var lines = System.IO.File.ReadAllLines(file);
					foreach (var line in lines)
					{
						if (line.StartsWith("Hardware", System.StringComparison.CurrentCultureIgnoreCase) ||
							line.StartsWith("model name", System.StringComparison.CurrentCultureIgnoreCase))
						{
							var splite = line.Split(':', 2);
							return splite[splite.Length - 1].Trim();
						}
					}
				}
			}
			catch { }
		}
		return cpu;
	}

	/// <summary>
	/// 通知栏显示下载进度, 参数text可以这样写：下载中: 10%
	/// </summary>
	/// <param name="title"></param>
	/// <param name="text"></param>
	/// <param name="progressCurrent"></param>
	/// <param name="progressMax"></param>
	public static void ProgressNotify(string title, string text, int progressCurrent, int progressMax)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		javaObject.Call("ProgressNotify", title, text, progressCurrent, progressMax);
#endif
	}

	/// <summary>
	/// 取消通知栏下载进度显示
	/// </summary>
	public static void CancelProgressNotify()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		javaObject.Call("CancelProgressNotify");
#endif
	}

	public static string GetNetworkTypeName() 
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		return javaObject.Call<string>("GetNetworkTypeName");
#endif
		return "WIFI/Editor";
	}

	public static System.Net.NetworkInformation.NetworkInterface GetNetworkInterface() 
	{
		System.Net.NetworkInformation.NetworkInterface net = null;
		try
		{
			var nets = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();
			net = nets != null && nets.Length > 0 ? nets[0] : null;
		}
		catch { }
		return net;
	}

	public static void StartDownload( string url, string filePath) 
	{
		javaObject.Call("StartDownload", url, filePath);
	}

	public static void SetDownloadCallback(Callback callback)
	{
		//javaObject.Call("SetDownloadCallback", callback);
	}

	public static void CheckDownloads() 
	{
		//javaObject.Call("CheckDownloads");
	}



	public static void SetReportURL(string url) 
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		javaObject.Call("SetReportURL", url);
#endif
	}

	//static object[] parambody = new object[1];
	public static void StartReport(string body)
	{
		//parambody[0] = body;
		javaObject.Call("StartReport", body);
	}
	public static void CheckReports() 
	{
		javaObject.Call("CheckReports");
	}

	public static string GetInternalStorageSpace()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		return javaObject.Call<string>("getInternalStorageSpace", "/storage/emulated");
#endif
		return "GetInternalStorageSpace not support";
	}
}

