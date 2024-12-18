using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;


namespace UnityEngine.UI
{
    public class MyList<T> : List<T>
    {
        public MyList() : base()
        {
            Log.LogInfo($"new MyList");
        }
    }
    [Serializable]
    public abstract class MySpriteImageBase : MaskableGraphic, IMySprite, IMyTexture, IBlackRangeClickHandler
    {

        public static void ClearResource(MaskableGraphic graphic, Texture texture)
        {
            if (texture is RenderTexture && texture.name != "Screenshot Camera")
            {
                //清除其它控件对_renderTexture的引用，以免被重复ReleaseTemporary
                var _3d = graphic.GetComponent<My3DRoomImage>();
                if (_3d)
                {
                    _3d.ClearRenderTexture(texture as RenderTexture);
                }
                RenderTexture.ReleaseTemporary(texture as RenderTexture);
            }
        }


        [HideInInspector] [SerializeField]
        protected UnityEngine.Texture _tex;

        [HideInInspector] [SerializeField]
        protected UnityEngine.Sprite _sprite;

        [HideInInspector] [SerializeField]
        protected MySpritePacker _sp_packer;

        [HideInInspector] [SerializeField]
        protected string _sp_name;

        [HideInInspector] [SerializeField]
        protected bool _noTexShow = false;

        [HideInInspector] [SerializeField]
        protected bool _useLineShader = false;

        [HideInInspector] [SerializeField]
        protected bool _shouldPreserveAspect = false;

        [HideInInspector] [SerializeField] protected bool m_x_reversal;       //x轴镜像翻转
        [HideInInspector] [SerializeField] protected bool m_y_reversal;       //y轴镜像翻转

        [HideInInspector] [SerializeField]
        [Range(0.1f, 100f)]
        protected float _scale = 1f;

        [HideInInspector] [SerializeField]
        protected bool _bgNotFillCenter = false;

        protected System.Object _dept;
        protected Texture _tmpTex = null;
        protected MyList<string> _animSpriteFrames = null;
        protected int _animSpriteIndex = -1;


        [HideInInspector]
        [SerializeField]
        string _packerName, _spriterName, _textureName;//用于多语言

        public string PackerName => _packerName;
        public string SpriteName => _spriterName;
        public string TextureName => _textureName;
        public string PackerSpriteName => _sp_name;

        public Texture iTexture => _tex;
        public Sprite iSprite => (!string.IsNullOrEmpty(_spriterName)) ? _sprite : ((_sp_packer && !string.IsNullOrEmpty(_sp_name)) ? _sp_packer.GetSprite(_sp_name, null) : null);

#if UNITY_EDITOR
        //static List<object> losts = new List<object>();
        public static bool Editor_CheckIsMissing<T>(MaskableGraphic _this, ref T obj) where T : Object
        {
            if (ReferenceEquals(obj, null))
            {
                //Log.LogInfo($"fixnameX {_this.gameObject.GetLocation()}/{_this.GetType().Name}.{typeof(T).Name} isNull");
            }
            else
            {
                if (!obj)
                {
                    var b = UnityEditor.AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out var guid, out long localid);
                    if (b || localid > 0)
                    {
                        Log.LogInfo($"fixnameX {_this.gameObject.GetLocation()}/{_this.GetType().Name}.{obj.GetType().Name} isMissing, {guid}, {localid}");
                        //return true;
                        obj = null;
                        UnityEditor.EditorUtility.SetDirty(_this.transform.root.gameObject);
                    }
                    else 
                    {                        
                        //Log.LogInfo($"fixnameX {_this.gameObject.GetLocation()}/{_this.GetType().Name}.{obj.GetType().Name} isLost, {guid}, {localid}");
                    }
                }
                else
                {
                    //Log.LogInfo($"fixnameX {_this.gameObject.GetLocation()}/{_this.GetType().Name}.{obj.GetType().Name} isExist");
                }
            }
            return false;
        }

