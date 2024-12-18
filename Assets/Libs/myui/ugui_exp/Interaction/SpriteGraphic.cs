using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    public class SpriteGraphic : MaskableGraphic
    {
        public Sprite[] sprites;

        public Sprite m_sprite;
        
        public override Texture mainTexture
        {
            get
            {
                if (m_sprite == null)
                    return s_WhiteTexture;

                return m_sprite.texture;
            }
        }

        protected override void OnRectTransformDimensionsChange()
        {
            // base.OnRectTransformDimensionsChange();  
        }

        /// <summary>  
        /// 绘制后 需要更新材质  
        /// </summary>  
        public new void UpdateMaterial()
        {
            base.UpdateMaterial();
        }
    }
}