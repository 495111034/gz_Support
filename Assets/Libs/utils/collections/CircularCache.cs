using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;



/**
 * 简单的循环缓存
 * 		. 用循环数组实现, 类似队列
 * 		. 可快速 分配/释放
 * 			. 分配时, 从 tail 位置查找空闲元素, 如果无空闲, 则释放最早的 head 位置
 * 			. 释放时, 删除 head 位置
 * 		. 同时可分配个数是个定值 
 */
public class CircularCache<T> where T : class, new()
{
    T[] _queue;        // 循环数组
    int _max;
    int _head;          // _head 指向第一个元素, _tail 指向第一个空位子, 当 _head==_tail 时表示空 
    int _tail;
    int _count;
    T[] _array;        // 平坦数组

    //
    public CircularCache(int max)
    {
        resize(max);
    }

    // 重新初始化
    public void resize(int max)
    {
        if (max < 2)
        {
            throw new ArgumentException();
        }
        _queue = new T[max];
        for (int i = 0; i < max; i++)
        {
            if (_queue[i] == null) _queue[i] = new T();
        }
        _max = max;
        clear();
    }

    // 清空
    public void clear()
    {
        _head = _tail = _count = 0;
        _array = null;
    }

    // 获取原始数组
    public T[] getQueue()
    {
        return _queue;
    }

    // 获取平坦数组
    public T[] getArray()
    {
        if (_array == null && _count > 0)
        {
            if (_head < _tail)
            {
                //_array = _queue.slice( _head, _tail );
                _array = _queue.Skip(_head).Take(_count).ToArray();
            }
            else
            {
                //_array = _queue.slice( _head, _max ).concat( _queue.slice( 0, _tail ) );
                _array = _queue.Skip(_head).Take(_max - _head).Concat(_queue.Take(_tail)).ToArray();
            }
        }
        return _array;
    }

    // 获取内容个数
    public int getCount()
    {
        return _count;
    }

    // 从末尾分配元素
    public T allocTail()
    {
        T ret = _queue[_tail];
        _count++;
        _tail = (_tail + 1) % _max;
        if (_head == _tail)
        {
            _head = (_head + 1) % _max;
            _count--;
        }
        _array = null;
        return ret;
    }

    // 释放开始的元素
    public void freeHead()
    {
        freeHead(1);
    }
    public void freeHead(int count)
    {
        if (count >= _count)
        {
            clear();
        }
        else if (count > 0)
        {
            _head = (_head + count) % _max;		// 无法追上 _tail
            _array = null;
        }
    }
}
