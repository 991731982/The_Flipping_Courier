// Made with Amplify Shader Editor v1.9.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "DoorDisappear"
{
	Properties
	{
		_Door_Door01_BaseMap("Door_Door01_BaseMap", 2D) = "white" {}
		_Door_Door01_Normal("Door_Door01_Normal", 2D) = "bump" {}
		_Door_Door01_Emissive("Door_Door01_Emissive", 2D) = "white" {}
		[HDR]_Color0("Color 0", Color) = (0.8553459,0.519125,0.519125,0)
		_Door_Door01_MaskMap("Door_Door01_MaskMap", 2D) = "white" {}
		_AxisHeight("Axis Height", Float) = 0.01
		[HDR]_Glowingedge("Glowing edge", Color) = (0.9308176,0.8833435,0.4185751,0)
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		[HDR]_MeltingColor("Melting Color", Color) = (1,1,1,0)
		_TextureBrightness("Texture Brightness", Range( 0 , 1)) = 0.87
		_WholeBrightness("Whole Brightness", Range( 0 , 1)) = 0.15
		_DoorShader02("DoorShader02", 2D) = "white" {}
		_GlowingEdgeProcess("Glowing Edge Process", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord3( "", 2D ) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IsEmissive" = "true"  }
		Cull Back
		ZWrite On
		Blend SrcAlpha OneMinusSrcAlpha
		
		CGINCLUDE
		#include "UnityPBSLighting.cginc"
		#include "Lighting.cginc"
		#pragma target 3.0
		struct Input
		{
			float2 uv_texcoord;
			float2 uv3_texcoord3;
		};

		uniform sampler2D _Door_Door01_Normal;
		uniform float4 _Door_Door01_Normal_ST;
		uniform sampler2D _Door_Door01_BaseMap;
		uniform float4 _Door_Door01_BaseMap_ST;
		uniform sampler2D _Door_Door01_Emissive;
		uniform float4 _Door_Door01_Emissive_ST;
		uniform float4 _Color0;
		uniform float _AxisHeight;
		uniform float4 _Glowingedge;
		uniform sampler2D _TextureSample0;
		uniform float4 _MeltingColor;
		uniform float _TextureBrightness;
		uniform float _WholeBrightness;
		uniform float _GlowingEdgeProcess;
		uniform sampler2D _DoorShader02;
		uniform float4 _DoorShader02_ST;
		uniform sampler2D _Door_Door01_MaskMap;
		uniform float4 _Door_Door01_MaskMap_ST;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float2 uv_Door_Door01_Normal = i.uv_texcoord * _Door_Door01_Normal_ST.xy + _Door_Door01_Normal_ST.zw;
			o.Normal = UnpackNormal( tex2D( _Door_Door01_Normal, uv_Door_Door01_Normal ) );
			float2 uv_Door_Door01_BaseMap = i.uv_texcoord * _Door_Door01_BaseMap_ST.xy + _Door_Door01_BaseMap_ST.zw;
			o.Albedo = tex2D( _Door_Door01_BaseMap, uv_Door_Door01_BaseMap ).rgb;
			float2 uv_Door_Door01_Emissive = i.uv_texcoord * _Door_Door01_Emissive_ST.xy + _Door_Door01_Emissive_ST.zw;
			float temp_output_8_0 = ( i.uv3_texcoord3.x + _AxisHeight );
			float temp_output_15_0 = ( 1.0 - step( temp_output_8_0 , 0.49 ) );
			float2 uv_DoorShader02 = i.uv_texcoord * _DoorShader02_ST.xy + _DoorShader02_ST.zw;
			float4 tex2DNode29 = tex2D( _DoorShader02, uv_DoorShader02 );
			float temp_output_34_0 = step( _GlowingEdgeProcess , ( tex2DNode29.r + 0.02 ) );
			o.Emission = ( ( tex2D( _Door_Door01_Emissive, uv_Door_Door01_Emissive ).r * _Color0 ) + ( ( temp_output_15_0 - ( 1.0 - step( temp_output_8_0 , 0.5 ) ) ) * _Glowingedge ) + ( ( tex2D( _TextureSample0, i.uv3_texcoord3 ).r * _MeltingColor * _TextureBrightness ) + ( _MeltingColor * _WholeBrightness ) ) + ( _Glowingedge * ( temp_output_34_0 - step( _GlowingEdgeProcess , tex2DNode29.r ) ) ) ).rgb;
			float2 uv_Door_Door01_MaskMap = i.uv_texcoord * _Door_Door01_MaskMap_ST.xy + _Door_Door01_MaskMap_ST.zw;
			float4 tex2DNode6 = tex2D( _Door_Door01_MaskMap, uv_Door_Door01_MaskMap );
			o.Metallic = tex2DNode6.r;
			o.Smoothness = tex2DNode6.g;
			o.Alpha = saturate( ( saturate( temp_output_15_0 ) * temp_output_34_0 ) );
		}

		ENDCG
		CGPROGRAM
		#pragma surface surf Standard keepalpha fullforwardshadows 

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
				float3 worldPos : TEXCOORD2;
				float4 tSpace0 : TEXCOORD3;
				float4 tSpace1 : TEXCOORD4;
				float4 tSpace2 : TEXCOORD5;
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
				float3 worldPos = mul( unity_ObjectToWorld, v.vertex ).xyz;
				half3 worldNormal = UnityObjectToWorldNormal( v.normal );
				half3 worldTangent = UnityObjectToWorldDir( v.tangent.xyz );
				half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
				half3 worldBinormal = cross( worldNormal, worldTangent ) * tangentSign;
				o.tSpace0 = float4( worldTangent.x, worldBinormal.x, worldNormal.x, worldPos.x );
				o.tSpace1 = float4( worldTangent.y, worldBinormal.y, worldNormal.y, worldPos.y );
				o.tSpace2 = float4( worldTangent.z, worldBinormal.z, worldNormal.z, worldPos.z );
				o.customPack1.xy = customInputData.uv_texcoord;
				o.customPack1.xy = v.texcoord;
				o.customPack1.zw = customInputData.uv3_texcoord3;
				o.customPack1.zw = v.texcoord2;
				o.worldPos = worldPos;
				TRANSFER_SHADOW_CASTER_NORMALOFFSET( o )
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
				surfIN.uv3_texcoord3 = IN.customPack1.zw;
				float3 worldPos = IN.worldPos;
				half3 worldViewDir = normalize( UnityWorldSpaceViewDir( worldPos ) );
				SurfaceOutputStandard o;
				UNITY_INITIALIZE_OUTPUT( SurfaceOutputStandard, o )
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
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=19200
Node;AmplifyShaderEditor.SamplerNode;1;-549.8737,-463.9996;Inherit;True;Property;_Door_Door01_BaseMap;Door_Door01_BaseMap;1;0;Create;True;0;0;0;False;0;False;-1;a57489fbda1ab7b40b4d411819b530eb;a57489fbda1ab7b40b4d411819b530eb;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;2;-552.7654,-273.0097;Inherit;True;Property;_Door_Door01_Normal;Door_Door01_Normal;2;0;Create;True;0;0;0;False;0;False;-1;aa6e7733ea80ee448832467dc3cb1fc0;aa6e7733ea80ee448832467dc3cb1fc0;True;0;True;bump;Auto;True;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;3;-1131.762,-244.7041;Inherit;True;Property;_Door_Door01_Emissive;Door_Door01_Emissive;3;0;Create;True;0;0;0;False;0;False;-1;a9c89f60f9f8bc24aa0c8e30a9102c7d;a9c89f60f9f8bc24aa0c8e30a9102c7d;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;18;-1079.246,423.2377;Inherit;False;Property;_Glowingedge;Glowing edge;7;1;[HDR];Create;True;0;0;0;False;0;False;0.9308176,0.8833435,0.4185751,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;4;-822.9759,-119.0524;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;-1106.708,1059.45;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;23;-1574.112,872.018;Inherit;False;Property;_MeltingColor;Melting Color;9;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;22;-1128.733,698.4634;Inherit;True;3;3;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SamplerNode;21;-1636.872,668.3555;Inherit;True;Property;_TextureSample0;Texture Sample 0;8;0;Create;True;0;0;0;False;0;False;-1;9e05c035d0335eb4487471e6b81a62b1;9e05c035d0335eb4487471e6b81a62b1;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;25;-1632.753,1073.184;Inherit;True;Property;_TextureBrightness;Texture Brightness;10;0;Create;True;0;0;0;False;0;False;0.87;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;6;-317.6745,396.6649;Inherit;True;Property;_Door_Door01_MaskMap;Door_Door01_MaskMap;5;0;Create;True;0;0;0;False;0;False;-1;692df5f7c20746148bb86e4155991a1c;692df5f7c20746148bb86e4155991a1c;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleAddOpNode;19;-328.8387,38.8087;Inherit;True;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;38;-209.3786,1088.923;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;39;4.259116,1098.145;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;282.8009,39.961;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;DoorDisappear;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;Back;1;False;;0;False;;False;0;False;;0;False;;False;0;Custom;0.5;True;True;0;True;Transparent;;Transparent;All;12;all;True;True;True;True;0;False;;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;False;2;15;10;25;False;0.5;True;2;5;False;;10;False;;0;0;False;;0;False;;0;False;;0;False;;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;True;Relative;0;;0;-1;-1;-1;0;False;0;0;False;;-1;0;False;;0;0;0;False;0.1;False;;0;False;;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
Node;AmplifyShaderEditor.RangedFloatNode;33;-1514.323,1899.301;Inherit;False;Constant;_Float2;Float 2;14;0;Create;True;0;0;0;False;0;False;0.02;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;32;-1306.323,1787.301;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;34;-1034.323,1787.301;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;30;-1050.323,1515.301;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;35;-738.638,1770.592;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;31;-1625.539,1521.457;Inherit;False;Property;_GlowingEdgeProcess;Glowing Edge Process;13;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;29;-1629.226,1619.522;Inherit;True;Property;_DoorShader02;DoorShader02;12;0;Create;True;0;0;0;False;0;False;-1;c81e5c51fcb6ab54381a3b902a0f1886;c81e5c51fcb6ab54381a3b902a0f1886;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;36;-511.9428,769.5328;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;37;-410.3884,1093.527;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;28;-1633.374,1294.116;Inherit;True;Property;_WholeBrightness;Whole Brightness;11;0;Create;True;0;0;0;False;0;False;0.15;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;-776.5797,274.5702;Inherit;False;2;2;0;FLOAT;0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;20;-1954.257,691.5841;Inherit;False;2;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;47;-3718.93,263.9223;Inherit;False;Constant;_Float3;Float 3;14;0;Create;True;0;0;0;False;0;False;0.45;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;48;-3717.93,345.9221;Inherit;False;Constant;_Float4;Float 3;14;0;Create;True;0;0;0;False;0;False;0.55;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;49;-3510.93,282.9222;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;5;-1093.09,-53.76585;Inherit;False;Property;_Color0;Color 0;4;1;[HDR];Create;True;0;0;0;False;0;False;0.8553459,0.519125,0.519125,0;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;11;-2157.739,296.7964;Inherit;False;Constant;_Float0;Float 0;7;0;Create;True;0;0;0;False;0;False;0.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;12;-2160.607,402.927;Inherit;False;Constant;_Float1;Float 1;7;0;Create;True;0;0;0;False;0;False;0.49;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;13;-1889.774,341.9548;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;15;-1639.738,342.1745;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;7;-2509.139,37.99325;Inherit;False;2;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.OneMinusNode;14;-1640.406,104.0295;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StepOpNode;10;-1888.825,100.9025;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;8;-2175.843,52.13858;Inherit;True;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;9;-2448.168,178.0953;Inherit;False;Property;_AxisHeight;Axis Height;6;0;Create;True;0;0;0;False;0;False;0.01;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;16;-1334.197,189.2894;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;24;-813.3574,724.8303;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
WireConnection;4;0;3;1
WireConnection;4;1;5;0
WireConnection;26;0;23;0
WireConnection;26;1;28;0
WireConnection;22;0;21;1
WireConnection;22;1;23;0
WireConnection;22;2;25;0
WireConnection;21;1;20;0
WireConnection;19;0;4;0
WireConnection;19;1;17;0
WireConnection;19;2;24;0
WireConnection;19;3;36;0
WireConnection;38;0;37;0
WireConnection;38;1;34;0
WireConnection;39;0;38;0
WireConnection;0;0;1;0
WireConnection;0;1;2;0
WireConnection;0;2;19;0
WireConnection;0;3;6;1
WireConnection;0;4;6;2
WireConnection;0;9;39;0
WireConnection;32;0;29;1
WireConnection;32;1;33;0
WireConnection;34;0;31;0
WireConnection;34;1;32;0
WireConnection;30;0;31;0
WireConnection;30;1;29;1
WireConnection;35;0;34;0
WireConnection;35;1;30;0
WireConnection;36;0;18;0
WireConnection;36;1;35;0
WireConnection;37;0;15;0
WireConnection;17;0;16;0
WireConnection;17;1;18;0
WireConnection;49;1;47;0
WireConnection;49;2;48;0
WireConnection;13;0;8;0
WireConnection;13;1;12;0
WireConnection;15;0;13;0
WireConnection;14;0;10;0
WireConnection;10;0;8;0
WireConnection;10;1;11;0
WireConnection;8;0;7;1
WireConnection;8;1;9;0
WireConnection;16;0;15;0
WireConnection;16;1;14;0
WireConnection;24;0;22;0
WireConnection;24;1;26;0
ASEEND*/
//CHKSM=C601B1DDBA5D237B7A54D528A0F44459D455EAA5