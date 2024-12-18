using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


/// <summary>
/// 带宽统计
/// </summary>
class BandwidthStat
{
    const float TIME_CYCLE = 1;     // 统计周期

    //
    DataStatistics _send_stat;
    DataStatistics _recv_stat;

    //
    float _next_time;       // 下次统计时间
    int _total_send;
    int _total_recv;

    //
    public BandwidthStat(int num_samples = 10)
    {
        _send_stat = new DataStatistics(num_samples);
        _recv_stat = new DataStatistics(num_samples);
    }

    // 每帧更新
    public void Update(int total_send, int total_recv)
    {
        var now_time = Time.realtimeSinceStartup;
        if (now_time >= _next_time)
        {
            var num_send = total_send - _total_send;
            var num_recv = total_recv - _total_recv;

            _send_stat.AddValue(num_send);
            _recv_stat.AddValue(num_recv);

            _next_time = now_time + TIME_CYCLE;
            _total_send = total_send;
            _total_recv = total_recv;
        }
    }

    //
    public DataStatistics SendStat { get { return _send_stat; } }
    public DataStatistics RecvStat { get { return _recv_stat; } }
}

