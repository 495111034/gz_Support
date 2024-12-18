using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;

/// <summary>
/// 网络工具
/// </summary>
public static class NetworkUtils
{
    // 获取最佳的 IP
    public static string GetBestIp(string host, string def_ips)
    {
        // 如果是 ip 格式, 则直接使用
        if (char.IsDigit(host[0])) return host;

        // 解析域名, 获得本地 ip 列表
        string[] local_ip_list = null;
        try
        {
            var arr = Dns.GetHostAddresses(host);
            local_ip_list = new string[arr.Length];
            for (int i = 0; i < arr.Length; i++) local_ip_list[i] = arr[i].ToString();
        }
        catch (Exception e)
        {
            Console.WriteLine("GetBestIp, host:{0}, error:{1}", host, e.ToString());
            local_ip_list = null;
        }

        // 解析服务端 ip 列表
        var remote_ip_list = def_ips.Split(',');

        // 解析成功
        if (local_ip_list != null && local_ip_list.Length > 0)
        {
            // 使用最佳IP, 匹配每个 remote_ip, 如果在 local_ip_list 中则使用
            foreach (var remote_ip in remote_ip_list)
            {
                foreach (var local_ip in local_ip_list)
                {
                    if (remote_ip == local_ip) return remote_ip;
                }
            }

            // 如果没有最佳, 则使用第一个成功的 IP
            return local_ip_list[0];
        }
        // 解析失败, 使用默认值
        else
        {
            return remote_ip_list[0];
        }
    }

    // 把 host 部分转为 ip 格式
    public static string ReplaceHost2Ip(string url, string def_ips)
    {
        // 获取主机名
        var host = GetUrlHostName(url);
        if (host == null)
        {
            Console.WriteLine("ReplaceHost2Ip, get domain failed! url:{0}", url);
            return url;
        }

        // 获取 ip
        string ip = GetBestIp(host, def_ips);

        // 替换
        var url2 = url.Replace(host, ip);
        return url2;
    }

    // 根据 url 获取 host 名字, 例如 "http://www.baidu.com/index.php" 返回 "www.baidu.com"
    public static string GetUrlHostName(string url)
    {
        var prefix = "http://";
        if (url.StartsWith(prefix))
        {
            var idx = url.IndexOf('/', prefix.Length);
            if (idx > 0)
            {
                return url.Substring(prefix.Length, idx - prefix.Length);
            }
            else
            {
                return url.Substring(prefix.Length);
            }
        }
        return null;
    }

}
