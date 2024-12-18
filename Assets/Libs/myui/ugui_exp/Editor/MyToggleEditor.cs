using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(MyToggle), true)]
    [CanEditMultipleObjects]
    public class MyToggleEditor : SelectableEditor
    {       
        SerializedProperty m_TransitionProperty;
        SerializedProperty m_GraphicProperty, m_falseObj;
        SerializedProperty m_GroupProperty;
        SerializedProperty m_IsOnProperty;
        SerializedProperty m_tipsObj;
        SerializedProperty m_animChange;
        SerializedProperty m_soundOnChange;
        SerializedProperty m_animCoefficient;
        GUIContent m_tipsContent,cGraphic,cGroup,cFlaseObj, canimChange,canimCoefficient, mc_soundOnChange;

        protected override void OnEnable()
        {
            base.OnEnable();

            m_TransitionProperty = serializedObject.FindProperty("toggleTransition");
            m_GraphicProperty = serializedObject.FindProperty("graphic");
            m_GroupProperty = serializedObject.FindProperty("m_Group");
            m_IsOnProperty = serializedObject.FindProperty("m_IsOn");
            m_tipsObj = serializedObject.FindProperty("stringTips");
            m_falseObj = serializedObject.FindProperty("falseGraphic");
            m_animChange = serializedObject.FindProperty("animChange");
            m_soundOnChange = serializedObject.FindProperty("soundOnSelectOn");
            m_animCoefficient = serializedObject.FindProperty("animCoefficient");
            m_tipsContent = new GUIContent("备注");
            cGraphic = new GUIContent("为真时显示");
            cFlaseObj = new GUIContent("为假时显示");
            cGroup = new GUIContent("群组");
            canimChange = new GUIContent("动态效果");
            canimCoefficient = new GUIContent("动态效果系数");
            mc_soundOnChange = new GUIContent("选中声音");
        }

        public override void OnInspectorGUI()
        {

            EditorGUILayout.Space();

            serializedObject.Update();
            EditorGUILayout.PropertyField(m_soundOnChange, mc_soundOnChange);
            EditorGUILayout.PropertyField(m_IsOnProperty);
            EditorGUILayout.PropertyField(m_TransitionProperty);
            EditorGUILayout.PropertyField(m_GraphicProperty, cGraphic);
            EditorGUILayout.PropertyField(m_falseObj, cFlaseObj);
            EditorGUILayout.PropertyField(m_GroupProperty, cGroup);
            EditorGUILayout.PropertyField(m_animChange, canimChange);

            if(m_animChange.boolValue)
            {
                EditorGUILayout.LabelField(canimCoefficient);
                m_animCoefficient.floatValue = EditorGUILayout.Slider(m_animCoefficient.floatValue, 0.01f, 1, new GUILayoutOption[0]);
            }

            EditorGUILayout.LabelField(m_tipsContent);
            m_tipsObj.stringValue = EditorGUILayout.TextField(m_tipsObj.stringValue, new GUILayoutOption[0]);

            EditorGUILayout.Space();


            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }
}
