Shader "hc/partical/AlphaAdd_UI" {
Properties {
	_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_MainTex ("Particle Texture", 2D) = "white" {}	

	[HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
	[HideInInspector] _Stencil ("Stencil ID", Float) = 0
	[HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
	[HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
	[HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
}

CGINCLUDE
	#pragma multi_compile_particles
	#pragma multi_compile_instancing
	#pragma multi_compile __ UNITY_UI_CLIP_RECT
	#include "UnityCG.cginc"
	#include "UnityUI.cginc"
	
	sampler2D _MainTex;
	float4 _MainTex_ST;
	fixed4 _TintColor;		
	float4 _ClipRect;	
	
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

		UNITY_VERTEX_INPUT_INSTANCE_ID
	};
	
	

	v2f vert (appdata_t v)
	{
		v2f o;
		UNITY_INITIALIZE_OUTPUT(v2f, o);  
		UNITY_SETUP_INSTANCE_ID(v)
		UNITY_TRANSFER_INSTANCE_ID(v,o);    
		o.vertex = UnityObjectToClipPos(v.vertex);  
		o.pos = v.vertex;                 
		o.color = v.color;
		o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);				
		return o;
	}
	
	
	fixed4 frag (v2f i) : SV_Target
	{		
		UNITY_SETUP_INSTANCE_ID(i);              
		fixed4 col =  2.0f * i.color * _TintColor * tex2D(_MainTex, i.texcoord);				
		#ifdef UNITY_UI_CLIP_RECT
			col.a *= UnityGet2DClipping(i.pos.xy, _ClipRect);
		#endif
		return col;
	}

ENDCG 
	
	

SubShader {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" "RenderTag" = "AlphaBlend"}
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

Fallback Off
}
