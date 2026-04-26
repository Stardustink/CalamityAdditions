sampler uImage0 : register(s0); // The contents of the screen.
sampler uImage1 : register(s1); // Noise texture.
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);

float3 uColor;
float3 uSecondaryColor;

float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float2 uDirection;

float uOpacity;
float uTime;
float uIntensity;
float uProgress;

float2 uImageSize1;
float2 uImageSize2;
float2 uImageSize3;
float2 uImageOffset;

float uSaturation;
float4 uSourceRect;
float2 uZoom;

float4 FilterMyShader(float2 coords : TEXCOORD0) : COLOR0
{
    // Moving noise, used only for screen distortion.
    float2 noiseUV = coords * 2.0f + float2(uTime * 0.02f, 0.0f);

    float noise = tex2D(uImage1, noiseUV).r;
    float signedNoise = noise * 2.0f - 1.0f;

    // Stronger distortion toward the bottom of the screen.
    float bottomNoiseMask = smoothstep(0.55f, 1.0f, coords.y);
    float maskedNoise = signedNoise * bottomNoiseMask;

    // Distort the sampled screen coordinates.
    float displacementStrength = 0.01f;
    float2 displacedCoords = coords + float2(maskedNoise * displacementStrength, maskedNoise * displacementStrength);

    float4 screenColor = tex2D(uImage0, displacedCoords);

    // Clean vertical gradient. No noise distortion.
    float gradientInterpolant = saturate(coords.y);
    float3 gradientColor = lerp(uColor, uSecondaryColor, gradientInterpolant);

    float strength = uOpacity * uIntensity;

    float3 finalColor = lerp(
        screenColor.rgb,
        screenColor.rgb * gradientColor,
        strength
    );

    return float4(finalColor, screenColor.a);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 FilterMyShader();
    }
}