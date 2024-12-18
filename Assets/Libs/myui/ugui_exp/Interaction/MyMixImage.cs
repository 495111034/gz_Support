using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{

    public class MyMixImage : MonoBehaviour
    {
        public MyImage image;
        public List<Sprite> _spriteLiss;
        public List<float> weights;

        int idx = 0;

        //void Start()
        //{
        //    UpdateHp();
        //}

        public void SetWeights(List<float> weights)
        {
            this.weights = weights;
        }

        private void OnDisable()
        {
            idx = 0;

            if (image)
            {
                if (Application.isPlaying)
                {
                    Object.Destroy(image.sprite);
                }
                else 
                {
                    Object.DestroyImmediate(image.sprite, true);
                }
            }
        }

        private void OnEnable()
        {
            UpdateHp();
        }

        public void UpdateHp()
        {
            if (!gameObject.activeInHierarchy) return;

            idx = 0;

            var rect = gameObject.GetRectTransform();

            int W = (int)rect.sizeDelta.x;
            int H = (int)rect.sizeDelta.y;

            var _tex = new Texture2D(W, H);
            var before_weight = 0f;

            for (int w = 0; w < W; w++)
            {
                for (int h = 0; h < H; h++)
                {
                    _tex.SetPixel(w, h, _spriteLiss[idx].texture.GetPixel(w, h));
                }

                if (w > weights[idx] * W + before_weight)
                {
                    idx++;
                    before_weight = weights[idx - 1] * W;
                    idx = Mathf.Min(idx, 2);
                }
            }

            _tex.Apply();

            image.SetSprite(Sprite.Create(_tex, new Rect(0, 0, W, H), Vector2.one * 0.5f), null);
        }
    }
}
