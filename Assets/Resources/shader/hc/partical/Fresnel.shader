// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "hc/partical/Fresnel"
{
	Properties
	{
		[Enum(UnityEngine.Rendering.BlendMode)]_Src("Src", Float) = 0
		[Enum(UnityEngine.Rendering.BlendMode)]_Dst("Dst", Float) = 0
		[HDR]_Color("Color", Color) = (1,1,1,1)
		_Frenel_range("Frenel_range", Range( 0 , 5)) = 0
		_Tex("Tex", 2D) = "white" {}
		_DepthFade("DepthFade", Float) = 0
		_u_speed("u_speed", Float) = 0
		_v_speed("v_speed", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)]_Ztest("Ztest", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)]_Zwrite("Zwrite", Float) = 0
		[Enum(UnityEngine.Rendering.CullMode)]_Cullmode1("Cull mode", Float) = 0
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
		Cull [_Cullmode1]
		ZWrite [_Zwrite]
		ZTest [_Ztest]
		Blend [_Src] [_Dst]
		
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityCG.cginc"
		#include "UnityUI.cginc"
		#pragma target 3.0
		#pragma surface surf Unlit keepalpha noshadow 

		#pragma multi_compile __ UNITY_UI_CLIP_RECT
		//�Ż� add
		#pragma skip_variants  LIGHTPROBE_SH DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING _SPECULARHIGHLIGHTS_OFF VERTEXLIGHT_ON 
		#pragma skip_variants  UNITY_SPECCUBE_BOX_PROJECTION UNITY_SPECCUBE_BLENDING 
		#pragma skip_variants FOG_EXP FOG_EXP2
		#pragma skip_variants DIRECTIONAL_COOKIE POINT_COOKIE SPOT _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
		struct Input
		{
			float4 vertexColor : COLOR;
			float2 uv_texcoord;
			float3 viewDir;
			half3 worldNormal;
			half ASEVFace : VFACE;
			float4 screenPos;
			float3 vertex : POSITION;
		};

		uniform half4 _Color;
		uniform sampler2D _Tex;
		SamplerState sampler_Tex;
		uniform float _u_speed;
		uniform float _v_speed;
		uniform float4 _Tex_ST;
		uniform float _Frenel_range;
		UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
		uniform float4 _CameraDepthTexture_TexelSize;
		uniform float _DepthFade;
		uniform float4 _ClipRect;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			half4 appendResult22 = (half4(_u_speed , _v_speed , 0.0 , 0.0));
			float2 uv_Tex = i.uv_texcoord * _Tex_ST.xy + _Tex_ST.zw;
			half2 panner18 = ( 1.0 * _Time.y * appendResult22.xy + uv_Tex);
			half4 tex2DNode17 = tex2D( _Tex, panner18 );
			half3 ase_worldNormal = i.worldNormal;
			half dotResult5 = dot( i.viewDir , ( ase_worldNormal * i.ASEVFace ) );
			half temp_output_11_0 = pow( ( 1.0 - saturate( abs( dotResult5 ) ) ) , _Frenel_range );
			float4 ase_screenPos = float4( i.screenPos.xyz , i.screenPos.w + 0.00000000001 );
			half4 ase_screenPosNorm = ase_screenPos / ase_screenPos.w;
			ase_screenPosNorm.z = ( UNITY_NEAR_CLIP_VALUE >= 0 ) ? ase_screenPosNorm.z : ase_screenPosNorm.z * 0.5 + 0.5;
			float screenDepth23 = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE( _CameraDepthTexture, ase_screenPosNorm.xy ));
			half distanceDepth23 = abs( ( screenDepth23 - LinearEyeDepth( ase_screenPosNorm.z ) ) / ( _DepthFade ) );
			half clampResult26 = clamp( distanceDepth23 , 0.0 , 1.0 );
			o.Emission = ( _Color * i.vertexColor * ( ( tex2DNode17.r * temp_output_11_0 ) + ( 1.0 - clampResult26 ) ) ).rgb;
			float mask2dIsClip = 1;
			#ifdef UNITY_UI_CLIP_RECT
			mask2dIsClip = UnityGet2DClipping(i.vertex.xy, _ClipRect);
			#endif
			o.Alpha = ( _Color.a * i.vertexColor.a * temp_output_11_0 * tex2DNode17.r * mask2dIsClip );
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18703
7;1;1906;1130;-1422.644;1069.28;1;True;True
Node;AmplifyShaderEditor.WorldNormalVector;7;-423,-76.5;Inherit;False;False;1;0;FLOAT3;0,0,1;False;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.FaceVariableNode;35;-468.3339,143.2943;Inherit;False;0;1;FLOAT;0
Node;AmplifyShaderEditor.ViewDirInputsCoordNode;6;-424,-238.5;Float;False;World;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;34;-132.3303,89.2937;Inherit;False;2;2;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DotProductOpNode;5;-70,-84.5;Inherit;False;2;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;21;88.54024,-565.7744;Float;False;Property;_v_speed;v_speed;8;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;20;92.65021,-651.838;Float;False;Property;_u_speed;u_speed;7;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.AbsOpNode;10;126.9272,-59.64612;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;9;315.9272,-55.64612;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;19;65.34363,-827.6897;Inherit;False;0;17;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;24;391.0133,461.0118;Float;False;Property;_DepthFade;DepthFade;6;0;Create;True;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;22;300.6382,-631.8835;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.PannerNode;18;441.894,-752.5206;Inherit;False;3;0;FLOAT2;0,0;False;2;FLOAT2;0,0;False;1;FLOAT;1;False;1;FLOAT2;0
Node;AmplifyShaderEditor.OneMinusNode;8;533.7666,-72.946;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;12;254.8272,121.8539;Float;False;Property;_Frenel_range;Frenel_range;4;0;Create;True;0;0;False;0;False;0;0.39;0;5;0;1;FLOAT;0
Node;AmplifyShaderEditor.DepthFade;23;600.6718,406.5005;Inherit;False;True;False;True;2;1;FLOAT3;0,0,0;False;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClampOpNode;26;983.6481,389.7278;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;17;675.1194,-774.5245;Inherit;True;Property;_Tex;Tex;5;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.PowerNode;11;760.0928,-64.7528;Inherit;True;False;2;0;FLOAT;0;False;1;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;29;1239.764,336.8362;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;28;1026.977,-446.1108;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;27;1374.125,-377.0704;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;15;1235.418,-1027.582;Half;False;Property;_Color;Color;2;1;[HDR];Create;True;0;0;False;0;False;1,1,1,1;0.02745098,0,0.1254902,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;13;1319.045,-779.8275;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;32;-851.1102,-708.3751;Float;False;Property;_Dst;Dst;1;1;[Enum];Fetch;False;0;1;UnityEngine.Rendering.BlendMode;True;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;14;1968.451,-831.1348;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;37;-883.4246,-321.7876;Float;False;Property;_Zwrite;Zwrite;10;1;[Enum];Fetch;True;0;1;UnityEngine.Rendering.CompareFunction;True;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;36;-876.3971,-427.5509;Float;False;Property;_Ztest;Ztest;9;1;[Enum];Fetch;True;0;1;UnityEngine.Rendering.CompareFunction;True;0;False;0;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;16;1982.477,-353.5834;Inherit;False;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;33;-860.1102,-898.3751;Float;False;Property;_Cullmode1;Cull mode;11;1;[Enum];Fetch;True;0;1;UnityEngine.Rendering.CullMode;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;31;-860.1102,-810.3751;Float;False;Property;_Src;Src;0;1;[Enum];Fetch;False;0;1;UnityEngine.Rendering.BlendMode;True;0;False;0;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;1;2529.792,-481.7548;Half;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;hc/partical/Fresnel;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;1;True;37;0;True;36;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;False;0;True;Custom;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;1;5;True;31;10;True;32;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;3;-1;-1;-1;0;False;0;0;True;33;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;34;0;7;0
WireConnection;34;1;35;0
WireConnection;5;0;6;0
WireConnection;5;1;34;0
WireConnection;10;0;5;0
WireConnection;9;0;10;0
WireConnection;22;0;20;0
WireConnection;22;1;21;0
WireConnection;18;0;19;0
WireConnection;18;2;22;0
WireConnection;8;0;9;0
WireConnection;23;0;24;0
WireConnection;26;0;23;0
WireConnection;17;1;18;0
WireConnection;11;0;8;0
WireConnection;11;1;12;0
WireConnection;29;0;26;0
WireConnection;28;0;17;1
WireConnection;28;1;11;0
WireConnection;27;0;28;0
WireConnection;27;1;29;0
WireConnection;14;0;15;0
WireConnection;14;1;13;0
WireConnection;14;2;27;0
WireConnection;16;0;15;4
WireConnection;16;1;13;4
WireConnection;16;2;11;0
WireConnection;16;3;17;1
WireConnection;1;2;14;0
WireConnection;1;9;16;0
ASEEND*/
//CHKSM=8407A70E57CD39FA73AEF7316CC2F8C6AE6FD5A7