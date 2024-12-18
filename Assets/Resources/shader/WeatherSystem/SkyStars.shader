Shader "WeatherSystem/SkyStars" 
{	
	Properties
    { 
	    _Brightness("_Brightness", float) =   10.00
		_Color("_Color", Color) =  (0.15429, 0.72658, 1.00, 1.00)

    }
	SubShader {
		//Tags {"Queue" = "Background+2" "RenderType" = "Background"   "RenderPipeline" = "UniversalPipeline"}
		Tags {"Queue" = "Background+2" "RenderType" = "Background" }
		//Tags {"Queue"="Background" "RenderType" = "Geometry+501"   "RenderPipeline" = "UniversalPipeline"}
//		Blend OneMinusDstAlpha  OneMinusSrcAlpha	// alpha 0
//		Blend OneMinusDstAlpha  SrcAlpha			// alpha 1
		Cull Off         
        ZWrite Off   
		Blend SrcAlpha One
		ZTest LEqual
		//ZTest Off

	Pass{	
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
		uniform float _Brightness;
		half4 _Color;
		
		struct appdata_t {
			float4 vertex : POSITION;
			float4 ColorAndMag : COLOR;
			float2 texcoord : TEXCOORD;
		};
		
		struct v2f 
		{
			float4 pos : SV_POSITION;
			half4 Color : COLOR;
			half2 uv : TEXCOORD0;
		};
			

		#include "UnityCG.cginc"
		#include "WeatherCommon.cginc"
			
			float GetFlickerAmount(in float2 pos)
		{
			const float2 tab[8] = 
  			{
				float2(0.897907815,-0.347608525), float2(0.550299290, 0.273586675), float2(0.823885965, 0.098853070), float2(0.922739035,-0.122108860),
				float2(0.800630175,-0.088956800), float2(0.711673375, 0.158864420), float2(0.870537795, 0.085484560), float2(0.956022355,-0.058114540)
			};
	
			float2 hash = frac(pos.xy * 256);
			float index = frac(hash.x + (hash.y + 1) * (_Time.x * 2 + unity_DeltaTime.z)); // flickering speed
//			float index = frac(hash.x + (hash.y + 1) * (_Time.w * 5 * unity_DeltaTime.z)); // flickering speed
			index *= 8;

			float f = frac(index)* 2.5;
			int i = (int)index;

			// using default const tab array. 
			// occasionally this is not working for WebGL and some android build
			return tab[i].x + f * tab[i].y;
		}	
		
		v2f vert(appdata_t v)
		{
			v2f OUT = (v2f)0;

			half4 positionWS = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1.0));
			positionWS = half4(positionWS.xyz + _WorldSpaceCameraPos.xyz, positionWS.w);
			float4 positionCS = UnityWorldToClipPos(positionWS);
			OUT.pos = positionCS ;

			float appMag = 6.5 + v.ColorAndMag.w * (-1.44 -2.5);
			float brightness = GetFlickerAmount(v.vertex.xy) * pow(5.0, (-appMag -1.44)/ 2.5);
						
			half4 starColor = _Brightness * float4( brightness * _Color.xyz, brightness );
			
			// full spherical starfield billboard mesh and only render upper half
			OUT.Color = starColor * saturate( positionWS.y );
			
			OUT.uv = 5 * (v.texcoord.xy - float2(0.5, 0.5));
			
			return OUT;
		}

		half4 frag(v2f IN) : SV_Target
		{
			float2 distCenter = IN.uv.xy;
			float scale = exp(-dot(distCenter, distCenter));
			float3 colFinal = IN.Color.xyz * scale + 5 * IN.Color.w * pow(scale, 10);
			return half4( colFinal, 1);
		}
		ENDHLSL
	  }
	} 
}
