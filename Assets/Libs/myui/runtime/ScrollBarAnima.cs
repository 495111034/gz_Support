
namespace UnityEngine.UI
{
    public class ScrollBarAnima : MonoBehaviour
    {
        public System.Action<int> finshEvent;

        public MyImageText imageText;
        public float parentRectWidth = 435; //裁剪区域宽度
        public float speed = 0; //移动速度

        private Transform trans;
        private Vector2 textSizeDelta;
        private Vector3 endPos;
        private float nextStartX;

        [HideInInspector]
        public float speedRatio = 1;

        private Vector3 target_pos;

        private bool is_remove_raycastTarget = false;

        public void SetIsRemoveRaycastTarget(bool b)
        {
            is_remove_raycastTarget = b;
        }

        private void Awake()
        {
            trans = transform;
            textSizeDelta = imageText.rectTransform.sizeDelta;
            endPos = trans.localPosition;
            if (parentRectWidth == 0)
            {
                parentRectWidth = (trans.parent as RectTransform).sizeDelta.x;
            }
            this.enabled = false;
        }
        public void SetStartPosY(float y)
        {
            endPos.y = y;
        }

        public void PlayAnima()
        {
            target_pos = endPos;
            imageText.rectTransform.sizeDelta = textSizeDelta;
            float width = imageText.preferredWidth;
            target_pos.x = width * 0.5f + parentRectWidth * 0.5f;
            target_pos.y += imageText.emoji_y_offset;
            trans.localPosition = target_pos;
            imageText.rectTransform.sizeDelta = new Vector2(width, textSizeDelta.y);
            target_pos.x -= parentRectWidth + width;
            nextStartX = target_pos.x + parentRectWidth * 0.75f;
            imageText.RecalculateClipping();
            imageText.RecalculateMasking();
            //imageText.enabled = false;
            //imageText.enabled = true;
            this.enabled = true;
        }

        private void LateUpdate()
        {
            if (is_remove_raycastTarget)
            {
                var href_list = imageText.GetAllHrefInfos();
                if (href_list != null && href_list.Count > 0)
                {
                    href_list.Clear();
                    imageText.raycastTarget = false;
                }
            }
            trans.localPosition = Vector3.MoveTowards(trans.localPosition, target_pos, Time.deltaTime * speed * speedRatio);
            if (trans.localPosition.x < target_pos.x + 1)
            {
                this.imageText.text = null;
                this.enabled = false;
                if (finshEvent != null)
                {
                    finshEvent(2);
                }
            }
            else if (nextStartX != 0 && trans.localPosition.x < nextStartX)
            {
                nextStartX = 0;
                if (finshEvent != null)
                {
                    finshEvent(1);
                }
            }
        }
    }
}
