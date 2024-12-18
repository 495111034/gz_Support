using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using easing;
using System.Linq;
using UnityEngine.EventSystems;
#if UNITY_ASYNC
using System.Threading;
using System.Threading.Tasks;
#endif

namespace UnityEngine.UI
{
    public class MyButtonBar : MyGridLayoutGroup, IPointerLongpressHandler
    {

        PointerEventData _pressingEventData;
        public PointerEventData pressingEventData => _pressingEventData;

        /// <summary>
        /// 按钮效果类型
        /// </summary>
        public enum ButtonEffectType
        {
            effect = 1,         //显示附加特效
            anim = 2,           //播放动画
        };

        public class ButtonInfo
        {
            public int id;                  //功能ID
            public bool visible;    
            public MyButton btn;
            public EmptyImage rootRect;     //rootrect,必须是emptyImage类型
            public int sort_id;             //排序id
            public string btnText;          //显示字符
            public string btnImagePath;          //显示图片
            public bool flash;              //提醒
            public bool play_effect;        //特效
            public bool play_notify;        //通知
            public GameObject effect_go;    //特效gameobject,effectType=effect时有用
            public GameObject effect_notify; //通知gameobject,notifyType=effect时有用            

            public ButtonEffectType effectType;
            public ButtonEffectType notifyType;

            public class ClientAniInfo
            {
                public string icon;
                public bool use_scale = false;
                public bool use_rotate = false;
                public Vector3 scale_form;
                public Vector3 scale_to;
                public Vector3 rotate_form;
                public Vector3 rotate_to;
            }

            public bool isRemoved;

            public virtual void OnInitCallback(ButtonInfo info) { }
            public virtual bool MayShow() { return true; }
            public virtual bool MayNotity() { return true; }
            public virtual bool MayEffect() { return true; }
            public virtual string effect_id() { return ""; }
            public virtual string notify_id() { return ""; }
            public virtual void OnClickCall() { }
            public virtual void OnUpdateBaseCall() { }

            public bool isMoving = false;
        }

        class itweenResetParam
        {
            public Transform target;
            public Vector3 localPosition;
        }

        [HideInInspector]
        [SerializeField]
        private bool _isVertical;

        [HideInInspector]
        [SerializeField]
        private float m_itemWidth = 55f;
        [HideInInspector]
        [SerializeField]
        private float m_itemHeight = 55f;

        private List<ButtonInfo> _btn_infos = new List<ButtonInfo>();

        private int _current_click_btn = 0;
        private bool _isStart = false;

        public float ItemWidth
        {
            get
            {
                return m_itemWidth;
            }
            set
            {
                m_itemWidth = value;
            }
        }

        public float ItemHeight
        {
            get
            {
                return m_itemHeight;
            }
            set
            {
                m_itemHeight = value;
            }
        }


        public List<ButtonInfo> CurrentButtonList
        {
            get
            {
                return _btn_infos;
            }
        }

        /// <summary>
        /// 最后一次点击的按钮
        /// </summary>
        public ButtonInfo CurrentClickBtn
        {
            get                                                             
            {
                return _current_click_btn >= 0 && _current_click_btn < _btn_infos.Count ? _btn_infos[_current_click_btn] : null;
            }
        }

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            //CalcAlongAxis(0, _isVertical);
        }

        public override void CalculateLayoutInputVertical()
        {
            base.CalculateLayoutInputVertical();
            //CalcAlongAxis(1, _isVertical);
        }

        public override void SetLayoutHorizontal()
        {
            SetCellsAlongAxis(0);
        }

        public override void SetLayoutVertical()
        {
            SetCellsAlongAxis(1);
        }

        protected override void Awake()
        {

            base.Awake();
           // Start();
        }
        protected override void OnEnable()
        {
            base.OnEnable();
        }

        public void OnClick(MyButton btn)
        {
            var info = btn.GetParam() as ButtonInfo;
            if (info == null) return;

            if (info.isRemoved) return;

            _current_click_btn = _btn_infos.IndexOf(info);

            info.OnClickCall();
           // SendMessageUpwards("OnClickEvent", this);
        }

