float4x4 World;
float4x4 View;
float4x4 Projection;

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
    float3 WorldPos : TEXCOORD2;
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
    output.WorldPos = input.Position.xyz;
    return output;
}

float4 PSMain(PSInput input) : SV_Target
{
    return float4(0, 0, 0, .6);
}

technique StaticShadowEffect
{
    pass P0
    {
        VertexShader = compile vs_3_0 VSMain();
        PixelShader = compile ps_3_0 PSMain();
    }
}

