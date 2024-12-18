/*
* Copyright (C) 2016, BingFeng Studio（冰峰工作室）.
* All rights reserved.
* 
* 文件名称：InspectorShader_BF_Scene_PBSNormalDiffuse
* 创建标识：引擎组
* 创建日期：2020/11/6
* 文件简述：
*/

using UnityEngine;

namespace UnityEditor
{
    public class InspectorShader_BF_Scene_PBSNormalDiffuse : ShaderGUI
    {
        private MaterialEditor m_MaterialEditor;

        private MaterialProperty m_SrcBlend = null;
        private MaterialProperty m_DstBlend = null;
        private MaterialProperty m_ZWrite = null;

        private MaterialProperty m_MainTex = null;
        private MaterialProperty m_MainTexAlphaType = null;
        private MaterialProperty m_Color = null;
        private MaterialProperty m_BumpMap = null;
        private MaterialProperty m_BumpMapScale = null;
        private MaterialProperty m_MetallicTex = null;
        private MaterialProperty m_MetallicScale = null;
        private MaterialProperty m_GlossScale = null;
        private MaterialProperty m_Specular = null;

        //扩展贴图
        private MaterialProperty m_HasExtendMap = null;
        private MaterialProperty m_ExtendMap = null;

        //透明度混合
        private MaterialProperty m_Blend_ON = null;

        //剔除
        private MaterialProperty m_ClipOn = null;
        private MaterialProperty m_ClipAlpha = null;

        //镂空
        private MaterialProperty m_HollowOn = null;
        private MaterialProperty m_HollowRate = null;

        //自发光
        private MaterialProperty m_EmissionOn = null;
        private MaterialProperty m_Emission = null;
        private MaterialProperty m_EmissionLow = null;
        private MaterialProperty m_EmissionMapOn = null;
        private MaterialProperty m_EmissionMap = null;
        private MaterialProperty m_EmissionStart = null;
        private MaterialProperty m_EmissionSpeed = null;

        //反射开关
        private MaterialProperty m_ReflectOn = null;
        private MaterialProperty m_ReflectMetallic = null;
        private MaterialProperty m_ReflectSmoothness = null;

        //镜面反射
        private MaterialProperty m_MirrorReflectionOn = null;
        private MaterialProperty m_MirrorReflectionTex = null;
        private MaterialProperty m_MirrorNormal = null;
        private MaterialProperty m_MirrorReflectionScale = null;
        private MaterialProperty m_FresnelExp = null;

        //天光开关
        private MaterialProperty m_GiDiffuse = null;

        //镜头光
        private MaterialProperty m_CameraLight_IsOpening = null;
        private MaterialProperty m_CameraLightColorScale = null;
        private MaterialProperty m_CameraLightTrans = null;
        private MaterialProperty m_CameraLightMin = null;
        private MaterialProperty m_CameraLightOffset = null;

        //fade
        private MaterialProperty m_FadeON = null;
        private MaterialProperty m_FadeDistanceNear = null;
        private MaterialProperty m_FadeDistanceFar = null;

        //覆盖功能
        private MaterialProperty m_isCover_on = null;
        private MaterialProperty m_CoverMode = null;
        private MaterialProperty m_CoverColor = null;
        private MaterialProperty m_CoverTex = null;
        private MaterialProperty m_isUseConverBumpTex = null;
        private MaterialProperty m_CoverBumpTex = null;
        private MaterialProperty m_CoverBumpMapScale = null;
        private MaterialProperty m_CoverAmount = null;
        private MaterialProperty m_CoverFade = null;
        private MaterialProperty m_CoverSmoothnessScale = null;

        //冰石功能
        private MaterialProperty m_IsICEOn = null;
        private MaterialProperty m_UseThicknessMask = null;
        private MaterialProperty m_SSSColor = null;
        private MaterialProperty m_Translucency = null;
        private MaterialProperty m_TransScattering = null;
        private MaterialProperty m_TransShadow = null;
        private MaterialProperty m_BackTransNormalDistortion = null;
        private MaterialProperty m_FrontTransNormalDistortion = null;
        private MaterialProperty m_FrontTransIntensity = null;

