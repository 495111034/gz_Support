#if UNITY_EDITOR || UNITY_ASYNC
#define USING_ASYNC
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI.MyCoroutineTween;

#if USING_ASYNC
using System.Threading;
using System.Threading.Tasks;
#endif

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/My Menu", 36)]
    [RequireComponent(typeof(RectTransform))]
    public class MyMenu : UIBehaviour, ICancelHandler, IPointerLongpressHandler
    {

        PointerEventData _pressingEventData;
        public PointerEventData pressingEventData => _pressingEventData;

        protected internal class MenuItem : MonoBehaviour, IPointerEnterHandler, ICancelHandler
        {
            [SerializeField]
            private MyButton m_button;
            [SerializeField]
            private RectTransform m_RectTransform;


            public RectTransform rectTransform { get { return m_RectTransform; } set { m_RectTransform = value; } }
            public MyButton button { get { return m_button; } set { m_button = value; } }

            public virtual void OnPointerEnter(PointerEventData eventData)
            {
                EventSystem.current.SetSelectedGameObject(gameObject);
            }

            public virtual void OnCancel(BaseEventData eventData)
            {
                MyMenu dropdown = GetComponentInParent<MyMenu>();
                if (dropdown)
                    dropdown.Hide();
            }
        }

        [SerializeField]
        private RectTransform m_Template;
        public RectTransform template { get { return m_Template; } set { m_Template = value; Refresh(); } }

        [SerializeField]
        bool m_useLanguageID = false;

        public bool UseLangugaeID
        {
            get { return m_useLanguageID; }
            set { m_useLanguageID = value; LoadLanguageIDs();  Refresh(); }
        }

        [SerializeField]
        string[] m_languageIDs;

        public List<string> LanguageList
        {
            get { return m_languageIDs != null ? new List<string>(m_languageIDs) : new List<string>(); }
            set { m_useLanguageID = true; m_languageIDs = value.ToArray(); LoadLanguageIDs();  Refresh(); }
        }

        // Items that will be visible when the dropdown is shown.
        // We box this into its own class so we can use a Property Drawer for it.
        [SerializeField]
        private string[] m_Options;
        public List<string> options
        {
            get { return m_Options != null ? new List<string>(m_Options) : new List<string>(); }
            set
            {
                if (Application.isPlaying)
                {
                    m_useLanguageID = false; m_languageIDs = null;
                }
                m_Options = value.ToArray(); Refresh();
            }
        }



        [SerializeField]
        float m_space = 0f;
        public float Space { get { return m_space; } set { m_space = value; } }

        [SerializeField]
        bool m_resizeAsContent = false;
        public bool ResizeAsContent { get { return m_resizeAsContent; } set { m_resizeAsContent = value; } }

        [SerializeField]
        float m_limitSize = 0f;
        public float LimitSize { get { return m_limitSize; }set { m_limitSize = value; } }


        private int _value = 0;
        public int Value { get { return _value; } set { _value = value; } }

        public MyMenu() { }


        public Action<int> OnMenuSelect;
        public Action<int> OnMenuLongPress;

        void Refresh()
        {
        }

        public void LoadLanguageIDs()
        {
            if (m_useLanguageID)
            {
                if (m_languageIDs == null || m_languageIDs.Length == 0)
                {
                    m_Options = new string[0];
                }
                else
                {
                    if (m_Options == null || m_Options.Length != m_languageIDs.Length)
                    {
                        m_Options = new string[m_languageIDs.Length];
                    }
                    for (int i = 0, len = m_languageIDs.Length; i < len; ++i)
                    {
                        m_Options[i] = MyUITools.UIResPoolInstans.LangFromId(m_languageIDs[i], null);
                    }
                }
            }
        }


        public bool isShow
        {
            get
            {
                return m_Dropdown;
            }
        }

        private GameObject m_Dropdown;
        private GameObject m_Blocker;
        private List<MenuItem> m_Items = new List<MenuItem>();
        private TweenRunner<FloatTween> m_AlphaTweenRunner;
        private bool validTemplate = false;

        protected override void Awake()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif

            m_AlphaTweenRunner = new TweenRunner<FloatTween>();
            m_AlphaTweenRunner.Init(this);

            if (m_Template)
                m_Template.gameObject.SetActive(false);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (!IsActive())
                return;

            Refresh();
        }
