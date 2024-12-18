using UnityEngine;
using UnityEditor;
using UnityEditor.Macros;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SimpleGrass
{

    [CustomEditor(typeof(SimpleGrassProtoInfo))]
    public class SimpleGrassProtoInfoEditor : Editor
    {
        // SimpleGrassChunk grassChunk;
        SerializedProperty _LayerID, _CullingMaxDistance, _MergeChunkDistance, _CastShadows, _ReceiveShadows,_Density, _UseLightProbe;
        SimpleGrassProtoInfo protoInfoTarget;

        //SimpleGrassProtoInfoEditor()
        //{
        //    Debug.Log("");
        //}

        void OnEnable()
        {
            protoInfoTarget = target as SimpleGrassProtoInfo;
            if(!protoInfoTarget.inited)
            {
                protoInfoTarget.LayerID = protoInfoTarget.gameObject.layer;

                //初始化 gameobject.tag="EditorOnly"
                BackUPGameObjectTag();
                protoInfoTarget.gameObject.tag = "EditorOnly";

                protoInfoTarget.inited = true;
            }

            _LayerID = serializedObject.FindProperty("layerID");
            _CullingMaxDistance = serializedObject.FindProperty("cullingMaxDistance");
            _MergeChunkDistance = serializedObject.FindProperty("mergeChunkDistance");
            _CastShadows = serializedObject.FindProperty("castShadows");
            _ReceiveShadows = serializedObject.FindProperty("receiveShadows");
            _Density = serializedObject.FindProperty("density");
            _UseLightProbe = serializedObject.FindProperty("useLightProbe");
        }
               
        void OnDisable()
        {
            BackUPGameObjectTag();           
        }

        void BackUPGameObjectTag()
        {
            if (!protoInfoTarget)
                return;

            if (string.IsNullOrEmpty(protoInfoTarget.BackupTag))
            {
                protoInfoTarget.BackupTag = protoInfoTarget.gameObject.tag;
            }
            else
            {
                if (!protoInfoTarget.gameObject.CompareTag("EditorOnly"))
                {
                    if (!protoInfoTarget.gameObject.CompareTag(protoInfoTarget.BackupTag))
                    {
                        protoInfoTarget.BackupTag = protoInfoTarget.gameObject.tag;
                    }
                }
            }
        }

        public override void OnInspectorGUI()
        {

            base.OnInspectorGUI();

            serializedObject.UpdateIfRequiredOrScript();

            EditorGUILayout.PropertyField(_CullingMaxDistance, new GUIContent("Culling Max Distance"));
            EditorGUILayout.PropertyField(_MergeChunkDistance, new GUIContent("Merge Chunk Distance"));
            EditorGUILayout.PropertyField(_CastShadows, new GUIContent("Cast Shadows"));
            EditorGUILayout.PropertyField(_ReceiveShadows, new GUIContent("Receive Shadows"));
                        
           _LayerID.intValue = EditorGUILayout.LayerField("LayerID", _LayerID.intValue);
            GUILayout.Space(10);

            //EditorGUILayout.PropertyField(_Density, new GUIContent("Density"));
            EditorGUILayout.Slider(_Density, 0.01f, 1.0f, "Density");

            //_UseLightProbe， 暂不使用
            //EditorGUILayout.PropertyField(_UseLightProbe, new GUIContent("UseLightProbe"));
            _UseLightProbe.boolValue = false;


            serializedObject.ApplyModifiedProperties();
        }

        public void OnSceneGUI()
        {
            

        }

    }


}

