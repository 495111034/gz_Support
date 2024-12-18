using UnityEngine;

namespace UnityEditor
{
    public class InspectorShader_BF_Actor_PBSSkin : ShaderGUI
    {
        private enum RenderMode
        {
            Opaque,
            Transparent,
        }
        private readonly string[] m_RenderModeStrArray =
        {
            "不透明",
            "透明",
        };

        private MaterialEditor m_MaterialEditor;

        //设置
        private MaterialProperty m_RenderMode = null;
        //基础
        private MaterialProperty m_MainTex = null;
        private MaterialProperty m_Color = null;
        private MaterialProperty m_BumpTex = null;
        private MaterialProperty m_MetallicTex = null;
        private MaterialProperty m_MetallicScale = null;
        private MaterialProperty m_GlossScale = null;
        private MaterialProperty m_GiDiffuse = null;
        //剔除
        private MaterialProperty m_ClipOn = null;
        private MaterialProperty m_ClipAlpha = null;
        //镂空
        private MaterialProperty m_HollowOn = null;
        private MaterialProperty m_HollowNumerator = null;
        private MaterialProperty m_HollowDenominator = null;
        //边缘光
        private MaterialProperty m_RimOn = null;
        private MaterialProperty m_RimColor = null;
        private MaterialProperty m_RimPow = null;
        //自发光
        private MaterialProperty m_EmissionOn = null;
        private MaterialProperty m_EmissionColor = null;
        private MaterialProperty m_EmissionSpeed = null;
        private MaterialProperty m_EmissionMinValue = null;
        //消融
        private MaterialProperty m_DissolveOn = null;
        private MaterialProperty m_DissolveColor = null;
        private MaterialProperty m_DissolveEdgeColor = null;
        private MaterialProperty m_DissolveThreshold = null;
        private MaterialProperty m_DissolveColorRatio = null;
        private MaterialProperty m_DissolveEdgeRatio = null;
        //流光
        private MaterialProperty m_FlowLightOn = null;
        private MaterialProperty m_FlowLightTex = null;
        private MaterialProperty m_FlowLightColor = null;
        private MaterialProperty m_FlowLightMaskRange = null;
        private MaterialProperty m_FlowLightXSpeed = null;
        private MaterialProperty m_FlowLightZSpeed = null;


        private MaterialProperty m_CameraLight_IsOpening = null;
        private MaterialProperty m_CameraLightColorScale = null;


        //描边
        //private MaterialProperty m_OutlineOn = null;
        //private MaterialProperty m_OutlineColor = null;
        //private MaterialProperty m_OutlineWidth = null;
        //中配画质
        //private MaterialProperty m_MidSpecularColor = null;
        //private MaterialProperty m_MidGloss = null;
        //private MaterialProperty m_MidSpecularScale = null;
        //private MaterialProperty m_Smooth = null;
        //低配画质
        //private MaterialProperty m_MagicColor = null;

