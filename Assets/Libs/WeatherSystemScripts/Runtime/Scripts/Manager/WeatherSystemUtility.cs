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

using UnityEngine;

using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace WeatherSystem
{
    public static class WeatherSystemUtility
    {       
        public static void Destroy(UnityEngine.Object obj)
        {
            if (obj != null)
            {
                if (Application.isEditor)
                    GameObject.DestroyImmediate(obj);
                else
                    GameObject.Destroy(obj);
            }
        }

        /// <summary>
        /// Interpolates between two values given an interpolation factor.
        /// </summary>
        public static float FloatInterpolation(float from, float to, float t)
        {
            return from + (to - from) * t;
        }

        /// <summary>
        /// Interpolates between two vectors given an interpolation factor.
        /// </summary>
        public static Vector2 Vector2Interpolation(Vector2 from, Vector2 to, float t)
        {
            Vector2 ret;
            ret.x = from.x + (to.x - from.x) * t;
            ret.y = from.y + (to.y - from.y) * t;
            return ret;
        }

        /// <summary>
        /// Interpolates between two vectors given an interpolation factor.
        /// </summary>
        public static Vector3 Vector3Interpolation(Vector3 from, Vector3 to, float t)
        {
            Vector3 ret;
            ret.x = from.x + (to.x - from.x) * t;
            ret.y = from.y + (to.y - from.y) * t;
            ret.z = from.z + (to.z - from.z) * t;
            return ret;
        }

        public static Vector4 Vector4Interpolation(Vector4 from, Vector4 to, float t)
        {
            Vector4 ret;
            ret.x = from.x + (to.x - from.x) * t;
            ret.y = from.y + (to.y - from.y) * t;
            ret.z = from.z + (to.z - from.z) * t;
            ret.w = from.w + (to.w - from.w) * t;
            return ret;
        }

        /// <summary>
        /// Interpolates between two colors given an interpolation factor.
        /// </summary>
        public static  Color ColorInterpolation(Color from, Color to, float t)
        {
            Color ret;
            ret.r = from.r + (to.r - from.r) * t;
            ret.g = from.g + (to.g - from.g) * t;
            ret.b = from.b + (to.b - from.b) * t;
            ret.a = from.a + (to.a - from.a) * t;
            return ret;
        }

    }
}
   
