using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
[System.Serializable]
public class AnimationEditorWidget
{
    [Range(0, 1)]
    public float process;
    public AnimationClip animation;
}
#endif

[System.Serializable] public struct CombinedLightingProperties
{
    //skybox 
    [Header("天空盒")]
    public Color m_SkyColor001;
    public Color m_SkyColor002;
    public Color m_SkyColor003;
    [Range(1, 2)] public float m_SkyColor1;
    [Range(-1, 1.5f)] public float m_SkyColor2;
    [Range(0, 200)] public float m_Cloud01Size;
    public Color m_Cloud01Color;
    public Color m_Cloud01Reflector;
    [Range(0, 100)] public float m_Cloud02Size;
    public Color m_Cloud02Color;
    [Range(0, 2)] public float m_SunRadius;
    [Range(0, 1)] public float m_SunBloom;
    [Range(-1, 1)] public float m_PhiHorizontal;
    [Range(-1, 1)] public float m_ThetaVertical;
    //public Color m_SunColor_Skybox;
    [Range(0.2f, 2)] public float m_BloomRadius;
    public Color m_BloomColor;
    [Range(-2, 5)] public float m_Cloud03Size;
    public Color m_Cloud03Color;
    public Color m_Fog;
    [Range(0, 1)] public float m_FogDensity;
    [Range(0, 30)] public float m_MoonRadius;
    [Range(0, 1)] public float m_Bloom;
    [Range(0, 1)] public float m_CloudHorizontal;
    [Range(-1000, 1000)] public float m_CloudHorizontal02;

    //indoor
    [Header("灯强弱")]
    [Range(0, 10)] public float m_LightHeight;      // 0-5
    [Range(0, 10)] public float m_LightHeight2;     // 0-5
    [Header("灯颜色")]
    public Color m_LightColor;
    public Color m_LightColor2;
    [Header("光照")]
    public Color m_GIColor;
    public Color m_SunColor_Indoor;
    public Color m_SkyLightColor;
    [Range(-1, 1)] public float m_Alpha;          //透明度
}


public class CombinedLightingMgr : MonoBehaviour
{

    public HashSet<Material> m_GatheredMaterials;

    public Material m_SkyboxMaterial;

    public CombinedLightingProperties m_AnimProperties;

    public GameObject root;
#if UNITY_EDITOR
    public AnimationEditorWidget[] animationClips;
#endif

    #region shader 参数
    //skybox
    [System.NonSerialized] [HideInInspector] public int m_SkyColor001Param;
    [System.NonSerialized] [HideInInspector] public int m_SkyColor002Param;
    [System.NonSerialized] [HideInInspector] public int m_SkyColor003Param;
    [System.NonSerialized] [HideInInspector] public int m_SkyColor1Param;
    [System.NonSerialized] [HideInInspector] public int m_SkyColor2Param;
    [System.NonSerialized] [HideInInspector] public int m_Cloud01SizeParam;
    [System.NonSerialized] [HideInInspector] public int m_Cloud01ColorParam;
    [System.NonSerialized] [HideInInspector] public int m_Cloud01ReflectorParam;
    [System.NonSerialized] [HideInInspector] public int m_Cloud02SizeParam;
    [System.NonSerialized] [HideInInspector] public int m_Cloud02ColorParam;
    [System.NonSerialized] [HideInInspector] public int m_SunRadiusParam;
    [System.NonSerialized] [HideInInspector] public int m_SunBloomParam;
    [System.NonSerialized] [HideInInspector] public int m_PhiHorizontalParam;
    [System.NonSerialized] [HideInInspector] public int m_ThetaVerticalParam;
    [System.NonSerialized] [HideInInspector] public int m_SunColorParam_Skybox;
    [System.NonSerialized] [HideInInspector] public int m_BloomRadiusParam;
    [System.NonSerialized] [HideInInspector] public int m_BloomColorParam;
    [System.NonSerialized] [HideInInspector] public int m_Cloud03SizeParam;
    [System.NonSerialized] [HideInInspector] public int m_Cloud03ColorParam;
    [System.NonSerialized] [HideInInspector] public int m_FogParam;
    [System.NonSerialized] [HideInInspector] public int m_FogDensityParam;
    [System.NonSerialized] [HideInInspector] public int m_MoonRadiusParam;
    [System.NonSerialized] [HideInInspector] public int m_BloomParam;
    [System.NonSerialized] [HideInInspector] public int m_CloudHorizontalParam;
    [System.NonSerialized] [HideInInspector] public int m_CloudHorizontal02Param;


