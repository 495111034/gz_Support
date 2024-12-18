
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    public class MyDragHandler : MonoBehaviour, IBeginDragHandler,IDragHandler,IEndDragHandler,IDropHandler
    {

        void Start() { }

        public void OnBeginDrag(PointerEventData eventData)
        {
            SendMessageUpwards("__OnBeginDrag", eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            SendMessageUpwards("__OnDrag", eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            SendMessageUpwards("__OnEndDrag", eventData);
        }

        public void OnDrop(PointerEventData eventData)
        {
            SendMessageUpwards("__OnDrop", eventData);
        }
    }
}
