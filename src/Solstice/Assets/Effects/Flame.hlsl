#include "common.h"

sampler2D Texture : register(s0);
sampler2D Noise : register(s1);

SCREEN_SIZE(uScreenSize)
SCREEN_POSITION(uScreenPos)
GLOBAL_TIME(uTime)

float2 uDirection;
float uNoiseStrength;
int uColorQuantity = 32;
int uNoiseColorQuantity = 32;

float4 FlameShaderFragment(float4 pos : SV_POSITION, float2 uv : TEXCOORD0, float4 baseColor : COLOR0) : SV_TARGET
{
    float2 noiseUV = 2 * (pos.xy / uScreenSize) + uScreenPos + uDirection * uTime;
    
    float4 noiseColor = floor(tex2D(Noise, noiseUV) * uNoiseColorQuantity) / uNoiseColorQuantity;
    float4 mask = tex2D(Texture, uv);
    float4 color = tex2D(Texture, uv + uDirection * noiseColor.r * mask.r * uNoiseStrength);
   
    float maxChannel = max(color.r, max(color.g, color.b));
    float4 properColor = float4(maxChannel, maxChannel, maxChannel, maxChannel);
    
    float4 final = mul(properColor, noiseColor);
    
    return floor(final * baseColor * uColorQuantity) / uColorQuantity;
}

BEGIN_TECHNIQUE(Technique1)  
    BEGIN_PASS(FlameShader)   
        PIXEL_SHADER(compile ps_3_0 FlameShaderFragment())   
    END_PASS
END_TECHNIQUE