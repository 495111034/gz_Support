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

using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

namespace WeatherSystem
{
    public class WeatherSystemSkySettingsScript : WeatherSystemBaseSettingScript
    {
        /// <summary>Atmosphere profile</summary>
        
        [Header("Sky Rendering")]
        [Range(0.0f, 24.0f)]
        [Tooltip("This value controls the light vertically. It represents sunrise/day and sunset/night time( Rotation X )")]
        public float TimeOfDay = 17.0f;

        [Tooltip("It represents sunrise/day and sunset/night time offset")]
        [Range(0.0f, 1.0f)]
        public float TimeOfDayOffset = 0.0f;

        [Range(-180.0f, 180.0f)]
        [Space(5)]
        [Tooltip("This value controls the light horizionally.( Rotation Y )")]
        public float SunDirection = 0.0f;

        [Range(-180.0f, 180.0f)]
        [Space(5)]
        [Tooltip("This value controls the light vertical.( Rotation X )")]
        public float SunXDirection = 0.0f;

        [Tooltip("The zenith color of the sky. (Top of the sky)")]
        [ColorUsage(true, true)]
        public Color ZenithColor = new Color(0.00557f, 0.11459f, 0.40619f);

        [Tooltip("The horizon color of the sky. (Horizon of the sky)")]
        [ColorUsage(true, true)]
        public Color HorizonColor = new Color(0.55772f, 1.48804f, 1.6889f);

        [Range(0.0f, 100.0f)]
        [Tooltip("The horizon fall off of the sky. (Horizon of the sky)")]
        public float HorizonFalloff = 4.0f;

        [Tooltip("The mie Scatter color of the sky. (mieScatter of the sky)")]
        [ColorUsage(true, true)]
        public Color MieScatterColor = new Color(0.2902f, 0.47843f, 0.97255f);

        [Range(0.0f, 1.0f)]
        [Tooltip("The mie Scatter factor of the sky. (mie Scatter factor of the sky)")]
        public float MieScatterFactor = 0.619f;
        
        [Range(0.0f, 3.0f)]
        [Tooltip("The mie Scatter power of the sky. (mie Scatter factor of the sky)")]
        public float MieScatterPower = 1.5f;

        
        //[Header("Cloud")]
        [Tooltip("Enabled of cloud")]
        public bool IsCloudEnabled;

        [Tooltip("This texture of cloud")]
        public Texture CloudMap;

        [Tooltip("This texture of cloud offset")]
        public float CloudMapOffset;

        [Tooltip("Mask of cloud")]
        public float Mask = 1.0f;

        [Tooltip("Distortion Tile of cloud")]
        public float DistortionTile = 1.0f;

        [Tooltip("Distortion Amount of cloud")]
        public float DistortionAmount = 1.0f;

        [Tooltip("Distortion Mirror")]
        public float DistortionMirror = 1.0f;

        [Header("Light")]
        [Header("Sun And Moon Rendering")]
        [Tooltip("Enable of Sun")]
        public bool SunEnabled = true;

        [Tooltip("Brightness of Sun")]
        [Range(0.0f, 100.0f)]
        public float SunBrightness = 10;

        [Tooltip("Detail of Sun")]
        [Range(0.0f, 20.0f)]
        public float SunDetail = 0;

        [Tooltip("Distance of Sun")]
        [Range(0.0f, 100.0f)]
        public float SunDistance = 10;

        [Tooltip("Color of Sun")]
        public Color SunColor = new Color(0.21404f, 0.21404f, 0.21404f, 1.0f);

        [Tooltip("Enable of Moon")]
        public bool MoonEnabled = true;

        [Tooltip("Brightness of Moon")]
        [Range(0.0f, 100.0f)]
        public float MoonBrightness = 3;

        [Tooltip("Detail of Moon")]
        [Range(0.0f, 20.0f)]
        public float MoonDetail = 1;

        [Tooltip("Distance of Moon")]
        [Range(0.0f, 100.0f)]
        public float MoonDistance = 6;

        [Tooltip("Color of Moon")]
        public Color MoonColor = new Color(0.08599999f, 0.16832f, 0.34562f, 1.0f);

        [Header("Star Rendering")]
        [Tooltip("Enable of Star")]
        public bool StarEnabled = false;

