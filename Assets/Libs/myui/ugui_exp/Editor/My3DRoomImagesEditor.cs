using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(My3DRoomImage), true)]
    [CanEditMultipleObjects]
    public class My3DRoomImagesEditor : Editor
    {

        static List<string> ImageAccuracyLevelText = new List<string>()
        {
            "低","中","高"
        };

        static List<string> ImageRoomLightType = new List<string>()
        {
            "射灯","阳光","点光","区域"
        };


        SerializedProperty resName;
        SerializedProperty isOrthoGraphic;
        SerializedProperty uiroomType;

        SerializedProperty dropType;
        SerializedProperty clickType;

        SerializedProperty characterType;
        SerializedProperty useCameraConfig2;
        SerializedProperty characterUseHeightQaulity;

        SerializedProperty attachmentName;
        SerializedProperty cameraFieldView;

        SerializedProperty cameraOffset;
        SerializedProperty useHDR;
        



        GUIContent m_OffsetContent,m_dropContent,m_clickContent;

        SerializedProperty accuracyLevel;
        SerializedProperty usePointLight;
        SerializedProperty pointLightColor;
        SerializedProperty pointOffset;
        SerializedProperty lightDirection;
        SerializedProperty lightType;
        SerializedProperty lightIntensity;

        SerializedProperty useBloom;
        SerializedProperty thresholdGamma;
        SerializedProperty bloomRadius;
        SerializedProperty bloomIntensity;
        SerializedProperty bloomSoftKnee;
        SerializedProperty useRenderTexture;
        SerializedProperty shaderProjectorDir;
        SerializedProperty shadowProjectorPos;
        SerializedProperty showShadows;
        SerializedProperty shadowLevel;
        SerializedProperty shadowSize;
        SerializedProperty useSSS;
        SerializedProperty useColorSuite;


        GUIContent m_accuracyLevel;
        GUIContent m_usePointLight;
        GUIContent m_pointLightColor;
        GUIContent m_pointOffset;
        GUIContent m_useHdr;
        GUIContent m_lightDirection;
        GUIContent m_lightType;
        GUIContent m_lightIntensity;
        GUIContent m_useBloom;
        GUIContent m_bloomThreshold;
        GUIContent m_bloomRadius;
        GUIContent m_bloomIntensity;
        GUIContent m_bloomSoftKnee;
        GUIContent m_useRenderTexture;
        GUIContent m_shadowProjector;
        GUIContent m_showShadows;
        GUIContent m_shadowSize;
        GUIContent m_shadowLevel;
        GUIContent m_shadowProjectorPos;
        GUIContent m_useSSS;
        GUIContent m_useColorSuite;

        GUIContent m_charactor_use_heightquality;


        #region 颜色校正参数
        SerializedProperty propColorTemp;
        SerializedProperty propColorTint;

        SerializedProperty propToneMapping;
        SerializedProperty propExposure;

        SerializedProperty propSaturation;

        SerializedProperty propRCurve;
        SerializedProperty propGCurve;
        SerializedProperty propBCurve;
        SerializedProperty propCCurve;
        

        GUIContent labelToneMap;
        GUIContent labelExposure;
        GUIContent labelColorTemp;
        GUIContent labelColorTint;
        GUIContent labelColorSaturation;       

        #endregion


        protected virtual void OnEnable()
        {           
            resName = serializedObject.FindProperty("resName");
            isOrthoGraphic = serializedObject.FindProperty("isOrthoGraphic");
            uiroomType = serializedObject.FindProperty("uiroomType");
            characterType = serializedObject.FindProperty("characterType");
            characterUseHeightQaulity = serializedObject.FindProperty("foceHeightQuality");
            useCameraConfig2 = serializedObject.FindProperty("useCameraConfig2");
            attachmentName = serializedObject.FindProperty("attachmentName");
            cameraFieldView = serializedObject.FindProperty("cameraFieldView");
            cameraOffset = serializedObject.FindProperty("cameraOffset");
            dropType = serializedObject.FindProperty("dropType");
            clickType = serializedObject.FindProperty("clickType");
            accuracyLevel = serializedObject.FindProperty("accuracyLevel");
            pointLightColor = serializedObject.FindProperty("pointLightColor");
            useHDR = serializedObject.FindProperty("useHDR");
            usePointLight = serializedObject.FindProperty("usePointLight");
            pointOffset = serializedObject.FindProperty("pointOffset");
            lightDirection = serializedObject.FindProperty("lightDirection");
            lightType = serializedObject.FindProperty("lightType");
            lightIntensity = serializedObject.FindProperty("lightIntensity");
            useBloom = serializedObject.FindProperty("useBloom");
            thresholdGamma = serializedObject.FindProperty("thresholdGamma");
            bloomRadius = serializedObject.FindProperty("bloomRadius");
            bloomIntensity = serializedObject.FindProperty("bloomIntensity");
            bloomSoftKnee = serializedObject.FindProperty("bloomSoftKnee");
            useRenderTexture = serializedObject.FindProperty("useRenderTexture");
            shaderProjectorDir = serializedObject.FindProperty("shadowProjectorRotation");
            shadowProjectorPos = serializedObject.FindProperty("shadowProjectorPosition");
            showShadows = serializedObject.FindProperty("showShadows");
            shadowSize = serializedObject.FindProperty("shadowSize");
            shadowLevel = serializedObject.FindProperty("shadowLevel");
            useSSS = serializedObject.FindProperty("useSSS");
            useColorSuite = serializedObject.FindProperty("useColorSuite");


            m_charactor_use_heightquality = new GUIContent("低配置下使用高清资源");
            m_OffsetContent = new GUIContent("相机偏移量");           
            m_dropContent = new GUIContent("拖拽类型");
            m_clickContent = new GUIContent("点击类型");
            m_accuracyLevel = new GUIContent("图片精度");
            m_usePointLight = new GUIContent("使用光源");
            m_pointLightColor = new GUIContent("点光源颜色");
            m_pointOffset = new GUIContent("光源偏移量");
            m_useHdr = new GUIContent("使用HDR（高动态范围）");
            m_lightDirection = new GUIContent("光源方向");
            m_lightType = new GUIContent("光源类型");
            m_lightIntensity = new GUIContent("光源亮度");
            m_useBloom = new GUIContent("使用光线溢出");
            m_bloomThreshold = new GUIContent("溢出阀值（伽马）");
            m_bloomRadius = new GUIContent("溢出范围");
            m_bloomIntensity = new GUIContent("溢出混合因素");
            m_bloomSoftKnee = new GUIContent("溢出阀值缓冲区");
            m_useSSS = new GUIContent("使用次表面散射(sss)");
            m_useRenderTexture = new GUIContent("3转2显示在UI上");
            m_showShadows = new GUIContent("显示实时阴影");
            m_shadowProjector = new GUIContent("阴影投射方向");
            m_shadowProjectorPos = new GUIContent("阴影投射器位置");
            m_shadowSize = new GUIContent("阴影范围");
            m_shadowLevel = new GUIContent("阴影品质");
            m_useColorSuite = new GUIContent("颜色校正");


            #region 颜色校正参数
            propColorTemp = serializedObject.FindProperty("_colorTemp");
            propColorTint = serializedObject.FindProperty("_colorTint");
            propToneMapping = serializedObject.FindProperty("_toneMapping");
            propExposure = serializedObject.FindProperty("_exposure");
            propSaturation = serializedObject.FindProperty("_saturation");
            propRCurve = serializedObject.FindProperty("_rCurve");
            propGCurve = serializedObject.FindProperty("_gCurve");
            propBCurve = serializedObject.FindProperty("_bCurve");
            propCCurve = serializedObject.FindProperty("_cCurve");
           

            labelToneMap = new GUIContent("色调映射");
            labelExposure = new GUIContent("曝光度");
            labelColorTemp = new GUIContent("色温调节");
            labelColorTint = new GUIContent("颜色(绿-紫)");
            labelColorSaturation = new GUIContent("饱和度");            

            #endregion
        }

        public override void OnInspectorGUI()
        {           
            serializedObject.Update();

            EditorGUILayout.PropertyField(resName);
            EditorGUILayout.PropertyField(isOrthoGraphic);
            EditorGUILayout.PropertyField(uiroomType);

            EditorGUILayout.PropertyField(dropType, m_dropContent);
            EditorGUILayout.PropertyField(clickType, m_clickContent);

            if (uiroomType.intValue == 1)
            {
                EditorGUILayout.PropertyField(characterType);
                EditorGUILayout.PropertyField(useCameraConfig2);
                EditorGUILayout.PropertyField(characterUseHeightQaulity, m_charactor_use_heightquality);
            }

            EditorGUILayout.PropertyField(useRenderTexture, m_useRenderTexture);

            EditorGUILayout.PropertyField(attachmentName);
            EditorGUILayout.PropertyField(cameraFieldView);

            EditorGUILayout.PropertyField(cameraOffset, m_OffsetContent,true,new GUILayoutOption[0]);

            //EditorGUILayout.PropertyField(cameraOffset.FindPropertyRelative("x"), m_OffsetContent);
            //EditorGUILayout.PropertyField(cameraOffset.FindPropertyRelative("y"), m_OffsetContent_y);
            //EditorGUILayout.PropertyField(cameraOffset.FindPropertyRelative("z"), m_OffsetContent_z);

            EditorGUILayout.PropertyField(usePointLight, m_usePointLight);
            if(usePointLight.boolValue)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(pointLightColor, m_pointLightColor);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(m_lightType);
                var selectLightType = Mathf.Clamp(lightType.enumValueIndex, 0, ImageRoomLightType.Count - 1);
                selectLightType = EditorGUILayout.Popup(selectLightType, ImageRoomLightType.ToArray());
                lightType.enumValueIndex = selectLightType;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.PropertyField(pointOffset, m_pointOffset, true, new GUILayoutOption[0]);
                EditorGUILayout.PropertyField(lightIntensity, m_lightIntensity, true, new GUILayoutOption[0]);

                if (lightType.enumValueIndex !=(int) LightType.Point)
                {
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.PropertyField(lightDirection, m_lightDirection, true, new GUILayoutOption[0]);
                    --EditorGUI.indentLevel;
                }

                --EditorGUI.indentLevel;
            }

            if (Graphics.activeTier >= UnityEngine.Rendering.GraphicsTier.Tier3)
            {
                EditorGUILayout.PropertyField(useHDR, m_useHdr);
                if (useHDR.boolValue)
                {
                    EditorGUILayout.HelpBox(new GUIContent("开启HDR必须使用不透明内容."));
                }
            }
            else
            {
                useHDR.boolValue = false;
            }

            EditorGUILayout.PropertyField(useBloom, m_useBloom);
            if(useBloom.boolValue)
            {
                ++EditorGUI.indentLevel;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(m_bloomThreshold);
                thresholdGamma.floatValue = EditorGUILayout.Slider(thresholdGamma.floatValue,0.5f,5.0f, new GUILayoutOption[0]);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(m_bloomRadius);
                bloomRadius.floatValue = EditorGUILayout.Slider(bloomRadius.floatValue, 0.1f, 10f, new GUILayoutOption[0]);                
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(m_bloomIntensity);
                bloomIntensity.floatValue = EditorGUILayout.Slider(bloomIntensity.floatValue, 0f, 5f, new GUILayoutOption[0]);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(m_bloomSoftKnee);
                bloomSoftKnee.floatValue = EditorGUILayout.Slider(bloomSoftKnee.floatValue, 0f, 1f, new GUILayoutOption[0]);
                EditorGUILayout.EndHorizontal();

                --EditorGUI.indentLevel;
            }

            EditorGUILayout.PropertyField(useSSS, m_useSSS);
            if(useSSS.boolValue)
            {
                ++EditorGUI.indentLevel;

                --EditorGUI.indentLevel;
            }

            EditorGUILayout.PropertyField(useColorSuite, m_useColorSuite);
            if(useColorSuite.boolValue)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(propToneMapping, labelToneMap);
                if (propToneMapping.hasMultipleDifferentValues || propToneMapping.boolValue)
                {
                    EditorGUILayout.Slider(propExposure, 0, 5, labelExposure);
                    if (QualitySettings.activeColorSpace != ColorSpace.Linear)
                        EditorGUILayout.HelpBox("需要使用线性空间", MessageType.Warning);
                }

                EditorGUILayout.Space();

                EditorGUILayout.Slider(propColorTemp, -1.0f, 1.0f, labelColorTemp);
                EditorGUILayout.Slider(propColorTint, -1.0f, 1.0f, labelColorTint);

                EditorGUILayout.Space();

                EditorGUILayout.Slider(propSaturation, 0, 2, labelColorSaturation);

                EditorGUILayout.LabelField("曲线 (R, G, B, 混合)");
                EditorGUILayout.BeginHorizontal();
                var doubleHeight = GUILayout.Height(EditorGUIUtility.singleLineHeight * 2);
                EditorGUILayout.PropertyField(propRCurve, GUIContent.none, doubleHeight);
                EditorGUILayout.PropertyField(propGCurve, GUIContent.none, doubleHeight);
                EditorGUILayout.PropertyField(propBCurve, GUIContent.none, doubleHeight);
                EditorGUILayout.PropertyField(propCCurve, GUIContent.none, doubleHeight);
                EditorGUILayout.EndHorizontal();
                --EditorGUI.indentLevel;
            }

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(m_accuracyLevel);
            var selectIdx = Mathf.Clamp(accuracyLevel.enumValueIndex, 0, ImageAccuracyLevelText.Count - 1);
            selectIdx = EditorGUILayout.Popup(selectIdx, ImageAccuracyLevelText.ToArray());
            accuracyLevel.enumValueIndex = selectIdx;

            if (Application.isPlaying)
            {

            }


            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(showShadows, m_showShadows);
            if(showShadows.boolValue)
            {
                ++EditorGUI.indentLevel;

                shadowProjectorPos.vector3Value = EditorGUILayout.Vector3Field(m_shadowProjectorPos, shadowProjectorPos.vector3Value, new GUILayoutOption[0]);
                shaderProjectorDir.vector3Value = EditorGUILayout.Vector3Field(m_shadowProjector, shaderProjectorDir.vector3Value, new GUILayoutOption[0]);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(m_shadowLevel);
                var slevel = Mathf.Clamp(shadowLevel.enumValueIndex, 0, ImageAccuracyLevelText.Count - 1);
                slevel = EditorGUILayout.Popup(slevel, ImageAccuracyLevelText.ToArray());
                shadowLevel.enumValueIndex = slevel;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(m_shadowSize);
                shadowSize.floatValue = EditorGUILayout.Slider(shadowSize.floatValue, 1f, 30f, new GUILayoutOption[0]);
                EditorGUILayout.EndHorizontal();

                --EditorGUI.indentLevel;
            }

           
           

            serializedObject.ApplyModifiedProperties();
        }


    }
}
