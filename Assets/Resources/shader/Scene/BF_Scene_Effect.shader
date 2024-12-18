Shader "BF/Scene/Effect"
{
    Properties
    {
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("源混合模式", Float) = 5.0
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("目标混合模式", Float) = 10.0

        [Space]
        _MainTex("主纹理", 2D) = "white" {}
        [HDR]_MainColor("颜色", Color) = (1,1,1,1)
        [NoScaleOffset]_MaskTex("遮罩", 2D) = "white" {}
        // _MaskThreshold("遮罩度", float) = 0

        [Space]
        [Toggle(FRAME_ANIM_ON)] _Frame_Animation("序列帧动画", float) = 0
        [IntRange]_AnimRowCount("行数", range(1, 10)) = 1
        [IntRange]_AnimColumnCount("列数", range(1, 10)) = 1
        _AnimSpeed("动画速度", float) = 1

        [Space]
        [Enum(Off, 0, On, 1)] _Flowing("流动", float) = 0
        _FlowingParams("x：u方向流动速度， y：v方向流动速度", vector) = (0, 0, 0, 0)
        [Enum(Off, 0, Ver1, 1)] _FlowingBoost("流动增强", float) = 0
        _FlowingBoostParams1("流动增强：xy：uv方向频率，zw：强度", vector) = (1, 1, 1, 1)
        // _FlowingBoostParams2("流动增强：xy：uv中心偏移，zw：偏移强度", vector) = (0, 0, 1, 1)

        [Space]
        [Toggle(TWIST_ON)] _Twist("扭曲", float) = 0
        [NoScaleOffset]_TwistNoiseTex("扭曲噪声图, rg：uv扭曲参数",2D) = "white" {}
        _TwistStrength("扭曲强度", float) = 1
        [Enum(Off, 0, On, 1)]_TwistByTimeOn("随时间变化", float) = 0
        _TwistChangeSpeed("x：u变化速度， y：v变化速度", vector) = (0, 0, 0, 0)

        [Space]
        [KeywordEnum(Off, Ver1, Ver2)] ImageBlockGlitch("错位图块", float) = 0
        [Enum(R, 0, G, 1, B, 2)] _ImageBlockSplitBaseColor("不分离通道", float) = 0
        _ImageBlockglitchFade("ImageBlockSplitglitchFade", range(0, 1)) = 0.5
        _BlockCommonParams("xy：uv错位频率，zw：uv错位占比", vector) = (1, 1, 0.2, 0.2)
        _BlockParamsVer1("第一层错位，xy：图块数，zw：uv错位强度缩放", vector) = (0.05, 0.05, 1, 1)
        _BlockParamsVer2("第二层错位，xy：图块数，zw：uv错位强度缩放", vector) = (0.05, 0.05, 1, 1)

        [Space]
        [KeywordEnum(Off, Ver1, Ver2)] RGBSplitGlitch("RGB颜色故障分离", float) = 0
        [Enum(R, 0, G, 1, B, 2)] _RGBSplitBaseColor("不分离通道", float) = 0
        _SplitglitchFade("RGBSplitglitchFade", range(0, 1)) = 0.5
        _SplitCommonParams("xy：抖动烈度，zw：抖动幅度", vector) = (1, 1, 1, 1)
        _SplitVer1Params1("Ver1: 1阶：xy：频率，zw：偏移", vector) = (6, 6, 0, 0)
        _SplitVer1Params2("Ver1: 2阶：xy：频率，zw：偏移", vector) = (16, 16, 0, 0)
        _SplitVer2Params1("Ver2: 1阶：xy：频率，zw：偏移", vector) = (19, 19, 0, 0)
        _SplitVer2Params2("Ver2: 2阶：xy：频率，zw：偏移", vector) = (27, 27, 0, 0)


        [Space]
        [Toggle(DISSOLVE_ON)] _Dissolve("溶解", float) = 0
        [NoScaleOffset]_DissolveMap("溶解贴图", 2D) = "white" {}
        _DissolveThreshold("溶解度", float) = 0
        [Enum(Off, 0, On, 1)]_DirectionalDissolve("定向溶解", float) = 0
        _DissolveStartPos("溶解开始处", vector) = (0, 0, 0)
        _DissolveEndPos("溶解结束处", vector) = (0, 1, 0)
        _DissolveRatio("定向范围", range(0.1, 10)) = 1

    }

    SubShader
    {
        Tags{ "Queue" = "Transparent"  "DisableBatching" = "True" "IgnoreProjector" = "True" }
        Pass
        {
            ZWrite On
            Cull Back
            Blend [_SrcBlend][_DstBlend]

            CGPROGRAM


            #pragma vertex vert
            #pragma fragment frag 
            // #pragma enable_d3d11_debug_symbols
            #pragma target 3.0
            #pragma multi_compile __ FOG_LINEAR FOG_HEIGHT
            #pragma multi_compile_instancing

            #pragma shader_feature_local FRAME_ANIM_ON
            #pragma shader_feature_local DISSOLVE_ON
            // #pragma shader_feature_local FLOWING_ON
            #pragma shader_feature_local TWIST_ON
            #pragma shader_feature_local RGBSPLITGLITCH_OFF RGBSPLITGLITCH_VER1 RGBSPLITGLITCH_VER2
            #pragma shader_feature_local IMAGEBLOCKGLITCH_OFF IMAGEBLOCKGLITCH_VER1 IMAGEBLOCKGLITCH_VER2

            #pragma skip_variants FOG_EXP FOG_EXP2

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "../common/BingFengCG.cginc"
            #include "BF_Scene_GlobalQualityVars.cginc"

            struct a2v
            {
                float4 vertex   : POSITION;
                half4  color    : COLOR;
                half4   uv      : TEXCOORD0;

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos      : SV_POSITION;
                half4 color     : COLOR;
                half2 uv0       : TEXCOORD0;
            #if defined(DISSOLVE_ON)
                float3 posOS    : TEXCOORD1;
            #endif
            #if USING_FOG
                UNITY_FOG_COORDS(5)
            #endif

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };


            sampler2D _MainTex;
            half4 _MainTex_ST;
            half4 _MainColor;
            sampler2D _MaskTex;
            // half _MaskThreshold;
        
            //溶解
        #if defined(DISSOLVE_ON)
            sampler2D _DissolveMap;
            float _DissolveThreshold;
            float _DirectionalDissolve;
            float4 _DissolveStartPos, _DissolveEndPos;
            float _DissolveRatio;
        #endif

            //扭曲
        #if defined(TWIST_ON)
            sampler2D _TwistNoiseTex;
            half _TwistStrength;
            half _TwistByTimeOn;
            half4 _TwistChangeSpeed;
        #endif

            //错位图块故障
        #if !defined(IMAGEBLOCKGLITCH_OFF)
            half _ImageBlockSplitBaseColor;
            half _ImageBlockglitchFade;
            half4 _BlockCommonParams;
            half4 _BlockParamsVer1;
        #if defined(IMAGEBLOCKGLITCH_VER2)
            half4 _BlockParamsVer2;
        #endif
        #endif

            //RGB颜色分离故障
        #if !defined(RGBSPLITGLITCH_OFF)
            half _RGBSplitBaseColor;
            half _SplitglitchFade;
            half4 _SplitCommonParams;
            half4 _SplitVer1Params1;
            half4 _SplitVer1Params2;
            // half _SplitClamp;
            // half4 _SplitUParamsVer1;
            // half4 _SplitVParamsVer1;
        #if defined(RGBSPLITGLITCH_VER2)
            half4 _SplitVer2Params1;
            half4 _SplitVer2Params2;
            // half4 _SplitUParamsVer2;
            // half4 _SplitVParamsVer2;
        #endif
        #endif

            //流动
        //#if defined(FLOWING_ON)
            half _Flowing;
            float4 _FlowingParams;
            half _FlowingBoost;
            half4 _FlowingBoostParams1;
//            half4 _FlowingBoostParams2;
        //#elif

            //序列帧动画
        #if defined(FRAME_ANIM_ON)
            half _AnimRowCount, _AnimColumnCount, _AnimSpeed;
        #endif


            v2f vert(a2v v)
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_SETUP_INSTANCE_ID(v);

                float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.uv0 = TRANSFORM_TEX(v.uv, _MainTex);

            //#if defined(FLOWING_ON)
                // half2 flowingUV = _Time.y * _FlowingParams.xy + _FlowingBoostParams.xy * abs(sin(_FlowingBoostParams.zw * _Time.y));
                // o.uv0 -= flowingUV * _Flowing;
            //#endif

            #if defined(DISSOLVE_ON)
                o.posOS = v.vertex.xyz;
            #endif

            #if USING_FOG
            #if defined(FOG_HEIGHT)
                UNITY_TRANSFER_FOG(o, worldPos);
            #else
                UNITY_TRANSFER_FOG(o, o.pos);
            #endif           
            #endif

                UNITY_TRANSFER_INSTANCE_ID(v, o);

                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i)
                

                half maskValue = tex2D(_MaskTex, i.uv0).r;
                // clip(maskValue - _MaskThreshold);

                half2 mainTexUV = i.uv0;

            // #if defined(FLOWING_ON)
                half2 flowingUV = _Time.y * _FlowingParams.xy;
                //half2 gain = (0.5 + _FlowingBoostParams2.xy - i.uv0) * _FlowingBoostParams2.zw;
                //gain *= 1.5707963 / max(_FlowingBoostParams1.xy, 0.00001) * step(1.5, _FlowingBoost);
                half2 boost = sin(_Time.y * _FlowingBoostParams1.xy) / max(_FlowingBoostParams1.xy, 0.00001);
                flowingUV = frac(lerp(flowingUV, (flowingUV + boost) * _FlowingBoostParams1.zw, step(0.5, _FlowingBoost)));
                mainTexUV -= flowingUV * _Flowing;
            // #endif


            #if defined(DISSOLVE_ON)
                half dissolveMask = tex2D(_DissolveMap, i.uv0).r;
                float3 dissolveDir = _DissolveEndPos - _DissolveStartPos;
                float directionalStrength = pow(length(dissolveDir), 2);
                float offset = dot(dissolveDir, i.posOS - _DissolveStartPos) / directionalStrength;

                float finalDissolveValue = lerp(dissolveMask, dissolveMask * offset * _DissolveRatio + offset, _DirectionalDissolve);
                clip(finalDissolveValue - _DissolveThreshold);
            #endif


            #if defined(TWIST_ON)
                half2 twistSampleUV = mainTexUV + frac(_Time.y * _TwistChangeSpeed.xy * _TwistByTimeOn);
                half2 twistUV = tex2D(_TwistNoiseTex, twistSampleUV).rg * 2 - 1;
                mainTexUV += twistUV * _TwistStrength;
            #endif

                half4 mainColor = 1;
                half2 imageBlockGlitchOffset = 0;
            #if !defined(IMAGEBLOCKGLITCH_OFF)
                float2 frequencySeed = fmod(floor(_Time.y * _BlockCommonParams.xy), 10000.0);
                half2 oneMinusImageBlockGlitchRatio = (1 - _BlockCommonParams.zw);
                half2 remap = max(0.001, _BlockCommonParams.zw);
                
                half2 blockIndex1 = ceil(i.uv0 * _BlockParamsVer1.xy);
                half2 seed1 = half2(dot(blockIndex1 * frequencySeed.x, half2(7.13, 3.71)), 
                                    dot(blockIndex1 * frequencySeed.y, half2(7.13, 3.71)));
                half2 blockUVOffsetIntensity1 = frac(sin(seed1) * 43758.5453123); //* (1 + sin(7 * _Time.y) * 0.5) ;
                blockUVOffsetIntensity1 = clamp(blockUVOffsetIntensity1, oneMinusImageBlockGlitchRatio, 100);
                blockUVOffsetIntensity1 -= oneMinusImageBlockGlitchRatio;
                blockUVOffsetIntensity1 *= frac(sin(_Time.y) * 43758.5453123);
                blockUVOffsetIntensity1 *= _BlockParamsVer1.zw / remap;

                // half4 temp = tex2D(_MainTex, float2(0.5839,0.00025));
                // temp.a = 1;
                // return temp;
                half2 finalIntensity = blockUVOffsetIntensity1 ;     // clamp(frac(sin(seed1) * 43758.5453123), 1 - _BlockCommonParams.zw, 100) - 1 + _BlockCommonParams.zw;
            #if defined(IMAGEBLOCKGLITCH_VER2)
                half2 blockIndex2 = ceil(i.uv0 * _BlockParamsVer2.xy);
                float2 seed2 = float2(dot(blockIndex2 * frequencySeed.x, half2(17.13, 3.71)), 
                                    dot(blockIndex2 * frequencySeed.y, half2(17.13, 3.71)));
                half2 blockUVOffsetIntensity2 = frac(cos(seed2) * 43758.5453123); //* (1 + sin(7 * _Time.y) * 0.5) ;
                blockUVOffsetIntensity2 = clamp(blockUVOffsetIntensity2, oneMinusImageBlockGlitchRatio, 100);
                blockUVOffsetIntensity2 -= oneMinusImageBlockGlitchRatio;
                blockUVOffsetIntensity2 *= frac(cos(_Time.y) * 43758.5453123);
                blockUVOffsetIntensity2 *= _BlockParamsVer2.zw / remap;

                finalIntensity *= blockUVOffsetIntensity2;          // clamp(frac(sin(seed2) * 43758.5453123), 1 - _BlockCommonParams.zw, 100) - 1 + _BlockCommonParams.zw;
            #endif
                imageBlockGlitchOffset = finalIntensity;
            #endif

                half2 RGBSplitGlitchOffset = 0;
            #if !defined(RGBSPLITGLITCH_OFF)
                half2 splitAmount = (1.0 + sin(_SplitVer1Params1.xy * _Time.y + _SplitVer1Params1.zw)) * 0.5;
                splitAmount *=(2.0 + sin(_SplitVer1Params2.xy * _Time.y + _SplitVer1Params2.zw)) * 0.5;
            #if defined(RGBSPLITGLITCH_VER2)
                splitAmount *=(2.0 + sin(_SplitVer2Params1.xy * _Time.y + _SplitVer2Params1.zw)) * 0.5;
                splitAmount *=(2.0 + sin(_SplitVer2Params2.xy * _Time.y + _SplitVer2Params2.zw)) * 0.5;
            #endif
                splitAmount = pow(splitAmount, _SplitCommonParams.xy);
                splitAmount *= _SplitCommonParams.zw;

                RGBSplitGlitchOffset = splitAmount;
            #endif


                half2 uvs[3] = {mainTexUV, mainTexUV, mainTexUV};
                uint baseColorIndex = 0;
                uint otherIndex1 = 0;
                uint otherIndex2 = 0;
                half glitchFade = 1;
            #if !defined(IMAGEBLOCKGLITCH_OFF)
                baseColorIndex = _ImageBlockSplitBaseColor;
                glitchFade = lerp(glitchFade, glitchFade * _ImageBlockglitchFade, 
                            step(0, imageBlockGlitchOffset.x + imageBlockGlitchOffset.y));
                otherIndex1 = (baseColorIndex + 1) % 3; //fmod(baseColorIndex + 1, 3);
                otherIndex2 = (baseColorIndex + 2) % 3; //fmod(baseColorIndex + 2, 3);
                uvs[otherIndex1] = uvs[otherIndex1] + imageBlockGlitchOffset;
                uvs[otherIndex2] = uvs[otherIndex2] - imageBlockGlitchOffset;
            #endif
            #if !defined(RGBSPLITGLITCH_OFF)
                baseColorIndex = _RGBSplitBaseColor;
                glitchFade = lerp(glitchFade, glitchFade * _SplitglitchFade, 
                            step(0, RGBSplitGlitchOffset.x + RGBSplitGlitchOffset.y));
                otherIndex1 = (baseColorIndex + 1) % 3; //fmod(baseColorIndex + 1, 3);
                otherIndex2 = (baseColorIndex + 2) % 3; //fmod(baseColorIndex + 2, 3);
                uvs[otherIndex1] = uvs[otherIndex1] + RGBSplitGlitchOffset;
                uvs[otherIndex2] = uvs[otherIndex2] - RGBSplitGlitchOffset;                
            #endif

            #if defined(FRAME_ANIM_ON)
                half rowSplit = 1 / _AnimRowCount;
                half columnSplit = 1 / _AnimColumnCount;
                int frameCount = _AnimRowCount * _AnimColumnCount;
                int currentFrame = floor(frac(_Time.y * _AnimSpeed / frameCount) * frameCount); //fmod(_Time.y * _AnimSpeed, rowCount * columnCount));// floor(_Time.y * _AnimSpeed) % (rowCount * columnCount);
                int rowIndex = floor(currentFrame / _AnimColumnCount);
                int columIndex = currentFrame - rowIndex * _AnimColumnCount;
                half2 framAnimOffset = half2(columIndex * columnSplit, (_AnimColumnCount - 1 - rowIndex) * rowSplit);
                half2 columRow = half2(_AnimColumnCount, _AnimRowCount);
                uvs[0] = frac(uvs[0]) / columRow + framAnimOffset;
                uvs[1] = frac(uvs[1]) / columRow + framAnimOffset;
                uvs[2] = frac(uvs[2]) / columRow + framAnimOffset;
            #endif

            #if !defined(RGBSPLITGLITCH_OFF) || !defined(IMAGEBLOCKGLITCH_OFF)
                half4 colorRGB[3];
                /// 如果mainTex开启mipmap不使用dx会出现问题；
                // float2 dx = ddx(i.uv0);
                // float2 dy = ddy(i.uv0);
                colorRGB[0] = tex2D(_MainTex, uvs[0]);
                colorRGB[1] = tex2D(_MainTex, uvs[1]);
                colorRGB[2] = tex2D(_MainTex, uvs[2]);
                mainColor = colorRGB[baseColorIndex];
                half4 otherColor = 1;
                otherColor.r = colorRGB[0].r;
                otherColor.g = colorRGB[1].g;
                otherColor.b = colorRGB[2].b;
                mainColor.rgb = lerp(mainColor.rgb, otherColor.rgb, glitchFade);
            #else
                mainColor = tex2D(_MainTex, uvs[0]);
            #endif
                //mainColor = tex2D(_MainTex, i.uv0);
                mainColor *= i.color * _MainColor;
                mainColor.a *= maskValue;
                
            #if USING_FOG
                UNITY_APPLY_FOG(i.fogCoord, mainColor.rgb);
            #endif

                return mainColor;
            }
            ENDCG
        }
    }
}