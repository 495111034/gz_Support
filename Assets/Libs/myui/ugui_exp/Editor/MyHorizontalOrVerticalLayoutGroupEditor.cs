using UnityEngine;
using UnityEngine.UI;
using UnityEditorInternal;
using UnityEditor.AnimatedValues;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(MyHorizontalOrVerticalLayoutGroup), true)]
    [CanEditMultipleObjects]
    public class MyHorizontalOrVerticalLayoutGroupEditor : Editor
    {
        private SerializedProperty m_Padding;
        private SerializedProperty m_Spacing;
        private SerializedProperty m_ChildAlignment;
        private SerializedProperty m_ChildControlWidth;
        private SerializedProperty m_ChildControlHeight;
        private SerializedProperty m_ChildForceExpandWidth;
        private SerializedProperty m_ChildForceExpandHeight;
        private SerializedProperty m_autosize;
        protected virtual void OnEnable()
        {
            this.m_Padding = serializedObject.FindProperty("m_Padding");
            this.m_Spacing = serializedObject.FindProperty("m_Spacing");
            this.m_ChildAlignment = serializedObject.FindProperty("m_ChildAlignment");
            this.m_ChildControlWidth = serializedObject.FindProperty("m_ChildControlWidth");
            this.m_ChildControlHeight = serializedObject.FindProperty("m_ChildControlHeight");
            this.m_ChildForceExpandWidth = serializedObject.FindProperty("m_ChildForceExpandWidth");
            this.m_ChildForceExpandHeight = serializedObject.FindProperty("m_ChildForceExpandHeight");
            this.m_autosize = serializedObject.FindProperty("autosize");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_Padding, true, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(m_Spacing, true, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(m_ChildAlignment, true, new GUILayoutOption[0]);
            Rect rect = EditorGUILayout.GetControlRect(new GUILayoutOption[0]);
            rect = EditorGUI.PrefixLabel(rect, -1, new GUIContent("控制孩子尺寸"));
            rect.width = (Mathf.Max(50f, (rect.width - 4f) / 3f));
            EditorGUIUtility.labelWidth = (50f);
            ToggleLeft(rect, this.m_ChildControlWidth, new GUIContent("Width"));
            rect.x = (rect.x + (rect.width + 2f));
            ToggleLeft(rect, this.m_ChildControlHeight, new GUIContent("Height"));
            EditorGUIUtility.labelWidth = (0f);
            rect = EditorGUILayout.GetControlRect(new GUILayoutOption[0]);
            rect = EditorGUI.PrefixLabel(rect, -1, new GUIContent("强制对齐父尺寸"));
            rect.width = (Mathf.Max(50f, (rect.width - 4f) / 3f));
            EditorGUIUtility.labelWidth = (50f);
            ToggleLeft(rect, this.m_ChildForceExpandWidth, new GUIContent("Width"));
            rect.x = (rect.x + (rect.width + 2f));
            ToggleLeft(rect, this.m_ChildForceExpandHeight, new GUIContent("Height"));
            EditorGUIUtility.labelWidth = (0f);

            EditorGUILayout.PropertyField(m_autosize, new GUIContent("适应孩子总尺寸"), true, new GUILayoutOption[0]);

            serializedObject.ApplyModifiedProperties();            
        }

        private void ToggleLeft(Rect position, SerializedProperty property, GUIContent label)
        {
            bool flag = property.boolValue;
            EditorGUI.showMixedValue = (property.hasMultipleDifferentValues);
            EditorGUI.BeginChangeCheck();
            int indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = (0);
            flag = EditorGUI.ToggleLeft(position, label, flag);
            EditorGUI.indentLevel = (indentLevel);
            if (EditorGUI.EndChangeCheck())
            {
                property.boolValue = (property.hasMultipleDifferentValues || !property.boolValue);
            }
            EditorGUI.showMixedValue = (false);
        }
    }
}
