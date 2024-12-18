using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;
using System.Collections;
#if UNITY_ASYNC
using System.Threading;
using System.Threading.Tasks;
#endif

namespace UnityEngine.UI
{
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public abstract class LoopScrollRect : UIBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IScrollHandler, ICanvasElement, ILayoutElement, ILayoutGroup
    {
        //==========LoopScrollRect==========       

        [HideInInspector]
        [SerializeField]
        int _totalCount = 100;
        public int totalCount { get { return _totalCount; } set { _totalCount = value; } }

        [HideInInspector]
        [SerializeField]
        protected float _threshold = 100;
        public float threshold { get { return _threshold; } set { _threshold = value; } }

        [HideInInspector]
        [SerializeField]
        bool _reverseDirection = false;
        public bool reverseDirection { get { return _reverseDirection; } set { _reverseDirection = value; } }

        [HideInInspector]
        [SerializeField]
        float _rubberScale = 1f;
        public float rubberScale { get { return _rubberScale; } set { _rubberScale = value; } }

        [HideInInspector]
        [SerializeField]
        bool circleLayout = false;
        public bool CircleLayout { get { return circleLayout; } set { circleLayout = value; } }       

        [HideInInspector]
        [SerializeField]
        //圆心方位，垂直列表：=1右侧 =-1左侧 水平列表 =1下侧 =-1上侧
        float baseDir = 1;
        public float BaseDir { get { return baseDir; }set { baseDir = value; } }

        [HideInInspector]
        [SerializeField]
        [Range(1,359)]
        float radian = 180f;
        public float Radian { get { return radian; }set { radian = value; } }

        [HideInInspector]
        [SerializeField]
        bool scaleCircle = false;
        public bool ScaleCircle { get { return scaleCircle; }set { scaleCircle = value; } }

        [HideInInspector]
        [SerializeField]
        [Range(0,3)]
        float circleScaleTimes = 1f;
        public float CircleScaleTimes { get { return circleScaleTimes; }set { circleScaleTimes = value; } }


        protected int itemTypeStart = 0;
        protected int itemTypeEnd = 0;

        protected abstract float GetSize(RectTransform item);
        protected abstract float GetDimension(Vector2 vector);
        protected abstract Vector2 GetVector(float value);
        protected int directionSign = 0;

        private float m_ContentSpacing = -1;
        protected MyGridLayoutGroup m_GridLayout = null;

        //子项动画标记
        bool m_has_item_tween = false;
        private float m_anima_use_time = 0;

        EffectPanelConfig _config;

        MyTask m_tween_check_task;

        /// <summary>
        /// 初始化孩子数据
        /// </summary>
        private Action<Transform, int> _provideDataAction;
        /// <summary>
        /// 回收
        /// </summary>
        private Action<Transform> _recyclingAction;
        protected float contentSpacing
        {
            get
            {
                if (m_ContentSpacing >= 0)
                {
                    return m_ContentSpacing;
                }
                m_ContentSpacing = 0;
                if (content != null)
                {
                    MyHorizontalOrVerticalLayoutGroup layout1 = content.GetComponent<MyHorizontalOrVerticalLayoutGroup>();
                    if (layout1 != null)
                    {
                        m_ContentSpacing = layout1.spacing;
                    }
                    m_GridLayout = content.GetComponent<MyGridLayoutGroup>();
                    if (m_GridLayout != null)
                    {
                        m_ContentSpacing = GetDimension(m_GridLayout.spacing);
                    }
                }
                return m_ContentSpacing;
            }
        }

        private int m_ContentConstraintCount = 0;
        protected int contentConstraintCount
        {
            get
            {
                if (m_ContentConstraintCount > 0)
                {
                    return m_ContentConstraintCount;
                }
                m_ContentConstraintCount = 1;
                if (content != null)
                {
                    MyGridLayoutGroup layout2 = content.GetComponent<MyGridLayoutGroup>();
                    if (layout2 != null)
                    {
                        if (layout2.constraint == MyGridLayoutGroup.Constraint.Flexible)
                        {
                            if(Application.isEditor)  Debug.LogWarning("[LoopScrollRect] Flexible not supported yet");
                        }
                        m_ContentConstraintCount = layout2.constraintCount;
                    }
                }
                return m_ContentConstraintCount;
            }
        }

        protected virtual bool UpdateItems(Bounds viewBounds, Bounds contentBounds) { return false; }
        //==========LoopScrollRect==========

        public enum MovementType
        {
            Unrestricted, // 无限
            Elastic,        // 限制但灵活 - 可以拉出边缘但弹回原位
            Clamped,    //限制并不可拉出边缘
        }

        public enum ScrollbarVisibility
        {
            Permanent,      //常驻
            AutoHide,       //自动隐藏
            AutoHideAndExpandViewport,
        }



        [Serializable]
        public class ScrollRectEvent : UnityEvent<Vector2> { }

        [HideInInspector]
        [SerializeField]
        public GameObject childPrefab;

        [HideInInspector]
        [SerializeField]
        public float cellAnimaInterval;

        [HideInInspector]
        [SerializeField]
        private int poolSize;



        [HideInInspector]
        [SerializeField]
        private RectTransform m_Content;
        public RectTransform content { get { return m_Content; } set { m_Content = value; } }
        [HideInInspector]
        [SerializeField]
        private bool m_Horizontal = true;
        public bool horizontal { get { return m_Horizontal; } set { m_Horizontal = value; } }
        [HideInInspector]
        [SerializeField]
        private bool m_Vertical = true;
        public bool vertical { get { return m_Vertical; } set { m_Vertical = value; } }
        [HideInInspector]
        [SerializeField]
        private MovementType m_MovementType = MovementType.Elastic;
        public MovementType movementType { get { return m_MovementType; } set { m_MovementType = value; } }
        [HideInInspector]
        [SerializeField]
        private float m_Elasticity = 0.1f; // Only used for MovementType.Elastic
        /// <summary>
        /// 弹性
        /// </summary>
        public float elasticity { get { return m_Elasticity; } set { m_Elasticity = value; } }
        [HideInInspector]
        [SerializeField]
        private bool m_Inertia = true;
        /// <summary>
        /// 惯性
        /// </summary>
        public bool inertia { get { return m_Inertia; } set { m_Inertia = value; } }
        [HideInInspector]
        [SerializeField]
        private float m_DecelerationRate = 0.135f; // Only used when inertia is enabled
        /// <summary>
        /// 减速率
        /// </summary>
        public float decelerationRate { get { return m_DecelerationRate; } set { m_DecelerationRate = value; } }
        [HideInInspector]
        [SerializeField]
        private float m_ScrollSensitivity = 1.0f;
        /// <summary>
        /// 滚动灵敏度
        /// </summary>
        public float scrollSensitivity { get { return m_ScrollSensitivity; } set { m_ScrollSensitivity = value; } }
        [HideInInspector]
        [SerializeField]
        private RectTransform m_Viewport;
        public RectTransform viewport { get { return m_Viewport; } set { m_Viewport = value; SetDirtyCaching(); } }
        [HideInInspector]
        [SerializeField]
        private Scrollbar m_HorizontalScrollbar;
        public Scrollbar horizontalScrollbar
        {
            get
            {
                return m_HorizontalScrollbar;
            }
            set
            {
                if (m_HorizontalScrollbar)
                    m_HorizontalScrollbar.onValueChanged.RemoveListener(SetHorizontalNormalizedPosition);
                m_HorizontalScrollbar = value;
                if (m_HorizontalScrollbar)
                    m_HorizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPosition);
                SetDirtyCaching();
            }
        }
        [HideInInspector]
        [SerializeField]
        private Scrollbar m_VerticalScrollbar;
        public Scrollbar verticalScrollbar
        {
            get
            {
                return m_VerticalScrollbar;
            }
            set
            {
                if (m_VerticalScrollbar)
                    m_VerticalScrollbar.onValueChanged.RemoveListener(SetVerticalNormalizedPosition);
                m_VerticalScrollbar = value;
                if (m_VerticalScrollbar)
                    m_VerticalScrollbar.onValueChanged.AddListener(SetVerticalNormalizedPosition);
                SetDirtyCaching();
            }
        }
        [HideInInspector]
        [SerializeField]
        private ScrollbarVisibility m_HorizontalScrollbarVisibility;
        public ScrollbarVisibility horizontalScrollbarVisibility { get { return m_HorizontalScrollbarVisibility; } set { m_HorizontalScrollbarVisibility = value; SetDirtyCaching(); } }
        [HideInInspector]
        [SerializeField]
        private ScrollbarVisibility m_VerticalScrollbarVisibility;
        public ScrollbarVisibility verticalScrollbarVisibility { get { return m_VerticalScrollbarVisibility; } set { m_VerticalScrollbarVisibility = value; SetDirtyCaching(); } }
        [HideInInspector]
        [SerializeField]
        private float m_HorizontalScrollbarSpacing;
        public float horizontalScrollbarSpacing { get { return m_HorizontalScrollbarSpacing; } set { m_HorizontalScrollbarSpacing = value; SetDirty(); } }
        [HideInInspector]
        [SerializeField]
        private float m_VerticalScrollbarSpacing;
        public float verticalScrollbarSpacing { get { return m_VerticalScrollbarSpacing; } set { m_VerticalScrollbarSpacing = value; SetDirty(); } }
        [HideInInspector]
        [SerializeField]
        private bool m_onValueChanged;
        public bool OnValueChangedMessage { get { return m_onValueChanged; } set { m_onValueChanged = value; } }

