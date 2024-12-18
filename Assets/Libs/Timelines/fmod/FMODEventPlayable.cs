using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[System.Serializable]
public class FMODEventPlayable : PlayableAsset, ITimelineClipAsset
{
    [HideInInspector]
    [SerializeField]
    public FMODEventPlayableBehavior template = new FMODEventPlayableBehavior();
    public FMODEventPlayableBehavior Template { get { return template; } set { template = value; } }
    public Dictionary<int, FMODEventPlayableBehavior> _templateDic = new Dictionary<int, FMODEventPlayableBehavior>();

    public GameObject TrackTargetObject { get; set; }
    public float eventLength; //In seconds.

    public FMODEventTrack TrackObject { get; set; }

    //[FMODUnity.EventRef]
    [SerializeField] public string eventName;
    //[SerializeField] public STOP_MODE stopType;
    [SerializeField] public bool StopGameBGMusic = false;

    //[SerializeField] public FMODUnity.ParamRef[] parameters = new FMODUnity.ParamRef[0];

    [SerializeField]
    protected FModSoundType _soundPlayType = FModSoundType.DirectPlay;

    //[NonSerialized] public bool cachedParameters = false;

    public override double duration
    {
        get
        {
            if (eventName == null)
            {
                return base.duration;
            }
            else
            {
                return eventLength;
            }
        }
    }

    public ClipCaps clipCaps
    {
        get { return ClipCaps.None; }
    }

    public TimelineClip OwningClip { get; set; }

    public FMODEventPlayableBehavior GetTemplate(int goInstId)
    {
        if (goInstId == 0) return template;
        return _templateDic.ContainsKey(goInstId) ? _templateDic[goInstId] : null;
    }

    public void CreateTemplateInst(int goInstId)
    {
        if (!_templateDic.ContainsKey(goInstId))
        {
            _templateDic[goInstId] = new FMODEventPlayableBehavior();
        }
        template.CloneSerializeFiled(_templateDic[goInstId]);
        _templateDic[goInstId].GoInstID = goInstId;
    }

    public void RemoveTemplate(int goInstId)
    {
        if (_templateDic.ContainsKey(goInstId))
        {
            _templateDic[goInstId].GoInstID = 0;
            _templateDic[goInstId].FightId = 0;
            _templateDic.Remove(goInstId);
        }
        _templateDic.Remove(goInstId);
    }


    public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
    {

        var t = GetTemplate(go.GetInstanceID());
        ScriptPlayable<FMODEventPlayableBehavior> tpl;

        if (t == null)
        {
            template.OwningClip = OwningClip;
            tpl = ScriptPlayable<FMODEventPlayableBehavior>.Create(graph, template);
        }
        else
        {
            t.OwningClip = OwningClip;
            tpl = ScriptPlayable<FMODEventPlayableBehavior>.Create(graph, t);
        }

        //FMODEventPlayableBehavior behavior = tpl.GetBehaviour();

        var playable = ScriptPlayable<FMODEventPlayableBehavior>.Create(graph, template);
        var behavior = playable.GetBehaviour();

        behavior.TrackTargetObject = TrackTargetObject;
        behavior.eventName = eventName;
        behavior.soundPlayType = _soundPlayType;
        //behavior.stopType = stopType;
        //behavior.parameters = parameters;
        behavior.OwningClip = OwningClip;

        return playable;
    }

#if UNITY_EDITOR
    public void UpdateEventDuration(float duration)
    {
        eventLength = duration / 1000f;
    }

    public void OnValidate()
    {
        if (OwningClip == null) return;

        if (_soundPlayType == FModSoundType.AttackerSound)
        {
            OwningClip.displayName = "攻击者喊声";
        }
        else if (_soundPlayType == FModSoundType.SkillSound)
        {
            OwningClip.displayName = "技能喊声";
        }
        else 
        {
            if (eventName != null)
            {
                OwningClip.displayName = eventName.Split(',')[0];
            }
        }
    }
#endif //UNITY_EDITOR
}


public class FMODEventPlayableBehavior : PlayableBehaviour
{
    public string eventName;
    //public FModSoundType soundType;
    //public STOP_MODE stopType = STOP_MODE.AllowFadeout;
    //public FMODUnity.ParamRef[] parameters = new FMODUnity.ParamRef[0];

    public GameObject TrackTargetObject;
    public TimelineClip OwningClip;
    public FMODEventTrack TrackObject { get; set; }
    private bool isPlayheadInside;
    [NonSerialized]
    private int _isPlayheadInside = 0;//0未开始，1播放中，2结束

