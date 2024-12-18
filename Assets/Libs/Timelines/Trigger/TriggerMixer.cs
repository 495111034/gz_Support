
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class TriggerMixer: PlayableAsset, IPlayableBehaviour
{
    private TriggerPlayableData triggerData = new TriggerPlayableData();
    
    public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
    {
        var playable = ScriptPlayable<TriggerPlayableData>.Create(graph, triggerData);
        TriggerPlayableData behavior = playable.GetBehaviour();
        return playable;
    }
    
    
    public void OnPlayableCreate(Playable playable)
    {
        
    }
    
    public void OnGraphStart(Playable playable)
    {
       
    }

    public void OnBehaviourPlay(Playable playable, FrameData info)
    {
      
    }   

    public void PrepareFrame(Playable playable, FrameData info)
    {
       
    }
    
    public void ProcessFrame(Playable playable, FrameData info, object playerData)
    {       
       
        int inputCount = playable.GetInputCount();
        float time = (float)playable.GetGraph().GetRootPlayable(0).GetTime();
#if UNITY_EDITOR
        bool playing = playable.GetGraph().IsPlaying();
        
        if (!Application.isPlaying)
        {
           
        }
#endif //UNITY_EDITOR
        
        
        for (int i = 0; i < inputCount; i++)
        {
            ScriptPlayable<TriggerPlayableData> inputPlayable = (ScriptPlayable<TriggerPlayableData>)playable.GetInput(i);
            TriggerPlayableData input = inputPlayable.GetBehaviour();
            input.UpdateBehaviour(time,i,inputCount);
        }

    }
    
    public void OnBehaviourPause(Playable playable, FrameData info)
    {
        
    }

    public void OnGraphStop(Playable playable)
    {
        
    }

    public void OnPlayableDestroy(Playable playable)
    {
        
    }

}
