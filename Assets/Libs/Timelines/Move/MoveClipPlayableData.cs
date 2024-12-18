using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using easing;


[System.Serializable]
public class MoveClipPlayableData : PlayableBehaviour
{
    public enum MoveDestinationType
    {
        MoveToTargetPosition = 0,               //移动到目标位置
        MoveToTargetObjectAndOffset = 1,        //移动到目标物体和一定的偏移量
        MoveForward = 2,                        //向前移动
        RotationLeft = 3,                       //向左转
        RotationRight = 4,                      //向右转
    }

    public enum MoveStartType
    {
        CurrentPos = 0,                //当前位置
        SettingPos = 1,                //指定的位置       
    }

    public class MoveClipParams
    {
        public Vector3 startPos;
        public Vector3 targetPos;
        public Vector3 oldPos;
        public float MoveSpeed;
    }

    [HideInInspector]
    [NonSerialized]
    public TimelineClip OwningClip;
    [HideInInspector]
    [NonSerialized]
    public ObjectBehaviourBase trackTargetObject;
    /// <summary>
    /// Editor战斗timeline专用，攻击目标
    /// </summary>   
    [NonSerialized]
    public GameObject targetObject;


    public long FightId { get; set; }
    public int GoInstID { get; set; }

    [HideInInspector]
    [SerializeField]
    private MoveStartType startType = MoveStartType.CurrentPos;
    [HideInInspector]
    [SerializeField]
    private MoveDestinationType moveType = MoveDestinationType.MoveToTargetPosition;
    [HideInInspector]
    [SerializeField]
    private Vector3 startPos;
    [HideInInspector]
    [SerializeField]
    private Vector3 destPos;
    [HideInInspector]
    [SerializeField]
    private float destObjectDisOffset;  //距离偏移
    [HideInInspector]
    [SerializeField]
    private float destObjectHeightOffset;//高度偏移
    [HideInInspector]
    [SerializeField]
    private float destAngleOffset;          //角度偏移
    [HideInInspector]
    [SerializeField]
    protected bool forwardToDestination;
    [HideInInspector]
    [SerializeField]
    protected bool forwardIgnoreHeightDiff;     //朝向忽略高度差
    [HideInInspector]
    [SerializeField]
    protected bool ignoreHeightDiff;
    [HideInInspector]
    [SerializeField]
    protected bool allowMotionBlur;
    [HideInInspector]
    [SerializeField]
    EaseType moveEaseType1 = EaseType.None;
    [HideInInspector]
    [SerializeField]
    InOutType moveEaseType2 = InOutType.In;
    [HideInInspector]
    [SerializeField]
    private SkillHitTimelineClip.StartTimeType _startimeType = SkillHitTimelineClip.StartTimeType.ByTimeline;
    [HideInInspector]
    [SerializeField]
    private float _flySpeed = 0f;

    [NonSerialized]
    [HideInInspector]
    Vector3 _posByGame = Vector3.zero;

    public MoveStartType StartType { get { return startType; } set { startType = value; } }
    public MoveDestinationType MoveType { get { return moveType; } set { moveType = value; } }
    public SkillHitTimelineClip.StartTimeType StartTimeType { get { return _startimeType; } set { _startimeType = value; } }
    /// <summary>
    ///  从准备到开始移动的速度，仅适用于_startimeType = ByDistance的情况
    /// </summary>
    public float FlySpeed { get { return _flySpeed; } set { _flySpeed = value; } }
    public Vector3 StartPos { get { return startPos; }set { startPos = value; } }
    public Vector3 DestPos { get { return destPos; }set { destPos = value; } }
    public float DestObjectLineOffset { get { return destObjectDisOffset; }set { destObjectDisOffset = value; } }
    public float DestObjectHeightOffset { get { return destObjectHeightOffset; } set { destObjectHeightOffset = value; } }
    /// <summary>
    /// 朝向目的地
    /// </summary>
    public bool ForwardToDest { get { return forwardToDestination; }set { forwardToDestination = value; } }
    /// <summary>
    /// 忽略高度差
    /// </summary>
    public bool IgnoreHeightDiff { get { return ignoreHeightDiff; }set { ignoreHeightDiff = value; } }
    /// <summary>
    /// 开启相机运动模式
    /// </summary>
    public bool AllowMotionBlur { get { return allowMotionBlur; }set { allowMotionBlur = value; } }
    public EaseType MoveEaseType1 { get { return moveEaseType1; }set { moveEaseType1 = value; } }
    public InOutType MoveEaseType2 { get { return moveEaseType2; } set { moveEaseType2 = value; } }

