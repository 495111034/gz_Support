using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;


[TrackColor(1f, 0f, 1f)]
[TrackClipType(typeof(GPUAnimTimelineClip))]
[TrackBindingType(typeof(ObjectBehaviourBase))]
[System.Serializable]
public class GPUAnimationTrack : TrackAsset
{
    public GPUAnimationMixer template = new GPUAnimationMixer();
    public ScriptPlayable<GPUAnimationMixer> playable;
    public PlayableDirector director;

    [HideInInspector] [SerializeField] TimelineType timelineType;

    [HideInInspector]
    [SerializeField] ObjectBehaviourBase skillTarget;

    public long fightId = 0;

    public ObjectBehaviourBase SkillTarget { get { return skillTarget; }set { skillTarget = value; } }

    public ObjectBehaviourBase trackTargetObject;
    public TimelineType ScenarioType { get { return timelineType; } }
    public void OnEnable()
    {

    }

    public GPUAnimationTrack()
    {

    }
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        director = go.GetComponent<PlayableDirector>();
        trackTargetObject = director.GetGenericBinding(this) as ObjectBehaviourBase;
        foreach (var clip in GetClips())
        {            
            var myAsset = clip.asset as GPUAnimTimelineClip;
            if (myAsset)
            {
                myAsset.Templete.parentTrack = this;
                myAsset.Templete.FightId = fightId;
                myAsset.TrackTargetObject = trackTargetObject;
                myAsset.OwningClip = clip;
                myAsset.parentTrack = this;
            }
        }

#if UNITY_EDITOR
        var assetFile = UnityEditor.AssetDatabase.GetAssetPath(director.playableAsset).ToLower();
        if (assetFile.StartsWith(PathDefs.ASSETS_PATH_CHARACTER))
        {
            timelineType = TimelineType.FightSkill;
        }
        else
        {
            timelineType = TimelineType.SceneScenario;
        }
#endif
         playable =  ScriptPlayable<GPUAnimationMixer>.Create(graph, template,  inputCount);
        playable.GetBehaviour().player = director;
        playable.GetBehaviour().objBase = trackTargetObject;       
        return playable;
    }

    public void ReleaseData()
    {
        skillTarget = null;
        trackTargetObject = null;
        foreach (var clip in GetClips())
        {
            var myAsset = clip.asset as GPUAnimTimelineClip;
            if (myAsset)
            {
                myAsset.Templete.FightId = 0;
                myAsset.TrackTargetObject = null;
            }
        }
    }
}