        // The offset from handle position to mouse down position
        private Vector2 m_PointerStartLocalCursor = Vector2.zero;
        private Vector2 m_ContentStartPosition = Vector2.zero;

        private RectTransform m_ViewRect;

        protected RectTransform viewRect
        {
            get
            {
                if (m_ViewRect == null)
                    m_ViewRect = m_Viewport;
                if (m_ViewRect == null)
                    m_ViewRect = (RectTransform)transform;
                return m_ViewRect;
            }
        }

        private Bounds m_ContentBounds;
        private Bounds m_ViewBounds;

        private Vector2 m_Velocity;
        public Vector2 velocity { get { return m_Velocity; } set { m_Velocity = value; } }

        public bool m_Dragging{get;private set;}

        private Vector2 m_prevPosition = Vector2.zero;

        public Vector2 m_PrevPosition
        {
            get{return m_prevPosition;}
            private set
            {
                m_prevPosition = value;
            }
        } 

        private Bounds m_PrevContentBounds;
        private Bounds m_PrevViewBounds;
        [NonSerialized]
        private bool m_HasRebuiltLayout = false;

        private bool m_HSliderExpand;
        private bool m_VSliderExpand;
        private float m_HSliderHeight;
        private float m_VSliderWidth;

        [System.NonSerialized]
        private RectTransform m_Rect;
        private RectTransform rectTransform
        {
            get
            {
                if (m_Rect == null)
                    m_Rect = GetComponent<RectTransform>();
                return m_Rect;
            }
        }

        private RectTransform m_HorizontalScrollbarRect;
        private RectTransform m_VerticalScrollbarRect;

        private DrivenRectTransformTracker m_Tracker;

        protected LoopScrollRect()
        {
            flexibleWidth = -1;
        }

        //==========LoopScrollRect==========
        private void ReturnObjectAndSendMessage(Transform go)
        {
            UnityEngine.Profiling.Profiler.BeginSample("ReturnObjectAndSendMessage");
            _recyclingAction?.Invoke(go);
            SG.ResourceManager.Instance.ReturnObjectToPool(go.gameObject);
            UnityEngine.Profiling.Profiler.EndSample();
        }

        public void ClearCells()
        {
            if (Application.isPlaying && content)
            {
                itemTypeStart = 0;
                itemTypeEnd = 0;
                _totalCount = 0;
                //objectsToFill = null;
                for (int i = content.childCount - 1; i >= 0; i--)
                {
                    ReturnObjectAndSendMessage(content.GetChild(i));
                }
            }
        }

        public void RefreshCells()
        {
            if (Application.isPlaying && this.isActiveAndEnabled)
            {
                itemTypeEnd = itemTypeStart;
                // recycle items if we can
                for (int i = 0; i < content.childCount; i++)
                {
                    if (itemTypeEnd < _totalCount)
                    {                        

                        _provideDataAction?.Invoke(content.GetChild(i), itemTypeEnd);
                        itemTypeEnd++;
                    }
                    else
                    {
                        var childCount = content.childCount;
                        ReturnObjectAndSendMessage(content.GetChild(i));
                        if (childCount <= content.childCount)
                        {
                            //死循环！！
                            Log.LogError($"{this.name}, childCount from {childCount} to {content.childCount}, RefreshCells -> ReturnObjectAndSendMessage Error, break!!!!");
                            break;
                        }
                        i--;
                    }
                }
            }
        }

        public void RefillCellsFromEnd(Action<Transform, int> initItemAction = null, Action<Transform> RecyclingAction = null, int itemCount = 0, int offset = 0,EffectPanelConfig config = null)
        {
            if (itemCount > 0)
                _totalCount = itemCount;           
           

            InitPool();

            _provideDataAction = initItemAction;
            _recyclingAction = RecyclingAction;
            //_config = config;

            if (!Application.isPlaying || _totalCount < 0 || contentConstraintCount > 1)
                return;


            StartCoroutine(refillCellsFromEndAsync(offset));


        }

        IEnumerator refillCellsFromEndAsync(int offset)
         {
            yield return null;

            StopMovement();
            itemTypeEnd = _reverseDirection ? offset : _totalCount - offset;
            itemTypeStart = itemTypeEnd;

            for (int i = m_Content.childCount - 1; i >= 0; i--)
            {
                ReturnObjectAndSendMessage(m_Content.GetChild(i));
            }

            float sizeToFill = 0, sizeFilled = 0;
            if (directionSign == -1)
                sizeToFill = viewRect.rect.size.y;
            else
                sizeToFill = viewRect.rect.size.x;

            while (sizeToFill > sizeFilled)
            {
                float size = _reverseDirection ? NewItemAtEnd(true) : NewItemAtStart(true);
                if (size <= 0) break;
                sizeFilled += size;
            }

            Vector2 pos = m_Content.anchoredPosition;
            float dist = Mathf.Max(0, sizeFilled - sizeToFill);
            if (_reverseDirection)
                dist = -dist;
            if (directionSign == -1)
                pos.y = dist;
            else if (directionSign == 1)
                pos.x = -dist;
            m_Content.anchoredPosition = pos;
        }

        public void RefillCells(Action<Transform, int> initItemAction = null, Action<Transform> RecyclingAction = null, int itemCount = 0, int offset = 0,EffectPanelConfig config = null)
        {
            if (itemCount >= 0)
                _totalCount = itemCount;

            _provideDataAction = null;
            _recyclingAction = null;

            _provideDataAction = initItemAction;
            _recyclingAction = RecyclingAction;
            //_config = config;

            InitPool();

            if (!Application.isPlaying)
                return;

            StartCoroutine(refillAsync(offset));

        }


