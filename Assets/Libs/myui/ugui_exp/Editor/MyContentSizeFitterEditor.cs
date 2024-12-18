using System;
using UnityEngine;
using UnityEngine.UI;
namespace UnityEditor.UI
{
    [CanEditMultipleObjects, CustomEditor(typeof(MyContentSizeFitter), true)]
    public class MyContentSizeFitterEditor : SelfControllerEditor
    {
        private SerializedProperty m_HorizontalFit;
        private SerializedProperty m_VerticalFit;
        SerializedProperty m_fixSizeHorizontal, m_fixSizeVertical;
        protected virtual void OnEnable()
        {
            m_HorizontalFit = serializedObject.FindProperty("m_HorizontalFit");
            m_VerticalFit = serializedObject.FindProperty("m_VerticalFit");
            m_fixSizeHorizontal = serializedObject.FindProperty("fixSizeHorizontal");
            m_fixSizeVertical = serializedObject.FindProperty("fixSizeVertical");
        }
       
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_HorizontalFit,new GUIContent("宽度填充"), true, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(m_fixSizeHorizontal, new GUIContent("宽度修正"), true, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(m_VerticalFit, new GUIContent("高度填充"), true, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(m_fixSizeVertical, new GUIContent("高度修正"), true, new GUILayoutOption[0]);


            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}