
using UnityEngine;

namespace MyEffect
{
    /// <summary>
    ///全屏泛光效果（高光溢出）
    ///建议开启高动态范围(HDR)和线性空间，pc端默认开启，移动端需要在GraphicSetting中开启
    /// </summary>
    [ExecuteInEditMode]    
    [RequireComponent(typeof(MyPostEffectsBase))]
    [AddComponentMenu("Camera Effects/Bloom(溢出)")]
    public class Bloom : MonoBehaviour
    {
        #region Public Properties

        /// 阀值 (gamma)
        /// 过滤低于此值的区域
        public float thresholdGamma {
            get { return Mathf.Max(_threshold, 0); }
            set { _threshold = value; }
        }

        /// 阀值 (线性空间)       
        public float thresholdLinear {
            get { return GammaToLinear(thresholdGamma); }
            set { _threshold = LinearToGamma(value); }
        }

        [SerializeField]
        [Tooltip("亮度低于此值的将被过滤掉.")]
        float _threshold = 0.8f;

        /// 过渡区域       
        public float softKnee {
            get { return _softKnee; }
            set { _softKnee = value; }
        }

        [SerializeField, Range(0, 1)]
        [Tooltip("阀值上下值的缓动过度.")]
        float _softKnee = 0.5f;

        /// Bloom radius
        public float radius {
            get { return _radius; }
            set { _radius = value; }
        }

        [SerializeField, Range(1, 7)]
        [Tooltip("Changes extent of veiling effects\n" +
                 "in a screen resolution-independent fashion.")]
        float _radius = 2.5f;

        /// Bloom intensity
        /// Blend factor of the result image.
        public float intensity {
            get { return Mathf.Max(_intensity, 0); }
            set { _intensity = value; }
        }

        [SerializeField]
        [Tooltip("计算结果的混合因子.")]
        float _intensity = 0.8f;

        /// <summary>
        /// 如果选中此项，将按原尺寸进行溢出计算，否则将按1/2尺寸
        /// </summary>
        public bool highQuality {
            get { return _highQuality; }
            set { _highQuality = value; }
        }

        [SerializeField]
        [Tooltip("缓冲区的图片分辨率（用于模糊计算的图片）")]
        bool _highQuality = true;

        /// Anti-flicker filter
        /// Reduces flashing noise with an additional filter.
        [SerializeField]
        [Tooltip("抗抖动.")]
        bool _antiFlicker = true;

        public bool antiFlicker {
            get { return _antiFlicker; }
            set { _antiFlicker = value; }
        }

        bool _isInUI = false;
        public bool IsInUI
        {
            get { return _isInUI; }
            set { _isInUI = value; }
        }

        bool _IsTestActive = false;
        public bool IsTestActive
        {
            get { return _IsTestActive; }
            set { _IsTestActive = value; }
        }
        #endregion

        #region Private Members

        [SerializeField, HideInInspector]
        Shader _shader;

        Material _material;

        const int kMaxIterations = 16;
        RenderTexture[] _blurBuffer1 = new RenderTexture[kMaxIterations];
        RenderTexture[] _blurBuffer2 = new RenderTexture[kMaxIterations];

        float LinearToGamma(float x)
        {
        #if UNITY_5_3_OR_NEWER
            return Mathf.LinearToGammaSpace(x);
        #else
            if (x <= 0.0031308f)
                return 12.92f * x;
            else
                return 1.055f * Mathf.Pow(x, 1 / 2.4f) - 0.055f;
        #endif
        }

        float GammaToLinear(float x)
        {
            float f = 1f;
            
            if (Application.isMobilePlatform)
            {
#if UNITY_ANDROID
                if(IsInUI)
                {
                    f = 2f;
                }
                else
                {
                    f = 2f;
                }
#elif UNITY_IPHONE
                if(IsInUI)
                {
                    f = 2f;
                }
                else
                {
                    f = 2f;
                }
#endif
            }
            else
            {
                if (IsInUI)
                {
                    f = 1f;
                }
                else
                {
                    f = 1f;
                }
            }
           
            return Mathf.GammaToLinearSpace(x * f) ;

        }

