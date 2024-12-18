using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; 
/// <summary> 移动设备输入框的自适应组件 </summary> 
public class ChatViewAdaptMobileKeyBoard : MonoBehaviour 
{ 
    public MyInputField _inputField; 
    public RectTransform adaptPanelRt;
    public Dictionary<RectTransform, Vector2> other_rts;
    private RectTransform canvas_scale_rect;
    private Canvas canvas;
    float keyboardHeight_local = 0; 
    float keyboardHeight_last = 0; 
    /// <summary> 自适应（弹出输入框后整体抬高）的面板的初始位置 </summary> 
    private Vector2 _adaptPanelSourcePos;
    private Vector2 _adaptPanelOriginPos;
    private float keyboardHeightUi;
    private float speed = 0;
    private int is_up_move = 0;
    private RectTransform _inputFieldRect;
    private bool is_focused = false;
    public float up_offset = 0;

    public static void Create(RectTransform[] attachRoots, MyInputField inputField, float _up_offset) 
    { 
        ChatViewAdaptMobileKeyBoard instance = inputField.gameObject.AddComponent<ChatViewAdaptMobileKeyBoard>();
        instance.adaptPanelRt = attachRoots[0];
        if (attachRoots.Length > 1)
        {
            instance.other_rts = new Dictionary<RectTransform, Vector2>();
            for (int i = 1; i < attachRoots.Length; i++)
            {
                instance.other_rts[attachRoots[i]] = attachRoots[i].anchoredPosition;
            }
        }
        instance._inputField = inputField;
        instance.up_offset = _up_offset;
    }

    private RectTransform[] all_other = null;

    private void Start() 
    {
        _adaptPanelSourcePos = adaptPanelRt.anchoredPosition;
        _inputField._onEndEdit.AddListener(OnEndEdit);
        _inputField.keyboardType = TouchScreenKeyboardType.Default;
        _inputField.shouldHideMobileInput = !PluginApiTools.IsEmulator();
        _inputFieldRect = _inputField.GetComponent<RectTransform>();
        canvas_scale_rect = gameObject.GetComponentInParent<CanvasScaler>().GetComponent<RectTransform>();
        canvas = canvas_scale_rect.GetComponent<Canvas>();
        if (other_rts != null && other_rts.Count > 0)
        {
            all_other = new RectTransform[other_rts.Count];
            other_rts.Keys.CopyTo(all_other, 0);
        }
    } 
    private void Update() 
    { 
        if (_inputField.isFocused) 
        {
            if (!is_focused)
            {
                _adaptPanelOriginPos = adaptPanelRt.anchoredPosition;
                if (other_rts != null && other_rts.Count > 0)
                {
                    foreach (var t in all_other)
                    {
                        if (t == null)
                        {
                            continue;
                        }
                        other_rts[t] = t.anchoredPosition;
                    }
                }
            }
            is_focused = true;
#if UNITY_EDITOR 
            keyboardHeight_local = 0;
#elif UNITY_ANDROID
            keyboardHeight_local = GetKeyboardHeightAndroid();
#elif UNITY_IOS
            keyboardHeight_local = GetKeyboardHeightIOS(); 
#endif
            if (keyboardHeight_last != keyboardHeight_local) 
            { 
                keyboardHeight_last = keyboardHeight_local; 
                float keyboardHeight = keyboardHeight_local * Display.main.systemHeight / Screen.height;

                float k = canvas_scale_rect.sizeDelta.y; 
                keyboardHeightUi = keyboardHeight * k / Display.main.systemHeight;

                if (keyboardHeightUi > 100 && keyboardHeightUi < canvas_scale_rect.sizeDelta.y)
                {
                    adaptPanelRt.anchoredPosition = _adaptPanelSourcePos;
                    var screenPos = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, _inputField.transform.position);
                    screenPos.y -= _inputFieldRect.rect.height / 2;
                    if (keyboardHeightUi > screenPos.y)
                    {
                        keyboardHeightUi = keyboardHeightUi - screenPos.y;
                    }

                    speed = keyboardHeightUi / 0.1f;
                    is_up_move = 1;
                }
                else if (keyboardHeightUi <= 0 && adaptPanelRt.anchoredPosition.y > _adaptPanelOriginPos.y + 50)
                {
                    is_up_move = 2;
                }
                else
                {
                    is_up_move = 0;
                }
            }
        }
        else
        {
            keyboardHeightUi = 0;
            keyboardHeight_last = 0;
        }

