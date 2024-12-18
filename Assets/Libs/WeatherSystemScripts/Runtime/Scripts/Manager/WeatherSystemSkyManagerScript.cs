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
using UnityEngine.Serialization;

namespace WeatherSystem
{
    /// <summary>
    /// Sky manager interface
    /// </summary>
    public class ISkyManager : IWeatherSystemManager
    {
        [FormerlySerializedAs("SkySphereScript")]
        [Header("Dependencies")]
        [Tooltip("Sky script")]
        public WeatherSystemSkyScript skyScript;
    }

    /// <summary>
    /// Cloud manager default implementation
    /// </summary>
    public class WeatherSystemSkyManagerScript : ISkyManager
    {
        /// <summary>Sky sphere script</summary>
     

        public override void ApplyWeatherDefaultSettings( WeatherSystemProfileScript profile, float timeOfDay)
        {            
            skyScript.Profile.TimeOfDay           = timeOfDay;            
            skyScript.Profile.TimeOfDayOffset     = profile.SkyProfile.TimeOfDayOffset     .GetValue(timeOfDay);;
            skyScript.Profile.SunDirection        = profile.SkyProfile.SunDirection        .GetValue(timeOfDay);
            skyScript.Profile.SunXDirection       = profile.SkyProfile.SunXDirection       .GetValue(timeOfDay);
            skyScript.Profile.ZenithColor         = profile.SkyProfile.ZenithColor         .GetValue(timeOfDay);             
            skyScript.Profile.HorizonColor        = profile.SkyProfile.HorizonColor        .GetValue(timeOfDay);
            skyScript.Profile.HorizonFalloff      = profile.SkyProfile.HorizonFalloff      .GetValue(timeOfDay);
            skyScript.Profile.MieScatterColor     = profile.SkyProfile.MieScatterColor     .GetValue(timeOfDay);
            skyScript.Profile.MieScatterFactor    = profile.SkyProfile.MieScatterFactor    .GetValue(timeOfDay);
            skyScript.Profile.MieScatterPower     = profile.SkyProfile.MieScatterPower     .GetValue(timeOfDay);
            
            skyScript.Profile.IsCloudEnabled     = profile.SkyProfile.CloudMap != null;
            skyScript.Profile.CloudMap           = profile.SkyProfile.CloudMap;
            skyScript.Profile.CloudMapOffset     = profile.SkyProfile.CloudMapOffset.GetValue(timeOfDay);
            skyScript.Profile.Mask               = profile.SkyProfile.Mask.GetValue(timeOfDay);
            skyScript.Profile.DistortionTile     = profile.SkyProfile.DistortionTile.GetValue(timeOfDay);
            skyScript.Profile.DistortionAmount   = profile.SkyProfile.DistortionAmount.GetValue(timeOfDay);
            skyScript.Profile.DistortionMirror   = profile.SkyProfile.DistortionMirror.GetValue(timeOfDay);


            skyScript.Profile.SunEnabled          = profile.SkyProfile.SunEnabled          .GetValue(timeOfDay) > 0.0f;
            skyScript.Profile.SunBrightness       = profile.SkyProfile.SunBrightness       .GetValue(timeOfDay);           
            skyScript.Profile.SunDetail           = profile.SkyProfile.SunDetail           .GetValue(timeOfDay);       
            skyScript.Profile.SunDistance         = profile.SkyProfile.SunDistance         .GetValue(timeOfDay);         
            skyScript.Profile.SunColor            = profile.SkyProfile.SunColor            .GetValue(timeOfDay);   
            skyScript.Profile.MoonEnabled         = profile.SkyProfile.MoonEnabled         .GetValue(timeOfDay) > 0.0f;
            skyScript.Profile.MoonBrightness      = profile.SkyProfile.MoonBrightness      .GetValue(timeOfDay);            
            skyScript.Profile.MoonDetail          = profile.SkyProfile.MoonDetail          .GetValue(timeOfDay);        
            skyScript.Profile.MoonDistance        = profile.SkyProfile.MoonDistance        .GetValue(timeOfDay);
            skyScript.Profile.MoonColor           = profile.SkyProfile.MoonColor           .GetValue(timeOfDay);
            skyScript.Profile.StarEnabled         = profile.SkyProfile.StarEnabled         .GetValue(timeOfDay) > 0.0f;
            skyScript.Profile.StarBrightness      = profile.SkyProfile.StarBrightness      .GetValue(timeOfDay);
            skyScript.Profile.StarColor           = profile.SkyProfile.StarColor           .GetValue(timeOfDay);
            skyScript.UpdateMaterialFromProfile(weatherSystemScript);
        }

