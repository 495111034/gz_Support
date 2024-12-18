Shader "hc/partical/AlphaAdd" {
Properties {
	[HDR]_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_MainTex ("Particle Texture", 2D) = "white" {}
	_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0

	[HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
	[HideInInspector] _Stencil ("Stencil ID", Float) = 0
	[HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
	[HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
	[HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
}

	CGINCLUDE
		#pragma multi_compile_particles
		//优化  #pragma multi_compile_fog
        #pragma multi_compile __ FOG_LINEAR
		#pragma multi_compile_instancing   
		#pragma multi_compile __ UNITY_UI_CLIP_RECT
		#define USING_FOG (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2))                 

		#include "UnityCG.cginc"
		#include "Lighting.cginc"
		#include "AutoLight.cginc"
		#include "UnityUI.cginc"

		sampler2D _MainTex;	
		float4 _MainTex_ST;	
		fixed _InvFade;
		float4 _ClipRect;

		UNITY_INSTANCING_BUFFER_START(MyProperties)  
        	UNITY_DEFINE_INSTANCED_PROP(fixed4,_TintColor)        
    	UNITY_INSTANCING_BUFFER_END(MyProperties)

		sampler2D_float _CameraDepthTexture;

		struct appdata_t {
			float4 vertex : POSITION;
			fixed4 color : COLOR;
			float2 texcoord : TEXCOORD0;

			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		struct v2f {				
			float4 vertex : SV_POSITION;
			fixed4 color : COLOR;
			float2 texcoord : TEXCOORD0;
			float4 pos : TEXCOORD1;

#ifdef SOFTPARTICLES_ON
			float4 projPos : TEXCOORD2;
#if USING_FOG
			fixed fog : TEXCOORD3;
#endif
#else
			fixed fog : TEXCOORD2;
#endif
			UNITY_VERTEX_OUTPUT_STEREO
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};	

		
		v2f vert(appdata_t v) {
			v2f o;
			UNITY_INITIALIZE_OUTPUT(v2f, o);  

			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
			UNITY_TRANSFER_INSTANCE_ID(v, o);				

			o.vertex = UnityObjectToClipPos(v.vertex);
			o.pos = v.vertex;   
#ifdef SOFTPARTICLES_ON
			o.projPos = ComputeScreenPos (o.vertex);
			COMPUTE_EYEDEPTH(o.projPos.z);
#endif
			o.color = v.color;
			o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);

			#if USING_FOG
				half3 eyePos = UnityObjectToViewPos(v.vertex);
				half fogCoord = length(eyePos.xyz);
				UNITY_CALC_FOG_FACTOR_RAW(fogCoord);
				o.fog = saturate(unityFogFactor);
			#endif       

			return o;
		}		
		
		
		fixed4 frag (v2f i) : SV_Target
		{
			UNITY_SETUP_INSTANCE_ID(i);

#ifdef SOFTPARTICLES_ON
			float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
			float partZ = i.projPos.z;
			float fade = saturate (_InvFade * (sceneZ-partZ));
			i.color.a *= fade;
#endif
			
			fixed4 col = 2.0f * i.color * UNITY_ACCESS_INSTANCED_PROP(MyProperties,_TintColor)  * tex2D(_MainTex, i.texcoord);
			#if USING_FOG
				col.rgb = lerp(unity_FogColor.rgb, col.rgb, i.fog);
			#endif
			
			#ifdef UNITY_UI_CLIP_RECT
				col.a *= UnityGet2DClipping(i.pos.xy, _ClipRect);
			#endif

			return col;
		}

	ENDCG


	SubShader {		
		LOD 200
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane"   }
		Pass {
			Tags { "LightMode" = "ForwardBase"  }	
			Stencil
			{
				Ref [_Stencil]
				Comp [_StencilComp]
				Pass [_StencilOp]
				ReadMask [_StencilReadMask]
				WriteMask [_StencilWriteMask]
			}		
			Blend SrcAlpha One			
			Cull Off Lighting Off ZWrite Off
		
			CGPROGRAM
			#pragma target 2.0
			#pragma vertex vert
			#pragma fragment frag

			ENDCG 
		}
	}	

}
