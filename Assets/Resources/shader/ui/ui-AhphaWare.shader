Shader "MyShaders/UI/WareAlpha" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}  
        _Color ("Tint", Color) = (1,1,1,1)            
    }

    CGINCLUDE
        #include "UnityCG.cginc"
        #include "Lighting.cginc"
		#include "AutoLight.cginc"       

        #pragma multi_compile_instancing       
        #pragma exclude_path:prepass noforwardadd halfasview

        //uniform float _time;
        //uniform float _WaveStrong;
        uniform float _offsetX;
        
        uniform sampler2D _MainTex;
        half4 _MainTex_ST; 

#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
        StructureBuffer<half4> _ColorBuffer;        
#endif

        struct VertexInput {
            half4 vertex : POSITION;
            half2 texcoord0 : TEXCOORD0;

            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct VertexOutput {
            half4 pos : SV_POSITION;
            half2 uv : TEXCOORD0;
            
            UNITY_VERTEX_OUTPUT_STEREO
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        UNITY_INSTANCING_BUFFER_START(MyProperties)
            UNITY_DEFINE_INSTANCED_PROP(half4,_Color) 
        UNITY_INSTANCING_BUFFER_END(MyProperties)

        VertexOutput vert (VertexInput v) 
        {
            VertexOutput o ;
            UNITY_SETUP_INSTANCE_ID(v)
            UNITY_TRANSFER_INSTANCE_ID(v,o);  

            float4 offset;
            offset.yzw = float3(0.0, 0.0, 0.0);

            //half height = v.texcoord0.y;
            //offset.x =  sin(UNITY_PI * _time * clamp((1-height-0.1),0,1)) * _WaveStrong ;    
            offset.x = _offsetX *(1- v.texcoord0.y);
            o.pos = UnityObjectToClipPos(v.vertex + offset);

            o.uv = v.texcoord0.xy * _MainTex_ST.xy + _MainTex_ST.zw;      
            return o;
        }

        half4 frag(VertexOutput i) : COLOR 
        {
            UNITY_SETUP_INSTANCE_ID(i);
           
            half4 __color;
#ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            __color = _ColorBuffer[unity_InstanceID];
#else
            __color = UNITY_ACCESS_INSTANCED_PROP(MyProperties,_Color);
#endif

            half4 finalColor = tex2D(_MainTex, i.uv)  ;    
            finalColor.rgb =    finalColor.rgb *  __color;  
           
            return finalColor;
        }
       
    ENDCG

    SubShader {
        LOD 100
        Tags {"IgnoreProjector"="True" "Queue"="Transparent" "RenderType"="Transparent" "DisableBatching" = "True" "PreviewType"="Plane" "CanUseSpriteAtlas"="True"}

        Pass {
            Tags { "LightMode"="ForwardBase" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Lighting Off
            cull off

            CGPROGRAM
            #pragma vertex vert  
            #pragma fragment frag 
            #pragma target 2.0

            ENDCG
        }

       
    }


    FallBack Off
}
