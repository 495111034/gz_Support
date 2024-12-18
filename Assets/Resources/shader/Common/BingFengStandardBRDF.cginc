/*
* Copyright (C) 2016, BingFeng Studio（冰峰工作室）.
* All rights reserved.
* 
* 文件名称：BingFengStandardBRDF
* 创建标识：引擎组
* 创建日期：2020/7/8
* 文件简述：
*/

//UnityStandardBRDF.cginc

#ifndef BF_STANDARD_BRDF_INCLUDED
#define BF_STANDARD_BRDF_INCLUDED

#include "UnityCG.cginc"
#include "UnityStandardConfig.cginc"
#include "UnityLightingCommon.cginc"

inline half3 BF_DiffuseAndSpecularFromMetallic (half3 albedo, half metallic, out half3 specColor, out half oneMinusReflectivity)
{
    specColor = lerp (unity_ColorSpaceDielectricSpec.rgb, albedo, metallic);
    oneMinusReflectivity = OneMinusReflectivityFromMetallic(metallic);
    return albedo * oneMinusReflectivity;
}

//-------------------------------------------------------------------------------------
// Default BRDF to use:
/* differentizate use in shader
#if !defined (BF_BRDF_PBS) // allow to explicitly override BRDF in custom shader
    // still add safe net for low shader models, otherwise we might end up with shaders failing to compile
    #if SHADER_TARGET < 30 || defined(SHADER_TARGET_SURFACE_ANALYSIS) // only need "something" for surface shader analysis pass; pick the cheap one
        #define BF_BRDF_PBS BRDF3_Unity_PBS
    #elif defined(UNITY_PBS_USE_BRDF3)
        #define BF_BRDF_PBS BRDF3_BF_PBS
    #elif defined(UNITY_PBS_USE_BRDF2)
        #define BF_BRDF_PBS BRDF2_BF_PBS
    #elif defined(UNITY_PBS_USE_BRDF1)
        #define BF_BRDF_PBS BRDF1_BF_PBS
    #else
        #error something broke in auto-choosing BRDF
    #endif
#endif
*/

//UnityStandardBRDF - BRDF1_Unity_PBS
//cost lot in mobile
half4 BRDF1_BF_PBS (half3 diffColor, half3 specColor, half oneMinusReflectivity, half smoothness, float3 normal, float3 viewDir, UnityLight light, UnityIndirect gi)
{
    float perceptualRoughness = SmoothnessToPerceptualRoughness (smoothness);
    float3 halfDir = Unity_SafeNormalize (float3(light.dir) + viewDir);

	half nv = abs(dot(normal, viewDir));    // This abs allow to limit artifact
    float nl = saturate(dot(normal, light.dir));
    float nh = saturate(dot(normal, halfDir));
    half lh = saturate(dot(light.dir, halfDir));

    // Diffuse
    half diffuseTerm = DisneyDiffuse(nv, nl, lh, perceptualRoughness) * nl;
	half3 directDiffuse = diffColor * light.color * diffuseTerm;
	half3 indirectDiffuse = diffColor * gi.diffuse;

    // Specular
    float roughness = max(PerceptualRoughnessToRoughness(perceptualRoughness), 0.002);
	float V = SmithJointGGXVisibilityTerm (nl, nv, roughness);
	float D = GGXTerm (nh, roughness);
    float specularTerm = max(0, sqrt(max(1e-4h, V*D * UNITY_PI)) * nl);
	half3 directSpecular = specularTerm * light.color * FresnelTerm (specColor, lh);
	half surfaceReduction = 1.0-0.28*roughness*perceptualRoughness;      // 1-0.28*x^3 as approximation for (1/(x^4+1))^(1/2.2) on the domain [0;1]
    half grazingTerm = saturate(smoothness + (1-oneMinusReflectivity));
	half3 indirectSpecular = surfaceReduction * gi.specular * FresnelLerp (specColor, grazingTerm, nv);

    half3 color = directDiffuse + directSpecular + indirectDiffuse + indirectSpecular;

    return half4(color, 1);
}


