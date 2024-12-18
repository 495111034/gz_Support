
#include "../Common/CameraLight.hlsl"

float _Anisotropy, _Distortion, _Scale, _Power, _specularTermScale, _indirectReflectScale;
half _GlossScale, _MetallicScale;   
fixed4 _Color;
float4 _MainTex_ST, _MetallicTex_ST;
sampler2D _MainTex, _BumpMap, _MetallicTex;

#if _G_CAMERA_LIGHT && defined(_CameraLight_IsOpening)
	half4 _CameraLightColorScale;
#endif
            

struct SurfaceOutputStandardAnisotropic
{
	fixed Alpha;		
	half Anisotropy;
	half Metallic;		
	half Smoothness;	
	half Occlusion;	
	half3 Albedo;		
	float3 Normal;
	float3 Tangent;
	float3 BiTangent;
	half3 Emission;
	// float3x3 WorldVectors;
};


float ClampRoughnessForAnalyticalLights(float roughness)
{
	return max(roughness, 0.000001);
}

void ConvertAnisotropyToRoughness(float roughness, float anisotropy, out float roughnessT, out float roughnessB)
{
	float anisoAspect = sqrt(1.0 - 0.9 * anisotropy);
	roughnessT = roughness / anisoAspect; 
	roughnessB = roughness * anisoAspect; 
}

float D_GGXAnisotropic(float TdotH, float BdotH, float NdotH, float roughnessT, float roughnessB)
{
	float f = TdotH * TdotH / (roughnessT * roughnessT) + BdotH * BdotH / (roughnessB * roughnessB) + NdotH * NdotH;
	return 1.0 / (roughnessT * roughnessB * f * f);
}

float SmithJointGGXAnisotropic(float TdotV, float BdotV, float NdotV, float TdotL, float BdotL, float NdotL, float roughnessT, float roughnessB)
{
	float aT = roughnessT;
	float aT2 = aT * aT;
	float aB = roughnessB;
	float aB2 = aB * aB;
	float lambdaV = NdotL * sqrt(aT2 * TdotV * TdotV + aB2 * BdotV * BdotV + NdotV * NdotV);
	float lambdaL = NdotV * sqrt(aT2 * TdotL * TdotL + aB2 * BdotL * BdotL + NdotL * NdotL);

	return 0.5 / (lambdaV + lambdaL);
}

float3 ComputeGrainNormal(float3 grainDir, float3 V)
{
	float3 B = cross(-V, grainDir);
	return cross(B, grainDir);
}

float3 GetAnisotropicModifiedNormal(float3 grainDir, float3 N, float3 V, float anisotropy)
{
	float3 grainNormal = ComputeGrainNormal(grainDir, V);
	return normalize(lerp(N, grainNormal, anisotropy));
}

half3 CalcCameraLightHair(half3 camLightColor, half4 cameraLightColorScale, fixed3 viewDir, SurfaceOutputStandardAnisotropic s, SurfaceData surfaceData)
{	
	camLightColor *= cameraLightColorScale.xyz;
	
	float shiftAmount = dot(s.Normal, viewDir);
	s.Normal = shiftAmount < 0.0f ? s.Normal + viewDir * (-shiftAmount + 1e-5f) : s.Normal;

	float NdotL = saturate(dot(s.Normal, viewDir));
	float NdotV = abs(dot(s.Normal, viewDir));
	float3 H = Unity_SafeNormalize(viewDir+ viewDir);
	float NdotH = saturate(dot(s.Normal, H));
	float LdotH = saturate(dot(viewDir, H));
	float TdotH = dot(s.Tangent, H);
	float TdotL = dot(s.Tangent, viewDir);
	float BdotH = dot(s.BiTangent, H);
	float BdotL = dot(s.BiTangent, viewDir);
	float TdotV = dot(viewDir, s.Tangent);
	float BdotV = dot(viewDir, s.Tangent);

	half grazingTerm = saturate(surfaceData.smoothness + (1 - surfaceData.oneMinusReflectivity));

	float roughnessT;
	float roughnessB;
	ConvertAnisotropyToRoughness(surfaceData.roughness, s.Anisotropy, roughnessT, roughnessB);

	roughnessT = ClampRoughnessForAnalyticalLights(roughnessT);
	roughnessB = ClampRoughnessForAnalyticalLights(roughnessB);

	float V = SmithJointGGXAnisotropic(TdotV, BdotV, NdotV, TdotL, BdotL, NdotL, roughnessT, roughnessB);
	float D = D_GGXAnisotropic(TdotH, BdotH, NdotH, roughnessT, roughnessB);

#if _G_USE_SPECULAR
	float3 specularTerm = V * D;
	#ifdef UNITY_COLORSPACE_GAMMA
		specularTerm = sqrt(max(1e-4h, specularTerm));
	#endif
	specularTerm = clamp(specularTerm * NdotL, 0, 10);
	specularTerm = clamp(specularTerm , 0 , 1);
	specularTerm *= _specularTermScale;
#else
	float3 specularTerm = 0;
#endif

#if _G_CAMERA_LIGHT_DIFFUSE
	float diffuseTerm = DisneyDiffuse(NdotV, NdotL, LdotH, surfaceData.perceptualRoughness) * NdotL;
#else
	float diffuseTerm =0;
#endif
																	
	half3 color = (surfaceData.diffuseColor * (surfaceData.diffuseColor + camLightColor * diffuseTerm))
		+ specularTerm * camLightColor * (FresnelTerm(surfaceData.specularColor, LdotH));	
	return color;
}

