Shader "KK/Scene/Diffuse"
{
    Properties
    {
        [HDR]_Color ("Color Tint", Color) = (1, 1, 1, 1)
        _MainTex ("Main Tex", 2D) = "white" {}
    }

    CGINCLUDE

        #include "UnityCG.cginc"
        #include "Lighting.cginc"
        #include "AutoLight.cginc"       
        //#pragma multi_compile __MAPDEBUG  __NO_MAPDEBUG
        #include "Assets/Resources/shader/myshaders/common.cginc"
             
        #pragma multi_compile_fog       

        sampler2D _MainTex;
        half4 _MainTex_ST; 
        UNITY_INSTANCING_BUFFER_START(MyProperties)
            UNITY_DEFINE_INSTANCED_PROP(fixed4,_Color)
    	UNITY_INSTANCING_BUFFER_END(MyProperties)
        
		#define USING_FOG (defined(FOG_LINEAR) || defined(FOG_EXP) || defined(FOG_EXP2))  

        struct appdata
        {
            half4 vertex : POSITION;
            half3 uv0 : TEXCOORD0;            
            half3 normal : NORMAL;           
            
    #ifdef LIGHTMAP_ON 
            half3 lmUV : TEXCOORD1;
    #endif            
            
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct v2f
        {
            half4 uv0 : TEXCOORD0;  
            half4 pos : SV_POSITION;
            half4 worldPos : TEXCOORD1;
            half3 worldNormal : TEXCOORD2;
            half3 rolePos: TEXCOORD3; 
            half3 targetPos : TEXCOORD4;
    #if USING_FOG
			UNITY_FOG_COORDS(5)
            UNITY_SHADOW_COORDS(6)
    #else
            UNITY_SHADOW_COORDS(5)
    #endif
        
            UNITY_VERTEX_OUTPUT_STEREO
      		UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        v2f vert(appdata v)
        {
            v2f o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
            UNITY_TRANSFER_INSTANCE_ID(v,   o);   

            o.uv0.xy = v.uv0.xy * _MainTex_ST.xy + _MainTex_ST.zw;
            o.worldPos.xyz = mul(unity_ObjectToWorld,v.vertex).xyz;
    #ifdef LIGHTMAP_ON 
            o.uv0.zw = v.lmUV.xy * unity_LightmapST.xy + unity_LightmapST.zw;  
    #endif
            o.worldNormal = UnityObjectToWorldNormal(v.normal);
            o.pos = UnityObjectToClipPos(v.vertex);   
            o.rolePos =   _GameInfos[0].xyz - o.worldPos.xyz ;
            
            o.targetPos = _GameInfos[1].xyz - o.worldPos.xyz ;
           
            
    #if USING_FOG        
			UNITY_TRANSFER_FOG(o,o.pos);
    #endif

            //TRANSFER_SHADOW(o);
            return o;
        }


        //基本着色器
        fixed4 frag_base(v2f i) : SV_Target
        {
            UNITY_SETUP_INSTANCE_ID(i); 

            fixed4 tex = tex2D(_MainTex, i.uv0.xy);

            fixed4 albedo = fixed4( tex.rgb * UNITY_ACCESS_INSTANCED_PROP(MyProperties,_Color).rgb ,1.0 );	

            fixed3 worldNormal = normalize(i.worldNormal);
            fixed3 worldLightDir = normalize(UnityWorldSpaceLightDir(i.worldPos.xyz));
            
            fixed3 mainRoleColor = MainRoleLightColor(i.rolePos,albedo.rgb,worldLightDir,worldNormal);                 
            

           


            #ifdef LIGHTMAP_ON 	
                fixed4 bakedColorTex = UNITY_SAMPLE_TEX2D(unity_Lightmap, i.uv0.zw);
                half4 bakedColor = half4(DecodeLightmap(bakedColorTex), 1.0) ;  

                fixed4 col = fixed4(albedo.rgb * bakedColor.rgb,1);
            #else
                fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * albedo.rgb ;
				UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos.xyz);               

				//漫反射
				fixed3 diffuse = _LightColor0.rgb *  albedo.rgb  * max(0, dot(worldNormal, worldLightDir)) * atten;
				fixed4 col = fixed4(ambient + diffuse   , 1.0);	
            #endif 

           
            col.rgb += mainRoleColor;   
            

            col =  SceneColor(col,i.worldPos);   
           // col.rgb = fixed3(1,0,0); 
    #if USING_FOG
			UNITY_APPLY_FOG(i.fogCoord, col);
    #endif
                    
            return col;
        }      

        ENDCG

        SubShader
        {
            Tags { "RenderType"="Opaque" }

        
            Pass
            {
                Tags { "LightMode" = "ForwardBase"  }
                CGPROGRAM
                #pragma multi_compile_fwdbase
                #pragma multi_compile_instancing
                #pragma vertex vert
                #pragma fragment frag_base            
                
                ENDCG
            }
        }

    FallBack Off
}