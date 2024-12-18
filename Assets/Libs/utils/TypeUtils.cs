using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

//
public class FieldInfoEx
{
    MemberInfo _info;
    FieldInfo _fi;
    PropertyInfo _pi;

    public FieldInfoEx(MemberInfo info)
    {
        _info = info;
        _fi = info as FieldInfo;
        _pi = info as PropertyInfo;
    }

    public string Name
    {
        get { return _info.Name; }
    }

    public Type FieldType
    {
        get { return _fi != null ? _fi.FieldType : _pi.PropertyType; }
    }

    public void SetValue(object obj, object value)
    {
        if (_fi != null)
        {
            _fi.SetValue(obj, value);
        }
        else if (_pi != null)
        {
            _pi.SetValue(obj, value, null);
        }
    }

    public object GetValue(object obj)
    {
        if (_fi != null)
        {
            return _fi.GetValue(obj);
        }
        else if (_pi != null)
        {
            return _pi.GetValue(obj, null);
        }
        return null;
    }
}


//
public static class TypeUtils
{
    public static FieldInfoEx GetFieldEx(this Type type, string name)
    {
        MemberInfo info = type.GetField(name);
        if (info == null) info = type.GetProperty(name);
        if (info == null) return null;

        return new FieldInfoEx(info);
    }


    // 简单拷贝, 仅复制 简单类型 成员
    public static void SimpleCopy(object src, object dst)
    {
        foreach (var fi in src.GetType().GetFields())
        {
            var ftype = fi.FieldType;
            if (ftype.IsPrimitive || ftype == typeof(string))
            {
                var value = fi.GetValue(src);
                fi.SetValue(dst, value);
            }
        }
        foreach (var fi in src.GetType().GetProperties())
        {
            var ftype = fi.PropertyType;
            if (ftype.IsPrimitive || ftype == typeof(string))
            {
                var value = fi.GetValue(src, null);
                fi.SetValue(dst, value, null);
            }
        }
    }
}