        IEnumerator refillAsync(int offset)
         {
            yield return null;
            UnityEngine.Profiling.Profiler.BeginSample("refillAsync");
            StopMovement();
            itemTypeStart = _reverseDirection ? _totalCount - offset : offset;
            itemTypeEnd = itemTypeStart;


            // Don't `Canvas.ForceUpdateCanvases();` here, or it will new/delete cells to change itemTypeStart/End
            for (int i = m_Content.childCount - 1; i >= 0; i--)
            {
                ReturnObjectAndSendMessage(m_Content.GetChild(i));
            }

            float sizeToFill = 0, sizeFilled = 0;
            // m_ViewBounds may be not ready when RefillCells on Start
            if (directionSign == -1)
                sizeToFill = viewRect.rect.size.y;
            else
                sizeToFill = viewRect.rect.size.x;

            UnityEngine.Profiling.Profiler.BeginSample("_reverseDirection");
            while (sizeToFill > sizeFilled)
            {
                float size = _reverseDirection ? NewItemAtStart(true) : NewItemAtEnd(true);
                if (size <= 0) break;
                sizeFilled += size;
            }
            UnityEngine.Profiling.Profiler.EndSample();

            Vector2 pos = m_Content.anchoredPosition;
            if (directionSign == -1)
                pos.y = 0;
            else if (directionSign == 1)
                pos.x = 0;
            m_Content.anchoredPosition = pos;
            UnityEngine.Profiling.Profiler.EndSample();
        }

        protected float NewItemAtStart(bool first_show = false)
        {
            if (_totalCount >= 0 && itemTypeStart - contentConstraintCount < 0)
            {
                return 0;
            }
            float size = 0;
            for (int i = 0; i < contentConstraintCount; i++)
            {
                itemTypeStart--;
                RectTransform newItem = InstantiateNextItem(itemTypeStart, first_show);
                newItem.SetAsFirstSibling();
                size = Mathf.Max(GetSize(newItem), size);
            }

            if (!_reverseDirection)
            {
                Vector2 offset = GetVector(size);
                content.anchoredPosition += offset;
                m_PrevPosition += offset;
                m_ContentStartPosition += offset;
            }
            // protection
            if (size > 0 && _threshold < size)
                _threshold = size * 1.1f;
            return size;
        }

        protected float DeleteItemAtStart()
        {
            // special case: when moving or dragging, we cannot simply delete start when we've reached the end
            if (((m_Dragging || m_Velocity != Vector2.zero) && _totalCount >= 0 && itemTypeEnd >= _totalCount - 1)
                || content.childCount == 0)
            {
                return 0;
            }

            float size = 0;
            for (int i = 0; i < contentConstraintCount; i++)
            {
                RectTransform oldItem = content.GetChild(0) as RectTransform;
                size = Mathf.Max(GetSize(oldItem), size);
                ReturnObjectAndSendMessage(oldItem);

                itemTypeStart++;

                if (content.childCount == 0)
                {
                    break;
                }
            }

            if (!_reverseDirection)
            {
                Vector2 offset = GetVector(size);
                content.anchoredPosition -= offset;
                m_PrevPosition -= offset;
                m_ContentStartPosition -= offset;
            }
            return size;
        }


        protected float NewItemAtEnd(bool first_show = false)
        {
            if (_totalCount >= 0 && itemTypeEnd >= _totalCount)
            {
                return 0;
            }
            float size = 0;
            // issue 4: fill lines to end first
            int count = contentConstraintCount - (content.childCount % contentConstraintCount);
            for (int i = 0; i < count; i++)
            {
                RectTransform newItem = InstantiateNextItem(itemTypeEnd, first_show);
                size = Mathf.Max(GetSize(newItem), size);
                itemTypeEnd++;
                if (_totalCount >= 0 && itemTypeEnd >= _totalCount)
                {
                    break;
                }
            }

            if (_reverseDirection)
            {
                Vector2 offset = GetVector(size);
                content.anchoredPosition -= offset;
                m_PrevPosition -= offset;
                m_ContentStartPosition -= offset;
            }
            // protection
            if (size > 0 && _threshold < size)
                _threshold = size * 1.1f;
            return size;
        }

        protected float DeleteItemAtEnd()
        {
            if (((m_Dragging || m_Velocity != Vector2.zero) && _totalCount >= 0 && itemTypeStart < contentConstraintCount)
                || content.childCount == 0)
            {
                return 0;
            }

            float size = 0;
            for (int i = 0; i < contentConstraintCount; i++)
            {
                RectTransform oldItem = content.GetChild(content.childCount - 1) as RectTransform;
                size = Mathf.Max(GetSize(oldItem), size);
                ReturnObjectAndSendMessage(oldItem);

                itemTypeEnd--;
                if (itemTypeEnd % contentConstraintCount == 0 || content.childCount == 0)
                {
                    break;  //just delete the whole row
                }
            }

            if (_reverseDirection)
            {
                Vector2 offset = GetVector(size);
                content.anchoredPosition += offset;
                m_PrevPosition += offset;
                m_ContentStartPosition += offset;
            }
            return size;
        }

        private RectTransform InstantiateNextItem(int itemIdx,bool tween = false)
        {
            RectTransform nextItem = GetItemObjectFromPool();
            if (!nextItem) return null;
            nextItem.transform.SetParent(content, false);
            nextItem.gameObject.SetActive(true);
            if ((Time.time - m_anima_use_time) < 1 && cellAnimaInterval > 0)
            {
                nextItem.gameObject.AddMissingComponent<ScrollCellDelayAnima>().Init(cellAnimaInterval * itemIdx);
            }

            if (_provideDataAction == null)
            {
                nextItem.gameObject.name = $"item_{itemIdx}";
            }
            else
            {
                if (_config != null && tween && !m_has_item_tween)
                {
                    if (_config._listTween_1 || _config._listTween_2)
                    {
                        if (_config._listTween_1)
                            nextItem.transform.localScale = Vector3.zero;
                        else
                            nextItem.transform.localScale = new Vector3(1,0,1);

                        _provideDataAction?.Invoke(nextItem, itemIdx);

                        Hashtable hash = new Hashtable();
                        var list_tween_delay = _config._list_tween_delay;
                        if (_config._listTween_2)
                        {
                            hash.Add("y", 1);
                            hash.Add("easetype", _config._appearEaseType);
                            hash.Add("time", _config._list_tween_dur);
                            hash.Add("delay", 0.2f * (itemIdx + 1) + list_tween_delay);
                            iTween.ScaleTo(nextItem.gameObject, hash);

                            StartCoroutine(ResetScale(nextItem, 0.8f * (itemIdx + 1) + list_tween_delay));
                        }
                        else if(_config._listTween_1)
                        {
                            StartCoroutine(ResetScale(nextItem, _config._list_tween_dur + 0.1f * itemIdx + list_tween_delay));

                        }

                        if (m_tween_check_task != null && m_tween_check_task.IsRunning)
                            m_tween_check_task.Stop();

                        m_tween_check_task = MyTask.RunTask(TweenEndCheck());
                    }
                    else
                    {
                        _provideDataAction?.Invoke(nextItem, itemIdx);
                    }
                }
                else
                {
                    _provideDataAction?.Invoke(nextItem, itemIdx);
                }
            }
            //dataSource.ProvideData(nextItem, itemIdx);
            return nextItem;
        }

        IEnumerator ResetScale(RectTransform trans,float wait)
        {
            yield return new WaitForSeconds(wait);

            trans.localScale = Vector3.one;
        }

        IEnumerator TweenEndCheck()
        {
            yield return 1000;
            m_has_item_tween = true;
        }

        private bool inited = false;
        private string pool_key = "";
        private RectTransform GetItemObjectFromPool()
        {
            if (!inited)
            {
                if (Application.isEditor) Log.LogError($"LoopScrollRect {gameObject.name} ,inited:{inited} 没有初始化缓冲池");
                return null;
            }

            var result = SG.ResourceManager.Instance.GetObjectFromPool(pool_key, false);
            if(result)
                return result.GetComponent<RectTransform>();
            if(Application.isEditor) Log.LogError($"LoopScrollRect {gameObject.name} 获取失败");
            return null;
        }

