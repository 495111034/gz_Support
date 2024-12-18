
Shader "BF/Common/Albedo"
{
    Properties {
        _MainTex ("主纹理", 2D) = "white" {}
		_Color("主纹理混合颜色", Color) = (1,1,1,1)
		//Clip 
		[Toggle(CLIP_ON)] _ClipOn("开启提出", Float) = 0
		_ClipAlpha("剔除Alpha阈值",Range(0.001,1)) = 0.1
    }

    SubShader{
		LOD 200
        Tags {"RenderType"="Opaque" "IgnoreProjector"="true"}

        Pass{ 
			Name "ALBEDO_BASE"
			Tags {"LightMode"="ForwardBase"}
            CGPROGRAM
				#pragma multi_compile_fwdbase
				#pragma skip_variants VERTEXLIGHT_ON LIGHTPROBE_SH SHADOWS_SCREEN
				#pragma shader_feature CLIP_ON
			   
				#include "Lighting.cginc"
			   //优化  add
				#pragma skip_variants  LIGHTPROBE_SH DYNAMICLIGHTMAP_ON _SPECULARHIGHLIGHTS_OFF VERTEXLIGHT_ON 
				#pragma skip_variants  UNITY_SPECCUBE_BOX_PROJECTION UNITY_SPECCUBE_BLENDING 
				#pragma skip_variants FOG_EXP FOG_EXP2
				#pragma skip_variants DIRECTIONAL_COOKIE POINT_COOKIE SPOT _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

				#pragma vertex vert
				#pragma fragment frag

				struct a2v{
					float4 vertex : POSITION;
					float4 texcoord : TEXCOORD0;
				};

				struct v2f{
					float4 pos : SV_POSITION;
					float2 uv0 : TEXCOORD0;
				};

				sampler2D _MainTex; half4 _MainTex_ST;
				half4 _Color;
			#if CLIP_ON
				half _ClipAlpha;
			#endif

				v2f vert(a2v v){
					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);
					o.uv0 = v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw;
					return o;
				}

				half4 frag(v2f i) : SV_Target{
					fixed4 mainTex = tex2D(_MainTex, i.uv0);
					fixed3 albedo = mainTex.rgb * _Color.rgb;
				#if CLIP_ON
					clip(mainTex.a - _ClipAlpha);
				#endif
					half3 diffuse = _LightColor0.rgb * albedo;
					half4 color = half4(diffuse, 1);
					return color;
				}
            ENDCG
        }
    }

}