       // HeatDistortion _headDistortion;

        #endregion

        #region MonoBehaviour Functions

        void OnEnable()
        {
            var shader = _shader ? _shader : resource.ShaderManager.Find("Hidden/CameraEffects/Bloom");
            _material = new Material(shader);
            _material.hideFlags = HideFlags.DontSave;

            if (GetComponent<MyPostEffectsBase>())
            {
                GetComponent<MyPostEffectsBase>().OnComponentChange();
            }
        }

        void OnDisable()
        {
            DestroyImmediate(_material);
            _afterRender();
        }

        private void OnDestroy()
        {
            DestroyImmediate(_material);
            _afterRender();
        }

        #endregion


        RenderTexture _renderResult = null;
        public RenderTexture _renderImage(RenderTexture source)
        {
            if (!this.enabled) return null;

            RenderTexture newSource = null;
            RenderTexture _lastSource = null; 

            if (_lastSource == null) _lastSource = source;

            var useRGBM = !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.DefaultHDR) || Application.isMobilePlatform || Graphics.activeTier < UnityEngine.Rendering.GraphicsTier.Tier3;
            

            // source texture size
            var tw = _lastSource.width;
            var th = _lastSource.height;

            //if (QualityUtils.IsLowMem)
            //{
                tw /= 4;
                th /= 4;
            //}
            //else
            //{
            //    // 如果不选高品质，则按1/2分辨率
            //    if (!_highQuality)
            //    {
            //        if (Graphics.activeTier < UnityEngine.Rendering.GraphicsTier.Tier3)
            //        {
            //            tw /= 4;
            //            th /= 4;
            //        }
            //        else
            //        {
            //            tw /= 2;
            //            th /= 2;
            //        }
            //    }
            //}

            // blur buffer format
            var rtFormat = useRGBM ?
               RenderTextureFormat.Default : RenderTextureFormat.ARGB32;

            // determine the iteration count
            var logh = Mathf.Log(th, 2) + _radius - 8;
            var logh_i = (int)logh;
            var iterations = Mathf.Clamp(logh_i, 1, kMaxIterations);


            // update the shader properties
            var lthresh = thresholdLinear;
            _material.SetFloat("_Threshold", lthresh);

            var knee = lthresh * _softKnee + 1e-5f;
            var curve = new Vector3(lthresh - knee, knee * 2, 0.25f / knee);
            _material.SetVector("_Curve", curve);

            var pfo = !_highQuality && _antiFlicker;
            _material.SetFloat("_PrefilterOffs", pfo ? -0.5f : 0.0f);

            _material.SetFloat("_SampleScale", 0.5f + logh - logh_i);
            _material.SetFloat("_Intensity", intensity);

            // prefilter pass
            var prefiltered = RenderTexture.GetTemporary(tw, th, 0, rtFormat);
            var pass = _antiFlicker ? 1 : 0;
            Graphics.Blit(_lastSource, prefiltered, _material, pass);

            // construct a mip pyramid
            var last = prefiltered;
            for (var level = 0; level < iterations; level++)
            {
                _blurBuffer1[level] = RenderTexture.GetTemporary(
                    last.width / 2, last.height / 2, 0, rtFormat
                );

                pass = (level == 0) ? (_antiFlicker ? 3 : 2) : 4;
                Graphics.Blit(last, _blurBuffer1[level], _material, pass);

                last = _blurBuffer1[level];
            }

            // upsample and combine loop
            for (var level = iterations - 2; level >= 0; level--)
            {
                var basetex = _blurBuffer1[level];
                _material.SetTexture("_BaseTex", basetex);

                _blurBuffer2[level] = RenderTexture.GetTemporary(
                    basetex.width, basetex.height, 0, rtFormat
                );

                pass = _highQuality ? 6 : 5;
                Graphics.Blit(last, _blurBuffer2[level], _material, pass);
                last = _blurBuffer2[level];
            }

            if (_renderResult == null) _renderResult = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);

            // finish process
            _material.SetTexture("_BaseTex", _lastSource);
            pass = _highQuality ? 8 : 7;
            Graphics.Blit(last, _renderResult, _material, pass);