        public void OnLongClick(MyButton btn)
        {
            var info = btn.GetParam() as ButtonInfo;
            if (info == null) return;

            if (info.isRemoved) return;
            _current_click_btn = _btn_infos.IndexOf(info);

            _pressingEventData = btn.pressingEventData;
            SendMessageUpwards("__OnLongPress", this);
            _pressingEventData = null;
        }

        public void OnLongPressRepeat(PointerEventData eventData)
        {
        }


        public void InitButtonLists(List<ButtonInfo> btninfoList)
        {
            var names = btninfoList.Select(item =>$"button_{item.id}").ToList();
            InitChildren(names, btninfoList.ToList<object>());
            _isStart = false;
            Start();
            
        }

        public bool IsMoving()
        {
            for(int i = 0; i < _btn_infos.Count; ++i)
            {
                if (_btn_infos[i].isMoving) return true;
            }

            return false;
        }

        protected override void Start()
        {
            if (_isStart) return;
            _isStart = true;

            base.Start();

            _btn_infos.Clear();
           
            for (int i = 0; i < gameObject.transform.childCount; ++i)
            {
                var go = gameObject.transform.GetChild(i).gameObject;
                ButtonInfo btnInfo = go.GetParam() as ButtonInfo;
                if (btnInfo != null)
                {
                    if (go.GetComponent<EmptyImage>() && go.GetComponentInChildren<MyButton>())
                    {

                        btnInfo.rootRect = go.GetComponent<EmptyImage>();
                        btnInfo.btn = go.GetComponentInChildren<MyButton>();       
                        if(!btnInfo.btn)
                        {
                            Log.LogError($"11 MyButtons bar is null {gameObject.name},{go.name}");
                        }
                        //btnInfo.sort_id = i;

                        // if (go.GetComponentInChildren<MyUI3DObject>())
                        //     btnInfo.effect_go = go.GetComponentInChildren<MyUI3DObject>().gameObject;


                    }
                    else if (!go.GetComponent<EmptyImage>() && go.GetComponent<MyButton>())
                    {
                        btnInfo.rootRect = null;
                        btnInfo.btn = go.GetComponent<MyButton>();
                        if (!btnInfo.btn)
                        {
                            Log.LogError($"22 MyButtons bar is null {gameObject.name},{go.name}");
                        }
                        //btnInfo.sort_id = i;
                        // if (go.GetComponentInChildren<MyUI3DObject>())
                        //     btnInfo.effect_go = go.GetComponentInChildren<MyUI3DObject>().gameObject;

                    }
                    _btn_infos.Add(btnInfo);
                }
            }

            InitButtonList();
        }

        public void AddButton(ButtonInfo btnInfo, bool showAnim = false)
        {
            if (btnInfo.isRemoved) return;

            if (_btn_infos.Contains(btnInfo) && !btnInfo.isRemoved)
            {
                UpdateButton(btnInfo);
                return ;
            }

            var newitem = AddChildItem();
            var go = newitem.gameObject;
            go.SetParam(btnInfo);
           
            if (go.GetComponent<EmptyImage>() && go.GetComponentInChildren<MyButton>())
            {
                btnInfo.rootRect = go.GetComponent<EmptyImage>();
                btnInfo.btn = go.GetComponentInChildren<MyButton>();
            }
            else if (!go.GetComponent<EmptyImage>() && go.GetComponent<MyButton>())
            {
                btnInfo.rootRect = null;
                btnInfo.btn = go.GetComponent<MyButton>();
            }

            go.name = $"button_{btnInfo.id}";
            _btn_infos.Add(btnInfo);           

            InitButtonList();

            if (showAnim)
            {
#if UNITY_ASYNC
                ShowInsertAminAsync(btnInfo);
#else
                MyTask.Run(ShowInsertAminAsync(btnInfo));
#endif

            }
            else
            {
                btnInfo.rootRect.rectTransform.SetSize(new Vector2(m_itemWidth, m_itemHeight));               
            }
           
        }

