using System.Collections.Generic;
using System.Collections;

namespace UnityEngine.UI
{
    public abstract class MyHorizontalOrVerticalLayoutGroup : HorizontalOrVerticalLayoutGroup
    {
        [HideInInspector]
        [SerializeField]
        bool autosize = false;

        public bool AutoSize { get { return autosize; } set { autosize = value; SetDirty(); } }
        public List<RectTransform> Children { get { return rectChildren; } }

        protected RectTransform _seed = null;

        //子项动画标记
        bool m_has_item_tween = false;

        EffectPanelConfig _config;

        MyTask m_tween_check_task;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (Application.isPlaying)
                _config = gameObject.GetComponent<EffectPanelConfig>();

            m_has_item_tween = false;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            m_has_item_tween = false;
        }

        public RectTransform AddChildItem(string name = null, object param = null)
        {
            if (rectChildren.Count == 0)
                CalculateLayoutInputHorizontal();
            RectTransform newItem = null;
            if (rectChildren.Count == 0)
            {
                if (_seed)
                {
                    _seed.gameObject.SetActive(true);
                    newItem = _seed;
                    _seed = null;
                }
                else
                {
                    Log.LogError("not seek,cannt add new item");
                    newItem = GameObjectUtils.AddChild<RectTransform>(gameObject);
                }

            }
            else
            {
                var go = GameObject.Instantiate(rectChildren[rectChildren.Count - 1].gameObject, gameObject.GetRectTransform());
                var itws = go.GetComponentsInChildren<iTween>();
                for (int j = 0; j < itws.Length; ++j)
                {
                    GameObject.Destroy(itws[j]);
                }
                newItem = go.GetRectTransform();
            }

            if (!string.IsNullOrEmpty(name))
            {
                newItem.gameObject.name = name;
            }
            newItem.gameObject.SetParam(param);

            SetDirty();

            return newItem;
        }

        public void RemoveChildItem(GameObject waitRemove)
        {
            if (rectChildren.Count == 0)
                CalculateLayoutInputHorizontal();
            if (!waitRemove) return;
            for (int i = 0; i < rectChildren.Count; ++i)
            {
                if (rectChildren[i].gameObject == waitRemove)
                {
                    // Log.LogError($"current obj name:{rectChildren[i].gameObject.name},wait delete name:{waitRemove.name}");
                    if (rectChildren.Count == 1)
                    {
                        _seed = waitRemove.GetRectTransform();
                        _seed.gameObject.SetParam(null);
                        waitRemove.SetActive(false);
                    }
                    else
                    {
                        rectChildren.Remove(waitRemove.GetRectTransform());
                        GameObject.Destroy(waitRemove);
                    }
                    SetDirty();
                    return;
                }
            }

            Log.LogError("cannt found wait to remove object");
        }

        public virtual void InitChildren(IList<string> names, List<object> paramList = null, System.Action<int,RectTransform> initItemCallback = null)
        {
            if (rectChildren.Count == 0)
                CalculateLayoutInputHorizontal();
            _initChildCount(names != null && names.Count > 0 ? names.Count : 0);
            //if((names != null && m_RectChildren.Count != names.Count) || (names == null && m_RectChildren.Count != 0))
            //{
            //    Log.LogError($"error:{m_RectChildren} != {names.Count}");
            //    return;
            //}
            for (int i = 0; i < rectChildren.Count; ++i)
            {
                rectChildren[i].gameObject.name = names[i];
                if(paramList!= null && paramList.Count > i)
                {
                    rectChildren[i].gameObject.SetParam(paramList[i]);
                }
                else
                {
                    rectChildren[i].gameObject.SetParam(null);
                }

                if (_config != null && !m_has_item_tween)
                {
                    if (_config._listTween_1 || _config._listTween_2)
                    {
                        if (_config._listTween_1)
                            rectChildren[i].gameObject.transform.localScale = Vector3.zero;
                        else
                            rectChildren[i].gameObject.transform.localScale = new Vector3(1, 0, 1);

                        initItemCallback?.Invoke(i, rectChildren[i]);

                        var list_tween_delay = _config._list_tween_delay;

                        Hashtable hash = new Hashtable();
                        if (_config._listTween_2)
                        {
                            hash.Add("y", 1);
                            hash.Add("easetype", _config._appearEaseType);
                            hash.Add("time", _config._list_tween_dur);
                            hash.Add("delay", 0.2f * (i + 1) + list_tween_delay);
                            iTween.ScaleTo(rectChildren[i].gameObject, hash);

                            StartCoroutine(ResetScale(rectChildren[i].gameObject.GetRectTransform(), 0.8f * (i + 1) + list_tween_delay));
                        }
                        else if (_config._listTween_1)
                        {
                            StartCoroutine(ResetScale(rectChildren[i].gameObject.GetRectTransform(), _config._list_tween_dur + 0.1f * i + list_tween_delay));

                        }
                    }

                    if (m_tween_check_task != null && m_tween_check_task.IsRunning)
                        m_tween_check_task.Stop();

                    m_tween_check_task = MyTask.RunTask(TweenEndCheck()); ;
                }
                else
                {
                    initItemCallback?.Invoke(i, rectChildren[i]);
                }
            }
            initItemCallback = null;
        }

