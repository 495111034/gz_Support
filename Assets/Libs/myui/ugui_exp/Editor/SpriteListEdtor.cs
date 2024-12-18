using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(SpriteList), true)]
    [CanEditMultipleObjects]
    public class SpriteListEdtor : Editor
    {
        SerializedProperty spGap;   //间距
        SerializedProperty spBackImg;   //背景图
        SerializedProperty spBackExt;
        SerializedProperty spAlpha;
        SerializedProperty spSpList;
        SerializedProperty spMyPacker;
        SerializedProperty spRaycastTarget;

        GUIContent cGap, cBackImg, cBackExt, cAlpha, cSpList, cMyPacker, cRaycastTarget;

        protected virtual void OnEnable()
        {

            cGap = new GUIContent("间距");
            spGap = serializedObject.FindProperty("_gap");

            cBackImg = new GUIContent("背景图");
            spBackImg = serializedObject.FindProperty("back_img");

            cBackExt = new GUIContent("背景偏移量");
            spBackExt = serializedObject.FindProperty("back_ext");

            cAlpha = new GUIContent("透明度");
            spAlpha = serializedObject.FindProperty("alpha");

            cSpList = new GUIContent("图片列表");
            spSpList = serializedObject.FindProperty("sprite_arr");

            cMyPacker = new GUIContent("图集");
            spMyPacker = serializedObject.FindProperty("spritePacker");

            cRaycastTarget = new GUIContent("RayCast可见");
            spRaycastTarget = serializedObject.FindProperty("m_RaycastTarget");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();  //start

            EditorGUILayout.PropertyField(spMyPacker, cMyPacker, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(spGap, cGap, new GUILayoutOption[0]);
            //EditorGUILayout.PropertyField(spBackImg, cBackImg, new GUILayoutOption[0]);
            EditorGUILayout.LabelField(cBackImg);            
            MySpritePackerTools.DrawAdvancedSpriteField(spMyPacker.objectReferenceValue as MySpritePacker, spBackImg.stringValue, SelectSprite, null, false);

            EditorGUILayout.PropertyField(spBackExt, cBackExt, true, new GUILayoutOption[0]);           
            EditorGUILayout.PropertyField(spAlpha, cAlpha, new GUILayoutOption[0]);

            EditorGUILayout.LabelField(cSpList);
            ++EditorGUI.indentLevel;
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("图层数量");
            var count = EditorGUILayout.DelayedIntField(spSpList.arraySize, new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            spSpList.arraySize = count;
            for (int i = 0; i < count; ++i)
            {
                MySpritePackerTools.DrawAdvancedSpriteField(spMyPacker.objectReferenceValue as MySpritePacker, spSpList.GetArrayElementAtIndex(i).stringValue, SelectSprite, i, false);
            }


            --EditorGUI.indentLevel;

            

            EditorGUILayout.PropertyField(spRaycastTarget, cRaycastTarget, new GUILayoutOption[0]);

            serializedObject.ApplyModifiedProperties();//end
           // base.OnInspectorGUI();
        }


        void SelectSprite(string spriteName, object param)
        {
            serializedObject.Update();
            if (param != null)
            {
                var idx = (int)param;
                spSpList.GetArrayElementAtIndex(idx).stringValue = spriteName;
            }
            else
            {
                spBackImg.stringValue = spriteName;
            }
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(serializedObject.targetObject);
            MySpritePackerTools.selectedSprite = spriteName;
        }
    }
}
