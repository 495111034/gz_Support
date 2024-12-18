namespace UnityEngine.UI
{
    [AddComponentMenu("Layout/My Vertical Layout Group", 151)]
    public class MyVerticalLayoutGroup : MyHorizontalOrVerticalLayoutGroup
    {
        protected MyVerticalLayoutGroup()
        {}

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            CalcAlongAxis(0, true);
        }

        public override void CalculateLayoutInputVertical()
        {
            CalcAlongAxis(1, true);
        }

        public override void SetLayoutHorizontal()
        {
            SetChildrenAlongAxis(0, true);
        }

        public override void SetLayoutVertical()
        {
            SetChildrenAlongAxis(1, true);
            if (AutoSize)
            {
                var scrollRect = GameObjectUtils.FindInParents<ScrollRect>(gameObject);
                var toHeight = preferredHeight;
                if (scrollRect && scrollRect.content == rectTransform)
                {
                    var parentSize = scrollRect.gameObject.GetRectTransform().GetSize();
                    toHeight = parentSize.y > preferredHeight ? parentSize.y : preferredHeight;
                }
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, toHeight);
            }
        }
    }
}
