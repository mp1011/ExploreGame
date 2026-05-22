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

float DistanceBasedLight(PSInput input)
{    
    float distRatio = 0.0f;
    float bestRatio = 0.0;
    
    for (int i = 0; i < MAX_LIGHTS; i++)
    {
        if (i >= LightCount) 
            break;

        float3 toLight = LightPositions[i] - input.WorldPos;
        float distanceSquared = dot(toLight, toLight);
        float distance = sqrt(distanceSquared);

        // ideal for indoor light
        float mod = LightIntensities[i];
        float d1 = 0.5 * mod;
        float d2 = 2.0 * mod;
        float d3 = 4.0 * mod;
        float d4 = 12.0 * mod;
        float d5 = 20.0 * mod;
        
        float l1 = 2.0 * mod;
        float l2 = 0.8 * mod;
        float l3 = 0.4 * mod;
        float l4 = 0.2 * mod;
        float l5 = 0.1 * mod;
                
        // Distance-based with falloff and color scaling
        if (distance < d1)
        {
            float t = saturate(distance / d1);
            distRatio = lerp(l1, l2, t);
        }
        else if (distance < d2)
        {
            float t = saturate((distance - d1) / (d2 - d1));
            distRatio = lerp(l2, l3, t);
        }
        else if (distance < d3)
        {
            float t = saturate((distance - d2) / (d3 - d2));
            distRatio = lerp(l3, l4,
            t);
        }
        else if (distance < d4)
        {
            float t = saturate((distance - d3) / (d4 - d3));
            distRatio = lerp(l4, l5, t);
        }
        else if (distance < d5)
        {
            float t = saturate((distance - d4) / (d5 - d4));
            distRatio = lerp(l5, 0.0f, t);
        }
        else
        {
            distRatio = 0;
        }
               
        if (distRatio > bestRatio)
            bestRatio = distRatio;
    }
    
    return bestRatio;
}

float NormalBasedLight(PSInput input)
{    
    float normRatio = 0.0f;
    float bestNormRatio = 0.0;
    float3 normal = normalize(input.Normal);
    
    for (int i = 0; i < MAX_LIGHTS; i++)
    {
        if (i >= LightCount) 
            break;

        float3 lightVector = LightPositions[i] - input.WorldPos;

        float distance = length(lightVector);

        float3 lightDir = lightVector / distance;

        float NdotL = saturate(dot(normal, lightDir));

        float attenuation = saturate(1.0f - (distance / 8.0f));

        normRatio = NdotL * attenuation * LightIntensities[i];
        
        if (normRatio > bestNormRatio)
            bestNormRatio = normRatio;
    }
    
    return bestNormRatio;
}

float4 PSMain(PSInput input) : SV_Target
{
    float3 normal = normalize(input.Normal);
    float4 sampledColor = tex2D(TextureSampler, input.TexCoord) * input.Color;
    
    float distRatio = DistanceBasedLight(input);
    float normRatio = NormalBasedLight(input);
    float lightRatio = max(distRatio, normRatio);

    return float4(sampledColor.rgb * lightRatio, 1.0f);
}

technique PointLight
{
    pass P0
    {
        VertexShader = compile vs_3_0 VSMain();
        PixelShader = compile ps_3_0 PSMain();
    }
}
