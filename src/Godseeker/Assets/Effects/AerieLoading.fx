const float4x4 bayer =
    float4x4(0, 8, 2, 10,
             12, 4, 14, 6,
             3, 11, 1, 9,
             15, 7, 13, 5) / 16;


float uTime;
float2 uResolution;
float4 uColor = float4(0.9, 0.9, 1, 1);
float uProgress;
bool uLoaded;
float uCloudIntensity = 8;
float uLineIntensity = 0.1;
float uTopIntensity = 0.1;

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

	for (int i = 0; i < 15; i++) {
		result += amplitude * rnoise(uv);
		uv *= 2;
		amplitude *= 0.5;
	}
	return result;
}

float4 Main(float2 uv : TEXCOORD0) : SV_TARGET
{
    float2 buv = frac(uv * uResolution / 8) * 4;

    float4 cloud = fbm(2.5 * uv + float2(0, uTime * 10))
    * fbm(2 * uv + float2(uTime * 0.2, uTime * 5))
    * fbm(2 * uv + float2(uTime * -0.1, uTime * 6)) * uCloudIntensity;

    float4 lines = random(0.02 * uv.x + uTime) * (uv.y * 0.5 + 1) * uLineIntensity;

    float4 c = cloud + uv.y * uTopIntensity + lines;

    float4 color = floor((0.2 + c * 0.7) * 32) / 32;
    float4 color2 = color + bayer[buv.x][buv.y] * 0.5;
    color2 = floor(color2.a * 2) / 2;


    float4 final = lerp(color, color2, 1 - color.a);

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
