#ifndef VIRTUAL_TEXTURE_SHADING_INCLUDED
#define VIRTUAL_TEXTURE_SHADING_INCLUDED

#include "VirtualTexture.cginc"
//#define SHOW_MIPLVEL 1

float4 VTFragUnlit(VTV2f i) : SV_Target
{
    
#if SHOW_MIPLVEL
    float2 uvInt = i.uv - frac(i.uv * _VTPageParam.x) * _VTPageParam.y;
    float mip = tex2D(_VTLookupTex, uvInt).b * 255;
    return float4(clamp(1 - mip * 0.1 , 0, 1), 0, 0, 1);
#endif
    
	float2 uv = VTTransferUV(i.uv);
    float4 col = VTTex2D(uv);
	return col;
}

#endif