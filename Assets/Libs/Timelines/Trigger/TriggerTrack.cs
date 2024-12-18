using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[TrackClipType(typeof(TriggerTimelineClip))]
[TrackBindingType(typeof(GameObject))]
public class TriggerTrack:TrackAsset
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        var director = go.GetComponent<PlayableDirector>();
        var trackTargetObject = director.GetGenericBinding(this) as GameObject;
        foreach (var clip in GetClips())
        {            
            var myAsset = clip.asset as TriggerTimelineClip;
            if (myAsset)
            {
                myAsset.Templete.parentTrack = this;
                
                myAsset.TrackTargetObject = trackTargetObject;
                myAsset.OwningClip = clip;
            }
        }
        
        var tbl = ScriptPlayable<TriggerMixer>.Create(graph, inputCount);
        return tbl;
    }

}