#endif
        public void Hide()
        {
            if (m_Dropdown != null)
            {
                AlphaFadeList(0.15f, 0f);
                StartCoroutine(DelayedDestroyDropdownList(0.15f));
            }
            if (m_Blocker)
                Destroy(m_Blocker);
            m_Blocker = null;
        }

        private IEnumerator DelayedDestroyDropdownList(float delay)
        {
            yield return new WaitForSeconds(delay);

            m_Items.Clear();

            if (m_Dropdown)
                Destroy(m_Dropdown);
            m_Dropdown = null;
        }

        private void AlphaFadeList(float duration, float alpha)
        {
            CanvasGroup group = m_Dropdown.GetComponent<CanvasGroup>();
            AlphaFadeList(duration, group.alpha, alpha);
        }

        private void AlphaFadeList(float duration, float start, float end)
        {
            if (end.Equals(start))
                return;

            FloatTween tween = new FloatTween { duration = duration, startValue = start, targetValue = end };
            tween.AddOnChangedCallback(SetAlpha);
            tween.ignoreTimeScale = true;
            m_AlphaTweenRunner.StartTween(tween);
        }

        private void SetAlpha(float alpha)
        {
            if (!m_Dropdown)
                return;
            CanvasGroup group = m_Dropdown.GetComponent<CanvasGroup>();
            group.alpha = alpha;
        }

        public void Show(RectTransform targetUIPos, List<object> optionList = null, Action<Transform, int> SetItemCallback = null)
        {
            var canvas = GameObjectUtils.FindInParents<Canvas>(targetUIPos);
            if (!canvas || !canvas.worldCamera)
            {
                Show(Vector2.zero);
                return;
            }

            var pos = canvas.worldCamera.WorldToScreenPoint(targetUIPos.position);
            Show(pos, optionList, SetItemCallback);
        }

        public void Show(Vector2 pos, List<object> optionList = null, Action<Transform, int> SetItemCallback = null)
        {
            if (!IsActive() || m_Dropdown != null)
                return;


            if (!validTemplate)
            {
                SetupTemplate();
                if (!validTemplate)
                    return;
            }

            if (optionList != null)
            {
                m_Options = new string[optionList.Count];
            }

            // Get root Canvas.
            var list = MyListPool<Canvas>.Get();
            gameObject.GetComponentsInParent(false, list);
            if (list.Count == 0)
                return;
            Canvas rootCanvas = list[0];
            MyListPool<Canvas>.Release(list);

            m_Template.gameObject.SetActive(true);

            var tmpRect = m_Template.GetComponentInChildren<MyButton>().gameObject.GetComponent<RectTransform>();

            tmpRect.pivot = new Vector2(tmpRect.pivot.x, 1);

            m_Dropdown = Instantiate(m_Template.gameObject);
            m_Dropdown.name = "Menu List";
            m_Dropdown.SetActive(true);
            m_Dropdown.transform.SetParent(rootCanvas.transform, false);
            m_Dropdown.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
            m_Dropdown.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);

            var scrollRect = m_Dropdown.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.viewport = GameObjectUtils.FindInChild<Mask>(m_Dropdown).GetComponent<RectTransform>();
            scrollRect.viewport.pivot = new Vector2(scrollRect.viewport.pivot.x, 1);




            MenuItem itemTemplate = m_Dropdown.GetComponentInChildren<MenuItem>();

            GameObject content = itemTemplate.rectTransform.parent.gameObject;
            RectTransform contentRectTransform = content.transform as RectTransform;
            itemTemplate.rectTransform.gameObject.SetActive(true);

            contentRectTransform.pivot = new Vector2(contentRectTransform.pivot.x, 1);


            scrollRect.content = contentRectTransform;
            var mylayout = content.AddMissingComponent<MyVerticalLayoutGroup>();
            mylayout.spacing = m_space;

            var fitterSize = content.AddComponent<MyContentSizeFitter>();
            fitterSize.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitterSize.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            m_Items.Clear();
            mylayout.InitChildren(m_Options);
            for (int i = 0; i < mylayout.Children.Count; ++i)
            {
                var item = mylayout.Children[i].GetComponent<MenuItem>();
                item.gameObject.SetActive(true);
                item.gameObject.name = "Item " + m_Items.Count;
                var element = item.gameObject.AddComponent<LayoutElement>();
                element.preferredWidth = tmpRect.GetSize().x;
                element.preferredHeight = tmpRect.GetSize().y;


                if (item.button != null)
                {
                    item.button.menu = this;
                    item.button.butonBar = null;

                    item.button.btnText = m_Options[i];
                }
                SetItemCallback?.Invoke(item.gameObject.transform, i);
                m_Items.Add(item);
            }


            // Fade in the popup
            AlphaFadeList(0.15f, 0f, 1f);

            // Make drop-down template and item template inactive
            m_Template.gameObject.SetActive(false);
            // itemTemplate.gameObject.SetActive(false);

            m_Blocker = CreateBlocker(rootCanvas);

            var selectedItem = _value >= 0 && _value < m_Items.Count ? m_Items[_value].gameObject.GetComponent<RectTransform>() : null;
