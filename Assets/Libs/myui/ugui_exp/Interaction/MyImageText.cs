using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine.EventSystems;
using System.Diagnostics;

namespace UnityEngine.UI
{

    struct RichTextParseInfo 
    {
        /// <summary>
        /// 图片池
        /// </summary>
        public List<MaskableGraphic> m_ImagesPool;

        /// <summary>
        /// 图片的最后一个顶点的索引
        /// </summary>
        public List<int> m_ImagesVertexIndex;

    }



    //[AddComponentMenu("UI/Text", 0)]
    public class MyImageText : MyText, EventSystems.IPointerClickHandler
    {
        public static Color HrefColor = Color.blue; //超链接的颜色，允许的热更新代码中重新赋值
        public bool KeepEmptyChar; //保留多个空白字符,因为有的界面需要显示多个空白

        public float emoji_y_offset { set; get; } //当有表情存在时，文字向下偏移值
        /// <summary>
        /// 解析完最终的文本
        /// </summary>
        //private string m_OutputText = "";
        //int test = 1234; int abcd = 4567;

        RichTextParseInfo m_richInfo;

        /// <summary>
        /// 图片池
        /// </summary>
        private List<MaskableGraphic> m_ImagesPool
        {
            get => m_richInfo.m_ImagesPool;
            set
            {
                m_richInfo.m_ImagesPool = value;
            }
        }


        /// <summary>
        /// 图片的最后一个顶点的索引
        /// </summary>
        private List<int> m_ImagesVertexIndex
        {
            get => m_richInfo.m_ImagesVertexIndex;
            set
            {
                m_richInfo.m_ImagesVertexIndex = value;
            }
        }


        /// <summary>
        /// 超链接信息列表
        /// </summary>
        private List<HrefInfo> m_HrefInfos = null;

        public List<HrefInfo> GetAllHrefInfos()
        {
            return m_HrefInfos;
        }

        /// <summary>
        /// 正则取出所需要的属性
        /// </summary>
        private static readonly Regex s_ImageRegex =
            new Regex(@"<quad name=(.+?) size=(\d*\.?\d+%?) anim=(1|0)( width=(\d*\.?\d+%?))?? />", RegexOptions.Singleline);

        /// <summary>
        /// 超链接正则
        /// </summary>
        //private static readonly Regex s_HrefRegex = new Regex(@"<color=([^ ]*?) href=([^>]+?)>([^<]+.*?)</color>", RegexOptions.Singleline);
        private static readonly Regex s_HrefRegex = new Regex(@"<(?<HtmlTag>[\w]+)=([^>]*)\shref=([^>]+?)>(((?<Nested><\k<HtmlTag>[^>]*>)|</\k<HtmlTag>>(?<-Nested>)|.*?)*)</\k<HtmlTag>>", RegexOptions.Singleline);


        protected MyImageText() : base()
        {

        }

        static byte[] ctoi = null;

