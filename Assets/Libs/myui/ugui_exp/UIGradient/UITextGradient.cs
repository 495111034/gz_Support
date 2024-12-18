using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/Effects/Gradient 文本渐变色")]
    public class UITextGradient : BaseMeshEffect
    {
        [HideInInspector]
        [SerializeField]
        Color _color1 = Color.white;

        [HideInInspector]
        [SerializeField]
        Color _color2 = Color.white;

        [Range(0f, 360f)]
        [HideInInspector]
        [SerializeField]
        float _angle = 0f;

        public Color m_color1 { get { return _color1; } set { _color1 = value; } }
        public Color m_color2 { get { return _color2; } set { _color2 = value; } }
        public float m_angle { get { return _angle; } set { _angle = value; } }

        //protected override void OnEnable()
        //{
        //    if (GetComponent<Text>())
        //    {
        //        this.enabled = false;
        //    }
        //}

        public override void ModifyMesh(VertexHelper vh)
        {
            //if (enabled)
            {
                Rect rect = graphic.rectTransform.rect;
                Vector2 dir = UIGradientUtils.RotationDir(_angle);
                UIGradientUtils.Matrix2x3 localPositionMatrix = UIGradientUtils.LocalPositionMatrix(new Rect(0f, 0f, 1f, 1f), dir);

                UIVertex vertex = default(UIVertex);
                for (int i = 0; i < vh.currentVertCount; i++)
                {

                    vh.PopulateUIVertex(ref vertex, i);
                    Vector2 position = UIGradientUtils.VerticePositions[i % 4];
                    Vector2 localPosition = localPositionMatrix * position;
                    vertex.color *= Color.Lerp(_color2, _color1, localPosition.y);
                    vh.SetUIVertex(vertex, i);
                }
            }
        }
    }
}
