//
// Weather Maker for Unity
// (c) 2016 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 
// *** A NOTE ABOUT PIRACY ***
// 
// If you got this asset from a pirate site, please consider buying it from the Unity asset store at https://assetstore.unity.com/packages/slug/60955?aid=1011lGnL. This asset is only legally available from the Unity Asset Store.
// 
// I'm a single indie dev supporting my family by spending hundreds and thousands of hours on this and other assets. It's very offensive, rude and just plain evil to steal when I (and many others) put so much hard work into the software.
// 
// Thank you.
//
// *** END NOTE ABOUT PIRACY ***
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace WeatherSystem
{

#pragma warning disable 1591

    public static class WSS
    {
        #region Buffers

        #endregion Buffers

        #region Ids

        private static int ID(string s) { return Shader.PropertyToID(s); }
        
        public static readonly int _AlphaMultiplierAnimation = ID("_AlphaMultiplierAnimation");
        public static readonly int _AlphaMultiplierAnimation2 = ID("_AlphaMultiplierAnimation2");
        public static readonly int _ParticleDitherLevel = ID("_ParticleDitherLevel");
        public static readonly int _TemporalReprojection_BlendMode = ID("_TemporalReprojection_BlendMode");
        public static readonly int _TemporalReprojection_InverseProjection = ID("_TemporalReprojection_InverseProjection");
        public static readonly int _TemporalReprojection_InverseProjectionView = ID("_TemporalReprojection_InverseProjectionView");
        public static readonly int _TemporalReprojection_InverseView = ID("_TemporalReprojection_InverseView");
        public static readonly int _TemporalReprojection_ipivpvp = ID("_TemporalReprojection_ipivpvp");
        public static readonly int _TemporalReprojection_PrevDepth = ID("_TemporalReprojection_PrevDepth");
        public static readonly int _TemporalReprojection_PrevFrame = ID("_TemporalReprojection_PrevFrame");
        public static readonly int _TemporalReprojection_PreviousView = ID("_TemporalReprojection_PreviousView");
        public static readonly int _TemporalReprojection_PreviousViewProjection = ID("_TemporalReprojection_PreviousViewProjection");
        public static readonly int _TemporalReprojection_Projection = ID("_TemporalReprojection_Projection");
        public static readonly int _TemporalReprojection_SubFrame = ID("_TemporalReprojection_SubFrame");
        public static readonly int _TemporalReprojection_SubFrameNumber = ID("_TemporalReprojection_SubFrameNumber");
        public static readonly int _TemporalReprojection_SubPixelSize = ID("_TemporalReprojection_SubPixelSize");
        public static readonly int _TemporalReprojection_View = ID("_TemporalReprojection_View");
        public static readonly int _WeatherSystemTemporalReprojectionAlphaThreshold = ID("_WeatherSystemTemporalReprojectionAlphaThreshold");
        public static readonly int _WeatherSystemTemporalReprojectionDepthThreshold = ID("_WeatherSystemTemporalReprojectionDepthThreshold");

        static public int COLOR = Shader.PropertyToID("_Color");
        static public int TINT_COLOR = Shader.PropertyToID("_TintColor");
        static public int ALPHA = Shader.PropertyToID("_Alpha");
        static public int BRIGHTNESS = Shader.PropertyToID("_Brightness");
        static public int MOON_DETAIL = Shader.PropertyToID("_MoonDetails");
        static public int AMOUNT = Shader.PropertyToID("_Amount");
        static public int PARTICLE_ALPHA = Shader.PropertyToID("_VertexAlpha");
        static public int PAINTING_SKY_ALPHA = Shader.PropertyToID("_ChangeAlpha");
        static public int MIAN_TEX = Shader.PropertyToID("_MainTex");
        static public int DISTANCE = Shader.PropertyToID("_Distance");
        static public int DETAIL = Shader.PropertyToID("_Detail");
        
        //Fog
        public static int FOG_INFO1 = Shader.PropertyToID("_FogInfo");
        public static int FOG_INFO2 = Shader.PropertyToID("_FogInfo2");
        public static int FOG_INFO3 = Shader.PropertyToID("_FogInfo3");
        public static int FOG_INFO4 = Shader.PropertyToID("_FogInfo4");
        public static int FOG_INFO5 = Shader.PropertyToID("_FogInfo5");
        public static int FOG_COLOR1 = Shader.PropertyToID("_FogColor1");
        public static int FOG_COLOR2 = Shader.PropertyToID("_FogColor2");
        public static int FOG_COLOR3 = Shader.PropertyToID("_FogColor3");
        public static int FOG_COLOR4 = Shader.PropertyToID("_FogColor4");
        public static int UNDER_WATER_COLOR = Shader.PropertyToID("_UnderWaterColor");
        public static int UNDER_WATER_PARAM = Shader.PropertyToID("_UnderWaterParam");
        
        public static int SKYBOX_COLOR_BASE = Shader.PropertyToID("_NightSkyColBase");
        public static int SKYBOX_COLOR_DELTA = Shader.PropertyToID("_NightSkyColDelta");
        public static int SKYBOX_RAYLEIGH_SCATTERING = Shader.PropertyToID("_RayleighScattering");
        public static int SKYBOX_MIE_SCATTERING = Shader.PropertyToID("_MieScattering");
        public static int SKYBOX_MIE_PHASE_FUNCTION = Shader.PropertyToID("_MiePhaseParams");
        public static int SKYBOX_SKYLINE_POS = Shader.PropertyToID("_SkyLinePos");
        public static int SKYBOX_SUN_DIR = Shader.PropertyToID("_SkyboxSunDir");
        public static int SKYBOX_MOON_DIR = Shader.PropertyToID("_SkyboxMoonDir");
        public static int SKYBOX_LIGHT_DIR = Shader.PropertyToID("_SkyboxLightDir");
        public static int SKYBOX_MOON_RAYLEIGH_SCATTERING = Shader.PropertyToID("_MoonRayleighScattering");
        public static int SKYBOX_MOON_MIE_SCATTERING = Shader.PropertyToID("_MoonMieScattering");
        
        //Cloud
        public static int CLOUD_COLOR_BASE = Shader.PropertyToID("_CloudColor");
        public static int CLOUD_COLOR_SHADOW = Shader.PropertyToID("_ShadowColor");
        public static int CLOUD_PARAMS0 = Shader.PropertyToID("_CloudParams0");
        public static int CLOUD_PARAMS1 = Shader.PropertyToID("_CloudParams1");
        public static int CLOUD_MAP_TEXTURE = Shader.PropertyToID("_CloudMap");
        public static int CLOUD_SUN_COLOR = Shader.PropertyToID("_SunColor");
        public static int CLOUD_SUN_BRIGHTNESS = Shader.PropertyToID("_SunBrightness");
        public static int CLOUD_SUN_RANGE = Shader.PropertyToID("_SunRange");
        public static int CLOUD_COORD = Shader.PropertyToID("_Coord");
        
        public static int CLOUD_MAP = Shader.PropertyToID("_CloudMap");
        public static int CLOUD_AMBIENT_COLOR = Shader.PropertyToID("_CloudAmbient");
        public static int CLOUD_LIGHT_COLOR = Shader.PropertyToID("_CloudLight");
        public static int CLOUD_ATTENUATION = Shader.PropertyToID("_Attenuation");
        public static int CLOUD_STEP_SIZE = Shader.PropertyToID("_StepSize");
        public static int CLOUD_ALPHA_SATURATION = Shader.PropertyToID("_AlphaSaturation");
        public static int CLOUD_MASK = Shader.PropertyToID("_Mask");
        public static int CLOUD_SCATTER = Shader.PropertyToID("_ScatterMultiplier");
        public static int CLOUD_ROTATE = Shader.PropertyToID("_CloudRotate");
        public static int CLOUD_OFFSET = Shader.PropertyToID("_CloudOffset");
        //Sky
        public static int SKYBOX_ZENITH_COLOR = Shader.PropertyToID("_ZenithColor");
        public static int SKYBOX_HORIZON_COLOR = Shader.PropertyToID("_HorizonColor");
        public static int SKYBOX_HORIZON_FALLOFF = Shader.PropertyToID("_HorizonFalloff");
        public static int SKYBOX_MIE_SCATTER_COLOR = Shader.PropertyToID("_MieScatterColor");
        public static int SKYBOX_MIE_SCATTER_FACTOR = Shader.PropertyToID("_MieScatterFactor");
        public static int SKYBOX_MIE_SCATTER_POWER = Shader.PropertyToID("_MieScatterPower");
        public static int SKYBOX_DISTORTIONTILE = Shader.PropertyToID("_DistortionTile");
        public static int SKYBOX_DISTORTIONAMOUNT = Shader.PropertyToID("_DistortionAmount");
        public static int SKYBOX_CLOUDMAPOFFSET = Shader.PropertyToID("_CloudMapOffset");
        public static int SKYBOX_DISTORTIONMIRROR = Shader.PropertyToID("_DistortionMirror");

        //Rain
        public static int RAIN_SNOW_PARAMS = Shader.PropertyToID("_RainSnowParams");
        public static int RAIN_RIPPLE_STRENGTH = Shader.PropertyToID("_RainRippleStrength");
        public static int RAIN_RIPPLE_SPEED = Shader.PropertyToID("_RainRippleSpeed");
        public static int RAIN_RIPPLE_TILING = Shader.PropertyToID("_RainRippleTilling");
        public static int RAIN_RIPPLE_NORMAL = Shader.PropertyToID("_RainRippleNormalTex");
        
        public static int CLOUD_SEA_FACING_LIGHT_COLOR = Shader.PropertyToID("_CloudSeaFacingLightColor");
        public static int CLOUD_SEA_BACKING_LIGHT_COLOR = Shader.PropertyToID("_CloudSeaBackingLightColor");

        public static int FLARE_TEX = Shader.PropertyToID("_FlareTexture");
        public static int FLARE_COORDINATE = Shader.PropertyToID("_FlareCoordinate");
        public static int FLARE_FADESCALE = Shader.PropertyToID("_FlareFadeScale");
        public static int FLARE_ANGLE = Shader.PropertyToID("_FlareAngle");
        public static int FLARE_PARAM = Shader.PropertyToID("_FlareParam");

        public static MaterialPropertyBlock MATERIAL_PROP = new MaterialPropertyBlock();


        #endregion Ids

        static WSS()
        {
        }

        public static void Initialize()
        {
        }
    }

#pragma warning restore 1591

}