        static Color32 ParseColor(string str, Color def)
        {

            //return Color.yellow;

            if (string.IsNullOrEmpty(str)) 
            {
                return def;
            }

            if (ctoi == null)
            {
                ctoi = new byte[256];
                for (var i = '0'; i <= '9'; ++i)
                {
                    ctoi[i] = (byte)(i - '0');
                }
                for (var i = 'a'; i <= 'f'; ++i)
                {
                    ctoi[i] = (byte)(i - 'a' + 10);
                }
            }

            str = str.ToLower().Trim();
            if (str[0] != '#')
            {
                var f = typeof(Color).GetProperty(str, System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                if (f == null)
                {
                    return def;
                }
                return (Color)f.GetValue(null);
            }

            if (str.Length <= 6) 
            {
                return def;
            }
            //
            var r = (byte)(ctoi[str[1]] * 16 + ctoi[str[2]]);
            var g = (byte)(ctoi[str[3]] * 16 + ctoi[str[4]]);
            var b = (byte)(ctoi[str[5]] * 16 + ctoi[str[6]]);
            var a = (byte)255;
            if (str.Length > 8)
            {
                try
                {
                    a = (byte)(ctoi[str[7]] * 16 + ctoi[str[8]]);
                } catch
                {
                    a = (byte)255;
                }
            }
            return new Color32(r, g, b, a);
        }


        static short[] richs_real_pos = new short[1024];
        //static Regex _s_blank_rgx = new Regex("\\s+");
        /// <summary>
        /// 获取超链接解析后的最后输出文本
        /// </summary>
        /// <returns></returns>
        void DecodeHrefInfos(string m_Text)
        {
            m_HrefInfos?.Clear();
            
            if (string.IsNullOrEmpty(m_Text) || m_Text.Length < 13 || m_Text.IndexOf("href=") < 0)
            {
                return;
            }

            

            //new Regex(@"<a href=([^>\n\s]+)>(.*?)(</a>)", RegexOptions.Singleline);
            UnityEngine.Profiling.Profiler.BeginSample("MyImageText.DecodeHrefInfos.Matches");
            var matches = s_HrefRegex.Matches(m_Text);
            UnityEngine.Profiling.Profiler.EndSample();
            //
            int Count = matches.Count;
            if (Count == 0)
            {
                return;
            }

            UnityEngine.Profiling.Profiler.BeginSample("MyImageText.DecodeHrefInfos.foreach");
            if (m_HrefInfos == null)
            {
                //UnityEngine.Profiling.Profiler.BeginSample("new List");
                m_HrefInfos = new List<HrefInfo>(Count);
                //UnityEngine.Profiling.Profiler.EndSample();
            }
            for (int i = 0; i < Count; ++i)
            {
                //UnityEngine.Profiling.Profiler.BeginSample("matches[i]");
                var Groups = matches[i].Groups;
                //UnityEngine.Profiling.Profiler.EndSample();
                var color = ParseColor(Groups[1].Value, this.color * 2);
                var url = Groups[2].Value;
                //UnityEngine.Profiling.Profiler.BeginSample("groups[2]");
                var name_group = Groups[3];
                //UnityEngine.Profiling.Profiler.EndSample();

                //var name_group_content = Regex.Replace(name_group.Value, "<[^>]*>", "");
                //UnityEngine.Profiling.Profiler.BeginSample("groups[2].Length");
                //var Length = name_group_content.Length;
                //UnityEngine.Profiling.Profiler.EndSample();
                if (name_group.Length > 0)
                {
                    //UnityEngine.Profiling.Profiler.BeginSample("groups[2].Index");
                    //var Index = name_group.Index;
                    //var cut = skip_richs[Index];
                    //Index -= cut;
                    //if (Index < 0) Index = 0;
                    //UnityEngine.Profiling.Profiler.EndSample();                    

                    //UnityEngine.Profiling.Profiler.BeginSample("new HrefInfo");
                    var hrefInfo = new HrefInfo
                    {
                        url = url,       //url
                        title = name_group.Value,          //
                        startIndex = richs_real_pos[name_group.Index] * 4, // 超链接里的文本起始顶点索引
                        endIndex = richs_real_pos[name_group.Index + (name_group.Length - 1)] * 4 + 2,
                        color = color,
                        obj = this.gameObject,
                    };
                    //UnityEngine.Profiling.Profiler.EndSample();

                    //UnityEngine.Profiling.Profiler.BeginSample("add HrefInfo");
                    m_HrefInfos.Add(hrefInfo);
                    //UnityEngine.Profiling.Profiler.EndSample();
                }
                else
                {
                    if (!Application.isPlaying)
                    {
                        Log.LogError($"url 名字不能为空, {gameObject.GetLocation()}");
                    }
                }
            }
            UnityEngine.Profiling.Profiler.EndSample();
        }

        void UpdateQuadImage()
        {
            emoji_y_offset = 0;
            UnityEngine.Profiling.Profiler.BeginSample("UpdateQuadImage");
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    if (UnityEditor.PrefabUtility.GetPrefabInstanceStatus(this) == UnityEditor.PrefabInstanceStatus.Connected)
                    {
                        return;
                    }
                }
#endif
                //
                if (!supportRichText)
                {
                    UnityEngine.Profiling.Profiler.BeginSample("delete childs");
                    for (int i = 0, childCount = transform.childCount; i < childCount; ++i)
                    {
                        var c = transform.GetChild(i);
                        if (c.name.StartsWith("textImage"))
                        {
#if UNITY_EDITOR
                            if (!Application.isPlaying)
                            {
                                var del_pic = c;
                                //
                                UnityEditor.EditorApplication.CallbackFunction DelayCall = null;
                                DelayCall = () =>
                                {
                                    UnityEditor.EditorApplication.delayCall -= DelayCall;
                                    if (del_pic)
                                    {
                                        var go = del_pic.gameObject;
                                        GameObject.DestroyImmediate(del_pic, true);
                                        GameObject.DestroyImmediate(go, true);
                                    }
                                };
                                UnityEditor.EditorApplication.delayCall += DelayCall;
                            }
                            else
                            {
                                Destroy(c.gameObject);
                            }
#else                        
                            Destroy(c.gameObject);
#endif
                        }
                    }
                    UnityEngine.Profiling.Profiler.EndSample();

                    this.m_ImagesVertexIndex?.Clear();
                    this.m_HrefInfos?.Clear();
                    return;
                }

                var m_Text = this.m_Text;
                //unity2021 提取格式和空白
                {
                    var Length = m_Text.Length;
                    if (richs_real_pos.Length < Length)
                    {
                        richs_real_pos = new short[Length];
                    }
                    var _richs_real_pos = richs_real_pos;
                    bool skiping = false;
                    short pre = 0;
                    for (var i = 0; i < Length; ++i)
                    {
                        var ch = m_Text[i];
                        //quad 占用一个字符
                        var is_quad = ch == '<' && Length > i + 4 && m_Text[i + 1] == 'q' && m_Text[i + 2] == 'u' && m_Text[i + 3] == 'a' && m_Text[i + 4] == 'd';
                        var xskip = (!is_quad && (skiping || ch == ' ' || ch == '\n' || ch == '\r' || ch == '\t' || ch == '<' || ch == '>')) ? 0 : 1;//空白不占用字符
                        _richs_real_pos[i] = pre = (short)(pre + xskip);
                        if (ch == '<')
                        {
                            skiping = true;
                        }
                        else
                        {
                            if (ch == '>')
                            {
                                skiping = false;
                            }
                        }
                    }
                    //下标从0开始
                    for (var i = 0; i < Length; ++i)
                    {
                        if (_richs_real_pos[i] > 0)
                        {
                            --_richs_real_pos[i];
                        }
                    }
                }
                //
                UnityEngine.Profiling.Profiler.BeginSample("DecodeHrefInfos");
                DecodeHrefInfos(m_Text);
                UnityEngine.Profiling.Profiler.EndSample();

                UnityEngine.Profiling.Profiler.BeginSample("UpdateQuadImage");
                {
                    var m_ImagesPool = this.m_ImagesPool;
                    var m_ImagesVertexIndex = this.m_ImagesVertexIndex;
                    if (m_ImagesVertexIndex != null)
                    {
                        m_ImagesVertexIndex.Clear();
                    }
                    if (m_ImagesPool != null)
                    {
                        m_ImagesPool.Clear();
                    }

                    MatchCollection matches = null;
                    int Count = 0;
                    if (!string.IsNullOrEmpty(m_Text) && m_Text.Length > 20 && m_Text.IndexOf("<quad") >= 0)
                    {
                        //s_ImageRegex = new Regex(@"<quad name=(.+?) size=(\d*\.?\d+%?) anim=(1|0)( width=(\d*\.?\d+%?))?? />", RegexOptions.Singleline);
                        UnityEngine.Profiling.Profiler.BeginSample("UpdateQuadImage.Matches");
                        matches = s_ImageRegex.Matches(m_Text);
                        UnityEngine.Profiling.Profiler.EndSample();
                        if (matches != null)
                        {
                            UnityEngine.Profiling.Profiler.BeginSample("UpdateQuadImage.MatchesCount");
                            Count = matches.Count;
                            UnityEngine.Profiling.Profiler.EndSample();
                        }
                    }

                    if (Count > 0)
                    {
                        //Log.LogInfo(m_OutputText + (new System.Diagnostics.StackTrace(true).ToString()));
                        if (m_ImagesVertexIndex == null)
                        {
                            UnityEngine.Profiling.Profiler.BeginSample("get m_ImagesVertexIndex");
                            m_ImagesVertexIndex = this.m_ImagesVertexIndex = MyListPool<int>.Get();
                            UnityEngine.Profiling.Profiler.EndSample();

                            if (Count > m_ImagesVertexIndex.Capacity)
                            {
                                UnityEngine.Profiling.Profiler.BeginSample("reserve m_ImagesVertexIndex");
                                m_ImagesVertexIndex.Capacity = Count + 1;
                                UnityEngine.Profiling.Profiler.EndSample();
                            }
                        }
                        //
                        //
                        if (m_ImagesPool == null)
                        {
                            UnityEngine.Profiling.Profiler.BeginSample("get m_ImagesPool");
                            m_ImagesPool = this.m_ImagesPool = MyListPool<MaskableGraphic>.Get();
                            UnityEngine.Profiling.Profiler.EndSample();
                            if (Count > m_ImagesPool.Capacity)
                            {
                                UnityEngine.Profiling.Profiler.BeginSample("reserve m_ImagesPool");
                                m_ImagesPool.Capacity = Count + 1;
                                UnityEngine.Profiling.Profiler.EndSample();
                            }
                        }
                        //
                        {
                            UnityEngine.Profiling.Profiler.BeginSample("UpdateQuadImage.InitPool");
                            for (int i = 0, childCount = transform.childCount; i < childCount; ++i)
                            {
                                var c = transform.GetChild(i);
                                if (c.name.StartsWith("textImage"))
                                {
                                    var bhv = c.GetComponent<MaskableGraphic>();
                                    if (!(bhv is MySpriteFrameSeq || bhv is MyImageInText))
                                    {
#if UNITY_EDITOR
                                        if (!Application.isPlaying)
                                        {
                                            //Log.LogError($"{i} error bhv=[{bhv?.GetType()}] in {c.gameObject.name},{c.gameObject.GetHashCode()}");
                                            c.gameObject.name = "delay delete1" + c.gameObject.name;
                                            //
                                            var del_go = c.gameObject;
                                            var del_gab = bhv;
                                            UnityEditor.EditorApplication.CallbackFunction delayCall = null;
                                            delayCall = () =>
                                            {
                                                UnityEditor.EditorApplication.delayCall -= delayCall;
                                                if (del_gab)
                                                {
                                                //Log.LogInfo("aaaaa");
                                                GameObject.DestroyImmediate(del_gab, true);
                                                //Log.LogInfo("bbbbb");
                                            }
                                                if (del_go)
                                                {
                                                //Log.LogInfo("ccccc");
                                                GameObject.DestroyImmediate(del_go, true);
                                                //Log.LogInfo("ddddd");
                                            }
                                            };
                                            UnityEditor.EditorApplication.delayCall += delayCall;
                                        }
#endif
                                        continue;
                                    }
                                    bhv.transform.localScale = Vector3.zero;
                                    m_ImagesPool.Add(bhv);
                                }
                            }
                            UnityEngine.Profiling.Profiler.EndSample();

                            UnityEngine.Profiling.Profiler.BeginSample("UpdateQuadImage.InitVertIndexs");
                            for (int i = 0; i < Count; ++i)
                            {
                                //UnityEngine.Profiling.Profiler.BeginSample(i.ToString());
                                {
                                    //UnityEngine.Profiling.Profiler.BeginSample("get matche");
                                    Match match = matches[i];
                                    //UnityEngine.Profiling.Profiler.EndSample();

                                    //UnityEngine.Profiling.Profiler.BeginSample("get Index");
                                    var picIndex = richs_real_pos[match.Index];
                                    var endIndex = picIndex * 4 + 3;
                                    //UnityEngine.Profiling.Profiler.EndSample();

                                    //UnityEngine.Profiling.Profiler.BeginSample("AddVertex");
                                    m_ImagesVertexIndex.Add(endIndex);
                                    //UnityEngine.Profiling.Profiler.EndSample();

                                    //UnityEngine.Profiling.Profiler.BeginSample("get Groups");
                                    var Groups = match.Groups;
                                    bool isAnim = Groups.Count > 3 && Groups[3].Value == "1";
                                    //UnityEngine.Profiling.Profiler.EndSample();

                                    //UnityEngine.Profiling.Profiler.BeginSample("get grap");
                                    MaskableGraphic grap = null;
                                    if (i >= m_ImagesPool.Count)
                                    {
                                        grap = _createChildImageObj(isAnim);
                                        m_ImagesPool.Add(grap);
                                    }
                                    else
                                    {
                                        grap = m_ImagesPool[i];
                                        if (!grap || isAnim != (grap is MySpriteFrameSeq))
                                        {
                                            if (!Application.isPlaying)
                                            {
                                                Log.LogInfo($"delete quad image {grap?.gameObject?.name} in {gameObject.GetLocation()}");
                                            }
                                            //
                                            UnityEngine.Profiling.Profiler.BeginSample("_deleteChildImageObj");
                                            if (grap)
                                            {
                                                var go = grap.gameObject;
                                                //Log.LogError($"delete unmath child {go.name},{go.GetHashCode()}");
                                                go.name = "delay delete2 " + go.name;
                                                if (Application.isPlaying)
                                                {
                                                    Object.Destroy(go, 0.1f);
                                                }
                                                else
                                                {
#if UNITY_EDITOR
                                                    var del_grap = grap;
                                                    var del_go = go;
                                                    UnityEditor.EditorApplication.CallbackFunction delayCall = null;
                                                    delayCall = () =>
                                                    {
                                                        UnityEditor.EditorApplication.delayCall -= delayCall;
                                                        if (del_grap)
                                                        {
                                                            //Log.LogInfo("eeeee");
                                                            GameObject.DestroyImmediate(del_grap, true);
                                                            //Log.LogInfo("fffff");
                                                        }
                                                        if (del_go)
                                                        {
                                                            //Log.LogInfo("ggggg");
                                                            GameObject.DestroyImmediate(del_go, true);
                                                            //Log.LogInfo("hhhhh");
                                                        }
                                                    };
                                                    UnityEditor.EditorApplication.delayCall += delayCall;
#endif
                                                }
                                            }
                                            UnityEngine.Profiling.Profiler.EndSample();

                                            grap = _createChildImageObj(isAnim);
                                            m_ImagesPool[i] = grap;
                                        }
                                    }
                                    //UnityEngine.Profiling.Profiler.EndSample();

                                    //UnityEngine.Profiling.Profiler.BeginSample("spriteName");
                                    var spriteName = Groups[1].Value;
                                    //UnityEngine.Profiling.Profiler.EndSample();

                                    //UnityEngine.Profiling.Profiler.BeginSample("int.Parse");
                                    var size = int.Parse(Groups[2].Value);
                                    //UnityEngine.Profiling.Profiler.EndSample();
                                    emoji_y_offset = Mathf.Ceil(fontSize / 2f);
                                    //
                                    if (Application.isPlaying)
                                    {
                                        if (grap is MySpriteImage)
                                        {
                                            if (spriteName.Contains("/"))
                                            {
                                                var arr = spriteName.Split('/');
                                                MyUITools.UIResPoolInstans.SetSprite(grap as IMySprite, arr[0], arr[1]);
                                            }
                                            else
                                            {
                                                MyUITools.UIResPoolInstans.SetTexture(grap as IMyTexture, spriteName);
                                            }
                                        }
                                        //
                                        if (grap is MySpriteFrameSeq)
                                        {
                                            var seq = grap as MySpriteFrameSeq;
                                            if (spriteName.Contains("/"))
                                            {
                                                seq.UseAllFrame = false;
                                                var arr = spriteName.Split('/');
                                                seq.SetSpriteList(new List<string>() { arr[1] });
                                                MyUITools.UIResPoolInstans.SetSpritePacker(seq, arr[0]);
                                            }
                                            else
                                            {
                                                seq.UseAllFrame = true;
                                                MyUITools.UIResPoolInstans.SetSpritePacker(seq, spriteName);
                                            }
                                        }
                                    }
                                    else
                                    {
#if UNITY_EDITOR
                                        if (grap is MySpriteImage)
                                        {
                                            if (spriteName.Contains("/"))
                                            {
                                                var arr = spriteName.Split('/');
                                                var path = $"{PathDefs.PREFAB_PATH_UI_PACKERS}/{arr[0]}/{arr[0]}.prefab";
                                                var packer = UnityEditor.AssetDatabase.LoadAssetAtPath<MySpritePacker>(path);
                                                (grap as MySpriteImage).SetSprite(packer, arr[1], null);
                                            }
                                            else
                                            {
                                                Log.LogError($"美术工程不支持单图{spriteName}, at {grap.gameObject.GetLocation()}");
                                            }
                                        }
                                        else if (grap is MySpriteFrameSeq)
                                        {
                                            var _seq = (grap as MySpriteFrameSeq);
                                            if (spriteName.Contains("/"))
                                            {
                                                _seq.UseAllFrame = false;
                                                var arr = spriteName.Split('/');
                                                var path = $"{PathDefs.PREFAB_PATH_UI_PACKERS}{arr[0]}/{arr[0]}.prefab";
                                                var packer = UnityEditor.AssetDatabase.LoadAssetAtPath<MySpritePacker>(path);
                                                _seq.SetSpriteList(new List<string>() { arr[1] });
                                                _seq.SetSpritePacker(packer, null);
                                            }
                                            else
                                            {
                                                _seq.UseAllFrame = true;
                                                //
                                                var path = $"{PathDefs.PREFAB_PATH_UI_PACKERS}{spriteName}/{spriteName}.prefab";
                                                var packer = UnityEditor.AssetDatabase.LoadAssetAtPath<MySpritePacker>(path);
                                                _seq.SetSpritePacker(packer, null);
                                            }
                                        }
#endif
                                    }

                                    //Log.LogInfo($"{grap.GetHashCode()},{grap.gameObject.GetHashCode()}");
                                    //UnityEngine.Profiling.Profiler.BeginSample("set sizeDelta");
                                    grap.rectTransform.sizeDelta = new Vector2(size, size);
                                    //UnityEngine.Profiling.Profiler.EndSample();
                                }
                                //UnityEngine.Profiling.Profiler.EndSample();
                            }
                            UnityEngine.Profiling.Profiler.EndSample();
                        }
                    }
                    else
                    {
#if UNITY_EDITOR
                        if (!Application.isPlaying)
                        {
                            if (m_HrefInfos == null || m_HrefInfos.Count == 0)
                            {
                                //TODO
                                //Log.LogError($"{gameObject.GetLocation()} 没有富文本格式，请使用性能更好的 MyText");
                            }
                        }
#endif
                        for (int i = 0, childCount = transform.childCount; i < childCount; ++i)
                        {
                            var c = transform.GetChild(i);
                            if (c.name.StartsWith("textImage"))
                            {
                                c.localScale = Vector3.zero;
                            }
                        }
                    }
                    //
                    if (m_ImagesPool != null)
                    {
                        UnityEngine.Profiling.Profiler.BeginSample("UpdateQuadImage.UnsetPool");
                        Count = m_ImagesVertexIndex == null ? 0 : m_ImagesVertexIndex.Count;
                        for (var i = Count; i < m_ImagesPool.Count; i++)
                        {
                            if (m_ImagesPool[i])
                            {
                                if (m_ImagesPool[i] is MySpriteImage)
                                {
                                    (m_ImagesPool[i] as MySpriteImage).SetSprite(null, null);
                                    (m_ImagesPool[i] as MySpriteImage).SetTexture(null, null);
                                }
                                else if (m_ImagesPool[i] is MySpriteFrameSeq)
                                {
                                    (m_ImagesPool[i] as MySpriteFrameSeq).SetSpritePacker(null, null);
                                }
                                m_ImagesPool[i].transform.localScale = Vector3.zero;
                            }
                        }
                        UnityEngine.Profiling.Profiler.EndSample();
                    }
                }
                UnityEngine.Profiling.Profiler.EndSample();
            }
            UnityEngine.Profiling.Profiler.EndSample();
        }
        //int _idx = 0;
        MaskableGraphic _createChildImageObj(bool isAnim)
        {
            UnityEngine.Profiling.Profiler.BeginSample("_createChildImageObj");

            MaskableGraphic grap = null;
            var go = new GameObject("textImage");
            if (isAnim)
            {
                grap = go.AddComponent<MySpriteFrameSeq>();
                go.name += "_gif";
            }
            else
            {
                grap = go.AddComponent<MyImageInText>();
                grap.raycastTarget = false;
                go.name += "_tex";
            }

            if (!Application.isPlaying) 
            {
                Log.LogInfo($"create quad image {go.name} in {gameObject.GetLocation()}");
            }

            go.layer = gameObject.layer;
            var rt = go.transform as RectTransform;
            if (rt)
            {
                rt.SetParent(rectTransform);
                rt.localPosition = Vector3.zero;
                rt.localRotation = Quaternion.identity;
                rt.localScale = Vector3.one;// * 0.9f;
                rt.anchorMin = rt.anchorMax = rt.pivot = Vector2.one / 2;
            }
            UnityEngine.Profiling.Profiler.EndSample();

            grap.raycastTarget = false;

            return grap;
        }



