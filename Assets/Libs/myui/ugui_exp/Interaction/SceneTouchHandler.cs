using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;



namespace UnityEngine.UI
{
    public enum SceneEventType
    {
        LongPress = 1,
        Click = 2,
        Drag = 3,
        PressDown = 4,
        pressUp = 5,
        Scroll = 6,
        
    }


    public class SceneTouchHandler : Selectable, IPointerLongpressHandler, EventSystems.IPointerClickHandler, IBeginDragHandler, IDragHandler, IInitializePotentialDragHandler, IEndDragHandler, IScrollHandler
    {
        
        public PointerEventData pressingEventData => null;

        private bool isDrag = false;
        private bool isScale = false;
        

        private Dictionary<int, PointerEventData> _dragPoints = new Dictionary<int, PointerEventData>();

        public event Action<SceneEventType, PointerEventData> EventOnSceneTouchHandler;
        public event Action<float> OnEnlarge;



        public void OnLongPressRepeat(PointerEventData eventData)
        {
            if (!Application.isPlaying) return;
            if (isDrag) return;

            //_pressingEventData = eventData;
            EventOnSceneTouchHandler?.Invoke(SceneEventType.LongPress, eventData);
            //_pressingEventData = null;
        }

        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (!Application.isPlaying) return;
            if (isDrag) return;
          

            EventOnSceneTouchHandler?.Invoke(SceneEventType.Click, eventData);
        }


        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (!Application.isPlaying) return;           
        }

        float lastDis = 0;
        void EnlargeStart()
        {
            var points = new PointerEventData[_dragPoints.Count];
            _dragPoints.Values.CopyTo(points, 0);
            lastDis = Vector2.Distance(_dragPoints[0].position, _dragPoints[1].position);

            isScale = true;
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (!Application.isPlaying) return;           

            _dragPoints[eventData.pointerId] = eventData;
            
            if (_dragPoints.Count == 1)
            {               
                isScale = false;
                lastDis = 0;

                isDrag = true;
                EventOnSceneTouchHandler?.Invoke(SceneEventType.Drag, eventData);
            }
            else if (_dragPoints.Count >= 2 )
            {
                isDrag = false;
                var points = new PointerEventData[_dragPoints.Count];
                _dragPoints.Values.CopyTo(points, 0);

                if (!isScale)
                {
                    EnlargeStart();
                }
                else
                {                     
                    float newDis = Vector2.Distance(_dragPoints[0].position, _dragPoints[1].position);
                    OnEnlarge?.Invoke((lastDis - newDis));
                    lastDis = newDis;
                }

            }            
        }



        public virtual void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (!Application.isPlaying) return;
            eventData.useDragThreshold = false;
        }


        public override void OnPointerDown(PointerEventData eventData)
        {
            if (!Application.isPlaying) return;
           

            EventOnSceneTouchHandler?.Invoke(SceneEventType.PressDown, eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (!Application.isPlaying) return;
            

            EventOnSceneTouchHandler?.Invoke(SceneEventType.pressUp, eventData);
        }

        void _onClickOnBlank(MonoBehaviour behaviour)
        {

        }

        public void OnEndDrag(PointerEventData eventData)
        {            
            if (!Application.isPlaying) return;

            _dragPoints.Clear();

            if (_dragPoints.Count < 2)
            {              
                isScale = false;
                lastDis = 0;
            }
            if (_dragPoints.Count == 0)
            {                
                isDrag = false;
            }
           
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (!Application.isPlaying) return;
            EventOnSceneTouchHandler?.Invoke(SceneEventType.Scroll, eventData);
        }
        
    }

}
