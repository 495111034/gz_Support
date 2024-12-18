using UnityEngine;
using UnityEditor;
using UnityEditor.Macros;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SimpleGrass
{

    [CustomEditor(typeof(SimpleInstancingMgr))]
    public class SimpleInstancingMgrEditor : Editor
    {

        void OnEnable()
        {
            
        }
       
        void OnDisable()
        {

        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            base.OnInspectorGUI();
            bool hasModified = EditorGUI.EndChangeCheck();

            serializedObject.UpdateIfRequiredOrScript();
            if (Application.isPlaying && Application.isEditor)
            {
                if (hasModified)
                {
                    SimpleInstancingMgr instMgr = target as SimpleInstancingMgr;
                    if(instMgr != null)
                    {
                        for (int protoIndex = 0; protoIndex < instMgr.prototypeList.Count; ++protoIndex)
                        {
                            instMgr.AddDirty(protoIndex);
                        }

                        Debug.Log("######## SimpleInstancingMgr ModifiedProperties");
                    }
                }
            }
            SimpleInstancingMgr mgr = target as SimpleInstancingMgr;
            mgr.SetWindProperty();
            serializedObject.ApplyModifiedProperties();
        }

        public void OnSceneGUI()
        {
            
        }
      

    }


}

