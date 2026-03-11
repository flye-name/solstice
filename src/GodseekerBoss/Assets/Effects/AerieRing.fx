sampler img : register(s0);

float time;

static const float PI = 3.14159265359;

float4 Rings(float2 coords : TEXCOORD0) : SV_TARGET
{
    coords -= float2(0, 1);
    
    coords.x *= 2;
    
    float r = length(coords);
    float a = atan2(coords.y, coords.x) / 2 / PI;
    
    float2 polar = float2(a, r) * 6;
    
    polar.y -= 5;
    
    polar.y = 1 - polar.y;
    
    float4 rings = tex2D(img, polar);
    
    return rings;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 Rings();
    }
}