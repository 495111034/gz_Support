using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Runtime.InteropServices;
using Random = UnityEngine.Random;

public static class MathUtils
{
    // degree -> 旋转
    //public static Quaternion Degree2Quaternion(float degree)
    //{
    //    return Quaternion.AngleAxis(degree, Vector3.up);
    //}

    public static float GetRate(float value, float max)
    {
        var rate = Mathf.Clamp01(value / max);
        if (float.IsNaN(rate)) rate = 0;
        return rate;
    }
    public static float GetRate(int value, int max)
    {
        var rate = Mathf.Clamp01((float)value / max);
        if (float.IsNaN(rate)) rate = 0;
        return rate;
    }
    public static float GetRate(long value, long max)
    {
        var rate = Mathf.Clamp01((float)((double)value / max));
        if (float.IsNaN(rate)) rate = 0;
        return rate;
    }

    // xz -> degree
    public static float XZ2Degree(float dx, float dz)
    {
        var radian = Mathf.Atan2(dz, dx);
        var degree = Mathf.RoundToInt(90 - radian * Mathf.Rad2Deg + 360) % 360;
        return degree;
    }

    // 角度差
    public static float DiffDegree(float a, float b)
    {
        var c = (a - b + 360) % 360;
        if (c >= 180) c = 360 - c;
        return c;
    }

    // 判断是否足够接近
    public static bool IsNearBy(Vector3 pos1, Vector3 pos2, float distance)
    {
        return (pos1 - pos2).sqrMagnitude <= distance * distance;
    }
    public static bool IsNearBy(float x0, float y0, float x1, float y1, float distance)
    {
        var dx = x0 - x1;
        var dy = y0 - y1;
        return (dx * dx + dy * dy) <= (distance * distance);
    }

    // 距离
    public static float Distance_Sq(float x0, float y0, float x1, float y1)
    {
        var dx = x0 - x1;
        var dy = y0 - y1;
        return dx * dx + dy * dy;
    }
    public static float Distance(float x0, float y0, float x1, float y1)
    {
        return FastSqrt.Sqrt(Distance_Sq(x0, y0, x1, y1));
    }
    public static float DistanceReal(float x0, float y0, float x1, float y1)
    {
        return Mathf.Sqrt(Distance_Sq(x0, y0, x1, y1));
    }
    public static float Distance2D_Sq(Vector3 pos1, Vector3 pos2)
    {
        var dx = pos1.x - pos2.x;
        var dz = pos1.z - pos2.z;
        return dx * dx + dz * dz;
    }
    public static float Distance2D(Vector3 pos1, Vector3 pos2)
    {
        return FastSqrt.Sqrt(Distance2D_Sq(pos1, pos2));
    }

    /// <summary>
    /// 判断点是否在多边形内, 通过计算角度和为 2PI
    /// <param name="vertexs">坐标数组, 格式为: x1, y1, x2, y2, x3, y3, ...</param>
    /// </summary>
    public static bool IsPointInPolygon(float x0, float y0, float[] vertexs)
    {
        float angle = 0;
        int length = vertexs.Length;
        float x1 = vertexs[length - 2] - x0;
        float y1 = vertexs[length - 1] - y0;
        for (int i = 0; i < length; i += 2)
        {
            float x2 = vertexs[i + 0] - x0;
            float y2 = vertexs[i + 1] - y0;
            var a = Vector2.Angle(new Vector2(x1, y1), new Vector2(x2, y2));
            angle += a;
            x1 = x2;
            y1 = y2;
        }
        return Mathf.Abs(360 - angle) < 1;
    }

    // 遍历位置, 以 x0/y0 为中心, dist为距离, angle_start为角度, 每次递增 angle_add 角度, 所获得的所有坐标
    public static IEnumerable<Vector2> ForeachPosByAngle(float x0, float y0, float dist, float angle_start, float angle_add, float angle_max_add)
    {
        float rad, x1, y1;
        for (float add = 0; add < angle_max_add; add += angle_add)
        {
            rad = (angle_start + add) * Mathf.Deg2Rad;
            x1 = x0 + Mathf.Sin(rad) * dist;
            y1 = y0 + Mathf.Cos(rad) * dist;
            yield return new Vector2(x1, y1);

            rad = (angle_start - add) * Mathf.Deg2Rad;
            x1 = x0 + Mathf.Sin(rad) * dist;
            y1 = y0 + Mathf.Cos(rad) * dist;
            yield return new Vector2(x1, y1);
        }
    }

