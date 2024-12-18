//
// Spray - particle system
//
using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;
namespace MyParticle
{
    [ExecuteInEditMode]
    [AddComponentMenu("MyParcitle/Spray 粒子喷发器")]
    public partial class Spray : MonoBehaviour
    {
        #region 基本属性

        [SerializeField]
        int _maxParticles = 100;

        public int maxParticles {
            get {
                // Returns actual number of particles.
                if (_bulkMesh == null || _bulkMesh.copyCount < 1) return 0;
                return (_maxParticles / _bulkMesh.copyCount + 1) * _bulkMesh.copyCount;
            }
        }    
        
        public int meshVectexs
        {
            get
            {
                if (maxParticles <= 0) return 0;
                return _bulkMesh.mesh?_bulkMesh.mesh.vertexCount:0;
            }
        }

        /// <summary>
        /// 用于绘制的commandBuffer，如果为空，则全局Graphic绘制
        /// </summary>
        public CommandBuffer mCommandBuffer { get; set; }

        #endregion

        #region 喷发器参数

        [SerializeField]
        Vector3 _emitterCenter = Vector3.zero;

        public Vector3 emitterCenter {
            get { return _emitterCenter; }
            set { _emitterCenter = value; }
        }

        [SerializeField]
        Vector3 _emitterSize = Vector3.one;

        public Vector3 emitterSize {
            get { return _emitterSize; }
            set { _emitterSize = value; }
        }

        [SerializeField, Range(0, 1)]
        float _throttle = 1.0f;

        public float throttle {
            get { return _throttle; }
            set { _throttle = value; }
        }

        #endregion

        #region 特效生命周期参数

        [SerializeField]
        float _life = 4.0f;

        public float life {
            get { return _life; }
            set { _life = value; }
        }

        [SerializeField, Range(0, 1)]
        float _lifeRandomness = 0.6f;

        public float lifeRandomness {
            get { return _lifeRandomness; }
            set { _lifeRandomness = value; }
        }

        #endregion

        #region 速度相关

        [SerializeField]
        Vector3 _initialVelocity = Vector3.forward * 4.0f;

        public Vector3 initialVelocity {
            get { return _initialVelocity; }
            set { _initialVelocity = value; }
        }

        [SerializeField, Range(0, 1)]
        float _directionSpread = 0.2f;  //方向

        public float directionSpread {
            get { return _directionSpread; }
            set { _directionSpread = value; }
        }

        [SerializeField, Range(0, 1)]
        float _speedRandomness = 0.5f;

        public float speedRandomness {
            get { return _speedRandomness; }
            set { _speedRandomness = value; }
        }

        #endregion

        #region 发射相关

        [SerializeField]
        Vector3 _acceleration = Vector3.zero;

        /// <summary>
        /// 发射方向
        /// </summary>
        public Vector3 acceleration {
            get { return _acceleration; }
            set { _acceleration = value; }
        }

        [SerializeField, Range(0, 4)]
        float _drag = 0.1f;

        /// <summary>
        /// 阻力
        /// </summary>
        public float drag {
            get { return _drag; }
            set { _drag = value; }
        }

        /// <summary>
        /// 总发射时间，小于等于0为无限
        /// </summary>
        [SerializeField]
        float _total_seconds = 0f;

        public float TotalSeconds
        {
            get { return _total_seconds; }
            set { _total_seconds = value; }
        }

        #endregion

        #region 旋转相关
        [HideInInspector]
        [SerializeField]
        bool _Billboarding = false;

        public bool Billboarding {
            get { return _Billboarding; }
            set
            {
                _Billboarding = value;
                SetBillboardMat(_Billboarding);
            }
        }

        [HideInInspector]
        [SerializeField]
        float _VerticalBillboarding = 1f;
        public float VerticalBillboarding
        {
            get
            {
                return _VerticalBillboarding;
            }
            set
            {
                _VerticalBillboarding = value;
            }
        }

        [SerializeField]
        float _spin = 20.0f;