        public void RemoveButton(ButtonInfo btnInfo, bool showAnim = false)
        {
            if (btnInfo.isRemoved) return;
            btnInfo.isRemoved = true;
            btnInfo.rootRect.gameObject.SetParam(null);
            if (showAnim)
            {
#if UNITY_ASYNC
                ShowRemoveAnimAsync(btnInfo);
#else
                MyTask.Run(ShowRemoveAnimAsync(btnInfo));
#endif

                return;
            }
            else
            {               
                btnInfo.btn.SetParam(null);
                RemoveChildItem(btnInfo.rootRect.gameObject);
                _btn_infos.Remove(btnInfo);
                btnInfo.isRemoved = false;
            }
        }

        public void UpdateButton(ButtonInfo btnInfo)
        {
            if (btnInfo.isRemoved) return;
           if(_btn_infos.Contains(btnInfo) && !btnInfo.isRemoved)
            {
                btnInfo.OnInitCallback(btnInfo);
                __refreshButonItemState(btnInfo);
            }
            else
            {
                AddButton(btnInfo, false);
            }
        }

        public void RefreshButtons()
        {
            if (IsMoving()) return;
            List<ButtonInfo> waitRemove = new List<ButtonInfo>();
            for (int i = 0; i < _btn_infos.Count; ++i)
            {
                var show = _btn_infos[i].MayShow();
                if( !show)
                {
                    waitRemove.Add(_btn_infos[i]);
                }
                else
                {
                    _btn_infos[i].OnInitCallback(_btn_infos[i]);
                    __refreshButonItemState(_btn_infos[i]);
                }
            }
            for(int i = 0; i < waitRemove.Count; ++i)
            {
                RemoveButton(waitRemove[i], true);
            }
            waitRemove.Clear();
        }

        public void ClearButtonInfo()
        {
            _btn_infos.Clear();
            rectChildren.Clear();
        }

        void  __refreshButonItemState(ButtonInfo btnInfo)
        {          
            if (btnInfo.MayEffect())
            {
                if (!btnInfo.play_effect)
                {
                    btnInfo.btn.butonBar.PlayButtonEffect(btnInfo, btnInfo.effect_id());
                }

            }
            else
            {
                if (btnInfo.play_effect)
                {
                    btnInfo.btn.butonBar.StopButtonEffect(btnInfo);
                }
            }           
            if (btnInfo.MayNotity())
            {
                if (!btnInfo.play_notify)
                {
                    btnInfo.btn.butonBar.NotifyButton(btnInfo, btnInfo.notify_id());
                }
            }
            else
            {
                if (btnInfo.play_notify)
                {
                    btnInfo.btn.butonBar.StopNotifyButton(btnInfo);
                }
            }
        }

