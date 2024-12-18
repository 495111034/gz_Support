
namespace UnityEngine.UI
{
    public class MyUIAudioLoader : MonoBehaviour
    {
        [Header("音频资源名")]
        public string resName = "";

        private void OnEnable()
        {
            if (!string.IsNullOrEmpty(resName))
            {
                SendMessageUpwards("__OnPlaySound", resName);
            }
        }
    }
}
