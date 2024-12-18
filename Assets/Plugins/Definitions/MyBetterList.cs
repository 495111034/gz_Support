using UnityEngine;
using System;

/// <summary>
/// 自行扩充数组，与List相比没有GC开销，性能更好。
/// 注意，没有线程安全处理
/// </summary>
/// <typeparam name="T"></typeparam>
public class MyBetterList<T>
{
    public T[] buffer;

    public int size = 0;

    private int bufferIncrement = 0;

    public T this[int i]
    {
        get { return buffer[i]; }
        set { buffer[i] = value; }
    }

    public int IndexOf(T t)
    {
        if (buffer == null) return -1;
        return Array.IndexOf(buffer, t, 0, size);
    }
  

    public MyBetterList(int bufferIncrement)
    {
        this.bufferIncrement = Mathf.Max(1, bufferIncrement);
    }

    void AllocateMore()
    {
        T[] newList = (buffer != null) ? new T[buffer.Length + bufferIncrement] : new T[bufferIncrement];
        if (buffer != null && size > 0) buffer.CopyTo(newList, 0);
        buffer = newList;
    }

    public void Clear() { if (size != 0) { size = 0; Array.Fill(buffer, default); } }

    public void Release() { size = 0; buffer = null; }

    public void Add(T item)
    {
        if (buffer == null || size == buffer.Length) AllocateMore();
        buffer[size++] = item;
    }

    /// <summary>
    /// 获得有效的数据,裁剪后面没用到的缓存数据
    /// </summary>
    /// <returns></returns>
    public T[] GetValidRange()
    {
        if (buffer == null || buffer.Length == 0) return null;
        if (size >= buffer.Length) return buffer;
        if (size <= 0) return null;
        

        T[] tmp = new T[size];        
        Array.Copy(buffer, 0, tmp, 0, size);
        return tmp;
    }

    public void AddRange(T[] items)
    {
        if (items == null)
        {
            return;
        }
        int length = items.Length;
        if (length == 0)
        {
            return;
        }

        if (buffer == null)
        {
            buffer = new T[Mathf.Max(bufferIncrement, length)];
            items.CopyTo(buffer, 0);
            size = length;
        }
        else
        {
            if (size + length > buffer.Length)
            {
                T[] newList = new T[Mathf.Max(buffer.Length + bufferIncrement, size + length)];
                buffer.CopyTo(newList, 0);
                items.CopyTo(newList, size);
                buffer = newList;
            }
            else
            {
                items.CopyTo(buffer, size);
            }
            size += length;
        }
    }

    public void RemoveAt(int index)
    {
        if (buffer != null && index > -1 && index < size)
        {
            --size;
            buffer[index] = default(T);
            for (int b = index; b < size; ++b) buffer[b] = buffer[b + 1];
            buffer[size] = default(T);
        }
    }

    /// <summary>
    /// 出栈
    /// </summary>
    /// <returns></returns>
    public T Pop()
    {
        if(buffer == null || size == 0)
        {
            return default(T);
        }
        --size;
        T t = buffer[size];
        buffer[size] = default(T);
        return t;
    }

    /// <summary>
    /// 获取最新（不出栈）
    /// </summary>
    /// <returns></returns>
    public T Peek()
    {
        if (buffer == null || size == 0)
        {
            return default(T);
        }
        return buffer[size - 1];
    }
}
