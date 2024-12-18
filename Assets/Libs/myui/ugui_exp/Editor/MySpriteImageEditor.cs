using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEditor.AnimatedValues;
using System.Linq;
using UnityEngine.Events;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(MySpriteImage), true)]
    [CanEditMultipleObjects]
    public class MySpriteImageEditor : Editor
    {
        SerializedProperty spMyPacker;
        GUIContent cMyPacker;
        
        SerializedProperty spSpriteName;
        SerializedProperty spAlpha;
        SerializedProperty spFill;
        SerializedProperty spSprite;
        SerializedProperty spRaycastTarget;
        SerializedProperty spTexture;
        SerializedProperty spNoTexShow;
        SerializedProperty spLineColorShader;
        SerializedProperty spColor;
        SerializedProperty spScale;
        SerializedProperty spSaveTextureToAb;
        SerializedProperty spAutoAnchor;
        SerializedProperty spAsScaleAsParent;
        SerializedProperty spanchorScreenType;
        SerializedProperty spshouldPreserveAspect;
        SerializedProperty spFade;
        SerializedProperty spXReversal;
        SerializedProperty spYReversal;
        SerializedProperty _isBackground;

        GUIContent cSpriteName, cAlpha, cFill,cSprite, cRaycastTarget,cTexture,cNoTexShow,cLineColorShader,cColor,cScale,cSaveTexture;
        GUIContent cAutoAnchor;
        GUIContent cAsScaleAsParent;
        GUIContent C_shouldPreserveAspect;
        GUIContent cFade;
        GUIContent cXReversal, cYReversal;
        GUIContent isBackgroundText;

        protected AnimBool m_ShowNativeSize;
        private GUIContent m_CorrectButtonContent;
        protected virtual void OnEnable()
        {
            cMyPacker = new GUIContent("图集");
            spMyPacker = serializedObject.FindProperty("_sp_packer");

            cSpriteName = new GUIContent("图片名");
            spSpriteName = serializedObject.FindProperty("_sp_name");

            cAlpha = new GUIContent("透明度");
            spAlpha = serializedObject.FindProperty("alpha");

            cFill = new GUIContent("空心(九宫格)");
            spFill = serializedObject.FindProperty("_bgNotFillCenter");

            cSprite = new GUIContent("原生Sprite");
            spSprite = serializedObject.FindProperty("_sprite");

            cFade = new GUIContent("灰阶显示");
            spFade = serializedObject.FindProperty("_fade");

            cTexture = new GUIContent("单图");
            spTexture = serializedObject.FindProperty("_tex");

            C_shouldPreserveAspect = new GUIContent("保持原始宽高比");
            spshouldPreserveAspect = serializedObject.FindProperty("_shouldPreserveAspect");

            cSaveTexture = new GUIContent("保存到ab");
            spSaveTextureToAb = serializedObject.FindProperty("autoLoadtexture");

            cNoTexShow = new GUIContent("无图也显示");
            spNoTexShow = serializedObject.FindProperty("_noTexShow");

            cLineColorShader = new GUIContent("使用线性透明度");
            spLineColorShader = serializedObject.FindProperty("_useLineShader");

            cRaycastTarget = new GUIContent("RayCast可见");
            spRaycastTarget = serializedObject.FindProperty("m_RaycastTarget");

            spColor = serializedObject.FindProperty("_color");
            cColor = new GUIContent("颜色");
            cScale = new GUIContent("缩放");
            cAsScaleAsParent = new GUIContent("随上级缩放");
            spScale = serializedObject.FindProperty("_scale");
            spAsScaleAsParent = serializedObject.FindProperty("asScaleAsParent");

            m_CorrectButtonContent = new GUIContent("适配图片尺寸", "设置和为图片一样的尺寸,对九宫格无效");
            m_ShowNativeSize = new AnimBool(false);
            m_ShowNativeSize.valueChanged.AddListener(new UnityAction(base.Repaint));

            spAutoAnchor = serializedObject.FindProperty("_anchorScreen");
            cAutoAnchor = new GUIContent("对齐屏幕");
            spanchorScreenType = serializedObject.FindProperty("anchorScreenType");

            cXReversal = new GUIContent("X轴镜像翻转");
            spXReversal = serializedObject.FindProperty("m_x_reversal");

            cYReversal = new GUIContent("Y轴镜像翻转");
            spYReversal = serializedObject.FindProperty("m_y_reversal");

            _isBackground = serializedObject.FindProperty("_isBackground");
            isBackgroundText = new GUIContent("不透明图");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();  //start  

            DrawUIs();

            serializedObject.ApplyModifiedProperties();//end

        }

        protected void DrawUIs()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(spFill, cFill, new GUILayoutOption[0]);
            //EditorGUILayout.LabelField("(仅九宫格有效)");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(spColor, cColor, new GUILayoutOption[0]);
            EditorGUILayout.PropertyField(spshouldPreserveAspect, C_shouldPreserveAspect, new GUILayoutOption[0]);

            var myspriteiamge = target as MySpriteImage;
            //if (spSprite.objectReferenceValue == null && spMyPacker.objectReferenceValue == null)
            {
                var old = spTexture.objectReferenceValue;
                cTexture.text = $"单图({myspriteiamge.TextureName})";
                EditorGUILayout.PropertyField(spTexture, cTexture, new GUILayoutOption[0]);
                if (old != spTexture.objectReferenceValue) 
                {
                    spSprite.objectReferenceValue = spMyPacker.objectReferenceValue = null;
                }
            }
            //if (spTexture.objectReferenceValue == null)// && spMyPacker.objectReferenceValue == null
            {
                var old = spSprite.objectReferenceValue;
                cSprite.text = $"原生({myspriteiamge.SpriteName})";
                EditorGUILayout.PropertyField(spSprite, cSprite, new GUILayoutOption[0]);
                if (old != spSprite.objectReferenceValue)
                {
                    spTexture.objectReferenceValue = spMyPacker.objectReferenceValue = null;
                }
            }
            //if (spTexture.objectReferenceValue == null && spSprite.objectReferenceValue == null)
            {
                var old = spMyPacker.objectReferenceValue;
                cMyPacker.text = $"图集({myspriteiamge.PackerName})";
                EditorGUILayout.PropertyField(spMyPacker, cMyPacker, new GUILayoutOption[0]);
                if (old != spMyPacker.objectReferenceValue)
                {
                    spTexture.objectReferenceValue = spSprite.objectReferenceValue = null;
                }
            }
            if(spMyPacker.objectReferenceValue != null)
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.LabelField("图集设置");                                
                MySpritePackerTools.DrawAdvancedSpriteField(spMyPacker.objectReferenceValue as MySpritePacker, spSpriteName.stringValue, SelectSprite, null, false);
                --EditorGUI.indentLevel;
            }
            

            EditorGUILayout.PropertyField(spSaveTextureToAb, cSaveTexture, new GUILayoutOption[0]);            

            EditorGUILayout.PropertyField(spFade, cFade);

            EditorGUILayout.PropertyField(spNoTexShow, cNoTexShow);
            EditorGUILayout.PropertyField(spLineColorShader, cLineColorShader); 
            EditorGUILayout.PropertyField(spAsScaleAsParent, cAsScaleAsParent, new GUILayoutOption[0]);
            if (spAsScaleAsParent.boolValue || (spAutoAnchor.boolValue && (target as MySpriteImage).AnchorToScreenType == MySpriteImage.AnchorScreenType.AspectRatioCut))
            {
                ++EditorGUI.indentLevel;
                EditorGUILayout.LabelField($"缩放：{spScale.floatValue.ToString()}", new GUILayoutOption[0]);
                --EditorGUI.indentLevel;
            }
            else
                EditorGUILayout.PropertyField(spScale, cScale, new GUILayoutOption[0]);

            EditorGUILayout.PropertyField(_isBackground, isBackgroundText, new GUILayoutOption[0]);

            if (!_isBackground.boolValue)
                EditorGUILayout.PropertyField(spAlpha, cAlpha, new GUILayoutOption[0]);

            EditorGUILayout.PropertyField(spRaycastTarget, cRaycastTarget, new GUILayoutOption[0]);



            if ((target as Graphic).transform.parent.GetComponent<Canvas>())
            {
                spAutoAnchor.boolValue = EditorGUILayout.BeginToggleGroup(cAutoAnchor, spAutoAnchor.boolValue);
                ++EditorGUI.indentLevel;

                EditorGUILayout.BeginHorizontal();
                if (EditorGUILayout.Toggle((target as MySpriteImage).AnchorToScreenType == MySpriteImage.AnchorScreenType.AspectRatioCut))
                    (target as MySpriteImage).AnchorToScreenType = MySpriteImage.AnchorScreenType.AspectRatioCut;
                EditorGUILayout.LabelField("保持宽高比允许裁剪（边缘可能超出屏幕）");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (EditorGUILayout.Toggle((target as MySpriteImage).AnchorToScreenType == MySpriteImage.AnchorScreenType.AspectRatioNotCut))
                    (target as MySpriteImage).AnchorToScreenType = MySpriteImage.AnchorScreenType.AspectRatioNotCut;
                EditorGUILayout.LabelField("保持宽高比不裁剪（边缘可能有空白）");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (EditorGUILayout.Toggle((target as MySpriteImage).AnchorToScreenType == MySpriteImage.AnchorScreenType.FullScreen))
                    (target as MySpriteImage).AnchorToScreenType = MySpriteImage.AnchorScreenType.FullScreen;
                EditorGUILayout.LabelField("全屏（不保持宽高比）");
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (EditorGUILayout.Toggle((target as MySpriteImage).AnchorToScreenType == MySpriteImage.AnchorScreenType.FullScreenCut))
                    (target as MySpriteImage).AnchorToScreenType = MySpriteImage.AnchorScreenType.FullScreenCut;
                EditorGUILayout.LabelField("全屏（保持宽高比，边缘可能超出屏幕）");
                EditorGUILayout.EndHorizontal();

                --EditorGUI.indentLevel;
                EditorGUILayout.EndToggleGroup();
            }
            EditorGUILayout.PropertyField(spXReversal, cXReversal);
            EditorGUILayout.PropertyField(spYReversal, cYReversal);

            SetShowNativeSize(true, true);
            NativeSizeButtonGUI();
        }

        void SelectSprite(string spriteName,object param)
        {           
            serializedObject.Update();
            spSpriteName.stringValue = spriteName;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(serializedObject.targetObject);
            MySpritePackerTools.selectedSprite = spriteName;
        }

        protected void SetShowNativeSize(bool show, bool instant)
        {
            if (instant)
            {
                this.m_ShowNativeSize.value = (show);
            }
            else
            {
                this.m_ShowNativeSize.target = (show);
            }
        }

        protected void NativeSizeButtonGUI()
        {
            if (EditorGUILayout.BeginFadeGroup(m_ShowNativeSize.faded))
            {
                EditorGUILayout.BeginHorizontal(new GUILayoutOption[0]);
                GUILayout.Space(EditorGUIUtility.labelWidth);
                if (GUILayout.Button(this.m_CorrectButtonContent, EditorStyles.miniButton, new GUILayoutOption[0]))
                {
                    foreach (Graphic current in
                        from obj in targets
                        select obj as Graphic)
                    {
                        Undo.RecordObject(current.rectTransform, "适配图片尺寸");
                        current.SetNativeSize();
                        EditorUtility.SetDirty(current);
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndFadeGroup();
        }
    }
}