        public void Editor_FixPackerNameSpriteName()
        {
            if (!MySpriteImageBase.Editor_CheckIsMissing(this, ref _sp_packer))
            {
                var _sp_packer_name = _sp_packer ? _sp_packer.name : "";
                if (_packerName != _sp_packer_name)
                {
                    Log.LogInfo($"fixname8 {gameObject.GetLocation()} pack:{_packerName} -> {_sp_packer_name}");
                    _packerName = _sp_packer_name;
                    UnityEditor.EditorUtility.SetDirty(transform.root.gameObject);
                }

                if (!_sp_packer && _sp_name != "")
                {
                    Log.LogInfo($"fixname9 {gameObject.GetLocation()} pack_sp:{_sp_name} -> null");
                    _sp_name = "";
                    UnityEditor.EditorUtility.SetDirty(transform.root.gameObject);
                }
            }

            if (!MySpriteImageBase.Editor_CheckIsMissing(this, ref _sprite))
            {
                var _name = _sprite ? _sprite.name : "";
                if (_spriterName != _name)
                {
                    Log.LogInfo($"fixname10 {gameObject.GetLocation()} sp:{_spriterName} -> {_name}");
                    _spriterName = _name;
                    UnityEditor.EditorUtility.SetDirty(transform.root.gameObject);
                }
            }
        }
        public void Editor_FixTextureName() 
        {
            if (!MySpriteImageBase.Editor_CheckIsMissing(this, ref _tex))
            {
                var _name = _tex ? _tex.name : "";
                if (_textureName != _name)
                {
                    Log.LogInfo($"fixname11 {gameObject.GetLocation()} tex:{_textureName} -> {_name}");
                    _textureName = _name;
                    UnityEditor.EditorUtility.SetDirty(transform.root.gameObject);
                }
            }
        }

        public void Editor_SetPacker_sprite(MySpritePacker sp_packer, string sp_name)
        {
            _sp_packer = sp_packer;
            _sp_name = sp_name;
            _sprite = null;
            _tex = null;
        }

#endif



        protected abstract void AddQuad(VertexHelper vertexHelper, MySpritePacker packer, Vector2 posMin, Vector2 posMax, Vector2 uvMin, Vector2 uvMax);
        protected abstract void OnInit();

        public bool BgNotFillCenter { get { return _bgNotFillCenter; } set { _bgNotFillCenter = value; OnInit(); } }
        public float scale { get { return _scale; } set { _scale = value; OnInit(); } }
        public bool XReversal { get { return m_x_reversal; } set { m_x_reversal = value; OnInit(); } }
        public bool YReversal { get { return m_y_reversal; } set { m_y_reversal = value; OnInit(); } }
        public bool ShouldPreserveAspect { get { return _shouldPreserveAspect; } set { _shouldPreserveAspect = value; OnInit(); } }
        public bool NoTexShow { get { return _noTexShow; } set { _noTexShow = value; OnInit(); } }

        public bool UseLineShader { get { return _useLineShader; } set { _useLineShader = value; OnInit(); } }

        public override Texture mainTexture
        {
            get
            {
                if (_tex != null)
                {
                    return _tex;
                }

                if (_sprite != null)
                {
                    return _sprite.texture;
                }

                if (_sp_packer)
                {
                    return _sp_packer.PackerImage;
                }

                if (_noTexShow)
                {
                    if (!_tmpTex)
                    {
                        _tmpTex = Resources.Load("small/blankimage") as Texture;
                    }
                    return _tmpTex;
                }
                return null;
            }
        }

        private Vector4 GetDrawingDimensions(Vector4 padding, Vector2 imgSize)
        {
            Rect pixelAdjustedRect = GetPixelAdjustedRect();
            int size_x = Mathf.RoundToInt(imgSize.x);
            int size_y = Mathf.RoundToInt(imgSize.y);
            Vector4 result = new Vector4(padding.x / (float)size_x, padding.y / (float)size_y, ((float)size_x - padding.z) / (float)size_x, ((float)size_y - padding.w) / (float)size_y);
            if (_shouldPreserveAspect && imgSize.sqrMagnitude > 0f)
            {
                float num3 = imgSize.x / imgSize.y;
                float num4 = pixelAdjustedRect.width / pixelAdjustedRect.height;
                if (num3 > num4)
                {
                    float height = pixelAdjustedRect.height;
                    pixelAdjustedRect.height = (pixelAdjustedRect.width * (1f / num3));
                    pixelAdjustedRect.y = (pixelAdjustedRect.y + (height - pixelAdjustedRect.height) * rectTransform.pivot.y);
                }
                else
                {
                    float width = pixelAdjustedRect.width;
                    pixelAdjustedRect.width = (pixelAdjustedRect.height * num3);
                    pixelAdjustedRect.x = (pixelAdjustedRect.x + (width - pixelAdjustedRect.width) * base.rectTransform.pivot.x);
                }
            }
            result = new Vector4(pixelAdjustedRect.x + pixelAdjustedRect.width * result.x, pixelAdjustedRect.y + pixelAdjustedRect.height * result.y, pixelAdjustedRect.x + pixelAdjustedRect.width * result.z, pixelAdjustedRect.y + pixelAdjustedRect.height * result.w);
            return result;
        }

