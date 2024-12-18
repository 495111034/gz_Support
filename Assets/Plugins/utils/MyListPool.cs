using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;



public static class MyListPool<T>
{
    // Object pool to avoid allocations.
    private static readonly MyObjectPool<List<T>> s_ListPool = new MyObjectPool<List<T>>(null, l => l.Clear());

    public static List<T> Get()
    {
        return s_ListPool.Get();
    }

    public static void Release(List<T> toRelease)
    {
        s_ListPool.Release(toRelease);
    }

    public static int totalCount
    {
        get
        {
            return s_ListPool.countAll;
        }
    }
}

public static class MyObjectPools<T>  where T : class, new()
{
    private static readonly MyObjectPool<T> s_ListPool = new MyObjectPool<T>(null, null);
    public static T Get()
    {
        return s_ListPool.Get();
    }
    public static void Release(T toRelease)
    {
        if (toRelease != null)
        {
            s_ListPool.Release(toRelease);
        }
        else
        {
            Log.LogError($"toRelease is null, T={typeof(T)}");
        }
    }
    public static int totalCount
    {
        get
        {
            return s_ListPool.countAll;
        }
    }
}


