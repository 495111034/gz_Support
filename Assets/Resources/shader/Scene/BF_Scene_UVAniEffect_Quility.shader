Shader "BF/Scene/UVAniEffect_Quility"{
    Properties
    {
		[HideInInspector] _Mode("混合模式", Float) = 0.0
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("源混合模式", Float) = 5.0
		[HideInInspector][Enum(UnityEngine.Rendering.BlendMode)] _SrcBlendAlpha("源A通道混合模式", Float) = 5.0
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("目标混合模式", Float) = 6.0
		[HideInInspector][Enum(UnityEngine.Rendering.BlendMode)] _DstBlendAlpha("目标A通道混合模式", Float) = 6.0
		[Enum(UnityEngine.Rendering.CullMode)] _Cull("剔除模式", Float) = 0
		_ZWrite("深度写入", Float) = 0.0

        _MainTex ("Albedo (RGB)", 2D) = "white" {}
		_MainAlpha("主纹理",range(0,1)) =1.0

        [Space(30)]
        [Toggle(_BRUSH_GRASS_ON)] _BRUSH_GRASS_ON("刷草融入地形", float) = 0
		[HDR]_Color("正面混合颜色", Color) = (1, 1, 1, 1)
        [HDR]_Color1("正面混合颜色底部", Color) = (1, 1, 1, 1)
		[HDR]_Color2("正面混合颜色头部", Color) = (1, 1, 1, 1)
        _layerMiddle("中间混合系数*模型Y",Range(0, 10)) = 0.5
        _BrushStrength("融入地形颜色强度",Range(0, 1)) = 0.5
        _BrushShadowStrength("融入地形阴影强度",Range(0, 1)) = 0.75

        [Space(30)]
        [Toggle(_NATIVE_LIGHTMAP_ON)] _NATIVE_LIGHTMAP("使用导入光照贴图", float) = 0
        _Native_LightMap("导入光照贴图", 2D) = "black" {}
        _Native_LightMapInstensity("导入光照强度", range(0.0, 10.0)) = 1.0

        [Space(30)]
        [Toggle(_PBR_ON)] _PBR("开启PBR", float) = 0
        [NoScaleOffset]_NormalTex("法线贴图", 2D) = "bump" {}
        _NormalScale("法线增强", Range(0.01, 10)) = 1.0
        [NoScaleOffset]_MetallicTex("金属高光纹理,R:金属度|G:无|B:无|A:光滑度",2D) = "black" {}
        _MetallicScale("金属度", Range(0, 2)) = 0.5	//美术需要的超出物理的临时改动
        _GlossScale("光滑度", Range(0, 2)) = 0.5    //美术需要的超出物理的临时改动   
		[HDR]_Specular("高光颜色", Color) = (1, 1, 1, 1)
        [Space(30)]

        [Enum(Off, 0, On, 1)] _EnableCartoonLayer("卡通分层", float) = 0
        _LayerSplitDir("卡通分层方向", Vector) = (0, 1, 0, 0)
        [HDR]_LayerColor1("顶层颜色", Color) = (1, 1, 1, 1)
        [HDR]_LayerColor2("中间颜色", Color) = (1, 1, 1, 1)
        [HDR]_LayerColor3("底层颜色", Color) = (1, 1, 1, 1)
        _LayerSplitHeight1("顶中层分界", Range(0, 1)) = 0.75
        _LayerSplitRange1("顶中层分界过渡范围", Range(0, 1)) = 0.1
        _LayerSplitHeight2("中层层分界", Range(0, 1)) = 0.25
        _LayerSplitRange2("中底层分界过渡范围", Range(0, 1)) = 0.1
        [Space(30)]

		_MainTexUVAni("UV动画(XY:主纹理00，ZW：主纹理01)", Vector) = (0,0,0,0)

		//优化  [Toggle(WAVE_ON)]
		[Enum(Off, 0, On, 1)]_EnableWave("开启波动",float) = 0
		_WaveLength("波长",range(-1,10)) = 4.7
		_WaveAmplitude("幅度",range(0,0.5)) = 0.03

		[Toggle(SECONDTEX_ON)] _SecondTexOn("主纹理01",float) = 0
		_SecondaryTex("主二纹理", 2D) = "white" {}

		[Toggle(NOISE_ON)] _EnableNoiseTex("开启热扰动",float) = 0
		_NoiseTex("噪声图",2D) ="white"{}
		_NoiseSpeed("噪声速度",Range(0,2)) = 0.5
		_NoiseFactor("噪声强度",Range(0,0.1)) = 0.01
		//Clip 
		[Toggle(CLIP_ON)] _ClipOn("开启提出", Float) = 0
		_ClipAlpha("剔除Alpha阈值",Range(0.001,1)) = 0.1
			//优化 
/*
		[Toggle(SWING_ON)] _SwingON ("开启摆动",float) = 0
		_SwingSpeed("摆动速度", Range(20,50)) = 25
		_Rigidness("刚性", Range(0,1)) = 0.01
		_SwayMax("摆动最大值", Range(0, 0.1)) = 0.05
		_TurnOver("翻转",Range(-1,1)) = 1
		_Offset("偏移量",float) = 0

		 [Toggle(LEAF_SWING)] _LeafSwingOn("摇曳",float) =0
		 _LeafSpeed("摇曳速度",Range(0, 100)) = 25
		 _LeafMax("摇曳幅度", Range(0, 0.2)) = 0.05
		 _LeafBlock("摇曳同向",Range(0.1,1)) =0.5*/



        //add
		//自发光
		[Toggle(_EmissionEnable)] _EmissionEnable("开启自发光", Float) = 0
		[Enum(Vertex_Position_Based, 0, UV_Based, 1)] _Color2OverlayType("颜色混合模式", Float) = 0		
		[HDR]_EmissionColor1("自发光颜色1", Color) = (0,0,0,0)
		[HDR]_EmissionColor2("自发光颜色2", Color) = (0,0,0,0)
		_Emission2Level("自发光过渡等级", Float) = 0
		_EmissionFade("自发光过渡", Range(-1 , 1)) = 0.5
		[Enum(Off, 0, On, 1)]_HasEmissionMap("开启自发光掩码?", Float) = 0.0
		[NoScaleOffset] _EmissionMap("自发光掩码(a:mask)", 2D) = "white" {}

		//摇摆
		[KeywordEnum(Off, Wave, Wind)] _wave("Wave", Int) = 0
		_WaveParams("WaveParams(Threshold,Amplitude,Frequency)", Vector) = (0.3, 0.1, 0.15, 0)
		_WaveStartV("WaveStart V", Float) = 0.3

		//交互
		[Toggle(_CUSTOM_INTERACT)] _CUSTOM_INTERACT("Interact On", float) = 0
		//草地弯曲的强度
		_PushStrength("PushStrength", float) = 5
		//交互的范围
		_PushRadiu("PushRadiu ", float) = 2

		//阴影处补光
		_FillLight("FillLight", Range(0 , 1)) = 0.0

		//透光
		//优化  [Toggle(_HasTranslucency)]
		[Enum(Off, 0, On, 1)]_HasTranslucency("Translucency ON", Float) = 0.0
		_Translucency("Strength (X) Power (Y) Distortion (Z)", Vector) = (2, 3, 0.01, 0)

        [Toggle] _UseRealtimeShadow("使用实时物体阴影", float) = 0
        [Toggle] _UseRoleRealtimeShadow("使用角色实时阴影", float) = 0
        _RoleRealtimeShadowMap("实时阴影图", 2D) = "white" {}
        _RoleRealtimeShadowIntensity("实时阴影图强度", float) = 1
    }

    SubShader{
        LOD 200
		Tags{ "Queue" = "AlphaTest" "DisableBatching" = "True" "IgnoreProjector" = "True" }

        Pass{
			Tags { "LightMode" = "ForwardBase" }
			Blend[_SrcBlend][_DstBlend]//,[_SrcBlendAlpha][_DstBlendAlpha]
			ZWrite[_ZWrite]
			Cull[_Cull]     

            CGPROGRAM

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"
            #include "BF_Scene_LightingCommon.cginc"
            #include "BF_Scene_GlobalQualityVars.cginc"
            #include "BF_Scene_UVAniEffect_WaveFun.cginc"
            #include "../common/BingFengCG.cginc"

            #pragma vertex vert
            #pragma fragment frag
            // #pragma enable_d3d11_debug_symbols

            #pragma target 2.0
            #pragma multi_compile _ FOG_HEIGHT
            // #pragma multi_compile_fwdbase
            // #pragma multi_compile DIRECTIONAL
            // #pragma multi_compile _ SHADOWS_SHADOWMASK //固定ShadowMask
            #pragma multi_compile SHADOWS_SHADOWMASK
            #pragma multi_compile _ LIGHTMAP_ON
#if !_G_IN_RUNTIME
            #pragma multi_compile _ SHADOWS_SCREEN
#endif
            #pragma multi_compile _G_GAME_QUALITY_HIGH _G_GAME_QUALITY_LOW  _G_GAME_QUALITY_VERYLOW
            #pragma shader_feature _ LOD_FADE_CROSSFADE
            #pragma shader_feature_local _BRUSH_GRASS_ON
            #pragma shader_feature_local _PBR_ON
            #pragma shader_feature_local SECONDTEX_ON
            #pragma shader_feature_local NOISE_ON
            #pragma shader_feature_local CLIP_ON
            #pragma shader_feature_local _NATIVE_LIGHTMAP_ON
            #pragma shader_feature_local _WAVE_OFF _WAVE_WAVE _WAVE_WIND 
            #pragma multi_compile_instancing
            //优化 #pragma shader_feature  WAVE_ON
            //优化 #pragma shader_feature_local _CUSTOM_INTERACT
            //优化 #pragma shader_feature _HasTranslucency
            //优化 #pragma shader_feature_local _EmissionEnable	

            //优化  add
            #pragma skip_variants LIGHTPROBE_SH DYNAMICLIGHTMAP_ON _SPECULARHIGHLIGHTS_OFF VERTEXLIGHT_ON 
            #pragma skip_variants UNITY_SPECCUBE_BOX_PROJECTION UNITY_SPECCUBE_BLENDING 
            #pragma skip_variants FOG_EXP FOG_EXP2
            #pragma skip_variants DIRECTIONAL_COOKIE POINT_COOKIE SPOT _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
            //

            struct a2v{
                float4 vertex       : POSITION;
                half4 color         : COLOR;
                half3 normal        : NORMAL;
                float4 tangent      : TANGENT;
                float2 uv            : TEXCOORD0;
            // #if !defined(LIGHTMAP_OFF) || defined(_NATIVE_LIGHTMAP_ON)
                float2 lmUV          : TEXCOORD1;
            // #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f{
                float4 pos          : SV_POSITION;
                half4 color         : COLOR;
                half4 uv0           : TEXCOORD0;
                half4 uv1           : TEXCOORD1;
            #if _PBR_ON && _G_USE_NORMAL
                float4 tspace0      : TEXCOORD2;
                float4 tspace1      : TEXCOORD3;
                float4 tspace2      : TEXCOORD4;
            #else
                float3 worldNormal  : TEXCOORD2;
                float3 worldPos      : TEXCOORD3;
            #endif
                UNITY_SHADOW_COORDS(5)
            #if USING_FOG
                UNITY_FOG_COORDS(6)
            #endif
            #if defined(LOD_FADE_CROSSFADE) && _G_UVANI_LODFADE
                float4   screenPos : TEXCOORD7;
            #endif
                float3 vertexOS     : TEXCOORD8;
            #if _G_USE_CUSTOMREALTIMESHADOW
                float4 shadowScreenPos  : TEXCOORD9;
                float4 roleShadowScreenPos  : TEXCOORD10;
            #endif
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };


            sampler2D _MainTex;    half4 _MainTex_ST;
            half4 _MainTexUVAni;
            fixed _MainAlpha;
            half4 _Color;     
            half4 _Color1;     
            half4 _Color2;     
            half _layerMiddle;
            half _BrushStrength;
            half _BrushShadowStrength;

        #if defined(_NATIVE_LIGHTMAP_ON)
            sampler2D _Native_LightMap;
            half4 _Native_LightMap_ST;
            float _Native_LightMapInstensity;
        #endif       

        #if _PBR_ON
            sampler2D _NormalTex;
            float _NormalScale;
            sampler2D _MetallicTex;
            half _MetallicScale, _GlossScale;
            half4 _Specular;
        #endif
        

        #if _G_UVANI_CARTOONLAYER
            float _EnableCartoonLayer;
            float4 _LayerSplitDir;
            float4 _LayerColor1, _LayerColor2, _LayerColor3;
            half _LayerSplitHeight1, _LayerSplitHeight2, _LayerSplitRange1, _LayerSplitRange2;
        #endif
            //add
            ///emisison
            half _EmissionEnable;
            sampler2D _EmissionMap;
            half _Color2OverlayType;
            half4 _EmissionColor1;
            half4 _EmissionColor2;
            half _Emission2Level;
            half _EmissionFade;
            half _HasEmissionMap;



            half _FillLight;

            ///透光
            half  _HasTranslucency;
            half4 _Translucency;

            ///第二纹理
        #if SECONDTEX_ON && _G_UVANI_SECONDTEX
            sampler2D _SecondaryTex; half4 _SecondaryTex_ST;
        #endif

            ///热扰动
        #if NOISE_ON && _G_UVANI_NOISE
            sampler2D _NoiseTex; half4 _NoiseTex_ST;
            half _NoiseFactor;
            half _NoiseSpeed;            
        #endif

            ///剔除
        #if CLIP_ON
            half _ClipAlpha;
        #endif
            half _UseRoleRealtimeShadow;
            sampler2D _RoleRealtimeShadowMap;
            float4x4 _RoleShadowMatrix;
            half _RoleRealtimeShadowIntensity;

            UNITY_INSTANCING_BUFFER_START(Props)
			UNITY_DEFINE_INSTANCED_PROP(float4, _SurfColorArray)
			UNITY_INSTANCING_BUFFER_END(Props)


            half3 ApplyTranslucencyColor(half3 translucency, half3 diffuse, half3 lightColor, float3 normalWS, float3 viewDirWS, float3 lightDirWS)
            {
                half3 result = 0;
                half transPower = translucency.y;
                half3 transLightDir = _WorldSpaceLightPos0.xyz + normalWS * translucency.z;
                half transDot = dot(transLightDir, -viewDirWS);
                transDot = exp2(saturate(transDot) * transPower - transPower);
                result = transDot * (1.0 - saturate(dot(normalWS, lightDirWS))) * lightColor * diffuse * translucency.x;
                return result;
            }

            half3 GetEmissionColor(float vertexOS_Y, float2 uv)
            {
            #if _G_UVANI_EMISSION
                half3 emissionColor = half3(0, 0, 0);
                float referenceVal = (vertexOS_Y - vertexOS_Y * _Color2OverlayType) + uv.y * _Color2OverlayType;
                float color2Mask = saturate(((referenceVal + _Emission2Level) * (_EmissionFade * 2)));
                emissionColor = lerp(_EmissionColor1.rgb, _EmissionColor2.rgb, color2Mask);
            #if _G_SETTING_VERYLOW
                emissionColor = lerp(_EmissionColor1.rgb, _EmissionColor2.rgb, uv.y);
            #endif
            #if _G_UVANI_EMISSIONMAP
                half4 emission = tex2D(_EmissionMap, uv);		
                emissionColor = lerp(emissionColor, emissionColor * emission.a, _HasEmissionMap);
            #endif
                return emissionColor;
            #else
                return half3(0, 0, 0);
            #endif
            }

            v2f vert(a2v v)
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_SETUP_INSTANCE_ID(v);

                float4 vertexWS;
                half4 windColor = 1;
            #if defined(_WAVE_WIND) && _G_UVANI_WAVE
                GrassWind(v.vertex, vertexWS, windColor, v.uv);
            #else           // CalulateVertByWave 计算交互和_WAVE_WAVE
                CalulateVertByWave(v.uv.xy, v.vertex, vertexWS);
            #endif

                o.pos = UnityWorldToClipPos(vertexWS);
                o.vertexOS = v.vertex.xyz;
                o.color = windColor;
                o.uv0.xy = TRANSFORM_TEX(v.uv.xy, _MainTex);
                float3 worldNormal = UnityObjectToWorldNormal(v.normal);
            #if _PBR_ON && _G_USE_NORMAL
                float3 worldTangent = UnityObjectToWorldDir(v.tangent.xyz);
                float3 worldBitangent = cross(worldNormal, worldTangent) * v.tangent.w * unity_WorldTransformParams.w;
                o.tspace0 = float4(worldTangent.x, worldBitangent.x, worldNormal.x, vertexWS.x);
                o.tspace1 = float4(worldTangent.y, worldBitangent.y, worldNormal.y, vertexWS.y);
                o.tspace2 = float4(worldTangent.z, worldBitangent.z, worldNormal.z, vertexWS.z);
            #else
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = vertexWS.xyz;
            #endif

                UNITY_BRANCH
                if(_EnableWave > 0.5)
                {
                    half scale = _WaveAmplitude * sin(o.uv0.x * _WaveLength);
                    o.uv0.xy = o.uv0 + o.uv0 * scale;
                }

                o.uv0.xy += frac(half2(_MainTexUVAni.xy * _Time.x));

            #if SECONDTEX_ON && _G_UVANI_SECONDTEX
                o.uv0.zw = TRANSFORM_TEX(v.uv.xy, _SecondaryTex);
                o.uv0.zw += frac(half2(_MainTexUVAni.zw * _Time.x));                
            #endif
            
            #if NOISE_ON && _G_UVANI_NOISE
                o.uv1.xy = TRANSFORM_TEX(v.uv.xy, _NoiseTex);
            #endif

            #if defined(_NATIVE_LIGHTMAP_ON)
                o.uv1.zw = v.lmUV.xy * _Native_LightMap_ST.xy + _Native_LightMap_ST.zw;
            #elif defined(LIGHTMAP_ON)
                o.uv1.zw = v.lmUV.xy * unity_LightmapST.xy + unity_LightmapST.zw;
            #else
                o.uv1.zw = v.lmUV.xy;
            #endif

            #if USING_FOG
            #if defined(FOG_HEIGHT)
                UNITY_TRANSFER_FOG(o, vertexWS);
            #else
                UNITY_TRANSFER_FOG(o, o.pos);
            #endif
            #endif

            #if defined(LOD_FADE_CROSSFADE) && _G_UVANI_LODFADE
                o.screenPos = ComputeScreenPos(o.pos);
            #endif
            
            #if _G_USE_CUSTOMREALTIMESHADOW
                o.shadowScreenPos = mul(_ShadowMatrix, float4(vertexWS.xyz, 1));
                o.roleShadowScreenPos = mul(_RoleShadowMatrix, float4(vertexWS.xyz, 1));
            #endif
            
                TRANSFER_VERTEX_TO_FRAGMENT(o)
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
            #if NOISE_ON && _G_UVANI_NOISE
                float4 offset = tex2D(_NoiseTex, i.uv1.xy - frac(_Time.y * _NoiseSpeed));
                i.uv0.xy -= offset.xy * _NoiseFactor;
            #endif

                half4 main = tex2D(_MainTex, i.uv0.xy);
                half4 col = main * _Color * _MainAlpha ;
                half one2TwoLerp = i.vertexOS.y * _layerMiddle;
                
            #if _BRUSH_GRASS_ON
                half4 surfColor = UNITY_ACCESS_INSTANCED_PROP(Props, _SurfColorArray);
                //object用
                if(surfColor.g > 0){
                    _Color1.rgb = surfColor.rgb;
                }
                half brushShadow = dot(_Color1.rgb, half3(0.333333, 0.333333, 0.333333)) + 0.2;
                brushShadow = smoothstep(0, 1, brushShadow);
                half3 brushShadowColor = col.rgb * brushShadow;
                brushShadowColor = lerp(col.rgb, brushShadowColor, _BrushShadowStrength);
                half3 brushColor = lerp(brushShadowColor, _Color1.rgb, _BrushStrength);
                col.rgb = lerp(_Color1.rgb, brushColor, one2TwoLerp);
            #if _G_SETTING_VERYLOW
                col.rgb *= lerp(_Color1.rgb, _Color2.rgb, one2TwoLerp);
            #endif
            #else
                col.rgb *= lerp(_Color1.rgb, _Color2.rgb, one2TwoLerp);
            #endif

                col.rgb *= i.color;

            #if CLIP_ON
                clip(main.a - _ClipAlpha);
            #endif

            #if defined(LOD_FADE_CROSSFADE) && _G_UVANI_LODFADE
                float2 vpos = i.screenPos.xy / i.screenPos.w *_ScreenParams.xy;
                UnityApplyDitherCrossFade(vpos);
            #endif

            #if _PBR_ON && _G_USE_NORMAL
                float3 normalTS = UnpackNormalWithScale(tex2D(_NormalTex, i.uv0.xy), _NormalScale);
                float3 worldNormal = normalize(half3(dot(i.tspace0, normalTS), dot(i.tspace1, normalTS), dot(i.tspace2, normalTS)));
                float3 worldPos = float3(i.tspace0.w, i.tspace1.w, i.tspace2.w);
            #else
                float3 worldNormal = normalize(i.worldNormal);
                float3 worldPos = i.worldPos;
            #endif
                float3 viewDirectionWS = normalize(UnityWorldSpaceViewDir(worldPos));

                half3 albedo = col.rgb;    
                half3 ambient = 0;
                half shadowMask = 1;
            #if defined(_NATIVE_LIGHTMAP_ON)
                ambient = tex2D(_Native_LightMap, i.uv1.zw) * _Native_LightMapInstensity;
                shadowMask = 1;
            #else
            #if _G_USE_CUSTOMREALTIMESHADOW
                float3 shadowScreenPos = i.shadowScreenPos.xyz / i.shadowScreenPos.w;
                shadowScreenPos = shadowScreenPos * 0.5 + 0.5;
                initAmbientAndShadowMask(i.uv1.zw, ambient, shadowMask, worldPos, worldNormal, shadowScreenPos);
            #else
                initAmbientAndShadowMask(i.uv1.zw, ambient, shadowMask, worldPos, worldNormal, float3(0, 0, 0));
            #endif
            #endif    
            
                UnityIndirect gi;
                half atten = shadowMask;
                // ambient = modifyAmbientByShadowColor(ambient, atten);
                gi.diffuse = ambient;
                gi.specular = half3(0, 0, 0);


            #if _G_UVANI_CARTOONLAYER
                UNITY_BRANCH
                if(_EnableCartoonLayer > 0.5)
                {
                    float3 layerDir = normalize(_LayerSplitDir.xyz);
                    float spiltY = dot(layerDir, worldNormal);
                    half radius1 = _LayerSplitRange1 * 0.5;
                    half weight1 = smoothstep(_LayerSplitHeight1 - radius1, _LayerSplitHeight1 + radius1, spiltY);
                    half radius2 = _LayerSplitRange2 * 0.5;
                    half weight2 = smoothstep(_LayerSplitHeight2 - radius2, _LayerSplitHeight2 + radius2, spiltY);
                    half3 cartoonColor = lerp(_LayerColor3, lerp(_LayerColor2, _LayerColor1, weight1), weight2);
                    albedo *= cartoonColor;
                }
            #endif

                UnityLight light;
                light.dir = normalize(_WorldSpaceLightPos0.xyz);
                light.color = _LightColor0.rgb * atten;
            #if defined(_NATIVE_LIGHTMAP_ON)
                light.color = 0;
            #endif

            #if SECONDTEX_ON && _G_UVANI_SECONDTEX
                col *= tex2D(_SecondaryTex, i.uv0.zw);
            #endif

            #if _PBR_ON
                half4 metallicTex = tex2D(_MetallicTex, i.uv0.xy);
        
                half metallic = metallicTex.r *  _MetallicScale;
                half smoothness = metallicTex.a * _GlossScale;

                half3 specColor;
                half oneMinusReflectivity;
                half3 diffColor = DiffuseAndSpecularFromMetallic(albedo.rgb, metallic, specColor, oneMinusReflectivity);
                specColor = specColor * _Specular.rgb;
                col.rgb = UNIVERSAL_PBR(diffColor, specColor, oneMinusReflectivity, smoothness, worldNormal, viewDirectionWS, light, gi);
                #if _G_SETTING_UNLIT
                    col.rgb = albedo;
                #endif
            #else
                #if !_BRUSH_GRASS_ON
                    col.rgb = UniversalFragmentBlinnPhongLow(albedo, float3(0, 0, 0), 1, worldNormal, viewDirectionWS, light, gi);
                #if _G_SETTING_UNLIT
                    col.rgb = albedo;
                #endif
                #endif
            #endif
                //add
                //透光
            #if _G_UVANI_TRANSLUCENCY
                UNITY_BRANCH
                if(_HasTranslucency > 0.5)
                {
                    col.rgb += ApplyTranslucencyColor(_Translucency.xyz, albedo, _LightColor0.rgb * atten, worldNormal, viewDirectionWS, light.dir);
                }
            #endif

            #if _G_UVANI_FILLLIGHT
                //阴影处补光
                half3 fillColor = albedo * _LightColor0.xyz * _FillLight;
                fillColor = fillColor - atten * fillColor;
                col.rgb += fillColor;
            #endif

            #if _G_UVANI_EMISSION
                UNITY_BRANCH
                if(_EmissionEnable > 0.5)
                {
                    col.rgb += GetEmissionColor(i.vertexOS.y, i.uv0.xy);
                }
            #endif 

            #if USING_FOG
                UNITY_APPLY_FOG(i.fogCoord, col.rgb);
            #endif

            #if _G_USE_CUSTOMREALTIMESHADOW
                UNITY_BRANCH
                if(_UseRoleRealtimeShadow > 0.5)
                {
                    float3 roleShadowScreenPos = i.roleShadowScreenPos.xyz / i.roleShadowScreenPos.w;
                    roleShadowScreenPos = roleShadowScreenPos * 0.5 + 0.5;
                    float roleRealtimeShadowMap = tex2D(_RoleRealtimeShadowMap, roleShadowScreenPos.xy).r;
                #if UNITY_REVERSED_Z
                    roleRealtimeShadowMap = 1 - roleRealtimeShadowMap;
                #endif
                    half inShadow = step(roleRealtimeShadowMap, roleShadowScreenPos.z);
                    half outBox = saturate(dot(step(1, roleShadowScreenPos.xyz), half3(1, 1, 1)) + dot(step(roleShadowScreenPos.xyz, 0), half3(1, 1, 1)));
                    inShadow = inShadow - inShadow * outBox;
                    col.rgb *= lerp(1, _RoleRealtimeShadowIntensity * unity_ShadowColor, inShadow);
                }
            #endif
            
                return col;
            }

            ENDCG       
        }
        //UsePass "BF/Shadow/ShadowCaster/COMMON"
    }
}