        private Vector4 ReversalUV(Vector4 uv)
        {
            Vector4 temp;
            if (m_x_reversal && m_y_reversal)
                temp = new Vector4(uv.z, uv.w, uv.x, uv.y);
            else if (m_y_reversal)
                temp = new Vector4(uv.x, uv.w, uv.z, uv.y);
            else if (m_x_reversal)
                temp = new Vector4(uv.z, uv.y, uv.x, uv.w);
            else
                return uv;
            return temp;
        }

        //int OnPopulateMesh_cnt = 0;
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            //Log.LogInfo($"MySpriteImage.OnPopulateMesh {gameObject.GetLocation()} {OnPopulateMesh_cnt++}\n{new StackTrace(true)}");

            //base.OnPopulateMesh(vh);
            vh.Clear();
            var rc = GetPixelAdjustedRect();

            Vector4 pos;

            if (_tex != null)
            {
                var border = Vector4.zero;
                var uv = ReversalUV(new Vector4(0, 0, 1, 1));
                var size = new Vector2(_tex.width, _tex.height);

                pos = GetDrawingDimensions(Vector4.zero, size);
                if (_scale != 1f)
                {
                    pos = pos * _scale;
                }
                AddSprite(vh, null, pos, uv, uv, border, true);
            }
            else if (_sprite || _sp_packer)
            {
                var _cursp = _sprite;
                if (!_cursp) 
                {
                    var sp_name = _sp_name;
                    if (_animSpriteIndex >= 0)
                    {
                        sp_name = _animSpriteFrames[_animSpriteIndex];
                    }
                    if (!string.IsNullOrEmpty(sp_name))
                    {
                        _cursp = _sp_packer.GetSprite(sp_name, this.gameObject);
#if UNITY_EDITOR
                        if (!_cursp)
                        {
                            Log.LogWarning($"{gameObject.GetLocation()}.{this.GetType().Name}:{_sp_packer.name}//{sp_name} sprite not found");
                        }
#endif
                    }
                }
                if (_cursp)
                {
                    pos = GetDrawingDimensions(Sprites.DataUtility.GetPadding(_cursp), _cursp.rect.size);
                    if (_scale != 1f)
                    {
                        pos = pos * _scale;
                    }
                    var uv = ReversalUV(Sprites.DataUtility.GetOuterUV(_cursp));
                    AddSprite(vh, null, pos, uv, (_cursp.border.sqrMagnitude > 0f) ? Sprites.DataUtility.GetInnerUV(_cursp) : Vector4.zero, _cursp.border, !_bgNotFillCenter);
                }
            }
            else if (_noTexShow)
            {
                pos = new Vector4(rc.xMin, rc.yMin, rc.xMax, rc.yMax);
                if (_scale != 1f)
                {
                    pos.x = rc.xMin * _scale;
                    pos.y = rc.yMin * _scale;
                    pos.z = rc.xMax * _scale;
                    pos.w = rc.yMax * _scale;
                }
                var border = Vector4.zero;
                var uv = ReversalUV(new Vector4(0, 0, 1, 1));
                AddSprite(vh, null, pos, uv, uv, border, true);
            }
        }

        private static readonly Vector2[] VertScratch = new Vector2[4];
        private static readonly Vector2[] UVScratch = new Vector2[4];

        void AddSprite(VertexHelper vh, MySpritePacker packer, Vector4 pos, Vector4 outerUV, Vector4 innerUV, Vector4 border, bool fillCenter = true)
        {
            var width = (pos.z - pos.x);
            var height = pos.w - pos.y;

            if (border.z + border.x >= width)
            {
                border.z = 0;
                border.x = 0;
            }
            if (border.w + border.y >= height)
            {
                border.w = 0;
                border.y = 0;
            }

            if (border.sqrMagnitude > 0f)
            {
                //九宫格                

                //Rect pixelAdjustedRect = base.GetPixelAdjustedRect();
                //border.xyzw = left,bottom,right,top
                //------------------------------------------------ 3
                //|                    w                          |
                //|      ---------------------------------- 2     |
                //|     |                                  |      |
                //| -x- |                                  |  -z- |
                //|     | 1                                |      |
                //|      ----------------------------------       |
                //| 0                  y                          |
                //------------------------------------------------
                //九宫格添加顺序如下
                //3 6 9
                //2 5 8
                //1 4 7
                VertScratch[0] = new Vector2(0, 0);
                //
                VertScratch[1].x = border.x;
                VertScratch[1].y = border.y;
                //
                VertScratch[2].x = width - border.z;
                VertScratch[2].y = height - border.w;
                //
                VertScratch[3] = new Vector2(width, height);
                for (int i = 0; i < 4; i++)
                {
                    VertScratch[i].x = VertScratch[i].x + pos.x;
                    VertScratch[i].y = VertScratch[i].y + pos.y;
                }
                UVScratch[0] = new Vector2(outerUV.x, outerUV.y);
                UVScratch[1] = new Vector2(innerUV.x, innerUV.y);
                UVScratch[2] = new Vector2(innerUV.z, innerUV.w);
                UVScratch[3] = new Vector2(outerUV.z, outerUV.w);

                //Log.LogInfo($"AddQuads");
                for (int j = 0; j < 3; j++)
                {
                    int num = j + 1;
                    for (int k = 0; k < 3; k++)
                    {
                        if (fillCenter || j != 1 || k != 1)
                        {
                            int num2 = k + 1;
                            //Log.LogInfo($"[{j},{k}] -> [{num},{num2}]");
                            AddQuad(vh, packer, new Vector2(VertScratch[j].x, VertScratch[k].y), new Vector2(VertScratch[num].x, VertScratch[num2].y), new Vector2(UVScratch[j].x, UVScratch[k].y), new Vector2(UVScratch[num].x, UVScratch[num2].y));
                        }
                    }
                }
            }
            else
            {
                //非九宫格
                AddQuad(vh, packer, new Vector2(pos.x, pos.y), new Vector2(pos.z, pos.w), new Vector2(outerUV.x, outerUV.y), new Vector2(outerUV.z, outerUV.w));
            }
        }

        protected override void OnDestroy()
        {
            MySpriteImageBase.ClearResource(this, _tex);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            if (!Application.isPlaying)
            {
                //Log.LogInfo($"{this.GetType()}:{_packerName}/{_sp_name},{_spriterName}, at {gameObject.GetLocation()}");
                if (_tex)
                {
                    _sprite = null;
                    _sp_packer = null;
                    var path = UnityEditor.AssetDatabase.GetAssetPath(_tex).ToLower();
                    if (!PathDefs.IsAssetsResources(path))
                    {
                        if (!path.Contains(PathDefs.ASSETS_PATH_GUI_IMAGES))
                        {
                            _tex = null;
                            Log.LogError($"[单图]{this.GetType().Name}:{gameObject.GetLocation()},{path}, 只能使用{PathDefs.ASSETS_PATH_GUI_IMAGES}下的贴图");
                            return;
                        }
                    }
                    //if (false)
                    {
                        var importer = UnityEditor.AssetImporter.GetAtPath(path) as UnityEditor.TextureImporter;
                        if (importer.textureType == UnityEditor.TextureImporterType.Sprite && !PathDefs.IsAssetsResources(path))
                        {
                            Log.LogError($"[单图]{this.GetType().Name}:{gameObject.GetLocation()},{path}, 单图不能是Sprite格式");
                            //_tex = null;
                        }
                    }
                }
                else if (_sprite)
                {
                    _sp_packer = null;
                    var path = UnityEditor.AssetDatabase.GetAssetPath(_sprite.texture).ToLower();
                    if (!PathDefs.IsAssetsResources(path))
                    {
                        if (!path.StartsWith(PathDefs.ASSETS_PATH_GUI_IMAGES))
                        {
                            this._sprite = null;
                            this._spriterName = "";
                            if (path.StartsWith(PathDefs.ASSETS_PATH_GUI_SPRITES))
                            {
                                Log.LogInfo($"[原生sprite]MyImage:{gameObject.GetLocation()}, path={path}, 自动转换为图集模式");
                                _sp_name = Path.GetFileNameWithoutExtension(path);
                                var dir = Path.GetFileName(Path.GetDirectoryName(path));
                                var packerpath = PathDefs.PREFAB_PATH_UI_PACKERS + dir + "/" + dir + ".prefab";
                                _sp_packer = UnityEditor.AssetDatabase.LoadAssetAtPath<MySpritePacker>(packerpath);
                                if (_sp_packer)
                                {
                                    if (!_sp_packer.GetSprite(_sp_name, this.gameObject))
                                    {
                                        _sp_name = "";
                                    }
                                }
                                else
                                {
                                    _sp_name = "";
                                    Log.LogError($"[原生sprite]MyImage:{gameObject.GetLocation()},{path}, 找不到图集{packerpath}");
                                }                                
                                //UnityEditor.AssetDatabase.SaveAssets(); 
                            }
                            else
                            {
                                Log.LogError($"[原生sprite]MyImage:{gameObject.GetLocation()},{path}, 只能使用{PathDefs.ASSETS_PATH_GUI_IMAGES}下的贴图");
                            }
                            UnityEditor.EditorUtility.SetDirty(transform.root.gameObject);
                        }
                    }
                }
                else if (_sp_packer)
                {
                    if (!string.IsNullOrEmpty(_sp_name) && !_sp_packer.GetSprite(_sp_name, this.gameObject))
                    {
                        //_sp_name = "";
                    }
                }
                Editor_FixPackerNameSpriteName();
                Editor_FixTextureName();
            }
        }