        public override void ApplyGlobalWeatherTransition( WeatherSystemProfileScript from, WeatherSystemProfileScript to, float timeOfDay, float t)
        {
            skyScript.Profile.IsCloudEnabled      = true;
            skyScript.Profile.TimeOfDay           = timeOfDay;
            skyScript.Profile.TimeOfDayOffset     = WeatherSystemUtility.FloatInterpolation(from.SkyProfile.TimeOfDayOffset     .GetValue(timeOfDay), to.SkyProfile.TimeOfDayOffset     .GetValue(timeOfDay),t);
            skyScript.Profile.SunDirection        = WeatherSystemUtility.FloatInterpolation(from.SkyProfile.SunDirection        .GetValue(timeOfDay), to.SkyProfile.SunDirection        .GetValue(timeOfDay),t);
            skyScript.Profile.SunXDirection       = WeatherSystemUtility.FloatInterpolation(from.SkyProfile.SunXDirection       .GetValue(timeOfDay), to.SkyProfile.SunXDirection       .GetValue(timeOfDay),t);
            skyScript.Profile.ZenithColor         = WeatherSystemUtility.ColorInterpolation(from.SkyProfile.ZenithColor         .GetValue(timeOfDay), to.SkyProfile.ZenithColor         .GetValue(timeOfDay),t);             
            skyScript.Profile.HorizonColor        = WeatherSystemUtility.ColorInterpolation(from.SkyProfile.HorizonColor        .GetValue(timeOfDay), to.SkyProfile.HorizonColor        .GetValue(timeOfDay),t);
            skyScript.Profile.HorizonFalloff      = WeatherSystemUtility.FloatInterpolation(from.SkyProfile.HorizonFalloff      .GetValue(timeOfDay), to.SkyProfile.HorizonFalloff      .GetValue(timeOfDay),t);
            skyScript.Profile.MieScatterColor     = WeatherSystemUtility.ColorInterpolation(from.SkyProfile.MieScatterColor     .GetValue(timeOfDay), to.SkyProfile.MieScatterColor     .GetValue(timeOfDay),t);
            skyScript.Profile.MieScatterFactor    = WeatherSystemUtility.FloatInterpolation(from.SkyProfile.MieScatterFactor    .GetValue(timeOfDay), to.SkyProfile.MieScatterFactor    .GetValue(timeOfDay),t);
            skyScript.Profile.MieScatterPower     = WeatherSystemUtility.FloatInterpolation(from.SkyProfile.MieScatterPower     .GetValue(timeOfDay), to.SkyProfile.MieScatterPower     .GetValue(timeOfDay),t);
         
            if (from.SkyProfile.CloudMap != null && to.SkyProfile.CloudMap == null)
            {
                skyScript.Profile.Mask = WeatherSystemUtility.FloatInterpolation(from.SkyProfile.Mask.GetValue(timeOfDay), 0, t);
            }
            else if (from.SkyProfile.CloudMap == null && to.SkyProfile.CloudMap == null)
            {
                skyScript.Profile.IsCloudEnabled = false;
            }
            else if (from.SkyProfile.CloudMap == null && to.SkyProfile.CloudMap != null)
            {                
                skyScript.Profile.CloudMap           = to.SkyProfile.CloudMap;
                skyScript.Profile.Mask               = WeatherSystemUtility.FloatInterpolation(0, to.SkyProfile.Mask.GetValue(timeOfDay), t);
                skyScript.Profile.CloudMapOffset     = to.SkyProfile.CloudMapOffset.GetValue(timeOfDay);
                skyScript.Profile.DistortionTile     = to.SkyProfile.DistortionTile.GetValue(timeOfDay);
                skyScript.Profile.DistortionAmount   = to.SkyProfile.DistortionAmount.GetValue(timeOfDay);
                skyScript.Profile.DistortionMirror   = to.SkyProfile.DistortionMirror.GetValue(timeOfDay);

            }
            else if (from.SkyProfile.CloudMap != null && to.SkyProfile.CloudMap != null)
            {
                if (from.SkyProfile.CloudMap != to.SkyProfile.CloudMap)
                {
                    if (t < 0.5)
                    {
                        skyScript.Profile.Mask = WeatherSystemUtility.FloatInterpolation(from.SkyProfile.Mask.GetValue(timeOfDay), 0, t * 2);
                        skyScript.Profile.CloudMap = from.SkyProfile.CloudMap;
                        skyScript.Profile.CloudMapOffset = from.SkyProfile.CloudMapOffset.GetValue(timeOfDay);
                    }
                    else
                    {
                        skyScript.Profile.Mask = WeatherSystemUtility.FloatInterpolation(0, to.SkyProfile.Mask.GetValue(timeOfDay), (t - 0.5f) * 2);
                        skyScript.Profile.CloudMap = to.SkyProfile.CloudMap;
                        skyScript.Profile.CloudMapOffset = to.SkyProfile.CloudMapOffset.GetValue(timeOfDay);
                    }
                    //skyScript.Profile.CloudMapOffset   = WeatherSystemUtility.FloatInterpolation(from.SkyProfile.CloudMapOffset.GetValue(timeOfDay), to.SkyProfile.CloudMapOffset.GetValue(timeOfDay), t);
                    skyScript.Profile.DistortionTile   = WeatherSystemUtility.FloatInterpolation(from.SkyProfile.DistortionTile.GetValue(timeOfDay), to.SkyProfile.DistortionTile.GetValue(timeOfDay),t);
                    skyScript.Profile.DistortionAmount = WeatherSystemUtility.FloatInterpolation(from.SkyProfile.DistortionAmount.GetValue(timeOfDay), to.SkyProfile.DistortionAmount.GetValue(timeOfDay),t);
                    skyScript.Profile.DistortionMirror = to.SkyProfile.DistortionMirror.GetValue(timeOfDay);

                }
                else
                {
                    skyScript.Profile.CloudMap         = to.SkyProfile.CloudMap;
                    skyScript.Profile.CloudMapOffset   = to.SkyProfile.CloudMapOffset.GetValue(timeOfDay);
                    skyScript.Profile.Mask             = WeatherSystemUtility.FloatInterpolation(from.SkyProfile.Mask.GetValue(timeOfDay), to.SkyProfile.Mask.GetValue(timeOfDay), t);
                    skyScript.Profile.DistortionTile   = WeatherSystemUtility.FloatInterpolation(from.SkyProfile.DistortionTile.GetValue(timeOfDay), to.SkyProfile.DistortionTile.GetValue(timeOfDay),t);
                    skyScript.Profile.DistortionAmount = WeatherSystemUtility.FloatInterpolation(from.SkyProfile.DistortionAmount.GetValue(timeOfDay), to.SkyProfile.DistortionAmount.GetValue(timeOfDay),t);
                    skyScript.Profile.DistortionMirror = to.SkyProfile.DistortionMirror.GetValue(timeOfDay);
                }
            }            
         
            skyScript.Profile.SunEnabled          = WeatherSystemUtility.FloatInterpolation(from.SkyProfile.SunEnabled          .GetValue(timeOfDay), to.SkyProfile.SunEnabled          .GetValue(timeOfDay),t) > 0.0f;
            skyScript.Profile.SunBrightness       = WeatherSystemUtility.FloatInterpolation(from.SkyProfile.SunBrightness       .GetValue(timeOfDay), to.SkyProfile.SunBrightness       .GetValue(timeOfDay),t);           
            skyScript.Profile.SunDetail           = WeatherSystemUtility.FloatInterpolation(from.SkyProfile.SunDetail           .GetValue(timeOfDay), to.SkyProfile.SunDetail           .GetValue(timeOfDay),t);       
            skyScript.Profile.SunDistance         = WeatherSystemUtility.FloatInterpolation(from.SkyProfile.SunDistance         .GetValue(timeOfDay), to.SkyProfile.SunDistance         .GetValue(timeOfDay),t);         
            skyScript.Profile.SunColor            = WeatherSystemUtility.ColorInterpolation(from.SkyProfile.SunColor            .GetValue(timeOfDay), to.SkyProfile.SunColor            .GetValue(timeOfDay),t);     
            skyScript.Profile.MoonEnabled         = WeatherSystemUtility.FloatInterpolation(from.SkyProfile.MoonEnabled         .GetValue(timeOfDay), to.SkyProfile.MoonEnabled         .GetValue(timeOfDay),t) > 0.0f;
            skyScript.Profile.MoonBrightness      = WeatherSystemUtility.FloatInterpolation(from.SkyProfile.MoonBrightness      .GetValue(timeOfDay), to.SkyProfile.MoonBrightness      .GetValue(timeOfDay),t);            
            skyScript.Profile.MoonDetail          = WeatherSystemUtility.FloatInterpolation(from.SkyProfile.MoonDetail          .GetValue(timeOfDay), to.SkyProfile.MoonDetail          .GetValue(timeOfDay),t);        
            skyScript.Profile.MoonDistance        = WeatherSystemUtility.FloatInterpolation(from.SkyProfile.MoonDistance        .GetValue(timeOfDay), to.SkyProfile.MoonDistance        .GetValue(timeOfDay),t);
            skyScript.Profile.MoonColor           = WeatherSystemUtility.ColorInterpolation(from.SkyProfile.MoonColor           .GetValue(timeOfDay), to.SkyProfile.MoonColor           .GetValue(timeOfDay),t);
            skyScript.Profile.StarEnabled         = WeatherSystemUtility.FloatInterpolation(from.SkyProfile.StarEnabled         .GetValue(timeOfDay), to.SkyProfile.StarEnabled         .GetValue(timeOfDay),t) > 0.0f;
            skyScript.Profile.StarBrightness      = WeatherSystemUtility.FloatInterpolation(from.SkyProfile.StarBrightness      .GetValue(timeOfDay), to.SkyProfile.StarBrightness      .GetValue(timeOfDay),t);
            skyScript.Profile.StarColor           = WeatherSystemUtility.ColorInterpolation(from.SkyProfile.StarColor           .GetValue(timeOfDay), to.SkyProfile.StarColor           .GetValue(timeOfDay),t);
            
            
            skyScript.UpdateMaterialFromProfile(weatherSystemScript);
        }

        public override void ApplyWeatherZonesInfluence( WeatherSystemProfileScript climateZoneProfile, float timeOfDay, float t)
        {
           
            skyScript.UpdateMaterialFromProfile(weatherSystemScript);
        }

    }
}
