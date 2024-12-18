using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.UI
{
    /// <summary>
    /// 多 Prefab 数据源（缓存+实例创建）
    /// 参考 LoopScrollRect 的 InitOnStartMulti
    /// 测试中，修改频繁，不要写死
    /// </summary>
    public class NLoopScrollMultiPrefabSource : MonoBehaviour, NLoopScrollPrefabSource
    {
        public NLoopScrollRectMulti m_LoopScrollRect;

        // Cell MulitiPrefab
        public List<GameObject> m_ItemList = new List<GameObject>();

        [HideInInspector]
        public string m_ClickUniqueID = "";
        [HideInInspector]
        public object m_ClickObject;

        protected virtual void Awake()
        {
            m_LoopScrollRect = GetComponent<NLoopScrollRectMulti>();
            m_LoopScrollRect.prefabSource = this;
            for (int i = 0; i < m_ItemList.Count; i++)
            {
                m_ItemList[i].gameObject.SetActiveX(false);
            }
        }

        // Implement your own Cache Pool here. The following is just for example.
        Stack<Transform> pool = new Stack<Transform>();
        Dictionary<string, Stack<Transform>> m_Pool_Type = new Dictionary<string, Stack<Transform>>();

        public virtual GameObject GetObject(int index)
        {
            Transform candidate = null;
            NScrollIndexCallbackBase TempScrollIndexCallbackBase = null;
            // Is Use MulitiPrefab
            // Cell MulitiPrefab, Get Cell Preferred Type by custom data
            int CellTypeIndex = m_LoopScrollRect.GetCellPreferredTypeIndex(index);
            if (m_ItemList.Count <= CellTypeIndex)
            {
                Debug.LogWarningFormat("TempPrefab is null! CellTypeIndex: {0}", CellTypeIndex);
                return null;
            }
            var TempPrefab = m_ItemList[CellTypeIndex];
            string tempPrefabName = TempPrefab.name;

            Stack<Transform> TempStack = null;
            if (!m_Pool_Type.TryGetValue(tempPrefabName, out TempStack))
            {
                TempStack = new Stack<Transform>();
                m_Pool_Type.Add(tempPrefabName, TempStack);
            }

            if (TempStack.Count == 0)
            {
                candidate = Instantiate(TempPrefab).GetComponent<Transform>();
                candidate.gameObject.name = tempPrefabName;
                TempScrollIndexCallbackBase = candidate.GetComponent<NScrollIndexCallbackBase>();

                // Try Add MissingComponent
                if (TempScrollIndexCallbackBase == null)
                {
                    TempScrollIndexCallbackBase = candidate.gameObject.AddComponent<NScrollIndexCallbackBase>();
                }

                if (null != TempScrollIndexCallbackBase)
                {
                    TempScrollIndexCallbackBase.PrefabName = tempPrefabName;
                    TempScrollIndexCallbackBase.PrefabIndex = CellTypeIndex;
                }
            }
            else
            {
                candidate = TempStack.Pop();
                candidate.gameObject.SetActive(true);
            }

            //TempScrollIndexCallbackBase = candidate.gameObject.GetComponent<NScrollIndexCallbackBase>();
            //if (null != TempScrollIndexCallbackBase)
            //{
            //    TempScrollIndexCallbackBase.SetUniqueID(m_LoopListBank.GetLoopListBankData(index).UniqueID);
            //    TempScrollIndexCallbackBase.onClick_InitOnStart.RemoveAllListeners();
            //    TempScrollIndexCallbackBase.onClick_InitOnStart.AddListener(() => OnButtonScrollIndexCallbackClick(TempScrollIndexCallbackBase, index, TempScrollIndexCallbackBase.GetContent(), TempScrollIndexCallbackBase.GetUniqueID()));
            //}

            return candidate.gameObject;
        }

        public virtual void ReturnObject(Transform trans)
        {
            trans.SendMessage("ScrollCellReturn", SendMessageOptions.DontRequireReceiver);
            trans.gameObject.SetActive(false);
            trans.SetParent(transform, false);
            // Is Use MulitiPrefab
            // Use PrefabName as Key for Pool Manager
            NScrollIndexCallbackBase TempScrollIndexCallbackBase = trans.GetComponent<NScrollIndexCallbackBase>();
            if (null == TempScrollIndexCallbackBase)
            {
                // Use `DestroyImmediate` here if you don't need Pool
                DestroyImmediate(trans.gameObject);
                return;
            }

            Stack<Transform> TempStack = null;
            if (m_Pool_Type.TryGetValue(TempScrollIndexCallbackBase.PrefabName, out TempStack))
            {
                TempStack.Push(trans);
            }
            else
            {
                TempStack = new Stack<Transform>();
                TempStack.Push(trans);

                m_Pool_Type.Add(TempScrollIndexCallbackBase.PrefabName, TempStack);
            }
        }

        public virtual NScrollIndexCallbackBase GetScrollIndexCallbackByIndex(int idx)
        {
            foreach (var TempScrollIndexCallback in m_LoopScrollRect.content.GetComponentsInChildren<NScrollIndexCallbackBase>())
            {
                if (TempScrollIndexCallback.IndexID == idx)
                {
                    return TempScrollIndexCallback;
                }
            }
            return null;
        }
    }
}