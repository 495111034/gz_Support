using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;


public class MoveClip : PlayableAsset
{
    [HideInInspector]
    public TimelineClip OwningClip { get; set; }
    [HideInInspector]
    public MoveTrack parentTrack { get; set; }
    [HideInInspector]
    public MoveClipPlayableData templete = new MoveClipPlayableData();
    [HideInInspector]
    public MoveClipPlayableData Templete { get { return templete; } set { templete = value; } }

    public Dictionary<int, MoveClipPlayableData> _templateDic = new Dictionary<int, MoveClipPlayableData>();


    public MoveClipPlayableData GetTemplate(int goInstId)
    {
        if (goInstId == 0) return templete;
        return _templateDic.ContainsKey(goInstId) ? _templateDic[goInstId] : null;
    }

    public void CreateTemplateInst(int goInstId)
    {
        if (!_templateDic.ContainsKey(goInstId))
        {
            _templateDic[goInstId] = new MoveClipPlayableData();
        }
        templete.CloneSerializeFiled(_templateDic[goInstId]);
        _templateDic[goInstId].GoInstID = goInstId;
    }

    public void RemoveTemplate(int goInstId)
    {
        if (_templateDic.ContainsKey(goInstId))
        {

            _templateDic[goInstId].GoInstID = 0;
            _templateDic[goInstId].FightId = 0;
            _templateDic[goInstId].trackTargetObject = null;
            _templateDic.Remove(goInstId);
        }
        _templateDic.Remove(goInstId);
    }


    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        var t = GetTemplate(owner.GetInstanceID());
        ScriptPlayable<MoveClipPlayableData> tpl;
        if (t == null)
        {
            templete.OwningClip = OwningClip;
            tpl = ScriptPlayable<MoveClipPlayableData>.Create(graph, templete);
        }
        else
        {
            t.OwningClip = OwningClip;
            tpl = ScriptPlayable<MoveClipPlayableData>.Create(graph, t);
        }

        return tpl;
    }
}

