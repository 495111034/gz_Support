using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;


/// <summary>
/// 向施法者通知战斗事件
/// 此类型为track对象，
/// 注意坑：同时多个播放时，track对象只有一份，需考虑多实例数据隔离的问题
/// </summary>
[TrackColor(0.7f, 0.2f, 0.1f)]
[TrackClipType(typeof(FightEventInfoClip))]
[TrackBindingType(typeof(ObjectBehaviourBase))]
public class FightEventInfoTrack : TrackAsset{

    public FightEventInfoMixer template = new FightEventInfoMixer();
    public Dictionary<int, FightEventInfoMixer> TemplateDiction = new Dictionary<int, FightEventInfoMixer>();

    public enum FightEventTargetType
    {
        Attacker = 0,
        FightTarget = 1,
        AttackerPet = 2,
        OtherTarget = 3,
    }

    [HideInInspector]
    [SerializeField]
    private FightEventTargetType _eventTargetType = FightEventTargetType.Attacker;

    public FightEventTargetType eventTargetType { get { return _eventTargetType; } set { _eventTargetType = value; } }

    public void CreateInstaceContent(GameObject parentGo, ObjectBehaviourBase attacker, long fightID)
    {
        if (!TemplateDiction.ContainsKey(parentGo.GetInstanceID()))
        {
            TemplateDiction[parentGo.GetInstanceID()] = new FightEventInfoMixer();
        }

        template.CloneSerializeFiled(TemplateDiction[parentGo.GetInstanceID()]);
        TemplateDiction[parentGo.GetInstanceID()].trackTargetObject = attacker;
        TemplateDiction[parentGo.GetInstanceID()].FightId = fightID;

        foreach (var clip in GetClips())
        {
            var myAsset = clip.asset as FightEventInfoClip;
            if (myAsset)
            {
                myAsset.OwningClip = clip;
                
                myAsset.CreateTemplateInst(parentGo.GetInstanceID());
                var t1 = myAsset.GetTemplate(parentGo.GetInstanceID());
                t1.FightId = fightID;
                t1.attacker = attacker;
            }
        }
    }

    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        var director = go.GetComponent<PlayableDirector>();
        var trackTargetObject = director.GetGenericBinding(this) as ObjectBehaviourBase;

        ScriptPlayable<FightEventInfoMixer> tbl;

        if (!TemplateDiction.ContainsKey(go.GetInstanceID()))
        {
            tbl = ScriptPlayable<FightEventInfoMixer>.Create(graph, template, inputCount);
        }
        else
        {
            var t = TemplateDiction[go.GetInstanceID()];
            tbl = ScriptPlayable<FightEventInfoMixer>.Create(graph, t, inputCount);
        }        

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            template.trackTargetObject = trackTargetObject;

            foreach (var clip in GetClips())
            {
                var myAsset = clip.asset as FightEventInfoClip;
                if (myAsset)
                {                    
                    myAsset.OwningClip = clip;                    
                    if (myAsset.Templete != null)
                    {
                        myAsset.Templete.attacker = trackTargetObject;
                        myAsset.Templete.OwningClip = clip;

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
            

            TemplateDiction.Remove(go.GetInstanceID());
        }
        foreach (var clip in GetClips())
        {
            var myAsset = clip.asset as FightEventInfoClip;
            if (myAsset)
            {
                if (myAsset.Templete != null)
                {
                    myAsset.Templete.attacker = null;
                    myAsset.Templete.FightId = 0;
                }

                myAsset.RemoveTemplate(go.GetInstanceID());
            }
        }

        template.trackTargetObject = null;
    }
}
