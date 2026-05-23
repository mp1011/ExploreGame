// Skybox effect - renders at far plane (depth = 1.0) to never obscure geometry
float4x4 World;
float4x4 View;
float4x4 Projection;

texture Texture;
sampler2D textureSampler = sampler_state
{
    Texture = <Texture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

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
};

PSInput VSMain(VSInput input)
{
    PSInput output;
    
    // Transform position to clip space
    output.Position = mul(input.Position, World);
    output.Position = mul(output.Position, View);
    output.Position = mul(output.Position, Projection);
    
    // Force depth to 1.0 (far plane) so skybox is always behind everything
    output.Position.z = output.Position.w;
    
    output.Color = float4(input.Color.rgb * 0.4, input.Color.a);
    output.TexCoord = input.TexCoord;
    
    return output;
}

float4 PSMain(PSInput input) : SV_Target
{
    float4 texColor = tex2D(textureSampler, input.TexCoord);
    return texColor * input.Color;
}

technique Skybox
{
    pass P0
    {
        VertexShader = compile vs_3_0 VSMain();
        PixelShader = compile ps_3_0 PSMain();
    }
}
