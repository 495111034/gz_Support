using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    /// <summary>
    /// 多层图片进度条,可用于boss 血条等
    /// 要用所有图片处于同一MySpritePacker中
    /// </summary>
    [AddComponentMenu("UI/Multi Image Slider")]
    [ExecuteInEditMode]
    public class MyMultiImageSlider : MaskableGraphic, IMySpritePacker
    {
        [HideInInspector]
        [SerializeField]
        string back_img;

        [HideInInspector]
        [SerializeField]
        bool _bgNotFillCenter = false;

        [SerializeField]
        RectOffset back_ext;

        // 透明度
        [Range(0, 1)]
        [HideInInspector]
        [SerializeField]
        float alpha = 1;

        [Range(0, 1)]
        [HideInInspector]
        [SerializeField]
        float _value = 1;
        float _value_ing = 1;

        [SerializeField]
        List<string> sprite_arr;

        [HideInInspector]
        [SerializeField]
        MySpritePacker spritePacker;
        object _dept;

        [HideInInspector] [SerializeField] private bool m_x_reversal;    //进度条翻转


        [NonSerialized]
        bool _value_inited;
        public float Valueing { get => _value_ing; set => _value_ing = value; }
        public float Value
        {
            get { return _value; }
            set
            {
                value = value > 1f ? 1f : (value < 0f ? 0f : value);
                if (_value != value)
                {
                    _value = value;
                    if (!_value_inited || Mathf.Abs(_value - _value_ing) < 0.05f)
                    {
                        _value_inited = true;
                        _value_ing = _value;
                        SetVerticesDirty();
                    }
                }
            }
        }
        public string BackGroupImageID { get { return back_img; } set { back_img = value; InitSpList(); } }
        public RectOffset BGImageOffset { get { return back_ext; } set { back_ext = value; } }
        public bool BgNotFillCenter { get { return _bgNotFillCenter; } set { _bgNotFillCenter = value; } }
        public bool Reversal { get { return m_x_reversal; } set { m_x_reversal = value; } }

        private int progress_idx;
        private bool is_refsh_text = false;
        private MyText text_progress;


        [HideInInspector]
        [SerializeField]
        string _packerName;

        public string PackerName => _packerName;

        public MySpritePacker iPacker => spritePacker;
#if UNITY_EDITOR
        public void Editor_FixPackerName()
        {
            if (!MySpriteImageBase.Editor_CheckIsMissing(this, ref spritePacker))
            {
                var _name = spritePacker ? spritePacker.name : "";
                if (_packerName != _name)
                {
                    Log.LogInfo($"fixname6 {gameObject.GetLocation()} pack:{_packerName} -> {_name}");
                    _packerName = _name;
                    UnityEditor.EditorUtility.SetDirty(transform.root.gameObject);
                }
            }
        }
#endif



        public void SetSpriteList(List<string> list)
        {
            InitSpList(list);
        }

        public void SetProgressText(MyText myText)
        {
            text_progress = myText;
            is_refsh_text = true;
        }

        private void Update()
        {
            if (_value_ing != _value)
            {                
                if (_value_ing < _value)
                {
                    _value_ing += Time.deltaTime * 2f;
                    if (_value_ing > _value) 
                    {
                        _value_ing = _value;
                    }
                }
                else 
                {
                    _value_ing -= Time.deltaTime * 2f;
                    if (_value_ing < _value)
                    {
                        _value_ing = _value;
                    }
                }
                if (gameObject.activeInHierarchy)
                {
                    SetVerticesDirty();
                }
            }
            if (is_refsh_text)
            {
                is_refsh_text = false;
                if (text_progress != null)
                {
                    if (progress_idx > 0)
                    {
                        if (!text_progress.enabled) text_progress.enabled = true;
                        text_progress.ChangeLanguageParams(progress_idx + 1);
                    }
                    else
                    {
                        if (text_progress.enabled) text_progress.enabled = false;
                    }
                }
            }
        }


#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (!Application.isPlaying)
            {
                InitSpList();
                Editor_FixPackerName();
            }
            canvasRenderer.SetAlpha(alpha);
        }
