using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{


    [AddComponentMenu("UI/MyImage", 0)]
    public class MyImage : Image, IBlackRangeClickHandler, IMySprite
    {

        public float fillAmountEndTime;

        public override Material material
        {
            get
            {
                if (_fade)
                {

                    return UIGrapAssets.m_fade_ui_mat;
                }
                else
                {
                    if (_useLineShader)
                        return UIGrapAssets.m_default_ui_mat_line_color;
                    return UIGrapAssets.m_default_ui_mat;
                }
            }
        }
        object _dept;

        public bool test;

        [HideInInspector]
        [SerializeField]
        MySpritePacker _sp_packer;

        [HideInInspector]
        [SerializeField]
        string _sp_name;

        [HideInInspector]
        [SerializeField]
        private bool _fade = false;

        [HideInInspector]
        [SerializeField]
        protected bool _useLineShader = false;

        /// <summary>
        /// 灰阶显示
        /// </summary>
        public bool IsFade { get { return _fade; } set { _fade = value; } }
        public bool UseLineShader { get { return _useLineShader; } set { _useLineShader = value; SetAllDirty(); } }


        [HideInInspector]
        [SerializeField]
        bool _x_reversal;
        public bool x_reversal
        {
            get => _x_reversal;
            set
            {
                if (value != _x_reversal)
                {
                    _x_reversal = value;
                    SetVerticesDirty();
                }
            }
        }
        [HideInInspector]
        [SerializeField]
        bool _y_reversal;
        public bool y_reversal
        {
            get => _y_reversal;
            set
            {
                if (value != _y_reversal)
                {
                    _y_reversal = value;
                    SetVerticesDirty();
                }
            }
        }

        [HideInInspector]
        [SerializeField]
        string _packerName, _spriterName;//用于多语言

        public string PackerName => _packerName;
        public string SpriteName => _spriterName;
        public string PackerSpriteName => _sp_name;

        public Sprite iSprite => sprite;
#if UNITY_EDITOR
        public void Editor_FixPackerNameSpriteName()
        {

            if (_sp_packer || MySpriteImageBase.Editor_CheckIsMissing(this, ref _sp_packer))
            {
                if (_sp_packer)
                {
                    if (_packerName != _sp_packer.name)
                    {
                        Log.LogInfo($"fixname1 {gameObject.GetLocation()} pack:{_packerName} -> {_sp_packer.name}");
                        _packerName = _sp_packer.name;
                        UnityEditor.EditorUtility.SetDirty(transform.root.gameObject);
                    }

                    if (_spriterName != "")
                    {
                        Log.LogInfo($"fixname2 {gameObject.GetLocation()} sp:{_spriterName} -> null");
                        _spriterName = "";
                        UnityEditor.EditorUtility.SetDirty(transform.root.gameObject);
                    }
                }
            }
            else
            {

                if (_packerName != "")
                {
                    Log.LogInfo($"fixname3 {gameObject.GetLocation()} pack:{_packerName} -> null");
                    _packerName = "";
                    UnityEditor.EditorUtility.SetDirty(transform.root.gameObject);
                }

                if (_sp_name != "")
                {
                    Log.LogInfo($"fixname4 {gameObject.GetLocation()} pack_sp:{_sp_name} -> null");
                    _sp_name = "";
                    UnityEditor.EditorUtility.SetDirty(transform.root.gameObject);
                }

                var sprite = this.sprite;
                if (!MySpriteImageBase.Editor_CheckIsMissing(this, ref sprite))
                {
                    var _name = sprite ? sprite.name : "";
                    if (_spriterName != _name)
                    {
                        Log.LogInfo($"fixname5 {gameObject.GetLocation()} sp:{_spriterName} -> {_name}");
                        _spriterName = _name;
                        UnityEditor.EditorUtility.SetDirty(transform.root.gameObject);
                    }
                }
                if (!ReferenceEquals(this.sprite, sprite))
                {
                    var tmp = this.sprite = Sprite.Create(new Texture2D(0,0), Rect.zero, Vector2.zero);
                    //Object.DestroyImmediate(this.sprite);
                    this.sprite = sprite;
                    UnityEditor.EditorUtility.SetDirty(transform.root.gameObject);
                    Object.DestroyImmediate(tmp);
                }
            }
        }

        public void Editor_SetPacker_sprite(MySpritePacker sp_packer, string sp_name)
        {
            var sp = sp_packer.GetSprite(sp_name, this.gameObject);
            _sp_packer = sp_packer;
            _sp_name = sp_name;
            _spriterName = "";
            sprite = sp;
        }
#endif


        public void SetSprite(Sprite sp, object dept)
        {
            _dept = dept;
            if (sp == sprite)
            {
                return;
            }
            _sp_packer = null;
            _sp_name = null;
            sprite = sp;
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
            sprite = sp;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            if (_sp_packer && !string.IsNullOrEmpty(_sp_name))
            {
                var sprite = _sp_packer.GetSprite(_sp_name, this.gameObject);
                if (sprite != this.sprite)
                {
                    this.sprite = sprite;
                }
            }

            SetAllDirty();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            if (Application.isPlaying)
            {
                SetVerticesDirty();
                return;
            }

            var isdirty = false;
            if (this.sprite)
            {
                var path = UnityEditor.AssetDatabase.GetAssetPath(sprite.texture).ToLower();
                if (path.StartsWith(PathDefs.PREFAB_PATH_UI_PACKERS))
                {
                    //图集
                    var sprite = _sp_packer ? _sp_packer.GetSprite(_sp_name, this.gameObject) : null;
                    if (this.sprite != sprite)
                    {
                        this.sprite = sprite;
                        if (!sprite) 
                        {
                            //_sp_name = "";
                        }
                    }
                }
                else
                {
                    //原生sprite
                    if (!PathDefs.IsAssetsResources(path))
                    {
                        if (!path.StartsWith(PathDefs.ASSETS_PATH_GUI_IMAGES))
                        {
                            isdirty = true;
                            this.sprite = null;
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
                                    this.sprite = _sp_packer.GetSprite(_sp_name, this.gameObject);
                                }
                                else
                                {
                                    Log.LogError($"[原生sprite]MyImage:{gameObject.GetLocation()},{path}, 找不到图集{packerpath}");
                                }
                                //UnityEditor.EditorUtility.SetDirty(gameObject);
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
            }
            else 
            {
                if (_sp_packer && !string.IsNullOrEmpty(_sp_name))
                {
                    this.sprite = _sp_packer.GetSprite(_sp_name, this.gameObject);
                    if (!this.sprite) 
                    {
                        //_sp_name = "";
                    }
                }
            }
            //
            Editor_FixPackerNameSpriteName();
            //Log.Log2File($"{GetType()}.OnValidate()");
            //if (isdirty)
            {
                SetAllDirty();
            }            
        }
#endif


        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            base.OnPopulateMesh(toFill);
            if (sprite && (_x_reversal || _y_reversal))
            {
                var rect = Sprites.DataUtility.GetOuterUV(sprite);
                float x = rect.z + rect.x, y = rect.w + rect.y;
                var uv = UIVertex.simpleVert;
                var uv0 = uv.uv0;
                for (var i = 0; i < toFill.currentVertCount; ++i)
                {
                    toFill.PopulateUIVertex(ref uv, i);
                    uv0 = uv.uv0;
                    if (_x_reversal)
                    {
                        uv0.x = x - uv0.x;
                    }
                    if (_y_reversal)
                    {
                        uv0.y = y - uv0.y;
                    }
                    uv.uv0 = uv0;
                    toFill.SetUIVertex(uv, i);
                }
            }
        }

        #region 事件管理
        public void OnPointerClick(PointerEventData eventData)
        {
            if (Application.isPlaying && raycastTarget && !(gameObject.GetComponent<MyButton>() || gameObject.GetComponent<MyToggle>()))
            {
                SendMessageUpwards("_onClickOnBlank", this);
            }
        }
        #endregion
    }
}