        [Range(0.0f, 100.0f)]
        [Tooltip("This controls the brightness  of the Stars field in night sky.")]
        public float StarBrightness = 1.0f;

        [Tooltip("This controls the color of the Stars field in night sky.")]
        [ColorUsage(true, true)]
        public Color StarColor = new Color(0.15429f, 0.72658f, 1.00f, 1.00f);


        public override void UpdateMaterialProperties(WeatherSystemScript weatherSystemScript, MeshRenderer meshRenderer)
        {
            if (weatherSystemScript == null)
            {
                return;
            }

            float t = TimeOfDay * 360.0f / 24.0f - 90.0f;
            float time01 = TimeOfDay / 24;

            UpdateAmbientColors(weatherSystemScript, meshRenderer, t, time01);
            UpdateDayAnNight(weatherSystemScript, meshRenderer, t, time01);
        }


        private void UpdateAmbientColors(WeatherSystemScript weatherSystemScript, MeshRenderer meshRenderer, float t, float time01)
        {   //
            meshRenderer.GetPropertyBlock(WSS.MATERIAL_PROP);

            WSS.MATERIAL_PROP.SetColor(WSS.SKYBOX_ZENITH_COLOR, ZenithColor.gamma);

            WSS.MATERIAL_PROP.SetColor(WSS.SKYBOX_HORIZON_COLOR, HorizonColor.gamma);
            WSS.MATERIAL_PROP.SetFloat(WSS.SKYBOX_HORIZON_FALLOFF, HorizonFalloff);
            WSS.MATERIAL_PROP.SetColor(WSS.SKYBOX_MIE_SCATTER_COLOR, MieScatterColor.gamma);
            WSS.MATERIAL_PROP.SetFloat(WSS.SKYBOX_MIE_SCATTER_FACTOR, MieScatterFactor);
            WSS.MATERIAL_PROP.SetFloat(WSS.SKYBOX_MIE_SCATTER_POWER, MieScatterPower);

            if (IsCloudEnabled)
            {
                WSS.MATERIAL_PROP.SetTexture(WSS.CLOUD_MAP, CloudMap);

                WSS.MATERIAL_PROP.SetFloat(WSS.CLOUD_MASK, Mask);
                WSS.MATERIAL_PROP.SetFloat(WSS.SKYBOX_DISTORTIONTILE, DistortionTile);
                WSS.MATERIAL_PROP.SetFloat(WSS.SKYBOX_DISTORTIONAMOUNT, DistortionAmount);
                WSS.MATERIAL_PROP.SetFloat(WSS.SKYBOX_CLOUDMAPOFFSET, CloudMapOffset);
                WSS.MATERIAL_PROP.SetFloat(WSS.SKYBOX_DISTORTIONMIRROR, DistortionMirror);
            }     
            else
            {
                WSS.MATERIAL_PROP.SetFloat(WSS.CLOUD_MASK, 0);
            }           

            meshRenderer.SetPropertyBlock(WSS.MATERIAL_PROP);

        }

        private Vector4 GetLightPos(WeatherSystemCelestialObjectScript celestial)
        {
            Vector4 pos = -celestial.transform.localToWorldMatrix.GetColumn(2);
            pos.w = 0.0f;
            return pos;
        }

        private Vector4 GetLightPos1(WeatherSystemCelestialObjectScript celestial)
        {
            Vector4 pos = celestial.transform.localToWorldMatrix.GetColumn(2);
            pos.w = 0.0f;
            return pos;
        }

