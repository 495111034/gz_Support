Shader "one" {
	Properties
	{
		_MainTex("_MainTex", 2D) = "black" {}
	}
	SubShader
	{
		Tags { "Queue" = "AlphaTest" "IgnoreProjector" = "True" "RenderType" = "TransparentCutout" }

		Pass
		{
			Tags { "LightMode" = "ForwardBase" }

			ZWrite Off
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				#include "UnityCG.cginc"

				sampler2D _MainTex; half4 _MainTex_ST;

				struct a2v
				{
					float4 vertex  : POSITION;
					half2 uv1      : TEXCOORD0;
				};

				struct v2f
				{
					float4 pos      : SV_POSITION;
					half2 uv1       : TEXCOORD0;
				};


				v2f vert(a2v v)
				{
					v2f o;
					o.pos = UnityObjectToClipPos(v.vertex);
					o.uv1 = v.uv1.xy * _MainTex_ST.xy + _MainTex_ST.zw;
					return o;
				}

				fixed4 frag(v2f f) : SV_Target
				{
					fixed4 color = tex2D(_MainTex, f.uv1.xy);
					color.a = 0.4f;
					return color;
				}
			ENDCG
		}
	}
}