    // 获取 x0/y0 为中心, 固定角度/距离 后的位置
    public static void GetPosition(float x0, float y0, float angle, float distance, out float x1, out float y1)
    {
        x1 = x0 + Mathf.Sin(angle * Mathf.Deg2Rad) * distance;
        y1 = y0 + Mathf.Cos(angle * Mathf.Deg2Rad) * distance;
    }

    // 下对其到 mod 的整数倍
    public static int ModTo(int value, int mod)
    {
        return value / mod * mod;
    }

    // 坐标插值
    public static Vector3 GetLerpPos(Vector3 pos1, Vector3 pos2, float time_pass, float speed)
    {
        var dist = Vector3.Distance(pos1, pos2);
        var time_len = dist / speed;
        return Vector3.Lerp(pos1, pos2, time_pass / time_len);
    }

    // 获取从 pos1 前往 pos2 处, 距离 dist 的位置 ret, 并保证 ret 距离 pos2 不小于 nearby
    public static Vector3 GetDistancePos(Vector3 pos1, Vector3 pos2, float dist, float nearby)
    {
        var diff = pos2 - pos1;

        // 计算距离
        var distance = FastSqrt.Sqrt(diff.sqrMagnitude) - nearby;
        distance = Mathf.Clamp(distance, 0, dist);

        // 计算位置
        var ret = pos1 + diff.normalized * distance;

        //
        return ret;
    }

    // 获取当前播放帧
    public static int GetFrameId(int frame_len, float cycle, float time_start)
    {
        if (frame_len <= 1) return 0;
        var time = (Time.time - time_start) % cycle;    // [0, 1)
        var fid = Mathf.FloorToInt(time / cycle * frame_len);   // [0, len)
        return fid;
    }
    public static int GetFrameId(int frame_len, float time_start, float time_play, float time_stop, int fid_stop)
    {
        if (frame_len <= 1) return 0;
        var time_pass = (Time.time - time_start) % (time_play + time_stop);
        if (time_pass < time_play)
        {
            var fid = Mathf.FloorToInt(time_pass / time_play * frame_len);
            return fid;
        }
        else
        {
            return fid_stop;
        }
    }

    // 转为 2 的 N 次方, 如果不是, 则增大 value
    public static int MakePowerOfTow(int value)
    {
        return Mathf.IsPowerOfTwo(value) ? value : Mathf.NextPowerOfTwo(value);
    }

    // 获取范围内的随机坐标
    public static Vector3 GetRandomPos(Vector3 pos, float range)
    {
        var x = pos.x + Random.Range(-range, range);
        var z = pos.z + Random.Range(-range, range);
        return new Vector3(x, pos.y, z);
    }

    // 测试概率, prob=[0, 1]
    public static bool TestRand01(float prob)
    {
        return Random.Range(0f, 1f) < prob;
    }

    // 把角度 angle 限制在 [0, 360) 之间
    public static float RoundAngle(float angle)
    {
        while (angle < 0) angle += 360;
        while (angle >= 360) angle -= 360;
        return angle;
    }

    // 获取角度 分割值, 返回 [0, num_step)
    public static int GetAngleStep(float angle, int num_step)
    {
        var size = 360f / num_step;             // 360/4 = 90
        angle = MathUtils.RoundAngle(angle + size / 2);     // [0, 360)
        var index = Mathf.FloorToInt(angle / size);         // [0, 360) / 90 = [0, 4)
        return index;
    }

