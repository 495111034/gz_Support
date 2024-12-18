using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.AnimatedValues;
using System.Linq;
using UnityEngine.Events;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(MyImageInText), true)]
    [CanEditMultipleObjects]
    public class MyImageInTextEditor : MySpriteImageEditor
    {
        //SerializedProperty TestSpName;
        //SerializedProperty TestAltlasName;

        //GUIContent txtTestSpName, txtTestAltlasName;

        protected override void OnEnable()
        {
            //txtTestSpName = new GUIContent("测试图片");
            //TestSpName = serializedObject.FindProperty("_testSpName");

            //txtTestAltlasName = new GUIContent("测试图集");
            //TestAltlasName = serializedObject.FindProperty("_testAltlasName");

            base.OnEnable();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();  //start 
            //EditorGUILayout.PropertyField(TestAltlasName, txtTestAltlasName, new GUILayoutOption[0]);
            //EditorGUILayout.PropertyField(TestSpName, txtTestSpName, new GUILayoutOption[0]);
            //EditorGUILayout.LabelField(TestSpName.stringValue);
            //EditorGUILayout.LabelField(TestAltlasName.stringValue);
            DrawUIs();
            serializedObject.ApplyModifiedProperties();//end
        }
    }
}
