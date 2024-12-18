using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using static SkillHitTimelineClip;
using Random = UnityEngine.Random;

[System.Serializable]
public class SkillHitPlayableData : PlayableBehaviour
{

    [HideInInspector] [SerializeField] private ByHitType hitType = ByHitType.Normal;
    [HideInInspector] [SerializeField] private string eventContent;
    [HideInInspector]
    [SerializeField]
    private StartTimeType _startimeType = StartTimeType.ByTimeline;
    [HideInInspector]
    [SerializeField]
    private float _flySpeed = 0f;
    [HideInInspector]
    [SerializeField]
    private bool _addBuffer = false;
    [HideInInspector]
    [SerializeField]
    private bool _shockCamera = false;
    [HideInInspector]
    [SerializeField]
    private float _repealDistance;
    [HideInInspector]
    [SerializeField]
    private KnockFlyType flyType;
    [HideInInspector]
    [SerializeField]
    private float _upV0 = 0f;
    [HideInInspector]
    [SerializeField]
    private float _g = 10f;
    [HideInInspector]
    [SerializeField]
    ShockCameraEventType _shockCameraType = ShockCameraEventType.EveryOne;
    [HideInInspector]
    [SerializeField]
    Vector2[] flyFrames;        //关键帧，x表示时间,y表示高度
    [HideInInspector]
    [SerializeField]
    [ColorUsage(false, true)]
    Color _hitColor = Color.white;
    [HideInInspector]
    [SerializeField]
    float _hitColorScale = 0.144f;
    [HideInInspector]
    [SerializeField]
    float _hitColorBias = 1.155f;
    [HideInInspector]
    [SerializeField]
    float _weights = 1f;    //受击扣血权重(所有受击加起来不得超过1)

    [HideInInspector]
    [SerializeField]
    bool _byHitSound = false;
    [HideInInspector]
    [SerializeField]
    ShockCameraEventType _playSoundType = ShockCameraEventType.EveryOne;

    [HideInInspector]
    [SerializeField]
    string _playSoundName;

    [HideInInspector]
    [SerializeField]
    ByHitAnimType _byHitAnimType = ByHitAnimType.Hit;

    /// <summary>
    /// 怪在角色身前X米聚集
    /// </summary>
    [HideInInspector]
    [SerializeField]
    float _distJuGuai = 0;

    /// <summary>
    /// 聚怪圆形半径
    /// </summary>
    [HideInInspector]
    [SerializeField]
    float _radiusJuGuai = 0;


    [HideInInspector]
    [SerializeField]
    float _attack_before = 0;

    //临时 修正重复调用问题
    //public int frameCount;

    public SkillHitPlayableData() 
    {
        //Log.LogInfo($"new SkillHitPlayableData={this.GetHashCode()}, targetArray={targetArray?.GetHashCode()}");
    }
    ~SkillHitPlayableData() 
    {
        //Log.LogInfo($"~SkillHitPlayableData={this.GetHashCode()}, targetArray={targetArray?.GetHashCode()}");
        targetArray = null;
    }

    public void CloneSerializeFiled(SkillHitPlayableData target)
    {
        target.hitType = hitType;
        target.eventContent = eventContent;
        target._startimeType = _startimeType;
        target._flySpeed = _flySpeed;
        target._addBuffer = _addBuffer;
        target._shockCamera = _shockCamera;
        target._repealDistance = _repealDistance;
        target._upV0 = _upV0;
        target._g = _g;
        target._shockCameraType = _shockCameraType;
        target._byHitAnimType = _byHitAnimType;
        target._hitColor = _hitColor;
        target._hitColorScale = _hitColorScale;
        target._hitColorBias = _hitColorBias;
        target.flyType = flyType;
        target.flyFrames = flyFrames;
        target._playSoundType = _playSoundType;
        target._byHitSound = _byHitSound;
        target._weights = _weights;
        target.timeline_duration = timeline_duration;
        target._distJuGuai = _distJuGuai;
        target._radiusJuGuai = _radiusJuGuai;
        target._attack_before = _attack_before;
        //Log.LogInfo($"CloneSerializeFiled target={target.GetHashCode()}");
        target.isPlayHitList = false;
        target.isEndHitList = false;
        target._attack_before_called = false;
        target._playSoundName = _playSoundName;
    }


