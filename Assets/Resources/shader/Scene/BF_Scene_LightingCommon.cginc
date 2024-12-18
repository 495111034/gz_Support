#ifndef LIGHTINGCOMMON_INCLUDED
#define LIGHTINGCOMMON_INCLUDED

#include "BF_Scene_GlobalQualityVars.cginc"

#if _G_RENDERMODEL_HIGH
    #define UNIVERSAL_PBR(diffColor, specColor, oneMinusReflectivity, smoothness, worldNormal, worldViewDir, light, gi) UniversalFragmentPBR(diffColor, specColor, oneMinusReflectivity, smoothness, worldNormal, worldViewDir, light, gi)
    #define UNIVERSAL_PBR_PRECOM(surfaceData, light, gi) UniversalFragmentPBR(surfaceData, light, gi)
#elif _G_RENDERMODEL_LOW
    #define UNIVERSAL_PBR(diffColor, specColor, oneMinusReflectivity, smoothness, worldNormal, worldViewDir, light, gi) UniversalFragmentBlinnPhongLow(diffColor, specColor, smoothness, worldNormal, worldViewDir, light, gi)
    #define UNIVERSAL_PBR_PRECOM(surfaceData, light, gi) UniversalFragmentBlinnPhongLow(surfaceData, light, gi)
#else
    #define UNIVERSAL_PBR(diffColor, specColor, oneMinusReflectivity, smoothness, worldNormal, worldViewDir, light, gi) UniversalFragmentBlinnPhong(diffColor, specColor, oneMinusReflectivity, smoothness, worldNormal, worldViewDir, light, gi)
    #define UNIVERSAL_PBR_PRECOM(surfaceData, light, gi) UniversalFragmentBlinnPhong(surfaceData, light, gi)
#endif

struct SurfaceData{
    half3 diffuseColor;
    half3 specularColor;
    half oneMinusReflectivity;
    half smoothness;
    half perceptualRoughness;
    half roughness;
    half nl;
    half nv;
    float nh;
    float lh;
    float3 halfDir;
    float3 refDir;
};

inline SurfaceData GetSurfaceData(half3 albedo, half metallic, half smoothness, float3 normal, 
    float3 viewDir, float3 lightDir)
{
    SurfaceData data;
    UNITY_INITIALIZE_OUTPUT(SurfaceData, data);
// #if _G_SETTING_VERYLOW
//     data.diffuseColor = albedo;
//     data.specularColor = 0;
//     data.oneMinusReflectivity = 1;
// #else
    data.diffuseColor = DiffuseAndSpecularFromMetallic(albedo, metallic, data.specularColor, data.oneMinusReflectivity);
// #endif
    data.smoothness = smoothness;
    data.perceptualRoughness = SmoothnessToPerceptualRoughness(smoothness);
    data.roughness = PerceptualRoughnessToRoughness(data.perceptualRoughness);
    data.halfDir = Unity_SafeNormalize(lightDir + viewDir);
    data.refDir = reflect(-viewDir, normal);
    data.nl = saturate(dot(normal, lightDir));
    data.nv = saturate(dot(normal, viewDir));
    data.nh = saturate(dot(normal, data.halfDir));
    data.lh = saturate(dot(lightDir, data.halfDir));
    return data;
}