    //private FMOD.Studio.EventInstance eventInstance;
    int audio_source;

    public int GoInstID { get; set; }
    public long FightId { get; set; }

    [NonSerialized]
    string[] __eventNames;

    [NonSerialized]
    public bool IsEmptyTarget;

    public FModSoundType soundPlayType { get; set; } = FModSoundType.DirectPlay;

    public void CloneSerializeFiled(FMODEventPlayableBehavior target)
    {
        target.eventName = eventName;
    }

    protected void PlayEvent()
    {
        if (__eventNames == null)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                return;
            }
            __eventNames = eventName.Split(',');
        }


        if (Application.isPlaying)
        {
            string __eventName;
            if (soundPlayType == FModSoundType.HitSound)
            {
                //if (TrackTargetObject && TrackTargetObject.GetComponentEx<ObjectBehaviourBase>())
                {
                    //___eventName = TrackTargetObject.GetComponentEx<ObjectBehaviourBase>().GetByHitSoundEventName(this);
                }

                if (__eventNames.Length == 1)
                {
                    __eventName = __eventNames[0];
                }
                else
                {
                    __eventName = __eventNames[IsEmptyTarget ? 0 : 1];
                }
            }
            else
            {
                __eventName = __eventNames[UnityEngine.Random.Range(0, __eventNames.Length)];
            }
            if (!string.IsNullOrEmpty(__eventName) && RuntimeAudoiPlayer.Play3DSkillAudio1 != null)
            {
                float start_time = 0;
                audio_source = RuntimeAudoiPlayer.Play3DSkillAudio1(__eventName, TrackTargetObject?.transform, FightId, start_time);
            }
        }
#if UNITY_EDITOR
        else
        {
            var __eventName = __eventNames[UnityEngine.Random.Range(0, __eventNames.Length)];
            PlayAudio(__eventName);
        }
#endif
    }
#if UNITY_EDITOR
    public static void PlayAudio(string eventName)
    {
        var _AudioSources = GameObject.Find("_AudioSources");
        if (!_AudioSources)
        {
            _AudioSources = new GameObject("_AudioSources");
            for (var i = 1; i <= 5; ++i) 
            {
                var c = new GameObject($"AudioSource_{i}");
                c.transform.parent = _AudioSources.transform;
            }
        }
        var files = System.IO.Directory.GetFiles("assets/sound/", eventName + ".*", System.IO.SearchOption.AllDirectories);
        if (files.Length > 0)
        {
            foreach (var path in files)
            {
                if (!path.EndsWith(".meta"))
                {
                    var audio = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.AudioClip>(path);
                    if (audio)
                    {
                        foreach (Transform c in _AudioSources.transform) 
                        {
                            var ss = c.gameObject.AddMissingComponent<AudioSource>();
                            if (!ss.isPlaying)
                            {
                                ss.Stop();
                                //if (!audio.preloadAudioData)
                                {
                                    audio.LoadAudioData();
                                }
                                //ss.PlayOneShot(audio);
                                ss.clip = audio;
                                ss.Play();
                                break;
                            }                            
                        }
                        break;
                    }
                }
            }
        }
    }
#endif


    public void OnEnter()
    {
        if (_isPlayheadInside == 0)
        {
            _isPlayheadInside = 1;
            PlayEvent();
        }
    }

    public void OnExit()
    {
        if (_isPlayheadInside != 1)
        {
            return;
        }
        _isPlayheadInside = 2;

        if (audio_source > 0 && RuntimeAudoiPlayer.StopAudio != null)
        {
            RuntimeAudoiPlayer.StopAudio(audio_source);
            audio_source = 0;
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            var _AudioSource = GameObject.Find("_AudioSource");
            if (_AudioSource)
            {
                var ss = _AudioSource.GetComponent<AudioSource>();
                if (ss)
                {
                    ss.Stop();
                }
            }
        }
#endif
    }

    public void UpdateBehaviour(double time)
    {
        //Log.LogInfo($"{isPlayheadInside}, {time} -> [{OwningClip.start},{OwningClip.end}]");
        if (time <= 0)
        {
            _isPlayheadInside = 0;
        }
        if (_isPlayheadInside != 2 && time >= OwningClip.start)
        {
            if (time < OwningClip.end - 0.01)
            {
                OnEnter();
            }
            else
            {
                OnExit();
            }
        }
    }

    public void OnGraphStop() 
    {
        if (audio_source > 0 && RuntimeAudoiPlayer.StopAudio != null)
        {
            RuntimeAudoiPlayer.StopAudio(audio_source);
            audio_source = 0;
        }
    }
}
