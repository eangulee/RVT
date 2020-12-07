Shader "VirtualTexture/Debug"
{
	Properties
	{
	}

	// Universal Render Pipeline subshader. If URP is installed this will be used.
	SubShader
	{
		Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline"}

		Pass
		{
			Tags { "LightMode" = "UniversalForward" }

			HLSLPROGRAM
			#include "VTDebug.cginc"	
			#pragma vertex VTVert
			#pragma fragment VTFragDebug
			ENDHLSL
		}
	}
}
