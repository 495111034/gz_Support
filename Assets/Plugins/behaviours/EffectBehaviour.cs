using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("Effect/EffectBehaviour")]
public class EffectBehaviour : MonoBehaviour
{
    [HideInInspector]
    [SerializeField]
    float effective_seconds = 0f;
    [HideInInspector]
    [SerializeField]
    float callback_seconds = 0f;
    [HideInInspector]
    [SerializeField]
    bool isLowOverhead = false;



    float _endTime = 0;
    float _callbackTime = 0;
    bool _isCallback = false;
    

    public float EffectiveSeconds { get { return effective_seconds; } set { effective_seconds = value; } }
    public float CallbackSeconds { get { return callback_seconds; }set { callback_seconds = value; } }
    public bool IsLowOverhead { get { return isLowOverhead; } set { isLowOverhead = value; } }
   
    
    void Start()
    {
        gameObject.SetActive(true);
        if (effective_seconds > 1)
            _endTime = Time.time + effective_seconds;
        else
            _endTime = float.MaxValue;
        _callbackTime = Time.time + callback_seconds;
        _isCallback = false;
    }

    public void CheckHuaZhi()
    {
        InitPs();
        if (Graphics.activeTier < UnityEngine.Rendering.GraphicsTier.Tier3 && psList.Count > 0)
        {
            for (int i = 0; i < psList.Count; i++)
            {
                var emissionTmp = psList[i].emission;
                emissionTmp.enabled = false;
            }
        }
    }

    public void Resert()
    {
        if (gameObject.activeInHierarchy) 
        {
            var s = gameObject.GetComponent<PlaySoundBhv>();
            if (s)
            {
                s.enabled = false;
                s.enabled = true;
            }
        }
        gameObject.SetActive(true);
        if (effective_seconds > 1)
            _endTime = Time.time + effective_seconds;
        else
            _endTime = float.MaxValue;
        _callbackTime = Time.time + callback_seconds;
        _isCallback = false;        

        if (!Application.isPlaying)
        {
            Play();
        }

        PlayAnimstions();

    }


    void Update()
    {
        if (!gameObject.activeInHierarchy) return;
        if (!_isCallback && callback_seconds > 0 && Time.time >= _callbackTime)
        {
            SendMessageUpwards("__OnEffectCallBack", this);
            _isCallback = true;
        }
        if (Time.time >= _endTime) gameObject.SetActive(false);       
    }

    #region 来自timeline的调用
    

    List<ParticleSystem> psList = new List<ParticleSystem>();
    List<Animator> anim1List = new List<Animator>();
    List<Animation> anim2List = new List<Animation>();
    List<MyParticle.Spray> sprayList = new List<MyParticle.Spray>();

    public int PsCount
    {
        get
        {
            InitPs();
            return psList.Count;
        }
    }

    public bool useAutoRandomSeed
    {
        get
        {
            InitPs();
            if (psList.Count > 0) return psList[0].useAutoRandomSeed;
            return false;
        }
        set
        {
            InitPs();
            for(int i = 0; i < psList.Count; ++i)
            {
                psList[i].useAutoRandomSeed = value;
            }
        }
    }

    public uint randomSeed
    {
        get
        {
            InitPs();
            if (psList.Count > 0) return psList[0].randomSeed;
            return 0;
        }
        set
        {
            InitPs();
            for (int i = 0; i < psList.Count; ++i)
            {
                psList[i].randomSeed = value;
            }
        }
    }

    public float Speed
    {
        get
        {
            InitPs();
            if (psList.Count > 0) return psList[0].main.simulationSpeed;
            return 0;
        }
        set
        {
            InitPs();
            for (int i = 0; i < psList.Count; ++i)
            {
                var main = psList[i].main;
                main.simulationSpeed = value;
            }
        }
    }

    public ParticleSystem.MainModule main
    {
        get
        {
            InitPs();
            if (psList.Count > 0) return psList[0].main;
            return new ParticleSystem.MainModule();
        }
    }

    public void  Simulate(float v)
    {
        InitPs();
        for (int i = 0; i < psList.Count; ++i)
        {
            psList[i].Simulate(v);
        }
    }
    public void Simulate(float t, bool withChildren)
    {
        InitPs();
        for (int i = 0; i < psList.Count; ++i)
        {
            psList[i].Simulate(t, withChildren);
        }
    }
    public void Simulate(float t,  bool withChildren = true,  bool restart = true,  bool fixedTimeStep = true)
    {
        InitPs();
        for (int i = 0; i < psList.Count; ++i)
        {
            psList[i].Simulate(t, withChildren, restart, fixedTimeStep);
        }
    }

    public void Stop()
    {
        InitPs();
        for (int i = 0; i < psList.Count; ++i)
        {
            psList[i].Stop();
        }
    }

    public bool isPlaying
    {
        get
        {
            InitPs();
            if (psList.Count > 0) return psList[0].isPlaying;
            return false;
        }

    }

  

