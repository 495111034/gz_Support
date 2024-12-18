
using UnityEngine;

namespace MyEffect
{
    /// <summary>
    /// 运动模糊
    /// </summary>
    [RequireComponent(typeof(MyPostEffectsBase))]
    [AddComponentMenu("Camera Effects/Motion(运动)")]
    public partial class Motion : MonoBehaviour
    {
        #region Public properties

        /// The angle of rotary shutter. The larger the angle is, the longer
        /// the exposure time is.
        public float shutterAngle {
            get { return _shutterAngle; }
            set { _shutterAngle = value; }
        }

        [SerializeField, Range(0, 360)]
        [Tooltip("The angle of rotary shutter. Larger values give longer exposure.")]
        float _shutterAngle = 270;

        /// The amount of sample points, which affects quality and performance.
        public int sampleCount {
            get { return _sampleCount; }
            set { _sampleCount = value; }
        }

        [SerializeField]
        [Tooltip("The amount of sample points, which affects quality and performance.")]
        int _sampleCount = 8;

        /// The strength of multiple frame blending. The opacity of preceding
        /// frames are determined from this coefficient and time differences.
        public float frameBlending {
            get { return _frameBlending; }
            set { _frameBlending = value; }
        }

        [SerializeField, Range(0, 1)]
        [Tooltip("The strength of multiple frame blending")]
        float _frameBlending = 0;

        #endregion

        #region Private fields

        [SerializeField] Shader _reconstructionShader;
        [SerializeField] Shader _frameBlendingShader;

        ReconstructionFilter _reconstructionFilter;
        FrameBlendingFilter _frameBlendingFilter;

        #endregion

        #region MonoBehaviour functions

        void OnEnable()
        {
            _reconstructionFilter = new ReconstructionFilter();
            _frameBlendingFilter = new FrameBlendingFilter();

            if (GetComponent<MyPostEffectsBase>())
            {
                GetComponent<MyPostEffectsBase>().OnComponentChange();
            }
        }

        void OnDisable()
        {
            _reconstructionFilter.Release();
            _frameBlendingFilter.Release();

            _reconstructionFilter = null;
            _frameBlendingFilter = null;
        }

        void Update()
        {
            if (Application.isPlaying && QualityUtils.IsLowMem) return;
            // Enable motion vector rendering if reuqired.
            if (_shutterAngle > 0)
                GetComponent<Camera>().depthTextureMode |=
                    DepthTextureMode.Depth | DepthTextureMode.MotionVectors;
        }

        //void OnRenderImage(RenderTexture source, RenderTexture destination)
        //{
        //    if (Application.isPlaying && QualityUtils.IsLowMem) return;

        //    if (_shutterAngle > 0 && _frameBlending > 0)
        //    {
        //        // Reconstruction and frame blending
        //        var temp = RenderTexture.GetTemporary(
        //            source.width, source.height, 0, source.format
        //        );

        //        _reconstructionFilter.ProcessImage(
        //            _shutterAngle, _sampleCount, source, temp
        //        );

        //        _frameBlendingFilter.BlendFrames(
        //            _frameBlending, temp, destination
        //        );
        //        _frameBlendingFilter.PushFrame(temp);

        //        RenderTexture.ReleaseTemporary(temp);
        //    }
        //    else if (_shutterAngle > 0)
        //    {
        //        // Reconstruction only
        //        _reconstructionFilter.ProcessImage(
        //            _shutterAngle, _sampleCount, source, destination
        //        );
        //    }
        //    else if (_frameBlending > 0)
        //    {
        //        // Frame blending only
        //        _frameBlendingFilter.BlendFrames(
        //            _frameBlending, source, destination
        //        );
        //        _frameBlendingFilter.PushFrame(source);
        //    }
        //    else
        //    {
        //        // Nothing to do!
        //        Graphics.Blit(source, destination);
        //    }
        //}

        #endregion

        RenderTexture _renderResult = null;
        public RenderTexture _renderImage(RenderTexture source)
        {
            if (!this.enabled) return null;
            if (Application.isPlaying && QualityUtils.IsLowMem) return null;            

            if (_shutterAngle > 0 && _frameBlending > 0)
            {
                if (_renderResult == null) _renderResult = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);

                // Reconstruction and frame blending
                var temp = RenderTexture.GetTemporary(
                    source.width, source.height, 0, source.format
                );

                _reconstructionFilter.ProcessImage(
                    _shutterAngle, _sampleCount, source, temp
                );

                _frameBlendingFilter.BlendFrames(
                    _frameBlending, temp, _renderResult
                );
                _frameBlendingFilter.PushFrame(temp);

                RenderTexture.ReleaseTemporary(temp);
            }
            else if (_shutterAngle > 0)
            {
                if (_renderResult == null) _renderResult = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
                // Reconstruction only
                _reconstructionFilter.ProcessImage(
                    _shutterAngle, _sampleCount, source, _renderResult
                );
            }
            else if (_frameBlending > 0)
            {
                if (_renderResult == null) _renderResult = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
                // Frame blending only
                _frameBlendingFilter.BlendFrames(
                    _frameBlending, source, _renderResult
                );
                _frameBlendingFilter.PushFrame(source);
            }
            else
            {
                return null;
            }

            return _renderResult;
        }

        public void _afterRender()
        {
            if (_renderResult != null)
            {
                RenderTexture.ReleaseTemporary(_renderResult);
            }
            _renderResult = null;
        }
    }
}
