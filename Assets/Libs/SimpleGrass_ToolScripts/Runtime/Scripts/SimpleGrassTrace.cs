using UnityEngine;
using UnityEngine.Events;

namespace SimpleGrass
{
    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public class SimpleGrassTrace : MonoBehaviour
    {        
        //static public UnityAction<Transform, bool> OnValidAction;//事件
        static public UnityAction<Vector3> OnGenerateTrace;//生成新的轨迹点的事件
        static public UnityAction<float, Vector3,bool> OnUpdateHeroTrace;//更新英雄轨迹事件

        static public int PID_HEROTRACELIST = Shader.PropertyToID("_HeroTraceList");
        static public int PID_HEROTRACESTATES = Shader.PropertyToID("_HeroTraceStates");
        static public int PID_HEROTRACEPARAM = Shader.PropertyToID("_HeroTraceParam");

        static public bool S_HeroInTraceCollider = false;
        static public Vector3 S_HeroFootPos = Vector3.zero;
        static public Vector4[] S_HeroTraceList = new Vector4[15];
        static public float[] S_HeroTraceStateList = new float[15];
        static public float[] S_HeroTraceDurationList = new float[15];

        static public Vector4 S_HeroTraceParam = Vector4.zero;
        static public bool S_HeroTraceEnabled = false;

        static public int S_SimpleGrassTraceUsedNum = 0;
        //

        public float fadeRatio = 1.0f;
        public float fadeSpeed = 0.6f;//0.4 -15, 0.6-10
        public float fadeoutSpeedInStop = 0.4f;
        public float fadeoutDelay = 0.3f;
        public float tracePow = 5.0f;
        public float traceFar = 8.0f;        
        public float sampleInterval = 0.33f;
        public bool triggerByCollider = false;

        private bool _qualitySwitch = true;
        private bool _triggerSwitch = false;
        private bool _heroTraceWillDisable = false;        
        private float _heroTraceDelta = 0;
        private float _heroTraceMovedDist = 0;
        private bool _isTest = false;
        //
        private static SimpleGrassTrace _instance;
        public static SimpleGrassTrace Instance
        {
            get
            {
                return _instance;
            }
        }

        private void Awake()
        {            
        }
        
        private void Start()
        {
            Vector4 param = Vector4.zero;
            param.x = traceFar;
            param.y = tracePow;
            param.z = fadeRatio;
            param.w = 0.0f;
            S_HeroTraceParam = param;

            _isTest = (Application.installMode == ApplicationInstallMode.Editor);

        }

        private void OnEnable()
        {
           // OnValidAction?.Invoke(this.transform,true);
            ++S_SimpleGrassTraceUsedNum;
            //英雄轨迹点更新的事件处理
            SimpleGrassTrace.OnUpdateHeroTrace -= this._DoUpdateHeroTrace;
            SimpleGrassTrace.OnUpdateHeroTrace += this._DoUpdateHeroTrace;
            SimpleGrassTrace.ClearHeroTraceData();
            this.InitHeroTraceList();

            if (_instance != null && _instance != this)
            {
                if (_isTest)
                {
                    Debug.LogError("SimpleGrassTrace: SimpleGrassTrace: 1:" + this.gameObject.name + "  2:" + _instance.gameObject.name);
                }
            }
            _instance = this;
        }

        private void OnDisable()
        {
           // OnValidAction?.Invoke(this.transform, false);
            --S_SimpleGrassTraceUsedNum;
            SimpleGrassTrace.OnUpdateHeroTrace -= this._DoUpdateHeroTrace;
            SimpleGrassTrace.ClearHeroTraceData();
            this.InitHeroTraceList();            
            SimpleGrassTrace.UpdateGrassTraceShaderVars();

            if (_instance == this)
            {
                _instance = null;
            }
        }


        private void Update()
        {
            if (_isTest && S_SimpleGrassTraceUsedNum > 1)
            {
                Debug.LogError("错误：存在多个SimpleGrassTrace： [" + S_SimpleGrassTraceUsedNum.ToString() + "] name:"+ this.name  );
            }

            //只有高画质，才运行
            bool qualitySwitch = _QualitySwitch();
            if(_qualitySwitch != qualitySwitch)
            {
                _qualitySwitch = qualitySwitch;
                SimpleGrassTrace.ClearHeroTraceData();
                this.InitHeroTraceList();
                S_HeroTraceEnabled = _qualitySwitch;
            }else
            {
                this._heroTraceWillDisable = false;
                if (qualitySwitch)
                {
                    bool willTrigger = this.triggerByCollider && S_HeroInTraceCollider;
                    bool willEnabled = (willTrigger || (!this.triggerByCollider));
                    if (willEnabled != S_HeroTraceEnabled)
                    {
                        //退出触发区，
                        if(!willEnabled)
                        {
                            this._heroTraceWillDisable = true;
                            if (this.IsHeroTraceProcessOver())
                            {
                                SimpleGrassTrace.ClearHeroTraceData();
                                this.InitHeroTraceList();
                                S_HeroTraceEnabled = false;
                            }
                        }else
                        {
                            S_HeroTraceEnabled = true;
                        }                        
                    }
                }                                               
            }           

            if (S_HeroTraceEnabled)
            {         
                Vector4 param = Vector4.zero;
                param.x = traceFar;
                param.y = tracePow;
                param.z = fadeRatio;
                param.w = 1.0f;//enabled

                S_HeroTraceParam = param;
            }

            SimpleGrassTrace.UpdateGrassTraceShaderVars();
        }


        private bool _QualitySwitch()
        {
            int QualityLevel = QualitySettings.GetQualityLevel();
            return QualityLevel >= GameSupport.GameGlobalVars.TractIntract_QualityLevel;// return QualityLevel >= 2;//0低，1中，2高。。。
        }


        #region Hero Trace
        public void InitHeroTraceList()
        {                     
            //_heroTraceLastIndex = 0;
            _heroTraceDelta = 0;
            _heroTraceMovedDist = 0;
            _heroTraceWillDisable = false;
        }

        public bool IsHeroTraceProcessOver()
        {
            for (int i = 0; i <= S_HeroTraceList.Length - 1; ++i)
            {
                if (S_HeroTraceList[i].w != -1)
                {
                    return false;
                }
            }
            return true;
        }

        //英雄轨迹点更新的事件处理
        private void _DoUpdateHeroTrace(float moveDist, Vector3 pos, bool isStop)
        {
            S_HeroFootPos = pos;

            if (!S_HeroTraceEnabled)
            {
                //SimpleGrassTrace.UpdateGrassTraceShaderVars();
                return;
            }

            int maxTraceLen = S_HeroTraceList.Length - 1;
            _heroTraceDelta += Time.deltaTime;
            //定时采样英雄运动位置
            _heroTraceMovedDist += moveDist;
            bool addNew = (_heroTraceDelta >= this.sampleInterval && (_heroTraceMovedDist >= 0.1f));
            if (addNew && !_heroTraceWillDisable)
            {
                _heroTraceDelta = 0.0f;
                _heroTraceMovedDist = 0.0f;

                int endIndex = S_HeroTraceList.Length - 1;
                for (int i = endIndex; i > 0 ; --i)
                {
                    S_HeroTraceList[i] = S_HeroTraceList[i-1];
                    S_HeroTraceStateList[i] = S_HeroTraceStateList[i-1];
                    S_HeroTraceDurationList[i] = S_HeroTraceDurationList[i-1];
                }
                Vector4 newPos = new Vector4(pos.x, pos.y, pos.z, 0.0f);
                S_HeroTraceList[0] = newPos;
                S_HeroTraceStateList[0] = 1.0f;
                S_HeroTraceDurationList[0] = 0.0f;

                OnGenerateTrace?.Invoke(pos);
            }

            int nearestIdx = -1;
            float nearestDist = 99999f;
            int farestIdx = -1;
            float farestDist = 0f;
            for (int i = 0; i <= S_HeroTraceList.Length - 1; i++)
            {
                Vector4 tracPos = S_HeroTraceList[i];           
                float tracState = S_HeroTraceStateList[i];//.x 增减系数-1，1 ; .y 延迟时间 
                float tracStateDuration = S_HeroTraceDurationList[i];
                Vector3 tmpPos = new Vector3(tracPos.x, tracPos.y, tracPos.z);
                float dist = (pos - tmpPos).magnitude;

                float priorRatio = tracPos.w;
                //扩大,延时计算中
                if (priorRatio >= 1.0f && tracState >= 1.0f)
                {
                    tracStateDuration += Time.deltaTime;
                    if (tracStateDuration >= fadeoutDelay)
                    {
                        tracState = -1f;//开始缩减
                        tracStateDuration = 0.0f;
                    }
                }
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearestIdx = i;
                }
                float curFadeSpeed = fadeSpeed;                
                if (isStop && Mathf.Approximately(tracState, -1f))
                {
                    curFadeSpeed = fadeoutSpeedInStop;
                }
                tracPos.w += Time.deltaTime * curFadeSpeed * tracState;
                tracPos.w = Mathf.Clamp(tracPos.w, -1f, 1.0f);
                S_HeroTraceList[i] = tracPos;
                S_HeroTraceStateList[i] = tracState;
                S_HeroTraceDurationList[i] = tracStateDuration;
            }

            //当前位置，一直处于扩大状态
            //if (nearestIdx >= 0 && !_heroTraceWillDisable)
            //{
            //    Vector4 tracPos = S_HeroTraceList[nearestIdx];
            //    tracPos.x = pos.x;
            //    tracPos.y = pos.y;
            //    tracPos.z = pos.z;                               
            //    S_HeroTraceList[nearestIdx] = tracPos;

            //    //.x 增减系数-1，1 ; .y 延迟时间                  
            //    float tracState = 1.0f;
            //    float tracStateDuration = 0.0f;
            //    S_HeroTraceStateList[nearestIdx] = tracState;
            //    S_HeroTraceDurationList[nearestIdx] = tracStateDuration;
            //}

            //SimpleGrassTrace.UpdateGrassTraceShaderVars();
        }

        static public void ClearHeroTraceData()
        {
            for (int i = 0; i <= S_HeroTraceList.Length - 1; ++i)
            {
                S_HeroTraceList[i] = new Vector4(-9999f, -9999f, -9999f, -1.0f);
                S_HeroTraceStateList[i] = 1.0f;
                S_HeroTraceDurationList[i] = 0.0f;
            }
               

            S_HeroTraceParam = Vector4.zero;

            S_HeroTraceEnabled = false;
        }

        static public void UpdateGrassTraceShaderVars()
        {
            Shader.SetGlobalVectorArray(PID_HEROTRACELIST, S_HeroTraceList);
            Shader.SetGlobalFloatArray(PID_HEROTRACESTATES, S_HeroTraceStateList);
            Shader.SetGlobalVector(PID_HEROTRACEPARAM, S_HeroTraceParam);
        }

        static public void OnTraceEnclosureDidChange(bool inTraceCollider)
        {
            SimpleGrassTrace.S_HeroInTraceCollider = inTraceCollider;
        }

        #endregion
    }
}