using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    /// <summary>
    /// 环形部局
    /// </summary>
    [AddComponentMenu("UI/CircleLayoutGroup环形部局")]
    [ExecuteInEditMode]
    public class CircleLayoutGroup : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {

        private int _innerNo;
        public int SelectedIndex
        {
            get
            {
                var itemcount = ItemCount;
                if (itemcount == 0) return 0;
                int ret = ((itemcount - _innerNo) + Mathf.CeilToInt(itemcount / 2f) - 1) % itemcount ;
                return ret;
            }
            set
            {
                int cnt = ItemCount;
                if (cnt == 0) return;
                _innerNo = (_innerNo + cnt - value) % cnt;                
            }
        }


        /// <summary>
        /// 半径大小
        /// </summary>
        public float Radius
        {
            get { return _radius; }
            set { _radius = value;  }
        }
        [HideInInspector] [SerializeField] private float _radius = 150f;


        /// <summary>
        /// 表示旋转的角度（0..360）
        /// 分配一个值会自动更新位置
        /// </summary>
        public float Angle
        {
            get { return _angle; }
            set
            {
                _angle = value;
                UpdatePos(_angle);
            }
        }
        [HideInInspector] private float _angle;


        /// <summary>
        /// 整体旋转角度
        /// </summary>
        public float ViewRotation
        {
            get { return viewRotation; }
            set { viewRotation = value; if (viewRotation == 0f) viewRotation = 1f; UpdatePos(_angle); }
        }

        [HideInInspector] [SerializeField] [Range(-180f,180f)] float viewRotation = -20f;

        /// <summary>
        /// 显示区域
        /// </summary>
        public float ViewAngle
        {
            get
            {
                return viewAngle;
            }
            set
            {
                viewAngle = value;
                UpdatePos(_angle);
            }
        }
        [HideInInspector] [SerializeField] [Range(1f, 359f)] float viewAngle = 210;


        public RectTransform Content
        {
            get
            {
                return content;
            }
            set
            {
                content = value;
                UpdatePos(_angle);
            }
        }

        [HideInInspector]
        [SerializeField]
        private RectTransform content;


        /// <summary>
        /// 选定的项目
        /// </summary>
        public GameObject SelectedItem
        {
            get
            {
                if (content == null) return null;
                if (content.childCount == 0) return null;
                int index = SelectedIndex;
                if (index < 0 || index >= content.childCount) return null;
                return content.GetChild(index).gameObject;
            }
        }

        /// <summary>
        /// 项目元素的数量.
        /// </summary>
        public int ItemCount
        {
            get
            {
                return content.childCount;
            }
        }

        public float SpringAmount
        {
            get { return springAmount; }
            set { springAmount = value; }
        }
        /// <summary>
        /// 拖动完成后移动到目标位置时的动画速度.(0..1)
        /// </summary>
        [HideInInspector] [SerializeField] [Range(0.01f, 1f)] float springAmount = 0.3f;

        public float SelectedScaleAmount
        {
            get { return selectedScaleAmount; }
            set { selectedScaleAmount = value; }
        }
        /// <summary>
        /// 所选项目的放大比率.
        /// </summary>
        [HideInInspector] [SerializeField] [Range(1f, 2f)] float selectedScaleAmount = 1.2f;

        private float MinDiffAngle = 0.03f;



        /// <summary>
        /// 是否被拖拽.
        /// </summary>
        public bool IsDraging
        {
            get { return isDraging; }
        }
        private bool isDraging;


        [ContextMenu("Next")]
        public void Next()
        {
            _innerNo = (_innerNo + ItemCount - 1) % ItemCount;
        }
        [ContextMenu("Prev")]
        public void Prev()
        {
            _innerNo = (_innerNo + 1) % ItemCount;
        }

        [ContextMenu("Focus")]
        public void Focus()
        {
            Focus(SelectedIndex);
        }
        /// <summary>
        /// 将指定索引的UI设置为顶部.
        /// </summary>
        public void Focus(int index)
        {
            if (index >= ItemCount) { index %= ItemCount; }
            while (index < 0) { index += ItemCount; }

            SelectedIndex = index;

            float angle = 0f;
            if (ItemCount != 0)
            {
                angle = viewAngle / ItemCount * index;
            }
            Angle = angle;

            if (SelectedItem != null && content.childCount > 0)
            {
                for(int i = 0; i < content.childCount; ++i)
                {
                    var item = content.GetChild(i).gameObject;
                    if (item == null) continue;
                    if (item == SelectedItem)
                    {
                        OnSelect(item);
                    }
                    else
                    {
                        OnInSelect(item);
                    }
                }
            }
        }

        void OnSelect(GameObject obj)
        {
            if (obj == null) return;

            obj.transform.localScale = Vector3.one * selectedScaleAmount;
            foreach (var btn in obj.GetComponentsInChildren<Selectable>(true))
            {
                btn.interactable = true;
            }
        }

        void OnInSelect(GameObject obj)
        {
            if (obj == null) return;

            obj.transform.localScale = Vector3.one;
            foreach (var btn in obj.GetComponentsInChildren<Selectable>(true))
            {
                btn.interactable = false;
            }
        }


        /// <summary>
        /// 根据更改角度更新位置
        /// </summary>
        private void UpdatePos(float diffAngle)
        {
            GameObject selectedItem = SelectedItem;
            if (content == null) return;
            for (int i = 0; i < content.childCount; i++)
            {  
                float angle = diffAngle + (viewAngle / ItemCount) * i;
                if (angle >= viewAngle) angle = angle % viewAngle;
                if (angle < 0f) angle = angle % viewAngle + viewAngle;

                GameObject current = content.GetChild(i).gameObject;

                angle += viewRotation;

                var canvas = GameObjectUtils.FindInParents<Canvas>(gameObject);

                float x = basePos.x + (Mathf.Sin(angle * Mathf.Deg2Rad) * Radius) ;
                float y = basePos.y + (Mathf.Cos(angle * Mathf.Deg2Rad) * Radius) ;
                //float z = current.transform.localPosition.z;

                current.transform.localPosition = new Vector3(x, y, 0) + (content.transform.position - transform.position);
                
                // Scale更新
                if (current == selectedItem)
                {
                    OnSelect(current);
                }
                else
                {
                    OnInSelect(current);
                }
            }
        }


#region Unity方法============================================================


#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            UpdatePos(_angle);
        }

