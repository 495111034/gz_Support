#if ! SERVER
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// 动态数组
/// </summary>
/// <typeparam name="T"></typeparam>
public class DynArray<T>
{
    T[] _array;

    //
    public DynArray()
    {
        _array = new T[32];
    }

    // 增长
    public void Grow(int min_size)
    {
        if (_array.Length < min_size)
        {
            int size = MathUtils.MakePowerOfTow(min_size);
            var arr2 = new T[size];
            for (int i = 0; i < _array.Length; i++)
            {
                arr2[i] = _array[i];
            }
            _array = arr2;
        }
    }

    public T[] Array
    {
        get { return _array; }
    }

    public int MaxLength
    {
        get { return _array.Length; }
    }
}
#endif