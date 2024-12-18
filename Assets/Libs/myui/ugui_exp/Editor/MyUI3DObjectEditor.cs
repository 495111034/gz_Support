using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(MyUI3DObject), true)]
    [CanEditMultipleObjects]
    public class MyUI3DObjectEditor : Editor
    {
        SerializedProperty prefabname;
        SerializedProperty renderQueue;
        //SerializedProperty layerType;

        GUIContent m_renderQueue;
        GUIContent m_objLayer;

        protected virtual void OnEnable()
        {
            prefabname = serializedObject.FindProperty("prefabname");
            //renderQueue = serializedObject.FindProperty("renderQueue");
            //layerType = serializedObject.FindProperty("layerType");
          //  m_renderQueue = new GUIContent("渲染层次");
          //  m_objLayer = new GUIContent("内容Layer");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(prefabname);
            //EditorGUILayout.PropertyField(renderQueue, m_renderQueue);
           // EditorGUILayout.PropertyField(layerType, m_objLayer);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
