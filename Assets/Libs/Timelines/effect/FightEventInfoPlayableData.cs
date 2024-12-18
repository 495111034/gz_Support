using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;


[System.Serializable]
public class FightEventInfoPlayableData : PlayableBehaviour
{
    public enum EffectType
    {
        CanBreak = 0,
        ShockCameraOnce = 1,
        ShockCameraLoop = 2,
        FieldViewChange = 3,//主相机镜头距离变化
        MotionBlur = 4,     //主相机运动模式
        DepthBlur = 5,      //主相机景深模式
        CancleCameraFollow = 6, //暂停相机跟随
        PuGongLianJiPoint = 7,  //普攻连击动作记录点（从这一点开始到“可打断”位置这一段时间，如果点击了普攻按钮，就自动续结下一个普攻技能）
        CollectTargets = 8,//攻击点
        RigidMove = 9,//平移点
        PetSwitch = 10,//宠物切换点
    };

    public enum TargetType
    {
        Attacker = 0,
        BeAttacked = 1,
        All =2
    };


    public TimelineClip OwningClip;
    public ObjectBehaviourBase attacker;
    
    public long FightId { get; set; }

    [HideInInspector]
    [SerializeField]
    private EffectType _effectType = EffectType.CanBreak;

    [HideInInspector]
    [SerializeField]
    private TargetType _targetType = TargetType.Attacker;

    [HideInInspector]
    [SerializeField]
    private float _targetFieldView = 45;

    [HideInInspector]
    [SerializeField]
    private float _startChangeTime = 0f;

    [HideInInspector]
    [SerializeField]
    private float _endChangeTime = 0f;


    [HideInInspector]
    [SerializeField]
    private float _motionStrength;

    [HideInInspector]
    [SerializeField]
    private Vector3 _depthPointOffset;
    [HideInInspector]
    [SerializeField]
    private float _DepthFNumber = 1.4f;
    [HideInInspector]
    [SerializeField]
    private float _DepthFLength = 0.05f;
    [HideInInspector]
    [SerializeField]
    private MyEffect.DepthField.KernelSize _DepthKSize = MyEffect.DepthField.KernelSize.Medium;

    //[HideInInspector]
    //[SerializeField]
    //private int _CollectTargetsIdx = 0;
    //public int CollectTargetsIdx => _CollectTargetsIdx;
    [HideInInspector]
    [SerializeField]
    bool _JuGuaiOnCollectTargets;//攻击时是否聚怪
    public bool IsJuGuaiOnCollectTargets => _JuGuaiOnCollectTargets;

    [HideInInspector]    [SerializeField]
    float _JuGuaiDist;//聚怪前方距离
    [HideInInspector]    [SerializeField]
    float _JuGuaiRadius;//聚怪半径

    public float JuGuaiDist => _JuGuaiDist;
    public float JuGuaiRadius => _JuGuaiRadius;

    [HideInInspector]    
    [SerializeField]
    bool _add_buffer = true;//是否添加buffer
    public bool IsAddBuffer => _add_buffer;

    [HideInInspector]
    [SerializeField]
    bool _add_trap = true;//是否添加陷阱
    public bool IsAddTrap => _add_trap;

    [HideInInspector]
    [SerializeField]
    bool _break_bossai = false;//是否打断BossAI
    public bool IsBreakBossAI => _break_bossai;

    public int GoInstID { get; set; }

    public void CloneSerializeFiled(FightEventInfoPlayableData target)
    {
        target._effectType = _effectType;
        target._targetFieldView = _targetFieldView;
        target._startChangeTime = _startChangeTime;
        target._endChangeTime = _endChangeTime;
        target._motionStrength = _motionStrength;
        target._depthPointOffset = _depthPointOffset;
        target._DepthFNumber = _DepthFNumber;
        target._DepthFLength = _DepthFLength;
        target._DepthKSize = _DepthKSize;
        target._JuGuaiOnCollectTargets = _JuGuaiOnCollectTargets;
        target._JuGuaiDist = _JuGuaiDist;
        target._JuGuaiRadius = _JuGuaiRadius;
        target._add_buffer = _add_buffer;
        target._add_trap = _add_trap;
        target._break_bossai = _break_bossai;
    }