        /// <summary>
        /// 转动
        /// </summary>
        public float spin {
            get { return _spin; }
            set { _spin = value; }
        }

        [SerializeField]
        float _speedToSpin = 60.0f;

        /// <summary>
        /// 转动速度
        /// </summary>
        public float speedToSpin {
            get { return _speedToSpin; }
            set { _speedToSpin = value; }
        }

        [SerializeField, Range(0, 1)]
        float _spinRandomness = 0.3f;

        /// <summary>
        /// 转动随机性
        /// </summary>
        public float spinRandomness {
            get { return _spinRandomness; }
            set { _spinRandomness = value; }
        }

        #endregion

        #region 噪音相关

        [SerializeField]
        float _noiseAmplitude = 1.0f;

        /// <summary>
        /// 噪音振幅
        /// </summary>
        public float noiseAmplitude {
            get { return _noiseAmplitude; }
            set { _noiseAmplitude = value; }
        }

        [SerializeField]
        float _noiseFrequency = 0.2f;

        /// <summary>
        /// 噪音频率
        /// </summary>
        public float noiseFrequency {
            get { return _noiseFrequency; }
            set { _noiseFrequency = value; }
        }

        [SerializeField]
        float _noiseMotion = 1.0f;

        /// <summary>
        /// 噪音运动
        /// </summary>
        public float noiseMotion {
            get { return _noiseMotion; }
            set { _noiseMotion = value; }
        }

        #endregion

        #region 渲染相关

        [SerializeField]
        Mesh[] _shapes = new Mesh[1];       //网格，可以以多个合并

        public Mesh[] Shapes
        {
            get { return _shapes; }
            set { _shapes = value; }
        }


        [SerializeField]
        float _scale = 1.0f;

        /// <summary>
        /// 网格缩放
        /// </summary>
        public float scale {
            get { return _scale; }
            set { _scale = value; }
        }

        [SerializeField, Range(0, 1)]
        float _scaleRandomness = 0.5f;

        /// <summary>
        /// 缩放随机性
        /// </summary>
        public float scaleRandomness {
            get { return _scaleRandomness; }
            set { _scaleRandomness = value; }
        }

        [SerializeField]
        Material _material;            
       

        /// <summary>
        /// 材质
        /// </summary>
        public Material sharedMaterial {
            get { return _material; }
            set { _material = value; if (___matInstance) Destroy(___matInstance); }
        }

        Material ___matInstance = null;
        public Material materialInstance {
            get {
                if (!___matInstance && _material)
                {
                    ___matInstance = Instantiate<Material>(_material);
                    SetBillboardMat(_Billboarding);
                }
                return ___matInstance;
            }            
        }

        public void SetBillboardMat(bool isBillboard)
        {
            var mat = materialInstance;
            if (mat)
            {
                if (isBillboard)
                {                   
                    mat.EnableKeyword("_BILLBOARD");
                }
                else
                {                    
                    mat.DisableKeyword("_BILLBOARD");
                }
            }
        }

        [SerializeField]
        ShadowCastingMode _castShadows;

        /// <summary>
        /// 阴影投射模式（取决于shader中有没有实现阴影投射）
        /// </summary>
        public ShadowCastingMode shadowCastingMode {
            get { return _castShadows; }
            set { _castShadows = value; }
        }

        [SerializeField]
        bool _receiveShadows = false;

        /// <summary>
        /// 是否接收阴影（取决于shader中有没有实现阴影接收）
        /// </summary>
        public bool receiveShadows {
            get { return _receiveShadows; }
            set { _receiveShadows = value; }
        }

        #endregion

        #region 其它设置

        [SerializeField]
        int _randomSeed = 0;

        public int randomSeed {
            get { return _randomSeed; }
            set { _randomSeed = value; }
        }

        [SerializeField]
        bool _debug;

        #endregion

        #region 资源参数       

        #endregion

        #region 私有参数与属性