/// high renderModel
inline half4 UniversalFragmentPBR(half3 diffColor, half3 specColor, half oneMinusReflectivity, half smoothness,
    float3 normal, float3 viewDir, UnityLight light, UnityIndirect gi)
{
    float3 halfDir = Unity_SafeNormalize (float3(light.dir) + viewDir);

    half nl = saturate(dot(normal, light.dir));
    float nh = saturate(dot(normal, halfDir));
    half nv = saturate(dot(normal, viewDir));
    float lh = saturate(dot(light.dir, halfDir));

    /// directLight
    half3 directDiffuseTerm = diffColor;
    half perceptualRoughness = SmoothnessToPerceptualRoughness (smoothness);
    half roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
#if _G_BRDF_SPECULAR
    float a = roughness;
    float a2 = a*a;
    float d = nh * nh * (a2 - 1.f) + 1.00001f;

    //gama colorSpace
    float3 directSpecularTerm = a / (max(0.32f, lh) * (1.5f + roughness) * d);
    //line colorSpace
    // float directSpecularTerm = a2 / (max(0.1f, lh*lh) * (roughness + 0.5f) * (d * d) * 4);

    #if defined (SHADER_API_MOBILE)
    directSpecularTerm = clamp(directSpecularTerm - 1e-4f, 0.0, 100.0);
    #endif
    directSpecularTerm *= specColor;
#else
    half directSpecularTerm = 0.0;
#endif

    /// indirectLight
    half3 indirectDiffuseColor = gi.diffuse * diffColor;
#if _G_BRDF_REFLECT
    //gama colorSpace
    half surfaceReduction = 0.28;
    //line colorSpace
    //half surfaceReduction = (0.6-0.08*perceptualRoughness);
    surfaceReduction = 1.0 - roughness * perceptualRoughness*surfaceReduction;
    half grazingTerm = saturate(smoothness + (1-oneMinusReflectivity));
    half3 indirectSpecularColor = surfaceReduction * gi.specular * FresnelLerpFast(specColor, grazingTerm, nv);
#else
    half3 indirectSpecularColor = 0;
#endif

    return half4(indirectDiffuseColor + indirectSpecularColor + (directDiffuseTerm + directSpecularTerm) * light.color * nl, 1);
}

inline half4 UniversalFragmentPBR(SurfaceData surfaceData, UnityLight light, UnityIndirect gi)
{
    /// directLight
    half3 directDiffuseTerm = surfaceData.diffuseColor;
#if _G_BRDF_SPECULAR
    float a = surfaceData.roughness;
    float a2 = a * a;
    float d = surfaceData.nh * surfaceData.nh * (a2 - 1.f) + 1.00001f;

    //gama colorSpace
    float3 directSpecularTerm = a / (max(0.32f, surfaceData.lh) * (1.5f + surfaceData.roughness) * d);
    //line colorSpace
    // float directSpecularTerm = a2 / (max(0.1f, surfaceData.lh * surfaceData.lh) * (surfaceData.roughness + 0.5f) * (d * d) * 4);

    #if defined (SHADER_API_MOBILE)
    directSpecularTerm = clamp(directSpecularTerm - 1e-4f, 0.0, 100.0);
    #endif
    directSpecularTerm *= surfaceData.specularColor;
#else
    half directSpecularTerm = 0.0;
#endif

    /// indirectLight
    half3 indirectDiffuseColor = gi.diffuse * surfaceData.diffuseColor;
#if _G_BRDF_REFLECT
    //gama colorSpace
    half surfaceReduction = 0.28;
    //line colorSpace
    //half surfaceReduction = (0.6-0.08 * surfaceData.perceptualRoughness);
    surfaceReduction = 1.0 - surfaceData.roughness * surfaceData.perceptualRoughness * surfaceReduction;
    half grazingTerm = saturate(surfaceData.smoothness + (1 - surfaceData.oneMinusReflectivity));
    half3 indirectSpecularColor = surfaceReduction * gi.specular * FresnelLerpFast(surfaceData.specularColor, grazingTerm, surfaceData.nv);
#else
    half3 indirectSpecularColor = 0;
#endif

    return half4(indirectDiffuseColor + indirectSpecularColor + 
                (directDiffuseTerm + directSpecularTerm) * light.color * surfaceData.nl, 1);
}

