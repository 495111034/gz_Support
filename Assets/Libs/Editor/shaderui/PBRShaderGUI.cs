using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UnityEditor
{
    public class PBRShaderGUI : ShaderGUI
    {

        //const string KEY_OPEN_SHADER_DEBUG = "OPEN_SHADER_DEBUG";
        const string KEY_USE_SPECIAL_RIM_COLOR = "USE_SPECIAL_RIM_COLOR";
        const string KEY_ALPHA_TEST = "ALPHA_TEST";
        const string KEY_ALPHA_PREMULT = "ALPHA_PREMULT";
        //const string KEY_SELF_COLOR = "SELF_TRIPLE_COLOR";



        List<string> BlendModenameList = new List<string>()
        {
            "不透明",
            "透明度测试",
            "透明度测试并混合",
            "透明度混合"
        };

        internal enum BlendMode
        {
            Opaque,
            Cutout,
            CutoutTransparent,
            Transparent
        }

        internal enum AlphaMode
        {
            None = 1 << 0,
            AlphaTest = 1 << 1,
            AlphaPrume = 1 << 2
        }

        Material material;
        //MaterialProperty matDebugMode;
        MaterialProperty matRimColor;
       // MaterialProperty matDebugColor;

        bool initial = false;
        //bool open_debug = true;
        bool use_special_rim = true;
        //DebugMode debugMode = DebugMode.None;
        BlendMode blendMode = BlendMode.Opaque;
       // bool useSelfColor = false;
        Color rimColor = Color.blue;
        // Color debugColor = Color.white;

        float _Cutoff = 0.5f;

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            base.OnGUI(materialEditor, props);
            material = materialEditor.target as Material;
            if (!initial)
            {
                FindProperties(props);
                initial = true;
            }
            ShaderDebugGUI();
        }


        private void ShaderDebugGUI()
        {
            EditorGUI.BeginChangeCheck();

            use_special_rim = EditorGUILayout.Toggle("自定义菲涅尔颜色", use_special_rim);
            if (use_special_rim) rimColor = EditorGUILayout.ColorField(new GUIContent("菲涅尔颜色"), rimColor,true,false,true, new GUILayoutOption[0]);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("混合模式");
            blendMode =(BlendMode) Mathf.Clamp((int)blendMode, 0, BlendModenameList.Count - 1);
            blendMode =  (BlendMode)EditorGUILayout.Popup((int)blendMode, BlendModenameList.ToArray());
            //blendMode = (BlendMode)EditorGUILayout.Popup("混合模式", (int)blendMode, Enum.GetNames(typeof(BlendMode)));
            EditorGUILayout.EndHorizontal();

            if(blendMode == BlendMode.Cutout)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.LabelField("透明度阀值");
                _Cutoff = EditorGUILayout.Slider(_Cutoff, 0, 1, new GUILayoutOption[0]);
                --EditorGUI.indentLevel;
            }

           // useSelfColor = EditorGUILayout.Toggle("自定义调色", useSelfColor);

          
            if (EditorGUI.EndChangeCheck())
            {
                SetMatBlend(blendMode);
                //EnableMatKeyword(KEY_OPEN_SHADER_DEBUG, false);
                EnableMatKeyword(KEY_USE_SPECIAL_RIM_COLOR, use_special_rim);
               // EnableMatKeyword(KEY_SELF_COLOR, useSelfColor);
                if (use_special_rim) matRimColor.colorValue = rimColor;
                
            }
        }


        private void FindProperties(MaterialProperty[] props)
        {
            Shader shader = material.shader;
            //matDebugMode = FindProperty("_DebugMode", props, false);
            matRimColor = FindProperty("_FresnelColor", props, false);
           // matDebugColor = FindProperty("_DebugColor", props, false);
            string rendertype = material.GetTag("RenderType", true);
            if (matRimColor != null)
            {
                rimColor = matRimColor.colorValue;
            }
            //if (matDebugMode != null)
            //{
            //    debugMode = (DebugMode)(matDebugMode.floatValue);
            //}
            if (rendertype.Equals("TransparentCutout"))
            {
                blendMode = BlendMode.CutoutTransparent;
            }
            else if (rendertype.Equals("Transparent"))
            {
                blendMode = BlendMode.Transparent;
            }
            else if (material.renderQueue == (int)UnityEngine.Rendering.RenderQueue.Geometry)
            {
                blendMode = BlendMode.Opaque;
            }
            else
            {
                blendMode = BlendMode.Cutout;
            }
        }

        private void SetMatBlend(BlendMode mode)
        {
            switch (mode)
            {
                case BlendMode.Opaque:
                    SetMatBlend(UnityEngine.Rendering.BlendMode.One, UnityEngine.Rendering.BlendMode.Zero, AlphaMode.None, 1);
                    material.SetOverrideTag("RenderType", "Opaque");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
                    break;
                case BlendMode.Cutout:
                    SetMatBlend(UnityEngine.Rendering.BlendMode.One, UnityEngine.Rendering.BlendMode.Zero, AlphaMode.AlphaTest, 1);
                    material.SetOverrideTag("RenderType", "Opaque");
                    material.SetFloat("_Cutoff", _Cutoff);
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                    break;
                case BlendMode.CutoutTransparent:
                    SetMatBlend(UnityEngine.Rendering.BlendMode.One, UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha, AlphaMode.AlphaTest | AlphaMode.AlphaPrume, 1);
                    material.SetOverrideTag("RenderType", "TransparentCutout");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
                case BlendMode.Transparent:
                    SetMatBlend(UnityEngine.Rendering.BlendMode.One, UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha, AlphaMode.AlphaPrume, 0);
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
                default:
                    //TO-DO 
                    break;
            }
        }

        private void SetMatBlend(UnityEngine.Rendering.BlendMode src, UnityEngine.Rendering.BlendMode dst, AlphaMode alp, int zwrite)
        {
            if (material.HasProperty("_SrcBlend"))
                material.SetInt("_SrcBlend", (int)src);
            if (material.HasProperty("_DstBlend"))
                material.SetInt("_DstBlend", (int)dst);
            if (material.HasProperty("_ZWrite"))
                material.SetInt("_ZWrite", zwrite);

            EnableMatKeyword(KEY_ALPHA_TEST, (alp & AlphaMode.AlphaTest) != 0);
            EnableMatKeyword(KEY_ALPHA_PREMULT, (alp & AlphaMode.AlphaPrume) != 0);
        }


        private void EnableMatKeyword(string key, bool enable)
        {
            if (enable)
            {
                material.EnableKeyword(key);
            }
            else
            {
                material.DisableKeyword(key);
            }
        }
    }
}