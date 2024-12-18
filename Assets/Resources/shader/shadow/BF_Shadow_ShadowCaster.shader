Shader "BF/Shadow/ShadowCaster"
{
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
		//Clip 
		[Toggle(CLIP_ON)] _ClipOn("开启提出", Float) = 0
		_ClipAlpha("剔除Alpha阈值",Range(0.001,1)) = 0.1
    }

    SubShader{
        Pass{
			Name "COMMON"
			Tags { "LightMode"="ShadowCaster" }
			CGPROGRAM
				#pragma multi_compile_shadowcaster
				#pragma shader_feature CLIP_ON
				#include "UnityCG.cginc"
				#pragma vertex vert
				#pragma fragment frag
				#pragma multi_compile_instancing

				struct a2v{
					float4 vertex   : POSITION;
					half3 normal    : NORMAL;
				#if CLIP_ON
					half2 uv0      : TEXCOORD0;
				#endif
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				struct v2f{
					float4 pos  : SV_POSITION;
				#if CLIP_ON
					half2 uv0  : TEXCOORD0;
				#endif
					UNITY_VERTEX_INPUT_INSTANCE_ID
				};

				#if CLIP_ON
					sampler2D _MainTex; half4 _MainTex_ST;
					half _ClipAlpha;
				#endif

				v2f vert(a2v v) {
					v2f o;
                	UNITY_SETUP_INSTANCE_ID(v);
				#if CLIP_ON
					o.pos = UnityObjectToClipPos(v.vertex);
					o.uv0 = TRANSFORM_TEX(v.uv0, _MainTex);  
				#endif
					TRANSFER_SHADOW_CASTER_NORMALOFFSET(o);
					return o;
				}

				fixed4 frag(v2f i) : SV_Target {
                	UNITY_SETUP_INSTANCE_ID(i);
				#if CLIP_ON
					half4 tex = tex2D(_MainTex, i.uv0);
					clip(tex.a - _ClipAlpha);
				#endif
					SHADOW_CASTER_FRAGMENT(i)
				}
			ENDCG
		}
		Pass{
			Name "DOUBLESIDE"
			Tags { "LightMode" = "ShadowCaster" }
			Cull [_Cull]
			CGPROGRAM
				#pragma shader_feature CLIP_ON
				#include "UnityCG.cginc"
				#pragma vertex vert
				#pragma fragment frag

				struct a2v {
					float4 vertex   : POSITION;
					half3 normal    : NORMAL;
				#if CLIP_ON
					half2 uv0      : TEXCOORD0;
				#endif
				};

				struct v2f {
					float4 pos  : SV_POSITION;
				#if CLIP_ON
					half2 uv0  : TEXCOORD0;
				#endif
				};

				#if CLIP_ON
					sampler2D _MainTex; half4 _MainTex_ST;
					half _ClipAlpha;
				#endif

				v2f vert(a2v v) {
					v2f o;
				#if CLIP_ON
					o.pos = UnityObjectToClipPos(v.vertex);
					o.uv0 = TRANSFORM_TEX(v.uv0, _MainTex);
				#endif
					TRANSFER_SHADOW_CASTER_NORMALOFFSET(o);
					return o;
				}

				fixed4 frag(v2f i) : SV_Target {
				#if CLIP_ON
					half4 tex = tex2D(_MainTex, i.uv0);
					clip(tex.a - _ClipAlpha);
				#endif
					SHADOW_CASTER_FRAGMENT(i)
				}
			ENDCG
		}
    }
}
