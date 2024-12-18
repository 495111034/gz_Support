using UnityEngine;
using UnityEngine.UI;
using UnityEditorInternal;
using UnityEditor.AnimatedValues;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(MyButtonBar), true)]
    [CanEditMultipleObjects]
    public class MyButtonBarEditor : MyGridLayoutGroupEditor
    {
        SerializedProperty m_isVertical;
        SerializedProperty m_itemWidth, m_itemHeight;
        GUIContent m_isVerticalContent;
        protected override void OnEnable()
        {
            base.OnEnable();
            m_isVertical = serializedObject.FindProperty("_isVertical");
            m_itemWidth = serializedObject.FindProperty("m_itemWidth");
            m_itemHeight = serializedObject.FindProperty("m_itemHeight");

            m_isVerticalContent = new GUIContent("是否坚排");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(m_isVertical, m_isVerticalContent);
            EditorGUILayout.PropertyField(m_itemWidth, new GUIContent("单元宽度"));
            EditorGUILayout.PropertyField(m_itemHeight, new GUIContent("单元高度"));
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            base.OnInspectorGUI();
        }
    }
}
