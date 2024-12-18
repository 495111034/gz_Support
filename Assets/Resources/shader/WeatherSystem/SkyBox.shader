Shader "WeatherSystem/SkyBox"
{
    Properties
    {                  
       
       
       [HDR][Gamma]_ZenithColor("_ZenithColor", Color) =  (0.00557, 0.11459, 0.40619)
       [HDR][Gamma]_HorizonColor("_HorizonColor", Color) =  (0.55772, 1.48804, 1.6889)        
       _HorizonFalloff("_HorizonFalloff", float) =  4
       
       [HDR][Gamma]_MieScatterColor("_MieScatterColor", Color) =   (0.2902, 0.47843, 0.97255)
       _MieScatterFactor("_MieScatterFactor", float) =  0.619
       _MieScatterPower("_MieScatterPower", float) =  1.5
       //_SkyboxLightDir("_SkyboxLightDir", Vector) = (-0.03051, 0.72541, 0.68764, 0.35331)
       [Toggle]_Weather_IsOpening("_Weather_IsOpening", float) = 1.0
       
       _CloudMap("_CloudMap", 2D) = "black" {}
       _Mask("_Mask", Float) = 1
       _CloudMapOffset("_CloudMapOffset", Float) = 0
       _DistortionTex("_DistortionTex", 2D) = "white" {}
       _DistortionTile("_DistortionTile", Float) = 1
       _DistortionAmount("_DistortionAmount", Float) = 1
       _DistortionMirror("_DistortionMirror", Float) = 1
    }
    SubShader
    {
        //Tags {"Queue"="Background" "RenderType"="Background"  "RenderPipeline" = "UniversalPipeline"}
        //Tags {"Queue" = "Background" "RenderType" = "Background"  "RenderPipeline" = "UniversalPipeline"}       
        Tags {"RenderType"="Background" "Queue"="Background" "PreviewType"="Skybox"}      
        LOD 100

        Pass
        {
            Cull Off         
            ZWrite Off   
            Blend  SrcAlpha OneMinusSrcAlpha
            ZTest LEqual
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
                float3 uv : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 vertex: SV_POSITION;                
                float3 uv: TEXCOORD0;
                float3 positionWS: TEXCOORD1;
                float3 miePhase: TEXCOORD2; 
                FOG_INPUT_V2F(3)
            };
                   
            half3 _ZenithColor;
 	        half3 _HorizonColor;
 	        float _HorizonFalloff;            
            float _MieScatterFactor;
 	        half3 _MieScatterColor;
            //half3 _SkyboxLightDir;
            half _Weather_IsOpening;            

			sampler2D _CloudMap;
            half _Mask;
			sampler2D _DistortionTex;
			half _DistortionTile;
			half _DistortionAmount;
            half _CloudMapOffset;
            half _MieScatterPower;

            half _DistortionMirror;
            
            Varyings vert(Attributes input)
            {
                Varyings output;                                
                output.uv = input.uv;
                //float4 positionWS = mul(GetObjectToWorldMatrix(), float4( input.positionOS.xyz, 1.0));
                float4 positionWS = mul(unity_ObjectToWorld, float4(_ProjectionParams.z  * 1 *  input.positionOS.xyz, 1.0));
                positionWS = float4(positionWS.xyz + _WorldSpaceCameraPos.xyz, positionWS.w);
                float4 positionCS = UnityObjectToClipPos(positionWS);
                output.vertex = positionCS;

                float4 positionWS1 = mul(unity_ObjectToWorld, float4(100000.0f * input.positionOS.xyz, 1.0));
                positionWS1 = float4(positionWS1.xyz + _WorldSpaceCameraPos.xyz, positionWS.w);


                
                if (_DistortionMirror > 0.5)
                {
                    if (output.uv.y < 0.5)
                    {
                        output.uv.y = -(output.uv.y * 2.0 - 1.0);                     
                    }
                    else
                    {
                        output.uv.y = (output.uv.y * 2.0 - 1.0);
                    } 
                }
                
                output.positionWS = input.positionOS.xyz;
                
                float t = (-_MieScatterFactor) * _MieScatterFactor + 1.0;
                float t1 = _MieScatterFactor * _MieScatterFactor + 2.0;
                t = t / t1;

                // partial mie phase : approximated with the Cornette Shanks phase function
                output.miePhase.x = t * 0.119366199;
                output.miePhase.y = _MieScatterFactor * _MieScatterFactor + 1.0;
                output.miePhase.z = _MieScatterFactor + _MieScatterFactor;

                FOG_VERTEX(output, positionWS1, _Weather_IsOpening);

                return output;                
            }

            half4 frag(Varyings input) : SV_TARGET{

                
                 float3 normalPosWS = normalize(input.positionWS.xyz);                                
                 float cosTheta = dot(normalPosWS.xyz, _SkyboxLightDir.xyz);

                 float horizonOffset = 1.0 - saturate(normalPosWS.y);
                 horizonOffset = pow(horizonOffset, _HorizonFalloff);
                 horizonOffset = min(horizonOffset, 1.0);
                
                 float3 rayleigh = horizonOffset * (_HorizonColor.xyz -_ZenithColor.xyz ) + _ZenithColor.xyz;
                
                 // scattering phase
                 float miePhase = max(1e-5f, (-input.miePhase.z) * cosTheta + input.miePhase.y);
                 cosTheta = cosTheta * cosTheta + 1.0;
                
                 miePhase = pow(miePhase, _MieScatterPower);                
                
                 miePhase = input.miePhase.x / miePhase;
                 cosTheta = cosTheta * miePhase;

                 float2 panner = ( 0.1f * _Time.y * float2( 1,0 ) + ( input.uv.xy * _DistortionTile ));                                 
                 half3  cloudColor = tex2D( _CloudMap, (input.uv.xy + half2(_CloudMapOffset, 0) + ( tex2D( _DistortionTex, panner ) *  float2(0.002f * _DistortionAmount, 0.002f) ) ).rg );
                
                 half3 color = cloudColor * _Mask  + _MieScatterColor.xyz * cosTheta + rayleigh.xyz;
                
                 SKYBOX_FOG(input, color, _Weather_IsOpening);

                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}
