using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(MyInputField), true)]
    public class MyInputFieldEditor : SelectableEditor
    {
        SerializedProperty m_TextComponent;
        SerializedProperty m_Text;
        SerializedProperty m_ContentType;
        SerializedProperty m_LineType;
        SerializedProperty m_InputType;
        SerializedProperty m_CharacterValidation;
        SerializedProperty m_KeyboardType;
        SerializedProperty m_CharacterLimit;
        SerializedProperty m_CaretBlinkRate;
        SerializedProperty m_SelectionColor;
        SerializedProperty m_HideMobileInput;
        SerializedProperty m_Placeholder;

        private SerializedProperty m_DialogType;
        private SerializedProperty m_DialogTitle;
        private SerializedProperty m_DialogOkBtn;
        private SerializedProperty m_DialogCancelBtn;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_TextComponent = serializedObject.FindProperty("m_TextComponent");
            m_Text = serializedObject.FindProperty("m_Text");
            m_ContentType = serializedObject.FindProperty("m_ContentType");
            m_LineType = serializedObject.FindProperty("m_LineType");
            m_InputType = serializedObject.FindProperty("m_InputType");
            m_CharacterValidation = serializedObject.FindProperty("m_CharacterValidation");
            m_KeyboardType = serializedObject.FindProperty("m_KeyboardType");
            m_CharacterLimit = serializedObject.FindProperty("m_CharacterLimit");
            m_CaretBlinkRate = serializedObject.FindProperty("m_CaretBlinkRate");
            m_SelectionColor = serializedObject.FindProperty("m_SelectionColor");
            m_HideMobileInput = serializedObject.FindProperty("m_HideMobileInput");
            m_Placeholder = serializedObject.FindProperty("m_Placeholder");

            m_DialogType = serializedObject.FindProperty("m_DialogType");
            m_DialogTitle = serializedObject.FindProperty("m_DialogTitle");
            m_DialogOkBtn = serializedObject.FindProperty("m_DialogOkBtn");
            m_DialogCancelBtn = serializedObject.FindProperty("m_DialogCancelBtn");
        }

        public override void OnInspectorGUI()
        {
            MyInputField dialog = target as MyInputField;

            serializedObject.Update();

            base.OnInspectorGUI();
            EditorGUILayout.Space();

#if UNITY_WEBGL
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(m_DialogType);
            EditorGUILayout.PropertyField(m_DialogTitle);

            if (dialog.m_DialogType == MyInputField.EDialogType.OverlayHtml)
            {
                EditorGUILayout.PropertyField(m_DialogOkBtn);
                EditorGUILayout.PropertyField(m_DialogCancelBtn);
            }

            
            EditorGUI.indentLevel--;
#endif


            EditorGUILayout.PropertyField(m_TextComponent, new GUIContent("文本显示者"), new GUILayoutOption[0]);

            EditorGUI.BeginDisabledGroup(m_TextComponent == null || m_TextComponent.objectReferenceValue == null);

            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("内容:");
            string value = m_Text.stringValue;
            value = EditorGUILayout.TextArea(value, new GUILayoutOption[0]);
            if (value.CompareTo(m_Text.stringValue) != 0)
                m_Text.stringValue = value;

            EditorGUI.indentLevel--;

            EditorGUILayout.PropertyField(m_CharacterLimit, new GUIContent("长度限制"), new GUILayoutOption[0]);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(m_ContentType, new GUIContent("内容类型"), new GUILayoutOption[0]);
            if (!m_ContentType.hasMultipleDifferentValues)
            {
                EditorGUI.indentLevel++;

                if (m_ContentType.enumValueIndex == (int)InputField.ContentType.Standard ||
                    m_ContentType.enumValueIndex == (int)InputField.ContentType.Autocorrected ||
                    m_ContentType.enumValueIndex == (int)InputField.ContentType.Custom)
                    EditorGUILayout.PropertyField(m_LineType, new GUIContent("换行方式"), new GUILayoutOption[0]);

                if (m_ContentType.enumValueIndex == (int)InputField.ContentType.Custom)
                {
                    EditorGUILayout.PropertyField(m_InputType);
                    EditorGUILayout.PropertyField(m_KeyboardType);
                    EditorGUILayout.PropertyField(m_CharacterValidation);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(m_Placeholder, new GUIContent("提示输入文本"), new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(m_CaretBlinkRate, new GUIContent("光标眨眼率"), new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(m_SelectionColor, new GUIContent("选中颜色"), new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(m_HideMobileInput, new GUIContent("隐藏手机键盘"), new GUILayoutOption[0]);

            EditorGUILayout.Space();


            EditorGUI.EndDisabledGroup();



            serializedObject.ApplyModifiedProperties();
        }
    }
}
