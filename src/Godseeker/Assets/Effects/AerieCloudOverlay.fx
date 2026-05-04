float uTime;

sampler uImage0 : register(s0);

float4 Main(float2 coords : TEXCOORD0) : COLOR0
{
    if (!any(tex2D(uImage0, coords)))
        return 0;
    
    // red for debugging
    return float4(1, 0, 0, 1);
}

Technique technique1
{
    pass AerieCloudOverlay
    {
        PixelShader = compile ps_2_0 Main();
    }
}