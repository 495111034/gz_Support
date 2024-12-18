using UnityEngine;
using UnityEngine.Playables;



/// <summary>
/// 特效clip的数据
/// </summary>
[System.Serializable]
public class ParticleSystemControlPlayable : PlayableBehaviour
{
   // public float rateOverTime = 10;
   // public float rateOverDistance = 0;

    /// <summary>
    /// 位置更新方式
    /// </summary>
    [SerializeField]
    [HideInInspector]
    ParticleSystemControlMixer.SnapType _snapType = ParticleSystemControlMixer.SnapType.FollowPos;

    /// <summary>
    /// 勾选则不跟随朝向
    /// </summary>
    [SerializeField]
    [HideInInspector]
    bool _dontFollowRotate = false;

    /// <summary>
    /// 飞行方式
    /// </summary>
    [SerializeField]
    [HideInInspector]
    ParticleSystemControlMixer.FlyType _flyType = ParticleSystemControlMixer.FlyType.ToTarget;

    ///// <summary>
    ///// 初始上抛速度
    ///// </summary>
    //[SerializeField]
    //[HideInInspector]
    //float _upSpeed = 0f;

        /// <summary>
        /// 忽略高度差值
        /// </summary>
    [SerializeField]
    [HideInInspector]
    bool ignoreHeightDiff = true;

    /// <summary>
    /// 重力加速度
    /// </summary>
    [SerializeField]
    [HideInInspector]
    float _gravity = 0f;

    /// <summary>
    /// 偏移量
    /// </summary>
    [SerializeField]
    [HideInInspector]
    Vector3 _offset = Vector3.zero;
    /// <summary>
    /// 目标点偏移量
    /// </summary>
    [SerializeField]
    [HideInInspector]
    Vector3 _offset_target = Vector3.zero;
    [SerializeField]
    [HideInInspector]
    float _localAngle = 0f;
    [SerializeField]
    [HideInInspector]
    float _localoffsetDistance = 0f;
    [SerializeField]
    [HideInInspector]
    bool _randAngleAndDistance = false;
    [SerializeField]
    [HideInInspector]
    float _randOffsetDistanceMin = 0f;
    [SerializeField]
    [HideInInspector]
    float _randOffsetDistanceMax = 0f;
    //旋转偏移
    [SerializeField]
    [HideInInspector]
    Vector3 _rotationOffset = Vector3.zero;
    [SerializeField]
    [HideInInspector]
    Vector3 _scaleOffset = Vector3.one;

    [SerializeField]
    [HideInInspector]
    ParticleSystemControlMixer.FlySpeedType _flySpeedType = ParticleSystemControlMixer.FlySpeedType.ByTimeline;
    [HideInInspector]
    [SerializeField]
    private SkillHitTimelineClip.StartTimeType _startimeType = SkillHitTimelineClip.StartTimeType.ByTimeline;
    /// <summary>
    /// 飞行速度，用于计算结束时间，仅当_flySpeedType为ByDistance时适用
    /// </summary>
    [SerializeField]
    [HideInInspector]
    float _flySpeed = 0f;

    /// <summary>
    /// 飞行速度，用于计算开始时间
    /// </summary>
    [SerializeField]
    [HideInInspector]
    float _flySpeed_before = 0f;

    /// <summary>
    /// 特效速度系数  适用于Animation特效
    /// </summary>
    [SerializeField]
    [HideInInspector]
    float _anim_speed = 1f;


    /// <summary>
    /// 记录初始目标位置
    /// </summary>
    [SerializeField]
    [HideInInspector]
    bool _is_record_origin = false;

    /// <summary>
    /// 无对象空放时，隐藏特效。为true时隐藏，默认不隐藏。
    /// 例：战士普攻勾选，没打到人，不显示刀口的火星（受击特效）
    /// </summary>
    [SerializeField]
    [HideInInspector]
    bool _hide_particle_no_enemy = false;


    [SerializeField]
    [HideInInspector]
    float _hudu_offset = 0f;

    /// <summary>
    /// 显隐不受时间轴控制
    /// </summary>
    [SerializeField]
    [HideInInspector]
    bool _ignoretime = false;

