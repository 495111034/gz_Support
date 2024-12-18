using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[System.Serializable]
public enum PlayHorseAnimType
{
    BothAnim = 0,   //同时播放人物与座骑动作
    PlayerOnly = 1, //仅播放角色动作
    HorseOnly = 2,  //仅播放座骑动作
    ShengwuOnly = 3,    //仅播放圣物动作
}

[System.Serializable]
public class GPUAnimTimelineClip: PlayableAsset
{

    [HideInInspector]
    [SerializeField]
    private GPUAnimPlayableData templete = new GPUAnimPlayableData();
    public GPUAnimPlayableData Templete { get { return templete; } set { templete = value; } }
   
    
    public ObjectBehaviourBase TrackTargetObject { get; set; }
    public TimelineClip OwningClip { get; set; }

    public GPUAnimationTrack parentTrack;


    [SerializeField][HideInInspector]
    string clipName;

    [SerializeField][HideInInspector]
    private float startOffset;
    
    [SerializeField][HideInInspector]
    private float endOffset;

    [SerializeField][HideInInspector]
    private float animSpeed = 1f;


    [System.NonSerialized]
    public float directorSpeed;
    //[SerializeField][HideInInspector]
    //private GPUSkinningWrapMode animWrapMode;

    [SerializeField]
    [HideInInspector]
    private PlayHorseAnimType horseAnimType = PlayHorseAnimType.BothAnim;

    /// <summary>
    /// animator state name
    /// </summary>
    public string ClipName { get { return clipName; } set { clipName = value; } }

    public float StartOffset
    {
        get { return startOffset; }
        set { startOffset = value; }
    }

    public float EndOffset
    {
        get { return endOffset; }
        set { endOffset = value; }
    }

    //public GPUSkinningWrapMode AnimWrapMode
    //{
    //    get { return animWrapMode; }
    //    set { animWrapMode = value; }
    //}

    public float AnimatorSpeed
    {
        get { return animSpeed; }
        set { animSpeed = value; }
    }

    public PlayHorseAnimType HorseAnimType
    {
        get { return horseAnimType; }
        set { horseAnimType = value; }
    }

    //[HideInInspector]
    //public ExposedReference<Transform> attacker;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
    {

        var playable = ScriptPlayable<GPUAnimPlayableData>.Create(graph, templete);
        GPUAnimPlayableData behavior = playable.GetBehaviour();
        
        behavior.TrackTargetObject = TrackTargetObject;
        behavior.OwningClip = OwningClip;
        behavior.GpuTimelineClip = this;        
        return playable;
    }

    [SerializeField]
    [HideInInspector]
    private bool unscaledTime = false;

    public bool UnscaledTime
    {
        get { return unscaledTime; }
        set { unscaledTime = value; }
    }
}
