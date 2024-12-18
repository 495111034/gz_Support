using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


/// <summary>
/// 作者：赵青青(569032731@qq.com)
/// 时间：2019/4/9 13:51:20
/// 说明：
/// </summary>
public class VoiceButton : UIBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IPointerLongpressHandler
{
    public PointerEventData pressingEventData => null;
    bool _is_down = false;
    Vector2 _start_pos;

    /// <summary>
    /// 当滑动
    /// </summary>
    public Action<VoiceButton,float> OnDragDir;
    public Action<VoiceButton, PointerEventData> OnDragEvent;

    /// <summary>
    /// 点击和抬起
    /// </summary>
    public Action<VoiceButton,bool, PointerEventData> OnPointer;


    //
    public void OnPointerDown(PointerEventData eventData)
    {
        _is_down = true;
        _start_pos = eventData.position;
//        Log.LogInfo("OnPointerDown");
        OnPointer?.Invoke(this,true, eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _is_down = false;
        //Log.LogInfo("OnPointerUp");
        OnPointer?.Invoke(this,false, eventData);
        CheckDistance(eventData);
    }

    public void CheckDistance(PointerEventData eventData)
    {
        float dy = _start_pos.y - eventData.position.y;
        if (dy < 0)
        {
            dy = -dy;
        }
    }

    public void OnLongPressRepeat(PointerEventData eventData)
    {
        if (Application.isPlaying)
        {
//            Log.LogInfo("OnLongPressRepeat");
            CheckDistance(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        OnDragEvent?.Invoke(this,eventData);
        if (!_is_down)
        {
            return;
        }
        
        float dy = _start_pos.y - eventData.position.y;
        if (dy < 0)
        {
            dy = -dy;
        }

        OnDragDir?.Invoke(this,dy);
    }
}