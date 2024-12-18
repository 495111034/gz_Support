// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "mapmask z" {
	Properties {
	}
	SubShader {
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		LOD 200
		ZTest LEqual
		ZWrite Off
		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha
		
		Pass{
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest
	
			#include "UnityCG.cginc"
	
			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
			};
	
			struct v2f {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
			};
			
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				return o;
			}
	
			fixed4 frag (v2f i) : COLOR
			{
				fixed4 col = i.color;
				return col;
			}
			
			ENDCG 
		}
	} 
	FallBack "Diffuse"
}