        private void InitPool()
        {
            if (!inited && !childPrefab)
            {
                if (Application.isEditor) Log.LogError($"LoopScrollRect {gameObject.name} ,childPrefab:{childPrefab},inited:{inited} 没有定义节点对象");
                return;
            }

            if (childPrefab && !inited)
            {
                if(string.IsNullOrEmpty(pool_key))
                    pool_key = GameObjectUtils.GetLocation(gameObject).Replace('/','_'); //$"{GameObjectUtils.FindInParents<Canvas>(this.gameObject).gameObject.name}_{gameObject.name}_{childPrefab.name}";
                SG.ResourceManager.Instance.InitPool(pool_key, childPrefab, poolSize,SG.PoolInflationType.INCREMENT);
                inited = true;

#if UNITY_EDITOR
                if (!Application.isPlaying)
                    UnityEditor.EditorApplication.delayCall += () => { GameObject.DestroyImmediate(childPrefab.gameObject); };
                else
                {
                    //Log.LogInfo($"LoopScrollRect destory {childPrefab}");
                    //GameObject.Destroy(childPrefab.gameObject);
                    childPrefab.transform.SetParent(gameObject.transform);
                    childPrefab.SetActive(false);
                }
#else
                childPrefab.transform.SetParent(gameObject.transform);
                childPrefab.SetActive(false);
                    //GameObject.Destroy(childPrefab.gameObject);
#endif

            }
        }
        //==========LoopScrollRect==========

        public virtual void Rebuild(CanvasUpdate executing)
        {
            if (executing == CanvasUpdate.Prelayout)
            {
                UpdateCachedData();
            }

            if (executing == CanvasUpdate.PostLayout)
            {
                UpdateBounds();
                UpdateScrollbars(Vector2.zero);
                UpdatePrevData();

                m_HasRebuiltLayout = true;
            }
        }

        public virtual void LayoutComplete()
        { }

        public virtual void GraphicUpdateComplete()
        { }

        void UpdateCachedData()
        {
            Transform transform = this.transform;
            m_HorizontalScrollbarRect = m_HorizontalScrollbar == null ? null : m_HorizontalScrollbar.transform as RectTransform;
            m_VerticalScrollbarRect = m_VerticalScrollbar == null ? null : m_VerticalScrollbar.transform as RectTransform;

            // These are true if either the elements are children, or they don't exist at all.
            bool viewIsChild = (viewRect.parent == transform);
            bool hScrollbarIsChild = (!m_HorizontalScrollbarRect || m_HorizontalScrollbarRect.parent == transform);
            bool vScrollbarIsChild = (!m_VerticalScrollbarRect || m_VerticalScrollbarRect.parent == transform);
            bool allAreChildren = (viewIsChild && hScrollbarIsChild && vScrollbarIsChild);

            m_HSliderExpand = allAreChildren && m_HorizontalScrollbarRect && horizontalScrollbarVisibility == ScrollbarVisibility.AutoHideAndExpandViewport;
            m_VSliderExpand = allAreChildren && m_VerticalScrollbarRect && verticalScrollbarVisibility == ScrollbarVisibility.AutoHideAndExpandViewport;
            m_HSliderHeight = (m_HorizontalScrollbarRect == null ? 0 : m_HorizontalScrollbarRect.rect.height);
            m_VSliderWidth = (m_VerticalScrollbarRect == null ? 0 : m_VerticalScrollbarRect.rect.width);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            m_anima_use_time = Time.time;
            _toCenterItem = null;
            m_has_item_tween = false;
            if (m_HorizontalScrollbar)
                m_HorizontalScrollbar.onValueChanged.AddListener(SetHorizontalNormalizedPosition);
            if (m_VerticalScrollbar)
                m_VerticalScrollbar.onValueChanged.AddListener(SetVerticalNormalizedPosition);

            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
            _config = gameObject.GetComponent<EffectPanelConfig>();
        }

        protected override void OnDisable()
        {
            CanvasUpdateRegistry.UnRegisterCanvasElementForRebuild(this);

            if (m_HorizontalScrollbar)
                m_HorizontalScrollbar.onValueChanged.RemoveListener(SetHorizontalNormalizedPosition);
            if (m_VerticalScrollbar)
                m_VerticalScrollbar.onValueChanged.RemoveListener(SetVerticalNormalizedPosition);

            m_HasRebuiltLayout = false;
            m_has_item_tween = false;
            m_Tracker.Clear();
            m_Velocity = Vector2.zero;
            LayoutRebuilderEx.MarkLayoutForRebuild(rectTransform);
            base.OnDisable();

            _provideDataAction = null;
            _recyclingAction = null;

            if (Application.isPlaying)
            {
                ClearResource();
            }
        }

        protected override void OnDestroy()
        {
            if (Application.isPlaying)
            {
                ClearResource();
            }
            base.OnDestroy();
        }

        public override bool IsActive()
        {
            return base.IsActive() && m_Content != null;
        }

        private void EnsureLayoutHasRebuilt()
        {
            if (!m_HasRebuiltLayout && !CanvasUpdateRegistry.IsRebuildingLayout())
                Canvas.ForceUpdateCanvases();
        }

        public virtual void StopMovement()
        {
            m_Velocity = Vector2.zero;
        }

        public virtual void OnScroll(PointerEventData data)
        {
            if (!IsActive())
                return;

            EnsureLayoutHasRebuilt();
            UpdateBounds();

            Vector2 delta = data.scrollDelta;
            // Down is positive for scroll events, while in UI system up is positive.
            delta.y *= -1;
            if (vertical && !horizontal)
            {
                if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                    delta.y = delta.x;
                delta.x = 0;
            }
            if (horizontal && !vertical)
            {
                if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
                    delta.x = delta.y;
                delta.y = 0;
            }

            Vector2 position = m_Content.anchoredPosition;
            position += delta * m_ScrollSensitivity;
            if (m_MovementType == MovementType.Clamped)
                position += CalculateOffset(position - m_Content.anchoredPosition);

            SetContentAnchoredPosition(position);
            UpdateBounds();
        }