        if (is_focused)
        {
            if (is_up_move == 1)
            {
                adaptPanelRt.anchoredPosition = Vector2.MoveTowards(adaptPanelRt.anchoredPosition, _adaptPanelSourcePos + Vector2.up * (keyboardHeightUi + up_offset), Time.deltaTime * speed);
                MoveOtherRectTransform();
            }
            else if ((is_up_move == 2 || keyboardHeightUi > 0) && adaptPanelRt.anchoredPosition.y > _adaptPanelOriginPos.y)
            {
                adaptPanelRt.anchoredPosition = Vector2.MoveTowards(adaptPanelRt.anchoredPosition, _adaptPanelOriginPos, Time.deltaTime * speed);
                MoveOtherRectTransform();
                if (adaptPanelRt.anchoredPosition.y <= _adaptPanelOriginPos.y)
                {
                    is_up_move = 0;
                    is_focused = false;
                    if (_inputField.isFocused)
                    {
                        _inputField.DeactivateInputField();
                    }
                }
            }
            else
            {
                is_up_move = 0;
            }
        }
    }

    private void MoveOtherRectTransform()
    {
        if (other_rts != null && other_rts.Count > 0)
        {
            foreach (var k in all_other)
            {
                if (k == null)
                {
                    continue;
                }
                if (is_up_move == 1)
                {
                    k.anchoredPosition = Vector2.MoveTowards(k.anchoredPosition, other_rts[k] + Vector2.up * (keyboardHeightUi + up_offset), Time.deltaTime * speed);
                }
                else
                {
                    k.anchoredPosition = Vector2.MoveTowards(k.anchoredPosition, other_rts[k], Time.deltaTime * speed);
                }
            }
        }
    }

    /// <summary> 结束编辑事件，TouchScreenKeyboard.isFocused为false的时候 </summary> 
    /// <param name="currentInputString"></param> 
    private void OnEndEdit(string currentInputString) 
    {
        is_up_move = 2;
    } 

    /// <summary> 获取安卓平台上键盘的高度 </summary> 
    /// <returns></returns> 
    public int GetKeyboardHeightAndroid()
    { 
        using (var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) 
        { 
            var unityPlayer = unityClass.GetStatic<AndroidJavaObject>("currentActivity").Get<AndroidJavaObject>("mUnityPlayer"); 
            var view = unityPlayer.Call<AndroidJavaObject>("getView"); 
            //var dialog = unityPlayer.Get<AndroidJavaObject>("mSoftInputDialog"); 
            if (view == null) return 0; 
            var decorHeight = 0; 
            //if (true) //includeInput
            //{
            //    var decorView = dialog.Call<AndroidJavaObject>("getWindow").Call<AndroidJavaObject>("getDecorView");
            //    if (decorView != null) decorHeight = decorView.Call<int>("getHeight");
            //}
            using (var rect = new AndroidJavaObject("android.graphics.Rect"))
            {
                view.Call("getWindowVisibleDisplayFrame", rect);
                return Display.main.systemHeight - rect.Call<int>("height") + decorHeight;
            }
        }
    }

    public static Dictionary<string, int> AndroidGetKeyboardHeight(int canvasHeight)
    {
        Dictionary<string, int> dic = new Dictionary<string, int>();
        //using (AndroidJavaClass unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        //{
        //    AndroidJavaObject player = unityClass.GetStatic<AndroidJavaObject>("currentActivity")
        //    .Get<AndroidJavaObject>("mUnityPlayer");
        //    AndroidJavaObject View = player.Call<AndroidJavaObject>("getView");

        //    using (AndroidJavaObject Rct = new AndroidJavaObject("android.graphics.Rect"))
        //    {
        //        View.Call("getWindowVisibleDisplayFrame", Rct);
        //        int androidRectH = Rct.Call<int>("height"); // 这是非键盘部分的高度。
        //        int h = Screen.height - androidRectH;// 软键盘高度
        //        try
        //        {// unity2018 值 "b", unity2019 值 "mSoftInputDialog";
        //            var dialog = player.Get<AndroidJavaObject>("mSoftInputDialog");
        //            if (dialog != null)
        //            {
        //                if (!TouchScreenKeyboard.hideInput)
        //                {
        //                    var decorView = dialog.Call("getWindow")
        //                    .Call("getDecorView");
        //                    if (decorView != null)
        //                    {
        //                        int h1 = decorView.Call("getHeight");
        //                        h += h1;
        //                        dic["dialogH"] = h1;
        //                    }
        //                }
        //            }
        //        }
        //        catch (Exception e)
        //        {
        //            dic["dialogH"] = -1;
        //        }

        //        dic["androidRectH"] = androidRectH;
        //        dic["keyboardInScreenHeight"] = h;
        //        dic["keyboardInCanvasHeight"] = (int)(h * canvasHeight / Screen.height);
        //    }
        //}

        return dic;
    }

    public int GetKeyboardHeightIOS()
    {
        return (int)TouchScreenKeyboard.area.height;
    }
}