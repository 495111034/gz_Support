using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/Effects/Gradient 文本4点渐变色")]
    public class UITextCornersGradient : BaseMeshEffect
    {
        public Color m_topLeftColor = Color.white;
        public Color m_topRightColor = Color.white;
        public Color m_bottomRightColor = Color.white;
        public Color m_bottomLeftColor = Color.white;

        //protected override void OnEnable()
        //{
        //    if (GetComponent<Text>())
        //    {
        //        this.enabled = false;
        //    }
        //}

        public override void ModifyMesh(VertexHelper vh)
        {
           // if (enabled)
            {
                Rect rect = graphic.rectTransform.rect;

                UIVertex vertex = default(UIVertex);
                for (int i = 0; i < vh.currentVertCount; i++)
                {
                    vh.PopulateUIVertex(ref vertex, i);
                    Vector2 normalizedPosition = UIGradientUtils.VerticePositions[i % 4];
                    vertex.color *= UIGradientUtils.Bilerp(m_bottomLeftColor, m_bottomRightColor, m_topLeftColor, m_topRightColor, normalizedPosition);
                    vh.SetUIVertex(vertex, i);
                }
            }
        }
    }
}
