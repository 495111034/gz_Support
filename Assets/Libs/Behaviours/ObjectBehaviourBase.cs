using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using System.Globalization;

public enum ObjectType
{
    ObjTypeNone = 0,
    ObjTypePlayer = 1,          //角色
    ObjTypePet = 2,             //宠物
    ObjTypeRobot = 3,           //机器人
    ObjTypeGuardNPC = 4,        //守护NPC(一些守护活动，会被怪物攻击的NPC等)
    ObjTypeMonster = 5,         //怪物;
    ObjTypeNPC = 6,             //NPC
    ObjTypeItem = 7,            //小物品
    ObjTypeGodness = 8,         //神
    ObjTypeStartPoint = 9,      //路径显示出发点(用法例:召唤怪物的特效门)
    ObjTypeTrap = 10,           //陷阱
    ObjTypeBeast = 11,          // 召唤兽
    ObjTypeStarMage = 12,       // 双星人
    ObjTypeTransmit = 20,       //传送点 (没有AOI对象存在，以静态的SceneAoi对象存在)
    ObjTypeHorse = 21,          // 坐骑(独立的坐骑对象，可捕获)
    ObjTypePlant = 22,          //静态采集物(客户端实现)
    ObjTypeSprite = 23,         //精灵
    ObjTypeBornPoint = 24,      //出生点
    ObjTypeRelivePoint = 25,    //复活点
    ObjTypeTrigger = 30,        //场景trigger
    ObjTypeTmpBlock = 31,       //临时阻挡
    ObjTypeGuardian = 32,       //守护灵
    ObjTypePokemon = 33,       //小精灵
}

public interface ISceneObject 
{
    public List<Renderer> CacheRendererList { get; set; }
    public bool iHightQualityShow { get; }
}

/// <summary>
/// 游戏对象在unity中的显示
/// 游戏对象包括：角色、怪物、NPC、掉落物等
/// </summary>
public class ObjectBehaviourBase : MonoBehaviour, ISceneObject
{
    public List<Renderer> CacheRendererList { get; set; }
    public bool iHightQualityShow => HightQualityShow;

    public string id = "";
    public object objBehaviour = null;    
    public GameObject objBody = null;

    [Range(0,2)]
    public float ZHanhunHeight = 0.6f;
    [Range(0, 2)]
    public float ShenwuHeight = 0.7f;

    public Action<string> OnAnimEvent;
    public Action<Collider> OnTriggerEnterEvent;
    public Action<Collider> OnTriggerExitEvent;
    public Action<Collider> OnTriggerStayEvent;

    public bool isCameraVisible;

#if !DISABLE_TIMELINE
    public Action<ObjectBehaviourBase,Vector2Int, SkillHitPlayableData, SkillHitTimelineClip.HitInfoParams> EventOnByHit;
    public Action<ObjectBehaviourBase, Vector2Int, SkillHitPlayableData, SkillHitTimelineClip.HitInfoParams> EventOnByHitEnd;
    public Action<ObjectBehaviourBase, Vector2Int,  SkillHitPlayableData, SkillHitTimelineClip.HitInfoParams> EventOnByHitUpdate;
    public Action<long, int, int, SkillHitPlayableData> EventOnAttackBeforeHit;
    public Action<GPUAnimTimelineClip> EventOnPlayAnim;
    public Action<GPUAnimTimelineClip, int, int> EventOnStopAnim;
    public Action<FightEventInfoPlayableData, int, int> EventOnFightInfoStart;
    public Action<FightEventInfoPlayableData> EventOnFightInfoEnd;
    public Action<Vector2Int,MoveClipPlayableData, MoveClipPlayableData.MoveClipParams> EventOnMoveclipStart;
    public Action<Vector2Int, MoveClipPlayableData,Vector2, Vector3> EventOnMoveclipUpdata;
    public Action<Vector2Int, MoveClipPlayableData,Vector3> EventOnMoveEndclipUpdata;
    //public Action<FMODEventPlayableBehavior> EventOnTimelinePlaySound;
    //public Action<FMODEventPlayableBehavior> EventOnTimelineStopSound;
    //public Func<FMODEventPlayableBehavior, string> EventOnTimlinePlayByHitSound;
#endif

    public Action OnAwake;
    public Action OnUpdate;
    public Action OnStart;
    public Action OnLateUpdate;
    public Action On_Enable;
    public Action On_Destory;    
    public Action On_Disable;
    public Action OnDrawGiz;
    public string BehavourName = "";
    public string expInfo = "";

    [HideInInspector]
    public object Lister = null;




