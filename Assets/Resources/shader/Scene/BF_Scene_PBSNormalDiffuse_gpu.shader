
Shader "BF/Scene/PBSNormalDiffuse"
{
    Properties {
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("源混合模式", Float) = 1.0
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("目标混合模式", Float) = 0.0
		[Enum(on, 1, off , 0)]_ZWrite("ZWrite", Float) = 1.0
		//PBS
        _MainTex("主纹理", 2D) = "white" {}
		[Enum(Alpha, 0, AO, 1 , None , 2)]_MainTexAlphaType("主纹理A通道类型" , float) = 0
        _Color("混合颜色", Color) = (1,1,1,1)
        [NoScaleOffset]_BumpMap ("法线贴图", 2D) = "bump" {}  
		_BumpMapScale("法线增强", Range(0.01,10)) = 1.0
        [NoScaleOffset]_MetallicTex("金属高光纹理,R:金属度|G:自发光|B:反射遮罩|A:光滑度",2D) = "black" {}
        _MetallicScale("金属度", Range(0, 2)) = 0.5	//美术需要的超出物理的临时改动
        _GlossScale("光滑度", Range(0, 2)) = 0.5    //美术需要的超出物理的临时改动   
		[HDR]_Specular("高光颜色", Color) = (1, 1, 1, 1)

		//R =未定义, G = Ice Thickness, B = (ICE Rim Mask), A = 未定义
        [Toggle(_HasExtendMap)]_HasExtendMap("使用扩展贴图?", Float) = 0.0
        [NoScaleOffset]_ExtendMap("扩展贴图(R:未定义|G:厚薄度|B:Rim掩码| A:未定义)", 2D) = "white" {}  
		
		//开启透明度混合
		[Toggle(Blend_ON)] _Blend_ON("开启透明度混合", Float) = 0

		//Clip 
		[Toggle(CLIP_ON)] _ClipOn("开启剔除", Float) = 0
		_ClipAlpha("剔除Alpha阈值",Range(0.001,1)) = 0.1
		//Hollow
		[Toggle(HOLLOW_ON)]_HollowOn("开启镂空", float) = 0
		_HollowRate("镂空率", Range(0, 1)) = 0
		//Emission		
		[Enum(Off, 0, On, 1)]_EmissionOn("开启自发光", Float) = 0 //优化  [Toggle(EMISSION_ON)] _EmissionOn("开启自发光", Float) = 0
		[HDR]_Emission("自发光颜色", Color) =  (0,0,0,0)
		_EmissionStart("自发光基础值", Range(0, 0.5)) = 0
        _EmissionSpeed("自发光闪烁速度",Range(0,20)) = 1

		//Scene Reflection 
		[Toggle(REFLECT_ON)] _ReflectOn("开启反射",Float) =0
		_ReflectMetallic("反射面金属度" , Range(0, 2)) = 1
		_ReflectSmoothness("反射面光滑度" , Range(0, 2)) = 1
		//Mirror Reflection
		[Toggle(MIRROR_REFLECTION_ON)] _MirrorReflectionOn("开启镜面反射",Float) = 0
		_MirrorReflectionTex("镜面反射贴图", 2D) = "" {}
		_MirrorNormal("镜面法线", Vector) = (0,1,0,0)
		_MirrorReflectionScale("镜面反射强度", Range(0, 2)) = 1
		_FresnelExp("菲尼尔指数", Range(0, 5)) = 1

		_GiDiffuse("天光开关",range(0,1)) = 1

		[Toggle(_CameraLight_IsOpening)] _CameraLight_IsOpening("开启镜头光", Float) = 1
		[HDR]_CameraLightColorScale("镜头光的强度控制", Color) = (1,1,1,1)
		//_MinMetallic("金属度阀值", Range(0, 1)) = 0.0
		_CameraLightTrans("镜头光变换", Range(0.1, 50)) = 2.0
		_CameraLightMin("镜头光阀值", Range(0.0, 1.0)) = 0.1
		_CameraLightOffset("镜头光偏移", Vector) = (0.0, 1.0, 0, 0)

		 [Enum(Off, 0, On, 1)] _FadeON("Open Fade", float) = 0
		_FadeDistanceNear("FadeDistanceNear ", float) = 20
		_FadeDistanceFar("FadeDistanceFar ", float) = 55

		//cover color
		[Toggle(ISCOVER_ON)] _isCover_on("开启覆盖功能" , float) = 0
		_CoverColor("覆盖颜色" , Color) = (1.0,1.0,1.0,1.0)
		_CoverTex("覆盖纹理" , 2D) = "white" {}
        [Enum(Off, 0, On, 1)]_isUseConverBumpTex("开启覆盖法线" , float) = 0
        _CoverBumpTex("覆盖法线", 2D) = "bump" {}
		_CoverBumpMapScale("覆盖法线增强", Range(0.01,10)) = 1.0
		_CoverAmount("Cover Amount" , Range(0.0, 1.0)) = 0.5
		_CoverFade("Cover Fade" , Range(0.1, 10.0)) = 0.5
        _CoverSmoothnessScale("Cover Smoothness Scale", Range(0.0, 10.0)) = 1.0

		[Space(10)] 
        [Toggle(_IsICEOn)]_IsICEOn("开启冰石功能" , float) = 0      
        [Enum(Off, 0, On, 1)]_UseThicknessMask("UseThicknessMask", float) = 0
        [HDR] _SSSColor("SSSColor", Color) = (1, 1, 1, 1)            
        _Translucency("Scaterring Strength", Range(0 , 50)) = 3        
        _TransScattering("Scaterring Falloff", Range(1 , 20)) = 1
        _TransShadow("Trans Shadow", Range(0, 1)) = 0.2
        _BackTransNormalDistortion("Back Normal Distortion", Range(0, 1)) = 0.2
        _FrontTransNormalDistortion("Front Normal Distortion", Range(0, 1)) = 0.2
        _FrontTransIntensity("Front Intensity", Range(0, 1)) = 0.15            
                 
        [Enum(Off, 0, On, 1)]_UseRimThicknessMask("UseRimThicknessMask", float) = 1
        //[HDR] _RimColor("RimColor", Color) = (1, 1, 1, 1)
        [HDR] _RimIceColor("RimColor", Color) = (1, 1, 1, 1)
        _RimBase("RimBase", Range(-2, 2)) = -0.59
        _RimPower("RimPower", Range(0, 10)) = 0.9
        _RimIntensity("RimIntensity", Range(0, 3)) = 0.3
       
        [Toggle(_IsDustOn)]_IsDustOn("Dust On", float) = 0        
        [NoScaleOffset]_DustMap("DustMap:RG", 2D) = "white" {}//Ice Texture(Dust:RG )
        [HDR] _DustColor("Dust Color", Color) = (1.0, 1.0, 1.0, 1.0)        
        _DustNoiseUVScale("Dust Noise UVScale", float) = 1
        _DustNoiseIntensity("Dust Noise Intensity", float) = 0.5
        _DustDepthShift("Dust Depth Shift", float) = 0.5
        _DustUVScale("Dust UV Scale(xy:scale zw:speed)", Vector) = (1, 1, 0, 0)
    } 

	//高配
    SubShader {
        //LOD 500
        Tags { "RenderType"="Opaque" }

		//ForwardBase
        Pass 
		{
			Tags { "LightMode"="ForwardBase"}
			Blend[_SrcBlend][_DstBlend]
			ZWrite[_ZWrite]
            ZTest LEqual

            CGPROGRAM
				#pragma target 3.0
				#pragma multi_compile_fwdbase			    
				#pragma multi_compile __ FOG_LINEAR //优化  #pragma multi_compile_fog
				//优化  #pragma multi_compile __ _DEBUG_NO_PBR
				//#pragma skip_variants VERTEXLIGHT_ON LIGHTPROBE_SH DIRLIGHTMAP_COMBINED				
			    //优化  multi_compile	        
				#pragma shader_feature _ LOD_FADE_CROSSFADE 
				#pragma shader_feature_local Blend_ON
				#pragma shader_feature_local CLIP_ON 
				#pragma shader_feature_local RAIN_ON
				#pragma shader_feature_local HOLLOW_ON
				//#pragma shader_feature EMISSION_ON
				#pragma shader_feature_local REFLECT_ON
				#pragma shader_feature_local MIRROR_REFLECTION_ON		
				#pragma shader_feature_local _CameraLight_IsOpening
				#pragma shader_feature_local ISCOVER_ON
				#pragma shader_feature_local _IsICEOn
                #pragma shader_feature_local _IsDustOn
				 #pragma shader_feature_local _HasExtendMap
				#pragma multi_compile_instancing
			    
			   //优化  add				
				#pragma skip_variants  LIGHTPROBE_SH DYNAMICLIGHTMAP_ON _SPECULARHIGHLIGHTS_OFF VERTEXLIGHT_ON 
				#pragma skip_variants  UNITY_SPECCUBE_BOX_PROJECTION UNITY_SPECCUBE_BLENDING 
				#pragma skip_variants FOG_EXP FOG_EXP2
				#pragma skip_variants DIRECTIONAL_COOKIE POINT_COOKIE SPOT _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
				//

				#include "UnityCG.cginc"
				#include "Lighting.cginc"
				#include "AutoLight.cginc" 
				#include "../Common/BingFengCG.cginc"    
				#include "../Common/CameraLight.hlsl"   
				#include "../Common/ICECommon.hlsl"  

                #pragma vertex vert  
                #pragma fragment frag  
				
				struct a2v{
    				float4 vertex   : POSITION;
    				half3 normal    : NORMAL;
					half4 tangent   : TANGENT;
    				float2 uv0      : TEXCOORD0;
    				float2 uv1      : TEXCOORD1;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f{
					float4 pos     : SV_POSITION;
					half4  uv01    : TEXCOORD0;     
					half4  uv23	   : TEXCOORD1;
					float4 tspace0 : TEXCOORD2;
					float4 tspace1 : TEXCOORD3;
					float4 tspace2 : TEXCOORD4;
					UNITY_LIGHTING_COORDS(6, 7)
				#if USING_FOG
					UNITY_FOG_COORDS(8)
				#endif
				#if MIRROR_REFLECTION_ON || defined(LOD_FADE_CROSSFADE)
					float4 screenPos : TEXCOORD9;
	
				#endif
					float3 worldPos: TEXCOORD10;
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};
				
				sampler2D _MainTex; half4 _MainTex_ST, _Color ;
				half _MainTexAlphaType;
				sampler2D _BumpMap; half4 _BumpMap_ST;
				half _BumpMapScale;
				sampler2D _MetallicTex; half4 _MetallicTex_ST; half _MetallicScale, _GlossScale;
				fixed _GiDiffuse;
				half4 _Specular;
			#if _HasExtendMap
			    sampler2D _ExtendMap;
			#endif
			#if CLIP_ON
				half _ClipAlpha;
			#endif
			#if HOLLOW_ON
				half _HollowRate;
			#endif
			#if RAIN_ON
				half _DiffuseScale, _NormalScale, _DirectSpecularScale, _IndirectDiffuseScale, _IndirectSpecularScale;
			#endif
			//优化  #if EMISSION_ON
				half4 _Emission; half _EmissionStart, _EmissionSpeed;
				half _EmissionOn; //xx add
			//#endif
			#if REFLECT_ON
				half _ReflectMetallic;
				half _ReflectSmoothness;
			#endif
			#if MIRROR_REFLECTION_ON
				sampler2D _MirrorReflectionTex; half4 _MirrorReflectionTex_ST;
				half3 _MirrorNormal;
				half _MirrorReflectionScale, _FresnelExp;
			#endif

			#if _G_CAMERA_LIGHT && defined(_CameraLight_IsOpening)
				half4 _CameraLightColorScale;
				half4 _CameraLightOffset;
				float _CameraLightTrans;
				float _CameraLightMin;				
			#endif

			#if ISCOVER_ON
				//覆盖色Cover属性
				half4 _CoverColor;
				half4 _CoverTex_ST;
				half _isUseConverBumpTex;
				half _CoverAmount;
				half _CoverFade;
				half _CoverSmoothnessScale;
				sampler2D _CoverTex;
				sampler2D _CoverBumpTex;
				half _CoverBumpMapScale;
			#endif

				half _FadeON;
				half _FadeDistanceNear;
				half _FadeDistanceFar;
            #if _IsICEOn
				half _UseThicknessMask;
				half  _Translucency;
				half  _TransScattering;
				half  _TransShadow;
				half  _BackTransNormalDistortion;
				half  _FrontTransNormalDistortion;
				half  _FrontTransIntensity;
				half4 _SSSColor;
				half _UseRimThicknessMask;
				half _RimBase;
				half _RimPower;
				half _RimIntensity;
				half4 _RimIceColor;

				
			#endif
			#if _IsDustOn
			    half4 _DustColor;
				half _DustNoiseUVScale;
				half _DustNoiseIntensity;
				half _DustDepthShift;
				half4 _DustUVScale;

                sampler2D _DustMap;
			#endif
				//float SmoothnessToPerceptualRoughness(float smoothness)
				//{
				//	return (1 - smoothness);				
				//}

				//float PerceptualRoughnessToRoughness(float perceptualRoughness)
				//{
				//	return perceptualRoughness * perceptualRoughness;
				//}

				//inline float GGXTerm(float NdotH, float roughness)
				//{
				//	float a2 = roughness * roughness;
				//	float d = (NdotH * a2 - NdotH) * NdotH + 1.0f; // 2 mad
				//	return UNITY_INV_PI * a2 / (d * d + 1e-7f); // This function is not intended to be running on Mobile,															
				//}

				float BakeryDirectionalLightmapSpecular(float2 lmUV, float3 normalWorld, float3 viewDir, float smoothness)
				{
					float3 dominantDir = UNITY_SAMPLE_TEX2D_SAMPLER(unity_LightmapInd, unity_Lightmap, lmUV).xyz;
					half len = length(dominantDir.xyz);
					if (len < 0.01f)
						return 0;
					dominantDir = dominantDir * 2 - 1;
					
					half3 halfDir = Unity_SafeNormalize(normalize(dominantDir) - viewDir);
					half nh = saturate(dot(normalWorld, halfDir));
					half perceptualRoughness = SmoothnessToPerceptualRoughness(smoothness);
					half roughness = PerceptualRoughnessToRoughness(perceptualRoughness);
					half spec = GGXTerm(nh, roughness);
					return spec;
				}
				
				void CalcAlbedoAlpha(half alpha, out half AO, out half clipValue)
				{
					AO = 1;
					clipValue = 1;
					//None
					if(_MainTexAlphaType > 1.5f)
					{
						return;
					}
					//AO
					if(_MainTexAlphaType > 0.5f)
					{
						AO = alpha;
						return;
					}
					//clipon
					if(_MainTexAlphaType < 0.5f)
					{
						clipValue = alpha;	
						return;
					}

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


				half GetFadeByViewDistance(float3 posWorld)
				{
					half fade = 1.0;
					UNITY_BRANCH
					if(_FadeON > 0.5)
					{ 
						//相机坐标系的物体坐标
						float3 posView = UnityWorldToViewPos(posWorld).xyz;
						//计算与相机距离
						float dis = length(posView);
						fade = 1 - saturate((dis - _FadeDistanceNear) / (_FadeDistanceFar - _FadeDistanceNear));
					}
					return fade;
				}

			#if ISCOVER_ON
				//覆盖颜色，可以覆盖雪、苔藓、草等贴图
				float Apply_CoverColor(inout half3 normalWorld, inout half3 color, inout half smooth , float2 uv , float4 tspace0 , float4 tspace1 , float4 tspace2)
				{
					float coverMask = 0;
					half3 finalColor = color;
					half finalSmooth = smooth;
					half3 finalNormalWS = normalWorld;
					half originWorldNormalY = tspace1.z;
					#if ISCOVER_ON //开启使用Cover
						half coverSmooth = _CoverSmoothnessScale; 
						float temp_CoverAmount = (_CoverAmount * 3);
						float saferPower = max(saturate((originWorldNormalY * temp_CoverAmount)), 0.0001);
						coverMask = pow(saferPower, (_CoverFade * 10)); //计算阈值

						half3 coverColor = _CoverColor; //cover的颜色值
					//#if _G_USE_COVER_BASETEX //使用贴图纹理覆盖
						uv = uv * _CoverTex_ST.xy + _CoverTex_ST.zw; 
						half3 texColor = tex2D(_CoverTex,uv ).rgb;
						coverColor =  coverColor * texColor;
						coverSmooth = _CoverSmoothnessScale * coverColor.r;//无法线贴图时，用coverColor.r作为高粗糙度的扰动
					//#if _G_USE_COVER_NORMAL//使用覆盖纹理的法线贴图
						if(_isUseConverBumpTex > 0.5f)
						{
							coverSmooth = _CoverSmoothnessScale;//有法线贴图时，以法线贴图的B通道作为粗糙度的扰动
							//half3 temp_normalTS = SampleCoverNormalTS(uv , coverSmooth);//采样法线贴图
							half4 converBumpColor = tex2D(_CoverBumpTex,uv);
							half3 temp_normalTS = UnpackNormalWithScale(converBumpColor , _CoverBumpMapScale);
							half3 temp_normalWS = normalize(half3(dot(tspace0, temp_normalTS), dot(tspace1, temp_normalTS), dot(tspace2, temp_normalTS)));//将法线贴图从切线空间转换成世界空间
							finalNormalWS = lerp(finalNormalWS , temp_normalWS, coverMask);//根据coverMask获得最终的法线贴图
						}
					//#endif
					//#endif
						finalColor = lerp(finalColor, coverColor, coverMask);
						finalSmooth = lerp(finalSmooth , coverSmooth , coverMask);
					#endif
					color = finalColor;
					smooth = finalSmooth;
					normalWorld = finalNormalWS;
                    return coverMask;
					
				}
			#endif

				v2f vert(a2v v){
					v2f o;
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					UNITY_SETUP_INSTANCE_ID(v);

					o.pos = UnityObjectToClipPos(v.vertex);
				#if MIRROR_REFLECTION_ON || defined(LOD_FADE_CROSSFADE)
					o.screenPos = ComputeScreenPos(o.pos);
				#endif

					o.uv01.xy = TRANSFORM_TEX(v.uv0, _MainTex);
				#ifndef LIGHTMAP_OFF
					o.uv01.zw = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
				#endif

					float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
					o.worldPos = worldPos;
		

					float3 worldNormal = UnityObjectToWorldNormal(v.normal);
					float3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
					half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
					float3 worldBitangent = cross(worldNormal, worldTangent) * tangentSign;
					o.tspace0 = float4(worldTangent.x, worldBitangent.x, worldNormal.x, worldPos.x);
					o.tspace1 = float4(worldTangent.y, worldBitangent.y, worldNormal.y, worldPos.y);
					o.tspace2 = float4(worldTangent.z, worldBitangent.z, worldNormal.z, worldPos.z);

					UNITY_TRANSFER_LIGHTING(o, v.uv1);

				#if USING_FOG
					UNITY_TRANSFER_FOG(o, o.pos);
				#endif

					return o;
				}
       	
				half4 frag(v2f i) : SV_Target{

				#if HOLLOW_ON
					clip((10 * i.pos.x * i.pos.y) % 9 - _HollowRate * 9);
				#endif
#ifdef LOD_FADE_CROSSFADE						
						float2 vpos = i.screenPos.xy / i.screenPos.w * _ScreenParams.xy;
						UnityApplyDitherCrossFade(vpos);
#endif

					half4 albedo = tex2D(_MainTex, i.uv01.xy) * _Color;
					half ao = 1;
					half albedoA = 1;
					CalcAlbedoAlpha(albedo.a , ao , albedoA);
#ifdef _DEBUG_NO_PBR
					return albedo;
#endif
				#if CLIP_ON
					clip(albedoA - _ClipAlpha);  
				#endif
				    float3 worldViewDir = normalize(UnityWorldSpaceViewDir(i.worldPos));
					half3 tangentNormal = UnpackNormalWithScale(tex2D(_BumpMap,i.uv01.xy) , _BumpMapScale);
					half3 worldNormal = normalize(half3(dot(i.tspace0, tangentNormal), dot(i.tspace1, tangentNormal), dot(i.tspace2, tangentNormal)));

					half4 metallicTex = tex2D(_MetallicTex, i.uv01.xy);
			
					half metallic = metallicTex.r *  _MetallicScale;
					half smoothness = metallicTex.a * _GlossScale;
				#if REFLECT_ON 
					half kBlendFactor = 0;
					half metallicTex_b = step(0.5001, metallicTex.b);
					metallic = lerp(metallic,_ReflectMetallic, metallicTex_b);
					smoothness = lerp(smoothness,_ReflectSmoothness, metallicTex_b);
					kBlendFactor = lerp(kBlendFactor, metallic, metallicTex_b);
				#endif
				half4 extend = half4(0.0, 0.0, 1.0, 0.0);
				 #ifdef _HasExtendMap
				{    
					extend = tex2D(_ExtendMap, i.uv01.xy);
				}
				#endif
				//冰石
				half iceThickNess = 0;
				#ifdef _IsICEOn
				{     
					float2  thicknessUV = i.uv01.xy;// *_ThicknessMap_ST.xy + _ThicknessMap_ST.zw;
					half thicknessChannel = extend.g;//SAMPLE_TEXTURE2D(_ThicknessMap, sampler_ThicknessMap, thicknessUV).b;
        
					iceThickNess =  1.0 * (1.0 - _UseThicknessMask) + thicknessChannel * _UseThicknessMask;
				}
				#endif

				#if defined(_IsDustOn)
					float3 _tangentWS = float3(i.tspace0.x,i.tspace1.x,i.tspace2.x);
					float3 _bitangentWS = float3(i.tspace0.y,i.tspace1.y,i.tspace2.y);
					float3 _normalWS = float3(i.tspace0.z,i.tspace1.z,i.tspace2.z);
				    float3x3 tangentToWorldMatrix = float3x3(_tangentWS,_bitangentWS,_normalWS);
					half3 iceColor = ICEDustColor(_DustMap, _DustColor.rgb, i.worldPos, worldViewDir, tangentToWorldMatrix,
						i.uv01.xy, 0, _DustNoiseUVScale, _DustNoiseIntensity, _DustDepthShift, _DustUVScale);
					#ifdef _IsICEOn            
						iceColor *= iceThickNess;
					#endif
					albedo.rgb += iceColor;
				#endif
				//
				half3 albedo_nocover = albedo.rgb;
				float coverMask = 0.0;
				#if ISCOVER_ON
					coverMask = Apply_CoverColor(worldNormal ,albedo.rgb  , smoothness, i.uv01.xy , i.tspace0 , i.tspace1 , i.tspace2);
				#endif
					half3 specColor;
					half oneMinusReflectivity;
					half3 diffColor = DiffuseAndSpecularFromMetallic(albedo.rgb, metallic, specColor, oneMinusReflectivity);
					specColor = specColor * _Specular.rgb;

					float3 worldPos = float3(i.tspace0.w, i.tspace1.w, i.tspace2.w);
					BF_LIGHT_ATTENUATION(atten, i, worldPos);
					//atten = UNITY_SHADOW_ATTENUATION( i, i.pos )
					//if (atten == 0)
					//{
					//	atten = 1;//临时代码 一直返回 0， 周一请教 韩老师
					//}

					half3 ambient = 0;
					half shadowMask = 1;
					initAmbientAndShadowMask(i.uv01.zw, ambient, shadowMask);
					ambient *= ao;
					UnityLight light;
					light.color = _LightColor0.rgb * atten * shadowMask;
					light.dir = UnityWorldSpaceLightDir(worldPos);
					float3 halfDir = Unity_SafeNormalize (float3(light.dir) + worldViewDir);
					half nl = saturate(dot(worldNormal, light.dir));
					float nh = saturate(dot(worldNormal, halfDir));
					half nv = saturate(dot(worldNormal, worldViewDir));
					float lh = saturate(dot(light.dir, halfDir));
				//#ifndef LIGHTMAP_OFF
				//	half4 bakedColorTex = UNITY_SAMPLE_TEX2D(unity_Lightmap, i.uv01.zw);
				//	half3 bakedColor = bakedColorTex.rgb * bakedColorTex.a * unity_Lightmap_HDR.x;	//DecodeLightmap(bakedColorTex);
				//	half3 ambient = bakedColor;
				//#else
				//	half3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;
				//#endif
					UnityIndirect gi;
					//gi.diffuse = ambient;
					gi.diffuse = _GiDiffuse * ambient + (1 - _GiDiffuse);
					// 
					//gi.diffuse = _GiDiffuse * ShadeSHPerPixel(worldNormal, ambient, worldPos) + (1 - _GiDiffuse);

					//gi.diffuse = ShadeSHPerPixel(worldNormal, ambient, worldPos);
					//gi.specular = 0;

					// Diffuse
					half3 directDiffuse = diffColor * light.color * nl;
					half3 indirectDiffuse = diffColor * gi.diffuse;
					indirectDiffuse = modifyAmbientByShadowColor(indirectDiffuse, atten * shadowMask);
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

				#if REFLECT_ON 
					Unity_GlossyEnvironmentData glossIn;
					glossIn.roughness = SmoothnessToPerceptualRoughness(smoothness);
					glossIn.reflUVW = reflect(-worldViewDir, worldNormal);

					UnityGIInput giInput;
					giInput.worldPos = i.worldPos.xyz;
					#if UNITY_SPECCUBE_BLENDING || UNITY_SPECCUBE_BOX_PROJECTION
						giInput.boxMin[0] = unity_SpecCube0_BoxMin;
						giInput.boxMin[1] = unity_SpecCube1_BoxMin;
					#endif
					#if UNITY_SPECCUBE_BOX_PROJECTION
						giInput.boxMax[0] = unity_SpecCube0_BoxMax;
						giInput.boxMax[1] = unity_SpecCube1_BoxMax;
						giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
						giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
					#endif
					giInput.probeHDR[0] = unity_SpecCube0_HDR;
					giInput.probeHDR[1] = unity_SpecCube1_HDR;
					half3 indirectSpecular = MyGI_IndirectSpecular(giInput, glossIn , kBlendFactor);
					if(metallicTex.b < 0.5f) //非B通道标记的不会产生反射
					{
						half3 env0 = indirectSpecular;
						half3 giSpecular = lerp(unity_IndirectSpecColor.rgb, env0, metallic);
						half surfaceReduction = 1.0 - roughness * perceptualRoughness * 0.28;
						half grazingTerm = saturate(smoothness + (1 - oneMinusReflectivity));
						indirectSpecular = surfaceReduction * giSpecular * FresnelLerpFast(specColor, grazingTerm, nv);
					}
	
					// half3 env0 = Unity_GlossyEnvironment(UNITY_PASS_TEXCUBE(unity_SpecCube0), unity_SpecCube0_HDR, glossIn);
					// gi.specular = lerp(unity_IndirectSpecColor.rgb, env0, metallic);
					// half surfaceReduction = 1.0 - roughness * perceptualRoughness * 0.28;
					// half grazingTerm = saturate(smoothness + (1 - oneMinusReflectivity));
					// half3 indirectSpecular = surfaceReduction * gi.specular * FresnelLerpFast(specColor, grazingTerm, nv);

				#else
					gi.specular = 0;
				#endif

				#if RAIN_ON
					directDiffuse *= _DiffuseScale;
					directSpecular *= _DirectSpecularScale;
					indirectDiffuse *= _IndirectDiffuseScale;
					//indirectSpecular *= _IndirectSpecularScale;
				#endif

				#if _G_CAMERA_LIGHT && defined(_CameraLight_IsOpening)
					half fTerm = saturate(smoothness + (1 - oneMinusReflectivity));
					half3 cameraLightDir = normalize(worldViewDir + _CameraLightOffset.xyz);

					//half ratio = step(_MinMetallic, metallic);
					//ratio = lerp(0.0, metallic, ratio);
					half ratio = ratio = saturate(pow(_CameraLightTrans, metallic) - (1.0 - _CameraLightMin));
					half3 cameraLightColor = APPLY_CAMERA_LIGHT(worldNormal, fTerm, cameraLightDir, roughness, diffColor, _CameraLightColorScale) * ratio;
				#else
					half3 cameraLightColor = half3(0, 0, 0);
				#endif

				directDiffuse *= ao;
				directSpecular *= ao;
				#if REFLECT_ON
					half4 color = half4(directDiffuse + directSpecular + indirectDiffuse + indirectSpecular + cameraLightColor, 1);
				#else
					half4 color = half4(directDiffuse + directSpecular + indirectDiffuse + cameraLightColor, 1);
				#endif

				//优化 
				/*#if EMISSION_ON
					half3 emission = metallicTex.g * (1 + _Emission.a * 5) * (_EmissionStart * sin( _Time.y * _EmissionSpeed) + 1 - _EmissionStart) * _Emission.rgb;
					color.rgb += emission;
				#endif*/
				half3 emission = metallicTex.g * (1 + _Emission.a * 5) * (_EmissionStart * sin(_Time.y * _EmissionSpeed) + 1 - _EmissionStart) * _Emission.rgb;
				color.rgb = lerp(color.rgb, color.rgb + emission, _EmissionOn);
				color.a = albedoA;
				//冰石
				 #ifdef _IsICEOn
				{       
					half outLightAtten = atten;

					/////SSS        
					half4 transParams = half4(_TransScattering, _BackTransNormalDistortion, _Translucency, _TransShadow);        
					float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
					float SSS = 0;
					
					half3 scatterSpecular = IceLightingScattering(outLightAtten, lightDir, worldViewDir, worldNormal, albedo_nocover, transParams,
						_SSSColor, _FrontTransNormalDistortion, _FrontTransIntensity, SSS) * iceThickNess;        
					////Rim      
					half rimMask = 1.0 * (1.0 - _UseRimThicknessMask) + extend.b * _UseRimThicknessMask;
					half3 rimCol = IceLightingRIM(ambient.rgb,outLightAtten,worldNormal, worldViewDir, _RimIceColor,
						_SSSColor , SSS * extend.b, _TransShadow,_RimPower, _RimIntensity,_RimBase, rimMask, _UseRimThicknessMask);
					color.rgb += (1.0 - coverMask) * (scatterSpecular + rimCol);     
				}
				#endif	

				#if MIRROR_REFLECTION_ON
					half4 mirrorReflColor = tex2Dproj(_MirrorReflectionTex, UNITY_PROJ_COORD(i.screenPos)); 
					half fresnel = pow(1 - saturate(dot(worldViewDir, normalize(_MirrorNormal))), _FresnelExp);
					color.rgb = lerp(color.rgb, mirrorReflColor.rgb, _MirrorReflectionScale * mirrorReflColor.a * fresnel);
				#endif

				half fade = GetFadeByViewDistance(worldPos.xyz);
				color.a *= fade;
				#if USING_FOG
					UNITY_APPLY_FOG(i.fogCoord, color.rgb);
				#endif
					//return UNITY_SHADOW_ATTENUATION( i, i.pos );
					return color;
				}
            ENDCG
        }

		UsePass "BF/Shadow/ShadowCaster/COMMON"
    }

	// //中配
    // SubShader {
	// 	LOD 300
    //     Tags { "RenderType"="Opaque" }

    //     UsePass "BF/Scene/NormalDiffuse/NORMAL_DIFFUSE_BASE"
	// 	UsePass "BF/Shadow/ShadowCaster/COMMON"
	// }

	// //低配
    // SubShader{
	// 	LOD 200
    //     Tags {"RenderType"="Opaque" "IgnoreProjector"="true"}

    //     UsePass "BF/Common/Albedo/ALBEDO_BASE"
    // }

	CustomEditor "InspectorShader_BF_Scene_PBSNormalDiffuse"
    FallBack Off
}
