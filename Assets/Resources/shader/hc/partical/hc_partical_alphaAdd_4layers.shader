Shader "hc/partical/AlphaAdd_4layers" {
Properties {	

	_FirstTex ("第一纹理", 2D) = "white" {}
    _FirstX ("第一纹理X速度", Range(-5,5)) = 0.0
    _FirstY ("第一纹理Y速度", Range(-5,5)) = 0.0
    _FirstRotation("第一纹理旋转速度",Range(-5,5)) = 0.0
    [HDR]_TintColor1("第一纹理颜色",Color) = (1,1,1,1)
	_EmissionFlashSpeed1("第一纹理闪烁速度",range(0,10)) = 0
	_EmissionFlashMinValue1("第一纹理闪烁最小值",range(0,1))    = 1
	_EmissionScale1("第一纹理闪烁亮度",range(0,5))            = 1 

    _SecondTex("第二纹理",2D) = "black"{}
    _SecondX ("第二纹理X速度", Range(-5,5)) = 0.0
    _SecondY ("第二纹理Y速度", Range(-5,5)) = 0.0
    _SecondRotation("第二纹理旋转速度",Range(-5,5)) = 0.0
    [HDR]_TintColor2("第二纹理颜色",Color) = (1,1,1,1)
	_EmissionFlashSpeed2("第二纹理闪烁速度",range(0,10)) = 0
	_EmissionFlashMinValue2("第二纹理闪烁最小值",range(0,1))    = 1
	_EmissionScale2("第二纹理闪烁亮度",range(0,5))            = 1 

    _ThirdTex("第三纹理",2D) = "black" {}
    _ThirdX ("第三纹理X速度", Range(-5,5)) = 0.0
    _ThirdY ("第三纹理Y速度", Range(-5,5)) = 0.0
    _ThirdRotation("第三纹理旋转速度",Range(-5,5)) = 0.0
    [HDR]_TintColor3("第三纹理颜色",Color) = (1,1,1,1)
	_EmissionFlashSpeed3("第三纹理闪烁速度",range(0,10)) = 0
	_EmissionFlashMinValue3("第三纹理闪烁最小值",range(0,1))    = 1
	_EmissionScale3("第三纹理闪烁亮度",range(0,5))            = 1 

    _FourthTex("第四纹理",2D) = "black" {}
    _FourthX ("第四纹理X速度", Range(-5,5)) = 0.0
    _FourthY ("第四纹理Y速度", Range(-5,5)) = 0.0
    _FourthRotation("第四纹理旋转速度",Range(-5,5)) = 0.0
    [HDR]_TintColor4("第四纹理颜色",Color) = (1,1,1,1)
	_EmissionFlashSpeed4("第四纹理闪烁速度",range(0,10)) = 0
	_EmissionFlashMinValue4("第四纹理闪烁最小值",range(0,1))    = 1
	_EmissionScale4("第四纹理闪烁亮度",range(0,5))            = 1

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
	//#pragma multi_compile __USING_FOG  __NOTUSING_FOG            
	//#pragma shader_feature __USING_FOG
	#include "UnityCG.cginc"
	#include "Lighting.cginc"
	#include "AutoLight.cginc"
	#include "UnityUI.cginc"

	sampler2D _FirstTex;	
	float4 _FirstTex_ST;	
	sampler2D _SecondTex;	
	float4 _SecondTex_ST;	
	sampler2D _ThirdTex;	
	float4 _ThirdTex_ST;	
	sampler2D _FourthTex;	
	float4 _FourthTex_ST;	
	float4 _ClipRect;

	uniform float4 _TimeEditor;

	fixed _FirstX,_FirstY,_SecondX,_SecondY,_ThirdX,_ThirdY,_FourthX,_FourthY,_FirstRotation,_SecondRotation,_ThirdRotation,_FourthRotation;	
	fixed4 _TintColor1,_TintColor2,_TintColor3,_TintColor4;	

	fixed _EmissionFlashSpeed1,_EmissionFlashMinValue1,_EmissionScale1,
		_EmissionFlashSpeed2,_EmissionFlashMinValue2,_EmissionScale2,
		_EmissionFlashSpeed3,_EmissionFlashMinValue3,_EmissionScale3,
		_EmissionFlashSpeed4,_EmissionFlashMinValue4,_EmissionScale4;		
	
	struct appdata_t {
		float4 vertex : POSITION;
		fixed4 color : COLOR;
		float2 texcoord : TEXCOORD0;

		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	struct v2f {				
		float4 vertex : SV_POSITION;
		fixed4 color : COLOR;                
		float4 uv12 : TEXCOORD0;
		float4 uv34 : TEXCOORD1;
		float4 pos : TEXCOORD2;

		#if USING_FOG 
			fixed fog : TEXCOORD3;
		#endif


		UNITY_VERTEX_OUTPUT_STEREO
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};
	
	//计算图片的offset值
	//oldSt:原始的offset值
	//_t:速度
	fixed2 _GetSTBySpeed(fixed2 oldSt,fixed xSpeed,fixed ySpeed,fixed _t)
	{
		fixed2 newSt;
		newSt.x =  (1-step(abs(xSpeed),0)) * clamp(frac(_t * xSpeed ) * 2 - 1,-1,1) + step(abs(xSpeed),0) * oldSt.x;
		newSt.y =  (1-step(abs(ySpeed),0)) * clamp(frac(_t * ySpeed ) * 2 - 1,-1,1) + step(abs(ySpeed),0) * oldSt.y;
		return newSt;
	}


	v2f vert (appdata_t v)
	{
		v2f o;
		UNITY_INITIALIZE_OUTPUT(v2f, o);
		UNITY_SETUP_INSTANCE_ID(v);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
		UNITY_TRANSFER_INSTANCE_ID(v,   o);        

		o.vertex = UnityObjectToClipPos(v.vertex);

		o.pos = v.vertex;    
		o.color = v.color;    

		fixed4 _t = _Time + _TimeEditor;
		fixed degrees1 = _t.y * _FirstRotation;     
		fixed degrees2 = _t.y * _SecondRotation;    
		fixed degrees3 = _t.y * _ThirdRotation;      
		fixed degrees4 = _t.y * _FourthRotation;   

		fixed2 pivot = float2(0.5, 0.5);         //中心点
		
		fixed sin1 = sin(degrees1);
		fixed cos1 = cos(degrees1);
		fixed2x2 rot1 = float2x2(cos1, -sin1, sin1, cos1);

		fixed sin2 = sin(degrees2); 
		fixed cos2 = cos(degrees2);
		fixed2x2 rot2 = float2x2(cos2, -sin2, sin2, cos2);

		fixed sin3 = sin(degrees3); 
		fixed cos3 = cos(degrees3);
		fixed2x2 rot3 = float2x2(cos3, -sin3, sin3, cos3);

		fixed sin4 = sin(degrees4); 
		fixed cos4 = cos(degrees4);
		fixed2x2 rot4 = float2x2(cos4, -sin4, sin4, cos4);

		o.uv12.xy = mul(rot1,(v.texcoord.xy  * _FirstTex_ST.xy +  _GetSTBySpeed(_FirstTex_ST.zw,_FirstX,_FirstY, _t.y)) - pivot)+ pivot;
		o.uv12.zw = mul(rot2, (v.texcoord.xy  * _SecondTex_ST.xy + _GetSTBySpeed(_SecondTex_ST.zw,_SecondX,_SecondY, _t.y))- pivot)+ pivot;				
		o.uv34.xy = mul(rot3, (v.texcoord.xy  * _ThirdTex_ST.xy + _GetSTBySpeed(_ThirdTex_ST.zw,_ThirdX,_ThirdY, _t.y)) - pivot) + pivot;
		o.uv34.zw = mul(rot4, (v.texcoord* _FourthTex_ST.xy + _GetSTBySpeed(_FourthTex_ST.zw,_FourthX,_FourthY, _t.y)) - pivot)   + pivot;
		//o.uv12.xy = TRANSFORM_TEX( mul(rot1, (v.texcoord + frac(fixed2(_FirstX, _FirstY) * _t.y ) ) - pivot),_FirstTex) + pivot;
		//o.uv12.zw = TRANSFORM_TEX( mul(rot2, (v.texcoord + frac(fixed2(_SecondX, _SecondY) * _t.y ) ) - pivot),_SecondTex) + pivot;
		//o.uv34.xy = TRANSFORM_TEX( mul(rot3, (v.texcoord + frac(fixed2(_ThirdX, _ThirdY) * _t.y )  ) - pivot),_ThirdTex) + pivot;
		//o.uv34.zw = TRANSFORM_TEX( mul(rot4, (v.texcoord + frac(fixed2(_FourthX, _FourthY) * _t.y )  ) - pivot),_FourthTex) + pivot;

		#if USING_FOG 
			half3 eyePos = UnityObjectToViewPos(v.vertex);
			half fogCoord = length(eyePos.xyz);
			UNITY_CALC_FOG_FACTOR_RAW(fogCoord);
			o.fog = saturate(unityFogFactor);
		#endif          
		return o;
	}

	sampler2D_float _CameraDepthTexture;
	
	
	fixed4 frag (v2f i) : SV_Target
	{
		UNITY_SETUP_INSTANCE_ID(i);
		fixed4 _t = _Time + _TimeEditor;

		fixed4 _FirstTexColor = tex2D(_FirstTex, i.uv12.xy);
		_FirstTexColor =  _FirstTexColor * _EmissionScale1 * (clamp(sin(  UNITY_PI * _t.y * _EmissionFlashSpeed1 ) , _EmissionFlashMinValue1,1) );	
		fixed4 firstLayer = fixed4(_FirstTexColor.rgb * _TintColor1.rgb * _FirstTexColor.a,_FirstTexColor.a * _TintColor1.a);

		fixed4 _SecondTexColor =  tex2D(_SecondTex, i.uv12.zw);
		_SecondTexColor =  _SecondTexColor * _EmissionScale2 * (clamp(sin(  UNITY_PI * _t.y * _EmissionFlashSpeed2 ) , _EmissionFlashMinValue2,1) );	
		fixed4 secondLayer = fixed4(_SecondTexColor.rgb * _TintColor2.rgb * _SecondTexColor.a,_SecondTexColor.a * _TintColor2.a);

		fixed4 _ThirdTexColor = tex2D(_ThirdTex, i.uv34.xy);
		_ThirdTexColor =  _ThirdTexColor * _EmissionScale3 * (clamp(sin(  UNITY_PI * _t.y * _EmissionFlashSpeed3 ) , _EmissionFlashMinValue3,1) );	
		fixed4 thirdLayer = fixed4(_ThirdTexColor.rgb * _TintColor3.rgb * _ThirdTexColor.a,_ThirdTexColor.a * _TintColor3.a);

		fixed4 _FourthTexColor = tex2D(_FourthTex,i.uv34.zw) ;
		_FourthTexColor =  _FourthTexColor * _EmissionScale4 * (clamp(sin(  UNITY_PI * _t.y * _EmissionFlashSpeed4 ) , _EmissionFlashMinValue4,1) );	
		fixed4 fourthLayer =fixed4(_FourthTexColor.rgb * _TintColor4.rgb * _FourthTexColor.a,_FourthTexColor.a * _TintColor4.a);

		
		fixed4 col = 2.0f * i.color  * (firstLayer + secondLayer + thirdLayer + fourthLayer);
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

}