        Vector3 _noiseOffset;
        RenderTexture _positionBuffer1;
        RenderTexture _positionBuffer2;
        RenderTexture _velocityBuffer1; 
        RenderTexture _velocityBuffer2;
        RenderTexture _rotationBuffer1;
        RenderTexture _rotationBuffer2;
        BulkMesh _bulkMesh;
        Material _kernelMaterial;
        Material _debugMaterial;
        MaterialPropertyBlock _props;
        bool _needsReset = true;
        float _startTime = 0f;

        static float deltaTime {
            get {
                var isEditor = !Application.isPlaying || Time.frameCount < 2;
                return isEditor ? 1.0f / 10 : Time.deltaTime;
            }
        }

        #endregion

        #region 资源管理

        public void NotifyConfigChange()
        {
            _needsReset = true;
            _startTime = 0f;
        }

        public void AddMesh(Mesh m)
        {
            if(_shapes == null)
            {
                _shapes = new Mesh[] { m};
            }
            else
            {
                for(int i = 0; i < _shapes.Length; ++i)
                {
                    if (_shapes[i] == null || _shapes[i] == m)
                    {
                        _shapes[i] = m;
                        return;
                    }
                }

                var tmp = new Mesh[_shapes.Length + 1];
                for (int i = 0; i < _shapes.Length; ++i)
                {
                    tmp[i] = _shapes[i];
                }
                tmp[_shapes.Length ] = m;
                _shapes = tmp;
            }
        }

        Material CreateMaterial(Shader shader)
        {
            var material = new Material(shader);
            material.hideFlags = HideFlags.DontSave;
            return material;
        }

        RenderTexture CreateBuffer()
        {
            var width = _bulkMesh.copyCount;
            var height = _maxParticles / width + 1;
            var buffer = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGBFloat); // new RenderTexture(width, height, 0, RenderTextureFormat.ARGBFloat);
            buffer.hideFlags = HideFlags.DontSave;
            buffer.filterMode = FilterMode.Point;
            buffer.wrapMode = TextureWrapMode.Repeat;
            return buffer;
        }

        void UpdateKernelShader(float deltaTime)
        {
            var m = _kernelMaterial;
            if (!m) return;

            m.SetVector("_EmitterPos", _emitterCenter);
            m.SetVector("_EmitterSize", _emitterSize);

            var invLifeMax = 1.0f / Mathf.Max(_life, 0.01f);
            var invLifeMin = invLifeMax / Mathf.Max(1 - _lifeRandomness, 0.01f);
            m.SetVector("_LifeParams", new Vector2(invLifeMin, invLifeMax));

            if (_initialVelocity == Vector3.zero)
            {
                m.SetVector("_Direction", new Vector4(0, 0, 1, 0));
                m.SetVector("_SpeedParams", Vector4.zero);
            }
            else
            {
                var speed = _initialVelocity.magnitude;
                var dir = _initialVelocity / speed;
                m.SetVector("_Direction", new Vector4(dir.x, dir.y, dir.z, _directionSpread));
                m.SetVector("_SpeedParams", new Vector2(speed, _speedRandomness));
            }

            var drag = Mathf.Exp(-_drag * deltaTime);
            var aparams = new Vector4(_acceleration.x, _acceleration.y, _acceleration.z, drag);
            m.SetVector("_Acceleration", aparams);

            var pi360 = Mathf.PI / 360;
            var sparams = new Vector3(_spin * pi360, _speedToSpin * pi360, _spinRandomness);
            m.SetVector("_SpinParams", sparams);

            m.SetVector("_NoiseParams", new Vector2(_noiseFrequency, _noiseAmplitude));

            // 如果发射方向为空，则噪音向上运动
            if (_acceleration == Vector3.zero)
                _noiseOffset += Vector3.up * _noiseMotion * deltaTime;
            else
                _noiseOffset += _acceleration.normalized * _noiseMotion * deltaTime;

            m.SetVector("_NoiseOffset", _noiseOffset);

            m.SetVector("_Config", new Vector4(_throttle, _randomSeed, deltaTime, Time.time));
        }