    public List<ParticleSystem.EmissionModule> emissions
    {

        get
        {
            InitPs();
            return (from id in psList select id.emission).ToList();
        }
        //set
        //{
        //    for(int i = 0; i < psList.Count; ++i)
        //    {
        //        var e = psList[i].emission;
        //        e.rateOverTimeMultiplier = value[i].rateOverTimeMultiplier;
        //    }
        //}
        
    }
    public float time {
        get
        {
            InitPs();
            if (psList.Count > 0)
            {

                float times = 0f;
                for(int i = 0; i < psList.Count; ++i)
                {
                    if (psList[i].time > times)
                        times = psList[i].time;
                }
                return times;
            }
            return 0f;
        }
        //set
        //{
        //    InitPs();
        //    for (int i = 0; i < psList.Count; ++i)
        //    {
        //        psList[i].time = value;
        //    }
        //}
    }

    public void Play()
    {        
        InitPs();
        for (int i = 0; i < psList.Count; ++i)
        {
            psList[i].Play();
        }
        
#if UNITY_EDITOR
        _last_time = 0;
#endif
    }

    public void PlayAnimstions()
    {
        if (anim1List.Count == 0) gameObject.GetComponentsInChildren(anim1List); //anim1List = gameObject.GetComponentsEx<Animator>();
        if (anim2List.Count == 0) gameObject.GetComponentsInChildren(anim2List); //anim2List = gameObject.GetComponentsEx<Animation>();
        for (int i = 0; i < anim1List.Count; ++i)
        {
            var anim1 = anim1List[i];
            var rt = anim1 ? anim1.runtimeAnimatorController : null;
            if (rt && anim1.layerCount > 0)
            {
                var animationClips = rt.animationClips;
                if (animationClips != null && animationClips.Length > 0)
                {
                    var clip = animationClips[0];
                    if (clip && !string.IsNullOrEmpty(clip.name))
                    {
                        var state_id = Animator.StringToHash(clip.name);
                        if (anim1.HasState(0, state_id))
                        {
                            anim1.Play(clip.name, 0);
                        }
                        else
                        {
                            Log.LogError($"控制器{rt.name}中的clip.name={clip.name} 和 state 命名不一致, 找朝旭或者玉明。修改后需要重新导出特效{gameObject.name}。");
                        }
                    }
                }
            }
        }
        //
        for (int i = 0; i < anim2List.Count; ++i)
        {
            if (!anim2List[i]) return;
            if (!anim2List[i].clip) continue;

            var clipName = anim2List[i].clip.name;
            try
            {
                var animState = anim2List[i][clipName];
                if (animState)
                {
                    animState.normalizedTime = 0;
                    anim2List[i].Play(clipName);
                }
            }
            catch (System.Exception e)
            {
                Log.LogError($"PlayAnimstions :{gameObject.name}, clipName:{clipName} ,msg:{e.Message}");
            }
        }
    }

#if UNITY_EDITOR   
    private float _last_time;
    public void OnTimelineUpdate(float timelineTime)
    {
        if(!Application.isPlaying)
        {
            float deltaTime = _last_time == 0 ? 0 : timelineTime - _last_time;
            _last_time = timelineTime;

            OnEditor_Update(deltaTime, timelineTime);

           
        }
    }

    //public float played_time = 0f;
    public void OnEditor_Update(float deltaTime,float played_time)
    {
        if (!Application.isPlaying)
        {
            if (anim1List.Count == 0) anim1List = gameObject.GetComponentsEx<Animator>();
            for (int i = 0; i < anim1List.Count; ++i)
            {
                anim1List[i].Update(deltaTime);
            }

            if (sprayList.Count == 0) sprayList = gameObject.GetComponentsEx<MyParticle.Spray>();
            for(int i = 0; i < sprayList.Count; ++i)
            {
                sprayList[i].OnEditor_Update(deltaTime);
            }

            if (anim2List.Count == 0) anim2List = gameObject.GetComponentsEx<Animation>();
            //Log.LogError(anim2List.Count + "," + played_time);
            for (int i = 0; i < anim2List.Count; ++i)
            {
                var animation = anim2List[i];
                if (!animation.clip) continue;

                var clipName = animation.clip.name;
                var state = animation[clipName];
                if (!state) 
                {
                    //Log.LogError($"AnimationState:{clipName} not found at Animation={animation}, gameObject={animation.gameObject.GetLocation()}");
                    continue;
                }
                
                if (played_time <= state.length)
                {
                    state.time = played_time;
                }
                else
                {
                    if (state.wrapMode == WrapMode.Loop)
                        state.time = played_time % state.length;
                    else
                        state.time = state.length;
                }
                anim2List[i].Sample();
            }
        }
    }

#endif

    void InitPs()
    {
        if (psList.Count == 0) psList = gameObject.GetComponentsEx<ParticleSystem>();
        
    }
    #endregion

}