///middle renderModel
//sampler2D_float unity_NHxRoughness;
inline half4 UniversalFragmentBlinnPhong(half3 diffColor, half3 specColor, half oneMinusReflectivity, half smoothness,
    float3 normal, float3 viewDir, UnityLight light, UnityIndirect gi)
{
    float3 refDir = reflect(-viewDir, normal);

    half nl = saturate(dot(normal, light.dir));
    half nv = saturate(dot(normal, viewDir));

    half2 rlPow4AndFresnelTerm = Pow4(float2(dot(refDir, light.dir), 1 - nv));
    half rlPow4 = rlPow4AndFresnelTerm.x;

    /// directLight
    half3 directDiffuseTerm = diffColor;
#if _G_BRDF_SPECULAR
    half LUT_RANGE = 16.0;
    half specular = tex2D(unity_NHxRoughness, half2(rlPow4, SmoothnessToPerceptualRoughness(smoothness))).r * LUT_RANGE;
    half3 directSpecularTerm = specular * specColor;
#else
    half3 directSpecularTerm = 0;
#endif

    /// indirectLight
    half3 indirectDiffuseColor = gi.diffuse * diffColor;
#if _G_BRDF_REFLECT
    half fresnelTerm = rlPow4AndFresnelTerm.y;
    half grazingTerm = saturate(smoothness + (1 - oneMinusReflectivity));
    half3 indirectSpecularColor = gi.specular * lerp(specColor, grazingTerm, fresnelTerm);
#else
    half3 indirectSpecularColor = 0;
#endif

    return half4(indirectDiffuseColor + indirectSpecularColor + 
                (directDiffuseTerm + directSpecularTerm) * light.color * nl, 1);
}

inline half4 UniversalFragmentBlinnPhong(SurfaceData surfaceData, UnityLight light, UnityIndirect gi)
{
    half2 rlPow4AndFresnelTerm = Pow4(float2(dot(surfaceData.refDir, light.dir), 1 - surfaceData.nv));
    half rlPow4 = rlPow4AndFresnelTerm.x;

    /// directLight
    half3 directDiffuseTerm = surfaceData.diffuseColor;
#if _G_BRDF_SPECULAR
    half LUT_RANGE = 16.0;
    half specular = tex2D(unity_NHxRoughness, half2(rlPow4, surfaceData.perceptualRoughness)).r * LUT_RANGE;
    half3 directSpecularTerm = specular * surfaceData.specularColor;
#else
    half3 directSpecularTerm = 0;
#endif

    /// indirectLight
    half3 indirectDiffuseColor = gi.diffuse * surfaceData.diffuseColor;
#if _G_BRDF_REFLECT
    half fresnelTerm = rlPow4AndFresnelTerm.y;
    half grazingTerm = saturate(surfaceData.smoothness + (1 - surfaceData.oneMinusReflectivity));
    half3 indirectSpecularColor = gi.specular * lerp(surfaceData.specularColor, grazingTerm, fresnelTerm);
#else
    half3 indirectSpecularColor = 0;
#endif

    return half4(indirectDiffuseColor + indirectSpecularColor + 
                (directDiffuseTerm + directSpecularTerm) * light.color * surfaceData.nl, 1);
}

/// low renderModel
inline half4 UniversalFragmentBlinnPhongLow(float3 diffColor, float3 specColor, half smoothness, float3 normal, float3 viewDir,UnityLight light, UnityIndirect gi)
{
    half3 ambient = gi.diffuse * diffColor;
    half3 diffuseTerm = diffColor * saturate(dot(normal, light.dir));

#if _G_BRDF_SPECULAR
    half3 halfDir = Unity_SafeNormalize(light.dir + viewDir);
    // uint glosss = smoothness * 255;
    uint glosss = exp2(10 * smoothness + 1);
    half3 specularTerm = pow(saturate(dot(normal, halfDir)) + 0.00004, glosss)  * specColor;
#else
    half3 specularTerm = 0; 
#endif

    return half4(ambient + (diffuseTerm + specularTerm) * light.color, 1);
}

inline half4 UniversalFragmentBlinnPhongLow(SurfaceData surfaceData, UnityLight light, UnityIndirect gi)
{
    half3 ambient = gi.diffuse * surfaceData.diffuseColor;
    half3 diffuseTerm = surfaceData.diffuseColor * surfaceData.nl;

#if _G_BRDF_SPECULAR
    // uint glosss = smoothness * 255;
    uint glosss = exp2(10 * surfaceData.smoothness + 1);
    half3 specularTerm = pow(surfaceData.nh + 0.00004, glosss)  * surfaceData.specularColor;
#else
    half3 specularTerm = 0; 
#endif

    return half4(ambient + (diffuseTerm + specularTerm) * light.color, 1);
}

#endif