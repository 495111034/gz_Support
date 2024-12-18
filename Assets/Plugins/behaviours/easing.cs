using System;
using UnityEngine;

namespace easing
{
    
    public delegate float EaseFunc(float time, float begin, float change, float duration);

    public enum EaseType
    {
        None,
        Linear,
        Back,
        Bounce,
        Circ,

        Cubic,
        Expo,
        Quad,
        Quart,

        Quint,
        Sine,
        Strong,
    }

    public enum InOutType
    {
        In,
        Out,
        InOut,
    }

    // 工具类
    public static class EaseUtils
    {
        // 获取函数
        public static EaseFunc GetEaseFunc(EaseType type1, InOutType type2)
        {
            return func_arr[(int)type1, (int)type2];
        }
        static EaseFunc[,] func_arr = new EaseFunc[,]
        {
            { None.rate, None.rate, None.rate },
            { Linear.easeIn, Linear.easeOut, Linear.easeInOut },
            { Back.easeIn, Back.easeOut, Back.easeInOut },
            { Bounce.easeIn, Bounce.easeOut, Bounce.easeInOut },
            { Circ.easeIn, Circ.easeOut, Circ.easeInOut },

            { Cubic.easeIn, Cubic.easeOut, Cubic.easeInOut },
            { Expo.easeIn, Expo.easeOut, Expo.easeInOut },
            { Quad.easeIn, Quad.easeOut, Quad.easeInOut },
            { Quart.easeIn, Quart.easeOut, Quart.easeInOut },

            { Quint.easeIn, Quint.easeOut, Quint.easeInOut },
            { Sine.easeIn, Sine.easeOut, Sine.easeInOut },
            { Strong.easeIn, Strong.easeOut, Strong.easeInOut },
        };       

        // 获取插值
        public static float GetRate(float time_pass, float duration, EaseFunc func)
        {
            if (duration <= 0) return 1;
            var rate = Mathf.Clamp01(time_pass / duration);
            if (func != null) rate = func(rate, 0, 1, 1);
            return rate;
        }

    }

    public class Back
    {
        const float s = 1.70158f;

        public static float easeIn(float t, float b, float c, float d)
        {
            return c * (t /= d) * t * ((s + 1) * t - s) + b;
        }
        public static float easeOut(float t, float b, float c, float d)
        {
            return c * ((t = t / d - 1) * t * ((s + 1) * t + s) + 1) + b;
        }
        public static float easeInOut(float t, float b, float c, float d)
        {
            float s = Back.s;
            if ((t /= d * 0.5f) < 1) return c * 0.5f * (t * t * (((s *= (1.525f)) + 1) * t - s)) + b;
            return c / 2 * ((t -= 2) * t * (((s *= (1.525f)) + 1) * t + s) + 2) + b;
        }
    }

    public class None
    {
        public static float rate(float t, float b, float c, float d)
        {
            return t;
        }
    }

    public class Bounce
    {
        public static float easeOut(float t, float b, float c, float d)
        {
            if ((t /= d) < (1 / 2.75f))
            {
                return c * (7.5625f * t * t) + b;
            }
            else if (t < (2 / 2.75f))
            {
                return c * (7.5625f * (t -= (1.5f / 2.75f)) * t + .75f) + b;
            }
            else if (t < (2.5f / 2.75f))
            {
                return c * (7.5625f * (t -= (2.25f / 2.75f)) * t + .9375f) + b;
            }
            else
            {
                return c * (7.5625f * (t -= (2.625f / 2.75f)) * t + .984375f) + b;
            }
        }
        public static float easeIn(float t, float b, float c, float d)
        {
            return c - easeOut(d - t, 0, c, d) + b;
        }
        public static float easeInOut(float t, float b, float c, float d)
        {
            if (t < d * 0.5f) return easeIn(t * 2, 0, c, d) * .5f + b;
            else return easeOut(t * 2 - d, 0, c, d) * .5f + c * .5f + b;
        }
    }

    public class Circ
    {
        public static float easeIn(float t, float b, float c, float d)
        {
            return -c * (Mathf.Sqrt(1 - (t /= d) * t) - 1) + b;
        }
        public static float easeOut(float t, float b, float c, float d)
        {
            return c * Mathf.Sqrt(1 - (t = t / d - 1) * t) + b;
        }
        public static float easeInOut(float t, float b, float c, float d)
        {
            if ((t /= d * 0.5f) < 1) return -c * 0.5f * (Mathf.Sqrt(1 - t * t) - 1) + b;
            return c * 0.5f * (Mathf.Sqrt(1 - (t -= 2) * t) + 1) + b;
        }
    }


    public class Cubic
    {
        const uint Power = 2;

        public static float easeIn(float t, float b, float c, float d)
        {
            return c * (t /= d) * t * t + b;
        }
        public static float easeOut(float t, float b, float c, float d)
        {
            return c * ((t = t / d - 1) * t * t + 1) + b;
        }
        public static float easeInOut(float t, float b, float c, float d)
        {
            if ((t /= d * 0.5f) < 1) return c * 0.5f * t * t * t + b;
            return c * 0.5f * ((t -= 2) * t * t + 2) + b;
        }
    }

    //public class Elastic {
    //    private static const float _2PI = Mathf.PI * 2;

