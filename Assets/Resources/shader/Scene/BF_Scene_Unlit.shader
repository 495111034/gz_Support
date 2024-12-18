Shader "BF/Scene/Unlit"
{
    Properties{
        _MainTex("主纹理", 2D) = "white" {}
        _MainColor("混合颜色", color) = (1, 1, 1, 1)
    }
    SubShader{
        Tags{"Queue" = "Background" "RenderType" = "Transparent"}
        Pass {
            Blend SrcAlpha OneMinusSrcAlpha
			ZWrite off
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct a2v {
                float4 vertex : POSITION;
                fixed4 color  : COLOR;
                float2 uv     : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                fixed4 color  : COLOR;
                float2 uv     : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };
            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _MainColor;

            v2f vert (a2v v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color;
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = i.color * tex2D(_MainTex, i.uv) * _MainColor;
                return col;
            }
            ENDCG
        }
    }
}