        void ResetResources()
        {
            if (___matInstance)
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
                    Destroy(___matInstance);
                else
                    DestroyImmediate(___matInstance);
#else
                Destroy(___matInstance);
#endif
            }
            ___matInstance = null;


            //一个网格最大64k顶点数，如果_maxParticles粒子数产生的顶点数超过此数值，则以64K为上限，并通过多次drawcall的方式达到_maxParticles的数量
            if (_bulkMesh == null)
                _bulkMesh = new BulkMesh(_shapes,_maxParticles);
            else
                _bulkMesh.Rebuild(_shapes, _maxParticles);


            if (_positionBuffer1) RenderTexture.ReleaseTemporary(_positionBuffer1);// DestroyImmediate(_positionBuffer1);
            if (_positionBuffer2) RenderTexture.ReleaseTemporary(_positionBuffer2);
            if (_velocityBuffer1) RenderTexture.ReleaseTemporary(_velocityBuffer1);
            if (_velocityBuffer2) RenderTexture.ReleaseTemporary(_velocityBuffer2);
            if (_rotationBuffer1) RenderTexture.ReleaseTemporary(_rotationBuffer1);
            if (_rotationBuffer2) RenderTexture.ReleaseTemporary(_rotationBuffer2);

            _positionBuffer1 = CreateBuffer();
            _positionBuffer2 = CreateBuffer();
            _velocityBuffer1 = CreateBuffer();
            _velocityBuffer2 = CreateBuffer();
            _rotationBuffer1 = CreateBuffer();
            _rotationBuffer2 = CreateBuffer();

            if (!_kernelMaterial)
            {
                _kernelMaterial = CreateMaterial(resource.ShaderManager.Find("Hidden/MyParcitle/Kernel"));
            }
            if (!_debugMaterial)
            {
                _debugMaterial = CreateMaterial(resource.ShaderManager.Find("Hidden/MyParcitle/Debug")); 
            }

            // 预处理
            InitializeAndPrewarmBuffers();
            _startTime = Time.time;
            _needsReset = false;
        }

        void InitializeAndPrewarmBuffers()
        {
            _noiseOffset = Vector3.zero;

            UpdateKernelShader(Time.deltaTime);

            Graphics.Blit(null, _positionBuffer2, _kernelMaterial, 0);//初始平移
            Graphics.Blit(null, _velocityBuffer2, _kernelMaterial, 1);//初始速度

            if (_Billboarding)
            {
                Graphics.Blit(null, _rotationBuffer2, _kernelMaterial, 7);//无旋转
            }
            else
            {
                Graphics.Blit(null, _rotationBuffer2, _kernelMaterial, 2);//初始旋转
            }

            for (var i = 0; i < 8; i++) {
                SwapBuffersAndInvokeKernels();
                UpdateKernelShader(Time.deltaTime);
            }
        }

        /// <summary>
        /// gpu 计算
        /// </summary>
        void SwapBuffersAndInvokeKernels()
        {
            // Swap the buffers.
            var tempPosition = _positionBuffer1;
            var tempVelocity = _velocityBuffer1;
            var tempRotation = _rotationBuffer1;

            _positionBuffer1 = _positionBuffer2;
            _velocityBuffer1 = _velocityBuffer2;
            _rotationBuffer1 = _rotationBuffer2;

            _positionBuffer2 = tempPosition;
            _velocityBuffer2 = tempVelocity;
            _rotationBuffer2 = tempRotation;

            // Invoke the position update kernel.
            _kernelMaterial.SetTexture("_PositionBuffer", _positionBuffer1);
            _kernelMaterial.SetTexture("_VelocityBuffer", _velocityBuffer1);            
            _kernelMaterial.SetTexture("_RotationBuffer", _rotationBuffer1);

            Graphics.Blit(null, _positionBuffer2, _kernelMaterial, 3);      //平移更新

            // Invoke the velocity and rotation update kernel
            // with the updated position.
            _kernelMaterial.SetTexture("_PositionBuffer", _positionBuffer2);
            Graphics.Blit(null, _velocityBuffer2, _kernelMaterial, 4);      //速度更新
            if (_Billboarding)
            {
                //Graphics.Blit(null, _rotationBuffer2, _kernelMaterial, 6);      //旋转更新(广告牌不在做旋转更新)
            }
            else
            {
                Graphics.Blit(null, _rotationBuffer2, _kernelMaterial, 5);      //旋转更新(随机旋转)
            }
        }

        public void Release()
        {
            if(_bulkMesh != null)
                _bulkMesh.Release();
            _bulkMesh = null;
        }

