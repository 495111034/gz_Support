using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(MyRawImage), true)]
    [CanEditMultipleObjects]
    public class MyRawImageEditor : RawImageEditor
    {
        SerializedProperty m_LoadTypeProperty;
        GUIContent m_LoadTypeContent;

        protected override void OnEnable()
        {
            base.OnEnable();
            m_LoadTypeContent = new GUIContent("加载类型");
            m_LoadTypeProperty = serializedObject.FindProperty("m_LoadType");
        }
        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(m_LoadTypeProperty, m_LoadTypeContent);
            serializedObject.ApplyModifiedProperties();
            base.OnInspectorGUI();
        }
    }

    [CustomEditor(typeof(Image), true)]
    [CanEditMultipleObjects]
    public class ImageExtEditor : ImageEditor
    {
        SerializedProperty spSprite;
        GUIContent cSprite;
        Image myImage = null;
        protected override void OnEnable()
        {
            base.OnEnable();
            myImage = target as Image;
            spSprite = serializedObject.FindProperty("m_Sprite");
        }

        Sprite _log;
        public override void OnInspectorGUI()
        {
            if (myImage && myImage.sprite && !Application.isPlaying) 
            {
                if (_log != myImage.sprite)
                {
                    var path = UnityEditor.AssetDatabase.GetAssetPath(_log = myImage.sprite).ToLower();
                    if (path.StartsWith(PathDefs.ASSETS_PATH_GUI_SPRITES))
                    {
                        var log = $"原生Image组件不允许使用图集中的图片！会引起内存和性能问题。请更换MyImage组件使用图集图片！\npath={myImage.gameObject.GetLocation()}\nsprite={path}";
                        Log.LogError(log);
                        EditorUtility.DisplayDialog("Image图集错误", log, "确定");
                    }
                }
            }
            base.OnInspectorGUI();
        }
    }

    [CustomEditor(typeof(MyImage), true)]
    [CanEditMultipleObjects]
    public class MyImageEditor : ImageEditor
    {
        MyImage myImage = null;

        SerializedProperty spSprite;
        GUIContent cSprite;
        //string cSpriteText;

        SerializedProperty spMyPacker;
        GUIContent cMyPacker;

        SerializedProperty spSpriteName;

        SerializedProperty spLineColorShader;
        GUIContent cLineColorShader;
        //GUIContent cSpriteName;


        SerializedProperty sp_x_reversal;
        GUIContent gc_x_reversal;
        SerializedProperty sp_y_reversal;
        GUIContent gc_y_reversal;

        protected override void OnEnable()
        {
            base.OnEnable();

            myImage = target as MyImage;

            spSprite = serializedObject.FindProperty("m_Sprite");

            cMyPacker = new GUIContent("图集");
            spMyPacker = serializedObject.FindProperty("_sp_packer");

            cLineColorShader = new GUIContent("使用线性透明度");
            spLineColorShader = serializedObject.FindProperty("_useLineShader");

            //cSpriteName = new GUIContent("图片名");
            spSpriteName = serializedObject.FindProperty("_sp_name");

            var f = typeof(ImageEditor).GetField("m_SpriteContent", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            cSprite = f.GetValue(this) as GUIContent;
            //cSpriteText = cSprite.text;


            sp_x_reversal = serializedObject.FindProperty("_x_reversal");
            gc_x_reversal = new GUIContent("x轴翻转");

            sp_y_reversal = serializedObject.FindProperty("_y_reversal");
            gc_y_reversal = new GUIContent("y轴翻转");

        }

        public override void OnInspectorGUI()
        {
            var packerValue = spMyPacker.objectReferenceValue;
            //if (spSprite.objectReferenceValue == null || spMyPacker.objectReferenceValue != null)
            {
                serializedObject.Update();
                ++EditorGUI.indentLevel;
                EditorGUILayout.PropertyField(spMyPacker, cMyPacker, new GUILayoutOption[0]);
                MySpritePackerTools.DrawAdvancedSpriteField(spMyPacker.objectReferenceValue as MySpritePacker, spSpriteName.stringValue, SelectSprite, null, false);
                --EditorGUI.indentLevel;
                if (packerValue != spMyPacker.objectReferenceValue)
                {
                    //spSpriteName.stringValue = null;
                    //Log.Log2File($"reset spSprite from {spSprite.objectReferenceValue}");
                    spSprite.objectReferenceValue = null;
                }
                EditorGUILayout.PropertyField(sp_x_reversal, gc_x_reversal, new GUILayoutOption[0]);
                EditorGUILayout.PropertyField(sp_y_reversal, gc_y_reversal, new GUILayoutOption[0]);

                EditorGUILayout.PropertyField(spLineColorShader, cLineColorShader);
                serializedObject.ApplyModifiedProperties();
            }

            if (spMyPacker.objectReferenceValue != null)
            {
                cSprite.text = "引用图集 sprite";
            }
            else
            {
                cSprite.text = "原生 sprite";
            }

            var spSpriteValue = spSprite.objectReferenceValue;
            base.OnInspectorGUI();
            if (spSpriteValue != spSprite.objectReferenceValue)
            {
                //Log.Log2File($"reset spMyPacker from {spMyPacker.objectReferenceValue}, sprite form {spSpriteValue} to {spSprite.objectReferenceValue}");
                spMyPacker.objectReferenceValue = null;                
                serializedObject.ApplyModifiedProperties();
            }
        }

        void SelectSprite(string spriteName, object param)
        {
            serializedObject.Update();
            spSpriteName.stringValue = spriteName;
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(serializedObject.targetObject);
            MySpritePackerTools.selectedSprite = spriteName;
        }
    }
}
