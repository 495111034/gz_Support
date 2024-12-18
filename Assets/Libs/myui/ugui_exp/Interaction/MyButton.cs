using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/MyButton", 0)]
    public class MyButton : Button, IPointerLongpressHandler, IUITips, IPointerUpHandler,IPointerDownHandler
    {


        PointerEventData _pressingEventData;
        public PointerEventData pressingEventData => _pressingEventData;

        [HideInInspector]
        [SerializeField]
        string soundOnClick ;

        /// <summary>
        /// 点击时的声音
        /// </summary>
        public string SoundOnClick
        {
            get { return soundOnClick; }
            set { soundOnClick = value; }
        }

        [SerializeField]
        float animCoefficient = 0.11f;

        Vector3 initScale;

        [HideInInspector]
        [SerializeField]
        string soundOnLongPress ;

        /// <summary>
        /// 长按声音
        /// </summary>
        public string SoundOnLongPress
        {
            get { return soundOnLongPress; }
            set { soundOnLongPress = value; }
        }

        public float AnimCoefficient
        {
            get
            {
                return animCoefficient;
            }

            set
            {
                animCoefficient = value;
            }
        }
        
        public MyButtonBar butonBar
        {
            get {
                return __butonBar;
            }
            set
            {
                __butonBar = value;
            }
        }

        private MyButtonBar __butonBar = null;
        public MyMenu menu = null;
        public Action<MyButton> ForceClickEvent { get; set; }

        public void OnLongPressRepeat(PointerEventData eventData)
        {

            //_step1("OnLongPressRepeat");
            if (Application.isPlaying)
            {
                if (butonBar)
                {
                    butonBar.OnLongClick(this);
                }
                else if(menu)
                {
                    menu.OnBtnLongPress(this);
                }                
                else
                {
                    _pressingEventData = eventData;
                    SendMessageUpwards("__OnLongPress", this);
                    _pressingEventData = null;
                }

                if (!string.IsNullOrEmpty(soundOnLongPress))
                {
                    SendMessageUpwards("__OnPlaySound", soundOnLongPress);
                }
            }
            //_step2("OnLongPressRepeat", eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            //_step1("OnPointerUp");

            if (butonBar)
            {              
            }
            else if (menu)
            {               
            }
            else
            {               
                SendMessageUpwards("__OnPointerUp", eventData);
            }

            //_step2("OnPointerUp", eventData);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {

            //_step1("OnPointerDown");

            if (butonBar)
            {

            }
            else if (menu)
            {

            }
            else
            {
                _pressingEventData = eventData;
                SendMessageUpwards("__OnGuidePointerDown", this);
                SendMessageUpwards("__OnPointerDown", this);
                _pressingEventData = null;
            }
            //_step2("OnPointerDown", eventData);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {

            //_step1("OnPointerClick");

            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            OnPostClientEvent();

            //step2("OnPointerClick", eventData);
        }

        public override void OnSubmit(BaseEventData eventData)
        {

            //_step1("OnSubmit");

            OnPostClientEvent();

            if (!IsActive() || !IsInteractable())
                return;

            DoStateTransition(SelectionState.Pressed, false);
            StartCoroutine(OnFinishSubmit());

            //_step2("OnSubmit", eventData);
        }



        private IEnumerator OnFinishSubmit()
        {
            var fadeTime = colors.fadeDuration;
            var elapsedTime = 0f;

            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }

            DoStateTransition(currentSelectionState, false);
        }

        public float intervalTime = 0;//间隔时间
        float nextClickTime;

        Hashtable PunchScale = null;
        void OnPostClientEvent()
        {
            if (Application.isPlaying)
            {
                if(intervalTime > 0)
                {
                    if (Time.unscaledTime < nextClickTime)
                    { 
                        return; 
                    }
                    nextClickTime = Time.unscaledTime + intervalTime;
                }

                if (animCoefficient >= 0.1f)
                {
                    //Log.LogInfo("PunchScale");
                    if (PunchScale == null) 
                    {
                        PunchScale = new Hashtable();
                    }
                    iTween.StopImmediate(gameObject);
                    gameObject.transform.localScale = initScale;
                    PunchScale["amount"] = Vector3.one * animCoefficient;
                    PunchScale["time"] = 0.5f;
                    iTween.PunchScale(gameObject, PunchScale);
                }

                if (!string.IsNullOrEmpty(soundOnClick))
                {
                    SendMessageUpwards("__OnPlaySound", soundOnClick);
                }

                if (butonBar)
                {
                    butonBar.OnClick(this);
                }
                else if (menu)
                {
                    menu.OnBtnClick(this);
                } 
                else if(ForceClickEvent != null)
                {
                    ForceClickEvent(this);
                }
                else
                {
                    SendMessageUpwards("OnClickEvent", this);
                }
            }
        }


        public string btnText
        {
            get
            {
                if (gameObject.FindInChild<MyText>())
                {
                    return gameObject.FindInChild<MyText>().text;
                }
                return "";
            }
            set
            {
                if (gameObject.FindInChild<MyText>())
                {
                    gameObject.FindInChild<MyText>().text = value;
                }
            }
        }

        private MySpriteImage[] sps;
        private MyImage[] images;
        private MyText[] texts;
        private MyImageText[] textimages;

        private bool _isfade = false;
        public bool IsFade
        {
            set
            {
                if (_isfade != value)
                {
                    _isfade = value;
                    if(sps == null) sps = this.GetComponentsInChildren<MySpriteImage>();
                    for (int i = 0; i < sps.Length; i++)
                    {
                        sps[i].IsFade = _isfade;
                    }

                    if (images == null) images = this.GetComponentsInChildren<MyImage>();
                    for (int i = 0; i < images.Length; i++)
                    {
                        images[i].IsFade = _isfade;
                    }

                    if (texts == null) texts = this.GetComponentsInChildren<MyText>();
                    for (int i = 0; i < texts.Length; i++)
                    {
                        texts[i].IsFade = _isfade;
                    }

                    if (textimages == null) textimages = this.GetComponentsInChildren<MyImageText>();
                    for (int i = 0; i < textimages.Length; i++)
                    {
                        textimages[i].IsFade = _isfade;
                    }
                }
            }
            get { return _isfade; }
        }

        protected override void OnEnable()
        {
            if(initScale == Vector3.zero)
            {
                initScale = gameObject.transform.localScale;
            }
            base.OnEnable();
        }

        protected override void OnDisable()
        {
            if(GetComponent<iTween>())
            {
                GameObject.Destroy(GetComponent<iTween>());
                gameObject.transform.localScale = initScale;
            }
            base.OnDisable();
        }

        


        #region tips object
        [HideInInspector]
        [SerializeField]
        string stringTips = "";
        object tipsObj = null;
        public object TipParams { get { return string.IsNullOrEmpty(stringTips)? tipsObj : stringTips; } set { tipsObj = value; stringTips = ""; } }
        #endregion

    }
}
