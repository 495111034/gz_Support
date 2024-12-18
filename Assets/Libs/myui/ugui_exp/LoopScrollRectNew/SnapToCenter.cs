using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System;

namespace UnityEngine.UI
{
    public class SnapToCenter : ScrollRect
    {
        public GameObject CenterObj { get { return centerObj; } }
        private GameObject centerObj;
        private Action scrollEndEvent;

        private Vector2 targetPosition;

        private float snapSpeed; // 对齐的平滑速度
        private float curLerpValue;

        private bool isSnapping = false;
        private bool mDragging = false;
        private bool isDebug = false;

        public void AddScrollEndEvent(Action callback)
        {
            scrollEndEvent = callback;
        }

        protected override void LateUpdate()
        {
            if (isSnapping)
            {
                curLerpValue = Mathf.SmoothDamp(curLerpValue, 1, ref snapSpeed, decelerationRate);
                if (isDebug)
                {
                    Debug.Log($"++++++isSnapping  content.anchoredPosition:{content.anchoredPosition} , targetPosition{targetPosition}, Time.deltaTime:{Time.deltaTime}");
                }
                content.anchoredPosition = Vector2.Lerp(content.anchoredPosition, targetPosition, curLerpValue);
                if (Vector2.Distance(content.anchoredPosition, targetPosition) < 1f)
                {
                    if (isDebug)
                    {
                        Debug.Log($"++++++content.anchoredPosition:{content.anchoredPosition}");
                    }
                    if(scrollEndEvent != null)
                    {
                        scrollEndEvent?.Invoke();
                    }
                    SetContentAnchoredPosition(targetPosition);
                    isSnapping = false;
                }
            }
            else
            {
                if (mDragging)
                {
                    base.LateUpdate();
                }
            }
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            base.OnBeginDrag(eventData);
            mDragging = true;
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            mDragging = false;
            SnapToCenterNearestItem();
        }

        public void SnapToCenterNearestItem()
        {
            float minDistance = float.MaxValue;
            float realDis = 0;
            curLerpValue = 0;

            int childCount = content.childCount;
            if (childCount > 0)
            {
                for (int i = 0; i < childCount; i++)
                {
                    var child = content.GetChild(i).GetComponent<RectTransform>();

                    if (!child.gameObject.activeInHierarchy)
                    {
                        continue;
                    }

                    // 获取子项的中心位置
                    var canvas = GetComponentInParent<Canvas>();
                    var screenPos = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, child.position);
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, screenPos, canvas.worldCamera, out Vector2 disV2);
                    float distance = Mathf.Abs(disV2.x);
                    if (isDebug)
                    {
                        Debug.Log($"++child:{child}++++distance:" + distance);
                    }
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        realDis = disV2.x;
                        centerObj = child.gameObject;
                    }
                }
                if (isDebug)
                {
                    Debug.Log($"++++++realDis:{realDis}  content.anchoredPosition.x :{content.anchoredPosition.x }");
                }
                // 计算目标位置
                targetPosition = new Vector2(content.anchoredPosition.x - realDis, content.anchoredPosition.y);
                if (isDebug)
                {
                    Debug.Log($"++++++targetPosition:{targetPosition}");
                }
                isSnapping = true;
            }
        }

        public void SnapObjToCenter(GameObject gameObject)
        {
            centerObj = gameObject;

            float realDis = 0;
            curLerpValue = 0;

            // 获取子项的中心位置
            var canvas = GetComponentInParent<Canvas>();
            var screenPos = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, centerObj.transform.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, screenPos, canvas.worldCamera, out Vector2 disV2);
            realDis = disV2.x;
            if (isDebug)
            {
                Debug.Log($"++++++realDis:{realDis}  content.anchoredPosition.x :{content.anchoredPosition.x }");
            }
            // 计算目标位置
            targetPosition = new Vector2(content.anchoredPosition.x - realDis, content.anchoredPosition.y);
            if (isDebug)
            {
                Debug.Log($"++++++targetPosition:{targetPosition}");
            }
            isSnapping = true;
        }
    }
}