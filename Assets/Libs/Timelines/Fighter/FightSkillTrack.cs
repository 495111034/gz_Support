using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;


/// <summary>
/// timeline中的一条轨道
/// </summary>
[TrackClipType(typeof(FightSkillClip))]
[TrackBindingType(typeof(ObjectBehaviourBase))]
public class FightSkillTrack : TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        var tbl = ScriptPlayable<FightSkillMixer>.Create(graph, inputCount);
        foreach (var clip in GetClips())
        {            
            var myAsset = clip.asset as FightSkillClip;
            if (myAsset)
            {
                myAsset.Templete.parentSkillClip = this;
              
                //Log.LogError($"CreateTrackMixer:{clip}");
            }

        }
        //tbl.GetBehaviour().aaaaaaa
        return tbl;
    }
}

