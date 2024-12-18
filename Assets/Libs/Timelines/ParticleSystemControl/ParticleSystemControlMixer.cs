using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Playables;
using UnityEngine.Timeline;


public class EffectBehaviourList
{
    public List<EffectBehaviour> effectList;    

    public void Init(EffectBehaviour source, int count)
    {
        if (effectList == null)
            effectList = new List<EffectBehaviour>();
        else
            effectList.Clear();

        if (!source)
        {
            //Log.LogError("EffectBehaviour source is null");
            return;
        }
        //source.CheckHuaZhi();
        effectList.Add(source);

        for (int i = 1; i < count; ++i)
        {
            var tmp = GameObject.Instantiate(source.gameObject);
            tmp.gameObject.name = source.name + "_timeline_temp_" + i;
            EffectBehaviour effect = tmp.GetComponent<EffectBehaviour>();
            //if (effect) effect.CheckHuaZhi();
            effectList.Add(effect);
        }

    }


    public bool IsEmpty
    {
        get { return effectList == null || effectList.Count == 0; }
    }

    public int Count
    {
        get
        {
            return IsEmpty ? 0 : effectList.Count;
        }
    }

    public int PsCount
    {
        get
        {
            return IsEmpty ? 0 : effectList[0].PsCount;
        }
    }
    public bool IsActive
    {
        get
        {
            return IsEmpty ? false : effectList[0].gameObject.activeInHierarchy;
        }
        set
        {
            for (int i = 0; i < effectList.Count; ++i)
            {
                if (effectList[0])
                {
                    if (value)
                        effectList[i].gameObject.SetActiveAndResert();
                    else
                        effectList[i].gameObject.SetActive(false);
                }
            }
        }
    }

    public bool useAutoRandomSeed
    {
        get
        {
            return IsEmpty ? false : effectList[0].useAutoRandomSeed;
        }
        set
        {
            for(int i = 0; i < effectList.Count; ++i)
            {
                if(effectList[i])
                    effectList[i].useAutoRandomSeed = value;
            }
        }
    }
    public uint randomSeed
    {
        get
        {
           
           return IsEmpty?0: effectList[0].randomSeed;         
        }
        set
        {
            for (int i = 0; i < effectList.Count; ++i)
            {
                if (effectList[i])
                    effectList[i].randomSeed = value;
            }
        }
    }

    public ParticleSystem.MainModule main
    {
        get
        {
            return IsEmpty ? new ParticleSystem.MainModule() : effectList[0].main;

        }

    }

    public void Simulate(float v)
    {
      
        for (int i = 0; i < effectList.Count; ++i)
        {
            if (effectList[i])
                effectList[i].Simulate(v);
        }
    }
    public void Simulate(float t, bool withChildren)
    {     
        for (int i = 0; i < effectList.Count; ++i)
        {
            if (effectList[i])
                effectList[i].Simulate(t, withChildren);
        }
    }
    public void Simulate(float t, bool withChildren = true, bool restart = true, bool fixedTimeStep = true)
    {     
        for (int i = 0; i < effectList.Count; ++i)
        {
            if (effectList[i])
                effectList[i].Simulate(t, withChildren, restart, fixedTimeStep);
        }
    }

    public void Stop()
    {    
        for (int i = 0; i < effectList.Count; ++i)
        {
            if (effectList[i])
                effectList[i].Stop();
        }
    }

    public bool isPlaying
    {
        get
        {  
            if (effectList.Count > 0) return effectList[0].isPlaying;
            return false;
        }

    }

    public float time
    {
        get
        {        
            if (effectList.Count > 0) return effectList[0].time;
            return 0f;
        }
        //set
        //{    
        //    for (int i = 0; i < effectList.Count; ++i)
        //    {
        //        effectList[i].time = value;
        //    }
        //}
    }

    public void Play()
    {
        for (int i = 0; i < effectList.Count; ++i)
        {
            if (effectList[i])
                effectList[i].Play();
        }
    }

#if UNITY_EDITOR
    public void OnEditorUpdate(float dtime)
    {
        if (!Application.isPlaying)
        {
            for (int i = 0; i < effectList.Count; ++i)
            {
                effectList[i].OnTimelineUpdate(dtime);
            }
        }
    }
#endif

    public void Clear()
    {
        if (effectList != null)
        {
            for (int i = 1; i < effectList.Count; ++i)
            {
                if (effectList[i])
                    GameObject.Destroy(effectList[i].gameObject);
            }
            effectList.Clear();
        }
        //effectList = null;
    }

    public Vector3 position
    {
        set
        {
            for (int i = 1; i < effectList.Count; ++i)
            {
                if (effectList[i])
                    effectList[i].transform.position = value;
            }
        }
    }
}



/// <summary>
/// 特效轨迹功能类
/// </summary>
[System.Serializable]
public class ParticleSystemControlMixer : PlayableBehaviour
{

    //public ParticleSystemControlMixer():base()
    //{
    //    //Debug.LogError($"instance ParticleSystemControlMixer");
    //}

    public int InstanceID = 0;

    public enum SnapType
    {
        InitPos = 0,        //初始化位置        
        FollowPos = 1,      //跟随
        FlyTarget = 2,      //飞行目标
        LookatTarget = 3,   //初始化位置并朝向受击者
    }

    public enum SnapTargetType
    {
        self = 0,            //施法者自己
        target = 1,         //施法目标
        pet = 2,            //宠物
        horse = 3,          //坐骑
        otherObj = 4,       //其它物体
        posByGame = 5,      //游戏中传入具体位置
        shengwu = 6,        //圣物
        boss_show = 7,       //boss登场
        placeholder = 8,
        self_guardian = 9,       //施法者守护灵
    }

    public enum SnapTargetMount
    {
        none = 0,
        leftHand = 1,
        rightHand =2,
        bothHand = 3,
        hair = 4,
        win = 5,
        sadde = 6,
        other = 7,
    }

    public enum FlyType
    {
        ToTarget = 0,
        TargetToMe = 1,
        Forward = 2
    }

    public enum FlySpeedType
    {
        ByTimeline = 0,
        ByDistance = 1,
    }

