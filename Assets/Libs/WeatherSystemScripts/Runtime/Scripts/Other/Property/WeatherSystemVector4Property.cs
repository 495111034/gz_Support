using System;
using System.Collections.Generic;
using UnityEngine;

namespace WeatherSystem
{
    [Serializable]
    public sealed class Vector4GradientKey
    {
        public Vector4GradientKey(Vector4 value, float time)
        {
            this.value = value;
            this.time = time;
        }
        public float time = 0; // 0 - 1
        public Vector4 value = WeatherSystemVector4Property.DefaultVector4Value;

        public void Copy(Vector4GradientKey key)
        {
            this.value = key.value;
            this.time = key.time;
        }
    }

    [Serializable]
    public sealed class WeatherSystemVector4Property : WeatherSystemPropertyBase
    {
        public static Vector4 DefaultVector4Value = new Vector4(1 , 1, 1, 0);

        public enum PropertyType
        {
            Vector4,
            Vector4Gradient,
        }

        public PropertyType type = PropertyType.Vector4;
        [ColorUsage(true, true)]
        public Vector4 value = WeatherSystemVector4Property.DefaultVector4Value;
        [GradientUsageAttribute(true)]
        public List<Vector4GradientKey> timelineGradient = new List<Vector4GradientKey>();

        public WeatherSystemVector4Property(Vector4 value, bool overrideState = true)
        {
            this.value = value;
            this.overrideState = overrideState;
        }

        public Vector4 GetValue(float time)
        {
           
            switch (type)
            {
                case PropertyType.Vector4:
                    return value;

                case PropertyType.Vector4Gradient:
                    return GetGradientEvaluate(time / 24.0f);
                default:
                    return value;
            }
        }

        //time range 0 - 1
        public Vector4 GetGradientEvaluate(float time)
        {
            time = Mathf.Clamp01(time);
            if (timelineGradient == null || timelineGradient.Count == 0)
            {
                return WeatherSystemVector4Property.DefaultVector4Value;
            }
            int length = timelineGradient.Count;
            Vector4GradientKey preKey = new Vector4GradientKey(WeatherSystemVector4Property.DefaultVector4Value, 0);
            Vector4GradientKey nextKey = new Vector4GradientKey(WeatherSystemVector4Property.DefaultVector4Value, 0);
            for (int i = 0; i < length; i++)
            {
                Vector4GradientKey key = timelineGradient[i];
                preKey.Copy(nextKey);
                nextKey.Copy(key);
                if (time >= preKey.time &&  time <= nextKey.time)
                {
                    break;
                }
            }
            float timeRange = nextKey.time - preKey.time;
            float radio = timeRange > 0 ? (time - preKey.time) / timeRange : 1;
            return WeatherSystemUtility.Vector4Interpolation(preKey.value , nextKey.value, radio);
        }

        public List<Vector4GradientKey> GetGradient()
        {
            return timelineGradient;
        }

        //time Range(0 - 1)
        public void AddGradient(Vector4 value , float time)
        {
            time = Mathf.Clamp01(time);
            time = (float)Math.Round(time, 3); // 保留小数点后三位
            int length = timelineGradient.Count;
            //至多插入10个节点
            if (length >= 8)
            {
                return;
            }

            bool hasInsert = false;
            for (int i = 0; i < length; i++)
            {
                Vector4GradientKey key = timelineGradient[i];
                if(!hasInsert && key.time >= time)
                {
                    hasInsert = true;
                    int insertPos = i;
                    if (key.time == time)//相等时在插入到后端
                    {
                        insertPos++;
                    }
                    Vector4GradientKey newKey = new Vector4GradientKey(value, time);
                    timelineGradient.Insert(insertPos, newKey);
                    break;
                }
            }
            if (!hasInsert)
            {
                timelineGradient.Add(new Vector4GradientKey(value, time));
            }
        }

        public void RemoveGradient(int index)
        {
            if (index >= 0 && index < timelineGradient.Count)
            {
                timelineGradient.RemoveAt(index);
            }
        }

        public void RemoveAll()
        {
            timelineGradient.Clear();
        }
    }
}