#endif
        void Awake()
        {

        }

        void Start()
        {
            UpdatePos(_angle);
        }

        void Update()
        {
            if (isDraging) return;
            if (content == null ||  content.childCount == 0) return;

            // 当前角度和目标角度
            float currentAngle = _angle;
            float targetAngle = viewAngle / ItemCount * _innerNo;
            if (targetAngle >= viewAngle) targetAngle = targetAngle % viewAngle;
            if (targetAngle < 0f) targetAngle = targetAngle % viewAngle + viewAngle;

            if (currentAngle == targetAngle || targetAngle - currentAngle == viewAngle) return;// 不要以相同的角度做任何事情

            //// 处理0° - > 320°转换为360° - > 320°
            //if (currentAngle < 45 && targetAngle > 135)
            //{
            //    currentAngle += viewAngle;
            //}
            float diff = Mathf.Abs(targetAngle - currentAngle);

            if (diff < MinDiffAngle)
            {
                currentAngle = targetAngle;
            }
            else
            {
                if (diff >= viewAngle) { targetAngle = targetAngle % viewAngle; }
                currentAngle = targetAngle * springAmount + currentAngle * (1f - springAmount);
            }

            // 角度更新
            Angle = currentAngle;

            UpdatePos(_angle);
        }
#endregion ============================================================


        /// <summary>
        /// 中心位置
        /// </summary>
        private Vector2 basePos { get { return Vector2.zero; } }
        /// <summary>
        /// 拖动的起始位置
        /// </summary>
        private Vector2 dragStart;
        /// <summary>
        /// 拖动的结束位置
        /// </summary>
        private Vector2 dragEnd;

        private float startAndle;
        private float endAngle;

        private float GetAndle(Vector2 p1, Vector2 p2)
        {
            return -Mathf.Atan2(p1.y - p2.y, p1.x - p2.x);
        }

        private float DiffAngleDeg
        {
            get
            {
                float diffAngle = (endAngle - startAndle) * Mathf.Rad2Deg;
                if (diffAngle >= viewAngle) diffAngle = diffAngle % viewAngle;
                if (diffAngle < 0f) diffAngle += diffAngle % viewAngle + viewAngle;
                return diffAngle;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            isDraging = true;

            // 拖动开始位置
            dragStart = dragEnd = eventData.position;
            startAndle = GetAndle(basePos, dragStart);
            endAngle = GetAndle(basePos, dragEnd);

            prevAngle = endAngle;

            //		Debug.Log(string.Format("OnBeginDrag - Base:{0}\n Start:{1}\n End:{2}\n SRot:{3}\n ERot:{4}", basePos, dragStart, dragEnd, startAndle * Mathf.Rad2Deg, endAngle * Mathf.Rad2Deg));
        }

        private float prevAngle;
        public void OnDrag(PointerEventData eventData)
        {
            dragEnd = eventData.position;
            startAndle = GetAndle(basePos, dragStart);
            endAngle = GetAndle(basePos, dragEnd);

#if true
            // 位置更新
            UpdatePos(Angle + DiffAngleDeg);
#else
		// 位置更新
		float diff = (_angle + DiffAngleDeg) - prevAngle;
		_angle = _angle + diff;

		prevAngle = _angle + DiffAngleDeg;

		while (_angle > 360f) { _angle -= 360f; }
		while (_angle < 0f) { _angle += 360f; }
		UpdatePos(_angle);

		// 计算并设置最接近的索引
		float region = 360f / (ItemCount*2);
		int indexCnt = (int)(_angle / region);
		int nearIndex = indexCnt / 2 + ((indexCnt % 2)==0 ? 0 : 1);
		nearIndex = nearIndex % ItemCount;
		_index = nearIndex;
#endif

            //		Debug.Log(string.Format("OnDrag - Base:{0}\n Start:{1}\n End:{2}\n SRot:{3}\n ERot:{4}\n DiffAngle:{5}", basePos, dragStart, dragEnd, startAndle * Mathf.Rad2Deg, endAngle * Mathf.Rad2Deg, DiffAngleDeg));
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDraging = false;

            dragEnd = eventData.position;
            startAndle = GetAndle(basePos, dragStart);
            endAngle = GetAndle(basePos, dragEnd);

            // 位置更新
            _angle = _angle + DiffAngleDeg;
            if (_angle >= viewAngle) _angle = _angle % viewAngle;
            if (_angle < 0f) _angle = _angle % viewAngle + viewAngle;
            UpdatePos(_angle);

            // 计算并设置最接近的索引
            float region = viewAngle  / (ItemCount * 2) ;
            int indexCnt = (int)((_angle) / region);
            int nearIndex = indexCnt / 2 + ((indexCnt % 2) == 0 ? 0 : 1) ;
            nearIndex = nearIndex % ItemCount;
            _innerNo = nearIndex;

            
            //		Debug.Log(string.Format("OnEndDrag - Base:{0}\n Start:{1}\n End:{2}\n SRot:{3}\n ERot:{4}\n DiffAngle:{5}", basePos, dragStart, dragEnd, startAndle * Mathf.Rad2Deg, endAngle * Mathf.Rad2Deg, DiffAngleDeg));
        }
    }

}