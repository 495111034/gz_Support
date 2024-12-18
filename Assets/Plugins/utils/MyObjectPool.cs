using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using System.Text;
using System.Collections;

public class MyObjectPool_infos
{
    public static Dictionary<string, object> pools = new Dictionary<string, object>();

    public static void get_cache_info(StringBuilder sb)
    {
        sb.Append("MyObjectPool_info:");
        sb.Append(pools.Count);
        sb.Append("\n");
        foreach (var kv in pools)
        {
            var v = kv.Value as ICollection;
            //if (v.Count > 0)
            {
                sb.Append("  ");
                sb.Append(kv.Key);
                sb.Append(":");
                sb.Append(v.Count);
                sb.Append("\n");
            }
        }
        sb.Append("\n");
    }
}


internal class MyObjectPool<T> where T : class, new()
{
    private readonly Stack<T> m_Stack = new Stack<T>();
    private readonly UnityAction<T> m_ActionOnGet;
    private readonly UnityAction<T> m_ActionOnRelease;

#if UNITY_EDITOR
    private readonly Stack<int> m_StackHash = new Stack<int>();
#endif

    public int countAll { get; private set; }
    public int countActive { get { return countAll - countInactive; } }
    public int countInactive { get { return m_Stack.Count; } }

    string profile_name = $"MyObjectPool new {typeof(T)}";

    public MyObjectPool(UnityAction<T> actionOnGet, UnityAction<T> actionOnRelease)
    {
        m_ActionOnGet = actionOnGet;
        m_ActionOnRelease = actionOnRelease;
        //
        MyObjectPool_infos.pools[typeof(T).ToString()] = m_Stack;
    }

    public T Get()
    {
        T element = null;
        while (m_Stack.Count > 0 && element == null) 
        {
            element = m_Stack.Pop();
#if UNITY_EDITOR
            var hash = m_StackHash.Pop();
            var Count = m_StackHash.Count;
#else
            var hash = 0;
            var Count = 0;
#endif
            if (element == null) 
            {
                Log.LogError($"Internal error. Pop object({typeof(T)}) that is null, hash={hash},element={element?.GetHashCode()}, m_Stack={m_Stack.Count},m_StackHash={Count}");
            }
        }
        if (element == null)
        {
            UnityEngine.Profiling.Profiler.BeginSample(profile_name);
            element = new T();
            UnityEngine.Profiling.Profiler.EndSample();
            //
            countAll++;
        }
        m_ActionOnGet?.Invoke(element);
        return element;
    }

    public void Release(T element)
    {
        if (element == null)
        {
            Log.LogError($"Internal error. Trying to Release object({typeof(T)}) that is null");
            return;
        }
        if (Application.isEditor)
        {
            try
            {
                foreach (var e in m_Stack)
                {
                    if (ReferenceEquals(e, element))
                    {
                        Log.LogError($"Internal error. Trying to destroy object({typeof(T)}) that is already released to pool.");
                        return;
                    }
                }
            }
            catch { }
        }
        m_ActionOnRelease?.Invoke(element);
        m_Stack.Push(element);
#if UNITY_EDITOR
        m_StackHash.Push(element.GetHashCode());
#endif
    }
}