    public ObjectType objectType = ObjectType.ObjTypeMonster;

    /// <summary>
    /// 角色外观资源id（非运行时）
    /// </summary>
    public string AssetList;
    /// <summary>
    /// 是否是主角（非运行时）
    /// </summary>
    public bool IsMainRole;
    public bool HightQualityShow;
    /// <summary>
    /// 是否是BOSS出场(BOSS不填SceneAoiId，根据场景里的当前boss生成)
    /// </summary>
    [Tooltip("不是BOSS出场Timeline，严禁勾选")]
    public bool isBossShow;
    /// <summary>
    /// 出场boss的缩放
    /// </summary>
    public float bossScale;

    [NonSerialized]
    public int FightTxtCnt;
    [NonSerialized]
    public float FightTxtTime;

    private void Update()
    {
        OnUpdate?.Invoke();
    }

    private void Awake()
    {
        _time = 0;
        OnAwake?.Invoke();
    }

    private void Start()
    {
        OnStart?.Invoke();
    }

    private void LateUpdate()
    {
        OnLateUpdate?.Invoke();
    }

    private void OnDestroy()
    {
        On_Destory?.Invoke();

        OnAnimEvent = null;
        OnTriggerEnterEvent = null;
        OnTriggerExitEvent = null;
        OnTriggerStayEvent = null;
#if !DISABLE_TIMELINE
        EventOnByHit = null;
        EventOnByHitEnd = null;
        EventOnByHitUpdate = null;
        EventOnPlayAnim = null;
        EventOnStopAnim = null;
        EventOnFightInfoStart = null;
        EventOnFightInfoEnd = null;
        EventOnMoveclipStart = null;
        EventOnMoveclipUpdata = null;
        EventOnMoveEndclipUpdata = null;
        //EventOnTimelinePlaySound = null;
        //EventOnTimelineStopSound = null;
        //EventOnTimlinePlayByHitSound = null;
#endif
        OnAwake = null;
        OnUpdate = null;
        OnStart = null;
        OnLateUpdate = null;
        On_Enable = null;
        On_Destory = null;
        On_Disable = null;
        OnDrawGiz = null;
    }

    private void OnEnable()
    {
        //Log.LogError($"OnEnable {this}");
        On_Enable?.Invoke();
    }

    private void OnDisable()
    {
        //Log.LogError($"OnDisable {this}");
        On_Disable?.Invoke();
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        OnDrawGiz?.Invoke();
    }

#endif
    //#region  GPU动画组管理

    private float _time;
    public string DefaultAnimName = "idle_01";
    //gpuskinning public List<GPUSkinningPlayerMono> _gpuAnimList;
    public List<Animator> _animaotrList;
    public List<Animation> _animationList;
    
    

    
    public void initGetAnimList()
    {
        if (_animaotrList == null)
            _animaotrList = new List<Animator>();
        _animaotrList.Clear();
        List<Animator> animList = MyListPool<Animator>.Get();
         gameObject.GetComponentsEx(animList);
        for (int i = 0; i < animList.Count; ++i)
        {
            if (animList[i].runtimeAnimatorController != null)
            {
                _animaotrList.Add(animList[i]);
            }
        }
        MyListPool<Animator>.Release(animList);

        if (_animationList == null)
            _animationList = new List<Animation>();
        _animationList.Clear();
        gameObject.GetComponentsEx(_animationList);
    }

    public string GetAnimClipName(string clipName)
    {
        // if (!Application.isPlaying)
        // {
        if ((_animaotrList == null || _animaotrList.Count == 0))
            initGetAnimList();

        if ((_animaotrList == null || _animaotrList.Count == 0))
            return clipName;

        if (_animaotrList.Count > 0)
        {
            var clips = _animaotrList[0].runtimeAnimatorController.animationClips;
            //(from i in clips where i.name.ToLower() == clipName select i).ToList();

            for (int i = 0; i < clips.Length; ++i)
            {
                var clip = clips[i];
                if (clip.name.ToLower() == clipName.ToLower())
                    return clipName;
            }

            if (clipName.Contains("_"))
            {
                var startName = clipName.ToLower().Split('_')[0];
                for (int i = 0; i < clips.Length; ++i)
                {
                    if (clips[i].name.ToLower() == startName.ToLower())
                    {
                        return clips[i].name;
                    }
                }
                for (int i = 0; i < clips.Length; ++i)
                {
                    if (clips[i].name.ToLower().StartsWith( startName.ToLower()))
                    {
                        return clips[i].name;
                    }
                }
            }
            else
            {
                for (int i = 0; i < clips.Length; ++i)
                {
                    if (clips[i].name.StartsWith(clipName.Trim()))
                    {
                        return clips[i].name;
                    }
                }
            }
        }
        // }

        return clipName;
    }


