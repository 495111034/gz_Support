using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;


namespace UnityEngine.UI
{
    [AddComponentMenu("UI/MyToggle", 31)]
    [RequireComponent(typeof(RectTransform))]
    public class MyToggle : Selectable, EventSystems.IPointerClickHandler, ISubmitHandler, ICanvasElement, IUITips, IPointerLongpressHandler
    {
        PointerEventData _pressingEventData;
        public PointerEventData pressingEventData => _pressingEventData;

        public enum ToggleTransition
        {
            None,
            Fade
        }

        [Serializable]
        public class ToggleEvent : UnityEvent<bool>
        { }

        /// <summary>
        /// Transition type.
        /// </summary>
        public ToggleTransition toggleTransition = ToggleTransition.Fade;

        /// <summary>
        /// Graphic the toggle should be working with.
        /// </summary>
        [SerializeField]
        public GameObject graphic;
        [SerializeField]
        public GameObject falseGraphic;

        [SerializeField]
        bool animChange = false;

        [SerializeField]
        float animCoefficient = 0.01f;

        // group that this toggle can belong to
        [SerializeField]
        private MyToggleGroup m_Group;

        public MyToggleGroup group
        {
            get { return m_Group; }
            set
            {
                m_Group = value;
#if UNITY_EDITOR
                if (Application.isPlaying)
#endif
                {
                    SetToggleGroup(m_Group, true);
                    PlayEffect(true);
                }
            }
        }

        private MyToggleBar m_togglebar;

        public MyToggleBar Toggleber
        {
            get
            {
                return m_togglebar;
            }

            set
            {
                m_togglebar = value;
            }
        }
         

        [HideInInspector]
        [SerializeField]
        protected string soundOnSelectOn = "";

        /// <summary>
        /// 选中时的声音
        /// </summary>
        public string SoundOnSelectOn
        {
            get { return soundOnSelectOn; }
            set { soundOnSelectOn = value; }
        }

        public bool AnimChange
        {
            get
            {
                return animChange;
            }
            set
            {
                animChange = value;
            }
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

        public Toggle.ToggleEvent onValueChangedEvent;
        private float _last_true = 0f;
        /// <summary>
        /// value改变时向上抛出事件
        /// </summary>
        /// <param name="value"></param>
        private void onValueChanged(bool value)
        {
            if (Application.isPlaying)
            {
                if (animChange && value)
                {                    
                     iTween.PunchScale(gameObject, Vector3.one * animCoefficient, 0.5f);
                }

                if (m_togglebar)
                {
                    m_togglebar.OnChange(this);
                }
                else
                {
                    SendMessageUpwards("__onValueChange", this);
                    onValueChangedEvent?.Invoke(this.isOn);
                }
               
      
                if(!string.IsNullOrEmpty(soundOnSelectOn) && isOn)
                {
                    SendMessageUpwards("__OnPlaySound", soundOnSelectOn);
                }
            }
        }

        public void OnLongPressRepeat(PointerEventData eventData)
        {
            if (Application.isPlaying)
            {
                _pressingEventData = eventData;
                SendMessageUpwards("__OnLongPress", this);
                _pressingEventData = null;
            }
        }



        // Whether the toggle is on
        [FormerlySerializedAs("m_IsActive")]
        [Tooltip("Is the toggle currently on or off?")]
        [SerializeField]
        private bool m_IsOn;

        protected MyToggle()
        { }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            SetValue(m_IsOn, false);
            PlayEffect(toggleTransition == ToggleTransition.None);

            var prefabType = UnityEditor.PrefabUtility.GetPrefabType(this);
            if (prefabType != UnityEditor.PrefabType.Prefab && !Application.isPlaying)
                CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
        }

#endif // if UNITY_EDITOR

        public virtual void Rebuild(CanvasUpdate executing)
        {
#if UNITY_EDITOR
            if (executing == CanvasUpdate.Prelayout)
                onValueChanged(m_IsOn);
#endif
        }

        public virtual void LayoutComplete()
        { }

        public virtual void GraphicUpdateComplete()
        { }

        protected override void OnEnable()
        {
            if (BuilderConfig.IsDebugBuild) Profiling.Profiler.BeginSample($"toggle {gameObject.name} to OnEnable");
            _last_true = Time.time;
            {
                if (BuilderConfig.IsDebugBuild) Profiling.Profiler.BeginSample("base OnEnable");
                base.OnEnable();
                if (BuilderConfig.IsDebugBuild) Profiling.Profiler.EndSample();
            }
            {
                if (BuilderConfig.IsDebugBuild) Profiling.Profiler.BeginSample("SetToggleGroup");
                SetToggleGroup(m_Group, false);
                if (BuilderConfig.IsDebugBuild) Profiling.Profiler.EndSample();
            }
            {
                if (BuilderConfig.IsDebugBuild) Profiling.Profiler.BeginSample("PlayEffect");
                PlayEffect(false);
                if (BuilderConfig.IsDebugBuild) Profiling.Profiler.EndSample();
            }
            if (BuilderConfig.IsDebugBuild) Profiling.Profiler.EndSample();
        }

        protected override void Awake()
        {
            _last_true = Time.time;
            base.Awake();
            PlayEffect(false);
        }

        protected override void OnDisable()
        {
            SetToggleGroup(null, false);
            base.OnDisable();
        }

