// Minimal point light effect for MonoGame
float4x4 World;
float4x4 View;
float4x4 Projection;

float3 LightPosition; // World-space position of the point light
float3 LightColor;
float LightIntensity;
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
    float3 toLight = normalize(LightPosition - input.WorldPos);
    float NdotL = max(dot(normal, toLight), 0);

    float3 texColor = tex2D(TextureSampler, input.TexCoord).rgb;
    float3 baseColor = texColor * input.Color.rgb;

    float3 diffuse = baseColor * LightColor * NdotL * LightIntensity;
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