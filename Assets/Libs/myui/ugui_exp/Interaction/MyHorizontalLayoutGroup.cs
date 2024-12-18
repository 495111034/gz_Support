namespace UnityEngine.UI
{
    [AddComponentMenu("Layout/My Horizontal Layout Group", 140)]
    public class MyHorizontalLayoutGroup : MyHorizontalOrVerticalLayoutGroup
    {
        protected MyHorizontalLayoutGroup()
        {}

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            CalcAlongAxis(0, false);
        }

        public override void CalculateLayoutInputVertical()
        {
            CalcAlongAxis(1, false);
        }

        public override void SetLayoutHorizontal()
        {
            SetChildrenAlongAxis(0, false);
            if (AutoSize)
            {
                var scrollRect = GameObjectUtils.FindInParents<ScrollRect>(gameObject);
                var toWidth = preferredWidth;
                if (scrollRect && scrollRect.content == rectTransform)
                {
                    var parentSize = scrollRect.gameObject.GetRectTransform().GetSize();
                    toWidth = parentSize.x > preferredWidth ? parentSize.x : preferredWidth;
                }
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, toWidth);
            }
        }

        public override void SetLayoutVertical()
        {
            SetChildrenAlongAxis(1, false);
        }
    }
}