    /// <summary>
    /// 三个点形成的夹角
    /// </summary>
    /// <param name="dPoint"></param>
    /// <returns></returns>
    public static double getAngle(double[] dPoint)  //6个double 依次为x1,y1,x2,y2,x3,y3
    {
        var startPos = new Vector2((float)dPoint[0], (float)dPoint[1]);
        var endPos = new Vector2((float)dPoint[2], (float)dPoint[3]);

        double s1, s2, s3, p, S;
        s1 = Math.Sqrt(Math.Pow(dPoint[0] - dPoint[2], 2) + Math.Pow(dPoint[1] - dPoint[3], 2));
        s2 = Math.Sqrt(Math.Pow(dPoint[4] - dPoint[2], 2) + Math.Pow(dPoint[5] - dPoint[3], 2));
        s3 = Math.Sqrt(Math.Pow(dPoint[0] - dPoint[4], 2) + Math.Pow(dPoint[1] - dPoint[5], 2));
        p = (s1 + s2 + s3) / 2;
        S = Math.Sqrt(p * (p - s1) * (p - s2) * (p - s3));
        var a = Math.Round(180 * Math.Asin(2 * S / (s1 * s2)) / 3.14, 1);//保留1位小数

        var angle = a;
        if (double.IsNaN(a))
        {
            if (startPos.x < endPos.x && startPos.y == endPos.y) angle = 90;
            if (startPos.x == endPos.x && startPos.y < endPos.y) angle = 0;
            if (startPos.x > endPos.x && startPos.y == endPos.y) angle = 270;
            if (startPos.x == endPos.x && startPos.y > endPos.y) angle = 180;
        }


        if (angle == 90)
        {
            if (endPos.x > startPos.x)
            {
                angle = 90;
            }
            else if (endPos.x < startPos.x)
            {
                angle = 270;
            }
        }
        if (angle == 0)
        {
            if (endPos.y < startPos.y)
            {
                angle = 180;
            }
        }
        if (endPos.x > startPos.x && endPos.y > startPos.y)
            angle = a;
        else if (endPos.x > startPos.x && endPos.y < startPos.y)
            angle = 90 + (90 - a);
        else if (endPos.x < startPos.x && endPos.y < startPos.y)
            angle = 180 + a;
        else if (endPos.x < startPos.x && endPos.y > startPos.y)
            angle = 270 + (90 - a);

        return angle;
    }

    /// <summary>
    /// 是否在扇形范围内
    /// </summary>
    /// <returns></returns>
    public static bool IsInSector(Vector2 source, Vector2 target, float angle, float distance)
    {
        bool isInDistance = Vector2.Distance(source, target) < distance;
        if (isInDistance)
            return Vector3.Angle(source, target - source) < angle / 2;

        return false;

    }

    // 获取摇晃角度, 返回 [-angle, angle)
    public static float GetShakeAngle(float time_pass, float cycle, float angle)
    {
        var rate = (time_pass % cycle) / cycle;     // [0, 1)
        rate = rate * Mathf.PI * 2f;                // [0, pi*2)
        rate = Mathf.Sin(rate);                     // [0, 1, 0, -1, 0)
        return rate * angle;                        // [0, a, 0, -a, 0)
    }

    // 任意2个数据相加, 返回类型和 v1 相同
    public static object Sum(object v1, object v2)
    {
        var sum = Convert.ToDouble(v1) + Convert.ToDouble(v2);
        return Convert.ChangeType(sum, v1.GetType());
    }

    public static void SplitInt64(long guid, out int aid, out long id) 
    {
        var head = (-1L) << 36;
        aid = (int)((guid & head) >> 36);
        id = guid & ~head;
    }

    public static int SplitServerId(long guid)
    {
        SplitInt64( guid, out var aid, out var id);
        return aid;
    }
}


//
//public class Approximate
public class FastSqrt
{
    public static float Sqrt(float z)
    {
        if (z == 0) return 0;
        FloatIntUnion u;
        u.tmp = 0;
        u.f = z;
        u.tmp -= 1 << 23; /* Subtract 2^m. */
        u.tmp >>= 1; /* Divide by 2. */
        u.tmp += 1 << 29; /* Add ((b + 1) / 2) * 2^m. */
        return u.f;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct FloatIntUnion
    {
        [FieldOffset(0)]
        public float f;

        [FieldOffset(0)]
        public int tmp;
    }
}