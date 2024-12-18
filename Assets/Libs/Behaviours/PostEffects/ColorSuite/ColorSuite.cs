
using UnityEngine;

namespace MyEffect
{
    [ExecuteInEditMode]
    [ImageEffectTransformsToLDR]   
    [RequireComponent(typeof(MyPostEffectsBase))]
    [AddComponentMenu("Camera Effects/Color Suite (颜色套件)")]
    public class ColorSuite : MonoBehaviour
    {
        #region 公共属性

        // White balance.
        [SerializeField] float _colorTemp = 0.0f;
        [SerializeField] float _colorTint = 0.0f;

        /// <summary>
        /// 色温
        /// </summary>
        public float colorTemp
        {
            get { return _colorTemp; }
            set { _colorTemp = value; }
        }
        /// <summary>
        /// 颜色(绿-紫)
        /// </summary>
        public float colorTint
        {
            get { return _colorTint; }
            set { _colorTint = value; }
        }

        // 色调映射.
        [SerializeField] bool _toneMapping = false;
        [SerializeField] float _exposure = 1.0f;

        /// <summary>
        /// 开启色调映射
        /// </summary>
        public bool toneMapping
        {
            get { return _toneMapping; }
            set { _toneMapping = value; }
        }
        /// <summary>
        /// 曝光度
        /// </summary>
        public float exposure
        {
            get { return _exposure; }
            set { _exposure = value; }
        }

        // 饱和度
        [SerializeField] float _saturation = 1.0f;

        /// <summary>
        /// 饱和度
        /// </summary>
        public float saturation
        {
            get { return _saturation; }
            set { _saturation = value; }
        }

