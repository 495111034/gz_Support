
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace WeatherSystem
{
    internal static class DrawGUIHelper
    {
        /// <summary>Style for a small checkbox</summary>
        public static readonly GUIStyle smallTickbox = new GUIStyle("ShurikenToggle");
        /// <summary>Style for a small checkbox in mixed state</summary>
        public static readonly GUIStyle smallMixedTickbox = new GUIStyle("ShurikenToggle");


        public static readonly GUIContent[] m_guiContent = new[]
        {
            new GUIContent("Slider", "Use this to setting the same value for each time of day."),
            new GUIContent("Timeline Curve", "Use this to setting different values for each time of day based on the timeline."),
            new GUIContent("Property Type", "Sets the way how the property should behave."),
            new GUIContent("Color", "Use this to setting the same color for each time of day."),
            new GUIContent("Timeline Gradient", "Use this to setting different colors for each time of day based on the timeline."),
            new GUIContent("Slider X", "Use this to setting the same value for each time of day."),
            new GUIContent("Slider Y", "Use this to setting the same value for each time of day."),
            new GUIContent("Timeline Curve X", "Use this to setting different values for each time of day based on the timeline."),
            new GUIContent("Timeline Curve Y", "Use this to setting different values for each time of day based on the timeline."),
        };

        static private bool DrawOverrideCheckbox(SerializedProperty property)
        {
            SerializedProperty overrideState = property.FindPropertyRelative("overrideState");
           
            bool toggleValue = EditorGUILayout.Toggle(EditorGUIUtility.TrTextContent("", "Override this setting for this volume."), overrideState.boolValue, smallTickbox , new []{ GUILayout.Width(50)});
            overrideState.boolValue = toggleValue;
            return overrideState.boolValue;
        }

        static public void DrawBoolProperty(SerializedProperty property, GUIContent content)
        {
            EditorGUILayout.PropertyField(property, content);
        }

        static public void DrawTextureProperty(SerializedProperty property, GUIContent content)
        {
            EditorGUILayout.PropertyField(property, content);
        }

        static public void DrawFloatProperty(SerializedProperty property, float minValue, float maxValue, float defaultValue, GUIContent content , bool showCheck = false)
        {
            bool isCheck = true; 
            EditorGUILayout.BeginHorizontal();
            if (showCheck)
            {
                isCheck = DrawOverrideCheckbox(property);
                GUILayout.Space(10);
            }
            using (new EditorGUI.DisabledScope(!isCheck))
            {
                property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, content);
                if (GUILayout.Button(EditorGUIUtility.IconContent("Refresh"), EditorStyles.miniButton, GUILayout.Width(50)))
                {
                    property.FindPropertyRelative("type").enumValueIndex = 0;
                    property.FindPropertyRelative("slider").floatValue = defaultValue;
                    property.FindPropertyRelative("timelineCurve").animationCurveValue = AnimationCurve.Linear(0.0f, defaultValue, 24.0f, defaultValue);
                }
            }
    
            EditorGUILayout.EndHorizontal();

            if (property.isExpanded && isCheck)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(property.FindPropertyRelative("type"), m_guiContent[2]);
                switch (property.FindPropertyRelative("type").enumValueIndex)
                {
                    case 0:                        
                        EditorGUILayout.Slider(property.FindPropertyRelative("slider"), minValue, maxValue, m_guiContent[0]);
                        maxValue -= minValue;
                        break;
                    case 1:
                        maxValue -= minValue;
                        EditorGUILayout.CurveField(property.FindPropertyRelative("timelineCurve"), Color.green, new Rect(0, minValue, 24, maxValue), m_guiContent[1]);
                        break;
                }
                EditorGUI.indentLevel--;
            }
        }

        static public void DrawIntProperty(SerializedProperty property, int minValue, int maxValue, int defaultValue, GUIContent content, bool showCheck = false)
        {
            bool isCheck = true;
            EditorGUILayout.BeginHorizontal();
            if (showCheck)
            {
                isCheck = DrawOverrideCheckbox(property);
                GUILayout.Space(10);
            }
            using (new EditorGUI.DisabledScope(!isCheck))
            {
                property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, content);
                if (GUILayout.Button(EditorGUIUtility.IconContent("Refresh"), EditorStyles.miniButton, GUILayout.Width(50)))
                {
                    property.FindPropertyRelative("type").enumValueIndex = 0;
                    property.FindPropertyRelative("slider").intValue = defaultValue;
                    property.FindPropertyRelative("timelineCurve").animationCurveValue = AnimationCurve.Linear(0.0f, defaultValue, 24.0f, defaultValue);
                }
            }

            EditorGUILayout.EndHorizontal();

            if (property.isExpanded && isCheck)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(property.FindPropertyRelative("type"), m_guiContent[2]);
                switch (property.FindPropertyRelative("type").enumValueIndex)
                {
                    case 0:
                        EditorGUILayout.IntSlider(property.FindPropertyRelative("slider"), minValue, maxValue, m_guiContent[0]);
                        maxValue -= minValue;
                        break;
                    case 1:
                        maxValue -= minValue;
                        EditorGUILayout.CurveField(property.FindPropertyRelative("timelineCurve"), Color.green, new Rect(0, minValue, 24, maxValue), m_guiContent[1]);
                        break;
                }
                EditorGUI.indentLevel--;
            }
        }

        static public void DrawVector2Property(SerializedProperty property, Vector2 XRange, Vector2 YRange, Vector2 defaultValue, GUIContent content, bool showCheck = false)
        {
            bool isCheck = true;
            EditorGUILayout.BeginHorizontal();
            if (showCheck)
            {
                isCheck = DrawOverrideCheckbox(property);
                GUILayout.Space(10);
            }
            using (new EditorGUI.DisabledScope(!isCheck))
            {
                property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, content);
                if (GUILayout.Button(EditorGUIUtility.IconContent("Refresh"), EditorStyles.miniButton, GUILayout.Width(50)))
                {
                    property.FindPropertyRelative("type").enumValueIndex = 0;
                    property.FindPropertyRelative("sliderX").floatValue = defaultValue.x;
                    property.FindPropertyRelative("timelineCurveX").animationCurveValue = AnimationCurve.Linear(0.0f, defaultValue.x, 24.0f, defaultValue.x);

                    property.FindPropertyRelative("sliderY").floatValue = defaultValue.y;
                    property.FindPropertyRelative("timelineCurveY").animationCurveValue = AnimationCurve.Linear(0.0f, defaultValue.y, 24.0f, defaultValue.y);
                }
            }

            EditorGUILayout.EndHorizontal();

            if (property.isExpanded && isCheck)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(property.FindPropertyRelative("type"), m_guiContent[2]);
                switch (property.FindPropertyRelative("type").enumValueIndex)
                {
                    case 0:
                        {
                            EditorGUILayout.Slider(property.FindPropertyRelative("sliderX"), XRange.x, XRange.y, m_guiContent[5]);
                            EditorGUILayout.Slider(property.FindPropertyRelative("sliderY"), YRange.x, YRange.y, m_guiContent[6]);
                            break;
                        }
                    case 1:
                        {
                            float minValue = XRange.x;
                           float maxValue = XRange.y - XRange.x;
                            EditorGUILayout.CurveField(property.FindPropertyRelative("timelineCurveX"), Color.green, new Rect(0, minValue, 24, maxValue), m_guiContent[7]);
                            
                            minValue = YRange.x;
                            maxValue = YRange.y - YRange.x;
                            EditorGUILayout.CurveField(property.FindPropertyRelative("timelineCurveY"), Color.green, new Rect(0, minValue, 24, maxValue), m_guiContent[8]);
                            break;
                        }
                }
                EditorGUI.indentLevel--;
            }
        }

        static public void DrawColorProperty(WeatherSystemProfileScript target , SerializedProperty property, ref WeatherSystemColorProperty targetProperty, GUIContent content, bool showCheck = false)
        {
            bool isCheck = true;

            EditorGUILayout.BeginHorizontal();
            if (showCheck)
            {
                isCheck = DrawOverrideCheckbox(property);
                GUILayout.Space(10);
            }

            using (new EditorGUI.DisabledScope(!isCheck))
            {
                property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, content);
                if (GUILayout.Button(EditorGUIUtility.IconContent("Refresh"), EditorStyles.miniButton, GUILayout.Width(50)))
                {
                    Undo.RecordObject(target, "Undo WeatherSystem Sky Profile");
                    targetProperty = new WeatherSystemColorProperty
                    (
                        Color.white,
                        new Gradient()
                    );
                }
            }

            EditorGUILayout.EndHorizontal();

            if (property.isExpanded && isCheck)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(property.FindPropertyRelative("type"), m_guiContent[2]);
                switch (property.FindPropertyRelative("type").enumValueIndex)
                {
                    case 0:
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("color"), m_guiContent[3]);
                        break;
                    case 1:
                        EditorGUILayout.PropertyField(property.FindPropertyRelative("timelineGradient"), m_guiContent[4]);
                        break;
                }
                EditorGUI.indentLevel--;
            }
        }

        static public void DrawCommonProperty(SerializedProperty property, GUIContent content)
        {
            EditorGUILayout.PropertyField(property, content);
        }

        static public void DrawListProperty(SerializedProperty property, GUIContent content)
        {
            EditorGUILayout.PropertyField(property, content, true);
        }

        static public void DrawLable(GUIContent content)
        {
            EditorGUILayout.LabelField(content);
        }

        static public void DrawSpace()
        {
            EditorGUILayout.Space();
        }

      

        static bool CheckGradientKeyChange(WeatherSystemVector4Property weatherSystemVector4, Gradient gradient)
        {
            GradientColorKey[] gradientColorKey = gradient.colorKeys;
            GradientAlphaKey[] gradientAphlaKey = gradient.alphaKeys;

            int length = gradientColorKey.Length;

            List<Vector4GradientKey> gradientColorKeys = weatherSystemVector4.GetGradient();
            if (length != gradientColorKeys.Count)
            {
                return true;
            }
            for (int i = 0; i < length; i++)
            {
                Vector4GradientKey originKey = gradientColorKeys[i];
                GradientColorKey colorKey = gradientColorKey[i];
                float alpha = 0;
                if (i < gradientAphlaKey.Length)
                {
                    GradientAlphaKey alphaKey = gradientAphlaKey[i];
                    alpha = alphaKey.alpha;
                }
                Vector4 vect4 = new Vector4(colorKey.color.r , colorKey.color.g , colorKey.color.b , alpha);
                float time = (float)Math.Round(colorKey.time, 3);
                if ((originKey.time != time) || (originKey.value != vect4))
                {
                    return true;
                }
            }
            return false;
        }

        static Gradient Vector4GardientToColorGradient(WeatherSystemVector4Property weatherSystemVector4)
        {
            List<Vector4GradientKey> lstGradient = weatherSystemVector4.GetGradient();
            Gradient gradient = new Gradient();
            int length = lstGradient.Count;
            GradientColorKey[] colorKeys = new GradientColorKey[length];
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[length];
            for (int i = 0; i < length; i++)
            {
                GradientColorKey colorkey = new GradientColorKey();
                GradientAlphaKey alphaKey = new GradientAlphaKey();

                Vector4GradientKey vectKey = lstGradient[i];
                colorkey.time = vectKey.time;
                colorkey.color = vectKey.value;

                alphaKey.time = vectKey.time;
                alphaKey.alpha = vectKey.value.w;

                colorKeys[i] = colorkey;
                alphaKeys[i] = alphaKey;
            }
            gradient.SetKeys(colorKeys , alphaKeys);
            return gradient;
        }

        static Vector3 GetLiftValue(Vector4 x) => new Vector3(x.x + x.w, x.y + x.w, x.z + x.w);
    }
}
