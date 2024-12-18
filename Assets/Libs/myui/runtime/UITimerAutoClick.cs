
namespace UnityEngine.UI
{
    //延迟时间自动触发按钮点击事件，该组件请挂在button上
    public class UITimerAutoClick : MonoBehaviour
    {
        public float delayTime = 0; //延迟的时间
        public int decimalPointNum = 0; //显示小数位
        public MyText text = null;
        private float time = 0;
        private string decimalStr = "f0";
        private string source_text = null;

        private void OnEnable()
        {
            time = delayTime;
            decimalStr = $"f{decimalPointNum}";
        }

        public void UseSourceText(string content)
        {
            source_text = content;
        }

        private void Update()
        {
            if (time > 0)
            {
                time -= Time.deltaTime;
                if (time < 0) time = 0;
                if (text != null)
                {
                    if (!string.IsNullOrEmpty(source_text))
                    {
                        text.text = string.Format(source_text, time.ToString(decimalStr));
                    }
                    else
                    {
                        text.ChangeLanguageParams(time.ToString(decimalStr));
                    }
                }
                if (time <= 0)
                {
                    MyUITools.PointerClick(gameObject);
                }
            }
        }
    }
}