        // Curves.
        [SerializeField] AnimationCurve _rCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] AnimationCurve _gCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] AnimationCurve _bCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] AnimationCurve _cCurve = AnimationCurve.Linear(0, 0, 1, 1);

        public AnimationCurve redCurve
        {
            get { return _rCurve; }
            set { _rCurve = value; UpdateLUT(); }
        }
        public AnimationCurve greenCurve
        {
            get { return _gCurve; }
            set { _gCurve = value; UpdateLUT(); }
        }
        public AnimationCurve blueCurve
        {
            get { return _bCurve; }
            set { _bCurve = value; UpdateLUT(); }
        }
        public AnimationCurve rgbCurve
        {
            get { return _cCurve; }
            set { _cCurve = value; UpdateLUT(); }
        }

        public void SetRGBCurvas(AnimationCurve r, AnimationCurve g, AnimationCurve b, AnimationCurve c)
        {

            if (r != null && r.length > 0) _rCurve = r;
            if (g != null && g.length > 0) _gCurve = g;
            if (b != null && b.length > 0) _bCurve = b;
            if (c != null && c.length > 0) _cCurve = c;
            UpdateLUT();
        }

        // Dithering.
        public enum DitherMode { Off, Ordered, Triangular }
        [SerializeField] DitherMode _ditherMode = DitherMode.Off;

        public DitherMode ditherMode
        {
            get { return _ditherMode; }
            set { _ditherMode = value; }
        }

        public void CloneFromOther(ColorSuite other)
        {
            _colorTemp = other._colorTemp;
            _colorTint = other._colorTint;
            _toneMapping = other._toneMapping;
            _exposure = other._exposure;
            _saturation = other._saturation;
            _rCurve = other._rCurve;
            _gCurve = other._gCurve;
            _bCurve = other._bCurve;
            rgbCurve = other.rgbCurve;
            _ditherMode = other._ditherMode;
        }

        #endregion

        #region 全局属性

        // Reference to the shader.
        //[SerializeField]
        Shader _shader;

        Shader shader { get { if (!_shader) { _shader = resource.ShaderManager.Find("Hidden/CameraEffects/ColorSuite"); } return _shader; } }

        // Temporary objects.
        Material _material;
        Texture2D _lutTexture;

        #endregion

        #region 静态算法

        // RGBM encoding.
        static Color EncodeRGBM(float r, float g, float b)
        {
            var a = Mathf.Max(Mathf.Max(r, g), Mathf.Max(b, 1e-6f));
            a = Mathf.Ceil(a * 255) / 255;
            return new Color(r / a, g / a, b / a, a);
        }

        // An analytical model of chromaticity of the standard illuminant, by Judd et al.
        // http://en.wikipedia.org/wiki/Standard_illuminant#Illuminant_series_D
        // Slightly modifed to adjust it with the D65 white point (x=0.31271, y=0.32902).
        static float StandardIlluminantY(float x)
        {
            return 2.87f * x - 3.0f * x * x - 0.27509507f;
        }

        // CIE xy chromaticity to CAT02 LMS.
        // http://en.wikipedia.org/wiki/LMS_color_space#CAT02
        static Vector3 CIExyToLMS(float x, float y)
        {
            var Y = 1.0f;
            var X = Y * x / y;
            var Z = Y * (1.0f - x - y) / y;

            var L = 0.7328f * X + 0.4296f * Y - 0.1624f * Z;
            var M = -0.7036f * X + 1.6975f * Y + 0.0061f * Z;
            var S = 0.0030f * X + 0.0136f * Y + 0.9834f * Z;

            return new Vector3(L, M, S);
        }

        #endregion

        #region 内部方法

        // 初始化计算资源
        void Setup()
        {
            if (!_material)
            {
                _material = new Material(shader);
                _material.hideFlags = HideFlags.DontSave;
            }

            if (_lutTexture == null)
            {
                _lutTexture = new Texture2D(512, 1, TextureFormat.ARGB32, false, true);
                _lutTexture.hideFlags = HideFlags.DontSave;
                _lutTexture.wrapMode = TextureWrapMode.Clamp;
                UpdateLUT();
            }
        }

        // Update the LUT texture.
        void UpdateLUT()
        {
            if (!_lutTexture) return;
            for (var x = 0; x < _lutTexture.width; x++)
            {
                var u = 1.0f / (_lutTexture.width - 1) * x;
                var r = _cCurve.Evaluate(_rCurve.Evaluate(u));
                var g = _cCurve.Evaluate(_gCurve.Evaluate(u));
                var b = _cCurve.Evaluate(_bCurve.Evaluate(u));
                _lutTexture.SetPixel(x, 0, EncodeRGBM(r, g, b));
            }
            _lutTexture.Apply();
        }

        // 计算颜色平衡系数
        Vector3 CalculateColorBalance()
        {
            // Get the CIE xy chromaticity of the reference white point.
            // Note: 0.31271 = x value on the D65 white point
            var x = 0.31271f - _colorTemp * (_colorTemp < 0.0f ? 0.1f : 0.05f);
            var y = StandardIlluminantY(x) + _colorTint * 0.05f;

            // Calculate the coefficients in the LMS space.
            var w1 = new Vector3(0.949237f, 1.03542f, 1.08728f); // D65 white point
            var w2 = CIExyToLMS(x, y);
            return new Vector3(w1.x / w2.x, w1.y / w2.y, w1.z / w2.z);
        }

        #endregion

        #region Monobehaviour方法

        private void OnEnable()
        {
            if(GetComponent<MyPostEffectsBase>())
            {
                GetComponent<MyPostEffectsBase>().OnComponentChange();
            }
        }

        void Start()
        {
            Setup();
        }

        void OnValidate()
        {
            Setup();
            UpdateLUT();
        }

        void Reset()
        {
            Setup();
            UpdateLUT();
        }

        private void OnDestroy()
        {
            _afterRender();
        }

        private void OnDisable()
        {
            _afterRender();
        }

        //void OnRenderImage(RenderTexture source, RenderTexture destination)
        //{    

        //    var linear = QualitySettings.activeColorSpace == ColorSpace.Linear;

        //    Setup();

        //    //线性空间
        //    if (linear)
        //    {
        //        _material.EnableKeyword("COLORSPACE_LINEAR");
        //    }
        //    else
        //    {
        //        _material.DisableKeyword("COLORSPACE_LINEAR");
        //    }

        //    //开启色温调节
        //    if (_colorTemp != 0.0f || _colorTint != 0.0f)
        //    {
        //        _material.EnableKeyword("BALANCING_ON");
        //        _material.DisableKeyword("BALANCING_OFF");
        //        _material.SetVector("_Balance", CalculateColorBalance());
        //    }
        //    else
        //    {
        //        _material.DisableKeyword("BALANCING_ON");
        //        _material.EnableKeyword("BALANCING_OFF");
        //    }

        //    //开启色调映射
        //    if (_toneMapping && linear)
        //    {
        //        _material.EnableKeyword("TONEMAPPING_ON");
        //        _material.SetFloat("_Exposure", _exposure);
        //    }
        //    else
        //    {
        //        _material.DisableKeyword("TONEMAPPING_ON");
        //    }

        //    _material.SetTexture("_Curves", _lutTexture);
        //    _material.SetFloat("_Saturation", _saturation);

        //    //抖动方式
        //    if (_ditherMode == DitherMode.Ordered)
        //    {
        //        _material.EnableKeyword("DITHER_ORDERED");
        //        _material.DisableKeyword("DITHER_TRIANGULAR");
        //    }
        //    else if (_ditherMode == DitherMode.Triangular)
        //    {
        //        _material.DisableKeyword("DITHER_ORDERED");
        //        _material.EnableKeyword("DITHER_TRIANGULAR");
        //    }
        //    else
        //    {
        //        _material.DisableKeyword("DITHER_ORDERED");
        //        _material.DisableKeyword("DITHER_TRIANGULAR");
        //    }

        //    Graphics.Blit(source, destination, _material);
        //}



        #endregion

        RenderTexture _renderResult = null;
        public RenderTexture _renderImage(RenderTexture source)
        {
            if (!this.enabled) return null;
            var linear = QualitySettings.activeColorSpace == ColorSpace.Linear;

            Setup();

            //线性空间
            if (linear)
            {
                _material.EnableKeyword("COLORSPACE_LINEAR");
            }
            else
            {
                _material.DisableKeyword("COLORSPACE_LINEAR");
            }

            //开启色温调节
            if (_colorTemp != 0.0f || _colorTint != 0.0f)
            {
                _material.EnableKeyword("BALANCING_ON");
                _material.DisableKeyword("BALANCING_OFF");
                _material.SetVector("_Balance", CalculateColorBalance());
            }
            else
            {
                _material.DisableKeyword("BALANCING_ON");
                _material.EnableKeyword("BALANCING_OFF");
            }

            //开启色调映射
            if (_toneMapping && linear)
            {
                _material.EnableKeyword("TONEMAPPING_ON");
                _material.SetFloat("_Exposure", _exposure);
            }
            else
            {
                _material.DisableKeyword("TONEMAPPING_ON");
            }

            _material.SetTexture("_Curves", _lutTexture);
            _material.SetFloat("_Saturation", _saturation);

            //抖动方式
            if (_ditherMode == DitherMode.Ordered)
            {
                _material.EnableKeyword("DITHER_ORDERED");
                _material.DisableKeyword("DITHER_TRIANGULAR");
            }
            else if (_ditherMode == DitherMode.Triangular)
            {
                _material.DisableKeyword("DITHER_ORDERED");
                _material.EnableKeyword("DITHER_TRIANGULAR");
            }
            else
            {
                _material.DisableKeyword("DITHER_ORDERED");
                _material.DisableKeyword("DITHER_TRIANGULAR");
            }

            if (_renderResult == null) _renderResult = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);

            Graphics.Blit(source, _renderResult, _material);

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
