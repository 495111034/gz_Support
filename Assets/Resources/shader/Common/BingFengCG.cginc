
#ifndef BF_CG
#define BF_CG
#include "../Scene/BF_Scene_GlobalQualityVars.cginc"

//Fog Begin
#if _G_USE_FOG
#define USING_FOG (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2) || defined(FOG_HEIGHT))
#endif
// HeightFog
#if defined(FOG_HEIGHT)
    #include "UnityCG.cginc"
    #undef FOG_LINEAR
    #undef FOG_EXP
    #undef FOG_EXP2
    #undef UNITY_FOG_COORDS
    #undef UNITY_TRANSFER_FOG
    #undef UNITY_APPLY_FOG
#endif

#if defined(FOG_HEIGHT)
    float4 _HeightFogParams ;   // x = fogDistanceStart, y = fogDistanceEnd, z = fogHeightStart, w = fogHeightEnd;
    float4 _FogIntensityParams;       // x = _HeightFogIntensity, y = directLightIntensity, z = boolHeightCameraSpace;
    half4 _HeightFog_ColorStart;
    //half4 _HeightFog_ColorEnd;
    half4 _FogFalloffParams ;  // x = distanceFalloff, y = heightFalloff, z = directLightFalloff;
    float4 _FogNoiseParams ;    // x = noiseScale, y = noiseDistanceEnd, z = noiseIntensity;
    float3 _FogNoiseSpeed ;
    // #define _G_HEIGHTFOG_FALLOFF 1
    // #define _G_HEIGHTFOG_DIRECTLIGHT 1
    // #define _G_HEIGHTFOG_NOISE3D 1

// #if (SHADER_TARGET < 30) || defined(SHADER_API_MOBILE)
    #define UNITY_FOG_COORDS(idx) float4 fogCoord : TEXCOORD##idx; //xyz = worldPos, w = coordFactory;
    #define UNITY_TRANSFER_FOG(o, worldPos) half fogFactory = ComputerFogFactory(worldPos); o.fogCoord.xyz = worldPos; o.fogCoord.w = fogFactory; 
    #define UNITY_APPLY_FOG(coord, col) col.rgb = lerp(col.rgb, GetFogColor(coord.xyz), coord.w);
