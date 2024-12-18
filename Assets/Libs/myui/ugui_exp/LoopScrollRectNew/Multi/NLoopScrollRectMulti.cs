using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    [RequireComponent(typeof(NLoopScrollMultiPrefabSource))]
    public abstract class NLoopScrollRectMulti : NLoopScrollRectBase
    {
        //[HideInInspector]
        //[NonSerialized]
        //public NLoopScrollMultiDataSource dataSource = null;

        [HideInInspector]
        [NonSerialized]
        public NLoopScrollPrefabSource prefabSource = null;

        public Func<int, int> GetPrefabIndexEvent = null;

        /// <summary>
        /// 根据数据索引返回指定 prefab 的索引
        /// </summary>
        public int GetCellPreferredTypeIndex(int index)
        {
            if (GetPrefabIndexEvent != null)
            {
                return GetPrefabIndexEvent.Invoke(index);
            }
            return 0;
        }

        protected override void ProvideData(Transform transform, int index)
        {
            //dataSource.ProvideData(transform, index);

            // ===== ILRUNTIME =====
            if (ProvideDataEvent != null)
            {
                ProvideDataEvent(transform, index);
            }
            // ===== ILRUNTIME =====
        }

        // Multi Data Source cannot support TempPool
        protected override RectTransform GetFromTempPool(int itemIdx)
        {
            RectTransform nextItem = prefabSource.GetObject(itemIdx).transform as RectTransform;
            nextItem.transform.SetParent(m_Content, false);
            nextItem.gameObject.SetActive(true);

            ProvideData(nextItem, itemIdx);
            return nextItem;
        }

        protected override void ReturnToTempPool(bool fromStart, int count)
        {
            Debug.Assert(m_Content.childCount >= count);
            if (fromStart)
            {
                for (int i = count - 1; i >= 0; i--)
                {
                    prefabSource.ReturnObject(m_Content.GetChild(i));
                }
            }
            else
            {
                int t = m_Content.childCount - count;
                for (int i = m_Content.childCount - 1; i >= t; i--)
                {
                    prefabSource.ReturnObject(m_Content.GetChild(i));
                }
            }
        }

        public override void RefreshCells()
        {
            int LeftIndex = this.GetFirstItem(out float offset);
            // offset 向下取整，避免出现 refill 时略微超出 sizeFilled 的情况。导致多创建一个Item，发生偏移
            this.RefillCells(LeftIndex, contentOffset: (int)offset); 
        }

        protected override void ClearTempPool()
        {
        }
    }
}