#endif

        void InitSpList(List<string> list = null)
        {
            //rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _total_size.x);
            //rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _total_size.y);

            if (list != null)
            {
                if (sprite_arr == null)
                {
                    sprite_arr = new List<string>();
                }
                else
                {
                    sprite_arr.Clear();
                }
                sprite_arr.AddRange(list);
            }

            if (sprite_arr == null)
            {
                return;
            }

            if (spritePacker == null)
            {
                return;
            }

            if (gameObject.activeInHierarchy)
            {
                UnityEngine.Profiling.Profiler.BeginSample("MyMultiImageSlider.SetAllDirty");
                SetAllDirty();
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        public override Material material
        {
            get
            {
                return UIGrapAssets.m_default_ui_mat;
            }
        }

        protected override void OnEnable()
        {            
            base.OnEnable();
            _value_ing = _value;
            InitSpList();
            canvasRenderer.SetAlpha(alpha);
            _value_inited = false;
        }

        public float GetBestSize(int axis)
        {
            return rectTransform.rect.size[axis];
        }

        protected override void OnDidApplyAnimationProperties()
        {
            canvasRenderer.SetAlpha(alpha);
        }


        public override Texture mainTexture { get { return spritePacker != null ? spritePacker.PackerImage : null; } }
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            //base.OnPopulateMesh(vh);
            vh.Clear();

            if (!spritePacker || sprite_arr == null || sprite_arr.Count == 0) return;

            var rc = GetPixelAdjustedRect();
            var xpos = rc.xMin;
            var ypos = rc.yMin;
            //
            {
                var height = rc.height;
                //
                var progress = sprite_arr.Count * _value_ing;
                int idx = Mathf.CeilToInt(progress) - 1;                //使用第几张图片
                if (progress_idx != idx)
                {
                    progress_idx = idx;
                    if(text_progress != null) is_refsh_text = true;
                }
                float cleft = progress - idx;     //最上面的图进度

                if (idx <= 0)
                {
                    //底图
                    if (!string.IsNullOrEmpty(back_img))
                    {
                        var sprite_info_back = spritePacker.GetUV(back_img);
                        if (sprite_info_back != null)
                        {
                            var pos = new Vector4(xpos - back_ext.left, ypos - back_ext.bottom, rc.xMax + back_ext.right, rc.yMax + back_ext.top);
                            AddSprite(vh, pos, sprite_info_back, !_bgNotFillCenter);
                        }
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(sprite_arr[idx - 1]))
                    {
                        var info = spritePacker.GetUV(sprite_arr[idx - 1]);
                        var pos = new Vector4(xpos, ypos, xpos + rc.width, ypos + height);
                        AddSprite(vh, pos, info);
                    }
                }
                //
                if (idx >= 0)
                {
                    if (!string.IsNullOrEmpty(sprite_arr[idx]))
                    {
                        var info = spritePacker.GetUV(sprite_arr[idx]);
                        var pos = new Vector4(xpos, ypos, xpos + rc.width * cleft, ypos + height);
                        AddSprite(vh, pos, info);
                    }
                }
            }
        }

        private static readonly Vector2[] VertScratch = new Vector2[4];
        private static readonly Vector2[] UVScratch = new Vector2[4];

        void AddSprite(VertexHelper vh, Vector4 pos, MySpriteInfo spInfo, bool fillCenter = true)
        {
            var width = (pos.z - pos.x);
            var height = pos.w - pos.y;
            var size = spInfo.size;
            if (size.x > 0 && size.y > 0 && width > 0 && height > 0)
            {
                if (spInfo.border.sqrMagnitude > 0f && (spInfo.border.z + spInfo.border.x) < width)
                {
                    //九宫格
                    Vector4 outerUV = spInfo.GetOuterUV();
                    Vector4 innerUV = spInfo.GetInnerUV();
                    Rect pixelAdjustedRect = base.GetPixelAdjustedRect();
                    if (!m_x_reversal)
                    {
                        VertScratch[0] = new Vector2(0, 0);
                        VertScratch[3] = new Vector2(width, height);
                        VertScratch[1].x = spInfo.border.x;
                        VertScratch[1].y = spInfo.border.y;
                        VertScratch[2].x = width - spInfo.border.z;
                        VertScratch[2].y = height - spInfo.border.w;
                    }
                    else
                    {
                        var rc = GetPixelAdjustedRect();
                        VertScratch[0] = new Vector2(rc.width - width, 0);
                        VertScratch[3] = new Vector2(rc.width, rc.height);
                        VertScratch[1] = new Vector2(rc.width, 0);
                        VertScratch[2].x = rc.width - width + spInfo.border.z;
                        VertScratch[2].y = height;
                    }
                    for (int i = 0; i < 4; i++)
                    {
                        VertScratch[i].x = VertScratch[i].x + pos.x;
                        VertScratch[i].y = VertScratch[i].y + pos.y;
                    }
                    UVScratch[0] = new Vector2(outerUV.x, outerUV.y);
                    UVScratch[1] = new Vector2(innerUV.x, innerUV.y);
                    UVScratch[2] = new Vector2(innerUV.z, innerUV.w);
                    UVScratch[3] = new Vector2(outerUV.z, outerUV.w);
                    for (int j = 0; j < 3; j++)
                    {
                        int num = j + 1;
                        for (int k = 0; k < 3; k++)
                        {
                            if (fillCenter || j != 1 || k != 1)
                            {
                                int num2 = k + 1;
                                AddQuad(vh, new Vector2(VertScratch[j].x, VertScratch[k].y), new Vector2(VertScratch[num].x, VertScratch[num2].y), color, new Vector2(UVScratch[j].x, UVScratch[k].y), new Vector2(UVScratch[num].x, UVScratch[num2].y));
                            }
                        }
                    }
                }
                else
                {
                    //非九宫格
                    //var oneX = 2f / spInfo.mainTextureSize.x;
                    //var oneY = 2f / spInfo.mainTextureSize.y;
                    //AddQuad(vh, new Vector2(pos.x, pos.y), new Vector2(pos.z, pos.w), color, new Vector2(spInfo.rect.xMin + oneX, spInfo.rect.yMin + oneY), new Vector2(spInfo.rect.xMax - oneX, spInfo.rect.yMax - oneY));
                    if (width > size.x) 
                    {
                        width = size.x;
                    }
                    if (height > size.y)
                    {
                        height = size.y;
                    }
                    var rect = spInfo.rect;
                    AddQuad(vh, new Vector2(pos.x, pos.y), new Vector2(pos.z, pos.w), color, new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMin + (rect.xMax - rect.xMin) * (width / size.x), rect.yMin + (rect.yMax - rect.yMin) * (height / size.y)));
                }
            }
        }

        void AddQuad(VertexHelper vertexHelper, Vector2 posMin, Vector2 posMax, Color32 color, Vector2 uvMin, Vector2 uvMax)
        {
            //color = Color.clear;
            int currentVertCount = vertexHelper.currentVertCount;
            vertexHelper.AddVert(new Vector3(posMin.x, posMin.y, 0f), color, new Vector2(uvMin.x, uvMin.y));
            vertexHelper.AddVert(new Vector3(posMin.x, posMax.y, 0f), color, new Vector2(uvMin.x, uvMax.y));
            vertexHelper.AddVert(new Vector3(posMax.x, posMax.y, 0f), color, new Vector2(uvMax.x, uvMax.y));
            vertexHelper.AddVert(new Vector3(posMax.x, posMin.y, 0f), color, new Vector2(uvMax.x, uvMin.y));
            vertexHelper.AddTriangle(currentVertCount, currentVertCount + 1, currentVertCount + 2);
            vertexHelper.AddTriangle(currentVertCount + 2, currentVertCount + 3, currentVertCount);
        }



        protected override void OnDestroy()
        {
            //_need_show = false;
            base.OnDestroy();
        }

        protected override void OnDisable()
        {
            //_need_show = false;
            base.OnDisable();
        }


        public void SetSpritePacker(MySpritePacker sp, object dept)
        {
            _dept = dept;
            if (this.spritePacker == sp)
            {
                return;
            }
            spritePacker = sp;
            
            InitSpList();
        }
    }
}