// #else
//     #define UNITY_FOG_COORDS(idx) float4 fogCoord : TEXCOORD##idx; //xyz = worldPos, w = coordFactory;
//     #define UNITY_TRANSFER_FOG(o, worldPos) o.fogCoord.xyz = worldPos;
//     #define UNITY_APPLY_FOG(coord, col) half fogFactory = ComputerFogFactory(coord.xyz); col.rgb = lerp(col.rgb, GetFogColor(coord.xyz), fogFactory);
// #endif

    float4 mod289( float4 x )
    {
        return x - floor(x * (1.0 / 289.0)) * 289.0;
    }
    
    float4 perm( float4 x )
    {
        return mod289(((x * 34.0) + 1.0) * x);
    }

    float SimpleNoise3D( float3 p )
    {
        float3 a = floor(p);
        float3 d = p - a;
        d = d * d * (3.0 - 2.0 * d);
        float4 b = a.xxyy + float4(0.0, 1.0, 0.0, 1.0);
        float4 k1 = perm(b.xyxy);
        float4 k2 = perm(k1.xyxy + b.zzww);
        float4 c = k2 + a.zzzz;
        float4 k3 = perm(c);
        float4 k4 = perm(c + 1.0);
        float4 o1 = frac(k3 * (1.0 / 41.0));
        float4 o2 = frac(k4 * (1.0 / 41.0));
        float4 o3 = o2 * d.z + o1 * (1.0 - d.z);
        float2 o4 = o3.yw * d.x + o3.xz * (1.0 - d.x);
        return o4.y * d.y + o4.x * (1.0 - d.y);
    }

    half ComputerFogFactory(float3 worldPos)
    {
        float4 fogParams = _HeightFogParams;

        half fogDistanceFactory = saturate((distance(worldPos.xz, _WorldSpaceCameraPos.xz) - fogParams.x) / (fogParams.y - fogParams.x));
        half fogHeightFactory = saturate((lerp(worldPos.y, worldPos.y - _WorldSpaceCameraPos.y, _FogIntensityParams.z) - fogParams.w) / (fogParams.z - fogParams.w));

    #if _G_HEIGHTFOG_FALLOFF
        half2 fogFalloffParams = _FogFalloffParams.xy;
        fogDistanceFactory = pow(fogDistanceFactory, fogFalloffParams.x);
        fogHeightFactory = pow(fogHeightFactory, fogFalloffParams.y);
    #endif

        half finalFogFactory = fogDistanceFactory * fogHeightFactory;

    #if _G_HEIGHTFOG_NOISE3D
        half3 noiseParams = _FogNoiseParams.xyz;
        float timeFactory =  _Time.y * 2.0;
        float3 noisedWorldPos = (worldPos * (1.0 / noiseParams.x)) - (_FogNoiseSpeed * timeFactory);
        float simpleNoise = SimpleNoise3D(noisedWorldPos);
        half noiseDistanceMask = saturate(((distance(worldPos, _WorldSpaceCameraPos) - noiseParams.y) / (0.0 - noiseParams.y)));
        half noiseFactory = lerp(1.0, (simpleNoise * 0.5 + 0.5), noiseParams.z * noiseDistanceMask);
        finalFogFactory *= noiseFactory;
    #endif

        finalFogFactory *= _FogIntensityParams.x;
        return saturate(finalFogFactory);
    }

    half3 GetFogColor(float3 worldPos)
    {
        half3 finalColor = _HeightFog_ColorStart.rgb;
    #if _G_HEIGHTFOG_DIRECTLIGHT
        float3 viewDir = normalize(UnityWorldSpaceViewDir(worldPos));
        float3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
        half directLightIntensity = _FogIntensityParams.y;
        half vlFactory = (dot(viewDir, lightDir) * 0.5 + 0.5) * directLightIntensity;
    #if _G_HEIGHTFOG_FALLOFF
        half directLightFalloff = _FogFalloffParams.z;
        vlFactory = pow(vlFactory, directLightFalloff);
    #endif
        finalColor = lerp(finalColor, _LightColor0.rgb, vlFactory);
    #endif
        finalColor = GammaToLinearSpace(finalColor);
        return finalColor;
    }
#endif

//Fog End

//Shadow Begin
half SHADOW_MIN_ATTENUATION = 1;
#define BF_LIGHT_ATTENUATION(destName, input, worldPos) fixed destName = max(SHADOW_ATTENUATION(input), SHADOW_MIN_ATTENUATION);
//Shadow End

//Occlusion Begin
sampler2D   _OcclusionMap;
half        _OcclusionStrength;

half _UseRealtimeShadow;
half _ShadowDistance;
float4x4 _ShadowMatrix;
half4 _ShadowColor;

half Occlusion(float2 uv)
{
    #if (SHADER_TARGET < 30)
        return tex2D(_OcclusionMap, uv).g;
    #else
        half occ = tex2D(_OcclusionMap, uv).g;
        return LerpOneTo (occ, _OcclusionStrength);
    #endif
}
//Occlusion End


inline half4 BF_VertexGIForward(float2 uv1, float uv2, float3 posWorld, half3 normalWorld)
{
    half4 ambientOrLightmapUV = 0;
    // Static lightmaps
    #ifdef LIGHTMAP_ON
        ambientOrLightmapUV.xy = uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
        ambientOrLightmapUV.zw = 0;
    // Sample light probe for Dynamic objects only (no static or dynamic lightmaps)
    #elif UNITY_SHOULD_SAMPLE_SH
        #ifdef VERTEXLIGHT_ON
            // Approximated illumination from non-important point lights
            ambientOrLightmapUV.rgb = Shade4PointLights (
                unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
                unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
                unity_4LightAtten0, posWorld, normalWorld);
        #endif

        ambientOrLightmapUV.rgb = ShadeSHPerVertex (normalWorld, ambientOrLightmapUV.rgb);
    #endif

    #ifdef DYNAMICLIGHTMAP_ON
        ambientOrLightmapUV.zw = uv2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
    #endif

    return ambientOrLightmapUV;
}

