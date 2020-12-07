Shader "Custom/UnlitTexture"
{
	Properties
	{
		[MainColor] _BaseColor("BaseColor", Color) = (1,1,1,1)
		[MainTexture] _BaseMap("BaseMap", 2D) = "white" {}
	}

		// Universal Render Pipeline subshader. If URP is installed this will be used.
		SubShader
	{
		Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline"}

		Pass
		{
			Tags { "LightMode" = "UniversalForward" }

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			struct Attributes
			{
				float4 positionOS   : POSITION;
				float2 uv           : TEXCOORD0;
								UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct Varyings
			{
				float2 uv           : TEXCOORD0;
				float4 positionHCS  : SV_POSITION;
								UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			TEXTURE2D(_BaseMap);
			SAMPLER(sampler_BaseMap);

			CBUFFER_START(UnityPerMaterial)
			float4 _BaseMap_ST;
			half4 _BaseColor;
			CBUFFER_END

			Varyings vert(Attributes IN)
			{
								Varyings OUT;
								UNITY_SETUP_INSTANCE_ID(IN);
								UNITY_TRANSFER_INSTANCE_ID(IN, OUT);
				
				VertexPositionInputs input;
				input.positionWS = TransformObjectToWorld(IN.positionOS);
				OUT.uv = input.positionWS.xy;

				OUT.positionHCS = float4(0,0,0,1);
				//OUT.positionHCS = TransformWorldToHClip(IN.positionOS.xyz);
				return OUT;
			}

			half4 frag(Varyings IN) : SV_Target
			{
				return 1;
			}
			ENDHLSL
		}
	}
}