#endif



        #region 事件管理
        public void SetTexture(Texture tex, object dept)
        {
            _dept = dept;
            if (_tex == tex)
            {
                return;
            }
            _sp_packer = null;
            _sp_name = null;
            _sprite = null;
            //
            _tex = tex;
            if (gameObject.activeInHierarchy)
            {
                OnInit();
            }
        }

        public void SetSprite(Sprite sprite, object dept)
        {
            _dept = dept;
            if (sprite == _sprite && !_sp_packer)
            {
                return;
            }
            _sp_packer = null;
            _sp_name = "";
            _sprite = sprite;
            _tex = null;
            _textureName = "";
#if UNITY_EDITOR
            if (sprite && !Application.isPlaying)
            {
                _spriterName = sprite.name;
            }         
#endif            
            if (gameObject.activeInHierarchy)
            {
                OnInit();
            }
        }

        public void SetSprite(MySpritePacker sp_packer, string sp_name, object dept)
        {
            var sp = sp_packer.GetSprite(sp_name, this.gameObject);
            if (!sp)
            {
                return;
            }

            _dept = dept;
            if (sp_packer == _sp_packer && sp_name == _sp_name)
            {
                return;
            }
            _sp_packer = sp_packer;
            _sp_name = sp_name;
            _sprite = null;
            _tex = null; 
            if (gameObject.activeInHierarchy)
            {
                OnInit();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Application.isPlaying && raycastTarget && !(gameObject.GetComponent<MyButton>() || gameObject.GetComponent<MyToggle>()))
            {
#if TestParticle
                SendMessageUpwards("_onClickOnBlank", this);
#else
                SendMessageUpwards("_onClickOnBlank", this);
#endif
            }
        }
        #endregion

    }



    /// <summary>
    /// 图片控件
    /// 可显示MySpritePacker中的图片,也可以显示sprite图片
    /// </summary>
    [AddComponentMenu("UI/My Sprite Image")]
    [ExecuteInEditMode]
    [Serializable]
    public class MySpriteImage : MySpriteImageBase
    {
        // 透明度
        [Range(0, 1)]
        [HideInInspector]
        [SerializeField]
        float alpha = 1;

        [HideInInspector]
        [SerializeField]
        private Color _color = Color.white;

        [HideInInspector]
        [SerializeField]
        private bool _fade = false;

        [HideInInspector]
        [SerializeField]
        bool asScaleAsParent = false;

        [HideInInspector]
        [SerializeField]
        bool _anchorScreen;

        [HideInInspector]
        [SerializeField]
        AnchorScreenType anchorScreenType = AnchorScreenType.AspectRatioCut;

        [HideInInspector]
        [SerializeField]
        bool _isBackground = false;

        [HideInInspector] [SerializeField] private bool autoLoadtexture;    //单图信息保存至assetbundle

        /// <summary>
        /// 对齐屏幕的方式
        /// </summary>
        public enum AnchorScreenType
        {
            /// <summary>
            /// 保持宽高比允许裁剪（边缘可能超出屏幕）
            /// </summary>
            AspectRatioCut = 1,
            /// <summary>
            /// 保持宽高比不裁剪（边缘可能有空白）
            /// </summary>
            AspectRatioNotCut = 2,
            /// <summary>
            /// 全屏（不保持宽高比）
            /// </summary>
            FullScreen = 3,
            /// <summary>
            /// 全屏（保持宽高比，边缘可能超出屏幕）
            /// </summary>
            FullScreenCut = 4,
        }



        public float Alpha { get { return alpha; } set { alpha = value; OnInit(); } }
        public override Color color { get { return _color; } set { _color = value; OnInit(); } }

        public bool AutoAnchorScreen { get { return _anchorScreen; } set { _anchorScreen = value; OnInit(); } }
        public AnchorScreenType AnchorToScreenType { get { return anchorScreenType; } set { anchorScreenType = value; OnInit(); } }
        public bool AsScaleAsParent { get { return asScaleAsParent; } set { asScaleAsParent = value; OnInit(); } }

        public bool isBackground { get { return _isBackground; } set { _isBackground = value; OnInit(); } }
        /// <summary>
        /// 灰阶显示
        /// </summary>
        public bool IsFade { get { return _fade; } set { _fade = value; OnInit(); } }

        //bool _need_show = false;
        float _animSpriteTimeNext;
        float _animSpriteTotalTimes = 1f;

        public MyList<string> AnimFrames
        {
            get
            {
                return _animSpriteFrames;
            }
            set
            {
                if (!_sp_packer)
                {
                    Log.LogError("please set main _sp_packer first");
                    return;
                }

                for (int i = 0; i < value.Count; ++i)
                {
                    if (!_sp_packer.GetSprite(value[i], this.gameObject))
                    {
                        return;
                    }
                }

                if (!value.Contains(_sp_name))
                {
                    value.Insert(0, _sp_name);
                }
                _animSpriteFrames = value;
                OnInit();
            }
        }

        public float AnimTotalTimes
        {
            get { return _animSpriteTotalTimes; }
            set { _animSpriteTotalTimes = value; }
        }

        protected override void OnInit()
        {
            //_need_show = true;
            if (!gameObject.activeInHierarchy) return;

            if (_anchorScreen)
                ComputeScaleByScreen();

            UnityEngine.Profiling.Profiler.BeginSample("MySpriteImage1.SetAllDirty");
            SetAllDirty();
            UnityEngine.Profiling.Profiler.EndSample();
        }


#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            OnInit();
            canvasRenderer.SetAlpha(alpha);
        }
