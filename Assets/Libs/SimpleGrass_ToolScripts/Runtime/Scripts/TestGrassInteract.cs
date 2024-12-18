using UnityEngine;
using static UnityEngine.ParticleSystem;

namespace SimpleGrass
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public class TestGrassInteract : MonoBehaviour
    {
        struct ParticleData
        {
         //  public bool loop;
          // public float duration;
            public int maxParticles;
        };

        public Transform hero;
        public bool debugHeroTrace;

        public Transform testParticle;
        public float _fadeoutMul = 0.1f;
        Vector3 orgAngles = Vector3.zero;
        private bool isTest = true;
        private Vector3 _heroFootPos = Vector3.zero;
        private Vector3 _heroPriorPos = Vector3.zero;

        private ParticleData[] _particleInits = null;
        private ParticleSystem[] _particleObjs = null;
        private bool TraceTriggerEnd = false;
        private void Awake()
        {
            //if (Application.isPlaying)
            //{                
            //    SimpleGrassTrace.OnValidAction -= OnTraceValidateEvent;
            //    SimpleGrassTrace.OnValidAction += OnTraceValidateEvent;
            //}
        }

        private void OnDestroy()
        {
            if (Application.isPlaying)
            {
               // SimpleGrassTrace.OnValidAction -= OnTraceValidateEvent;
            }
        }

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                SimpleGrassGlobal.Global.UseHeroPos(true);

                //SimpleGrassTrace.OnGenerateTrace -= OnGenerateTraceTest;
                //SimpleGrassTrace.OnGenerateTrace += OnGenerateTraceTest;

                SimpleGrassTraceEffectTrigger.Register(OnTriggerEffectEvent);
            }
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
            {
                SimpleGrassGlobal.Global.UseHeroPos(false);

                //SimpleGrassTrace.OnGenerateTrace -= OnGenerateTraceTest;

                SimpleGrassTraceEffectTrigger.UnRegister();
            }
        }


        private void Start()
        {
            isTest = (Application.installMode == ApplicationInstallMode.Editor);
        }


        private void Update()
        {
            UpdateTraceTriggerEnd();


            if (!isTest || hero == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                SimpleGrassGlobal.Global.UpdateHeroPos(hero.transform.position);
            }
            else
            {
                //编辑中
                //调试：英雄轨迹
                if (debugHeroTrace)
                {
                    //英雄轨迹：响应位置更新
                    _heroFootPos = hero.transform.position;
                    float dist = (_heroFootPos - _heroPriorPos).magnitude;
                    
                    SimpleGrassTrace.OnUpdateHeroTrace?.Invoke(dist, _heroFootPos,false);
                    //SimpleGrassTrace.UpdateGrassTraceShaderVars();
                }else
                {
                    if (SimpleGrassTrace.S_HeroTraceEnabled)
                    {
                        SimpleGrassTrace.ClearHeroTraceData();
                    }
                    //SimpleGrassTrace.UpdateGrassTraceShaderVars();
                }
            }

            ///
        }

      
        private void OnGenerateTraceTest(Vector3 pos)
        {
            if(testParticle != null)
            {
                Transform newObj = Instantiate(testParticle);
                newObj.position = pos;
                newObj.parent = this.transform;
            }
        }

        //private void DoTraceValidateEvent(Transform grassTrace, bool enabled)
        //{
        //    if(enabled)
        //    {
        //        SimpleGrassTraceEffectTrigger.Register(OnTriggerEffectEvent);
        //    }
        //    else
        //    {
        //        SimpleGrassTraceEffectTrigger.UnRegister();
        //    }
        //}

        private void OnTriggerEffectEvent(Transform triggerSource, GameSupport.BasicBoxTrigger.TRIGGER_STATUS status)
        {
            if (triggerSource == null)
            {
                return;
            }
            //进入触发区
            if (status == GameSupport.BasicBoxTrigger.TRIGGER_STATUS.TS_BEGIN)
            {
                TraceTriggerEnd = false;
                //创建光效 --TODO..                
                //testParticle.gameObject.SetActive(false);
                //缓存光效的信息
                bool init = false;
                if (_particleObjs == null || _particleInits == null)
                {
                    _particleObjs = testParticle.GetComponentsInChildren<ParticleSystem>();
                    _particleInits = new ParticleData[_particleObjs.Length];
                    init = true;
                }
                for (int i = 0; i < _particleObjs.Length; i++)
                {
                    MainModule main = _particleObjs[i].main;
                    if (init)
                    {
                        ParticleData data = new ParticleData();
                        data.maxParticles = main.maxParticles;
                        _particleInits[i] = data;
                    }

                    //初始光效：最大粒子数
                    main.maxParticles = _particleInits[i].maxParticles;
                }

                if (!testParticle.gameObject.activeSelf)
                {
                    testParticle.gameObject.SetActive(true);
                }
            }
            //出触发区
            else if (status == GameSupport.BasicBoxTrigger.TRIGGER_STATUS.TS_END)
            {
                TraceTriggerEnd = true;
            }
                //Transform newObj = Instantiate(testParticle);
                //newObj.position = triggerSource.position;
                //newObj.parent = triggerSource;            
        }

        //出触发区后，光效数量进行逐渐衰减
        void UpdateTraceTriggerEnd()
        {
            //_fadeoutMul衰减速度：2000
            if (TraceTriggerEnd && _particleObjs != null)
            {
                for (int i = 0; i < _particleObjs.Length; i++)
                {
                    MainModule main = _particleObjs[i].main;
                    main.maxParticles = (int)Mathf.Max(0, main.maxParticles - _fadeoutMul * Time.deltaTime);
                }
            }
        }

   
    }

}