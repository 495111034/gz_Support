using System;
using UnityEngine;

namespace WeatherSystem
{
    [Serializable]
    public sealed class WeatherSystemIntProperty : WeatherSystemPropertyBase
    {
        public enum PropertyType
        {
            Slider,
            TimelineCurve,
        }

        public PropertyType type = PropertyType.Slider;
        public int slider;
        public AnimationCurve timelineCurve;

        public WeatherSystemIntProperty(int slider, AnimationCurve timelineCurve , bool overrideState = true)
        {
            this.slider = slider;
            this.timelineCurve = timelineCurve;
            this.overrideState = overrideState;
        }
        
        public int GetValue(float time)
        {
            switch (type)
            {
                case PropertyType.Slider:
                    return slider;
                
                case PropertyType.TimelineCurve:
                    return (int)timelineCurve.Evaluate(time);
                
                default:
                    return slider;
            }
        }


    }
}