    /// <summary>
    /// 游戏中传入的位置
    /// </summary>
    public Vector3 PosByGame { get { return _posByGame; } set { _posByGame = value; } }

    [NonSerialized]
    private bool m_isFirstFrameProcess = true;
    [NonSerialized]
    private bool m_isComplete = false;


    public int frameCount;

    public Vector3 RunTimeStartPos { get; set; }
    public Vector3 RunTimeSpeed { get; set; } 
    public Vector3 RunTimeEndPos { get; set; } = Vector3.left;

    //[NonSerialized]
    [SerializeField]
    public bool MovePathTargets = false;
    
    //[NonSerialized]
    //public List<ObjectBehaviourBase> PathTargets;

    public void CloneSerializeFiled(MoveClipPlayableData target)
    {
        target.startType = startType;
        target.moveType = moveType;
        target.startPos = startPos;
        target.destPos = destPos;
        target.destObjectDisOffset = destObjectDisOffset;
        target.destObjectHeightOffset = destObjectHeightOffset;
        target.destAngleOffset = destAngleOffset;
        target.forwardToDestination = forwardToDestination;
        target.forwardIgnoreHeightDiff = forwardIgnoreHeightDiff;
        target.ignoreHeightDiff = ignoreHeightDiff;
        target.allowMotionBlur = allowMotionBlur;
        target.moveEaseType1 = moveEaseType1;
        target.moveEaseType2 = moveEaseType2;
        target._startimeType = _startimeType;
        target._flySpeed = _flySpeed;
        target._posByGame = _posByGame;
        target.MovePathTargets = MovePathTargets;
    }

    float startAngle = 0f;

