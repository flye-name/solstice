#include "common.h"

sampler2D Texture : register(s0);

float Time;

float4 CloudOverlayFragment(float2 textureUv : TEXCOORD0) : COLOR0
{
    if (!any(tex2D(Texture, textureUv)))
    {
        return 0;
    }
    
    // red for debugging
    return float4(1, 0, 0, 1);
}

BEGIN_TECHNIQUE(Technique1) 
    BEGIN_PASS(AerieCloudOverlayShader)  
        PIXEL_SHADER(compile ps_2_0 CloudOverlayFragment())   
    END_PASS
END_TECHNIQUE