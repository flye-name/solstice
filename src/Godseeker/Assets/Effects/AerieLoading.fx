float uTime;
float4 uColor = float4(0.9, 0.9, 1, 1);
float uProgress;
bool uLoaded;
float uCloudIntensity = 8;
float uLineSize = 0.02;
float uLineIntensity = 0.1;
float uTopIntensity = 0.1;
float uColorAmount = 32;

float random(float2 uv) {
	return frac(sin(dot(uv.xy, float2(12.9898, 78.233))) * 43758.545);
}

// https://www.shadertoy.com/view/4dS3Wd
float rnoise(float2 uv) {
    float2 i = floor(uv);
    float2 f = frac(uv);

	float4 corners = float4(random(i), random(i + float2(1.0, 0.0)), random(i + float2(0.0, 1.0)), random(i + float2(1.0, 1.0)));

    float2 u = pow(f, 3) * (3.0 - 2.0 * f);
    return lerp(corners.x, corners.y, u.x) + (corners.z - corners.x) * u.y * (1.0 - u.x) + (corners.w - corners.y) * u.x * u.y;
}

// https://thebookofshaders.com/13/
float fbm(float2 uv) {
	float result = 0.0;
	float amplitude = 0.5;
    
    float2 shift = float2(33, 33);
    
    float2x2 rot = float2x2(cos(0.5), sin(0.5), -sin(0.5), cos(0.50));

	for (int i = 0; i < 15; i++) {
		result += amplitude * rnoise(uv);
		uv = mul(uv * 2, rot) + shift;
		amplitude *= 0.5;
	}
	return result;
}

float4 Main(float2 uv : TEXCOORD0) : SV_TARGET
{
    float4 cloud = fbm(2.5 * uv + float2(0, uTime * 10))
    * fbm(2 * uv + float2(uTime * 0.2, uTime * 5))
    * fbm(2 * uv + float2(uTime * -0.1, uTime * 6)) * uCloudIntensity;

    float4 lines = random(uLineSize * uv.x + uTime) * (uv.y * 0.5 + 1) * uLineIntensity;

    float4 c = cloud + uv.y * uTopIntensity + lines;

    float4 color = floor((0.2 + c * 0.7) * uColorAmount) / uColorAmount;

    float4 final = color;

    float alpha = lerp(lerp(0, uv.y, uProgress), lerp(uv.y, 1, uProgress), uProgress);
    if (uLoaded)
        alpha = lerp(lerp(0, 1 - uv.y, uProgress), lerp(1 - uv.y, 1, uProgress), uProgress);

    return final * alpha * uColor;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 Main();
    }
}
