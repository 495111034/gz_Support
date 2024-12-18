using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

[TrackColor(0.066f, 0.134f, 0.244f)]
[TrackClipType(typeof(FMODEventPlayable))]
[TrackBindingType(typeof(GameObject))]
public class FMODEventTrack : TrackAsset
{
    public FMODEventMixerBehaviour template = new FMODEventMixerBehaviour();
    public Dictionary<int, FMODEventMixerBehaviour> TemplateDiction = new Dictionary<int, FMODEventMixerBehaviour>();


    [HideInInspector]
    [SerializeField]
    private FModTargetType _soundTargetType = FModTargetType.NOTarget;
    public FModTargetType SoundTargetType { get { return _soundTargetType; } set { _soundTargetType = value; } }

    public void CreateInstaceContent(GameObject parentGo, long fightID, bool IsEmptyTarget)
    {
        if (!TemplateDiction.ContainsKey(parentGo.GetInstanceID()))
        {
            TemplateDiction[parentGo.GetInstanceID()] = new FMODEventMixerBehaviour();
        }

        template.CloneSerializeFiled(TemplateDiction[parentGo.GetInstanceID()]);
        TemplateDiction[parentGo.GetInstanceID()].FightId = fightID;

        foreach (var clip in GetClips())
        {
            var myAsset = clip.asset as FMODEventPlayable;
            if (myAsset)
            {
                myAsset.OwningClip = clip;

                myAsset.CreateTemplateInst(parentGo.GetInstanceID());
                var t1 = myAsset.GetTemplate(parentGo.GetInstanceID());
                t1.FightId = fightID;
                t1.IsEmptyTarget = IsEmptyTarget;
            }
        }
    }

    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        var director = go.GetComponent<PlayableDirector>();
        var trackTargetObject = director.GetGenericBinding(this) as GameObject;

        ScriptPlayable<FMODEventMixerBehaviour> tbl;

        if (!TemplateDiction.ContainsKey(go.GetInstanceID()))
        {
            tbl = ScriptPlayable<FMODEventMixerBehaviour>.Create(graph, template, inputCount);
        }
        else
        {
            var t = TemplateDiction[go.GetInstanceID()];
            tbl = ScriptPlayable<FMODEventMixerBehaviour>.Create(graph, t, inputCount);
        }

        foreach (var clip in GetClips())
        {
            var myAsset = clip.asset as FMODEventPlayable;

            if (myAsset)
            {
                myAsset.TrackTargetObject = trackTargetObject;
                myAsset.TrackObject = this;
                myAsset.OwningClip = clip;
            }
        }

        //var tbl = ScriptPlayable<FMODEventMixerBehaviour>.Create(graph, inputCount);
        return tbl;
    }

    public void ReleaseData(GameObject go)
    {
        if (TemplateDiction.ContainsKey(go.GetInstanceID()))
        {
            var t = TemplateDiction[go.GetInstanceID()];
            t.FightId = 0;
        }

        foreach (var clip in GetClips())
        {
            var myAsset = clip.asset as FMODEventPlayable;
            if (myAsset)
            {
                if (myAsset.template != null)
                {
                    myAsset.template.FightId = 0;
                }

                myAsset.RemoveTemplate(go.GetInstanceID());
            }
        }

        template.FightId = 0;
    }
}

public class FMODEventMixerBehaviour : PlayableBehaviour
{
    public long FightId { get; set; }

    public void CloneSerializeFiled(FMODEventMixerBehaviour target)
    {

    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        int inputCount = playable.GetInputCount();
        double time = playable.GetGraph().GetRootPlayable(0).GetTime();
        //if (time > 0)
        {
            for (int i = 0; i < inputCount; i++)
            {
                ScriptPlayable<FMODEventPlayableBehavior> inputPlayable = (ScriptPlayable<FMODEventPlayableBehavior>)playable.GetInput(i);
                FMODEventPlayableBehavior input = inputPlayable.GetBehaviour();
                input.FightId = FightId;
                input.UpdateBehaviour(time);
            }
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        base.OnBehaviourPause(playable, info);
        if (Application.isPlaying)
        {
            int inputCount = playable.GetInputCount();
            //if (time > 0)
            {
                for (int i = 0; i < inputCount; i++)
                {
                    ScriptPlayable<FMODEventPlayableBehavior> inputPlayable = (ScriptPlayable<FMODEventPlayableBehavior>)playable.GetInput(i);
                    FMODEventPlayableBehavior input = inputPlayable.GetBehaviour();
                    if (input != null)
                    {
                        input.FightId = FightId;
                        input.OnGraphStop();
                    }
                }
            }
        }
    }
}