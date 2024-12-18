using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


namespace UnityEngine.UI
{
    [AddComponentMenu("UI/MyTouchHandler", 1)]
    public class MyTouchHandler : Selectable, IPointerLongpressHandler, EventSystems.IPointerClickHandler, IBeginDragHandler, IDragHandler, IInitializePotentialDragHandler, IEndDragHandler,IScrollHandler
    {
        private bool isDrag = false;
        PointerEventData _pressingEventData;
        public PointerEventData pressingEventData => _pressingEventData;

        public void OnLongPressRepeat(PointerEventData eventData)
        {
            if (isDrag) return;

            if (Application.isPlaying)
            {
                _pressingEventData = eventData;
                SendMessageUpwards("__OnLongPress", this);
                _pressingEventData = null;
            }
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {          
            if (isDrag) return;

           
            
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (Application.isPlaying)
                SendMessageUpwards("OnClickEvent", this);
        }


        public virtual void OnBeginDrag(PointerEventData eventData)
        {
           
            isDrag = true;

            if (!MayDrag(eventData))
                return;
          
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
           
            isDrag = true;
            if (!MayDrag(eventData))
                return;
           
        }

        private bool MayDrag(PointerEventData eventData)
        {
            return IsActive() && IsInteractable() && eventData.button == PointerEventData.InputButton.Left;
        }
       

        public virtual void OnInitializePotentialDrag(PointerEventData eventData)
        {
            
            eventData.useDragThreshold = false;
        }


        public override void OnPointerDown(PointerEventData eventData)
        {
            
            isDrag = false;
            if (!MayDrag(eventData))
                return;
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            
            isDrag = false;
        }

        public void OnScroll(PointerEventData eventData)
        {
            Log.LogError($"OnScroll:{eventData.button},{eventData.scrollDelta}");
        }
    }

}
