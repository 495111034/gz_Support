using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

/// <summary>
/// 注意坑：多个timeline同时播放时，只有一份SkillHitTimelineClip实例
/// </summary>
[System.Serializable]
public class SkillHitTimelineClip : PlayableAsset
{
    [System.Serializable]
    public enum ByHitType
    {
        Normal = 0,         //受击
        Repel = 1,          //击退
        KnockDown = 2,      //击倒
        KnockFly = 3,       //击飞
        Conjure = 4,        //招唤
        LockTarget = 5,     //锁定对方
        BreakFree = 6,      //挣脱
        JuGuai = 7,         //聚怪
    }

    [System.Serializable]
    public enum KnockFlyType
    {
        ByPhysical,             //根据上抛速度和重力加速度计算
        ByKeyFrame,             //根据关键帧曲线
    }

    [System.Serializable]
    public enum ByHitAnimType
    {
        Hit = 0,                //受击
        Jidao = 1,              //击倒
        Jifei = 2,              //击飞
        NoAnim = 3,             //无动作
    }

    /// <summary>
    /// 受击开始时间类型
    /// </summary>
    [System.Serializable]
    public enum StartTimeType
    {
        ByTimeline = 0,         //timeline上的图形时间
        ByDistance  = 1,         //根据与攻击者的距离和速度计算
    }

    [System.Serializable]
    public enum ShockCameraEventType
    {
        EveryOne = 0,           //每次震屏
        OnAddHp = 1,            //扣血震屏
        OnBaoji = 2,            //暴击震屏
    }

    //public enum ByHitSoundType
    //{
    //    EveryOne = 0,       //每次播放
    //    OnAddHp = 1,        //扣血播放
    //    OnBaiji = 2,        //暴击播放
    //}


    /// <summary>
    /// 用于战斗逻辑传入的打击参数
    /// </summary>
    /// 

    [System.Serializable]
    public class HitInfoParams
    {

        public HitInfoParams(int i) 
        {
            info_idx = i;
        }

        public int info_idx = -1;

        /// <summary>
        /// 击退距离
        /// </summary>
        public float RepealDistance = 0;
        /// <summary>
        /// 击退目标位置
        /// </summary>
        public Vector3 RepealDstPos = Vector3.zero;
        /// <summary>
        /// 击飞时挑起速度
        /// </summary>
        public float UpV0 = 0;
        /// <summary>
        /// 用于击飞计算的重力加速度
        /// </summary>
        public float g = 0;

        public void ZeroValues()
        {
            RepealDistance = 0;
            UpV0 = 0;
            g = 0;
            JuGuaiSpeed = RepealDstPos = Vector3.zero;
        }

        public Vector3 JuGuaiSpeed;
    }


    [HideInInspector]
    [SerializeField]
    private SkillHitPlayableData templete = new SkillHitPlayableData();
    public SkillHitPlayableData Templete { get { return templete; } set { templete = value; } }   
    public TimelineClip OwningClip { get; set; }
    public SkillHitTrack parentTrack { get; set; }

    public Dictionary<int, SkillHitPlayableData> _templateDic = new Dictionary<int, SkillHitPlayableData>();

    public SkillHitPlayableData GetTemplate(int goInstId)
    {
        if (goInstId == 0) return templete;
        return _templateDic.ContainsKey(goInstId) ? _templateDic[goInstId] : null;
    }

    public void CreateTemplateInst(int goInstId)
    {
        if (!_templateDic.ContainsKey(goInstId))
        {
            _templateDic[goInstId] = new SkillHitPlayableData();            
        }
        templete.CloneSerializeFiled(_templateDic[goInstId]);
        _templateDic[goInstId].GoInstID = goInstId;


    }

    public void RemoveTemplate(int goInstId)
    {
        if (_templateDic.TryGetValue(goInstId, out var _temp))
        {            
            _temp.targetArray = null;
            _temp.FightId = 0;
            _temp.attacker = null;           
            _templateDic.Remove(goInstId);
            //Log.LogError($"revmeo data,instid={goInstId}");
        }
        //_templateDic.Remove(goInstId);
    }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
    {
        var t = GetTemplate(go.GetInstanceID());

        ScriptPlayable<SkillHitPlayableData> pl ;
        if (t == null)
        {
            templete.OwningClip = OwningClip;
            pl = ScriptPlayable<SkillHitPlayableData>.Create(graph, templete);
           
        }
        else
        {
            t.OwningClip = OwningClip;
            pl = ScriptPlayable<SkillHitPlayableData>.Create(graph, t);           
        }

        SkillHitPlayableData behavior = pl.GetBehaviour();
        behavior.OwningClip = OwningClip;       
        return pl;
       
    }

    

    
}