        IEnumerator ResetScale(RectTransform trans,float wait)
        {
            yield return new WaitForSeconds(wait);

            if (!trans)
                yield break;

            trans.localScale = Vector3.one;
        }

        IEnumerator TweenEndCheck()
        {
            yield return 1000;
            m_has_item_tween = true;
        }

        public virtual void InitChildren(int count, System.Action<int, RectTransform> initItemCallback = null)
        {
            if (rectChildren.Count == 0)
                CalculateLayoutInputHorizontal();
            _initChildCount(count);

            for (int i = 0; i < rectChildren.Count; ++i)
            {
                if (_config != null && !m_has_item_tween)
                {
                    if (_config._listTween_1 || _config._listTween_2)
                    {
                        if (_config._listTween_1)
                            rectChildren[i].gameObject.transform.localScale = Vector3.zero;
                        else
                            rectChildren[i].gameObject.transform.localScale = new Vector3(1, 0, 1);

                        initItemCallback?.Invoke(i, rectChildren[i]);

                        Hashtable hash = new Hashtable();

                        var list_tween_delay = _config._list_tween_delay;

                        if (_config._listTween_2)
                        {
                            hash.Add("y", 1);
                            hash.Add("easetype", _config._appearEaseType);
                            hash.Add("time", _config._list_tween_dur);
                            hash.Add("delay", 0.2f * (i + 1) + list_tween_delay);
                            iTween.ScaleTo(rectChildren[i].gameObject, hash);

                            StartCoroutine(ResetScale(rectChildren[i].gameObject.GetRectTransform(), 0.8f * (i + 1) + list_tween_delay));
                        }
                        else if (_config._listTween_1)
                        {
                            StartCoroutine(ResetScale(rectChildren[i].gameObject.GetRectTransform(), _config._list_tween_dur + 0.1f * i + list_tween_delay));

                        }
                    }

                    if (m_tween_check_task != null && m_tween_check_task.IsRunning)
                        m_tween_check_task.Stop();

                    m_tween_check_task = MyTask.RunTask(TweenEndCheck()); ;
                }
                else
                {
                    initItemCallback?.Invoke(i, rectChildren[i]);
                }

            }
            initItemCallback = null;
        }

        void _initChildCount(int num)
        {
            if (num < rectChildren.Count)
            {
                if (num == 0)
                {
                    if (rectChildren.Count > 0)
                    {
                        _seed = rectChildren[0];
                        _seed.gameObject.SetActive(false);

                        for (int i = 1; i < rectChildren.Count; ++i)
                        {
                            GameObject.Destroy(rectChildren[i].gameObject);
                        }
                    }

                    rectChildren.Clear();
                }
                else
                {
                    for (int i = num; i < rectChildren.Count; ++i)
                    {
                        GameObject.Destroy(rectChildren[i].gameObject);
                    }

                    rectChildren.RemoveRange(num, rectChildren.Count - num);
                }

            }

            var n = rectChildren.Count;
            for (int i = n; i < num; ++i)
            {
                RectTransform newItem = null;
                if (rectChildren.Count == 0)
                {
                    if (_seed)
                    {
                        _seed.gameObject.SetActive(true);
                        newItem = _seed;
                        _seed = null;
                    }
                    else
                    {
                        Log.LogError("not seek,cannt add new item");
                        newItem = GameObjectUtils.AddChild<RectTransform>(gameObject);
                    }
                }
                else
                {
                    var go = GameObject.Instantiate(rectChildren[rectChildren.Count - 1].gameObject, gameObject.GetRectTransform());
                    var itws = go.GetComponentsInChildren<iTween>();
                    for (int j = 0; j < itws.Length; ++j)
                    {
                        GameObject.Destroy(itws[j]);
                    }
                    newItem = go.GetRectTransform();
                }

                rectChildren.Add(newItem);
            }

        }