    //indoor
    [System.NonSerialized] [HideInInspector] public int m_LightColorParam;
    [System.NonSerialized] [HideInInspector] public int m_LightHeightParam;
    [System.NonSerialized] [HideInInspector] public int m_LightColor2Param;
    [System.NonSerialized] [HideInInspector] public int m_LightHeight2Param;
    [System.NonSerialized] [HideInInspector] public int m_GIColorParam;
    [System.NonSerialized] [HideInInspector] public int m_SunColorParam_Indoor;
    [System.NonSerialized] [HideInInspector] public int m_SkyLightColorParam;
    [System.NonSerialized] [HideInInspector] public int m_AlphaParam;

    #endregion



    #region 参数初始化
    public void SetRoot(GameObject gameObject)
    {
        root = gameObject;
        ScanMaterials();
    }


    public void Start()
    {
        Init();
    }

    public void Init()
    {
        if (m_GatheredMaterials == null)
            m_GatheredMaterials = new  HashSet<Material>();
        if(GetComponent<Animation>() != null) GetComponent<Animation>().playAutomatically = false;


        //skybox
        m_SkyColor001Param = Shader.PropertyToID("_SkyColor001");
        m_SkyColor002Param = Shader.PropertyToID("_SkyColor002");
        m_SkyColor003Param = Shader.PropertyToID("_SkyColor003");
        m_SkyColor1Param = Shader.PropertyToID("_SkyColor1");
        m_SkyColor2Param = Shader.PropertyToID("_SkyColor2");
        m_Cloud01SizeParam = Shader.PropertyToID("_Cloud01Size");
        m_Cloud01ColorParam = Shader.PropertyToID("_Cloud01Color");
        m_Cloud01ReflectorParam = Shader.PropertyToID("_Cloud01Reflector");
        m_Cloud02SizeParam = Shader.PropertyToID("_Cloud02Size");
        m_Cloud02ColorParam = Shader.PropertyToID("_Cloud02Color");
        m_SunRadiusParam = Shader.PropertyToID("_SunRadius");
        m_SunBloomParam = Shader.PropertyToID("_SunBloom");
        m_PhiHorizontalParam = Shader.PropertyToID("_PhiHorizontal");
        m_ThetaVerticalParam = Shader.PropertyToID("_ThetaVertical");
        m_SunColorParam_Skybox = Shader.PropertyToID("_SunColor");
        m_BloomRadiusParam = Shader.PropertyToID("_BloomRadius");
        m_BloomColorParam = Shader.PropertyToID("_BloomColor");
        m_Cloud03SizeParam = Shader.PropertyToID("_Cloud03Size");
        m_Cloud03ColorParam = Shader.PropertyToID("_Cloud03Color");
        m_FogParam = Shader.PropertyToID("_Fog");
        m_FogDensityParam = Shader.PropertyToID("_FogDensity");
        m_MoonRadiusParam = Shader.PropertyToID("_MoonRadius");
        m_BloomParam = Shader.PropertyToID("_Bloom");
        m_CloudHorizontalParam = Shader.PropertyToID("_CloudHorizontal");
        m_CloudHorizontal02Param = Shader.PropertyToID("_CloudHorizontal02");


        //indoor
        m_LightColorParam = Shader.PropertyToID("_LightColor");
        m_LightHeightParam = Shader.PropertyToID("_LightHeight");
        m_LightColor2Param = Shader.PropertyToID("_LightColor2");
        m_LightHeight2Param = Shader.PropertyToID("_LightHeight2");
        m_GIColorParam = Shader.PropertyToID("_GIColor");
        m_SunColorParam_Indoor = Shader.PropertyToID("_SunColor");
        m_SkyLightColorParam = Shader.PropertyToID("_SkyLightColor");
        m_AlphaParam = Shader.PropertyToID("_ALPHA");

    }

    #endregion

    #region 帧更新
    public void Update() {
        ApplyAllProperties();
    }

    public void ApplyAllProperties()
    {

        UpdateSkyBox(m_SkyboxMaterial);
        if(m_GatheredMaterials != null)
        foreach (Material m in m_GatheredMaterials)
        {
            UpdateMaterial(m);
        }
    }
    #endregion

    #region 编辑器方法
    // Called by editor script
    public void ClearAll()
    {
        m_GatheredMaterials = new  HashSet<Material>();
        m_SkyboxMaterial = null;
    }

