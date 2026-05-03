#include "common.h"

sampler2D Texture : register(s0);

#define STEPS (6)
#define PIXEL_SIZE (2)

#define BAYER (float4x4(0, 8, 2, 10, 12, 4, 14, 6, 3, 11, 1, 9, 15, 7, 13, 5) / 16)

float Time;
float Parallax;
float4 Source;

float fbm(float2 p)
{
    float f = 0;
    float gat = 0;
    
    for (float octave = 0.; octave < 3.; ++octave)
    {
        float la = pow(2, octave);
        float ga = pow(0.5, octave + 1);
        
        float2 off = float2((tan(octave / 5) * Time + Time) * 0.07, -Time * 0.1);
        
        f += ga * tex2D(Texture, la * p + off);
        gat += ga;
    }
    
    f = f / gat;
    
    return f;
}

float cloud_noise(float2 uv)
{
    float clouds = 0.8 - uv.y + (-pow(uv.y, 7)) * 2.5;
    
    uv.y *= 3;
    
    uv *= 0.5;
    
    clouds += fbm(uv);
    
    return saturate(1 - pow(1 - clouds, 3));
}

float2 normalize_with_pixelation(float2 coords, float pixel_size, float2 resolution)
{
    return floor(coords / pixel_size) / (resolution / pixel_size);
}

float4 FogShaderFragment(float4 baseColor : COLOR0, float2 textureUv : TEXCOORD0) : COLOR0
{
    float2 coords = textureUv * Source.wz;
    
    coords += Source.xy;
    
    float2 bayerParallax = float2(floor(Source.z * Parallax / PIXEL_SIZE) * PIXEL_SIZE, 0);
    float2 bayeruv = frac((coords + bayerParallax) / PIXEL_SIZE / 4) * 4;
    
    coords = normalize_with_pixelation(coords, PIXEL_SIZE, Source.wz);
    coords.y *= Source.w / Source.z;
    
    coords.x += Parallax;
    
    coords.y = 1 - coords.y;

    float4 color = cloud_noise(coords);
    
    float4 posterized = color + (BAYER[bayeruv.x][bayeruv.y]) / STEPS;
    posterized = floor(posterized.a * STEPS) / STEPS;
    
    color = lerp(color, posterized, 1 - pow(color.a, 2));
    
    color *= baseColor;
    
    return color;
}

BEGIN_TECHNIQUE(Technique1) 
    BEGIN_PASS(FogShader) 
        PIXEL_SHADER(compile ps_3_0 FogShaderFragment())  
    END_PASS
END_TECHNIQUE