using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering;

namespace MyEffect
{
    [ExecuteInEditMode]   
    [RequireComponent(typeof(MyPostEffectsBase))]
    [AddComponentMenu("Camera Effects/HeatDistortion(扭曲)")]
    public class HeatDistortion : MonoBehaviour
    {
        // 主相机
        public Camera _camera;

        // 法线相机，用于捕捉特效喷出的法线图
        [SerializeField]        
        private Camera _normalCamera;

        // 后处理材质
        [SerializeField, HideInInspector]
        private Material _postEffectMaterial;

      
        // 强度
        [SerializeField]
        public float strength = 0.1f;

        /// <summary>
        /// 需要捕捉的产生法线的对象
        /// </summary>
        public static List<DistortionEffect> normalRanders = new List<DistortionEffect>();

        private CommandBuffer mCommandBuf;
        /// <summary>
        /// 法线相机画面（作为法线传入主相机后处理）
        /// </summary>
        private RenderTexture _renderTex;

        //开始时空扭曲的时间
        //public float FlockWareStartTime = 0;
        //时空扭曲总时长
        //public float flockRareLong = 0.5f;
        //时空扭曲的中心点
       // public Vector2 distortCenter = new Vector2(0.5f, 0.5f);

        public Material PostEffectMaterial { get { return _postEffectMaterial; } }

#if UNITY_EDITOR
        void Reset()
        {
            _camera = gameObject.GetComponent<Camera>();
        }
#endif

        private void OnEnable()
        {
            if (GetComponent<MyPostEffectsBase>())
            {
                GetComponent<MyPostEffectsBase>().OnComponentChange();
            }
        }

        void OnDisable()
        {
            ClearData();
            
        }
        private void OnDestroy()
        {
            ClearData();
        }


        private void CreateNormalCamera()
        {
            _camera = gameObject.GetComponent<Camera>();

            if (_camera == null || _normalCamera != null)
                return;
           
            GameObject obj = new GameObject("ImageEffect_HeatDistortion_Camera");

            obj.hideFlags = HideFlags.NotEditable | HideFlags.HideAndDontSave;
           
            obj.transform.SetParent(_camera.transform);
            obj.transform.position = this.transform.position;
            obj.transform.rotation = this.transform.rotation;
           
            _normalCamera = obj.AddComponent<Camera>();
           
            _normalCamera.CopyFrom(_camera);
           
            _normalCamera.clearFlags = CameraClearFlags.SolidColor;
            _normalCamera.backgroundColor = new Color(0f, 0f, 0f);
            _normalCamera.cullingMask = 0;

        }

        void CreateData()
        {
            

            CreateNormalCamera();

            _normalCamera.aspect = _camera.aspect;

            if (!_renderTex)
            {
                
                _renderTex = RenderTexture.GetTemporary(
                    (int)_normalCamera.pixelWidth,      // width
                    (int)_normalCamera.pixelHeight,     // height
                    0,                                  // depth
                    RenderTextureFormat.ARGBHalf);  // format
            }
            _normalCamera.RemoveAllCommandBuffers();
            _normalCamera.targetTexture = _renderTex;

            if (mCommandBuf == null)
            {
                mCommandBuf = CommandBufferPool.Get("CommandBuffer_HeatDistortion");
                _normalCamera.AddCommandBuffer(CameraEvent.BeforeImageEffectsOpaque, mCommandBuf);
            }

            
        }

        public void CreateMat()
        {
            if (!_postEffectMaterial)
            {
                Shader postShader = resource.ShaderManager.Find("Hidden/CameraEffects/HeatDistortion");
                _postEffectMaterial = new Material(postShader);
               // _postEffectMaterial.SetVector("_DistortCenter", new Vector2(0,0));                
                _postEffectMaterial.hideFlags = HideFlags.DontSave;
            }
        }

        void ClearData()
        { 
            if (_renderTex)
            {
                RenderTexture.ReleaseTemporary(_renderTex);
            }
            _renderTex = null;
            if (_normalCamera != null)
            {
                _normalCamera.RemoveAllCommandBuffers();
                GameObject.DestroyImmediate(_normalCamera.gameObject);
            }
            _normalCamera = null;

            if (mCommandBuf != null)
            {
                CommandBufferPool.Release(mCommandBuf);
                //mCommandBuf.Clear();              
                //mCommandBuf.Dispose();
            }
            mCommandBuf = null;
            _afterRender();

        }

