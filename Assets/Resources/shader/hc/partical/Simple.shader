Shader "hc/partical/Simple"{
	Properties{
		_MainTex("Particle Texture", 2D) = "white" {}
		_Color("Color", Color) = (1,1,1,1)
		_Brightness("brightness",range(0.5,3)) = 1
		[KeywordEnum(OFF, ON)] _USEVCOLOR("启用顶点色参与颜色计算", float) = 0

		[HideInInspector][Toggle]_UVMove("main uv move",Float) = 0
		[HideInInspector]_USpeed("X Speed", Float) = 0
		[HideInInspector]_VSpeed("Y Speed", Float) = 0
		[HideInInspector][Toggle]_UseMask("has mask",Float) = 0
		[HideInInspector]_MaskTex("Mask Texture", 2D) = "white" {}
		[HideInInspector][Toggle]_MUVMove("mask uv move",Float) = 0
		[HideInInspector]_MUSpeed("Mask X Speed", Float) = 0
		[HideInInspector]_MVSpeed("Mask Y Speed", Float) = 0

		[HideInInspector]_Zwrite("Zwrite", Float) = 0
		[HideInInspector][Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend Mode", Float) = 5
		[HideInInspector][Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend Mode", Float) = 10
		[HideInInspector][Enum(UnityEngine.Rendering.CullMode)] _CullMode("Cull Mode", Int) = 0

		[HideInInspector][Toggle] _ShowStencil("Use Stencil", Float) = 0
		[HideInInspector][Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison", Float) = 8
		[HideInInspector]_Stencil("Stencil ID", Float) = 0
		[HideInInspector]_StencilOp("Stencil Operation", Float) = 0
		[HideInInspector]_StencilWriteMask("Stencil Write Mask", Float) = 255
		[HideInInspector]_StencilReadMask("Stencil Read Mask", Float) = 255
	}

	SubShader{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
		Pass{
			Blend[_SrcBlend][_DstBlend]
			Cull[_CullMode]
			Lighting Off
			ZWrite[_Zwrite]
			Stencil
			{
				Ref[_Stencil]
				Comp[_StencilComp]
				Pass[_StencilOp]
				ReadMask[_StencilReadMask]
				WriteMask[_StencilWriteMask]
			}
			//ztest off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile __ UNITY_UI_CLIP_RECT
			//#pragma shader_feature _USEVCOLOR_ON _USEVCOLOR_OFF
			#pragma shader_feature_local _USEVCOLOR_ON
			#pragma shader_feature_local _UVMOVE_ON
			#pragma shader_feature_local _MASK_ON
			#pragma shader_feature_local _MASK_UVMOVE_ON
			#include "UnityCG.cginc"
			#include "UnityUI.cginc"
			sampler2D _MainTex;
			fixed4 _MainTex_ST;
			fixed4 _Color;
			fixed _Brightness;

			#if _UVMOVE_ON
			fixed _USpeed;
			fixed _VSpeed;
			#endif

			#if _MASK_ON
			#if _MASK_UVMOVE_ON
			fixed _MUSpeed;
			fixed _MVSpeed;
			#endif
			sampler2D _MaskTex;
			float4 _MaskTex_ST;
			#endif
			float4 _ClipRect;

			struct appdata_t {
				float4 pos : POSITION;
				#if _USEVCOLOR_ON
				fixed4 color : COLOR;
				#endif
				fixed2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 pos : POSITION;
				#if _USEVCOLOR_ON
				fixed4 color : COLOR;
				#endif
				fixed2 mainTexcoord : TEXCOORD0;
				#if _MASK_ON
				fixed2 maskTexcoord : TEXCOORD1;
				#endif
				float4 localPos : TEXCOORD2;
			};

			v2f vert(appdata_t v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos(v.pos);
				o.localPos = v.pos;    
				#if _USEVCOLOR_ON
				o.color = v.color;
				#endif

				o.mainTexcoord = TRANSFORM_TEX(v.texcoord, _MainTex);

				#if _MASK_ON
				o.maskTexcoord = TRANSFORM_TEX(v.texcoord, _MaskTex);
				#endif
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				fixed2 mainUV = i.mainTexcoord;

				#if _UVMOVE_ON
				mainUV = frac(mainUV + fixed2(_USpeed, _VSpeed) * _Time.x);
				#endif

				fixed4 col = tex2D(_MainTex, mainUV) * _Color;

				#if _MASK_ON
				fixed2 maskUV = i.maskTexcoord;
				#if _MASK_UVMOVE_ON
				maskUV = frac(maskUV + fixed2(_MUSpeed, _MVSpeed) * _Time.x);
				#endif
				fixed maskValue = tex2D(_MaskTex, maskUV).a;
				col.a *= maskValue;
				#endif

				#if _USEVCOLOR_ON
				col = col * i.color;
				#endif

				#ifdef UNITY_UI_CLIP_RECT
					col.a *= UnityGet2DClipping(i.pos.xy, _ClipRect);
				#endif

				return fixed4(col.rgb * _Brightness, col.a);
			}
			ENDCG
		}
	}
	CustomEditor "SimpleShaderGUI"
}
