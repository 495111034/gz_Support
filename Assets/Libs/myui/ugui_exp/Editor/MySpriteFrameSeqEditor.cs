using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(MySpriteFrameSeq), true)]
    [CanEditMultipleObjects]
    public class MySpriteFrameSeqEdtor : Editor
    {
        SerializedProperty spAlpha;
        SerializedProperty spSpList;
        SerializedProperty spMyPacker;
        SerializedProperty spUseAll;
        SerializedProperty spRaycastTarget;
        SerializedProperty spCompleteEvent;

        SerializedProperty spTime, spDelay, skKeyFrame, spLoopType, spAutoSize;

        GUIContent   cAlpha, cSpList, cMyPacker,cUseAll,cAutoSize;
        GUIContent cTime, cDelay, cKeyFrame, cLoopType, cRaycastTarget, cCompleteEvent;

        protected virtual void OnEnable()
        { 
            cAlpha = new GUIContent("透明度");
            spAlpha = serializedObject.FindProperty("alpha");

            cSpList = new GUIContent("图片列表");
            spSpList = serializedObject.FindProperty("sprite_arr");

            cMyPacker = new GUIContent("图集");
            spMyPacker = serializedObject.FindProperty("spritePacker");

            cUseAll = new GUIContent("所有图片");
            spUseAll = serializedObject.FindProperty("useAllFrame");

            cTime = new GUIContent("播放时间");
            spTime = serializedObject.FindProperty("time");

            cDelay = new GUIContent("延时播放");
            spDelay = serializedObject.FindProperty("delay");

            cKeyFrame = new GUIContent("关键帧通知");
            skKeyFrame = serializedObject.FindProperty("keyframe");

            cCompleteEvent = new GUIContent("完成事件");
            spCompleteEvent = serializedObject.FindProperty("completeEvent");

            cLoopType = new GUIContent("循环方式");
            spLoopType = serializedObject.FindProperty("loop");

            spAutoSize = serializedObject.FindProperty("autosize");
            cAutoSize = new GUIContent("尺寸自适应");

            cRaycastTarget = new GUIContent("RayCast可见");
            spRaycastTarget = serializedObject.FindProperty("m_RaycastTarget");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();  //start

            EditorGUILayout.PropertyField(spMyPacker, cMyPacker, new GUILayoutOption[0]);

            EditorGUILayout.PropertyField(spUseAll, cUseAll);
            if (!spUseAll.boolValue)
            {
                EditorGUILayout.LabelField(cSpList);
                ++EditorGUI.indentLevel;
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("图层数量");
                var count = EditorGUILayout.DelayedIntField(spSpList.arraySize, new GUILayoutOption[0]);
                GUILayout.EndHorizontal();
                --EditorGUI.indentLevel;
                spSpList.arraySize = count;
                for (int i = 0; i < count; ++i)
                {
                    MySpritePackerTools.DrawAdvancedSpriteField(spMyPacker.objectReferenceValue as MySpritePacker, spSpList.GetArrayElementAtIndex(i).stringValue, SelectSprite, i, false);
                }
            }
            else
            {
                spSpList.ClearArray();
                spSpList.arraySize = 0;
            }

            EditorGUILayout.PropertyField(spTime, cTime, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(spDelay, cDelay, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(skKeyFrame, cKeyFrame, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(spCompleteEvent, cCompleteEvent, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(spLoopType, cLoopType, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(spAutoSize, cAutoSize, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(spAlpha, cAlpha, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(spRaycastTarget, cRaycastTarget, new GUILayoutOption[0]);
            serializedObject.ApplyModifiedProperties();//end
            base.OnInspectorGUI();
        }

        void SelectSprite(string spriteName, object param)
        {
            serializedObject.Update();            
            var idx = (int)param;
            spSpList.GetArrayElementAtIndex(idx).stringValue = spriteName;          
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(serializedObject.targetObject);
            MySpritePackerTools.selectedSprite = spriteName;
        }
    }
}
