using System;
using UnityEngine.EventSystems;
using FitMode = UnityEngine.UI.ContentSizeFitter.FitMode;
namespace UnityEngine.UI
{
    /// <summary>
    ///   <para>调整RectTransform的大小以适合其内容的大小.</para>
    /// </summary>
    [AddComponentMenu("Layout/My Content Size Fitter", 141), ExecuteInEditMode, RequireComponent(typeof(RectTransform))]
    public class MyContentSizeFitter : UIBehaviour, ILayoutSelfController, ILayoutController
    {
        [HideInInspector]
        [SerializeField]
        protected FitMode m_HorizontalFit = FitMode.Unconstrained;
        [HideInInspector]
        [SerializeField]
        protected FitMode m_VerticalFit = FitMode.Unconstrained;

        [HideInInspector]
        [SerializeField]
        float fixSizeHorizontal = 0f;

        [HideInInspector]
        [SerializeField]
        float fixSizeVertical = 0f;

        [HideInInspector]
        [NonSerialized]
        private RectTransform m_Rect;
        private DrivenRectTransformTracker m_Tracker;
        /// <summary>
        ///   <para>用于确定宽度的拟合模式.</para>
        /// </summary>
        public FitMode horizontalFit
        {
            get
            {
                return m_HorizontalFit;
            }
            set
            {
                if (MySetPropertyUtility.SetStruct<ContentSizeFitter.FitMode>(ref m_HorizontalFit, value))
                {
                    SetDirty();
                }
            }
        }
        /// <summary>
        ///   <para>用于确定高度的拟合模式.</para>
        /// </summary>
        public FitMode verticalFit
        {
            get
            {
                return m_VerticalFit;
            }
            set
            {
                if (MySetPropertyUtility.SetStruct<FitMode>(ref m_VerticalFit, value))
                {
                    SetDirty();
                }
            }
        }

        public float FixSizeHorizontal { get { return fixSizeHorizontal; } set { fixSizeHorizontal = value; SetDirty(); } }
        public float FixSizeVertical { get { return fixSizeVertical; }set { fixSizeVertical = value;SetDirty(); } }
        private RectTransform rectTransform
        {
            get
            {
                if (m_Rect == null)
                {
                    m_Rect = GetComponent<RectTransform>();
                }
                return this.m_Rect;
            }
        }
        protected MyContentSizeFitter()
        {
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            this.SetDirty();
        }

        protected override void OnDisable()
        {
            this.m_Tracker.Clear();
            LayoutRebuilderEx.MarkLayoutForRebuild(this.rectTransform);
            base.OnDisable();
        }
        protected override void OnRectTransformDimensionsChange()
        {
            this.SetDirty();
        }
        private void HandleSelfFittingAlongAxis(RectTransform.Axis axis)
        {
            FitMode fitMode = (axis != 0) ? this.verticalFit : this.horizontalFit;
            if (fitMode == FitMode.Unconstrained)
            {
                m_Tracker.Add(this, rectTransform, 0);
            }
            else
            {
                m_Tracker.Add(this, rectTransform, (axis != RectTransform.Axis.Horizontal) ? DrivenTransformProperties.SizeDeltaY : DrivenTransformProperties.SizeDeltaX);
                if (fitMode == FitMode.MinSize)
                {
                    rectTransform.SetSizeWithCurrentAnchors(axis, LayoutUtility.GetMinSize(m_Rect, (int)axis) + ((axis == RectTransform.Axis.Horizontal) ? fixSizeHorizontal : fixSizeVertical));
                }
                else
                {
                    rectTransform.SetSizeWithCurrentAnchors(axis, LayoutUtility.GetPreferredSize(m_Rect, (int)axis) + ((axis == RectTransform.Axis.Horizontal) ? fixSizeHorizontal:fixSizeVertical));
                }
            }
        }
        /// <summary>
        ///   <para>由Layout控件调用</para>
        /// </summary>
        public virtual void SetLayoutHorizontal()
        {
            m_Tracker.Clear();
            HandleSelfFittingAlongAxis(RectTransform.Axis.Horizontal);
        }
        /// <summary>
        ///   <para>由Layout控件调用</para>
        /// </summary>
        public virtual void SetLayoutVertical()
        {
            HandleSelfFittingAlongAxis(RectTransform.Axis.Vertical);
        }
        /// <summary>
        ///   <para>标记为脏，将重新部局.</para>
        /// </summary>
        protected void SetDirty()
        {
            if (this.IsActive())
            {
                LayoutRebuilderEx.MarkLayoutForRebuild(this.rectTransform);
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            SetDirty();
        }
#endif
    }
}