        public virtual void OnInitializePotentialDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            m_Velocity = Vector2.zero;
        }

        public virtual void OnBeginDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!IsActive())
                return;

            UpdateBounds();

            m_PointerStartLocalCursor = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out m_PointerStartLocalCursor);
            m_ContentStartPosition = m_Content.anchoredPosition;
            m_Dragging = true;
        }

        public virtual void OnEndDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;
            _toCenterItem = null;
            m_Dragging = false;
            SendMessageUpwards("_onScrollRectEndDrag", this);
        }

        public virtual void OnDrag(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            if (!IsActive())
                return;

            _toCenterItem = null;

            Vector2 localCursor;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(viewRect, eventData.position, eventData.pressEventCamera, out localCursor))
                return;

            UpdateBounds();

            var pointerDelta = localCursor - m_PointerStartLocalCursor;
            Vector2 position = m_ContentStartPosition + pointerDelta;

            // Offset to get content into place in the view.
            Vector2 offset = CalculateOffset(position - m_Content.anchoredPosition);
            position += offset;
            if (m_MovementType == MovementType.Elastic)
            {
                //==========LoopScrollRect==========
                if (offset.x != 0)
                    position.x = position.x - RubberDelta(offset.x, m_ViewBounds.size.x) * rubberScale;
                if (offset.y != 0)
                    position.y = position.y - RubberDelta(offset.y, m_ViewBounds.size.y) * rubberScale;
                //==========LoopScrollRect==========
            }

            SetContentAnchoredPosition(position);
        }

        RectTransform _toCenterItem;
        int _moveToType = 0;//=0 到中间 =1到头部 =2到尾部

        /// <summary>
        /// 移动到相应位置
        /// </summary>
        /// <param name="childItem">要移动的节点</param>
        /// <param name="mtype">=0中间 =1头部 =2尾部</param>
        public virtual void  MoveToCenter(RectTransform childItem,int mtype = 0)
        {
            UpdateBounds();
            _moveToType = mtype;
            _toCenterItem = childItem;
        }

        public virtual void MoveTo(Vector2 movePos)
        {
            if (!IsActive())
                return; 

            UpdateBounds();

            var position = m_Content.anchoredPosition + movePos;
            if (!m_Horizontal)
                position.x = m_Content.anchoredPosition.x;
            if (!m_Vertical)
                position.y = m_Content.anchoredPosition.y;

            _toCenterItem = null;

        }





        protected virtual void SetContentAnchoredPosition(Vector2 position)
        {
            if (!m_Horizontal)
                position.x = m_Content.anchoredPosition.x;
            if (!m_Vertical)
                position.y = m_Content.anchoredPosition.y;

            if (position != m_Content.anchoredPosition)
            {
                m_Content.anchoredPosition = position;
                UpdateBounds(true);
            }                        
        }
       
        protected virtual void LateUpdate()
        {
            if (!m_Content)
                return;

            if (!inited)
            {
                if (childPrefab && childPrefab.gameObject.IsActive())
                    childPrefab.gameObject.SetActive(false);
                return;
            }

            // if (is_threading) return;
            UnityEngine.Profiling.Profiler.BeginSample("EnsureLayoutHasRebuilt");
            EnsureLayoutHasRebuilt();
            UnityEngine.Profiling.Profiler.EndSample();

            UnityEngine.Profiling.Profiler.BeginSample("UpdateScrollbarVisibility");
            UpdateScrollbarVisibility();
            UnityEngine.Profiling.Profiler.EndSample();

            UnityEngine.Profiling.Profiler.BeginSample("UpdateBounds");
            UpdateBounds();
            UnityEngine.Profiling.Profiler.EndSample();

            float deltaTime = Time.unscaledDeltaTime;            

            if (_toCenterItem)
            {
                Vector2 offset = Vector2.zero;

                if (this is LoopVerticalScrollRect)
                {
                    if(_moveToType == 0)
                        offset.y = ( transform.position.y  - _toCenterItem.position.y ) / GameObjectUtils.FindInParents<Canvas>(gameObject).transform.localScale.y - rectTransform.GetSize().y / 2 ;
                    else if(_moveToType == 1)
                        offset.y = (transform.position.y - _toCenterItem.position.y) / GameObjectUtils.FindInParents<Canvas>(gameObject).transform.localScale.y - (_toCenterItem.GetSize().y / 2);
                    else if (_moveToType == 2)
                        offset.y = (transform.position.y - _toCenterItem.position.y) / GameObjectUtils.FindInParents<Canvas>(gameObject).transform.localScale.y - rectTransform.GetSize().y + (_toCenterItem.GetSize().y / 2);
                }
                else if(this is LoopHorizontalScrollRect)
                {
                    if (_moveToType == 0)
                        offset.x = ( transform.position.x  - _toCenterItem.position.x ) / GameObjectUtils.FindInParents<Canvas>(gameObject).transform.localScale.x + rectTransform.GetSize().x / 2;
                    else if (_moveToType == 1)
                        offset.x = (transform.position.x - _toCenterItem.position.x) / GameObjectUtils.FindInParents<Canvas>(gameObject).transform.localScale.x + (_toCenterItem.GetSize().x / 2);
                    else if (_moveToType == 2)
                        offset.x = (transform.position.x - _toCenterItem.position.x) / GameObjectUtils.FindInParents<Canvas>(gameObject).transform.localScale.x + rectTransform.GetSize().x - (_toCenterItem.GetSize().x / 2);
                }

                if(offset.SqrMagnitude() <= 10f)
                {
                    _toCenterItem = null;
                    m_Velocity = Vector2.zero;
                    SetContentAnchoredPosition(m_Content.anchoredPosition + offset);
                }
                else
                {
                    Vector2 position = m_Content.anchoredPosition;
                    for (int axis = 0; axis < 2; axis++)
                    {
                        float speed = m_Velocity[axis];
                        position[axis] = Mathf.SmoothDamp(m_Content.anchoredPosition[axis], m_Content.anchoredPosition[axis] + offset[axis], ref speed, m_Elasticity, Mathf.Infinity, deltaTime);
                        m_Velocity[axis] = speed;
                    }
                    SetContentAnchoredPosition(position);
                    
                }
               

                if (m_ViewBounds != m_PrevViewBounds || m_ContentBounds != m_PrevContentBounds || m_Content.anchoredPosition != m_PrevPosition)
                {
                    UpdateScrollbars(offset);
                    if (m_onValueChanged)
                    {
                        SendMessageUpwards("_onScrollRectValueChanged", this);
                    }
                    UpdatePrevData();
                }

            }
            else
            {                
                Vector2 offset = CalculateOffset(Vector2.zero);
                if (!m_Dragging && offset.sqrMagnitude < 0.1f)
                {
                    //颤抖问题，下面 的 Mathf.SmoothDamp 算法有缺陷，speed会反向
                    offset = Vector2.zero;
                }
                if (!m_Dragging && (offset != Vector2.zero || m_Velocity != Vector2.zero))
                {                   
                    Vector2 position = m_Content.anchoredPosition;
                    for (int axis = 0; axis < 2; axis++)
                    {
                        // Apply spring physics if movement is elastic and content has an offset from the view.
                        if (m_MovementType == MovementType.Elastic && offset[axis] != 0)
                        {
                            float speed = m_Velocity[axis];
                            position[axis] = Mathf.SmoothDamp(m_Content.anchoredPosition[axis], m_Content.anchoredPosition[axis] + offset[axis], ref speed, m_Elasticity, Mathf.Infinity, deltaTime);
                            m_Velocity[axis] = speed;
                        }
                        // Else move content according to velocity with deceleration applied.
                        else if (m_Inertia)
                        {
                            m_Velocity[axis] *= Mathf.Pow(m_DecelerationRate, deltaTime);
                            if (Mathf.Abs(m_Velocity[axis]) < 1)
                                m_Velocity[axis] = 0;
                            position[axis] += m_Velocity[axis] * deltaTime;
                        }
                        // If we have neither elaticity or friction, there shouldn't be any velocity.
                        else
                        {
                            m_Velocity[axis] = 0;
                        }
                    }

                    if (m_Velocity != Vector2.zero)
                    {
                        if (m_MovementType == MovementType.Clamped)
                        {
                            offset = CalculateOffset(position - m_Content.anchoredPosition);
                            position += offset;
                        }

                        SetContentAnchoredPosition(position);
                    }
                }

                if (m_Dragging && m_Inertia)
                {
                    Vector3 newVelocity = (m_Content.anchoredPosition - m_PrevPosition) / deltaTime;
                    m_Velocity = Vector3.Lerp(m_Velocity, newVelocity, deltaTime * 10);
                }

                if (m_ViewBounds != m_PrevViewBounds || m_ContentBounds != m_PrevContentBounds || m_Content.anchoredPosition != m_PrevPosition)
                {
                    UpdateScrollbars(offset);
                    if (m_onValueChanged)
                    {
                        SendMessageUpwards("_onScrollRectValueChanged", this);
                    }                    
                    UpdatePrevData();
                }
            }

           

           

        }

        public void UpdateItem(int idx)
        {
            if(!inited)
            {
                Log.LogError("not inited");
                return;
            }
            if(_provideDataAction == null)
            {
                Log.LogError("_provideDataAction = null");
                return;
            }
            if(idx >= itemTypeStart && idx < itemTypeEnd)
            {
                _provideDataAction?.Invoke(content.GetChild(idx), idx);
            }

        }

        private void UpdatePrevData()
        {
            if (m_Content == null)
                m_PrevPosition = Vector2.zero;
            else
                m_PrevPosition = m_Content.anchoredPosition;
            m_PrevViewBounds = m_ViewBounds;
            m_PrevContentBounds = m_ContentBounds;
        }

        private void UpdateScrollbars(Vector2 offset)
        {
            if (m_HorizontalScrollbar)
            {
                //==========LoopScrollRect==========
                if (m_ContentBounds.size.x > 0 && _totalCount > 0)
                {
                    m_HorizontalScrollbar.size = Mathf.Clamp01((m_ViewBounds.size.x - Mathf.Abs(offset.x)) / m_ContentBounds.size.x * (itemTypeEnd - itemTypeStart) / _totalCount);
                }
                //==========LoopScrollRect==========
                else
                    m_HorizontalScrollbar.size = 1;

                m_HorizontalScrollbar.value = horizontalNormalizedPosition;
            }

            if (m_VerticalScrollbar)
            {
                //==========LoopScrollRect==========
                if (m_ContentBounds.size.y > 0 && _totalCount > 0)
                {
                    m_VerticalScrollbar.size = Mathf.Clamp01((m_ViewBounds.size.y - Mathf.Abs(offset.y)) / m_ContentBounds.size.y * (itemTypeEnd - itemTypeStart) / _totalCount);
                }
                //==========LoopScrollRect==========
                else
                    m_VerticalScrollbar.size = 1;

                m_VerticalScrollbar.value = verticalNormalizedPosition;
            }
        }

        public Vector2 normalizedPosition
        {
            get
            {
                return new Vector2(horizontalNormalizedPosition, verticalNormalizedPosition);
            }
            set
            {
                SetNormalizedPosition(value.x, 0);
                SetNormalizedPosition(value.y, 1);
            }
        }

        public float horizontalNormalizedPosition
        {
            get
            {
                UpdateBounds();
                //==========LoopScrollRect==========
                if (_totalCount > 0 && itemTypeEnd > itemTypeStart)
                {
                    //TODO: consider contentSpacing
                    float elementSize = m_ContentBounds.size.x / (itemTypeEnd - itemTypeStart);
                    float totalSize = elementSize * _totalCount;
                    float offset = m_ContentBounds.min.x - elementSize * itemTypeStart;

                    if (totalSize <= m_ViewBounds.size.x)
                        return (m_ViewBounds.min.x > offset) ? 1 : 0;
                    return (m_ViewBounds.min.x - offset) / (totalSize - m_ViewBounds.size.x);
                }
                else
                    return 0.5f;
                //==========LoopScrollRect==========
            }
            set
            {
                SetNormalizedPosition(value, 0);
            }
        }

        public float verticalNormalizedPosition
        {
            get
            {
                UpdateBounds();
                //==========LoopScrollRect==========
                if (_totalCount > 0 && itemTypeEnd > itemTypeStart)
                {
                    //TODO: consider contentSpacinge
                    float elementSize = m_ContentBounds.size.y / (itemTypeEnd - itemTypeStart);
                    float totalSize = elementSize * _totalCount;
                    float offset = m_ContentBounds.max.y + elementSize * itemTypeStart;

                    if (totalSize <= m_ViewBounds.size.y)
                        return (offset > m_ViewBounds.max.y) ? 1 : 0;
                    return (offset - m_ViewBounds.max.y) / (totalSize - m_ViewBounds.size.y);
                }
                else
                    return 0.5f;
                //==========LoopScrollRect==========
            }
            set
            {
                SetNormalizedPosition(value, 1);
            }
        }

        private void SetHorizontalNormalizedPosition(float value) { SetNormalizedPosition(value, 0); }
        private void SetVerticalNormalizedPosition(float value) { SetNormalizedPosition(value, 1); }

        private void SetNormalizedPosition(float value, int axis)
        {
            //==========LoopScrollRect==========
            if (_totalCount <= 0 || itemTypeEnd <= itemTypeStart)
                return;
            //==========LoopScrollRect==========

            EnsureLayoutHasRebuilt();
            UpdateBounds();

            //==========LoopScrollRect==========
            Vector3 localPosition = m_Content.localPosition;
            float newLocalPosition = localPosition[axis];
            if (axis == 0)
            {
                float elementSize = m_ContentBounds.size.x / (itemTypeEnd - itemTypeStart);
                float totalSize = elementSize * _totalCount;
                float offset = m_ContentBounds.min.x - elementSize * itemTypeStart;

                newLocalPosition += m_ViewBounds.min.x - value * (totalSize - m_ViewBounds.size[axis]) - offset;
            }
            else if (axis == 1)
            {
                float elementSize = m_ContentBounds.size.y / (itemTypeEnd - itemTypeStart);
                float totalSize = elementSize * _totalCount;
                float offset = m_ContentBounds.max.y + elementSize * itemTypeStart;

                newLocalPosition -= offset - value * (totalSize - m_ViewBounds.size.y) - m_ViewBounds.max.y;
            }
            //==========LoopScrollRect==========

            if (Mathf.Abs(localPosition[axis] - newLocalPosition) > 0.01f)
            {
                localPosition[axis] = newLocalPosition;
                m_Content.localPosition = localPosition;
                m_Velocity[axis] = 0;
                UpdateBounds(true);
            }
        }

        private static float RubberDelta(float overStretching, float viewSize)
        {
            return (1 - (1 / ((Mathf.Abs(overStretching) * 0.55f / viewSize) + 1))) * viewSize * Mathf.Sign(overStretching);
        }

        protected override void OnRectTransformDimensionsChange()
        {
            SetDirty();
        }

        private bool hScrollingNeeded
        {
            get
            {
                if (Application.isPlaying)
                    return m_ContentBounds.size.x > m_ViewBounds.size.x + 0.01f;
                return true;
            }
        }
        private bool vScrollingNeeded
        {
            get
            {
                if (Application.isPlaying)
                    return m_ContentBounds.size.y > m_ViewBounds.size.y + 0.01f;
                return true;
            }
        }

        public virtual void CalculateLayoutInputHorizontal() { }
        public virtual void CalculateLayoutInputVertical() { }

        public virtual float minWidth { get { return -1; } }
        public virtual float preferredWidth { get { return -1; } }
        public virtual float flexibleWidth { get; private set; }

        public virtual float minHeight { get { return -1; } }
        public virtual float preferredHeight { get { return -1; } }
        public virtual float flexibleHeight { get { return -1; } }

        public virtual int layoutPriority { get { return -1; } }

        public virtual void SetLayoutHorizontal()
        {
            m_Tracker.Clear();

            if (m_HSliderExpand || m_VSliderExpand)
            {
                m_Tracker.Add(this, viewRect,
                    DrivenTransformProperties.Anchors |
                    DrivenTransformProperties.SizeDelta |
                    DrivenTransformProperties.AnchoredPosition);

                // Make view full size to see if content fits.
                viewRect.anchorMin = Vector2.zero;
                viewRect.anchorMax = Vector2.one;
                viewRect.sizeDelta = Vector2.zero;
                viewRect.anchoredPosition = Vector2.zero;

                // Recalculate content layout with this size to see if it fits when there are no scrollbars.
                LayoutRebuilderEx.ForceRebuildLayoutImmediate(content);
                m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
                m_ContentBounds = GetBounds();
            }

            // If it doesn't fit vertically, enable vertical scrollbar and shrink view horizontally to make room for it.
            if (m_VSliderExpand && vScrollingNeeded)
            {
                viewRect.sizeDelta = new Vector2(-(m_VSliderWidth + m_VerticalScrollbarSpacing), viewRect.sizeDelta.y);

                // Recalculate content layout with this size to see if it fits vertically
                // when there is a vertical scrollbar (which may reflowed the content to make it taller).
                LayoutRebuilderEx.ForceRebuildLayoutImmediate(content);
                m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
                m_ContentBounds = GetBounds();
            }

            // If it doesn't fit horizontally, enable horizontal scrollbar and shrink view vertically to make room for it.
            if (m_HSliderExpand && hScrollingNeeded)
            {
                viewRect.sizeDelta = new Vector2(viewRect.sizeDelta.x, -(m_HSliderHeight + m_HorizontalScrollbarSpacing));
                m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
                m_ContentBounds = GetBounds();
            }

            // If the vertical slider didn't kick in the first time, and the horizontal one did,
            // we need to check again if the vertical slider now needs to kick in.
            // If it doesn't fit vertically, enable vertical scrollbar and shrink view horizontally to make room for it.
            if (m_VSliderExpand && vScrollingNeeded && viewRect.sizeDelta.x == 0 && viewRect.sizeDelta.y < 0)
            {
                viewRect.sizeDelta = new Vector2(-(m_VSliderWidth + m_VerticalScrollbarSpacing), viewRect.sizeDelta.y);
            }
        }

        public virtual void SetLayoutVertical()
        {
            UpdateScrollbarLayout();
            m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
            m_ContentBounds = GetBounds();
        }

        void UpdateScrollbarVisibility()
        {
            if (m_VerticalScrollbar && m_VerticalScrollbarVisibility != ScrollbarVisibility.Permanent && m_VerticalScrollbar.gameObject.activeSelf != vScrollingNeeded)
                m_VerticalScrollbar.gameObject.SetActive(vScrollingNeeded);

            if (m_HorizontalScrollbar && m_HorizontalScrollbarVisibility != ScrollbarVisibility.Permanent && m_HorizontalScrollbar.gameObject.activeSelf != hScrollingNeeded)
                m_HorizontalScrollbar.gameObject.SetActive(hScrollingNeeded);
        }

        void UpdateScrollbarLayout()
        {
            if (m_VSliderExpand && m_HorizontalScrollbar)
            {
                m_Tracker.Add(this, m_HorizontalScrollbarRect,
                              DrivenTransformProperties.AnchorMinX |
                              DrivenTransformProperties.AnchorMaxX |
                              DrivenTransformProperties.SizeDeltaX |
                              DrivenTransformProperties.AnchoredPositionX);
                m_HorizontalScrollbarRect.anchorMin = new Vector2(0, m_HorizontalScrollbarRect.anchorMin.y);
                m_HorizontalScrollbarRect.anchorMax = new Vector2(1, m_HorizontalScrollbarRect.anchorMax.y);
                m_HorizontalScrollbarRect.anchoredPosition = new Vector2(0, m_HorizontalScrollbarRect.anchoredPosition.y);
                if (vScrollingNeeded)
                    m_HorizontalScrollbarRect.sizeDelta = new Vector2(-(m_VSliderWidth + m_VerticalScrollbarSpacing), m_HorizontalScrollbarRect.sizeDelta.y);
                else
                    m_HorizontalScrollbarRect.sizeDelta = new Vector2(0, m_HorizontalScrollbarRect.sizeDelta.y);
            }

            if (m_HSliderExpand && m_VerticalScrollbar)
            {
                m_Tracker.Add(this, m_VerticalScrollbarRect,
                              DrivenTransformProperties.AnchorMinY |
                              DrivenTransformProperties.AnchorMaxY |
                              DrivenTransformProperties.SizeDeltaY |
                              DrivenTransformProperties.AnchoredPositionY);
                m_VerticalScrollbarRect.anchorMin = new Vector2(m_VerticalScrollbarRect.anchorMin.x, 0);
                m_VerticalScrollbarRect.anchorMax = new Vector2(m_VerticalScrollbarRect.anchorMax.x, 1);
                m_VerticalScrollbarRect.anchoredPosition = new Vector2(m_VerticalScrollbarRect.anchoredPosition.x, 0);
                if (hScrollingNeeded)
                    m_VerticalScrollbarRect.sizeDelta = new Vector2(m_VerticalScrollbarRect.sizeDelta.x, -(m_HSliderHeight + m_HorizontalScrollbarSpacing));
                else
                    m_VerticalScrollbarRect.sizeDelta = new Vector2(m_VerticalScrollbarRect.sizeDelta.x, 0);
            }
        }

        public void UpdateBounds(bool updateItems = false)
        {
            m_ViewBounds = new Bounds(viewRect.rect.center, viewRect.rect.size);
            m_ContentBounds = GetBounds();

            if (m_Content == null)
                return;

            // ============LoopScrollRect============
            // Don't do this in Rebuild
            if (Application.isPlaying && updateItems && UpdateItems(m_ViewBounds, m_ContentBounds))
            {
                Canvas.ForceUpdateCanvases();
                m_ContentBounds = GetBounds();
            }
            // ============LoopScrollRect============

            // Make sure content bounds are at least as large as view by adding padding if not.
            // One might think at first that if the content is smaller than the view, scrolling should be allowed.
            // However, that's not how scroll views normally work.
            // Scrolling is *only* possible when content is *larger* than view.
            // We use the pivot of the content rect to decide in which directions the content bounds should be expanded.
            // E.g. if pivot is at top, bounds are expanded downwards.
            // This also works nicely when MyContentSizeFitter is used on the content.
            Vector3 contentSize = m_ContentBounds.size;
            Vector3 contentPos = m_ContentBounds.center;
            Vector3 excess = m_ViewBounds.size - contentSize;
            if (excess.x > 0)
            {
                contentPos.x -= excess.x * (m_Content.pivot.x - 0.5f);
                contentSize.x = m_ViewBounds.size.x;
            }
            if (excess.y > 0)
            {
                contentPos.y -= excess.y * (m_Content.pivot.y - 0.5f);
                contentSize.y = m_ViewBounds.size.y;
            }

            m_ContentBounds.size = contentSize;
            m_ContentBounds.center = contentPos;

            
            if (circleLayout)
            {
                for (int i = 0; i < m_Content.childCount; ++i)
                {
                    var childItem = m_Content.GetChild(i);
                    var content = childItem.GetChild(0);
                    var pos = content.transform.localPosition;

                    float x = pos.x, y = pos.y, scale =1f;

                    if (this is LoopVerticalScrollRect)
                    {                        
                        var localv = ((childItem.position.y - transform.position.y) / GameObjectUtils.FindInParents<Canvas>(gameObject).transform.localScale.y) / (transform as RectTransform).GetSize().y;
                        float angle = localv * radian;
                        x = (Mathf.Sin(angle * Mathf.Deg2Rad) * baseDir * (m_Content.GetSize().x - (content as RectTransform).GetSize().x));

                        if(scaleCircle)
                            scale = 1+ Math.Abs(Mathf.Sin(angle * Mathf.Deg2Rad)) * circleScaleTimes;
                    }
                    else if(this is LoopHorizontalScrollRect)
                    {
                        var localv = ((transform.position.x - childItem.position.x ) / GameObjectUtils.FindInParents<Canvas>(gameObject).transform.localScale.x) / (transform as RectTransform).GetSize().x;
                        float angle = localv * radian;
                        y = (Mathf.Sin(angle * Mathf.Deg2Rad) * baseDir * (m_Content.GetSize().y - (content as RectTransform).GetSize().y));

                        if (scaleCircle)
                            scale =1+ Math.Abs(Mathf.Sin(angle * Mathf.Deg2Rad)) * circleScaleTimes;

                        
                    }

                    content.transform.localPosition = new Vector3(x, y, pos.z);
                    content.transform.localScale = Vector3.one * scale;
                }
            }
        }

        private readonly Vector3[] m_Corners = new Vector3[4];
        private Bounds GetBounds()
        {
            if (m_Content == null)
                return new Bounds();

            var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            var toLocal = viewRect.worldToLocalMatrix;
            m_Content.GetWorldCorners(m_Corners);
            for (int j = 0; j < 4; j++)
            {
                Vector3 v = toLocal.MultiplyPoint3x4(m_Corners[j]);
                vMin = Vector3.Min(v, vMin);
                vMax = Vector3.Max(v, vMax);
            }

            var bounds = new Bounds(vMin, Vector3.zero);
            bounds.Encapsulate(vMax);
            return bounds;
        }

        private Vector2 CalculateOffset(Vector2 delta)
        {
            Vector2 offset = Vector2.zero;
            if (m_MovementType == MovementType.Unrestricted)
                return offset;

            Vector2 min = m_ContentBounds.min;
            Vector2 max = m_ContentBounds.max;

            if (m_Horizontal)
            {
                min.x += delta.x;
                max.x += delta.x;
                if (min.x > m_ViewBounds.min.x)
                    offset.x = m_ViewBounds.min.x - min.x;
                else if (max.x < m_ViewBounds.max.x)
                    offset.x = m_ViewBounds.max.x - max.x;
            }

            if (m_Vertical)
            {
                min.y += delta.y;
                max.y += delta.y;
                if (max.y < m_ViewBounds.max.y)
                    offset.y = m_ViewBounds.max.y - max.y;
                else if (min.y > m_ViewBounds.min.y)
                    offset.y = m_ViewBounds.min.y - min.y;
            }

            return offset;
        }

        protected void SetDirty()
        {
            if (!IsActive())
                return;

            LayoutRebuilderEx.MarkLayoutForRebuild(rectTransform);
        }

        protected void SetDirtyCaching()
        {
            if (!IsActive())
                return;

            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
            LayoutRebuilderEx.MarkLayoutForRebuild(rectTransform);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            SetDirtyCaching();
        }
