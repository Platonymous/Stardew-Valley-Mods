#if OPENGL
#define SV_POSITION POSITION
#define VS_SHADERMODEL vs_3_0
#define PS_SHADERMODEL ps_3_0
#else
#define VS_SHADERMODEL vs_4_0_level_9_1
#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

float satr = 0;
float satg = 0;
float satb = 0;
float4 tint = float4(0, 0, 0, 0);

sampler2D TextureSampler : register(s0)
{
    Texture = (Texture);
};

float4 MainPS(float4 position : SV_Position, float4 color : COLOR0, float2 TextureCoordinates : TEXCOORD0) : COLOR0
{
    float4 col = tex2D(TextureSampler, TextureCoordinates) * color;
	
    if (col.a == 0)
    {
        return col;
    }
	
    float l = 0.2125 * col.r + 0.7154 * col.g + 0.0721 * col.b;
	
    col.r = max(0, min(1, col.r + satr * (l - col.r)));
    col.g = max(0, min(1, col.g + satg * (l - col.g)));
    col.b = max(0, min(1, col.b + satb * (l - col.b)));
        
    if (tint.a != 0)
    {
        col = col * tint;
    }
	    
    return col;
};

technique Visualize
{
    pass P0
    {
        PixelShader = compile PS_SHADERMODELMainPS();
    }
};