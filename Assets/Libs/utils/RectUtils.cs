using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// 矩形工具, 用于对齐等
/// </summary>
public static class RectUtils
{
    /// <summary>
    /// 对齐到中间
    /// </summary>
    public static void AlignCenterBoth(Rect border, ref Rect rc)
    {
        rc.x = border.x + (border.width - rc.width) / 2;
        rc.y = border.y + (border.height - rc.height) / 2;
    }
    public static void AlignCenterHorz(Rect border, ref Rect rc)
    {
        rc.x = border.x + (border.width - rc.width) / 2;
    }
    public static void AlignCenterVert(Rect border, ref Rect rc)
    {
        rc.y = border.y + (border.height - rc.height) / 2;
    }
    public static void AlignCenterBoth(Vector2 pos, ref Rect rc)
    {
        rc.x = pos.x - rc.width / 2;
        rc.y = pos.y - rc.height / 2;
    }

    public static Rect AlignCenterBoth(Rect border, Vector2 size)
    {
        var rc = new Rect(0, 0, size.x, size.y);
        AlignCenterBoth(border, ref rc);
        return rc;
    }

    /// <summary>
    /// 判断是否包含
    /// </summary>
    public static bool Contains(Rect border, Rect inner)
    {
        return border.xMin <= inner.xMin &&
            border.xMax >= inner.xMax &&
            border.yMin <= inner.yMin &&
            border.yMax >= inner.yMax;
    }

    /// <summary>
    /// 判断是否有相交
    /// </summary>
    public static bool HasAny(Rect border, Rect inner)
    {
        return inner.xMin < border.xMax &&
            inner.xMax > border.xMin &&
            inner.yMin < border.yMax &&
            inner.yMax > border.yMin;
    }

    /// <summary>
    /// 水平排列各矩形
    /// </summary>
    /// <param name="rects">矩形数组</param>
    /// <param name="ratios">各矩形在 border 中的比例制</param>
    /// <param name="border">外框矩形</param>
    /// <param name="vertAlign">垂直对齐, 0=上对齐, 1=中对齐, 2=下对齐</param>
    public static void HorzAlignRects(List<Rect> rects, out Rect[] ratios, out Rect border, int vertAlign)
    {
        int count = rects.Count;
        var rcs = rects.ToArray();

        // 水平排列矩形, 左上角对齐
        float left = 0, max_h = 0;
        for (int i = 0; i < count; i++)
        {
            var rc = rcs[i];
            rc.x = left;
            rc.y = 0;
            rcs[i] = rc;
            left += rc.width;
            if (rc.height > max_h) max_h = rc.height;
        }
        float width = left;
        float height = max_h;

        // 转换为比例值
        for (int i = 0; i < count; i++)
        {
            var rc = rcs[i];
            rc.xMin /= width;
            rc.xMax /= width;
            if (vertAlign == 0)
            {
                float h = rc.height / height;
                rc.yMin = 0;
                rc.yMax = h;
            }
            else if (vertAlign == 1)
            {
                float h = (1 - (rc.height / height)) / 2;
                rc.yMin = h;
                rc.yMax = 1 - h;
            }
            else
            {
                float h = rc.height / height;
                rc.yMin = 1 - h;
                rc.yMax = 1;
            }
            rcs[i] = rc;
        }

        // ok
        ratios = rcs;
        border = new Rect(0, 0, width, height);
    }

    /// <summary>
    /// 计算每个矩形的当前坐标
    /// </summary>
    /// <param name="border">外框矩形</param>
    /// <param name="ratios">各矩形的比例制, 调用 HorzAlignRects 后获得 </param>
    /// <param name="rects">用于返回矩形坐标</param>
    public static void CalculateRects(Rect border, Rect[] ratios, Rect[] rects)
    {
        float x = border.x, y = border.y, w = border.width, h = border.height;
        for (int i = 0; i < ratios.Length; i++)
        {
            Rect rc = ratios[i];
            rc.xMin = x + rc.xMin * w;
            rc.xMax = x + rc.xMax * w;
            rc.yMin = y + rc.yMin * h;
            rc.yMax = y + rc.yMax * h;
            rects[i] = rc;
        }
    }

    /// <summary>
    /// 对齐到整数边界, 去掉小数点
    /// </summary>
    public static Rect Bound(Rect rc)
    {
        rc.x = Mathf.Floor(rc.x);
        rc.y = Mathf.Floor(rc.y);
        rc.width = Mathf.Ceil(rc.width);
        rc.height = Mathf.Ceil(rc.height);
        return rc;
    }

