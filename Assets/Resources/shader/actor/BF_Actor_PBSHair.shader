
Shader "BF/Actor/PBSHair" {
	Properties {
		_MainTex ("主纹理", 2D) = "white" {}
		_Color ("混合颜色", Color) = (1,1,1,1)
		_BumpMap ("法线贴图", 2D) = "" {}
		_MetallicTex("金属高光纹理,金属度(R),自发光(G),未启用(B),光滑度(A)",2D) = "black" {}
		_MetallicScale ("金属度缩放", Range(0,2))             = 1
		_GlossScale("光滑度缩放", Range(0, 2))                = 1
		_Cutoff("剔除Alpha值",Range(0.001,1)) = 0.1
		_specularTermScale("(直接光)高光强度", Range(0, 1)) = 1
        _indirectReflectScale("(间接光)反射强度", Range(0, 1)) = 0.44
		_TangentMap("切线纹理(RG)", 2D) = "white" {}
		_Anisotropy ("各项异性", Range(0,1)) = 1.0
		_Distortion ("扭曲法线", Range(0,5)) = 1.0
		_Scale ("亮度缩放", Range(0,5)) = 1.0
		_Power ("亮度范围", Range(0.0001,5)) = 1.0
		_RimColor("边缘光颜色", Color) = (0, 0, 0, 1)

		[HDR]_DissolveColor("消融颜色", Color) = (0, 0, 0, 0)
        [HDR]_DissolveEdgeColor("消融外边缘颜色", Color) = (1, 1, 1, 1)
        _DissolveThreshold("消融阈值", Range(0, 1)) = 0
        _DissolveColorRatio("消融颜色总占比(对整个角色占比)", Range(0, 1)) = 0.7
        _DissolveEdgeRatio("消融外边缘颜色占比(对消融颜色占比)", Range(0, 1)) = 0.7
		_GiDiffuse("天光比例",Range(0,1)) = 1
		[Toggle(_CameraLight_IsOpening)] _CameraLight_IsOpening("开启镜头光", Float) = 1
		[HDR]_CameraLightColorScale("镜头光的强度控制", Color) = (1,1,1,1)
		
		[Space(15)]
		[Toggle(FRESNELRIM_ON)] FRESNEL_ON("开启FresnelRim",Float) =0
    	[Space(15)][Header(Fresnel Properties)][Space(15)]_FresnelColor("Color", Color) = (0.6933962,1,0.9814353,1)
    	_SelfIllumination("Self Illumination", Range( 1 , 10)) = 1		
		[Space(15)]_FresnelIntensity("Fresnel Intensity", Range( 0 , 200)) = 4
		_FresnelPower("Fresnel Power", Float) = 4
		_FresnelBias("Bias", Range( 0 , 1)) = 0
		[Toggle]_Invert("Invert", Float) = 0

        [Toggle] _UseObjectRealtimeShadow("使用物体实时阴影", float) = 1
		_ObjectRealtimeShadowIntensity("物体实时阴影强度", Range(0, 1)) = 1
        // [Toggle] _UseRoleRealtimeShadow("使用角色实时阴影", float) = 0
        // _RoleRealtimeShadowIntensity("角色实时阴影强度", Range(0, 1)) = 1
	}

	CGINCLUDE

		#define INTERNAL_DATA half3 internalSurfaceTtoW0; half3 internalSurfaceTtoW1; half3 internalSurfaceTtoW2;
		#define WorldReflectionVector(data,normal) reflect (data.worldRefl, half3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal)))
		#define WorldNormalVector(data,normal) fixed3(dot(data.internalSurfaceTtoW0,normal), dot(data.internalSurfaceTtoW1,normal), dot(data.internalSurfaceTtoW2,normal))

		#pragma target 3.0
		//优化  #pragma multi_compile_fog
		// #pragma enable_d3d11_debug_symbols
        #pragma multi_compile __ FOG_HEIGHT //FOG_LINEAR
		#pragma multi_compile _G_GAME_QUALITY_HIGH _G_GAME_QUALITY_LOW _G_GAME_QUALITY_VERYLOW
		#pragma multi_compile_fwdbasealpha noshadowmask nodynlightmap nolightmap noshadow
		#pragma shader_feature_local _CameraLight_IsOpening
		#pragma multi_compile __ FRESNELRIM_ON //#pragma shader_feature_local _ FRESNELRIM_ON//xx 

		//优化  add
		#pragma skip_variants  LIGHTPROBE_SH DYNAMICLIGHTMAP_ON DIRLIGHTMAP_COMBINED _SPECULARHIGHLIGHTS_OFF VERTEXLIGHT_ON 
		#pragma skip_variants  UNITY_SPECCUBE_BOX_PROJECTION UNITY_SPECCUBE_BLENDING 
		#pragma skip_variants FOG_EXP FOG_EXP2
		#pragma skip_variants DIRECTIONAL_COOKIE POINT_COOKIE SPOT _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

		#include "UnityCG.cginc"
		#include "Lighting.cginc"
		#include "AutoLight.cginc"
		#include "../Scene/BF_Scene_LightingCommon.cginc"
		#include "../Scene/BF_Scene_GlobalQualityVars.cginc"
		#include "../Common/BingFengCG.cginc"
		#include "../Common/BingFengHairCommon.cginc"
		#include "../Common/CameraLight.hlsl"  

		struct Input{
			float2 uv_MainTex;
			float3 normal;
			float3 viewDir;
			float3 normalDir;
			float3 tangentDir;
			float3 bitangentDir;
		};

		struct a2v {
			float4 vertex : POSITION;
			float4 tangent : TANGENT;
			float3 normal : NORMAL;
			float4 texcoord : TEXCOORD0;
			float4 texcoord1 : TEXCOORD1;
		};

		struct v2f {
			UNITY_POSITION(pos);
			float4 uv01 : TEXCOORD0; // _MainTexUV and LightmapUV
			float4 tSpace0 : TEXCOORD1;
			float4 tSpace1 : TEXCOORD2;
			float4 tSpace2 : TEXCOORD3;
			// float3 normal : TEXCOORD4;
			// float3 normalDir : TEXCOORD5;
			// float3 tangentDir : TEXCOORD6;
			// float3 bitangentDir : TEXCOORD7;
		#if UNITY_SHOULD_SAMPLE_SH
			half3 sh : TEXCOORD4; // SH
		#endif
			UNITY_LIGHTING_COORDS(5,6)
		#if USING_FOG
			UNITY_FOG_COORDS(7)
		#endif

		#if _G_USE_CUSTOMREALTIMESHADOW
			float4 objectShadowScreenPos  : TEXCOORD8;
			// float4 roleShadowScreenPos  : TEXCOORD11;
		#endif
			UNITY_VERTEX_OUTPUT_STEREO
		};

		//>dissolve
		fixed _DissolveColorRatio, _DissolveEdgeRatio, _DissolveThreshold;
		half4 _DissolveColor, _DissolveEdgeColor;
		//<
		fixed _GiDiffuse;

       	#if FRESNELRIM_ON
			half4 _FresnelColor;
			half _SelfIllumination;
			half _Invert;
			half _FresnelBias;
			half _FresnelIntensity;
			half _FresnelPower;
        #endif

		#if _G_USE_CUSTOMREALTIMESHADOW
			half _UseObjectRealtimeShadow;
			half _ObjectRealtimeShadowIntensity;
			sampler2D _ObjectShadowMap;
		#endif
	
		v2f vert (a2v v) {
			v2f o;
			UNITY_INITIALIZE_OUTPUT(v2f, o);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
	
			//o.normalDir = normalize(UnityObjectToWorldNormal(v.normal));
			// float3 tangentMul = normalize(mul(unity_ObjectToWorld, v.tangent.xyz));
			// o.tangentDir = float4(tangentMul, v.tangent.w);
			// o.bitangentDir = cross(o.normalDir, o.tangentDir);
			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv01.xy = TRANSFORM_TEX(v.texcoord, _MainTex);

			float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
			float3 worldNormal = UnityObjectToWorldNormal(v.normal);
			float3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
			half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
			float3 worldBitangent = cross(worldNormal, worldTangent) * tangentSign;
			o.tSpace0 = float4(worldTangent.x, worldBitangent.x, worldNormal.x, worldPos.x);
			o.tSpace1 = float4(worldTangent.y, worldBitangent.y, worldNormal.y, worldPos.y);
			o.tSpace2 = float4(worldTangent.z, worldBitangent.z, worldNormal.z, worldPos.z);

		#ifdef LIGHTMAP_ON
			o.uv01.zw = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
		#endif

			COMPUTE_LIGHT_COORDS(o); 

		#if USING_FOG
		#if defined(FOG_HEIGHT)
			UNITY_TRANSFER_FOG(o, worldPos);
		#else
			UNITY_TRANSFER_FOG(o, o.pos);
		#endif
		#endif

		#if _G_USE_CUSTOMREALTIMESHADOW
			o.objectShadowScreenPos = mul(_ShadowMatrix, float4(worldPos.xyz, 1));
		#endif

			return o;
		}

	ENDCG

	SubShader {
		LOD 200
		Tags { "RenderType"="Transparent" "Queue"="Transparent-10"}
		
		Pass {
			Tags { "LightMode" = "ForwardBase" }
            Cull back
            ZWrite On

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			fixed _Cutoff;
			half4 _RimColor;


			fixed4 frag(v2f IN) : SV_Target {

				half4 albedo = tex2D(_MainTex, IN.uv01.xy) * _Color;
				clip(albedo.a - _Cutoff);
				half4 metallicTex = tex2D(_MetallicTex, IN.uv01.xy);
			#if _G_HAIR_DISSOLVE
				clip(metallicTex.b - _DissolveThreshold);
			#endif

				SurfaceOutputStandardAnisotropic o;
				UNITY_INITIALIZE_OUTPUT(SurfaceOutputStandardAnisotropic, o);
				o.Albedo = albedo;
				o.Metallic = metallicTex.r * _MetallicScale;
				o.Smoothness = metallicTex.a * _GlossScale;
				o.Alpha = albedo.a;
				o.Anisotropy = _Anisotropy;
				o.Emission = 0.0;
				o.Occlusion = 1.0;	
				o.Tangent = float3(IN.tSpace0.x,  IN.tSpace1.x, IN.tSpace2.x);
				o.BiTangent = float3(IN.tSpace0.y,  IN.tSpace1.y, IN.tSpace2.y);
				// o.WorldVectors = float3x3(IN.tangentDir, IN.bitangentDir, IN.normalDir);

			#if _G_USE_NORMAL
				fixed3 normalTS = UnpackNormal(tex2D(_BumpMap, IN.uv01.xy));
				o.Normal = normalize(float3(dot(IN.tSpace0, normalTS), 
											dot(IN.tSpace1, normalTS), 
											dot(IN.tSpace2, normalTS)));
			#else
				o.Normal = normalize(float3(IN.tSpace0.z,  IN.tSpace1.z, IN.tSpace2.z));
			#endif
				float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);

				UnityGI gi;
				UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
				gi.light.color = _LightColor0.rgb;
			#ifndef USING_DIRECTIONAL_LIGHT
				gi.light.dir = normalize(UnityWorldSpaceLightDir(worldPos));
			#else
				gi.light.dir = _WorldSpaceLightPos0.xyz;
			#endif

			#if _G_USE_CUSTOMREALTIMESHADOW
				UNITY_BRANCH
				if(_UseObjectRealtimeShadow > 0.5)
				{
					float3 objectShadowScreenPos = IN.objectShadowScreenPos.xyz / IN.objectShadowScreenPos.w;
					objectShadowScreenPos = objectShadowScreenPos * 0.5 + 0.5;
					float objectRealtimeShadowMap = tex2D(_ObjectShadowMap, objectShadowScreenPos.xy).r;
				#if UNITY_REVERSED_Z
					objectRealtimeShadowMap = 1 - objectRealtimeShadowMap;
				#endif
					half inShadow = step(objectRealtimeShadowMap, objectShadowScreenPos.z);
					half outBox = saturate(dot(step(1, objectShadowScreenPos.xyz), half3(1, 1, 1)) + dot(step(objectShadowScreenPos.xyz, 0), half3(1, 1, 1)));
					inShadow = inShadow - inShadow * outBox;
					gi.light.color *= (1 - inShadow) + (inShadow - inShadow * _ObjectRealtimeShadowIntensity);
				}
			#endif

				UnityGIInput giInput;
				UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
				giInput.light = gi.light;
				giInput.worldPos = worldPos;
				giInput.worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
				UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)
				giInput.atten = atten;
			#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
				giInput.lightmapUV = IN.uv01.zw;
			#endif
			#if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
				giInput.ambient = _GiDiffuse * IN.sh +(1- _GiDiffuse);
					//giInput.ambient = IN.sh;
			#endif
				giInput.probeHDR[0] = unity_SpecCube0_HDR;
				giInput.probeHDR[1] = unity_SpecCube1_HDR;
			#if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
				giInput.boxMin[0] = unity_SpecCube0_BoxMin; 
			#endif
			#ifdef UNITY_SPECCUBE_BOX_PROJECTION
				giInput.boxMin[1] = unity_SpecCube1_BoxMin;
				giInput.boxMax[0] = unity_SpecCube0_BoxMax;
				giInput.boxMax[1] = unity_SpecCube1_BoxMax;
				giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
				giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
			#endif

				LightingStandardTranslucent_GI(o, giInput, gi);

				SurfaceData surfaceData = GetSurfaceData(o.Albedo, o.Metallic, o.Smoothness, o.Normal, giInput.worldViewDir, gi.light.dir);

			#if _G_HAIR_ANISOTROPICRENDER
				fixed4 color = LightingStandardTranslucent(giInput.worldViewDir, o, gi, surfaceData);
			#else
				// gi.indirect.diffuse = _GiDiffuse  +(1- _GiDiffuse);
				fixed4 color = UNIVERSAL_PBR_PRECOM(surfaceData, gi.light, gi.indirect);
			#endif

			#if _G_SETTING_UNLIT
				color = albedo;
			#endif

			#if _G_HAIR_RIM
				half3 rim = pow(1 - surfaceData.nv, _RimColor.a) * _RimColor.rgb;
				color.rgb += rim;
			#endif

			#if FRESNELRIM_ON && _G_HAIR_FRESNELRIM				
				float fresnel = ( _FresnelBias + _FresnelIntensity * pow( 1.0 - surfaceData.nv, _FresnelPower ) );
				float4 fresnelMask = clamp(((( _Invert )?( ( 1.0 - fresnel ) ):( fresnel ))) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
				float4 lerpFresnel= lerp( 0 , _FresnelColor , fresnelMask);
				color.rgb += lerpFresnel * _SelfIllumination; 
			#endif

			#if _G_HAIR_DISSOLVE
				float percentage = _DissolveThreshold / metallicTex.b;
				float lerpEdge = sign(percentage - (1 -_DissolveColorRatio) - _DissolveEdgeRatio);
				float lerpOut = sign(percentage - (1 - _DissolveColorRatio));
				fixed3 edgeColor = lerp(_DissolveEdgeColor.rgb, _DissolveColor.rgb, saturate(lerpEdge));
				color.rgb = lerp(color.rgb, edgeColor, saturate(lerpOut));
			#endif

			#if USING_FOG
				UNITY_APPLY_FOG(IN.fogCoord, color.rgb);
			#endif

				return color;
			}

			ENDCG
		}
	
		Pass {
			Tags { "LightMode" = "ForwardBase" }
			Blend SrcAlpha OneMinusSrcAlpha
            Cull back
            ZWrite Off
            ZTest Less
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			sampler2D _TangentMap;
					
			fixed4 frag (v2f IN) : SV_Target {

				half4 metallicTex = tex2D(_MetallicTex, IN.uv01.xy);
			#if _G_HAIR_DISSOLVE
				clip(metallicTex.b - _DissolveThreshold);
			#endif

				fixed4 albedo = tex2D(_MainTex, IN.uv01.xy) * _Color;

				SurfaceOutputStandardAnisotropic o;
				UNITY_INITIALIZE_OUTPUT(SurfaceOutputStandardAnisotropic, o);
				o.Emission = 0.0;
				o.Occlusion = 1.0;
				o.Albedo = albedo;
				o.Metallic = metallicTex.r * _MetallicScale;
				o.Smoothness = metallicTex.a * _GlossScale;
				o.Alpha = albedo.a;
				o.Anisotropy = _Anisotropy;
			#if _G_USE_NORMAL
				fixed3 normalTS = UnpackNormal(tex2D(_BumpMap, IN.uv01.xy));
				o.Normal = normalize(float3(dot(IN.tSpace0, normalTS), 
											dot(IN.tSpace1, normalTS), 
											dot(IN.tSpace2, normalTS)));
			#else
				o.Normal = normalize(float3(IN.tSpace0.z,  IN.tSpace1.z, IN.tSpace2.z));
			#endif
				float3 worldPos = float3(IN.tSpace0.w, IN.tSpace1.w, IN.tSpace2.w);

			#ifndef USING_DIRECTIONAL_LIGHT
				fixed3 lightDir = normalize(UnityWorldSpaceLightDir(worldPos));
			#else
				fixed3 lightDir = _WorldSpaceLightPos0.xyz;
			#endif
				float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));

				float3x3 worldToTangent;
				worldToTangent[0] = float3(1, 0, 0);
				worldToTangent[1] = float3(0, 1, 0);
				worldToTangent[2] = float3(0, 0, 1); 

				float3 tangentTS = tex2D(_TangentMap, IN.uv01.xy);
				float3 tangentTWS = mul(tangentTS, worldToTangent);
				float3 fTangent;
				if (tangentTS.z < 1)
					fTangent = tangentTWS;
				else
					fTangent = float3(IN.tSpace0.x,  IN.tSpace1.x, IN.tSpace2.x);
				o.Tangent = fTangent;
				o.BiTangent = float3(IN.tSpace0.y,  IN.tSpace1.y, IN.tSpace2.y);

				UNITY_LIGHT_ATTENUATION(atten, IN, worldPos)

				UnityGI gi;
				UNITY_INITIALIZE_OUTPUT(UnityGI, gi);
				gi.indirect.diffuse = 0;
				gi.indirect.specular = 0;
				gi.light.color = _LightColor0.rgb;
				gi.light.dir = lightDir;

			#if _G_USE_CUSTOMREALTIMESHADOW
				UNITY_BRANCH
				if(_UseObjectRealtimeShadow > 0.5)
				{
					float3 objectShadowScreenPos = IN.objectShadowScreenPos.xyz / IN.objectShadowScreenPos.w;
					objectShadowScreenPos = objectShadowScreenPos * 0.5 + 0.5;
					float objectRealtimeShadowMap = tex2D(_ObjectShadowMap, objectShadowScreenPos.xy).r;
				#if UNITY_REVERSED_Z
					objectRealtimeShadowMap = 1 - objectRealtimeShadowMap;
				#endif
					half inShadow = step(objectRealtimeShadowMap, objectShadowScreenPos.z);
					half outBox = saturate(dot(step(1, objectShadowScreenPos.xyz), half3(1, 1, 1)) + dot(step(objectShadowScreenPos.xyz, 0), half3(1, 1, 1)));
					inShadow = inShadow - inShadow * outBox;
					gi.light.color *= (1 - inShadow) + (inShadow - inShadow * _ObjectRealtimeShadowIntensity);
				}
			#endif

				UnityGIInput giInput;
				UNITY_INITIALIZE_OUTPUT(UnityGIInput, giInput);
				giInput.light = gi.light;
				giInput.worldPos = worldPos;
				giInput.worldViewDir = worldViewDir;
				giInput.atten = atten;
			#if defined(LIGHTMAP_ON) || defined(DYNAMICLIGHTMAP_ON)
				giInput.lightmapUV = IN.lmap;
			#else
				giInput.lightmapUV = 0.0;
			#endif
			#if UNITY_SHOULD_SAMPLE_SH && !UNITY_SAMPLE_FULL_SH_PER_PIXEL
				//giInput.ambient = IN.sh;
				giInput.ambient = _GiDiffuse * IN.sh + (1 - _GiDiffuse);
			#else
				giInput.ambient.rgb = 0.0;
			#endif
				giInput.probeHDR[0] = unity_SpecCube0_HDR;
				giInput.probeHDR[1] = unity_SpecCube1_HDR;
			#if defined(UNITY_SPECCUBE_BLENDING) || defined(UNITY_SPECCUBE_BOX_PROJECTION)
				giInput.boxMin[0] = unity_SpecCube0_BoxMin; 
			#endif
			#ifdef UNITY_SPECCUBE_BOX_PROJECTION
				giInput.boxMax[0] = unity_SpecCube0_BoxMax;
				giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
				giInput.boxMax[1] = unity_SpecCube1_BoxMax;
				giInput.boxMin[1] = unity_SpecCube1_BoxMin;
				giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
			#endif

				LightingStandardTranslucent_GI(o, giInput, gi);
				SurfaceData surfaceData = GetSurfaceData(o.Albedo, o.Metallic, o.Smoothness, o.Normal, giInput.worldViewDir, gi.light.dir);

			#if _G_HAIR_ANISOTROPICRENDER
				fixed4 color = LightingStandardTranslucent(giInput.worldViewDir, o, gi, surfaceData);
			#else
				fixed4 color = UNIVERSAL_PBR_PRECOM(surfaceData, gi.light, gi.indirect);
			#endif
			
			#if _G_SETTING_UNLIT
				color = albedo;
			#endif			

			#if _G_HAIR_DISSOLVE
				float percentage = _DissolveThreshold / metallicTex.b;
				float lerpEdge = sign(percentage - (1 -_DissolveColorRatio) - _DissolveEdgeRatio);
				float lerpOut = sign(percentage - (1 - _DissolveColorRatio));
				fixed3 edgeColor = lerp(_DissolveEdgeColor.rgb, _DissolveColor.rgb, saturate(lerpEdge));
				color.rgb = lerp(color.rgb, edgeColor, saturate(lerpOut)); 
			#endif

			#if USING_FOG
				UNITY_APPLY_FOG(IN.fogCoord, color.rgb);
			#endif

				return color;
			}

			ENDCG
		}

	}
}
