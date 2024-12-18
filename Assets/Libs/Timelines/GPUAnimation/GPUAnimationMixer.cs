using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]
public class GPUAnimationMixer: PlayableBehaviour
{
    public ObjectBehaviourBase objBase;
    private GPUAnimPlayableData gpuData = new GPUAnimPlayableData();
    public PlayableDirector player;

    [SerializeField]
    [HideInInspector]
    float[] _attack_keyframe;
    [SerializeField]
    [HideInInspector]
    float[] _cast_keyframe;

   
    public float[] attack_keyframe { get { return _attack_keyframe; }set { _attack_keyframe = value; } }
    public float[] cast_keyframe { get { return _cast_keyframe; }set { _cast_keyframe = value; } }

    private bool m_isFirstFrameProcess = true;
   


    public override  void OnPlayableCreate(Playable playable)
    {
       
    }

    public override void OnGraphStart(Playable playable)
    {

        m_isFirstFrameProcess = false;
    }

    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
       
    }   

    public override void PrepareFrame(Playable playable, FrameData info)
    {
      
    }

    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (playerData is null) return;

        if (!(playerData is ObjectBehaviourBase))
        {
            Log.LogError($"ProcessFrame:playerdata is not ObjectBehaviourBase:{playerData?.GetType()}");
            return;
        }

       
        if (objBase == null)
            return;
        
        int inputCount = playable.GetInputCount();
        float time = (float)playable.GetGraph().GetRootPlayable(0).GetTime();
#if UNITY_EDITOR
        bool playing = playable.GetGraph().IsPlaying();
        /*if (!playing )
        {
            return;
        }*/
        
        if (!Application.isPlaying)
        {
            objBase.OnUpdateEditor(time);
        }
#endif //UNITY_EDITOR        
        
        for (int i = 0; i < inputCount; i++)
        {
            ScriptPlayable<GPUAnimPlayableData> inputPlayable = (ScriptPlayable<GPUAnimPlayableData>)playable.GetInput(i);
            GPUAnimPlayableData input = inputPlayable.GetBehaviour();
            input.UpdateBehaviour(time,i,inputCount);
        }
    }

    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        //Debug.Log("5,OnBehaviourPause=======================");
    }

    public override void OnGraphStop(Playable playable)
    {
        if(objBase)objBase.OnStopTimeline();
       
    }

    public override void OnPlayableDestroy(Playable playable)
    {
       // Debug.Log("7,OnPlayableDestroy=======================");
    }
}