#endregion

        #region MonoBehaviour方法

        public void Reset()
        {
            _throttle = 1;
            _needsReset = true;
            _startTime = 0f;
        }

        void OnDestroy()
        {
            if (___matInstance)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    DestroyImmediate(___matInstance);
                }
                else
                {
                    Destroy(___matInstance);
                }
#else
                Destroy(___matInstance);
#endif

            }
            if (_bulkMesh != null) _bulkMesh.Release();
            if (_positionBuffer1) RenderTexture.ReleaseTemporary(_positionBuffer1);
            if (_positionBuffer2) RenderTexture.ReleaseTemporary(_positionBuffer2);
            if (_velocityBuffer1) RenderTexture.ReleaseTemporary(_velocityBuffer1);
            if (_velocityBuffer2) RenderTexture.ReleaseTemporary(_velocityBuffer2);
            if (_rotationBuffer1) RenderTexture.ReleaseTemporary(_rotationBuffer1);
            if (_rotationBuffer2) RenderTexture.ReleaseTemporary(_rotationBuffer2);
            if (_kernelMaterial)  DestroyImmediate(_kernelMaterial);
            if (_debugMaterial)   DestroyImmediate(_debugMaterial);
            _startTime = 0;

            if (mCommandBuffer != null)
                CommandBufferPool.Release(mCommandBuffer);
            mCommandBuffer = null;
        }

#if UNITY_EDITOR

        public void OnEditor_Update(float deltaTime)
        {
            if (!this || !gameObject || !gameObject.activeInHierarchy) return;
            if (UnityEditor.Selection.gameObjects != null && UnityEditor.Selection.gameObjects.Length > 0 && Array.IndexOf( UnityEditor.Selection.gameObjects , gameObject) > -1) return;
            DoUpdate(deltaTime);
        }
