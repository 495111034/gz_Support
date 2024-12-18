using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Task 特效
/// </summary>
public static class TaskEffect
{
    //
    public static IEnumerator FromTo(float from, float to, float speed, Action<float> set_value_handler)
    {
        var value = from;
        while (true)
        {
            yield return null;
            value = Mathf.MoveTowards(value, to, Time.deltaTime * speed);
            set_value_handler(value);
            if (value == to) break;
        }
    }
}
