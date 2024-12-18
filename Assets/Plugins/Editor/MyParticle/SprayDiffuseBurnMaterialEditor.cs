
using UnityEngine;
using UnityEditor;

namespace MyParticle
{
    public class SprayDiffuseBurnMaterialEditor : ShaderGUI
    {      
        MaterialProperty _colorMode;
        MaterialProperty _color;
        MaterialProperty _color2;
        MaterialProperty _mainTex;
        MaterialProperty _burnTex;
        MaterialProperty _LineWidth;
        MaterialProperty _BurnFirstColor;
        MaterialProperty _BurnSecondColor;

        bool _initial = true;

        void FindProperties(MaterialProperty[] props)
        {           
            _colorMode = FindProperty("_ColorMode", props);
            _color     = FindProperty("_Color", props);
            _color2    = FindProperty("_Color2", props);
            _mainTex   = FindProperty("_MainTex", props);

            _burnTex = FindProperty("_BurnMap", props);
            _LineWidth = FindProperty("_LineWidth", props);
            _BurnFirstColor = FindProperty("_BurnFirstColor", props);
            _BurnSecondColor = FindProperty("_BurnSecondColor", props);
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            FindProperties(properties);

            EditorGUILayout.LabelField("用于MyParticle的材质，漫反射和消融功能", EditorStyles.boldLabel);

            if (ShaderPropertiesGUI(materialEditor) || _initial)
            {

            }
                _initial = false;
        }

        bool ShaderPropertiesGUI(MaterialEditor materialEditor)
        {
            EditorGUI.BeginChangeCheck();
        
            materialEditor.ShaderProperty(_colorMode, "颜色模式");

            if (_colorMode.floatValue > 0)
            {
                var rect = EditorGUILayout.GetControlRect();
                rect.x += EditorGUIUtility.labelWidth;
                rect.width = (rect.width - EditorGUIUtility.labelWidth) / 2 - 2;
                materialEditor.ShaderProperty(rect, _color, "");
                rect.x += rect.width + 4;
                materialEditor.ShaderProperty(rect, _color2, "");
            }
            else
            {
                materialEditor.ShaderProperty(_color, " ");
            }

            materialEditor.ShaderProperty(_mainTex, "Texture");

            ++EditorGUI.indentLevel;
            materialEditor.ShaderProperty(_burnTex, "消融纹理");
            materialEditor.ShaderProperty(_LineWidth, "消融过渡宽度");
            materialEditor.ShaderProperty(_BurnFirstColor, "消融颜色1");
            materialEditor.ShaderProperty(_BurnSecondColor, "消融颜色2");
            --EditorGUI.indentLevel;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("GPU Instance", EditorStyles.miniLabel);
            (materialEditor.target as Material).enableInstancing = EditorGUILayout.Toggle((materialEditor.target as Material).enableInstancing);
            EditorGUILayout.EndHorizontal();



            return EditorGUI.EndChangeCheck();
        }

       
    }
}
