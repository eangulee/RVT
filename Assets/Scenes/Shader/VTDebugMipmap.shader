Shader "Hidden/VirtualTexture/DebugMipmap"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
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
			#pragma fragment frag

			sampler2D _MainTex;

			float4 frag(VTV2f i) : SV_Target
			{
				return VTDebugMipmap(_MainTex, i.uv);
			}
			ENDHLSL
		}
	}
}
