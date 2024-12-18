//#define TRACE_LOG
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;


public class ObjectArrayT<T> where T : Component
{
    static object locker = new object();
    class ObjectArrayCacheT
    {
        internal List<T> ObjectList = new List<T>();
        internal List<Vector3> PositionList = new List<Vector3>();
        internal List<Quaternion> rotationList = new List<Quaternion>();
        internal void ClearArray()
        {
            ObjectList.Clear();
            PositionList.Clear();
            rotationList.Clear();
        }

        public ObjectArrayCacheT()
        {
#if TRACE_LOG
            Log.LogWarning($"{typeof(T)}: new ObjectArrayCacheT={GetHashCode()}");
#endif
        }

        ~ObjectArrayCacheT() 
        {
            //Log.LogError($"{typeof(T)}: del ObjectArrayCacheT={GetHashCode()}");
        }
    }
    ObjectArrayCacheT cache;

    public List<T> ObjectList 
    {
        get
        {
            if (isInCache != 0)
                Log.LogError($"{typeof(T)}: Access array1={GetHashCode()}, isInCache={isInCache}");
            return cache?.ObjectList;
        }
    }
    public List<Vector3> PositionList
    {
        get
        {
            if (isInCache != 0)
                Log.LogError($"{typeof(T)}: Access array2={GetHashCode()}, isInCache={isInCache}");
            return cache?.PositionList;
        }
    }
    public List<Quaternion> rotationList
    {
        get
        {
            if (isInCache != 0)
                Log.LogError($"{typeof(T)}: Access array3={GetHashCode()}, isInCache={isInCache}");
            return cache?.rotationList;
        }
    }
    public bool IsEmpty
    {
        get
        {
            if (isInCache != 0)
                Log.LogError($"{typeof(T)}: Access array4={GetHashCode()}, isInCache={isInCache}");
            return ObjectList == null || ObjectList.Count == 0;
        }
    }
    public int Count
    {
        get
        {
            if (isInCache != 0)
                Log.LogError($"{typeof(T)}: Access array5={GetHashCode()}, isInCache={isInCache}");
            return ObjectList == null ? 0 : ObjectList.Count;
        }
    }

    public int isInCache = 1;
    public int refcnt = 0;
    public int hash => GetHashCode();

    public ObjectArrayT()
    {
#if TRACE_LOG
        Log.LogWarning($"{typeof(T)}: new array={GetHashCode()}");
#endif
    }
    ~ObjectArrayT()
    {
#if TRACE_LOG
        Log.LogWarning($"{typeof(T)}: del array={GetHashCode()}, cache={cache?.GetHashCode()}, refcnt={refcnt}, isInCache={isInCache}");
#endif
        Clear();
    }

    void Clear()
    {
        if (cache != null)
        {
            cache.ClearArray();
            lock (locker)
            {
                MyObjectPools<ObjectArrayCacheT>.Release(cache);
            }
            cache = null;
        }
    }

    void init(List<T> objs)
    {
        if (objs.Count > 0)
        {
            if (cache == null)
            {
                lock (locker)
                {
                    cache = MyObjectPools<ObjectArrayCacheT>.Get();
#if TRACE_LOG
                    Log.LogInfo($"{typeof(T)}: Get cache={cache.GetHashCode()}, array={GetHashCode()}");
#endif
                }
            }
            for (int i = 0; i < objs.Count; ++i)
            {
                var obj = objs[i];
                if (!obj) continue;
                ObjectList.Add(obj);
                PositionList.Add(obj.transform.position);
                rotationList.Add(obj.transform.rotation);
            }
        }
    }

    protected static V _Get<V>(ref List<T> tmp)  where V : ObjectArrayT<T> , new()
    {
        V array;
        lock (locker)
        {
            array = MyObjectPools<V>.Get();
        }
#if UNITY_EDITOR
        if (array.isInCache == 0)
        {
            Log.LogError($"{typeof(T)}: _Get1 array={array.GetHashCode()}, isInCache is 0");
        }
#endif
        array.refcnt = 0;
        array.isInCache = 0;
#if TRACE_LOG
        Log.LogInfo($"{typeof(T)}: Get array1={array.GetHashCode()}");
#endif
        array.init(tmp);
        MyListPool<T>.Release(tmp);
        tmp = null;
        return array;
    }

