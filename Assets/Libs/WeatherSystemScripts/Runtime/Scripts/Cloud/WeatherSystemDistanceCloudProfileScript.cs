//
// Weather System for Unity
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
    public class WeatherSystemDistanceCloudSettingScript : WeatherSystemBaseSettingScript
    {
        [Header("Cloud Rendering")]
        [Tooltip("This texture of cloud")]
        public Texture CloudMap;

        [Range(0.0f, 360.0f)]
        [Tooltip("The born Rotation if CloudSpeed is 0.")]
        public float CloudBornRotate = 0;

        [Range(-10.0f, 10.0f)]
        [Tooltip("This speed of cloud")]
        public float CloudSpeed = 1;

        [Tooltip("The ambient color of the cloud. ")]
        [ColorUsage(true, true)]
        public Color CloudAmbient = new Vector4(0.39338f, 0.63859f, 0.98113f, 0.00f);

        [Tooltip("The light color  of the cloud.")]
        [ColorUsage(true, true)]
        public Color CloudLight = new Color(2.99608f, 2.71373f, 2.22745f, 0.00f);

        [Range(0.0f, 10.0f)]
        [Tooltip("The scatter multiplier of the cloud.")]
        public float ScatterMultiplier = 1.0f;

        [Range(0.0f, 10.0f)]
        [Tooltip("The atten of the cloud.")]
        public float Attenuation = 0.56f;

        [Range(0.0f, 10.0f)]
        [Tooltip("The alpha saturation of the cloud.")]
        public float AlphaSaturation = 2.61f;

        [Range(0.0f, 1.0f)]
        [Tooltip("The step size of the cloud.")]
        [HideInInspector] public float StepSize = 0.00733f;

        [Range(0.0f, 3.0f)]
        [Tooltip("The mask of the cloud.")]
        public float Mask = 1.1f;
        
        [Range(-10.0f, 10.0f)]
        [Tooltip("The offset of the cloud.")]
        public float CloudOffset = 0f;

        public float CloudRotate = 0.0f;

        public override void UpdateMaterialProperties(WeatherSystemScript weatherSystemScript, MeshRenderer meshRenderer)
        {
            CloudRotate += Time.deltaTime * CloudSpeed % 360.0f;
         
            meshRenderer.GetPropertyBlock(WSS.MATERIAL_PROP);

            //no hdr        color property none,  hdr color  property gamma
            //no hdr global color property liner, hdr global property none
            WSS.MATERIAL_PROP.SetTexture(WSS.CLOUD_MAP, CloudMap);
            //material.SetFloat(WSS.CLOUD_SPEED, CloudSpeed);
            WSS.MATERIAL_PROP.SetColor(WSS.CLOUD_AMBIENT_COLOR, CloudAmbient.gamma);
            WSS.MATERIAL_PROP.SetColor(WSS.CLOUD_LIGHT_COLOR, CloudLight.gamma);
            WSS.MATERIAL_PROP.SetFloat(WSS.CLOUD_SCATTER, ScatterMultiplier);
            WSS.MATERIAL_PROP.SetFloat(WSS.CLOUD_ATTENUATION, Attenuation);
            WSS.MATERIAL_PROP.SetFloat(WSS.CLOUD_ALPHA_SATURATION, AlphaSaturation);
            WSS.MATERIAL_PROP.SetFloat(WSS.CLOUD_MASK, Mask);
            WSS.MATERIAL_PROP.SetFloat(WSS.CLOUD_STEP_SIZE, StepSize);
            WSS.MATERIAL_PROP.SetFloat(WSS.CLOUD_ROTATE, CloudRotate);
            WSS.MATERIAL_PROP.SetFloat(WSS.CLOUD_OFFSET, CloudOffset);

            meshRenderer.SetPropertyBlock(WSS.MATERIAL_PROP);
        }
    }

    /// <summary>
    /// Full screen fog profile, contains all full screen fog rendering configuration
    /// </summary>
    //[CreateAssetMenu(fileName = "WeatherSystemDistanceCloudProfileScript", menuName = "WeatherSystem/Distance Cloud Profile", order = 70)]
    [System.Serializable]
    public class WeatherSystemDistanceCloudProfileScript : WeatherSystemBaseProfileScript
    {
        [Tooltip ("This texture of cloud")]
        public Texture CloudMap;

        [Range(-10.0f, 10.0f)]
        [Tooltip("This Enable of cloud")]
        public WeatherSystemFloatProperty CloudEnable = new WeatherSystemFloatProperty
        (
            -1.0f,
            AnimationCurve.Linear(0.0f, 1f, 24.0f, -1f)
        );

        [Range(0f, 360f)]
        [Tooltip("This speed of cloud")]
        public WeatherSystemFloatProperty CloudBornRotate = new WeatherSystemFloatProperty
         (
             0f,
             AnimationCurve.Linear(0.0f, 0f, 24.0f, 0f)
         );

        [Range(-10.0f, 10.0f)][Tooltip ("This speed of cloud")]
        public WeatherSystemFloatProperty CloudSpeed = new WeatherSystemFloatProperty
        (
            -1.0f,
            AnimationCurve.Linear(0.0f, 1f, 24.0f, -1f)
        );

        [Tooltip("The ambient color of the cloud. ")]
        [ColorUsage(true, true)]
        public WeatherSystemColorProperty CloudAmbient = new WeatherSystemColorProperty
        (
            new Color(0.39338f, 0.63859f, 0.98113f, 0.00f),
            new Gradient()
        );
        
        [Tooltip("The light color  of the cloud.")]
        public WeatherSystemColorProperty CloudLight =  new WeatherSystemColorProperty
        (
            new Color(2.99608f, 2.71373f, 2.22745f, 0.00f),
            new Gradient()
        );
  
        [Range(0.0f, 10.0f)][Tooltip("The scatter multiplier of the cloud.")]
        public WeatherSystemFloatProperty ScatterMultiplier = new WeatherSystemFloatProperty
        (
            1.0f,
            AnimationCurve.Linear(0.0f, 1.0f, 24.0f, 1.0f)
        );
          
        [Range(0.0f, 10.0f)][Tooltip("The atten of the cloud.")]
        public WeatherSystemFloatProperty Attenuation = new WeatherSystemFloatProperty
        (
            0.56f,
            AnimationCurve.Linear(0.0f, 0.56f, 24.0f, 0.56f)
        );
        
        [Range(0.0f, 10.0f)][Tooltip("The alpha saturation of the cloud.")]
        public WeatherSystemFloatProperty AlphaSaturation = new WeatherSystemFloatProperty
        (
            2.61f,
            AnimationCurve.Linear(0.0f, 1f, 24.0f, 1f)
        );

        [Range(0.0f, 1.0f)][Tooltip("The step size of the cloud.")]
        [HideInInspector]public float StepSize = 0.00733f;
        
        [Range(0.0f, 3.0f)][Tooltip("The mask of the cloud.")]
        public WeatherSystemFloatProperty Mask = new WeatherSystemFloatProperty
        (
            1.1f,
            AnimationCurve.Linear(0.0f, 1.1f, 24.0f, 1.1f)
        );
        
        [Range(-10.0f, 10.0f)][Tooltip("The cloudOffset of the cloud.")]
        public WeatherSystemFloatProperty CloudOffset = new WeatherSystemFloatProperty
        (
            0f,
            AnimationCurve.Linear(0.0f, -10f, 24.0f, 10f)
        );
        
    

    }
}
