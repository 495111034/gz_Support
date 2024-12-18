using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public enum MaterialAnimationType
{
    None = 0,
    Tilling = 1,
    Color = 2,          //颜色
    BurnAmount = 3,     //消融进度
    BumpScale = 4,      //凹凸倍数
    BumpHeight = 5,     //高度倍数
    
    Translate = 6,      //平移
    Rotation = 7,       //旋转
    Scale = 8  ,        //缩放
    
}

public enum BillboardType
{
    None = 0,
    Billboard = 1,
    HorizontalBillboard = 2,
    VerticalBillboard = 3,
}


[Serializable]
public struct MaterialAnimationInfo
{
    public string property_name;        //属性名
    public int property_id ;            //属性ID（不可信任打包机上的ID）

    public int effect_times ;       //效果次数 <=0表示无限循环    

    public float delay_seconds;         //延迟
    public float total_seconds ;        //播放时间（单次）  

    public MaterialAnimationType matPropertyType;   //属性类型

    public float[] frameTimes;

    public float fromValue;
    public float dstValue;
    public float[] values;

    public Vector4 fromTextureSt;
    public Vector4 dstTextureSt; 
    public Vector4[] TextureSts;

    [ColorUsage(false, true)]
    public Color fromColor;
    [ColorUsage(false, true)]
    public Color dstColor;
    [ColorUsage(false, true)]
    public Color[] Colors;

    public Vector3 fromV3;
    public Vector3 dstV3;
    public Vector3[] Vector3s;
}

[ExecuteInEditMode]
[RequireComponent(typeof(Renderer))]
[AddComponentMenu("Effect/MaterialAnimation")]
public class MaterialAnimationBehaviour : MonoBehaviour
{

    [HideInInspector]
    [SerializeField]
    protected MaterialAnimationInfo[] _materialAnimtionList;

    [HideInInspector]
    [SerializeField]
    protected BillboardType _binboardType = BillboardType.None;

    [HideInInspector]
    [SerializeField]
    protected bool _disableInstance = false;

    class PlayedData
    {
        public int _played_times;       //已演示次数
        public float _played_seconds;      //已演示时间(单次)
        
    }
  

    MaterialPropertyBlock _block = null;
    Material _shaderMaterial;
    Renderer _render;

    int _frameCount = -1;
    bool _isVisible = false;

    Dictionary<int, PlayedData> PlayedDataDic = new Dictionary<int, PlayedData>();

    /// <summary>
    /// 广告牌
    /// </summary>
    public BillboardType binboardType
    {
        get
        {
            return _binboardType;
        }
        set
        {
            _binboardType = value;
        }
    }
    /// <summary>
    /// 禁用硬件实例化
    /// </summary>
    public bool DisableInstance
    {
        get
        {
            return _disableInstance;
        }       
    }

    public MaterialPropertyBlock materialPropertys
    {
        get
        {
            return _block;
        }
    }

    static Dictionary<GameObject, MaterialAnimationBehaviour> InstanceList = new Dictionary<GameObject, MaterialAnimationBehaviour>();

    public static MaterialAnimationBehaviour GetInstanceByGameobject(GameObject obj)
    {
        if (InstanceList.ContainsKey(obj))
            return InstanceList[obj];
        return null;
    }

    private void Start()
    {       
        //Resert();
    }

    private void OnEnable()
    {        
        if(_block == null)
        {
            _block = new MaterialPropertyBlock();
        }
        if(!_render)
        {
            _render = GetComponent<Renderer>();
        }
        if(!_shaderMaterial)
        {           
            if(_render)
            {
                _shaderMaterial = _render.sharedMaterial;
            }
        }
        Resert();

        InstanceList[gameObject] = this;
    }

    private void OnBecameInvisible()
    {
        _isVisible = false;
    }

    private void OnBecameVisible()
    {
        _isVisible = true;
    }

    private void OnDestroy()
    {        
        ClearData();
        if (InstanceList.ContainsKey(gameObject))
            InstanceList.Remove(gameObject);
    }

    private void OnDisable()
    {
        if(InstanceList.ContainsKey(gameObject))
            InstanceList.Remove(gameObject);
        ClearData();
        SetLogicActive(true);
    }

