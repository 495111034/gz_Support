using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Rendering;

namespace WeatherSystem
{

    

    [CustomEditor(typeof(WeatherSystemProfileScript))]
    public class WeatherSystemProfileScriptInspector : Editor
    {
        // Editor only
        private WeatherSystemProfileScript m_target;
        public Texture2D logoTexture;
        private Rect m_controlRect;
        private readonly Color m_greenColor = new Color(0.85f, 1.0f, 0.85f);
        private readonly Color m_redColor = new Color(1.0f, 0.75f, 0.75f);

        // Tab folds

        private bool m_showSkyGroup = true;
        private bool m_showCloudsGroup = true;
        private bool m_showFogGroup = true;
        private bool m_showWeatherGroup = true;
        private bool m_showRainGroup = true;
        private bool m_showLightningGroup = true;
        
        // Serialized properties
        private SerializedProperty  m_skyProfile          ;
        private SerializedProperty  m_timeOfDayOffset;
        private SerializedProperty  m_sunDirection        ;
        private SerializedProperty  m_sunXDirection        ;
        private SerializedProperty  m_zenithColor         ;
        private SerializedProperty  m_horizonColor        ;
        private SerializedProperty  m_horizonFalloff      ;
        private SerializedProperty  m_mieScatterColor     ;
        private SerializedProperty  m_mieScatterFactor    ;
        private SerializedProperty  m_mieScatterPower;
        
        private SerializedProperty m_skyCloudMap;
        private SerializedProperty m_skyCloudMapOffset;
        private SerializedProperty m_skyCloudMask;
        private SerializedProperty m_distortionTile;
        private SerializedProperty m_distortionAmount;
        private SerializedProperty m_distortionMirror;
        
        private SerializedProperty  m_sunEnabled          ;
        private SerializedProperty  m_sunBrightness       ;
        private SerializedProperty  m_sunDetail           ;
        private SerializedProperty  m_sunDistance         ;
        private SerializedProperty  m_sunColor            ;
        
        private SerializedProperty  m_moonEnabled         ;
        private SerializedProperty  m_moonBrightness      ;
        private SerializedProperty  m_moonDetail          ;
        private SerializedProperty  m_moonDistance        ;
        private SerializedProperty  m_moonColor           ;
        
        private SerializedProperty  m_starEnabled         ;
        private SerializedProperty  m_starBrightness      ;
        private SerializedProperty  m_starColor           ;
        

        private SerializedProperty m_cloudProfile         ;
        private SerializedProperty m_cloudMap             ;
        private SerializedProperty m_cloudBornRotate      ;
        private SerializedProperty m_cloudSpeed           ;
        private SerializedProperty m_cloudAmbient         ;
        private SerializedProperty m_cloudLight           ;
        private SerializedProperty m_scatterMultiplier    ;
        private SerializedProperty m_attenuation          ;
        private SerializedProperty m_alphaSaturation      ;
        private SerializedProperty m_mask                 ;
        private SerializedProperty m_cloudOffset          ;

        public WeatherSystemScript weatherSystemScript = null;