    /// <summary>
    /// 设置偏移量
    /// </summary>
    public static Rect Offset(Rect rc, float l, float t, float r, float b)
    {
        rc.xMin += l;
        rc.yMin += t;
        rc.xMax += r;
        rc.yMax += b;
        return rc;
    }
    public static Rect Offset(Rect rc, float off)
    {
        rc.xMin -= off;
        rc.yMin -= off;
        rc.xMax += off;
        rc.yMax += off;
        return rc;
    }

    // 转换坐标, 返回 pos2, 使得 pos1/rc1 = pos2/rc2
    public static Vector2 ConvertPos(Vector2 pos1, Rect rc1, Rect rc2)
    {
        var x2 = rc2.xMin + (pos1.x - rc1.xMin) / rc1.width * rc2.width;
        var y2 = rc2.yMin + (pos1.y - rc1.yMin) / rc1.height * rc2.height;
        return new Vector2(x2, y2);
    }

    /// <summary>
    /// 格式化字符串
    /// </summary>
    public static string Format(Rect rect)
    {
        return string.Format("({0}, {1}, {2}x{3})", rect.x, rect.y, rect.width, rect.height);
    }
    public static string FormatMinMax(Rect rect)
    {
        return string.Format("(x:{0}, {1}, y:{2}, {3})", rect.xMin, rect.xMax, rect.yMin, rect.yMax);
    }
    public static string Format(IEnumerable<Rect> rects)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var rc in rects)
        {
            sb.Append(Format(rc));
            sb.Append(" ");
        }
        return sb.ToString();
    }

    /// <summary>
    /// 把 rect 约束在 border 内
    /// </summary>
    public static Rect PosRect(Rect rect, Rect border)
    {
        if (rect.xMax > border.xMax) rect.x = border.xMax - rect.width;
        if (rect.yMax > border.yMax) rect.y = border.yMax - rect.height;
        if (rect.x < border.x) rect.x = border.x;
        if (rect.y < border.y) rect.y = border.y;
        return rect;
    }

    // 剪裁, 返回是否可见
    public static bool ClipRect(ref Rect rc, Rect border)
    {
        if (rc.xMin < border.xMin) rc.xMin = border.xMin;
        if (rc.xMax > border.xMax) rc.xMax = border.xMax;
        if (rc.yMin < border.yMin) rc.yMin = border.yMin;
        if (rc.yMax > border.yMax) rc.yMax = border.yMax;
        return rc.xMin < rc.xMax && rc.yMin < rc.yMax;
    }

    /// <summary>
    /// 返回把 rect 按照 mode 绘制到 target 时, rect 实际所处的坐标范围
    /// </summary>
    public static Rect ScaleRect(Rect rect, Rect target, ScaleMode mode)
    {
        // 允许变形
        if (mode == ScaleMode.StretchToFill)
        {
            return target;
        }
        // 保持比例, 变大/变小
        else if (mode == ScaleMode.ScaleAndCrop || mode == ScaleMode.ScaleToFit)
        {
            float sx = target.width / rect.width;
            float sy = target.height / rect.height;
            float scale = mode == ScaleMode.ScaleAndCrop ? Mathf.Max(sx, sy) : Mathf.Min(sx, sy);
            rect.width *= scale;
            rect.height *= scale;
            rect.x = target.x + (target.width - rect.width) / 2;
            rect.y = target.y + (target.height - rect.height) / 2;
            return rect;
        }
        else
        {
            throw new NotImplementedException();
        }
    }

    // 以中心放大
    public static Rect ScaleRect(Rect rc, float scale)
    {
        var cx = (rc.xMin + rc.xMax) / 2;
        var cy = (rc.yMin + rc.yMax) / 2;
        var width = rc.width * scale;
        var height = rc.height * scale;
        return new Rect(cx - width / 2, cy - height / 2, width, height);
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

    /// <summary>
    /// 获取包围矩形
    /// </summary>
    public static Rect GetBorder(float[] vertexs)
    {
        float xmin, xmax, ymin, ymax;
        xmin = xmax = vertexs[0];
        ymin = ymax = vertexs[1];
        for (int i = 2; i < vertexs.Length - 1; i += 2)
        {
            var x = vertexs[i];
            var y = vertexs[i + 1];
            if (x < xmin) xmin = x; else if (x > xmax) xmax = x;
            if (y < ymin) ymin = y; else if (y > ymax) ymax = y;
        }
        return new Rect(xmin, ymin, xmax - xmin, ymax - ymin);
    }

    /// <summary>
    /// 寻找浮动窗口(菜单/tips)合适的显示位置
    ///     border      外框矩形
    ///     ox/oy       鼠标坐标
    ///     ow/oh       鼠标自身尺寸
    ///     wid/hgt     tips 窗口的尺寸
    ///     order       匹配顺序, "0123"=左上/右上/左下/右下
    /// </summary>
    static public Vector2 GetFloatRect(Rect border, float ox, float oy, float ow, float oh, float wid, float hgt, string order)
    {
        // 计算可选的四个位置, 格式: [x, y, offy]
        var pos_info = new float[4, 3]{
			{ox-wid, 	oy-hgt,		1},
            {ox+ow, 	oy-hgt,		1},
			{ox-wid, 	oy+oh,		-1},
            {ox+ow, 	oy+oh,		-1},
        };

        // a), 判断 tmp 位于四个角落时, 是否符合条件
        var tmp = new Rect(0, 0, wid, hgt);
        var orders = order.ToCharArray();
        for (int i = 0; i < 4; i++)
        {
            var index = orders[i] - 48;
            tmp.x = pos_info[index, 0];
            tmp.y = pos_info[index, 1];

            if (Contains(border, tmp))
            {
                return new Vector2(tmp.x, tmp.y);
            }
        }

        // b), 上下移动矩形 tmp, 找到合适的位置
        for (int i = 0; i < 4; i++)
        {
            var index = orders[i] - 48;
            tmp.x = pos_info[index, 0];
            tmp.y = pos_info[index, 1];

            // 左/右/高度 都符合条件, 上下移动
            if (tmp.xMin >= border.xMin && tmp.xMax <= border.xMax && tmp.height <= border.height)
            {
                var offy = pos_info[index, 2];
                if (offy > 0)
                {
                    tmp.y = border.y;
                }
                else
                {
                    tmp.y = border.yMax - tmp.height;
                }
                return new Vector2(tmp.x, tmp.y);
            }
        }

        return new Vector2(border.x, border.y);
    }

    /// <summary>
    /// 获取下拉框的位置
    /// </summary>
    static public Vector2 GetDropDownRect(Rect border, Rect menu, Rect align)
    {
        // 下, 左对齐
        menu.x = align.x;
        menu.y = align.yMax;
        if (Contains(border, menu)) return new Vector2(menu.x, menu.y);

        // 下, 右对齐
        menu.x = align.xMax - menu.width;
        menu.y = align.yMax;
        if (Contains(border, menu)) return new Vector2(menu.x, menu.y);

        // 上, 左对齐
        menu.x = align.x;
        menu.y = align.y - menu.height;
        if (Contains(border, menu)) return new Vector2(menu.x, menu.y);

        // 上, 右对齐
        menu.x = align.xMax - menu.width;
        menu.y = align.y - menu.height;
        if (Contains(border, menu)) return new Vector2(menu.x, menu.y);

        // 默认, (0,0)
        return Vector2.zero;
    }

    // 屏幕坐标(0,0,sw,sh) -> 投影空间坐标(-1,-1,1,1)
    public static Rect ScreenToProjector(Rect rc, int sw, int sh)
    {
        rc.xMin = rc.xMin / sw * 2 - 1;
        rc.xMax = rc.xMax / sw * 2 - 1;
        rc.yMin = 1 - rc.yMin / sh * 2;
        rc.yMax = 1 - rc.yMax / sh * 2;
        return rc;
    }

    // 剪裁 rect
    public static Rect ClipRect(Rect rect, Rect clip)
    {
        var xmin = Mathf.Max(rect.xMin, clip.xMin);
        var xmax = Mathf.Min(rect.xMax, clip.xMax);
        var ymin = Mathf.Max(rect.yMin, clip.yMin);
        var ymax = Mathf.Min(rect.yMax, clip.yMax);
        if (xmax < xmin) xmax = xmin;
        if (ymax < ymin) ymax = ymin;
        return Rect.MinMaxRect(xmin, ymin, xmax, ymax);
    }

    // 剪裁 rect/uv
    public static void ClipRectUV(Rect rect, Rect uv, Rect clip, out Rect rect2, out Rect uv2)
    {
        if (rect.width <= 0 || rect.height <= 0)
        {
            rect2 = new Rect(rect.xMin, rect.yMin, 0, 0);
            uv2 = new Rect(uv.xMin, uv.yMin, 0, 0);
            return;
        }

        rect2 = ClipRect(rect, clip);
        if (rect2.width <= 0 || rect2.height <= 0)
        {
            uv2 = new Rect(uv.xMin, uv.yMin, 0, 0);
            return;
        }

        var xmin = uv.xMin + (rect2.xMin - rect.xMin) / rect.width * uv.width;
        var xmax = uv.xMax + (rect2.xMax - rect.xMax) / rect.width * uv.width;
        var ymin = uv.yMin + (rect.yMax - rect2.yMax) / rect.height * uv.height;
        var ymax = uv.yMax + (rect.yMin - rect2.yMin) / rect.height * uv.height;
        uv2 = Rect.MinMaxRect(xmin, ymin, xmax, ymax);
    }

    // 交换 min/max
    public static void SwapMinMax(ref Rect rc, bool x, bool y)
    {
        if (x)
        {
            var tmp = rc.xMin;
            rc.xMin = rc.xMax;
            rc.xMax = tmp;
        }
        if (y)
        {
            var tmp = rc.yMin;
            rc.yMin = rc.yMax;
            rc.yMax = tmp;
        }
    }

    // 镜像 X
    public static void FlipX(ref Rect rc, float center)
    {
        rc.xMin = center + (center - rc.xMin);
        rc.xMax = center + (center - rc.xMax);
    }

    // 获取包围
    public static Rect GetBorder(Rect a, Rect b)
    {
        if (b.xMin < a.xMin) a.xMin = b.xMin;
        if (b.yMin < a.yMin) a.yMin = b.yMin;
        if (b.xMax > a.xMax) a.xMax = b.xMax;
        if (b.yMax > a.yMax) a.yMax = b.yMax;
        return a;
    }


    // 创建临时阻挡
    //    public static byte[] CreateTempMask(byte[] src, int width, int height, byte value, IEnumerable<TempBlockDefine> blocks)
    //    {
    //        byte[] ret = new byte[height * width];
    //        Array.Copy(src, ret, ret.Length);

    //        var border = new Rect(0, 0, width, height);

    //        foreach (var block in blocks)
    //        {
    //            var bounds = block.bounds;
    //            Rect rc = new Rect(bounds.min.x, bounds.min.z, bounds.size.x, bounds.size.z);
    //            var tmp = rc;
    //            if (RectUtils.ClipRect(ref tmp, border))
    //            {

    //                var x1 = Mathf.FloorToInt(tmp.xMin);
    //                var x2 = Mathf.CeilToInt(tmp.xMax);
    //                var y1 = Mathf.FloorToInt(tmp.yMin);
    //                var y2 = Mathf.CeilToInt(tmp.yMax);



    //                var idx = y1 * width + x1;
    //                var add = width - (x2 - x1);
    //                var h = bounds.center.y;

    //                for (var y = y1; y < y2; y++, idx += add)
    //                {
    //                    for (var x = x1; x < x2; x++, idx++)
    //                    {
    //                        if (RectUtils.InRect(new Vector3(x, h, y), block.begin_pos, block.end_pos, block.width))
    //                        {
    //                            ret[idx] = value;
    ////							GameObject go = GameObject.CreatePrimitive( PrimitiveType.Cube);
    ////							go.transform.position = new Vector3(x, PluginMM.mainRole.Postion.y, y);
    //                        }

    //                    }
    //                }
    //            }
    //        }
    //        return ret;
    //    }

    // 像素坐标(y向下) -> 纹理坐标(y向上)
    static public Rect Rect2UV(Rect rc, int width, int height)
    {
        Rect uv = rc;

        uv.xMin = rc.xMin / width;
        uv.xMax = rc.xMax / width;

        uv.yMin = 1f - rc.yMax / height;          // y 是颠倒的
        uv.yMax = 1f - rc.yMin / height;

        return uv;
    }

    // uv -> rc
    public static Rect UV2Rect(Rect uv, int width, int height)
    {
        var rc = uv;

        rc.xMin = uv.xMin * width;
        rc.xMax = uv.xMax * width;

        rc.yMin = (1 - uv.yMax) * height;
        rc.yMax = (1 - uv.yMin) * height;

        return rc;
    }

    //坐标是否在矩形中
    //矩形底边中点begin_pos，矩形高边中点end_pos， 矩形宽度 width
    public static bool InRect(Vector3 postion, Vector3 begin_pos, Vector3 end_pos, float width)
    {
        return InRect(postion.x, postion.z, begin_pos.x, begin_pos.z, end_pos.x, end_pos.z, Vector2.Distance(new Vector2(begin_pos.x, begin_pos.z), new Vector2(end_pos.x, end_pos.z)), width);
    }

    //计算一个点是否在矩形中
    //|P2P|×|P1P2|*|P3P|×|P3P4|<=0 And |P1P|×|P1P4|*|P2P|×|P2P3|<=0
    static bool InRect(float x, float y, float x1, float y1, float x2, float y2, float length, float wide)
    {
        //获取矩形的四个顶点
        Vector2 p1;
        Vector2 p2;
        GetRect2Point(x1, y1, x2, y2, length, wide, out p1, out p2);
        float x1v = p1.x;
        float y1v = p1.y;
        float x2v = p2.x;
        float y2v = p2.y;

        GetRect2Point(x2, y2, x1, y1, length, wide, out p1, out p2);
        float x3v = p1.x;
        float y3v = p1.y;
        float x4v = p2.x;
        float y4v = p2.y;

        //TODO:为了提给效率，可以展开来，不掉用_mulitpy函数
        if ((Mulitpy(x, y, x1v, y1v, x2v, y2v) * Mulitpy(x, y, x4v, y4v, x3v, y3v)) <= 0 &&
            (Mulitpy(x, y, x4v, y4v, x1v, y1v) * Mulitpy(x, y, x3v, y3v, x2v, y2v)) <= 0)
            return true;
        else
            return false;
    }

    //获取矩形的2个顶点
    static void GetRect2Point(float x1, float y1, float x2, float y2, float length, float wide, out Vector2 point1, out Vector2 point2)
    {
        float x = x2 - x1;
        float y = y2 - y1;
        float x1_vertex = ((wide * 0.5f) / length) * y * -1;
        float y1_vertex = ((wide * 0.5f) / length) * x;
        float x2_vertex = (-1) * x1_vertex;
        float y2_vertex = (-1) * y1_vertex;
        point1.x = x1_vertex + x1;
        point1.y = y1_vertex + y1;
        point2.x = x2_vertex + x1;
        point2.y = y2_vertex + y1;
    }

    //计算叉乘 |PP1| × |PP2| 
    static float Mulitpy(float x1, float y1, float x2, float y2, float x, float y)
    {
        return (x1 - x) * (y2 - y) - (x2 - x) * (y1 - y);
    }

    // 在 border 范围内返回 num 个位置, 要求它们尽量散开
    public static List<float> GetRandomPos(Rect border, int num, float radius)
    {
        var list = new List<float>();
        if (num <= 0) return list;

        // 计算直径
        var diameter = radius + radius;
        var diameter_sq = diameter * diameter;

        // 计算包围矩形, 并排除掉边界
        var xmin = border.xMin + radius;
        var xmax = border.xMax - radius;
        var ymin = border.yMin + radius;
        var ymax = border.yMax - radius;

        if (xmax < xmin) xmax = xmin = (xmin + xmax) / 2;
        if (ymax < ymin) ymax = ymin = (ymin + ymax) / 2;

        // 随机 num 个位置
        for (int i = 0; i < num; i++)
        {
            // 尝试 max_try 次数, 寻找距离 已知点 最远的点
            var max_try = i + 5;
            float find_x = 0f, find_y = 0f, find_dist_sq = -1f;
            for (int j = 0; j < max_try; j++)
            {
                var x = UnityEngine.Random.Range(xmin, xmax);
                var y = UnityEngine.Random.Range(ymin, ymax);
                var dist_sq = GetDistSq(list, x, y);
                if (dist_sq > find_dist_sq)
                {
                    find_dist_sq = dist_sq;
                    find_x = x;
                    find_y = y;

                    // 如果距离大于直径, 则直接使用
                    if (dist_sq >= diameter_sq) break;
                }
            }

            // 使用该位置
            list.Add(find_x);
            list.Add(find_y);
        }

        // ok
        return list;
    }

    // 获取 x/y 距离 pos_list 的最近距离
    static float GetDistSq(List<float> pos_list, float x, float y)
    {
        var min_dist_sq = float.MaxValue;
        for (int i = 0; i < pos_list.Count; i += 2)
        {
            var x2 = pos_list[i];
            var y2 = pos_list[i + 1];

            var dx = x2 - x;
            var dy = y2 - y;
            var dist_sq = dx * dx + dy * dy;

            if (dist_sq < min_dist_sq)
            {
                min_dist_sq = dist_sq;
            }
        }
        return min_dist_sq;
    }
}
