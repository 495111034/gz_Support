
Shader "ProjectorShadow/ShadowCaster"
{
	Properties
	{
		//_ShadowColor("Main Color", COLOR) = (1, 1, 1, 1)
	}
	CGINCLUDE
#include "UnityCG.cginc"
#include "Lighting.cginc"
#include "AutoLight.cginc"

		//#pragma exclude_path:prepass noforwardadd halfasview		
#pragma multi_compile_instancing	

//UNITY_INSTANCING_BUFFER_START(MyProperties)
//    UNITY_DEFINE_INSTANCED_PROP(fixed4,_ShadowColor)
//UNITY_INSTANCING_BUFFER_END(MyProperties)

struct a2v {
		half4 vertex : POSITION;
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};


	struct v2f
	{
		half4 pos : POSITION;
		UNITY_VERTEX_OUTPUT_STEREO
		UNITY_VERTEX_INPUT_INSTANCE_ID
	};

	v2f vert(a2v v)
	{
		v2f o;

		UNITY_SETUP_INSTANCE_ID(v);
		UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
		UNITY_TRANSFER_INSTANCE_ID(v,   o);      	

		o.pos = UnityObjectToClipPos(v.vertex);
		return o;
	}

	half4 frag(v2f i) :SV_TARGET
	{
		UNITY_SETUP_INSTANCE_ID(i); 
		return 0;
	}

		ENDCG
		SubShader
	{
		Tags{ "RenderType" = "Opaque" "Queue" = "Geometry" }
			LOD 200
			Pass
		{
			Tags { "LightMode" = "ForwardBase" }

			CGPROGRAM

			#pragma multi_compile_fwdbase
			#pragma vertex vert
			#pragma fragment frag

			ENDCG
		}
	}
	FallBack Off
}