        private void OnEnable()
        {
            // m_listStyle = new GUIStyle("ListStyle")
            //{
            //    fontSize = 12,
            //    alignment = TextAnchor.UpperLeft,
            //    fontStyle = FontStyle.Normal,
            //    fixedWidth = 100
            //};

            // Get target
            m_target = (WeatherSystemProfileScript) target;

            // Find the serialized properties
            //Sky
            m_skyProfile           = serializedObject.FindProperty("SkyProfile")             ;

            m_timeOfDayOffset      = m_skyProfile.FindPropertyRelative("TimeOfDayOffset")    ;
            m_sunDirection         = m_skyProfile.FindPropertyRelative("SunDirection")       ;
            m_sunXDirection        = m_skyProfile.FindPropertyRelative("SunXDirection")      ;
            m_zenithColor          = m_skyProfile.FindPropertyRelative("ZenithColor")        ;
            m_horizonColor         = m_skyProfile.FindPropertyRelative("HorizonColor")       ;
            m_horizonFalloff       = m_skyProfile.FindPropertyRelative("HorizonFalloff")     ;
            m_mieScatterColor      = m_skyProfile.FindPropertyRelative("MieScatterColor")    ;
            m_mieScatterFactor     = m_skyProfile.FindPropertyRelative("MieScatterFactor")   ;
            m_mieScatterPower      = m_skyProfile.FindPropertyRelative("MieScatterPower")   ;

            m_skyCloudMap          = m_skyProfile.FindPropertyRelative("CloudMap");
            m_skyCloudMapOffset    = m_skyProfile.FindPropertyRelative("CloudMapOffset");
            m_skyCloudMask         = m_skyProfile.FindPropertyRelative("Mask");
            m_distortionTile       = m_skyProfile.FindPropertyRelative("DistortionTile");
            m_distortionAmount     = m_skyProfile.FindPropertyRelative("DistortionAmount");
            m_distortionMirror     = m_skyProfile.FindPropertyRelative("DistortionMirror");


             m_sunEnabled           = m_skyProfile.FindPropertyRelative("SunEnabled")         ;
            m_sunBrightness        = m_skyProfile.FindPropertyRelative("SunBrightness")      ;
            m_sunDetail            = m_skyProfile.FindPropertyRelative("SunDetail")          ;
            m_sunDistance          = m_skyProfile.FindPropertyRelative("SunDistance")        ;
            m_sunColor             = m_skyProfile.FindPropertyRelative("SunColor")           ;
            m_moonEnabled          = m_skyProfile.FindPropertyRelative("MoonEnabled")        ;
            m_moonBrightness       = m_skyProfile.FindPropertyRelative("MoonBrightness")     ;
            m_moonDetail           = m_skyProfile.FindPropertyRelative("MoonDetail")         ;
            m_moonDistance         = m_skyProfile.FindPropertyRelative("MoonDistance")       ;
            m_moonColor            = m_skyProfile.FindPropertyRelative("MoonColor")          ;
            m_starEnabled          = m_skyProfile.FindPropertyRelative("StarEnabled")        ;
            m_starBrightness       = m_skyProfile.FindPropertyRelative("StarBrightness")     ;
            m_starColor            = m_skyProfile.FindPropertyRelative("StarColor")          ;
            
            //Cloud            
            m_cloudProfile         = serializedObject.FindProperty("CloudProfile")            ;
            m_cloudMap             = m_cloudProfile.FindPropertyRelative("CloudMap")          ;
            m_cloudSpeed           = m_cloudProfile.FindPropertyRelative("CloudSpeed")        ;
            m_cloudBornRotate      = m_cloudProfile.FindPropertyRelative("CloudBornRotate")   ;
            m_cloudAmbient         = m_cloudProfile.FindPropertyRelative("CloudAmbient")      ;
            m_cloudLight           = m_cloudProfile.FindPropertyRelative("CloudLight")        ;
            m_scatterMultiplier    = m_cloudProfile.FindPropertyRelative("ScatterMultiplier") ;
            m_attenuation          = m_cloudProfile.FindPropertyRelative("Attenuation")       ;
            m_alphaSaturation      = m_cloudProfile.FindPropertyRelative("AlphaSaturation")   ;
            m_mask                 = m_cloudProfile.FindPropertyRelative("Mask")              ;
            m_cloudOffset          = m_cloudProfile.FindPropertyRelative("CloudOffset")      ;
            
            weatherSystemScript =  GameObject.FindObjectOfType<WeatherSystemScript>();
        }  