#endif


        public void MoveToEnd(bool useAnim = true)
        {
            if (!Application.isPlaying || _totalCount < 0 || contentConstraintCount > 1)
                return;

            StopMovement();
            itemTypeEnd = _reverseDirection ? 0 : _totalCount;
            itemTypeStart = itemTypeEnd;

            for (int i = m_Content.childCount - 1; i >= 0; i--)
            {
                ReturnObjectAndSendMessage(m_Content.GetChild(i));
            }

            float sizeToFill = 0, sizeFilled = 0;
            if (directionSign == -1)
                sizeToFill = viewRect.rect.size.y;
            else
                sizeToFill = viewRect.rect.size.x;

            while (sizeToFill > sizeFilled)
            {
                float size = _reverseDirection ? NewItemAtEnd() : NewItemAtStart();
                if (size <= 0) break;
                sizeFilled += size;
            }


            Vector2 pos = m_Content.anchoredPosition;
            Vector2 movement = Vector2.zero;


            if (directionSign == 1)
            {

                //水平
                if (!_reverseDirection)
                {
                    //pos.y = m_Content.GetSize().y / 2f;
                    if (useAnim)
                    {
                        pos.x = this.rectTransform.GetSize().x + m_Content.GetChild(m_Content.childCount - 1).gameObject.GetRectTransform().GetSize().x * (3.5f / 5f);
                        movement = new Vector2(-m_Content.GetChild(m_Content.childCount - 1).gameObject.GetRectTransform().GetSize().x * 3, 0);
                    }
                    else
                    {
                        pos.x = this.rectTransform.GetSize().x;
                        movement = Vector2.zero;
                    }

                }
                else
                {
                    // pos.y = 0f;
                    if (useAnim)
                    {
                        pos.x = UnityEngine.Mathf.Max(-10f, -m_Content.GetChild(0).gameObject.GetRectTransform().GetSize().x / 2f);
                        movement = new Vector2(m_Content.GetChild(0).gameObject.GetRectTransform().GetSize().x, 0);
                    }
                    else
                    {
                        pos.x = 0;
                        movement = Vector2.zero;
                    }
                }

            }
            else if (directionSign == -1)
            {
                //垂直

                if (!_reverseDirection)
                {
                    if ((m_Content.rect.size.y + m_Content.GetChild(0).gameObject.GetRectTransform().GetSize().y) < rectTransform.rect.size.y)
                        useAnim = false;
                    // pos.x = 0f;
                    if (useAnim)
                    {
                        pos.y = UnityEngine.Mathf.Max(0, rectTransform.GetSize().y - m_Content.rect.size.y) - m_Content.GetChild(0).gameObject.GetRectTransform().GetSize().y * (3.5f / 5f);
                        movement = new Vector2(0, m_Content.GetChild(0).gameObject.GetRectTransform().GetSize().y * 2f);
                    }
                    else
                    {
                        pos.y = UnityEngine.Mathf.Max(0, rectTransform.GetSize().y - m_Content.rect.size.y);
                        movement = Vector2.zero;
                    }
                }
                else
                {
                    if ((m_Content.rect.size.y + m_Content.GetChild(0).gameObject.GetRectTransform().GetSize().y )< rectTransform.rect.size.y)
                        useAnim = false;
                    // pos.x = -m_Content.GetSize().x / 2f;
                    if (useAnim)
                    {
                        pos.y = UnityEngine.Mathf.Min(-rectTransform.rect.size.y, -m_Content.rect.size.y) + m_Content.GetChild(0).gameObject.GetRectTransform().GetSize().y * (3.5f / 5f);
                        movement = new Vector2(0, -m_Content.GetChild(0).gameObject.GetRectTransform().GetSize().y * 3f);
                    }
                    else
                    {
                        pos.y = UnityEngine.Mathf.Min(-rectTransform.rect.size.y, -m_Content.rect.size.y);
                        movement = Vector2.zero;
                    }
                }
            }


            if (useAnim)
            {
                m_Content.anchoredPosition = pos;
                m_Velocity = movement;
            }
            else
            {
                m_Content.anchoredPosition = pos;
                StopMovement();
            }


        }

        public bool IsAtEnd
        {
            get
            {
                return itemTypeEnd >= totalCount;
            }
        }
        public bool IsAtStart
        {
            get
            {
                return itemTypeStart <= 0;
            }
        }

        public int currStartIndex
        {
            get
            {
                return itemTypeStart;
            }
        }

        public int currEndIndex
        {
            get
            {
                return itemTypeEnd;
            }
        }

        public void ClearResource()
        {
            //return;
#if UNITY_EDITOR
            if (!childPrefab && !Application.isPlaying)
            {
                Log.LogError($"错误：{gameObject.name}:节点对象未赋值");
                return;
            }
#endif
            ClearCells();
            if (Application.isPlaying)
            {
                if (!childPrefab && content)
                {
                    if (content.childCount == 0)
                    {
                        var poolObj = GetItemObjectFromPool();
                        if (poolObj)
                        {
                            Log.LogWarning($"childPrefab 丢失，从pool找回！");
                            childPrefab = poolObj.gameObject;
                            childPrefab.gameObject.transform.SetParent(content, false);
                            //childPrefab.gameObject.gameObject.SetActive(true);//???
                            childPrefab.gameObject.SetActive(false);
                        }
                        else
                        {
                            Log.LogError($"content.childCount is null,and pool is null too");
                        }
                    }
                    else
                    {
                        childPrefab = content.GetChild(0).gameObject;
                        Log.LogWarning($"childPrefab 丢失，从child找回！");
                    }
                }
                SG.ResourceManager.Instance.DeletePool(pool_key);
            }

            while (content && content.childCount > 1)
            {
                var obj = content.GetChild(1);
                obj.SetParent(null);
#if UNITY_EDITOR
                if (Application.isPlaying)
                {
                    GameObject.Destroy(obj.gameObject);
                }
                else
                {
                    UnityEditor.EditorApplication.delayCall += () => { GameObject.DestroyImmediate(obj.gameObject); };
                }
#else
                GameObject.Destroy(obj.gameObject);
#endif
            }
            //Log.LogInfo($"LoopScrollRect ClearResource,{gameObject.name}");
            inited = false;
        }
        
        public bool IsInit(){
            return inited;
        }
    }
}