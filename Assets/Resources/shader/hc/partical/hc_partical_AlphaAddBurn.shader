Shader "hc/partical/AlpalAddBurn" {
Properties {
	[HDR]_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
	_MainTex ("Particle Texture", 2D) = "white" {}

    _BurnAmount ("消融进度", Range(0.0, 1.0)) = 0.0
    _LineWidth("消融过渡宽度", Range(0.0, 0.2)) = 0.1
    _BurnMap("消融噪音纹理(R通道)", 2D) = "white"{}
    [HDR]_BurnFirstColor("消融颜色1", Color) = (1, 0, 0, 1)
    [HDR]_BurnSecondColor("消融颜色2", Color) = (1, 0, 0, 1)    

	_BurnX ("消融纹理X速度", Range(-5,5)) = 0.0
    _BurnY ("消融纹理Y速度", Range(-5,5)) = 0.0
    _BurnRotation("消融纹理旋转速度",Range(-5,5)) = 0.0

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

	#include "UnityCG.cginc"
	#include "Lighting.cginc"
	#include "AutoLight.cginc"
	#include "UnityUI.cginc"

	sampler2D _MainTex;	
	float4 _MainTex_ST;	
	sampler2D _BurnMap;
	half4 _BurnMap_ST;	

	half4 _BurnFirstColor;
	half4 _BurnSecondColor;
	half _BurnAmount;
	half _LineWidth;
	fixed4 _TintColor;			
	fixed _InvFade;
	fixed _BurnX,_BurnY,_BurnRotation;
	uniform float4 _TimeEditor;
	float4 _ClipRect;

	//UNITY_INSTANCING_BUFFER_START(MyProperties)
	//	UNITY_DEFINE_INSTANCED_PROP(fixed4,_TintColor)		
	//    UNITY_DEFINE_INSTANCED_PROP(fixed,_InvFade)		
	//UNITY_INSTANCING_BUFFER_END(MyProperties)
	
	struct appdata_t {
		float4 vertex : POSITION;
		fixed4 color : COLOR;
		float2 texcoord : TEXCOORD0;

		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	struct v2f {				
		float4 vertex : SV_POSITION;
		fixed4 color : COLOR;
		float4 texcoord : TEXCOORD0;

		UNITY_FOG_COORDS(1)

#ifdef SOFTPARTICLES_ON
		float4 projPos : TEXCOORD2;
#endif
		float4 pos : TEXCOORD3;

		UNITY_VERTEX_OUTPUT_STEREO
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};
	
	

	v2f vert (appdata_t v)
	{
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
		o.texcoord.xy = TRANSFORM_TEX(v.texcoord,_MainTex);
		fixed4 _t = _Time + _TimeEditor; 
		fixed2 pivot = float2(0.5, 0.5);         //中心点
		fixed degrees = _t.y * _BurnRotation; 
		fixed sin1 = sin(degrees);
		fixed cos1 = cos(degrees);    
		fixed2x2 rot1 = float2x2(cos1, -sin1, sin1, cos1);
		o.color = v.color;				
		o.texcoord.zw = TRANSFORM_TEX( mul(rot1, (v.texcoord + frac(fixed2(_BurnX, _BurnY) * _t.y ) ) - pivot),_BurnMap) + pivot;
		UNITY_TRANSFER_FOG(o,o.vertex);
		return o;
	}
	
	
	sampler2D_float _CameraDepthTexture;
	fixed4 frag (v2f i) : SV_Target
	{
		UNITY_SETUP_INSTANCE_ID(i);

#ifdef SOFTPARTICLES_ON
		float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
		float partZ = i.projPos.z;
		float fade = saturate (_InvFade * (sceneZ-partZ));
		i.color.a *= fade;
#endif

		fixed4 col = 2.0f * i.color *_TintColor * tex2D(_MainTex, i.texcoord.xy);

		//消融
		fixed3 burn = tex2D(_BurnMap, i.texcoord.zw).rgb; 
		clip(burn.r - _BurnAmount);  
		fixed t = 1 - smoothstep(0.0, _LineWidth, burn.r - _BurnAmount);
		fixed3 burnColor = lerp(_BurnFirstColor, _BurnSecondColor, t);
		burnColor = pow(burnColor, 5);
		col.rgb = lerp(col.rgb,burnColor, t * step(0.0001, _BurnAmount));  
		
		
		UNITY_APPLY_FOG_COLOR(i.fogCoord, col, fixed4(0,0,0,0)); 
		#ifdef UNITY_UI_CLIP_RECT
			col.a *= UnityGet2DClipping(i.pos.xy, _ClipRect);
		#endif

		return col;
	}

ENDCG


	
	
	
	SubShader {
		LOD 200
		Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
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
			//Blend SrcAlpha One			
			Blend SrcAlpha One,Zero One
			Cull Off 
			Lighting Off 
			ZWrite Off
			ZTest [unity_GUIZTestMode]
			CGPROGRAM
			#pragma target 2.0
			#pragma vertex vert
			#pragma fragment frag
			ENDCG 
		}
	}	

}
