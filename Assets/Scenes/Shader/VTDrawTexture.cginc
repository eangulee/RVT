
sampler2D _MainTex;
sampler2D _Diffuse1;
sampler2D _Diffuse2;
sampler2D _Diffuse3;
sampler2D _Diffuse4;
sampler2D _Normal1;
sampler2D _Normal2;
sampler2D _Normal3;
sampler2D _Normal4;
sampler2D _Blend;
float4x4 _ImageMVP;
float4 _TileOffset1;
float4 _TileOffset2;
float4 _TileOffset3;
float4 _TileOffset4;
float4 _BlendTile;

struct PixelOutput
{
    float4 col0 : COLOR0;
    float4 col1 : COLOR1;
};

struct v2f_drawTex
{
    float4 pos : SV_POSITION;
    float2 uv : TEXCOORD0;
};

v2f_drawTex vert(appdata_img v)
{
    v2f_drawTex o;
    o.pos = mul(_ImageMVP, v.vertex);
    o.uv = v.texcoord;

    return o;
}

PixelOutput frag(v2f_drawTex i) : SV_Target
{
    float4 blend = tex2D(_Blend, i.uv * _BlendTile.xy + _BlendTile.zw);
    
#ifdef TERRAIN_SPLAT_ADDPASS
    clip(blend.x + blend.y + blend.z + blend.w <= 0.005h ? -1.0h : 1.0h);
#endif
    
    float2 transUv = i.uv * _TileOffset1.xy + _TileOffset1.zw;
    float4 Diffuse1 = tex2D(_Diffuse1, transUv);
    float4 Normal1 = tex2D(_Normal1, transUv);
    transUv = i.uv * _TileOffset2.xy + _TileOffset2.zw;
    float4 Diffuse2 = tex2D(_Diffuse2, transUv);
    float4 Normal2 = tex2D(_Normal2, transUv);
    transUv = i.uv * _TileOffset3.xy + _TileOffset3.zw;
    float4 Diffuse3 = tex2D(_Diffuse3, transUv);
    float4 Normal3 = tex2D(_Normal3, transUv);
    transUv = i.uv * _TileOffset4.xy + _TileOffset4.zw;
    float4 Diffuse4 = tex2D(_Diffuse4, transUv);
    float4 Normal4 = tex2D(_Normal4, transUv);

    PixelOutput o;
    o.col0 = blend.r * Diffuse1 + blend.g * Diffuse2 + blend.b * Diffuse3 + blend.a * Diffuse4;
    o.col1 = blend.r * Normal1 + blend.g * Normal2 + blend.b * Normal3 + blend.a * Normal4;
    return o;
}