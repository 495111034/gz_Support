
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/MyPassEvent")]
    public class MyPassEvent : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
    {
        List<RaycastResult> results = new List<RaycastResult>();
        //把事件透下去
        public void PassEvent<T>(PointerEventData data, ExecuteEvents.EventFunction<T> function)
            where T : IEventSystemHandler
        {
            results.Clear();
            EventSystem.current.RaycastAll(data, results);
            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].gameObject.GetComponent<MyPassEvent>() == null)
                {
                    ExecuteEvents.Execute(results[i].gameObject, data, function);
                }
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            PassEvent(eventData, ExecuteEvents.pointerDownHandler);
        }

        //监听抬起
        public void OnPointerUp(PointerEventData eventData)
        {
            PassEvent(eventData, ExecuteEvents.pointerUpHandler);
        }

        //监听点击
        public void OnPointerClick(PointerEventData eventData)
        {
            PassEvent(eventData, ExecuteEvents.submitHandler);
            PassEvent(eventData, ExecuteEvents.pointerClickHandler);
        }

        public void OnDrag(PointerEventData eventData)
        {
            PassEvent(eventData, ExecuteEvents.dragHandler);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            PassEvent(eventData, ExecuteEvents.beginDragHandler);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            PassEvent(eventData, ExecuteEvents.endDragHandler);
        }
    }
}
