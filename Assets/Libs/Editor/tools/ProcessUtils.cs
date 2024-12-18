

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public static class ProcessUtils
{
    /// <summary>
    /// 执行python脚本
    /// </summary>
    /// <param name="command"> 脚本路径+参数 </param>
    public static void ExecPython(string command)
    {
        Process p = new Process();
        p.StartInfo.FileName = "cmd";
        p.StartInfo.UseShellExecute = false;     //这句是关键，可以让运行结果不显示在cmd窗口上。
        p.StartInfo.CreateNoWindow = true;   //执行不显示窗体
        p.StartInfo.Arguments = " /c " + command;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.RedirectStandardInput = true;
        p.StartInfo.RedirectStandardOutput = true;
        UnityEngine.Debug.Log(command);
        p.Start();
        var output = p.StandardOutput.ReadToEnd();
        if (output.Contains("error,"))
        {
            Log.LogError(output);
        }
        else 
        {
            Log.LogInfo(output);
        }
        p.Close();
        //
    }

    public static string ExecSystemComm(string command)
    {
        if (command.StartsWith("svn ci"))
        {
            //return "";
        }
        var t1 = System.DateTime.UtcNow.Ticks;
        Process p = new Process();
        p.StartInfo.FileName = "cmd";
        p.StartInfo.UseShellExecute = false;     //这句是关键，可以让运行结果不显示在cmd窗口上。
        p.StartInfo.CreateNoWindow = true;   //执行不显示窗体
        p.StartInfo.Arguments = " /c " + command;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.RedirectStandardInput = true;
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.StandardOutputEncoding = p.StartInfo.StandardErrorEncoding = Encoding.GetEncoding("gb2312");
        //UnityEngine.Debug.Log(command);
        p.Start();
        var ret = p.StandardOutput.ReadToEnd() + "\n" + p.StandardError.ReadToEnd();
        Log.LogInfo(command + $" -> cost {(System.DateTime.UtcNow.Ticks - t1) / (10000)}ms \n" + ret);
        p.Close();
        return ret;
    }
}
