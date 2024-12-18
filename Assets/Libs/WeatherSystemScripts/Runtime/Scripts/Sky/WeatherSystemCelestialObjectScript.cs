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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WeatherSystem
{
    /// <summary>
    /// Makes a directional light a celestial object
    /// </summary>
    [ExecuteInEditMode]
    public class WeatherSystemCelestialObjectScript : WeatherSystemBaseObjectScript
    {
        [Range(-360.0f, 360.0f)]
        [Tooltip("Rotation about y axis - changes how the celestial body orbits over the scene")]
        public float RotateYDegrees;

        [Range(-360.0f, 360.0f)]
        [Tooltip("Rotation about x axis - changes how the celestial body orbits over the scene")]
        public float RotateXDegrees;

    }

}
