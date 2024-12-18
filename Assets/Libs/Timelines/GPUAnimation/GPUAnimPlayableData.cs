using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[System.Serializable]
public class GPUAnimPlayableData: PlayableBehaviour
{
    [HideInInspector]
    public GPUAnimationTrack parentTrack;
    
    public ObjectBehaviourBase TrackTargetObject;
    
    [HideInInspector]
    private bool m_isFirstFrameProcess = true;
    [HideInInspector]
    private ObjectBehaviourBase objBase;
    
    public TimelineClip OwningClip;
    public GPUAnimTimelineClip GpuTimelineClip;

    public long FightId { get; set; }


    public override void OnPlayableCreate(Playable playable)
    {
        var duration = playable.GetDuration();
        if (Mathf.Approximately((float)duration, 0))
        {
            throw new UnityException("A Clip Cannot have a duration of zero");
        }
    }
    
    public override void OnGraphStart(Playable playable)
    {
        m_isFirstFrameProcess = false;
        base.OnGraphStart(playable);
    }
    
    public override void OnGraphStop(Playable playable)
    {
        base.OnGraphStop(playable);
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        base.OnBehaviourPlay(playable, info);
        
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        base.OnBehaviourPause(playable, info);
    }

    public override void OnBehaviourDelay(Playable playable, FrameData info)
    {
        base.OnBehaviourDelay(playable, info);
        
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (playerData is null) return;

        if (!(playerData is ObjectBehaviourBase))
        {
            Log.LogError($"clip ProcessFrame:playerdata is not GPUObjectGroup:{playerData?.GetType()}");
            return;
        }

        objBase = playerData as ObjectBehaviourBase;
        if (objBase == null)
            return;

        base.ProcessFrame(playable, info, playerData);
    }
    


    public void UpdateBehaviour(float time,int cur_idx,int total_clip)
    {
        //if(GpuTimelineClip)
        //    OwningClip.displayName = GpuTimelineClip.name;
        if ((time >= OwningClip.start) && (time < OwningClip.end))
        {
            OnEnter();
        }
        else if(time >= OwningClip.end)
        {
            OnExit(cur_idx,total_clip);
        }
    }

    private void OnEnter()
    {
        if (!m_isFirstFrameProcess)
        {
            if (parentTrack is null || parentTrack.director is null || !parentTrack.director.playableGraph.IsValid() || parentTrack.director.playableGraph.GetPlayableCount() <= 0)
            {
                return;
            }
            //var speed1 = parentTrack.playable.GetSpeed();
            var speed2 = (float)parentTrack.director.playableGraph.GetRootPlayable(0).GetSpeed();
            //Log.Log2File($"{speed1}, {speed2}");
            GpuTimelineClip.directorSpeed = speed2;
            TrackTargetObject.PlayAnim(GpuTimelineClip);
            GpuTimelineClip.directorSpeed = 1;
            m_isFirstFrameProcess = true;
        }
    }
    
    private void OnExit(int cur_idx,int total_clip)
    {       
        if (m_isFirstFrameProcess)
        {            
            TrackTargetObject.StopAnim(GpuTimelineClip, cur_idx, total_clip);
           // if(cur_idx == total_clip -1)
           //    TrackTargetObject.StopAnim(GpuTimelineClip);
           m_isFirstFrameProcess = false;
        }
    }
    
}
