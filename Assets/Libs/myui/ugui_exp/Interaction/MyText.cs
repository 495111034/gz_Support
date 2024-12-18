using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    public enum MyKeyValuePairType
    {
        None = 0,
        FontSizes = 1,
        TextColor = 2,
    }

    [Serializable]
    public class MyKeyValuePair
    {
        [SerializeField]
        public int Key;

        [SerializeField]
        [TextArea(3, 10)]
        public string Value;

        public MyKeyValuePair() 
        {
            //Debug.Log($"MyKeyValuePair {this.GetHashCode()}\n{new StackTrace(true)}");
        }

        public static int IndexOf(MyKeyValuePair[] values, int key, out MyKeyValuePair value) 
        {
            value = null;
            if (values == null) 
            {
                return -1;
            }
            for (int i = 0, len = values.Length; i < len; ++i) 
            {
                if (key == values[i].Key) 
                {
                    value = values[i];
                    return i;
                }
            }
            return -1;
        }
    }
    public class MyText : Text, IBlackRangeClickHandler
    {
        public Events.UnityEvent onValueChange { set; get; }

        public override string text { 
            get => base.text;
            set
            {
                if (gradientInfos != null)
                {
                    gradientInfos.Clear();
                    MyListPool<GradientInfo>.Release(gradientInfos);
                }
                gradientInfos = null;
                //<color=#asdaaa>aaadd</color>sdsadsd<color=#ffffff #adc123,#adc321,1>aaaaaaaaaaaa</color>
                if (supportRichText && !string.IsNullOrEmpty(value))
                {
                    value = ParseGradientColor(value);
                }
                base.text = value;
                this.Check_LateUpdate();
                onValueChange?.Invoke();
            }
        }

        private string ParseGradientColor(string value)
        {
            var idx = value.IndexOf(" #");
            if (idx >= 0)
            {
                var idx1 = value.LastIndexOf('<', idx);
                if (idx1 >= 0)
                {
                    var idx2 = value.IndexOf('>', idx);
                    if (idx2 > idx)
                    {
                        string gradient_info = value.Substring(idx + 1, idx2 - idx - 1);
                        if (!string.IsNullOrEmpty(gradient_info))
                        {
                            string[] infos = gradient_info.Split(',');
                            if (infos.Length >= 3)
                            {
                                GradientInfo info = new GradientInfo(0, 0);
                                ColorUtility.TryParseHtmlString(infos[0], out info.gradient_top);
                                ColorUtility.TryParseHtmlString(infos[1], out info.gradient_bottom);
                                float.TryParse(infos[2], out info.gradient_angle);
                                if (gradientInfos == null)
                                {
                                    gradientInfos = MyListPool<GradientInfo>.Get();
                                    gradientInfos.Clear();
                                }

                                Vector2 dir = UIGradientUtils.RotationDir(info.gradient_angle);
                                info.localPositionMatrix = UIGradientUtils.LocalPositionMatrix(new Rect(0f, 0f, 1f, 1f), dir);
                                info.start_index = idx1;
                                bool is_start = false;
                                for (int i = idx1 - 1; i >= 0; i--)
                                {
                                    var v = value[i];
                                    if (v == '>')
                                    {
                                        is_start = true;
                                    }
                                    if (is_start)
                                    {
                                        info.start_index--;
                                        if (v == '<')
                                        {
                                            is_start = false;
                                        }
                                    }
                                    else if (v == ' ')
                                    {
                                        info.start_index--;
                                    }
                                }
                                info.end_index = info.start_index;
                                for (int i = idx2 + 1; i < value.Length; i++)
                                {
                                    var v = value[i];
                                    if (v != ' ')
                                    {
                                        if (v == '<')
                                        {
                                            break;
                                        }
                                        else
                                        {
                                            info.end_index++;
                                        }
                                    }
                                }
                                info.start_index = info.start_index * 4;
                                info.end_index = info.end_index * 4;
                                gradientInfos.Add(info);
                                value = value.Remove(idx, idx2 - idx);
                                value = ParseGradientColor(value);
                            }
                        }
                    }
                }
            }
            return value;
        }

        private struct GradientInfo
        {
            public Color gradient_top;
            public Color gradient_bottom;
            public float gradient_angle;
            public int start_index;
            public int end_index;
            public UIGradientUtils.Matrix2x3 localPositionMatrix;

            public GradientInfo(int _start_index, int _end_index)
            {
                gradient_top = Color.white;
                gradient_bottom = Color.white;
                gradient_angle = -1;
                start_index = _start_index;
                end_index = _end_index;
                localPositionMatrix = default;
            }
        }

        private List<GradientInfo> gradientInfos = null;
        /// <summary>
        /// 强制刷新SetVerticesDirty，屏蔽文本无改变不刷规则
        /// </summary>
        //public static int IsForceRefshDirty = 0;
        //[NonSerialized] int _IsForceRefshDirty = IsForceRefshDirty;

        [HideInInspector] [SerializeField] protected string m_Text_id;
        [HideInInspector] [SerializeField] protected string m_FontData_id;
        [HideInInspector] [SerializeField] protected bool m_saveToAB;
        [HideInInspector] [SerializeField] int m_maxWidth;
        [HideInInspector] [SerializeField] bool m_autoSize;
        [HideInInspector] [SerializeField] float _textSpacing = 0f;
        [HideInInspector] [NonSerialized] bool _fade = false;
        //[HideInInspector] [SerializeField] protected string mytext_value = "";
        [NonSerialized]
        object dept;
        //[NonSerialized] protected string m_Text_Last;

        public int MaxWidth { get { return m_maxWidth; } set { m_maxWidth = value; if (m_autoSize) { _set_dirty(); } } }
        public bool AutoSize { get { return m_autoSize; } set { m_autoSize = value; _set_dirty(); } }

        public float TextSpace { get { return _textSpacing; } set { _textSpacing = value; _set_dirty(); } }

        /// <summary>
        /// 置灰
        /// </summary>
        public bool IsFade { get { return _fade; } set { _fade = value; UpdateMaterial(); } }

        [SerializeField]
        MyKeyValuePair[] m_Extras = Array.Empty<MyKeyValuePair>();

        public string FontData_id 
        {
            get => m_FontData_id;
#if UNITY_EDITOR
            set => m_FontData_id = value;
#endif
        }

        protected MyText() : base()
        {
            //Debug.Log($"ctor {m_Extras?.GetHashCode()}\n{new StackTrace(true)}");
        }

        protected override void Start()
        {
            //Debug.Log($"Start {name} {m_Extras?.GetHashCode()}\n{new StackTrace(true)}");
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            if (!Application.isPlaying)
            {
                const char xspace = '\u00A0';//特殊空格//不换行
                if (true)
                {
                    Profiling.Profiler.BeginSample($"MyText.SetVerticesDirty Replace");
                    this.m_Text = this.m_Text.Replace(xspace, ' ');//单词换行
                    Profiling.Profiler.EndSample();
                }
                else
                {
                    if (!supportRichText)
                    {
                        this.m_Text = this.m_Text.Replace(' ', xspace);//字符换行
                    }
                    else
                    {
                        //this.m_Text = this.m_Text.Replace(xspace,' ');
                        //标签内的使用 空格，标签外的使用 xspace
                        if (this.m_Text.IndexOf(' ') >= 0 || this.m_Text.IndexOf(xspace) >= 0)
                        {
                            var mt = this.m_Text;
                            sb.Clear();
                            bool dirty = false;
                            bool inrich = false;
                            for (int i = 0, len = mt.Length; i < len; ++i)
                            {
                                var ch = mt[i];
                                if (ch == '<')
                                {
                                    inrich = true;
                                }
                                else if (ch == '>')
                                {
                                    inrich = false;
                                }

                                if (!inrich && ch == ' ')
                                {
                                    dirty = true;
                                    sb.Append(xspace);
                                }
                                else if (inrich && ch == xspace)
                                {
                                    dirty = true;
                                    sb.Append(' ');
                                }
                                else
                                {
                                    sb.Append(ch);
                                }
                            }
                            if (dirty)
                            {
                                this.m_Text = sb.ToString();
                            }
                        }
                    }
                }

                if (this.font)
                {
                    var dept = UnityEditor.AssetDatabase.GetAssetPath(this.font).ToLower();
                    if (PathDefs.IsAssetsResources(dept))
                    {
                        //去掉后缀名， 游戏运行时 使用 resources.load
                        var FontData_id = dept.Substring(0, dept.LastIndexOf('.'));
                        if (this.m_FontData_id != FontData_id)
                        {
                            this.m_FontData_id = FontData_id;
                        }
                    }
                    else 
                    {
                        if (!File.Exists(dept))
                        {
                            Log.LogError($"error font path={dept}, {this.font.name}");
                            this.font = null;
                            this.m_FontData_id = "";
                        }
                        else
                        {
                            if (this.m_FontData_id != this.font.name)
                            {
                                this.m_FontData_id = this.font.name;
                            }
                        }
                    }
                }
                else
                {
                    if (this.m_FontData_id != "")
                    {
                        this.m_FontData_id = "";
                    }
                }

                if (!__use_language_file) 
                {
                    if (m_saveToAB)
                    {
                        m_Text_id = gameObject.GetLocation();
                    }
                    else if (!string.IsNullOrEmpty(m_Text_id)) 
                    {
                        m_Text_id = string.Empty;
                    }
                }

                if (m_Extras != null)
                {
                    Array.Sort(m_Extras, (a, b) => { return a.Key - b.Key; });
                }
            }
            base.OnValidate();
        }
#endif

        private void _set_dirty() 
        {
            SetVerticesDirty();
            SetLayoutDirty();            
        }

        //protected override void OnDisable()
        //{
        //    base.OnDisable();
        //}

        //protected override void OnDestroy()
        //{
        //    base.OnDestroy();
        //}

        public static int frameCountCapture;
        //int SetVerticesDirty_cnt = 0;
        static StringBuilder sb = new StringBuilder();
        public override void SetVerticesDirty()
        {
            if (!font || !enabled || !gameObject.activeInHierarchy)
            {
                //Debug.Log($"{Time.frameCount} dirty skip {gameObject.GetLocation()} [{m_Text}], {_v_dirty},{frameCountCapture}\n{new StackTrace(true)}");
                return;
            }


            
            if (m_autoSize && (frameCountCapture == 0 || frameCountCapture != Time.frameCount))
            {
                //            GridLayoutGroup
                //HorizontalOrVerticalLayoutGroup
                //Debug.Log($"{Time.frameCount} dirty prep0 {gameObject.GetLocation()} [{m_Text}], {_v_dirty},{frameCountCapture} auto={m_autoSize}\n{new StackTrace(true)}");
                if (!Application.isPlaying)
                {
                    _LateUpdate();
                }
                else
                {
                    var frameCount = Time.frameCount;
                    if (frameCount - _v_dirty > 30)
                    {
                        _autosizes.Add(this);
                        _v_dirty = Time.frameCount;
                    }                    
                }
            }
            //else
            {
                //Debug.Log($"{Time.frameCount} dirty prep1 {gameObject.GetLocation()} [{m_Text}], {_v_dirty},{frameCountCapture} auto={m_autoSize}\n{new StackTrace(true)}");
                Profiling.Profiler.BeginSample($"Text.SetVerticesDirty");
                base.SetVerticesDirty();
                Profiling.Profiler.EndSample();
            }
        }

        static List<MyText> _autosizes = new List<MyText>();
        public static void CheckAutoSize() 
        {
            var frameCount = Time.frameCount;
            var autosizes = _autosizes;
            for (var i = autosizes.Count - 1; i >= 0; --i) 
            {
                var _text = autosizes[i];
                if (!_text || _text._v_dirty == 0) 
                {
                    autosizes.swap_tail_and_fast_remove( i );
                    continue;
                }
                if (frameCount - _text._v_dirty >= 2) 
                {
                    autosizes.swap_tail_and_fast_remove(i);
                    Profiling.Profiler.BeginSample(_text.name);
                    _text._LateUpdate();
                    Profiling.Profiler.EndSample();
                    _text._v_dirty = 0;
                }
            }
        }

        static unsafe void NoGcReplace(string txt, char search, char replace, ref int idx)
        {
            if (idx == -1)
            {
                idx = txt.IndexOf(search);
                if (idx == -1)
                {
                    return;
                }
            }
            fixed (char* ptr = txt)
            {
                for (var i = txt.Length - 1; i >= idx; --i)
                {
                    if (ptr[i] == search)
                    {
                        ptr[i] = replace;
                    }
                }
            }
        }

        [NonSerialized]
        int _v_dirty = 0;
        public int v_dirty => _v_dirty;
        private void _LateUpdate()
        {
            //Debug.Log($"{Time.frameCount} dirty flush0 {gameObject.GetLocation()} [{m_Text}] {_v_dirty},{frameCountCapture}");
            this.m_DisableFontTextureRebuiltCallback = true;
            if (string.IsNullOrEmpty(m_Text))
            {
                //Debug.Log($"{Time.frameCount} dirty flush1");
                Profiling.Profiler.BeginSample("SetSize one");
                rectTransform.sizeDelta = Vector2.one;
                Profiling.Profiler.EndSample();
            }
            else
            {
                //Log.LogInfo($"{gameObject.GetLocation()} -> {m_Text}");
                Profiling.Profiler.BeginSample($"{name} MyText.SetVerticesDirty autosize");
                {
                    Profiling.Profiler.BeginSample("GetGenerationSettings1");
                    var settings = GetGenerationSettings(Vector2.zero);
                    Profiling.Profiler.EndSample();
                    if (this.resizeTextForBestFit && settings.fontSize < this.resizeTextMaxSize)
                    {
                        settings.fontSize = this.resizeTextMaxSize;
                    }
                    //Profiling.Profiler.BeginSample("GetPreferredWidth0");
                    //float x = this.preferredWidth;
                    //Profiling.Profiler.EndSample();
                    Profiling.Profiler.BeginSample("GetPreferredWidth1");
                    float x = cachedTextGeneratorForLayout.GetPreferredWidth(m_Text, settings) / pixelsPerUnit;
                    Profiling.Profiler.EndSample();
                    //Debug.Log($"skipx x0={x0} -> x={x}");

                    if (m_maxWidth > settings.fontSize && x > m_maxWidth)
                    {
                        x = m_maxWidth;
                    }


                    
                    Profiling.Profiler.BeginSample("GetGenerationSettings2");
                    settings = GetGenerationSettings(new Vector2(x, 0.0f));
                    Profiling.Profiler.EndSample();                    
                    if (this.resizeTextForBestFit && settings.fontSize < this.resizeTextMaxSize)
                    {
                        settings.fontSize = this.resizeTextMaxSize;
                    }

                    float y;
                    Profiling.Profiler.BeginSample("GetPreferredHeight2");
                    if (settings.resizeTextForBestFit)
                    {
                        // bestfit 不对空格生效，这里转成 No-Break Space (U+00A0) 进行测量
                        int idx = -1;
                        NoGcReplace(m_Text,' ', '\u00A0', ref idx);
                        y = cachedTextGeneratorForLayout.GetPreferredHeight(m_Text, settings) / pixelsPerUnit;
                        if (idx != -1)
                        {
                            NoGcReplace(m_Text, '\u00A0', ' ', ref idx);
                        }
                    }
                    else
                    {
                        y = cachedTextGeneratorForLayout.GetPreferredHeight(m_Text, settings) / pixelsPerUnit;
                    }
                    Profiling.Profiler.EndSample();

                    //Debug.Log($"{Time.frameCount} dirty flush2 [{x},{y}]");
                    Profiling.Profiler.BeginSample("SetSize");
                    rectTransform.SetSize(new Vector2(x + 2, y + 2));
                    Profiling.Profiler.EndSample();
                }
                Profiling.Profiler.EndSample();
            }
            this.m_DisableFontTextureRebuiltCallback = false;
            Profiling.Profiler.BeginSample($"Text.SetVerticesDirty");
            base.SetVerticesDirty();
            Profiling.Profiler.EndSample();
        }


        public void Check_LateUpdate()
        {
            if (_v_dirty > 0)
            {
                this._LateUpdate();
                _v_dirty = 0;
            }
        }

        //protected override void UpdateGeometry()
        //{
        //    base.UpdateGeometry();
        //}

        //int OnPopulateMesh_cnt = 0;
        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            if (!font || !gameObject.activeInHierarchy)
            {
                //if (gameObject.name == "ImageText") Log.LogInfo($"MyText.OnPopulateMesh {gameObject.GetLocation()}  return\n{new StackTrace(true)}");
                return;
            }

            //if (gameObject.name == "ImageText") Log.LogInfo($"MyText.OnPopulateMesh {gameObject.GetLocation()} {++OnPopulateMesh_cnt}\n{new StackTrace(true)}");

            if (BuilderConfig.IsDebugBuild) Profiling.Profiler.BeginSample("Text.OnPopulateMesh");
            base.OnPopulateMesh(toFill);
            if (BuilderConfig.IsDebugBuild) Profiling.Profiler.EndSample();

            if (BuilderConfig.IsDebugBuild) Profiling.Profiler.BeginSample("MyText.SetTextSpace");
            SetTextSpace(toFill);
            if (BuilderConfig.IsDebugBuild) Profiling.Profiler.EndSample();

            UpdateGradient(toFill);
        }

        protected void UpdateGradient(VertexHelper toFill)
        {
            if (gradientInfos != null && gradientInfos.Count > 0)
            {
                int index = 0;
                UIVertex vertex = default(UIVertex);
                for (int i = 0; i < toFill.currentVertCount; i++)
                {
                    var info = gradientInfos[index];
                    if (info.start_index == -1 || (i >= info.start_index && i < info.end_index))
                    {
                        toFill.PopulateUIVertex(ref vertex, i);
                        Vector2 position = UIGradientUtils.VerticePositions[i % 4];
                        Vector2 localPosition = info.localPositionMatrix * position;
                        vertex.color *= Color.Lerp(info.gradient_bottom, info.gradient_top, localPosition.y);
                        toFill.SetUIVertex(vertex, i);

                        if (i + 1 == info.end_index)
                        {
                            if (index < gradientInfos.Count - 1)
                            {
                                index++;
                            }
                        }
                    }
                }
            }
        }

        protected void SetTextSpace(VertexHelper toFill)
        {
            if (_textSpacing == 0f) return;

            var vertexs = MyListPool<UIVertex>.Get();
            //List<UIVertex> vertexs = new List<UIVertex>();
            toFill.GetUIVertexStream(vertexs);
            int indexCount = toFill.currentIndexCount;
            string[] lineTexts = text.Split('\n');
            Line[] lines = new Line[lineTexts.Length];

            //根据lines数组中各个元素的长度计算每一行中第一个点的索引，每个字、字母、空母均占6个点
            for (int i = 0; i < lines.Length; i++)
            {
                //除最后一行外，vertexs对于前面几行都有回车符占了6个点
                if (i == 0)
                {
                    lines[i] = new Line(0, lineTexts[i].Length + 1);
                }
                else if (i > 0 && i < lines.Length - 1)
                {
                    lines[i] = new Line(lines[i - 1].EndVertexIndex + 1, lineTexts[i].Length + 1);
                }
                else
                {
                    lines[i] = new Line(lines[i - 1].EndVertexIndex + 1, lineTexts[i].Length);
                }
            }


            UIVertex vt;
            for (int i = 0; i < lines.Length; i++)
            {
                List<UIVertex> forstUIList = new List<UIVertex>() { new UIVertex(), new UIVertex(), new UIVertex(), new UIVertex(), new UIVertex(), new UIVertex() };
                for (int t = 0; t < 6; ++t)
                {
                    if (t < vertexs.Count)
                        forstUIList[t] = vertexs[t];
                }

                int sizeNum = 1;
                for (int j = lines[i].StartVertexIndex + 6; j <= lines[i].EndVertexIndex; j += 6)
                {

                    if (j < 0 || j >= vertexs.Count)
                    {
                        continue;
                    }

                    vt = vertexs[j];
                    if (Mathf.Abs(vt.position.x - forstUIList[0].position.x) <= font.fontSize / 3f) { sizeNum = 1; continue; }

                    for (int n = 0; n < 6; ++n)
                    {
                        vt = vertexs[j + n];

                        vt.position += new Vector3(_textSpacing * sizeNum, 0, 0);
                        vertexs[j + n] = vt;

                        //以下注意点与索引的对应关系
                        if (n <= 2)
                        {
                            toFill.SetUIVertex(vt, ((j + n) / 6) * 4 + (j + n) % 6);
                        }
                        if (n == 4)
                        {
                            toFill.SetUIVertex(vt, ((j + n) / 6) * 4 + (j + n) % 6 - 1);
                        }
                    }

                    sizeNum++;
                }


            }

            MyListPool<UIVertex>.Release(vertexs);

        }