    public string EventContent { get { return eventContent; } set { eventContent = value; } }
    public ByHitType HitType { get { return hitType; } set { hitType = value; } }
    public StartTimeType startTimeType { get { return _startimeType; } set { _startimeType = value; } }
    /// <summary>
    /// 从攻击者到当前位置的速度，当startTimeType== ByDistance适用
    /// </summary>
    public float flySpeed { get { return _flySpeed; } set { _flySpeed = value; } }
    /// <summary>
    /// 是否附加buffer，取决于技能数据
    /// </summary>
    public bool addBuffer { get { return _addBuffer; } set { _addBuffer = value; } }
    /// <summary>
    /// 受击震屏
    /// </summary>
    public bool shockCamera { get { return _shockCamera; } set { _shockCamera = value; } }
    /// <summary>
    /// 受击震屏第件
    /// </summary>
    public ShockCameraEventType shockCameraType { get { return _shockCameraType; } set { _shockCameraType = value; } }

    /// <summary>
    /// 击退距离
    /// </summary>
    public float repealDistance { get { return _repealDistance; } set { _repealDistance = value; } }
    /// <summary>
    /// 击飞轨迹计算方式
    /// </summary>
    public KnockFlyType knockFlyType { get { return flyType; }set { flyType = value; } }
    /// <summary>
    /// 初始上抛速度
    /// </summary>
    public float upV0 { get { return _upV0; } set { _upV0 = value; } }
    /// <summary>
    /// 重力加速度
    /// </summary>
    public float ga { get { return _g; } set { _g = value; } }
    /// <summary>
    /// 受击动作类型
    /// </summary>
    public ByHitAnimType byHitAnimType { get { return _byHitAnimType; } set { _byHitAnimType = value; } }
    /// <summary>
    /// 受击颜色 
    /// </summary>
    public Color hitColor { get { return _hitColor; }set { _hitColor = value; } }
    /// <summary>
    /// 受击颜色倍数
    /// </summary>
    public float hitColorScale { get { return _hitColorScale; }set { _hitColorScale = value; } }
    /// <summary>
    /// 受击颜色范围
    /// </summary>
    public float hitColorBias { get { return _hitColorBias; }set { _hitColorBias = value; } }
    /// <summary>
    /// 受击扣血权重
    /// </summary>
    public float Weights { get { return _weights; }set { _weights = value; } }
    /// <summary>
    /// 受击声音
    /// </summary>
    public bool PlayByHitSound { get { return _byHitSound; }set { _byHitSound = value; } }
    /// <summary>
    /// 播放受击声音的条件
    /// </summary>
    public ShockCameraEventType ByHitSoundType { get { return _playSoundType; }set {  _playSoundType = value; } }

    public string ByHitSoundName => _playSoundName;

    //public float attack_before => _attack_before;
    //public float distJuGuai { get { return _distJuGuai; } set { _distJuGuai = value; } }
    //public float radiusJuGuai { get { return _radiusJuGuai; } set { _radiusJuGuai = value; } }


    public TimelineClip OwningClip;
    public long FightId { get; set; }
    public ObjectBehaviourBase attacker;
    public Vector3 attackerPosition;
    public Quaternion attackerRotation;

    public int GoInstID = 0;

#if UNITY_EDITOR
    [NonSerialized]
    public ObjectBehaviourBase targetObject;
#endif

    //public ObjectArray _next_targetArray;

    int _hash = 0;
    ObjectArray _targetArray;
    public ObjectArray targetArray
    {
        get => _targetArray;
        set
        {
            if (value != _targetArray)
            {
                _hash = this.GetHashCode();
                if (value != null)
                {
                    ++value.refcnt;
                    //Log.LogInfo($"value={value.GetHashCode()}, ref={value.refcnt}, count={value.Count} bind to {this.GetHashCode()}");
                }
                if (_targetArray != null)
                {
                    //Log.LogInfo($"_targetArray1={_targetArray.GetHashCode()}, before ref={_targetArray.refcnt} unbind from {this.GetHashCode()}");
                    ObjectArray.Release(_targetArray);
                }
                _targetArray = value;
            }
        }
    }

