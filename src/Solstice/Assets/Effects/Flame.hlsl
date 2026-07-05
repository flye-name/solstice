#include "common.h"

sampler2D Texture : register(s0);
sampler2D Noise : register(s1);

SCREEN_SIZE(screenSize)

float4 FlameShaderFragment(float4 pos : SV_POSITION, float2 coords : TEXCOORD0, float4 baseColor : COLOR0) : SV_TARGET
{
    float2 screenUV = pos.xy / screenSize;
    
    float4 noiseColor = tex2D(Noise, screenUV);
    float4 color = tex2D(Texture, coords);
    
    return color * baseColor;
}

BEGIN_TECHNIQUE(Technique1)  
    BEGIN_PASS(FlameShader)   
        PIXEL_SHADER(compile ps_3_0 FlameShaderFragment())   
    END_PASS
END_TECHNIQUE