        [NonSerialized] string m_Text_Last;
        public override void SetVerticesDirty()
        {
            if (!font || !gameObject.activeInHierarchy)
            {
                //if (gameObject.name == "ImageText") Log.LogInfo($"MyImageText.SetVerticesDirty {gameObject.GetLocation()} return1\n{new StackTrace(true)}");
                return;
            }
#if UNITY_EDITOR
            //Log.LogInfo($"UnityEditor.Selection.activeGameObject={UnityEditor.Selection.activeGameObject}");
            if (UnityEditor.Selection.activeGameObject == gameObject)
            {
                m_Text_Last = null;
            }
#endif
            base.SetVerticesDirty();
            //if (m_Text_Last != m_Text)
            {
                m_Text_Last = m_Text;
                UnityEngine.Profiling.Profiler.BeginSample("MyImageText.UpdateQuadImage");
                UpdateQuadImage();
                UnityEngine.Profiling.Profiler.EndSample();
                //
                var b = m_HrefInfos?.Count > 0;
                if (b != raycastTarget)
                {
                    raycastTarget = b;
                }
            }
        }

        //protected override void OnDestroy() 
        //{
        //    base.OnDestroy();
        //    Debug.Log($"{gameObject?.GetLocation()} OnDestroy");
        //}

        //int OnPopulateMesh_cnt = 0;
        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            if (!font || !gameObject.activeInHierarchy)
            {
                //if (gameObject.name == "ImageText") Log.LogInfo($"skip MyImageText.OnPopulateMesh {gameObject.GetLocation()} \n{new StackTrace(true)}");
                return;
            }
            UnityEngine.Profiling.Profiler.BeginSample("MyImageText.OnPopulateMesh");
            {
                //
                var m_ImagesVertexIndex = this.m_ImagesVertexIndex;
                //if (gameObject.name == "ImageText") Log.LogInfo($"MyImageText.OnPopulateMesh {gameObject.GetLocation()}, verts={toFill.currentVertCount}, quad={m_ImagesVertexIndex?.Count}, call={OnPopulateMesh_cnt++}\n{new StackTrace(true)}");
                //
                //var currentVertCount = toFill.currentVertCount;
                toFill.Clear();
                //if(toFill.currentVertCount == 0)
                {
                    this.m_DisableFontTextureRebuiltCallback = true;
                    base.OnPopulateMesh(toFill);
                    this.m_DisableFontTextureRebuiltCallback = false;
                }
                //Log.LogInfo($"VertCount={currentVertCount} -> {toFill.currentVertCount} at {gameObject.GetLocation()}");
                //
                //UnityEngine.Profiling.Profiler.BeginSample("MyImageText.OnPopulateMesh");
                //BaseOnPopulateMesh(toFill);
                //UnityEngine.Profiling.Profiler.EndSample();

                if (supportRichText)
                {
                    UIVertex vert = new UIVertex();
                    //quad            
                    if (m_ImagesVertexIndex != null)
                    {
                        UnityEngine.Profiling.Profiler.BeginSample("MyImageText.m_ImagesVertexIndex");
                        var m_ImagesPool = this.m_ImagesPool;
                        //var rootSize = rectTransform.GetSize();
                        //var rootPivot = rectTransform.pivot;
                        for (int i = 0, Count = m_ImagesVertexIndex.Count; i < Count; i++)
                        {
                            var endIndex = m_ImagesVertexIndex[i];
                            if (endIndex >= toFill.currentVertCount)
                            {
                                if (!Application.isPlaying)
                                {
                                    Log.LogError($"m_ImagesVertexIndex[{i}].endIndex={endIndex}, toFill.currentVertCount={toFill.currentVertCount}, at {this.gameObject.GetLocation()}");
                                }
                                break;
                            }
                            else
                            {
                                toFill.PopulateUIVertex(ref vert, endIndex);//左下角
                                var pos = vert.position;
                                //
                                var rt = m_ImagesPool != null ? m_ImagesPool[i].rectTransform : null;
                                //toFill.PopulateUIVertex(ref vert, endIndex - 2);//右上角
                                //rt.anchoredPosition = new Vector2((vert.position.x + size.x * rt.pivot.x) + rootSize.x * (rootPivot.x - 0.5f), (vert.position.y + size.y * rt.pivot.y) + rootSize.y * (rootPivot.y - 0.5f));
                                //rt.anchoredPosition = vert.position; // (pos + vert.position) / 2;//中心
                                if (rt)
                                {
                                    var size = rt.sizeDelta;
                                    rt.anchoredPosition = (Vector2)pos + new Vector2(size.x / 2.1f, fontSize / 3);
                                    rt.localScale = Vector3.one;
                                }
                                //if (gameObject.name == "ImageText") Log.LogInfo($"quad endIndex={endIndex}");
                                //Log.LogWarning($"{name} {i}, endIndex={endIndex}, pos={pos}");
                                //Debug.LogWarning($"{name} {i}, endIndex={endIndex}, pos={pos}"); 
                                // 抹掉左下角的quad 预览(将4个顶点重叠)
                                for (int j = endIndex - 3; j < endIndex; ++j)
                                {
                                    toFill.PopulateUIVertex(ref vert, j);
                                    vert.position = pos;// (vert.position + pos) / 2;
                                                        //vert.color = Color.clear;
                                    toFill.SetUIVertex(vert, j);
                                }
                            }
                        }
                        //m_ImagesVertexIndex.Clear();
                        //MyListPool<int>.Release(m_ImagesVertexIndex);
                        //this.m_ImagesVertexIndex = null;

                        if (m_ImagesPool != null)
                        {
                            MyListPool<MaskableGraphic>.Release(m_ImagesPool);
                            this.m_ImagesPool = null;
                        }
                        UnityEngine.Profiling.Profiler.EndSample();
                    }

                    // 处理超链接包围框            
                    if (m_HrefInfos != null)
                    {
                        UnityEngine.Profiling.Profiler.BeginSample("MyImageText.m_HrefInfos");
                        var currentVertCount = toFill.currentVertCount;
                        foreach (var hrefInfo in m_HrefInfos)
                        {
                            hrefInfo.boxes.Clear();
                            //
                            var startIndex = hrefInfo.startIndex;
                            if (startIndex >= currentVertCount)
                            {
                                if (!Application.isPlaying)
                                {
                                    Log.LogError($"hrefInfo.startIndex={startIndex}, currentVertCount={currentVertCount}");
                                }
                                break;
                            }

                            // 将超链接里面的文本顶点索引坐标加入到包围框                    
                            //toFill.PopulateUIVertex(ref vert, startIndex);//左上角
                            //vert.color = Color.yellow; toFill.SetUIVertex(vert, startIndex);
                            //Vector2 pos = vert.position;
                            //int idx2 = hrefInfo.endIndex;
                            //

                            float xMin = 0, xMax = 0, yMin = 0, yMax = 0;

                            //var bounds = new Bounds(pos, Vector3.zero);
                            for (int i = startIndex, m = hrefInfo.endIndex; i <= m; i += 4)
                            {
                                if (i + 2 >= toFill.currentVertCount)
                                {
                                    if (Application.isEditor && v_dirty == 0)
                                    {
                                        Log.LogError($"i={i}, toFill.currentVertCount={toFill.currentVertCount}");
                                    }
                                    break;
                                }

                                //左上角
                                toFill.PopulateUIVertex(ref vert, i);
                                //vert.color = hrefInfo.color;
                                toFill.SetUIVertex(vert, i);
                                var pos1 = vert.position;

                                //右下角
                                toFill.PopulateUIVertex(ref vert, i + 2);
                                //vert.color = hrefInfo.color; 
                                toFill.SetUIVertex(vert, i + 2);
                                var pos2 = vert.position;

                                {
                                    toFill.PopulateUIVertex(ref vert, i + 1);
                                    //vert.color = hrefInfo.color;
                                    toFill.SetUIVertex(vert, i + 1);
                                    toFill.PopulateUIVertex(ref vert, i + 3);
                                    //vert.color = hrefInfo.color;
                                    toFill.SetUIVertex(vert, i + 3);
                                }

                                if (i == startIndex || pos1.x <= xMin)
                                {
                                    if (i != startIndex) // 换行, 重新添加包围框
                                    {
                                        var rect = Rect.MinMaxRect(xMin, yMin, xMax, yMax);
                                        //Log.LogInfo(gameObject.GetLocation());
                                        //Log.LogInfo($"line pos={pos1},{pos2}, rect x=[{rect.xMin},{rect.xMax}],y=[{rect.yMin},{rect.yMax}]");
                                        hrefInfo.boxes.Add(rect);
                                    }
                                    xMin = pos1.x;
                                    xMax = pos2.x;
                                    yMax = pos1.y;
                                    yMin = pos2.y;
                                }
                                else
                                {
                                    xMax = pos2.x;
                                    if (yMax < pos1.y)
                                    {
                                        yMax = pos1.y;
                                    }
                                    if (yMin > pos2.y)
                                    {
                                        yMin = pos2.y;
                                    }
                                }
                            }
                            //
                            {
                                var rect = Rect.MinMaxRect(xMin, yMin, xMax, yMax);
                                //Log.LogInfo(gameObject.GetLocation());
                                //Log.LogInfo($"rect x=[{rect.xMin},{rect.xMax}],y=[{rect.yMin},{rect.yMax}]");
                                hrefInfo.boxes.Add(rect);
                            }
                        }
                        UnityEngine.Profiling.Profiler.EndSample();
                    }
                }

                UpdateGradient(toFill);
            }
            UnityEngine.Profiling.Profiler.EndSample();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (m_ImagesVertexIndex != null)
            {
                MyListPool<int>.Release(m_ImagesVertexIndex);
                m_ImagesVertexIndex = null;
            }
        }
        /*
        private readonly UIVertex[] m_TempVerts = new UIVertex[4];
        void BaseOnPopulateMesh(VertexHelper toFill)
        {
            UnityEngine.Profiling.Profiler.BeginSample("MyImageText.BaseOnPopulateMesh");
            if (font)
            {
                this.m_DisableFontTextureRebuiltCallback = true;
                Vector2 size = rectTransform.rect.size;
                IList<UIVertex> verts = null;
                TextGenerationSettings generationSettings = GetGenerationSettings(size);
                if (cachedTextGenerator.PopulateWithErrors(m_Text, generationSettings, gameObject) && (verts = cachedTextGenerator.verts).Count > 0)
                {
                    //
                    float pixel = 1f / pixelsPerUnit;
                    int vertsLength = verts.Count - 4;
                    Vector2 vector = new Vector2(verts[0].position.x, verts[0].position.y) * pixel;
                    vector = PixelAdjustPoint(vector) - vector;
                    if (vector != Vector2.zero)
                    {
                        for (int i = 0; i < vertsLength; i++)
                        {
                            int idx = i & 3;
                            m_TempVerts[idx] = verts[i];
                            m_TempVerts[idx].position = m_TempVerts[idx].position * pixel;
                            m_TempVerts[idx].position.x = m_TempVerts[idx].position.x + vector.x;
                            m_TempVerts[idx].position.y = m_TempVerts[idx].position.y + vector.y;
                            if (idx == 3)
                            {
                                toFill.AddUIVertexQuad(m_TempVerts);
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < vertsLength; i++)
                        {
                            int idx = i & 3;
                            m_TempVerts[idx] = verts[i];
                            m_TempVerts[idx].position = m_TempVerts[idx].position * pixel;
                            if (idx == 3)
                            {
                                toFill.AddUIVertexQuad(m_TempVerts);
                            }
                        }
                    }
                    m_DisableFontTextureRebuiltCallback = false;
                    //
                    Profiling.Profiler.BeginSample("MyText.SetTextSpace");
                    SetTextSpace(toFill);
                    Profiling.Profiler.EndSample();
                }
                else 
                {
                    this.m_DisableFontTextureRebuiltCallback = false;
                }
            }
            UnityEngine.Profiling.Profiler.EndSample();
        }
        */
        /// <summary>
        /// 点击事件检测是否点击到超链接文本
        /// </summary>
        /// <param name="eventData"></param>
        public new void OnPointerClick(PointerEventData eventData)
        {
            if (m_HrefInfos != null)
            {
                Vector2 lp;
                var b = RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out lp);
                //Log.LogInfo($"click {gameObject.name} at {lp}, b={b}");
                foreach (var hrefInfo in m_HrefInfos)
                {
                    var boxes = hrefInfo.boxes;
                    for (var i = 0; i < boxes.Count; ++i)
                    {
                        if (boxes[i].Contains(lp))
                        {
                            if (Application.isPlaying)
                            {
                                //Log.LogInfo($"hit href at {i} = {boxes[i]}");
                                SendMessageUpwards("OnHrefClickEvent", hrefInfo);
                            }
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 超链接信息类
        /// </summary>
        public class HrefInfo
        {
            public string url;
            public string title;
            public int startIndex;
            public int endIndex;
            public Color32 color;
            public readonly List<Rect> boxes = new List<Rect>();
            public GameObject obj;
        }
    }

}