        private void FillCommandBuffer()
        {
            if (mCommandBuf == null ) return;
            if (normalRanders.Count <= 0) return;

            UnityEngine.Profiling.Profiler.BeginSample("HeatDistortion FillCommandBuffer");

            mCommandBuf.Clear();
            List<DistortionEffect> waitRemove = null;
            for(int i = 0; i < normalRanders.Count; ++i)
            {
                if (normalRanders[i].Renderer && normalRanders[i].Material)
                {
                    mCommandBuf.DrawRenderer(normalRanders[i].Renderer, normalRanders[i].Material);
                }
                else
                {
                    if(waitRemove == null)
                    {
                        waitRemove = MyListPool<DistortionEffect>.Get();
                    }
                    waitRemove.Add(normalRanders[i]);                    
                    break;
                }
            }
            if(waitRemove != null)
            {
                for(int i = 0; i< waitRemove.Count; ++i)
                {
                    normalRanders.Remove(waitRemove[i]);
                }
                MyListPool<DistortionEffect>.Release(waitRemove);
                waitRemove = null;
            }

            UnityEngine.Profiling.Profiler.EndSample();
        }

        void Update()
        {
            for (int i = 0; i < normalRanders.Count; ++i)
            {
                if (!normalRanders[i])
                {
                    normalRanders.Remove(normalRanders[i]);
                    return;
                }
            }

            if (normalRanders.Count <= 0 )
            {
                if (mCommandBuf != null || _renderTex || _normalCamera)
                    ClearData();

                return;
            }

            if (mCommandBuf == null || _normalCamera == null)
            {
                UnityEngine.Profiling.Profiler.BeginSample("HeatDistortion Create Init Data");

                CreateData();
                if (!_postEffectMaterial)
                    CreateMat();
                if (_normalCamera)
                    _postEffectMaterial.SetTexture("_DistortionTex", _normalCamera.targetTexture);

                UnityEngine.Profiling.Profiler.EndSample();
            }

            FillCommandBuffer();

        }

        //void OnRenderImage(RenderTexture src, RenderTexture dest)
        //{          
        //    if (normalRanders.Count > 0 && _postEffectMaterial)
        //    {                
        //        _postEffectMaterial.SetFloat("_Strength", strength);
        //        Graphics.Blit(src, dest, _postEffectMaterial, 0);
        //    }

        //    if (Time.realtimeSinceStartup - FlockWareStartTime < flockRareLong)
        //    {
        //        if (!_postEffectMaterial) CreateMat();
        //        float progress = ((Time.realtimeSinceStartup - FlockWareStartTime) / flockRareLong) / 2f;
        //        _postEffectMaterial.SetFloat("_Strength", progress < 0.25f? progress : 0.5f - progress);
        //        Graphics.Blit(src, dest, _postEffectMaterial, 1);
        //    }
        //}

        RenderTexture _renderResult = null;

        public RenderTexture _renderImage(RenderTexture source)
        {
            if (!this.enabled) return null;
            if (HeatDistortion.normalRanders.Count > 0 && PostEffectMaterial)
            {
                if (_renderResult == null) _renderResult = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);

                PostEffectMaterial.SetFloat("_Strength", strength);
                Graphics.Blit(source, _renderResult, PostEffectMaterial, 0);
                return _renderResult;
            }
            else
            {
                return null;
            }

            
        }

        public void _afterRender()
        {
            if(_renderResult != null)
            {
                RenderTexture.ReleaseTemporary(_renderResult);
            }
            _renderResult = null;
        }


        public static void RegisterRender(DistortionEffect e)
        {
            if (!normalRanders.Contains(e))
                normalRanders.Add(e);
        }

        public static void DeregisterRender(DistortionEffect e)
        {
            if (normalRanders.Contains(e))
                normalRanders.Remove(e);
        }
    }

}