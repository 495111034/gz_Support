using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace UnityEngine.UI
{
    /// <summary>
    /// mySprite基本信息
    /// </summary>
    [System.Serializable]
    public class MySpriteInfo
    {
        /// <summary>
        /// 图片名称,将作为索引
        /// </summary>
        public string name;
        /// <summary>
        /// 原始尺寸
        /// </summary>
        public Vector2 size;
        /// <summary>
        /// uv信息
        /// </summary>
        public Rect rect;
        /// <summary>
        /// 中心点 像数
        /// </summary>
        public Vector2 pivot;
        /// <summary>
        /// 边框信息(九宫格)
        /// </summary>
        public Vector4 border;
        /// <summary>
        /// 大图的完整尺寸
        /// </summary>
        public Vector2 mainTextureSize;
        /// <summary>
        /// 填充信息
        /// </summary>
        //public Vector4 padding = Vector4.zero;
        public float pixelsPerUnit = 0f;

        /// <summary>
        /// 完整尺寸
        /// </summary>
        /// <returns></returns>
        public Vector4 GetOuterUV()
        {
            Rect outer = new Rect(rect.x, rect.y, rect.width, rect.height);
            return new Vector4(outer.xMin, outer.yMin, outer.xMax, outer.yMax);
        }

        /// <summary>
        /// 九宫格的内部尺寸
        /// </summary>
        /// <returns></returns>
        public Vector4 GetInnerUV()
        {
            Rect inner = new Rect
                (
                    rect.x + border.x / mainTextureSize.x,
                    rect.y + border.w / mainTextureSize.y,
                    rect.width - border.x / mainTextureSize.x - border.z / mainTextureSize.x,
                    rect.height - border.y / mainTextureSize.y - border.w / mainTextureSize.y
                );
            return new Vector4(inner.xMin, inner.yMin, inner.xMax, inner.yMax);
        }
    }

    /// <summary>
    /// 手动合并图集之后的信息
    /// </summary>
    public class MySpritePacker : MonoBehaviour
    {
        // [HideInInspector]
        [SerializeField]
        public Texture2D PackerImage;

        [SerializeField]
        public string PackerImageMD5;

        [SerializeField]
        private MySpriteInfo[] _uvList;

        private Dictionary<string, MySpriteInfo> _uvDic;
        //
        private Dictionary<string, Sprite> _spDic;

        //bool _spDic_isPlaying = false;

        private void OnDestroy()
        {
            if (Application.isPlaying)
            {
                if (_spDic != null)
                {
                    foreach (var kv in _spDic)
                    {
                        if (kv.Value)
                        {
                            GameObject.Destroy(kv.Value);
                        }
                    }
                }
                _uvDic = null;
                _spDic = null;
            }
        }

        public MySpriteInfo[] uvList => _uvList;

        public MySpriteInfo GetUV(string name)
        {
            if (name == null) 
            {
                Log.LogError($"GetUV name is null");
            }
            if (_uvDic == null || _uvDic.Count == 0) ReSerializePackInfo();
            if (!_uvDic.TryGetValue(name, out var info))
            {
                if (Application.isEditor)
                {
                    Log.LogError($"MySpritePacker=[{this.name}] not found uv=[{name}]\n{new StackTrace(true)}\n\n");
                }
            }
            return info;
        }

        public Sprite GetSprite(string name, GameObject go)
        {
            Sprite sprite = null;
            if ((_spDic == null || _spDic.Count == 0) || (!Application.isPlaying && (!_spDic.TryGetValue(name, out sprite) || !sprite))) ReBuilderSprites();
            if (name == null)
            {
                if (Application.isEditor)
                {
                    Log.LogError($"MySpritePacker=[{this.name}] not found Sprite=NULL, at {go?.GetLocation()}\n{new StackTrace(true)}\n\n");
                }
                else 
                {
                    Log.LogWarning($"MySpritePacker=[{this.name}] not found Sprite=NULL, at {go?.GetLocation()}\n{new StackTrace(true)}\n\n");
                }
            } 
            else if (!sprite && !_spDic.TryGetValue(name, out sprite))
            {
                if (Application.isEditor)
                {
                    Log.LogError($"MySpritePacker=[{this.name}] not found Sprite=[{name}], at {go?.GetLocation()}\n{new StackTrace(true)}\n\n");
                }
                else 
                {
                    Log.LogWarning($"MySpritePacker=[{this.name}] not found Sprite=[{name}], at {go?.GetLocation()}\n{new StackTrace(true)}\n\n");
                }
            }
            else if (!sprite) 
            {
                if (Application.isEditor)
                {
                    Log.LogError($"MySpritePacker=[{this.name}] found Sprite=[{name}] is false, at {go?.GetLocation()}\n{new StackTrace(true)}\n\n");
                }
                else 
                {
                    Log.LogWarning($"MySpritePacker=[{this.name}] found Sprite=[{name}] is false, at {go?.GetLocation()}\n{new StackTrace(true)}\n\n");
                }
            }
            return sprite;
        }

        public void SerializePackInfo(UnityEngine.UI.MySpriteInfo[] packerDic)
        {
            _uvList = packerDic;
            ReSerializePackInfo();
        }
        void ReBuilderSprites()
        {
            //_spDic_isPlaying = Application.isPlaying;

            UnityEngine.Profiling.Profiler.BeginSample("ReBuilderSprites");
            if (_spDic == null)
            {
                _spDic = new Dictionary<string, Sprite>();
            }
            else 
            {
                _spDic.Clear();
            }
            //
            if (_uvDic == null)
            {
                //var t1 = Time.realtimeSinceStartup;
                ReSerializePackInfo();
                //Log.Log2File($"ReSerializePackInfo,{name},cost={(Time.realtimeSinceStartup - t1) * 1000}ms");
            }
            {
                //var t1 = Time.realtimeSinceStartup;
                var PackerImage = this.PackerImage;
                if (!PackerImage)
                {
                    Log.LogError($"图集[{name}]丢失贴图");
                }
                else
                {
                    foreach (var de in _uvDic)
                    {
                        var Value = de.Value;
                        var rect = Value.rect;
                        var mainTextureSize = new Vector2(PackerImage.width, PackerImage.height); //Value.mainTextureSize;
                        var sp_rect = new Rect(Mathf.RoundToInt(rect.x * mainTextureSize.x), Mathf.RoundToInt(rect.y * mainTextureSize.y), Mathf.RoundToInt(rect.width * mainTextureSize.x), Mathf.RoundToInt(rect.height * mainTextureSize.y));
                        var _sp = _spDic[de.Key] = Sprite.Create(PackerImage, sp_rect, Value.pivot / Value.size, Value.pixelsPerUnit, 0, SpriteMeshType.FullRect, Value.border);
                        _sp.name = de.Key;
                    }
                }
                //Log.Log2File($"ReBuilderSprites,{name},cost={(Time.realtimeSinceStartup - t1) * 1000}ms");
            }
            UnityEngine.Profiling.Profiler.EndSample();
        }
        void ReSerializePackInfo()
        {
            UnityEngine.Profiling.Profiler.BeginSample("ReSerializePackInfo");
            if (_uvDic == null)
            {
                _uvDic = new Dictionary<string, MySpriteInfo>();
            }
            else 
            {
                _uvDic.Clear();
            }
            var _uvList = this._uvList;
            if (_uvList != null)
            {
                for (int i = 0, len = _uvList.Length; i < len; ++i)
                {
                    var info = _uvList[i];
                    _uvDic[info.name] = info;
                }
            }
            UnityEngine.Profiling.Profiler.EndSample();
        }
    }
}

