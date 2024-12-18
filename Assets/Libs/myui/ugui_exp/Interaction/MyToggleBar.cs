using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;

namespace UnityEngine.UI
{
    public class MyToggleBar : MyHorizontalOrVerticalLayoutGroup
    {
        public enum EffectType
        {
            effect = 1,
            anim,
        }

        public class ToggleInfo
        {
            public int id;
            public int sort_id;
            public EmptyImage rootRect;
            public MyToggle toggle;
            public bool isClickEnable;
            public bool visible;
            public bool isRemoved;
            public GameObject effect;

            public Action<MyToggle> callback;
            public virtual void InitCallBack(ToggleInfo toggle) { }
            public virtual void OnChange(MyToggle toggle) { callback?.Invoke(toggle); }
            public virtual void OnEffectShow() { }
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

        private List<ToggleInfo> _toggle_infos = new List<ToggleInfo>();
        bool _isStart;

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

        public List<ToggleInfo> ToggleList
        {
            get
            {
                return _toggle_infos;
            }
        }


        public override void CalculateLayoutInputVertical()
        {
            CalcAlongAxis(1, _isVertical);
        }

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            CalcAlongAxis(0, _isVertical);
        }

        public override void SetLayoutHorizontal()
        {
            SetChildrenAlongAxis(0, _isVertical);
        }

        public override void SetLayoutVertical()
        {
            SetChildrenAlongAxis(1, _isVertical);
        }

        protected override void Awake()
        {
            base.Awake();
            Start();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        public void InitToggleLists(List<ToggleInfo> toggleInfoList)
        {
            var names = toggleInfoList.Select(item => $"toggle_{item.id}").ToList();
            InitChildren(names, toggleInfoList.ToList<object>());
            _isStart = false;
            Start();
        }

        protected override void Start()
        {
            if (_isStart) return;
            _isStart = true;

            base.Start();

            _toggle_infos.Clear();

            for (int i = 0; i < gameObject.transform.childCount; ++i)
            {
                var go = gameObject.transform.GetChild(i).gameObject;
                ToggleInfo toggleInfo = go.GetParam() as ToggleInfo;
                if (toggleInfo != null)
                {
                    if (go.GetComponent<EmptyImage>() && go.GetComponentInChildren<MyToggle>())
                    {

                        toggleInfo.rootRect = go.GetComponent<EmptyImage>();
                        toggleInfo.toggle = go.GetComponentInChildren<MyToggle>();
                        toggleInfo.sort_id = i;

                        // if (go.GetComponentInChildren<MyUI3DObject>())
                        //     btnInfo.effect_go = go.GetComponentInChildren<MyUI3DObject>().gameObject;


                    }
                    else if (!go.GetComponent<EmptyImage>() && go.GetComponent<MyToggle>())
                    {
                        toggleInfo.rootRect = null;
                        toggleInfo.toggle = go.GetComponent<MyToggle>();
                        toggleInfo.sort_id = i;
                        // if (go.GetComponentInChildren<MyUI3DObject>())
                        //     btnInfo.effect_go = go.GetComponentInChildren<MyUI3DObject>().gameObject;

                    }
                    _toggle_infos.Add(toggleInfo);
                }
            }

            InitToggleList();
        }

        public void AddToggle(ToggleInfo info,GameObject go = null)
        {
            if (info.isRemoved) return;
            if (_toggle_infos.Contains(info))
            {
                UpdateToggle(info);
                return;
            }

            if (!go)
            {
                var newitem = AddChildItem();
                go = newitem.gameObject;
                go.SetParam(info);
            }
            
            if (go.GetComponent<EmptyImage>() && go.GetComponentInChildren<MyToggle>())
            {
                info.rootRect = go.GetComponent<EmptyImage>();
                info.toggle = go.GetComponentInChildren<MyToggle>();
            }
            else if (!go.GetComponent<EmptyImage>() && go.GetComponent<MyToggle>())
            {
                info.rootRect = null;
                info.toggle = go.GetComponent<MyToggle>();
            }
            _toggle_infos.Add(info);

            InitToggleList();
        }

        public void UpdateToggle(ToggleInfo info)
        {
            if (info.isRemoved) return;

            if (_toggle_infos.Contains(info))
            {
                info.InitCallBack(info);
            }
            else
            {
                AddToggle(info);
            }
        }

        public void RemoveToggle(ToggleInfo info)
        {
            if (info.isRemoved) return;
            info.rootRect.gameObject.SetParam(null);
            info.toggle.SetParam(null);
            RemoveChildItem(info.rootRect.gameObject);
            _toggle_infos.Remove(info);
            info.isRemoved = false;
        }

        public void InitToggleList()
        {
            for (int i = 0; i < _toggle_infos.Count; i++)
            {
                if (!_toggle_infos[i].rootRect)
                {
                    _toggle_infos[i].rootRect = GameObjectUtils.AddChild<EmptyImage>(gameObject);
                    _toggle_infos[i].toggle.transform.SetParent(_toggle_infos[i].rootRect.transform, true);
                    _toggle_infos[i].rootRect.rectTransform.SetSize(_toggle_infos[i].toggle.targetGraphic.rectTransform.GetSize());
                }

                _toggle_infos[i].toggle.SetParam(_toggle_infos[i]);
                _toggle_infos[i].toggle.Toggleber = this;
                _toggle_infos[i].toggle.group = _toggle_infos[i].toggle.GetComponentInParent<MyToggleGroup>();

                var rt = _toggle_infos[i].toggle.targetGraphic.rectTransform;
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(0, 0);
            }
        }

        public void OnChange(MyToggle toggle)
        {
            var info = toggle.GetParam() as ToggleInfo;

            if (info == null) return;

            if (info.isRemoved) return;

            if (!info.isClickEnable) return;

            info.OnChange(toggle);
        }

    }
}


