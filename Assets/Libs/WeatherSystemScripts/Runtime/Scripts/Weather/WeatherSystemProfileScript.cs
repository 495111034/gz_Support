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

using System.Collections;
using System.Collections.Generic;

namespace WeatherSystem
{
    [System.Serializable]
    public struct WeatherSystemGlobalProfileScript
    {
        public WeatherSystemProfileScript profile;
        public float transitionTime;
    }
    [System.Serializable]
    public struct WeatherSystemGlobalProfileConfigScript
    {        
        public string profile;
        public string profilePath;
        public float transitionTime;
    }

    /// <summary>
    /// Weather profile script, contains all profiles for all Weather Maker effects
    /// </summary>
    [CreateAssetMenu(fileName = "WeatherSystemProfile", menuName = "WeatherSystem/Weather Profile", order = 10)]
    [System.Serializable]
    public class WeatherSystemProfileScript : WeatherSystemBaseScriptableObjectScript
    {                
        public bool showSkyGroup = true;
        public bool showCloudsGroup = true;
        public bool showFogGroup = true;
        public bool showLightingGroup = true;
        public bool showWindGroup = true;
        public bool showWeatherGroup = true;
        public bool showRainGroup = true;
        public bool showLightningGroup = true;
        public bool showSnowGroup = true;
        public bool showPostProcessingGroup = true;

        /// <summary>Sky profile</summary>
        [Tooltip("Sky profile")]
        public WeatherSystemSkyProfileScript SkyProfile;

        /// <summary>Cloud profile</summary>
        [Tooltip("Cloud profile")]
        public WeatherSystemDistanceCloudProfileScript CloudProfile;


        public static void ApplyGlobalWeatherTransition(IWeatherSystemProvider managers, WeatherSystemProfileScript from, WeatherSystemProfileScript to, float timeOfDay, float t)
        {
            if (managers == null)
            {
                return;
            }

            // notify clouds
            if (managers.CloudManager != null)
            {
                managers.CloudManager.ApplyGlobalWeatherTransition( from, to, timeOfDay, t);
            }

            // notify sky
            if (managers.SkyManager != null)
            {
                managers.SkyManager.ApplyGlobalWeatherTransition( from, to, timeOfDay, t);
            }

        }

        public static void ApplyWeatherDefaultSettings(IWeatherSystemProvider managers,  WeatherSystemProfileScript profile, float timeOfDay)
        {
            if (managers == null)
            {
                return;
            }

            // notify clouds
            if (managers.CloudManager != null)
            {
                managers.CloudManager.ApplyWeatherDefaultSettings( profile, timeOfDay);
            }

            // notify sky
            if (managers.SkyManager != null)
            {
                managers.SkyManager.ApplyWeatherDefaultSettings( profile, timeOfDay);
            }

        }

        /// <summary>
        /// Computes local weather zones influence.
        /// </summary>
        public static void ApplyWeatherZonesInfluence(IWeatherSystemProvider managers,  WeatherSystemProfileScript climateZoneProfile, float timeOfDay, float t)
        {
            if (managers == null)
            {
                return;
            }

            // notify clouds
            if (managers.CloudManager != null)
            {
                managers.CloudManager.ApplyWeatherZonesInfluence( climateZoneProfile, timeOfDay, t);
            }

            // notify sky
            if (managers.SkyManager != null)
            {
                managers.SkyManager.ApplyWeatherZonesInfluence( climateZoneProfile, timeOfDay, t);
            }
        }
    }

    /// <summary>
    /// Provider of weather maker interfaces
    /// </summary>
    public interface IWeatherSystemProvider
    {
        /// <summary>
        /// Cloud manager
        /// </summary>
        ICloudManager CloudManager { get; }
        
        /// <summary>
        /// Sky manager
        /// </summary>
        ISkyManager SkyManager { get; }

       
    }
}
