// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "hc/partical/Trail_uv"
{
	Properties
	{
		[Toggle]_Desaturate("Desaturate", Float) = 1
		[Enum(Off,0,On,1)] _AlphaRed("Alpha/Red", Float) = 0//[Toggle(_ALPHARED_ON)] _AlphaRed("Alpha/Red", Float) = 0//�Ż�
		[HDR]_TintColor("Color", Color) = (1,1,1,1)
		_MainTex("Texture", 2D) = "white" {}
		_Dissolvew("Distortion", 2D) = "white" {}
		[Enum(Off,0,On,1)]_OneMinus_Dissolve("OneMinus_Distortion", Float) = 0 //[Toggle(_ONEMINUS_DISSOLVE_ON)] _OneMinus_Dissolve("OneMinus_Distortion", Float) = 0 //�Ż�
		_Strength1("Strength_distortion", Float) = 0
		_Length("Length", Float) = 0
		_Mask("Mask", 2D) = "white" {}
		_UV_speed("UV_speed(xy-d/zw-m)", Vector) = (0,0,0,0)
		[Enum(UnityEngine.Rendering.BlendMode)]_Src("Src", Float) = 5
		[Enum(UnityEngine.Rendering.BlendMode)]_Dst("Dst", Float) = 10
		[Enum(UnityEngine.Rendering.CompareFunction)]_ZTestMode("ZTest Mode", Float) = 2
		[Enum(UnityEngine.Rendering.CullMode)]_CullMode("Cull Mode", Float) = 0
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
		Tags{ "RenderType" = "Transparent"  "Queue" = "Geometry+1000" "IsEmissive" = "true"  }
		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp]
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}	
		Cull [_CullMode]
		ZWrite Off
		ZTest [_ZTestMode]
		Blend [_Src] [_Dst]
		
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityUI.cginc"
		#pragma target 3.0
		#pragma multi_compile __ UNITY_UI_CLIP_RECT
		//�Ż�del #pragma shader_feature _ONEMINUS_DISSOLVE_ON
		//�Ż�del#pragma shader_feature _ALPHARED_ON
		#pragma surface surf Unlit keepalpha noshadow 
		//�Ż� add
		#pragma skip_variants  LIGHTPROBE_SH DYNAMICLIGHTMAP_ON  LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING  _SPECULARHIGHLIGHTS_OFF VERTEXLIGHT_ON 
		#pragma skip_variants  UNITY_SPECCUBE_BOX_PROJECTION UNITY_SPECCUBE_BLENDING 
		#pragma skip_variants FOG_EXP FOG_EXP2
		#pragma skip_variants DIRECTIONAL_COOKIE POINT_COOKIE SPOT _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
		struct Input
		{
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
			float3 vertex : POSITION;
		};

		uniform half _ZTestMode;
		uniform half _Src;
		uniform half _CullMode;
		uniform half _Dst;
		uniform half4 _TintColor;
		uniform sampler2D _MainTex;
		uniform float4 _MainTex_ST;
		uniform half _Length;
		SamplerState sampler_MainTex;
		uniform half _Strength1;
		uniform sampler2D _Dissolvew;
		SamplerState sampler_Dissolvew;
		uniform half4 _UV_speed;
		uniform float4 _Dissolvew_ST;
		uniform half _Desaturate;
		uniform sampler2D _Mask;
		SamplerState sampler_Mask;
		uniform float4 _Mask_ST;
		half _OneMinus_Dissolve;
		half _AlphaRed;
		uniform float4 _ClipRect;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv_MainTex = i.uv_texcoord * _MainTex_ST.xy + _MainTex_ST.zw;
			half temp_output_148_0 = ( _Length * uv_MainTex.x );
			half temp_output_185_0 = ( temp_output_148_0 * _Strength1 );
			half2 appendResult91 = (half2(_UV_speed.x , _UV_speed.y));
			float2 uv_Dissolvew = i.uv_texcoord * _Dissolvew_ST.xy + _Dissolvew_ST.zw;
			half2 panner29 = ( 1.0 * _Time.y * appendResult91 + uv_Dissolvew);
			half4 tex2DNode6 = tex2D( _Dissolvew, panner29 );
			half temp_output_8_0 = ( tex2DNode6.r * tex2DNode6.a );
			
			//�Ż�
			/*#ifdef _ONEMINUS_DISSOLVE_ON
				float staticSwitch9 = temp_output_8_0;
			#else
				float staticSwitch9 = ( 1.0 - temp_output_8_0 );
			#endif*/
			float staticSwitch9 = (temp_output_8_0 * _OneMinus_Dissolve) + (1.0 - temp_output_8_0) * (1.0 - _OneMinus_Dissolve);
				 
			half dissolve178 = staticSwitch9;
			half2 temp_cast_0 = (( temp_output_185_0 * 0.5 )).xx;
			half4 tex2DNode2 = tex2D( _MainTex, ( ( uv_MainTex + ( temp_output_185_0 * dissolve178 ) ) - temp_cast_0 ) );
			half3 desaturateInitialColor107 = tex2DNode2.rgb;
			half desaturateDot107 = dot( desaturateInitialColor107, float3( 0.299, 0.587, 0.114 ));
			half3 desaturateVar107 = lerp( desaturateInitialColor107, desaturateDot107.xxx, (( _Desaturate )?( 1.0 ):( 0.0 )) );
			o.Emission = ( _TintColor * half4( desaturateVar107 , 0.0 ) * i.vertexColor ).rgb;
			//�Ż�
			/*#ifdef _ALPHARED_ON
				float staticSwitch188 = (desaturateVar107).x;
			#else
				float staticSwitch188 = tex2DNode2.a;
			#endif*/
			float staticSwitch188 = (desaturateVar107).x * _AlphaRed + tex2DNode2.a * (1.0 - _AlphaRed);

			half2 appendResult92 = (half2(_UV_speed.z , _UV_speed.w));
			float2 uv_Mask = i.uv_texcoord * _Mask_ST.xy + _Mask_ST.zw;
			half2 panner19 = ( 1.0 * _Time.y * appendResult92 + uv_Mask);
			half4 tex2DNode13 = tex2D( _Mask, panner19 );
			half clampResult173 = clamp( ( 1.0 - ( temp_output_148_0 * dissolve178 ) ) , 0.0 , 1.0 );
			float mask2dIsClip = 1;
			#ifdef UNITY_UI_CLIP_RECT
			mask2dIsClip = UnityGet2DClipping(i.vertex.xy, _ClipRect);
			#endif
			o.Alpha = ( _TintColor.a * staticSwitch188 * i.vertexColor.a * tex2DNode13.a * tex2DNode13.r * clampResult173 * mask2dIsClip );
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18703
7;7;1906;1124;3436.013;2040.506;2.603389;True;True
Node;AmplifyShaderEditor.Vector4Node;90;-2941.008,509.0539;Half;False;Property;_UV_speed;UV_speed(xy-d/zw-m);10;0;Create;False;0;0;False;0;False;0,0,0,0;-0.2,0.1,0,0;0;5;FLOAT4;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;91;-2563.687,312.763;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;28;-2813.458,28.96399;Inherit;True;0;6;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;29;-2396.527,128.5273;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;6;-2120.113,180.1701;Inherit;True;Property;_Dissolvew;Distortion;5;0;Create;False;0;0;False;0;False;-1;None;3cfe8b8d69e5a2e43b4df4b50df5edd2;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;-1789.096,252.922;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;10;-1543.053,375.0695;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;129;-2846.17,-1557.158;Inherit;False;Property;_Length;Length;8;0;Create;True;0;0;False;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;9;-1343.352,265.2481;Float;False;Property;_OneMinus_Dissolve;OneMinus_Distortion;6;0;Create;False;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;False;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;143;-2982.981,-1341.519;Inherit;False;0;2;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RegisterLocalVarNode;178;-1012.039,252.6425;Inherit;False;dissolve;-1;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;135;-2983.851,-964.3425;Half;False;Property;_Strength1;Strength_distortion;7;0;Create;False;0;0;False;0;False;0;0.01;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;148;-2442.695,-1467.946;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.GetLocalVarNode;180;-2537.682,-938.8521;Inherit;False;178;dissolve;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;185;-2735.548,-975.4754;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;139;-2838.964,-676.5294;Half;False;Constant;_05;0.5;5;0;Create;True;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;145;-2331.166,-1028.989;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;141;-2509.363,-705.3829;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;140;-1994.145,-1026.624;Inherit;True;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;142;-1748.757,-832.9524;Inherit;True;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.GetLocalVarNode;186;-1660.609,-1372.466;Inherit;False;178;dissolve;1;0;OBJECT;;False;1;FLOAT;0
Node;AmplifyShaderEditor.ToggleSwitchNode;106;-814.4786,-329.0957;Half;False;Property;_Desaturate;Desaturate;1;0;Create;True;0;0;False;0;False;1;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;2;-1214.266,-548.4783;Inherit;True;Property;_MainTex;Texture;4;0;Create;False;0;0;False;0;False;-1;None;d4d634a39382ea84ca5c595afc3a4bb2;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;17;-1166.342,375.0407;Inherit;False;0;13;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;92;-1782.978,580.4236;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;166;-1437.043,-1435.665;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.DesaturateOpNode;107;-360.9444,-360.4518;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.OneMinusNode;167;-979.7063,-1106.873;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.PannerNode;19;-740.0505,458.7621;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.ComponentMaskNode;187;-157.1548,-537.6396;Inherit;False;True;False;False;True;1;0;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;13;-565.8716,177.7464;Inherit;True;Property;_Mask;Mask;9;0;Create;True;0;0;False;0;False;-1;None;4bf69cc6f32f05e46a0f28428c5ced20;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;20;-524.1616,-520.8802;Half;False;Property;_TintColor;Color;3;1;[HDR];Create;False;0;0;False;0;False;1,1,1,1;5.992157,5.992157,5.992157,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;173;-457.429,-940.7716;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.StaticSwitch;188;68.04182,-599.5447;Float;True;Property;_AlphaRed;Alpha/Red;2;0;Create;True;0;0;False;0;False;0;0;0;True;;Toggle;2;Key0;Key1;Create;False;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.VertexColorNode;12;-573.439,-39.47113;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.CommentaryNode;101;-2652.806,-507.4683;Inherit;False;183.2;357.4001;value;4;105;104;103;102;;1,1,1,1;0;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;341.4272,-19.05168;Inherit;True;6;6;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;321.6523,-330.2469;Inherit;True;3;3;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;102;-2619.138,-466.1561;Half;False;Property;_Src;Src;11;1;[Enum];Create;True;1;Option1;0;1;UnityEngine.Rendering.BlendMode;True;0;False;5;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;103;-2619.51,-227.9086;Half;False;Property;_CullMode;Cull Mode;14;1;[Enum];Create;True;1;Option1;0;1;UnityEngine.Rendering.CullMode;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;105;-2614.484,-392.1991;Half;False;Property;_Dst;Dst;12;1;[Enum];Create;True;1;Option1;0;1;UnityEngine.Rendering.BlendMode;True;0;False;10;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;104;-2628.64,-310.2534;Half;False;Property;_ZTestMode;ZTest Mode;13;1;[Enum];Create;True;1;Option1;0;1;UnityEngine.Rendering.CompareFunction;True;0;False;2;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;24;641.9103,-302.3074;Half;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;hc\partical\Trail_uv;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;2;False;-1;0;True;104;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;False;1000;True;Transparent;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;True;102;10;True;105;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;0;-1;-1;-1;0;False;0;0;True;103;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;91;0;90;1
WireConnection;91;1;90;2
WireConnection;29;0;28;0
WireConnection;29;2;91;0
WireConnection;6;1;29;0
WireConnection;8;0;6;1
WireConnection;8;1;6;4
WireConnection;10;0;8;0
WireConnection;9;1;10;0
WireConnection;9;0;8;0
WireConnection;178;0;9;0
WireConnection;148;0;129;0
WireConnection;148;1;143;1
WireConnection;185;0;148;0
WireConnection;185;1;135;0
WireConnection;145;0;185;0
WireConnection;145;1;180;0
WireConnection;141;0;185;0
WireConnection;141;1;139;0
WireConnection;140;0;143;0
WireConnection;140;1;145;0
WireConnection;142;0;140;0
WireConnection;142;1;141;0
WireConnection;2;1;142;0
WireConnection;92;0;90;3
WireConnection;92;1;90;4
WireConnection;166;0;148;0
WireConnection;166;1;186;0
WireConnection;107;0;2;0
WireConnection;107;1;106;0
WireConnection;167;0;166;0
WireConnection;19;0;17;0
WireConnection;19;2;92;0
WireConnection;187;0;107;0
WireConnection;13;1;19;0
WireConnection;173;0;167;0
WireConnection;188;1;2;4
WireConnection;188;0;187;0
WireConnection;22;0;20;4
WireConnection;22;1;188;0
WireConnection;22;2;12;4
WireConnection;22;3;13;4
WireConnection;22;4;13;1
WireConnection;22;5;173;0
WireConnection;21;0;20;0
WireConnection;21;1;107;0
WireConnection;21;2;12;0
WireConnection;24;2;21;0
WireConnection;24;9;22;0
ASEEND*/
//CHKSM=57DB0CEAE6166581A481B7DD77C870E1A9D5D895