inline UnityGI BF_FragmentGI (float3 worldPos, float3 worldNormal, half3 worldViewDir, half occlusion, half4 i_ambientOrLightmapUV, half atten, UnityLight light)
{
    UnityGIInput d;
    d.light = light;
    d.worldPos = worldPos;
    d.worldViewDir = worldViewDir;
    d.atten = atten;
    #if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
        d.ambient = 0;
        d.lightmapUV = i_ambientOrLightmapUV;
    #else
        d.ambient = i_ambientOrLightmapUV.rgb;
        d.lightmapUV = 0;
    #endif

    d.probeHDR[0] = unity_SpecCube0_HDR;
    d.probeHDR[1] = unity_SpecCube1_HDR;
    #if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
      d.boxMin[0] = unity_SpecCube0_BoxMin; // .w holds lerp value for blending
    #endif
    #ifdef UNITY_SPECCUBE_BOX_PROJECTION
      d.boxMax[0] = unity_SpecCube0_BoxMax;
      d.probePosition[0] = unity_SpecCube0_ProbePosition;
      d.boxMax[1] = unity_SpecCube1_BoxMax;
      d.boxMin[1] = unity_SpecCube1_BoxMin;
      d.probePosition[1] = unity_SpecCube1_ProbePosition;
    #endif

    return UnityGlobalIllumination (d, occlusion, worldNormal);
}

half SampleBakedOcclusion(float2 lightmapUV, float3 positionWS, float3 shadowCoord)
{
#if defined(LIGHTMAP_ON)
    fixed4 rawOcclusionMask = UNITY_SAMPLE_TEX2D(unity_ShadowMask, lightmapUV.xy);
#if _G_USE_CUSTOMREALTIMESHADOW
    half outBox = dot(step(1, shadowCoord.xyz), half3(1, 1, 1)) + dot(step(shadowCoord.xyz, 0), half3(1, 1, 1));
    outBox = saturate(1 - outBox);
    rawOcclusionMask = lerp(rawOcclusionMask, 1, saturate(_UseRealtimeShadow * outBox));
#endif
    return rawOcclusionMask.r;
#else
    return 1;
#endif
}

half3 SampleSH(half3 ambient, half3 normal, float3 positionWS)
{
    half3 ambient_contrib = 0.0;
#if UNITY_SAMPLE_FULL_SH_PER_PIXEL
    #if UNITY_LIGHT_PROBE_PROXY_VOLUME
        if (unity_ProbeVolumeParams.x == 1.0)
            ambient_contrib = SHEvalLinearL0L1_SampleProbeVolume(half4(normal, 1.0), positionWS);
        else
            ambient_contrib = SHEvalLinearL0L1(half4(normal, 1.0));
    #else
        ambient_contrib = SHEvalLinearL0L1(half4(normal, 1.0));
    #endif

        ambient_contrib += SHEvalLinearL2(half4(normal, 1.0));

        ambient += max(half3(0, 0, 0), ambient_contrib);

    #ifdef UNITY_COLORSPACE_GAMMA
        ambient = LinearToGammaSpace(ambient);
    #endif
#elif (SHADER_TARGET < 30) || UNITY_STANDARD_SIMPLE
    // Completely per-vertex
    ambient += max(half3(0,0,0), ShadeSH9 (half4(normal, 1.0)));
#else
    #ifdef UNITY_COLORSPACE_GAMMA
        ambient = GammaToLinearSpace (ambient);
    #endif
    ambient += SHEvalLinearL2 (half4(normal, 1.0));   
    #if UNITY_LIGHT_PROBE_PROXY_VOLUME
        if (unity_ProbeVolumeParams.x == 1.0)
            ambient_contrib = SHEvalLinearL0L1_SampleProbeVolume (half4(normal, 1.0), positionWS);
        else
            ambient_contrib = SHEvalLinearL0L1 (half4(normal, 1.0));
    #else
        ambient_contrib = SHEvalLinearL0L1 (half4(normal, 1.0));
    #endif

    ambient = max(half3(0, 0, 0), ambient + ambient_contrib);     // include L2 contribution in vertex shader before clamp.
    #ifdef UNITY_COLORSPACE_GAMMA
        ambient = LinearToGammaSpace (ambient);
    #endif
#endif

    return ambient;
}

