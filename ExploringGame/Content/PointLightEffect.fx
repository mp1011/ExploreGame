// Minimal point light effect for MonoGame
#define MAX_LIGHTS 4
float4x4 World;
float4x4 View;
float4x4 Projection;

float3 LightPositions[MAX_LIGHTS];
float3 LightColors[MAX_LIGHTS];
float LightIntensities[MAX_LIGHTS];
int LightCount;
float3 AmbientColor;

Texture2D Texture : register(t0);
sampler TextureSampler = sampler_state { Texture = <Texture>; };

struct VSInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

struct PSInput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
    float3 WorldPos : TEXCOORD1;
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
    return output;
}

float4 PSMain(PSInput input) : SV_Target
{
    float3 normal = float3(0, 1, 0); // Approximate normal (upwards)
    float3 texColor = tex2D(TextureSampler, input.TexCoord).rgb;
    float3 baseColor = texColor * input.Color.rgb;

    float3 diffuse = float3(0,0,0);
    for (int i = 0; i < LightCount; ++i)
    {
        float3 toLight = normalize(LightPositions[i] - input.WorldPos);
        float NdotL = max(dot(normal, toLight), 0);
        diffuse += baseColor * LightColors[i] * NdotL * LightIntensities[i];
    }
    float3 ambient = baseColor * AmbientColor;
    return float4(diffuse + ambient, 1);
}

technique PointLight
{
    pass P0
    {
        VertexShader = compile vs_3_0 VSMain();
        PixelShader = compile ps_3_0 PSMain();
    }
}