
Shader "ProjectorShadow/ShadowReceive" 
{
	Properties 
	{
		_ShadowTex ("Shadow Texture", 2D) = "gray" {}
		_FalloffTex ("Falloff Texture",2D) = "white"{}
		_Intensity ("Intensity",Range(0,6)) = 0.5
		_DepthOffset("偏移", float) = 0.001
		[Toggle(CLIP_ON)] _ClipOn("开启剔除", Float) = 0
		//_Test("test", Range(0, 1)) = 0
		[Header(Stencil)]
		[IntRange] refValue("ref value", range(0, 255)) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)] compOp("Comp", float) = 8
		[Enum(UnityEngine.Rendering.StencilOp)] passOp("passOp", float) = 0
		[IntRange] readMask("read mask", range(0, 255)) = 255
		[IntRange] writeMask("write mask", range(0, 255)) = 255
	}
	
	SubShader 
	{
		Tags { "Queue"="AlphaTest+1" }
			
		Pass 
		{
			ZWrite off
			ColorMask RGB
			Cull off
			Blend DstColor SrcColor
			Offset -1, -1

			Stencil
			{
				Ref [refValue]
				Comp [compOp]
				Pass [passOp]
				Fail keep
				ZFail keep
				ReadMask [readMask]
				WriteMask [writeMask]
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			//#pragma multi_compile_fog
            //#pragma enable_d3d11_debug_symbols
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "../Common/BingFengCG.cginc"    

			#pragma multi_compile _ FOG_HEIGHT
            #pragma shader_feature_local CLIP_ON 
			#pragma multi_compile_instancing	

			struct a2v {
				half4 vertex : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f 
			{
				float4 pos:POSITION;
				float4 sproj:TEXCOORD0;
                float4 screenPos : TEXCOORD1;
				#if USING_FOG
					UNITY_FOG_COORDS(2)
				#endif
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			float4x4 unity_Projector;
			float4x4 projectorVP;
			sampler2D _ShadowTex;
			sampler2D _FalloffTex;
			sampler2D _CameraDepthTexture;
			float _DepthOffset;
			half _Intensity;
			//half _Test;

			v2f vert(a2v v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
            	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
            	//UNITY_TRANSFER_INSTANCE_ID(vertex,   o);      

				float3 positionWS = mul(unity_ObjectToWorld, v.vertex);
				o.pos = UnityWorldToClipPos(positionWS);
			#if INSTANCING_ON
				o.sproj = mul(projectorVP, float4(positionWS, 1));
			#else
				o.sproj = mul(unity_Projector, v.vertex);
			#endif

				o.screenPos = ComputeScreenPos(o.pos);

			#if USING_FOG
            #if defined(FOG_HEIGHT)
                UNITY_TRANSFER_FOG(o, positionWS);
            #else
                UNITY_TRANSFER_FOG(o, o.pos);
            #endif
            #endif

				return o;
			}

			float4 frag(v2f i):SV_TARGET
			{
				UNITY_SETUP_INSTANCE_ID(i); 
			#if CLIP_ON
				float eyeDepth = LinearEyeDepth(tex2Dproj(_CameraDepthTexture, i.screenPos).r);
				float depth = (eyeDepth - i.screenPos.w);
				clip(_DepthOffset - depth);
			#endif
			#if INSTANCING_ON
				i.sproj.xy = i.sproj.xy * 0.5 + 0.5;
			#endif
				i.sproj.z = i.sproj.z  * 0.5 + 0.5;
				float2 sprojUV = i.sproj / i.sproj.w;
				float4 shadowCol = tex2D(_ShadowTex, sprojUV);
				float4 a = shadowCol.rrrr;
				//return a > _Test ? a : 0;
			#if UNITY_REVERSED_Z
				a = 1 - a;
			#endif
				half inShadow = step(a, i.sproj.z);
				half outBox = dot(step(1, i.sproj.xyz), half3(1, 1, 1)) + dot(step(i.sproj.xyz, 0), half3(1, 1, 1));
				if (inShadow < 0.5 || outBox > 0.5)
				{
					a = 1;
					clip(-1);
				}
				else
				{
					a = _Intensity * unity_ShadowColor;
				}
			#if FOG_HEIGHT
				a = lerp(a, 1, i.fogCoord.w);
            #endif
				
				return a;
			}

			ENDCG
		}
	}
	
	FallBack Off
}
