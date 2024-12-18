using System;
using UnityEngine;

namespace WeatherSystem
{
    [Serializable]
    public sealed class WeatherSystemFloatProperty : WeatherSystemPropertyBase
    {
        public enum PropertyType
        {
            Slider,
            TimelineCurve,
        }

        public PropertyType type = PropertyType.Slider;
        public float slider;
        public AnimationCurve timelineCurve;

        public WeatherSystemFloatProperty(float slider, AnimationCurve timelineCurve , bool overrideState = true)
        {
            this.slider = slider;
            this.timelineCurve = timelineCurve;
            this.overrideState = overrideState;
        }
        
        public float GetValue(float time)
        {
            switch (type)
            {
                case PropertyType.Slider:
                    return slider;
                
                case PropertyType.TimelineCurve:
                    return timelineCurve.Evaluate(time);
                
                default:
                    return slider;
            }
        }

    }
}