//UnityStandardBRDF - BRDF2_BF_PBS
//optimize for mobile
half4 BRDF2_BF_PBS (half3 diffColor, half3 specColor, half oneMinusReflectivity, half smoothness, float3 normal, float3 viewDir, UnityLight light, UnityIndirect gi)
{
    float3 halfDir = Unity_SafeNormalize (float3(light.dir) + viewDir);

    half nl = saturate(dot(normal, light.dir));
    float nh = saturate(dot(normal, halfDir));
    half nv = saturate(dot(normal, viewDir));
    float lh = saturate(dot(light.dir, halfDir));

	// Diffuse
	half3 directDiffuse = diffColor * light.color * nl;
	half3 indirectDiffuse = diffColor * gi.diffuse;

    // Specular
    half perceptualRoughness = SmoothnessToPerceptualRoughness (smoothness);
    half roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
    half a = roughness;
    float a2 = a*a;
    float d = nh * nh * (a2 - 1.f) + 1.00001f;
    float specularTerm = a / (max(0.32f, lh) * (1.5f + roughness) * d);
    #if defined (SHADER_API_MOBILE)
        specularTerm = clamp(specularTerm - 1e-4f, 0.0, 100.0);
    #endif
	half3 directSpecular = specularTerm * light.color * specColor * nl;

    half surfaceReduction = 1.0 - roughness*perceptualRoughness*0.28;
    half grazingTerm = saturate(smoothness + (1-oneMinusReflectivity));
	half3 indirectSpecular = surfaceReduction * gi.specular * FresnelLerpFast (specColor, grazingTerm, nv);

    half3 color = directDiffuse + directSpecular + indirectDiffuse + indirectSpecular;

    return half4(color, 1);
}


//sampler2D_float unity_NHxRoughness;
half3 BRDF3_BF_Direct(half3 diffColor, half3 specColor, half rlPow4, half smoothness)
{
    half LUT_RANGE = 16.0; // must match range in NHxRoughness() function in GeneratedTextures.cpp
    // Lookup texture to save instructions
    half specular = tex2D(unity_NHxRoughness, half2(rlPow4, SmoothnessToPerceptualRoughness(smoothness))).r * LUT_RANGE;
    #if defined(_SPECULARHIGHLIGHTS_OFF)
        specular = 0.0;
    #endif

    return diffColor + specular * specColor;
}

half3 BRDF3_BF_Indirect(half3 diffColor, half3 specColor, UnityIndirect indirect, half grazingTerm, half fresnelTerm)
{
    half3 c = indirect.diffuse * diffColor;
    c += indirect.specular * lerp (specColor, grazingTerm, fresnelTerm);
    return c;
}

//UnityStandardBRDF - BRDF3_BF_PBS
//optimize for mobile
half4 BRDF3_BF_PBS (half3 diffColor, half3 specColor, half oneMinusReflectivity, half smoothness,
    float3 normal, float3 viewDir,
    UnityLight light, UnityIndirect gi)
{
    float3 reflDir = reflect (viewDir, normal);

    half nl = saturate(dot(normal, light.dir));
    half nv = saturate(dot(normal, viewDir));

    // Vectorize Pow4 to save instructions
    half2 rlPow4AndFresnelTerm = Pow4 (float2(dot(reflDir, light.dir), 1-nv));  // use R.L instead of N.H to save couple of instructions
    half rlPow4 = rlPow4AndFresnelTerm.x; // power exponent must match kHorizontalWarpExp in NHxRoughness() function in GeneratedTextures.cpp
    half fresnelTerm = rlPow4AndFresnelTerm.y;

    half grazingTerm = saturate(smoothness + (1-oneMinusReflectivity));

    half3 color = BRDF3_BF_Direct(diffColor, specColor, rlPow4, smoothness);
    color *= light.color * nl;
    color += BRDF3_BF_Indirect(diffColor, specColor, gi, grazingTerm, fresnelTerm);

    return half4(color, 1);
}



#endif // BF_STANDARD_BRDF_INCLUDED