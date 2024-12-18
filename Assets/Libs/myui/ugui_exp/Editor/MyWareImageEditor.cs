using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.AnimatedValues;
using System.Linq;
using UnityEngine.Events;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(MyWareImage), true)]
    [CanEditMultipleObjects]
    public class MyWareImageEditor : Editor
    {
        //GUIContent cMyPacker;
        //SerializedProperty spMyPacker;

        //GUIContent cSpriteName;
        //SerializedProperty spSpriteName;

        SerializedProperty spSprite;
        SerializedProperty spTexture;
        SerializedProperty spScale;
        SerializedProperty spWareStrong;
        SerializedProperty spWertexDensity;
        SerializedProperty spSaveTextureToAb;

        GUIContent cSprite, cTexture, cScale ,cWareStrong, cWertexDensity, cSaveTexture;

        protected virtual void OnEnable()
        {
            //cMyPacker = new GUIContent("图集");
            //spMyPacker = serializedObject.FindProperty("spritePacker");

            //cSpriteName = new GUIContent("图片名");
            //spSpriteName = serializedObject.FindProperty("_spriteName");

            cSprite = new GUIContent("原生Sprite");
            spSprite = serializedObject.FindProperty("_sprite");

            cTexture = new GUIContent("单图");
            spTexture = serializedObject.FindProperty("_tex");

            cScale = new GUIContent("缩放");
            spScale = serializedObject.FindProperty("_scale");

            cWareStrong = new GUIContent("摆动强度");
            spWareStrong = serializedObject.FindProperty("_wareStrong");

            cWertexDensity = new GUIContent("最长网格长度");
            spWertexDensity = serializedObject.FindProperty("_vertexDensity");

            cSaveTexture = new GUIContent("保存到ab");
            spSaveTextureToAb = serializedObject.FindProperty("autoLoadtexture");

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (spSprite.objectReferenceValue == null)
            {
                EditorGUILayout.PropertyField(spTexture, cTexture, new GUILayoutOption[0]);
            }

            if (spTexture.objectReferenceValue == null)
            {
                EditorGUILayout.PropertyField(spSprite, cSprite, new GUILayoutOption[0]);

                /*
                if (spSprite.objectReferenceValue == null)
                {
                    EditorGUILayout.LabelField("图集设置");
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.PropertyField(spMyPacker, cMyPacker, new GUILayoutOption[0]);
                    MySpritePackerTools.DrawAdvancedSpriteField(spMyPacker.objectReferenceValue as MySpritePacker, spSpriteName.stringValue, SelectSprite, null, false);
                    --EditorGUI.indentLevel;
                }*/
            }
            EditorGUILayout.PropertyField(spScale, cScale, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(spWareStrong, cWareStrong, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(spWertexDensity, cWertexDensity, new GUILayoutOption[0]);

            serializedObject.ApplyModifiedProperties();//end

        }

        /*
        void SelectSprite(string spriteName, object param)
        {
            serializedObject.Update();
            spSpriteName.stringValue = spriteName;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(serializedObject.targetObject);
            MySpritePackerTools.selectedSprite = spriteName;
        }*/

        private float time = 0;
        private float lasttime = 0;

        private void Awake()
        {
            time = 0;
            lasttime = Time.realtimeSinceStartup;
            EditorApplication.update += UpdateHandler;
        }

        private void OnDestroy()
        {
            EditorApplication.update -= UpdateHandler;
        }

        void UpdateHandler()
        {
            if (!Application.isPlaying)
            {
                var s = target as MyWareImage;
                float deltaTime = Time.realtimeSinceStartup - lasttime;
                lasttime = Time.realtimeSinceStartup;

                time += deltaTime;
                s.Update_Editor(time);

            }
        }
    }
}
