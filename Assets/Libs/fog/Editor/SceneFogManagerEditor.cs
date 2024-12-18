using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[CustomEditor(typeof(SceneFogManager))]
public class SceneHeightFogManagerEditor : Editor
{
    SceneFogManager m_FogManager;
    SerializedObject m_serializedObject;

    private void OnEnable()
    {
        m_FogManager = (SceneFogManager)target;
        m_serializedObject = serializedObject;
        CatchSerializedProperty();
    }

    public override void OnInspectorGUI()
    {
        m_serializedObject.Update();
        EditorGUI.BeginChangeCheck();
        {
            EditorGUILayout.PropertyField(fogType);
            EditorGUILayout.Space();
        }
        if (EditorGUI.EndChangeCheck())
        {
            m_serializedObject.ApplyModifiedProperties();
            m_FogManager.SwitchFog();
            m_serializedObject.Update();
        }

        EditorGUI.BeginChangeCheck();
        {
            if (((FogManager.FogType)fogType.intValue) == FogManager.FogType.FogHeight)
            {
                DrawHeightFogInspectorGUI();
            }
            else if (((FogManager.FogType)fogType.intValue) == FogManager.FogType.FogLinear)
            {
                DrawBuiltInFogInspectorGUI();
            }
            else if(((FogManager.FogType)fogType.intValue) != FogManager.FogType.None)
            {
                EditorGUILayout.HelpBox("FogType Not Support In Shader", MessageType.Info);
            }
        }
        if (EditorGUI.EndChangeCheck())
        {
            m_serializedObject.ApplyModifiedProperties();
        }
        m_FogManager.RefreshFogProperty();
    }

    private void DrawHeightFogInspectorGUI()
    {
        EditorGUILayout.LabelField("Height Fog Settings");
        EditorGUILayout.Space(10);
        EditorGUILayout.PropertyField(fogIntensity);
        EditorGUILayout.PropertyField(heightFogColor);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(fogDistanceStart);
        EditorGUILayout.PropertyField(fogDistanceEnd);
        EditorGUILayout.PropertyField(fogDistanceFalloff);
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(fogHeightBasedOnCameraPos);
        EditorGUILayout.PropertyField(fogHeightStart);
        EditorGUILayout.PropertyField(fogHeightEnd);
        EditorGUILayout.PropertyField(fogHeightFalloff);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Light Settings");
        EditorGUILayout.PropertyField(lightFalloff);
        EditorGUILayout.PropertyField(lightIntensity);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Noise Settings");
        EditorGUILayout.PropertyField(noiseIntensity);
        EditorGUILayout.PropertyField(noiseDistanceEnd);
        EditorGUILayout.PropertyField(noiseScale);
        EditorGUILayout.PropertyField(noiseSpeed);
    }

    private void DrawBuiltInFogInspectorGUI()
    {
        EditorGUILayout.LabelField("Linear Fog Settings");
        EditorGUILayout.Space(10);
        EditorGUILayout.PropertyField(linearFogColor, MyGUIStyle.builtInFogColor);
        EditorGUILayout.PropertyField(linearFogDistanceStart, MyGUIStyle.builtInFogDistanceStart);
        EditorGUILayout.PropertyField(linearInFogDistanceEnd, MyGUIStyle.builtInFogDistanceEnd);
    }
    static class MyGUIStyle
    {
        public static GUIContent builtInFogColor = new GUIContent("Fog Color");
        public static GUIContent builtInFogDistanceStart = new GUIContent("Start");
        public static GUIContent builtInFogDistanceEnd = new GUIContent("End");
    }
    SerializedProperty fogType;
    SerializedProperty fogDistanceStart, fogDistanceEnd, fogDistanceFalloff, fogHeightBasedOnCameraPos, fogHeightStart, fogHeightEnd, fogHeightFalloff, fogIntensity, heightFogColor,
                        lightFalloff, lightIntensity, noiseIntensity, noiseDistanceEnd, noiseScale, noiseSpeed;
    SerializedProperty linearFogColor, linearFogDistanceStart, linearInFogDistanceEnd;
    private void CatchSerializedProperty()
    {
        fogType = m_serializedObject.FindProperty("fogType");
        //heightFogProperty
        fogDistanceStart = m_serializedObject.FindProperty("fogDistanceStart");
        fogDistanceEnd = m_serializedObject.FindProperty("fogDistanceEnd");
        fogDistanceFalloff = m_serializedObject.FindProperty("fogDistanceFalloff");
        fogHeightBasedOnCameraPos = m_serializedObject.FindProperty("fogHeightBasedOnCameraPos");
        fogHeightStart = m_serializedObject.FindProperty("fogHeightStart");
        fogHeightEnd = m_serializedObject.FindProperty("fogHeightEnd");
        fogHeightFalloff = m_serializedObject.FindProperty("fogHeightFalloff");
        fogIntensity = m_serializedObject.FindProperty("fogIntensity");
        heightFogColor = m_serializedObject.FindProperty("heightFogColor");
        lightFalloff = m_serializedObject.FindProperty("lightFalloff");
        lightIntensity = m_serializedObject.FindProperty("lightIntensity");
        noiseIntensity = m_serializedObject.FindProperty("noiseIntensity");
        noiseDistanceEnd = m_serializedObject.FindProperty("noiseDistanceEnd");
        noiseScale = m_serializedObject.FindProperty("noiseScale");
        noiseSpeed = m_serializedObject.FindProperty("noiseSpeed");
        //builtInFogProperty
        linearFogColor = m_serializedObject.FindProperty("builtInFogColor");
        linearFogDistanceStart = m_serializedObject.FindProperty("builtInFogDistanceStart");
        linearInFogDistanceEnd = m_serializedObject.FindProperty("builtInFogDistanceEnd");
    }
}

