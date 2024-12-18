
namespace UnityEngine.UI
{
    public class UICalcGridLayoutGroupCount : MonoBehaviour
    {
        public bool is_hor;
        private GridLayoutGroup group;
        private Vector2 cell_size;
        private Vector2 cell_spacing;
        private Vector2 parent_rect_wh;
        private RectTransform parent;

        private void Start()
        {
            group = gameObject.GetComponent<GridLayoutGroup>();
            cell_size = group.cellSize;
            cell_spacing = group.spacing;
            parent = transform.parent as RectTransform;
        }

        private void LateUpdate()
        {
            if (cell_size.x != group.cellSize.x || cell_size.y != group.cellSize.y
                || cell_spacing.x != group.spacing.x || cell_spacing.y != group.spacing.y
                || parent_rect_wh.x != parent.rect.size.x || parent_rect_wh.y != parent.rect.size.y)
            {
                cell_size = group.cellSize;
                cell_spacing = group.spacing;
                parent_rect_wh = parent.rect.size;
                int count = 0;
                if (is_hor)
                {
                    float size = cell_size.x + cell_spacing.x;
                    count = (int)(parent.rect.width / size);
                    if (parent.rect.width - count * size > cell_size.x)
                    {
                        count++;
                    }
                }
                else
                {
                    float size = cell_size.y + cell_spacing.y;
                    count = (int)(parent.rect.height / size);
                    if (parent.rect.height - count * size > cell_size.y)
                    {
                        count++;
                    }
                }
                group.constraintCount = count;
            }
        }
    }
}
