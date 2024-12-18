using UnityEngine;
using UnityEngine.Playables;



/// <summary>
/// ��Чclip������
/// </summary>
[System.Serializable]
public class ParticleSystemControlPlayable : PlayableBehaviour
{
   // public float rateOverTime = 10;
   // public float rateOverDistance = 0;

    /// <summary>
    /// λ�ø��·�ʽ
    /// </summary>
    [SerializeField]
    [HideInInspector]
    ParticleSystemControlMixer.SnapType _snapType = ParticleSystemControlMixer.SnapType.FollowPos;

    /// <summary>
    /// ��ѡ�򲻸��泯��
    /// </summary>
    [SerializeField]
    [HideInInspector]
    bool _dontFollowRotate = false;

    /// <summary>
    /// ���з�ʽ
    /// </summary>
    [SerializeField]
    [HideInInspector]
    ParticleSystemControlMixer.FlyType _flyType = ParticleSystemControlMixer.FlyType.ToTarget;

    ///// <summary>
    ///// ��ʼ�����ٶ�
    ///// </summary>
    //[SerializeField]
    //[HideInInspector]
    //float _upSpeed = 0f;

        /// <summary>
        /// ���Ը߶Ȳ�ֵ
        /// </summary>
    [SerializeField]
    [HideInInspector]
    bool ignoreHeightDiff = true;

    /// <summary>
    /// �������ٶ�
    /// </summary>
    [SerializeField]
    [HideInInspector]
    float _gravity = 0f;

    /// <summary>
    /// ƫ����
    /// </summary>
    [SerializeField]
    [HideInInspector]
    Vector3 _offset = Vector3.zero;
    /// <summary>
    /// Ŀ���ƫ����
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
    //��תƫ��
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
    /// �����ٶȣ����ڼ������ʱ�䣬����_flySpeedTypeΪByDistanceʱ����
    /// </summary>
    [SerializeField]
    [HideInInspector]
    float _flySpeed = 0f;

    /// <summary>
    /// �����ٶȣ����ڼ��㿪ʼʱ��
    /// </summary>
    [SerializeField]
    [HideInInspector]
    float _flySpeed_before = 0f;

    /// <summary>
    /// ��Ч�ٶ�ϵ��  ������Animation��Ч
    /// </summary>
    [SerializeField]
    [HideInInspector]
    float _anim_speed = 1f;


    /// <summary>
    /// ��¼��ʼĿ��λ��
    /// </summary>
    [SerializeField]
    [HideInInspector]
    bool _is_record_origin = false;

    /// <summary>
    /// �޶���շ�ʱ��������Ч��Ϊtrueʱ���أ�Ĭ�ϲ����ء�
    /// ����սʿ�չ���ѡ��û���ˣ�����ʾ���ڵĻ��ǣ��ܻ���Ч��
    /// </summary>
    [SerializeField]
    [HideInInspector]
    bool _hide_particle_no_enemy = false;


    [SerializeField]
    [HideInInspector]
    float _hudu_offset = 0f;

    /// <summary>
    /// ��������ʱ�������
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
    /// ��ѡ�򲻸��泯��
    /// </summary>
    public bool dontFollowRotate { get { return _dontFollowRotate; } set { _dontFollowRotate = value; } }
    public ParticleSystemControlMixer.FlyType flyType { get { return _flyType; } set { _flyType = value; } }
    ///// <summary>
    ///// ��ʼ�����ٶ�
    ///// </summary>
    //public float UpSpeed { get { return _upSpeed; }set { _upSpeed = value; } }
    /// <summary>
    /// �������ٶ�
    /// </summary>
    public float Gravity { get { return _gravity; } set { _gravity = value; } }
    /// <summary>
    /// ƫ����
    /// </summary>
    public Vector3 Offset { get { return _offset; } set { _offset = value; } }
    /// <summary>
    /// Ŀ���ƫ����
    /// </summary>
    public Vector3 OffsetTarget { get { return _offset_target; }set { _offset_target = value; } }
    public float localAngle { get { return _localAngle; }set { _localAngle = value; } }
    public float localDistance { get { return _localoffsetDistance; }set { _localoffsetDistance = value; } }
    /// <summary>
    /// ��תƫ����
    /// </summary>
    public Vector3 RotationOffset { get { return _rotationOffset; }set { _rotationOffset = value; } }
    /// <summary>
    /// ����ƫ����
    /// </summary>
    public Vector3 ScaleOffset { get { return _scaleOffset; }set { _scaleOffset = value; } }

    /// <summary>
    /// ƫ������Ƕ������
    /// </summary>
    public bool randAngleAndDistance { get { return _randAngleAndDistance; }set { _randAngleAndDistance = value; } }
    public float randOffsetDistanceMax { get { return _randOffsetDistanceMax; }set { _randOffsetDistanceMax = value; } }
    public float randOffsetDistanceMin { get { return _randOffsetDistanceMin; }set { _randOffsetDistanceMin = value; } }


    /// <summary>
    /// �ٶȼ��㷽ʽ
    /// </summary>
    public ParticleSystemControlMixer.FlySpeedType flySpeedType { get { return _flySpeedType; } set { _flySpeedType = value; } }
    public float flySpeed { get { return _flySpeed; } set { _flySpeed = value; } }
    /// <summary>
    /// ǰ���ٶȣ����ڼ��㿪ʼʱ���
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


