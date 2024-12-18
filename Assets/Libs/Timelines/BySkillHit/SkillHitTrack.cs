using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// 注意坑：多个timeline同时播放时，只有一份SkillHitTrack实例
/// </summary>
[TrackColor(0.5f, 1f, 0.5f)]
[TrackClipType(typeof(SkillHitTimelineClip))]
[TrackBindingType(typeof(ObjectBehaviourBase))]
[System.Serializable]
public class SkillHitTrack : TrackAsset
{     
    public SkillHitMixer template = new SkillHitMixer(0);
    public Dictionary<int, SkillHitMixer> TemplateDiction = new Dictionary<int, SkillHitMixer>();

    [System.NonSerialized]
    public bool fixed_weights = true;//暂时不用，真多段 会误判
    public void CreateInstaceContent(GameObject parentGo, ObjectBehaviourBase attacker,long fightID, ObjectArray targetList, double dur, bool is_update)
    {
#if UNITY_EDITOR
        if (targetList.isInCache != 0)
            Log.LogError($"Assign array={GetHashCode()}, isInCache={targetList.isInCache}");
#endif

        int instanceID = parentGo.GetInstanceID();
        if(!TemplateDiction.TryGetValue(instanceID, out var mix))
        {
            TemplateDiction[instanceID] = mix = new SkillHitMixer(fightID);
        }
        mix.InstanceID = instanceID;
        mix.FightId = fightID;
        mix.attacker = attacker;
        if (!is_update)
        {
            template.CloneSerializeFiled(mix);
        }
        //Log.Log2File($"SkillHitMixer {fightID}, CreateInstaceContent {instanceID} => {mix.clip_idx}, {mix.GetHashCode()}");
        UnityEngine.Profiling.Profiler.BeginSample("Begin SkillHitTrack");
        var clips = m_Clips;
        int len = clips.Count;
        for (int i = 0; i < len; i++)
        {
            var clip = clips[i];
            var myAsset = clip.asset as SkillHitTimelineClip;
            if (myAsset)
            {
                myAsset.OwningClip = clip;
                myAsset.parentTrack = this;
                if (!is_update)
                {
                    myAsset.CreateTemplateInst(instanceID);
                }
                var t1 = myAsset.GetTemplate(instanceID);
                if (t1 != null)
                {
                    t1.OwningClip = clip;
                    t1.FightId = fightID;
                    t1.attacker = attacker;
                    if (!is_update || !t1.isPlayHitList)
                    {
                        t1.targetArray = targetList;
                    }
                    //++targetList.refcnt;
                    t1.timeline_duration = dur;
                }
            }
        }
        UnityEngine.Profiling.Profiler.EndSample();
    }


    /// <summary>
    /// 每次播放时执行，重复播放时也会执行
    /// </summary>
    /// <param name="graph"></param>
    /// <param name="go"></param>
    /// <param name="inputCount"></param>
    /// <returns></returns>
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    { 

        var director = go.GetComponent<PlayableDirector>();
        var  trackTargetObject = director.GetGenericBinding(this) as ObjectBehaviourBase;

        ScriptPlayable<SkillHitMixer> tbl;
        if (!TemplateDiction.ContainsKey(go.GetInstanceID()))
        {
            tbl = ScriptPlayable<SkillHitMixer>.Create(graph, template, inputCount);            
        }
        else
        {
            var t = TemplateDiction[go.GetInstanceID()];
            tbl = ScriptPlayable<SkillHitMixer>.Create(graph, t, inputCount);           
        }

        tbl.GetBehaviour().trackTargetObject = trackTargetObject;
        tbl.GetBehaviour().parentTrack = this;
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            var attacker = template.attacker;
            if (!attacker) 
            {
                foreach (var tk in (director.playableAsset as TimelineAsset).GetRootTracks()) 
                {
                    if (tk is GPUAnimationTrack || tk is ParticleSystemControlTrack) 
                    {
                        attacker = director.GetGenericBinding(tk) as ObjectBehaviourBase;
                        if (attacker) 
                        {
                            break;
                        }
                    }
                }
                template.attacker = attacker;
            }
            foreach (var clip in GetClips())
            {
                var myAsset = clip.asset as SkillHitTimelineClip;
                if (myAsset)
                {
                    myAsset.Templete.attacker = attacker;
                    myAsset.Templete.targetObject = trackTargetObject;
                    myAsset.OwningClip = clip;
                    myAsset.parentTrack = this;
                    

                    var t1 = myAsset.GetTemplate(go.GetInstanceID());
                    if (t1 != null)
                    {
                        t1.GoInstID = go.GetInstanceID();
                        t1.targetObject = trackTargetObject;
                        t1.OwningClip = clip;                       
                    }
                }
            }
        }

#endif

        return tbl;


    }

    public void ReleaseData(GameObject go)
    {
        if (!go)
            return;

        var instanceID = go.GetInstanceID();
       if (TemplateDiction.ContainsKey(instanceID))
        {
            var t = TemplateDiction[go.GetInstanceID()];
            //Log.Log2File($"SkillHitMixer {t.FightId}, ReleaseData {instanceID} => {t.clip_idx}, {t.GetHashCode()}");
            //t.targetArray.ClearArray();
            // t.targetArray = null;
            t.attacker = null;
            t.trackTargetObject = null;
            t.FightId = 0;
            t.InstanceID = 0;
            TemplateDiction.Remove(go.GetInstanceID());
        }
        
        template.attacker = null;
        template.trackTargetObject = null;


        foreach (var clip in GetClips())
        {
            var myAsset = clip.asset as SkillHitTimelineClip;
            if (myAsset)
            {
                if (myAsset.Templete != null)
                {
                    myAsset.Templete.targetArray = null;
#if UNITY_EDITOR
                    myAsset.Templete.targetObject = null;
#endif
                }
                myAsset.RemoveTemplate(go.GetInstanceID());
            }
        }
    }
    
}
