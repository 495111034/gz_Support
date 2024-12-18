using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI.MyCoroutineTween;
#if UNITY_ASYNC
using System.Threading;
using System.Threading.Tasks;
#endif

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/MyDropdown", 35)]
    [RequireComponent(typeof(RectTransform))]
    public class MyDropdown : Selectable, EventSystems.IPointerClickHandler, ISubmitHandler, ICancelHandler
    {
        public class DropdownItem : MonoBehaviour, IPointerEnterHandler, ICancelHandler
        {
            [SerializeField]
            private MyImageText m_Text;          
            [SerializeField]
            private RectTransform m_RectTransform;
            [SerializeField]
            private MyToggle m_Toggle;

            public MyImageText text { get { return m_Text; } set { m_Text = value; } }           
            public RectTransform rectTransform { get { return m_RectTransform; } set { m_RectTransform = value; } }
            public MyToggle toggle { get { return m_Toggle; } set { m_Toggle = value; } }

            public virtual void OnPointerEnter(PointerEventData eventData)
            {
                EventSystem.current.SetSelectedGameObject(gameObject);
            }

            public virtual void OnCancel(BaseEventData eventData)
            {
                MyDropdown dropdown = GetComponentInParent<MyDropdown>();
                if (dropdown)
                    dropdown.Hide();
            }
        }


        // Template used to create the dropdown.
        [SerializeField]
        private RectTransform m_Template;
        public RectTransform template { get { return m_Template; } set { m_Template = value; Refresh(); } }

        // Text to be used as a caption for the current value. It's not required, but it's kept here for convenience.
        [SerializeField]
        private MyImageText m_CaptionText;
        public MyImageText captionText { get { return m_CaptionText; } set { m_CaptionText = value; Refresh(); } }
      

        [Space]

        [SerializeField]
        private MyImageText m_ItemText;
        public MyImageText itemText { get { return m_ItemText; } set { m_ItemText = value; Refresh(); } }
   

        [Space]

        [SerializeField]
        private int m_Value;

        [Space]

        [SerializeField]
        bool m_useLanguageID = false;

        public bool UseLangugaeID
        {
            get { return m_useLanguageID; }
            set { m_useLanguageID = value; LoadLanguageIDs(); Refresh(); }
        }

        [SerializeField]
        string[] m_languageIDs;

        public List<string> LanguageList
        {
            get { return m_languageIDs != null ? new List<string>(m_languageIDs) : new List<string> (); }
            set { m_useLanguageID = true; m_languageIDs = value.ToArray(); LoadLanguageIDs();  Refresh(); }
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
                        if (Application.isPlaying)
                        {
                            m_Options[i] = MyUITools.UIResPoolInstans.LangFromId(m_languageIDs[i], null);
                        }
                        else 
                        {
                            m_Options[i] = m_languageIDs[i];
                        }
                    }
                }
            }
        }

        // Items that will be visible when the dropdown is shown.
        // We box this into its own class so we can use a Property Drawer for it.
        [SerializeField]
        private string[] m_Options;
        public List<string> options
        {
            get { return m_Options != null? new List<string>(m_Options): new List<string>(); }
            set
            {
                if (Application.isPlaying)
                {
                    m_useLanguageID = false; m_languageIDs = null;
                }
                m_Options = value.ToArray(); Refresh(); }
        }


        [SerializeField]
        float m_space = 0f;
        public float Space { get { return m_space; } set { m_space = value; } }


        private void onValueChanged(int value)
        {
            if (Application.isPlaying)
            {
                SendMessageUpwards("__onValueChange", this);
            }
        }

        private GameObject m_Dropdown;
        private GameObject m_Blocker;
        private List<DropdownItem> m_Items = new List<DropdownItem>();
        private TweenRunner<FloatTween> m_AlphaTweenRunner;
        private bool validTemplate = false;

        // Current value.
        public int value
        {
            get
            {
                return m_Value;
            }
            set
            {
                if (m_Options == null || m_Options.Length == 0)
                {
                    return; 
                }

                int curr = Mathf.Clamp(value, 0, m_Options.Length - 1);
                if (m_Value == curr) 
                {
                    return;
                }
                m_Value = curr;

                Refresh();

                // Notify all listeners
                onValueChanged(m_Value);
            }
        }

        public List<DropdownItem> Items { get => m_Items; set => m_Items = value; }

        public Action<int> OnMenuSelect;
        public Action OnPopupEnd;

        protected MyDropdown()
        { }

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

        void Refresh()
        {
            if (!m_CaptionText)
            {
                return;
            }
            //
            if (m_Options == null || m_Options.Length == 0)
            {
                return;
            }
            //
            m_CaptionText.text = m_Options[m_Value];
        }

        private void SetupTemplate()
        {
            validTemplate = false;

            if (!m_Template)
            {
                Debug.LogError("The dropdown template is not assigned. The template needs to be assigned and must have a child GameObject with a Toggle component serving as the item.", this);
                return;
            }

            GameObject templateGo = m_Template.gameObject;
            templateGo.SetActive(true);
            MyToggle itemToggle = m_Template.GetComponentInChildren<MyToggle>();

            validTemplate = true;
            if (!itemToggle || itemToggle.transform == template)
            {
                validTemplate = false;
                Debug.LogError("The dropdown template is not valid. The template must have a child GameObject with a MyToggle component serving as the item.", template);
            }
            else if (!(itemToggle.transform.parent is RectTransform))
            {
                validTemplate = false;
                Debug.LogError("The dropdown template is not valid. The child GameObject with a Toggle component (the item) must have a RectTransform on its parent.", template);
            }
            else if (itemText != null && !itemText.transform.IsChildOf(itemToggle.transform))
            {
                validTemplate = false;
                Debug.LogError("The dropdown template is not valid. The Item Text must be on the item GameObject or children of it.", template);
            }
          

            if (!validTemplate)
            {
                templateGo.SetActive(false);
                return;
            }

            DropdownItem item = itemToggle.gameObject.AddComponent<DropdownItem>();
            item.text = m_ItemText;           
            item.toggle = itemToggle;
            item.rectTransform = (RectTransform)itemToggle.transform;

            Canvas popupCanvas = templateGo.AddMissingComponent<Canvas>();
            popupCanvas.overrideSorting = true;
            popupCanvas.sortingOrder = 30000;

            templateGo.AddMissingComponent<GraphicRaycaster>();
            templateGo.AddMissingComponent<CanvasGroup>();
            templateGo.SetActive(false);

            validTemplate = true;
        }


        public virtual void OnPointerClick(PointerEventData eventData)
        {
            Show();
        }

        public virtual void OnSubmit(BaseEventData eventData)
        {
            Show();
        }

        public virtual void OnCancel(BaseEventData eventData)
        {
            Hide();
        }

        // Show the dropdown.
        //
        // Plan for dropdown scrolling to ensure dropdown is contained within screen.
        //
        // We assume the Canvas is the screen that the dropdown must be kept inside.
        // This is always valid for screen space canvas modes.
        // For world space canvases we don't know how it's used, but it could be e.g. for an in-game monitor.
        // We consider it a fair constraint that the canvas must be big enough to contains dropdowns.
        public void Show()
        {
            if (!IsActive() || !IsInteractable() || m_Dropdown != null)
                return;

            if (!validTemplate)
            {
                SetupTemplate();
                if (!validTemplate)
                    return;
            }

            // Get root Canvas.
            var list = MyListPool<Canvas>.Get();
            gameObject.GetComponentsInParent(false, list);
            if (list.Count == 0)
                return;
            Canvas rootCanvas = list[0];
            MyListPool<Canvas>.Release(list);

            m_Template.gameObject.SetActive(true);

            var tmpRect = m_Template.GetComponentInChildren<MyToggle>().gameObject.GetComponent<RectTransform>();

            tmpRect.pivot = new Vector2(tmpRect.pivot.x, 1);

            // Instantiate the drop-down template
            m_Dropdown = CreateDropdownList(m_Template.gameObject);
            m_Dropdown.name = "Dropdown List";
            m_Dropdown.SetActive(true);

            var scrollRect = m_Dropdown.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            scrollRect.viewport = GameObjectUtils.FindInChild<Mask>(m_Dropdown).GetComponent<RectTransform>();
            scrollRect.viewport.pivot = new Vector2(scrollRect.viewport.pivot.x, 1);


            // Make drop-down RectTransform have same values as original.
            RectTransform dropdownRectTransform = m_Dropdown.transform as RectTransform;
            dropdownRectTransform.SetParent(m_Template.transform.parent, false);

            // Instantiate the drop-down list items

            // Find the dropdown item and disable it.
            DropdownItem itemTemplate = m_Dropdown.GetComponentInChildren<DropdownItem>();

            GameObject content = itemTemplate.rectTransform.parent.gameObject;
            RectTransform contentRectTransform = content.transform as RectTransform;
            itemTemplate.rectTransform.gameObject.SetActive(true);

            contentRectTransform.pivot = new Vector2(contentRectTransform.pivot.x, 1);

            // Get the rects of the dropdown and item
            Rect dropdownContentRect = contentRectTransform.rect;
            Rect itemTemplateRect = itemTemplate.rectTransform.rect;

            scrollRect.content = contentRectTransform;
            var mylayout = content.AddMissingComponent<MyVerticalLayoutGroup>();
            mylayout.spacing = m_space;

            var fitterSize = content.AddComponent<MyContentSizeFitter>();
            fitterSize.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitterSize.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
        
            m_Items.Clear();
            mylayout.InitChildren(options);
            for(int i = 0; i < mylayout.Children.Count; ++i)
            {
                var item = mylayout.Children[i].GetComponent< DropdownItem>();
                item.gameObject.SetActive(true);
                item.gameObject.name = "Item " + m_Items.Count ;
                var element = item.gameObject.AddComponent<LayoutElement>();
                element.preferredWidth = tmpRect.GetSize().x;
                element.preferredHeight = tmpRect.GetSize().y;



                if (item.toggle != null)
                {
                   // item.toggle.group = toggGroup;
                    item.toggle.isOn = value == i;
                    item.toggle.onValueChangedEvent.AddListener(x => OnSelectItem(item.toggle));
                }

                // Set the item's data
                if (item.text)
                    item.text.text = m_Options[i];

                m_Items.Add(item);
            }
            OnPopupEnd?.Invoke();
            dropdownRectTransform.SetSize(m_Template.GetSize());

            Vector3[] corners = new Vector3[4];
            dropdownRectTransform.GetWorldCorners(corners);
            bool outside = false;
            RectTransform rootCanvasRectTransform = rootCanvas.transform as RectTransform;
            for (int i = 0; i < 4; i++)
            {
                Vector3 corner = rootCanvasRectTransform.InverseTransformPoint(corners[i]);
                if (!rootCanvasRectTransform.rect.Contains(corner))
                {
                    outside = true;
                    break;
                }
            }
            if (outside)
            {
                RectTransformUtility.FlipLayoutOnAxis(dropdownRectTransform, 0, false, false);
                RectTransformUtility.FlipLayoutOnAxis(dropdownRectTransform, 1, false, false);
            }
         

            // Fade in the popup
            AlphaFadeList(0.15f, 0f, 1f);

            // Make drop-down template and item template inactive
            m_Template.gameObject.SetActive(false);
           // itemTemplate.gameObject.SetActive(false);

            m_Blocker = CreateBlocker(rootCanvas);

            var selectedItem = value >= 0 && value < m_Items.Count ? m_Items[value].gameObject.GetComponent<RectTransform>() : null;
#if UNITY_EDITOR || UNITY_ASYNC
            _setPos(scrollRect, selectedItem);
#else
            StartCoroutine(_setPos(scrollRect, selectedItem));
#endif
        }