    private bool _isAnimatorPlaying = false;
    public GPUAnimTimelineClip _currentTimeLineClip = null;
#if UNITY_EDITOR
    private float _animatorStartTime = 0f;
    private float _animatorLength = 0f;
    private float _timelineTime = 0f;

    public bool IsByHitFly = false;
    public string SceneAoiId = "";

    /// <summary>
    /// 由职业和性别拼接的key，用于结婚过场动画
    /// </summary>
    [Tooltip("由职业和性别拼接的key，用于结婚过场动画")]
    public string profGenderKey;


    /// <summary>
    /// 来自timeline的调用，当拖动timeline时间轴时
    /// </summary>
    public void OnUpdateEditor(float timelineTime)
    {
        _timelineTime = timelineTime;
        float deltaTime =_time == 0?0: timelineTime - _time;
        _time = timelineTime;
        if (deltaTime <= 0) return;
        
        if (!Application.isPlaying )
        {
            if (_animaotrList.Count > 0 && _isAnimatorPlaying)
            {
                for (int i = 0; i < _animaotrList.Count; ++i)
                {
                    var anim = _animaotrList[i];
                    anim.playbackTime = timelineTime - _animatorStartTime;
                    //anim.Update(deltaTime);
                    anim.Update(deltaTime);
                }
            }

            if(_animationList.Count > 0)
            {
                for(int i = 0; i < _animationList.Count; ++i)
                {
                    var anim = _animationList[i];                    
                }
            }
        }
    }

    public string GetAssetListByChild()
    {
        if (_animaotrList.Count != 0)
            return (_animaotrList.Select(i => i.name.GetNumberFromString().ToString(CultureInfo.InvariantCulture)).Aggregate((s1, s2) => s1 + ", " + s2));

        return "";
    }

#endif

#if !DISABLE_TIMELINE
    public void OnTimelineEvent(FightEventInfoPlayableData fightEventInfo, int cur_idx, int total_clip)
    {
        //Log.LogError($"aaaaa:{fightEventInfo.name}");
        if (Application.isPlaying)
        {
            EventOnFightInfoStart?.Invoke(fightEventInfo, cur_idx, total_clip);
        }
    }

    public void OnTimelineEventOut(FightEventInfoPlayableData fightEventInfo)
    {
        if(Application.isPlaying)
        {
            EventOnFightInfoEnd?.Invoke(fightEventInfo);
        }
    }


    /// <summary>
    /// 来自timeline的调用
    /// </summary>
    /// <param name="animName"></param>
    public void PlayAnim(GPUAnimTimelineClip timelineClipData)
    {
        if (Application.isPlaying)
        {
            try
            {
                EventOnPlayAnim?.Invoke(timelineClipData);
            }
            catch (System.Exception err)
            {
                Log.LogError($"PlayAnim error:{err.Message}/n{err.StackTrace}");
            }
        }
        else
        {
            if (_animaotrList == null || _animaotrList.Count == 0) initGetAnimList();

            var statename = timelineClipData.ClipName;
           
            if (_animaotrList.Count > 0)
            {
                float __l = 0f;
                for (int i = 0; i < _animaotrList.Count; ++i)
                {
                    var anim = _animaotrList[i];
                    anim.enabled = true;
                    if (_currentTimeLineClip == null)
                    {
                        anim.SetTrigger(statename);
                        //anim.Play(statename, 0, timelineClipData.StartOffset);
                    }
                    else
                    {
                        anim.SetTrigger(statename);
                        //var mixTime = timelineClipData.OwningClip.start - _currentTimeLineClip.OwningClip.end;
                        //if (mixTime > 0)
                        //{
                        //    anim.PlayInFixedTime(statename, 0, (float)mixTime);
                        //}
                        //else
                        //{
                        //    anim.Play(statename, 0, timelineClipData.StartOffset);
                        //}
                    }
                    anim.speed = timelineClipData.AnimatorSpeed;

#if UNITY_EDITOR
                    var animInfo = anim.GetCurrentAnimatorStateInfo(0);

                    if (animInfo.length > __l)
                    {
                        __l = animInfo.length;
                    }
#endif
                }
#if UNITY_EDITOR
                _animatorStartTime = _timelineTime;
                _animatorLength = __l;
#endif
                _isAnimatorPlaying = true;
            }

            if (_animationList.Count > 0)
            {

            }
        }

        _currentTimeLineClip = timelineClipData;
    }


