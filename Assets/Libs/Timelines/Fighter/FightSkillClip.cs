using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;


/// <summary>
/// timeline轨道中的一个Clip
/// 技能asset，不能直接与场景中的物体关联，需要关联场景中的物体时要用ExposedReference来声明， 并且通过Resolve方法赋值。
/// </summary>
[System.Serializable]
public class FightSkillClip : PlayableAsset
{
    [HideInInspector]
    [SerializeField]
    private FightSkillPlayableData templete = new FightSkillPlayableData();

    public FightSkillPlayableData Templete { get { return templete; } set { templete = value; } }

    /// <summary>
    /// 攻击技能id
    /// </summary>
    [HideInInspector]
    [SerializeField]
    int skillID;



    //[HideInInspector]
    //[SerializeField]
    //Entity.SkillBase skillData;

    public int SkillID { get { return skillID; } set { skillID = value; } }



    [HideInInspector]
    public ExposedReference<Transform> attacker;
    // public ExposedReference<FightSkillTrack> parentTrack;
    public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
    {

        var playable = ScriptPlayable<FightSkillPlayableData>.Create(graph, templete);
        var clone = playable.GetBehaviour();
        clone.attacker = attacker.Resolve(graph.GetResolver());       
        // clone.parentSkillClip = parentTrack.Resolve(graph.GetResolver());
        return playable;
    }

}