    #region Editor属性

    public ExposedReference<Transform> snapTarget;
    public ExposedReference<Transform> attacker;

    public Transform attackerObj;

   
    /// <summary>
    /// 时间轨道对象
    /// </summary>
    public TrackAsset parentTrack;
    public GameObject parentParent;

    [SerializeField]
    [HideInInspector]
    public uint randomSeed = 0xffffffff;


    [SerializeField]
    [HideInInspector]
    SnapTargetType _snapTartetType = SnapTargetType.self;

    [SerializeField]
    [HideInInspector]
    int _snapTargetId = 0;

    [SerializeField]
    [HideInInspector]
    bool _mutiInstance = false;//允许多实例

    [SerializeField]
    [HideInInspector]
    string _otherSnapTargetName = "";

    [SerializeField]
    [HideInInspector]
    SnapTargetMount _snapMount = SnapTargetMount.none;

    [SerializeField]
    [HideInInspector]
    string _mountOtherName = "";


    [HideInInspector]
    [SerializeField]
    Vector3 _posByGame = Vector3.zero;
    [HideInInspector]
    [SerializeField]
    Quaternion _rotationByGame = Quaternion.identity;

    [SerializeField]
    [HideInInspector]
    int _targetZhenYing;

    public void CloneSerializeFiled(ParticleSystemControlMixer target)
    {
        target.randomSeed = randomSeed;
        target._snapTartetType = _snapTartetType;
        target._snapTargetId = _snapTargetId;
        target._mutiInstance = _mutiInstance;
        target._otherSnapTargetName = _otherSnapTargetName;
        target._snapMount = _snapMount;
        target.mountOtherName = mountOtherName;
        target._posByGame = _posByGame;
        target._rotationByGame = _rotationByGame;
        target._targetZhenYing = _targetZhenYing;
    }

    #endregion

    #region 运行时属性

    public EffectBehaviour _effectObj;
    public uint _track_id = 0;

    int _hash = 0;
    TransformArray _SnapTargetList;
    private Transform recordTr;
    public TransformArray SnapTargetList
    {
        get => _SnapTargetList;
        set
        {
            if (value != _SnapTargetList)
            {
                _hash = this.GetHashCode();
                if (value != null)
                {
                    ++value.refcnt;
                    //Log.LogInfo($"TransformArray={value.GetHashCode()}, ref={value.refcnt} bind to {this.GetHashCode()}");
                }
                if (_SnapTargetList != null)
                {
                    //Log.LogInfo($"_SnapTargetList1={_SnapTargetList.GetHashCode()}, before ref={_SnapTargetList.refcnt} unbind from {this.GetHashCode()}");
                    TransformArray.Release(_SnapTargetList);
                }
                _SnapTargetList = value;
            }
        }
    }

    public override void OnPlayableCreate(Playable playable)
    {
        base.OnPlayableCreate(playable);
        if (_hash != this.GetHashCode())
        {
            _hash = this.GetHashCode();
            if (_SnapTargetList != null)
            {
                ++_SnapTargetList.refcnt;
                //Log.LogInfo($"_SnapTargetList2={_SnapTargetList.GetHashCode()}, ref={_SnapTargetList.refcnt} bind to {this.GetHashCode()}");
            }
        }
    }

    public SnapTargetType snapTartetType { get { return _snapTartetType; } set { _snapTartetType = value; } }
    public int snapTargetId { get { return _snapTargetId; }set { _snapTargetId = value; } }
    
    public SnapTargetMount snapMount { get { return _snapMount; } set { _snapMount = value; } }
    public string mountOtherName { get { return _mountOtherName; } set { _mountOtherName = value;  } }
    public string otherSnapTargetName { get { return _otherSnapTargetName; } set { _otherSnapTargetName = value; } }

    /// <summary>
    /// 允许多实例
    /// </summary>
    public bool mutiInstance { get { return _mutiInstance; } set { _mutiInstance = value; } }

    public int targetZhenYing { get { return _targetZhenYing; } set { _targetZhenYing = value; } }

    /// <summary>
    /// 游戏中传入的位置
    /// </summary>
    public Vector3 PosByGame { get { return _posByGame; }set { _posByGame = value; } }
    /// <summary>
    /// 游戏中传入的朝向
    /// </summary>
    public Quaternion RotationByGame { get { return _rotationByGame; }set { _rotationByGame = value; } }

    #endregion

    public ParticleSystemControlMixer() 
    {
        //Log.LogInfo($"new ParticleSystemControlMixer={GetHashCode()}, SnapTargetList={SnapTargetList?.GetHashCode()}");
    }

    ~ParticleSystemControlMixer() 
    {
        SnapTargetList = null;
    }


    #region 私有成员
    bool _needRestart = false;
    bool _needStop = true;
    EffectBehaviourList particleSystem;

    int ____mountCount = 1;

    void PrepareParticleSystem(Playable playable)
    {
        if (particleSystem == null || particleSystem.PsCount == 0) return;

        // Disable automatic random seed to get deterministic results.
       // if (particleSystem.useAutoRandomSeed)
       //     particleSystem.useAutoRandomSeed = false;

        // Override the random seed number.
       // if (particleSystem.randomSeed != randomSeed)
       //     particleSystem.randomSeed = randomSeed;

        // Retrieve the total duration of the track.
      //  var rootPlayable = playable.GetGraph().GetRootPlayable(0);
      //  var duration = (float)rootPlayable.GetDuration();

        // Particle system duration should be longer than the track duration.
       // var main = particleSystem.main;
       // if (main.duration < duration) main.duration = duration;
    }

    void ResetSimulation(float time)
    {
        if (particleSystem == null || particleSystem.PsCount == 0) return;
        const float maxSimTime = 2.0f / 3;

        if (time < maxSimTime)
        {
            // The target time is small enough: Use the default simulation
            // function (restart and simulate for the given time).
            particleSystem.Simulate(time);
        }
        else
        {
            // The target time is larger than the threshold: The default
            // simulation can be heavy in this case, so use fast-forward
            // (simulation with just a single step) then simulate for a small
            // period of time.
            particleSystem.Simulate(time - maxSimTime, true, true, false);
            particleSystem.Simulate(maxSimTime, true, false, true);
        }
    }