    /// <summary>
    /// 来自timeline的调用
    /// </summary>
    /// <param name="timelineClipData">动画结束事件</param>
    /// <param name="cur_idx">当前是时间轴上的第几个动画，从0开始编号，即最后一个为total_Num - 1</param>
    /// <param name="total_Num">时间轴上的动画总数，从1开始编号</param>
    /// <param name="fightid">战斗包编号 </param>
    public void StopAnim(GPUAnimTimelineClip timelineClipData, int cur_idx, int total_Num)
    {
        if (_currentTimeLineClip != timelineClipData)
        {
            return;
        }

        if (Application.isPlaying)
        {
            EventOnStopAnim?.Invoke(timelineClipData, cur_idx, total_Num);
        }

        _currentTimeLineClip = null;
    }


    public void OnStopTimeline()
    {
        
        _isAnimatorPlaying = false;
        //_currentTimeLineClip = null;

        if (Application.isPlaying) return;

        if (_animaotrList != null)
        {
            for (int i = 0; i < _animaotrList.Count; ++i)
            {
                var anim = _animaotrList[i];
                anim.enabled = false;
            }
        }
    }
#endif



    //#region 来自动画帧调用

    void OnTriggerEnter(Collider col)
    {
        OnTriggerEnterEvent?.Invoke(col);
    }

    void OnTriggerExit(Collider col)
    {
        OnTriggerExitEvent?.Invoke(col);
    }

    void OnTriggerStay(Collider col)
    {
        OnTriggerStayEvent?.Invoke(col);
    }

#if !DISABLE_TIMELINE
    

    /// <summary>
    /// 受击事件，来自timeline的调用
    /// </summary>
    /// <param name="eventContent"></param>
    public void OnByHit(ObjectBehaviourBase attacker,Vector2Int idx, SkillHitPlayableData clipData, SkillHitTimelineClip.HitInfoParams hitInfo)
    {
        //Log.LogError($"{gameObject.name} OnByHit :{clipData.EventContent}");
        EventOnByHit?.Invoke(attacker, idx,  clipData, hitInfo);
#if UNITY_EDITOR

#endif
    }

    /// <summary>
    /// 受击事件结束，来自timeline的调用
    /// </summary>
    /// <param name="eventContent"></param>
    public void OnByHitEnd(ObjectBehaviourBase attacker, Vector2Int idx, SkillHitPlayableData clipData, SkillHitTimelineClip.HitInfoParams hitInfo)
    {
        //Log.LogError($"{gameObject.name} OnByHitEnd :{clipData.EventContent}");
        EventOnByHitEnd?.Invoke(attacker, idx,  clipData, hitInfo);
#if UNITY_EDITOR

#endif
    }

    /// <summary>
    /// 受击过程中每帧更新
    /// </summary>
    /// <param name="attacker"></param>
    
    /// <param name="fightId"></param>
    /// <param name="clipData"></param>
    /// <param name="hitInfo"></param>
    public void OnByHitUpdate(ObjectBehaviourBase attacker, Vector2Int idx,  SkillHitPlayableData clipData, SkillHitTimelineClip.HitInfoParams hitInfo)
    {
        EventOnByHitUpdate?.Invoke(attacker, idx,  clipData, hitInfo);
    }

    /// <summary>
    /// 开始执行移动clip时
    /// </summary>
    /// <param name="moveClipData"></param>
    /// <param name="moveParams"></param>
    public void OnTimeLineStartMove(Vector2Int idx, MoveClipPlayableData moveClipData, MoveClipPlayableData.MoveClipParams moveParams)
    {
        EventOnMoveclipStart?.Invoke(idx, moveClipData, moveParams);
    }

    public void OnTimelineMoveUpdate(Vector2Int idx,  MoveClipPlayableData moveClipData, Vector2 times, Vector3 curPos)
    {
        EventOnMoveclipUpdata?.Invoke(idx, moveClipData, times, curPos);
    }

    public void OnTimelineMoveEnd(Vector2Int idx, MoveClipPlayableData moveClipData,  Vector3 curPos)
    {
        EventOnMoveEndclipUpdata?.Invoke(idx, moveClipData, curPos);
    }

    /// <summary>
    /// 获得受击音效
    /// </summary>
    /// <returns></returns>
    //public string GetByHitSoundEventName(FMODEventPlayableBehavior p)
    //{
    //    if (p != null)
    //    {
    //        return EventOnTimlinePlayByHitSound?.Invoke(p);
    //    }
    //    return p.eventName;
    //}

#endif



}