void initAmbientAndShadowMask(float2 lightmapUV, out half3 ambient, out half shadowMask, float3 positionWS, half3 normalWS, float3 shadowCoord)
{
    ambient = 0;
    #if defined(LIGHTMAP_ON)
    half4 bakedColorTex = UNITY_SAMPLE_TEX2D(unity_Lightmap, lightmapUV);
    ambient = DecodeLightmap(bakedColorTex).rgb;
    // Sample light probe for Dynamic objects only (no static or dynamic lightmaps)
    #else
        #ifdef VERTEXLIGHT_ON
            // Approximated illumination from non-important point lights
            ambient.rgb = Shade4PointLights (
                unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
                unity_LightColor[0].rgb, unity_LightColor[1].rgb, unity_LightColor[2].rgb, unity_LightColor[3].rgb,
                unity_4LightAtten0, positionWS, normalWS);
        #endif

        ambient.rgb = SampleSH(ambient.rgb, normalWS, positionWS);
    // #else
    //     ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;
    #endif

    #ifdef LIGHTMAP_ON
    shadowMask = SampleBakedOcclusion(lightmapUV, positionWS, shadowCoord);
    #else
    shadowMask = 1;
    #endif
    // #ifdef LIGHTMAP_ON
    //     half4 bakedColorTex = UNITY_SAMPLE_TEX2D(unity_Lightmap, lightmapUV);
    //     shadowMask = SampleBakedOcclusion(lightmapUV);    
    //     ambient = DecodeLightmap(bakedColorTex).rgb;
    // #else
    //     ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;
    //     shadowMask = 1;
    // #endif
}

half3 modifyAmbientByShadowColor(half3 ambient, half atten)
{
    //if (atten < 0.5 && length(_ShadowColor) > 0.01)
    //    ambient *= _ShadowColor.rgb  * (1- atten);
    // 暂定
    if (length(_ShadowColor) > 0.01)
        // ambient = ambient * unity_ShadowColor * atten + ambient * (1 - atten);
        // ambient = ambient * (unity_ShadowColor * atten + 1 - atten);
        ambient *= unity_ShadowColor * atten + 1 - atten;
    return ambient;
}

half3 MyGI_IndirectSpecular(UnityGIInput data, Unity_GlossyEnvironmentData glossIn , float kBlendFactor)
{
    half3 specular;

    #ifdef UNITY_SPECCUBE_BOX_PROJECTION
        // we will tweak reflUVW in glossIn directly (as we pass it to Unity_GlossyEnvironment twice for probe0 and probe1), so keep original to pass into BoxProjectedCubemapDirection
        half3 originalReflUVW = glossIn.reflUVW;
        glossIn.reflUVW = BoxProjectedCubemapDirection (originalReflUVW, data.worldPos, data.probePosition[0], data.boxMin[0], data.boxMax[0]);
    #endif

    #ifdef _GLOSSYREFLECTIONS_OFF
        specular = unity_IndirectSpecColor.rgb;
    #else
        half3 env0 = Unity_GlossyEnvironment (UNITY_PASS_TEXCUBE(unity_SpecCube0), data.probeHDR[0], glossIn);
        #ifdef UNITY_SPECCUBE_BLENDING
            //const float kBlendFactor = 0.99999;
            float blendLerp = data.boxMin[0].w;
            UNITY_BRANCH
            if (blendLerp < kBlendFactor)
            {
                #ifdef UNITY_SPECCUBE_BOX_PROJECTION
                    glossIn.reflUVW = BoxProjectedCubemapDirection (originalReflUVW, data.worldPos, data.probePosition[1], data.boxMin[1], data.boxMax[1]);
                #endif

                half3 env1 = Unity_GlossyEnvironment (UNITY_PASS_TEXCUBE_SAMPLER(unity_SpecCube1,unity_SpecCube0), data.probeHDR[1], glossIn);
                specular = lerp(env1, env0, blendLerp);
        
            }
            else
            {
                specular = env0;
            }
        #else
            specular = env0;
        #endif
    #endif

    return specular;
}

#endif //BF_CG