    public void CloneSerializeFiled(ParticleSystemControlPlayable target)
    {
        target._snapType = _snapType;
        target._flyType = _flyType;
        target._gravity = _gravity;
        target._offset = _offset;
        target._localAngle = _localAngle;
        target._localoffsetDistance = _localoffsetDistance;
        target._flySpeedType = _flySpeedType;
        target._startimeType = _startimeType;
        target._randAngleAndDistance = _randAngleAndDistance;
        target._randOffsetDistanceMin = _randOffsetDistanceMin;
        target._randOffsetDistanceMax = _randOffsetDistanceMax;
        target._flySpeed = _flySpeed;
        target._flySpeed_before = _flySpeed_before;
        target.ignoreHeightDiff = ignoreHeightDiff;
        target._rotationOffset = this._rotationOffset;
        target._scaleOffset = _scaleOffset;
        target._offset_target = _offset_target;
        target._dontFollowRotate = _dontFollowRotate;
        target._anim_speed = _anim_speed;
        target._is_record_origin = _is_record_origin;
        target._hide_particle_no_enemy = _hide_particle_no_enemy;
        target._hudu_offset = _hudu_offset;
        target._ignoretime = _ignoretime;
    }


    public ParticleSystemControlMixer.SnapType snapType { get { return _snapType; } set { _snapType = value; } }
    /// <summary>
    /// 勾选则不跟随朝向
    /// </summary>
    public bool dontFollowRotate { get { return _dontFollowRotate; } set { _dontFollowRotate = value; } }
    public ParticleSystemControlMixer.FlyType flyType { get { return _flyType; } set { _flyType = value; } }
    ///// <summary>
    ///// 初始上抛速度
    ///// </summary>
    //public float UpSpeed { get { return _upSpeed; }set { _upSpeed = value; } }
    /// <summary>
    /// 重力加速度
    /// </summary>
    public float Gravity { get { return _gravity; } set { _gravity = value; } }
    /// <summary>
    /// 偏移量
    /// </summary>
    public Vector3 Offset { get { return _offset; } set { _offset = value; } }
    /// <summary>
    /// 目标点偏移量
    /// </summary>
    public Vector3 OffsetTarget { get { return _offset_target; }set { _offset_target = value; } }
    public float localAngle { get { return _localAngle; }set { _localAngle = value; } }
    public float localDistance { get { return _localoffsetDistance; }set { _localoffsetDistance = value; } }
    /// <summary>
    /// 旋转偏移量
    /// </summary>
    public Vector3 RotationOffset { get { return _rotationOffset; }set { _rotationOffset = value; } }
    /// <summary>
    /// 缩放偏移量
    /// </summary>
    public Vector3 ScaleOffset { get { return _scaleOffset; }set { _scaleOffset = value; } }

    /// <summary>
    /// 偏移随机角度与距离
    /// </summary>
    public bool randAngleAndDistance { get { return _randAngleAndDistance; }set { _randAngleAndDistance = value; } }
    public float randOffsetDistanceMax { get { return _randOffsetDistanceMax; }set { _randOffsetDistanceMax = value; } }
    public float randOffsetDistanceMin { get { return _randOffsetDistanceMin; }set { _randOffsetDistanceMin = value; } }


    /// <summary>
    /// 速度计算方式
    /// </summary>
    public ParticleSystemControlMixer.FlySpeedType flySpeedType { get { return _flySpeedType; } set { _flySpeedType = value; } }
    public float flySpeed { get { return _flySpeed; } set { _flySpeed = value; } }
    /// <summary>
    /// 前置速度，用于计算开始时间点
    /// </summary>
    public float flySpeedBefore { get { return _flySpeed_before; }set { _flySpeed_before = value; } }
    public SkillHitTimelineClip.StartTimeType startTimeType { get { return _startimeType; }set { _startimeType = value; } }
    public bool IgnoreHeightDiff { get { return ignoreHeightDiff; }set { ignoreHeightDiff = value; } }
    public float animSpeed { get { return _anim_speed; } set { _anim_speed = value; } }

    public bool isRecordOrigin { get { return _is_record_origin; } set { _is_record_origin = value; } }

    public bool hideParticleNoEnemy { get => _hide_particle_no_enemy; set => _hide_particle_no_enemy = value; }

    public float hudu_offset => _hudu_offset;

    public bool ignoreTime { get => _ignoretime; set => _ignoretime = value; }
}


