using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    /// <summary>
    /// 多边形图片，可自定义颜色填充空图
    /// </summary>
    public class PolygonImage : MySpriteImageBase
    {
        [HideInInspector]
        [Range(0, 1)]
        [SerializeField]
        private float alpha = 1f;

        [HideInInspector]
        [SerializeField]
        private Color _color = Color.white;

        [HideInInspector]
        [SerializeField]
        [Range(0, 1)]
        float[] verticess = new float[3] { 1f, 1f, 1f };

        [HideInInspector]
        [SerializeField]
        [Range(-180, 180)]
        public float rotation = -180;

        private float edges = 0;

        public float Rotation { get { return rotation; } set { rotation = value; OnInit(); } }
        public float[] Vertices { get { return verticess; } set { verticess = value; OnInit(); } }
        public float Alpha { get { return alpha; } set { alpha = value; OnInit(); } }
        public override Color color { get { return _color; } set { _color = value; OnInit(); } }
        

        override protected void OnInit()
        {
            if (!gameObject.activeInHierarchy) return;

            UnityEngine.Profiling.Profiler.BeginSample("PolygonImage.SetAllDirty");
            SetAllDirty();
            UnityEngine.Profiling.Profiler.EndSample();
        }


        protected override void OnEnable()
        {
            base.OnEnable();
            OnInit();
            canvasRenderer.SetAlpha(alpha);
        }


        protected override void OnDidApplyAnimationProperties()
        {
            canvasRenderer.SetAlpha(alpha);
        }

        public override Material material => UIGrapAssets.m_default_ui_mat;

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            Vector4 uv = new Vector4(0, 0, 1, 1);
            if (_tex != null)
                uv = new Vector4(0, 0, 1, 1);
            else if (_sprite != null)
                uv = Sprites.DataUtility.GetOuterUV(_sprite);
            else
            {
                //无图
                if (!_noTexShow) return;
            }

            if (verticess.Length < 3)
            {
                Log.LogError("至少需要3条边才能形成图形");
            }
            else
            {
                AddPolygon(vh, uv);
            }
        }

        protected override void AddQuad(VertexHelper vertexHelper, MySpritePacker packer, Vector2 posMin, Vector2 posMax, Vector2 uvMin, Vector2 uvMax)
        {
            throw new NotImplementedException();
        }

        static Vector2[] _pos4 = new Vector2[3];
        static Vector2[] _uv4 = new Vector2[3];
        //static UIVertex[] _vbo = new UIVertex[3];
        /// <summary>
        /// 多边形
        /// </summary>
        /// <param name="vh"></param>
        /// <param name="color"></param>
        /// <param name="outerUV"></param>
        private void AddPolygon(VertexHelper vh, Vector4 outerUV)
        {
            if (verticess.Length < 3) return;

            vh.Clear();

            var _pos4 = PolygonImage._pos4;
            var _uv4 = PolygonImage._uv4;

            Rect pixelAdjustedRect = GetPixelAdjustedRect();
            edges = Mathf.Min(pixelAdjustedRect.height, pixelAdjustedRect.width) / 2;

            Vector2 prevX = Vector2.zero;
            _uv4[0] = new Vector2((outerUV.x + outerUV.z)/2, outerUV.y);
            _uv4[1] = new Vector2(outerUV.x, outerUV.w);
            _uv4[2] = new Vector2(outerUV.z, outerUV.w);

            float degrees = 360f / verticess.Length;
            int vertices = verticess.Length + 1;
            int cnt = 0;
            for (int i = 0; i < vertices; i++)
            {
                var vect = (i == vertices - 1) ? verticess[0] : verticess[i];

                float outer = edges * vect;
                float rad = Mathf.Deg2Rad * (i * degrees + rotation);
                float c = Mathf.Cos(rad);
                float s = Mathf.Sin(rad);

                _pos4[0] = Vector2.zero;
                _pos4[2] = prevX;
                _pos4[1] = prevX = new Vector2(outer * c, outer * s) * _scale;
                if (i == 0) 
                {
                    continue;
                }                
                for (int n = 0; n < 3; n++)
                {
                    var vert = UIVertex.simpleVert;
                    vert.color = _color;
                    vert.position = _pos4[n];
                    vert.uv0 = _uv4[n];
                    vh.AddVert(vert);
                }
                vh.AddTriangle(cnt++, cnt++, cnt++);
            }
        }
    }
}