    private int m_isFirstFrameProcess = 0;
    /// <summary>
    /// 效果类型
    /// </summary>
    public EffectType effectType { get { return _effectType; }set { _effectType = value; } }
    /// <summary>
    /// 作用对象
    /// </summary>
    public TargetType targetType { get { return _targetType; } set { _targetType = value; } }
    /// <summary>
    /// 目标FieldView
    /// </summary>
    public float fieldView { get { return _targetFieldView; }set { _targetFieldView = value; } }
    /// <summary>
    /// 开始渐变时间比例
    /// </summary>
    public float startChangeTime { get { return _startChangeTime; }set { _startChangeTime = value; } }
    /// <summary>
    /// 结束渐变时间比例
    /// </summary>
    public float endChangeTime { get { return _endChangeTime; }set { _endChangeTime = value; } }
    /// <summary>
    /// 运动模糊强度
    /// </summary>
    public float motionStrength { get { return _motionStrength; }set { _motionStrength = value; } }
    /// <summary>
    /// 景深焦点偏移量
    /// </summary>
    public Vector3 depthPointOffset { get { return _depthPointOffset; }set { _depthPointOffset = value; } }
    /// <summary>
    /// 景深光圈
    /// </summary>
    public float depthFNumber { get { return _DepthFNumber; }set { _DepthFNumber = value; } }
    /// <summary>
    /// 景深集距
    /// </summary>
    public float depthFLength { get { return _DepthFLength; }set { _DepthFLength = value; } }
    /// <summary>
    /// 景深光圈大小
    /// </summary>
    public MyEffect.DepthField.KernelSize depthKSize { get { return _DepthKSize; }set { _DepthKSize = value; } }




    public bool UpdateBehaviour(float time, int cur_idx, int total_clip)
    {
        //if (this.effectType == EffectType.CollectTargets)        
        //    Debug.LogWarning($"fight, idx={cur_idx}/{total_clip} time={time}/{Time.time}/{Time.frameCount}, start={(float)OwningClip.start}, end={(float)OwningClip.end}");
        
        if (time < OwningClip.start)
        {
            //if (this.effectType == EffectType.CollectTargets)
            //    Debug.LogWarning($"fight, idx={cur_idx}/{total_clip} time < start");
        }
        else 
        {         
            if (m_isFirstFrameProcess == 0)
            {
                m_isFirstFrameProcess = 1;
                //if (this.effectType == EffectType.CollectTargets)
                //    Debug.Log($"fight, idx={cur_idx}/{total_clip} OnEnter");
                OnEnter(cur_idx, total_clip);
            }

            if (m_isFirstFrameProcess == 1)
            {
                if (time < OwningClip.end - Math.Min(0.1, OwningClip.duration / 10))
                {
                    //OnUpdate(cur_idx, total_clip);
                }
                else
                {
                    m_isFirstFrameProcess = 2;
                    //if (this.effectType == EffectType.CollectTargets)
                    //    Debug.Log($"fight, idx={cur_idx}/{total_clip} OnExit");
                    OnExit(cur_idx, total_clip);
                    return true;
                }
            }
        }
        return false;
    }

    private void OnEnter(int cur_idx, int total_clip)
    {
        if (!Application.isPlaying) return;
        //if (!m_isFirstFrameProcess)
        {
            if (attacker)
                attacker.OnTimelineEvent(this, cur_idx, total_clip);
            else
            {
                //Log.LogError("FightEventInfoPlayableData OnEnter,attacker is null");
            }
             //m_isFirstFrameProcess = true;
        }
    }

    private void OnExit(int cur_idx, int total_clip)
    {
        if (!Application.isPlaying) return;
        //if (m_isFirstFrameProcess)
        {
            if (attacker)
            {
                attacker.OnTimelineEventOut(this);
            }
            else
            {
               // Log.LogError("FightEventInfoPlayableData OnExit,attacker is null");
            }
            //m_isFirstFrameProcess = false;
        }
    }


}
