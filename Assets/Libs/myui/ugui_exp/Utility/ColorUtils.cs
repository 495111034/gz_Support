using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEngine;

#if UNITY_ASYNC
using System.Threading;
using System.Threading.Tasks;
#endif

/// <summary>
/// 颜色工具类
/// </summary>
public static class ColorUtils
{

    public static Color HtmlToColor(string hex)
    {
        if (ColorUtility.TryParseHtmlString(hex, out var c))
        {
            return c;
        }
        return Color.white;
    }

    // rrggbbaa, rrggbb => Color
    public static Color HexToColor(string hex)
    {
        if (string.IsNullOrEmpty(hex)) return Color.black;
        if (hex.StartsWith("hdr:"))
        {
            string color_str = hex.Split(':')[1];
            string[] colors = color_str.Split('_');
            float r,g,b,a = 1f;
            if(!float.TryParse(colors[0], out r))
            {
                r = 0f;
            }
            if (!float.TryParse(colors[1], out g))
            {
                g = 0f;
            }
            if (!float.TryParse(colors[2], out b))
            {
                b = 0f;
            }
            if(colors.Length>= 4)
            {
                if(!float.TryParse(colors[3], out a))
                {
                    a = 1f;
                }
            }
            return new Color(r, g, b, a);
        }
        else
        {

            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
            byte a = 255;
            if (hex.Length >= 8)
            {
                a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
            }

            return new Color32(r, g, b, a);
        }
       
    }

    // 返回 "rgb:r_g_b_a"
    public static string ColorToHex(Color c)
    {       
        
        string hex =  string.Format($"hdr:{c.r}_{c.g}_{c.b}_{c.a}");
        //Log.LogError($"1111 ColorToHex:r={c.r},g ={c.g},b = {c.b},hex={hex}");
       // Color c2 = HexToColor(hex);
        //Log.LogError($"2222 HexToColor：hex={hex}，r={c2.r},g ={c2.g},b = {c2.b}");
        return hex;
    }

    // 返回 "#rrggbb"
    public static string ColorToHex(Color32 c)
    {
        return string.Format("{0:x2}{1:x2}{2:x2}", (int)(c.r * 255), (int)(c.g * 255), (int)(c.b * 255));
    }
}