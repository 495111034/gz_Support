Shader "WeatherSystem/SkyMoon1"
{
    Properties
    {
       _MainTex ("_MainTex", 2D) = "white" {}               
       _Brightness("_Brightness", float) =   10.00
       _Detail("_Detail", float) =   0.00
       _Distance("_Distance", float) =   10.00
       _Color("_Color", Color) = (0.21404, 0.21404, 0.21404, 1.00)   
    }
    SubShader
    {
        //Tags {"Queue" = "Background+4" "RenderType" = "Transparent"   "RenderPipeline" = "UniversalPipeline"}
        Tags {"Queue" = "Background+4"  "RenderType" = "Transparent"}
        LOD 100

        Pass
        {
            Cull Off         
            ZWrite Off   
            Blend SrcAlpha One
            ZTest LEqual
            HLSLPROGRAM
            
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
        
           struct Attributes
            {
                float4 positionOS : POSITION;         
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 vertex: SV_POSITION;                
                float2 uv: TEXCOORD0;
            };
            
            float4 _Color;
            float _Brightness;
            float _Detail;            
            float _Distance;
            sampler2D _MainTex;

            #include "UnityCG.cginc"	
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                float4 positionOS = float4(input.positionOS.xyz * 100 , 1.0);
                positionOS.z -= _Distance * 100;
                float4 positionWS = mul(unity_ObjectToWorld, positionOS);
                positionWS = float4(positionWS.xyz + _WorldSpaceCameraPos.xyz, positionWS.w);
                float4 positionCS = UnityWorldToClipPos(positionWS);
                
                output.vertex = positionCS;
                output.uv = input.uv;
                return output;
                               
            }

            float4 frag (Varyings input) : SV_TARGET {
                      
                float4 mainColor = tex2D(_MainTex, input.uv.xy);
                float3 detailNormal = mainColor.xyz + float3(-1.0, -1.0, -1.0);
               
                float4 color;
                color.w = mainColor.w * _Color.w;
                detailNormal = _Detail * detailNormal + float3(1.0, 1.0, 1.0);
                 
                detailNormal = detailNormal * _Color.xyz;
                detailNormal = (_Brightness + _Brightness) * detailNormal;
                color.xyz = min(detailNormal, float3(6.0, 6.0, 6.0));
                
                return color;
            }
            ENDHLSL
        }
    }
}
