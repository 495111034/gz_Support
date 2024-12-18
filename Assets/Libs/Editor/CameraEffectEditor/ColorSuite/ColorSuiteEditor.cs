
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

namespace MyEffect
{
    [CustomEditor(typeof(ColorSuite)), CanEditMultipleObjects]
    public class ColorSuiteEditor : Editor
    {
        SerializedProperty propColorTemp;
        SerializedProperty propColorTint;

        SerializedProperty propToneMapping;
        SerializedProperty propExposure;

        SerializedProperty propSaturation;

        SerializedProperty propRCurve;
        SerializedProperty propGCurve;
        SerializedProperty propBCurve;
        SerializedProperty propCCurve;

        SerializedProperty propDitherMode;
        //SerializedProperty propShader;

        GUIContent labelToneMap;
        GUIContent labelExposure;
        GUIContent labelColorTemp;
        GUIContent labelColorTint;
        GUIContent labelColorSaturation;
        GUIContent labelDitherMode;


        static List<string> ditherFunction = new List<string>()
        {
            "无","交错梯度噪声","三角形"
        };

        void OnEnable()
        {
            propColorTemp = serializedObject.FindProperty("_colorTemp");
            propColorTint = serializedObject.FindProperty("_colorTint");

            propToneMapping = serializedObject.FindProperty("_toneMapping");
            propExposure = serializedObject.FindProperty("_exposure");

            propSaturation = serializedObject.FindProperty("_saturation");

            propRCurve = serializedObject.FindProperty("_rCurve");
            propGCurve = serializedObject.FindProperty("_gCurve");
            propBCurve = serializedObject.FindProperty("_bCurve");
            propCCurve = serializedObject.FindProperty("_cCurve");

            propDitherMode = serializedObject.FindProperty("_ditherMode");

            //propShader = serializedObject.FindProperty("_shader");

            labelToneMap = new GUIContent("色调映射");
            labelExposure = new GUIContent("曝光度");
            labelColorTemp = new GUIContent("色温调节");
            labelColorTint = new GUIContent("颜色(绿-紫)");
            labelColorSaturation = new GUIContent("饱和度");
            labelDitherMode = new GUIContent("抖动模式");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

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

            //if (propShader.objectReferenceValue)
            //{
            //   // EditorGUILayout.LabelField("Shader:" + propShader.objectReferenceValue.name);
            //}
            //else
            //{
            //    propShader.objectReferenceValue = resource.ShaderManager.Find("Hidden/CameraEffects/ColorSuite");
            //    {
            //        if (!propShader.objectReferenceValue)
            //            EditorGUILayout.HelpBox("错误，未找到shader:Hidden/CameraEffects/ColorSuite", MessageType.Error);
            //    }
            //}

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(labelDitherMode);
            var selectIdx = Mathf.Clamp(propDitherMode.enumValueIndex, 0, ditherFunction.Count - 1);
            selectIdx = EditorGUILayout.Popup(selectIdx, ditherFunction.ToArray());
            propDitherMode.enumValueIndex = selectIdx;

            serializedObject.ApplyModifiedProperties();
        }
    }
}
