using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.EventSystems;

namespace UnityEngine.UI
{
    /// <summary>
    /// Sprite 阵列
    ///     . 拥有一个背景
    ///     . 多个 Sprite 水平排列
    ///     . 可改变透明度
    ///     
    /// 要求所有帧图片都在同一MySpritePacker图集中
    ///     
    /// </summary>
    [AddComponentMenu("UI/Sprite List")]
    [ExecuteInEditMode]
    public class SpriteList : MaskableGraphic, IMySpritePacker
    {
        [HideInInspector]
        [SerializeField]
        float _gap = 0f;

        [HideInInspector]
        [SerializeField]
        string back_img;
        //
        MySpriteInfo back_img_info;
        //[HideInInspector]
        //[SerializeField] string m_SpritePacker_id;

        
        [HideInInspector]
        [SerializeField]
        RectOffset back_ext;
         
        // 透明度
        [Range(0, 1)]
        [HideInInspector]
        [SerializeField]
        float alpha = 1;

        [HideInInspector]
        [SerializeField]
        string[] sprite_arr;
        //
        List<MySpriteInfo> sprite_info_list;

        [HideInInspector]
        [SerializeField]
        MySpritePacker spritePacker;

        object _dept;

        //[HideInInspector]
        //List<string> _sprite_list;

        [HideInInspector]
        Vector2 _total_size;


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
                    Log.LogInfo($"fixname13 {gameObject.GetLocation()} pack:{_packerName} -> {_name}");
                    _packerName = _name;
                    UnityEditor.EditorUtility.SetDirty(transform.root.gameObject);
                }
            }
        }
#endif


        // 间距
        public float gap { get { return _gap; } set { _gap = value; } }

        public float Alpha { get { return alpha; } set { alpha = value; } }
        //public MySpritePacker SpritePacker { get { return spritePacker; } set { spritePacker = value; InitSpList(); } }
        public string BackGroupImageID {get{ return back_img; }set { back_img = value; InitSpList(); } }
        public RectOffset BGImageOffset { get { return back_ext; }set { back_ext = value; InitSpList(); } }

        // 设置 sprite 列表
        public void SetSpriteList(List<string> list)
        {
            //_need_show = true;
            InitSpList(list);
        }

        void InitSpList(List<string> list = null)
        {
            if (spritePacker == null)
            {
                if (list != null)
                {
                    sprite_info_list?.Clear();
                    sprite_arr = list.ToArray();
                }
                return;
            }

            if (list == null && (sprite_arr == null || sprite_arr.Length == 0))
            {
                //保持不变
                return;
            }

            //
            //list 覆盖 sprite_arr
            if (list != null && Application.isPlaying)
            {
                if (Application.isEditor)
                {
                    //调试用 editor 中查看
                    if (sprite_arr != null && sprite_arr.Length == list.Count)
                    {
                        list.CopyTo(sprite_arr);
                    }
                    else
                    {
                        UnityEngine.Profiling.Profiler.BeginSample("for_editor_debug");
                        sprite_arr = list.ToArray(); //会产生gc
                        UnityEngine.Profiling.Profiler.EndSample();
                    }
                }
            }

            _total_size = Vector2.zero;

            if (!string.IsNullOrEmpty(back_img))
            {
                back_img_info = spritePacker.GetUV(back_img);
            }
            else
            {
                back_img_info = null;
            }


            var count = list == null ? sprite_arr.Length : list.Count;
            if (sprite_info_list == null)
            {
                sprite_info_list = new List<MySpriteInfo>(count);
            }
            else
            {
                sprite_info_list.Clear();
                if (sprite_info_list.Capacity < count)
                {
                    sprite_info_list.Capacity = count;
                }
            }


            for (int i = 0; i < count; i++)
            {
                var name = list == null ? sprite_arr[i] : list[i];
                if (string.IsNullOrEmpty(name)) 
                {
                    if (list != null)
                    {
                        Log.LogError($"name is null, i={i}/{count}, list={ string.Join(",", list) }");
                    }else if (sprite_arr != null)
                    {
                        Log.LogError($"name is null, i={i}/{count}, sprite_arr={ string.Join(",", sprite_arr) }");
                    }                    
                }
                var info = spritePacker.GetUV(name);
                if (info == null)
                {
                    continue;
                }
                sprite_info_list.Add(info);
                var size = info.size;
                _total_size.x += size.x;
                _total_size.y = Mathf.Max(_total_size.y, size.y);
            }
            if (count > 0) _total_size.x += (count - 1) * gap;

            OnInit();
        }

        void OnInit()
        {
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _total_size.x);
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _total_size.y);

            if (gameObject.activeInHierarchy)
            {
                UnityEngine.Profiling.Profiler.BeginSample("SpriteList.SetAllDirty");
                SetAllDirty();
                UnityEngine.Profiling.Profiler.EndSample();
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
            InitSpList();
            //_need_show = true;
        }

        public float GetBestSize(int axis)
        {
            return _total_size[axis];
        }

        protected override void OnDidApplyAnimationProperties()
        {
            canvasRenderer.SetAlpha(alpha);
        }


        public override Texture mainTexture { get { return spritePacker != null? spritePacker.PackerImage:null ; } }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            //base.OnPopulateMesh(vh);            
            vh.Clear();
            if (spritePacker == null || sprite_info_list == null || sprite_info_list.Count == 0) return;

            var rc = GetPixelAdjustedRect();
            var scale = new Vector2(rc.width / _total_size.x, rc.height / _total_size.y);

            var xpos = rc.xMin;
            var height = rc.height;
            
            if ( back_img_info != null)
            {
                var pos = new Vector4(rc.xMin - back_ext.left, rc.yMin - back_ext.bottom, rc.xMax + back_ext.right, rc.yMax + back_ext.top);
                var uv = back_img_info.rect;
                AddSprite(vh, pos, uv);
            }

            var length = sprite_info_list.Count;

            for (int i = 0; i < length; i++)
            {
                var info = sprite_info_list[i];

                var size = info.size;
                size.x *= scale.x;
                size.y *= scale.y;

                var ypos = rc.yMin + (height - size.y) / 2;
                var pos = new Vector4(xpos, ypos, xpos + size.x, ypos + size.y);
                
                AddSprite(vh, pos, info.rect);

                xpos = pos.z + gap;
            }
           
        }

        int totalSp
        {
            get
            {
                return (sprite_info_list == null ? 0 : sprite_info_list.Count) + (back_img_info == null ? 0 : 1);
            }
        }

        void AddSprite(VertexHelper vh, Vector4 pos, Rect rect)
        {
            int currentVertCount = vh.currentVertCount;
            vh.AddVert(new Vector3(pos.x, pos.y, 0), color, new Vector2(rect.xMin , rect.yMin));
            vh.AddVert(new Vector3(pos.x, pos.w, 0), color, new Vector2(rect.xMin, rect.yMax));
            vh.AddVert(new Vector3(pos.z, pos.w, 0), color, new Vector2(rect.xMax , rect.yMax));
            vh.AddVert(new Vector3(pos.z, pos.y, 0), color, new Vector2(rect.xMax, rect.yMin));

            
            vh.AddTriangle(currentVertCount, currentVertCount + 1, currentVertCount + 2);
            vh.AddTriangle(currentVertCount + 2, currentVertCount + 3, currentVertCount);
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
