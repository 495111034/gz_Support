using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;

[System.Serializable]
public enum TimelineActorType
{
    ActorRoot = 1,   
    EffectRoot = 3,   
    UIPanelRoot = 5,
    UIPanelChild = 6,
    ActorPet = 8,
    CurrentMainRole = 9,
    ActorFightTarget = 10,
    ExternalTarget = 11,
}

[System.Serializable]
public enum TimelineVCamTargetType
{
    None = 0,
    ActorSelf = 1,
    SkillTarget = 2,
    SceneCenter = 3,
    EnemyCenter = 4,
    OurCenter = 5,   
    Effect = 6,
    OtherSceneObject = 7,
    CurrentMainRole = 8,
    CreatedNpc = 9,
    ExistsNpc = 10,
    //ActorSelfCam = 11, //跟随角色的某个节点
}

[System.Serializable]
public enum TimelineType
{
    FightSkill = 1,
    SceneScenario = 2,
}

[System.Serializable]
public enum TimelineTrackType
{
    UnityAnim = 1,
    MyAnim = 2,
    Cinemachine = 3,
    FMOD = 4,
    Activation = 5,
    Particle = 6,
}


[Serializable]
public struct ActorAnimationClip
{
    public double start;
    public double duration;
    public double end;
    public string animationClipName;
    public double clipIn;
}

[Serializable]
public struct ActorData
{
    public TimelineActorType actorType;     //演员类型（角色根、角色身体、宠物或其它特效的prefab）  
    public int aoi_type;                    //角色类型
    public string actorName;                //需要动态加载的prefab名称
    public bool isGpuGroup;                 //是否是gpu动画组
    public string ScenarioID;               //ID
    public bool isBossShow;                 //是否是 BOSS出场动画
    public float bossScale;
    public bool isIndependent;              //是否独立对象
    public string childObjectName;
    public string actorPos;                 //需要动态加载演员出生位置、转角
    public TimelineTrackType trackType;
    public bool isLowOverhead;              //是否低开销特效
    public ActorAnimationClip[] clips;      //演员动画
    public string profGenderKey;            //由职业和性别拼接的key，用于结婚过场动画
}

[Serializable]
public struct VCamData
{
    public string vCamAssetName;                        //自动填充
    //public GameObject vCamPrefab;                     //
    //
    //#if UNITY_EDITOR
    //    [System.NonSerialized]
    //    public UnityEngine.Object vCamAssetObject;
    //#endif    
}


[System.Serializable]
public enum STOP_MODE : int
{
    AllowFadeout,
    Immediate,
    None
}

[System.Serializable]
public enum FModTargetType
{
    NOTarget = 0,       //无目标（2D音效）
    SkillAttacker = 1,  //技能施法者
    SkillByHitter = 2,  //技能受击者    
}

[System.Serializable]
public enum FModSoundType
{
    DirectPlay = 0,     //直接播放
    SkillSound = 1,     //技能喊招
    AttackerSound = 2,  //攻击者声音
    HitSound = 3,       //根据目标数量播放声音
}

[Serializable]
public struct FModeClipData
{
    public string eventName;
    public STOP_MODE stopMode;
    public double start;
    public double duration;
    public double end;
    public bool stopBgMusic;
    public FMODUnity.ParamRef[] parameters;
}

[Serializable]
public struct FModData
{    
    public string gameobjectName;
    public FModTargetType targetType;
    public FModeClipData[] clips;
}

[Serializable]
public class TimelineData : ScriptableObject
{
    [SerializeField]
    public TimelineType DataType;           //此timeline数据类型

    [SerializeField]
    public TimelineAsset timeLineDataAsset;
    [SerializeField]
    public string timeLineDataAssetName;
    [SerializeField]
    public ActorData[] actorDatas = new ActorData[0];
    //public ActorData[] ActorDatas { get { return actorDatas; } }
    [SerializeField]
    public VCamData[] vCamDatas = new VCamData[0];
    [SerializeField]
    public FModData[] fModDatas = new FModData[0];

    //[SerializeField]
    //public int fix = 1;
    //
}
