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
using UnityEngine;

namespace WeatherSystem
{
    /// <summary>
    /// Sky sphere renderer and general sky handling script
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class WeatherSystemSkyScript : WeatherSystemBaseScript<WeatherSystemSkySettingsScript>
    {
        //public Mesh Mesh;

        public GameObject Day;
        public GameObject Night;
        
        public WeatherSystemStarObjectScript Star;
        //public GameObject Auroras;

        public WeatherSystemAuroraObjectScript AuroraStar;
        public WeatherSystemAuroraObjectScript Aurora;

        public WeatherSystemCelestialObjectScript Sun;
        public WeatherSystemCelestialObjectScript Moon;       


    }
}