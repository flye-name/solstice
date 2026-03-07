struct PSInput
{
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
    float4 Position : SV_Position;
};
struct VSInput
{
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
    float4 Position : POSITION;
};
texture2D tex;
sampler2D texSampler = sampler_state
{
    Texture = <tex>;
    MinFilter = Linear;
    MagFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};
bool useActualCol = false;
matrix WorldViewProjection;
PSInput MainVS(VSInput input)
{
    PSInput output = (PSInput) 0;
    output.Color = input.Color;
    output.TexCoord = input.TexCoord;
    output.Position = mul(input.Position, WorldViewProjection);
    
    return output;
}

float4 MainPS(PSInput input) : COLOR0
{
    return input.Color;
}
float4 TexturePS(PSInput input) : COLOR0
{
    float4 c = tex2D(texSampler, input.TexCoord);
    float4 col = input.Color * max(c.r, max(c.g, c.b));
    if (useActualCol)
        col = c * max(c.r, max(c.g, c.b)) * input.Color.a;

    return col;
}

technique Technique1
{
    pass Texture
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader = compile ps_2_0 TexturePS();
    }
}