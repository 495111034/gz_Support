Shader "Hidden/PostProcessing/CustomBloom"
{
    HLSLINCLUDE
        #pragma exclude_renderers gles
        //#pragma enable_d3d11_debug_symbols

        #include "Assets/Libs/Cinemachine/PostProcessing/Shaders/StdLib.hlsl"
        #include "Assets/Libs/Cinemachine/PostProcessing/Shaders/Colors.hlsl"
		#include "Assets/Libs/Cinemachine/PostProcessing/Shaders/Sampling.hlsl"

		struct VertexInput
		{
		    float4 positionOS : POSITION;
		    float2 texcoord   : TEXCOORD0;
		};

		struct VertexOutput
		{
			float4 positionCS : SV_POSITION;
			float4 uv0 : TEXCOORD0;
			float4 uv1 : TEXCOORD1;
		};

		TEXTURE2D_SAMPLER2D(_SourceTex , sampler_SourceTex);
		TEXTURE2D_SAMPLER2D(_MultiBlurTex , sampler_MultiBlurTex);

        float4 _SourceTex_TexelSize;
        float4 _MultiBlurTex_TexelSize;

		float4 _Scaler;
        float4 _Params; // x: intensity, y: clamp, z: threshold (linear), w: threshold knee
		float4 _UVTransformSource; //xy: scale, zw: offset
		float4 _UVTransformTarget; //xy: scale, zw: offset
		float4 _UVTransformBlur1; //xy: scale, zw: offset
		float4 _UVTransformBlur2; //xy: scale, zw: offset
		float4 _UVTransformBlur3; //xy: scale, zw: offset
		float4 _BlurComposeWeights;

        #define Intensity           _Params.x
        #define ClampMax            _Params.y
        #define Threshold           _Params.z
        #define ThresholdKnee       _Params.w

		// Transform to homogenous clip space
		float4x4 GetWorldToHClipMatrix()
		{
			return unity_MatrixVP;
		}

		// Tranforms position from world space to homogenous space
		float4 TransformWorldToHClip(float3 positionWS)
		{
			return mul(GetWorldToHClipMatrix(), float4(positionWS, 1.0));
		}

        half4 EncodeHDR(half3 color)
        {
        #if _USE_RGBM
            half4 outColor = EncodeRGBM(color);
        #else
            half4 outColor = half4(color, 1.0);
        #endif

        #if UNITY_COLORSPACE_GAMMA
            return half4(sqrt(outColor.xyz), outColor.w); // linear to γ
        #else
            return outColor;
        #endif
        }

        half3 DecodeHDR(half4 color)
        {
        #if UNITY_COLORSPACE_GAMMA
            color.xyz *= color.xyz; // γ to linear
        #endif

        #if _USE_RGBM
            return DecodeRGBM(color);
        #else
            return color.xyz;
        #endif
        }

		VertexOutput VertBox( VertexInput input  )
		{
			VertexOutput output = (VertexOutput)0;

        	float2 worldPosition = input.positionOS.xy * _UVTransformTarget.xy + _UVTransformTarget.zw;
        	output.positionCS = TransformWorldToHClip(float3(worldPosition.xy, 0));
			output.positionCS.zw = float2(0.0, 1.0);
			
			float4 uv = (input.positionOS.xyxy + 1.0) * 0.5 * _UVTransformSource.xyxy + _UVTransformSource.zwzw;
			output.uv0 = _SourceTex_TexelSize.xyxy * float4(0.95999998, 0.25, 0.25, -0.95999998) + uv.zwzw;
			output.uv1 = _SourceTex_TexelSize.xyxy * float4(-0.95999998, -0.25, -0.25, 0.95999998) + uv;
			
			return output;
		}

		float4 FragPrefilter( VertexOutput input  ) : SV_Target
		{
            half3 color = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, input.uv0.xy).xyz;
            
			color += SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, input.uv0.zw).xyz;
            color += SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, input.uv1.xy).xyz;
            color += SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, input.uv1.zw).xyz;
			
			color = color * 0.2500;

            // User controlled clamp to limit crazy high broken spec
            color = min(ClampMax, color);

            // Thresholding
            half brightness = Max3(color.r, color.g, color.b);
            half softness = clamp(brightness - Threshold + ThresholdKnee, 0.0, 2.0 * ThresholdKnee);
            softness = (softness * softness) / (4.0 * ThresholdKnee + 1e-4);
            half multiplier = max(brightness - Threshold, softness) / max(brightness, 1e-4);
            color *= multiplier;
			
            // Thresholding
            //color = max(color - Threshold, 0) * Intensity;
			
            return EncodeHDR(color);
		}

		VertexOutput VertBlur( VertexInput input  )
		{
			VertexOutput output = (VertexOutput)0;

        	float2 worldPosition = input.positionOS.xy * _UVTransformTarget.xy + _UVTransformTarget.zw;
        	output.positionCS = TransformWorldToHClip(float3(worldPosition.xy, 0));
			output.positionCS.zw = float2(0.0, 1.0);
			
			output.uv0 = (input.positionOS.xyxy + 1.0) * 0.5 * _UVTransformSource.xyxy + _UVTransformSource.zwzw;
        	
        	float4 sourceUVBounds = float4(0.0, 1.0, 0.0, 1.0) * _UVTransformSource.xxyy + _UVTransformSource.zzww;
			output.uv1 = _SourceTex_TexelSize.xxyy * float4(1.0, -1.0, 1.0, -1.0) + sourceUVBounds;
			
			return output;
		}

		float4 FragGaussBlur( VertexOutput input  ) : SV_Target
		{
			float4 uv = _Scaler.xyxy * float4(-3.074399, -3.074399, -1.253424, -1.253424) + input.uv0.xyxy;
			uv = clamp(uv, input.uv1.xzxz, input.uv1.ywyw);
			half3 c0 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.xy).xyz;
			half3 c1 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.zw).xyz;
			
			uv = _Scaler.xyxy * float4(0.4109247, 0.4109247, 2.141763, 2.141763) + input.uv0.xyxy;
			uv = clamp(uv, input.uv1.xzxz, input.uv1.ywyw);
			half3 c2 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.xy).xyz;
			half3 c3 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.zw).xyz;

			uv.xy = _Scaler.xy * float2(4.0, 4.0) + input.uv0.xy;
			uv.xy = clamp(uv.xy, input.uv1.xz, input.uv1.yw);
			half3 c4 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.xy).xyz;
			
			half3 color = c0 * 0.01430866 + c1 * 0.31638631 + c2 * 0.57481652 + c3 * 0.093424067 + c4 * 0.001064543;

            return EncodeHDR(color);
		}

		float4 FragBoxBlur( VertexOutput input  ) : SV_Target
		{
            half3 color = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, input.uv0.xy).xyz;
            color += SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, input.uv0.zw).xyz;
            color += SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, input.uv1.xy).xyz;
            color += SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, input.uv1.zw).xyz;
			
			color = color * 0.2500;

            return EncodeHDR(color);
		}
    
		float4 FragBlur9x9( VertexOutput input  ) : SV_Target
		{
			float4 uv = _Scaler.xyxy * float4(-7.1588202, -7.1588202, -5.2274981, -5.2274981) + input.uv0.xyxy;
			uv = clamp(uv, input.uv1.xzxz, input.uv1.ywyw);
			half3 c0 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.xy).xyz;
			half3 c1 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.zw).xyz;
			
			uv = _Scaler.xyxy * float4(-3.3147621, -3.3147621, -1.417412, -1.417412) + input.uv0.xyxy;
			uv = clamp(uv, input.uv1.xzxz, input.uv1.ywyw);
			half3 c2 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.xy).xyz;
			half3 c3 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.zw).xyz;
			
			uv = _Scaler.xyxy * float4(0.47224459, 0.47224459, 2.364548, 2.364548) + input.uv0.xyxy;
			uv = clamp(uv, input.uv1.xzxz, input.uv1.ywyw);
			half3 c4 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.xy).xyz;
			half3 c5 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.zw).xyz;

			uv = _Scaler.xyxy * float4(4.268898, 4.268898, 6.1908078, 6.1908078) + input.uv0.xyxy;
			uv = clamp(uv, input.uv1.xzxz, input.uv1.ywyw);
			half3 c6 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.xy).xyz;
			half3 c7 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.zw).xyz;

			uv.xy = _Scaler.xy * float2(8.0, 8.0) + input.uv0.xy;
			uv.xy = clamp(uv.xy, input.uv1.xz, input.uv1.yw);
			half3 c8 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.xy).xyz;
			
			half3 color = c0 * 0.00096486782 + c1 * 0.01512981 + c2 * 0.1009583 + c3 * 0.28889999 + c4 * 0.35640359
						+ c5 * 0.1897708 + c6 * 0.043465629 + c7 * 0.0042536259 + c8 * 0.0001532399;

            return EncodeHDR(color);
		}

		float4 FragBlur16x16( VertexOutput input  ) : SV_Target
		{

			float4 uv = _Scaler.xyxy * float4(-14.26509, -14.26509, -12.29338, -12.29338) + input.uv0.xyxy;
			uv = clamp(uv, input.uv1.xzxz, input.uv1.ywyw);
			half3 c0 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.xy).xyz;
			half3 c1 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.zw).xyz;
			
			uv = _Scaler.xyxy * float4(-10.32336, -10.32336, -8.3548632, -8.3548632) + input.uv0.xyxy;
			uv = clamp(uv, input.uv1.xzxz, input.uv1.ywyw);
			half3 c2 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.xy).xyz;
			half3 c3 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.zw).xyz;
			
			uv = _Scaler.xyxy * float4(-6.3876772, -6.3876772, -4.4215422, -4.4215422) + input.uv0.xyxy;
			uv = clamp(uv, input.uv1.xzxz, input.uv1.ywyw);
			half3 c4 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.xy).xyz;
			half3 c5 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.zw).xyz;

			uv = _Scaler.xyxy * float4(-2.456162, -2.456162, -0.49121079, -0.49121079) + input.uv0.xyxy;
			uv = clamp(uv, input.uv1.xzxz, input.uv1.ywyw);
			half3 c6 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.xy).xyz;
			half3 c7 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.zw).xyz;
			
			uv = _Scaler.xyxy * float4(1.473654, 1.473654, 3.4387779, 3.4387779) + input.uv0.xyxy;
			uv = clamp(uv, input.uv1.xzxz, input.uv1.ywyw);
			half3 c8 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.xy).xyz;
			half3 c9 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.zw).xyz;

			uv = _Scaler.xyxy * float4(5.4044962, 5.4044962, 7.3711209, 7.3711209) + input.uv0.xyxy;
			uv = clamp(uv, input.uv1.xzxz, input.uv1.ywyw);
			half3 c10 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.xy).xyz;
			half3 c11 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.zw).xyz;

			uv = _Scaler.xyxy * float4(9.338933, 9.338933, 11.30817, 11.30817) + input.uv0.xyxy;
			uv = clamp(uv, input.uv1.xzxz, input.uv1.ywyw);
			half3 c12 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.xy).xyz;
			half3 c13 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.zw).xyz;
			
			uv = _Scaler.xyxy * float4(13.27902, 13.27902, 15.0, 15.0) + input.uv0.xyxy;
			uv = clamp(uv, input.uv1.xzxz, input.uv1.ywyw);
			half3 c14 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.xy).xyz;
			half3 c15 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.zw).xyz;
			
			half3 color = c0 * 0.00014632 + c1 * 0.00094709668 + c2 * 0.0046462719 + c3 * 0.01727958
						+ c4 * 0.048726629 + c5 * 0.1042022 + c6 * 0.1690129 + c7 * 0.207937
						+ c8 * 0.1940565 + c9 * 0.13737381 + c10 * 0.073762059 + c11 * 0.03003788
						+ c12 * 0.0092757363 + c13 * 0.002171654 + c14 * 0.00038539231 + c15 * 3.8788519e-05;

            return EncodeHDR(color);
		}

		float4 FragBlur20x20( VertexOutput input  ) : SV_Target
		{
			float4 uv = _Scaler.xyxy * float4(-18.303101, -18.303101, -16.322439, -16.322439) + input.uv0.xyxy;
			uv = clamp(uv, input.uv1.xzxz, input.uv1.ywyw);
			half3 c0 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.xy).xyz;
			half3 c1 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.zw).xyz;
			
			uv = _Scaler.xyxy * float4(-14.34241, -14.34241, -12.36296, -12.36296) + input.uv0.xyxy;
			uv = clamp(uv, input.uv1.xzxz, input.uv1.ywyw);
			half3 c2 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.xy).xyz;
			half3 c3 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.zw).xyz;
			
			uv = _Scaler.xyxy * float4(-10.38401, -10.38401, -8.4055147, -8.4055147) + input.uv0.xyxy;
			uv = clamp(uv, input.uv1.xzxz, input.uv1.ywyw);
			half3 c4 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.xy).xyz;
			half3 c5 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.zw).xyz;

			uv = _Scaler.xyxy * float4(-6.4273849, -6.4273849, -4.449542, -4.449542) + input.uv0.xyxy;
			uv = clamp(uv, input.uv1.xzxz, input.uv1.ywyw);
			half3 c6 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.xy).xyz;
			half3 c7 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.zw).xyz;
			
			uv = _Scaler.xyxy * float4(-2.4719019, -2.4719019, -0.49437469, -0.49437469) + input.uv0.xyxy;
			uv = clamp(uv, input.uv1.xzxz, input.uv1.ywyw);
			half3 c8 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.xy).xyz;
			half3 c9 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.zw).xyz;

			uv = _Scaler.xyxy * float4(1.48313, 1.48313, 3.4607019, 3.4607019) + input.uv0.xyxy;
			uv = clamp(uv, input.uv1.xzxz, input.uv1.ywyw);
			half3 c10 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.xy).xyz;
			half3 c11 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.zw).xyz;

			uv = _Scaler.xyxy * float4(5.4384332, 5.4384332, 7.416409, 7.416409) + input.uv0.xyxy;
			uv = clamp(uv, input.uv1.xzxz, input.uv1.ywyw);
			half3 c12 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.xy).xyz;
			half3 c13 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.zw).xyz;
			
			uv = _Scaler.xyxy * float4(9.3947134, 9.3947134, 11.37342, 11.37342) + input.uv0.xyxy;
			uv = clamp(uv, input.uv1.xzxz, input.uv1.ywyw);
			half3 c14 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.xy).xyz;
			half3 c15 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.zw).xyz;
			
			uv = _Scaler.xyxy * float4(13.35262, 13.35262, 15.33235, 15.33235) + input.uv0.xyxy;
			uv = clamp(uv, input.uv1.xzxz, input.uv1.ywyw);
			half3 c16 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.xy).xyz;
			half3 c17 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.zw).xyz;
			
			uv = _Scaler.xyxy * float4(17.312691, 17.312691, 19.0, 19.0) + input.uv0.xyxy;
			uv = clamp(uv, input.uv1.xzxz, input.uv1.ywyw);
			half3 c18 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.xy).xyz;
			half3 c19 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv.zw).xyz;
			
			half3 color = c0 * 8.2805367e-05 + c1 * 0.00039338661 + c2 * 0.001563755 + c3 * 0.0052015018
						+ c4 * 0.01447843 + c5 * 0.033726029 + c6 * 0.065747052 + c7 * 0.1072673
						+ c8 * 0.1464697 + c9 * 0.1673879 + c10 * 0.1601027 + c11 * 0.12816539
						+ c12 * 0.08586894 + c13 * 0.048148919 + c14 * 0.022594919 + c15 * 0.0088734962
						+ c16 * 0.0029162241 + c17 * 0.00080199027 + c18 * 0.0001845513 + c19 * 2.5098239e-05;

            return EncodeHDR(color);
		}

		VertexOutput VertMerge( VertexInput input  )
		{
			VertexOutput output = (VertexOutput)0;

        	output.positionCS = TransformWorldToHClip(input.positionOS.xyz);
			output.positionCS.zw = float2(0.0, 1.0);

        	output.uv0.xy = input.positionOS.xy * 0.5 + 0.5;
        	
			float2 uv = (input.positionOS.xy + 1.0) * 0.5;
			output.uv0.zw = uv * _UVTransformBlur1.xy + _UVTransformBlur1.zw;
			output.uv1.xy = uv * _UVTransformBlur2.xy + _UVTransformBlur2.zw;
			output.uv1.zw = uv * _UVTransformBlur3.xy + _UVTransformBlur3.zw;
			
			return output;
		}

		float4 FragMerge( VertexOutput input  ) : SV_Target
		{
            half3 c0 = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, input.uv0.xy).xyz;
            half3 c1 = SAMPLE_TEXTURE2D(_MultiBlurTex, sampler_MultiBlurTex, input.uv0.zw).xyz;
            half3 c2 = SAMPLE_TEXTURE2D(_MultiBlurTex, sampler_MultiBlurTex, input.uv1.xy).xyz;
            half3 c3 = SAMPLE_TEXTURE2D(_MultiBlurTex, sampler_MultiBlurTex, input.uv1.zw).xyz;
			
			half3 color = c0 * _BlurComposeWeights.x + c1 * _BlurComposeWeights.y
						+ c2 * _BlurComposeWeights.z + c3 * _BlurComposeWeights.w;

            return EncodeHDR(color);
		}

    ENDHLSL

	SubShader
	{
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
		Blend One Zero, One Zero
		Cull Off
		ZWrite Off
		ZTest Always
		Offset 0 , 0
		ColorMask RGBA

		Pass
		{
			Name "Bloom Prefilter"
			
            HLSLPROGRAM
				#pragma vertex VertBox
				#pragma fragment FragPrefilter
            ENDHLSL
		}

		Pass
		{
			Name "Bloom Gauss Blur Horizontal"
			
            HLSLPROGRAM
				#pragma vertex VertBlur
				#pragma fragment FragGaussBlur
            ENDHLSL
		}

		Pass
		{
			Name "Bloom Gauss Blur Vertical"
			
            HLSLPROGRAM
				#pragma vertex VertBlur
				#pragma fragment FragGaussBlur
            ENDHLSL
		}

		Pass
		{
			Name "DownSample"
			
            HLSLPROGRAM
				#pragma vertex VertBox
				#pragma fragment FragBoxBlur
            ENDHLSL
		}

		Pass
		{
			Name "BloomBlur9x9"
			
            HLSLPROGRAM
				#pragma vertex VertBlur
				#pragma fragment FragBlur9x9
            ENDHLSL
		}

		Pass
		{
			Name "BloomBlur16x16"
			
            HLSLPROGRAM
				#pragma vertex VertBlur
				#pragma fragment FragBlur16x16
            ENDHLSL
		}

		Pass
		{
			Name "BloomBlur20x20"
			
            HLSLPROGRAM
				#pragma vertex VertBlur
				#pragma fragment FragBlur20x20
            ENDHLSL
		}

		Pass
		{
			Name "BloomFinal"
			
            HLSLPROGRAM
				#pragma vertex VertMerge
				#pragma fragment FragMerge
            ENDHLSL
		}
	}
}