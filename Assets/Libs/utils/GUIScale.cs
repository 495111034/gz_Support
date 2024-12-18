using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// 设置缩放
/// </summary>
public static class GUIScale
{
    static float GlobalScale = 1;        // 全局缩放   

    static Vector2 StandardScreen = new Vector2(MyUITools.RefScreenWidth, MyUITools.RefScreenHeight);

    public static void UpdateScale(float width, float height)
    {
        var sx = width / StandardScreen.x;
        var sy = height / StandardScreen.y;
        GlobalScale = Mathf.Min(sx, sy);
        if (GlobalScale > 1) GlobalScale = Mathf.Max(1, GlobalScale * 0.95f);        
    }
    // 缩放
    public static int Scale(int v)
    {
        return Mathf.RoundToInt(v * GlobalScale);
    }

    public static float Scale(float v)
    {
        return v * GlobalScale;
    }

    public static Vector2 Scale(int w, int h)
    {
        w = Mathf.RoundToInt(w * GlobalScale);
        h = Mathf.RoundToInt(h * GlobalScale);
        return new Vector2(w, h);
    }

    public static Vector2 Scale(Vector2 s)
    {
        var w = Mathf.RoundToInt(s.x * GlobalScale);
        var h = Mathf.RoundToInt(s.y * GlobalScale);
        return new Vector2(w, h);
    }

    //
    public static RectOffset Scale(RectOffset rc)
    {
        var l = Mathf.FloorToInt(rc.left * GlobalScale);
        var t = Mathf.FloorToInt(rc.top * GlobalScale);
        var r = Mathf.CeilToInt(rc.right * GlobalScale);
        var b = Mathf.CeilToInt(rc.bottom * GlobalScale);
        return new RectOffset(l, r, t, b);
    }

    public static Rect Scale(Rect rc)
    {
        var x = Mathf.FloorToInt(rc.x * GlobalScale);
        var y = Mathf.FloorToInt(rc.y * GlobalScale);
        var w = Mathf.CeilToInt(rc.width * GlobalScale);
        var h = Mathf.CeilToInt(rc.height * GlobalScale);
        return new Rect(x, y, w, h);
    }

    //
    public static string Scale(string values)
    {
        var arr = values.Split(',');
        for (int i = 0; i < arr.Length; i++)
        {
            var v1 = Convert.ToInt32(arr[i]);
            var v2 = Scale(v1);
            arr[i] = v2.ToString();
        }
        return string.Join(",", arr);
    }

    //
    public static Vector2 Scale(Texture tex)
    {
        var v = new Vector2(tex.width, tex.height);
        return Scale(v);
    }
}