        /// <summary>
        ///  设置当前的轴向孩子的位置和尺寸
        /// </summary>
        /// <param name="axis"> 
        /// 轴向
        /// 0 : horizontal,x轴 ; 
        /// 1 : vertical,y轴.</param>
        /// <param name="isVertical">layoutgroup类型,是否纵向layout</param>
        protected new void SetChildrenAlongAxis(int axis, bool isVertical)
        {
            float size = rectTransform.rect.size[axis];
            bool childControlSize = (axis != 0) ? m_ChildControlHeight : m_ChildControlWidth;
            bool childForceExpand = (axis != 0) ? childForceExpandHeight : childForceExpandWidth;
            float alignmentOnAxis = GetAlignmentOnAxis(axis);
            bool alongOtherAxis = isVertical ^ axis == 1;
            bool reverse = isVertical ? childAlignment >= TextAnchor.LowerLeft && childAlignment <= TextAnchor.LowerRight : childAlignment == TextAnchor.LowerRight || childAlignment == TextAnchor.MiddleRight || childAlignment == TextAnchor.UpperRight;
            //排列非控制轴
            if (alongOtherAxis)
            {
                float innerSize = size - (float)((axis != 0) ? base.padding.vertical : base.padding.horizontal);                
                for (int i = 0; i < rectChildren.Count; ++i)
                {
                    RectTransform child = rectChildren[i];
                    float min, preferred, flexible;
                    GetChildSizes(child, axis, childControlSize, childForceExpand, out min, out preferred, out flexible);
                    float requiredSpace = Mathf.Clamp(innerSize, min, (flexible <= 0f) ? preferred : size);
                    float startOffset = GetStartOffset(axis, requiredSpace);
                    
                    if (childControlSize)
                    {
                        SetChildAlongAxis(child, axis, startOffset, requiredSpace);
                    }
                    else
                    {
                        float posInc = (requiredSpace - child.sizeDelta[axis]) * alignmentOnAxis;
                        SetChildAlongAxis(child, axis, startOffset + posInc);                                            
                    }
                }
                
            }
            else
            {
                float pos; 
                if (axis == 0 && childAlignment == TextAnchor.UpperRight)
                {
                    pos = size;
                }
                else 
                {
                    pos = (float)((axis != 0) ? base.padding.top : base.padding.left);
                }
                if (GetTotalFlexibleSize(axis) == 0f && GetTotalPreferredSize(axis) < size)
                {
                    pos = GetStartOffset(axis, base.GetTotalPreferredSize(axis) - (float)((axis != 0) ? padding.vertical : padding.horizontal));
                }
                float minMaxLerp = 0f;
                if (GetTotalMinSize(axis) != GetTotalPreferredSize(axis))
                {
                    minMaxLerp = Mathf.Clamp01((size - GetTotalMinSize(axis)) / (GetTotalPreferredSize(axis) - GetTotalMinSize(axis)));
                }
                float itemFlexibleMultiplier = 0f;
                if (size > GetTotalPreferredSize(axis))
                {
                    if (GetTotalFlexibleSize(axis) > 0f)
                    {
                        itemFlexibleMultiplier = (size - GetTotalPreferredSize(axis)) / GetTotalFlexibleSize(axis);
                    }
                }
                for (int j = 0; j < rectChildren.Count; ++j)
                {
                    RectTransform child = rectChildren[j];
                    float min, preferred, flexible;
                    GetChildSizes(child, axis, childControlSize, childForceExpand, out min, out preferred, out flexible);
                    float childSize = Mathf.Lerp(min, preferred, minMaxLerp);
                    childSize += flexible * itemFlexibleMultiplier;
                    if (j == 0)
                    {
                        if (axis == 0 && childAlignment == TextAnchor.UpperRight)
                        {
                            pos -= childSize + spacing;
                        }
                    }
                    if (childControlSize)
                    {
                        SetChildAlongAxis(child, axis, pos, childSize);
                    }
                    else
                    {
                        float posInc = (childSize - child.sizeDelta[axis]) * alignmentOnAxis;
                        SetChildAlongAxis(child, axis, pos + posInc);
                    }
                    if(reverse) pos -= childSize + spacing;
                    else
                        pos += childSize + spacing;
                }
            }
        }
     

        private void GetChildSizes(RectTransform child, int axis, bool controlSize, bool childForceExpand, out float min, out float preferred, out float flexible)
        {
            if (!controlSize)
            {
                min = child.sizeDelta[axis];
                preferred = min;
                flexible = 0f;
            }
            else
            {
                min = LayoutUtility.GetMinSize(child, axis);
                preferred = LayoutUtility.GetPreferredSize(child, axis);
                flexible = LayoutUtility.GetFlexibleSize(child, axis);
            }
            if (childForceExpand)
            {
                flexible = Mathf.Max(flexible, 1f);
            }
        }

        /// <summary>
        /// 计算当前方向首个元素的位置
        /// </summary>
        /// <param name="axis">方向, 0 水平,x轴;  1 纵向,y轴</param>
        /// <param name="requiredSpaceWithoutPadding">当前轴向所有元素(不包括间距)的总尺寸</param>
        /// <returns>
        ///   <para>当前轴向首个元素的位置</para>
        /// </returns>
        protected new float GetStartOffset(int axis, float requiredSpaceWithoutPadding)
        {
            float requiredSpace = requiredSpaceWithoutPadding + (float)((axis != 0) ? this.padding.vertical : this.padding.horizontal);
            float availableSpace = rectTransform.rect.size[axis];
            float surplusSpace = availableSpace - requiredSpace;
            float alignmentOnAxis = GetAlignmentOnAxis(axis);
            return (float)((axis != 0) ? padding.top : this.padding.left) + surplusSpace * alignmentOnAxis;
        }

    }
}
