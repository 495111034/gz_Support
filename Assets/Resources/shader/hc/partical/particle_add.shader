// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "hc/partical/partilce_Shader"
{
	Properties
	{
		[Toggle(_IS_UV_SPEED_ON)] _Is_uv_speed("Is_uv_speed", Float) = 0
		[HDR]_Color("Color", Color) = (1,1,1,1)
		[Enum(UnityEngine.Rendering.BlendMode)]_Src("Src", Float) = 0
		[Enum(UnityEngine.Rendering.BlendMode)]_Dst("Dst", Float) = 0
		[Enum(UnityEngine.Rendering.CullMode)]_Cullmode("Cull mode", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)]_Ztest("Ztest", Float) = 0
		[Toggle]_Zwrite("Zwrite", Float) = 0
		_Tile_Tex("Tile_Tex", 2D) = "white" {}
		_u_speed("u_speed", Float) = 0
		_v_speed("v_speed", Float) = 0
		_Refine("Refine", Vector) = (1,1,1,0)
		_Noise_Tex("Noise_Tex", 2D) = "white" {}
		_noise_u_speed("noise_u_speed", Float) = 0
		_noise_v_speed("noise_v_speed", Float) = 0
		_noise_intensity("noise_intensity", Range( 0 , 1)) = 0
		[Toggle(_IS_NOISE_ON)] _Is_noise("Is_noise", Float) = 0
		_Dissolve_Tile("Dissolve_Tile", 2D) = "white" {}
		_Dissolve_soft("Dissolve_soft", Float) = 0
		_Dissolve_intensity("Dissolve_intensity", Float) = 0
		_Dissolve_u("Dissolve_u", Float) = 0
		_Dissolve_v("Dissolve_v", Float) = 0
		[Toggle(_IS_DISSOLVE_ON)] _Is_Dissolve("Is_Dissolve", Float) = 0
		_mask_tile("mask_tile", 2D) = "white" {}
		_mask_u("mask_u", Float) = 0
		_mask_v("mask_v", Float) = 0
		_opactiy("opactiy", Float) = 1
		_wpo_intensity("wpo_intensity", Float) = 0
		_wpo_speed("wpo_speed", Vector) = (0,0,0,0)
		_wpo_tile("wpo_tile", 2D) = "white" {}
		_Wpo_u_speed("Wpo_u_speed", Float) = 0
		_Wpo_v_speed("Wpo_v_speed", Float) = 0
		[HideInInspector] _texcoord2( "", 2D ) = "white" {}
		[HideInInspector] _tex4coord2( "", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1

		[HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
		[HideInInspector] _Stencil ("Stencil ID", Float) = 0
		[HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
		[HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
		[HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
	}

	SubShader
	{
		Tags{ "RenderType" = "Custom"  "Queue" = "Transparent+0" "IsEmissive" = "true"  }
		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp]
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}	
		Cull Off
		ZWrite Off
		ZTest [_Ztest]
		Blend [_Src] [_Dst]
		
		CGINCLUDE
		#include "UnityShaderVariables.cginc"
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#include "UnityUI.cginc"
		#pragma target 3.0
		
		#pragma multi_compile __ UNITY_UI_CLIP_RECT
		//�Ż�
		#pragma shader_feature_local _IS_NOISE_ON
		#pragma shader_feature_local _IS_UV_SPEED_ON
		#pragma shader_feature_local _IS_DISSOLVE_ON

		//�Ż� add
		#pragma skip_variants  LIGHTPROBE_SH DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING _SPECULARHIGHLIGHTS_OFF VERTEXLIGHT_ON 
		#pragma skip_variants  UNITY_SPECCUBE_BOX_PROJECTION UNITY_SPECCUBE_BLENDING 
		#pragma skip_variants FOG_EXP FOG_EXP2
		#pragma skip_variants DIRECTIONAL_COOKIE POINT_COOKIE SPOT _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

		#undef TRANSFORM_TEX
		#define TRANSFORM_TEX(tex,name) float4(tex.xy * name##_ST.xy + name##_ST.zw, tex.z, tex.w)
		struct Input
		{
			float2 uv_texcoord;
			float4 uv2_tex4coord2;
			float2 uv2_texcoord2;
			float4 vertexColor : COLOR;
			float3 vertex : POSITION;
		};

		uniform float _wpo_intensity;
		uniform sampler2D _wpo_tile;
		SamplerState sampler_wpo_tile;
		uniform float _Wpo_u_speed;
		uniform float _Wpo_v_speed;
		uniform float4 _wpo_tile_ST;
		uniform float3 _wpo_speed;
		uniform sampler2D _Tile_Tex;
		uniform float4 _Tile_Tex_ST;
		uniform float _u_speed;
		uniform float _v_speed;
		uniform sampler2D _Noise_Tex;
		SamplerState sampler_Noise_Tex;
		uniform float4 _Noise_Tex_ST;
		uniform float _noise_u_speed;
		uniform float _noise_v_speed;
		uniform float _noise_intensity;
		uniform float4 sampler_Noise_Tex_ST;
		uniform float4 _Refine;
		uniform float4 _Color;
		uniform sampler2D _mask_tile;
		SamplerState sampler_mask_tile;
		uniform float4 _mask_tile_ST;
		uniform float _mask_u;
		uniform float _mask_v;
		uniform float _opactiy;
		uniform sampler2D _Dissolve_Tile;
		SamplerState sampler_Dissolve_Tile;
		uniform float4 _Dissolve_Tile_ST;
		uniform float _Dissolve_u;
		uniform float _Dissolve_v;
		uniform float _Dissolve_soft;
		uniform float _Dissolve_intensity;
		SamplerState sampler_Tile_Tex;
		uniform float4 _ClipRect;

		void vertexDataFunc( inout appdata_full v, out Input o )
		{
			UNITY_INITIALIZE_OUTPUT( Input, o );
			float3 ase_vertexNormal = v.normal.xyz;
			float2 appendResult83 = (float2(_Wpo_u_speed , _Wpo_v_speed));
			float2 uv_wpo_tile = v.texcoord.xy * _wpo_tile_ST.xy + _wpo_tile_ST.zw;
			float2 panner79 = ( 1.0 * _Time.y * appendResult83 + uv_wpo_tile);
			v.vertex.xyz += ( ase_vertexNormal * _wpo_intensity * tex2Dlod( _wpo_tile, float4( panner79, 0, 0.0) ).r * _wpo_speed );
			v.vertex.w = 1;
			o.vertex = v.vertex;
		}

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv_Tile_Tex = i.uv_texcoord * _Tile_Tex_ST.xy + _Tile_Tex_ST.zw;
			float2 appendResult9 = frac(float2(( _u_speed * _Time.y ) , ( _v_speed * _Time.y )));
			float2 uv_Noise_Tex = i.uv_texcoord * _Noise_Tex_ST.xy + _Noise_Tex_ST.zw;
			float2 appendResult22 = frac(float2(( _noise_u_speed * _Time.y ) , ( _noise_v_speed * _Time.y )));
			float4 uv2s4sampler_Noise_Tex = i.uv2_tex4coord2;
			uv2s4sampler_Noise_Tex.xy = i.uv2_tex4coord2.xy * sampler_Noise_Tex_ST.xy + sampler_Noise_Tex_ST.zw;
			#ifdef _IS_NOISE_ON
				float staticSwitch102 = uv2s4sampler_Noise_Tex.z;
			#else
				float staticSwitch102 = _noise_intensity;
			#endif
			float4 temp_cast_1 = (1.0).xxxx;
			float4 appendResult134 = (float4(i.uv2_texcoord2.x , i.uv2_texcoord2.y , 0.0 , 0.0));
			#ifdef _IS_UV_SPEED_ON
				float4 staticSwitch131 = appendResult134;
			#else
				float4 staticSwitch131 = temp_cast_1;
			#endif
			float4 tex2DNode1 = tex2D( _Tile_Tex, ( float4( ( uv_Tile_Tex + appendResult9 ), 0.0 , 0.0 ) + ( tex2D( _Noise_Tex, ( uv_Noise_Tex + appendResult22 ) ).r * staticSwitch102 ) + staticSwitch131 ).xy );
			float clampResult32 = clamp( _Refine.w , 0.0 , 1.0 );
			float3 desaturateInitialColor30 = tex2DNode1.rgb;
			float desaturateDot30 = dot( desaturateInitialColor30, float3( 0.299, 0.587, 0.114 ));
			float3 desaturateVar30 = lerp( desaturateInitialColor30, desaturateDot30.xxx, clampResult32 );
			float3 temp_cast_4 = (_Refine.x).xxx;
			float3 lerpResult36 = lerp( ( pow( desaturateVar30 , temp_cast_4 ) * _Refine.y ) , ( desaturateVar30 * _Refine.z ) , 0.5);
			o.Emission = ( float4( lerpResult36 , 0.0 ) * i.vertexColor * _Color ).rgb;
			float2 uv_mask_tile = i.uv_texcoord * _mask_tile_ST.xy + _mask_tile_ST.zw;
			float2 appendResult66 = (float2(( _mask_u * _Time.y ) , ( _mask_v * _Time.y )));
			float2 uv_Dissolve_Tile = i.uv_texcoord * _Dissolve_Tile_ST.xy + _Dissolve_Tile_ST.zw;
			float2 appendResult140 = (float2(( _Dissolve_u * _Time.y ) , ( _Dissolve_v * _Time.y )));
			#ifdef _IS_DISSOLVE_ON
				float staticSwitch99 = i.uv2_tex4coord2.w;
			#else
				float staticSwitch99 = _Dissolve_intensity;
			#endif
			float lerpResult98 = lerp( -1.5 , _Dissolve_soft , staticSwitch99);
			float mask2dIsClip = 1;
			#ifdef UNITY_UI_CLIP_RECT
			mask2dIsClip = UnityGet2DClipping(i.vertex.xy, _ClipRect);
			#endif
			o.Alpha = ( ( tex2D( _mask_tile, ( uv_mask_tile + appendResult66 ) ).r * _opactiy ) * saturate( ( ( tex2D( _Dissolve_Tile, ( uv_Dissolve_Tile + appendResult140 ) ).r * _Dissolve_soft ) - lerpResult98 ) ) * i.vertexColor.a * tex2DNode1.a * _Color.a * mask2dIsClip);
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Unlit keepalpha fullforwardshadows noambient novertexlights nolightmap  nodynlightmap nodirlightmap nofog nometa noforwardadd vertex:vertexDataFunc 

		ENDCG
		Pass
		{
			Name "ShadowCaster"
			Tags{ "LightMode" = "ShadowCaster" }
			ZWrite On
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma multi_compile_shadowcaster
			#pragma multi_compile UNITY_PASS_SHADOWCASTER
			#pragma skip_variants FOG_LINEAR FOG_EXP FOG_EXP2
			#include "HLSLSupport.cginc"
			#if ( SHADER_API_D3D11 || SHADER_API_GLCORE || SHADER_API_GLES || SHADER_API_GLES3 || SHADER_API_METAL || SHADER_API_VULKAN )
				#define CAN_SKIP_VPOS
			#endif
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			#include "UnityPBSLighting.cginc"
			sampler3D _DitherMaskLOD;
			struct v2f
			{
				V2F_SHADOW_CASTER;
				float4 customPack1 : TEXCOORD1;
				float4 customPack2 : TEXCOORD2;
				float3 worldPos : TEXCOORD3;
				half4 color : COLOR0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};
			v2f vert( appdata_full v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID( v );
				UNITY_INITIALIZE_OUTPUT( v2f, o );
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO( o );
				UNITY_TRANSFER_INSTANCE_ID( v, o );
				Input customInputData;
				vertexDataFunc( v, customInputData );
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.customPack2.xyzw = customInputData.uv2_tex4coord2;
				o.customPack2.xyzw = v.texcoord1;
				o.customPack1.zw = customInputData.uv2_texcoord2;
				o.customPack1.zw = v.texcoord1;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
				o.color = v.color;
				return o;
			}
			half4 frag( v2f IN
			#if !defined( CAN_SKIP_VPOS )
			, UNITY_VPOS_TYPE vpos : VPOS
			#endif
			) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID( IN );
				Input surfIN;
				UNITY_INITIALIZE_OUTPUT( Input, surfIN );
				surfIN.uv_texcoord = IN.customPack1.xy;
				surfIN.uv2_tex4coord2 = IN.customPack2.xyzw;
				surfIN.uv2_texcoord2 = IN.customPack1.zw;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				surfIN.vertexColor = IN.color;
				SurfaceOutput o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutput, o )
				surf( surfIN, o );
				#if defined( CAN_SKIP_VPOS )
				float2 vpos = IN.pos;
				#endif
				half alphaRef = tex3D( _DitherMaskLOD, float3( vpos.xy * 0.25, o.Alpha * 0.9375 ) ).a;
				clip( alphaRef - 0.01 );
				SHADOW_CASTER_FRAGMENT( IN )
			}
			ENDCG
		}
	}
	//�Ż�  del Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18703
7;1;1906;1131;370.0063;47.76109;1;True;True
Node;AmplifyShaderEditor.RangedFloatNode;18;-149.7366,421.5505;Float;False;Property;_noise_u_speed;noise_u_speed;13;0;Create;True;0;0;False;0;False;0;0.1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;19;-142.7366,516.5505;Float;False;Property;_noise_v_speed;noise_v_speed;14;0;Create;True;0;0;False;0;False;0;0.05;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;17;-165.7366,632.5505;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;221.2634,574.5505;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;239.2634,448.5505;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;13;-135.5143,-106.3498;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;15;-89.77001,-324.1708;Float;False;Property;_u_speed;u_speed;9;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;23;380.8583,301.1107;Inherit;False;0;16;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;14;-125.2503,-201.5074;Float;False;Property;_v_speed;v_speed;10;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;22;431.3843,482.1664;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleAddOpNode;24;754.3489,373.6533;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;126;1514.709,598.6524;Inherit;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;125;218.9804,-151.047;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;26;660.1731,637.3201;Float;False;Property;_noise_intensity;noise_intensity;15;0;Create;True;0;0;False;0;False;0;0.02;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;124;230.8889,-292.1082;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;103;661.6186,747.028;Inherit;False;1;16;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;109;1538.417,476.1921;Inherit;False;1;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;102;1031.092,643.2938;Float;False;Property;_Is_noise;Is_noise;16;0;Create;True;0;0;False;0;False;0;0;1;True;;Toggle;2;Key0;Key1;Create;False;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;16;937.5807,313.5984;Inherit;True;Property;_Noise_Tex;Noise_Tex;12;0;Create;True;0;0;False;0;False;-1;None;13dac46096cf24843bfd470c2b322171;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;134;1916.378,441.8736;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;9;445.8403,-318.6633;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;137;817.2623,1496.798;Float;False;Property;_Dissolve_v;Dissolve_v;21;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;133;1577.037,141.7518;Inherit;False;Constant;_Float7;Float 7;30;0;Create;True;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;135;781.1725,1678.892;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;101;148.1154,-488.5044;Inherit;False;0;1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;136;810.2625,1401.798;Float;False;Property;_Dissolve_u;Dissolve_u;20;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;139;1199.262,1428.798;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;8;1301.383,-323.1612;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;138;1181.262,1554.798;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;25;1443.691,12.25698;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;131;1839.387,36.83765;Inherit;False;Property;_Is_uv_speed;Is_uv_speed;0;0;Create;True;0;0;False;0;False;0;0;1;True;;Toggle;2;Key0;Key1;Create;False;True;9;1;FLOAT4;0,0,0,0;False;0;FLOAT4;0,0,0,0;False;2;FLOAT4;0,0,0,0;False;3;FLOAT4;0,0,0,0;False;4;FLOAT4;0,0,0,0;False;5;FLOAT4;0,0,0,0;False;6;FLOAT4;0,0,0,0;False;7;FLOAT4;0,0,0,0;False;8;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;62;2047.64,765.8179;Float;False;Property;_mask_u;mask_u;24;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector4Node;31;2276.197,120.3928;Float;False;Property;_Refine;Refine;11;0;Create;True;0;0;False;0;False;1,1,1,0;1,1,1,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;27;1873.377,-260.7298;Inherit;False;3;3;0;FLOAT2;0,0;False;1;FLOAT;0;False;2;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;141;1340.858,1281.359;Inherit;False;0;39;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;61;2018.55,1042.911;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;140;1391.383,1462.415;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;63;2054.64,860.8179;Float;False;Property;_mask_v;mask_v;25;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;64;2436.64,792.8179;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;94;2314.22,1839.557;Inherit;False;1;-1;4;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;65;2418.64,918.8179;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;142;1714.349,1353.902;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;53;2292.273,1694.742;Float;False;Property;_Dissolve_intensity;Dissolve_intensity;19;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;32;2560.128,131.83;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;2203.95,-175.2515;Inherit;True;Property;_Tile_Tex;Tile_Tex;8;0;Create;True;0;0;False;0;False;-1;None;8782e4ace66537d489c2941660060b84;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;67;2578.235,645.3781;Inherit;False;0;56;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;81;3948.294,2031.236;Float;False;Property;_Wpo_u_speed;Wpo_u_speed;30;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;66;2628.761,826.434;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;93;2543.106,1495.78;Float;False;Constant;_Float6;Float 6;27;0;Create;True;0;0;False;0;False;-1.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;39;1897.74,1292.17;Inherit;True;Property;_Dissolve_Tile;Dissolve_Tile;17;0;Create;True;0;0;False;0;False;-1;None;13dac46096cf24843bfd470c2b322171;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;82;3914.758,2116.74;Float;False;Property;_Wpo_v_speed;Wpo_v_speed;31;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;99;2563.515,1726.083;Float;False;Property;_Is_Dissolve;Is_Dissolve;22;0;Create;True;0;0;False;0;False;0;0;1;True;;Toggle;2;Key0;Key1;Create;False;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;91;2286.397,1551.737;Float;False;Property;_Dissolve_soft;Dissolve_soft;18;0;Create;True;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DesaturateOpNode;30;2708.796,-45.99519;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;98;2772.688,1536.193;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;68;2951.726,717.9211;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT2;0,0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.PowerNode;33;2973.69,-49.34576;Inherit;False;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;1;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;92;2579.472,1343.954;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;80;3890.302,1755.464;Inherit;False;0;77;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;83;4230.835,2011.831;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.RangedFloatNode;69;3237.914,876.9938;Float;False;Property;_opactiy;opactiy;26;0;Create;True;0;0;False;0;False;1;3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;97;2935.999,1287.521;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;38;2955.112,149.7806;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;56;3153.505,686.4438;Inherit;True;Property;_mask_tile;mask_tile;23;0;Create;True;0;0;False;0;False;-1;None;e486692edaceb724aa11dd862682bca8;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;79;4426.8,1821.621;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;34;3219.699,-21.96509;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;37;3252.964,157.6661;Float;False;Constant;_Float1;Float 1;13;0;Create;True;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;76;5067.703,1886.536;Float;False;Property;_wpo_speed;wpo_speed;28;0;Create;True;0;0;False;0;False;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SaturateNode;95;3135.718,1305.106;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;57;3887.361,201.3946;Float;False;Property;_Color;Color;2;1;[HDR];Create;True;0;0;False;0;False;1,1,1,1;1.498039,1.498039,1.498039,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.LerpOp;36;3489.249,13.8835;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;77;4736.661,1784.834;Inherit;True;Property;_wpo_tile;wpo_tile;29;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;59;3635.171,678.3474;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.NormalVertexDataNode;74;5314.994,1523.696;Inherit;False;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;75;5073.406,1722.578;Float;False;Property;_wpo_intensity;wpo_intensity;27;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;70;4212.391,178.0893;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;3;-928.5,181.5;Float;False;Property;_Src;Src;3;1;[Enum];Fetch;True;0;1;UnityEngine.Rendering.BlendMode;True;0;False;0;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;6;-932.9335,454.902;Float;False;Property;_Zwrite;Zwrite;7;1;[Toggle];Fetch;True;0;1;UnityEngine.Rendering.CompareFunction;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;4;-919.5,283.5;Float;False;Property;_Dst;Dst;4;1;[Enum];Fetch;True;0;1;UnityEngine.Rendering.BlendMode;True;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;2;-928.5,93.5;Float;False;Property;_Cullmode;Cull mode;5;1;[Enum];Fetch;True;0;1;UnityEngine.Rendering.CullMode;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;60;4551.671,582.7921;Inherit;False;5;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;58;4588.597,70.73862;Inherit;False;3;3;0;FLOAT3;0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;73;5022.503,1232.066;Inherit;False;4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;5;-910.1002,372.2001;Float;False;Property;_Ztest;Ztest;6;1;[Enum];Fetch;True;0;1;UnityEngine.Rendering.CompareFunction;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;5567.925,146.7296;Float;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;hc/partical/partilce_Shader;False;False;False;False;True;True;True;True;True;True;True;True;False;False;False;False;False;False;False;False;False;Off;2;False;5;0;True;5;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;True;0;True;Custom;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;True;1;0;True;3;0;True;4;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;20;0;19;0
WireConnection;20;1;17;0
WireConnection;21;0;18;0
WireConnection;21;1;17;0
WireConnection;22;0;21;0
WireConnection;22;1;20;0
WireConnection;24;0;23;0
WireConnection;24;1;22;0
WireConnection;125;0;14;0
WireConnection;125;1;13;0
WireConnection;124;0;15;0
WireConnection;124;1;13;0
WireConnection;102;1;26;0
WireConnection;102;0;103;3
WireConnection;16;1;24;0
WireConnection;134;0;109;1
WireConnection;134;1;126;2
WireConnection;9;0;124;0
WireConnection;9;1;125;0
WireConnection;139;0;136;0
WireConnection;139;1;135;0
WireConnection;8;0;101;0
WireConnection;8;1;9;0
WireConnection;138;0;137;0
WireConnection;138;1;135;0
WireConnection;25;0;16;1
WireConnection;25;1;102;0
WireConnection;131;1;133;0
WireConnection;131;0;134;0
WireConnection;27;0;8;0
WireConnection;27;1;25;0
WireConnection;27;2;131;0
WireConnection;140;0;139;0
WireConnection;140;1;138;0
WireConnection;64;0;62;0
WireConnection;64;1;61;0
WireConnection;65;0;63;0
WireConnection;65;1;61;0
WireConnection;142;0;141;0
WireConnection;142;1;140;0
WireConnection;32;0;31;4
WireConnection;1;1;27;0
WireConnection;66;0;64;0
WireConnection;66;1;65;0
WireConnection;39;1;142;0
WireConnection;99;1;53;0
WireConnection;99;0;94;4
WireConnection;30;0;1;0
WireConnection;30;1;32;0
WireConnection;98;0;93;0
WireConnection;98;1;91;0
WireConnection;98;2;99;0
WireConnection;68;0;67;0
WireConnection;68;1;66;0
WireConnection;33;0;30;0
WireConnection;33;1;31;1
WireConnection;92;0;39;1
WireConnection;92;1;91;0
WireConnection;83;0;81;0
WireConnection;83;1;82;0
WireConnection;97;0;92;0
WireConnection;97;1;98;0
WireConnection;38;0;30;0
WireConnection;38;1;31;3
WireConnection;56;1;68;0
WireConnection;79;0;80;0
WireConnection;79;2;83;0
WireConnection;34;0;33;0
WireConnection;34;1;31;2
WireConnection;95;0;97;0
WireConnection;36;0;34;0
WireConnection;36;1;38;0
WireConnection;36;2;37;0
WireConnection;77;1;79;0
WireConnection;59;0;56;1
WireConnection;59;1;69;0
WireConnection;60;0;59;0
WireConnection;60;1;95;0
WireConnection;60;2;70;4
WireConnection;60;3;1;4
WireConnection;60;4;57;4
WireConnection;58;0;36;0
WireConnection;58;1;70;0
WireConnection;58;2;57;0
WireConnection;73;0;74;0
WireConnection;73;1;75;0
WireConnection;73;2;77;1
WireConnection;73;3;76;0
WireConnection;0;2;58;0
WireConnection;0;9;60;0
WireConnection;0;11;73;0
ASEEND*/
//CHKSM=BC66B0E8D0E419EF1129D31DEE6FFE75288AE1C5