#endif


        protected override void OnDidApplyAnimationProperties()
        {
            canvasRenderer.SetAlpha(alpha);
        }

#if !TestParticle
        public override Material material
        {
            get
            {
                if (_fade)
                {
                    if (_isBackground)
                        return UIGrapAssets.m_fade_bg_ui_mat;
                    else
                        return UIGrapAssets.m_fade_ui_mat;
                }
                else
                {
                    if (_useLineShader)
                        return UIGrapAssets.m_default_ui_mat_line_color;
                    else if (_isBackground)
                        return UIGrapAssets.m_default_bg_mat;
                    else
                        return UIGrapAssets.m_default_ui_mat;
                }
            }
        }
#endif




        protected override void OnEnable()
        {
            //_need_show = true;
            base.OnEnable();
            OnInit();
            gameObject.AddMissingComponent<CanvasRenderer>();
            canvasRenderer.SetAlpha(alpha);
            _beScaleCompluteComplete = false;
        }

        int calc_CurrentSpriteFrame()
        {
            if (UnityEngine.Time.time <= _animSpriteTimeNext)
            {
                return _animSpriteIndex;
            }
            var Count = _animSpriteFrames.Count;
            _animSpriteTimeNext = UnityEngine.Time.time + _animSpriteTotalTimes / Count;
            var idx = _animSpriteIndex + 1;
            if (idx >= Count)
            {
                idx = 0;
            }
            return idx;
        }

        MySpriteImage _parent = null;
        void LateUpdate()
        {
            if (_sprite)
            {
                var idx = -1;
                var _animSpriteFrames = this._animSpriteFrames;
                if (_animSpriteFrames != null && _animSpriteFrames.Count > 0)
                {
                    idx = calc_CurrentSpriteFrame();
                }
                //
                if (idx != _animSpriteIndex)
                {
                    _animSpriteIndex = idx;
                    UnityEngine.Profiling.Profiler.BeginSample("MySpriteImage2.SetAllDirty");
                    SetAllDirty();
                    UnityEngine.Profiling.Profiler.EndSample();
                }
            }

            if (_anchorScreen && !_beScaleCompluteComplete)
            {
                ComputeScaleByScreen();
            }

            if (asScaleAsParent)
            {
                if (!_parent)
                {
                    _parent = GameObjectUtils.FindInParents<MySpriteImage>(gameObject, false);
                }
                if (!_parent) return;

                _scale = _parent.scale;
            }
        }

        protected override void OnDisable()
        {
            //_need_show = false;           
            _beScaleCompluteComplete = false;
            base.OnDisable();
        }

        protected override void OnDestroy()
        {
            //_need_show = false;
            if (Application.isPlaying)
            {
                ClearResource();
            }
            base.OnDestroy();
        }



        bool _beScaleCompluteComplete = false;
        private void ComputeScaleByScreen()
        {
            var canvas = transform.parent.GetComponent<Canvas>();
            if (!canvas)
            {
                return;
            }

            SetNativeSize();

            var oldRect = rectTransform.rect;

            var canvasRectSize = (transform.parent as RectTransform).rect.size;

            int curckb = (int)((canvasRectSize.x / canvasRectSize.y) * 100f);    //当前宽高比
            int standckb = (int)(((float)MyUITools.RefScreenWidth / (float)MyUITools.RefScreenHeight) * 100f);//标准宽高比
            int contentckb = (int)((oldRect.width / oldRect.height) * 100f); //图片长宽比


            if (anchorScreenType == AnchorScreenType.AspectRatioCut)
            {
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

                //如果屏幕宽高比大于或等于标准，则对准宽度，否则对准高度
                if (curckb >= standckb)
                {
                    _scale = canvasRectSize.x / oldRect.width;
                }
                else
                {
                    _scale = canvasRectSize.y / oldRect.height;
                }
                //Log.LogError($"dimensions={dimensions},canvasRectSize={canvasRectSize},parentrect={(transform.parent as RectTransform).rect},rectTransform.rect.size={rectTransform.rect.size},_scale={_scale}");
            }
            else if (anchorScreenType == AnchorScreenType.AspectRatioNotCut)
            {
                // Log.LogError($"curckb={curckb},contentckb={contentckb},rectTransform.rect.size={oldRect},canvasRectSize={canvasRectSize}");
                if (curckb >= contentckb)
                {
                    rectTransform.anchorMin = new Vector2(0.5f, 0);//left,bottom;
                    rectTransform.anchorMax = new Vector2(0.5f, 1);//right,top
                    rectTransform.sizeDelta = new Vector2(canvasRectSize.y * (oldRect.width / oldRect.height), 0);
                }
                else
                {
                    rectTransform.anchorMin = new Vector2(0, 0.5f);//left,bottom;
                    rectTransform.anchorMax = new Vector2(1, 0.5f);//right,top
                    rectTransform.sizeDelta = new Vector2(0, canvasRectSize.x / (oldRect.width / oldRect.height));

                    //  rectTransform.FillToBothSide(Mathf.RoundToInt(canvasRectSize .x / (rectTransform.rect.width / rectTransform.rect.height)));
                }

            }
            else if (anchorScreenType == AnchorScreenType.FullScreen)
            {
                rectTransform.anchorMin = new Vector2(0, 0);
                rectTransform.anchorMax = new Vector2(1, 1);
                rectTransform.sizeDelta = new Vector2(0, 0);
                //rectTransform.SetFullByParent();
            }
            else if (anchorScreenType == AnchorScreenType.FullScreenCut)
            {
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);

                if (oldRect.width < canvasRectSize.x || oldRect.height < canvasRectSize.y)
                {
                    // 图片小于屏幕，需要放大
                    float scaleX = canvasRectSize.x / oldRect.width;
                    float scaleY = canvasRectSize.y / oldRect.height;
                    _scale = Mathf.Max(scaleX, scaleY);
                } else
                {
                    float scaleX = canvasRectSize.x / oldRect.width;
                    float scaleY = canvasRectSize.y / oldRect.height;
                    _scale = Mathf.Max(scaleX, scaleY);
                }
            }

            _beScaleCompluteComplete = true;

        }



        protected override void AddQuad(VertexHelper vertexHelper, MySpritePacker packer, Vector2 posMin, Vector2 posMax, Vector2 uvMin, Vector2 uvMax)
        {
            //Log.LogInfo($"posMin:{posMin} -> posMax:{posMax} = { Vector2.SqrMagnitude(posMin - posMax) }");
            if (posMin.x == posMax.x || posMin.y == posMax.y)
            {
                //面积为0
                return;
            }

            if (packer)
            {
                //Log.Log2File($"2 uvMin=({uvMin.x},{uvMin.y}), uvMax=({uvMax.x},{uvMax.y})");
                if (uvMin.x == 0)
                {
                    uvMin.x += 1f / packer.PackerImage.width;
                }
                //else if (uvMin.x == 1)
                //{
                //    uvMin.x -= 1f / sprite.texture.width;
                //}
                if (uvMin.y == 0)
                {
                    uvMin.y += 1f / packer.PackerImage.height;
                }
            }

            int currentVertCount = vertexHelper.currentVertCount;
            vertexHelper.AddVert(new Vector3(posMin.x, posMin.y, 0f), _color, new Vector2(uvMin.x, uvMin.y));
            vertexHelper.AddVert(new Vector3(posMin.x, posMax.y, 0f), _color, new Vector2(uvMin.x, uvMax.y));
            vertexHelper.AddVert(new Vector3(posMax.x, posMax.y, 0f), _color, new Vector2(uvMax.x, uvMax.y));
            vertexHelper.AddVert(new Vector3(posMax.x, posMin.y, 0f), _color, new Vector2(uvMax.x, uvMin.y));
            vertexHelper.AddTriangle(currentVertCount, currentVertCount + 1, currentVertCount + 2);
            vertexHelper.AddTriangle(currentVertCount + 2, currentVertCount + 3, currentVertCount);
        }


        public override void SetNativeSize()
        {
            _scale = 1f;
            if (_tex != null)
            {
                rectTransform.anchorMax = rectTransform.anchorMin;
                rectTransform.sizeDelta = new Vector2(_tex.width, _tex.height);
            }
            else if (_sprite != null)
            {
                if (_sprite.border.sqrMagnitude > 0f)
                {
                    if (!Application.isPlaying)
                        Log.LogError("九宫格不能自动适配尺寸");
                    return;
                }
                rectTransform.anchorMax = rectTransform.anchorMin;
                rectTransform.sizeDelta = _sprite.rect.size;
            }
            else if(_sp_packer != null)
            {
                if (!string.IsNullOrEmpty(_sp_name))
                {
                    var info = _sp_packer.GetUV(_sp_name);
                    if (info != null)
                    {
                        rectTransform.sizeDelta = info.size;
                    }
                }
            }
        }



        public void ClearResource()
        {
            if (_tex is RenderTexture && !_tex.name.Equals("Screenshot Camera"))
            {
                //清除其它控件对_renderTexture的引用，以免被重复ReleaseTemporary
                if (GetComponent<My3DRoomImage>())
                {
                    GetComponent<My3DRoomImage>().ClearRenderTexture(_tex as RenderTexture);
                }
                // Log.LogError($"22222 ReleaseTemporary texture:{_tex.ToString()}");
                RenderTexture.ReleaseTemporary(_tex as RenderTexture);
            }
        }
    }
}
