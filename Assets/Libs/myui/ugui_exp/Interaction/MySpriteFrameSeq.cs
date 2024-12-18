using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    /// <summary>
    /// 序列帧播放器
    /// 要求所有帧图片都在同一MySpritePacker图集中
    /// </summary>
    [AddComponentMenu("UI/MySprite Sequence")]
    [ExecuteInEditMode]
    [RequireComponent(typeof(CanvasRenderer))]
    public class MySpriteFrameSeq : MaskableGraphic, IMySpritePacker
    {
        // 透明度
        [Range(0, 1)]
        [HideInInspector]
        [SerializeField]
        float alpha = 1;


        [HideInInspector]
        [SerializeField]
        MySpritePacker spritePacker;
        object _dept;

        [HideInInspector]
        [SerializeField]
        bool useAllFrame = false;

        [HideInInspector]
        [SerializeField]
        float time = 1f;

        [HideInInspector]
        [SerializeField]
        float delay = 0f;

        [HideInInspector]
        [SerializeField]
        float keyframe = 0f; //播放多久后出发事件

        [HideInInspector]
        [SerializeField]
        iTween.LoopType loop = iTween.LoopType.loop;

        [HideInInspector]
        [SerializeField]
        bool completeEvent = false;

        [HideInInspector]
        [SerializeField]
        bool autosize = false;        

        [HideInInspector]
        Vector2 _total_size;

        [HideInInspector]
        [SerializeField]
        private bool _fade = false;

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
                    Log.LogInfo($"fixname12 {gameObject.GetLocation()} pack:{_packerName} -> {_name}");
                    _packerName = _name;
                    UnityEditor.EditorUtility.SetDirty(transform.root.gameObject);
                }
            }
        }
#endif


        /// <summary>
        /// 灰阶显示
        /// </summary>
        public bool IsFade { get { return _fade; } set { _fade = value; } }

        public bool IsPause = false;

        //public MySpritePacker SpritePacker { get { return spritePacker; } set { spritePacker = value; InitBySpList(); } }
        public float Time { get { return time; } set { time = value; StartPlay(); } }
        public float Delay { get { return delay; } set { delay = value; StartPlay(); } }
        public float KeyFrame { get { return keyframe; } set { keyframe = value; StartPlay(); } }
        public bool CompleteEvent { get { return completeEvent; } set { completeEvent = value; StartPlay(); } }
        public bool UseAllFrame { get { return useAllFrame; } set { if (value != useAllFrame) { useAllFrame = value; StartPlay(); } } }
        public iTween.LoopType Loop { get { return loop; } set { loop = value; StartPlay(); } }
        public bool AutoSize { get { return autosize; } set { autosize = value; StartPlay(); } }


        // [HideInInspector]
        [SerializeField]
        string[] sprite_arr;

        //
        MySpriteInfo[] sprite_info_arr;
        int current_pic_idx;

        float startTime;
        float lastChangeTime;
        float perFrameTime;

        bool hasKeyFrame = false;
        bool hasComplete = false;
        //bool hasPlayed = false;

        public void SetSpriteList(List<string> list)
        {
            if (useAllFrame)
            {
                Log.LogError($"useAllFrame is true, not allowed SetSpriteList()");
                return;
            }            

            InitSpList(list);

            StartPlay();
        }

        void InitSpList(List<string> list)
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            //
            if (UseAllFrame)
            {
                //使用全部
                list = null;
                sprite_arr = null;
            }
            else if (list != null)
            {
                //运行时指定指定
                sprite_arr = null;
            }

            if (spritePacker == null)
            {
                if (list != null)
                {
                    list.Sort((a, b) =>
                    {
                        return a.CompareTo(b);
                    });
                    sprite_arr = list.ToArray();
                }
                return;
            }

            if ((list != null && list.Count == 1) || (sprite_arr != null && sprite_arr.Length == 1))
            {
                string findkey = list != null ? list[0] : sprite_arr[0];
                list = new List<string>();
                foreach (var uv_info in spritePacker.uvList)
                {
                    if (uv_info.name.StartsWith(findkey))
                    {
                        list.Add(uv_info.name);
                    }
                }
                list.Sort((a, b) =>
                {
                    return a.CompareTo(b);
                });
                sprite_arr = list.ToArray();
            }
            //
            if (sprite_arr != null)
            {
                //美术配置指定
                foreach (var key in sprite_arr)
                {
                    spritePacker.GetUV(key);
                }
            }
            else 
            {
                //
                if (list == null)
                {
                    list = new List<string>();
                    foreach (var info in spritePacker.uvList)
                    {
                        list.Add(info.name);
                    }
                }
                else 
                {
                    foreach (var key in list)
                    {
                        spritePacker.GetUV(key);
                    }
                }
                list.Sort((a, b) =>
                {
                    return a.CompareTo(b);
                });
                sprite_arr = list.ToArray();
            }
            if (sprite_info_arr?.Length != sprite_arr.Length) 
            {
                sprite_info_arr = new MySpriteInfo[sprite_arr.Length];
            }            
            current_pic_idx = 0;
            //Log.LogInfo($"reset current_pic_idx");
            //
            var count = sprite_info_arr.Length;
            //
            _total_size = Vector2.zero;            
            for (var i=0;i< count; ++i) 
            {
                var info = sprite_info_arr[i] = spritePacker.GetUV(sprite_arr[i]);
                var size = info.size;
                _total_size.x = Mathf.Max(_total_size.x, size.x);
                _total_size.y = Mathf.Max(_total_size.y, size.y);
            }
        }

        void OnInit()
        {
            if (autosize)
            {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _total_size.x);
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _total_size.y);
            }
            if (gameObject.activeInHierarchy)
            {
                UnityEngine.Profiling.Profiler.BeginSample("MySpriteFrameSeq.SetAllDirty");
                SetAllDirty();
                UnityEngine.Profiling.Profiler.EndSample();
            }
        }

        void StartPlay()
        {
            if (!gameObject.activeInHierarchy) 
            {
                return;
            }
            if (sprite_info_arr == null) return;

            lastChangeTime = startTime = UnityEngine.Time.time + delay;
            current_pic_idx = 0;
            hasKeyFrame = false;
            hasComplete = false;
            perFrameTime = time / sprite_info_arr.Length;
            //hasPlayed = !(delay > 0f);
            OnInit();
        }