    public bool isPlayHitList;
    public bool isEndHitList;
    List<SkillHitTimelineClip.HitInfoParams> hitInfoList = new List<SkillHitTimelineClip.HitInfoParams>();
    List<Vector3> oldPosList = new List<Vector3>();
    List<Vector3> TimesByDistance = new List<Vector3>();

    bool _is_get_attack_pos = false;

    public double timeline_duration = 0f;// 时间轴总时间


    public override void OnPlayableCreate(Playable playable)
    {
        __isDebug = false;
        _is_get_attack_pos = false;
        var duration = playable.GetDuration();
        if (Mathf.Approximately((float)duration, 0))
        {
            throw new UnityException("A Clip Cannot have a duration of zero");
        }
        if (_hash != this.GetHashCode())
        {
            _hash = this.GetHashCode();
            if (_targetArray != null)
            {
                ++_targetArray.refcnt;
                //Log.LogInfo($"_targetArray2={_targetArray.GetHashCode()}, ref={_targetArray.refcnt} bind to {this.GetHashCode()}");
            }
        }
    }

    //[System.NonSerialized]
    //bool _isEnd = false;
    bool _attack_before_called = false;
    bool __isDebug = false;

    List<bool> _is_first_getDistance = new List<bool>();

    public bool UpdateBehaviour(float time, int cur_idx, int total_clip)
    {        
        if (OwningClip == null) return false;

#if UNITY_EDITOR
        if (targetArray?.isInCache != 0)
        {
            Log.LogInfo($"UpdateBehaviour={this.GetHashCode()}, targetArray={targetArray?.GetHashCode()}");
        }
        if (targetArray == null || targetArray.Count == 0 || !targetArray.ObjectList[0] || !Application.isPlaying)
        {
            if (targetObject)
            {
                targetArray = ObjectArray.Get(targetObject);
                //++targetArray.refcnt;
            }
        }
#endif
        if (targetArray == null || targetArray.Count == 0) return time >= (float)OwningClip.end;        

        if (hitInfoList.Count != targetArray.Count || oldPosList.Count != targetArray.Count || _is_first_getDistance.Count != targetArray.Count || TimesByDistance.Count != targetArray.Count)
        {
            hitInfoList.Clear();
            oldPosList.Clear();
            _is_first_getDistance.Clear();
            TimesByDistance.Clear();
            for (int i = 0; i < targetArray.Count; ++i)
            {
                _is_first_getDistance.Add(false);
                hitInfoList.Add(new SkillHitTimelineClip.HitInfoParams(i));
                oldPosList.Add(targetArray.ObjectList[i] ? targetArray.ObjectList[i].transform.position : Vector3.zero);
                TimesByDistance.Add(Vector3.zero);
            }
        }


        //if (targetArray.PositionList == null || targetArray.PositionList.Count != targetArray.Count)
        //{
        //    targetArray.PositionList = new List<Vector3>();
        //}

        if (!_is_get_attack_pos && attacker)
        {
            _is_get_attack_pos = true;
            attackerPosition = attacker.transform.position;
            attackerRotation = attacker.transform.rotation;
        }

        bool isEndHitList =  this.isEndHitList;
        bool isPlayHitList = this.isPlayHitList;

        var iscalc_time = startTimeType == SkillHitTimelineClip.StartTimeType.ByDistance && attacker && flySpeed > 0;
        if (!iscalc_time) 
        {
            var startTime = (float)OwningClip.start;
            //Debug.LogWarning($"hit, idx={cur_idx}/{total_clip} time={time}/{Time.time}/{Time.frameCount}, start={startTime}, end={(float)OwningClip.end}");
            if (time < startTime)
            {
                if (!_attack_before_called && _attack_before > 0)
                {
                    if (time > startTime - _attack_before)
                    {
                        _attack_before_called = true;
                        if (attacker)
                        {
                            if (attacker.EventOnAttackBeforeHit != null)
                            {
                                attacker.EventOnAttackBeforeHit(FightId, cur_idx, total_clip, this);
                            }
                        }
                    }
                }
                return false;
            }            
        }

        var _isEnd = false;
        for (int i = 0;  i < targetArray.Count; ++i)
        {
            if (iscalc_time)
            {
                float dis = 0, s_time = 0, e_time = 0;
                if (!_is_first_getDistance[i])
                {
                    var _dis = Vector3.Distance(attackerPosition, targetArray.PositionList[i]);                   //距离
                    var _s_time = _dis / flySpeed + (float)OwningClip.start;                                      //开始时间
                    var _e_time = _s_time + (float)(OwningClip.end - OwningClip.start);                                       //结束时间     

                    //如果大于时间轴总时间
                    _s_time = Mathf.Min(_s_time, (float)timeline_duration - 0.05f);
                    _e_time = Mathf.Min(_e_time, (float)timeline_duration - 0.05f);

                    TimesByDistance[i] = new Vector3(_dis, _s_time, _e_time);
                    _is_first_getDistance[i] = true;
                }

                dis = TimesByDistance[i].x;
                s_time = TimesByDistance[i].y;
                e_time = TimesByDistance[i].z;

                if (i == 0 && time < s_time)
                {
                    if (!_attack_before_called && _attack_before > 0)
                    {
                        if (time > s_time - _attack_before)
                        {
                            _attack_before_called = true;
                            if (attacker)
                            {
                                if (attacker.EventOnAttackBeforeHit != null)
                                {
                                    attacker.EventOnAttackBeforeHit(FightId, cur_idx, total_clip, this);
                                }
                            }
                        }
                    }
                }
                float clipCurentTime = time - s_time;
                //Log.LogInfo($"time={time}, s_time={s_time}");
                if (time > s_time)
                {
                    if (!isPlayHitList)
                    {
                        this.isPlayHitList = true;
                        //this.isEndHitList = false;      
                        //Log.LogError($"{System.DateTime.Now.Second}.{System.DateTime.Now.Millisecond},distance={dis},starttime={s_time},e_time={e_time},time={time},speed={_timelineClip.flySpeed}");
                        hitInfoList[i].ZeroValues();
                        oldPosList[i] = targetArray.PositionList[i];
                        //Log.LogInfo($"OnEnterClip {i}/{targetArray.Count}, {this.GetHashCode()}, clip={cur_idx}/{total_clip}");
                        OnEnterClip(targetArray.ObjectList[i], cur_idx, total_clip, hitInfoList[i]);
                    }
                    if (time >= e_time)
                    {
                        //this.isPlayHitList = false;                        
                        if (!isEndHitList)
                        {
                            this.isEndHitList = true;
                            OnExitClip(targetArray.ObjectList[i], cur_idx, total_clip, hitInfoList[i], oldPosList[i]);                       
                        }
                        _isEnd = true;
                    }
                    else
                    {
                        if (isPlayHitList)
                        {
                            OnClipUpdate(targetArray.ObjectList[i], cur_idx, total_clip, clipCurentTime, hitInfoList[i], oldPosList[i]);
                        }
                    }                    
                }
            }
            else
            {
                var startTime = (float)OwningClip.start;
                float clipCurentTime = time - startTime;
                if (time > startTime)
                {                    
                    if (!isPlayHitList)
                    {
                        this.isPlayHitList = true;
                        //this.isEndHitList = false;
                        oldPosList[i] = targetArray.PositionList[i];
                        hitInfoList[i].ZeroValues();
                        //Debug.Log($"hit, idx={cur_idx}/{total_clip} OnEnterClip {i}/{targetArray.ObjectList.Count}");
                        OnEnterClip(targetArray.ObjectList[i], cur_idx, total_clip, hitInfoList[i]);
                    }

                    if (time >= (float)OwningClip.end)
                    {
                        //this.isPlayHitList = false;                        
                        if (!isEndHitList)
                        {
                            this.isEndHitList = true;
                            //Debug.Log($"hit, idx={cur_idx}/{total_clip} OnExitClip {i}/{targetArray.ObjectList.Count}");
                            OnExitClip(targetArray.ObjectList[i], cur_idx, total_clip, hitInfoList[i], oldPosList[i]);
                        }
                        _isEnd = true;
                    }
                    else
                    {
                        if (isPlayHitList)
                        {
                            //Debug.LogWarning($"hit, idx={cur_idx}/{total_clip} OnClipUpdate {i}/{targetArray.ObjectList.Count}");
                            OnClipUpdate(targetArray.ObjectList[i], cur_idx, total_clip, clipCurentTime, hitInfoList[i], oldPosList[i]);
                        }
                    }                    
                }
            }
        }

        return _isEnd;
    }


#if UNITY_EDITOR
    [NonSerialized]
    GameObject conjure = null;    
#endif