    bool m_LogicActive = true;
    public void SetLogicActive(bool isActive)
    {
        if(isActive != m_LogicActive)
        {
            Resert();
            m_LogicActive = isActive;
        }        
    }
    public void SetVisibleInScene(bool isVisible)
    {
        _isVisible = isVisible;
    }

    private void Update()
    {
        if(Application.isPlaying)
        {
            if (_frameCount == Time.frameCount)
                return;

            _frameCount = Time.frameCount;

            __doMoveScaleRotationUpdate(Time.deltaTime);
            if (_isVisible) __doTransfromUdpate();
            if (m_LogicActive) __doUpdate(Time.deltaTime);         
        }
    }

    public void DoUpdateByTimeline(float deltaTime)
    {        
        __doUpdate(deltaTime);
    }

#if UNITY_EDITOR

    public void OnEditor_Update(float deltaTime)
    {
        if(!Application.isPlaying)
        {
            __doMoveScaleRotationUpdate(deltaTime);
            __doTransfromUdpate();
            __doUpdate(deltaTime);
        }
    }


#endif

    /// <summary>
    /// 平移、旋转与缩放更新
    /// </summary>
    private void __doMoveScaleRotationUpdate(float deltaTime)
    {

        UnityEngine.Profiling.Profiler.BeginSample("MaterialAnimationBehaviour __doMoveScaleRotationUpdate ");
        for (int i = 0; i < _materialAnimtionList.Length; ++i)
        {
            var item = _materialAnimtionList[i];
            if (item.total_seconds <= 0) continue;

            if (item.matPropertyType < MaterialAnimationType.Translate || item.matPropertyType > MaterialAnimationType.Scale) continue;

            if (!PlayedDataDic.ContainsKey(i))
            {
                PlayedDataDic[i] = new PlayedData()
                {

                };
            }

            if (item.effect_times > 0 && PlayedDataDic[i]._played_times >= item.effect_times) continue;

            PlayedDataDic[i]._played_seconds = PlayedDataDic[i]._played_seconds + deltaTime;

            var progress = Mathf.Clamp01((PlayedDataDic[i]._played_seconds - item.delay_seconds) / item.total_seconds);
            int frameIdx = getCurrentFrameIdx(progress, item.frameTimes);

            
            Vector3 srcvector;
            Vector3 dstvector;
            float srcTime = 0;
            float dstTime = 0;
            float sub_progress = 0;


            if (frameIdx == -1)
            {
                srcvector = item.fromV3;
                dstvector = item.Vector3s != null && item.Vector3s.Length > 0 ? item.Vector3s[0] : item.dstV3;

                srcTime = 0;
                dstTime = (item.frameTimes != null && item.frameTimes.Length > 0 ? item.frameTimes[0] : 1) * item.total_seconds;
            }
            else if (item.frameTimes != null && frameIdx == item.frameTimes.Length - 1)
            {
                srcvector = item.Vector3s != null && item.Vector3s.Length > frameIdx ? item.Vector3s[frameIdx] : item.fromV3;
                dstvector = item.dstV3;                

                srcTime = item.frameTimes[frameIdx] * item.total_seconds;
                dstTime = item.total_seconds;
            }
            else
            {               
                if (item.frameTimes != null && item.frameTimes.Length > frameIdx + 1 && item.Vector3s != null && item.Vector3s.Length > frameIdx + 1)
                {
                    srcvector = item.Vector3s[frameIdx];
                    dstvector = item.Vector3s[frameIdx + 1];

                    srcTime = item.frameTimes[frameIdx] * item.total_seconds;
                    dstTime = item.frameTimes[frameIdx + 1] * item.total_seconds;
                }
                else
                {
                    srcvector = item.fromV3;
                    dstvector = item.dstV3;

                    srcTime = 0;
                    dstTime = item.total_seconds;
                }
            }
            sub_progress = ((PlayedDataDic[i]._played_seconds - item.delay_seconds) - srcTime) / (dstTime - srcTime);

            // var func = easing.EaseUtils.GetEaseFunc(easing.EaseType.Linear, easing.InOutType.In);
            // if (func != null) progress = func(progress, 0, 1, 1);

           
            switch (item.matPropertyType)
            {
                case MaterialAnimationType.Translate:
                    gameObject.transform.localPosition = srcvector + (dstvector - srcvector) * sub_progress;
                    break;
                case MaterialAnimationType.Scale:
                    gameObject.transform.localScale = srcvector + (dstvector - srcvector) * sub_progress;
                    break;
                case MaterialAnimationType.Rotation:
                    gameObject.transform.localRotation = Quaternion.Euler(srcvector + (dstvector - srcvector) * sub_progress);
                    
                    break;
               
            }
           

            if (PlayedDataDic[i]._played_seconds >= item.total_seconds + item.delay_seconds)
            {
                PlayedDataDic[i]._played_seconds =0;
                PlayedDataDic[i]._played_times++;
            }
        }
        UnityEngine.Profiling.Profiler.EndSample();
    }

