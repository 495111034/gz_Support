using System;
using System.Collections.Generic;

namespace poolcaches 
{
    public interface WeakDictionaryValue
    {
        LinkedListNode<WeakDictionaryValue> WeakKeyNode { get; set; }
    }

    public class WeakDictionary<K, V> where V : class, WeakDictionaryValue
    {

        int _caches_size = 0;
        LinkedList<WeakDictionaryValue> _caches;

        Dictionary<K, WeakReference> _weaks = new Dictionary<K, WeakReference>();
        List<K> _keys = new List<K>();
        Dictionary<K, WeakReference>.Enumerator _update;

        public int Count => _weaks.Count;

        public List<K> Keys
        {
            get
            {
                if (_keys.Count != _weaks.Count)
                {
                    _keys.Clear();
                    _keys.AddRange(_weaks.Keys);
                }
                return _keys;
            }
        }

        public WeakDictionary(int cache) 
        {
            _caches_size = cache;
            if (_caches_size > 0) 
            {
                _caches = new LinkedList<WeakDictionaryValue>();
            }
        }


        void _update_lru(V v)
        {
            var _caches_size = this._caches_size;
            //
            if (_caches_size > 0)
            {
                var _caches = this._caches;

                var node = v.WeakKeyNode;
                if (node != null && node.Value == v)
                {
                    if (node != _caches.First)
                    {
                        _caches.Remove(node);
                        _caches.AddFirst(node);
                    }
                    return;
                }

                if (_caches.Count >= _caches_size)
                {
                    node = _caches.Last;
                    if (node != _caches.First)
                    {
                        _caches.Remove(node);
                        _caches.AddFirst(node);
                    }
                    node.Value = v;
                    v.WeakKeyNode = node;
                }
                else
                {
                    v.WeakKeyNode = _caches.AddFirst(v);
                }
            }
        }

        public bool TryGetValue(K key, out V v)
        {
            var _weaks = this._weaks;
            if (!_weaks.TryGetValue(key, out var w) || !w.IsAlive)
            {
                if (w != null) 
                {
                    //UnityEngine.Debug.Log("Remove1");
                    _weaks.Remove(key);
                    _keys.Clear();
                }
                v = null;
                return false;
            }
            v = w.Target as V;
            _update_lru(v);
            return true;
        }

        public V Add(K key, V v) 
        {
            //UnityEngine.Debug.Log("Add");
            _weaks.Add( key, new WeakReference(v) );
            _keys.Clear();
            _update_lru(v);
            return v;
        }
     

        public void RemoveByKey(K key) 
        {
            if (_weaks.TryGetValue(key, out var w)) 
            {
                //UnityEngine.Debug.Log("Remove2");
                _weaks.Remove(key);
                _keys.Clear();
            }
        }

        List<K> _dels = new List<K>();
        int _loop = 1;
        public void UpdateRemoveUnAlive(string tag) 
        {
            try
            {
                if (_dels.Count > 0)
                {
                    _dels.Clear();
                }
                for (var i = 0; i < _loop; ++i)
                {
                    if (_update.MoveNext())
                    {
                        var kv = _update.Current;
                        if (!kv.Value.IsAlive)
                        {
                            _dels.Add(kv.Key);
                        }
                    }
                    else
                    {
                        _loop = 1;
                        if (tag != null)
                        {
                            Log.LogInfo($"{tag} UpdateRemoveUnAlive GetEnumerator after MoveNext");
                        }
                        if (_dels.Count == 0)
                        {
                            _update = _weaks.GetEnumerator();
                        }
                        break;
                    }
                }
            }
            catch 
            {
                _loop = 100;
                if (tag != null)
                {
                    Log.LogInfo($"{tag} UpdateRemoveUnAlive GetEnumerator after catch");
                }
                if (_dels.Count == 0)
                {
                    _update = _weaks.GetEnumerator();
                }
            }

            if (_dels.Count > 0)
            {
                _loop = 100;
                foreach (var Key in _dels)
                {
                    if (tag != null)
                    {
                        Log.LogInfo($"{tag} UpdateRemoveUnAlive remove {Key}");
                    }
                    _weaks.Remove(Key);
                }
                _update = _weaks.GetEnumerator();
                _dels.Clear();
                _keys.Clear();
            }

        }
        //public Dictionary<K, WeakReference>.KeyCollection Keys => _weaks.Keys;
    }


}