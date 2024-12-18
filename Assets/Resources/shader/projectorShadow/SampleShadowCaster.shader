
Shader "ProjectorShadow/SampleShadowCaster"
{
	Properties
	{
		_ShadowColor("Main Color", COLOR) = (1, 1, 1, 1)
	}
	CGINCLUDE
		#include "UnityCG.cginc"
		#include "Lighting.cginc"
		#include "AutoLight.cginc"

		#pragma exclude_path:prepass noforwardadd halfasview		
		#pragma multi_compile_instancing	

		UNITY_INSTANCING_BUFFER_START(MyProperties)
            UNITY_DEFINE_INSTANCED_PROP(fixed4,_ShadowColor)
    	UNITY_INSTANCING_BUFFER_END(MyProperties)

		struct a2v {
			half4 vertex : POSITION;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};
	

		struct v2f
		{
			half4 pos : POSITION;           
            half3 rolePos: TEXCOORD1;
			UNITY_VERTEX_INPUT_INSTANCE_ID
		};

		v2f vert(a2v v)
		{
			v2f o;

			UNITY_SETUP_INSTANCE_ID(v);
			UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);			 	

			o.pos = UnityObjectToClipPos(v.vertex);
            half3 modelPos = mul(unity_ObjectToWorld, fixed4(0,0,0,1)).xyz;
            o.rolePos = mul(unity_ObjectToWorld, v.vertex).xyz - modelPos;
			return o;
		}

		half4 frag(v2f i) :SV_TARGET
		{		
            half dist =   length(i.rolePos);
            half swif = step(dist, UNITY_PI);        //a<=b为1,否则为0  
            half a = clamp( cos(fmod(dist , UNITY_PI) ),0,1) ;
			return (a) * swif * 1.1;
		}

	ENDCG
	SubShader
	{
		Tags { "Queue"="Transparent"  "RenderType"="Transparent"  "IgnoreProjector"="True"}
        //Tags{ "RenderType"="Opaque" "Queue"="Geometry" }
		LOD 200
		Pass
		{
			Tags { "LightMode"="ForwardBase" }
            Blend SrcAlpha One
			CGPROGRAM

			#pragma multi_compile_fwdbase
			#pragma vertex vert
			#pragma fragment frag
			
			ENDCG
		}
	}
	FallBack Off 
}
