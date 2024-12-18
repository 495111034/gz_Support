using UnityEngine;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    [AddComponentMenu("Layout/My Grid Layout Group", 152)]
    public class MyGridLayoutGroup : LayoutGroup
    {
        public enum Corner { UpperLeft = 0, UpperRight = 1, LowerLeft = 2, LowerRight = 3 }
        public enum Axis { Horizontal = 0, Vertical = 1 }
        public enum Constraint { Flexible = 0, FixedColumnCount = 1, FixedRowCount = 2 }

        [SerializeField] protected Corner m_StartCorner = Corner.UpperLeft;
        public Corner startCorner { get { return m_StartCorner; } set { SetProperty(ref m_StartCorner, value); } }

        [SerializeField] protected Axis m_StartAxis = Axis.Horizontal;
        public Axis startAxis { get { return m_StartAxis; } set { SetProperty(ref m_StartAxis, value); } }

        [SerializeField] protected Vector2 m_CellSize = new Vector2(100, 100);
        public Vector2 cellSize { get { return m_CellSize; } set { SetProperty(ref m_CellSize, value); } }

        [SerializeField] protected Vector2 m_Spacing = Vector2.zero;
        public Vector2 spacing { get { return m_Spacing; } set { SetProperty(ref m_Spacing, value); } }

        [SerializeField] protected Constraint m_Constraint = Constraint.Flexible;
        public Constraint constraint { get { return m_Constraint; } set { SetProperty(ref m_Constraint, value); } }

        [SerializeField] protected int m_ConstraintCount = 2;
        public int constraintCount { get { return m_ConstraintCount; } set { SetProperty(ref m_ConstraintCount, Mathf.Max(1, value)); } }

        protected MyGridLayoutGroup()
        {}

        #if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            constraintCount = constraintCount;
        }

        #endif

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();

            int minColumns = 0;
            int preferredColumns = 0;
            if (m_Constraint == Constraint.FixedColumnCount)
            {
                minColumns = preferredColumns = m_ConstraintCount;
            }
            else if (m_Constraint == Constraint.FixedRowCount)
            {
                minColumns = preferredColumns = Mathf.CeilToInt(rectChildren.Count / (float)m_ConstraintCount - 0.001f);
            }
            else
            {
                minColumns = 1;
                preferredColumns = Mathf.CeilToInt(Mathf.Sqrt(rectChildren.Count));
            }

            SetLayoutInputForAxis(
                padding.horizontal + (cellSize.x + spacing.x) * minColumns - spacing.x,
                padding.horizontal + (cellSize.x + spacing.x) * preferredColumns - spacing.x,
                -1, 0);
        }

        public override void CalculateLayoutInputVertical()
        {
            int minRows = 0;
            if (m_Constraint == Constraint.FixedColumnCount)
            {
                minRows = Mathf.CeilToInt(rectChildren.Count / (float)m_ConstraintCount - 0.001f);
            }
            else if (m_Constraint == Constraint.FixedRowCount)
            {
                minRows = m_ConstraintCount;
            }
            else
            {
                float width = rectTransform.rect.size.x;
                int cellCountX = Mathf.Max(1, Mathf.FloorToInt((width - padding.horizontal + spacing.x + 0.001f) / (cellSize.x + spacing.x)));
                minRows = Mathf.CeilToInt(rectChildren.Count / (float)cellCountX);
            }

            float minSpace = padding.vertical + (cellSize.y + spacing.y) * minRows - spacing.y;
            SetLayoutInputForAxis(minSpace, minSpace, -1, 1);
        }

        public override void SetLayoutHorizontal()
        {
            SetCellsAlongAxis(0);
        }

        public override void SetLayoutVertical()
        {
            SetCellsAlongAxis(1);
        }

        public void SetCellsAlongAxis(int axis)
        {
            // Normally a Layout Controller should only set horizontal values when invoked for the horizontal axis
            // and only vertical values when invoked for the vertical axis.
            // However, in this case we set both the horizontal and vertical position when invoked for the vertical axis.
            // Since we only set the horizontal position and not the size, it shouldn't affect children's layout,
            // and thus shouldn't break the rule that all horizontal layout must be calculated before all vertical layout.

            if (axis == 0)
            {
                // Only set the sizes when invoked for horizontal axis, not the positions.
                for (int i = 0; i < rectChildren.Count; i++)
                {
                    RectTransform rect = rectChildren[i];

                    m_Tracker.Add(this, rect,
                        DrivenTransformProperties.Anchors |
                        DrivenTransformProperties.AnchoredPosition |
                        DrivenTransformProperties.SizeDelta);

                    rect.anchorMin = Vector2.up;
                    rect.anchorMax = Vector2.up;
                    rect.sizeDelta = cellSize;
                }
                return;
            }

            float width = rectTransform.rect.size.x;
            float height = rectTransform.rect.size.y;

            int cellCountX = 1;
            int cellCountY = 1;
            if (m_Constraint == Constraint.FixedColumnCount)
            {
                cellCountX = m_ConstraintCount;
                cellCountY = Mathf.CeilToInt(rectChildren.Count / (float)cellCountX - 0.001f);
            }
            else if (m_Constraint == Constraint.FixedRowCount)
            {
                cellCountY = m_ConstraintCount;
                cellCountX = Mathf.CeilToInt(rectChildren.Count / (float)cellCountY - 0.001f);
            }
            else
            {
                if (cellSize.x + spacing.x <= 0)
                    cellCountX = int.MaxValue;
                else
                    cellCountX = Mathf.Max(1, Mathf.FloorToInt((width - padding.horizontal + spacing.x + 0.001f) / (cellSize.x + spacing.x)));

                if (cellSize.y + spacing.y <= 0)
                    cellCountY = int.MaxValue;
                else
                    cellCountY = Mathf.Max(1, Mathf.FloorToInt((height - padding.vertical + spacing.y + 0.001f) / (cellSize.y + spacing.y)));
            }

            int cornerX = (int)startCorner % 2;
            int cornerY = (int)startCorner / 2;

            int cellsPerMainAxis, actualCellCountX, actualCellCountY;
            if (startAxis == Axis.Horizontal)
            {
                cellsPerMainAxis = cellCountX;
                actualCellCountX = Mathf.Clamp(cellCountX, 1, rectChildren.Count);
                actualCellCountY = Mathf.Clamp(cellCountY, 1, Mathf.CeilToInt(rectChildren.Count / (float)cellsPerMainAxis));
            }
            else
            {
                cellsPerMainAxis = cellCountY;
                actualCellCountY = Mathf.Clamp(cellCountY, 1, rectChildren.Count);
                actualCellCountX = Mathf.Clamp(cellCountX, 1, Mathf.CeilToInt(rectChildren.Count / (float)cellsPerMainAxis));
            }

            Vector2 requiredSpace = new Vector2(
                    actualCellCountX * cellSize.x + (actualCellCountX - 1) * spacing.x,
                    actualCellCountY * cellSize.y + (actualCellCountY - 1) * spacing.y
                    );
            Vector2 startOffset = new Vector2(
                    GetStartOffset(0, requiredSpace.x),
                    GetStartOffset(1, requiredSpace.y)
                    );

            for (int i = 0; i < rectChildren.Count; i++)
            {
                int positionX;
                int positionY;
                if (startAxis == Axis.Horizontal)
                {
                    positionX = i % cellsPerMainAxis;
                    positionY = i / cellsPerMainAxis;
                }
                else
                {
                    positionX = i / cellsPerMainAxis;
                    positionY = i % cellsPerMainAxis;
                }

                if (cornerX == 1)
                    positionX = actualCellCountX - 1 - positionX;
                if (cornerY == 1)
                    positionY = actualCellCountY - 1 - positionY;

                SetChildAlongAxis(rectChildren[i], 0, startOffset.x + (cellSize[0] + spacing[0]) * positionX, cellSize[0]);
                SetChildAlongAxis(rectChildren[i], 1, startOffset.y + (cellSize[1] + spacing[1]) * positionY, cellSize[1]);
            }
        }

        public List<RectTransform> Children { get { return rectChildren; } }

        protected RectTransform _seed = null;

        public RectTransform AddChildItem(string name = null, object param = null)
        {
            if (rectChildren.Count == 0) CalculateLayoutInputHorizontal();
            RectTransform newItem = null;
            if (rectChildren.Count == 0)
            {
                if (_seed)
                {
                    _seed.gameObject.SetActive(true);
                    newItem = _seed;
                    _seed = null;
                }
                else
                {
                    var go = gameObject.FindChild("item",false,false);

                    if(!go)
                    {
                        Log.LogError("not seek,cannt add new item");
                        newItem = GameObjectUtils.AddChild<RectTransform>(gameObject);
                    }
                    else
                    {
                        var newObj = GameObject.Instantiate(go, gameObject.GetRectTransform());
                        newItem = newObj.GetRectTransform();
                    }
                }

            }
            else
            {
                var go = GameObject.Instantiate(rectChildren[rectChildren.Count - 1].gameObject, gameObject.GetRectTransform());

                if (go == null)
                {
                    go = gameObject.FindChild("item",false,false);
                }

                var itws = go.GetComponentsInChildren<iTween>();
                for (int j = 0; j < itws.Length; ++j)
                {
                    GameObject.Destroy(itws[j]);
                }
                newItem = go.GetRectTransform();
            }

            if (!string.IsNullOrEmpty(name))
            {
                newItem.gameObject.name = name;
            }
            if (param != null)
            {
                newItem.gameObject.SetParam(param);
            }

            SetDirty();

            return newItem;
        }

        public void RemoveChildItem(GameObject waitRemove)
        {
            if (rectChildren.Count == 0) CalculateLayoutInputHorizontal();
            if (!waitRemove) return;
            for (int i = 0; i < rectChildren.Count; ++i)
            {
                if (rectChildren[i].gameObject == waitRemove)
                {
                    if (rectChildren.Count == 1)
                    {
                        _seed = waitRemove.GetRectTransform();
                        waitRemove.SetActive(false);
                    }
                    else
                    {
                        rectChildren.Remove(waitRemove.GetRectTransform());
                        GameObject.Destroy(waitRemove);
                    }
                    SetDirty();
                    return;
                }
            }

            Log.LogError("cannt found wait to remove object");
        }

        public void InitChildren(List<string> names)
        {
            if (rectChildren.Count == 0) CalculateLayoutInputHorizontal();
            _initChildCount(names != null && names.Count > 0 ? names.Count : 0);
            //if((names != null && m_RectChildren.Count != names.Count) || (names == null && m_RectChildren.Count != 0))
            //{
            //    Log.LogError($"error:{m_RectChildren} != {names.Count}");
            //    return;
            //}
            for (int i = 0; i < rectChildren.Count; ++i)
            {
                rectChildren[i].gameObject.name = names[i];
            }
        }
        public virtual void InitChildren(int count, System.Action<int, RectTransform> initItemCallback = null)
        {
            if (rectChildren.Count == 0)
                CalculateLayoutInputHorizontal();
            _initChildCount(count);

            for (int i = 0; i < rectChildren.Count; ++i)
            {
                initItemCallback?.Invoke(i, rectChildren[i]);
            }
        }

        public virtual void InitChildren(List<string> names, List<object> paramList = null, System.Action<int, RectTransform> initItemCallback = null)
        {
            if (rectChildren.Count == 0)
                CalculateLayoutInputHorizontal();
            _initChildCount(names != null && names.Count > 0 ? names.Count : 0);
            //if((names != null && m_RectChildren.Count != names.Count) || (names == null && m_RectChildren.Count != 0))
            //{
            //    Log.LogError($"error:{m_RectChildren} != {names.Count}");
            //    return;
            //}
            for (int i = 0; i < rectChildren.Count; ++i)
            {
                rectChildren[i].gameObject.name = names[i];
                if (paramList != null && paramList.Count > i)
                {
                    rectChildren[i].gameObject.SetParam(paramList[i]);
                }
                else
                {
                    rectChildren[i].gameObject.SetParam(null);
                }

                initItemCallback?.Invoke(i, rectChildren[i]);
            }
            initItemCallback = null;
        }

        void _initChildCount(int num)
        {
            if (num < rectChildren.Count)
            {
                if (num == 0)
                {
                    if (rectChildren.Count > 0)
                    {
                        _seed = rectChildren[0];
                        _seed.gameObject.SetActive(false);

                        for (int i = 1; i < rectChildren.Count; ++i)
                        {
                            GameObject.Destroy(rectChildren[i].gameObject);
                        }
                    }

                    rectChildren.Clear();
                }
                else
                {
                    for (int i = num; i < rectChildren.Count; ++i)
                    {
                        GameObject.Destroy(rectChildren[i].gameObject);
                    }

                    rectChildren.RemoveRange(num, rectChildren.Count - num);
                }

            }

            var n = rectChildren.Count;
            for (int i = n; i < num; ++i)
            {
                RectTransform newItem = null;
                if (rectChildren.Count == 0)
                {
                    if (_seed)
                    {
                        _seed.gameObject.SetActive(true);
                        newItem = _seed;
                        _seed = null;
                    }
                    else
                    {
                        Log.LogError("not seek,cannt add new item");
                        newItem = GameObjectUtils.AddChild<RectTransform>(gameObject);
                    }
                }
                else
                {
                    var go = GameObject.Instantiate(rectChildren[rectChildren.Count - 1].gameObject, gameObject.GetRectTransform());
                    var itws = go.GetComponentsInChildren<iTween>();
                    for (int j = 0; j < itws.Length; ++j)
                    {
                        GameObject.Destroy(itws[j]);
                    }
                    newItem = go.GetRectTransform();
                }

                rectChildren.Add(newItem);
            }

        }
    }
}
