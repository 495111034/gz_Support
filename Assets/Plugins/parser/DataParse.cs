using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public static class DataParse
{
    // Vector2
    public static string ToString(Vector2 v)
    {
        return string.Format("{0:0.#####},{1:0.#####}", v.x, v.y);
    }
    public static Vector2 GetVector2(string text)
    {
        var arr = text.Split(',');
        return new Vector2(float.Parse(arr[0]), float.Parse(arr[1]));
    }

    //
    public static string ToString(Vector3 v)
    {
        return string.Format("{0:0.#####},{1:0.#####},{2:0.#####}", v.x, v.y, v.z);
    }

    public static string ToStringL(Vector3 v)
    {
        return string.Format("{0:0.#####}|{1:0.#####}|{2:0.#####}", v.x, v.y, v.z);
    }

    public static string ToStringL(Vector2 v)
    {
        return string.Format("{0:0.#####}|{1:0.#####}", v.x, v.y);
    }

    public static Vector3 GetVector3(string text, Vector3 def)
    {
        if (string.IsNullOrEmpty(text)) return def;
        var arr = text.Split(',');
        return new Vector3(float.Parse(arr[0]), float.Parse(arr[1]), float.Parse(arr[2]));
    }

    public static Vector3 GetVector3L(string text)
    {
        if (string.IsNullOrEmpty(text)) return Vector3.zero;
        var arr = text.Split('|');
        return new Vector3(float.Parse(arr[0]), float.Parse(arr[1]), float.Parse(arr[2]));
    }
    public static Vector2 GetVector2L(string text)
    {
        if (string.IsNullOrEmpty(text)) return Vector2.zero;
        var arr = text.Split('|');
        return new Vector2(float.Parse(arr[0]), float.Parse(arr[1]));
    }
    //
    public static string ToString(Vector4 v)
    {
        return string.Format("{0:0.########},{1:0.########},{2:0.########},{3:0.########}", v.x, v.y, v.z, v.w);
    }
    public static Vector4 GetVector4(string text)
    {
        if (string.IsNullOrEmpty(text)) return Vector4.zero;
        var arr = text.Split(',');        
        return new Vector4(float.Parse(arr[0]), float.Parse(arr[1]), float.Parse(arr[2]), float.Parse(arr[3]));
    }

    //
    public static string ToString(Color c)
    {
        return string.Format("{0:0.#####},{1:0.#####},{2:0.#####},{3:0.#####}", c.r, c.g, c.b, c.a);
    }
    public static Color GetColor(string text)
    {
        
        var arr = text.Split(',');
        return new Color(float.Parse(arr[0]), float.Parse(arr[1]), float.Parse(arr[2]), float.Parse(arr[3]));
    }


    //
    public static string ToString(Quaternion v)
    {
        return string.Format("{0:0.#####},{1:0.#####},{2:0.#####},{3:0.#####}", v.x, v.y, v.z, v.w);
    }
    public static Quaternion GetQuaternion(string text)
    {
        if (string.IsNullOrEmpty(text)) return Quaternion.identity;
        var arr = text.Split(',');
        return new Quaternion(float.Parse(arr[0]), float.Parse(arr[1]), float.Parse(arr[2]), float.Parse(arr[3]));
    }

    //
    public static string ToString(bool b)
    {
        return b ? "1" : "0";
    }
    public static bool GetBool(string text)
    {
        return text == "1";
    }

    public static int ToInt(string str)
    {
        int n;
        if (!int.TryParse(str, out n)) n = 0;
        return n;
    }

    public static float ToFloat(string str)
    {
        float n;
        if (!float.TryParse(str, out n)) n = 0;
        return n;
    }

    public static long ToLong(string str)
    {
        long l;
        if (!long.TryParse(str, out l)) l = 0L;
        return l;
    }

    public static ulong ToULong(string str)
    {
        ulong l;
        if (!ulong.TryParse(str, out l)) l = 0L;
        return l;
    }
}
