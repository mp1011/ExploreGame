// Point light effect for MonoGame - Second pass additive lighting
#define MAX_LIGHTS 20

float4x4 World;
float4x4 View;
float4x4 Projection;

float DAtten;
float DMod;
float LAtten;
float LMod;


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

        // Distance-based lighting (no normal check - purely on proximity)
        float distanceAttenuation = exp(DAtten * distance) * LightIntensities[i] * DMod;
        float newDistanceLight = baseColor.rgb * distanceAttenuation;

        // Normal-based lighting (directional shading)
        float facing = saturate(dot(normal, lightDirection));
        float normalAttenuation = exp(LAtten * distance) * LightIntensities[i] * LMod;
        float newNormalLight = LightColors[i].rgb * (facing * normalAttenuation);
        
         totalLight += max(newDistanceLight, newNormalLight);
    }
    
    float NORMAL_LIGHT_THRESHOLD = 0.5;
    float currentLuminance = dot(totalLight, float3(0.2126, 0.7152, 0.0722));
    
    // Blend factor: 1 when lit normally (shows baseColor), 0 when dark (shows black)
    float blendFactor = saturate(saturate(currentLuminance / NORMAL_LIGHT_THRESHOLD) - 0.5);
    
    return float4(saturate(lerp(float3(0, 0, 0), baseColor.rgb + totalLight, blendFactor)), baseColor.a);
}

technique PointLight
{
    pass P0
    {
        VertexShader = compile vs_3_0 VSMain();
        PixelShader = compile ps_3_0 PSMain();
    }
}