        void InitButtonList()
        {
            _btn_infos.Sort((a, b) =>
            {
                return a.sort_id - b.sort_id;
            });

            for (int i = 0; i < _btn_infos.Count; ++i)
            {
                if(!_btn_infos[i].btn)
                {
                    Log.LogError($"MyButtons InitButtonList,_btn_infos[{i}].btn is null");
                    continue;   
                }

                if (!_btn_infos[i].rootRect)
                {
                    _btn_infos[i].rootRect = GameObjectUtils.AddChild<EmptyImage>(gameObject);
                    _btn_infos[i].btn.transform.SetParent(_btn_infos[i].rootRect.transform, true);
                    _btn_infos[i].rootRect.rectTransform.SetSize(_btn_infos[i].btn.targetGraphic.rectTransform.GetSize());
                }

                _btn_infos[i].btn.butonBar = this;
               
                _btn_infos[i].btn.SetParam(_btn_infos[i]);

                var rt = _btn_infos[i].btn.targetGraphic.rectTransform;
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0, 0);

                _btn_infos[i].rootRect.rectTransform.SetSiblingIndex(_btn_infos[i].sort_id);

                _btn_infos[i].OnInitCallback(_btn_infos[i]);
                __refreshButonItemState(_btn_infos[i]);


            }
        }

       

        public void FlashButton(ButtonInfo btn)
        {
            if (btn.flash) return;
            btn.flash = true;

            if (!btn.btn)
            {
                Log.LogError($"{btn.btnText} cannt to flash,because cannt found mybutton.");
                return;
            }

            iTween.Stop(btn.btn.targetGraphic.gameObject);
            btn.btn.targetGraphic.gameObject.transform.localPosition = Vector3.zero;

            Hashtable p = new Hashtable();
            p["x"] = _isVertical ? btn.btn.targetGraphic.rectTransform.GetSize().x / Screen.width  : 0;
            p["y"] = _isVertical ? 0 : btn.btn.targetGraphic.rectTransform.GetSize().y / Screen.height ;
            p["z"] = 0;
            p["islocal"] = false;
            p["looptype"] = "loop";
            p["oncomplete"] = "resertITweenPosition";
            p["oncompletetarget"] = this.gameObject;
            p["oncompleteparams"] = new itweenResetParam()
            {
                target = btn.btn.targetGraphic.gameObject.transform,
                localPosition = btn.btn.targetGraphic.gameObject.transform.localPosition,
            };
            iTween.PunchPosition(btn.btn.targetGraphic.gameObject, p);
        }

        void resertITweenPosition(itweenResetParam p)
        {            
            p.target.localPosition = p.localPosition;
        }

        public void StopFlashButton(ButtonInfo btn)
        {
            if (!btn.flash) return;
            btn.flash = false;

            if (!btn.btn)
            {
                Log.LogError($"{btn.btnText} cannt to StopFlash,because cannt found mybutton.");
                return;
            }

            iTween.Stop(btn.btn.targetGraphic.gameObject);
            btn.btn.targetGraphic.gameObject.transform.localPosition = Vector3.zero;
        }

        public void NotifyButton(ButtonInfo btn,string effect_param = "")
        {
            if (btn.play_notify) return;
            btn.play_notify = true;

            if (!btn.btn)
            {
                Log.LogError($"{btn.btnText} cannt to Notify,because cannt found mybutton.");
                return;
            }

            if(btn.notifyType == ButtonEffectType.effect)
            {
                if(!btn.effect_notify)
                {
                    Log.LogError($"{btn.btnText} cannt to Notify,because not define effect_notify gameobject");
                    return;
                }

                btn.effect_notify.SetActiveAndResert();
                if(!string.IsNullOrEmpty(effect_param) &&  btn.effect_notify.GetComponent<MyUI3DObject>())
                {
                    btn.effect_notify.GetComponent<MyUI3DObject>().PrefabName = effect_param;
                }
            }
            else if(btn.notifyType == ButtonEffectType.anim)
            {
                if (btn.effect_notify)
                    btn.effect_notify.gameObject.SetActive(false);

                iTween.Stop(btn.btn.targetGraphic.gameObject);
                btn.btn.targetGraphic.gameObject.transform.localPosition = Vector3.zero;

                Hashtable p = new Hashtable();
                p["x"] = _isVertical ? btn.btn.targetGraphic.rectTransform.GetSize().x / Screen.width : 0;
                p["y"] = _isVertical ? 0 : btn.btn.targetGraphic.rectTransform.GetSize().y / Screen.height;
                p["z"] = 0;
                p["islocal"] = false;
                p["looptype"] = "loop";
                p["oncomplete"] = "resertITweenPosition";
                p["oncompletetarget"] = this.gameObject;
                p["oncompleteparams"] = new itweenResetParam()
                {
                    target = btn.btn.targetGraphic.gameObject.transform,
                    localPosition = btn.btn.targetGraphic.gameObject.transform.localPosition,
                };
                iTween.PunchPosition(btn.btn.targetGraphic.gameObject, p);
            }
        }

        public void StopNotifyButton(ButtonInfo btn)
        {
            if (!btn.play_notify) return;
            btn.play_notify = false;

            if (!btn.btn)
            {
                Log.LogError($"{btn.btnText} cannt to StopNotify,because cannt found mybutton.");
                return;
            }

            if(btn.effect_notify)
            {
                btn.effect_notify.SetActive(false);
            }

            if (btn.notifyType == ButtonEffectType.anim)
            {
                iTween.Stop(btn.btn.targetGraphic.gameObject);
                btn.btn.targetGraphic.gameObject.transform.localPosition = Vector3.zero;
            }
        }

        public void PlayButtonEffect(ButtonInfo btn, string effect_param = "")
        {
            if (btn.play_effect) return;
            btn.play_effect = true;

            if (!btn.btn)
            {
                Log.LogError($"{btn.btnText} cannt to PlayBuuutonEffect,because cannt found mybutton.");
                return;
            }

            if (btn.effectType == ButtonEffectType.effect)
            {
                if (!btn.effect_go)
                {
                    Log.LogError($"{btn.btnText} cannt to Notify,because not define effect_notify gameobject");
                    return;
                }

                btn.effect_go.SetActiveAndResert();
                if (!string.IsNullOrEmpty(effect_param) && btn.effect_go.GetComponent<MyUI3DObject>())
                {
                    btn.effect_go.GetComponent<MyUI3DObject>().PrefabName = effect_param;
                }
            }
            else if (btn.effectType == ButtonEffectType.anim)
            {
                iTween.Stop(btn.btn.targetGraphic.gameObject);
                btn.btn.targetGraphic.gameObject.transform.localPosition = Vector3.zero;

                Hashtable p = new Hashtable();
                p["x"] =  btn.btn.targetGraphic.rectTransform.GetSize().x / Screen.width / 2f;
                p["y"] =  btn.btn.targetGraphic.rectTransform.GetSize().y / Screen.height / 2f;
                p["z"] = 0;
                p["islocal"] = false;
                p["looptype"] = "loop";
                p["oncomplete"] = "resertITweenPosition";
                p["oncompletetarget"] = this.gameObject;
                p["oncompleteparams"] = new itweenResetParam()
                {
                    target = btn.btn.targetGraphic.gameObject.transform,
                    localPosition = btn.btn.targetGraphic.gameObject.transform.localPosition,
                };
                iTween.ShakePosition(btn.btn.targetGraphic.gameObject, p);
              
            }
        }

        public void StopButtonEffect(ButtonInfo btn)
        {
            if (!btn.play_effect) return;
            btn.play_effect = false;

            if (!btn.btn)
            {
                Log.LogError($"{btn.btnText} cannt to StopButtonEffect,because cannt found mybutton.");
                return;
            }

            if (btn.effect_go)
            {
                btn.effect_go.SetActive(false);
            }

            if (btn.effectType == ButtonEffectType.anim)
            {
                iTween.Stop(btn.btn.targetGraphic.gameObject);
                btn.btn.targetGraphic.gameObject.transform.localPosition = Vector3.zero;
            }
        }

       
