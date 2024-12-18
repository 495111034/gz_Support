
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Linq;

public class SkillHitMixer : PlayableBehaviour
{ 
    public ObjectBehaviourBase attacker;

    //private SkillHitPlayableData skillData = new SkillHitPlayableData();
    public ObjectBehaviourBase trackTargetObject;
   // public ObjectArray targetArray;
    public long FightId { get; set; }
    public int InstanceID { get; set; }

    public TrackAsset parentTrack;

    public int clip_idx = 0;

    //bool _fix_weights = false;

    public SkillHitMixer()
    {
        //Log.Log2File($"SkillHitMixer new {this.GetHashCode()}");
    }
    public SkillHitMixer(long FightId) 
    {
        //Log.Log2File($"SkillHitMixer {FightId}, new {this.GetHashCode()}");
    }

    public void CloneSerializeFiled(SkillHitMixer target)
    {
        //Log.Log2File($"SkillHitMixer {target.FightId}, reset clip_idx={target.clip_idx}, {target.InstanceID}, {target.GetHashCode()}");
        target.clip_idx = 0;
    }   
    

    
    public override void OnGraphStart(Playable playable)
    {
       
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
       
    }

    public override void PrepareFrame(Playable playable, FrameData info)
    {
       
    }
    
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {       

        int clipCount = playable.GetInputCount();
        var rootPlayable = playable.GetGraph().GetRootPlayable(0);

        float time = (float)rootPlayable.GetTime();
#if UNITY_EDITOR
        //bool playing = playable.GetGraph().IsPlaying();        
        if (!Application.isPlaying)
        {
            if(trackTargetObject)
                trackTargetObject.OnUpdateEditor(time);
        }
#endif //UNITY_EDITOR

        var clips = parentTrack.GetClips() as TimelineClip[];

        var skillhittrack = parentTrack as SkillHitTrack;
        if (skillhittrack && !skillhittrack.fixed_weights)
        {
            skillhittrack.fixed_weights = true;
            var total = 0f;
            for (int i = 0; i < clipCount; i++)
            {
                TimelineClip currentClip = clips[i];
                var Templete = (currentClip.asset as SkillHitTimelineClip).Templete;
                if (Templete.HitType == SkillHitTimelineClip.ByHitType.Normal)
                {
                    total += Templete.Weights;
                }
            }
            if (total > 0 && !Mathf.Approximately(1, total))
            {
                Log.LogError($"{skillhittrack.parent} 多段攻击权重的和:{total} != 1");
                if (Application.isPlaying)
                {
                    var total2 = 0f;
                    var xishu = 1 / total;
                    for (int i = 0; i < clipCount; i++)
                    {
                        TimelineClip currentClip = clips[i];
                        var Templete = (currentClip.asset as SkillHitTimelineClip).Templete;
                        if (Templete.HitType == SkillHitTimelineClip.ByHitType.Normal)
                        {
                            if (i == clipCount - 1)
                            {
                                Templete.Weights = 1 - total2;
                            }
                            else
                            {
                                Templete.Weights *= xishu;
                            }
                            total2 += Templete.Weights;
                        }
                    }
                }
            }
        }

        for (int i = clip_idx; i < clipCount; i++)
        {
            TimelineClip currentClip = clips[i];
            var _clipData = currentClip.asset as SkillHitTimelineClip;
            if (Application.isPlaying)
            {
                var _clipTemplateData = _clipData.GetTemplate(InstanceID);
                if (_clipTemplateData != null)
                {
                    if (!_clipTemplateData.UpdateBehaviour(time, i, clipCount)) 
                    {
                        break;
                    }
                    clip_idx = i + 1;
                    //Debug.LogWarning($"hit {FightId}, add clip_idx={clip_idx}/{clipCount}");
                }
            }
            else
            {
#if UNITY_EDITOR
                if (time == 0) 
                {
                    _clipData.Templete.isPlayHitList = _clipData.Templete.isEndHitList = false;
                }
                _clipData.Templete.UpdateBehaviour(time, i, clipCount);
#endif
            }
        }

    }
    
    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        
    }

    public override void OnGraphStop(Playable playable)
    {
        
    }

    public override void OnPlayableDestroy(Playable playable)
    {
        
    }
    

}
