// Minimal point light effect for MonoGame
#define MAX_LIGHTS 20

float4x4 World;
float4x4 View;
float4x4 Projection;

float3 LightPositions[MAX_LIGHTS];
float3 LightColors[MAX_LIGHTS];
float LightIntensities[MAX_LIGHTS];
float3 LightRangeMin[MAX_LIGHTS];
float3 LightRangeMax[MAX_LIGHTS];

int LightCount;
float3 AmbientColor;

Texture2D Texture : register(t0);
sampler TextureSampler = sampler_state
{
    Texture = <Texture>;
};

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
    float3 texColor = tex2D(TextureSampler, input.TexCoord).rgb;
    float3 baseColor = texColor * input.Color.rgb;
    float3 diffuse = float3(0, 0, 0);
    float attenuation = 1.0;
    
    for (int i = 0; i < LightCount; ++i)
    {
        float3 rangeMin = LightRangeMin[i];
        float3 rangeMax = LightRangeMax[i];
        
        float3 wp = input.WorldPos;
        if (wp.x < rangeMin.x || wp.x > rangeMax.x ||
           wp.y < rangeMin.y || wp.y > rangeMax.y ||
           wp.z < rangeMin.z || wp.z > rangeMax.z)
            continue;
                           
        float3 toLight = LightPositions[i] - input.WorldPos;
        float dist = length(toLight);
        float3 toLightDir = normalize(toLight);
        float NdotL = dot(normal, toLightDir);
        float lightFactor;
        if (NdotL > 0.1)
        {
            lightFactor = NdotL;
            attenuation = (1.0 / (1.0 + dist * dist)) * 10.0;
        }
        else
        {
            lightFactor = 1.0 / (1.0 + dist * dist * 0.1);
            attenuation = 1.0;
        }
        diffuse += baseColor * LightColors[i] * lightFactor * LightIntensities[i] * attenuation;
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