#if UNITY_EDITOR || UNITY_ASYNC
        async void _setPos(ScrollRect scrollRect,RectTransform child)
        {
            await System.Threading.Tasks.Task.Delay(1000);
            if (!scrollRect || !child) return;
            if (child && (-child.localPosition.y + child.GetSize().y) > scrollRect.viewport.GetSize().y)
            {
                var lpos = scrollRect.content.localPosition;
                lpos.y = (-child.localPosition.y) - scrollRect.viewport.GetSize().y + child.GetSize().y;
                scrollRect.content.localPosition = lpos;
            }
        }
#else
        IEnumerator _setPos(ScrollRect scrollRect, RectTransform child)
        {
            yield return null;
            if (!scrollRect || !child) yield break;
            if (child && (-child.localPosition.y + child.GetSize().y) > scrollRect.viewport.GetSize().y)
            {
                var lpos = scrollRect.content.localPosition;
                lpos.y = (-child.localPosition.y) - scrollRect.viewport.GetSize().y + child.GetSize().y;
                scrollRect.content.localPosition = lpos;
            }
        }
#endif

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

        protected virtual void DestroyBlocker(GameObject blocker)
        {
            Destroy(blocker);
        }

        protected virtual GameObject CreateDropdownList(GameObject template)
        {
            return (GameObject)Instantiate(template);
        }

        protected virtual void DestroyDropdownList(GameObject dropdownList)
        {
            Destroy(dropdownList);
        }


        protected virtual void DestroyItem(DropdownItem item)
        {
            // No action needed since destroying the dropdown list destroys all contained items as well.
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

        // Hide the dropdown.
        public void Hide()
        {
            if (m_Dropdown != null)
            {
                AlphaFadeList(0.15f, 0f);
                StartCoroutine(DelayedDestroyDropdownList(0.15f));
            }
            if (m_Blocker != null)
                DestroyBlocker(m_Blocker);
            m_Blocker = null;
            Select();
        }

        private IEnumerator DelayedDestroyDropdownList(float delay)
        {
            yield return new WaitForSeconds(delay);
            for (int i = 0; i < m_Items.Count; i++)
            {
                if (m_Items[i] != null)
                    DestroyItem(m_Items[i]);
                m_Items.Clear();
            }
            if (m_Dropdown != null)
                DestroyDropdownList(m_Dropdown);
            m_Dropdown = null;
        }

        // Change the value and hide the dropdown.
        private void OnSelectItem(MyToggle toggle)
        {
            if (!toggle.isOn)
                toggle.isOn = true;

            int selectedIndex = -1;
            Transform tr = toggle.transform;
            Transform parent = tr.parent;
            for (int i = 0; i < parent.childCount; i++)
            {
                if (parent.GetChild(i) == tr)
                {                  
                    selectedIndex = i;
                    break;
                }
            }

            if (selectedIndex < 0)
                return;
            value = selectedIndex;
            OnMenuSelect?.Invoke(value);
            Hide();
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