    public static Camera CurrentCamera = null;

    /// <summary>
    /// 广告牌更新
    /// </summary>
    private void __doTransfromUdpate()
    { 
        if (_binboardType != BillboardType.None)
        {          
            if (Application.isPlaying )
            {
                //Camera.main勿每帧调用，其背后在做find操作
                if (!CurrentCamera)
                    CurrentCamera = Camera.main;                
            }
            else
            {
                if (!CurrentCamera)
                    CurrentCamera = Camera.current;
            }
            if (CurrentCamera)
            {
                UnityEngine.Profiling.Profiler.BeginSample("MaterialAnimationBehaviour __doTransfromUdpate ");
                transform.LookAt(transform.position + CurrentCamera.transform.rotation * Vector3.forward,
                     CurrentCamera.transform.rotation * Vector3.up);

                Vector3 eulerAngles = transform.eulerAngles;
                switch (_binboardType)
                {
                    case BillboardType.Billboard:
                        break;
                    case BillboardType.HorizontalBillboard:

                        eulerAngles.z = 0;
                        transform.eulerAngles = eulerAngles;
                        break;
                    case BillboardType.VerticalBillboard:

                        eulerAngles.x = 0;
                        transform.eulerAngles = eulerAngles;
                        break;
                }
                UnityEngine.Profiling.Profiler.EndSample();

            }
        }
    }

