#ifndef CAMERALIGHT_INCLUDED
#define CAMERALIGHT_INCLUDED

#define _G_CAMERA_LIGHT 1                      //是否开启镜头光
#define _G_CAMERA_LIGHT_SPEC 1                 //是否开启镜头高光
#define _G_CAMERA_LIGHT_DIFFUSE 1              //是否开启镜头漫反射光
#define _CameraLightColor half3(0.2, 0.2, 0.2) //镜头光的颜色值

#if _G_CAMERA_LIGHT && defined(_CameraLight_IsOpening)
	//half3 _CameraLightColor;

	//half _CamLightIntensity;
	half3 DirectBDRFURP(half NoH, half LoH, half roughness)
	{
		half r2 = roughness * roughness;
		half d = NoH * NoH * (r2 - 1.0) + 1.0;

		half LoH2 = LoH * LoH;
		half normalizationTerm = roughness * 4.0 + 2.0;
		half specularTerm = r2 / ((d * d) * max(0.1h, LoH2) * normalizationTerm);

		half3 color = min(15.0, specularTerm);
		return color;
	}



	half3 CalcDiffuse(half3 camLightColor, half3 normalWS, float3 viewDir, half3 diffuseColor)
	{
		float3 halfDir = viewDir;// normalize(viewDir + lightDir);
		float3 lightDir = viewDir;
		half dotNL = dot(lightDir, normalWS);
		half NoL = saturate(dotNL);
		half3 diffuse = camLightColor * NoL * diffuseColor;
		return diffuse;
	}


	half3 CalcCameraLight(half3 camLightColor, half3 normalWS, half3 fTerm, float3 viewDir, half roughness, half3 diffuseColor, half4 cameraLightColorScale)
	{

		camLightColor *= cameraLightColorScale.xyz;

		float3 halfDir = viewDir;
		float3 lightDir = viewDir;

		#if _G_CAMERA_LIGHT_DIFFUSE
		half3 diffuse = CalcDiffuse(camLightColor, normalWS, viewDir, diffuseColor);
		#else
		half3 diffuse = half3(0, 0, 0);
		#endif

		#if _G_CAMERA_LIGHT_SPEC
			half NoH = saturate(dot(halfDir, normalWS));
			half LoH = saturate(dot(lightDir, halfDir));
			half3 spec = DirectBDRFURP(NoH, LoH, roughness)* fTerm* camLightColor.rgb;// *NoLfinal;
			return diffuse + spec;
		#else
			return diffuse;
		#endif
	}	

	#define APPLY_CAMERA_LIGHT(normalWS, fTerm, viewDir, roughness, diffuseColor, cameraLightColorScale) CalcCameraLight(_CameraLightColor, normalWS, fTerm, viewDir, roughness, diffuseColor, cameraLightColorScale)	
#else
	#define APPLY_CAMERA_LIGHT(normalWS, fTerm, viewDir, roughness, diffuseColor, cameraLight_IsOpening, cameraLightColorScale)  half3(0, 0, 0)	
#endif

#endif