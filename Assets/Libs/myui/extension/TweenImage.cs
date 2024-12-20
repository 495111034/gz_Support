using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace uTools {
	
	public class TweenImage : Tweener {

		[Range(0,1)]
		public float from;
		[Range(0, 1)]
		public float to;

		float mValue;
		public float value {
			get { return mValue;}
			set { 
				mValue = value;
			}
		}

		private MyImage mImage;
		public MyImage cacheImage {
			get {
				if (mImage == null) {
					mImage = GetComponent<MyImage>();
					if (mImage.type != Image.Type.Filled) {
						if(Application.isEditor) Debug.LogWarning("[uTweenImage] To use tween the image type must be [Image.Type.Filled]");
					}
				}
				return mImage;
			}
		}

		protected override void OnUpdate (float factor, bool isFinished) {
			value = from + factor * (to - from);

            if(cacheImage)
			    cacheImage.fillAmount = value;
		}

		public static TweenImage Begin(MyImage go, float from, float to, float duration, float delay) {
			TweenImage comp = Begin<TweenImage>(go.gameObject, duration);
            comp.value = from;
			comp.from = from;
			comp.to = to;
			comp.delay = delay;
			
			if (duration <=0) {
				comp.Sample(1, true);
				comp.enabled = false;
			}
			return comp;
		}
	}
}
