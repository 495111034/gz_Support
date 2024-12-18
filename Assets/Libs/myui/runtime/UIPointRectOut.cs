
using System;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    //判断按下或者抬起在指定RectTransform区域外触发事件或者隐藏对象
    public class UIPointRectOut : MonoBehaviour
    {
        public enum PointType
        {
            PointDown,
            PointUp,
        }

        public PointType pointType;

        public RectTransform check_rtTrans;
        public GameObject hide_target;

        public float delayExcute = 0;

        public Action EventOnTrigger;

        private Vector2 touchPosition = Vector2.zero;
        private int touchCount = 0;

        private List<RectTransform> ex_checkTrans = null;

        private Canvas canvas = null;
        private bool isExcute = false;
        private bool isPress = false;
        private float ti = 0;

        private void OnEnable()
        {
            ResetStart();
        }

        public void ResetStart()
        {
            isPress = false;
            isExcute = false;
            ti = 0;
            touchCount = GetTouchCount;

            if (canvas == null)
            {
                canvas = gameObject.GetComponentInParent<Canvas>();
            }
        }

        private void Update()
        {
            if (isExcute)
            {
                ti += Time.deltaTime;
                if (ti >= delayExcute)
                {
                    ResetStart();
                    OnTrigger();
                }
                return;
            }
            else if(ti < 0.1f)
            {
                ti += Time.deltaTime;
                touchCount = GetTouchCount;
                return;
            }

            if (pointType == PointType.PointDown || (!isPress && pointType == PointType.PointUp))
            {
                if (touchCount != GetTouchCount)
                {
                    touchCount = GetTouchCount;
                    if (touchCount > 0)
                    {
                        touchPosition = GetTouchPosition(touchCount - 1);
                        if (IsOutRectByMousePos(touchPosition))
                        {
                            if (pointType == PointType.PointUp)
                            {
                                isPress = true;
                            }
                            else
                            {
                                isExcute = true;
                                ti = 0;
                            }
                        }
                    }
                }
            }
            else if (isPress && pointType == PointType.PointUp)
            {
                if (GetTouchCount < touchCount)
                {
                    if (IsOutRectByMousePos(touchPosition))
                    {
                        isExcute = true;
                        ti = 0;
                    }
                }
                if (GetTouchCount > 0) touchPosition = GetTouchPosition(GetTouchCount - 1);
            }
        }

        private int GetTouchCount
        {
            get
            {
#if UNITY_STANDALONE || UNITY_EDITOR
                if (Input.GetMouseButton(0))
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
#else
                return Input.touchCount;
#endif
            }
        }

        private Vector2 GetTouchPosition(int index)
        {
#if UNITY_STANDALONE || UNITY_EDITOR
            return Input.mousePosition;
#else
            return Input.GetTouch(index).position;
#endif
        }

        //检查是否当前位置在指定矩形外
        private bool IsOutRectByMousePos(Vector3 mousePos)
        {
            bool is_point_out = true;
            if (check_rtTrans != null)
            {
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(check_rtTrans, mousePos, canvas.worldCamera, out Vector2 localPoint))
                {
                    is_point_out = !check_rtTrans.rect.Contains(localPoint);
                }
            }
            if (is_point_out)
            {
                if (ex_checkTrans != null)
                {
                    for (int i = 0; i < ex_checkTrans.Count; i++)
                    {
                        if (ex_checkTrans[i] != null && ex_checkTrans[i].gameObject.activeInHierarchy)
                        {
                            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(ex_checkTrans[i], mousePos, canvas.worldCamera, out Vector2 localPoint))
                            {
                                is_point_out = !ex_checkTrans[i].rect.Contains(localPoint);
                                if (!is_point_out)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
            }
            return is_point_out;
        }

        private void OnTrigger()
        {
            if (hide_target != null)
            {
                hide_target.SetActive(false);
            }

            if (EventOnTrigger != null)
            {
                EventOnTrigger();
            }
        }

        public void AddCheck_rtTrans(RectTransform rect)
        {
            if (ex_checkTrans == null)
            {
                ex_checkTrans = new List<RectTransform>();
            }
            if(!ex_checkTrans.Contains(rect)) ex_checkTrans.Add(rect);
        }

        public void Remove_rtTrans(RectTransform rect)
        {
            if (ex_checkTrans != null)
            {
                if (ex_checkTrans.Contains(rect)) ex_checkTrans.Remove(rect);
            }
        }
    }
}