    void InitEffectList()
    {
        ClearEffectList();       

        particleSystem = particleSystem ?? new EffectBehaviourList();
        particleSystem.Init(_effectObj, SnapTargetList != null ? SnapTargetList.Count * ____mountCount : 0);
        if (!_effectObj)
        {
            //Log.LogError($"error:ParticleSystem is null");
            return;
        }       
    }

    void ClearEffectList()
    {
        if(particleSystem != null)
            particleSystem.Clear();
        //particleSystem = null;
    }


    #endregion

    public void OnEnd()
    {
        ClearEffectList();
        if (recordTr) GameObjectUtils.Destroy(recordTr.gameObject);
    }



    #region PlayableBehaviour overrides

    public override void OnGraphStart(Playable playable)
    {
        _needStop = true;
        if (particleSystem == null) return;        

        if (Application.isPlaying)
        {
            // Play mode: Prepare particle system only on graph start.
            particleSystem.Stop();
            PrepareParticleSystem(playable);
        }
    }

    public override void OnGraphStop(Playable playable)
    {
        //Log.LogError("ParticleSystemControlMixer on stop");
        OnEnd();
    }

    bool _error = false;
    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        _error = false;
        //Log.LogError($"OnBehaviourPlay {parentParent}");
        base.OnBehaviourPlay(playable, info);        
    }


    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (_error) 
        {
            return;
        }
        _error = true;

        // clip总数
        var clipCount = playable.GetInputCount();
        var rootPlayable = playable.GetGraph().GetRootPlayable(0);
        

        //当前时间轴的时间
        var time = (float)rootPlayable.GetTime();
        var clips = parentTrack.GetClips() as TimelineClip[];
        var totalTime = (float)rootPlayable.GetDuration();

        //if (time >= totalTime)
        //{          
        //    OnEnd();
        //    _error = false;
        //    return;
        //}

        UnityEngine.Profiling.Profiler.BeginSample("Split mountOtherName");
        ____mountCount = 1;
        if (Application.isPlaying && _snapMount == SnapTargetMount.other && !string.IsNullOrEmpty((parentTrack as ParticleSystemControlTrack).template.mountOtherName) && (parentTrack as ParticleSystemControlTrack).template.mountOtherName.Contains(','))
        {
            foreach (var c in (parentTrack as ParticleSystemControlTrack).template.mountOtherName)
            {
                if (c == ',')
                {
                    ++____mountCount;
                }
            }
            //____mountCount = (parentTrack as ParticleSystemControlTrack).template.mountOtherName.Split(',').Length;
        }
        UnityEngine.Profiling.Profiler.EndSample();

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (!attackerObj)
            {
                attackerObj = attacker.Resolve(playable.GetGraph().GetResolver());
                if (attackerObj)
                {
                    attackerPos = attackerObj.transform.position;
                    attackerRot = attackerObj.transform.rotation;
                }
            }

            if (SnapTargetList == null)
            {
                var tran = snapTarget.Resolve(playable.GetGraph().GetResolver());
                if (tran)
                {
                    SnapTargetList = TransformArray.Get(tran);// new TransformArray(tran);
                }
                //
                if (
                    SnapTargetList == null
                    ||
                    SnapTargetList.IsEmpty
                   )
                {
                    SnapTargetList = TransformArray.Get(_effectObj.transform);// new TransformArray(_effectObj.transform);
                }
            }
        }
