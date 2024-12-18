// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "hc/partical/uv_mask"
{
	Properties
	{
		[Enum(UnityEngine.Rendering.BlendMode)]_Src1("Src", Float) = 0
		[Enum(UnityEngine.Rendering.BlendMode)]_Dst1("Dst", Float) = 0
		[Enum(UnityEngine.Rendering.CompareFunction)]_Ztest1("Ztest", Float) = 0
		[Toggle]_Zwrite1("Zwrite", Float) = 0
		[HDR]_Color("Color", Color) = (1,1,1,1)
		[Enum(UnityEngine.Rendering.CullMode)]_Cullmode1("Cull mode", Float) = 0
		[KeywordEnum(R,G,B,A)] _USE("USE", Float) = 0
		_WaveA_Tex("WaveA_Tex", 2D) = "white" {}
		_U_waveA("U_waveA", Float) = 0
		_V_waveA("V_waveA", Float) = 0
		_WaveB_Tex("WaveB_Tex", 2D) = "white" {}
		_U_waveB("U_waveB", Float) = 0
		_V_waveB("V_waveB", Float) = 0
		_Mask_Tex("Mask_Tex", 2D) = "white" {}
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
		ZWrite [_Zwrite1]
		ZTest [_Ztest1]
		Blend [_Src1] [_Dst1]
		
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#include "UnityUI.cginc"
		#pragma target 3.0

		#pragma multi_compile __ UNITY_UI_CLIP_RECT
		//�Ż� _USE_A
		#pragma shader_feature_local _USE_R _USE_G _USE_B _USE_A
		#pragma surface surf Unlit keepalpha noshadow 
		//�Ż� add
		#pragma skip_variants  LIGHTPROBE_SH DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MIXING _SPECULARHIGHLIGHTS_OFF  VERTEXLIGHT_ON 
		#pragma skip_variants  UNITY_SPECCUBE_BOX_PROJECTION UNITY_SPECCUBE_BLENDING 
		#pragma skip_variants FOG_EXP FOG_EXP2
		#pragma skip_variants DIRECTIONAL_COOKIE POINT_COOKIE SPOT _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A
		struct Input
		{
			float2 uv_texcoord;
			float4 vertexColor : COLOR;
			float3 vertex : POSITION;
		};

		uniform half4 _Color;
		uniform sampler2D _WaveA_Tex;
		SamplerState sampler_WaveA_Tex;
		uniform float4 _WaveA_Tex_ST;
		uniform half _U_waveA;
		uniform half _V_waveA;
		uniform sampler2D _WaveB_Tex;
		SamplerState sampler_WaveB_Tex;
		uniform float4 _WaveB_Tex_ST;
		uniform half _U_waveB;
		uniform half _V_waveB;
		uniform sampler2D _Mask_Tex;
		SamplerState sampler_Mask_Tex;
		uniform half4 _Mask_Tex_ST;
		uniform float4 _ClipRect;

		inline half4 LightingUnlit( SurfaceOutput s, half3 lightDir, half atten )
		{
			return half4 ( 0, 0, 0, s.Alpha );
		}

		void surf( Input i , inout SurfaceOutput o )
		{
			float2 uv_WaveA_Tex = i.uv_texcoord * _WaveA_Tex_ST.xy + _WaveA_Tex_ST.zw;
			half4 appendResult9 = (half4(_U_waveA , _V_waveA , 0.0 , 0.0));
			half4 tex2DNode5 = tex2D( _WaveA_Tex, ( half4( uv_WaveA_Tex, 0.0 , 0.0 ) + ( appendResult9 * _Time.y ) ).xy );
			
			#if defined(_USE_R)
				half staticSwitch42 = tex2DNode5.r;
			#elif defined(_USE_G)
				half staticSwitch42 = tex2DNode5.g;
			#elif defined(_USE_B)
				half staticSwitch42 = tex2DNode5.b;
			#elif defined(_USE_A)
				half staticSwitch42 = tex2DNode5.a;
			#else
				half staticSwitch42 = tex2DNode5.r;
			#endif
			float2 uv_WaveB_Tex = i.uv_texcoord * _WaveB_Tex_ST.xy + _WaveB_Tex_ST.zw;
			half4 appendResult14 = (half4(_U_waveB , _V_waveB , 0.0 , 0.0));
			half temp_output_4_0 = ( staticSwitch42 * tex2D( _WaveB_Tex, ( half4( uv_WaveB_Tex, 0.0 , 0.0 ) + ( appendResult14 * _Time.y ) ).xy ).r );
			o.Emission = ( _Color * temp_output_4_0 * i.vertexColor ).rgb;
			float2 uv_Mask_Tex = i.uv_texcoord * _Mask_Tex_ST.xy + _Mask_Tex_ST.zw;
			float mask2dIsClip = 1;
			#ifdef UNITY_UI_CLIP_RECT
			mask2dIsClip = UnityGet2DClipping(i.vertex.xy, _ClipRect);
			#endif
			o.Alpha = ( _Color.a * temp_output_4_0 * i.vertexColor.a * tex2D( _Mask_Tex, uv_Mask_Tex ).r  * mask2dIsClip);
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18703
24;7;1626;1124;4418.015;3925.963;5.48303;True;True
Node;AmplifyShaderEditor.RangedFloatNode;10;-2204.48,-656.9794;Inherit;False;Property;_U_waveA;U_waveA;8;0;Create;True;0;0;False;0;False;0;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;11;-2265.095,-529.0915;Inherit;False;Property;_V_waveA;V_waveA;9;0;Create;True;0;0;False;0;False;0;0.3;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;15;-1525.848,355.3259;Inherit;False;Property;_V_waveB;V_waveB;12;0;Create;True;0;0;False;0;False;0;-0.15;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;16;-1538.848,267.3259;Inherit;False;Property;_U_waveB;U_waveB;11;0;Create;True;0;0;False;0;False;0;0.2;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;22;-1802.51,-410.9452;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.DynamicAppendNode;9;-1771.855,-585.376;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.DynamicAppendNode;14;-1300.967,236.4303;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleTimeNode;28;-1205.85,453.6506;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;32;-1421.661,-460.4454;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;8;-1937.855,-806.376;Inherit;False;0;5;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.TextureCoordinatesNode;13;-1416.848,57.32594;Inherit;False;0;6;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;-959.282,316.2767;Inherit;False;2;2;0;FLOAT4;0,0,0,0;False;1;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleAddOpNode;7;-1190.51,-703.8392;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SamplerNode;5;-1108.948,-739.698;Inherit;True;Property;_WaveA_Tex;WaveA_Tex;7;0;Create;True;0;0;False;0;False;-1;None;13dac46096cf24843bfd470c2b322171;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;26;-966.209,135.385;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT4;0,0,0,0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SamplerNode;6;-674.5762,-75.37062;Inherit;True;Property;_WaveB_Tex;WaveB_Tex;10;0;Create;True;0;0;False;0;False;-1;None;4f1d02de04811ba44803d33592b684e5;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.StaticSwitch;42;-671.9861,-692.465;Inherit;False;Property;_USE;USE;6;0;Create;True;0;0;False;0;False;0;0;0;True;;KeywordEnum;4;R;G;B;A;Create;False;True;9;1;FLOAT;0;False;0;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT;0;False;7;FLOAT;0;False;8;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;3;-177.0299,-709.5858;Inherit;False;Property;_Color;Color;4;1;[HDR];Create;True;0;0;False;0;False;1,1,1,1;95.87451,12.04706,4.517647,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;4;-320,-112.5;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;18;-317.8489,119.7202;Inherit;True;Property;_Mask_Tex;Mask_Tex;14;0;Create;True;0;0;False;0;False;-1;None;None;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;17;-319.531,-516.0858;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;35;-3080.719,-309.6329;Float;False;Property;_Dst1;Dst;1;1;[Enum];Fetch;True;0;1;UnityEngine.Rendering.BlendMode;True;0;False;0;10;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;36;-3089.719,-499.6329;Float;False;Property;_Cullmode1;Cull mode;5;1;[Enum];Fetch;True;0;1;UnityEngine.Rendering.CullMode;True;0;False;0;1;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;2;159.2359,-80.93147;Inherit;False;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;1;272.8144,-442.4166;Inherit;False;3;3;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.RangedFloatNode;34;-3089.719,-411.6329;Float;False;Property;_Src1;Src;0;1;[Enum];Fetch;True;0;1;UnityEngine.Rendering.BlendMode;True;0;False;0;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;41;-2846.519,213.7434;Float;False;Property;_Ztest1;Ztest;2;1;[Enum];Fetch;True;0;1;UnityEngine.Rendering.CompareFunction;True;0;False;0;4;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;37;-2869.352,296.4453;Float;False;Property;_Zwrite1;Zwrite;3;1;[Toggle];Fetch;True;0;1;UnityEngine.Rendering.CompareFunction;True;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;637.8672,-291.4627;Half;False;True;-1;2;ASEMaterialInspector;0;0;Unlit;hc/partical/uv_mask;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Off;2;True;37;0;True;41;False;0;False;-1;0;False;-1;False;0;Custom;0.5;True;False;0;True;Custom;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;15;10;25;False;0.5;False;1;5;True;34;10;True;35;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;13;-1;-1;-1;0;False;0;0;True;36;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;15;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;9;0;10;0
WireConnection;9;1;11;0
WireConnection;14;0;16;0
WireConnection;14;1;15;0
WireConnection;32;0;9;0
WireConnection;32;1;22;0
WireConnection;27;0;14;0
WireConnection;27;1;28;0
WireConnection;7;0;8;0
WireConnection;7;1;32;0
WireConnection;5;1;7;0
WireConnection;26;0;13;0
WireConnection;26;1;27;0
WireConnection;6;1;26;0
WireConnection;42;1;5;1
WireConnection;42;0;5;2
WireConnection;42;2;5;3
WireConnection;42;3;5;4
WireConnection;4;0;42;0
WireConnection;4;1;6;1
WireConnection;2;0;3;4
WireConnection;2;1;4;0
WireConnection;2;2;17;4
WireConnection;2;3;18;1
WireConnection;1;0;3;0
WireConnection;1;1;4;0
WireConnection;1;2;17;0
WireConnection;0;2;1;0
WireConnection;0;9;2;0
ASEEND*/
//CHKSM=EDD92A88330F28417F6FC1E4B5AA1A944D2A4D53