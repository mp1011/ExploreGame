// Glass effect for MonoGame
// Simulates transparent glass with slight blur and tint

float4x4 World;
float4x4 View;
float4x4 Projection;

texture Texture;
sampler TextureSampler = sampler_state
{
    Texture = <Texture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

// Glass tint - very subtle, mostly transparent
float4 GlassTint = float4(0.95, 0.97, 1.0, 0.15);
// Blur amount - size of blur kernel in texture coordinates
float BlurSize = 0.003;

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
    float3 Normal : TEXCOORD1;
};

PSInput VSMain(VSInput input)
{
    PSInput output;
    output.Position = mul(input.Position, World);
    output.Position = mul(output.Position, View);
    output.Position = mul(output.Position, Projection);

    output.Color = input.Color;
    output.TexCoord = input.TexCoord;
    output.Normal = normalize(mul(float4(input.Normal, 0), World).xyz);
    return output;
}

float4 PSMain(PSInput input) : SV_Target
{
    // Apply a simple 3x3 box blur
    float4 blurredColor = float4(0, 0, 0, 0);
    float totalWeight = 0;

    // Sample 3x3 grid around current pixel
    for (int x = -1; x <= 1; x++)
    {
        for (int y = -1; y <= 1; y++)
        {
            float2 offset = float2(x, y) * BlurSize;
            float weight = 1.0; // Equal weight for all samples (box blur)
            blurredColor += tex2D(TextureSampler, input.TexCoord + offset) * weight;
            totalWeight += weight;
        }
    }

    blurredColor /= totalWeight;

    // Apply very light glass tint
    float4 finalColor = blurredColor * input.Color;
    finalColor.rgb = lerp(finalColor.rgb, GlassTint.rgb, GlassTint.a);

    // Make edges slightly more visible using fresnel-like effect
    float3 viewDir = float3(0, 0, -1); // Simplified view direction
    float fresnel = 1.0 - abs(dot(normalize(input.Normal), viewDir));
    fresnel = pow(fresnel, 2.0);

    // Edges are slightly more opaque but still very transparent
    finalColor.a = lerp(0.15, 0.4, fresnel);

    return finalColor;
}

technique GlassEffect
{
    pass P0
    {
        VertexShader = compile vs_3_0 VSMain();
        PixelShader = compile ps_3_0 PSMain();
    }
}

