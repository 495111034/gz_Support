using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(UITextGradient), true)]
    [CanEditMultipleObjects]
    public class UITextGradientEditor : Editor
    {
        SerializedProperty m_color1, m_color2, m_angle;
        GUIContent str_color1, str_color2, str_angle;


        protected virtual void OnEnable()
        {
            m_color1 = serializedObject.FindProperty("_color1");
            m_color2 = serializedObject.FindProperty("_color2");
            m_angle = serializedObject.FindProperty("_angle");

            str_color1 = new GUIContent("颜色1");
            str_color2 = new GUIContent("颜色2");
            str_angle = new GUIContent("角度");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(m_color1, str_color1, true, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(m_color2, str_color2, true, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(m_angle, str_angle, true, new GUILayoutOption[0]);

            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}