#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            if (!Application.isPlaying)
            {
                InitSpList(null);
                Editor_FixPackerName();
            }
            canvasRenderer.SetAlpha(alpha);
        }
#endif


        protected override void OnEnable()
        {
            base.OnEnable();
            if (hasComplete && loop != iTween.LoopType.loop)
            {
                OnInit();
                return;
            }

            InitSpList(null);

            StartPlay();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        protected override void OnDidApplyAnimationProperties()
        {
            canvasRenderer.SetAlpha(alpha);
        }

        void LateUpdate()
        {
            if (sprite_info_arr == null) return;
            //
            if (hasComplete && loop != iTween.LoopType.loop) return;
            //
            if (IsPause)
            {
                startTime += UnityEngine.Time.deltaTime;
                lastChangeTime += UnityEngine.Time.deltaTime;
                return;
            }

            if (UnityEngine.Time.time < startTime)
            {
                return;
            }

            if (keyframe > 0f && !hasKeyFrame && (UnityEngine.Time.time - startTime) >= keyframe)
            {
                hasKeyFrame = true;
                SendMessageUpwards("__OnMySpriteSeqKeyFrame", this);
            }

            if ((UnityEngine.Time.time - lastChangeTime) < perFrameTime) return;

            lastChangeTime = UnityEngine.Time.time;


            if ( ++current_pic_idx == sprite_info_arr.Length)
            {
                if (completeEvent && !hasComplete)
                {
                    SendMessageUpwards("__OnMySpriteSeqComplete", this);
                }
                hasComplete = true;
            }
            else
            {
                OnInit();
            }

            //Log.LogInfo($"{lastChangeTime}, {gameObject.name}, current_pic_idx={current_pic_idx}, hasComplete={hasComplete}");

            if (hasComplete)
            {
                if (loop == iTween.LoopType.loop)
                {
                    StartPlay();
                    return;
                }
                else if (loop == iTween.LoopType.pingPong)
                {
                    current_pic_idx = sprite_info_arr.Length - 1;
                    OnInit();
                    return;
                }
                else if (loop == iTween.LoopType.none)
                {
                    current_pic_idx = 0;
                    OnInit();
                    return;
                }
            }

        }

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
                    return UIGrapAssets.m_default_ui_mat;
                }
            }

        }
        public override Texture mainTexture { get { return spritePacker != null ? spritePacker.PackerImage : null; } }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            base.OnPopulateMesh(vh);
            vh.Clear();

            if (sprite_info_arr == null || sprite_info_arr.Length == 0) return;

            if (current_pic_idx >= sprite_info_arr.Length) 
            {
                Log.LogError($"spritePacker={spritePacker.name}, current_pic_idx={current_pic_idx} >= sprite_info_arr={sprite_info_arr.Length}");
                current_pic_idx = 0;
            }

            var info = sprite_info_arr[current_pic_idx];

            var rc = GetPixelAdjustedRect();
            var scale = new Vector2(rc.width / _total_size.x, rc.height / _total_size.y);


            var height = rc.height;
            var width = rc.width;



            var size = info.size;
            size.x *= scale.x;
            size.y *= scale.y;

            var ypos = rc.yMin + (height - size.y) / 2;
            var xpos = rc.xMin + (width - size.x) / 2;
            var pos = new Vector4(xpos, ypos, xpos + size.x, ypos + size.y);

            AddSprite(vh, pos, info.rect, 0);

        }

        void AddSprite(VertexHelper vh, Vector4 pos, Rect rect, int idx)
        {
            //Log.LogInfo($"gif pos={pos}");

            vh.AddVert(new Vector3(pos.x, pos.y, 0), color, new Vector2(rect.xMin, rect.yMin));
            vh.AddVert(new Vector3(pos.x, pos.w, 0), color, new Vector2(rect.xMin, rect.yMax));
            vh.AddVert(new Vector3(pos.z, pos.w, 0), color, new Vector2(rect.xMax, rect.yMax));
            vh.AddVert(new Vector3(pos.z, pos.y, 0), color, new Vector2(rect.xMax, rect.yMin));

            vh.AddTriangle(0 + idx * 4, 1 + idx * 4, 2 + idx * 4);
            vh.AddTriangle(2 + idx * 4, 3 + idx * 4, 0 + idx * 4);
        }




        public void SetSpritePacker(MySpritePacker packer, object dept)
        {
            _dept = dept;
            if (this.spritePacker == packer) 
            {
                return;
            }

            this.spritePacker = packer;
            
            //
            InitSpList(null);
            StartPlay();
        }
    }
}
