using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public  class FogManager
{
    private static FogManager m_Instance;

    public static FogManager Instance
    {
        get
        {
            if (m_Instance == null)
                m_Instance = new FogManager();
            return m_Instance;
        }
    }
    
    private FogManager()
    {

    }

    private FogConfig m_FogConfig;
    private FogType m_FogType;

    private void EnableHeightFog()
    {
        Shader.EnableKeyword("FOG_HEIGHT");
        CacheShaderConstant();
    }

    private void DisableHeightFog()
    {
        Shader.DisableKeyword("FOG_HEIGHT");
    }
    public enum FogType
    {
        None,
        FogLinear,
        FogExp,
        FogExp2,
        FogHeight
    }

    public struct FogConfig
    {
        public BuiltInFogConfig builtInFogConfig;
        public HeightFogConfig heightFogConfig;
    }

    public struct BuiltInFogConfig
    {
        public Vector4 builtInFogColor;
        public Vector4 builtInFogParams;               // x = fogDistanceStart, y = fogDistanceEnd
    }
    public struct HeightFogConfig
    {
        public Vector4 heightFogParams;                // x = fogDistanceStart, y = fogDistanceEnd, z = fogHeightStart, w = fogHeightEnd;
        public Vector4 fogIntensityParams;             // x = _HeightFogIntensity, y = directLightIntensity, z = boolHeightBasedInCameraPos;
        public Vector4 heightFogColor;
        public Vector4 fogFalloffParams;               // x = distanceFalloff, y = heightFalloff, z = directLightFalloff;
        public Vector4 fogNoiseParams;                 // x = noiseScale, y = noiseDistanceEnd, z = noiseIntensity;
        public Vector4 fogNoiseSpeed;
    }

    int HeightFogParamsID, FogIntensityParamsID, HeightFog_ColorStartID, HeightFog_ColorEndID, FogFalloffParamsID, FogNoiseParamsID, FogNoiseSpeedID;

    private void CacheShaderConstant()
    {
        HeightFogParamsID = Shader.PropertyToID("_HeightFogParams");
        FogIntensityParamsID = Shader.PropertyToID("_FogIntensityParams");
        HeightFog_ColorStartID = Shader.PropertyToID("_HeightFog_ColorStart");
        //HeightFog_ColorEnd = Shader.PropertyToID("_HeightFog_ColorEnd");
        FogFalloffParamsID = Shader.PropertyToID("_FogFalloffParams");
        FogNoiseParamsID = Shader.PropertyToID("_FogNoiseParams");
        FogNoiseSpeedID = Shader.PropertyToID("_FogNoiseSpeed");
    }

    public void SwitchFog(FogType fogType)
    {
        m_FogType = fogType;
        switch (fogType)
        {
            case FogType.None:
                {
                    RenderSettings.fog = false;
                    DisableHeightFog();
                    break;
                }
            case FogType.FogLinear:
                {
                    RenderSettings.fog = true;
                    RenderSettings.fogMode = FogMode.Linear;
                    DisableHeightFog();
                    break;
                }
            case FogType.FogExp:
                {
                    RenderSettings.fog = true;
                    RenderSettings.fogMode = FogMode.Exponential;
                    DisableHeightFog();
                    break;
                }
            case FogType.FogExp2:
                {
                    RenderSettings.fog = true;
                    RenderSettings.fogMode = FogMode.ExponentialSquared;
                    DisableHeightFog();
                    break;
                }
            case FogType.FogHeight:
                {
                    RenderSettings.fog = false;
                    EnableHeightFog();
                    break;
                }
        }
        RefreshFogProperty();
    }

    public void SwitchFog(FogType fogType, FogConfig config)
    {
        m_FogConfig = config;
        SwitchFog(fogType);
    }

    public void RefreshFogProperty(FogConfig fogConfig)
    {
        m_FogConfig = fogConfig;
        RefreshFogProperty();
    }

    private void RefreshFogProperty()
    {
        if(m_FogType == FogType.FogHeight)
        {
            RefreshHeightPropertyBuffer();
        }
        else if(m_FogType != FogType.None)
        {
            RefreshBuiltInFogProperty();
        }
    }

    private void RefreshBuiltInFogProperty()
    {
        RenderSettings.fogColor = m_FogConfig.builtInFogConfig.builtInFogColor;
        RenderSettings.fogStartDistance = m_FogConfig.builtInFogConfig.builtInFogParams.x;
        RenderSettings.fogEndDistance = m_FogConfig.builtInFogConfig.builtInFogParams.y;
    }

    private void RefreshHeightPropertyBuffer()
    {
        if(m_FogType != FogType.FogHeight)
        {
#if UNITY_EDITOR
            Debug.Log("HeightFogIsDisabled");
#endif
            return;
        }
        Shader.SetGlobalVector(HeightFogParamsID, m_FogConfig.heightFogConfig.heightFogParams);
        Shader.SetGlobalVector(FogIntensityParamsID, m_FogConfig.heightFogConfig.fogIntensityParams);
        Shader.SetGlobalVector(HeightFog_ColorStartID, m_FogConfig.heightFogConfig.heightFogColor);
        //Shader.SetGlobalColor(HeightFog_ColorEnd, fogColorEnd);
        Shader.SetGlobalVector(FogFalloffParamsID, m_FogConfig.heightFogConfig.fogFalloffParams);
        Shader.SetGlobalVector(FogNoiseParamsID, m_FogConfig.heightFogConfig.fogNoiseParams);
        Shader.SetGlobalVector(FogNoiseSpeedID, m_FogConfig.heightFogConfig.fogNoiseSpeed);
    }
}