        private MaterialProperty m_UseRimThicknessMask = null;
        private MaterialProperty m_RimIceColor = null;
        private MaterialProperty m_RimBase = null;
        private MaterialProperty m_RimPower = null;
        private MaterialProperty m_RimIntensity = null;

        private MaterialProperty m_IsDustOn = null;
        private MaterialProperty m_DustMap = null;
        private MaterialProperty m_DustColor = null;
        private MaterialProperty m_DustNoiseUVScale = null;
        private MaterialProperty m_DustNoiseIntensity = null;
        private MaterialProperty m_DustDepthShift = null;
        private MaterialProperty m_DustUVScale = null;

        private MaterialProperty m_UseRealtimeShadowToggle = null;

        public void FindProperties(MaterialProperty[] props)
        {
            m_SrcBlend = FindProperty("_SrcBlend", props);
            m_DstBlend = FindProperty("_DstBlend", props);
            m_ZWrite = FindProperty("_ZWrite", props);

            m_MainTex = FindProperty("_MainTex", props);
            m_MainTexAlphaType = FindProperty("_MainTexAlphaType", props);
            m_Color = FindProperty("_Color", props);
            m_BumpMap = FindProperty("_BumpMap", props);
            m_BumpMapScale = FindProperty("_BumpMapScale", props);
            m_MetallicTex = FindProperty("_MetallicTex", props);
            m_MetallicScale = FindProperty("_MetallicScale", props);
            m_GlossScale = FindProperty("_GlossScale", props);
            m_Specular = FindProperty("_Specular", props);

            m_HasExtendMap = FindProperty("_HasExtendMap", props);
            m_ExtendMap = FindProperty("_ExtendMap", props);

            m_Blend_ON = FindProperty("_Blend_ON", props);

            m_ClipOn = FindProperty("_ClipOn", props);
            m_ClipAlpha = FindProperty("_ClipAlpha", props);

            m_HollowOn = FindProperty("_HollowOn", props);
            m_HollowRate = FindProperty("_HollowRate", props);

            m_EmissionOn = FindProperty("_EmissionOn", props);
            m_Emission = FindProperty("_Emission", props);
            m_EmissionLow = FindProperty("_EmissionLowQuility", props);
            m_EmissionMapOn = FindProperty("_EmissionMapOn", props);
            m_EmissionMap = FindProperty("_EmissionMap", props);
            m_EmissionStart = FindProperty("_EmissionStart", props);
            m_EmissionSpeed = FindProperty("_EmissionSpeed", props);

            m_ReflectOn = FindProperty("_ReflectOn", props);
            m_ReflectMetallic = FindProperty("_ReflectMetallic", props);
            m_ReflectSmoothness = FindProperty("_ReflectSmoothness", props);

            m_MirrorReflectionOn = FindProperty("_MirrorReflectionOn", props);
            m_MirrorReflectionTex = FindProperty("_MirrorReflectionTex", props);
            m_MirrorNormal = FindProperty("_MirrorNormal", props);
            m_MirrorReflectionScale = FindProperty("_MirrorReflectionScale", props);
            m_FresnelExp = FindProperty("_FresnelExp", props);

            m_GiDiffuse = FindProperty("_GiDiffuse", props);

            m_CameraLight_IsOpening = FindProperty("_CameraLight_IsOpening", props);
            m_CameraLightColorScale = FindProperty("_CameraLightColorScale", props);
            m_CameraLightTrans = FindProperty("_CameraLightTrans", props);
            m_CameraLightMin = FindProperty("_CameraLightMin", props);
            m_CameraLightOffset = FindProperty("_CameraLightOffset", props);

            m_FadeON = FindProperty("_FadeON", props);
            m_FadeDistanceNear = FindProperty("_FadeDistanceNear", props);
            m_FadeDistanceFar = FindProperty("_FadeDistanceFar", props);

            m_isCover_on = FindProperty("_isCover_on", props);
            m_CoverMode = FindProperty("_CoverMode", props);
            m_CoverColor = FindProperty("_CoverColor", props);
            m_CoverTex = FindProperty("_CoverTex", props);
            m_isUseConverBumpTex = FindProperty("_isUseConverBumpTex", props);
            m_CoverBumpTex = FindProperty("_CoverBumpTex", props);
            m_CoverBumpMapScale = FindProperty("_CoverBumpMapScale", props);
            m_CoverAmount = FindProperty("_CoverAmount", props);
            m_CoverFade = FindProperty("_CoverFade", props);
            m_CoverSmoothnessScale = FindProperty("_CoverSmoothnessScale", props);

            m_IsICEOn = FindProperty("_IsICEOn", props);
            m_UseThicknessMask = FindProperty("_UseThicknessMask", props);
            m_SSSColor = FindProperty("_SSSColor", props);
            m_Translucency = FindProperty("_Translucency", props);
            m_TransScattering = FindProperty("_TransScattering", props);
            m_TransShadow = FindProperty("_TransShadow", props);
            m_BackTransNormalDistortion = FindProperty("_BackTransNormalDistortion", props);
            m_FrontTransNormalDistortion = FindProperty("_FrontTransNormalDistortion", props);
            m_FrontTransIntensity = FindProperty("_FrontTransIntensity", props);

            m_UseRimThicknessMask = FindProperty("_UseRimThicknessMask", props);
            m_RimIceColor = FindProperty("_RimIceColor", props);
            m_RimBase = FindProperty("_RimBase", props);
            m_RimPower = FindProperty("_RimPower", props);
            m_RimIntensity = FindProperty("_RimIntensity", props);

            m_IsDustOn = FindProperty("_IsDustOn", props);
            m_DustMap = FindProperty("_DustMap", props);
            m_DustColor = FindProperty("_DustColor", props);
            m_DustNoiseUVScale = FindProperty("_DustNoiseUVScale", props);
            m_DustNoiseIntensity = FindProperty("_DustNoiseIntensity", props);
            m_DustDepthShift = FindProperty("_DustDepthShift", props);
            m_DustUVScale = FindProperty("_DustUVScale", props);

            m_UseRealtimeShadowToggle = FindProperty("_UseRealtimeShadow", props);
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            m_MaterialEditor = materialEditor;
            Material material = materialEditor.target as Material;

            FindProperties(props);
            ShaderPropertiesGUI(material);
        }