        public void FindProperties(MaterialProperty[] props)
        {
            //设置
            m_RenderMode = FindProperty("_Mode", props);
            //基础
            m_MainTex = FindProperty("_MainTex", props);
            m_Color = FindProperty("_Color", props);
            m_BumpTex = FindProperty("_BumpMap", props);
            m_MetallicTex = FindProperty("_MetallicTex", props);
            m_MetallicScale = FindProperty("_MetallicScale", props);
            m_GlossScale = FindProperty("_GlossScale", props);
            m_GiDiffuse = FindProperty("_GiDiffuse", props);
            //剔除
            m_ClipOn = FindProperty("_ClipOn", props);
            m_ClipAlpha = FindProperty("_ClipAlpha", props);
            //镂空
            m_HollowOn = FindProperty("_HollowOn", props);
            m_HollowNumerator = FindProperty("_HollowNumerator", props);
            m_HollowDenominator = FindProperty("_HollowDenominator", props);
            //边缘光
            m_RimOn = FindProperty("_RimOn", props);
            m_RimColor = FindProperty("_RimColor", props);
            m_RimPow = FindProperty("_RimPow", props);
            //自发光
            m_EmissionOn = FindProperty("_EmissionOn", props);
            m_EmissionColor = FindProperty("_Emission", props);
            m_EmissionSpeed = FindProperty("_EmissionFlashSpeed", props);
            m_EmissionMinValue = FindProperty("_EmissionFlashMinValue", props);
            //消融
            m_DissolveOn = FindProperty("_DissolveOn", props);
            m_DissolveColor = FindProperty("_DissolveColor", props);
            m_DissolveEdgeColor = FindProperty("_DissolveEdgeColor", props);
            m_DissolveThreshold = FindProperty("_DissolveThreshold", props);
            m_DissolveColorRatio = FindProperty("_DissolveColorRatio", props);
            m_DissolveEdgeRatio = FindProperty("_DissolveEdgeRatio", props);
            //流光
            m_FlowLightOn = FindProperty("_FlowLightOn", props);
            m_FlowLightTex = FindProperty("_FlowLightTex", props);
            m_FlowLightColor = FindProperty("_FlowLightColor", props);
            m_FlowLightMaskRange = FindProperty("_FlowLightMaskRange", props);
            m_FlowLightXSpeed = FindProperty("_FlowLightXSpeed", props);
            m_FlowLightZSpeed = FindProperty("_FlowLightZSpeed", props);


            m_CameraLight_IsOpening = FindProperty("_CameraLight_IsOpening", props);
            m_CameraLightColorScale = FindProperty("_CameraLightColorScale", props);

            //描边
            //m_OutlineOn = FindProperty("_OutlineOn", props);
            //m_OutlineColor = FindProperty("_OutlineColor", props);
            //m_OutlineWidth = FindProperty("_OutlineWidth", props);
            //中配画质
            //m_MidSpecularColor = FindProperty("_SpecularColor", props);
            //m_MidGloss = FindProperty("_Gloss", props);
            //m_MidSpecularScale = FindProperty("_SpecularScale", props);
            //m_Smooth = FindProperty("_Smooth", props);
            ////低配画质
            //m_MagicColor = FindProperty("_MagicColor", props);
        }

        #region OnGUI

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            m_MaterialEditor = materialEditor;
            Material mat = materialEditor.target as Material;

            FindProperties(props);

            EditorGUIUtility.labelWidth = 0f;
            EditorGUIUtility.fieldWidth = 64f;
            OnGUISetting(mat);
            OnGUIBase();
            OnGUIClip(mat);
            OnGUIHollow();
            OnGUIRim();
            OnGUIEmission();
            OnGUIDissolve();
            OnGUIFlowLight();
            OnGUIOutline(mat);
            OnGUIMid();
            OnGUILow();
            OnGUIOthers();
        }

        private void OnGUISetting(Material mat)
        {
            EditorGUI.BeginChangeCheck();

            GUILayout.BeginVertical("", GUI.skin.box);
            GUILayout.Label("[设置]", EditorStyles.boldLabel);
            EditorGUI.showMixedValue = m_RenderMode.hasMixedValue;
            RenderMode renderMode = (RenderMode)m_RenderMode.floatValue;
            renderMode = (RenderMode)EditorGUILayout.Popup("渲染模式", (int)renderMode, m_RenderModeStrArray);
            EditorGUI.showMixedValue = false;
            GUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck())
            {
                m_MaterialEditor.RegisterPropertyChangeUndo("RenderMode");
                m_RenderMode.floatValue = (float)renderMode;
                if (renderMode == RenderMode.Transparent)
                {
                    mat.SetOverrideTag("RenderType", "Transparent");
                    mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    mat.SetInt("_BlendSrcColor", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_BlendDstColor", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    //mat.SetInt("_CullMode", (int)UnityEngine.Rendering.CullMode.Off);
                    mat.SetInt("_ZWrite", 0);
                }
                else
                {
                    mat.SetOverrideTag("RenderType", "Opaque");
                    mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry + 10;
                    mat.SetInt("_BlendSrcColor", (int)UnityEngine.Rendering.BlendMode.One);
                    mat.SetInt("_BlendDstColor", (int)UnityEngine.Rendering.BlendMode.Zero);
                    //mat.SetInt("_CullMode", (int)UnityEngine.Rendering.CullMode.Back);
                    mat.SetInt("_ZWrite", 1);
                }
            }
        }

