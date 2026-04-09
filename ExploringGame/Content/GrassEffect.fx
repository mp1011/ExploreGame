// Grass effect - renders one triangle per grass blade using vertex shading to position blades
float4x4 World;
float4x4 View;
float4x4 Projection;

// VSInput matches GrassVertex: RootPosition (float3), Offset (float2), Color (float4)
struct VSInput
{
    float3 RootPosition : POSITION0;
    float2 Offset       : TEXCOORD0; // x = lateral offset, y = height
    float4 Color        : COLOR0;
};

struct PSInput
{
    float4 Position : SV_POSITION;
    float4 Color    : COLOR0;
};

PSInput VSMain(VSInput input)
{
    PSInput output;

    // Vertex shader positions each blade vertex from the shared root position.
    // Base vertices are spread laterally along the X axis; the apex is lifted in Y.
    float3 pos = input.RootPosition;
    pos.x += input.Offset.x;
    pos.y += input.Offset.y;

    float4 worldPos = mul(float4(pos, 1.0), World);
    output.Position = mul(mul(worldPos, View), Projection);
    output.Color = input.Color;

    return output;
}

float4 PSMain(PSInput input) : SV_Target
{
    return input.Color;
}

technique Grass
{
    pass P0
    {
        VertexShader = compile vs_3_0 VSMain();
        PixelShader  = compile ps_3_0 PSMain();
    }
}
