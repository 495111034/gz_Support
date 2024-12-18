using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// 多个同时播放时，此实例只有一个，数据需要通过实例id在字典中获取
/// </summary>
[TrackColor(1f, 1f, 0f)]
[TrackClipType(typeof(MoveClip))]
[TrackBindingType(typeof(ObjectBehaviourBase))]
public class MoveTrack : TrackAsset
{
    //[NonSerialized]
    //[HideInInspector]
    public MoveTrackMixer template = new MoveTrackMixer();
    //多个MoveTrack同时播放时，通过实例id取MoveTrackMixer的对象
    [HideInInspector]    
    public Dictionary<int, MoveTrackMixer> TemplateDiction = new Dictionary<int, MoveTrackMixer>();

    [HideInInspector] [SerializeField] TimelineType timelineType = TimelineType.FightSkill;

    public void CreateInstaceContent(GameObject parentGo, ObjectBehaviourBase target, long fightID, GameObject targetObject)
    {
        if (!TemplateDiction.ContainsKey(parentGo.GetInstanceID()))
        {
            TemplateDiction[parentGo.GetInstanceID()] = new MoveTrackMixer();
        }
       
        TemplateDiction[parentGo.GetInstanceID()].InstanceID = parentGo.GetInstanceID();
        TemplateDiction[parentGo.GetInstanceID()].FightId = fightID;
        TemplateDiction[parentGo.GetInstanceID()].trackTargetObject = target;
        TemplateDiction[parentGo.GetInstanceID()].parentTrack = this;

        template.CloneSerializeFiled(TemplateDiction[parentGo.GetInstanceID()]);
        template.parentTrack = this;

        foreach (var clip in GetClips())
        {
            var myAsset = clip.asset as MoveClip;
            if (myAsset)
            {
                myAsset.OwningClip = clip;
                myAsset.parentTrack = this;
                myAsset.CreateTemplateInst(parentGo.GetInstanceID());

                var t1 = myAsset.GetTemplate(parentGo.GetInstanceID());
                if (t1 != null)
                {
                    t1.OwningClip = clip;
                    t1.FightId = fightID;
                    t1.trackTargetObject = target;
                    t1.targetObject = targetObject;
                }
            }
        }

    }

    public TimelineType ScenarioType { get { return timelineType; } }
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {       
        var director = go.GetComponent<PlayableDirector>();
        var trackTargetObject = director.GetGenericBinding(this) as ObjectBehaviourBase;

        ScriptPlayable<MoveTrackMixer> tbl;

        if (!TemplateDiction.ContainsKey(go.GetInstanceID()))
        {
            template.trackTargetObject = trackTargetObject;
            template.parentTrack = this;
            tbl = ScriptPlayable<MoveTrackMixer>.Create(graph, template, inputCount);
        }
        else
        {
            var t = TemplateDiction[go.GetInstanceID()];
            t.trackTargetObject = trackTargetObject;
            t.parentTrack = this;
            tbl = ScriptPlayable<MoveTrackMixer>.Create(graph, t, inputCount);
        }

#if UNITY_EDITOR       

        if (!Application.isPlaying)
        {
            var assetFile = UnityEditor.AssetDatabase.GetAssetPath(director.playableAsset).ToLower();

            if (assetFile.StartsWith(PathDefs.ASSETS_PATH_CHARACTER))
            {
                timelineType = TimelineType.FightSkill;
            }
            else
            {
                timelineType = TimelineType.SceneScenario;
            }

            

            foreach (var clip in GetClips())
            {
                var myAsset = clip.asset as MoveClip;
                if (myAsset)
                {
                    myAsset.OwningClip = clip;
                    myAsset.parentTrack = this;

                    if (myAsset.Templete != null)
                    {
                        myAsset.Templete.OwningClip = clip;
                        myAsset.Templete.trackTargetObject = trackTargetObject;
                    }
                }
            }
        }
#endif

        return tbl;
    }

    public void ReleaseData(GameObject go)
    {
        if (TemplateDiction.ContainsKey(go.GetInstanceID()))
        {
            var t = TemplateDiction[go.GetInstanceID()];          
            
            t.trackTargetObject = null;
            t.FightId = 0;
            t.InstanceID = 0;

        }
       
        template.trackTargetObject = null;
        template.FightId = 0;
        template.InstanceID = 0;

        foreach (var clip in GetClips())
        {
            var myAsset = clip.asset as MoveClip;
            if (myAsset)
            {
                if (myAsset.Templete != null)
                {                   
                   myAsset.Templete.targetObject = null;
                    myAsset.Templete.trackTargetObject = null;
                    myAsset.Templete.FightId = 0;
                }

                myAsset.RemoveTemplate(go.GetInstanceID());
            }
        }

    }
}
