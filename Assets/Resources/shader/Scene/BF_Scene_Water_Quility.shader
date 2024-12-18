Shader "BF/Scene/Water_Quility"{
    Properties{
        		//Color
		// _ColorTop("顶层颜色",Color) = (0, 0, 0, 0)
		_ColorA("颜色渐变0-A",Color) = (1,1,1,1)
		_ColorB("颜色渐变A-B",Color) = (1,1,1,1)
		_ColorC("颜色渐变B-C",Color) = (1,1,1,1)
		_DepthMul("最大颜色渐变深度", Range(0.0001, 50)) = 1.0
		_MaxFadeDepth("最大透明度渐变深度", Range(0.0001, 50)) = 1.0
		_DepthA("深度渐变A-B", Range(0.001, 0.99)) = 0
		_DepthB("深度渐变B-C", Range(0.001, 0.99)) = 0.5

		[Space]
		//Spec
		_SpecularColor ("高光颜色", Color) = (1,1,1,1)
		_Smoothness ("光滑度", Range(0.01,5)) = 0.5
		
		//Normal Map
		_BumpMap("法线波纹贴图", 2D) = "bump" {}
		_BumpStrength ("法线波纹强度", Range(0,10)) = 1
		_SpeedX("X轴滚动速度",float) = 0.5
		_SpeedY("Y轴滚动速度",float) = 0.5

		[Space]
		//Reflection
		[Toggle(REFLECTION_ON)] _ReflectionEnable("开启反射", Float)  = 0
		[NoScaleOffset]
		_Cube("环境立方体贴图", Cube) = "black" {}
		_CubeColor("环境颜色（RGB：颜色 A：强度）",Color) = (1,1,1,1)
		_Reflection("反射强度", Range(0,2) ) = 1
		_RimPower("菲尼尔角度", Range(1,10) ) = 5
		[Toggle(SCREMSPACEREFLECTION_ON)] _SSR("开启屏幕空间反射", float) = 0
		_SSRMaxSampleCount("SSR Max Sample Count", Range(0, 4)) = 0//Range(0, 64)) = 12
		_SSRSampleStep("SSR Sample Step", Range(4, 32)) = 20//Range(4, 32)) = 16
		_SSRIntensity("SSR Intensity", Range(0, 2)) = 0.5
		_SSRLerp("SSR Lerp", range(0, 1)) = 0.8


		[Space]
		//Foam
		[Toggle(FOAM_ON)] _FoamEnable ("开启泡沫", Float) = 0
		_FoamRippledInstensity("泡沫被波纹扰动强度", range(0, 2)) = 1
		_FoamTex("泡沫纹理", 2D) = "white" {}
		// [Enum(Off, 0, On, 1)] _FoamNoiseEnable("开启随机泡沫", Float) = 0
		// _FoamNoiseTex("噪声贴图", 2D) = "white" {}
		// _FoamNoiseFactory("噪声", float) = 0
		[Enum(Off, 0, On, 1)] _EdgeFoamEnable("开启边缘泡沫", float) = 1
		[HDR]_FoamColor("边缘泡沫颜色", Color) = (1,1,1,1)
		_FoamRange("边缘泡沫范围", Range(0.001, 5)) = 0.5
		_FoamInstensity("边缘泡沫强度", float) = 10
		_FoamSpeed("边缘泡沫流动速度", vector) = (0.01, 0.01, 0, 0)
		[Enum(Off, 0, On, 1)] _ShoreFoamEnable("开启近岸浪花", float) = 0
		_ShoreFoamParams("近岸浪花参数：x:密集程度，y:速度，z:衰减，w:缩放", vector) = (10, 1, 1, 0.5)
		_ShoreFoamNoiseParams("近岸浪花噪声参数：xy:noise1、2速度，zw:noise1、2缩放", vector) = (0.5, -0.1, 0.5, 1)
		_ShoreFoamNoiseOffset("近岸浪花噪声大小：xy:noise1、2大小偏移，zw:noise1、2大小缩放", vector) = (0, 0, 1, 0.2)
		_ShoreFoamNoiseLimit("近岸浪花噪声限制：xy:noise1、2最小值，zw:noise1、2最大值", vector) = (-1, -1, 1, 1)

		[Space]
		//noise
		[Toggle(NOISE_ON)] _NoiseOn("开启波浪", Float) = 0
		_NoiseTex("波浪纹理", 2D) = "white" {}
		_NoiseColor("波浪颜色", Color) = (1, 1, 1, 1)
		_NoiseIntensity("波浪强度", float) = 1
		_NoiseSpeedOffset("波浪速度和偏移", vector) = (0.01, 0.04, 1, 1)
		[Enum(Off, 0, On, 1)] _WaveFoamEnable("开启波浪泡沫", float) = 0
		[HDR]_WaveFoamColor("波浪泡沫颜色", Color) = (1, 1, 1, 1)
		_WaveFoamInstensity("波浪泡沫强度", float) = 1
		_WaveFoamMaskTex("波浪泡沫遮罩", 2D) = "black" {}
		_WaveFoamSpeedOffset("波浪泡沫速度和偏移", vector) = (0, 0, 0, 0)
		_WaveFoamNoiseFactory("波浪泡沫噪声", Range(0, 1)) = 0.3
		_WaveFoamNoiseParams("xy: 波浪泡沫噪声速度, zw: 波浪泡沫噪声淡入范围", vector) = (0, 0, 0.1, 0.1)
		_WaveFoamFalloffStart("波动深度衰减开始深度", Range(0, 50)) = 5
		_WaveFoamFalloffEnd("波动深度衰减结束深度", Range(0, 50)) = 0.5
		// _NoiseDepthFalloffStrength("波动深度衰减强度", Range(0, 10)) = 1

		[Space]
		//add
		[Toggle(_CameraLight_IsOpening)] _CameraLight_IsOpening("开启镜头光", Float) = 1
		[HDR]_CameraLightColorScale("镜头光的强度控制", Color) = (1,1,1,1)
		_CameraLightSmoothness("镜头光系数", Range(0.001,5)) = 0.17 //128
		_CameraLightOffset("镜头光偏移Y",  Range(0.0,10)) = 0.0
		_CameraLightMaxRate("镜头光的高光截断系数", Range(0, 1.0)) = 1.0
    }
    SubShader {
        Tags{"Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True"}

		GrabPass{"_OpaqueTex"}
        Pass{
            Tags{"LightMode" = "ForwardBase"}
            ZWrite Off
            Cull Back
            Blend SrcAlpha OneMinusSrcAlpha
            
            CGPROGRAM

			#define _G_WATER_CAMERA_LIGHT_SPEC 1                 //是否开启镜头高光
			#define _G_WATER_CAMERA_LIGHT_DIFFUSE 0             //是否开启镜头漫反射光
			#define _G_WATER_CameraLightColor half3(0.2, 0.2, 0.2) //镜头光的颜色值

			uniform sampler2D _CameraDepthTexture;

			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "Lighting.cginc"
			#include "../common/BingFengCG.cginc"
			#include  "../common/BingFengCommon.cginc"
            #include "BF_Scene_GlobalQualityVars.cginc"
			#include "../Common/BingFeng_SSR.cginc"

            #pragma vertex vert
            #pragma fragment frag

			#pragma target 3.0
			// #pragma multi_compile_fwdbase
            #pragma multi_compile DIRECTIONAL
			// #pragma multi_compile _ SHADOWS_SHADOWMASK //固定ShadowMask
			#pragma multi_compile SHADOWS_SHADOWMASK
			#pragma multi_compile _ LIGHTMAP_ON
#if !_G_IN_RUNTIME
			#pragma multi_compile _ SHADOWS_SCREEN //不需要使用级联阴影
#endif
            #pragma multi_compile _G_GAME_QUALITY_HIGH _G_GAME_QUALITY_LOW _G_GAME_QUALITY_VERYLOW

			//优化  #pragma multi_compile_fog
			#pragma multi_compile __ FOG_HEIGHT
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma shader_feature_local REFLECTION_ON
			#pragma shader_feature_local SCREMSPACEREFLECTION_ON
			#pragma shader_feature_local FOAM_ON
			#pragma shader_feature_local NOISE_ON
			//add
			#pragma shader_feature_local _CameraLight_IsOpening

			//优化  add
			#pragma skip_variants  LIGHTPROBE_SH DYNAMICLIGHTMAP_ON _SPECULARHIGHLIGHTS_OFF VERTEXLIGHT_ON 
			#pragma skip_variants  UNITY_SPECCUBE_BOX_PROJECTION UNITY_SPECCUBE_BLENDING 
			#pragma skip_variants FOG_EXP FOG_EXP2
			#pragma skip_variants DIRECTIONAL_COOKIE POINT_COOKIE SPOT _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
			//	
			struct a2v {
				float4 vertex 		: POSITION;
				float4 color 		: COLOR;
				float3 normal 		: NORMAL;
				float4 tangent 		: TANGENT;
				float2 texcoord 	: TEXCOORD0;
			};

			struct v2f {
				float4 pos 			: SV_POSITION;
				float4 vertexColor 	: COLOR;
				half4 tspace0 		: TEXCOORD0;
				half4 tspace1 		: TEXCOORD1;
				half4 tspace2 		: TEXCOORD2;
				float3 worldPos 	: TEXCOORD3;
				float4 uv01			: TEXCOORD4;
				UNITY_FOG_COORDS(5)
			#if (defined(FOAM_ON) && _G_WATER_FOAM) || RAIN_ON 
				half4 uv23			: TEXCOORD6;
			#endif
			#if NOISE_ON && _G_WATER_NOISE
				half4 uv45  		: TEXCOORD7;
			#endif
				float4 screenPos  	: TEXCOORD8;

				float3 vertexLight	: TEXCOORD9;
			};

			sampler2D _BumpMap; 
			half4 _BumpMap_ST; 
			float _BumpStrength;
			sampler2D _OpaqueTex;

		#if defined(FOAM_ON) && _G_WATER_FOAM
			half _FoamRippledInstensity;
			sampler2D _FoamTex; 
			half4 _FoamTex_ST; 

			// half _FoamNoiseEnable;
			// sampler2D _FoamNoiseTex;
			// half4 _FoamNoiseTex_ST;
			// half _FoamNoiseFactory;

			half _EdgeFoamEnable;
			half3 _FoamColor; 
			half _FoamRange;

			half _FoamInstensity;
			half4 _FoamSpeed;

			half _ShoreFoamEnable;
			half4 _ShoreFoamParams;
			half4 _ShoreFoamNoiseParams;
			half4 _ShoreFoamNoiseOffset;
			half4 _ShoreFoamNoiseLimit;

			half _WaveFoamEnable;
			sampler2D _WaveFoamMaskTex;
			half4 _WaveFoamMaskTex_ST;
			half _WaveFoamInstensity;
			half3 _WaveFoamColor;
			half4 _WaveFoamSpeedOffset;
			half _WaveFoamNoiseFactory;
			half4 _WaveFoamNoiseParams;
			half _WaveFoamFalloffStart;
			half _WaveFoamFalloffEnd;
		#endif

			samplerCUBE _Cube; half4 _CubeColor;
			float _UseDepthTex;
			half4 _ColorA, _ColorB, _ColorC; 
			half _DepthMul, _DepthA, _DepthB;
			half _MaxFadeDepth;
			half3 _SpecularColor;
			float _Reflection;
			float _SpeedX, _SpeedY;
			float _RimPower;
			half _Smoothness;

			float _SSRMaxSampleCount;
			float _SSRSampleStep;
			half _SSRIntensity;
			half _SSRLerp;

			half  _NoiseRatio, _NoiseIntensity;
			half4 _NoiseColor, _NoiseSpeedOffset;
			sampler2D _NoiseTex; half4 _NoiseTex_ST;
			
		#ifndef LIGHTING_INCLUDED
			uniform fixed4 _LightColor0;
		#endif

		#if defined(_CameraLight_IsOpening) && _G_WATER_CAMERALIGHT
			half4 _CameraLightColorScale;
			half _CameraLightSmoothness;
			half _CameraLightOffset;
			half _CameraLightMaxRate;
		#endif

			half3 _CalcDiffuse(half3 camLightColor, half3 normalWS, float3 viewDir, half3 diffuseColor)
			{
				float3 halfDir = viewDir;// normalize(viewDir + lightDir);
				float3 lightDir = viewDir;
				half dotNL = dot(lightDir, normalWS);
				half NoL = saturate(dotNL);
				half3 diffuse = camLightColor * NoL * diffuseColor;
				return diffuse;
			}

			half3 _CalcCameraLight(half3 camLightColor, half3 normalWS,  float3 viewDir, half specularRate, half3 diffuseColor, half4 cameraLightColorScale)
			{
				camLightColor *= cameraLightColorScale.xyz;

				float3 halfDir = viewDir;
				float3 lightDir = viewDir;			
			#if _G_WATER_CAMERA_LIGHT_DIFFUSE
				half3 diffuse = _CalcDiffuse(camLightColor, normalWS, viewDir, diffuseColor);			
			#else
				half3 diffuse = half3(0, 0, 0);
			#endif

			#if _G_WATER_CAMERA_LIGHT_SPEC
				half3 spec = specularRate * camLightColor.rgb;//
				return diffuse + spec;
			#else
				return diffuse;
			#endif
			}

			half randomNoise(half2 block)
			{
    			return frac(sin(dot(block, half2(12.9898, 78.233))) * 43758.5453);
			}

			half4 randomNoise(half4 blocks)
			{
				blocks = half4(dot(blocks.xy, half2(127.1,311.7)), dot(blocks.xy, half2(269.5, 183.3)),
								dot(blocks.zw, half2(127.1,311.7)), dot(blocks.zw, half2(269.5, 183.3)));
				//block = half2(dot(block, half2(127.1,311.7)), dot(block, half2(269.5, 183.3)));
    			return -1.0 + 2.0 * frac(sin(blocks) * 43758.5453123);
			}

			float2 Noise(float4 uvs)
			{
				float4 i = floor(uvs);
				float4 f = frac(uvs);
				float4 u = smoothstep(0, 1, f);
				half4 tl = randomNoise(i + half4(0.0, 0.0, 0.0, 0.0));
				half4 tr = randomNoise(i + half4(1.0, 0.0, 1.0, 0.0));
				half4 bl = randomNoise(i + half4(0.0, 1.0, 0.0, 1.0));
				half4 br = randomNoise(i + half4(1.0, 1.0, 1.0, 1.0));
				half4 f1 = f - half4(0.0, 0.0, 0.0, 0.0);
				half4 f2 = f - half4(1.0, 0.0, 1.0, 0.0);
				half4 f3 = f - half4(0.0, 1.0, 0.0, 1.0);
				half4 f4 = f - half4(1.0, 1.0, 1.0, 1.0);
				float2 result1 = float2(lerp(dot(tl.xy, f1.xy), dot(tr.xy, f2.xy), u.x), 
										lerp(dot(tl.zw, f1.zw), dot(tr.zw, f2.zw), u.z));
				float2 result2 = float2(lerp(dot(bl.xy, f3.xy), dot(br.xy, f4.xy), u.x), 
										lerp(dot(bl.zw, f3.zw), dot(br.zw, f4.zw), u.z));
				return lerp(result1, result2, u.yw);
			}

			v2f vert(a2v v)
			{
				v2f o;
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_INITIALIZE_OUTPUT(v2f,o);

			#if _G_USE_NORMAL
				o.uv01.xy = TRANSFORM_TEX(v.texcoord, _BumpMap);
				o.uv01.zw = o.uv01.xy * 0.5;//TRANSFORM_TEX(v.texcoord, _BumpMap) * 0.5;
				float4 uvSpeed = float4(_SpeedX, _SpeedY, _SpeedX, _SpeedY);
				uvSpeed.zw *= -0.5;
				o.uv01 += frac(uvSpeed * _Time.y);
			#endif

			#if defined(FOAM_ON) && _G_WATER_FOAM
				o.uv23.xy = TRANSFORM_TEX(v.texcoord, _FoamTex);
				o.uv23.xy += frac(_FoamSpeed.xy * _Time.y);
				o.uv23.zw = TRANSFORM_TEX(v.texcoord, _WaveFoamMaskTex);
				o.uv23.zw += fmod(_WaveFoamSpeedOffset.xy * _Time.y + _WaveFoamSpeedOffset.zw, 1000);
			#endif

				float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
			#if NOISE_ON && _G_WATER_NOISE
				o.uv45.xy = TRANSFORM_TEX(v.texcoord, _NoiseTex);
				o.uv45.xy += frac(_NoiseSpeedOffset.xy * _Time.y + _NoiseSpeedOffset.zw);
				worldPos.y += (tex2Dlod(_NoiseTex, float4(o.uv45.xy, 0, 0)).r) * _NoiseIntensity;
			#endif
				o.pos = UnityWorldToClipPos(worldPos);

				o.worldPos = worldPos;
				half3 worldNormal 	= UnityObjectToWorldNormal(v.normal);
			#if _G_USE_NORMAL
				half3 worldTangent	= UnityObjectToWorldDir(v.tangent.xyz);
				half3 worldBinormal = cross(worldTangent,worldNormal); // 实际上应该要再 * v.tangent.w * unity_WorldTransformParams.w;
				o.tspace0 = half4(worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x);
				o.tspace1 = half4(worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y);
				o.tspace2 = half4(worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z);
			#else
				o.tspace0 = half4(0, 0, worldNormal.x, worldPos.x);
				o.tspace1 = half4(0, 0, worldNormal.y, worldPos.y);
				o.tspace2 = half4(0, 0, worldNormal.z, worldPos.z);
			#endif

				o.vertexLight.rgb = ShadeSH9(half4(worldNormal, 1));
				o.vertexColor = v.color;

				o.screenPos = ComputeScreenPos(o.pos);

			#if USING_FOG
            #if defined(FOG_HEIGHT)
                UNITY_TRANSFER_FOG(o, worldPos);
            #else
                UNITY_TRANSFER_FOG(o, o.pos);
            #endif
			#endif

				return o;
			}

			half4 frag(v2f i) : SV_Target
			{
			#if _G_USE_NORMAL
				half3 tangentNormal = UnpackNormalBlend(tex2D(_BumpMap, i.uv01.xy), tex2D(_BumpMap, i.uv01.zw), _BumpStrength);
				half3 worldNormal = WorldNormal(i.tspace0.xyz, i.tspace1.xyz, i.tspace2.xyz, tangentNormal);
			#else
				half3 tangentNormal = UnpackNormalBlend(half4(0.5, 0.5, 1.0, 1.0), half4(0.5, 0.5, 1.0, 1.0), _BumpStrength);
				half3 worldNormal = normalize(half3(i.tspace0.z, i.tspace1.z, i.tspace2.z));
			#endif

				float3 worldViewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
				fixed3 lightDir = normalize(UnityWorldSpaceLightDir(i.worldPos));
			
				half3 mainColor;
				half foamAlpha = 0;
				half alpha = 1;

				UNITY_BRANCH
				if(_UseDepthTex > 0.5)
				{
					float eyeDepth = LinearEyeDepth(tex2Dproj(_CameraDepthTexture, i.screenPos).r);
					float depth = (eyeDepth - i.screenPos.w);
					alpha = saturate(depth / _MaxFadeDepth);

					float colorDepthPercent = saturate(depth / max(_DepthMul, 0.001));

					half3 depthLayerTop = half3(0, _DepthA, _DepthB);
					half3 depthLayerBottom = half3(_DepthA, _DepthB, 1);
					half3 weights = smoothstep(depthLayerTop, depthLayerBottom, colorDepthPercent);
					mainColor = lerp(lerp(lerp(0, _ColorA, weights.x), _ColorB, weights.y), _ColorC, weights.z);

				#if defined(FOAM_ON) && _G_WATER_FOAM
					half2 foamRippledUV = i.uv23.xy + worldNormal.xz * _FoamRippledInstensity;
					half3 foamTex = tex2D(_FoamTex, foamRippledUV).rgb;
					UNITY_BRANCH
					if(_EdgeFoamEnable > 0.5)
					{
						half depthPercent = saturate(depth / _FoamRange);
						half foamMask = 1 - depthPercent;
						UNITY_BRANCH
						if(_ShoreFoamEnable > 0.5)
						{
							foamMask *= depthPercent * 4;
							float2 noise = Noise((i.worldPos.xzxz + _Time.y * _ShoreFoamNoiseParams.xxyy) *
												_ShoreFoamNoiseParams.zzww);
							//noise.x = (noise.x + noise.y) * 0.5;
							noise = _ShoreFoamNoiseOffset.xy + _ShoreFoamNoiseOffset.zw * noise;
							noise = clamp(noise, _ShoreFoamNoiseLimit.xy, _ShoreFoamNoiseLimit.zw);
							foamMask *= saturate(noise.x);
							foamMask *= saturate(sin(_Time.y * _ShoreFoamParams.y + _ShoreFoamParams.x / _ShoreFoamParams.z * 
												pow((depthPercent + noise.y) * _ShoreFoamParams.w , _ShoreFoamParams.z)));
							//return half4(noise.y, noise.y, noise.y, 1);
						}

						half foamIntensity = _FoamInstensity * foamMask;
						half3 edgeFoamColor = _FoamColor.rgb * foamTex;
						mainColor += edgeFoamColor * foamIntensity;
						foamAlpha += foamIntensity * dot(edgeFoamColor , half3(0.33333, 0.33333, 0.33333));		
					}
					UNITY_BRANCH
					if(_WaveFoamEnable > 0.5)
					{
						half dx = ddx(i.uv23.zw);
						half dy = ddy(i.uv23.zw);
						half2 uvBlock = floor(i.uv23.zw);
						half2 noiseUV = half2(randomNoise(half2(uvBlock.x, 114)), randomNoise(half2(514, uvBlock.y))) * _Time.y * _WaveFoamNoiseParams.xy;
						half2 noiseBlockFadeRadiu = clamp(_WaveFoamNoiseParams.zw, 0.001, 0.5);
						half2 noiseFade = saturate((0.5 - abs(frac(i.uv23.zw) - 0.5)) / noiseBlockFadeRadiu);
						noiseFade = smoothstep(0, 1, noiseFade);
						half noiseMask = smoothstep(1 - _WaveFoamNoiseFactory, 1, randomNoise(uvBlock)) * noiseFade.x * noiseFade.y;
						half waveFoamMask = tex2D(_WaveFoamMaskTex, i.uv23.zw + noiseUV, dx, dy).r * noiseMask;
						half waveFoamIntensity = waveFoamMask * _WaveFoamInstensity; 
						waveFoamIntensity *= saturate((depth - _WaveFoamFalloffEnd) / (_WaveFoamFalloffStart - _WaveFoamFalloffEnd));
						half3 waveFoamColor = _WaveFoamColor * foamTex;
						foamAlpha += waveFoamIntensity * dot(waveFoamColor, half3(0.33333,0.33333,0.33333)); //(waveFoamColor.r + waveFoamColor.g + waveFoamColor.b) / 3;
						mainColor += waveFoamIntensity * waveFoamColor;
					}
					alpha = lerp(alpha, 1, saturate(foamAlpha));
				#endif
				}
				else
				{
					mainColor = i.vertexColor.rgb;
					alpha = i.vertexColor.a;
				}
				fixed NdotL = max(0, dot(worldNormal,lightDir));
				half3 reflectColor = 0;
			#if _G_WATER_REFRECT
				half3 worldReflection = normalize(reflect(-worldViewDir, worldNormal));
				half NdotV = saturate(dot(worldNormal, worldViewDir));
				half fresnel = saturate(pow(1 -NdotV, _RimPower));
				#if REFLECTION_ON
					half3 cubeMap = texCUBE(_Cube, worldReflection).rgb * _CubeColor.rgb;
					reflectColor = lerp(unity_IndirectSpecColor.rgb, cubeMap, fresnel * _Reflection);
				#endif
				#if SCREMSPACEREFLECTION_ON
					// float3 uvz = SSRRayMarch(i.pos, i.worldPos, worldReflection, _SSRSampleStep, _SSRMaxSampleCount);
					float3 uvz = GetSSRUVZ(i.pos, i.worldPos, worldReflection, NdotV, i.screenPos.xy / i.screenPos.w, _SSRSampleStep, _SSRMaxSampleCount);
					float3 ssrColor = tex2D(_OpaqueTex, uvz.xy) * _SSRIntensity;
					reflectColor = lerp(reflectColor, ssrColor, saturate(uvz.z * _SSRLerp));
					// return half4 (reflectColor, 1);
				#endif
				// float perceptualRoughness = SmoothnessToPerceptualRoughness(_Smoothness);
				// float roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
				// reflectColor *= _SpecularColor * (1.0 - 0.28 * perceptualRoughness * roughness);
			#endif

				half3 diffuseColor = mainColor;
				half3 diffuse = mainColor * max(i.vertexLight.rgb, NdotL * _LightColor0.rgb);
			#if _G_SETTING_UNLIT
				diffuse = mainColor;
			#endif
			#if _G_SETTINT_VERYLOW
				half3 specular = 0;
			#else
				half specularRate = SpecularRate(_Smoothness, lightDir, worldViewDir, worldNormal);
				half3 specular = specularRate * _LightColor0.rgb  * _SpecularColor.rgb;
			#endif
				//add camera light 
				half3 camLight = half3(0, 0, 0);
			#if defined(_CameraLight_IsOpening) && _G_WATER_CAMERALIGHT
				//float pw = max(0.001,(200 - _CameraLightPow * 200));//_Smoothness
				half3 cameraLightDir = SafeNormalize(worldViewDir + half3(0, _CameraLightOffset, 0));
				half cameraSpecularRate = SpecularRate(_CameraLightSmoothness, cameraLightDir, worldViewDir, worldNormal);
				cameraSpecularRate = min(cameraSpecularRate, _CameraLightMaxRate);
				camLight = _CalcCameraLight(_G_WATER_CameraLightColor, worldNormal, worldViewDir, cameraSpecularRate, diffuseColor, _CameraLightColorScale);
			#endif

				half4 color = half4(diffuse + specular + camLight + reflectColor, alpha);

			#if USING_FOG
				UNITY_APPLY_FOG(i.fogCoord, color.rgb);
			#endif

			return color;
			}

            ENDCG
        }
    }

    // CustomEditor "InspectorShader_BF_Scene_Water"
    Fallback Off
}
