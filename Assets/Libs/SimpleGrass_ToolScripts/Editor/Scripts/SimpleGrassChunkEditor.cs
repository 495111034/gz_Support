using UnityEngine;
using UnityEditor;
using UnityEditor.Macros;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SimpleGrass
{

    [CustomEditor(typeof(SimpleGrassChunk))]
    public class SimpleGrassChunkEditor : Editor
    {
       // SimpleGrassChunk grassChunk;
        SerializedProperty _LayerID;
        void OnEnable()
        {
            _LayerID = serializedObject.FindProperty("layerID");
           // grassChunk = target as SimpleGrassChunk;
        }
       
        void OnDisable()
        {

        }

        public override void OnInspectorGUI()
        {

            base.OnInspectorGUI();
//#if UNITY_5_6_OR_NEWER
            serializedObject.UpdateIfRequiredOrScript();
//#else
//			serializedObject.UpdateIfDirtyOrScript ();
//#endif

            _LayerID.intValue = EditorGUILayout.LayerField("LayerID", _LayerID.intValue);
            GUILayout.Space(10);

            serializedObject.ApplyModifiedProperties();
        }

        public void OnSceneGUI()
        {
            

        }
      

    }


}

