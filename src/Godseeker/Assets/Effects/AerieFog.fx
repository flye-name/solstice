sampler img : register(s0);

float time;

float parallax;

float4 source;

float steps = 6;

float pixelSize = 2;

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

float2 normalize_with_pixelation(float2 coords, float pixel_size, float2 resolution)
{
    return floor(coords / pixel_size) / (resolution / pixel_size);
}

float4 Fog(float4 sampleColor : COLOR0, float2 uv : TEXCOORD0, float2 screenCoords : SV_POSITION) : COLOR0
{
    float2 coords = uv * source.wz;
    
    coords += source.xy;
    
    float2 bayerParallax = float2(floor(source.z * parallax / pixelSize) * pixelSize, 0);
    float2 bayeruv = frac((coords + bayerParallax) / pixelSize / 4) * 4;
    
    coords = normalize_with_pixelation(coords, pixelSize, source.wz);
    coords.y *= source.w / source.z;
    
    coords.x += parallax;
    
    coords.y = 1 - coords.y;

    float4 color = cloudNoise(coords);
    
    float4 posterized = color + (bayer[bayeruv.x][bayeruv.y]) / steps;
    posterized = floor(posterized.a * steps) / steps;
    
    color = lerp(color, posterized, 1 - pow(color.a, 2));
    
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