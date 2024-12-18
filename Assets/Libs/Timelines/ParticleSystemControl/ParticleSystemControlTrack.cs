
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;


/// <summary>
/// 特效timeline轨迹
/// 所有的的timeline player实例共用一个Track对象
/// </summary>
[TrackColor(1f, 0f, 0f)]
[TrackClipType(typeof(ParticleSystemControlClip))]
[TrackBindingType(typeof(EffectBehaviour))]

public class ParticleSystemControlTrack : TrackAsset
{
    public ParticleSystemControlMixer template = new ParticleSystemControlMixer();    
    //static int number_id = 0;

    public Dictionary<int, ParticleSystemControlMixer> _templateList = new Dictionary<int, ParticleSystemControlMixer>();

    [HideInInspector] [SerializeField] TimelineType timelineType;
    [HideInInspector]
    [SerializeField]
    bool _independent = false;

    public TimelineType m_TimelineType { get { return timelineType; } set { timelineType = value; } }
    /// <summary>
    /// 独立timeline对象
    /// </summary>
    public bool Independent { get { return _independent; } set { _independent = value; } }

    public void CreateInstaceContent(GameObject parentGo)
    {
        int instanceID = parentGo.GetInstanceID();
        if (!_templateList.ContainsKey(instanceID))
        {
            //Log.LogError($"added {parentGo.GetInstanceID()}");
            _templateList[instanceID] = new ParticleSystemControlMixer();           
        }

        template.CloneSerializeFiled(_templateList[instanceID]);
        _templateList[instanceID].InstanceID = instanceID;

        var clips = m_Clips;
        int len = clips.Count;
        for (int i = 0; i < len; i++)
        {
            var clip = clips[i];
            var myAsset = clip.asset as ParticleSystemControlClip;
            if (myAsset)
            {
                myAsset.CreateTemplateInst(instanceID);
            }
        }
    }

    public void RemoveInstanceContent(GameObject parentGo)
    {
        if(_templateList.ContainsKey(parentGo.GetInstanceID()))
        {
            _templateList.Remove(parentGo.GetInstanceID());           
        }
    }


    public void OnEnable()
    {
        //if (template.randomSeed == 0xffffffff)
        //    template.randomSeed = (uint)Random.Range(0, 0x7fffffff);
    }

    public ScriptPlayable<ParticleSystemControlMixer> playable;
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        //播放时执行
        //不同的实例共用一个Track对象

        var director = go.GetComponent<PlayableDirector>();
        var ps = director.GetGenericBinding(this) as EffectBehaviour;
        template.parentTrack = this;

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (!ps.name.StartsWith("fx_"))
            {
                UnityEditor.EditorUtility.DisplayDialog("绑定特效错误", $"[{ps.name}]\n特效命名错误, 绑定的特效命名必须以fx_开头！", "确定");
            }
            var assetFile = UnityEditor.AssetDatabase.GetAssetPath(director.playableAsset).ToLower();
            if (assetFile.StartsWith(PathDefs.ASSETS_PATH_CHARACTER))
            {
                timelineType = TimelineType.FightSkill;
            }
            else
            {
                timelineType = TimelineType.SceneScenario;
            }
        }
#endif

        if (!_templateList.ContainsKey(go.GetInstanceID()))
        {            
            template.parentParent = go;
            ComputClipsTime();

            var playable = ScriptPlayable<ParticleSystemControlMixer>.Create(graph, template, inputCount);
            playable.GetBehaviour()._effectObj = ps;            
            return playable;
        }
        else
        {

           // Log.LogError($"CreateTrackMixer,ParticleSystemControlTrack id {GetInstanceID()},parent is {go.GetInstanceID()},currentCrackID = {_templateList[go.GetInstanceID()].InstanceID},template.InstanceID={template.InstanceID}");
            
            _templateList[go.GetInstanceID()].parentTrack = this;
            _templateList[go.GetInstanceID()].parentParent = go;
            ComputClipsTime();

            // Create a track mixer playable and give the reference to the particle
            // system (it has to be initialized before OnGraphStart).
            playable = ScriptPlayable<ParticleSystemControlMixer>.Create(graph, _templateList[go.GetInstanceID()], inputCount);
            playable.GetBehaviour()._effectObj = ps;
            return playable;
        }
        
        
    }

    public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
    {
        //
        // In this track, the following properties will be modified.
        //
        // - transform.position
        // - transform.rotation
        // - particleSystem.useAutoRandomSeed
        // - particleSystem.main.duration
        // - particleSystem.emission.rateOverTimeMultiplier
        // - particleSystem.emission.rateOverDistanceMultiplier
        //
        // Note that the serialized property names are a bit defferent from
        // their property name.
        //

        var ps = director.GetGenericBinding(this) as EffectBehaviour;
        if (ps == null) return;

        var go = ps.gameObject;

        driver.AddFromName<Transform>(go, "m_LocalPosition");
        driver.AddFromName<Transform>(go, "m_LocalRotation");

        //driver.AddFromName<EffectBehaviour>(go, "lengthInSec");
        //driver.AddFromName<EffectBehaviour>(go, "autoRandomSeed");
        //driver.AddFromName<EffectBehaviour>(go, "randomSeed");

        //driver.AddFromName<EffectBehaviour>(go, "EmissionModule.rateOverTime.scalar");
        //driver.AddFromName<EffectBehaviour>(go, "EmissionModule.rateOverDistance.scalar");
    }

    public void ComputClipsTime()
    {
        foreach (var clip in GetClips())
        {
            var myAsset = clip.asset as ParticleSystemControlClip;
            if (myAsset)
            {
                myAsset.OwningClip = clip;
                myAsset.parentTrack = this;
            }
        }
    }

    public void releaseData(GameObject instGo)
    {
        if (!instGo) return;

        if (_templateList.TryGetValue(instGo.GetInstanceID(), out var _temp))
        {
            _temp.OnEnd();
            //TransformArray.Release(_temp.SnapTargetList);
            _temp.SnapTargetList = null;//cache
            _temp.snapTarget.defaultValue = null;
            _temp.attackerObj = null;
            _temp.RotationByGame = Quaternion.identity;
            _temp.PosByGame = Vector3.zero;

            _templateList.Remove(instGo.GetInstanceID());
        }
        //RemoveInstanceContent(instGo);

        foreach (var clip in GetClips())
        {
            var myAsset = clip.asset as ParticleSystemControlClip;
            if (myAsset)
            {

                myAsset.RemoveTemplate(instGo.GetInstanceID());
            }
        }
    }
}


