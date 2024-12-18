
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    public class MyImageTextDragable : MyImageText, EventSystems.IDragHandler, EventSystems.IPointerDownHandler, EventSystems.IPointerLongpressHandler
    {
        public PointerEventData pressingEventData => throw new System.NotImplementedException();

        public void OnDrag(PointerEventData eventData)
        {            
            if (Input.touchCount == 2)
            {
                this.rectTransform.localScale *= 1f + eventData.delta.y / Screen.height;
            }
            else 
            {
                this.rectTransform.anchoredPosition += eventData.delta;
            }
            SendMessageUpwards("__OnDrag", eventData);            
        }

        public void OnLongPressRepeat(PointerEventData eventData)
        {
            
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            SendMessageUpwards("__OnPointerDown", this);
        }
    }
}