#endif
       
        /// <summary>
        /// 计算与绘制
        /// </summary>
        /// <param name="delTime"></param>
        void DoUpdate(float delTime)
        {
            if (Application.isPlaying)
            {
                if (_total_seconds > 0 && Time.time - _startTime >= _total_seconds) { _throttle = 0; }
            }

            //修改(用于计算的shader)数据
            UnityEngine.Profiling.Profiler.BeginSample("UpdateKernelShader");
            UpdateKernelShader(delTime);
            UnityEngine.Profiling.Profiler.EndSample();

            //gpu计算，计算结果存储到RenderTexture中
            UnityEngine.Profiling.Profiler.BeginSample("SwapBuffersAndInvokeKernels");
            SwapBuffersAndInvokeKernels();
            UnityEngine.Profiling.Profiler.EndSample();


            // 设置meterial属性，为drawcall参数
            if (_props == null)
            {
                UnityEngine.Profiling.Profiler.BeginSample("new MaterialPropertyBlock");
                _props = new MaterialPropertyBlock();
                UnityEngine.Profiling.Profiler.EndSample();
            }

            var props = _props;
            props.SetTexture("_PositionBuffer", _positionBuffer2);          //传入粒子位置信息(xyz)和时间信息(w)，gpu计算的结果
            if (!_Billboarding)
            {
                props.SetTexture("_RotationBuffer", _rotationBuffer2);          //传入粒子旋转信息（四元数），gpu计算的结果
            }
            props.SetFloat("_ScaleMin", _scale * (1 - _scaleRandomness));   //传入缩放随机值限制
            props.SetFloat("_ScaleMax", _scale);                            //传入缩放随机值限制
            props.SetFloat("_RandomSeed", _randomSeed);                     //传入随机种子
            if(_Billboarding)
            {
                props.SetFloat("_VerticalBillboarding", _VerticalBillboarding); //广告牌垂直约束
            }

              var mesh = _bulkMesh.mesh;
            var position = transform.position;
            var rotation = transform.rotation;
            var material = materialInstance;
            if (!materialInstance)
            {
                UnityEngine.Profiling.Profiler.BeginSample("Load MyParcitleDefaultMaterial");
                material = Resources.Load<Material>("shader/MyParticle/MyParcitleDefaultMaterial");
                UnityEngine.Profiling.Profiler.EndSample();
            }
            var uv = new Vector2(0.5f / _positionBuffer2.width, 0);


            // 调用drawcall
            // 一个网格最大64k顶点数，如果_maxParticles粒子数产生的顶点数超过此数值，则以64K为上限，并通过多次drawcall的方式达到_maxParticles的数量
            // 每次drawcall填充单行数值，如果网格在64k以下，则只有单行(一次drawcall)
            for (var i = 0; i < _positionBuffer2.height; i++)
            {
                uv.y = (0.5f + i) / _positionBuffer2.height;
                props.SetVector("_BufferOffset", uv);

                if (mCommandBuffer != null)
                {
                    var matex = Matrix4x4.TRS(position, rotation, Vector3.one);
                    UnityEngine.Profiling.Profiler.BeginSample("mCommandBuffer.DrawMesh");
                    mCommandBuffer.DrawMesh(mesh, matex, material, 0, 0, props);
                    UnityEngine.Profiling.Profiler.EndSample();
                }
                else
                {
                    UnityEngine.Profiling.Profiler.BeginSample("Graphics.DrawMesh");
                    Graphics.DrawMesh(
                        mesh, position, rotation,
                        material, (int)ObjLayer.BackGround, null, 0, props,
                        _castShadows, _receiveShadows);
                    UnityEngine.Profiling.Profiler.EndSample();
                }
            }
        }
        int _frameCount = -1;

        bool _isDeviceSupports = false;
        bool _isGetedDeivceInfo = false;
        void Update()
        {

            if(!_isGetedDeivceInfo)
            {
                _isDeviceSupports = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBFloat);
                _isGetedDeivceInfo = true;
            }

            if(!_isDeviceSupports)
            {
                OnDestroy();
                this.enabled = false;
                return;
            }

            if (Application.isPlaying && QualityUtils.IsLowMem && _maxParticles > 300)
            {
                OnDestroy();
                return;
            }
          
            
            if (!this) return;
            if (_needsReset)
            {
                UnityEngine.Profiling.Profiler.BeginSample("DoUpdate");
                ResetResources();
                UnityEngine.Profiling.Profiler.EndSample();
            }

            //已标记为ExecuteInEditMode，在editor下非运行模式也会执行
            // if (Application.isPlaying)
            {
                if (_frameCount == Time.frameCount)
                    return;
                _frameCount = Time.frameCount;

                UnityEngine.Profiling.Profiler.BeginSample("DoUpdate");
                DoUpdate(Time.deltaTime);
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }

#if UNITY_EDITOR
        void OnGUI()
        {
            if (_debug && Event.current.type.Equals(EventType.Repaint))
            {
                if (_debugMaterial && _positionBuffer2 && _velocityBuffer2 && _rotationBuffer2)
                {
                    var w = _positionBuffer2.width;
                    var h = _positionBuffer2.height;

                    var rect = new Rect(0, 0, w, h);
                    Graphics.DrawTexture(rect, _positionBuffer2, _debugMaterial);

                    rect.y += h;
                    Graphics.DrawTexture(rect, _velocityBuffer2, _debugMaterial);

                    rect.y += h;
                    Graphics.DrawTexture(rect, _rotationBuffer2, _debugMaterial);
                }
            }
        }
#endif
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(_emitterCenter, _emitterSize);
        }

        #endregion
    }
}
