using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SimpleGrass
{

    [System.Serializable]
    public class Data_Matx
    {
        public Matrix4x4[] matrPtr;
        public int dataSize = 0;
        public bool isUsed = false;
    }

    [System.Serializable]
    public class Data_Vector4
    {
        public Vector4[] vector4Ptr;
        public int dataSize = 0;
        public bool isUsed = false;
    }

    [System.Serializable]
    public class Data_Float
    {
        public float[] floatPtr;
        public int dataSize = 0;
        public bool isUsed = false;
    }

    [System.Serializable]
    public class Pool
    {
        public string key;
        public int size;
        public bool expandable = true;
        public int expendLen = 0;
        public int dataLen = 0;

        private Stack<Data_Matx> poolMatx;
        private Stack<Data_Vector4> poolVec4;
        private Stack<Data_Float> poolFloat;
        // private List<Data_Matx> poolMatx;
        //private List<Data_Vector4> poolVec4;        

        public Pool(string keyName, int expendCount, int datasize)
        {
            key = keyName;
            size = expendCount;

            poolMatx = new Stack<Data_Matx>();
            poolVec4 = new Stack<Data_Vector4>();
            poolFloat = new Stack<Data_Float>();

            for (int i = 0; i < expendCount; i++)
            {
                AddItemMatx(datasize);
                AddItemVec4(datasize);
                AddItemFloat(datasize);
            }

            expendLen = expendCount;
            this.dataLen = datasize;
        }

        

        public void Clear()
        {
            if(poolMatx != null)
              poolMatx.Clear();

            if(poolVec4 != null)
               poolVec4.Clear();
            if (poolFloat != null)
                poolFloat.Clear();
        }
        public Data_Matx GetMatx()
        {
            // Is there one ready?
            if(poolMatx.Count > 0)
            {
                Data_Matx ret = poolMatx.Pop();
                ret.isUsed = true;
                return ret;
            }
            //for (int i = 0; i < poolMatx.Count; i++)
            //{
            //    if (!poolMatx[i].isUsed)
            //    {
            //        poolMatx[i].isUsed = true;
            //        return poolMatx[i];
            //    }
            //}

            // Can one be added?
            if (expandable)
            {
                Data_Matx newObj = ExpendMatx();// AddItemMatx(dataLen);
                newObj.isUsed = true;
                return newObj;
            }
            else
            {
                Debug.LogWarning("No available item from pool with key: " + key);
                return null;
            }
        }

        public void RecMatx(Data_Matx data)
        {
            if(data != null)
            {
                data.isUsed = false;
                poolMatx.Push(data);
            }            
        }

        public Data_Vector4 GetVec4()
        {
            // Is there one ready?
            if(poolVec4.Count > 0)
            {
                Data_Vector4 ret = poolVec4.Pop();
                ret.isUsed = true;
                return ret;
            }

            //for (int i = 0; i < poolVec4.Count; i++)
            //{
            //    if (!poolVec4[i].isUsed)
            //    {
            //        poolVec4[i].isUsed = true;
            //        return poolVec4[i];
            //    }
            //}

            // Can one be added?
            if (expandable)
            {
                Data_Vector4 newObj = ExpendVec4();// AddItemVec4(dataLen);
                newObj.isUsed = true;
                return newObj;
            }
            else
            {
                Debug.LogWarning("No available item from pool with key: " + key);
                return null;
            }
        }


        public void RecVector4(Data_Vector4 data)
        {
            if (data != null)
            {
                data.isUsed = false;
                poolVec4.Push(data);
            }
        }


        public Data_Float GetFloat()
        {
            // Is there one ready?
            if (poolFloat.Count > 0)
            {
                Data_Float ret = poolFloat.Pop();
                ret.isUsed = true;
                return ret;
            }
            // Can one be added?
            if (expandable)
            {
                Data_Float newObj = ExpendFloat();
                newObj.isUsed = true;
                return newObj;
            }
            else
            {
                Debug.LogWarning("No available item from pool with key: " + key);
                return null;
            }
        }

        public void RecFloat(Data_Float data)
        {
            if (data != null)
            {
                data.isUsed = false;
                poolFloat.Push(data);
            }
        }

        private Data_Matx ExpendMatx()
        {
           // Data_Matx firtData = null;
            for (int i = 0; i < expendLen; i++)
            {
               // if (i == 0)
                  //  firtData = 
                AddItemMatx(this.dataLen);
               // else
                //    AddItemMatx(this.dataLen);
            }
            return poolMatx.Pop(); ;
        }

        private Data_Vector4 ExpendVec4()
        {
           // Data_Vector4 firtData = null;
            for (int i = 0; i < expendLen; i++)
            {
               // if (i == 0)
               //     firtData = AddItemVec4(this.dataLen);
              //  else
                    AddItemVec4(this.dataLen);
            }
            return poolVec4.Pop();//firtData;
        }


        private Data_Float ExpendFloat()
        {
            for (int i = 0; i < expendLen; i++)
            {
                AddItemFloat(this.dataLen);
            }
            return poolFloat.Pop();
        }

        private Data_Matx AddItemMatx(int datasize)
        {
            int index = poolMatx.Count;
            Data_Matx matx = new Data_Matx();
            matx.dataSize = datasize;
            matx.isUsed = false;
            matx.matrPtr = new Matrix4x4[datasize];
            poolMatx.Push(matx);
            //poolMatx.Add(matx);
            return matx;
        }

        private Data_Vector4 AddItemVec4(int datasize)
        {
            int index = poolVec4.Count;
            Data_Vector4 vec4 = new Data_Vector4();
            vec4.dataSize = datasize;
            vec4.isUsed = false;
            vec4.vector4Ptr = new Vector4[datasize];

            //poolVec4.Add(vec4);
            poolVec4.Push(vec4);
            return vec4;
        }

        private Data_Float AddItemFloat(int datasize)
        {
            int index = poolFloat.Count;
            Data_Float floatPtr = new Data_Float();
            floatPtr.dataSize = datasize;
            floatPtr.isUsed = false;
            floatPtr.floatPtr = new float[datasize];

            poolFloat.Push(floatPtr);
            return floatPtr;
        }
    }

    //public class PoolManager : MonoBehaviour
    //{
    //    public Pool[] pools;

    //    private static Dictionary<string, Pool> cache;
    //    private static PoolManager poolManager;

    //    public static PoolManager instance
    //    {
    //        get
    //        {
    //            if (!poolManager)
    //            {
    //                poolManager = FindObjectOfType(typeof(PoolManager)) as PoolManager;

    //                if (!poolManager)
    //                {
    //                    Debug.LogError("There needs to be one active PoolManger script on a GameObject in your scene.");
    //                }
    //                else
    //                {
    //                    poolManager.Init();
    //                }
    //            }

    //            return poolManager;
    //        }
    //    }

    //    void Init()
    //    {
    //        if (cache == null)
    //        {
    //            cache = new Dictionary<string, Pool>();
    //        }
    //    }

    //    void Start()
    //    {
    //        //if (pools != null)
    //        //{
    //        //    cache = new Dictionary<string, Pool>(pools.Length);

    //        //    for (int i = 0; i < pools.Length; i++)
    //        //    {
    //        //        Pool tempPool = pools[i];
    //        //        cache[tempPool.key] = new Pool(tempPool.key, tempPool.poolObject, tempPool.size, tempPool.parentingGroup, tempPool.expandable);
    //        //    }
    //        //}
    //    }

    //    /// <summary>
    //    /// Grabs the next item from the pool.
    //    /// </summary>
    //    /// <param name="key">Name of the pool to draw from.</param>
    //    /// <returns>Next free item.  Null if none available.</returns>
    //    //public static GameObject Pull(string key)
    //    //{
    //    //    return (cache[key].Pull());
    //    //}

    //    //public static GameObject Pull(string key, Vector3 position, Quaternion rotation)
    //    //{
    //    //    GameObject clone = cache[key].Pull();
    //    //    clone.transform.position = position;
    //    //    clone.transform.rotation = rotation;
    //    //    return clone;
    //    //}
    //}


}