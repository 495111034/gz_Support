using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Object 引用, 使不释放
/// </summary>
public class ObjRefBehaviour : MonoBehaviour
{
    List<object> _list = new List<object>();
#if UNITY_EDITOR
    public List<string> _names = new List<string>();
    public int _listHash;
#endif

    private void Start()
    {
        this.enabled = false;
#if UNITY_EDITOR
        this._listHash = _list.GetHashCode();
#endif
    }

    public void CloneRef(ObjRefBehaviour other) 
    {
        _list.Clear();
        _list.AddRange(other._list);
#if UNITY_EDITOR
        _names.Clear();        
        _names.AddRange(other._names);
        this._listHash = _list.GetHashCode();
#endif
    }


    // 添加引用
    public void AddRef(object obj)
    {
        if (obj != null)
        {
            if (!_list.Contains(obj))
            {
                _list.Add(obj);
#if UNITY_EDITOR
                _names.Add(obj.ToString());
#endif
            }
        }
    }

    public bool HasRef<T>() 
    {
        for (var i = _list.Count - 1; i >= 0; --i) 
        {
            if (_list[i] is T) 
            {
                return true;
            }
        }
        return false;
    }

    public T GetRef<T>() where T : class
    {
        for (var i = _list.Count - 1; i >= 0; --i)
        {
            if (_list[i] is T t)
            {
                return t;
            }
        }
        return null;
    }

    private void OnDestroy()
    {
        _list.Clear();
#if UNITY_EDITOR
        _names.Clear();
#endif
    }
}
