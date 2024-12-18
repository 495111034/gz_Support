using System;
using UnityEngine;

namespace UnityEngine.UI
{
    /// <summary>
    /// 顶点缓存
    /// 
    ///     . UGUI 中使用 List<UIVertex> 来调用 OnFillVBO, 它修改时很耗时
    ///     
    /// </summary>
    public class VertexBuff
    {
        UIVertex[] _arr;
        int _count;

        //
        public UIVertex[] Buff { get { return _arr; } }
        public int Count { get { return _count; } }
        public int MaxCount { get { return _arr.Length; } }

        //
        public VertexBuff(int max_count)
        {
            _arr = new UIVertex[max_count];
            _count = 0;
        }
        
        public void Clear()
        {
            _count = 0;
        }

        public void Add(ref UIVertex v)
        {
            _arr[_count++] = v;
        }

        public void Add(UIVertex[] arr, bool skip_empty_planes)
        {
            // 过滤空白
            if (skip_empty_planes)
            {
                for (int i = 0; i < arr.Length; i += 4)
                {
                    if (arr[i].color.a > 0)
                    {
                        _arr[_count++] = arr[i + 0];
                        _arr[_count++] = arr[i + 1];
                        _arr[_count++] = arr[i + 2];
                        _arr[_count++] = arr[i + 3];
                    }
                }
            }
            // 完全复制
            else
            {
                Array.Copy(arr, 0, _arr, _count, arr.Length);
                _count += arr.Length;
            }
        }

        // 清空某个平面对应的4个顶点
        public void ClearPlane(int i)
        {
            var idx = i * 4;
            var pos = _arr[idx].position;
            _arr[idx + 1].position = pos;
            _arr[idx + 2].position = pos;
            _arr[idx + 3].position = pos;
        }


        public void SetVertices(CanvasRenderer cr)
        {
            cr.SetVertices(_arr, _count);
        }
    }
}