    public void UpdateBehaviour(float time, int cur_idx, int total_clip, TimelineClip currentClip)
    {
        if (currentClip != OwningClip) return;
        if (!trackTargetObject) return;

        if ((!targetObject) && _startimeType == SkillHitTimelineClip.StartTimeType.ByDistance) return;

        float clipStarttime = (float)currentClip.start;
        float clipEndtime = (float)currentClip.end;
        float clipDurations = (float)currentClip.duration;
        float clipCurentTime = time - clipStarttime;

        var dis = Vector3.Distance(startPos, destPos);   //距离
        float speed = dis / clipDurations; 

        if (_startimeType == SkillHitTimelineClip.StartTimeType.ByDistance && total_clip == 1 && FlySpeed > 0)
        {
            //根据距离和速度计算开始时间，暂不支持
        }
        else
        {
            if (clipCurentTime > 0)//已经开始
            {
                if (clipCurentTime >= clipDurations)//已结束
                {
                    if(!m_isComplete)
                    {
                        trackTargetObject.OnTimelineMoveEnd(new Vector2Int( cur_idx, total_clip), this,  trackTargetObject.transform.position);
                    }
                    m_isComplete = true;
                   // m_isFirstFrameProcess = true;
                }
                else//移动中
                {
                    m_isComplete = false;
                    //开始移动时，移动到指定起始位置
                    if (m_isFirstFrameProcess)
                    {
                        if (startType == MoveStartType.CurrentPos)
                        {
                            startPos = trackTargetObject.transform.position;
                        }

                        if (moveType == MoveDestinationType.MoveToTargetObjectAndOffset)//目标物体
                        {
                            if (targetObject && targetObject.GetComponent<BoxCollider>())
                            {
                                Vector3 dir = (startPos - targetObject.transform.position).normalized;

                                Vector3 size = targetObject.GetComponent<BoxCollider>().size;
                                Vector2 size2 = new Vector2(size.x, size.z);
                                float boxLength = Mathf.Max(size2.magnitude / 2, 1f);

                                Vector3 pos = targetObject.transform.position + dir * boxLength;
                                destPos = GetPosOffset(startPos, pos);
                            }
                            else
                            {
                                destPos = GetPosOffset(startPos, targetObject ? targetObject.transform.position : destPos);
                            }
                        }
                        else if (moveType == MoveDestinationType.MoveForward)//向前移动
                        {
                            destPos = GetPosByForward(startPos, trackTargetObject.transform.forward);
                        }
                        else if (moveType == MoveDestinationType.MoveToTargetPosition)//目标位置
                        {
                            destPos = GetPosOffset(startPos, destPos);
                        }
                        else if (moveType == MoveDestinationType.RotationLeft || moveType == MoveDestinationType.RotationRight)//旋转
                        {
                            destPos = StartPos;
                            startAngle = trackTargetObject.transform.rotation.eulerAngles.y;
                            destAngleOffset = destAngleOffset % 360;
                        }

                        var moveParams = new MoveClipParams()
                        {
                            startPos = startPos,
                            targetPos = Vector3.zero,
                            oldPos = destPos,
                            MoveSpeed = speed,
                        };

                        if (Application.isPlaying)
                        {
                            //Log.LogError($"time={time},cur_idx={cur_idx},total_clip={total_clip},clipStarttime={clipStarttime},clipEndtime={clipEndtime},clipDurations={clipDurations},clipCurentTime={clipCurentTime}");
                            trackTargetObject.OnTimeLineStartMove(new Vector2Int(cur_idx, total_clip), this, moveParams);

                            if (moveParams.targetPos != Vector3.zero)
                            {
                                destPos = moveParams.targetPos;
                            }
                        }

                        dis = Vector3.Distance(startPos, destPos);  //距离
                        speed = dis / clipDurations;
                    }
                    else
                    {
                        if (moveType == MoveDestinationType.MoveForward || moveType == MoveDestinationType.MoveToTargetObjectAndOffset || moveType == MoveDestinationType.MoveToTargetPosition)
                        {
                            Vector3 direction = GetDirectionHeightDiff(startPos, destPos, false, targetObject);
                            if (RunTimeEndPos == Vector3.left)
                            {
                                trackTargetObject.transform.position = startPos + (moveEaseType1 != EaseType.None ? (EaseUtils.GetRate(clipCurentTime, clipDurations, EaseUtils.GetEaseFunc(moveEaseType1, moveEaseType2)) * direction * clipDurations * speed) : (direction * clipCurentTime * speed));
                            }
                            if (forwardToDestination)
                            {
                                if (forwardIgnoreHeightDiff)
                                {
                                    direction = new Vector3(direction.x, 0, direction.z);
                                }
                                if (direction != Vector3.zero)
                                {
                                    trackTargetObject.transform.forward = direction;
                                }
                            }
                        }
                        else if (moveType == MoveDestinationType.RotationLeft || moveType == MoveDestinationType.RotationRight)
                        {
                            float currentAngle = destAngleOffset / clipDurations * clipCurentTime;
                            var _cAngle = trackTargetObject.transform.rotation.eulerAngles;
                            if (moveType == MoveDestinationType.RotationLeft)
                                _cAngle.y = startAngle - currentAngle;
                            else if (moveType == MoveDestinationType.RotationRight)
                                _cAngle.y = startAngle + currentAngle;
                            trackTargetObject.transform.rotation = Quaternion.Euler(_cAngle);
                        }
                        if (Application.isPlaying)
                        {
                            trackTargetObject.OnTimelineMoveUpdate(new Vector2Int(cur_idx, total_clip), this, new Vector2(clipCurentTime, clipDurations), trackTargetObject.transform.position);
                        }
                    }
                }

                m_isFirstFrameProcess = false;
            }
            else//未开始
            {
                m_isFirstFrameProcess = true;
            }
        }

    }

    Vector3 GetPosOffset(Vector3 p1,Vector3 p2)
    { 
        var targetPos = ignoreHeightDiff ? new Vector3(p2.x, p1.y, p2.z) : p2;
        var dir = (p1 - targetPos).normalized;

        if(destObjectDisOffset < 0 && Vector3.Distance(p1, targetPos) < -destObjectDisOffset)
        {
            //忽略回退的情况
            return p1;
        }

        return targetPos - dir * destObjectDisOffset + new Vector3(0, destObjectHeightOffset, 0);
    }

    Vector3 GetPosByForward(Vector3 p1,Vector3 forward)
    {
        if (ignoreHeightDiff) forward.y = 0;

        return p1 + forward * destObjectDisOffset + new Vector3(0, destObjectHeightOffset, 0);
    }

    Vector3 GetDirectionHeightDiff(Vector3 pos1, Vector3 pos2, bool ignoreHight,GameObject targetObj)
    {
        Vector3 p1,p2;
        p1 = ignoreHight ? new Vector3(pos1.x, 0, pos1.z) : pos1;
        if (moveType == MoveDestinationType.MoveToTargetObjectAndOffset && targetObj && Vector3.Distance(pos1,pos2) < 0.1f)
        {
            p2 = ignoreHight ? new Vector3(targetObj.transform.position.x, 0, targetObj.transform.position.z) : targetObj.transform.position;
        }
        else
        {
            p2 = ignoreHight ? new Vector3(pos2.x, 0, pos2.z) : pos2;
        }      
       

        return (p2 - p1).normalized;
    }


}