float4 TranslucentBRDF(half3 viewDir, SurfaceOutputStandardAnisotropic s, UnityGI gi, SurfaceData surfaceData)
{
	float shiftAmount = dot(s.Normal, viewDir);
	s.Normal = shiftAmount < 0.0f ? s.Normal + viewDir * (-shiftAmount + 1e-5f) : s.Normal;

	float NdotL = saturate(dot(s.Normal, gi.light.dir));
	float NdotV = abs(dot(s.Normal, viewDir));
	float3 H = Unity_SafeNormalize(gi.light.dir + viewDir);
	float NdotH = saturate(dot(s.Normal, H));
	float LdotH = saturate(dot(gi.light.dir, H));
	float TdotH = dot(s.Tangent, H);
	float TdotL = dot(s.Tangent, gi.light.dir);
	float BdotH = dot(s.BiTangent, H);
	float BdotL = dot(s.BiTangent, gi.light.dir);
	float TdotV = dot(viewDir, s.Tangent);
	float BdotV = dot(viewDir, s.BiTangent);

	half grazingTerm = saturate(surfaceData.smoothness + (1 - surfaceData.oneMinusReflectivity));

	float roughnessT;
	float roughnessB;
	ConvertAnisotropyToRoughness(surfaceData.roughness, s.Anisotropy, roughnessT, roughnessB);

	roughnessT = ClampRoughnessForAnalyticalLights(roughnessT);
	roughnessB = ClampRoughnessForAnalyticalLights(roughnessB);

	float V = SmithJointGGXAnisotropic(TdotV, BdotV, NdotV, TdotL, BdotL, NdotL, roughnessT, roughnessB);
	float D = D_GGXAnisotropic(TdotH, BdotH, NdotH, roughnessT, roughnessB);

#if _G_USE_SPECULAR
	float3 specularTerm = V * D;
	#ifdef UNITY_COLORSPACE_GAMMA
		specularTerm = sqrt(max(1e-4h, specularTerm));
	#endif
	specularTerm = max(0, specularTerm * NdotL);
	specularTerm = clamp(specularTerm , 0 , 1);
	specularTerm *= _specularTermScale;
#else
	float3 specularTerm = 0;
#endif

	float diffuseTerm = DisneyDiffuse(NdotV, NdotL, LdotH, surfaceData.perceptualRoughness) * NdotL;
	half surfaceReduction;
#ifdef UNITY_COLORSPACE_GAMMA
	surfaceReduction = 1.0 - 0.28 * surfaceData.roughness * surfaceData.perceptualRoughness;		
#else
	surfaceReduction = 1.0 / (surfaceData.roughness * surfaceData.roughness + 1.0);			
#endif

#if _G_HAIR_REFLECT
	gi.indirect.specular = lerp(unity_IndirectSpecColor.rgb, gi.indirect.specular, _indirectReflectScale);
#else
	gi.indirect.specular = 0;
#endif

	half3 color = (surfaceData.diffuseColor * (gi.indirect.diffuse + gi.light.color * diffuseTerm))
					+ specularTerm * gi.light.color * (FresnelTerm(surfaceData.specularColor, LdotH))
					+ (surfaceReduction * gi.indirect.specular * FresnelLerp(surfaceData.specularColor, grazingTerm, NdotV));//* _Fresnel * lerp(float3(1,1,1),specColor,_FresnelDamp));
	return half4(color, 1);
}