    /// <summary>
    /// 材质更新
    /// </summary>
    /// <param name="deltaTime"></param>
    private void __doUpdate(float deltaTime)
    {
        if (!_shaderMaterial)
        {
            if (_render)
            {
                _shaderMaterial = _render.sharedMaterial;
            }
        }

        if (_block == null ||! _render || !_shaderMaterial || _materialAnimtionList == null || _materialAnimtionList.Length == 0) return;
        //Log.LogError($"{_render.name},{_render.bounds.size},{_render.bounds.center}");

        UnityEngine.Profiling.Profiler.BeginSample("MaterialAnimationBehaviour SetPropertys ");
        // _block.Clear();
        for (int i = 0; i < _materialAnimtionList.Length; ++i)
        {

            if (_materialAnimtionList[i].total_seconds <= 0) continue;

            if (_materialAnimtionList[i].matPropertyType <= MaterialAnimationType.None || _materialAnimtionList[i].matPropertyType >= MaterialAnimationType.Translate) continue;

            if (!PlayedDataDic.ContainsKey(i))
            {
                PlayedDataDic[i] = new PlayedData()
                {

                };
            }

            if (_materialAnimtionList[i].effect_times > 0 && PlayedDataDic[i]._played_times >= _materialAnimtionList[i].effect_times) continue;
            if (string.IsNullOrEmpty(_materialAnimtionList[i].property_name)) { Log.LogError($"cannt found property_name:{_materialAnimtionList[i].property_name}"); continue; }

            //打包的机器得到的Shader.PropertyToID不一至，因此不能信任打包机上获得的shaderid
            if (_materialAnimtionList[i].property_id <= 0)
            {               
                _materialAnimtionList[i].property_id = resource.ShaderNameHash.ShaderNameId(_materialAnimtionList[i].property_name);
            }
            if (_materialAnimtionList[i].property_id <= 0) continue;
            //if (!_shaderMaterial.HasProperty(_materialAnimtionList[i].property_id)) continue;


            PlayedDataDic[i]._played_seconds = PlayedDataDic[i]._played_seconds + deltaTime;           

            var progress = Mathf.Clamp01((PlayedDataDic[i]._played_seconds - _materialAnimtionList[i].delay_seconds) / _materialAnimtionList[i].total_seconds);
            int frameIdx = getCurrentFrameIdx(progress, _materialAnimtionList[i].frameTimes);

            Color srccolor;
            Color dstcolor;
            float srcvalue = 0;
            float dstvalue = 0;
            Vector4 srcvector;
            Vector4 dstvector;
            float srcTime = 0;
            float dstTime = 0;
            float sub_progress = 0;


            if (frameIdx == -1)
            {
                //srcvalue = _materialAnimtionList[i].fromValue;
                // dstvalue = _materialAnimtionList[i].values[0];
                srcTime = 0;
                dstTime = (_materialAnimtionList[i].frameTimes != null && _materialAnimtionList[i].frameTimes.Length > 0 ? _materialAnimtionList[i].frameTimes[0] : 1) * _materialAnimtionList[i].total_seconds;
            }
            else if (_materialAnimtionList[i].frameTimes != null && frameIdx == _materialAnimtionList[i].frameTimes.Length - 1)
            {
                //  srcvalue = _materialAnimtionList[i].values[frameIdx];
                // dstvalue = _materialAnimtionList[i].dstValue;

                srcTime = _materialAnimtionList[i].frameTimes[frameIdx] * _materialAnimtionList[i].total_seconds;
                dstTime = _materialAnimtionList[i].total_seconds;
            }
            else
            {
                //  srcvalue = _materialAnimtionList[i].values[frameIdx];
                //  dstvalue = _materialAnimtionList[i].values[frameIdx + 1];
                if (_materialAnimtionList[i].frameTimes.Length > frameIdx + 1)
                {
                    srcTime = _materialAnimtionList[i].frameTimes[frameIdx] * _materialAnimtionList[i].total_seconds;
                    dstTime = _materialAnimtionList[i].frameTimes[frameIdx + 1] * _materialAnimtionList[i].total_seconds;
                }
                else
                {
                    srcTime = 0;
                    dstTime = _materialAnimtionList[i].total_seconds;
                }
            }

            sub_progress = Mathf.Clamp01(((PlayedDataDic[i]._played_seconds - _materialAnimtionList[i].delay_seconds) - srcTime) / (dstTime - srcTime));

            

            // var func = easing.EaseUtils.GetEaseFunc(easing.EaseType.Linear, easing.InOutType.In);
            // if (func != null) progress = func(progress, 0, 1, 1);

            UnityEngine.Profiling.Profiler.BeginSample("MaterialAnimationBehaviour SetPropertys to block");
            switch (_materialAnimtionList[i].matPropertyType)
            {
                case MaterialAnimationType.BurnAmount:
                case MaterialAnimationType.BumpScale:
                    if (frameIdx == -1)
                    {
                        srcvalue = _materialAnimtionList[i].fromValue;
                        dstvalue = _materialAnimtionList[i].values != null && _materialAnimtionList[i].values.Length > 0 ? _materialAnimtionList[i].values[0] : _materialAnimtionList[i].dstValue;
                    }
                    else if (_materialAnimtionList[i].frameTimes != null && frameIdx == _materialAnimtionList[i].frameTimes.Length - 1)
                    {
                        srcvalue = _materialAnimtionList[i].values[frameIdx];
                        dstvalue = _materialAnimtionList[i].dstValue;
                    }
                    else
                    {
                        if (_materialAnimtionList[i].values != null && _materialAnimtionList[i].values.Length > frameIdx + 1)
                        {
                            srcvalue = _materialAnimtionList[i].values[frameIdx];
                            dstvalue = _materialAnimtionList[i].values[frameIdx + 1];
                        }
                        else
                        {
                            srcvalue = _materialAnimtionList[i].fromValue;
                            dstvalue = _materialAnimtionList[i].dstValue;
                        }
                    }

                    _block.SetFloat(_materialAnimtionList[i].property_id, srcvalue + (dstvalue - srcvalue) * sub_progress);
                    break;
                case MaterialAnimationType.Color:
                    if (frameIdx == -1)
                    {
                        srccolor = _materialAnimtionList[i].fromColor;
                        dstcolor = _materialAnimtionList[i].Colors != null && _materialAnimtionList[i].Colors.Length > 0 ? _materialAnimtionList[i].Colors[0] : _materialAnimtionList[i].dstColor;
                    }
                    else if (_materialAnimtionList[i].frameTimes != null && frameIdx == _materialAnimtionList[i].frameTimes.Length - 1)
                    {
                        srccolor = _materialAnimtionList[i].Colors[frameIdx];
                        dstcolor = _materialAnimtionList[i].dstColor;
                    }
                    else
                    {
                        if (_materialAnimtionList[i].Colors != null && _materialAnimtionList[i].Colors.Length > frameIdx + 1)
                        {
                            srccolor = _materialAnimtionList[i].Colors[frameIdx];
                            dstcolor = _materialAnimtionList[i].Colors[frameIdx + 1];
                        }
                        else
                        {
                            srccolor = _materialAnimtionList[i].fromColor;
                            dstcolor = _materialAnimtionList[i].dstColor;
                        }
                    }
                    
                    var color = srccolor + (dstcolor - srccolor) * sub_progress;
                    _block.SetColor(_materialAnimtionList[i].property_id, color);
                    break;
                case MaterialAnimationType.Tilling:
                    if (frameIdx == -1)
                    {
                        srcvector = _materialAnimtionList[i].fromTextureSt;
                        dstvector = _materialAnimtionList[i].TextureSts != null && _materialAnimtionList[i].TextureSts.Length > 0 ? _materialAnimtionList[i].TextureSts[0] : _materialAnimtionList[i].dstTextureSt;
                    }
                    else if (_materialAnimtionList[i].TextureSts != null && frameIdx == _materialAnimtionList[i].TextureSts.Length - 1)
                    {
                        srcvector = _materialAnimtionList[i].TextureSts[frameIdx];
                        dstvector = _materialAnimtionList[i].dstTextureSt;
                    }
                    else
                    {
                        if (_materialAnimtionList[i].TextureSts != null && _materialAnimtionList[i].TextureSts.Length > frameIdx + 1)
                        {
                            srcvector = _materialAnimtionList[i].TextureSts[frameIdx];
                            dstvector = _materialAnimtionList[i].TextureSts[frameIdx + 1];
                        }
                        else
                        {
                            srcvector = _materialAnimtionList[i].fromTextureSt;
                            dstvector = _materialAnimtionList[i].dstTextureSt;
                        }
                    }

                    var vector = srcvector + (dstvector - srcvector) * sub_progress;
                    _block.SetVector(_materialAnimtionList[i].property_id, vector);


                    //_block.SetVector(_materialAnimtionList[i].property_id, _materialAnimtionList[i].fromTextureSt + (_materialAnimtionList[i].dstTextureSt - _materialAnimtionList[i].fromTextureSt) * progress);
                    break;
            }
            UnityEngine.Profiling.Profiler.EndSample();

            if (PlayedDataDic[i]._played_seconds >= _materialAnimtionList[i].total_seconds + _materialAnimtionList[i].delay_seconds)
            {
                PlayedDataDic[i]._played_seconds = 0;
                PlayedDataDic[i]._played_times++;
            }
        }

        if (_render.enabled && m_LogicActive)
        {
            UnityEngine.Profiling.Profiler.BeginSample("MaterialAnimationBehaviour SetPropertys to render ");
            _render.SetPropertyBlock(_block);
            UnityEngine.Profiling.Profiler.EndSample();
        }

        UnityEngine.Profiling.Profiler.EndSample();
    }

