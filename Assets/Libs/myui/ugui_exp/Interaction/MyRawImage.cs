using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityEngine.UI
{
    [AddComponentMenu("UI/MyRawImage", 0)]
    public class MyRawImage : RawImage, IMyTexture
    {        
        object _dept;

        [HideInInspector] [SerializeField]
        private bool _fade = false;


        [HideInInspector]
        [SerializeField]
        string _textureName;

        public string TextureName => _textureName;
        public Texture iTexture => texture; 

#if UNITY_EDITOR
        public void Editor_FixTextureName() 
        {
            var texture = this.texture;
            if (!MySpriteImageBase.Editor_CheckIsMissing(this, ref texture))
            {
                var _name = texture ? texture.name : "";
                if (_textureName != _name)
                {
                    Log.LogInfo($"fixname7 {gameObject.GetLocation()} tex:{_textureName} -> {_name}");
                    _textureName = _name;
                    UnityEditor.EditorUtility.SetDirty(transform.root.gameObject);
                }
            }
            this.texture = texture;
        }

        protected override void OnValidate()
        {
            if (!Application.isPlaying) 
            {
                Editor_FixTextureName();
                if (texture)
                {
                    var path = UnityEditor.AssetDatabase.GetAssetPath(texture).ToLower();
                    var importer = UnityEditor.AssetImporter.GetAtPath(path) as UnityEditor.TextureImporter;
                    if (importer.textureType == UnityEditor.TextureImporterType.Sprite && !PathDefs.IsAssetsResources(path))
                    {
                        //texture = null;
                        Log.LogError($"[单图]{this.GetType().Name}:{gameObject.GetLocation()},{path}, 单图不能是Sprite格式");
                    }
                }
            }
        }
#endif

        /// <summary>
        /// 灰阶显示
        /// </summary>
        public bool IsFade { get { return _fade; } set { _fade = value; } }

        public void SetTexture(Texture tex, object dept)
        {
            texture = tex;
            _dept = dept;
            if (gameObject.activeInHierarchy)
            {
                UnityEngine.Profiling.Profiler.BeginSample("MyRawImage.SetAllDirty");
                SetAllDirty();
                UnityEngine.Profiling.Profiler.EndSample();
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

        protected override void OnDestroy()
        {
            MySpriteImageBase.ClearResource(this, texture);
            base.OnDestroy();
        }
    }
}
