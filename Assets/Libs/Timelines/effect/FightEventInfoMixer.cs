
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;


public class FightEventInfoMixer : PlayableBehaviour
{
    

    public ObjectBehaviourBase trackTargetObject;
    public long FightId { get; set; }

    int clip_idx;
    public void CloneSerializeFiled(FightEventInfoMixer target)
    {
        target.clip_idx = 0;
    }

    public override void OnPlayableCreate(Playable playable)
    {

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
        bool playing = playable.GetGraph().IsPlaying();

        if (!Application.isPlaying)
        {
            if (trackTargetObject)
                trackTargetObject.OnUpdateEditor(time);
        }
#endif //UNITY_EDITOR


        for (int i = clip_idx; i < clipCount; i++)
        {
            ScriptPlayable<FightEventInfoPlayableData> inputPlayable = (ScriptPlayable<FightEventInfoPlayableData>)playable.GetInput(i);
            FightEventInfoPlayableData input = inputPlayable.GetBehaviour();
            if (!input.UpdateBehaviour(time, i, clipCount)) 
            {
                break;
            }
            clip_idx = clip_idx + 1;
            //Debug.LogWarning($"fight {FightId}, add clip_idx={clip_idx}/{clipCount}");
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
