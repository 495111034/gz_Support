using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using UnityEngine.EventSystems;

public class IUIEvent : MonoBehaviour
{
    public Action EventOnEnable;
    public event Action EventOnDisable;
    public Action EventOnDestroy;
    public Action<MonoBehaviour> EventOnClick;
    public Action<MonoBehaviour> EventOnResize;
    //public Action<MonoBehaviour> EventOnPressStart;
    //public Action<MonoBehaviour> EventOnPressEnd;
    public Action<IPointerLongpressHandler> Event__OnLongPress;
    public Action<MonoBehaviour> Event__OnEffectCallBack;
    public Action<MonoBehaviour> Event__3dObjOnInit;
    public Action<MonoBehaviour> Event__3dObjLoadComplete;
    public Action<MonoBehaviour> Event__onValueChange;
    public Action<MonoBehaviour> Event__OnInputEditEnd;
    public Action<GameObject> Event__OnClickByUIRoomObject;
    public Action<BundleRequestInfo> EventRequestBundle;
    public Action<MonoBehaviour> EventUIRoomLoadComplete;
    public Action<MonoBehaviour> EventMySpReqKeyFrame;
    public Action<MonoBehaviour> EventMySpReqPlayComplete;
    public Action<string> Event__OnPlaySound;
    public Action<MyImageText.HrefInfo> Event__OnHrefClick;
    public Action<MonoBehaviour> EventOnScrollRectChanged;
    public Action<MonoBehaviour> EventOnScrollRectEndDrag;
    public Action<MonoBehaviour> EventOnSelected;
    public Action<MonoBehaviour> EventOnClickBlank;
    public Action<PointerEventData> EventOnPointerUp;
    public Action<IPointerLongpressHandler> EventOnPointerDown;
    public Action<IPointerLongpressHandler> EventOnGuidePointerDown;
    public Action<PointerEventData> Event__OnDrag;
    public Action<bool> EventFocus;
    public Action<MonoBehaviour> EventOnFuncOpen_Init;
    public Action<PointerEventData> Event__OnBeginDrag;
    public Action<PointerEventData> Event__OnDrop;
    public Action<PointerEventData> Event__OnEndDrag;
    public Action<MonoBehaviour> Event__OnTextEffectFinish;


    /// <summary>
    /// 焦点丢失或获取
    /// </summary>
    /// <param name="hasFocus"></param>
    void OnApplicationFocus(bool hasFocus)
    {
        EventFocus?.Invoke(hasFocus);
    }

    void _invoke<T>(string opcode, Action<T> action, T p)
    {
        if (action != null)
        {
            MyTask.Last_opcode = opcode;
            var t1 = Time.realtimeSinceStartup;
            UnityEngine.Profiling.Profiler.BeginSample(opcode);
            action.Invoke(p);
            UnityEngine.Profiling.Profiler.EndSample();
            var cost = (Time.realtimeSinceStartup - t1) * 1000;
            if (cost > 20)
            {
                if (!BuilderConfig.IsDebugBuild)
                {
                    Log.Log2File($"ui:{opcode} slow, cost={cost}ms, action={action.Target}.{action.Method}");
                }
            }
            MyTask.Last_opcode = null;
        }
    }

