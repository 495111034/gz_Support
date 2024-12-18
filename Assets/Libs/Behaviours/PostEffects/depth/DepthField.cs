using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace MyEffect
{
    /// <summary>
    /// 景深效果
    /// </summary>
    [ExecuteInEditMode]    
    [RequireComponent(typeof(MyPostEffectsBase))]
    [AddComponentMenu("Camera Effects/Depth Field(景深)")]
    public class DepthField : MonoBehaviour
    {
        #region 可序列化属性

        [HideInInspector]
        [SerializeField]
        GameObject ____pointOfFocus;

        public GameObject pointOfFocus {
            get { return ____pointOfFocus; }
            set { ____pointOfFocus = value; }
        }

        [HideInInspector]
        [SerializeField]
        private Vector3 _pointOffset;
        public Vector3 pointOffset
        {
            get
            {
                return _pointOffset;
            }
            set { _pointOffset = value; }
        }


        [SerializeField]
        float _focusDistance = 10.0f;

        public float focusDistance {
            get { return _focusDistance; }
            set { _focusDistance = value; }
        }

        [SerializeField]
        float _fNumber = 1.4f;

        public float fNumber {
            get { return _fNumber; }
            set { _fNumber = value; }
        }

        [SerializeField]
        bool _useCameraFov = true;

        public bool useCameraFov {
            get { return _useCameraFov; }
            set { _useCameraFov = value; }
        }

        [SerializeField]
        float _focalLength = 0.05f;

        public float focalLength {
            get { return _focalLength; }
            set { _focalLength = value; }
        }

        public enum KernelSize { Small, Medium, Large, VeryLarge }

        [SerializeField]
        public KernelSize _kernelSize = KernelSize.Medium;

        public KernelSize kernelSize {
            get { return _kernelSize; }
            set { _kernelSize = value; }
        }



        #endregion

        #region 公共成员

        public Action<RenderTexture> renderedTexCallBack;//如果有渲染完成时的回调，则表明其它功能正在使用渲染结果，这时不能销毁当前帧的渲染结果
        public bool UseDepthField = true;
       
        #endregion

#if UNITY_EDITOR

        #region 测试属性

        [SerializeField]
        bool _visualize;

        #endregion

        #endif

        #region 内部成员

        // Height of the 35mm full-frame format (36mm x 24mm)
        const float kFilmHeight = 0.024f;

        [SerializeField] Shader _shader;
        Material _material;

        Camera TargetCamera {
            get { return GetComponent<Camera>(); }
        }

        float CalculateFocusDistance()
        {
            if (!____pointOfFocus) return _focusDistance;
            var cam = TargetCamera.transform;
            return Vector3.Dot((____pointOfFocus.transform.position - _pointOffset) - cam.position, cam.forward);
        }

        float CalculateFocalLength()
        {
            if (!_useCameraFov) return _focalLength;
            var fov = TargetCamera.fieldOfView * Mathf.Deg2Rad;
            return 0.5f * kFilmHeight / Mathf.Tan(0.5f * fov);
        }

        float CalculateMaxCoCRadius(int screenHeight)
        {
            // Estimate the allowable maximum radius of CoC from the kernel
            // size (the equation below was empirically derived).
            var radiusInPixels = (float)_kernelSize * 4 + 6;

            // Applying a 5% limit to the CoC radius to keep the size of
            // TileMax/NeighborMax small enough.
            return Mathf.Min(0.05f, radiusInPixels / screenHeight);
        }

        void SetUpShaderParameters(RenderTexture source)
        {
            var s1 = CalculateFocusDistance();
            var f = CalculateFocalLength();
            s1 = Mathf.Max(s1, f);
            _material.SetFloat("_Distance", s1);

            var coeff = f * f / (_fNumber * (s1 - f) * kFilmHeight * 2);
            _material.SetFloat("_LensCoeff", coeff);

            var maxCoC = CalculateMaxCoCRadius(source.height);
            _material.SetFloat("_MaxCoC", maxCoC);
            _material.SetFloat("_RcpMaxCoC", 1 / maxCoC);

            var rcpAspect = (float)source.height / source.width;
            _material.SetFloat("_RcpAspect", rcpAspect);
        }

        #endregion

        #region MonoBehaviour functions

        void OnEnable()
        {
            ClearOldRenderTexture();
            // Check system compatibility.
            var shader = resource.ShaderManager.Find("Hidden/CameraEffects/DepthField");
            if (!shader.isSupported)
            {
                enabled = false;
                return;
            }
            if (!SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBHalf))
            {
                enabled = false;
                return;
            }

            if (!_material)
            {
                _material = new Material(shader);
                _material.hideFlags = HideFlags.HideAndDontSave;
            }

            if (UseDepthField)
            {
                // Requires camera depth texture.
                TargetCamera.depthTextureMode |= DepthTextureMode.Depth;
            }

            if (GetComponent<MyPostEffectsBase>())
            {
                GetComponent<MyPostEffectsBase>().OnComponentChange();
            }
        }

        void OnDestroy()
        {
            // Destroy the temporary objects.
            if (_material != null)
                if (Application.isPlaying)
                    Destroy(_material);
                else
                    DestroyImmediate(_material);
        }

        void Update()
        {
            if (_focusDistance < 0.01f) _focusDistance = 0.01f;
            if (_fNumber < 0.1f) _fNumber = 0.1f;
        }

        RenderTexture dstTex = null;
        static int dstCount = 0;

        public void ClearOldRenderTexture()
        {
            if (dstTex)
            {                
                RenderTexture.ReleaseTemporary(dstTex);                
            }
            dstTex = null;
        }
        //void OnRenderImage(RenderTexture source, RenderTexture destination)
        //{           

        //    // If the material hasn't been initialized because of system
        //    // incompatibility, just blit and return.
        //    if (_material == null)
        //    {
        //        Graphics.Blit(source, destination);
        //        // Try to disable itself if it's Player.
        //        if (Application.isPlaying) enabled = false;
        //        return;
        //    }

        //    var width = source.width;
        //    var height = source.height;
        //    var format = RenderTextureFormat.ARGBHalf;

        //    SetUpShaderParameters(source);

        //    #if UNITY_EDITOR

        //    // Focus range visualization
        //    if (_visualize)
        //    {
        //        Graphics.Blit(source, destination, _material, 7);
        //        return;
        //    }

        //    #endif

        //    // Pass #1 - Downsampling, 过渡 and CoC calculation
        //    var rt1 = RenderTexture.GetTemporary(width / 2, height / 2, 0, format);
        //    source.filterMode = FilterMode.Point;
        //    Graphics.Blit(source, rt1, _material, 0);

        //    // Pass #2 - 背景虚化模拟
        //    var rt2 = RenderTexture.GetTemporary(width / 2, height / 2, 0, format);
        //    rt1.filterMode = FilterMode.Bilinear;
        //    Graphics.Blit(rt1, rt2, _material, 1 + (int)_kernelSize);

        //    // Pass #3 - 模糊计算
        //    rt2.filterMode = FilterMode.Bilinear;
        //    Graphics.Blit(rt2, rt1, _material, 5);

        //    // Pass #4 - 传入图像并合并结果
        //    _material.SetTexture("_BlurTex", rt1);

           
        //    if (renderedTexCallBack != null)
        //    {
        //        if (!dstTex || dstTex == destination)
        //        {
        //            ////  var rtFormat = Application.isConsolePlatform && Graphics.activeTier == UnityEngine.Rendering.GraphicsTier.Tier3 ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
        //            //  dstTex = RenderTexture.GetTemporary(width, height, 24, rtFormat);
        //            //  dstTex.name = $"Depth Return Texture {++dstCount }";

        //            var rtFormat = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.DefaultHDR) && Application.isConsolePlatform && Graphics.activeTier == UnityEngine.Rendering.GraphicsTier.Tier3 ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
        //            if (QualityUtils.IsLowMem)
        //            {
        //                dstTex = RenderTexture.GetTemporary(width / 2, height / 2, 24, rtFormat);
        //            }
        //            else
        //            {
        //                dstTex = RenderTexture.GetTemporary(width, height, 24, rtFormat);
        //            }

        //            dstTex.name = $"Depth Return Texture {++dstCount }";
        //        }
        //    }
        //    else
        //    {
        //        dstTex = destination;
        //    }

        //    Graphics.Blit(source, dstTex, _material, 6);

        //    RenderTexture.ReleaseTemporary(rt1);
        //    RenderTexture.ReleaseTemporary(rt2);

        //    renderedTexCallBack?.Invoke(dstTex);
        //}

        
        public RenderTexture _renderImage(RenderTexture source)
        {
            if (!this.enabled) return null;
            if (_material == null) return null;

            var width = source.width;
            var height = source.height;
            

            if (!dstTex)
            {                

                var rtFormat = SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.DefaultHDR) && Application.isConsolePlatform && Graphics.activeTier == UnityEngine.Rendering.GraphicsTier.Tier3 ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
                if (QualityUtils.IsLowMem)
                {
                    dstTex = RenderTexture.GetTemporary(width / 2, height / 2, 24, rtFormat);
                }
                else
                {
                    dstTex = RenderTexture.GetTemporary(width, height, 24, rtFormat);
                }

                dstTex.name = $"Depth Return Texture {++dstCount }";
            }

            if (renderedTexCallBack != null && !UseDepthField)
            {
                Graphics.Blit(source, dstTex);
                renderedTexCallBack(dstTex);
                return null;
            }

            if (!_material)
            {
                Graphics.Blit(source, dstTex);
                return dstTex;
            }


            SetUpShaderParameters(source);
            var format = RenderTextureFormat.ARGBHalf;
#if UNITY_EDITOR

            // Focus range visualization
            if (_visualize)
            {
                Graphics.Blit(source, dstTex, _material, 7);
                return dstTex;
            }

#endif

            // Pass #1 - Downsampling, 过渡 and CoC calculation
            var rt1 = RenderTexture.GetTemporary(width / 2, height / 2, 0, format);
            source.filterMode = FilterMode.Point;
            Graphics.Blit(source, rt1, _material, 0);

            // Pass #2 - 背景虚化模拟
            var rt2 = RenderTexture.GetTemporary(width / 2, height / 2, 0, format);
            rt1.filterMode = FilterMode.Bilinear;
            Graphics.Blit(rt1, rt2, _material, 1 + (int)_kernelSize);

            // Pass #3 - 模糊计算
            rt2.filterMode = FilterMode.Bilinear;
            Graphics.Blit(rt2, rt1, _material, 5);

            // Pass #4 - 传入图像并合并结果
            _material.SetTexture("_BlurTex", rt1);
            Graphics.Blit(source, dstTex, _material, 6);

            RenderTexture.ReleaseTemporary(rt1);
            RenderTexture.ReleaseTemporary(rt2);

            if (renderedTexCallBack != null)
            {
                renderedTexCallBack(dstTex);
                return null;
            }
            else
            {
                return dstTex;
            }
            
            
        }

        public void _afterRender()
        {
            if(renderedTexCallBack == null)
                ClearOldRenderTexture();
        }

        #endregion
    }
}
