#include "common.h"

sampler2D Texture : register(s0);

// https://github.com/patriciogonzalezvivo/lygia/blob/main/color/space/oklab2rgb.hlsl
// https://github.com/patriciogonzalezvivo/lygia/blob/main/color/space/rgb2oklab.hlsl

#define RGB2OKLAB_A (float3x3( 0.2104542553, 1.9779984951, 0.0259040371, 0.7936177850, -2.4285922050, 0.7827717662, -0.0040720468, 0.4505937099, -0.8086757660 ))
#define RGB2OKLAB_B (float3x3( 0.4122214708, 0.2119034982, 0.0883024619, 0.5363325363, 0.6806995451, 0.2817188376, 0.0514459929, 0.1073969566, 0.6299787005 ))

#define OKLAB2RGB_A (float3x3( 1.0,           1.0,           1.0, 0.3963377774, -0.1055613458, -0.0894841775, 0.2158037573, -0.0638541728, -1.2914855480 ))
#define OKLAB2RGB_B (float3x3( 4.0767416621, -1.2684380046, -0.0041960863, -3.3077115913, 2.6097574011, -0.7034186147, 0.2309699292, -0.3413193965, 1.7076147010 ))

float4 BottomColor;
float4 MiddleColor;
float4 TopColor;

float4 rgb2oklab(float4 rgb) {
    float3 lms = mul(RGB2OKLAB_B, rgb);
    return float4(mul(RGB2OKLAB_A, sign(lms) * pow(abs(lms), float3(0.3333333333333, 0.3333333333333, 0.3333333333333))), rgb.a);
}

float4 oklab2rgb(float4 oklab) {
    float3 lms = mul(OKLAB2RGB_A, oklab);
    return float4(mul(OKLAB2RGB_B, (lms * lms * lms)), oklab.a);
}

float4 oklabLerp(float4 a, float4 b, float t)
{
    return lerp(rgb2oklab(a), rgb2oklab(b), t);
} 

float4 SkyShaderFragment(float4 baseColor : COLOR0, float2 uv : TEXCOORD0) : COLOR0 
{
    float4 bottomToMiddle = oklabLerp(BottomColor, MiddleColor, 1 - uv.y);
    float4 middleToTop = oklabLerp(MiddleColor, TopColor, 1 - uv.y);
    
    return pow(oklab2rgb(lerp(bottomToMiddle, middleToTop, 1 - uv.y)), 1.5);
}


BEGIN_TECHNIQUE(Technique1) 
    BEGIN_PASS(SkyShader) 
        PIXEL_SHADER(compile ps_3_0 SkyShaderFragment())  
    END_PASS
END_TECHNIQUE