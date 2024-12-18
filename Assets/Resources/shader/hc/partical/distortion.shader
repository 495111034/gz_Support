// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "hc/partical/Distortion"
{
	Properties
	{
		_SpecColor("Specular Color",Color)=(1,1,1,1)
		[Enum(Off,0,On,1)] _Mask_alpha("Mask_alpha", Float) = 0//[Toggle(_MASK_ALPHA)] _Mask_alpha("Mask_alpha", Float) = 0//优化
		_MaskTexture("MaskTexture", 2D) = "white" {}
		[Header(Refraction)]
		_ChromaticAberration("Chromatic Aberration", Range( 0 , 0.3)) = 0.1
		_Distortion_In("Distortion_In", Range( 0 , 1)) = 1
		[Normal]_NormalTexture("NormalTexture", 2D) = "bump" {}
		_TimeScale("TimeScale", Range( 0 , 1)) = 0
		_V_Direction("V_Direction", Range( 0 , 1)) = 0
		_U_Direction("U_Direction", Range( 0 , 1)) = 0
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
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" }
		Stencil
		{
			Ref [_Stencil]
			Comp [_StencilComp]
			Pass [_StencilOp]
			ReadMask [_StencilReadMask]
			WriteMask [_StencilWriteMask]
		}
		Cull Off
		GrabPass{ }
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityUI.cginc"
		#pragma target 3.0
		#pragma multi_compile __ UNITY_UI_CLIP_RECT
		//#pragma multi_compile _ALPHAPREMULTIPLY_ON
		////优化 del  #pragma shader_feature _MASK_ALPHA
		#pragma surface surf BlinnPhong alpha:fade keepalpha finalcolor:RefractionF noshadow exclude_path:deferred 

		//优化 add
		#pragma skip_variants  LIGHTPROBE_SH DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING  _SPECULARHIGHLIGHTS_OFF  VERTEXLIGHT_ON 
		#pragma skip_variants  UNITY_SPECCUBE_BOX_PROJECTION UNITY_SPECCUBE_BLENDING 
		#pragma skip_variants DIRECTIONAL_COOKIE POINT_COOKIE SPOT _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
		struct Input
		{
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
			float4 screenPos;
			float3 worldPos;
			float3 vertex : POSITION;
		};

		uniform sampler2D _NormalTexture;
		uniform half _TimeScale;
		uniform half _U_Direction;
		uniform half _V_Direction;
		uniform float4 _NormalTexture_ST;
		uniform half _Distortion_In;
		uniform sampler2D _GrabTexture;
		uniform float _ChromaticAberration;
		uniform sampler2D _MaskTexture;
		uniform half4 _MaskTexture_ST;
		uniform half _Mask_alpha;
		SamplerState sampler_MaskTexture;
		uniform float4 _ClipRect;

		inline float4 Refraction( Input i, SurfaceOutput o, float indexOfRefraction, float chomaticAberration ) {
			float3 worldNormal = o.Normal;
			float4 screenPos = i.screenPos;
			#if UNITY_UV_STARTS_AT_TOP
				float scale = -1.0;
			#else
				float scale = 1.0;
			#endif
			float halfPosW = screenPos.w * 0.5;
			screenPos.y = ( screenPos.y - halfPosW ) * _ProjectionParams.x * scale + halfPosW;
			#if SHADER_API_D3D9 || SHADER_API_D3D11
				screenPos.w += 0.00000000001;
			#endif
			float2 projScreenPos = ( screenPos / screenPos.w ).xy;
			float3 worldViewDir = normalize( UnityWorldSpaceViewDir( i.worldPos ) );
			float3 refractionOffset = ( indexOfRefraction - 1.0 ) * mul( UNITY_MATRIX_V, float4( worldNormal, 0.0 ) ) * ( 1.0 - dot( worldNormal, worldViewDir ) );
			float2 cameraRefraction = float2( refractionOffset.x, refractionOffset.y );
			float4 redAlpha = tex2D( _GrabTexture, ( projScreenPos + cameraRefraction ) );
			float green = tex2D( _GrabTexture, ( projScreenPos + ( cameraRefraction * ( 1.0 - chomaticAberration ) ) ) ).g;
			float blue = tex2D( _GrabTexture, ( projScreenPos + ( cameraRefraction * ( 1.0 + chomaticAberration ) ) ) ).b;
			return float4( redAlpha.r, green, blue, redAlpha.a );
		}

		void RefractionF( Input i, SurfaceOutput o, inout half4 color )
		{
			#ifdef UNITY_PASS_FORWARDBASE
			half4 temp_output_10_0 = ( i.vertexColor * _Distortion_In );
			float2 uv_MaskTexture = i.uv_texcoord * _MaskTexture_ST.xy + _MaskTexture_ST.zw;
			half4 tex2DNode8 = tex2D( _MaskTexture, uv_MaskTexture );
			half3 desaturateInitialColor7 = tex2DNode8.rgb;
			half desaturateDot7 = dot( desaturateInitialColor7, float3( 0.299, 0.587, 0.114 ));
			half3 desaturateVar7 = lerp( desaturateInitialColor7, desaturateDot7.xxx, 0.0 );
			half3 temp_cast_2 = (tex2DNode8.a).xxx;
			//优化
			/*#ifdef _MASK_ALPHA
				half3 staticSwitch6 = temp_cast_2;
			#else
				half3 staticSwitch6 = desaturateVar7;
			#endif*/
			half3 staticSwitch6 = _Mask_alpha * temp_cast_2 + (1.0 - _Mask_alpha) * desaturateVar7;

			half lerpResult1 = lerp( 1.0 , 1.4 , ( temp_output_10_0 * half4( staticSwitch6 , 0.0 ) ).r);
			color.rgb = color.rgb + Refraction( i, o, lerpResult1, _ChromaticAberration ) * ( 1 - color.a );
			color.a = 1;
			#endif

			#ifdef UNITY_UI_CLIP_RECT
			color.a = UnityGet2DClipping(i.vertex.xy, _ClipRect);
			#endif
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			o.Normal = float3(0,0,1);
			half mulTime38 = _Time.y * _TimeScale;
			half2 appendResult37 = (half2(_U_Direction , _V_Direction));
			float2 uv_NormalTexture = i.uv_texcoord * _NormalTexture_ST.xy + _NormalTexture_ST.zw;
			half2 panner35 = ( mulTime38 * appendResult37 + uv_NormalTexture);
			half4 temp_output_10_0 = ( i.vertexColor * _Distortion_In );
			half3 lerpResult9 = lerp( half3(0,0,0) , (UnpackNormal( tex2D( _NormalTexture, panner35 ) )).xyz , temp_output_10_0.rgb);
			//o.Normal = normalize(lerpResult9);
			o.Normal = Unity_SafeNormalize(lerpResult9 + 0.00001 * i.screenPos * i.worldPos);
			o.Alpha = 0.0;
		
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18703
419;256;1906;1124;2591.931;1186.639;2.117645;True;True
Node;AmplifyShaderEditor.RangedFloatNode;41;-1801.625,-503.6948;Inherit;False;Property;_U_Direction;U_Direction;9;0;Create;True;0;0;False;0;False;0;0.15;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;39;-1759.625,-256.6948;Inherit;False;Property;_TimeScale;TimeScale;7;0;Create;True;0;0;False;0;False;0;0.67;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;40;-1776.625,-400.6948;Inherit;False;Property;_V_Direction;V_Direction;8;0;Create;True;0;0;False;0;False;0;0.2;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;37;-1410.626,-395.6948;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;36;-1442.626,-569.6948;Inherit;False;0;34;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleTimeNode;38;-1404.626,-276.6948;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;8;-1118,78.5;Inherit;True;Property;_MaskTexture;MaskTexture;2;0;Create;True;0;0;False;0;False;-1;None;89e3d168bdd5cdf44a0744d50c5ebd75;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PannerNode;35;-1102.626,-440.6948;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.VertexColorNode;14;-762.6256,-244.6948;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;15;-805.6256,-54.69482;Inherit;False;Property;_Distortion_In;Distortion_In;5;0;Create;True;0;0;False;0;False;1;0.35;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.DesaturateOpNode;7;-729,99.5;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SamplerNode;34;-910.6256,-460.6948;Inherit;True;Property;_NormalTexture;NormalTexture;6;1;[Normal];Create;True;0;0;False;0;False;-1;None;bbc4bddfb08d19741ad65e80621e2262;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;10;-491.5894,-141.3541;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StaticSwitch;6;-465,90.5;Inherit;False;Property;_Mask_alpha;Mask_alpha;1;0;Create;True;0;0;False;0;False;0;0;1;True;;Toggle;2;Key0;Key1;Create;False;False;9;1;FLOAT3;0,0,0;False;0;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT3;0,0,0;False;5;FLOAT3;0,0,0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;5;-134,24.5;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT3;0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;3;-259,-169.9;Inherit;False;Constant;_Float1;Float 1;3;0;Create;True;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;4;-259.4,-81.30001;Inherit;False;Constant;_Float2;Float 2;3;0;Create;True;0;0;False;0;False;1.4;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.Vector3Node;12;-571.6256,-488.6948;Inherit;False;Constant;_Vector0;Vector 0;5;0;Create;True;0;0;False;0;False;0,0,0;0,0,0;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.ComponentMaskNode;13;-621.6256,-351.6948;Inherit;False;True;True;True;True;1;0;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.LerpOp;1;39,-148.5;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;9;-333.0894,-362.0543;Inherit;False;3;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.RangedFloatNode;2;122,16.5;Inherit;False;Constant;_Float0;Float 0;3;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;357,-287;Half;False;True;-1;2;ASEMaterialInspector;0;0;BlinnPhong;hc/partical/Distortion;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;False;False;False;False;False;False;Off;2;False;-1;3;False;-1;False;0;False;-1;0;False;-1;False;3;Transparent;0.5;True;False;0;False;Transparent;;Transparent;ForwardOnly;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;1;-1;3;-1;0;False;0;0;False;-1;0;0;False;-1;0;0;0;False;0;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;37;0;41;0
WireConnection;37;1;40;0
WireConnection;38;0;39;0
WireConnection;35;0;36;0
WireConnection;35;2;37;0
WireConnection;35;1;38;0
WireConnection;7;0;8;0
WireConnection;34;1;35;0
WireConnection;10;0;14;0
WireConnection;10;1;15;0
WireConnection;6;1;7;0
WireConnection;6;0;8;4
WireConnection;5;0;10;0
WireConnection;5;1;6;0
WireConnection;13;0;34;0
WireConnection;1;0;3;0
WireConnection;1;1;4;0
WireConnection;1;2;5;0
WireConnection;9;0;12;0
WireConnection;9;1;13;0
WireConnection;9;2;10;0
WireConnection;0;1;9;0
WireConnection;0;8;1;0
WireConnection;0;9;2;0
ASEEND*/
//CHKSM=5367B211488E262085B63F7C797449CAD1DF1B0D