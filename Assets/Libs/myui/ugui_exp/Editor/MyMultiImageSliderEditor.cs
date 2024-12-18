using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(MyMultiImageSlider), true)]
    [CanEditMultipleObjects]
    public class MyMultiImageSliderEditor : Editor
    {
        SerializedProperty spBackImg;   //背景图
        SerializedProperty spBgFill;
        SerializedProperty spBackExt;
        SerializedProperty spAlpha;
        SerializedProperty spValue;
        SerializedProperty spSpList;
        SerializedProperty spMyPacker;
        SerializedProperty spRaycastTarget;
        SerializedProperty spSliRever;

        GUIContent cBackImg, cBgFill, cBackExt, cAlpha, cValue, cSpList, cMyPacker, cRaycastTarget, cSliRever;

        protected virtual void OnEnable()
        {
            cBackImg = new GUIContent("背景图");
            spBackImg = serializedObject.FindProperty("back_img");

            cBgFill = new GUIContent("背景空心(九宫格)");
            spBgFill = serializedObject.FindProperty("_bgNotFillCenter");

            cBackExt = new GUIContent("背景偏移量");
            spBackExt = serializedObject.FindProperty("back_ext");

            cAlpha = new GUIContent("透明度");
            spAlpha = serializedObject.FindProperty("alpha");

            cValue = new GUIContent("值");
            spValue = serializedObject.FindProperty("_value");

            cSpList = new GUIContent("图片列表(上面的排在底层)");
            spSpList = serializedObject.FindProperty("sprite_arr");

            cMyPacker = new GUIContent("图集");
            spMyPacker = serializedObject.FindProperty("spritePacker");

            cRaycastTarget = new GUIContent("RayCast可见");
            spRaycastTarget = serializedObject.FindProperty("m_RaycastTarget");

            cSliRever = new GUIContent("进度条翻转");
            spSliRever = serializedObject.FindProperty("m_x_reversal");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();  //start
            EditorGUILayout.PropertyField(spMyPacker, cMyPacker, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(spAlpha, cAlpha, new GUILayoutOption[0]);

            EditorGUILayout.PropertyField(spValue, cValue, new GUILayoutOption[0]);
            EditorGUILayout.LabelField(cBackImg);
            ++EditorGUI.indentLevel; ++EditorGUI.indentLevel;
            //EditorGUILayout.PropertyField(spBackImg, cBackImg, new GUILayoutOption[0]);
            MySpritePackerTools.DrawAdvancedSpriteField(spMyPacker.objectReferenceValue as MySpritePacker, spBackImg.stringValue, SelectSprite, null, false);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(spBgFill, cBgFill, new GUILayoutOption[0]);
            //EditorGUILayout.LabelField("仅九宫格有效");
            EditorGUILayout.EndHorizontal();            
            EditorGUILayout.PropertyField(spBackExt, cBackExt, true, new GUILayoutOption[0]);
            --EditorGUI.indentLevel; --EditorGUI.indentLevel;



            EditorGUILayout.PropertyField(spRaycastTarget, cRaycastTarget, new GUILayoutOption[0]);

            EditorGUILayout.LabelField(cSpList);
            ++EditorGUI.indentLevel;
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("图层数量");
            var count = EditorGUILayout.DelayedIntField(spSpList.arraySize, new GUILayoutOption[0]);
            GUILayout.EndHorizontal();
            spSpList.arraySize = count;
            for(int i = 0; i < count; ++i)
            {
                MySpritePackerTools.DrawAdvancedSpriteField(spMyPacker.objectReferenceValue as MySpritePacker, spSpList.GetArrayElementAtIndex(i).stringValue, SelectSprite, i, false);
            }

            EditorGUILayout.PropertyField(spSliRever, cSliRever);

            --EditorGUI.indentLevel;

            serializedObject.ApplyModifiedProperties();//end
           
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