    int getCurrentFrameIdx(float t,float[] times)
    {
        int idx = -1;
        for (int i = 0; i < times.Length; ++i)
        {
            if (t >= times[i])
                idx = i;

            if (t < times[i])
                break;
        }

        return idx;
    }

    void ClearData()
    {
        if(_block != null)
        {
            _block.Clear();
        }
        _block = null;
    }

    public void Resert()
    {
        if (_materialAnimtionList == null) return;
        PlayedDataDic.Clear();
        for (int i = 0; i < _materialAnimtionList.Length; ++i)
        {
            //以下代码中，item与_materialAnimtionList[i]不是同一实例，修改item不会影响已序列化的_materialAnimtionList，因此修改item对序列化数据的修改无效
            //var item = _materialAnimtionList[i];
            if (!PlayedDataDic.ContainsKey(i))
            {
                PlayedDataDic[i] = new PlayedData()
                {

                };
            }
            PlayedDataDic[i]._played_times = 0;
            PlayedDataDic[i]._played_seconds = 0;

            //不能的机器得到的Shader.PropertyToID不一至，因此不能信任打包机上获得的shaderid
            if (Application.isPlaying)
            {
                _materialAnimtionList[i].property_id = -1;
            }
        }

        m_LogicActive = true;
    }
}