#if USING_ASYNC
            _setPos(scrollRect, selectedItem, pos, rootCanvas);
#else
            StartCoroutine(_setPos(scrollRect, selectedItem,pos,rootCanvas));
#endif
        }

#if USING_ASYNC
        async void
#else
        IEnumerator
#endif
             _setPos(ScrollRect scrollRect, RectTransform child, Vector2 pos, Canvas rootCanvas)
        {
            var rootCanvasRectTransform = rootCanvas.transform as RectTransform;
            var dropdownRectTransform = scrollRect.gameObject.GetRectTransform();

#if USING_ASYNC
            await System.Threading.Tasks.Task.Delay(10);
#else
            yield return null;
#endif
            if (m_resizeAsContent)
            {
                var newsize = scrollRect.content.GetSize() - scrollRect.viewport.sizeDelta;
                if (m_limitSize > 1f && newsize.y > m_limitSize)
                    newsize.y = m_limitSize;
                dropdownRectTransform.SetSize(newsize);
            }
            else
            {
                dropdownRectTransform.SetSize(m_Template.GetSize());
            }

            var size = dropdownRectTransform.GetSize();
            var rat = (float)Screen.height / (float)MyUITools.RefScreenHeight;
            size *= rat;

            if (pos.y < size.y) pos.y = size.y;
            if (pos.y > Screen.height) pos.y = Screen.height;
            if (pos.x < size.x / 2) pos.x = size.x / 2;
            if (pos.x + size.x / 2 > Screen.width) pos.x = Screen.width - size.x / 2f;


            Vector2 localPos;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rootCanvas.transform as RectTransform, pos, rootCanvas.worldCamera, out localPos))
            {
                // Log.LogError($"pos={pos},localPos={localPos},size={size},Screen.height={Screen.height},Screen.width={Screen.width}");
                dropdownRectTransform.anchoredPosition = localPos;
            }
            else
            {
                dropdownRectTransform.anchoredPosition = new Vector2(pos.x - Screen.width / 2, -(Screen.height / 2 - pos.y));
            }


#if USING_ASYNC
            await System.Threading.Tasks.Task.Delay(10);
#else
            yield return null;
