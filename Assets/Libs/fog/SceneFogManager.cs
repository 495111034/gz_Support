using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class SceneFogManager : MonoBehaviour
{
    public FogManager.FogType fogType = FogManager.FogType.FogHeight;

    /// <summary>
    /// HeightFog
    /// </summary> 

    public float fogDistanceStart = -10;

    public float fogDistanceEnd = 100;

    public bool fogHeightBasedOnCameraPos = false;

    [Range(1f, 8f)]
    public float fogDistanceFalloff = 1;

    public float fogHeightStart = 0;

    public float fogHeightEnd = 10;

    [Range(1f, 8f)]
    public float fogHeightFalloff = 1;

    [Range(0f, 1f)]
    public float fogIntensity = 1;

    [ColorUsage(false, true)]
    public Color heightFogColor = Color.white;

    //public Color heightFogColorEnd = Color.white;

    [Range(1f, 8f)]
    public float lightFalloff = 1;

    [Range(0f, 1f)]
    public float lightIntensity = 1;

    [Range(0f, 1f)]
    public float noiseIntensity = 1;

    public float noiseDistanceEnd = 50;

    public float noiseScale = 10;

    public Vector3 noiseSpeed = new Vector3(0f, 0f, 0f);


    /// <summary>
    /// BuiltInFog
    /// </summary>
    public Color builtInFogColor = Color.white;

    public float builtInFogDistanceStart = 0;

    public float builtInFogDistanceEnd = 100;

    private void OnEnable()
    {
        InitializeFog();
    }
    public void SwitchFog()
    {
        FogManager.Instance.SwitchFog(fogType);
    }
    public void RefreshFogProperty()
    {
        FogManager.FogConfig fogConfig;
        PackFogProperty(out fogConfig);
        FogManager.Instance.RefreshFogProperty(fogConfig);
    }

    public void InitializeFog()
    {
        FogManager.FogConfig fogConfig;
        PackFogProperty(out fogConfig);
        FogManager.Instance.SwitchFog(fogType, fogConfig);
    }

    public void PackFogProperty(out FogManager.FogConfig fogConfig)
    {
        fogConfig.heightFogConfig.heightFogParams = new Vector4() { };
        fogConfig.heightFogConfig.heightFogParams = new Vector4(fogDistanceStart, fogDistanceEnd, fogHeightStart, fogHeightEnd);
        fogConfig.heightFogConfig.fogIntensityParams = new Vector4(fogIntensity, lightIntensity, (fogHeightBasedOnCameraPos ? 1 : 0), 0);
        fogConfig.heightFogConfig.heightFogColor = heightFogColor;
        //Color fogColorEnd = heightFogColorEnd;
        fogConfig.heightFogConfig.fogFalloffParams = new Vector4(fogDistanceFalloff, fogHeightFalloff, lightFalloff, 0);
        fogConfig.heightFogConfig.fogNoiseParams = new Vector4(noiseScale, noiseDistanceEnd, noiseIntensity, 0);
        fogConfig.heightFogConfig.fogNoiseSpeed = noiseSpeed;

        fogConfig.builtInFogConfig.builtInFogColor = builtInFogColor;
        fogConfig.builtInFogConfig.builtInFogParams = new Vector4(builtInFogDistanceStart, builtInFogDistanceEnd, 0, 0);
    }

    private void OnDisable()
    {
        FogManager.Instance.SwitchFog(FogManager.FogType.None);
    }
}
