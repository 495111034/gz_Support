
using UnityEngine;
using UnityEditor;

namespace MyParticle
{
    public class SprayUnlitMaterialEditor : ShaderGUI
    {
        MaterialProperty _blendMode;
        MaterialProperty _colorMode;
        MaterialProperty _color;
        MaterialProperty _color2;
        MaterialProperty _mainTex;
      

        void FindProperties(MaterialProperty[] props)
        {
            _blendMode = FindProperty("_BlendMode", props);
            _colorMode = FindProperty("_ColorMode", props);
            _color     = FindProperty("_Color", props);
            _color2    = FindProperty("_Color2", props);
            _mainTex   = FindProperty("_MainTex", props);
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            FindProperties(properties);

            EditorGUILayout.LabelField("用于MyParticle的材质，alpha 混合透明材质", EditorStyles.boldLabel);

            if (ShaderPropertiesGUI(materialEditor))
            {
            }
        }

        bool ShaderPropertiesGUI(MaterialEditor materialEditor)
        {
            EditorGUI.BeginChangeCheck();

            materialEditor.ShaderProperty(_blendMode, "透明模式");
            materialEditor.ShaderProperty(_colorMode, "Color Mode");

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

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("GPU Instance", EditorStyles.miniLabel);
            (materialEditor.target as Material).enableInstancing = EditorGUILayout.Toggle((materialEditor.target as Material).enableInstancing);
            EditorGUILayout.EndHorizontal();



            return EditorGUI.EndChangeCheck();
        }

    }
}
