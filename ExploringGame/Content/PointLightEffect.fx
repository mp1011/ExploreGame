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

Texture2D Texture : register(t0);
sampler TextureSampler = sampler_state
{
    Texture = <Texture>;
};

PSInput VSMain(VSInput input)
{
    PSInput output;
    output.Position = mul(input.Position, World);
    output.WorldPos = input.Position.xyz;
    output.Position = mul(output.Position, View);
    output.Position = mul(output.Position, Projection);
    output.Color = input.Color;
    output.TexCoord = input.TexCoord;
    float4 worldNormal = mul(float4(input.Normal, 0), World);
    output.Normal = normalize(worldNormal.xyz);
    return output;
}

float4 PSMain(PSInput input) : SV_Target
{
    float4 baseColor = tex2D(TextureSampler, input.TexCoord) * input.Color;
    float3 normal = normalize(input.Normal);
    float3 totalLight = float3(0, 0, 0);
   
    for (int i = 0; i < MAX_LIGHTS; i++)
    {
        if (i >= LightCount)
            break;

        float3 toLight = LightPositions[i] - input.WorldPos;
        float distanceSquared = dot(toLight, toLight);
        float distance = sqrt(distanceSquared);
        float3 lightDirection = distance > 0.0001 ? toLight / distance : float3(0, 0, 0);

        float facing = saturate(dot(normal, lightDirection));
        float attenuation = exp(-2.5 * distance) * LightIntensities[i];
        
        float3 newLight = LightColors[i] * (facing * attenuation);
        totalLight += newLight * 1000;               
    }
    
    
    return float4(saturate(baseColor.rgb + totalLight), baseColor.a);
}

technique PointLight
{
    pass P0
    {
        VertexShader = compile vs_3_0 VSMain();
        PixelShader = compile ps_3_0 PSMain();
    }
}
