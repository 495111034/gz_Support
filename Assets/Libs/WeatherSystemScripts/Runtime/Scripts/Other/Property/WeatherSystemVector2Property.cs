using System;
using UnityEngine;

namespace WeatherSystem
{
    [Serializable]
    public sealed class WeatherSystemVector2Property
    {
        public enum PropertyType
        {
            Slider,
            TimelineCurve,
        }

        public PropertyType type = PropertyType.Slider;
        public float sliderX;
        public AnimationCurve timelineCurveX;
        public float sliderY;
        public AnimationCurve timelineCurveY;

        public WeatherSystemVector2Property(float sliderX, AnimationCurve timelineCurveX, float sliderY, AnimationCurve timelineCurveY)
        {
            this.sliderX = sliderX;
            this.timelineCurveX = timelineCurveX;
            this.sliderY = sliderY;
            this.timelineCurveY = timelineCurveY;
        }
        
        public Vector2 GetValue(float time)
        {
            switch (type)
            {
                case PropertyType.Slider:                    
                    return new Vector2(sliderX,sliderY);
                
                case PropertyType.TimelineCurve:
                    return new Vector2(timelineCurveX.Evaluate(time), timelineCurveY.Evaluate(time));
                
                default:
                    return new Vector2(sliderX, sliderY); 
            }
        }

    }
}