#endif

            if (scrollRect && child)
            {
                if (child && (-child.localPosition.y + child.GetSize().y) > scrollRect.viewport.GetSize().y)
                {
                    var lpos = scrollRect.content.localPosition;
                    lpos.y = (-child.localPosition.y) - scrollRect.viewport.GetSize().y + child.GetSize().y;
                    scrollRect.content.localPosition = lpos;
                }
            }
        }

        protected virtual GameObject CreateBlocker(Canvas rootCanvas)
        {
            // Create blocker GameObject.
            GameObject blocker = new GameObject("Blocker");

            // Setup blocker RectTransform to cover entire root canvas area.
            RectTransform blockerRect = blocker.AddComponent<RectTransform>();
            blockerRect.SetParent(rootCanvas.transform, false);
            blockerRect.anchorMin = Vector3.zero;
            blockerRect.anchorMax = Vector3.one;
            blockerRect.sizeDelta = Vector2.zero;

            // Make blocker be in separate canvas in same layer as dropdown and in layer just below it.
            Canvas blockerCanvas = blocker.AddComponent<Canvas>();
            blockerCanvas.overrideSorting = true;
            Canvas dropdownCanvas = m_Dropdown.GetComponent<Canvas>();
            blockerCanvas.sortingLayerID = dropdownCanvas.sortingLayerID;
            blockerCanvas.sortingOrder = dropdownCanvas.sortingOrder - 1;

            // Add raycaster since it's needed to block.
            blocker.AddComponent<GraphicRaycaster>();

            // Add image since it's needed to block, but make it clear.
            Image blockerImage = blocker.AddComponent<Image>();
            blockerImage.color = Color.clear;

            // Add button since it's needed to block, and to close the dropdown when blocking area is clicked.
            Button blockerButton = blocker.AddComponent<Button>();
            blockerButton.onClick.AddListener(Hide);

            return blocker;
        }


        private void SetupTemplate()
        {
            validTemplate = false;

            if (!m_Template)
            {
                Log.LogError($"{gameObject.name}:此菜单没有模板");
                return;
            }

            GameObject templateGo = m_Template.gameObject;
            templateGo.SetActive(true);
            MyButton itemBtn = m_Template.GetComponentInChildren<MyButton>();

            validTemplate = true;
            if (!itemBtn || itemBtn.transform == template)
            {
                validTemplate = false;
                Log.LogError($"{gameObject.name}:模板必须拥有按钮作为孩子");
            }
            else if (!(itemBtn.transform.parent is RectTransform))
            {
                validTemplate = false;
                Log.LogError($"{gameObject.name}:按钮的上级必须是RectTransform");
            }


            if (!validTemplate)
            {
                templateGo.SetActive(false);
                return;
            }
            itemBtn.menu = this;
            itemBtn.butonBar = null;

            var item = itemBtn.gameObject.AddComponent<MenuItem>();
            item.button = itemBtn;
            item.rectTransform = (RectTransform)itemBtn.transform;

            Canvas popupCanvas = templateGo.AddMissingComponent<Canvas>();
            popupCanvas.overrideSorting = true;
            popupCanvas.sortingOrder = 30000;

            templateGo.AddMissingComponent<GraphicRaycaster>();
            templateGo.AddMissingComponent<CanvasGroup>();
            templateGo.SetActive(false);

            validTemplate = true;
        }

        public void OnBtnClick(MyButton clickBtn)
        {
            var selectedIndex = -1;
            for (int i = 0; i < m_Items.Count; ++i)
            {
                if (m_Items[i].button == clickBtn)
                {
                    selectedIndex = i;
                    break;
                }
            }
            _value = selectedIndex;
            Hide();
            OnMenuSelect?.Invoke(_value);
            SendMessageUpwards("__OnGuidePointerDown", this);
            SendMessageUpwards("OnClickEvent", this);

        }

        public void OnBtnLongPress(MyButton longPressBtn)
        {
            var selectedIndex = -1;
            for (int i = 0; i < m_Items.Count; ++i)
            {
                if (m_Items[i].button == longPressBtn)
                {
                    selectedIndex = i;
                    break;
                }
            }
            _value = selectedIndex;
            Hide();
            OnMenuLongPress?.Invoke(_value);

            _pressingEventData = longPressBtn.pressingEventData;
            SendMessageUpwards("__OnLongPress", this);
            _pressingEventData = null;
        }
        public void OnLongPressRepeat(PointerEventData eventData)
        {
        }

        public void OnCancel(BaseEventData eventData)
        {
            throw new NotImplementedException();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }
    }
}
