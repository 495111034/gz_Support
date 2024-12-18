
namespace UnityEngine.UI
{
    public class UITextHeightFontAutoSize : MonoBehaviour
    {
        private int font_size = 0;
        private MyText myText;
        private RectTransform rect_trans;
        private string lost_text = "";
        private void Start()
        {
            myText = gameObject.GetComponent<MyText>();
            if (myText == null)
            {
                return;
            }
            rect_trans = myText.transform as RectTransform;
            font_size = myText.fontSize;
            myText.onValueChange ??= new Events.UnityEvent();
            myText.onValueChange.AddListener(ActiveUpdate);
        }

        private void ActiveUpdate()
        {
            this.enabled = true;
        }

        private void LateUpdate()
        {
            if (!lost_text.Equals(myText.text))
            {
                myText.fontSize = font_size;
                float height = rect_trans.rect.height;
                while (myText.fontSize > 6 && myText.preferredHeight > height)
                {
                    myText.fontSize--;
                }
                lost_text = myText.text;
            }
            else
            {
                this.enabled = false;
            }
        }
    }
}
