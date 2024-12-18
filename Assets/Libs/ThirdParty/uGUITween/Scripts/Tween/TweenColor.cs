using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace uTools {
	
	public class TweenColor : Tween<Color> {

        public bool includeChildren = false;

        Graphic[] mGraphics;
        Color mColor = Color.white;
        public override Color value
        {
            get
            {
                return mColor;
            }
            set
            {
                SetColor(value);
                mColor = value;
            }
        }

        protected override void Start()
        {
            mGraphics = includeChildren ? gameObject.GetComponentsInChildren<Graphic>() : gameObject.GetComponents<Graphic>();
            base.Start();
        }

        protected override void OnUpdate(float factor, bool isFinished)
        {
            UnityEngine.Profiling.Profiler.BeginSample("set color");
            mColor = Color.Lerp(from, to, factor);
            SetColor(mColor);
            UnityEngine.Profiling.Profiler.EndSample();
        }

        void SetColor( Color _color)
        {            
            if (mGraphics != null && mGraphics.Length > 0)
            {
                for (int i = 0; i < mGraphics.Length; ++i)
                {
                    if (mGraphics[i] is Text) continue;
                    //var item = mGraphics[i].material;
                    //if (item)
                    //{
                    //    Color color = item.GetColor(resource.ShaderNameHash.TintColor);
                    //    if(color != _color)
                    //    {
                    //        item.SetColor(resource.ShaderNameHash.TintColor, _color);
                    //    }
                    //}
                    if (mGraphics[i] && mGraphics[i].color != _color)
                        mGraphics[i].color = _color;
                }
            }
        }

        public static TweenColor Begin(GameObject go, Color from, Color to, float duration, float delay)
        {
            TweenColor comp = Tweener.Begin<TweenColor>(go, duration);
            comp.value = from;
            comp.from = from;
            comp.to = to;
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
