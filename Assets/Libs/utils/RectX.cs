using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


/// <summary>
/// 矩形, 用作排版
/// </summary>
public class RectX
{
    Rect rect = new Rect(0, 0, 0, 0);
    float sx, sy, hgap, vgap;
    float maxx, maxy;

    /// <summary>
    /// 返回当前矩形
    /// </summary>
    public Rect Current()
    {
        return rect;
    }

    /// <summary>
    /// 设置开始坐标
    /// </summary>
    public void Start(float x, float y)
    {
        maxx = rect.x = sx = x;
        maxy = rect.y = sy = y;
    }

    /// <summary>
    /// 换行, 以 bottom 作为 top
    /// </summary>
    public void NewLine()
    {
        rect.Set(sx, rect.yMax + vgap, 0, rect.height);
        maxy = Math.Max(maxy, rect.yMax);
    }

    /// <summary>
    /// 设置当前行的高度
    /// </summary>
    public void Height(float height)
    {
        rect.height = height;
        maxy = Math.Max(maxy, rect.yMax);
    }

    /// <summary>
    /// 设置水平间距
    /// </summary>
    public void HGap(float v)
    {
        hgap = v;
    }

    /// <summary>
    /// 向右移动 width 距离, 返回移动之前的矩形
    /// </summary>
    public Rect MoveRight(float width)
    {
        rect.x = rect.xMax + hgap;
        rect.width = width;
        maxx = Math.Max(maxx, rect.xMax);
        return rect;
    }

    /// <summary>
    /// 中间
    /// </summary>
    public Rect Center(float width)
    {
        rect.x = sx + ((maxx - sx) - width) / 2;
        rect.width = width;
        Debug.Log("Center: " + rect);
        return rect;
    }

    /// <summary>
    /// 向下移动高度
    /// </summary>
    public void MoveDown(float height)
    {
        rect.y += height;
        maxy = Math.Max(maxy, rect.yMax);
    }

    /// <summary>
    /// 返回最大的矩形范围
    /// </summary>
    public Rect MaxRect
    {
        get
        {
            return new Rect(sx, sy, maxx - sx, maxy - sy);
        }
    }
}

