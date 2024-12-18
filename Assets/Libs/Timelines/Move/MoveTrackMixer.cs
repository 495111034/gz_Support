
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Linq;


[System.Serializable]
public class MoveTrackMixer : PlayableBehaviour
{
    public enum MoverType
    {
        Attacker = 0,
        Pet = 1,
        FightTarget = 2,
        CurrentMainRole = 3,
        OtherRole = 4,
        Npc = 5,
        CreateTmpRole = 6,
        CreateTmpNpc = 7,
        CreateNewMonster = 8,
        CreateTmpObject = 9,
        CreateItem = 10,
    }

    public ObjectBehaviourBase trackTargetObject;

    [HideInInspector]
    [SerializeField]
    public MoverType moverType;
    public long FightId { get; set; }
    public int InstanceID { get; set; }
    public TrackAsset parentTrack;

    public void CloneSerializeFiled(MoveTrackMixer target)
    {
        FightId = target.FightId;
        moverType = target.moverType;
        trackTargetObject = target.trackTargetObject;
        InstanceID = target.InstanceID;
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
        var clips = parentTrack.GetClips() as TimelineClip[];
        var rootPlayable = playable.GetGraph().GetRootPlayable(0);

        float time = (float)rootPlayable.GetTime();

#if UNITY_EDITOR
        //bool playing = playable.GetGraph().IsPlaying();
        //if (!Application.isPlaying)
        //{
        //    if (trackTargetObject)
        //        trackTargetObject.OnUpdateEditor(time);
        //}
#endif

        for (int i = 0; i < clipCount; i++)
        {
            TimelineClip currentClip = clips[i];
            var _clipData = currentClip.asset as MoveClip;
            if (Application.isPlaying)
            {
                var _clipTemplateData = _clipData.GetTemplate(InstanceID);
                if (_clipTemplateData != null)
                    _clipTemplateData.UpdateBehaviour(time, i, clipCount, clips[i]);
            }
            else
            {
#if UNITY_EDITOR
                _clipData.Templete.UpdateBehaviour(time, i, clipCount, clips[i]);
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

