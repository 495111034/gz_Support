using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// 注意坑：同时多个FightEventInfoClip播放时，只有一份实例，需要考虑数据隔离的问题
/// </summary>
[System.Serializable]
public class FightEventInfoClip : PlayableAsset
{
    [HideInInspector]
    [SerializeField]
    private FightEventInfoPlayableData templete = new FightEventInfoPlayableData();
    public FightEventInfoPlayableData Templete { get { return templete; } set { templete = value; } }

    public Dictionary<int, FightEventInfoPlayableData> _templateDic = new Dictionary<int, FightEventInfoPlayableData>();
    public TimelineClip OwningClip { get; set; }  

    public FightEventInfoPlayableData GetTemplate(int goInstId)
    {
        if (goInstId == 0) return templete;
        return _templateDic.ContainsKey(goInstId) ? _templateDic[goInstId] : null;
    }

    public void CreateTemplateInst(int goInstId)
    {
        if (!_templateDic.ContainsKey(goInstId))
        {
            _templateDic[goInstId] = new FightEventInfoPlayableData();
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
            _templateDic[goInstId].attacker = null;
            _templateDic.Remove(goInstId);
        }
        _templateDic.Remove(goInstId);
    }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
    { 
        var t = GetTemplate(go.GetInstanceID());
        ScriptPlayable<FightEventInfoPlayableData> tpl;
        if (t == null)
        {
            templete.OwningClip = OwningClip;
            tpl = ScriptPlayable<FightEventInfoPlayableData>.Create(graph, templete);
        }
        else
        {
            t.OwningClip = OwningClip;
            tpl = ScriptPlayable<FightEventInfoPlayableData>.Create(graph, t);
        }

        return tpl;
    }
}