        public override void OnInspectorGUI()
        {
            // Logo
            m_controlRect = EditorGUILayout.GetControlRect();
            // Start custom Inspector
            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            if (weatherSystemScript)
            {
                if (weatherSystemScript.CurrentProfile)
                {
                    EditorGUILayout.LabelField(new GUIContent("Current Use Weather Profile :  " + weatherSystemScript.CurrentProfile.name));
                }
                else if (weatherSystemScript.DefaultProfile)
                {
                    EditorGUILayout.LabelField(new GUIContent("Current Use Weather Profile :  " + weatherSystemScript.DefaultProfile.name));
                }
                else
                {
                    EditorGUILayout.LabelField(new GUIContent("Current Use Weather Profile :  "));
                }

                var timeOfDay = EditorGUILayout.Slider(weatherSystemScript.TimeOfDay, 0, 24);
                if (timeOfDay != weatherSystemScript.TimeOfDay)
                {
                    weatherSystemScript.TimeOfDay = timeOfDay;
                    weatherSystemScript.UpdateInEditor();
                    EditorUtility.SetDirty(weatherSystemScript);
                }
            }
            
            // Sky tab
            //m_showSkyGroup = EditorGUILayout.BeginFoldoutHeaderGroup(m_target.showSkyGroup, "Sky");
            if (m_showSkyGroup)
            {
                EditorGUI.indentLevel++;
                DrawGUIHelper.DrawFloatProperty(m_timeOfDayOffset     ,-1.0f, 1.0f, 0.0f                                ,new GUIContent("TimeOfDayOffset       "));

                DrawGUIHelper.DrawLable(new GUIContent("[Light]      "));
             
                DrawGUIHelper.DrawLable(new GUIContent("[Sky Box]      "));
                DrawGUIHelper.DrawTextureProperty(m_skyCloudMap                                                        , new GUIContent("SkyBox Tex         "));
                DrawGUIHelper.DrawFloatProperty(m_skyCloudMapOffset   , 0.0f, 1f,   0.0f                               , new GUIContent("SkyBox Offset      "));
                DrawGUIHelper.DrawFloatProperty(m_skyCloudMask        , 0.0f, 1f,   1.0f                               , new GUIContent("SkyBox Alpha       "));
                DrawGUIHelper.DrawFloatProperty(m_distortionTile      , 0.0f, 100f, 15.0f                              , new GUIContent("DistortionTile    "));
                DrawGUIHelper.DrawFloatProperty(m_distortionAmount    , 0.0f, 100f, 1.0f                               , new GUIContent("DistortionAmount   "));
                DrawGUIHelper.DrawFloatProperty(m_distortionMirror    , 0.0f, 1.0f, 1.0f                               , new GUIContent("DistortionMirror   "));

                DrawGUIHelper.DrawColorProperty(m_target, m_zenithColor, ref m_target.SkyProfile.ZenithColor              ,new GUIContent("ZenithColor        "));
                DrawGUIHelper.DrawColorProperty(m_target, m_horizonColor, ref m_target.SkyProfile.HorizonColor             ,new GUIContent("HorizonColor       "));
                DrawGUIHelper.DrawFloatProperty(m_horizonFalloff      ,0.0f, 100.0f, 4f                                 ,new GUIContent("HorizonFalloff     "));
                DrawGUIHelper.DrawColorProperty(m_target,  m_mieScatterColor     ,ref m_target.SkyProfile.MieScatterColor          ,new GUIContent("MieScatterColor    "));
                DrawGUIHelper.DrawFloatProperty(m_mieScatterFactor    ,0.0f, 1.0f, 0.619f                               ,new GUIContent("MieScatterFactor   "));
                DrawGUIHelper.DrawFloatProperty(m_mieScatterPower     ,0.0f, 3.0f, 1.5f                                 ,new GUIContent("MieScatterPower   "));

                DrawGUIHelper.DrawLable(new GUIContent("[Sky Cloud]      "));
                DrawGUIHelper.DrawTextureProperty(m_cloudMap                                                             ,new GUIContent("CloudMap         "));
                DrawGUIHelper.DrawFloatProperty  (m_cloudBornRotate    , 0f   , 360f , 0f                                , new GUIContent("CloudBornRotate       "));                
				DrawGUIHelper.DrawFloatProperty  (m_cloudSpeed         ,-10.0f, 10.0f, -1f                               ,new GUIContent("CloudSpeed       "));
                DrawGUIHelper.DrawColorProperty  (m_target , m_cloudAmbient, ref m_target.CloudProfile.CloudAmbient           ,new GUIContent("CloudAmbient     "));
                DrawGUIHelper.DrawColorProperty  (m_target , m_cloudLight, ref m_target.CloudProfile.CloudLight             ,new GUIContent("CloudLight       "));
                DrawGUIHelper.DrawFloatProperty  (m_scatterMultiplier  ,0.0f, 10.0f, 1f                                  ,new GUIContent("ScatterMultiplier"));
                DrawGUIHelper.DrawFloatProperty  (m_attenuation        ,0.0f, 10.0f, 0.56f                               ,new GUIContent("Attenuation      "));
                DrawGUIHelper.DrawFloatProperty  (m_alphaSaturation    ,0.0f, 10.0f, 2.61f                               ,new GUIContent("AlphaSaturation  "));
                DrawGUIHelper.DrawFloatProperty  (m_mask               ,0.0f, 3.0f,  1.1f                                ,new GUIContent("CloudMap Alpha   "));
                DrawGUIHelper.DrawFloatProperty  (m_cloudOffset         ,-1.0f, 1.0f, 0f                               ,new GUIContent("CloudOffset       "));

                DrawGUIHelper.DrawLable(new GUIContent("[Sky Effect]      "));
                DrawGUIHelper.DrawFloatProperty(m_sunDirection        ,-180.0f, 180.0f, 0.0f                            ,new GUIContent("SunDirection       "));
                DrawGUIHelper.DrawFloatProperty(m_sunXDirection       ,-180.0f, 180.0f, 0.0f                           ,new GUIContent("SunXDirection       "));
                DrawGUIHelper.DrawFloatProperty(m_sunEnabled          ,-1.0f, 1.0f, 1.0f                                ,new GUIContent("SunEnabled         "));
                DrawGUIHelper.DrawFloatProperty(m_sunBrightness       ,0.0f, 100.0f, 10.0f                              ,new GUIContent("SunBrightness      "));
                DrawGUIHelper.DrawFloatProperty(m_sunDetail           ,0.0f, 20.0f,   0.0f                              ,new GUIContent("SunDetail          "));
                DrawGUIHelper.DrawFloatProperty(m_sunDistance         ,0.0f, 100.0f, 10.0f                              ,new GUIContent("SunDistance        "));
                DrawGUIHelper.DrawColorProperty(m_target , m_sunColor            ,ref m_target.SkyProfile.SunColor                 ,new GUIContent("SunColor           "));
                DrawGUIHelper.DrawFloatProperty(m_moonEnabled         ,-1.0f, 1.0f, 1.0f                                ,new GUIContent("MoonEnabled        "));
                DrawGUIHelper.DrawFloatProperty(m_moonBrightness      ,0.0f, 100.0f, 3.0f                               ,new GUIContent("MoonBrightness     "));
                DrawGUIHelper.DrawFloatProperty(m_moonDetail          ,0.0f, 20.0f,  1.0f                               ,new GUIContent("MoonDetail         "));
                DrawGUIHelper.DrawFloatProperty(m_moonDistance        ,0.0f, 100.0f, 6.0f                               ,new GUIContent("MoonDistance       "));
                DrawGUIHelper.DrawColorProperty(m_target , m_moonColor, ref m_target.SkyProfile.MoonColor                ,new GUIContent("MoonColor          "));

                DrawGUIHelper.DrawFloatProperty(m_starEnabled         ,-1.0f, 1.0f, 1.0f                                ,new GUIContent("StarEnabled        "));
                DrawGUIHelper.DrawFloatProperty(m_starBrightness      ,-1.0f, 100.0f, 1.0f                              ,new GUIContent("StarBrightness     "));
                DrawGUIHelper.DrawColorProperty(m_target, m_starColor           ,ref m_target.SkyProfile.StarColor                ,new GUIContent("StarColor          "));
              

                EditorGUI.indentLevel--;
            }
            //EditorGUILayout.EndFoldoutHeaderGroup();

            // End custom Inspector
            if (EditorGUI.EndChangeCheck() )
            {
                Undo.RecordObject(m_target, "Undo Azure Sky Profile");
                m_target.showSkyGroup = m_showSkyGroup;
                m_target.showFogGroup = m_showFogGroup;
                m_target.showCloudsGroup = m_showCloudsGroup;
               // m_target.showLightingGroup = m_showLightingGroup;
                m_target.showWeatherGroup = m_showWeatherGroup;
                m_target.showRainGroup = m_showRainGroup;
                m_target.showLightningGroup = m_showLightningGroup;
                EditorUtility.SetDirty(m_target);
                serializedObject.ApplyModifiedProperties();
                WeatherSystemScript.UpdateWeahterSystemRunningInstances();
            }
        }
    }
}