using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
    public class ScrollOnDragCell : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IScrollHandler
    {
        public LoopScrollRect m_loopScrollRect = null;
        private LoopScrollRect loopScrollRect
        {
            get
            {
                if (m_loopScrollRect == null)
                {
                    m_loopScrollRect = gameObject.GetComponentInParent<LoopScrollRect>();
                }
                return m_loopScrollRect;
            }
        }
#if UNITY_EDITOR
        private void OnEnable()
        {
            var r = loopScrollRect;
        }
#endif
        public void OnBeginDrag(PointerEventData eventData)
        {
            loopScrollRect.OnBeginDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            loopScrollRect.OnDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            loopScrollRect.OnEndDrag(eventData);
        }

        public void OnScroll(PointerEventData eventData)
        {
            loopScrollRect.OnScroll(eventData);
        }
    }
}