#endif

        if (SnapTargetList != null && SnapTargetList.isInCache != 0)
        {
            Log.LogInfo($"ParticleSystemControlMixer={this.GetHashCode()}, SnapTargetList={SnapTargetList.GetHashCode()}");
        }

        int targetCount = SnapTargetList != null ? SnapTargetList.Count : 0;

        if (particleSystem == null || particleSystem.Count != targetCount * ____mountCount)
        {
            UnityEngine.Profiling.Profiler.BeginSample("InitEffectList");
            InitEffectList();
            UnityEngine.Profiling.Profiler.EndSample();
        }

        if (!Application.isPlaying && !particleSystem.isPlaying)
        {
            UnityEngine.Profiling.Profiler.BeginSample("PrepareParticleSystem");
            PrepareParticleSystem(playable);
            UnityEngine.Profiling.Profiler.EndSample();
        }

        if (needStartList.Count != targetCount || needStopList.Count != targetCount)
        {           
            needStopList.Clear();
            needStartList.Clear();
            for (int i = 0; i < targetCount; ++i)
            {
                needStartList.Add(false);              
                needStopList.Add(true);
            }
        }


        // Emission rates control
        //var totalOverTime = 0.0f;
        // var totalOverDist = 0.0f;

        //for (var i = 0; i < clips.Length; i++)
        //{
        //    var clipData = (clips[i].asset as ParticleSystemControlClip).template; // ((ScriptPlayable<ParticleSystemControlPlayable>)playable.GetInput(i)).GetBehaviour();

        //    var w = playable.GetInputWeight(i);
        //    totalOverTime += clipData.rateOverTime * w;
        //    totalOverDist += clipData.rateOverDistance * w;
        //}
        for (int i = 0; i < clips.Length; i++)
        {
            UnityEngine.Profiling.Profiler.BeginSample("OnClipUpdate");
            OnClipUpdate(time, i, clipCount, clips[i]);
            UnityEngine.Profiling.Profiler.EndSample();
        }

        _error = false;

    }

    IEnumerator WaitSetPosi(Transform objTr, ParticleSystemControlPlayable _clipTemplateData,Vector3 posi, EffectBehaviour effect)
    {
        int waitCount = 20;
        while (!objTr.GetComponentInParent<ObjectBehaviourBase>() && waitCount > 0)
        {
            yield return null;
            waitCount--;
        }
        var parentGO = objTr.GetComponentInParent<ObjectBehaviourBase>();
        if (parentGO && effect)
        {
            Vector3 tmp = parentGO.transform.TransformPoint(_clipTemplateData.Offset);
            effect.transform.position = tmp + (_clipTemplateData.randAngleAndDistance ? getRandOffset(posi, _clipTemplateData.randOffsetDistanceMin, _clipTemplateData.randOffsetDistanceMax) : GetLocalOffset(objTr, _clipTemplateData.localAngle, _clipTemplateData.localDistance));
            effect.transform.rotation = Quaternion.Euler(objTr.eulerAngles + _clipTemplateData.RotationOffset);
        }
    }

    IEnumerator WaitSetAnimSpeed(EffectBehaviour objEffect, float animSpeed)
    {
        if (!objEffect) yield break;
        int waitCount = 100;
        while (objEffect && objEffect.transform && waitCount > 0 && objEffect.transform.childCount == 0)
        {
            yield return null;
            waitCount--;
        }
        if (!objEffect) yield break;
        objEffect.Speed = animSpeed;
    }

    public Vector3 attackerPos = Vector3.zero;
    public Quaternion attackerRot = Quaternion.identity;
    List<bool> needStartList = new List<bool>();
    List<bool> needStopList = new List<bool>();
    public void OnClipUpdate(float time, int cur_idx, int total_clip, TimelineClip currentClip)
    {
        var particleSystem = this.particleSystem;
        var SnapTargetList = this.SnapTargetList;
#if UNITY_EDITOR
        if (SnapTargetList != null && SnapTargetList.isInCache != 0)
        {
            Log.LogInfo($"OnClipUpdate, SnapTargetList={SnapTargetList.GetHashCode()},{SnapTargetList.isInCache},{SnapTargetList.refcnt}, ParticleSystemControlMixer={this.GetHashCode()}");
        }
#endif
        int targetCount = SnapTargetList != null ? SnapTargetList.Count : 0;

        var _clipData = currentClip.asset as ParticleSystemControlClip;
        var _clipTemplateData = _clipData.GetTemplate(InstanceID);
        if (_clipTemplateData == null)
        {
            Log.LogWarning($"OnClipUpdate, _clipTemplateData is null, InstanceID={InstanceID}, _clipData={_clipData.GetInstanceID()},{_clipData.name},{_clipData.OwningClip.displayName}, parentTrack={_clipData.parentTrack?.name}, {_clipData.parentTrack?.timelineAsset}");
            return;
        }

        if (targetCount == 0)
        {
            //Log.LogError($"track_id:{_track_id}:SnapTargetList.cou == 0");
        }

        for (int i = 0; i < targetCount; ++i)
        {
            UnityEngine.Profiling.Profiler.BeginSample("loop targetCount");
            {
                if (SnapTargetList.ObjectList[i])
                {
                    if (!_clipTemplateData.isRecordOrigin)
                    {
                        SnapTargetList.PositionList[i] = SnapTargetList.ObjectList[i].transform.position;
                        SnapTargetList.rotationList[i] = SnapTargetList.ObjectList[i].transform.rotation;
                    }
                    else
                    {
                        if (recordTr is null)
                        {
                            recordTr = new GameObject("particle tr temp").transform;
                            recordTr.position = _SnapTargetList.ObjectList[i].position;
                            recordTr.rotation = _SnapTargetList.ObjectList[i].rotation;
                        }
                    }
                }

                float clipStarttime = (float)currentClip.start;
                float clipEndtime = (float)currentClip.end;
                float clipDurations = (float)currentClip.duration;
                float speed = 0;

                var dis = Vector3.Distance(attackerPos, SnapTargetList.PositionList[i]);  //距离

                if (_clipTemplateData.startTimeType == SkillHitTimelineClip.StartTimeType.ByDistance && _clipTemplateData.snapType != SnapType.FlyTarget)
                {
                    clipStarttime = dis / _clipTemplateData.flySpeedBefore + (float)currentClip.start;    //开始时间
                    clipEndtime = clipStarttime + (float)(currentClip.end - currentClip.start);             //结束时间

                    // Log.LogError($"clipStarttime={clipStarttime},dis={dis},speed={_clipData.template.flySpeedBefore},attackerPos={attackerPos},targetPos ={SnapTargetList.ObjectList[i].transform.position},target={SnapTargetList.ObjectList[i].name},attacker={attackerObj.name}");
                }

                float clipCurentTime = time - clipStarttime;


                if (_clipTemplateData.snapType == SnapType.FlyTarget)
                {
                    if (_clipTemplateData.flySpeedType == FlySpeedType.ByDistance && _clipTemplateData.flySpeed > 0f)
                    {
                        if (_clipTemplateData.flyType == FlyType.Forward)
                        {
                            speed = _clipTemplateData.flySpeed;
                        }
                        else
                        {
                            clipDurations = dis / _clipTemplateData.flySpeed;                                //总时长
                            clipEndtime = clipStarttime + clipDurations;
                            speed = _clipTemplateData.flySpeed;
                        }
                    }
                    else
                    {
                        speed = dis / clipDurations;
                    }
                }

                if (clipCurentTime > 0)
                {
                    if (clipCurentTime < clipDurations)
                    {
                        if (needStartList[i] ||
                            (particleSystem != null && particleSystem.effectList != null && particleSystem.effectList.Count > i && particleSystem.effectList[i] && !particleSystem.effectList[i].gameObject.IsActive())
                            )
                        {
                            //第一次显示时，初始化位置
                            UnityEngine.Profiling.Profiler.BeginSample("init");
                            if (_snapMount != SnapTargetMount.none)
                            {
                                // Log.LogError((parentTrack as ParticleSystemControlTrack).template.mountOtherName);
                                UnityEngine.Profiling.Profiler.BeginSample("mountOtherName.Split");
                                string[] mountNames = (parentTrack as ParticleSystemControlTrack).template.mountOtherName.Split(',');
                                UnityEngine.Profiling.Profiler.EndSample();
                                //
                                for (int n = 0; n < ____mountCount; ++n)
                                {
                                    var mountIdx = i + targetCount * n;
                                    if (particleSystem != null && particleSystem.effectList != null && particleSystem.effectList.Count > mountIdx && particleSystem.effectList[mountIdx] && SnapTargetList.ObjectList[i])
                                    {
                                        if (!_clipTemplateData.ignoreTime) particleSystem.effectList[mountIdx].gameObject.SetActiveAndResert();
                                        var mountParent = SnapTargetList.ObjectList[i].gameObject.FindChild(mountNames[n], false, false);
                                        mountParent = mountParent ? mountParent : SnapTargetList.ObjectList[i].gameObject;
                                        particleSystem.effectList[mountIdx].gameObject.transform.parent = mountParent.transform;
                                        particleSystem.effectList[mountIdx].gameObject.transform.localPosition = _clipTemplateData.Offset;
                                        particleSystem.effectList[mountIdx].gameObject.transform.localRotation = Quaternion.Euler(_clipTemplateData.RotationOffset);
                                        particleSystem.effectList[mountIdx].gameObject.transform.localScale = _clipTemplateData.ScaleOffset;
                                    }
                                }
                            }
                            else
                            {
                                //if (particleSystem != null && particleSystem.effectList != null && particleSystem.effectList.Count > i && particleSystem.effectList[i])
                                //    particleSystem.effectList[i].gameObject.SetActiveAndResert();

                                if (particleSystem != null && particleSystem.effectList != null && particleSystem.effectList.Count > i && particleSystem.effectList[i])
                                {
                                    if (!_clipTemplateData.ignoreTime)
                                    {
                                        particleSystem.effectList[i].gameObject.SetActiveAndResert();
                                    }

                                    if (_clipTemplateData.snapType == SnapType.InitPos)
                                    {
                                        UnityEngine.Profiling.Profiler.BeginSample("InitPos");
                                        if (Application.isPlaying && snapTartetType == SnapTargetType.posByGame)
                                        {
                                            particleSystem.effectList[i].transform.position = _posByGame;
                                            particleSystem.effectList[i].transform.rotation = _rotationByGame;
                                        }
                                        else
                                        {
                                            if (_clipTemplateData.Offset.x != 0 || _clipTemplateData.Offset.z != 0)
                                            {
                                                if (SnapTargetList.ObjectList[i])
                                                {
                                                    if (_clipTemplateData.isRecordOrigin && recordTr)
                                                    {
                                                        Vector3 tmp = recordTr.TransformPoint(_clipTemplateData.Offset);
                                                        particleSystem.effectList[i].transform.position = tmp + (_clipTemplateData.randAngleAndDistance ? getRandOffset(SnapTargetList.PositionList[i], _clipTemplateData.randOffsetDistanceMin, _clipTemplateData.randOffsetDistanceMax) : getLocalOffset(SnapTargetList.PositionList[i], attackerPos, _clipTemplateData.localAngle, _clipTemplateData.localDistance));
                                                        particleSystem.effectList[i].transform.rotation = Quaternion.Euler(recordTr.eulerAngles + _clipTemplateData.RotationOffset);
                                                    }
                                                    else
                                                    {
                                                        var parentGO = SnapTargetList.ObjectList[i].GetComponentInParent<ObjectBehaviourBase>();
                                                        if (parentGO)
                                                        {
                                                            Vector3 tmp = parentGO.transform.TransformPoint(_clipTemplateData.Offset);
                                                            particleSystem.effectList[i].transform.position = tmp + (_clipTemplateData.randAngleAndDistance ? getRandOffset(SnapTargetList.PositionList[i], _clipTemplateData.randOffsetDistanceMin, _clipTemplateData.randOffsetDistanceMax) : getLocalOffset(SnapTargetList.PositionList[i], attackerPos, _clipTemplateData.localAngle, _clipTemplateData.localDistance));
                                                            particleSystem.effectList[i].transform.rotation = Quaternion.Euler(parentGO.transform.eulerAngles + _clipTemplateData.RotationOffset);
                                                        }
                                                        else
                                                        {
                                                            Vector3 tmp = SnapTargetList.ObjectList[i].TransformPoint(_clipTemplateData.Offset);
                                                            particleSystem.effectList[i].transform.position = tmp + (_clipTemplateData.randAngleAndDistance ? getRandOffset(SnapTargetList.PositionList[i], _clipTemplateData.randOffsetDistanceMin, _clipTemplateData.randOffsetDistanceMax) : getLocalOffset(SnapTargetList.PositionList[i], attackerPos, _clipTemplateData.localAngle, _clipTemplateData.localDistance));
                                                            particleSystem.effectList[i].transform.rotation = Quaternion.Euler(SnapTargetList.ObjectList[i].eulerAngles + _clipTemplateData.RotationOffset);
                                                            //MyTask.Run(WaitSetPosi(SnapTargetList.ObjectList[i], _clipTemplateData, SnapTargetList.PositionList[i], particleSystem.effectList[i]));
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                particleSystem.effectList[i].transform.position = SnapTargetList.PositionList[i] + _clipTemplateData.Offset + (_clipTemplateData.randAngleAndDistance ? getRandOffset(SnapTargetList.PositionList[i], _clipTemplateData.randOffsetDistanceMin, _clipTemplateData.randOffsetDistanceMax) : getLocalOffset(SnapTargetList.PositionList[i], attackerPos, _clipTemplateData.localAngle, _clipTemplateData.localDistance));
                                                particleSystem.effectList[i].transform.rotation = _clipTemplateData.RotationOffset == Vector3.zero ? SnapTargetList.rotationList[i] : Quaternion.Euler(SnapTargetList.rotationList[i].eulerAngles + _clipTemplateData.RotationOffset);
                                            }
                                        }
                                        UnityEngine.Profiling.Profiler.EndSample();
                                    }
                                    else if (_clipTemplateData.snapType == SnapType.LookatTarget)
                                    {
                                        UnityEngine.Profiling.Profiler.BeginSample("LookatTarget");
                                        if (Application.isPlaying && snapTartetType == SnapTargetType.posByGame)
                                        {
                                            particleSystem.effectList[i].transform.position = _posByGame;
                                            particleSystem.effectList[i].transform.forward = IgnoreForwardHeightDiff(_posByGame - attackerPos, _clipTemplateData.IgnoreHeightDiff);
                                        }
                                        else
                                        {
                                            particleSystem.effectList[i].transform.position = attackerPos + _clipTemplateData.Offset + (_clipTemplateData.randAngleAndDistance ? getRandOffset(SnapTargetList.PositionList[i], _clipTemplateData.randOffsetDistanceMin, _clipTemplateData.randOffsetDistanceMax) : getLocalOffset(SnapTargetList.PositionList[i], attackerPos, _clipTemplateData.localAngle, _clipTemplateData.localDistance));
                                            particleSystem.effectList[i].transform.forward = IgnoreForwardHeightDiff(SnapTargetList.PositionList[i] - attackerPos, _clipTemplateData.IgnoreHeightDiff);
                                        }
                                        UnityEngine.Profiling.Profiler.EndSample();
                                    }
                                    else if (_clipTemplateData.snapType == SnapType.FlyTarget)
                                    {
                                        UnityEngine.Profiling.Profiler.BeginSample("FlyTarget");
                                        if (_clipTemplateData.flyType == FlyType.ToTarget
                                            || _clipTemplateData.flyType == FlyType.Forward)
                                        {
                                            if (Application.isPlaying && snapTartetType == SnapTargetType.posByGame)
                                            {
                                                particleSystem.effectList[i].transform.position = attackerPos;
                                                particleSystem.effectList[i].transform.rotation = attackerRot;
                                            }
                                            else
                                            {
                                                particleSystem.effectList[i].transform.position = attackerPos + _clipTemplateData.Offset + (_clipTemplateData.randAngleAndDistance ? getRandOffset(SnapTargetList.PositionList[i], _clipTemplateData.randOffsetDistanceMin, _clipTemplateData.randOffsetDistanceMax) : getLocalOffset(attackerPos + _clipTemplateData.Offset, SnapTargetList.PositionList[i] + _clipTemplateData.OffsetTarget, _clipTemplateData.localAngle, _clipTemplateData.localDistance));
                                                particleSystem.effectList[i].transform.rotation = Quaternion.Euler(attackerRot.eulerAngles + _clipTemplateData.RotationOffset);
                                            }

                                        }
                                        else if (_clipTemplateData.flyType == FlyType.TargetToMe)
                                        {
                                            if (Application.isPlaying && snapTartetType == SnapTargetType.posByGame)
                                            {
                                                particleSystem.effectList[i].transform.position = _posByGame;
                                                particleSystem.effectList[i].transform.rotation = _rotationByGame;
                                            }
                                            else
                                            {

                                                particleSystem.effectList[i].transform.position = SnapTargetList.PositionList[i] + _clipTemplateData.Offset + (_clipTemplateData.randAngleAndDistance ? getRandOffset(SnapTargetList.PositionList[i], _clipTemplateData.randOffsetDistanceMin, _clipTemplateData.randOffsetDistanceMax) : getLocalOffset(SnapTargetList.PositionList[i], attackerPos, _clipTemplateData.localAngle, _clipTemplateData.localDistance));
                                                particleSystem.effectList[i].transform.rotation = Quaternion.Euler(SnapTargetList.rotationList[i].eulerAngles + _clipTemplateData.RotationOffset);
                                            }
                                        }
                                        UnityEngine.Profiling.Profiler.EndSample();
                                    }
                                    else if (_clipTemplateData.snapType == SnapType.FollowPos)
                                    {
                                        UnityEngine.Profiling.Profiler.BeginSample("FollowPos");
                                        if (Application.isPlaying && snapTartetType == SnapTargetType.posByGame)
                                        {
                                            particleSystem.effectList[i].transform.position = _posByGame;
                                            particleSystem.effectList[i].transform.rotation = _rotationByGame;
                                        }
                                        else
                                        {
                                            Vector3 xiangdui = GetLocalOffset(SnapTargetList.ObjectList[i], _clipTemplateData.localAngle, _clipTemplateData.localDistance);
                                            particleSystem.effectList[i].transform.position = xiangdui + _clipTemplateData.Offset;
                                            if (!_clipTemplateData.dontFollowRotate)
                                            {
                                                particleSystem.effectList[i].transform.rotation = Quaternion.Euler(SnapTargetList.rotationList[i].eulerAngles + _clipTemplateData.RotationOffset);
                                            }
                                        }
                                        UnityEngine.Profiling.Profiler.EndSample();
                                    }

                                    particleSystem.effectList[i].transform.localScale = _clipTemplateData.ScaleOffset;
                                }
                            }

                            if (_clipData.template.animSpeed != 1)
                            {
                                UnityEngine.Profiling.Profiler.BeginSample("WaitSetAnimSpeed");
                                MyTask.Run(WaitSetAnimSpeed(particleSystem.effectList[i], _clipData.template.animSpeed));
                                UnityEngine.Profiling.Profiler.EndSample();
                            }
                            //
                            if (SnapTargetList.ObjectList[i])
                            {
                                var layer = _clipTemplateData.hideParticleNoEnemy && SnapTargetList.ObjectList[i].name.StartsWith("fightTimeline") ? (int)ObjLayer.Hidden : (int)ObjLayer.RoleEffect;
                                if (particleSystem.effectList[i] != null && particleSystem.effectList[i].gameObject.layer != layer)
                                {
                                    GameObjectUtils.SetLayerRecursively(particleSystem.effectList[i].gameObject, layer);
                                }
                            }
                            UnityEngine.Profiling.Profiler.EndSample();//init
                        }
                        else
                        {
                            //已经处于显示状态时，每帧计算位置

                            UnityEngine.Profiling.Profiler.BeginSample("calc");

                            if (particleSystem != null && particleSystem.effectList != null && particleSystem.effectList.Count > i && particleSystem.effectList[i])
                            {
                                if (!_clipTemplateData.ignoreTime && !particleSystem.effectList[i].gameObject.IsActive())
                                {
                                    particleSystem.effectList[i].gameObject.SetActiveAndResert();
                                }
                            }
                            //
                            if (_snapMount == SnapTargetMount.none)
                            {
                                if (particleSystem != null && particleSystem.effectList != null && particleSystem.effectList.Count > i && particleSystem.effectList[i] && SnapTargetList.ObjectList[i])
                                {
                                    if (_clipTemplateData.snapType == SnapType.FollowPos)
                                    {
                                        if (snapTartetType == SnapTargetType.posByGame && Application.isPlaying)
                                        {
                                            particleSystem.effectList[i].transform.position = _posByGame;
                                            particleSystem.effectList[i].transform.rotation = _rotationByGame;
                                        }
                                        else
                                        {
                                            Vector3 xiangdui = GetLocalOffset(SnapTargetList.ObjectList[i], _clipTemplateData.localAngle, _clipTemplateData.localDistance);
                                            particleSystem.effectList[i].transform.position = xiangdui + _clipTemplateData.Offset;
                                            if (!_clipTemplateData.dontFollowRotate)
                                            {
                                                particleSystem.effectList[i].transform.rotation = Quaternion.Euler(SnapTargetList.rotationList[i].eulerAngles + _clipTemplateData.RotationOffset);
                                            }
                                            var layer = _clipTemplateData.hideParticleNoEnemy && SnapTargetList.ObjectList[i] && SnapTargetList.ObjectList[i].name.StartsWith("fightTimeline") ? (int)ObjLayer.Hidden : (int)ObjLayer.RoleEffect;
                                            if (layer != particleSystem.effectList[i].gameObject.layer)
                                            {
                                                GameObjectUtils.SetLayerRecursively(particleSystem.effectList[i].gameObject, layer);
                                            }
                                        }
                                    }
                                    else if (_clipTemplateData.snapType == SnapType.FlyTarget)
                                    {

                                        //由于时间已经确定，只需要重力加速度或初始上抛速度其中一个即可，这里选择输入重力加速度速，计算出初始上抛速度
                                        Vector3 height_offset = Vector3.zero;
                                        if (_clipTemplateData.Gravity > 0)
                                        {
                                            var v0 = (_clipTemplateData.Gravity * clipDurations) / 2;
                                            height_offset = new Vector3(0, (v0 * clipCurentTime - _clipTemplateData.Gravity * (clipCurentTime * clipCurentTime) / 2f), 0);
                                        }
                                        
                                        Vector3 direction = Vector3.zero;
                                        if (_clipTemplateData.flyType == FlyType.ToTarget)
                                        {
                                            Vector3 hudu_pos = Vector3.zero;
                                            if (_clipTemplateData.hudu_offset != 0)
                                            {
                                                var huxing_dir = SnapTargetList.PositionList[i] - (attackerPos + _clipTemplateData.Offset);
                                                huxing_dir.y = 0;
                                                huxing_dir = Quaternion.AngleAxis(90f, Vector3.up) * huxing_dir.normalized;
                                                //
                                                var half = clipDurations / 2;
                                                var v = _clipTemplateData.hudu_offset * 2 / half;
                                                var a = v / half;
                                                var hudu_time = clipCurentTime;
                                                if (clipCurentTime > half)
                                                {
                                                    hudu_time = clipDurations - clipCurentTime;
                                                }
                                                hudu_pos = huxing_dir * (v * hudu_time - a * hudu_time * hudu_time / 2);
                                            }
                                            direction = -GetDirectionHeightDiff(attackerPos + _clipTemplateData.Offset, SnapTargetList.PositionList[i] + _clipTemplateData.OffsetTarget, _clipTemplateData.IgnoreHeightDiff);
                                            particleSystem.effectList[i].transform.position = hudu_pos + (attackerPos + _clipTemplateData.Offset) + direction * clipCurentTime * speed + height_offset + (_clipTemplateData.randAngleAndDistance ? getRandOffset(SnapTargetList.PositionList[i], _clipTemplateData.randOffsetDistanceMin, _clipTemplateData.randOffsetDistanceMax) : getLocalOffset(attackerPos + _clipTemplateData.Offset, SnapTargetList.PositionList[i] + _clipTemplateData.OffsetTarget, _clipTemplateData.localAngle, _clipTemplateData.localDistance));
                                        }
                                        else if (_clipTemplateData.flyType == FlyType.TargetToMe)
                                        {
                                            direction = -GetDirectionHeightDiff(SnapTargetList.PositionList[i] + _clipTemplateData.Offset, attackerPos + _clipTemplateData.OffsetTarget, _clipTemplateData.IgnoreHeightDiff);
                                            particleSystem.effectList[i].transform.position = (SnapTargetList.PositionList[i] + _clipTemplateData.Offset) + direction * clipCurentTime * speed + height_offset + (_clipTemplateData.randAngleAndDistance ? getRandOffset(SnapTargetList.PositionList[i], _clipTemplateData.randOffsetDistanceMin, _clipTemplateData.randOffsetDistanceMax) : getLocalOffset(SnapTargetList.PositionList[i] + _clipTemplateData.Offset, attackerPos + _clipTemplateData.OffsetTarget, _clipTemplateData.localAngle, _clipTemplateData.localDistance));
                                        }
                                        else if (_clipTemplateData.flyType == FlyType.Forward)
                                        {
                                            //direction = Quaternion.Euler(attackerRot.eulerAngles.x , attackerRot.eulerAngles.y+_clipTemplateData.localAngle, attackerRot.eulerAngles.z).eulerAngles;
                                            Quaternion q = Quaternion.Euler(0, _clipTemplateData.localAngle, 0);
                                            direction = q * attackerObj.transform.forward;
                                            //if(particleSystem.effectList[i].name == "fx_boss_002_skillattack_2_0_trail") Log.LogError($"direction:{direction},clipCurentTime:{clipCurentTime},speed:{speed}");
                                            particleSystem.effectList[i].transform.position = (attackerPos + _clipTemplateData.Offset) + direction * clipCurentTime * speed + height_offset + getLocalOffset(attackerPos + _clipTemplateData.Offset, SnapTargetList.PositionList[i] + _clipTemplateData.OffsetTarget, _clipTemplateData.localAngle, _clipTemplateData.localDistance);
                                        }
                                    }
                                }
                            }
                            else
                            {

                            }
                            UnityEngine.Profiling.Profiler.EndSample();
                            //if (particleSystem.effectList[i].PsCount > 0)
                            //{
                            //    var em = particleSystem.effectList[i].emissions;
                            //    for (int n = 0; n < em.Count; ++n)
                            //    {
                            //        var e = em[n];
                            //        e.rateOverTimeMultiplier = totalOverTime;
                            //        e.rateOverDistanceMultiplier = totalOverDist;
                            //    }
                            //}
                        }

                        needStopList[i] = true;
                        needStartList[i] = false;

                        // Time control
                        if (Application.isPlaying)
                        {
                            //// Play mode time control: Only resets the simulation when a large
                            //// gap between the time variables was found.
                            //var maxDelta = Mathf.Max(1.0f / 30, Time.smoothDeltaTime * 2);

                            //if (Mathf.Abs(clipCurentTime - particleSystem.time) > maxDelta)
                            //{
                            //    ResetSimulation(clipCurentTime);
                            //    particleSystem.Play();
                            //}
                        }
                        else
                        {
#if UNITY_EDITOR
                            // Edit mode time control
                            var minDelta = 1.0f / 240;
                            var smallDelta = Mathf.Max(0.1f, Time.fixedDeltaTime * 2);
                            var largeDelta = 0.2f;


                            particleSystem.OnEditorUpdate(clipCurentTime);


                            if (clipCurentTime < particleSystem.time ||
                                clipCurentTime > particleSystem.time + largeDelta)
                            {
                                // Backward seek or big leap
                                // Reset the simulation with the current playhead position.
                                ResetSimulation(clipCurentTime);
                            }
                            else if (clipCurentTime > particleSystem.time + smallDelta)
                            {
                                // Fast-forward seek
                                // Simulate without restarting but with fixed steps.
                                particleSystem.Simulate(clipCurentTime - particleSystem.time, true, false, true);
                            }
                            else if (clipCurentTime > particleSystem.time + minDelta)
                            {
                                // Edit mode playback
                                // Simulate without restarting nor fixed step.
                                particleSystem.Simulate(clipCurentTime - particleSystem.time, true, false, false);
                            }
                            else
                            {
                                // Delta time is too small; Do nothing.
                            }
#endif
                        }
                    }
                    else
                    {
                        if (needStopList[i] && particleSystem.effectList.Count > i && particleSystem.effectList[i])
                        {
                            if (!_clipTemplateData.ignoreTime) particleSystem.effectList[i].gameObject.SetActive(false);

                            if (_snapMount != SnapTargetMount.none)
                            {
                                for (int n = 1; n < ____mountCount; ++n)
                                {
                                    var mountIdx = i + targetCount * n;
                                    if (particleSystem != null && particleSystem.effectList != null && particleSystem.effectList.Count > mountIdx && particleSystem.effectList[mountIdx])
                                    {
                                        if (!_clipTemplateData.ignoreTime) particleSystem.effectList[mountIdx].gameObject.SetActive(false);
                                    }
                                }
                            }
                        }

                        needStartList[i] = true;
                        needStopList[i] = false;
                    }

                }
                else
                {
                    if (attackerObj)
                    {
                        attackerPos = attackerObj.transform.position;
                        attackerRot = attackerObj.transform.rotation;
                    }
                    if (needStopList[i] && particleSystem.effectList.Count > i && particleSystem.effectList[i])
                    {
                        if (!_clipTemplateData.ignoreTime) particleSystem.effectList[i].gameObject.SetActive(false);
                    }
                    needStopList[i] = false;
                    needStartList[i] = true;
                }                
            }
            UnityEngine.Profiling.Profiler.EndSample();
        }
    }
    Vector3 IgnoreForwardHeightDiff(Vector3 forward,bool ignoreHight)
    {
        return ignoreHight ? new Vector3(forward.x, 0, forward.z) : forward;
    }

    Vector3 GetDirectionHeightDiff(Vector3 pos1,Vector3 pos2, bool ignoreHight)
    {
        var p1 = ignoreHight ? new Vector3(pos1.x, 0, pos1.z) : pos1;
        var p2 = ignoreHight ? new Vector3(pos2.x, 0, pos2.z) : pos2;

        return (p1 - p2).normalized;
    }

    Vector3 getRandOffset(Vector3 pos,float minDistance,float maxDistance )
    {        
        Vector3 offset2 = Vector3.zero;

        var angle = UnityEngine.Random.Range(0, 360);
        var localDistance = UnityEngine.Random.Range(minDistance, maxDistance);

        float x, y;
        MathUtils.GetPosition(pos.x, pos.z, angle, localDistance, out x, out y);
        offset2 = new Vector3(x, pos.y, y) - pos;

        return offset2;
    }

    Vector3 getLocalOffset(Vector3 pos, Vector3 target,float localAngle,float localDistance)
    {
        Vector3 offset2 = Vector3.zero;
        if (localDistance != 0 )
        {
            var angle = 0f;
            var dx = target.x - pos.x;
            var dy = target.z - pos.z;

            if (dx != 0 || dy != 0)
            {
                angle = MathUtils.XZ2Degree(dx, dy);
            }
            else
            {
                return offset2;
            }

            MathUtils.GetPosition(pos.x, pos.z, (angle + localAngle) % 360, localDistance, out float x, out float y);
            offset2 = new Vector3(x, pos.y, y) - pos;
        }

        return offset2;
    }

    Vector3 GetLocalOffset(Transform tr, float localAngle, float localDistance)
    {
        if (!tr) return Vector3.zero;
        if (localDistance != 0)
        {
            Quaternion rot = tr.rotation;
            if (localAngle > 0)
            {
                //旋转localAngle角度
                rot = Quaternion.Euler(rot.eulerAngles.x, rot.eulerAngles.y + localAngle, rot.eulerAngles.z);
            }
            //旋转localAngle度之后，正前方localDistance米的位置
            Vector3 result = tr.position + (rot * Vector3.forward) * localDistance;
            return result;
        }
        return tr.position;
    }

    #endregion
}


