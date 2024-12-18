using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;


/// <summary>
/// Data
/// 用来存放攻击技能数据
/// </summary>
[System.Serializable]
public class FightSkillPlayableData : PlayableBehaviour
{
    /// <summary>
    /// 攻击者，在程序中赋值
    /// </summary>
    [HideInInspector]
    public Transform attacker;
    [HideInInspector]
    public FightSkillTrack parentSkillClip;
    [HideInInspector]
    private ObjectBehaviourBase objBase;   
    [HideInInspector]
    private bool m_isFirstFrameProcess = true;
    
    

    public override void OnPlayableCreate(Playable playable)
    {
        var duration = playable.GetDuration();
        if (Mathf.Approximately((float)duration, 0))
        {
            throw new UnityException("A Clip Cannot have a duration of zero");
        }
    }

    public override void OnGraphStart(Playable playable)
    {
        m_isFirstFrameProcess = false;
        base.OnGraphStart(playable);
       // Debug.Log("OnGraphStart 11111111111111111111111111");
    }

    public override void OnGraphStop(Playable playable)
    {
        base.OnGraphStop(playable);
        //Debug.Log("OnGraphStop 1111111111111111111111111");
        //if(objBase && m_isFirstFrameProcess)
        //{
        //    objBase.OnFightClipStop(this);
        //}
        //m_isFirstFrameProcess = false;
        objBase = null;
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        base.OnBehaviourPlay(playable, info);
        //Debug.Log("OnBehaviourPlay 1111111111111111111111");
        
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        base.OnBehaviourPause(playable, info);
       // Debug.Log("OnBehaviourPause 11111111111111111111");

    }

    public override void OnBehaviourDelay(Playable playable, FrameData info)
    {
        base.OnBehaviourDelay(playable, info);
       // Debug.Log("OnBehaviourDelay 11111111111111111111");
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (!(playerData is ObjectBehaviourBase))
        {
            Log.LogError($"clip ProcessFrame:playerdata is not ObjectBehaviourBase:{playerData?.GetType()}");
            return;
        }

        objBase = playerData as ObjectBehaviourBase;
        if (objBase == null)
            return;

        if (!m_isFirstFrameProcess)
        {
            m_isFirstFrameProcess = true;
           // objBase.OnFightClipStart(this);
        }
        else
        {

          //  objBase.OnFightClipFrame(this, info);
        }
       // Debug.Log($"clip ProcessFrame111111111111111111111 {objBase.gameObject.name}");
        base.ProcessFrame(playable, info, playerData);
    }

}