#if UNITY_ASYNC
        async void ShowInsertAminAsync(ButtonInfo btnInfo)
#else
        IEnumerator ShowInsertAminAsync(ButtonInfo btnInfo)
#endif

        {
            var oldSize = new Vector2(m_itemWidth, m_itemHeight);// btnInfo.rootRect.rectTransform.GetSize();
            var oldPos = btnInfo.btn.targetGraphic.rectTransform.localPosition;
            var oldcolor = Color.white; //btnInfo.btn.targetGraphic.color;
           // btnInfo.btn.btnText = btnInfo.btnText;            
            btnInfo.isMoving = true;

            btnInfo.rootRect.rectTransform.SetSize(Vector2.zero);

            var glist = GameObjectUtils.GetComponentsEx<Graphic>(btnInfo.rootRect.gameObject);
            foreach (var g in glist)
            {
                g.color = new Color(oldcolor.r, oldcolor.g, oldcolor.b, 0f);
            }             

           Action Leavefun = ()=>
            {               
                btnInfo.rootRect.rectTransform.SetSize(oldSize);
                btnInfo.btn.targetGraphic.rectTransform.localPosition = oldPos;
                foreach (var g in glist)
                {
                    g.color = oldcolor;
                }
                btnInfo.OnInitCallback(btnInfo);               
                btnInfo.isMoving = false;               
            };

#if UNITY_ASYNC
            try
            {
#else
                MyTask.SetLeave(Leavefun);
#endif

                float t = UnityEngine.Time.time;
                while (UnityEngine.Time.time - t < 0.5f)
                {
                    var rate = EaseUtils.GetRate((UnityEngine.Time.time - t), 0.5f, easing.Back.easeInOut);

                    var size = oldSize;
                    if (_isVertical) size = new Vector2(oldSize.x, oldSize.y * rate);
                    else
                        size = new Vector2(oldSize.x * rate, oldSize.y);

                    btnInfo.rootRect.rectTransform.SetSize(size);

#if UNITY_ASYNC
                    await MyTask.WaitForMilliseconds(10);
#else
                    yield return 10;
#endif
                }

                t = UnityEngine.Time.time;
                while (UnityEngine.Time.time - t < 0.5f)
                {
                    var s = (UnityEngine.Time.time - t) / 0.5f;
                    btnInfo.btn.targetGraphic.color = new Color(oldcolor.r, oldcolor.g, oldcolor.b, oldcolor.a * s);

#if UNITY_ASYNC
                    await MyTask.WaitForMilliseconds(10);
#else
                    yield return 10;
#endif
                }
#if UNITY_ASYNC
            }
            catch (System.Exception err)
            {
                Log.LogError("ShowInsertAminAsync error:{0},{1}", err.Message, err.StackTrace);
            }
            finally
            {
                Leavefun();
            }            
#endif
        }