    //    public static float easeIn (float t, float b, float c, float d, float a = 0, float p = 0 ){
    //        float s;
    //        if (t==0) return b;  if ((t/=d)==1) return b+c;  if (!p) p=d*.3f;
    //        if (!a || (c > 0 && a < c) || (c < 0 && a < -c)) { a=c; s = p/4; }
    //        else s = p/_2PI * Mathf.Asin (c/a);
    //        return -(a*Mathf.Pow(2,10*(t-=1)) * Mathf.Sin( (t*d-s)*_2PI/p )) + b;
    //    }
    //    public static float easeOut (float t, float b, float c, float d, float a = 0, float p = 0 ){
    //        float s;
    //        if (t==0) return b;  if ((t/=d)==1) return b+c;  if (!p) p=d*.3f;
    //        if (!a || (c > 0 && a < c) || (c < 0 && a < -c)) { a=c; s = p/4; }
    //        else s = p/_2PI * Mathf.Asin (c/a);
    //        return (a*Mathf.Pow(2,-10*t) * Mathf.Sin( (t*d-s)*_2PI/p ) + c + b);
    //    }
    //    public static float easeInOut (float t, float b, float c, float d, float a = 0, float p = 0 ){
    //        float s;
    //        if (t==0) return b;  if ((t/=d*0.5f)==2) return b+c;  if (!p) p=d*(.3f*1.5f);
    //        if (!a || (c > 0 && a < c) || (c < 0 && a < -c)) { a=c; s = p/4; }
    //        else s = p/_2PI * Mathf.Asin (c/a);
    //        if (t < 1) return -.5f*(a*Mathf.Pow(2,10*(t-=1)) * Mathf.Sin( (t*d-s)*_2PI/p )) + b;
    //        return a*Mathf.Pow(2,-10*(t-=1)) * Mathf.Sin( (t*d-s)*_2PI/p )*.5f + c + b;
    //    }
    //}

    public class Expo
    {
        public static float easeIn(float t, float b, float c, float d)
        {
            return (t == 0) ? b : c * Mathf.Pow(2, 10 * (t / d - 1)) + b - c * 0.001f;
        }
        public static float easeOut(float t, float b, float c, float d)
        {
            return (t == d) ? b + c : c * (-Mathf.Pow(2, -10 * t / d) + 1) + b;
        }
        public static float easeInOut(float t, float b, float c, float d)
        {
            if (t == 0) return b;
            if (t == d) return b + c;
            if ((t /= d * 0.5f) < 1) return c * 0.5f * Mathf.Pow(2, 10 * (t - 1)) + b;
            return c * 0.5f * (-Mathf.Pow(2, -10 * --t) + 2) + b;
        }
    }


    public class Linear
    {
        const uint Power = 0;

        public static float easeNone(float t, float b, float c, float d)
        {
            return c * t / d + b;
        }
        public static float easeIn(float t, float b, float c, float d)
        {
            return c * t / d + b;
        }
        public static float easeOut(float t, float b, float c, float d)
        {
            return c * t / d + b;
        }
        public static float easeInOut(float t, float b, float c, float d)
        {
            return c * t / d + b;
        }
    }


    public class Quad
    {
        const uint Power = 1;

        public static float easeIn(float t, float b, float c, float d)
        {
            return c * (t /= d) * t + b;
        }
        public static float easeOut(float t, float b, float c, float d)
        {
            return -c * (t /= d) * (t - 2) + b;
        }
        public static float easeInOut(float t, float b, float c, float d)
        {
            if ((t /= d * 0.5f) < 1) return c * 0.5f * t * t + b;
            return -c * 0.5f * ((--t) * (t - 2) - 1) + b;
        }
    }


    public class Quart
    {
        const uint Power = 3;

        public static float easeIn(float t, float b, float c, float d)
        {
            return c * (t /= d) * t * t * t + b;
        }
        public static float easeOut(float t, float b, float c, float d)
        {
            return -c * ((t = t / d - 1) * t * t * t - 1) + b;
        }
        public static float easeInOut(float t, float b, float c, float d)
        {
            if ((t /= d * 0.5f) < 1) return c * 0.5f * t * t * t * t + b;
            return -c * 0.5f * ((t -= 2) * t * t * t - 2) + b;
        }
    }


    public class Quint
    {
        const uint Power = 4;

        public static float easeIn(float t, float b, float c, float d)
        {
            return c * (t /= d) * t * t * t * t + b;
        }
        public static float easeOut(float t, float b, float c, float d)
        {
            return c * ((t = t / d - 1) * t * t * t * t + 1) + b;
        }
        public static float easeInOut(float t, float b, float c, float d)
        {
            if ((t /= d * 0.5f) < 1) return c * 0.5f * t * t * t * t * t + b;
            return c * 0.5f * ((t -= 2) * t * t * t * t + 2) + b;
        }
    }

    public class Sine
    {
        const float _HALF_PI = Mathf.PI * 0.5f;

        public static float easeIn(float t, float b, float c, float d)
        {
            return -c * Mathf.Cos(t / d * _HALF_PI) + c + b;
        }
        public static float easeOut(float t, float b, float c, float d)
        {
            return c * Mathf.Sin(t / d * _HALF_PI) + b;
        }
        public static float easeInOut(float t, float b, float c, float d)
        {
            return -c * 0.5f * (Mathf.Cos(Mathf.PI * t / d) - 1) + b;
        }
    }

    public class Strong
    {
        const uint Power = 4;

        public static float easeIn(float t, float b, float c, float d)
        {
            return c * (t /= d) * t * t * t * t + b;
        }
        public static float easeOut(float t, float b, float c, float d)
        {
            return c * ((t = t / d - 1) * t * t * t * t + 1) + b;
        }
        public static float easeInOut(float t, float b, float c, float d)
        {
            if ((t /= d * 0.5f) < 1) return c * 0.5f * t * t * t * t * t + b;
            return c * 0.5f * ((t -= 2) * t * t * t * t + 2) + b;
        }
    }

}