            // release the temporary buffers
            for (var i = 0; i < kMaxIterations; i++)
            {
                if (_blurBuffer1[i] != null)
                    RenderTexture.ReleaseTemporary(_blurBuffer1[i]);

                if (_blurBuffer2[i] != null)
                    RenderTexture.ReleaseTemporary(_blurBuffer2[i]);

                _blurBuffer1[i] = null;
                _blurBuffer2[i] = null;
            }

            RenderTexture.ReleaseTemporary(prefiltered);
            if (newSource != null)
                RenderTexture.ReleaseTemporary(newSource);

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
#if UNITY_EDITOR
        private void OnGUI()
        {
             if(IsTestActive)
                 TestButton();
        }
#endif






        #region 测试功能

        Rect sliderRect1, sliderRect2, sliderRect3,btnRect;
        GUIStyle style1, style2,styleBtn;
        GUIStyle styleFont;
        Rect lbl1, lbl2, lbl3, lbl4, lbl5, lbl6;

        bool isDebug = true;
        public void TestButton()
        {          
            if(style1 == null)
            {
                style1 = new GUIStyle();
                style1.fontSize = GUIScale.Scale(28);
                style1.normal.textColor = Color.red;

                style2 = new GUIStyle(style1);
                style2.normal.textColor = Color.blue;

                sliderRect1 = new Rect(Screen.width / 2 - GUIScale.Scale(150), Screen.height / 2 - GUIScale.Scale(100), GUIScale.Scale(300), GUIScale.Scale(50));
                sliderRect2 = new Rect(Screen.width / 2 - GUIScale.Scale(150), Screen.height / 2 - GUIScale.Scale(40), GUIScale.Scale(300), GUIScale.Scale(50));
                sliderRect3 = new Rect(Screen.width / 2 - GUIScale.Scale(150), Screen.height / 2 + GUIScale.Scale(20), GUIScale.Scale(300), GUIScale.Scale(50));

                styleBtn = new GUIStyle();
                styleBtn.normal.textColor = Color.red;
                styleBtn.fontSize = 36;
                styleBtn.normal.background = Resources.Load<Texture2D>("small/black");



                styleFont = new GUIStyle();
                styleFont.normal.textColor = Color.black;
                styleFont.normal.background = Resources.Load<Texture2D>("small/blankimage");
                styleFont.fontSize = 28;

                lbl1 = lbl2 = sliderRect1;
                lbl1.y -= 25;
                lbl2.x += 300;
                lbl3 = lbl4 = sliderRect2;
                lbl3.y -= 25;
                lbl4.x += 300;
                lbl5 = lbl6 = sliderRect3;
                lbl5.y -= 25;
                lbl6.x += 300;

                btnRect = new Rect(Screen.width - GUIScale.Scale(100), GUIScale.Scale(25), GUIScale.Scale(100), GUIScale.Scale(50));
            }

            if(GUI.Button(btnRect,new GUIContent("Bloom参数"), styleBtn))
            {
                isDebug = !isDebug;
                //App.ShowMemoryInfo = !App.ShowMemoryInfo;
            }
          
            if (isDebug)
            {
                GUI.Label(lbl1, new GUIContent("阀值："), styleFont);
                GUI.Label(lbl2, new GUIContent(_threshold.ToString()), styleFont);
                _threshold = GUI.HorizontalSlider(sliderRect1, _threshold, 0.5f, 5f);
                GUI.Label(lbl3, new GUIContent("范围："), styleFont);
                GUI.Label(lbl4, new GUIContent(_radius.ToString()), styleFont);
                _radius = GUI.HorizontalSlider(sliderRect2, _radius, 1f, 7f);
                GUI.Label(lbl5, new GUIContent("混合因子"), styleFont);
                GUI.Label(lbl6, new GUIContent(_intensity.ToString()), styleFont);
                _intensity = GUI.HorizontalSlider(sliderRect3, _intensity, 1f, 7f);
                // _threshold = GUI.Slider(sliderRect1, 1, _threshold, 0.5f, 5, style1, style2, true,0);
            }
        }
#endregion


    }
}