        private void OnGUIBase()
        {
            GUILayout.BeginVertical("", GUI.skin.box);
            GUILayout.Label("[PBS]", EditorStyles.boldLabel);
            m_MaterialEditor.TexturePropertySingleLine(EditorGUIUtility.TrTextContent("主纹理"), m_MainTex);
            m_MaterialEditor.TextureScaleOffsetProperty(m_MainTex);
            m_MaterialEditor.ShaderProperty(m_Color, "主纹理混合颜色");
            m_MaterialEditor.TexturePropertySingleLine(EditorGUIUtility.TrTextContent("法线贴图"), m_BumpTex);
            m_MaterialEditor.TextureScaleOffsetProperty(m_BumpTex);
            m_MaterialEditor.TexturePropertySingleLine(EditorGUIUtility.TrTextContent("金属纹理", "金属度(R),自发光(G),消融噪音(B),光滑度(A)"), m_MetallicTex);
            m_MaterialEditor.TextureScaleOffsetProperty(m_MetallicTex);
            m_MaterialEditor.ShaderProperty(m_MetallicScale, "金属度");
            m_MaterialEditor.ShaderProperty(m_GlossScale, "光滑度");
            m_MaterialEditor.ShaderProperty(m_GiDiffuse, "天光比例");

            m_MaterialEditor.ShaderProperty(m_CameraLight_IsOpening, "开启镜头光");
            m_MaterialEditor.ShaderProperty(m_CameraLightColorScale, "镜头光参数");

            GUILayout.EndVertical();
        }

        private void OnGUIClip(Material mat)
        {
            GUILayout.BeginVertical("", GUI.skin.box);
            RenderMode renderMode = (RenderMode)m_RenderMode.floatValue;
            m_MaterialEditor.ShaderProperty(m_ClipOn, EditorGUIUtility.TrTextContent("[剔除]"));
            if (m_ClipOn.floatValue != 0f)
            {
                m_MaterialEditor.ShaderProperty(m_ClipAlpha, "剔除Alpha阈值");
                if (renderMode == RenderMode.Opaque)
                {
                    mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                }
            }
            else
            {
                if (renderMode == RenderMode.Opaque)
                {
                    mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry + 10;
                }
            }
            GUILayout.EndVertical();
        }

        private void OnGUIHollow()
        {
            GUILayout.BeginVertical("", GUI.skin.box);
            m_MaterialEditor.ShaderProperty(m_HollowOn, EditorGUIUtility.TrTextContent("[镂空]"));
            if (m_HollowOn.floatValue != 0f)
            {
                m_MaterialEditor.ShaderProperty(m_HollowDenominator, "镂空分母");
                m_MaterialEditor.ShaderProperty(m_HollowNumerator, "镂空分子");
            }
            GUILayout.EndVertical();
        }

        private void OnGUIRim()
        {
            GUILayout.BeginVertical("", GUI.skin.box);
            m_MaterialEditor.ShaderProperty(m_RimOn, "[边缘光]");
            if (m_RimOn.floatValue != 0f)
            {
                m_MaterialEditor.ShaderProperty(m_RimColor, "边缘光颜色");
                m_MaterialEditor.ShaderProperty(m_RimPow, "边缘光强度");
            }
            GUILayout.EndVertical();
        }

        private void OnGUIEmission()
        {
            GUILayout.BeginVertical("", GUI.skin.box);
            m_MaterialEditor.ShaderProperty(m_EmissionOn, "[自发光]");
            if (m_EmissionOn.floatValue != 0f)
            {
                m_MaterialEditor.ShaderProperty(m_EmissionColor, "自发光颜色");
                m_MaterialEditor.ShaderProperty(m_EmissionSpeed, "自发光闪烁速度");
                m_MaterialEditor.ShaderProperty(m_EmissionMinValue, "自发光闪烁最小值");
            }
            GUILayout.EndVertical();
        }

