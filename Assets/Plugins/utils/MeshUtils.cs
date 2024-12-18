using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;


static class MeshUtils
{

    // 获取阻挡区域
    public static Rect CalcuateBlockRect(this Transform t)
    {
        var xmin = float.MaxValue;
        var xmax = float.MinValue;
        var zmin = float.MaxValue;
        var zmax = float.MinValue;

        var toWorld = t.localToWorldMatrix;
        for (int i = 0; i < 4; i++)
        {
            var pos = toWorld.MultiplyPoint3x4(s_corners[i]);

            var x1 = Mathf.FloorToInt(pos.x);
            var x2 = Mathf.CeilToInt(pos.x);
            var z1 = Mathf.FloorToInt(pos.z);
            var z2 = Mathf.CeilToInt(pos.z);

            if (x1 < xmin) xmin = x1;
            if (x2 > xmax) xmax = x2;
            if (z1 < zmin) zmin = z1;
            if (z2 > zmax) zmax = z2;
        }

        //
        return Rect.MinMaxRect(xmin, zmin, xmax, zmax);
    }

    static readonly Vector3[] s_corners = new Vector3[]
     {
                // 左下角开始, 顺时针, 4个顶点
                new Vector3(-0.5f, 0, -0.5f),
                new Vector3(-0.5f, 0, +0.5f),
                new Vector3(+0.5f, 0, +0.5f),
                new Vector3(+0.5f, 0, -0.5f),
     };
    static Vector3[] s_corners2 = new Vector3[4];
    static MyHash s_hash = null;
    static Vector2 GetGridPos(Vector3 v)
    {
        return new Vector2(v.x, v.z);
    }

    static List<int> _tmpIntList = new List<int>();
    // 获取占据的格子数组(近似算法, 会有重复格子)
    public static int[] CalculatePosList(this Transform trans)
    {
        var ret = _tmpIntList;
        ret.Clear();

        // 计算 trans 各顶点的世界坐标
        GetTempWorldCorners(trans);

        // 确定 中心/右/上 3个点
        var pos_center = GetGridPos(s_corners2[0]);
        var pos_right = GetGridPos(s_corners2[3]);
        var pos_up = GetGridPos(s_corners2[1]);

        // 计算 右/上 方向的 差值/距离/移动速度/移动次数
        var move_speed = 0.47f; // 每步移动的距离, 越小精度越高

        var right_diff = pos_right - pos_center;
        var right_dist = right_diff.magnitude;
        var right_speed = right_diff.normalized * move_speed;
        var right_count = Mathf.CeilToInt(right_dist / move_speed);

        var up_diff = pos_up - pos_center;
        var up_dist = up_diff.magnitude;
        var up_speed = up_diff.normalized * move_speed;
        var up_count = Mathf.CeilToInt(up_dist / move_speed);

        // 向 上/右 遍历每个格子, 并添加它们
        if (s_hash == null) s_hash = new MyHash();
        s_hash.Clear();

        // 向上遍历
        var pos_to_up = pos_center;
        for (int up_i = 0; up_i <= up_count; up_i++, pos_to_up += up_speed)
        {
            if (up_i == up_count) pos_to_up = pos_up;   // 避免溢出

            // 向右遍历
            var pos_to_right = pos_to_up;
            for (int right_i = 0; right_i <= right_count; right_i++, pos_to_right += right_speed)
            {
                if (right_i == right_count) pos_to_right = pos_to_up + right_diff;  // 避免溢出

                // 添加该位置
                var x = Mathf.FloorToInt(pos_to_right.x);
                var y = Mathf.FloorToInt(pos_to_right.y);
                if (s_hash.Add(x, y))
                {
                    ret.Add(x);
                    ret.Add(y);
                }
            }
        }
        //
        return ret.ToArray();
    }

    // 计算 trans 各顶点的世界坐标, 左下角为起点, 顺时针
    public static Vector3[] GetTempWorldCorners(Transform t)
    {
        var toWorld = t.localToWorldMatrix;
        for (int i = 0; i < 4; i++)
        {
            s_corners2[i] = toWorld.MultiplyPoint3x4(s_corners[i]);
        }
        return s_corners2;
    }


    class MyHash
    {
        const int MAX_WIDTH = 512;      // 128x128=16kb
        const int MAX_HEIGHT = 512;

        byte[] _buff = new byte[MAX_WIDTH * MAX_HEIGHT];
        byte _time;

        // 添加, 返回是否成功
        public bool Add(int x, int y)
        {
            if (x >= 0 && x < MAX_WIDTH && y >= 0 && y < MAX_HEIGHT)
            {
                var idx = y * MAX_WIDTH + x;
                if (_buff[idx] != _time)
                {
                    _buff[idx] = _time;
                    return true;
                }
            }
            return false;
        }

        // 清空
        public void Clear()
        {
            if (_time < 255)
            {
                ++_time;
            }
            else
            {
                _time = 1;
                Array.Clear(_buff, 0, _buff.Length);
            }
        }
    }
}

