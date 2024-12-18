using UnityEngine;
using System.Collections;

namespace UnityEngine.UI
{
    public interface NLoopScrollPrefabSource
    {
        GameObject GetObject(int index);

        void ReturnObject(Transform trans);
    }
}
