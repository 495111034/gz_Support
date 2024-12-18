using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/Effects/Gradient 渐变色")]
    public class UIGradient : BaseMeshEffect
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

        [HideInInspector]
        [SerializeField]
        bool _ignoreRatio = false;

        [HideInInspector]
        [SerializeField]
        bool _autoRotation = false;

        [Range(0f, 10f)]
        [HideInInspector]
        [SerializeField]
        float _cycle_time = 1f;

        public Color m_color1 { get { return _color1; }set { _color1 = value;  } }
        public Color m_color2 { get { return _color2; }set { _color2 = value;  } }

        
        public float m_angle { get { return _angle; }set { _angle = value; } }
        public bool m_ignoreRatio { get { return _ignoreRatio; }set { _ignoreRatio = value; } }
        
        public bool m_autoRotation { get { return _autoRotation; }set { _autoRotation = value; } }

        public float m_cycle_time { get { return _cycle_time; }set { _cycle_time = value; } }

        //protected override void OnEnable()
        //{
        //    if(GetComponent<Text>())
        //    {
        //        this.enabled = false;
        //    }
        //}



        public override void ModifyMesh(VertexHelper vh)
        {
           // if (enabled)
            {
                Rect rect = graphic.rectTransform.rect;
                Vector2 dir = UIGradientUtils.RotationDir(_angle);

                if (!_ignoreRatio)
                    dir = UIGradientUtils.CompensateAspectRatio(rect, dir);

                UIGradientUtils.Matrix2x3 localPositionMatrix = UIGradientUtils.LocalPositionMatrix(rect, dir);

                UIVertex vertex = default;
                for (int i = 0; i < vh.currentVertCount; i++)
                {
                    vh.PopulateUIVertex(ref vertex, i);
                    Vector2 localPosition = localPositionMatrix * vertex.position;
                    vertex.color *= Color.Lerp(_color2, _color1, localPosition.y);
                    vh.SetUIVertex(vertex, i);
                }
            }
        }

        //void LateUpdate()
        //{
        //    if (enabled)
        //    {
        //        if (!_autoRotation || _cycle_time <= 0f)
        //            return;

        //        _angle = (_angle + Time.deltaTime * (360 / _cycle_time)) % 360;
        //        graphic.SetVerticesDirty();
        //    }
        //}
    }
}
