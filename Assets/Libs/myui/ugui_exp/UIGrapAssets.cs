using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityEngine.UI
{
    public static class UIGrapAssets
    {
        static Material _default_ui_mat = null;
        public static Material m_default_ui_mat
        {
            get
            {
                if(!_default_ui_mat)
                {
                    _default_ui_mat = new Material(resource.ShaderManager.Find("MyShaders/UI/Default"));
                    _default_ui_mat.name = "UIDefault";
                    _default_ui_mat.hideFlags = HideFlags.DontSave;
                    _default_ui_mat.enableInstancing = true;
                }
                return _default_ui_mat;
            }
        }

        static Material _default_ui_mat_line_color = null;
        public static Material m_default_ui_mat_line_color
        {
            get
            {
                if (!_default_ui_mat_line_color)
                {
                    _default_ui_mat_line_color = new Material(resource.ShaderManager.Find("MyShaders/UI/Default_Line"));
                    _default_ui_mat_line_color.name = "Default_Line";
                    _default_ui_mat_line_color.hideFlags = HideFlags.DontSave;
                    _default_ui_mat_line_color.enableInstancing = true;
                }
                return _default_ui_mat_line_color;
            }
        }

        static Material _default_bg_mat = null;
        public static Material m_default_bg_mat
        {
            get
            {
                if(!_default_bg_mat)
                {
                    _default_bg_mat = new Material(resource.ShaderManager.Find("MyShaders/UI/Default_bg"));
                    _default_bg_mat.hideFlags = HideFlags.DontSave;
                    _default_bg_mat.enableInstancing = true;
                    _default_bg_mat.name = "UI_Debault_bg";
                }

                return _default_bg_mat;
            }
        }

        static Material _fade_ui_mat = null;
        public static Material m_fade_ui_mat
        {
            get
            {
                if(!_fade_ui_mat)
                {
                    _fade_ui_mat = new Material(resource.ShaderManager.Find("MyShaders/others/ImgFadeAlpha"));
                    _fade_ui_mat.hideFlags = HideFlags.DontSave;
                    _fade_ui_mat.enableInstancing = true;
                    _fade_ui_mat.name = "UI_Img Fade Alpha";
                }

                return _fade_ui_mat;
            }
        }

        static Material _fade_bg_ui_mat = null;
        public static Material m_fade_bg_ui_mat
        {
            get
            {
                if (!_fade_bg_ui_mat)
                {
                    _fade_bg_ui_mat = new Material(resource.ShaderManager.Find("MyShaders/others/ImgFade"));
                    _fade_bg_ui_mat.hideFlags = HideFlags.DontSave;
                    _fade_bg_ui_mat.enableInstancing = true;
                    _fade_bg_ui_mat.name = "UI Image Fade";
                }

                return _fade_bg_ui_mat;
            }
        }

        static Material _ware_ui_mat;
        public static Material m_ware_ui_mat
        {
            get
            {
                if(!_ware_ui_mat)
                {
                    _ware_ui_mat = new Material(resource.ShaderManager.Find("MyShaders/UI/WareAlpha"));
                    _ware_ui_mat.hideFlags = HideFlags.DontSave;
                    _ware_ui_mat.enableInstancing = true;
                }
                return _ware_ui_mat;
            }


        }



        public static void OnApplicationQuit() 
        {
            if (_default_ui_mat) 
            {
                Object.Destroy(_default_ui_mat);
            }

            if (_default_bg_mat)
            {
                Object.Destroy(_default_bg_mat);
            }

            if (_fade_ui_mat)
            {
                Object.Destroy(_fade_ui_mat);
            }

            if (_fade_bg_ui_mat)
            {
                Object.Destroy(_fade_bg_ui_mat);
            }

            if (_ware_ui_mat)
            {
                Object.Destroy(_ware_ui_mat);
            }
        }

    }
}
