sampler img : register(s0);

float time;

float parallax;

float fbm(float2 p)
{
    float f = 0;
    float gat = 0;
    
    for (float octave = 0.; octave < 3.; ++octave)
    {
        float la = pow(2, octave);
        float ga = pow(0.5, octave + 1);
        
        float2 off = float2((tan(octave / 5) * time + time) * 0.1, -time * 0.02);
        
        f += ga * tex2D(img, la * p + off);
        gat += ga;
    }
    
    f = f / gat;
    
    return f;
}

float cloudNoise(float2 uv)
{
    float clouds = 2 - uv.y + (-pow(uv.y, 4)) * 4;
    
    uv.y *= 3;
    
    uv *= .5;
    
    clouds += fbm(uv);
    
    return saturate(1 - pow(1 - clouds, 3));
}

float4 Fog(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    coords.x += parallax;
    
    coords.y = 1 - coords.y;

    float4 color = cloudNoise(coords);
    
    color *= sampleColor;
    
    return color * color.a;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 Fog();
    }
}