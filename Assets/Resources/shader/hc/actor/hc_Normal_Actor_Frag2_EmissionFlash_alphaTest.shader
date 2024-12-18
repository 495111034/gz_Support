Shader "hc/Charactor/Actor_Normal_Frag2 Emission_Flash_alphaTest"
{
    Properties
    {
        _MainTex("主纹理", 2D)                 = "white" {}
        _Color("混合颜色", Color)              =(1, 1, 1, 1)   
        _Cutoff("透明度阀值",range(0,1))             = 0.5     
        _BumpMap("法线纹理", 2D)                 = "bump" {}
        _Mask("遮罩纹理,高光(R)，自发光(G)，溜光(B),环境反射(A)",2D) = "black" {}

        [HDR]_Emission("自发光颜色",Color)                =  (0,0,0,0)
        _EmissionScale("自发光亮度",range(0,5))            = 1 
        _EmissionFlashSpeed("闪烁速度",range(0,10))         = 1
        _EmissionFlashMinValue("波动最小值",range(0,1))    = 0.5

        [HDR]_Specular("高光颜色", Color)       = (1, 1, 1, 1)
        _SpecularScale ("高光倍数", Float) =        1.0
        _Gloss("高光范围", Range(0, 512)) =         20
        _BumpScale("凹凸倍数", Range(-5, 5)) = 1.0
        _HeightScale("高度倍数（法线Z方向）",Range(-1,1)) = 0.1

        [HDR]_FresnelColor	 ("菲涅尔颜色", Color) = (1,1,1,1)
		_FresnelScale ("菲涅尔倍数", Range(0, 1)) = 0	
		_FresnelBias("菲涅尔范围", Range(0, 2)) = 0

        _HaloTex("溜光纹理",2D)                    = "white" {}
        [HDR]_HaloColor("溜光颜色", Color)       = (1, 1, 1, 1)
        _HaloScale ("溜光倍数",  Range(0, 3)) =        1.0
        _HaloXSpeed("溜光X速度",Range(0, 10)) =        1.0
        _HaloYSpeed("溜光Y速度",Range(0, 10)) =        1.0

        _Cubemap ("天空盒（用于反射的环境模拟）", Cube) = "_Skybox" {}
        _RefScale ("反射倍数", Range(0, 1)) = 1		
		_RefBias("反射范围", Range(0, 2)) = 1    

        _ScatteringMap("次表面散射遮罩",2D) = "white" {}   
        _ScatteringColor("次表面散射颜色",color) = (0.93,0.22,0.22,1)
    }
  


CGINCLUDE
    
    #include "UnityCG.cginc"
    #include "Lighting.cginc"
    #include "AutoLight.cginc"
    #include "Assets/Resources/shader/myshaders/common.cginc"

    //#pragma multi_compile_instancing
    //#pragma exclude_path:prepass nolightmap noshadow noforwardadd halfasview

    #define USING_FOG (defined(FOG_LINEAR) )
    //#pragma multi_compile_fog     
    //优化 
    #pragma multi_compile __ FOG_LINEAR

    //优化  add
    #pragma skip_variants  LIGHTPROBE_SH DYNAMICLIGHTMAP_ON DIRLIGHTMAP_COMBINED _SPECULARHIGHLIGHTS_OFF VERTEXLIGHT_ON 
    #pragma skip_variants  UNITY_SPECCUBE_BOX_PROJECTION UNITY_SPECCUBE_BLENDING 
    #pragma skip_variants FOG_EXP FOG_EXP2
    #pragma skip_variants DIRECTIONAL_COOKIE POINT_COOKIE SPOT _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A



    struct appdata
    {
        half4 vertex : POSITION;
        fixed2 uv0 : TEXCOORD0;       
        half3 normal : NORMAL;
        half4 tangent : TANGENT;

        UNITY_VERTEX_INPUT_INSTANCE_ID
    };


    struct v2f
    {
        float4 pos      : SV_POSITION;
        float4 uv       : TEXCOORD0;       
        
        half4 viewDir  : TEXCOORD1;
        half3 lightDir : TEXCOORD2;
        half3 worldPos : TEXCOORD3;
        half3 modelPos : TEXCOORD4; //模型中心点的世界坐标

        #if USING_FOG
            fixed fog : TEXCOORD5;
            UNITY_SHADOW_COORDS(6)
        #else
            UNITY_SHADOW_COORDS(5)
        #endif
        
        
        UNITY_VERTEX_OUTPUT_STEREO
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

  
   
    sampler2D _MainTex;
    sampler2D _BumpMap;
    sampler2D _Mask;
    sampler2D _HaloTex;
    samplerCUBE _Cubemap;     
    half4 _MainTex_ST; 
    half4 _BumpMap_ST;   
    half4 _Mask_ST;
    half4 _HaloTex_ST; 
    half4 _Emission;
    half4 _FresnelColor;
    half _FresnelScale;
    half _FresnelBias;
    fixed4 _Specular;
    fixed _Gloss;	
    fixed _BumpScale;	
    fixed _HeightScale;		
    fixed _SpecularScale;
    fixed _HaloScale;
    fixed4 _HaloColor;
    fixed _HaloXSpeed;     
    fixed _HaloYSpeed;         
    half _EmissionScale;
    uniform float4 _TimeEditor;

    fixed _RefScale;		
	fixed _RefBias;	

    fixed _EmissionFlashSpeed;
    fixed _EmissionFlashMinValue;

    sampler2D _ScatteringMap;
    float4 _ScatteringMap_ST;
    fixed4 _ScatteringColor;


    UNITY_INSTANCING_BUFFER_START(MyProperties)  
        UNITY_DEFINE_INSTANCED_PROP(fixed4,_Color)	        
        UNITY_DEFINE_INSTANCED_PROP(fixed,_Cutoff)
    UNITY_INSTANCING_BUFFER_END(MyProperties)

    v2f vert(appdata v)
    {
        v2f o;

        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
        UNITY_TRANSFER_INSTANCE_ID(v,   o);   

        o.pos = UnityObjectToClipPos(v.vertex);     
        o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
        o.modelPos = mul(unity_ObjectToWorld, fixed4(0,0,0,1)).xyz;     //世界矩阵乘以模型空间的0点，即获得模型中心的点世界坐标,顶点齐次坐标为1
        o.uv.xy = v.uv0.xy * _MainTex_ST.xy + _MainTex_ST.zw;	
        //o.uv.zw = v.uv0.xy * _HaloTex_ST.xy + _HaloTex_ST.zw;
        o.uv.zw = v.uv0.xy * _Mask_ST.xy + _Mask_ST.zw;	

        TANGENT_SPACE_ROTATION;
        
        half3 cameraDir =  ObjSpaceViewDir(v.vertex);
        o.lightDir=mul(rotation,ObjSpaceLightDir(v.vertex)).xyz;
        o.viewDir.xyz=mul(rotation,cameraDir).xyz;
        o.viewDir.w =  length(cameraDir);

        #if USING_FOG
            half3 eyePos = UnityObjectToViewPos(v.vertex);
            half fogCoord = length(eyePos.xyz);
            UNITY_CALC_FOG_FACTOR_RAW(fogCoord);
            o.fog = saturate(unityFogFactor);
        #endif

        return o;
    }

   


    float4 frag(v2f i) : COLOR0
    {
        UNITY_SETUP_INSTANCE_ID(i);  

        //dif
        half4 clr  = tex2D(_MainTex, i.uv.xy);   

        clip (clr.a - UNITY_ACCESS_INSTANCED_PROP(MyProperties,_Cutoff));     

        half3 tangentLightDir = normalize(i.lightDir);
		half3 tangentViewDir = normalize(i.viewDir.xyz);

        half3 tangentNormal = MyUnpackNormal(tex2D(_BumpMap, i.uv.xy), _BumpScale, _HeightScale);       
        half3 halfDir = normalize(tangentLightDir + tangentViewDir);

        //mask
        half4 maskTex = tex2D(_Mask,i.uv.zw);

        // 遮罩取样，maskTex的r通道作为高光遮罩，g通道为自发光遮罩，b通道为溜光遮罩,a通道为环境反射遮罩
        half specularMask = maskTex.r * _SpecularScale;
        half haloMask = maskTex.b;
        half _EmissionMask = maskTex.g ;

        half4 _t = _Time + _TimeEditor;
        half2 _offset = (i.worldPos.xy - i.modelPos.xy) + half2(_t.r *  _HaloXSpeed , _t.r *  _HaloYSpeed);
        half4 _halo = tex2D(_HaloTex,_offset);        
        //溜光
        half3 halo = _halo.rgb *   _HaloColor.rgb *  _HaloScale  * haloMask;

        fixed3 reflDir = reflect(-tangentViewDir, tangentNormal);
        fixed3 reflCol = texCUBE(_Cubemap, reflDir).rgb   * _RefScale;            
        fixed ref = pow(_RefBias - saturate(dot(tangentViewDir, tangentNormal)), 1);
        fixed3 finalColor =   reflCol * ref * maskTex.a;	

        
        //菲涅尔
        fixed fresnel = _FresnelScale + ( _FresnelBias - _FresnelScale)  * pow( _FresnelBias - dot(tangentViewDir, tangentNormal), 5);
        clr.rgb = lerp(clr.rgb , _FresnelColor.rgb, saturate(fresnel) ) ;
        

        half4 albedo = half4(clr.rgb * UNITY_ACCESS_INSTANCED_PROP(MyProperties,_Color),1.0);

        //自发光(带闪)，闪烁范围控制在0-1，如果要控制在-1 - 1，则取消求PI的余数操作
        half3 emission = _EmissionMask * _Emission.rgb * _EmissionScale * (clamp(sin( fmod ( UNITY_PI * _t.y * _EmissionFlashSpeed  ,UNITY_PI) ),_EmissionFlashMinValue,1) );
        //half swif = step(_EmissionFlashSpeed, 0);        //a<=b为1,否则为0     
        
        UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos);
        
        // 通过遮罩计算高光
        half3 specular = _LightColor0.rgb * _Specular.rgb * pow(max(0, dot(tangentNormal, halfDir)), _Gloss) * specularMask  ;       

        half3 ambient = GetAmbient().xyz * albedo.rgb ;

        //漫反射部分采用lambert光照算法
        half halfLambert =   dot(tangentNormal, tangentLightDir) * 0.5 + 0.5;//dot(tangentViewDir, tangentLightDir) * 0.5 + 0.5;
        half3 diffuse = _LightColor0.rgb * albedo.rgb * halfLambert;        

        //如果采用Blinn-Phong光照算法，则取消下行注释，并注释lambert光照算法
        //half3 diffuse = _LightColor0.rgb *  albedo.rgb  *  max(0, dot(tangentNormal, tangentLightDir)) ;

        half4 c = half4(ambient + ( diffuse + specular) *  atten + halo + emission + finalColor , 1.0);	
        //c.a = clr.a ;
        
        #if USING_FOG
		    c.rgb = lerp(unity_FogColor.rgb, c.rgb, i.fog);
        #endif
        
        return c;
    }


    struct VertexInput_shadow
    {
        float4   vertex : POSITION;  
        float3  normal : NORMAL;
        half4 tangent : TANGENT;
        float4  texcoord : TEXCOORD0;  
    };

    struct v2f_shadow
    {
       // half2 uv : TEXCOORD0;
        V2F_SHADOW_CASTER;
    };


    v2f_shadow vert_shadow(VertexInput_shadow v) {
		v2f_shadow o;			
		
       // o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
		TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)				
		return o;
	}

	fixed4 frag_shadow(v2f_shadow i) : SV_Target {
        //fixed4 tex = tex2D(_MainTex, i.uv.xy);	
		//clip (tex.a -_Cutoff);
		SHADOW_CASTER_FRAGMENT(i)
	}


ENDCG


    SubShader
    {
        LOD 200
        Tags {"Queue"="AlphaTest"  "RenderType"="TransparentCutout"  "RenderTag" = "Actor"  "IgnoreProjector"="True"}

        //Pass
        //{          
        //    ZWrite On
        //    ColorMask 0
        //}
        Pass
        {
            Tags { "LightMode" = "ForwardBase"  }  
            
            //Blend SrcAlpha OneMinusSrcAlpha
            //ZWrite On           
            CGPROGRAM
                #pragma multi_compile_fwdbase
                #pragma multi_compile_instancing
                #pragma vertex   vert
                #pragma fragment frag            
            ENDCG
        }
       
        Pass 
        {
			Tags { "LightMode"= "ShadowCaster" }
			CGPROGRAM
            
			#pragma vertex vert_shadow
			#pragma fragment frag_shadow             
			ENDCG
		}


    }

    


Fallback Off
}