    void OnClickEvent(MonoBehaviour behaviour) { _invoke("OnClickEvent", EventOnClick, behaviour); }
    void OnResize(MonoBehaviour behaviour) { _invoke("EventOnResize", EventOnResize, behaviour); }
    //void OnPressStart(MonoBehaviour behaviour){ MyTask.Last_opcode = "OnPressStart"; EventOnPressStart?.Invoke(behaviour); }
    //void OnPressEnd(MonoBehaviour behaviour) { MyTask.Last_opcode = "OnPressEnd"; EventOnPressEnd?.Invoke(behaviour); }
    void __OnLongPress(IPointerLongpressHandler behaviour) { _invoke("Event__OnLongPress", Event__OnLongPress, behaviour);}
    void __OnEffectCallBack(MonoBehaviour behaviour) { _invoke("Event__OnEffectCallBack", Event__OnEffectCallBack, behaviour);}
    void __3dObjOnInit(MonoBehaviour behaviour) { _invoke("Event__3dObjOnInit", Event__3dObjOnInit, behaviour);}
    void __onValueChange(MonoBehaviour behaviour) { _invoke("Event__onValueChange", Event__onValueChange, behaviour); }
    void __OnInputEditEnd(MonoBehaviour behaviour) { _invoke("Event__OnInputEditEnd", Event__OnInputEditEnd, behaviour); }
    void __OnClickByUIRoomObject(GameObject obj) { _invoke("Event__OnClickByUIRoomObject", Event__OnClickByUIRoomObject, obj); }
    void RequestBundle(BundleRequestInfo param) { _invoke("EventRequestBundle", EventRequestBundle, param);}
    void UIRoomLoadComplete(MonoBehaviour behaviour) { _invoke("EventUIRoomLoadComplete", EventUIRoomLoadComplete, behaviour);}
    void __OnPlaySound(string sound_id) { _invoke("Event__OnPlaySound", Event__OnPlaySound, sound_id);}
    void __OnMySpriteSeqKeyFrame(MonoBehaviour behaviour) { _invoke("EventMySpReqKeyFrame", EventMySpReqKeyFrame, behaviour);}
    void __OnMySpriteSeqComplete(MonoBehaviour behaviour) { _invoke("EventMySpReqPlayComplete", EventMySpReqPlayComplete, behaviour);}
    void __OnSelected(MonoBehaviour behaviour) { _invoke("EventOnSelected", EventOnSelected, behaviour);}
    void OnHrefClickEvent(MyImageText.HrefInfo href) { _invoke("Event__OnHrefClick", Event__OnHrefClick, href);}
    void _onScrollRectValueChanged(MonoBehaviour behaviour) { _invoke("EventOnScrollRectChanged", EventOnScrollRectChanged, behaviour); }
    void _onScrollRectEndDrag(MonoBehaviour behaviour) { _invoke("EventOnScrollRectEndDrag", EventOnScrollRectEndDrag, behaviour); }
    void _onClickOnBlank(MonoBehaviour behaviour) { _invoke("EventOnClickBlank", EventOnClickBlank, behaviour); }
    void __3dObjLoadComplete(MonoBehaviour behaviour) { _invoke("Event__3dObjLoadComplete", Event__3dObjLoadComplete, behaviour);}
    void __OnPointerUp(PointerEventData eventData) { _invoke("EventOnPointerUp", EventOnPointerUp, eventData); }
    void __OnPointerDown(IPointerLongpressHandler behaviour) { _invoke("EventOnPointerDown", EventOnPointerDown, behaviour);}
    void __OnGuidePointerDown(IPointerLongpressHandler behaviour) { _invoke("EventOnGuidePointerDown", EventOnGuidePointerDown, behaviour); }
    void __OnDrag(PointerEventData eventData) { _invoke("Event__OnDrag", Event__OnDrag, eventData);}
    void __FuncOpenCompInit(MonoBehaviour behaviour) { _invoke("EventOnFuncOpen_Init", EventOnFuncOpen_Init, behaviour); }
    void __OnBeginDrag(PointerEventData eventData) { _invoke("Event__OnBeginDrag", Event__OnBeginDrag, eventData); }
    void __OnDrop(PointerEventData eventData) { _invoke("Event__OnDrop", Event__OnDrop, eventData); }
    void __OnEndDrag(PointerEventData eventData) { _invoke("Event__OnEndDrag", Event__OnEndDrag, eventData); }
    void __OnTextEffectFinish(MonoBehaviour behaviour) { _invoke("Event__OnTextEffectFinish", Event__OnTextEffectFinish, behaviour); }


    ///////
    void OnEnable() { MyTask.Last_opcode = "OnEnable"; EventOnEnable?.Invoke(); }
    void OnDisable() { MyTask.Last_opcode = "OnDisable"; EventOnDisable?.Invoke(); }
    void OnDestroy()
    {
        MyTask.Last_opcode = "OnDestroy";
        EventOnDestroy?.Invoke();
        EventOnEnable = null;
        EventOnDisable = null;
        EventOnDestroy = null;
        EventOnClick = null;
        EventOnResize = null;
        //EventOnPressStart = null;
        //EventOnPressEnd = null;
        Event__OnLongPress = null;
        Event__OnEffectCallBack = null;
        Event__3dObjOnInit = null;
        Event__3dObjLoadComplete = null;
        Event__onValueChange = null;
        Event__OnInputEditEnd = null;
        Event__OnClickByUIRoomObject = null;
        EventRequestBundle = null;
        EventUIRoomLoadComplete = null;
        EventMySpReqKeyFrame = null;
        EventMySpReqPlayComplete = null;
        Event__OnPlaySound = null;
        Event__OnHrefClick = null;
        EventOnScrollRectChanged = null;
        EventOnScrollRectEndDrag = null;
        EventOnSelected = null;
        EventOnClickBlank = null;
        EventOnPointerUp = null;
        EventOnPointerDown = null;
        EventFocus = null;
        EventOnFuncOpen_Init = null;
        Event__OnBeginDrag = null;
        Event__OnDrop = null;
        Event__OnEndDrag = null;
        Event__OnTextEffectFinish = null;
    }

}