    protected static V _Get<V>(T obj) where V : ObjectArrayT<T>, new()
    {
        V array;
        lock (locker)
        {
            array = MyObjectPools<V>.Get();
        }
#if UNITY_EDITOR
        if (array.isInCache == 0)
        {
            Log.LogError($"{typeof(T)}: _Get2 array={array.GetHashCode()}, isInCache is 0");
        }
#endif
        array.refcnt = 0;
        array.isInCache = 0;
#if TRACE_LOG
        Log.LogInfo($"{typeof(T)}: Get array2={array.GetHashCode()}");
#endif
        var tmp = MyListPool<T>.Get();
        tmp.Add(obj);
        array.init(tmp);
        MyListPool<T>.Release(tmp);        
        return array;
    }
    protected static V _Get<V>() where V : ObjectArrayT<T>, new()
    {
        V array;
        lock (locker)
        {
            array = MyObjectPools<V>.Get();
        }
#if UNITY_EDITOR
        if (array.isInCache == 0)
        {
            Log.LogError($"{typeof(T)}: _Get3 array={array.GetHashCode()}, isInCache is 0");
        }
#endif
        array.refcnt = 0;
        array.isInCache = 0;
#if TRACE_LOG
        Log.LogInfo($"{typeof(T)}: Get array3={array.GetHashCode()}");
#endif
        return array;
    }
    protected static bool _Release<V>(V array) where V : ObjectArrayT<T>, new()
    {
        if (array != null)
        {
            if (--array.refcnt == 0)
            {
                if (++array.isInCache > 1)
                {
#if TRACE_LOG
                    Log.LogInfo($"{typeof(T)}: fail Release array={array.GetHashCode()}, isInCache={array.isInCache}");
#endif
                }
                else
                {
#if TRACE_LOG
                    Log.LogInfo($"{typeof(T)}: Release array={array.GetHashCode()}");
#endif
                    //array.Clear();
                    array.cache?.ClearArray();
                    lock (locker)
                    {
                        MyObjectPools<V>.Release(array);
                    }                    
                    return true;
                }
            }
            else 
            {
#if TRACE_LOG
                Log.LogInfo($"{typeof(T)}: fail Release array={array.GetHashCode()}, isInCache={array.isInCache}, now refcnt={array.refcnt}");
#endif
            }
        }
        return false;
    } 
}

public class ObjectArray : ObjectArrayT<ObjectBehaviourBase>
{
    public static ObjectArray Get() 
    {
        return _Get<ObjectArray>();
    }
    public static ObjectArray Get(ObjectBehaviourBase obj)
    {
        return _Get<ObjectArray>(obj);
    }
    public static ObjectArray Get(ref List<ObjectBehaviourBase> tmp)
    {
        return _Get<ObjectArray>(ref tmp);
    }
    public static bool Release(ObjectArray array) 
    {
        return _Release(array);
    }
}


public class TransformArray : ObjectArrayT<Transform>
{
    //public static TransformArray Get()
    //{
    //    return _Get<TransformArray>();
    //}
    public static TransformArray Get(Transform obj)
    {
        return _Get<TransformArray>(obj);
    }
    public static TransformArray Get(ref List<Transform> tmp)
    {
        return _Get<TransformArray>(ref tmp);
    }
    public static bool Release(TransformArray array)
    {
        return _Release(array);
    }
}

/*
public class TransformArray 
{
    static object locker = new object();
    class ObjectArrayCache
    {
        internal List<Transform> ObjectList = new List<Transform>();
        internal List<Vector3> PositionList = new List<Vector3>();
        internal List<Quaternion> rotationList = new List<Quaternion>();
        internal void ClearArray()
        {
            ObjectList.Clear();
            PositionList.Clear();
            rotationList.Clear();
        }
    }
    ObjectArrayCache cache;

    public List<Transform> ObjectList => cache?.ObjectList;
    public List<Vector3> PositionList => cache?.PositionList;
    public List<Quaternion> rotationList => cache?.rotationList;

    public bool IsEmpty { get { return ObjectList == null || ObjectList.Count == 0; } }
    public int Count { get { return ObjectList == null ? 0 : ObjectList.Count; } }
    public TransformArray(params Transform[] objs)
    {
        if (objs != null && objs.Length > 0)
        {
            lock (locker)
            {
                cache = MyObjectPools<ObjectArrayCache>.Get();
            }
            for (int i = 0; i < objs.Length; ++i)
            {
                ObjectList.Add(objs[i]);
                PositionList.Add(objs[i].position);
                rotationList.Add(objs[i].rotation);
            }
        }
    }
    ~TransformArray()
    {
        if (cache != null)
        {
            cache.ClearArray();
            lock (locker)
            {
                MyObjectPools<ObjectArrayCache>.Release(cache);
            }
            cache = null;
        }
    }
}
*/
