// Point light effect for MonoGame - Second pass additive lighting
#define MAX_LIGHTS 20

float4x4 World;
float4x4 View;
float4x4 Projection;

float3 LightPositions[MAX_LIGHTS];
float3 LightColors[MAX_LIGHTS];
float LightIntensities[MAX_LIGHTS];

int LightCount;

struct VSInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float3 Normal : NORMAL0;
    float2 TexCoord : TEXCOORD0;
};

struct PSInput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
    float3 WorldPos : TEXCOORD1;
    float3 Normal : TEXCOORD2;
};

PSInput VSMain(VSInput input)
{
    PSInput output;
    output.Position = mul(input.Position, World);
    output.WorldPos = output.Position.xyz;
    output.Position = mul(output.Position, View);
    output.Position = mul(output.Position, Projection);
    output.Color = input.Color;
    output.TexCoord = input.TexCoord;
    // Transform normal to world space (if needed)
    output.Normal = mul(float4(input.Normal, 0), World).xyz;
    return output;
}

float4 PSMain(PSInput input) : SV_Target
{
    float3 normal = normalize(input.Normal);
    float3 additionalLight = float3(0, 0, 0);

    for (int i = 0; i < LightCount; ++i)
    {
        float3 toLight = LightPositions[i] - input.WorldPos;
        float dist = length(toLight);
        float3 toLightDir = normalize(toLight);

        // Calculate lighting based on normal angle
        float NdotL = max(dot(normal, toLightDir), 0.0);

        // Strong attenuation - lights fall off very quickly with distance
        float attenuation = 1.0 / (1.0 + dist * dist);

        // Add light contribution
        additionalLight += LightColors[i] * NdotL * LightIntensities[i] * attenuation;
    }

    // Second pass - only return the additional light, not the base color
    // The base color was already rendered in the first pass (BasicEffect)
    return float4(additionalLight, 1);
}

technique PointLight
{
    pass P0
    {
        VertexShader = compile vs_3_0 VSMain();
        PixelShader = compile ps_3_0 PSMain();
    }
}