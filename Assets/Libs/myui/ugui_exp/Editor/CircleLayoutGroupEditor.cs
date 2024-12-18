using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(CircleLayoutGroup), true)]
    [CanEditMultipleObjects]
    public class CircleLayoutGroupEdtor : Editor
    {
        SerializedProperty _radius;
        SerializedProperty viewRotation;
        SerializedProperty viewAngle;
        SerializedProperty springAmount;
        SerializedProperty selectedScaleAmount;
        SerializedProperty content;



        GUIContent c_radius, cviewRotation, cviewAngle, cspringAmount, cselectedScaleAmount, ccontent;
       

        protected virtual void OnEnable()
        {
            c_radius = new GUIContent("半径");
            _radius = serializedObject.FindProperty("_radius");

            cviewRotation = new GUIContent("角度");
            viewRotation = serializedObject.FindProperty("viewRotation");

            cviewAngle = new GUIContent("有效区域");
            viewAngle = serializedObject.FindProperty("viewAngle");

            cspringAmount = new GUIContent("惯性");
            springAmount = serializedObject.FindProperty("springAmount");

            cselectedScaleAmount = new GUIContent("选中放大比例");
            selectedScaleAmount = serializedObject.FindProperty("selectedScaleAmount");

            ccontent = new GUIContent("内容区域");
            content = serializedObject.FindProperty("content");


        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();  //start

            EditorGUILayout.PropertyField(content, ccontent, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(_radius, c_radius, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(viewRotation, cviewRotation);
            EditorGUILayout.PropertyField(viewAngle, cviewAngle);
            EditorGUILayout.PropertyField(springAmount, cspringAmount);
            EditorGUILayout.PropertyField(selectedScaleAmount, cselectedScaleAmount);

            serializedObject.ApplyModifiedProperties();//end
            base.OnInspectorGUI();
        }

        
    }
}
