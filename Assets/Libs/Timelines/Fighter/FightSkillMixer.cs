using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;


/// <summary>
///  A behaviour that is attached to a playable
/// </summary>
public class FightSkillMixer : PlayableAsset, IPlayableBehaviour
{
    public ExposedReference<Transform> attacker;

    private ObjectBehaviourBase objBase;
    private FightSkillPlayableData fightData;
    private bool m_isFirstFrameProcess = true;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
    {
        return Playable.Create(graph);
    }

    public void OnPlayableCreate(Playable playable)
    {
        
        objBase = null;
        fightData = null;

       // Debug.Log($"1,OnPlayableCreate=======================");
    }

    public void OnGraphStart(Playable playable)
    {
        m_isFirstFrameProcess = false;
        //var director = playable.GetGraph().GetResolver() as PlayableDirector;
        //Debug.Log($"gettime:{ playable.GetTime()},GetSpeed={playable.GetSpeed()},GetPropagateSetTime={playable.GetPropagateSetTime()},GetDelay={playable.GetDelay()},GetDuration={playable.GetDuration()},duration={director.duration}");
        //int inputCount = playable.GetInputCount();
        //if (inputCount <= 0)
        //{
        //    Log.LogError($"FightSkillMixer, Clip Count error.current count is {inputCount}");
        //    return;
        //}
        // ScriptPlayable<FightSkillPlayableData> playableInput = (ScriptPlayable<FightSkillPlayableData>)playable.GetInput(0);
        // fightData = playableInput.GetBehaviour();       

        // Debug.Log($"2,OnGraphStart======================={fightData.SkillID},{fightData.parentSkillClip.duration},{fightData.parentSkillClip.start}");

    }

    public void OnBehaviourPlay(Playable playable, FrameData info)
    {
       // Debug.Log($"3,OnBehaviourPlay=======================,frameId={info.frameId}");
    }   

    public void PrepareFrame(Playable playable, FrameData info)
    {
       // Debug.Log($"4.1,PrepareFrame======================={info.deltaTime},{info.weight},frameId={info.frameId}");
    }

    public void ProcessFrame(Playable playable, FrameData info, object playerData)
    {       
#if UNITY_EDITOR
        var root = playable.GetGraph().GetRootPlayable(0);
        var graph = root.GetGraph();
        Log.LogError(graph.GetEditorName());
#endif
        if (!(playerData is ObjectBehaviourBase))
        {
            Log.LogError($"ProcessFrame:playerdata is not ObjectBehaviourBase:{playerData?.GetType()}");
            return;

        }

        objBase = playerData as ObjectBehaviourBase;
        if (objBase == null)
            return;

        if (!m_isFirstFrameProcess)
        {            
            m_isFirstFrameProcess = true;
            //objBase.OnFightStart(playable);
        }
        else
        {
            
            //objBase.OnFightFrame(playable, info);
        }

        //Debug.Log($"4.2,ProcessFrame======================={objBase.gameObject.name}");

        //var director = playable.GetGraph().GetResolver() as PlayableDirector;
        //director.GetGenericBinding()

    }

    public void OnBehaviourPause(Playable playable, FrameData info)
    {
        //Debug.Log("5,OnBehaviourPause=======================");
    }

    public void OnGraphStop(Playable playable)
    {
      //  Debug.Log("6,OnGraphStop=======================");

        if (objBase == null || !m_isFirstFrameProcess)
            return;

        m_isFirstFrameProcess = false;
        //objBase.OnFightStop(playable);

        objBase = null;
       // fightData = null;
    }

    public void OnPlayableDestroy(Playable playable)
    {
       // Debug.Log("7,OnPlayableDestroy=======================");
    }
}

