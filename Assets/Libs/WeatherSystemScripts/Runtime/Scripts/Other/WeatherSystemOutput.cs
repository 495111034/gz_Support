using System;
using UnityEngine;

namespace WeatherSystem
{
    [Serializable]
    public class WeatherSystemOutput
    {
        // Not included in build
        #if UNITY_EDITOR
        public string description;
        #endif
        
        public WeatherSystemOutputType type = WeatherSystemOutputType.Slider;
        public float floatOutput;
        public Color colorOutput;
    }
}