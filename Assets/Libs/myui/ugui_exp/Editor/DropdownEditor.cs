using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(MyDropdown), true)]
    [CanEditMultipleObjects]
    public class MyDropdownEditor : SelectableEditor
    {
        SerializedProperty m_Template;
        SerializedProperty m_CaptionText;
        SerializedProperty m_ItemText;
        SerializedProperty m_Value;
        SerializedProperty m_Options;
        SerializedProperty m_space;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Template = serializedObject.FindProperty("m_Template");
            m_CaptionText = serializedObject.FindProperty("m_CaptionText");
            m_ItemText = serializedObject.FindProperty("m_ItemText");
            m_Value = serializedObject.FindProperty("m_Value");
            m_Options = serializedObject.FindProperty("m_Options");
            m_space = serializedObject.FindProperty("m_space");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();

            serializedObject.Update();
            EditorGUILayout.PropertyField(m_Template);
            EditorGUILayout.PropertyField(m_CaptionText);
            EditorGUILayout.PropertyField(m_ItemText);
            EditorGUILayout.PropertyField(m_Value);
            EditorGUILayout.PropertyField(m_Options,new UnityEngine.GUIContent("选项"), true,new UnityEngine.GUILayoutOption[0]);
            EditorGUILayout.PropertyField(m_space,new UnityEngine.GUIContent("间距"), true, new UnityEngine.GUILayoutOption[0]);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
