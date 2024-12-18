Shader "WeatherSystem/SkyCloud"
{
    Properties
    {
       _CloudMap ("_CloudMap", 2D) = "white" {}      
       _StepSize("_StepSize", float) = 0.00733              
       [Gamma]_CloudAmbient("_CloudAmbient", Color) =  (0.39338, 0.63859, 0.98113, 0.00)
       [HDR][Gamma]_CloudLight("_CloudLight", Color) =  (2.99608, 2.71373, 2.22745, 0.00)       
       _CloudRotate("_CloudRotate", float) =  0.0
       _CloudOffset("_CloudOffset", float) =  0.0
       _Attenuation("_Attenuation", float) = 0.56
       _AlphaSaturation("_AlphaSaturation", float) = 2.61
       
       
       _Mask("_Mask", float) = 0.84
       _ScatterMultiplier("_ScatterMultiplier", float) = 1.0       
       [Toggle]_Weather_IsOpening("_Weather_IsOpening", float) = 1.0
    }
    SubShader
    {
        //Tags {"Queue" = "Background+8" "RenderType" = "Opaque"  "RenderPipeline" = "UniversalPipeline"}
        //Tags {"Queue" = "Background+8" "RenderType" = "Opaque"}
        Tags {"Queue" = "Geometry" "RenderType" = "Opaque"}
        LOD 100

        Pass
        {
            Cull Off         
            ZWrite Off   
            Blend  SrcAlpha OneMinusSrcAlpha

            
            HLSLPROGRAM
            
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag

            
            
            #include "WeatherCommon.cginc"
            #include "UnityCG.cginc"	
            
        
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                float2 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 vertex: SV_POSITION;                
                float2 uv: TEXCOORD0;
                float3 toSun: TEXCOORD1;
                float3 posParam: TEXCOORD2; 
                float3 scatterParam: TEXCOORD3;
                float3 lightDir: TEXCOORD4;
                FOG_INPUT_V2F(5)

                float3 posWS : TEXCOORD6;
               // half3 normalWS : TEXCOORD7;                
            };
                 
            half3 _CloudAmbient;
            half3 _CloudLight;
            float _Attenuation;
            float _AlphaSaturation;
            float _Mask;
            float _ScatterMultiplier;            
            float _CloudRotate;
            float _CloudOffset;

            float _StepSize;            
            sampler2D _CloudMap;
            half _Weather_IsOpening;
           


            float3 RotateAroundYInDegreesSky(half3 vertex, half degrees)
            {
                float alpha = degrees * 3.14f / 180.0;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                float2x2 m = float2x2(cosa, -sina, sina, cosa);
                return float3(mul(m, vertex.xz), vertex.y).xzy;
            }
            
            float2 RotateAroundYInDegreesTime(float degrees)
            {
                float alpha = degrees * 3.14f / 180.0;
                float sina, cosa;
                sincos(alpha, sina, cosa);
                return float2(sina, cosa);
            }
            
            Varyings vert(Attributes input)
            {
                float4 positionWS;
                
                Varyings output;
                float offsetValue = _CloudRotate;
                input.positionOS.y += _CloudOffset;
                
                positionWS.xyz = RotateAroundYInDegreesSky(_ProjectionParams.z * input.positionOS.xyz, offsetValue);
                positionWS = mul(unity_ObjectToWorld, half4(positionWS.xyz, 1.0));
                positionWS.xyz = positionWS.xyz + _WorldSpaceCameraPos.xyz;

                float4 positionWS1;
                positionWS1.xyz = RotateAroundYInDegreesSky(10000 * input.positionOS.xyz, offsetValue);
                positionWS1 = mul(unity_ObjectToWorld, half4(positionWS1.xyz, 1.0));
                positionWS1.xyz = positionWS1.xyz + _WorldSpaceCameraPos.xyz;

               
                
                float4 positionCS = UnityWorldToClipPos(positionWS);
                output.vertex = positionCS;
                
                output.posWS = positionWS;
                
                
                positionWS.xyz = positionWS.www * positionWS.xyz;

                float3 binormal = cross( input.normalOS, input.tangentOS.xyz ) * input.tangentOS.w;
                float3x3 rotation = float3x3( input.tangentOS.xyz, binormal, input.normalOS );

                half3 dir;
                
                dir = RotateAroundYInDegreesSky(_WorldSpaceLightPos0, -offsetValue);
                                
                output.toSun.xy = mul(rotation, dir).xy * _StepSize;               
               
                output.uv.xy = input.uv.xy;

                positionWS = mul(unity_ObjectToWorld, half4(_ProjectionParams.z * 0.5h * input.positionOS.xyz, 1.0));
                //positionWS.xyz = positionWS.xyz + _WorldSpaceCameraPos.xyz;
                output.posParam.xyz = positionWS.xyz;
         
                output.scatterParam.x = _ScatterMultiplier * 0.0195609424;
                output.scatterParam.yz = half2(1.5776, 1.51999998);
                output.lightDir.xyz = dir;


                FOG_VERTEX(output, positionWS1, _Weather_IsOpening);
                return output;                
            }

            half4 frag (Varyings input) : SV_TARGET {
                
                float3 positionWS;
                positionWS.xyz = normalize(input.posParam.xyz);
                
                float dotPL = dot(positionWS.xyz, input.lightDir.xyz);
                float scatter = (-input.scatterParam.z) * dotPL + input.scatterParam.y;
                float dotPL2 = dotPL * dotPL + 1.0;

                scatter = pow(scatter, 1.5);
                
                scatter = input.scatterParam.x / scatter;
                scatter = _ScatterMultiplier * 0.0596831031 + scatter;
                scatter = dotPL2 * scatter;

                // r = cloud density , g = Opacity mask
                //https://www.youtube.com/watch?v=TIgQ5baInBE http://terragen4.com/beta/
                float2 uv1 = input.toSun.xy + input.uv.xy;                
                float cloudColor1 = tex2D(_CloudMap, uv1).y;
                
                float4 uv3 = input.toSun.xyxy * float4(3.0, 3.0, 5.0, 5.0) + input.uv.xyxy;
                float cloudColor3 = tex2D(_CloudMap, uv3.xy).y;
                float cloudColor5 = tex2D(_CloudMap, uv3.zw).y;
                
                cloudColor3 = cloudColor3 + cloudColor3;
                cloudColor3 = cloudColor1 * 2.0 + cloudColor3;
                cloudColor5 = cloudColor5 * 2.0 + cloudColor3;
                
                float2 uv7 = input.toSun.xy * float2(7.0, 7.0) + input.uv.xy;
                float cloudColor7 = tex2D(_CloudMap, uv7).y;                
                cloudColor7 = cloudColor7 * 2.0 + cloudColor5;

                
                float atten  = (-_Attenuation) * cloudColor7 + scatter;
                atten = exp2(atten);
                //CloudLight > _CloudAmbient
                float3 cloudLSubA = max(0, (-_CloudAmbient.xyz) + _CloudLight.xyz);
                float3 color = atten * cloudLSubA + _CloudAmbient.xyz;
                
                SKYBOX_FOG(input, color, _Weather_IsOpening);
                         
                half cloudColor = tex2D(_CloudMap, input.uv.xy).x;
                cloudColor = cloudColor * _Mask;
                cloudColor = min(cloudColor, 1.0);
                cloudColor = pow(cloudColor, _AlphaSaturation);  
                      
              
                                
                return  half4(color, cloudColor);
            }
            ENDHLSL
        }
    }
}
