//简单的双向队列
//可以快速地在队列两端进行添加和删除元素操作
//删除元素时必须确保队列非空
//通过下标获取元素时必须确保该下标小于队列大小

using System.Diagnostics;

namespace System.Collections.Generic
{
    //简单的双向队列
    public class DequeLite<T>
    {
        //头部索引，base 0
        int _start = 0;

        //队列元素个数
        int _count = 0;

        //队列缓存
        T[] _objs;
        //缓存大小
        int _capacity;

        private void _copy_to(T[] to, int cnt)
        {
            var _end = _start + cnt;
            var _over = _end - _capacity;
            if (_over <= 0)
            {
                Array.Copy(_objs, _start, to, 0, cnt);
            }
            else
            {
                Array.Copy(_objs, _start, to, 0, cnt - _over);
                Array.Copy(_objs, 0, to, cnt - _over, _over);
            }
        }
        private void _realloc()
        {
            int _new_capacity = 1 + (int)(_capacity * 1.2f);
            Log.LogWarning($"_realloc {_capacity} -> {_new_capacity}");
            var _new_objs = new T[_new_capacity];
            _copy_to(_new_objs, _count);
            _objs = _new_objs;
            _start = 0;
            _capacity = _new_capacity;
        }


        public DequeLite(int reserve = 0)
        {
            if (reserve < 16)
            {
                reserve = 16;
            }
            _capacity = reserve;
            _objs = new T[_capacity];
        }

        public int Count
        {
            get { return _count; }
        }


        //下标操作符重载，base 0
        public T this[int index]
        {
            //根据下标快速索引获取 元素
            get
            {
                Debug.Assert(index >= 0);
                Debug.Assert(index < _count);
                int _pos = _start + index;
                if (_pos >= _capacity)
                {
                    _pos -= _capacity;
                }
                return _objs[_pos];
            }

            set
            {
                Debug.Assert(index >= 0);
                Debug.Assert(index < _count);
                int _pos = _start + index;
                if (_pos >= _capacity)
                {
                    _pos -= _capacity;
                }
                _objs[_pos] = value;
            }
        }

        //获取头部元素
        public T front()
        {
            return this[0];
        }

        //在头部追加元素
        public void push_front(T e)
        {
            if (_count == _capacity)
            {
                _realloc();
            }
            if (++_count > 1)
            {
                if (_start == 0)
                {
                    //循环
                    _start = _capacity - 1;
                }
                else
                {
                    //前移
                    --_start;
                }
            }
            else
            {
                //第一个元素
                _start = 0;
            }
            _objs[_start] = e;
        }

        //删除头部元素并返回
        public T pop_front()
        {
            Debug.Assert(_count > 0);
            --_count;
            //后移
            int _pos = _start++;
            if (_start == _capacity || _count == 0)
            {
                _start = 0;
            }
            var e = _objs[_pos];
            _objs[_pos] = default;
            return e;
        }


        //获取尾端元素
        public T back()
        {
            return _objs[_count - 1];
        }

        //在尾端追加元素
        public void push_back(T e)
        {
            if (_count == _capacity)
            {
                _realloc();
            }
            int _pos = _start + _count++;
            if (_pos >= _capacity)
            {
                _pos -= _capacity;
            }
            _objs[_pos] = e;
        }

        //删除尾端元素并返回
        public T pop_back()
        {
            Debug.Assert(_count > 0);

            int _pos = _start + (--_count);
            if (_pos >= _capacity)
            {
                _pos -= _capacity;
            }
            if (_count == 0)
            {
                _start = 0;
            }
            var e = _objs[_pos];
            _objs[_pos] = default;
            return e;
        }

        public bool Contains(T e)
        {
            int idx = _start;
            for (int i=0;i<_count;++i)
            {
                if (Object.Equals(e, _objs[idx++]))
                {
                    return true;
                }
                if (idx == _capacity)
                {
                    idx = 0;
                }
            }
            return false;
        }

        //清空队列
        public void Clear()
        {
            if (_count > 0)
            {
                var _end = _start + _count;
                var _over = _end - _capacity;
                if (_over <= 0)
                {
                    Array.Clear(_objs, _start, _count);
                }
                else
                {
                    Array.Clear(_objs, _start, _count - _over);
                    Array.Clear(_objs, 0, _over);
                }
            }
            _start = 0;
        }

        //队列转换成数组
        public T[] ToArray()
        {
            var objs = new T[_count];
            _copy_to(objs, _count);
            return objs;
        }

        //复制
        public void CopyTo(T[] to)
        {
            _copy_to(to, _count);
        }

        public override string ToString()
        {
            return string.Format("_count={0}, _start={1}, _end={2}, _capacity={3}", _count, _start, (_start + _count) % _capacity, _capacity);
        }
    }
}
