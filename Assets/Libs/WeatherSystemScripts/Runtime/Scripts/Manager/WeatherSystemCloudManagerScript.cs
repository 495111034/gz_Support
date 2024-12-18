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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WeatherSystem
{
    /// <summary>
    /// Fog manager interface
    /// </summary>
    public class ICloudManager : IWeatherSystemManager
    {
        // TODO: Expose things like fog density
    }

    /// <summary>
    /// Fog manager default implementation
    /// </summary>
    public class WeatherSystemCloudManagerScript : ICloudManager
    {
        /// <summary>Full screen fog script</summary>
        [Header("Dependencies")]
        [Tooltip("distance cloud script")]
        public WeatherSystemDistanceCloudScript CloudScript;


        public override void ApplyWeatherDefaultSettings( WeatherSystemProfileScript profile, float timeOfDay)
        {
            CloudScript.Profile.IsEnabled         = profile.CloudProfile.CloudMap != null;
            CloudScript.Profile.CloudMap          = profile.CloudProfile.CloudMap;
            CloudScript.Profile.StepSize          = profile.CloudProfile.StepSize;

            CloudScript.Profile.CloudSpeed        = profile.CloudProfile.CloudSpeed       .GetValue(timeOfDay);
            CloudScript.Profile.CloudAmbient      = profile.CloudProfile.CloudAmbient     .GetValue(timeOfDay);
            CloudScript.Profile.CloudLight        = profile.CloudProfile.CloudLight       .GetValue(timeOfDay);
            CloudScript.Profile.ScatterMultiplier = profile.CloudProfile.ScatterMultiplier.GetValue(timeOfDay);
            CloudScript.Profile.Attenuation       = profile.CloudProfile.Attenuation      .GetValue(timeOfDay);
            CloudScript.Profile.AlphaSaturation   = profile.CloudProfile.AlphaSaturation  .GetValue(timeOfDay);            
            CloudScript.Profile.Mask              = profile.CloudProfile.Mask             .GetValue(timeOfDay);
            CloudScript.Profile.CloudOffset       = profile.CloudProfile.CloudOffset      .GetValue(timeOfDay);
            
            float cloudBornRotate = profile.CloudProfile.CloudBornRotate.GetValue(timeOfDay);
            if (cloudBornRotate != CloudScript.Profile.CloudBornRotate)
            {
                CloudScript.Profile.CloudBornRotate = cloudBornRotate;
                CloudScript.Profile.CloudRotate = cloudBornRotate;
            }

            CloudScript.UpdateMaterialFromProfile(weatherSystemScript);


        }

        public override void ApplyGlobalWeatherTransition( WeatherSystemProfileScript from, WeatherSystemProfileScript to, float timeOfDay, float t)
        {
            float cloudBornRotate = CloudScript.Profile.CloudBornRotate;
            if (from.CloudProfile.CloudMap != null && to.CloudProfile.CloudMap == null)
            {
                CloudScript.Profile.Mask = WeatherSystemUtility.FloatInterpolation(from.CloudProfile.Mask.GetValue(timeOfDay), 0, t);
            }
            else if (from.CloudProfile.CloudMap == null && to.CloudProfile.CloudMap == null)            
            {
                CloudScript.Profile.IsEnabled = false;
            }
            else if (from.CloudProfile.CloudMap == null && to.CloudProfile.CloudMap != null)
            {
                CloudScript.Profile.CloudMap  = to.CloudProfile.CloudMap;
                CloudScript.Profile.StepSize  = to.CloudProfile.StepSize;
                CloudScript.Profile.IsEnabled = to.CloudProfile.CloudMap != null;

                cloudBornRotate = to.CloudProfile.CloudBornRotate.GetValue(timeOfDay);
                CloudScript.Profile.CloudSpeed        = to.CloudProfile.CloudSpeed       .GetValue(timeOfDay);
                CloudScript.Profile.CloudAmbient      = to.CloudProfile.CloudAmbient     .GetValue(timeOfDay);
                CloudScript.Profile.CloudLight        = to.CloudProfile.CloudLight       .GetValue(timeOfDay);
                CloudScript.Profile.ScatterMultiplier = to.CloudProfile.ScatterMultiplier.GetValue(timeOfDay);
                CloudScript.Profile.Attenuation       = to.CloudProfile.Attenuation      .GetValue(timeOfDay);
                CloudScript.Profile.AlphaSaturation   = to.CloudProfile.AlphaSaturation  .GetValue(timeOfDay);     
                CloudScript.Profile.Mask              = WeatherSystemUtility.FloatInterpolation(0, to.CloudProfile.Mask.GetValue(timeOfDay),t);
                CloudScript.Profile.CloudOffset      = to.CloudProfile.CloudOffset       .GetValue(timeOfDay);
            }
            else if (from.CloudProfile.CloudMap != null && to.CloudProfile.CloudMap != null)
            {
                if (from.CloudProfile.CloudMap != to.CloudProfile.CloudMap)
                {   
                    if (t < 0.5)
                    {
                        CloudScript.Profile.Mask = WeatherSystemUtility.FloatInterpolation(from.CloudProfile.Mask.GetValue(timeOfDay), 0, t * 2);
                        CloudScript.Profile.CloudMap = from.CloudProfile.CloudMap;
                    }
                    else
                    {
                        CloudScript.Profile.Mask = WeatherSystemUtility.FloatInterpolation(0, to.CloudProfile.Mask.GetValue(timeOfDay), (t - 0.5f) * 2);
                        CloudScript.Profile.CloudMap = to.CloudProfile.CloudMap;
                    }

                    CloudScript.Profile.StepSize          = to.CloudProfile.StepSize;
                    CloudScript.Profile.IsEnabled         = CloudScript.Profile.CloudMap != null;

                    cloudBornRotate = WeatherSystemUtility.FloatInterpolation(from.CloudProfile.CloudBornRotate.GetValue(timeOfDay),   to.CloudProfile.CloudBornRotate  .GetValue(timeOfDay), t);
                    CloudScript.Profile.CloudSpeed        = WeatherSystemUtility.FloatInterpolation(from.CloudProfile.CloudSpeed       .GetValue(timeOfDay), to.CloudProfile.CloudSpeed       .GetValue(timeOfDay),t);
                    CloudScript.Profile.CloudAmbient      = WeatherSystemUtility.ColorInterpolation(from.CloudProfile.CloudAmbient     .GetValue(timeOfDay), to.CloudProfile.CloudAmbient     .GetValue(timeOfDay),t);
                    CloudScript.Profile.CloudLight        = WeatherSystemUtility.ColorInterpolation(from.CloudProfile.CloudLight       .GetValue(timeOfDay), to.CloudProfile.CloudLight       .GetValue(timeOfDay),t);
                    CloudScript.Profile.ScatterMultiplier = WeatherSystemUtility.FloatInterpolation(from.CloudProfile.ScatterMultiplier.GetValue(timeOfDay), to.CloudProfile.ScatterMultiplier.GetValue(timeOfDay),t);
                    CloudScript.Profile.Attenuation       = WeatherSystemUtility.FloatInterpolation(from.CloudProfile.Attenuation      .GetValue(timeOfDay), to.CloudProfile.Attenuation      .GetValue(timeOfDay),t);
                    CloudScript.Profile.AlphaSaturation   = WeatherSystemUtility.FloatInterpolation(from.CloudProfile.AlphaSaturation  .GetValue(timeOfDay), to.CloudProfile.AlphaSaturation  .GetValue(timeOfDay),t);    
                    CloudScript.Profile.CloudOffset       = WeatherSystemUtility.FloatInterpolation(from.CloudProfile.CloudOffset      .GetValue(timeOfDay), to.CloudProfile.CloudOffset     .GetValue(timeOfDay),t); 
                }   
                else
                {
                     CloudScript.Profile.CloudMap          = to.CloudProfile.CloudMap;
                     CloudScript.Profile.StepSize          = to.CloudProfile.StepSize;
                     CloudScript.Profile.IsEnabled         = CloudScript.Profile.CloudMap != null;

                     cloudBornRotate = WeatherSystemUtility.FloatInterpolation(from.CloudProfile.CloudBornRotate   .GetValue(timeOfDay), to.CloudProfile.CloudBornRotate  .GetValue(timeOfDay), t);
                     CloudScript.Profile.CloudSpeed         = WeatherSystemUtility.FloatInterpolation(from.CloudProfile.CloudSpeed       .GetValue(timeOfDay), to.CloudProfile.CloudSpeed       .GetValue(timeOfDay),t);
                     CloudScript.Profile.CloudAmbient      = WeatherSystemUtility.ColorInterpolation(from.CloudProfile.CloudAmbient     .GetValue(timeOfDay), to.CloudProfile.CloudAmbient     .GetValue(timeOfDay),t);
                     CloudScript.Profile.CloudLight        = WeatherSystemUtility.ColorInterpolation(from.CloudProfile.CloudLight       .GetValue(timeOfDay), to.CloudProfile.CloudLight       .GetValue(timeOfDay),t);
                     CloudScript.Profile.ScatterMultiplier = WeatherSystemUtility.FloatInterpolation(from.CloudProfile.ScatterMultiplier.GetValue(timeOfDay), to.CloudProfile.ScatterMultiplier.GetValue(timeOfDay),t);
                     CloudScript.Profile.Attenuation       = WeatherSystemUtility.FloatInterpolation(from.CloudProfile.Attenuation      .GetValue(timeOfDay), to.CloudProfile.Attenuation      .GetValue(timeOfDay),t);
                     CloudScript.Profile.AlphaSaturation   = WeatherSystemUtility.FloatInterpolation(from.CloudProfile.AlphaSaturation  .GetValue(timeOfDay), to.CloudProfile.AlphaSaturation  .GetValue(timeOfDay),t);    
                     CloudScript.Profile.Mask              = WeatherSystemUtility.FloatInterpolation(from.CloudProfile.Mask             .GetValue(timeOfDay), to.CloudProfile.Mask             .GetValue(timeOfDay),t);
                     CloudScript.Profile.CloudOffset       = WeatherSystemUtility.FloatInterpolation(from.CloudProfile.CloudOffset       .GetValue(timeOfDay), to.CloudProfile.CloudOffset     .GetValue(timeOfDay),t);
                }
            }
            if (cloudBornRotate != CloudScript.Profile.CloudBornRotate)
            {
                CloudScript.Profile.CloudBornRotate = cloudBornRotate;
                CloudScript.Profile.CloudRotate = cloudBornRotate;
            }
            CloudScript.UpdateMaterialFromProfile(weatherSystemScript);
        }

        public override void ApplyWeatherZonesInfluence( WeatherSystemProfileScript climateZoneProfile, float timeOfDay, float t)
        {
            float cloudBornRotate = CloudScript.Profile.CloudBornRotate;
            if (CloudScript.Profile.CloudMap != null && climateZoneProfile.CloudProfile.CloudMap == null)
            {
                CloudScript.Profile.Mask = WeatherSystemUtility.FloatInterpolation(CloudScript.Profile.Mask, 0, t);
            }
            else if (CloudScript.Profile.CloudMap == null && climateZoneProfile.CloudProfile.CloudMap == null)            
            {
                CloudScript.Profile.IsEnabled = false;
            }
            else if (CloudScript.Profile.CloudMap == null && climateZoneProfile.CloudProfile.CloudMap != null)
            {
                CloudScript.Profile.CloudMap          = climateZoneProfile.CloudProfile.CloudMap;
                CloudScript.Profile.StepSize          = climateZoneProfile.CloudProfile.StepSize;
                CloudScript.Profile.IsEnabled         = climateZoneProfile.CloudProfile.CloudMap != null;

                cloudBornRotate = climateZoneProfile.CloudProfile.CloudBornRotate  .GetValue(timeOfDay);
                CloudScript.Profile.CloudSpeed        = climateZoneProfile.CloudProfile.CloudSpeed       .GetValue(timeOfDay);
                CloudScript.Profile.CloudAmbient      = climateZoneProfile.CloudProfile.CloudAmbient     .GetValue(timeOfDay);
                CloudScript.Profile.CloudLight        = climateZoneProfile.CloudProfile.CloudLight       .GetValue(timeOfDay);
                CloudScript.Profile.ScatterMultiplier = climateZoneProfile.CloudProfile.ScatterMultiplier.GetValue(timeOfDay);
                CloudScript.Profile.Attenuation       = climateZoneProfile.CloudProfile.Attenuation      .GetValue(timeOfDay);
                CloudScript.Profile.AlphaSaturation   = climateZoneProfile.CloudProfile.AlphaSaturation  .GetValue(timeOfDay);     
                CloudScript.Profile.Mask              = WeatherSystemUtility.FloatInterpolation(0, climateZoneProfile.CloudProfile.Mask.GetValue(timeOfDay),t);
            }
            else if (CloudScript.Profile.CloudMap != null && climateZoneProfile.CloudProfile.CloudMap != null)
            {
                 if (CloudScript.Profile.CloudMap != climateZoneProfile.CloudProfile.CloudMap)
                {   
                    if (t < 0.5)
                    {
                        CloudScript.Profile.Mask = WeatherSystemUtility.FloatInterpolation(CloudScript.Profile.Mask, 0, t * 2);
                        //CloudScript.Profile.CloudMap = CloudScript.Profile.CloudMap;
                    }
                    else
                    {
                        CloudScript.Profile.Mask = WeatherSystemUtility.FloatInterpolation(0, climateZoneProfile.CloudProfile.Mask.GetValue(timeOfDay), (t - 0.5f) * 2);
                        CloudScript.Profile.CloudMap = climateZoneProfile.CloudProfile.CloudMap;
                    }

                    CloudScript.Profile.StepSize          = climateZoneProfile.CloudProfile.StepSize;
                    CloudScript.Profile.IsEnabled         = CloudScript.Profile.CloudMap != null;

                    cloudBornRotate = WeatherSystemUtility.FloatInterpolation(CloudScript.Profile.CloudBornRotate  , climateZoneProfile.CloudProfile.CloudBornRotate  .GetValue(timeOfDay), t);
                    CloudScript.Profile.CloudSpeed        = WeatherSystemUtility.FloatInterpolation(CloudScript.Profile.CloudSpeed       , climateZoneProfile.CloudProfile.CloudSpeed       .GetValue(timeOfDay),t);
                    CloudScript.Profile.CloudAmbient      = WeatherSystemUtility.ColorInterpolation(CloudScript.Profile.CloudAmbient     , climateZoneProfile.CloudProfile.CloudAmbient     .GetValue(timeOfDay),t);
                    CloudScript.Profile.CloudLight        = WeatherSystemUtility.ColorInterpolation(CloudScript.Profile.CloudLight       , climateZoneProfile.CloudProfile.CloudLight       .GetValue(timeOfDay),t);
                    CloudScript.Profile.ScatterMultiplier = WeatherSystemUtility.FloatInterpolation(CloudScript.Profile.ScatterMultiplier, climateZoneProfile.CloudProfile.ScatterMultiplier.GetValue(timeOfDay),t);
                    CloudScript.Profile.Attenuation       = WeatherSystemUtility.FloatInterpolation(CloudScript.Profile.Attenuation      , climateZoneProfile.CloudProfile.Attenuation      .GetValue(timeOfDay),t);
                    CloudScript.Profile.AlphaSaturation   = WeatherSystemUtility.FloatInterpolation(CloudScript.Profile.AlphaSaturation  , climateZoneProfile.CloudProfile.AlphaSaturation  .GetValue(timeOfDay),t);     
                }    
                else
                {
                    CloudScript.Profile.CloudMap          = climateZoneProfile.CloudProfile.CloudMap;
                    CloudScript.Profile.StepSize          = climateZoneProfile.CloudProfile.StepSize;
                    CloudScript.Profile.IsEnabled         = CloudScript.Profile.CloudMap != null;

                    cloudBornRotate = WeatherSystemUtility.FloatInterpolation(CloudScript.Profile.CloudBornRotate  , climateZoneProfile.CloudProfile.CloudBornRotate  .GetValue(timeOfDay), t);
                    CloudScript.Profile.CloudSpeed        = WeatherSystemUtility.FloatInterpolation(CloudScript.Profile.CloudSpeed       , climateZoneProfile.CloudProfile.CloudSpeed       .GetValue(timeOfDay),t);
                    CloudScript.Profile.CloudAmbient      = WeatherSystemUtility.ColorInterpolation(CloudScript.Profile.CloudAmbient     , climateZoneProfile.CloudProfile.CloudAmbient     .GetValue(timeOfDay),t);
                    CloudScript.Profile.CloudLight        = WeatherSystemUtility.ColorInterpolation(CloudScript.Profile.CloudLight       , climateZoneProfile.CloudProfile.CloudLight       .GetValue(timeOfDay),t);
                    CloudScript.Profile.ScatterMultiplier = WeatherSystemUtility.FloatInterpolation(CloudScript.Profile.ScatterMultiplier, climateZoneProfile.CloudProfile.ScatterMultiplier.GetValue(timeOfDay),t);
                    CloudScript.Profile.Attenuation       = WeatherSystemUtility.FloatInterpolation(CloudScript.Profile.Attenuation      , climateZoneProfile.CloudProfile.Attenuation      .GetValue(timeOfDay),t);
                    CloudScript.Profile.AlphaSaturation   = WeatherSystemUtility.FloatInterpolation(CloudScript.Profile.AlphaSaturation  , climateZoneProfile.CloudProfile.AlphaSaturation  .GetValue(timeOfDay),t);     
                    CloudScript.Profile.Mask              = WeatherSystemUtility.FloatInterpolation(CloudScript.Profile.Mask             , climateZoneProfile.CloudProfile.Mask             .GetValue(timeOfDay),t);

                }
            }
            if (cloudBornRotate != CloudScript.Profile.CloudBornRotate)
            {
                CloudScript.Profile.CloudBornRotate = cloudBornRotate;
                CloudScript.Profile.CloudRotate = cloudBornRotate;
            }
            CloudScript.UpdateMaterialFromProfile(weatherSystemScript);
        }

    }
}
