#include "common.h"

sampler2D Texture : register(s0);

float4 bottomColor;
float4 middleColor;
float4 topColor;

float4 SkyShaderFragment(float4 baseColor : COLOR0, float2 uv : TEXCOORD0) : COLOR0 
{
    float4 bottomToMiddle = lerp(bottomColor, middleColor, 1 - uv.y);
    float4 middleToTop = lerp(middleColor, topColor, 1 - uv.y);
    
    return lerp(bottomToMiddle, middleToTop, 1 - uv.y);
}


BEGIN_TECHNIQUE(Technique1) 
    BEGIN_PASS(SkyShader) 
        PIXEL_SHADER(compile ps_3_0 SkyShaderFragment())  
    END_PASS
END_TECHNIQUE