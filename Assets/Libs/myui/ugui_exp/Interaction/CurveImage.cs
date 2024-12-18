using System;
using System.Collections.Generic;
using UnityEngine.Serialization;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Sprites;

namespace UnityEngine.UI
{
    /// <summary>
    /// 曲线图片
    /// </summary>
    [AddComponentMenu("UI/Curve Image")]
    public class CurveImage : MaskableGraphic
    {
        [SerializeField] private Sprite m_Sprite;
        public Sprite sprite { get { return m_Sprite; } set { if (MySetPropertyUtility.SetClass(ref m_Sprite, value)) SetMaterialDirty(); } }

        [SerializeField] AnimationCurve m_Curve;

        [SerializeField] float m_Distance = 20f;
        public float distance { get { return m_Distance; } set { if (MySetPropertyUtility.SetStruct(ref m_Distance, value)) SetVerticesDirty(); } }

        [SerializeField] float m_Width = 10f;
        public float width { get { return m_Width; } set { if (MySetPropertyUtility.SetStruct(ref m_Width, value)) SetVerticesDirty(); } }

        [SerializeField] [Range(1, 100)] int m_Step = 20;
        public int step { get { return m_Step; } set { if (MySetPropertyUtility.SetStruct(ref m_Step, value)) SetVerticesDirty(); } }

        // left/right 不会使 cache 无效
        [SerializeField] [Range(0, 1)] float m_Left = 0.3f;
        public float left { get { return m_Left; } set { if (MySetPropertyUtility.SetStruct(ref m_Left, value)) SetVerticesDirty(); } }

        [SerializeField] [Range(0, 1)] float m_Right = 0.7f;
        public float right { get { return m_Right; } set { if (MySetPropertyUtility.SetStruct(ref m_Right, value)) SetVerticesDirty(); } }


        public override Texture mainTexture
        {
            get
            {
                return m_Sprite == null ? s_WhiteTexture : m_Sprite.texture;
            }
        }

        public float GetBestSize(int axis)
        {
            if (m_Sprite == null) return 0;
            return Sprites.DataUtility.GetMinSize(m_Sprite)[axis];
        }


#if UNITY_EDITOR

        protected override void OnValidate()
        {
            _cache_valid = false;
            base.OnValidate();
        }

#endif

        #region cache 管理

        //
        bool _cache_valid;
        Rect _cache_rc;
        float _cache_dist;
        float _cache_width;
        int _cache_step;
        Vector4[] _cache_data;          // 每个位置的坐标点信息


        // 判断 cache 是否有效, 如果无效, 则更新到最新值
        bool IsCacheValid(Rect rc)
        {
            // 有效
            if (_cache_valid &&
                _cache_rc.Equals(rc) &&
                _cache_dist == m_Distance &&
                _cache_width == m_Width &&
                _cache_step == m_Step)
            {
                return true;
            }

            // 无效, 更新为最新值
            _cache_valid = true;        // 设置有效

            _cache_rc = rc;
            _cache_dist = m_Distance;
            _cache_width = m_Width;
            _cache_step = m_Step;

            return false;
        }

        // 构造 cache
        void MakeCacheValid()
        {
            var rc = rectTransform.rect;
            if (IsCacheValid(rc)) return;

            //
            var x_step = m_Step;
            var x_add = rc.width / x_step;

            var rate = 0f;
            var rate_add = 1f / x_step;

            var half_width = m_Width / 2;

            // 获取每个点的坐标
            var list = new List<Vector2>();
            {
                list.Add(Vector2.zero);     // 第一个占位

                var x = rc.xMin;            // 开始 X 
                var cy = rc.center.y;       // 中间 y

                for (int i = 0; i <= x_step; i++, x += x_add, rate += rate_add)
                {
                    var off_y = m_Curve.Evaluate(rate) * m_Distance;       // y 偏移量
                    var y = cy + off_y;             // y 坐标值

                    list.Add(new Vector2(x, y));    // 该点坐标
                }

                // 添加第一个
                var offset = list[2] - list[1];
                list[0] = list[1] - offset;

                // 添加最后1个
                var count = list.Count;
                offset = list[count - 1] - list[count - 2];
                list.Add(list[count - 1] + offset);
            }

            // 计算每个位置的坐标
            _cache_data = new Vector4[x_step + 1];
            for (int i = 0; i <= x_step; i++)
            {
                _cache_data[i] = GetSideAtIndex(list, i, half_width);
            }
        }

        // 获取 下面/上面 2个顶点的位置
        static Vector4 GetSideAtIndex(List<Vector2> list, int index, float half_width)
        {
            var prev = list[index];
            var cur = list[index + 1];
            var next = list[index + 2];

            var diff = (next - prev);
            var angle = Mathf.Atan2(diff.y, diff.x) + Mathf.PI * 0.5f;

            var y = Mathf.Sin(angle) * half_width;
            var x = Mathf.Cos(angle) * half_width;
            var add = new Vector2(x, y);

            var max = cur + add;
            var min = cur - add;

            var ret = new Vector4(min.x, min.y, max.x, max.y);
            //Log.LogInfo("GetSideAtIndex, index:" + index + ", pos:" + cur + ", add:" + add + ", min:" + min + ", max:" + max);
            return ret;
        }

        #endregion

        //
        void FillVBO(VertexBuff vbo)
        {
            MakeCacheValid();
            
            // 获取 sprite 信息
            var outer_uv = Sprites.DataUtility.GetOuterUV(m_Sprite);        // min.x, min.y, max.x, max.y
            var inner_uv = DataUtility.GetInnerUV(m_Sprite);
            var left_uv = new Vector4(outer_uv.x, outer_uv.y, inner_uv.x, inner_uv.w);
            var right_uv = new Vector4(inner_uv.z, inner_uv.y, outer_uv.z, outer_uv.w);

            //
            var x_step = m_Step;

            var rate = 0f;
            var rate_add = 1f / x_step;

            var rate_left = m_Left;
            var rate_right = m_Right;

            var vert = UIVertex.simpleVert;
            vert.color = base.color;

            // 计算每个线段
            var uv = (rate > rate_right ? right_uv : rate > rate_left ? inner_uv : left_uv);
            var side = _cache_data[0];

            for (int i = 0; i < x_step; i++)
            {
                vert.position = new Vector3(side.x, side.y);
                vert.uv0 = new Vector2(uv.x, uv.y);
                vbo.Add(ref vert);

                vert.position = new Vector3(side.z, side.w);
                vert.uv0 = new Vector2(uv.z, uv.w);
                vbo.Add(ref vert);

                rate += rate_add;
                uv = (rate > rate_right ? right_uv : rate > rate_left ? inner_uv : left_uv);
                side = _cache_data[i + 1];

                vert.position = new Vector3(side.z, side.w);
                vert.uv0 = new Vector2(uv.z, uv.w);
                vbo.Add(ref vert);

                vert.position = new Vector3(side.x, side.y);
                vert.uv0 = new Vector2(uv.x, uv.y);
                vbo.Add(ref vert);
            }
        }


        protected void OnFillVBO(VertexBuff vbo)
        {
            if (m_Sprite == null) return;
            //Log.LogInfo("OnFillVBO");

            try
            {
                FillVBO(vbo);
            }
            catch (Exception e)
            {
                Log.LogInfo("error:" + e);
                vbo.Clear();
            }
        }

    }
}