        private void OnGUIDissolve()
        {
            GUILayout.BeginVertical("", GUI.skin.box);
            m_MaterialEditor.ShaderProperty(m_DissolveOn, "[消融]");
            if (m_DissolveOn.floatValue != 0f)
            {
                m_MaterialEditor.ShaderProperty(m_DissolveColor, "消融颜色");
                m_MaterialEditor.ShaderProperty(m_DissolveEdgeColor, "消融边缘颜色");
                m_MaterialEditor.ShaderProperty(m_DissolveThreshold, "消融阈值");
                m_MaterialEditor.ShaderProperty(m_DissolveColorRatio, "消融颜色总占比(对整个角色占比)");
                m_MaterialEditor.ShaderProperty(m_DissolveEdgeRatio, "消融边缘颜色占比(对消融颜色占比)");
            }
            GUILayout.EndVertical();
        }

        private void OnGUIFlowLight()
        {
            GUILayout.BeginVertical("", GUI.skin.box);
            m_MaterialEditor.ShaderProperty(m_FlowLightOn, "[流光]");
            if (m_FlowLightOn.floatValue != 0f)
            {
                m_MaterialEditor.TexturePropertySingleLine(EditorGUIUtility.TrTextContent("流光贴图"), m_FlowLightTex);
                //m_MaterialEditor.TextureScaleOffsetProperty(m_FlowLightTex);
                m_MaterialEditor.ShaderProperty(m_FlowLightColor, "流光颜色");
                m_MaterialEditor.ShaderProperty(m_FlowLightMaskRange, "流光遮罩范围");
                m_MaterialEditor.ShaderProperty(m_FlowLightXSpeed, "流光X方向速度");
                m_MaterialEditor.ShaderProperty(m_FlowLightZSpeed, "流光Z方向速度");
            }
            GUILayout.EndVertical();
        }

        //PS:Unity2018不支持通过SetShaderPassEnabled(PassName)来启用或禁用Pass，需要2019以后版本
        private void OnGUIOutline(Material mat)
        {
            //GUILayout.BeginVertical("", GUI.skin.box);
            //m_MaterialEditor.ShaderProperty(m_OutlineOn, "[描边]");
            //if (m_OutlineOn.floatValue != 0f)
            //{
            //    if (!mat.GetShaderPassEnabled("Outline"))
            //    {
            //        mat.SetShaderPassEnabled("Outline", true);
            //    }
            //    m_MaterialEditor.ShaderProperty(m_OutlineColor, "描边颜色");
            //    m_MaterialEditor.ShaderProperty(m_OutlineWidth, "描边宽度");
            //}
            //else if (mat.GetShaderPassEnabled("Outline"))
            //{
            //    mat.SetShaderPassEnabled("Outline", false);
            //}
            //GUILayout.EndVertical();
        }

        private void OnGUIOthers()
        {
            GUILayout.BeginVertical("", GUI.skin.box);
            GUILayout.Label("[其它]", EditorStyles.boldLabel);
            m_MaterialEditor.RenderQueueField();
            m_MaterialEditor.EnableInstancingField();
            GUILayout.EndVertical();
        }
        private void OnGUIMid()
        {
            //GUILayout.BeginVertical("", GUI.skin.box);
            //GUILayout.Label("[中配画质]（镂空效果可调）", EditorStyles.boldLabel);
            //m_MaterialEditor.ShaderProperty(m_MidSpecularColor, "高光颜色");
            //m_MaterialEditor.ShaderProperty(m_MidGloss, "高光范围");
            //m_MaterialEditor.ShaderProperty(m_MidSpecularScale, "高光倍数");
            //m_MaterialEditor.ShaderProperty(m_Smooth, "光滑度");
            //GUILayout.EndVertical();
        }
        private void OnGUILow()
        {
            //GUILayout.BeginVertical("", GUI.skin.box);
            //GUILayout.Label("[低配画质]", EditorStyles.boldLabel);
            //m_MaterialEditor.ShaderProperty(m_MagicColor, "低配颜色");
            //GUILayout.EndVertical();
        }
        #endregion

    }
}