inline half4 LightFunctionStandardAnisotropic(half3 viewDir, SurfaceOutputStandardAnisotropic s, UnityGI gi, SurfaceData surfaceData)
{
	half outputAlpha;
	surfaceData.diffuseColor = PreMultiplyAlpha(surfaceData.diffuseColor, s.Alpha, surfaceData.oneMinusReflectivity, outputAlpha);

	half4 color = TranslucentBRDF(viewDir, s, gi, surfaceData);

#if _G_CAMERA_LIGHT && defined(_CameraLight_IsOpening) && _G_HAIR_CAMERALIGHT
	half3 cameraLightColor = CalcCameraLightHair(_CameraLightColor, _CameraLightColorScale, viewDir, s, surfaceData);
	color.rgb += cameraLightColor;
#endif
		
	color.a = outputAlpha;
	return color;
}


inline fixed4 LightingStandardTranslucent(fixed3 viewDir, SurfaceOutputStandardAnisotropic s, UnityGI gi, SurfaceData surfaceData)
{
	fixed4 pbr = LightFunctionStandardAnisotropic(viewDir, s, gi, surfaceData);

	float3 L = gi.light.dir;
	float3 V = viewDir;
	float3 N = s.Normal;
 
	float3 H = normalize(L + N * _Distortion);
	float I = (pow(saturate(dot(V, -H)), _Power) * _Scale);
 
	pbr.rgb = pbr.rgb + gi.light.color * (I*s.Albedo);
	
	return pbr;
}

inline half3 UnityGI_AnisotropicIndirectSpecular(UnityGIInput data, SurfaceOutputStandardAnisotropic surfaceData, Unity_GlossyEnvironmentData glossIn)
{
	half3 specular;

	float3 iblNormalWS = GetAnisotropicModifiedNormal(surfaceData.BiTangent, surfaceData.Normal, data.worldViewDir, surfaceData.Anisotropy);
	float3 iblR = reflect(-data.worldViewDir, iblNormalWS);

	#ifdef UNITY_SPECCUBE_BOX_PROJECTION
		half3 originalReflUVW = glossIn.reflUVW;
		glossIn.reflUVW = BoxProjectedCubemapDirection(iblR, data.worldPos, data.probePosition[0], data.boxMin[0], data.boxMax[0]);
	#endif

	#ifdef _GLOSSYREFLECTIONS_OFF
		specular = unity_IndirectSpecColor.rgb;
	#else
		half3 env0 = Unity_GlossyEnvironment(UNITY_PASS_TEXCUBE(unity_SpecCube0), data.probeHDR[0], glossIn);
		#ifdef UNITY_SPECCUBE_BLENDING
			const float kBlendFactor = 0.99999;
			float blendLerp = data.boxMin[0].w;
			UNITY_BRANCH
			if (blendLerp < kBlendFactor)
			{
				#ifdef UNITY_SPECCUBE_BOX_PROJECTION
					glossIn.reflUVW = BoxProjectedCubemapDirection(iblR, data.worldPos, data.probePosition[1], data.boxMin[1], data.boxMax[1]);
				#endif
				half3 env1 = Unity_GlossyEnvironment(UNITY_PASS_TEXCUBE_SAMPLER(unity_SpecCube1, unity_SpecCube0), data.probeHDR[1], glossIn);
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

	return specular * surfaceData.Occlusion;
}

inline UnityGI UnityAnisotropicGlobalIllumination(UnityGIInput data, SurfaceOutputStandardAnisotropic surfaceData, Unity_GlossyEnvironmentData glossIn)
{
	UnityGI o_gi = UnityGI_Base(data, surfaceData.Occlusion, surfaceData.Normal);
#if _G_HAIR_REFLECT
	o_gi.indirect.specular = UnityGI_AnisotropicIndirectSpecular(data, surfaceData, glossIn);
#else
	o_gi.indirect.specular = 0;
#endif
	return o_gi;
}


inline void LightingStandardTranslucent_GI(SurfaceOutputStandardAnisotropic surfaceData, UnityGIInput data, inout UnityGI gi)
{
#if defined(UNITY_PASS_DEFERRED) && UNITY_ENABLE_REFLECTION_BUFFERS
	gi = UnityGI_Base(data, surfaceData.Occlusion, surfaceData.Normal);
#else
	Unity_GlossyEnvironmentData g;
	g.roughness = SmoothnessToPerceptualRoughness(surfaceData.Smoothness);
	g.reflUVW = reflect(-data.worldViewDir, surfaceData.Normal);
	// Unity_GlossyEnvironmentData g = UnityGlossyEnvironmentSetup(s.Smoothness, data.worldViewDir, s.Normal, lerp(unity_ColorSpaceDielectricSpec.rgb, s.Albedo, s.Metallic));
	gi = UnityAnisotropicGlobalIllumination(data, surfaceData, g);
#endif
}

