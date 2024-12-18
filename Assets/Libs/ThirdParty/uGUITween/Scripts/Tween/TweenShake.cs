using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace uTools
{
    public class TweenShake : Tweener
    {
        RectTransform mRectTransform = null;
        bool is3D = true;
        private Transform mTarget;
        public Transform target
        {
            get
            {
                if (mTarget == null)
                {
                    mTarget = transform;
                    mRectTransform = mTarget as RectTransform;
                    is3D = (mRectTransform != null) ? false : true;
                    CacheTargetInfo();
                }
                return mTarget;
            }
        }

        private Vector3 localPosition = Vector3.zero;
        private Vector3 anchoredPosition = Vector3.zero;
        private Vector3 position = Vector3.zero;
        private Vector3 localScale = Vector3.zero;
        private Vector3 localEulerAngles = Vector3.zero;
        private Vector3 eulerAngles = Vector3.zero;

        [SerializeField]
        protected Vector3 limit;
        [SerializeField]
        protected bool isLocal = true;
        [SerializeField]
        protected ShakeType shakeType = ShakeType.ePosition;

        private Vector3 mValue;
        public Vector3 value
        {
            get
            {
                return mValue;
            }
            set
            {
                mValue = value;
                Shake();
            }
        }
        
        private void CacheTargetInfo()
        {
            localPosition = target.localPosition;
            position = target.position;
            localScale = target.localScale;
            localEulerAngles = target.localEulerAngles;
            eulerAngles = target.eulerAngles;
            if (mRectTransform != null)
            {
                anchoredPosition = mRectTransform.anchoredPosition;
            }
        }

        protected override void OnUpdate(float factor, bool isFinished)
        {
            factor = 1 - factor;
            float x = limit.x * factor;
            float y = limit.y * factor;
            float z = limit.z * factor;
            mValue.x = UnityEngine.Random.Range(x * -1, x);
            mValue.y = UnityEngine.Random.Range(y * -1, y);
            mValue.z = UnityEngine.Random.Range(z * -1, z);
            value = mValue;
        }

        private Vector3 tempVector3 = Vector3.one;
        void Shake()
        {
            if (shakeType == ShakeType.ePosition)
            {
                if (isLocal)
                {

                    if (is3D)
                    {
                        tempVector3 = value + localPosition;
                        target.localPosition = tempVector3;
                    }

                    else
                    {
                        tempVector3 = value + anchoredPosition;
                        mRectTransform.anchoredPosition3D = new Vector3(tempVector3.x, tempVector3.y, 0f);
                    }
                }
                else
                {
                    tempVector3 = value + position;
                    if (is3D)
                        target.position = tempVector3;
                    else
                        mRectTransform.anchoredPosition3D = tempVector3;
                }
            }
            else if (shakeType == ShakeType.eScale)
            {
                target.localScale = value + localScale;
            }
            else
            {
                if (isLocal)
                {
                    target.localEulerAngles = value + localEulerAngles;
                }
                else
                {
                    target.eulerAngles = value + eulerAngles;
                }
            }
        }

        public static TweenShake Begin(GameObject go, Vector3 from, float duration = 1f, float delay = 0f, Style style = Style.PingPong)
        {
            TweenShake comp = Tweener.Begin<TweenShake>(go, duration);
            comp.limit = from;
            comp.duration = duration;
            comp.delay = delay;
            comp.style = style;
            if (duration <= 0)
            {
                comp.Sample(1, true);
                comp.enabled = false;
            }
            return comp;
        }        
    }

}