using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[System.Serializable]
public class TriggerPlayableData: PlayableBehaviour
{
    public TimelineClip OwningClip;
    public GameObject targetObject;
    public TriggerTimelineClip tirggerTimelineClip;
    
    [HideInInspector]
    public TriggerTrack parentTrack;
    
    public override void OnPlayableCreate(Playable playable)
    {
        var duration = playable.GetDuration();
        if (Mathf.Approximately((float)duration, 0))
        {
            throw new UnityException("A Clip Cannot have a duration of zero");
        }
    }

    private bool m_isFirstFrameProcess = false;
    
    public override void OnGraphStart(Playable playable)
    {
        m_isFirstFrameProcess = false;
        base.OnGraphStart(playable);
    }

    public void UpdateBehaviour(float time,int cur_idx,int total_clip)
    {
        if ((time >= OwningClip.start) && (time < OwningClip.end))
        {
            OnEnter();
        }
        else
        {
            OnExit(cur_idx,total_clip);
        }
    }

    public override void OnGraphStop(Playable playable)
    {
        base.OnGraphStop(playable);
        if (targetObject)
        {
            //targetObject.SendMessage("_onTimelineTriggerExit", tirggerTimelineClip);
            targetObject.SendMessage("_onTimelineTriggerEnd", tirggerTimelineClip);
        }
    }

    private void OnEnter()
    {
        if (!m_isFirstFrameProcess)
        {
            //Log.LogInfo($"触发tigger:{tirggerTimelineClip.TriggerContent}");

            // Log.LogError($"{(targetObject? targetObject.name:"NULL")} to notify");
            if (targetObject)
                targetObject.SendMessage("_onTimelineTriggerEnter", tirggerTimelineClip);
            m_isFirstFrameProcess = true;
        }
    }
    
    
    private void OnExit(int cur_idx,int total_clip)
    {
        if (m_isFirstFrameProcess)
        {
            //Log.LogInfo(
            //    $"离开trigger clip:{tirggerTimelineClip.TriggerContent}，当前clip idx:{cur_idx}，总共clip:{total_clip}");

            if (targetObject)
                targetObject.SendMessage("_onTimelineTriggerExit", tirggerTimelineClip);

            m_isFirstFrameProcess = false;
        }
    }
}
