using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.AnimatedValues;
using System.Linq;
using UnityEngine.Events;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(PolygonImage), true)]
    [CanEditMultipleObjects]
    public class PolygonImageEditor : Editor
    {

        //SerializedProperty spMyPacker;
        //SerializedProperty spSpriteName;

        SerializedProperty spAlpha;
        SerializedProperty spSprite;
        SerializedProperty spRaycastTarget;
        SerializedProperty spTexture;
        SerializedProperty spNoTexShow;
        SerializedProperty spVerticess;
        SerializedProperty spRotation;
        SerializedProperty spColor;
        SerializedProperty spScale;

        //GUIContent cMyPacker, cSpriteName;
        GUIContent    cAlpha,  cSprite, cRaycastTarget, cTexture, cNoTexShow, cVerticess,CRotation, cColor, cScale;

        protected AnimBool m_ShowNativeSize;
        private GUIContent m_CorrectButtonContent;
        protected virtual void OnEnable()
        {
            //cMyPacker = new GUIContent("图集");
            //spMyPacker = serializedObject.FindProperty("spritePacker");

            //cSpriteName = new GUIContent("图片名");
            //spSpriteName = serializedObject.FindProperty("_spriteName");

            cAlpha = new GUIContent("透明度");
            spAlpha = serializedObject.FindProperty("alpha");


            cSprite = new GUIContent("原生Sprite");
            spSprite = serializedObject.FindProperty("_sprite");

            cTexture = new GUIContent("单图");
            spTexture = serializedObject.FindProperty("_tex");

            cNoTexShow = new GUIContent("无图也显示");
            spNoTexShow = serializedObject.FindProperty("_noTexShow");

            cVerticess = new GUIContent("顶点数");
            spVerticess = serializedObject.FindProperty("verticess");

            CRotation = new GUIContent("角度");
            spRotation = serializedObject.FindProperty("rotation");

            cColor = new GUIContent("颜色");
            spColor = serializedObject.FindProperty("_color");

            spScale = serializedObject.FindProperty("_scale");            
            cScale = new GUIContent("缩放");

            cRaycastTarget = new GUIContent("RayCast可见");
            spRaycastTarget = serializedObject.FindProperty("m_RaycastTarget");

            m_CorrectButtonContent = new GUIContent("适配图片尺寸", "设置和为图片一样的尺寸,对九宫格无效");
            m_ShowNativeSize = new AnimBool(false);
            m_ShowNativeSize.valueChanged.AddListener(new UnityAction(base.Repaint));

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();  //start  

            if (spSprite.objectReferenceValue == null)
            {
                EditorGUILayout.PropertyField(spTexture, cTexture, new GUILayoutOption[0]);
            }

            if (spTexture.objectReferenceValue == null)
            {
                EditorGUILayout.PropertyField(spSprite, cSprite, new GUILayoutOption[0]);

                //if (spSprite.objectReferenceValue == null)
                //{
                //    EditorGUILayout.LabelField("图集设置");
                //    ++EditorGUI.indentLevel;
                //    EditorGUILayout.PropertyField(spMyPacker, cMyPacker, new GUILayoutOption[0]);
                //    MySpritePackerTools.DrawAdvancedSpriteField(spMyPacker.objectReferenceValue as MySpritePacker, spSpriteName.stringValue, SelectSprite, null, false);
                //    --EditorGUI.indentLevel;
                //}
            }
            ++EditorGUI.indentLevel;
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(cVerticess);
            var count = EditorGUILayout.DelayedIntField(spVerticess.arraySize, new GUILayoutOption[0]);
            spVerticess.arraySize = count;
            GUILayout.EndHorizontal();
            if (count < 3)
            {
                EditorGUILayout.HelpBox("至少3条边才能形成图形", MessageType.Error);
            }
            else
            {
                for (int i = 0; i < count; ++i)
                {
                    ++EditorGUI.indentLevel;
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField($"顶点{i + 1}");
                    spVerticess.GetArrayElementAtIndex(i).floatValue = EditorGUILayout.Slider(spVerticess.GetArrayElementAtIndex(i).floatValue, 0f, 1f, new GUILayoutOption[0]);
                    GUILayout.EndHorizontal();
                    --EditorGUI.indentLevel;
                }
            }
          
            --EditorGUI.indentLevel;

            EditorGUILayout.PropertyField(spColor, cColor, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(spScale, cScale, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(spRotation, CRotation);
            EditorGUILayout.PropertyField(spNoTexShow, cNoTexShow);
            EditorGUILayout.PropertyField(spAlpha, cAlpha, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(spRaycastTarget, cRaycastTarget, new GUILayoutOption[0]);
          

            serializedObject.ApplyModifiedProperties();//end

        }

        void SelectSprite(string spriteName, object param)
        {
            serializedObject.Update();
            //spSpriteName.stringValue = spriteName;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(serializedObject.targetObject);
            MySpritePackerTools.selectedSprite = spriteName;
        }

     
    }
}
