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
        /// ����������������ָ�� prefab ������
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
            // offset ����ȡ����������� refill ʱ��΢���� sizeFilled ����������¶ഴ��һ��Item������ƫ��
            this.RefillCells(LeftIndex, contentOffset: (int)offset); 
        }

        protected override void ClearTempPool()
        {
        }
    }
}