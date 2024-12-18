using System;
using System.Collections.Generic;
using UnityEngine.Serialization;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    /// <summary>
    /// 一个看不见的UI区域
    /// </summary>
    [AddComponentMenu("UI/Empty Image", 10)]
    [RequireComponent(typeof(CanvasRenderer))]
    public class EmptyImage : Graphic, IBlackRangeClickHandler
    {        

        public override void Rebuild(CanvasUpdate update) { }

        #region 事件管理
        public void OnPointerClick(PointerEventData eventData)
        {
            if (Application.isPlaying && raycastTarget && !(gameObject.GetComponent<MyButton>() || gameObject.GetComponent<MyToggle>()))
            {
                SendMessageUpwards("_onClickOnBlank", this);
            }
        }
        #endregion
    }
}
