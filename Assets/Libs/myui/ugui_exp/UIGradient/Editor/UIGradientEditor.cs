using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(UIGradient), true)]
    [CanEditMultipleObjects]
    public class UIGradientEditor : Editor
    {
        SerializedProperty m_color1, m_color2, m_angle, m_ignoreRatio,m_autoRotation, m_cycle_time;
        GUIContent str_color1, str_color2, str_angle, str_ignoreRatio, str_autoRotation, str_cycle_time;

        protected virtual void OnEnable()
        {
            m_color1 = serializedObject.FindProperty("_color1");
            m_color2 = serializedObject.FindProperty("_color2");
            m_angle = serializedObject.FindProperty("_angle");
            m_ignoreRatio = serializedObject.FindProperty("_ignoreRatio");
            m_autoRotation = serializedObject.FindProperty("_autoRotation");
            m_cycle_time = serializedObject.FindProperty("_cycle_time");

            str_color1 = new GUIContent("颜色1");
            str_color2 = new GUIContent("颜色2");
            str_angle = new GUIContent("角度");
            str_ignoreRatio = new GUIContent("忽略比率");
            str_autoRotation = new GUIContent("自动旋转");
            str_cycle_time = new GUIContent("旋转周期");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(m_color1, str_color1,true,new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(m_color2, str_color2, true, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(m_angle, str_angle, true, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(m_ignoreRatio, str_ignoreRatio, true, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(m_autoRotation, str_autoRotation, true, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(m_cycle_time, str_cycle_time, true, new GUILayoutOption[0]);

            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();

        }
    }

}