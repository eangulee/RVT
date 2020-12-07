#ifndef VIRTUAL_TEXTURE_DEBUG_INCLUDED
#define VIRTUAL_TEXTURE_DEBUG_INCLUDED

#include "VirtualTexture.cginc"

float VTGetMipLevel(float2 uv)
{
    float4 page = tex2D(_VTLookupTex, uv);
	return page.b * 255;
}

float4 VTGetMipColor(float mip)
{
    return float4(clamp(1 - mip * 0.1, 0, 1), 0, 0, 1);
}


float4 VTDebugMipmap(sampler2D tex, float2 uv) : SV_Target
{
	return VTGetMipColor(tex2D(tex, uv).b * 255);
}


float4 VTFragDebug(VTV2f i) : SV_Target
{
	float2 uv = VTTransferUV(i.uv);
    float4 col = VTTex2D(uv) + VTGetMipColor(VTGetMipLevel(i.uv));
	return col;
}


#endif