        private void UpdateDayAnNight(WeatherSystemScript weatherSystemScript, MeshRenderer meshRenderer, float t, float time01)
        {
            var isShow = time01 > (0.249f - TimeOfDayOffset) && time01 < (0.751f + TimeOfDayOffset);
            if (isShow != weatherSystemScript.SkyManager.skyScript.Day.activeSelf)
            {
                weatherSystemScript.SkyManager.skyScript.Day.SetActive(isShow);
                weatherSystemScript.SkyManager.skyScript.Night.SetActive(!isShow);
            }

            var sun = weatherSystemScript.SkyManager.skyScript.Sun;
            var moon = weatherSystemScript.SkyManager.skyScript.Moon;
            var star = weatherSystemScript.SkyManager.skyScript.Star;

            var aurora = weatherSystemScript.SkyManager.skyScript.Aurora;
            var auroraStar = weatherSystemScript.SkyManager.skyScript.AuroraStar;

            if (SunEnabled != sun.gameObject.activeSelf)
            {
                sun.gameObject.SetActive(SunEnabled);
            }

            if (MoonEnabled != moon.gameObject.activeSelf)
            {
                moon.gameObject.SetActive(MoonEnabled);
            }


            if (StarEnabled != weatherSystemScript.SkyManager.skyScript.Star.gameObject.activeSelf)
            {
                weatherSystemScript.SkyManager.skyScript.Star.gameObject.SetActive(StarEnabled);
            }

            var sunEuler = Quaternion.Euler(new Vector3(sun.RotateXDegrees + SunXDirection, sun.RotateYDegrees + SunDirection, 0)) * Quaternion.Euler(t, 0f, 0f);
            sun.transform.rotation = sunEuler;

            if (SunEnabled)
            {
                sun.MeshRenderer.GetPropertyBlock(WSS.MATERIAL_PROP);

                WSS.MATERIAL_PROP.SetColor(WSS.COLOR, SunColor.gamma);
                WSS.MATERIAL_PROP.SetFloat(WSS.BRIGHTNESS, SunBrightness);
                WSS.MATERIAL_PROP.SetFloat(WSS.DETAIL, SunDetail);
                WSS.MATERIAL_PROP.SetFloat(WSS.DISTANCE, SunDistance);

                sun.MeshRenderer.SetPropertyBlock(WSS.MATERIAL_PROP);
            }

            var moonEuler = Quaternion.Euler(new Vector3(sun.RotateXDegrees + SunXDirection, sun.RotateYDegrees + SunDirection, 0)) * Quaternion.Euler(1 - t, 0f, 0f);
            moon.transform.rotation = moonEuler;

            if (MoonEnabled)
            {
                moon.MeshRenderer.GetPropertyBlock(WSS.MATERIAL_PROP);

                WSS.MATERIAL_PROP.SetColor(WSS.COLOR, MoonColor.gamma);
                WSS.MATERIAL_PROP.SetFloat(WSS.BRIGHTNESS, MoonBrightness);
                WSS.MATERIAL_PROP.SetFloat(WSS.DETAIL, MoonDetail);
                WSS.MATERIAL_PROP.SetFloat(WSS.DISTANCE, MoonDistance);

                moon.MeshRenderer.SetPropertyBlock(WSS.MATERIAL_PROP);
            }

            //meshRenderer.GetPropertyBlock(WSS.MATERIAL_PROP);            
            if (isShow)
            {
                Shader.SetGlobalVector(WSS.SKYBOX_LIGHT_DIR, GetLightPos(sun));
            }
            else
            {
                Shader.SetGlobalVector(WSS.SKYBOX_LIGHT_DIR, GetLightPos(moon));
            }

            //meshRenderer.SetPropertyBlock(WSS.MATERIAL_PROP);

            if (StarEnabled)
            {
                star.MeshRenderer.GetPropertyBlock(WSS.MATERIAL_PROP);

                WSS.MATERIAL_PROP.SetFloat(WSS.BRIGHTNESS, StarBrightness);
                WSS.MATERIAL_PROP.SetColor(WSS.COLOR, StarColor.gamma);

                star.MeshRenderer.SetPropertyBlock(WSS.MATERIAL_PROP);
            }
        }
    }
    
    /// <summary>
    /// Sky profile script, contains all properties for sky rendering
    /// </summary>
    //[CreateAssetMenu(fileName = "WeatherSystemSkyProfile", menuName = "WeatherSystem/Sky Profile", order = 30)]
    [System.Serializable]
    public class WeatherSystemSkyProfileScript : WeatherSystemBaseProfileScript
    {
        /// <summary>Atmosphere profile</summary>
        [Header("Sky Rendering")]
        [Range(-1.0f, 1.0f)]        
        [Tooltip("It represents sunrise/day and sunset/night time offset")]
        public WeatherSystemFloatProperty TimeOfDayOffset = new WeatherSystemFloatProperty
        (
             0.0f,
            AnimationCurve.Linear(0.0f, -1.0f, 24.0f, 1.0f)
        );

