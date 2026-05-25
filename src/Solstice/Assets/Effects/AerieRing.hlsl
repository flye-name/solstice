#include "common.h"

sampler2D Texture : register(s0);

#define PI (3.14159265359)

float4 RingShaderFragment(float2 coords : TEXCOORD0, float4 baseColor : COLOR0) : SV_TARGET
{
    coords -= float2(0, 1);
    
    coords.x *= 2;
    
    float r = length(coords);
    float a = atan2(coords.y, coords.x) / 2 / PI;
    
    float2 polar = float2(a, r) * 6;
    
    polar.y -= 5;
    
    polar.y = 1 - polar.y;
    
    float4 rings = tex2D(Texture, polar);
    
    return rings * baseColor;
}

BEGIN_TECHNIQUE(Technique1)  
    BEGIN_PASS(RingShader)   
        PIXEL_SHADER(compile ps_3_0 RingShaderFragment())   
    END_PASS
END_TECHNIQUE