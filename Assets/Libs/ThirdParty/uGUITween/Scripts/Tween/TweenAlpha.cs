using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace uTools {
	
	public class TweenAlpha : Tween<float> {

        public bool includeChildren = false;
        private bool isCanvasGroup { get; set; } = false;
        float mAlpha = 0f;

        Transform mTransform;
        Transform CachedTranform
        {
            get
            {
                if (mTransform == null)
                {
                    mTransform = GetComponent<Transform>();
                }
                return mTransform;
            }
        }

        Graphic[] mGraphics;
        Graphic[] CachedGraphics
        {
            get
            {
                if (mGraphics == null)
                {
                    mGraphics = includeChildren ? gameObject.GetComponentsInChildren<Graphic>() : gameObject.GetComponents<Graphic>();
                }
                return mGraphics;
            }
        }

        CanvasGroup mCanvasGroup;
        CanvasGroup CacheCanvasGroup
        {
            get
            {
                if (mCanvasGroup == null)
                {
                    mCanvasGroup = gameObject.GetComponent<CanvasGroup>();
                }
                return mCanvasGroup;
            }
        }

        public void CheckCanvasGroup()
        {
            if (CacheCanvasGroup != null)
            {
                isCanvasGroup = true;
            }
        }

        protected override void Start()
        {
            base.Start();
            CheckCanvasGroup();
        }

        public override float value
        {
            get
            {
                return mAlpha;
            }
            set
            {
                mAlpha = value;
                SetAlpha(CachedTranform, value);
            }
        }

        protected override void OnUpdate(float factor, bool isFinished)
        {
            value = from + factor * (to - from);
        }

        void SetAlpha(Transform _transform, float _alpha)
        {
            if (isCanvasGroup)
            {
                if (CacheCanvasGroup.alpha != _alpha)
                {
                    CacheCanvasGroup.alpha = _alpha;
                }
            }
            else
            {
                
                for (int i = 0; i < CachedGraphics.Length; ++i)
                {
                    //var item = CachedGraphics[i].material;
                    //if (item)
                    //{
                    //    Color color = item.GetColor(resource.ShaderNameHash.TintColor);
                    //    if (color.a != _alpha)
                    //    {
                    //        color.a = _alpha;
                    //        item.SetColor(resource.ShaderNameHash.TintColor, color);                   
                    //    }
                    //}
                    var item = CachedGraphics[i];
                    if (item)
                    {
                        Color color = item.color;
                        if (color.a != _alpha)
                        {
                            color.a = _alpha;                           
                            item.color = color;
                        }
                    }
                }
            }
        }

        public static TweenAlpha Begin(GameObject go, float from, float to, float duration = 1f, float delay = 0f)
        {
            TweenAlpha comp = Begin<TweenAlpha>(go, duration);
            comp.CheckCanvasGroup();

            comp.value = from;
            comp.from = from;
            comp.to = to;
            comp.duration = duration;
            comp.delay = delay;
            if (duration <= 0)
            {
                comp.Sample(1, true);
                comp.enabled = false;
            }
            return comp;
        }


    }

}