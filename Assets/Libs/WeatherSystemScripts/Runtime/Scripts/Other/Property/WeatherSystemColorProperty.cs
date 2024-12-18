using System;
using UnityEngine;

namespace WeatherSystem
{
    [Serializable]
    public sealed class WeatherSystemColorProperty : WeatherSystemPropertyBase
    {
        public enum PropertyType
        {
            Color,
            TimelineGradient,
        }

        public PropertyType type = PropertyType.Color;
        [ColorUsage(true, true)]
        public Color color;
        [GradientUsageAttribute(true)]
        public Gradient timelineGradient;

        public WeatherSystemColorProperty(Color color, Gradient timelineGradient , bool overrideState = true)
        {
            this.color = color;
            this.timelineGradient = timelineGradient;
            this.overrideState = overrideState;
        }
        
        public Color GetValue(float time)
        {
            switch (type)
            {
                case PropertyType.Color:
                    return color;
                
                case PropertyType.TimelineGradient:
                    return timelineGradient.Evaluate(time / 24.0f);                
                default:
                    return color;
            }
        }

        public Vector4 GetVect4Value(float time)
        {
            switch (type)
            {
                case PropertyType.Color:
                    return color;

                case PropertyType.TimelineGradient:
                    return timelineGradient.Evaluate(time / 24.0f);
                default:
                    return color;
            }
        }
    }
}