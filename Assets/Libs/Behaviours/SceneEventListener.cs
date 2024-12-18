using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneEventListener : MonoBehaviour {

    public Action<ScenarioBehaviour> EventOnEnterScene;
    public Action<ScenarioBehaviour> EventOnSceneEvent;
    public Action<ScenarioBehaviour.ScenarioTriggerParams> EventOnTriggerEnter;
    public Action<ScenarioBehaviour.ScenarioTriggerParams> EventOnTriggerExit;
    public Action<ScenarioBehaviour.ScenarioTriggerParams> EventOnTriggerStay;
    public Action<ScenarioBehaviour> EventOnPhaseComplete;

#if !DISABLE_TIMELINE
    public Action<TriggerTimelineClip> EventOnUITimelineEnter;
    public Action<TriggerTimelineClip> EventOnUITimelineExit;
    public Action<TriggerTimelineClip> EventOnUITimelineEnd;
    //public Action<FMODEventPlayableBehavior> EventOnTimelinePlaySound;
    //public Action<FMODEventPlayableBehavior> EventOnTimelineStopSound;
#endif

    void Start () {
		
	}
	
	
//	void Update () {
//		
//	}


    #region 来自场景物体的上抛事件

    void _onEnterScene(ScenarioBehaviour scenaroObj)
    {
        EventOnEnterScene?.Invoke(scenaroObj);
    }

    void _onSceneEvent(ScenarioBehaviour scenaroObj)
    {
        EventOnSceneEvent?.Invoke(scenaroObj);
    }

    void _onTriggerEnter(ScenarioBehaviour.ScenarioTriggerParams triggerObj)
    {
       // Log.LogError($"1111:_onTriggerEnter:{scenaroObj.name}");
        EventOnTriggerEnter?.Invoke(triggerObj);
    }
    void _onTriggerExit(ScenarioBehaviour.ScenarioTriggerParams triggerObj)
    {
        EventOnTriggerExit?.Invoke(triggerObj);
    }
    void _onTriggerStay(ScenarioBehaviour.ScenarioTriggerParams triggerObj)
    {
        EventOnTriggerStay?.Invoke(triggerObj);
    }
    void _onPhaseComplete(ScenarioBehaviour scenaroObj)
    {
        EventOnPhaseComplete?.Invoke(scenaroObj);
    }

    #endregion

#if !DISABLE_TIMELINE

    #region 来自剧情timeline调用

    void _onTimelineTriggerEnter(object p)
    {
        if (p is TriggerTimelineClip)
        {
            var triggerClip = p as TriggerTimelineClip;
            //Log.LogInfo($"_onTimelineTriggerEnter = {triggerClip.TriggerContent}");
            EventOnUITimelineEnter?.Invoke(triggerClip);
        }
    }

    void _onTimelineTriggerExit(object p)
    {
        if (p is TriggerTimelineClip)
        {
            var triggerClip = p as TriggerTimelineClip;
           // Log.LogInfo($"_onTimelineTriggerExit = {triggerClip.TriggerContent}");
            EventOnUITimelineExit?.Invoke(triggerClip);
        }
    }

    void _onTimelineTriggerEnd(object p)
    {
        if (p is TriggerTimelineClip)
        {
            var triggerClip = p as TriggerTimelineClip;
            //Log.LogInfo($"_onTimelineTriggerEnd = {triggerClip.TriggerContent}");
            EventOnUITimelineEnd?.Invoke(triggerClip);
        }
    }
    //已经改为在timeline内部播放，不在上发事件
    //void _onTimelineFmod(object p)
    //{
    //    if(p is FMODEventPlayableBehavior)
    //    {
    //        var fmodPlayer = p as FMODEventPlayableBehavior;
    //        EventOnTimelinePlaySound?.Invoke(fmodPlayer);
    //    }
    //}
    //已经改为在timeline内部播放，不在上发事件
    //void _onTimelineFmodEnd(object p)
    //{
    //    if (p is FMODEventPlayableBehavior)
    //    {
    //        var fmodPlayer = p as FMODEventPlayableBehavior;
    //        EventOnTimelineStopSound?.Invoke(fmodPlayer);
    //    }
    //}

    #endregion
#endif
}
