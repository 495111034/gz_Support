using UnityEngine;
using UnityEngine.UI;

public class FuncOpenComponent : MonoBehaviour
{
    [Header("未解锁时置灰列表")]
    public MaskableGraphic[] fadeImages = null;

    [Header("未解锁时隐藏列表")]
    public GameObject[] hideObjs = null;

    [Header("未解锁时显示列表")]
    public GameObject[] showObjs = null;

    [Header("未解锁时内容文本")]
    public MyText text_desc = null;

    [Header("绑定的功能id")]
    public int funcOpenId = 0;

    private MyToggle toggle = null; //如果自己是MyToggle则需要在子级新建一个button用于拦截，并且button上也加上FuncOpenComponent组件，作为独立的判断对象

    private void Start()
    {
        toggle = gameObject.GetComponent<MyToggle>();

        this.SendMessageUpwards("__FuncOpenCompInit", this);
    }

    public void SetState(bool isOpen)
    {
        if (toggle != null)
        {
            toggle.interactable = isOpen;
        }

        if (text_desc != null)
        {
            text_desc.gameObject.SetActiveX(!isOpen);
        }

        if (fadeImages != null)
        {
            for (int i = 0; i < fadeImages.Length; i++)
            {
                if (fadeImages[i] != null)
                {
                    if (fadeImages[i] is MyImage)
                    {
                        (fadeImages[i] as MyImage).IsFade = !isOpen;
                    }
                    else if (fadeImages[i] is MySpriteImage)
                    {
                        (fadeImages[i] as MySpriteImage).IsFade = !isOpen;
                    }
                    else if (fadeImages[i] as MyText)
                    {
                        (fadeImages[i] as MyText).color = isOpen ? Color.white : Color.gray;
                    }
                }
            }
        }

        if (hideObjs != null)
        {
            for (int i = 0; i < hideObjs.Length; i++)
            {
                if (hideObjs[i] != null)
                {
                    hideObjs[i].SetActiveX(isOpen);
                }
            }
        }

        if (showObjs != null)
        {
            for (int i = 0; i < showObjs.Length; i++)
            {
                if (showObjs[i] != null)
                {
                    showObjs[i].SetActiveX(!isOpen);
                }
            }
        }
    }

    public void SetLockDesc(string text)
    {
        text_desc.text = text;
    }
}
