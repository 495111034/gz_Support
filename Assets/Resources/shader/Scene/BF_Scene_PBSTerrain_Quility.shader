Shader "BF/Scene/PBSTerrain_Quility"
{
    Properties{
        		//PBS
		_V_T2M_Control ("Control Map (RGBA)", 2D) = "black" {}
        _Weight("Height Blending Weight", Range(0.1, 1)) = 1.0
		_Color0("Color 1", color) = (1, 1, 1, 1)
		[NoScaleOffset]_V_T2M_Splat1 ("Layer 1 (R)", 2D) = "black" {}
		[NoScaleOffset]_V_T2M_Splat1_bumpMap("Normal 1", 2D) = "bump"{}
		_V_T2M_Splat1_bumpMapScale("Normal 1 Scale", Range(0.01,10)) = 1.0
		[NoScaleOffset]_V_T2M_Splat1_MetallicMap("MetallicMap 1",2D) = "black"{}
		_V_T2M_Splat1_uvScale("", float) = 1
		_Color1("Color 2", color) = (1, 1, 1, 1)
		[NoScaleOffset]_V_T2M_Splat2 ("Layer 2 (G)", 2D) = "black" {}
		[NoScaleOffset]_V_T2M_Splat2_bumpMap("Normal 2", 2D) = "bump"{}
		_V_T2M_Splat2_bumpMapScale("Normal 2 Scale", Range(0.01,10)) = 1.0
		[NoScaleOffset]_V_T2M_Splat2_MetallicMap("MetallicMap 2",2D) = "black"{}
		_V_T2M_Splat2_uvScale("Layer 1 Scale", float) = 1
		_Color2("Color 3", color) = (1, 1, 1, 1)
		[NoScaleOffset]_V_T2M_Splat3 ("Layer 3 (B)", 2D) = "black" {}
		[NoScaleOffset]_V_T2M_Splat3_bumpMap("Normal 3", 2D) = "bump"{}
		_V_T2M_Splat3_bumpMapScale("Normal 3 Scale", Range(0.01,10)) = 1.0
		[NoScaleOffset]_V_T2M_Splat3_MetallicMap("MetallicMap 3",2D) = "black"{}
		_V_T2M_Splat3_uvScale("", float) = 1
		_Color3("Color 4", color) = (1, 1, 1, 1)
		[NoScaleOffset]_V_T2M_Splat4 ("Layer 4 (A)", 2D) = "black" {}
		[NoScaleOffset]_V_T2M_Splat4_bumpMap("Normal 4", 2D) = "bump"{}
		_V_T2M_Splat4_bumpMapScale("Normal 4 Scale", Range(0.01,10)) = 1.0
		[NoScaleOffset]_V_T2M_Splat4_MetallicMap("MetallicMap 4",2D) = "black"{}
		_V_T2M_Splat4_uvScale("", float) = 1
        //除了控制图_V_T2M_Control其他图片进行此UV缩放,修正地形导出工具非正方形地形导出后出现的uv错误
        _ExtraUVScale("额外UV缩放",Vector) = (1.0, 1.0, 0, 0)
        _MetallicScale("金属度", Range(0, 2)) = 0.5	//美术需要的超出物理的临时改动
        _GlossScale("光滑度", Range(0, 2)) = 0.5    //美术需要的超出物理的临时改动  
 
        [HDR]_Specular("高光颜色", Color)       = (1, 1, 1, 1)

        //Raining
        [Toggle(RAIN_ON)] _RainOn("开启下雨", Float) = 0
		_RainRippleTex("涟漪贴图:强度(R),高度(GB),时间差(A)", 2D) = "white" {}
		_RainRippleScale("涟漪强度", Range(0, 2)) = 1
		_RainRippleSpeed("涟漪速度", Range(0, 10)) = 1
		_RainRippleOffsetX("波纹偏移X",Range(-1,2)) = 1
		_RainRippleOffsetY("波纹偏移Y",Range(-1,2)) = 1
		_RainWaveTex("涌动贴图", 2D) = "white" {}
		_RainWaveScale("涌动程度", Range(0, 10)) = 1
		_RainWaveSpeed("涌动速度", Range(0, 10)) = 1
		_DiffuseScale("漫反射强度", Range(0, 2)) = 1
		_NormalScale("法线强度", Range(0, 2)) = 1
		_DirectSpecularScale("直接高光强度", Range(0, 2)) = 1
		_IndirectSpecularScale("间接高光强度", Range(0, 2)) = 1

        //Scene Reflection 
		[Toggle(REFLECT_ON)] _ReflectOn("开启反射",Float) =0
		_ReflectMetallic("反射面金属度" , Range(0, 2)) = 1
		_ReflectSmoothness("反射面光滑度" , Range(0, 2)) = 1
		[NoScaleOffset]
		_Cube("环境立方体贴图", Cube) = "black" {}
		_CubeColor("环境颜色（RGB：颜色 A：强度）",Color) = (1,1,1,1)
		_Reflection("反射强度", Range(0,2) ) = 1
		_RimPower("菲尼尔角度", Range(1,10) ) = 5

		[Toggle(_CameraLight_IsOpening)] _CameraLight_IsOpening("开启镜头光", Float) = 1
		[HDR]_CameraLightColorScale("镜头光的强度控制", Color) = (1,1,1,1)		
        //_MinMetallic("金属度阀值", Range(0, 1)) = 0.0
		_CameraLightTrans("镜头光变换", Range(0.1, 50)) = 2.0
		_CameraLightMin("镜头光阀值", Range(0.0, 1.0)) = 0.1
		_CameraLightOffset("镜头光偏移", Vector) = (0.0, 1.0, 0, 0)

        [Toggle] _UseRealtimeShadow("使用实时阴影", float) = 0
    }
    SubShader{
        LOD 500
        Tags{"Queue" = "Geometry" "RenderType" = "Opaque"}

        pass{
            Tags{"LightMode" = "ForwardBase"}
            CGPROGRAM
            #define _G_IS_TERRAIN 1
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            #include "../Common/BingFengCG.cginc"
            #include "../Common/CameraLight.hlsl"
            #include "BF_Scene_LightingCommon.cginc"

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

            // #pragma shader_feature_local RAIN_ON
            #pragma shader_feature_local REFLECT_ON

            #pragma shader_feature_local _CameraLight_IsOpening

            //优化  add
            #pragma skip_variants VERTEXLIGHT_ON LIGHTPROBE_SH DIRLIGHTMAP_COMBINED DIRLIGHTMAP_COMBINED
            #pragma skip_variants UNITY_SPECCUBE_BOX_PROJECTION UNITY_SPECCUBE_BLENDING 
            #pragma skip_variants FOG_EXP FOG_EXP2
            #pragma skip_variants DIRECTIONAL_COOKIE POINT_COOKIE SPOT _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            //

            struct a2v{
                float4 vertex   : POSITION;
                half3 normal    : NORMAL;
                half4 tangent   : TANGENT;
                float2 uv0      : TEXCOORD0;
                float2 uv1      : TEXCOORD1;
            };

            struct v2f{
                float4 pos      : SV_POSITION;
                float4 uv0      : TEXCOORD0;
                half4  uv23	    : TEXCOORD1;
                float4 tspace0  : TEXCOORD2;
                float4 tspace1  : TEXCOORD3;
                float4 tspace2  : TEXCOORD4;
                UNITY_LIGHTING_COORDS(6,7)
            #if USING_FOG
                UNITY_FOG_COORDS(8)
            #endif

            #if _G_USE_CUSTOMREALTIMESHADOW
                float4 shadowScreenPos  : TEXCOORD10;
            #endif
                // float3 worldPos: TEXCOORD10;
            };

            //base params
            fixed4 _Color0, _Color1, _Color2, _Color3;

            sampler2D _V_T2M_Control;
            float4 _V_T2M_Control_ST;
            half _Weight;
            Texture2D _V_T2M_Splat1,_V_T2M_Splat2,_V_T2M_Splat3,_V_T2M_Splat4;
            Texture2D _V_T2M_Splat1_bumpMap,_V_T2M_Splat2_bumpMap,_V_T2M_Splat3_bumpMap,_V_T2M_Splat4_bumpMap;
            Texture2D _V_T2M_Splat1_MetallicMap,_V_T2M_Splat2_MetallicMap,_V_T2M_Splat3_MetallicMap,_V_T2M_Splat4_MetallicMap;
            //三种四张纹理共享一组SamplerState
            SamplerState sampler_V_T2M_Splat1,sampler_V_T2M_Splat2,sampler_V_T2M_Splat3,sampler_V_T2M_Splat4;
            float _V_T2M_Splat1_uvScale, _V_T2M_Splat2_uvScale, _V_T2M_Splat3_uvScale,_V_T2M_Splat4_uvScale;
            half _V_T2M_Splat1_bumpMapScale, _V_T2M_Splat2_bumpMapScale, _V_T2M_Splat3_bumpMapScale, _V_T2M_Splat4_bumpMapScale;

            half4 _Specular;
            half2 _ExtraUVScale;
            half _MetallicScale, _GlossScale;
			samplerCUBE _Cube; half4 _CubeColor;
			float _Reflection;
			float _RimPower;

            //Rain
            // todo
        #if RAIN_ON

        #endif

        #if REFLECT_ON
            half _ReflectMetallic;
            half _ReflectSmoothness;
        #endif

        #if _CameraLight_IsOpening
            half4 _CameraLightColorScale;		
            half4 _CameraLightOffset;
            //half _MinMetallic;
            float _CameraLightTrans;
            float _CameraLightMin;
        #endif

            v2f vert(a2v v)
            {
                v2f o;
				UNITY_INITIALIZE_OUTPUT(v2f,o);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv0.xy = TRANSFORM_TEX(v.uv0, _V_T2M_Control);
            #ifdef LIGHTMAP_ON
                o.uv0.zw = v.uv1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
            #else
                o.uv0.zw = v.uv1.xy;
            #endif

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
                // o.worldPos = worldPos;
                float3 worldNormal = UnityObjectToWorldNormal(v.normal);
            #if _G_TERRAIN_NORMAL
                float3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                float3 worldBitangent = cross(worldNormal, worldTangent) * tangentSign;
                o.tspace0 = float4(worldTangent.x, worldBitangent.x, worldNormal.x, worldPos.x);
                o.tspace1 = float4(worldTangent.y, worldBitangent.y, worldNormal.y, worldPos.y);
                o.tspace2 = float4(worldTangent.z, worldBitangent.z, worldNormal.z, worldPos.z);
            #else
                o.tspace0 = float4(0, 0, worldNormal.x, worldPos.x);
                o.tspace1 = float4(0, 0, worldNormal.y, worldPos.y);
                o.tspace2 = float4(0, 0, worldNormal.z, worldPos.z);
            #endif
                //UNITY_TRANSFER_LIGHTING(o, v.uv1);
            #if _G_USE_CUSTOMREALTIMESHADOW
                o.shadowScreenPos = mul(_ShadowMatrix, float4(worldPos, 1));
            #endif
            // #if RAIN_ON
            //     o.uv23.xy = TRANSFORM_TEX(v.uv0, _RainRippleTex);
            //     o.uv23.zw = TRANSFORM_TEX(v.uv0, _RainWaveTex);
            // #endif

            #if USING_FOG
            #if defined(FOG_HEIGHT)
                UNITY_TRANSFER_FOG(o, worldPos);
            #else
                UNITY_TRANSFER_FOG(o, o.pos);
            #endif
            #endif
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float3 worldPos = float3(i.tspace0.w, i.tspace1.w, i.tspace2.w);
                float3 worldViewDir = normalize(UnityWorldSpaceViewDir(worldPos));
                half4 splat_control = tex2D(_V_T2M_Control, i.uv0.xy);

                float2 uv = i.uv0.xy * _ExtraUVScale.xy;
                
                fixed4 mainTex0 = _V_T2M_Splat1.Sample(sampler_V_T2M_Splat1, uv * _V_T2M_Splat1_uvScale);
                fixed4 mainTex1 = _V_T2M_Splat2.Sample(sampler_V_T2M_Splat2, uv * _V_T2M_Splat2_uvScale);
                fixed4 mainTex2 = _V_T2M_Splat3.Sample(sampler_V_T2M_Splat3, uv * _V_T2M_Splat3_uvScale);
                fixed4 mainTex3 = _V_T2M_Splat4.Sample(sampler_V_T2M_Splat4, uv * _V_T2M_Splat4_uvScale);

            #if _G_TERRAIN_HEIGHTBLEND
                half4 heightBlend;
                heightBlend.r = mainTex0.a * splat_control.r;
                heightBlend.g = mainTex1.a * splat_control.g;
                heightBlend.b = mainTex2.a * splat_control.b;
                heightBlend.a = mainTex3.a * splat_control.a;
                half ma = max(heightBlend.r, max(heightBlend.g, max(heightBlend.b, heightBlend.a)));
                heightBlend = max(heightBlend - ma + _Weight, 0) * splat_control;

                splat_control = heightBlend / (heightBlend.r + heightBlend.g + heightBlend.b + heightBlend.a);
            #endif


                half4 albedo = splat_control.r * mainTex0 * _Color0 
                            + splat_control.g * mainTex1 * _Color1
                            + splat_control.b * mainTex2 * _Color2
                            + splat_control.a * mainTex3 * _Color3;
            #if _G_TERRAIN_NORMAL
                fixed4 bumpTex = _V_T2M_Splat1_bumpMap.Sample(sampler_V_T2M_Splat1, uv * _V_T2M_Splat1_uvScale) * splat_control.r +
                                _V_T2M_Splat2_bumpMap.Sample(sampler_V_T2M_Splat2, uv * _V_T2M_Splat2_uvScale) * splat_control.g +
                                _V_T2M_Splat3_bumpMap.Sample(sampler_V_T2M_Splat3, uv * _V_T2M_Splat3_uvScale) * splat_control.b +
                                _V_T2M_Splat4_bumpMap.Sample(sampler_V_T2M_Splat4, uv * _V_T2M_Splat4_uvScale) * splat_control.a;
                float bumpScale =  _V_T2M_Splat1_bumpMapScale * splat_control.r +
                                _V_T2M_Splat2_bumpMapScale * splat_control.g +
                                _V_T2M_Splat3_bumpMapScale * splat_control.b +
                                _V_T2M_Splat4_bumpMapScale * splat_control.a;	
                half3 tangentNormal = UnpackNormalWithScale(bumpTex, bumpScale);
                half3 worldNormal = half3(dot(i.tspace0, tangentNormal), dot(i.tspace1, tangentNormal), dot(i.tspace2, tangentNormal));
            #else
                half3 tangentNormal = half3(0, 0, 1);   
                half3 worldNormal = normalize(half3(i.tspace0.z, i.tspace1.z, i.tspace2.z));
            #endif

                fixed4 metallicTex = fixed4(0, 0, 0, 1);
            #if _G_TERRAIN_METALLICTEX
                metallicTex = _V_T2M_Splat1_MetallicMap.Sample(sampler_V_T2M_Splat1, uv * _V_T2M_Splat1_uvScale) * splat_control.r +
                                    _V_T2M_Splat2_MetallicMap.Sample(sampler_V_T2M_Splat2, uv * _V_T2M_Splat2_uvScale) * splat_control.g +
                                    _V_T2M_Splat3_MetallicMap.Sample(sampler_V_T2M_Splat3, uv * _V_T2M_Splat3_uvScale) * splat_control.b +
                                    _V_T2M_Splat4_MetallicMap.Sample(sampler_V_T2M_Splat4, uv * _V_T2M_Splat4_uvScale) * splat_control.a;

                half metallic = metallicTex.r * _MetallicScale;
                half smoothness = metallicTex.a * _GlossScale;
            #else
                half metallic = 0;
                half smoothness = 1;
            #endif
                // half3 specColor;
                // half oneMinusReflectivity;
                // half3 diffColor = DiffuseAndSpecularFromMetallic(albedo.rgb, metallic, specColor, oneMinusReflectivity);
                // specColor = specColor * _Specular.rgb;
            #if REFLECT_ON && _G_TERRAIN_REFLECT
                half metallicTex_b = step(0.5001, metallicTex.b);
                metallic = lerp(metallic, _ReflectMetallic, metallicTex_b);
                smoothness = lerp(smoothness, _ReflectSmoothness, metallicTex_b);
            #endif

                half3 ambient = 0;
                half shadowMask = 1;
            #if _G_USE_CUSTOMREALTIMESHADOW
                float3 shadowScreenPos = i.shadowScreenPos.xyz / i.shadowScreenPos.w;
                shadowScreenPos = shadowScreenPos * 0.5 + 0.5;
                initAmbientAndShadowMask(i.uv0.zw, ambient, shadowMask, worldPos, worldNormal, shadowScreenPos);
            #else
                initAmbientAndShadowMask(i.uv0.zw, ambient, shadowMask, worldPos, worldNormal, float3(0, 0, 0));
            #endif
                half SHADOW_MIN_ATTENUATION = 0;
                BF_LIGHT_ATTENUATION(atten, i, worldPos);

                UnityLight light;
                light.color = _LightColor0.rgb * atten * shadowMask;
                light.dir = normalize(UnityWorldSpaceLightDir(worldPos));

                UnityIndirect gi;
                gi.diffuse = ambient;

                SurfaceData surfaceData = GetSurfaceData(albedo.xyz, metallic, smoothness, worldNormal, worldViewDir, light.dir);
                surfaceData.specularColor *= _Specular.rgb;
                // gi.diffuse = modifyAmbientByShadowColor(gi.diffuse, atten * shadowMask);
            #if REFLECT_ON && _G_TERRAIN_REFLECT
                // Unity_GlossyEnvironmentData glossIn;
                // glossIn.roughness = surfaceData.perceptualRoughness;
                // glossIn.reflUVW = surfaceData.refDir;

                // UnityGIInput giInput;
                // giInput.worldPos = worldPos;
                // #if UNITY_SPECCUBE_BLENDING || UNITY_SPECCUBE_BOX_PROJECTION
                //     giInput.boxMin[0] = unity_SpecCube0_BoxMin;
                //     giInput.boxMin[1] = unity_SpecCube1_BoxMin;
                // #endif
                // #if UNITY_SPECCUBE_BOX_PROJECTION
                //     giInput.boxMax[0] = unity_SpecCube0_BoxMax;
                //     giInput.boxMax[1] = unity_SpecCube1_BoxMax;
                //     giInput.probePosition[0] = unity_SpecCube0_ProbePosition;
                //     giInput.probePosition[1] = unity_SpecCube1_ProbePosition;
                // #endif
                // giInput.probeHDR[0] = unity_SpecCube0_HDR;
                // giInput.probeHDR[1] = unity_SpecCube1_HDR;

				half fresnel = saturate(pow(1 - surfaceData.nv, _RimPower));
                half3 cubeMap = texCUBE(_Cube, surfaceData.refDir).rgb * _CubeColor.rgb;
                half3 reflectColor = lerp(unity_IndirectSpecColor.rgb, cubeMap, fresnel * _Reflection);
                gi.specular = lerp(unity_IndirectSpecColor.rgb, reflectColor, step(0.5, metallicTex.b));
                // half3 indirectSpecular = MyGI_IndirectSpecular(giInput, glossIn, kBlendFactor);
                // half3 maskIndirectSpecular = lerp(unity_IndirectSpecColor.rgb, reflectColor, metallic);
                // gi.specular = lerp(reflectColor, maskIndirectSpecular, step(metallicTex.b, 0.5));
            #else
                gi.specular = 0;
            #endif

                ///todo
            // #if RAIN_ON && _G_TERRAIN_RAIN
            //     gi.diffuse *= _IndirectSpecularScale;      
            // #endif

                //half3 color = UNIVERSAL_PBR(diffColor, specColor, oneMinusReflectivity, smoothness, worldNormal, worldViewDir, light, gi);
                half3 color = UNIVERSAL_PBR_PRECOM(surfaceData, light, gi);
			#if _G_SETTING_UNLIT
				color = albedo.xyz;
			#endif

            #if _CameraLight_IsOpening && _G_TERRAIN_CAMERALIGHT
                half fTerm = saturate(smoothness + (1 - surfaceData.oneMinusReflectivity));
                half3 cameraLightDir = normalize(worldViewDir + _CameraLightOffset.xyz);
                half ratio = saturate(pow(_CameraLightTrans, metallic) - (1.0 - _CameraLightMin));
                half3 cameraLightColor = APPLY_CAMERA_LIGHT(worldNormal, fTerm, cameraLightDir, surfaceData.roughness, surfaceData.diffuseColor, _CameraLightColorScale) * ratio;
                color += cameraLightColor;	
            #endif
            
            #if USING_FOG
                UNITY_APPLY_FOG(i.fogCoord, color);
            #endif

                return half4(color, 1);
            }

            ENDCG
        }
		UsePass "BF/Shadow/ShadowCaster/COMMON"

    }
}