        [Range(-180.0f, 180.0f)]
        [Space (5)][Tooltip ("This value controls the light horizionally.( Rotation Y )")]
        public WeatherSystemFloatProperty SunDirection = new WeatherSystemFloatProperty
        (
             0.0f,
            AnimationCurve.Linear(0.0f, 0.0f, 24.0f, 0.0f)
        );
        [Range(-180.0f, 180.0f)]
        [Space(5)]
        [Tooltip("This value controls the light vertical.( Rotation X )")]
        public WeatherSystemFloatProperty SunXDirection = new WeatherSystemFloatProperty
        (
            0.0f,
            AnimationCurve.Linear(0.0f, 0.0f, 24.0f, 0.0f)
        );

        [Tooltip("The zenith color of the sky. (Top of the sky)")]
        [ColorUsage(true, true)]
        public WeatherSystemColorProperty ZenithColor = new WeatherSystemColorProperty
        (
            new Color(0.00557f, 0.11459f, 0.40619f, 0.00f),
            new Gradient()
        );
        
        [Tooltip("The horizon color of the sky. (Horizon of the sky)")]
        [ColorUsage(true, true)]
        public WeatherSystemColorProperty HorizonColor = new WeatherSystemColorProperty
        (
            new Color(0.55772f, 1.48804f, 1.6889f, 0.0f),
            new Gradient()
        );
        
        [Range(0.0f, 100.0f)]
        [Tooltip("The horizon fall off of the sky. (Horizon of the sky)")]
        public WeatherSystemFloatProperty HorizonFalloff = new WeatherSystemFloatProperty
        (
            4.0f,
            AnimationCurve.Linear(0.0f, 4.0f, 24.0f, 4.0f)
        );
        
        [Tooltip("The mie Scatter color of the sky. (mieScatter of the sky)")]
        [ColorUsage(true, true)]
        public WeatherSystemColorProperty MieScatterColor = new WeatherSystemColorProperty
        (
            new Color(0.2902f, 0.47843f, 0.97255f, 0.0f),
            new Gradient()
        );

        [Range(0.0f, 1.0f)]
        [Tooltip("The mie Scatter factor of the sky. (mie Scatter factor of the sky)")]
        public WeatherSystemFloatProperty MieScatterFactor = new WeatherSystemFloatProperty
        (
             0.619f,
            AnimationCurve.Linear(0.0f, 0.619f, 24.0f, 0.619f)
        );

        [Range(0.0f, 3.0f)]
        [Tooltip("The mie Scatter power of the sky. (mie Scatter factor of the sky)")]
        public WeatherSystemFloatProperty MieScatterPower = new WeatherSystemFloatProperty
         (
              1.5f,
             AnimationCurve.Linear(0.0f, 1.5f, 24.0f, 1.5f)
         );

     
        [Tooltip("This texture of cloud")]
        public Texture CloudMap;

        [Tooltip("This texture of cloud offset")]
        public WeatherSystemFloatProperty CloudMapOffset = new WeatherSystemFloatProperty
        (
            0.0f,
            AnimationCurve.Linear(0.0f, 1f, 24.0f, 1f)
        );        

        [Tooltip("Mask of cloud")]
        public WeatherSystemFloatProperty Mask = new WeatherSystemFloatProperty
        (
            1.0f,
            AnimationCurve.Linear(0.0f, 1f, 24.0f, 1f)
        );        


        [Tooltip("Distortion Tile of cloud")]        
        public WeatherSystemFloatProperty DistortionTile = new WeatherSystemFloatProperty
        (
            15.0f,
            AnimationCurve.Linear(0.0f, 1f, 24.0f, 100f)
        );

        [Tooltip("Distortion Amount of cloud")]
        public WeatherSystemFloatProperty DistortionAmount = new WeatherSystemFloatProperty
        (
            1.0f,
            AnimationCurve.Linear(0.0f, 1f, 24.0f, 100f)
        );

        [Tooltip("Distortion Amount of cloud")]
        public WeatherSystemFloatProperty DistortionMirror = new WeatherSystemFloatProperty
        (
            1.0f,
            AnimationCurve.Linear(0.0f, 1f, 24.0f, 1)
        );