    // Called by editor script
    public void ScanMaterials()
    {
        m_GatheredMaterials = new  HashSet<Material>();
        if (root == null) return;
        //Renderer[] renderers = root.GetComponentsInChildren<Renderer>();
        //for (int j = 0; j < renderers.Length; ++j)
        //{
        //    Renderer r = renderers[j];

        //    if (r == null) continue;

        //    for (int k = 0; k < r.sharedMaterials.Length; ++k)
        //    {
        //        if (r.sharedMaterials[k] != null)
        //            m_GatheredMaterials.Add(r.sharedMaterials[k]);
        //    }
        //}
        m_SkyboxMaterial = RenderSettings.skybox;
        m_AnimProperties.m_Alpha = -1;
    }
    #endregion


    #region 参数更新
    public void SetMaterialProperty(Material m, int paramHash, Color value) { m.SetColor(paramHash, value); }
    public void SetMaterialProperty(Material m, int paramHash, float value) { m.SetFloat(paramHash, value); }


    private void UpdateSkyBox(Material m1)
    {
        if (m1 == null) return;
        //skybox
        SetMaterialProperty(m1, m_SkyColor001Param, m_AnimProperties.m_SkyColor001);
        SetMaterialProperty(m1, m_SkyColor002Param, m_AnimProperties.m_SkyColor002);
        SetMaterialProperty(m1, m_SkyColor003Param, m_AnimProperties.m_SkyColor003);
        SetMaterialProperty(m1, m_SkyColor1Param, m_AnimProperties.m_SkyColor1);
        SetMaterialProperty(m1, m_SkyColor2Param, m_AnimProperties.m_SkyColor2);
        SetMaterialProperty(m1, m_Cloud01SizeParam, m_AnimProperties.m_Cloud01Size);
        SetMaterialProperty(m1, m_Cloud01ColorParam, m_AnimProperties.m_Cloud01Color);
        SetMaterialProperty(m1, m_Cloud01ReflectorParam, m_AnimProperties.m_Cloud01Reflector);
        SetMaterialProperty(m1, m_Cloud02SizeParam, m_AnimProperties.m_Cloud02Size);
        SetMaterialProperty(m1, m_Cloud02ColorParam, m_AnimProperties.m_Cloud02Color);
        SetMaterialProperty(m1, m_SunRadiusParam, m_AnimProperties.m_SunRadius);
        SetMaterialProperty(m1, m_SunBloomParam, m_AnimProperties.m_SunBloom);
        SetMaterialProperty(m1, m_PhiHorizontalParam, m_AnimProperties.m_PhiHorizontal);
        SetMaterialProperty(m1, m_ThetaVerticalParam, m_AnimProperties.m_ThetaVertical);
        SetMaterialProperty(m1, m_SunColorParam_Skybox, m_AnimProperties.m_SunColor_Indoor);
        SetMaterialProperty(m1, m_BloomRadiusParam, m_AnimProperties.m_BloomRadius);
        SetMaterialProperty(m1, m_BloomColorParam, m_AnimProperties.m_BloomColor);
        SetMaterialProperty(m1, m_Cloud03SizeParam, m_AnimProperties.m_Cloud03Size);
        SetMaterialProperty(m1, m_Cloud03ColorParam, m_AnimProperties.m_Cloud03Color);
        SetMaterialProperty(m1, m_FogParam, m_AnimProperties.m_Fog);
        SetMaterialProperty(m1, m_FogDensityParam, m_AnimProperties.m_FogDensity);
        SetMaterialProperty(m1, m_MoonRadiusParam, m_AnimProperties.m_MoonRadius);
        SetMaterialProperty(m1, m_BloomParam, m_AnimProperties.m_Bloom);
        SetMaterialProperty(m1, m_CloudHorizontalParam, m_AnimProperties.m_CloudHorizontal);
        SetMaterialProperty(m1, m_CloudHorizontal02Param, m_AnimProperties.m_CloudHorizontal02);
    }

    private void UpdateMaterial(Material m2)
    {
        if (m2 == null) return;
        //indoor
        m2.SetColor(m_LightColorParam, m_AnimProperties.m_LightColor);
        m2.SetFloat(m_LightHeightParam, m_AnimProperties.m_LightHeight);
        m2.SetColor(m_LightColor2Param, m_AnimProperties.m_LightColor2);
        m2.SetFloat(m_LightHeight2Param, m_AnimProperties.m_LightHeight2);
        m2.SetColor(m_GIColorParam, m_AnimProperties.m_GIColor);
        m2.SetColor(m_SunColorParam_Indoor, m_AnimProperties.m_SunColor_Indoor);
        m2.SetColor(m_SkyLightColorParam, m_AnimProperties.m_SkyLightColor);
        m2.SetFloat(m_AlphaParam, m_AnimProperties.m_Alpha);

    }
    #endregion
}
