using System;
using UnityEngine;
using UnityEngine.Events;

namespace WeatherSystem
{
      
    public enum WeatherSystemTimeSystem
    {
        Simple,
        Realistic
    }
    
    public enum WeatherSystemTimeRepeatMode
    {
        Off,
        ByDay,
        ByMonth,
        ByYear
    }
    
    public enum WeatherSystemScatteringMode
    {
        Automatic,
        CustomColor
    }

    public enum WeatherSystemCloudMode
    {
        EmptySky,
        StaticClouds,
        DynamicClouds
    }

    public enum WeatherSystemTimeDirection
    {
        Forward,
        Back
    }

    public enum WeatherSystemEventScanMode
    {
        ByMinute,
        ByHour
    }
    
    public enum WeatherSystemOutputType
    {
        Slider,
        TimelineCurve,
        SunCurve,
        MoonCurve,
        Color,
        TimelineGradient,
        SunGradient,
        MoonGradient
    }

    public enum WeatherSystemReflectionProbeState
    {
        On,
        Off
    }

    public enum WeatherSystemShaderUpdateMode
    {
        Global,
        ByMaterial
    }

    [Serializable]
    public sealed class WeatherSystemEventAction
    {
        // Not included in build
        #if UNITY_EDITOR
        public bool isExpanded = true;
        #endif
        
        public UnityEvent eventAction;
        public int hour = 6;
        public int minute = 0;
        public int year = 2020;
        public int month = 1;
        public int day = 1;
    }
    
    /// <summary>
    /// Thunder settings container.
    /// </summary>
    [Serializable]
    public sealed class WeatherSystemThunderSettings
    {
        public Transform thunderPrefab;
        public AudioClip audioClip;
        public AnimationCurve lightFrequency;
        public float audioDelay;
        public Vector3 position;
    }

    public class DragPathAttribute : PropertyAttribute { };

}