        protected override void OnDidApplyAnimationProperties()
        {
            // isOn是否被动画修改
            // 但是检测不了图形
            if (graphic)
            {
                bool oldValue = !Mathf.Approximately(graphic.GetComponentInChildren<Graphic>().canvasRenderer.GetColor().a, 0);
                if (m_IsOn != oldValue)
                {
                    m_IsOn = oldValue;
                    Set(!oldValue);
                }
            }

            base.OnDidApplyAnimationProperties();
        }

        private void SetToggleGroup(MyToggleGroup newGroup, bool setMemberValue)
        {
            MyToggleGroup oldGroup = m_Group;

            // Sometimes IsActive returns false in OnDisable so don't check for it.
            // Rather remove the toggle too often than too little.
            if (m_Group != null)
                m_Group.UnregisterToggle(this);

            // At runtime the group variable should be set but not when calling this method from OnEnable or OnDisable.
            // That's why we use the setMemberValue parameter.
            if (setMemberValue)
                m_Group = newGroup;

            // Only register to the new group if this Toggle is active.
            if (m_Group != null && IsActive())
                m_Group.RegisterToggle(this);

            // If we are in a new group, and this toggle is on, notify group.
            // Note: Don't refer to m_Group here as it's not guaranteed to have been set.
            if (newGroup != null && newGroup != oldGroup && isOn && IsActive())
                m_Group.NotifyToggleOn(this);
        }

        /// <summary>
        /// 当前值
        /// </summary>
        public bool isOn
        {
            get { return m_IsOn; }
            set
            {
                Set(value);
            }
        }

        void Set(bool value)
        {
            SetValue(value, true);
        }

        public void SetValue(bool value, bool sendCallback)
        {
            if (m_IsOn == value)
            {
                PlayEffect(toggleTransition == ToggleTransition.None);
                return;
            }

            // if we are in a group and set to true, do group logic
            m_IsOn = value;
            if (m_Group != null && IsActive())
            {
                if (m_IsOn || (!m_Group.AnyTogglesOn() && !m_Group.allowSwitchOff))
                {
                    m_IsOn = true;
                    m_Group.NotifyToggleOn(this);
                }
            }

            // Always send event when toggle is clicked, even if value didn't change
            // due to already active toggle in a toggle group being clicked.
            // Controls like Dropdown rely on this.
            // It's up to the user to ignore a selection being set to the same value it already was, if desired.
            PlayEffect(toggleTransition == ToggleTransition.None);
            if (sendCallback)
                onValueChanged(m_IsOn);
        }

        /// <summary>
        /// Play the appropriate effect.
        /// </summary>
        private void PlayEffect(bool instant)
        {
            SetGraphicAlpha(instant);
        }

        private void SetGraphicAlpha(bool instant)
        {
            if (graphic )
            {
                var glist = MyListPool<Graphic>.Get();
                graphic.gameObject.GetComponentsEx(glist);
                for(int i = 0; i < glist.Count; ++i)
                {
                    var g = glist[i];
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                        g.canvasRenderer.SetAlpha(m_IsOn ? 1f : 0f);
                    else
                    {
#endif
                        if (toggleTransition == ToggleTransition.None) g.enabled = m_IsOn;
                        else g.CrossFadeAlpha(m_IsOn ? 1f : 0f, instant ? 0f : 0.1f, true);
#if UNITY_EDITOR
                    }
#endif
                }

                MyListPool<Graphic>.Release(glist);
            }

            if(falseGraphic)
            {
                var glist = MyListPool<Graphic>.Get();
                falseGraphic.gameObject.GetComponentsEx(glist);
                for (int i = 0; i < glist.Count; ++i)
                {
                    var g = glist[i];
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                        g.canvasRenderer.SetAlpha(m_IsOn ? 0f : 1f);
                    else
                    {
#endif
                        if (toggleTransition == ToggleTransition.None) g.enabled = !m_IsOn;
                        else g.CrossFadeAlpha(m_IsOn ? 0f : 1f, instant ? 0f : 0.1f, true);
#if UNITY_EDITOR
                    }
#endif
                }
                MyListPool<Graphic>.Release(glist);
            }
        }

        /// <summary>
        /// Assume the correct visual state.
        /// </summary>
        protected override void Start()
        {
            PlayEffect(true);
        }

        

        private void InternalToggle()
        {
            if (!IsActive() || !IsInteractable())
                return;

            isOn = !isOn;
        }

        /// <summary>
        /// React to clicks.
        /// </summary>
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            InternalToggle();
            if (Application.isPlaying)
            {
                SendMessageUpwards("__OnGuidePointerDown", this);
                SendMessageUpwards("OnClickEvent", this);
            }
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {
            InternalToggle();
        }

        string _btnText = "";
        public string buttonText
        {
            get
            {
               
                return _btnText;
            }
            set
            {
                _btnText = value;
                if(falseGraphic)
                {
                    var lbls = falseGraphic.gameObject.FindInChild<MyText>();
                    if (lbls) lbls.text = _btnText;                   
                }
                if(graphic)
                {
                    var lbls = graphic.gameObject.FindInChild<MyText>();
                    if (lbls) lbls.text = _btnText;
                }                
            }
        }



        #region tips object
        [HideInInspector]
        [SerializeField]
        string stringTips = "";
        object tipsObj = null;
        public object TipParams { get { return string.IsNullOrEmpty(stringTips) ? tipsObj : stringTips; } set { tipsObj = value; stringTips = ""; } }
        #endregion
    }
}
