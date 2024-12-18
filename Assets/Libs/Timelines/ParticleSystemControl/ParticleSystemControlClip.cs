
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 特效clip对象
/// 所有timeline实例共用一个ParticleSystemControlClip对象
/// </summary>
[System.Serializable]
public class ParticleSystemControlClip : PlayableAsset, ITimelineClipAsset
{
    public ParticleSystemControlPlayable template = new ParticleSystemControlPlayable();

    //public ParticleSystemControlPlayable Template { get { return template; } }

    private Dictionary<int, ParticleSystemControlPlayable> _templateDic = new Dictionary<int, ParticleSystemControlPlayable>();

    public ParticleSystemControlPlayable GetTemplate(int goInstId)
    {
        if (goInstId == 0) return template;
        if (_templateDic.TryGetValue(goInstId, out var t)) 
        {
            return t;
        }
        return null;
    }

    public void CreateTemplateInst(int goInstId)
    {
        if(!_templateDic.ContainsKey(goInstId))
        {
            //Log.LogInfo($"CreateTemplateInst {this.GetInstanceID()} for goInstId={goInstId}");
            var t = _templateDic[goInstId] = new ParticleSystemControlPlayable();
            template.CloneSerializeFiled(t);
        }
    }
    public void RemoveTemplate(int goInstId)
    {
        //if(_templateDic.ContainsKey(goInstId))
        {
            //Log.LogInfo($"RemoveTemplate {this.GetInstanceID()} for goInstId={goInstId}");
            _templateDic.Remove(goInstId);
        }
    }

    public TimelineClip OwningClip { get; set; }
    public ParticleSystemControlTrack parentTrack { get; set; }
    public ClipCaps clipCaps { get { return ClipCaps.Blending; } }

    /// <summary>
    /// 开始播放时调用
    /// </summary>
    /// <param name="graph"></param>
    /// <param name="go"></param>
    /// <returns></returns>
    public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
    {

        var t = GetTemplate(go.GetInstanceID());

        if (t == null)
        {
            var pl = ScriptPlayable<ParticleSystemControlPlayable>.Create(graph, template);
            return pl;
        }
        else
        {
            var pl = ScriptPlayable<ParticleSystemControlPlayable>.Create(graph, t);
            return pl;
        }
    }

    
}


