Shader "Hidden/VirtualTexture/FeedbackDownScale"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	}

	// Universal Render Pipeline subshader. If URP is installed this will be used.
	SubShader
	{
		Cull Off ZWrite Off ZTest Always
		Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline"}

		Pass
		{
			Tags { "LightMode" = "UniversalForward" }

			HLSLPROGRAM
			#include "VTFeedback.cginc"	
			#pragma vertex VTVert
			#pragma fragment frag
			float4 frag(VTV2f i) : SV_Target
			{
				return GetMaxFeedback(i.uv, 2);
			}
			ENDHLSL
		}

		Pass
		{
			Tags { "LightMode" = "UniversalForward" }

			HLSLPROGRAM
			#include "VTFeedback.cginc"	
			#pragma vertex VTVert
			#pragma fragment frag
			float4 frag(VTV2f i) : SV_Target
			{
				return GetMaxFeedback(i.uv, 4);
			}
			ENDHLSL
		}

		Pass
		{
			Tags { "LightMode" = "UniversalForward" }

			HLSLPROGRAM
			#include "VTFeedback.cginc"	
			#pragma vertex VTVert
			#pragma fragment frag
			float4 frag(VTV2f i) : SV_Target
			{
				return GetMaxFeedback(i.uv, 8);
			}
			ENDHLSL
		}
	}
}

