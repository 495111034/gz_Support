
namespace UnityEngine.UI
{
#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
    public class UISliderMoveToPosition : MonoBehaviour
    {
        public RectTransform target;

        public Vector2 from;
        public Vector2 to;

        public float slider = 0;

        public bool use_curr_from;

        private float _slider = 0;

        private void OnEnable()
        {
            if (target != null && use_curr_from)
            {
                from = target.anchoredPosition;
            }
        }

        private void Update()
        {
            if (target != null)
            {
                if (slider != _slider)
                {
                    _slider = slider;
                    target.anchoredPosition = Vector2.Lerp(from, to, slider);
                }
            }
        }
    }
}
