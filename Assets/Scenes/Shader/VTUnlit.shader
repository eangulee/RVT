Shader "VirtualTexture/Unlit"
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
			#include "VTShading.cginc"	
			#pragma vertex VTVert
			#pragma fragment VTFragUnlit
			ENDHLSL
		}
	}
}