    void OnEnterClip(ObjectBehaviourBase target, int cur_idx, int total_clip, SkillHitTimelineClip.HitInfoParams hitInfo)
    {
        //Log.LogError($"attacker == null ?({attacker == null}),target==null?{(target == null)},instid={GoInstID}");
        if (target)
        {
            if (FightId == 0 && Application.isPlaying)
                Log.LogError($"FightId == 0");

            if(!attacker)
            {
               // if(Application.isPlaying)
               //     Log.LogError($"FightId={FightId},cur_idx={cur_idx},total_clip={total_clip},HitType={HitType},attacker is null");
                return;
            }

            //Log.LogError($"attacker == null ?({attacker == null}),instid={GoInstID}");

            target.OnByHit(attacker, new Vector2Int( cur_idx, total_clip),  this, hitInfo);
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (HitType == SkillHitTimelineClip.ByHitType.Conjure)
                {
                    var pos = GetRandomPos(target.transform.position.x, target.transform.position.z, 2, 4);

                    conjure = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    conjure.transform.position = new Vector3(pos.x, target.transform.position.y, pos.y);


                    UnityEditor.EditorApplication.CallbackFunction DelayCall = null;

                    DelayCall = () =>
                    {
                        UnityEditor.EditorApplication.delayCall -= DelayCall;
                        GameObject.DestroyImmediate(conjure);
                    };

                    UnityEditor.EditorApplication.delayCall += DelayCall;

                }else if (HitType == ByHitType.Normal) 
                {
                    if (_byHitSound && !string.IsNullOrEmpty(_playSoundName)) 
                    {
                        FMODEventPlayableBehavior.PlayAudio(_playSoundName);
                    }
                }
                else
                {

                    GPUAnimTimelineClip clip = new GPUAnimTimelineClip();
                    string clipName = "hit";
                    switch (byHitAnimType)
                    {
                        case SkillHitTimelineClip.ByHitAnimType.Hit:
                            if (target.IsByHitFly)
                                clipName = "hit_insky";
                            else
                                clipName = "hit";
                            break;
                        case SkillHitTimelineClip.ByHitAnimType.Jidao:
                            clipName = "hit_jidao";
                            break;
                        case SkillHitTimelineClip.ByHitAnimType.Jifei:
                            clipName = "hit_jifei";
                            target.IsByHitFly = true;
                            break;
                        case SkillHitTimelineClip.ByHitAnimType.NoAnim:
                            return;
                    }

                    clipName = target.GetAnimClipName(clipName);
                    if (string.IsNullOrEmpty(clipName)) clipName = "hit";

                    //Log.LogError($"byhit:{clipName}");

                    clip.ClipName = clipName;
                    clip.name = clipName;
                    clip.AnimatorSpeed = 1f;
                    target.PlayAnim(clip);
                }
            }
#endif
        }

    }

    void OnExitClip(ObjectBehaviourBase target, int cur_idx, int total_clip, SkillHitTimelineClip.HitInfoParams hitInfo, Vector3 old_pos)
    {
        if (target)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (HitType == SkillHitTimelineClip.ByHitType.KnockFly && hitInfo.UpV0 >= 0)
                {
                    target.transform.position = old_pos;
                }

                if (conjure)
                {
                    GameObject.DestroyImmediate(conjure);
                }

                if (byHitAnimType == SkillHitTimelineClip.ByHitAnimType.Jidao || byHitAnimType == SkillHitTimelineClip.ByHitAnimType.Jifei)
                {
                    GPUAnimTimelineClip clip = new GPUAnimTimelineClip();
                    var clipName = target.GetAnimClipName("qishen_2");
                    if (string.IsNullOrEmpty(clipName)) clipName = "qishen";

                    clip.ClipName = clipName;
                    clip.name = clipName;
                    clip.AnimatorSpeed = 1f;
                    target.PlayAnim(clip);
                }
                if (byHitAnimType == SkillHitTimelineClip.ByHitAnimType.Jifei)
                {
                    target.IsByHitFly = false;
                }
            }