#if UNITY_ASYNC
        async void ShowRemoveAnimAsync(ButtonInfo btnInfo)
#else
        IEnumerator ShowRemoveAnimAsync(ButtonInfo btnInfo)
#endif
        {
            var oldcolor = Color.white;
            var oldSize = new Vector2(m_itemWidth, m_itemHeight);           
            btnInfo.isMoving = true;
            Action Leavefun = () =>
             {
                 btnInfo.isMoving = false;
                 btnInfo.btn.SetParam(null);
                 RemoveChildItem(btnInfo.rootRect.gameObject);
                 _btn_infos.Remove(btnInfo);
                 btnInfo.isRemoved = false;

             };

#if UNITY_ASYNC
            try
            {
#else
                MyTask.SetLeave(Leavefun);
#endif

                float t = UnityEngine.Time.time;
                while (UnityEngine.Time.time - t < 0.5f && btnInfo.btn)
                {
                    var s = (UnityEngine.Time.time - t) / 0.5f;
                    var glist = GameObjectUtils.GetComponentsEx<Graphic>(btnInfo.rootRect.gameObject);
                    foreach (var g in glist)
                    {
                        g.color = new Color(oldcolor.r, oldcolor.g, oldcolor.b, 1 - oldcolor.a * s);
                    }                    

#if UNITY_ASYNC
                    await MyTask.WaitForMilliseconds(10);
#else
                    yield return 10;
#endif
                }
#if UNITY_ASYNC
                if (!btnInfo.btn) return;
#else
                if (!btnInfo.btn) yield break;
#endif
                t = UnityEngine.Time.time;
                while (UnityEngine.Time.time - t < 0.5f)
                {
                    var rate = EaseUtils.GetRate((UnityEngine.Time.time - t), 0.5f, easing.Back.easeInOut);

                    var size = oldSize;
                    if (_isVertical) size = new Vector2(oldSize.x, oldSize.y - oldSize.y * rate);
                    else
                        size = new Vector2(oldSize.x - oldSize.x * rate, oldSize.y);

                    btnInfo.rootRect.rectTransform.SetSize(size);

#if UNITY_ASYNC
                    await MyTask.WaitForMilliseconds(10);
#else
                    yield return 10;
#endif
                }
#if UNITY_ASYNC
            }
            catch (System.Exception err)
            {
                Log.LogError("ShowInsertAminAsync error:{0},{1}", err.Message, err.StackTrace);
            }
            finally
            {
                Leavefun();
            }
#endif

        }


    }
}