#if !TestParticle
        public override Material material
        {
            get
            {
                if (_fade) return UIGrapAssets.m_fade_ui_mat;
                return UIGrapAssets.m_default_ui_mat;
            }
        }
#endif

        [HideInInspector] [SerializeField] bool __use_language_file = false;
        [HideInInspector] [SerializeField] string[] __language_params;

        public bool SaveToAB
        {
            get
            {
                return m_saveToAB;
            }
        }

#if UNITY_EDITOR
        public virtual bool SaveLanguageID(out string key, out string value) 
        {
            if (!__use_language_file && m_saveToAB && !string.IsNullOrEmpty(text)) 
            {
                key = gameObject.GetLocation();
                value = text;
                if (key != m_Text_id) 
                {
                    Log.LogError($"reset m_Text_id={m_Text_id} -> {key}");
                    m_Text_id = key;
                    //UnityEditor.PrefabUtility.SavePrefabAsset(transform.root.gameObject);                    
                    return true;
                }
                ///text = "";
                return false;
            }
            key = value = null;
            return false;
        }
#endif
        public void LoadLanguageID()
        {
            if (!string.IsNullOrEmpty(m_Text_id))
            {
                if (__use_language_file)
                {
                    m_Text = MyUITools.UIResPoolInstans.LangFromId(m_Text_id, __language_params);
                }
                else if (m_saveToAB)
                {
                    m_Text = MyUITools.UIResPoolInstans.UILangFromId(m_Text_id);
                }
                if (Application.isEditor && m_Text == m_Text_id)
                {
                    Log.LogError($"lang=[{m_Text_id}] not found at {gameObject.GetLocation()}");
                }
                //m_Text_id = null;
                __language_params = null;
            }
        }

        public void SetTextLangId(string lang_id)
        {
            __use_language_file = true;
            m_Text_id = lang_id;
        }

        public void ChangeLanguageParams(object language_params) 
        {
            if (__use_language_file && !string.IsNullOrEmpty(m_Text_id)) 
            {
                text = MyUITools.UIResPoolInstans.LangFromId(m_Text_id, language_params);
            }
        }

        public void ApplyFontSize(string lang) 
        {
            var index = MyKeyValuePair.IndexOf(m_Extras, (int)MyKeyValuePairType.FontSizes, out var fontsizes);
            if (index >=0 && fontsizes != null && !string.IsNullOrEmpty(fontsizes.Value)) 
            {
                var langsFontSizes = fontsizes.Value;
                var idx = langsFontSizes.IndexOf(lang);//{"sgp":[a,b,c]}
                if (idx > 0 && langsFontSizes[idx-1] == '"' && langsFontSizes[idx+lang.Length] == '"') 
                {
                    idx = langsFontSizes.IndexOf('[', idx + lang.Length) + 1;
                    var idx2 = langsFontSizes.IndexOf(']', idx);
                    var sizes = langsFontSizes.Substring(idx, idx2 - idx).Split(',');
                    if (int.TryParse(sizes[0], out var fSize)) 
                    {
                        this.fontSize = fSize;
                    }
                    if (sizes.Length > 1 && int.TryParse(sizes[1], out var minSize)) 
                    {
                        this.resizeTextMinSize = minSize;
                    }
                    if (sizes.Length > 2 && int.TryParse(sizes[2], out var maxSize))
                    {
                        this.resizeTextMaxSize = maxSize;
                    }
                }
            }
        }

        #region 事件管理

        public void SetFont(Font font, object dept)
        {
            this.dept = dept;
            if (this.font == font)
            {
                return;
            }
            this.font = font;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (Application.isPlaying && raycastTarget && !(gameObject.GetComponent<MyButton>() || gameObject.GetComponent<MyToggle>()))
            {
                SendMessageUpwards("_onClickOnBlank", this);
            }
        }
        #endregion
    }
    public class Line
    {

        private int _startVertexIndex = 0;
        /// <summary>
        /// 起点索引
        /// </summary>
        public int StartVertexIndex
        {
            get
            {
                return _startVertexIndex;
            }
        }

        private int _endVertexIndex = 0;
        /// <summary>
        /// 终点索引
        /// </summary>
        public int EndVertexIndex
        {
            get
            {
                return _endVertexIndex;
            }
        }

        private int _vertexCount = 0;
        /// <summary>
        /// 该行占的点数目
        /// </summary>
        public int VertexCount
        {
            get
            {
                return _vertexCount;
            }
        }

        public Line(int startVertexIndex, int length)
        {
            _startVertexIndex = startVertexIndex;
            _endVertexIndex = length * 6 - 1 + startVertexIndex;
            _vertexCount = length * 6;
        }
    }

}