#endif

            if (hitType == ByHitType.JuGuai) 
            {
                if (hitInfo.JuGuaiSpeed != Vector3.zero)
                {
                    target.transform.position = hitInfo.RepealDstPos;
                }
            }

            if (FightId <= 0 && Application.isPlaying && FightId > -9999)
            {
                Log.LogError($"FightId == 0");
            }
            target.OnByHitEnd(attacker, new Vector2Int( cur_idx, total_clip), this, hitInfo);
        }
    }

    /// <summary>
    /// 受击过程中每帧更新
    /// </summary>
    /// <param name="target"></param>
    /// <param name="cur_idx"></param>
    /// <param name="total_clip"></param>
    /// <param name="clipCurentTime"></param>
    void OnClipUpdate(ObjectBehaviourBase target, int cur_idx, int total_clip,float clipCurentTime, SkillHitTimelineClip.HitInfoParams hitInfo,Vector3 old_pos)
    {
        //Log.LogInfo($"target={target.objBehaviour}, HitType={HitType}");

        float totalTime = (float) OwningClip.duration;
        if (clipCurentTime >= totalTime) return;
        if (!target) return;

        if (HitType == SkillHitTimelineClip.ByHitType.Repel)
        {
            if (repealDistance != 0f && hitInfo.RepealDistance >= 0)
            {
                var distance = hitInfo.RepealDistance > 0 ? hitInfo.RepealDistance : repealDistance;
                var dstPos = hitInfo.RepealDstPos;
                if (dstPos == Vector3.zero)
                {
                    var angle = 0f;
                    var dx = old_pos.x - attackerPosition.x;
                    var dy = old_pos.z - attackerPosition.z;

                    if (dx != 0 || dy != 0)
                    {
                        angle = MathUtils.XZ2Degree(dx, dy);
                    }
                    else
                    {
                        return;
                    }

                    float dstx, dsty;
                    MathUtils.GetPosition(old_pos.x, old_pos.z, angle, distance, out dstx, out dsty);

                    dstPos = new Vector3(dstx, old_pos.y, dsty);
                    hitInfo.RepealDstPos = dstPos;
                    //Log.LogError($"clipCurentTime={clipCurentTime},totalTime={totalTime},distance={distance}");
                }

                var rate = easing.EaseUtils.GetRate(clipCurentTime, totalTime, easing.Back.easeOut);
                var x = Mathf.Lerp(old_pos.x, dstPos.x, rate);
                var y = Mathf.Lerp(old_pos.z, dstPos.z, rate);

                target.transform.position = new Vector3(x, old_pos.y, y);
            }
        }
        else if (HitType == SkillHitTimelineClip.ByHitType.KnockFly)
        {
            if (hitInfo.UpV0 >= 0)
            {
                if (knockFlyType == KnockFlyType.ByPhysical && upV0 > 0 && ga > 0)
                {
                    var _upV0 = hitInfo.UpV0 > 0 ? hitInfo.UpV0 : upV0;
                    var _ga = hitInfo.g > 0 ? hitInfo.g : ga;


                    //最小往返时间
                    //往返时间t=2*V0/g
                    var _totaltime = 2 * _upV0 / _ga;
                    // Log.LogError($"_t={_totaltime},totalTime={totalTime}");

                    //如果最小往返时间大于clip总时长，则重新计算初始上抛速度
                    // v = gt/2
                    if (_totaltime > totalTime)
                    {
                        _upV0 = (_ga * totalTime) / 2;
                        _totaltime = totalTime;
                    }


                    //上升阶段
                    if (clipCurentTime < (_totaltime / 2f))
                    {
                        //竖直上抛运动 位移s=V0t-g*pow(t,2)/2         
                        float height = (_upV0 * clipCurentTime - _ga * Mathf.Pow(clipCurentTime, 2) / 2);
                        target.transform.position = new Vector3(target.transform.position.x, old_pos.y + height, target.transform.position.z);
                    }

                    //顶部停留阶段
                    if (clipCurentTime >= (_totaltime / 2f) && clipCurentTime < (totalTime - _totaltime / 2f))
                    {

                    }

                    //下落阶段
                    if (clipCurentTime >= (totalTime - _totaltime / 2f) && clipCurentTime < totalTime)
                    {
                        var ctime = clipCurentTime - (totalTime - _totaltime / 2f) + _totaltime / 2f;
                        //竖直上抛运动 位移s=V0t-g*pow(t,2)/2        
                        float height = (_upV0 * ctime - _ga * Mathf.Pow(ctime, 2) / 2);
                        target.transform.position = new Vector3(target.transform.position.x, old_pos.y + height, target.transform.position.z);
                    }
                }
                else if (knockFlyType == KnockFlyType.ByKeyFrame && flyFrames.Length > 0)
                {
                    float _t = clipCurentTime / totalTime;      //当前时间进度(0-1)
                    int frameIdx = getCurrentFrameIdx(_t);
                    float srcHeight = 0;
                    float dstHeight = 0;
                    float srcTime = 0;
                    float dstTime = 0;
                    float progress = 0;

                    if (frameIdx == -1)
                    {
                        srcHeight = old_pos.y;
                        dstHeight = old_pos.y + flyFrames[frameIdx + 1].y;
                        srcTime = 0;
                        dstTime = flyFrames[frameIdx + 1].x * totalTime;
                    }
                    else if (frameIdx == flyFrames.Length - 1)
                    {
                        srcHeight = old_pos.y + flyFrames[frameIdx].y;
                        dstHeight = old_pos.y;

                        srcTime = flyFrames[frameIdx].x * totalTime;
                        dstTime = totalTime;
                    }
                    else
                    {
                        srcHeight = old_pos.y + flyFrames[frameIdx].y;
                        dstHeight = old_pos.y + flyFrames[frameIdx + 1].y;

                        srcTime = flyFrames[frameIdx].x * totalTime;
                        dstTime = flyFrames[frameIdx + 1].x * totalTime;
                    }

                    progress = (clipCurentTime - srcTime) / (dstTime - srcTime);
                    //Log.LogError($"aaaaaa:srcHeight={srcHeight},dstHeight={dstHeight},srcTime={srcTime},dstTime={dstTime},progress={progress},currentHeight={(srcHeight + (dstHeight - srcHeight) * progress)},frameIdx={frameIdx}");
                    target.transform.position = new Vector3(target.transform.position.x, srcHeight + (dstHeight - srcHeight) * progress, target.transform.position.z);

                }
            }
        }
        else if (HitType == ByHitType.JuGuai) 
        {
            if (hitInfo.JuGuaiSpeed != Vector3.zero)
            {
                target.transform.position += hitInfo.JuGuaiSpeed * Time.deltaTime;
            }
        }

        if (FightId == 0 && Application.isPlaying)
            Log.LogError($"FightId == 0");

        target.OnByHitUpdate(attacker, new Vector2Int( cur_idx, total_clip),  this, hitInfo);
    }

    public Vector2 GetRandomPos(float x, float y, float max_dist, float min_dist)
    {
        var times = Mathf.CeilToInt(max_dist * max_dist);

        var xmin = x - max_dist;
        var xmax = x + max_dist;
        var ymin = y - max_dist;
        var ymax = y + max_dist;
        var max_dist_2 = max_dist * max_dist;
        var min_dist_2 = min_dist * min_dist;


        for (int i = 0; i < times; i++)
        {
            System.Random r = new System.Random(System.DateTime.Now.Second);
            var xx = r.Next((int)(xmin * 100f),(int)( xmax * 100f)) / 100f;
            var yy = r.Next((int)(ymin * 100f), (int)(ymax * 100f)) / 100f;
            var diff = new Vector2(xx - x, yy - y);
            var dist2 = diff.sqrMagnitude;

            if (dist2 >= min_dist
                && dist2 <= max_dist_2)
            {
                return new Vector2(xx, yy);
            }
        }

        return new Vector2(x, y);
    }

    int getCurrentFrameIdx(float t)
    {
        int idx = -1;
        for(int i = 0; i < flyFrames.Length; ++i)
        {
            if (t >= flyFrames[i].x)
                idx = i;

            if (t < flyFrames[i].x)
                break;
        }

        return idx;
    }
}
