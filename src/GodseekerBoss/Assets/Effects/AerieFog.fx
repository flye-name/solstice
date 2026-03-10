sampler img : register(s0);

float time;

float parallax;

float4 source;

float steps = 2;

const float4x4 bayer =
    float4x4(0, 8, 2, 10,
             12, 4, 14, 6,
             3, 11, 1, 9,
             15, 7, 13, 5) / 16;

float fbm(float2 p)
{
    float f = 0;
    float gat = 0;
    
    for (float octave = 0.; octave < 3.; ++octave)
    {
        float la = pow(2, octave);
        float ga = pow(0.5, octave + 1);
        
        float2 off = float2((tan(octave / 5) * time + time) * 0.07, -time * 0.1);
        
        f += ga * tex2D(img, la * p + off);
        gat += ga;
    }
    
    f = f / gat;
    
    return f;
}

float cloudNoise(float2 uv)
{
    float clouds = .8 - uv.y + (-pow(uv.y, 7)) * 2.5;
    
    uv.y *= 3;
    
    uv *= .5;
    
    clouds += fbm(uv);
    
    return saturate(1 - pow(1 - clouds, 3));
}

float4 Fog(float4 sampleColor : COLOR0, float2 uv : TEXCOORD0, float2 screenCoords : SV_POSITION) : COLOR0
{
    float2 bayeruv = frac((screenCoords.xy + float2(floor(source.z * parallax / 2) * 2, 0)) / 4) * 4;
    
    float2 coords = (screenCoords - source.xy) / max(source.z, source.w);
    
    coords.x += parallax;
    
    coords.y = 1 - coords.y;

    float4 color = cloudNoise(coords);
    
    color += (bayer[bayeruv.x][bayeruv.y]) / steps;
    color = floor(color.a * steps) / steps;
    
    color *= sampleColor;
    
    return color;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 Fog();
    }
}