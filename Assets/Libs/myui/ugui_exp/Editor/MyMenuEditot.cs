using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(MyMenu), true)]
    [CanEditMultipleObjects]
    public class MyMenuEditor : Editor
    {
        SerializedProperty m_Template;       
        SerializedProperty m_Options;
        SerializedProperty m_space;
        SerializedProperty m_useLanguage;
        SerializedProperty m_languageIDs;
        SerializedProperty m_resizeAsContent;
        SerializedProperty m_limitSize;


        protected virtual void OnEnable()
        {           
            m_Template = serializedObject.FindProperty("m_Template");         
            m_Options = serializedObject.FindProperty("m_Options");
            m_space = serializedObject.FindProperty("m_space");
            m_useLanguage = serializedObject.FindProperty("m_useLanguageID");
            m_languageIDs = serializedObject.FindProperty("m_languageIDs");
            m_resizeAsContent = serializedObject.FindProperty("m_resizeAsContent");
            m_limitSize = serializedObject.FindProperty("m_limitSize");

        }

        public override void OnInspectorGUI()
        {          
            

            serializedObject.Update();
            EditorGUILayout.PropertyField(m_Template); 

            EditorGUILayout.LabelField("选项", new GUILayoutOption[0]);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("使用游戏语言包", new GUILayoutOption[0]);
            m_useLanguage.boolValue = EditorGUILayout.Toggle(m_useLanguage.boolValue, new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            if (!m_useLanguage.boolValue)
            {
                EditorGUILayout.PropertyField(m_Options, new GUIContent("选项内容"), true, new UnityEngine.GUILayoutOption[0]);
            }
            else
            {
                EditorGUILayout.PropertyField(m_languageIDs, new GUIContent("语言包ID"), true, new UnityEngine.GUILayoutOption[0]);

               
            }
            EditorGUILayout.PropertyField(m_space, new UnityEngine.GUIContent("间距"), true, new UnityEngine.GUILayoutOption[0]);
            EditorGUILayout.PropertyField(m_resizeAsContent, new UnityEngine.GUIContent("适应内容高度"), true, new UnityEngine.GUILayoutOption[0]);
            if(m_resizeAsContent.boolValue )
            {
                EditorGUILayout.PropertyField(m_limitSize,new GUIContent("最大高度"),true, new UnityEngine.GUILayoutOption[0]);
            }
            else
            {
                m_limitSize.floatValue = 0f;
            }

            serializedObject.ApplyModifiedProperties();            
            
        }

    }
}