        public void ShaderPropertiesGUI(Material material)
        {
            EditorGUIUtility.labelWidth = 0f;
            EditorGUIUtility.fieldWidth = 64f;

            EditorGUI.BeginChangeCheck();
            {
                m_MaterialEditor.ShaderProperty(m_SrcBlend, "源混合模式");
                m_MaterialEditor.ShaderProperty(m_DstBlend, "目标混合模式");
                m_MaterialEditor.ShaderProperty(m_ZWrite, "_ZWrite");

                GUILayout.BeginVertical("", Styles.moduleHeader);
                m_MaterialEditor.ShaderProperty(m_MainTex, "主纹理");
                m_MaterialEditor.ShaderProperty(m_MainTexAlphaType, "主纹理A通道类型");
                m_MaterialEditor.ShaderProperty(m_Color, "混合颜色");
                m_MaterialEditor.ShaderProperty(m_BumpMap, "法线贴图");
                m_MaterialEditor.ShaderProperty(m_BumpMapScale, "法线增强");
                m_MaterialEditor.ShaderProperty(m_MetallicTex , "金属高光纹理,R:金属度|G:自发光|B:反射遮罩|A:光滑度");
                m_MaterialEditor.ShaderProperty(m_MetallicScale, "金属度");
                m_MaterialEditor.ShaderProperty(m_GlossScale, "光滑度");
                m_MaterialEditor.ShaderProperty(m_Specular, "高光颜色");
                GUILayout.EndVertical();

                //扩展贴图
                GUILayout.BeginVertical("", Styles.moduleHeader);
                m_MaterialEditor.ShaderProperty(m_HasExtendMap, "使用扩展贴图");
                if (m_HasExtendMap.floatValue > 0)
                {
                    m_MaterialEditor.ShaderProperty(m_ExtendMap, "扩展贴图(R:未定义|G:厚薄度|B:Rim掩码| A:未定义)");
                }
                GUILayout.EndVertical();

                //透明度混合
                GUILayout.BeginVertical("", Styles.moduleHeader);
                EditorGUI.BeginChangeCheck();
                m_MaterialEditor.ShaderProperty(m_Blend_ON, "开启透明度混合");
                if (EditorGUI.EndChangeCheck())
                {
                    if (m_Blend_ON.floatValue > 0)
                    {
                        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                        m_SrcBlend.floatValue = (int)UnityEngine.Rendering.BlendMode.SrcAlpha;
                        m_DstBlend.floatValue = (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha;
                        m_ZWrite.floatValue = 0;
                    }
                    else
                    {
                        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
                        m_SrcBlend.floatValue = (int)UnityEngine.Rendering.BlendMode.One;
                        m_DstBlend.floatValue = (int)UnityEngine.Rendering.BlendMode.Zero;
                        m_ZWrite.floatValue = 1;
                    }
                }
                GUILayout.EndVertical();

                //剔除
                GUILayout.BeginVertical("", Styles.moduleHeader);
                EditorGUI.BeginChangeCheck();
                m_MaterialEditor.ShaderProperty(m_ClipOn, "开启剔除");
                if (EditorGUI.EndChangeCheck())
                {
                    if (m_Blend_ON.floatValue <= 0)
                    {
                        if (m_ClipOn.floatValue > 0)
                        {
                            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                        }
                        else
                        {
                            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
                        }
                    }
                  
                }
                if (m_ClipOn.floatValue > 0)
                {
                    m_MaterialEditor.ShaderProperty(m_ClipAlpha, "剔除Alpha阈值");
                }
                GUILayout.EndVertical();

                //镂空
                GUILayout.BeginVertical("", Styles.moduleHeader);
                EditorGUI.BeginChangeCheck();
                m_MaterialEditor.ShaderProperty(m_HollowOn, "开启镂空");
                if (EditorGUI.EndChangeCheck())
                {
                    if (m_Blend_ON.floatValue <= 0)
                    {
                        if (m_HollowOn.floatValue > 0)
                        {
                            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                        }
                        else
                        {
                            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Geometry;
                        }
                    }
                }
                if (m_HollowOn.floatValue > 0)
                {
                    m_MaterialEditor.ShaderProperty(m_HollowRate, "镂空率");
                }
                GUILayout.EndVertical();

                //自发光
                GUILayout.BeginVertical("", Styles.moduleHeader);
                m_MaterialEditor.ShaderProperty(m_EmissionOn, "开启自发光");
                if (m_EmissionOn.floatValue > 0)
                {
                    m_MaterialEditor.ShaderProperty(m_Emission, "自发光颜色");
                    m_MaterialEditor.ShaderProperty(m_EmissionLow, "低配模式下自发光颜色");
                    m_MaterialEditor.ShaderProperty(m_EmissionMapOn, "开启自发光贴图");
                    if(m_EmissionMapOn.floatValue > 0)
                    {
                        m_MaterialEditor.ShaderProperty(m_EmissionMap, "自发光贴图");
                    }
                    m_MaterialEditor.ShaderProperty(m_EmissionStart, "自发光基础值");
                    m_MaterialEditor.ShaderProperty(m_EmissionSpeed, "自发光闪烁速度");
                }
                GUILayout.EndVertical();

                //反射开关
                GUILayout.BeginVertical("", Styles.moduleHeader);
                m_MaterialEditor.ShaderProperty(m_ReflectOn, "开启反射");
                if (m_ReflectOn.floatValue > 0)
                {
                    m_MaterialEditor.ShaderProperty(m_ReflectMetallic, "反射面金属度");
                    m_MaterialEditor.ShaderProperty(m_ReflectSmoothness, "反射面光滑度");
                }
                GUILayout.EndVertical();


                //镜面反射
                GUILayout.BeginVertical("", Styles.moduleHeader);
                m_MaterialEditor.ShaderProperty(m_MirrorReflectionOn, "开启镜面反射");
                if (m_MirrorReflectionOn.floatValue > 0)
                {
                    m_MaterialEditor.ShaderProperty(m_MirrorReflectionTex, "镜面反射贴图");
                    m_MaterialEditor.ShaderProperty(m_MirrorNormal, "镜面法线");
                    m_MaterialEditor.ShaderProperty(m_MirrorReflectionScale, "镜面反射强度");
                    m_MaterialEditor.ShaderProperty(m_FresnelExp, "菲尼尔指数");
                }
                GUILayout.EndVertical();

                //天光开关
                GUILayout.BeginVertical("", Styles.moduleHeader);
                m_MaterialEditor.ShaderProperty(m_GiDiffuse, "天光开关");
                GUILayout.EndVertical();

                //镜头光
                GUILayout.BeginVertical("", Styles.moduleHeader);
                m_MaterialEditor.ShaderProperty(m_CameraLight_IsOpening, "开启镜头光");
                if (m_CameraLight_IsOpening.floatValue > 0)
                {
                    m_MaterialEditor.ShaderProperty(m_CameraLightColorScale, "镜头光的强度控制");
                    m_MaterialEditor.ShaderProperty(m_CameraLightTrans, "镜头光变换");
                    m_MaterialEditor.ShaderProperty(m_CameraLightMin, "镜头光阀值");
                    m_MaterialEditor.ShaderProperty(m_CameraLightOffset, "镜头光偏移");
                }
                GUILayout.EndVertical();

                //fade
                GUILayout.BeginVertical("", Styles.moduleHeader);
                m_MaterialEditor.ShaderProperty(m_FadeON, "Open Fade");
                if (m_FadeON.floatValue > 0)
                {
                    m_MaterialEditor.ShaderProperty(m_FadeDistanceNear, "FadeDistanceNear");
                    m_MaterialEditor.ShaderProperty(m_FadeDistanceFar, "FadeDistanceFar");
                }
                GUILayout.EndVertical();

                //覆盖功能
                GUILayout.BeginVertical("", Styles.moduleHeader);
                m_MaterialEditor.ShaderProperty(m_isCover_on, "开启覆盖功能");
                if (m_isCover_on.floatValue > 0)
                {
                    m_MaterialEditor.ShaderProperty(m_CoverMode, "覆盖模式");
                    m_MaterialEditor.ShaderProperty(m_CoverColor, "覆盖颜色");
                    m_MaterialEditor.ShaderProperty(m_CoverTex, "覆盖纹理");
                    m_MaterialEditor.ShaderProperty(m_isUseConverBumpTex, "开启覆盖法线");
                    if (m_isUseConverBumpTex.floatValue > 0)
                    {
                        m_MaterialEditor.ShaderProperty(m_CoverBumpTex, "覆盖法线");
                        m_MaterialEditor.ShaderProperty(m_CoverBumpMapScale, "覆盖法线增强");

                    }
                    m_MaterialEditor.ShaderProperty(m_CoverAmount, "Cover Amount");
                    m_MaterialEditor.ShaderProperty(m_CoverFade, "Cover Fade");
                    m_MaterialEditor.ShaderProperty(m_CoverSmoothnessScale, "Cover Smoothness Scale");
                }
                GUILayout.EndVertical();

                //冰石功能
                GUILayout.BeginVertical("", Styles.moduleHeader);
                m_MaterialEditor.ShaderProperty(m_IsICEOn, "开启冰石功能");
                if (m_IsICEOn.floatValue > 0)
                {
                    m_MaterialEditor.ShaderProperty(m_UseThicknessMask, "UseThicknessMask");
                    m_MaterialEditor.ShaderProperty(m_SSSColor, "SSSColor");
                    m_MaterialEditor.ShaderProperty(m_Translucency, "Scaterring Strength");
                    m_MaterialEditor.ShaderProperty(m_TransScattering, "Scaterring Falloff");
                    m_MaterialEditor.ShaderProperty(m_TransShadow, "Trans Shadow");
                    m_MaterialEditor.ShaderProperty(m_BackTransNormalDistortion, "Back Normal Distortion");
                    m_MaterialEditor.ShaderProperty(m_FrontTransNormalDistortion, "Front Normal Distortion");
                    m_MaterialEditor.ShaderProperty(m_FrontTransIntensity, "Front Intensity");
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical("", Styles.moduleHeader);
                m_MaterialEditor.ShaderProperty(m_UseRimThicknessMask, "UseRimThicknessMask");
                if (m_UseRimThicknessMask.floatValue > 0)
                {
                    m_MaterialEditor.ShaderProperty(m_RimIceColor, "RimColor");
                    m_MaterialEditor.ShaderProperty(m_RimBase, "RimBase");
                    m_MaterialEditor.ShaderProperty(m_RimPower, "RimPower");
                    m_MaterialEditor.ShaderProperty(m_RimIntensity, "RimIntensity");
                }
                GUILayout.EndVertical();

                GUILayout.BeginVertical("", Styles.moduleHeader);
                m_MaterialEditor.ShaderProperty(m_IsDustOn, "Dust On");
                if (m_IsDustOn.floatValue > 0)
                {
                    m_MaterialEditor.ShaderProperty(m_DustMap, "DustMap:RG");
                    m_MaterialEditor.ShaderProperty(m_DustColor, "Dust Color");
                    m_MaterialEditor.ShaderProperty(m_DustNoiseUVScale, "Dust Noise UVScale");
                    m_MaterialEditor.ShaderProperty(m_DustNoiseIntensity, "Dust Noise Intensity");
                    m_MaterialEditor.ShaderProperty(m_DustDepthShift, "Dust Depth Shift");
                    m_MaterialEditor.ShaderProperty(m_DustUVScale, "Dust UV Scale(xy:scale zw:speed)");
                }
                GUILayout.EndVertical();
                GUILayout.BeginVertical("", Styles.moduleHeader);
                m_MaterialEditor.ShaderProperty(m_UseRealtimeShadowToggle, "使用实时阴影");
                GUILayout.EndVertical();
                GUILayout.BeginVertical("", Styles.moduleHeader);
                m_MaterialEditor.RenderQueueField();
                m_MaterialEditor.EnableInstancingField();
                GUILayout.EndVertical();
            }

        }

        public static class Styles
        {
            public static Color headerColor = new Color(0.15f, 0.15f, 0.15f);
            public static GUIStyle moduleHeader;

            static Styles()
            {
                moduleHeader = new GUIStyle(GUI.skin.box);
                moduleHeader.padding = new RectOffset(32, 10, 5, 5);
            }
        }

         private static bool _BeginFoldoutBox(string foldoutName)
        {
            EditorGUI.DrawRect(EditorGUILayout.BeginHorizontal(Styles.moduleHeader), Styles.headerColor);
            Rect foldoutRect = GUILayoutUtility.GetRect(40f, 16f);
            EditorPrefs.SetBool(foldoutName + "_FoldOut", EditorGUI.Foldout(foldoutRect, EditorPrefs.GetBool(foldoutName + "_FoldOut"), foldoutName, true));
            EditorGUILayout.EndHorizontal();
            bool foldout = EditorPrefs.GetBool(foldoutName + "_FoldOut");
            if (foldout)
            {
                EditorGUILayout.BeginVertical("Box");
            }
            return foldout;
        }

        private static void _EndFoldoutBox()
        {
            EditorGUILayout.EndVertical();
        }
    }
}
