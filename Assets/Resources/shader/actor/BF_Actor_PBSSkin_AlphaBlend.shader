/*
* Copyright (C) 2016, BingFeng Studio（冰峰工作室）.
* All rights reserved.
* 
* 文件名称：BF_Actor_PBSSkin
* 创建标识：引擎组
* 创建日期：2020/11/12
* 文件简述：
*/
Shader "BF/Actor/PBSSkin_AlphaBlend"
{
    Properties{
		//Setting
		_Mode ("模式", Float) = 0
		[Enum(UnityEngine.Rendering.BlendMode)] _BlendSrcColor ("源颜色混合比例", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)]_BlendDstColor ("混合颜色混合比例", Float) = 10
		[Enum(UnityEngine.Rendering.CullMode)]_CullMode ("剔除模式", Float) = 2
		[Enum(Off, 0, On, 1)]_ZWrite ("深度写入", Float) = 1
		//Base
        _MainTex("主纹理", 2D)                 = "white" {}
        _Color("混合颜色", Color)              = (1, 1, 1, 1)      
        _BumpMap("法线纹理", 2D)               = "bump" {}
		[Enum(None , 0 , FlowMask , 1)]_BumpMapAlpha("法线纹理A通道,0不起作用,1流光度" ,float ) = 0
        _MetallicTex("金属纹理,金属度(R),自发光(G),消融噪音(B),光滑度(A)", 2D) = "black" {}
		_MetallicScale ("金属度", Range(0,1))             = 1
        _GlossScale("光滑度", Range(0, 1))                = 1
		//LOD MID
		[HDR]_SpecularColor("高光颜色", Color) = (0, 0, 0, 1)
		_SpecularScale("高光倍数", Float) = 1.0
		_Gloss("高光范围", Range(0.01, 200)) = 20
		_Smooth("光滑度",Range(-1,5)) = 0.5
		// LOD LOW
		_MagicColor("低配颜色",Color) =(1,1,1,1)
    	//Alpha		
        _Alpha("透明度",Range(0.001,1)) = 0.1
		//Hollow
		[Toggle(HOLLOW_ON)]_HollowOn("开启镂空", float) = 0
		_HollowNumerator("镂空分子", float) = 0
		_HollowDenominator("镂空分母", float) = 16
		//Emission
		[Toggle(EMISSION_ON)]_EmissionOn("开启自发光", float) = 0
        [HDR]_Emission("自发光颜色", Color)                      = (0,0,0,0)
        _EmissionFlashSpeed("闪烁速度", Range(0,10))             = 1
        _EmissionFlashMinValue("波动最小值", Range(0,1))         = 0.5
        //Rim
		[Toggle(RIM_ON)]_RimOn("开启边缘光", float) = 0
		[HDR]_RimColor("边缘光颜色", Color) = (1, 1, 1, 1)
		_RimPow("边缘光强度", float) = 1
		//Dissolve
		[Toggle(DISSOLVE_ON)]_DissolveOn("开启消融", float) = 0
        [HDR]_DissolveColor("消融颜色", Color) = (0, 0, 0, 0)
        [HDR]_DissolveEdgeColor("消融边缘颜色", Color) = (1, 1, 1, 1)
		_DissolveAlphaSpeed("消融渐变速度", float) = 0
        _DissolveThreshold("消融阈值", Range(0, 1)) = 0
        _DissolveColorRatio("消融颜色总占比(对整个角色占比)", Range(0, 1)) = 0.7
        _DissolveEdgeRatio("消融外边缘颜色占比(对消融颜色占比)", Range(0, 1)) = 0.7
		//Flow Light
		[Toggle(FLOW_LIGHT_ON)]_FlowLightOn("开启流光", float) = 0
		_FlowLightTex("流光纹理(RGB),流光强度(A)", 2D) = "white" {}
		[HDR]_FlowLightColor("流光颜色", Color) = (1,1,1,1)
		_FlowLightMaskRange("流光遮罩范围", Range(0, 1)) = 1
		_FlowLightXSpeed("流光X方向速度",  Range(-1, 1)) = 0
		_FlowLightZSpeed("流光Z方向速度", Range(-1, 1)) = 0
		_GiDiffuse("天光开关",range(0,1)) = 1


		//[HDR]_FresnelColor("菲涅尔颜色", Color) = (1,1,1,1)
		_FresnelScale2("菲涅尔倍数", Range(0, 1)) = 0
		//_FresnelBias("菲涅尔范围", Range(0, 2)) = 0

		[Toggle(_CameraLight_IsOpening)] _CameraLight_IsOpening("开启镜头光", Float) = 1
		[HDR]_CameraLightColorScale("镜头光的强度控制", Color) = (1,1,1,1)
    	
	    [Toggle(RIM2_ON)] _Rim2On("开启边缘光2",Float) =0
	    [Space(20)]
		[HDR]_RimColor2("边缘光颜色2", Color) = (1,1,1,1)
		[HDR]_RimSecondColor2("边缘光颜色3", Color) = (0.28,0.38,0.93,1)
		_RimWidth2("边缘光宽度", Range(0, 3)) = 0
    	_RimPow2("边缘光Pow", Range(1, 4)) = 2
    	
    	[Toggle(FRESNELRIM_ON)] FRESNEL_ON("开启FresnelRim",Float) =0
    	[Space(15)][Header(Fresnel Properties)][Space(15)]_FresnelColor("Color", Color) = (0.6933962,1,0.9814353,1)
    	_SelfIllumination("Self Illumination", Range( 1 , 10)) = 1		
		[Space(15)]_FresnelIntensity("Fresnel Intensity", Range( 0 , 200)) = 4
		_FresnelPower("Fresnel Power", Float) = 4
		_FresnelBias("Bias", Range( 0 , 1)) = 0
		[Toggle]_Invert("Invert", Float) = 0
		 [Toggle(ROLLTEX_ON)] ROLLTEX_ON("开启RollTex",Float) =0    	
		[HDR] _TintRColor("Tint R Color", Color) = (0.5,0.5,0.5,0.5)
		[HDR] _TintGColor("Tint G Color", Color) = (0.5,0.5,0.5,0.5)
		[HDR] _TintBColor("Tint B Color", Color) = (0.5,0.5,0.5,0.5)
		[HDR] _TintAColor("Tint A Color", Color) = (0.5,0.5,0.5,0.5)
		_RollTex("Roll Texture", 2D) = "black" {}		
		_RollMaskTex("Roll Mask Texture", 2D) = "white" {}
    	_RollSpeedX("Roll Speed X", Vector) = ( 1, 1, 1, 1)
    	_RollSpeedY("Roll Speed Y", Vector) = ( 1, 1, 1, 1)

        [Toggle] _UseObjectRealtimeShadow("使用物体实时阴影", float) = 1
		_ObjectRealtimeShadowIntensity("物体实时阴影强度", Range(0, 1)) = 1
        // [Toggle] _UseRoleRealtimeShadow("使用角色实时阴影", float) = 0
        // _RoleRealtimeShadowIntensity("角色实时阴影强度", Range(0, 1)) = 1
    }
	//高配
    SubShader{
		LOD 300

        Tags {"RenderType"="TransparentCutout" "Queue"="Transparent" "RenderTag"="Actor"}

        Pass{
			Tags {"LightMode" = "ForwardBase"}  
			ZWrite [_ZWrite]
			ZTest LEqual
        	//Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
			Blend [_BlendSrcColor] [_BlendDstColor]
			//Cull Off //此项目临时改动：美术没给斗篷等做双面
			Cull [_CullMode]        	

            CGPROGRAM

				#include "UnityCG.cginc"
				#include "Lighting.cginc"
				#include "AutoLight.cginc"
				#include "UnityStandardBRDF.cginc"
				#include "../Common/BingFengCG.cginc"
				//#include "../Scene/BF_Scene_GlobalQualityVars.cginc"
				//#include "../Common/BingFengStandardCore.cginc"
				//#include "../Common/BingFengStandardBRDF.cginc"
				#include "../Scene/BF_Scene_LightingCommon.cginc"
				#include "../Common/CameraLight.hlsl"   

				#pragma target 3.0
				//优化  #pragma multi_compile_fog
				#pragma multi_compile _ FOG_HEIGHT //FOG_LINEAR
            	#pragma multi_compile _G_GAME_QUALITY_HIGH _G_GAME_QUALITY_LOW _G_GAME_QUALITY_VERYLOW
            	// #pragma multi_compile_fwdbase
				// #pragma multi_compile DIRECTIONAL
				// #pragma multi_compile _ SHADOWS_SHADOWMASK //固定ShadowMask
				#pragma multi_compile SHADOWS_SHADOWMASK
				#pragma multi_compile _ LIGHTMAP_ON
#if !_G_IN_RUNTIME
				#pragma multi_compile _ SHADOWS_SCREEN //不需要使用级联阴影
#endif
				//优化  #pragma multi_compile __ _DEBUG_NO_PBR
				//#pragma skip_variants VERTEXLIGHT_ON LIGHTPROBE_SH SHADOWS_SCREEN
				//#pragma shader_feature CLIP_ON
				//优化 #pragma shader_feature_local RIM_ON
				//优化 #pragma shader_feature_local EMISSION_ON
				#pragma shader_feature_local DISSOLVE_ON
				#pragma shader_feature_local HOLLOW_ON
				#pragma shader_feature_local FLOW_LIGHT_ON
				#pragma shader_feature_local _CameraLight_IsOpening
				//优化 #pragma shader_feature_local RIM2_ON
				#pragma multi_compile __ FRESNELRIM_ON
				#pragma shader_feature_local ROLLTEX_ON

		//优化  add
				#pragma skip_variants  LIGHTPROBE_SH DYNAMICLIGHTMAP_ON DIRLIGHTMAP_COMBINED _SPECULARHIGHLIGHTS_OFF VERTEXLIGHT_ON 
				#pragma skip_variants  UNITY_SPECCUBE_BOX_PROJECTION UNITY_SPECCUBE_BLENDING 
				#pragma skip_variants FOG_EXP FOG_EXP2
				#pragma skip_variants DIRECTIONAL_COOKIE POINT_COOKIE SPOT _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

				#pragma vertex   vert
				#pragma fragment frag   

				struct a2v{
					float4 vertex   	: POSITION;
					float3 normal    	: NORMAL;
					float4 tangent   	: TANGENT;
					half2 uv0      		: TEXCOORD0;
					half2 uv1      		: TEXCOORD1;         
				};

				struct v2f{
					float4 pos      	: SV_POSITION;
					float4 uv       	: TEXCOORD0;     
					float4 tspace0 		: TEXCOORD1;
					float4 tspace1 		: TEXCOORD2;
					float4 tspace2 		: TEXCOORD3;
					float3 worldViewDir : TEXCOORD4;
					UNITY_LIGHTING_COORDS(5, 6)
				#if USING_FOG
					UNITY_FOG_COORDS(7)
				#endif
				#if FLOW_LIGHT_ON
					float4 worldModelPos : TEXCOORD8;
				#endif
					half3 vertexAmbient : TEXCOORD9;
				#if _G_USE_CUSTOMREALTIMESHADOW
					float4 objectShadowScreenPos  : TEXCOORD10;
					// float4 roleShadowScreenPos  : TEXCOORD11;
				#endif
				};

				//PBS
				sampler2D _MainTex; half4 _MainTex_ST;
				sampler2D _BumpMap; half4 _BumpMap_ST;
				half _BumpMapAlpha;
				sampler2D _MetallicTex; half4 _MetallicTex_ST;
				half4 _Color; half _GlossScale, _MetallicScale;  
				//Emission
				half _EmissionOn;
				float4 _TimeEditor; half4 _Emission; half _EmissionFlashSpeed, _EmissionFlashMinValue;
				//Dissolve
				half4 _DissolveColor, _DissolveEdgeColor; half _DissolveThreshold, _DissolveColorRatio, _DissolveEdgeRatio, _DissolveAlphaSpeed;
				//Rim
				half _RimOn;
				half4 _RimColor; half _RimPow;
				fixed _GiDiffuse;
			#if HOLLOW_ON
				half _HollowNumerator, _HollowDenominator;
			#endif

			//#if CLIP_ON
				half _Alpha;
			//#endif

			#if FLOW_LIGHT_ON
				sampler2D _FlowLightTex;half4 _FlowLightTex_ST; half4 _FlowLightColor; half _FlowLightMaskRange;
				float _FlowLightXSpeed, _FlowLightZSpeed;
			#endif

			#if _G_CAMERA_LIGHT && defined(_CameraLight_IsOpening)
				half4 _CameraLightColorScale;
			#endif
            
			#if _G_ACTOR_RIM2
				half _Rim2On;
				half4 _RimColor2;
				half  _RimWidth2;	
				half4 _RimSecondColor2;
				half  _RimPow2;
            #endif


            #if ROLLTEX_ON
	            sampler2D _RollTex;				
				sampler2D _RollMaskTex;
				float4 _RollTex_ST;
				float4 _RollSpeedX;
				float4 _RollSpeedY;
            
				half4 _TintRColor;
				half4 _TintGColor;
				half4 _TintBColor;
				half4 _TintAColor;
            #endif

            #if FRESNELRIM_ON
				half4 _FresnelColor;
				half _SelfIllumination;
				half _Invert;
				half _FresnelBias;
				half _FresnelIntensity;
				half _FresnelPower;
            #endif
            
            
				//half4 _FresnelColor;
				half _FresnelScale2;
				//half _FresnelBias;

            #if _G_USE_CUSTOMREALTIMESHADOW
				half _UseObjectRealtimeShadow;
				half _ObjectRealtimeShadowIntensity;
				sampler2D _ObjectShadowMap;
            #endif

				v2f vert(a2v v){
					v2f o;

					o.pos = UnityObjectToClipPos(v.vertex);     
					o.uv.xy = TRANSFORM_TEX(v.uv0,_MainTex);
					o.uv.zw = TRANSFORM_TEX(v.uv0,_MetallicTex);			

					float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
					o.worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));

					float3 worldNormal = UnityObjectToWorldNormal(v.normal);
				#if _G_USE_NORMAL
					float3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
					half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
					float3 worldBitangent = cross(worldNormal, worldTangent) * tangentSign;
					o.tspace0 = float4(worldTangent.x, worldBitangent.x, worldNormal.x, worldPos.x);
					o.tspace1 = float4(worldTangent.y, worldBitangent.y, worldNormal.y, worldPos.y);
					o.tspace2 = float4(worldTangent.z, worldBitangent.z, worldNormal.z, worldPos.z);
				#else
					o.tspace0.xyz = worldNormal;
					o.tspace1.xyz = worldPos;
				#endif

					UNITY_TRANSFER_LIGHTING(o, v.uv1);

				#if USING_FOG
				#if defined(FOG_HEIGHT)
					UNITY_TRANSFER_FOG(o, worldPos);
				#else
					UNITY_TRANSFER_FOG(o, o.pos);
				#endif
				#endif

				#if FLOW_LIGHT_ON & _G_ACTOR_FLOWLIGHT
					o.worldModelPos = mul(unity_ObjectToWorld, half4(0,0,0,1));
				#endif

					o.vertexAmbient = ShadeSHPerVertex (worldNormal, 0);

				#if _G_USE_CUSTOMREALTIMESHADOW
					o.objectShadowScreenPos = mul(_ShadowMatrix, float4(worldPos.xyz, 1));
				#endif

					return o;
				}

				float4 frag(v2f i) : SV_Target{
					
				#if HOLLOW_ON && _G_ACTOR_HOLLOW
					clip(i.pos.x % _HollowDenominator - _HollowNumerator);
					clip(i.pos.y % _HollowDenominator - _HollowNumerator);
				#endif

					half4 mainTex  = tex2D(_MainTex, i.uv.xy);
#ifdef _DEBUG_NO_PBR
					return mainTex;
#endif
				// #if CLIP_ON
					//clip(mainTex.a - _ClipAlpha);
				// #endif

					half4 metallicTex = tex2D(_MetallicTex, i.uv.zw);
				#if DISSOLVE_ON && _G_ACTOR_DISSOLVE
					clip(metallicTex.b - _DissolveThreshold);
				#endif

					i.worldViewDir = normalize(i.worldViewDir);
				#if _G_USE_NORMAL
					float3 worldPos = float3(i.tspace0.w, i.tspace1.w, i.tspace2.w);
					float4 bumpTex = tex2D(_BumpMap, i.uv.xy);
					float3 bump = UnpackNormal(bumpTex); 
					float3 worldNormal = normalize(half3(dot(i.tspace0.xyz, bump), dot(i.tspace1.xyz, bump), dot(i.tspace2.xyz, bump)));
				#else
					float3 worldPos = i.tspace1.xyz;
					float4 bumpTex = float4(0, 0, 0, 1);
					float3 worldNormal = normalize(i.tspace0.xyz);
				#endif	

					half4 albedo = mainTex * _Color;		
					half metallic = metallicTex.r * _MetallicScale;
					half smoothness = metallicTex.a * _GlossScale;
					UNITY_LIGHT_ATTENUATION(atten, i, worldPos);

					UnityLight light;
					light.color = _LightColor0;
					light.dir = normalize(UnityWorldSpaceLightDir(worldPos));

				#if _G_USE_CUSTOMREALTIMESHADOW
					UNITY_BRANCH
					if(_UseObjectRealtimeShadow > 0.5)
					{
						float3 objectShadowScreenPos = i.objectShadowScreenPos.xyz / i.objectShadowScreenPos.w;
						objectShadowScreenPos = objectShadowScreenPos * 0.5 + 0.5;
						float objectRealtimeShadowMap = tex2D(_ObjectShadowMap, objectShadowScreenPos.xy).r;
					#if UNITY_REVERSED_Z
						objectRealtimeShadowMap = 1 - objectRealtimeShadowMap;
					#endif
						half inShadow = step(objectRealtimeShadowMap, objectShadowScreenPos.z);
						half outBox = saturate(dot(step(1, objectShadowScreenPos.xyz), half3(1, 1, 1)) + dot(step(objectShadowScreenPos.xyz, 0), half3(1, 1, 1)));
						inShadow = inShadow - inShadow * outBox;
						light.color *= (1 - inShadow) + (inShadow - inShadow * _ObjectRealtimeShadowIntensity);
					}
				#endif

					SurfaceData surfaceData = GetSurfaceData(albedo.xyz, metallic, smoothness, worldNormal, i.worldViewDir, light.dir);
					surfaceData.diffuseColor *= atten;
					surfaceData.specularColor *= atten;

					UnityIndirect gi;
					gi.diffuse = _GiDiffuse * ShadeSHPerPixel(worldNormal, i.vertexAmbient, worldPos)+(1- _GiDiffuse);

				#if _G_ACTOR_REFLECT
					Unity_GlossyEnvironmentData glossIn;
					glossIn.roughness = surfaceData.perceptualRoughness;
					glossIn.reflUVW = surfaceData.refDir;
					half3 env0 = Unity_GlossyEnvironment(UNITY_PASS_TEXCUBE(unity_SpecCube0), unity_SpecCube0_HDR, glossIn);
					gi.specular = lerp(unity_IndirectSpecColor.rgb, env0, metallic);
				#else
					gi.specular = 0;
				#endif


					// half4 color = UNIVERSAL_PBR(diffColor, specColor, oneMinusReflectivity, smoothness, worldNormal, i.worldViewDir, light, gi);
					half4 color = UNIVERSAL_PBR_PRECOM(surfaceData, light, gi);
				#if _G_SETTING_UNLIT
					color = albedo;
				#endif
				#if _G_CAMERA_LIGHT && defined(_CameraLight_IsOpening) && _G_ACTOR_CAMERALIGHT
					half fTerm = saturate(smoothness + (1 - surfaceData.oneMinusReflectivity));
					half3 cameraLightColor = APPLY_CAMERA_LIGHT(worldNormal, fTerm, i.worldViewDir, surfaceData.roughness, surfaceData.diffuseColor, _CameraLightColorScale);
					color.rgb += cameraLightColor;
				#endif

				#if _G_ACTOR_EMISSION
					UNITY_BRANCH
					if(_EmissionOn > 0.5)
					{
						half4 time = _Time + _TimeEditor;
						half emissionMask = metallicTex.g * _Emission.a;
						half3 emission = emissionMask * _Emission.rgb * albedo.rgb * (clamp(sin( fmod(UNITY_PI * time.y * _EmissionFlashSpeed, UNITY_PI)), _EmissionFlashMinValue, 1));
						color.rgb += emission;
					}
				#endif

				#if _G_ACTOR_RIM
					UNITY_BRANCH
					if(_RimOn > 0.5)
					{
						half3 rim = pow(1 - surfaceData.nv, _RimPow) * _RimColor.rgb;
						color.rgb += rim;
					}
				#endif
				#if _G_ACTOR_RIM2
					UNITY_BRANCH
					if(_Rim2On > 0.5)
					{
						half nv1 = saturate(pow(1.0 - surfaceData.nv, _RimPow2));
						half3 _Rim = _RimWidth2 * lerp(_RimSecondColor2.rgb, _RimColor2.rgb, nv1);
						color.rgb += _Rim; 
					}
				#endif

					float fade = 1.0;
				#if DISSOLVE_ON && _G_ACTOR_DISSOLVE
					float percentage = _DissolveThreshold / metallicTex.b;
					float lerpEdge = sign(percentage - (1 -_DissolveColorRatio) - _DissolveEdgeRatio);
					float lerpOut = sign(percentage - (1 - _DissolveColorRatio));
					fixed3 edgeColor = lerp(_DissolveEdgeColor.rgb, _DissolveColor.rgb, saturate(lerpEdge));
					color.rgb = lerp(color.rgb, edgeColor, saturate(lerpOut)); 

					fade = lerp(saturate(1.0 - (_DissolveThreshold * _DissolveAlphaSpeed)), 1.0, saturate(lerpOut));
				#endif

				#if FLOW_LIGHT_ON &&  _G_ACTOR_FLOWLIGHT
					half flowLightMask = 1;
					if(_BumpMapAlpha > 0.5f && bumpTex.a < 0.9f)
					{
						flowLightMask = 0;
					}
					flowLightMask = saturate(flowLightMask * _FlowLightMaskRange);
					float2 flowLightUV = ( worldPos.xz - i.worldModelPos.xz) * _FlowLightTex_ST.xy + _FlowLightTex_ST.zw + float2(frac(_Time.y * _FlowLightXSpeed), frac(_Time.y * _FlowLightZSpeed));
					half4 flowLightTex = tex2D(_FlowLightTex, flowLightUV);
					half3 flowLight = flowLightMask * flowLightTex.a * _FlowLightColor.a * flowLightTex.rgb * _FlowLightColor.rgb;
					color.rgb += flowLight;
				#endif


				#if FRESNELRIM_ON  && _G_ACTOR_FRESNELRIM
					float fresnel = ( _FresnelBias + _FresnelIntensity * pow( 1.0 - surfaceData.nv, _FresnelPower ) );
					float4 fresnelMask = clamp( ( (( _Invert )?( ( 1.0 - fresnel ) ):( fresnel ))) , float4( 0,0,0,0 ) , float4( 1,1,1,0 ) );
					float4 lerpFresnel= lerp( 0 , _FresnelColor , fresnelMask);
					color.rgb += lerpFresnel * _SelfIllumination; 
				#endif

				#if ROLLTEX_ON && _G_ACTOR_ROLLTEX
					half4 rollAlphaColor = tex2D(_RollMaskTex, i.uv.xy);
				
					float4 uvOffsetX = _Time.x * _RollSpeedX;
					float4 uvOffsetY = _Time.x * _RollSpeedY;

					half2 uvROffset = i.uv.xy + half2(uvOffsetX.x, uvOffsetY.x);
					half2 uvGOffset = i.uv.xy + half2(uvOffsetX.y, uvOffsetY.y);
					half2 uvBOffset = i.uv.xy + half2(uvOffsetX.z, uvOffsetY.z);
					half2 uvAOffset = i.uv.xy + half2(uvOffsetX.w, uvOffsetY.w);

					half4 rollRColor = tex2D(_RollTex, TRANSFORM_TEX(uvROffset, _RollTex));
					half4 rollGColor = tex2D(_RollTex, TRANSFORM_TEX(uvGOffset, _RollTex));
					half4 rollBColor = tex2D(_RollTex, TRANSFORM_TEX(uvBOffset, _RollTex));
					half4 rollAColor = tex2D(_RollTex, TRANSFORM_TEX(uvAOffset, _RollTex));

					rollRColor = _TintRColor * half4(rollRColor.r, rollRColor.r, rollRColor.r, rollAlphaColor.r);
					rollGColor = _TintGColor * half4(rollGColor.g, rollGColor.g, rollGColor.g, rollAlphaColor.g);
					rollBColor = _TintBColor * half4(rollBColor.b, rollBColor.b, rollBColor.b, rollAlphaColor.b);
					rollAColor = _TintAColor * half4(rollAColor.a, rollAColor.a, rollAColor.a, rollAlphaColor.a);

					//Blend SrcAlpha One
					color.rgb = color.rgb + rollRColor.rgb * rollRColor.a + rollGColor.rgb * rollGColor.a + rollBColor.rgb * rollBColor.a + rollAColor.rgb * rollAColor.a ;
				#endif
					
				#if USING_FOG
					UNITY_APPLY_FOG(i.fogCoord, color.rgb);
				#endif				
					float alpha = fade * (mainTex.a * _Alpha - _FresnelScale2);
					return half4(color.rgb, alpha) + half4(_FresnelScale2, _FresnelScale2, _FresnelScale2, 0);
				}
            ENDCG
        }

		UsePass "BF/Shadow/ShadowCaster/COMMON"
    }
	//中配
	//SubShader{
	//	LOD 300
	//	Tags { "RenderType" = "Opaque" "Queue" = "Geometry+10" "RenderTag" = "Actor" }

	//	UsePass "BF/Actor/EXPSkin/EXPSKIN_BASE"
	//	UsePass "BF/Shadow/ShadowCaster/COMMON"
	//}

	//低配
	//SubShader{
	//	LOD 200
	//	Tags {"RenderType" = "Opaque" "IgnoreProjector" = "true" "Queue" = "Geometry+10" "RenderTag" = "Actor"}

	//	UsePass "BF/Actor/Albedo/BASE"
	//}


	//CustomEditor "InspectorShader_BF_Actor_PBSSkin"
	Fallback Off
}