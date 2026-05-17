#include "common.h"

sampler2D Texture : register(s0);
sampler2D NoiseTex : register(s1);

float Time;
float Zoom;

float2 ScreenOffset;
float2 ScreenSize;

float4 CloudOverlayFragment(float2 textureUv : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float4 input = tex2D(Texture, textureUv);
    
    if (!any(input.a))
        return float4(0, 0, 0, 0);
    
    float2 worldUV = (textureUv * ScreenSize + ScreenOffset) / Zoom;
    
    float4 gabagool = pow(tex2D(NoiseTex, worldUV + float2(-Time, Time)), 2);
    
    input += gabagool;
    input = lerp(float4(0.7, 0.5, 0.3, 1), float4(0.97, 0.9, 0.8, 1), input);
    
    return input * color;
}

BEGIN_TECHNIQUE(Technique1) 
    BEGIN_PASS(AerieCloudOverlayShader)  
        PIXEL_SHADER(compile ps_2_0 CloudOverlayFragment())   
    END_PASS
END_TECHNIQUE