        [Tooltip("Enable of Sun")]
        public WeatherSystemFloatProperty SunEnabled =  new WeatherSystemFloatProperty
        (
             1.0f,
            AnimationCurve.Linear(0.0f, 1.0f, 24.0f, 1.0f)
        );

        [Tooltip("Brightness of Sun")]
        [Range(0.0f, 100.0f)]
        public WeatherSystemFloatProperty SunBrightness = new WeatherSystemFloatProperty
        (
             10f,
            AnimationCurve.Linear(0.0f, 10f, 24.0f, 10f)
        );
        
        [Tooltip("Detail of Sun")]
        [Range(0.0f, 20.0f)]
        public WeatherSystemFloatProperty SunDetail = new WeatherSystemFloatProperty
        (
             0.0f,
            AnimationCurve.Linear(0.0f, 0.0f, 24.0f, 0.0f)
        );
        
        [Tooltip("Distance of Sun")]
        [Range(0.0f, 100.0f)]
        public WeatherSystemFloatProperty SunDistance = new WeatherSystemFloatProperty
        (
             10f,
            AnimationCurve.Linear(0.0f, 10f, 24.0f, 10f)
        );

        [Tooltip("Color of Sun")]
        public WeatherSystemColorProperty SunColor = new WeatherSystemColorProperty
        (
            new Color(0.21404f, 0.21404f, 0.21404f, 1.0f),
            new Gradient()
        );

        [Tooltip("Enable of SunFlarLen")]
        public WeatherSystemFloatProperty SunFlarLenEnabled =  new WeatherSystemFloatProperty
        (
             1.0f,
            AnimationCurve.Linear(0.0f, 1.0f, 24.0f, 1.0f)
        );

        [Tooltip("Enable of Moon")]
        public WeatherSystemFloatProperty MoonEnabled = new WeatherSystemFloatProperty
        (
             1.0f,
            AnimationCurve.Linear(0.0f, 1.0f, 24.0f, 1.0f)
        );

        [Tooltip("Brightness of Moon")]
        [Range(0.0f, 100.0f)]
        public WeatherSystemFloatProperty MoonBrightness = new WeatherSystemFloatProperty
        (
             3.0f,
            AnimationCurve.Linear(0.0f, 3.0f, 24.0f, 3.0f)
        );
        
        [Tooltip("Detail of Moon")]
        [Range(0.0f, 20.0f)]
        public WeatherSystemFloatProperty MoonDetail = new WeatherSystemFloatProperty
        (
             1.0f,
            AnimationCurve.Linear(0.0f, 1.0f, 24.0f, 1.0f)
        );
        
        [Tooltip("Distance of Moon")]
        [Range(0.0f, 100.0f)]
        public WeatherSystemFloatProperty MoonDistance = new WeatherSystemFloatProperty
        (
             6.0f,
            AnimationCurve.Linear(0.0f, 6.0f, 24.0f, 6.0f)
        );
        
        [Tooltip("Color of Moon")]
        public WeatherSystemColorProperty MoonColor = new WeatherSystemColorProperty
        (
            new Color(0.08599999f, 0.16832f, 0.34562f, 1.0f),
            new Gradient()
        );
        
        
        [Tooltip("Enable of Star")]
        public WeatherSystemFloatProperty StarEnabled = new WeatherSystemFloatProperty
        (
             1.0f,
            AnimationCurve.Linear(0.0f, 1.0f, 24.0f, 1.0f)
        );

        [Range(0.0f, 100.0f)][Tooltip ("This controls the brightness  of the Stars field in night sky.")]
        public WeatherSystemFloatProperty StarBrightness = new WeatherSystemFloatProperty
        (
             1.0f,
            AnimationCurve.Linear(0.0f, 1.0f, 24.0f, 1.0f)
        );       
        
        [Tooltip("This controls the color of the Stars field in night sky.")]
        [ColorUsage(true, true)]
        public WeatherSystemColorProperty StarColor = new WeatherSystemColorProperty
        (
            new Color(0.15429f, 0.72658f, 1.00f, 1.00f),
            new Gradient()
        );

        
        
    }
}

