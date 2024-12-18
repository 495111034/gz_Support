using System;
using UnityEngine;

namespace WeatherSystem
{
    [Serializable]
    public sealed class WeatherSystemOutputProperty
    {
        public WeatherSystemOutputType type = WeatherSystemOutputType.Slider;
        public float slider = 0.0f;
        public AnimationCurve timelineCurve = AnimationCurve.Linear(0.0f, 0.0f, 24.0f, 0.0f);
        public AnimationCurve sunCurve = AnimationCurve.Linear(-1.0f, 0.0f, 1.0f, 0.0f);
        public AnimationCurve moonCurve = AnimationCurve.Linear(-1.0f, 0.0f, 1.0f, 0.0f);
        public Color color = Color.white;
        public Gradient timelineGradient = new Gradient();
        public Gradient sunGradient = new Gradient();
        public Gradient moonGradient = new Gradient();
        
        public float GetFloatValue(float time, float sunElevation, float moonElevation)
        {
            switch (type)
            {
                case WeatherSystemOutputType.Slider:
                    return slider;
                
                case WeatherSystemOutputType.TimelineCurve:
                    return timelineCurve.Evaluate(time);
                
                case WeatherSystemOutputType.SunCurve:
                    return sunCurve.Evaluate(sunElevation);
                
                case WeatherSystemOutputType.MoonCurve:
                    return moonCurve.Evaluate(moonElevation);
            }

            return slider;
        }
        
        public Color GetColorValue(float time, float sunElevation, float moonElevation)
        {
            switch (type)
            {
                case WeatherSystemOutputType.Color:
                    return color;
                
                case WeatherSystemOutputType.TimelineGradient:
                    return timelineGradient.Evaluate(time / 24.0f);
                
                case WeatherSystemOutputType.SunGradient:
                    return sunGradient.Evaluate(Mathf.InverseLerp(-1.0f, 1.0f, sunElevation));
                
                case WeatherSystemOutputType.MoonGradient:
                    return moonGradient.Evaluate(Mathf.InverseLerp(-1.0f, 1.0f, moonElevation));
            }
            
            return color;
        }
    }
}