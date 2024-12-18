using UnityEngine.UI;
using UnityEngine;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(UnityEngine.UI.MyButton), true)]
    [CanEditMultipleObjects]
    public class MyButtonEditor : SelectableEditor
    {
        SerializedProperty m_tipsObj;
        SerializedProperty m_clickSound;
        SerializedProperty m_longPressSound;
        SerializedProperty m_animCoefficient;
        GUIContent m_tipsContent;
        GUIContent mc_clickSound;
        GUIContent mc_longPressSound;
        GUIContent canimCoefficient;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_tipsObj = serializedObject.FindProperty("stringTips");
            m_clickSound = serializedObject.FindProperty("soundOnClick");
            m_longPressSound = serializedObject.FindProperty("soundOnLongPress");
            m_animCoefficient = serializedObject.FindProperty("animCoefficient");
            m_tipsContent = new GUIContent("备注");
            mc_clickSound = new GUIContent("点击声音");
            mc_longPressSound = new GUIContent("长按声音");
            canimCoefficient = new GUIContent("动态效果系数");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField(mc_clickSound);
            m_clickSound.stringValue = EditorGUILayout.TextField(m_clickSound.stringValue, new GUILayoutOption[0]);
            EditorGUILayout.LabelField(mc_longPressSound);
            m_longPressSound.stringValue = EditorGUILayout.TextField(m_longPressSound.stringValue, new GUILayoutOption[0]);
            EditorGUILayout.LabelField(m_tipsContent);
            m_tipsObj.stringValue = EditorGUILayout.TextField(m_tipsObj.stringValue, new GUILayoutOption[0]);

            EditorGUILayout.LabelField(canimCoefficient);
            m_animCoefficient.floatValue = EditorGUILayout.Slider(m_animCoefficient.floatValue, 0.01f, 1, new GUILayoutOption[0]);

            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}