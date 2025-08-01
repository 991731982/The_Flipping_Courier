// Made with Amplify Shader Editor v1.9.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "EdgeMelt"
{
	Properties
	{
		_TextureSample0("Texture Sample 0", 2D) = "white" {}
		_DoorDisappear("DoorDisappear", Range( 0 , 1)) = 0
		_EdgeWidth("Edge Width", Range( 0 , 1)) = 0.2436928
		_DoorRamp("DoorRamp", 2D) = "white" {}
		_MainTex("MainTex", 2D) = "white" {}
		[HDR]_EdgeColor("EdgeColor", Color) = (1,1,1,1)
		_MainColor("MainColor", Color) = (1,1,1,1)
		_Maskrange("Mask range", Range( 0 , 1)) = 0.5
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Transparent" "Queue"="Transparent" }
	LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend SrcAlpha OneMinusSrcAlpha
		AlphaToMask Off
		Cull Back
		ColorMask RGBA
		ZWrite Off
		ZTest LEqual
		Offset 0 , 0
		
		
		
		Pass
		{
			Name "Unlit"

			CGPROGRAM

			

			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			//only defining to not throw compilation error over Unity 5.5
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			#define ASE_NEEDS_FRAG_COLOR


			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 worldPos : TEXCOORD0;
				#endif
				float4 ase_texcoord1 : TEXCOORD1;
				float4 ase_color : COLOR;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform sampler2D _DoorRamp;
			uniform float _EdgeWidth;
			uniform sampler2D _TextureSample0;
			uniform float4 _TextureSample0_ST;
			uniform float _DoorDisappear;
			uniform float4 _EdgeColor;
			uniform sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			uniform float4 _MainColor;
			uniform float _Maskrange;

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.ase_texcoord1.xy = v.ase_texcoord.xy;
				o.ase_color = v.color;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord1.zw = 0;
				float3 vertexValue = float3(0, 0, 0);
				#if ASE_ABSOLUTE_VERTEX_POS
				vertexValue = v.vertex.xyz;
				#endif
				vertexValue = vertexValue;
				#if ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif
				o.vertex = UnityObjectToClipPos(v.vertex);

				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				#endif
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				fixed4 finalColor;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 WorldPosition = i.worldPos;
				#endif
				float2 uv_TextureSample0 = i.ase_texcoord1.xy * _TextureSample0_ST.xy + _TextureSample0_ST.zw;
				float smoothstepResult7 = smoothstep( 0.0 , _EdgeWidth , ( tex2D( _TextureSample0, uv_TextureSample0 ).r + 1.0 + ( -2.0 * _DoorDisappear ) ));
				float2 appendResult9 = (float2(smoothstepResult7 , 0.0));
				float4 tex2DNode12 = tex2D( _DoorRamp, appendResult9 );
				float2 uv_MainTex = i.ase_texcoord1.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				float4 tex2DNode13 = tex2D( _MainTex, uv_MainTex );
				float3 temp_cast_0 = (tex2DNode12.b).xxx;
				float4 lerpResult18 = lerp( ( tex2DNode12 * _EdgeColor ) , ( tex2DNode13 * _MainColor * i.ase_color ) , saturate( ( 1.0 - ( ( distance( float3( 0,0,0 ) , temp_cast_0 ) - _Maskrange ) / max( 0.0 , 1E-05 ) ) ) ));
				float4 appendResult25 = (float4((lerpResult18).rgb , ( tex2DNode13.a * _MainColor.a * i.ase_color.a * saturate( smoothstepResult7 ) )));
				
				
				finalColor = appendResult25;
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	Fallback Off
}
/*ASEBEGIN
Version=19200
Node;AmplifyShaderEditor.DynamicAppendNode;9;290.4862,-15.90015;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;12;523.1617,-20.08319;Inherit;True;Property;_DoorRamp;DoorRamp;3;0;Create;True;0;0;0;False;0;False;-1;3ad2c86c39f0eae48a608da70213d052;3ad2c86c39f0eae48a608da70213d052;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;15;908.996,42.02147;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.ColorNode;14;524.5961,171.9548;Inherit;False;Property;_EdgeColor;EdgeColor;5;1;[HDR];Create;True;0;0;0;False;0;False;1,1,1,1;8.3522,9.368564,32,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SamplerNode;13;252.755,586.8456;Inherit;True;Property;_MainTex;MainTex;4;0;Create;True;0;0;0;False;0;False;-1;a57489fbda1ab7b40b4d411819b530eb;a57489fbda1ab7b40b4d411819b530eb;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;21;300.3928,787.434;Inherit;False;Property;_MainColor;MainColor;6;0;Create;True;0;0;0;False;0;False;1,1,1,1;0.9779873,0.9786703,1,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;20;327.8082,958.98;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;17;658.932,584.6054;Inherit;True;3;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.FunctionNode;22;842.9225,304.9333;Inherit;True;Color Mask;-1;;1;eec747d987850564c95bde0e5a6d1867;0;4;1;FLOAT3;0,0,0;False;3;FLOAT3;0,0,0;False;4;FLOAT;0;False;5;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;23;512.884,344.8203;Inherit;True;Property;_Maskrange;Mask range;7;0;Create;True;0;0;0;False;0;False;0.5;1;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;18;1244.192,398.2089;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.ComponentMaskNode;24;1586.308,405.1831;Inherit;True;True;True;True;False;1;0;COLOR;0,0,0,0;False;1;FLOAT3;0
Node;AmplifyShaderEditor.DynamicAppendNode;25;1967.641,421.65;Inherit;False;FLOAT4;4;0;FLOAT3;0,0,0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;1459.774,756.1835;Inherit;True;4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;2;-369.9696,-16.95981;Inherit;True;3;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;3;-618.6367,176.3735;Inherit;False;Constant;_Float0;Float 0;1;0;Create;True;0;0;0;False;0;False;1;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;4;-846.6368,247.7069;Inherit;False;Constant;_Float1;Float 1;1;0;Create;True;0;0;0;False;0;False;-2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;6;-619.3033,266.3735;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;1;-751.9702,-17.6265;Inherit;True;Property;_TextureSample0;Texture Sample 0;0;0;Create;True;0;0;0;False;0;False;-1;c81e5c51fcb6ab54381a3b902a0f1886;c81e5c51fcb6ab54381a3b902a0f1886;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SmoothstepOpNode;7;-59.98048,-17.30008;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;8;-372.9802,244.2332;Inherit;False;Property;_EdgeWidth;Edge Width;2;0;Create;True;0;0;0;False;0;False;0.2436928;0.5;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SaturateNode;27;205.0417,1267.782;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;2188.085,410.114;Float;False;True;-1;2;ASEMaterialInspector;100;5;EdgeMelt;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;True;True;2;5;False;;10;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;0;False;;255;False;;255;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;0;False;;True;True;2;False;;True;3;False;;True;True;0;False;;0;False;;True;2;RenderType=Transparent=RenderType;Queue=Transparent=Queue=0;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;0;1;True;False;;False;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;29;-995.401,637.8678;Inherit;False;1;-1;3;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;5;-1008.836,356.6402;Inherit;True;Property;_DoorDisappear;DoorDisappear;1;0;Create;True;0;0;0;False;0;False;0;0;0;1;0;1;FLOAT;0
WireConnection;9;0;7;0
WireConnection;12;1;9;0
WireConnection;15;0;12;0
WireConnection;15;1;14;0
WireConnection;17;0;13;0
WireConnection;17;1;21;0
WireConnection;17;2;20;0
WireConnection;22;1;12;3
WireConnection;22;4;23;0
WireConnection;18;0;15;0
WireConnection;18;1;17;0
WireConnection;18;2;22;0
WireConnection;24;0;18;0
WireConnection;25;0;24;0
WireConnection;25;3;26;0
WireConnection;26;0;13;4
WireConnection;26;1;21;4
WireConnection;26;2;20;4
WireConnection;26;3;27;0
WireConnection;2;0;1;1
WireConnection;2;1;3;0
WireConnection;2;2;6;0
WireConnection;6;0;4;0
WireConnection;6;1;5;0
WireConnection;7;0;2;0
WireConnection;7;2;8;0
WireConnection;27;0;7;0
WireConnection;0;0;25;0
ASEEND*/
//CHKSM=D9692637CF04764C6B022C67FA30400A8EA6AEF8