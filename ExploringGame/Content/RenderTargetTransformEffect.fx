texture SceneTexture;
sampler2D SceneSampler = sampler_state
{
    Texture = <SceneTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

float Brightness = 1.0f;
float3 TintColor = float3(1.0f, 1.0f, 1.0f);
float2 ViewportSize;

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
    float2 clipSpace;
    clipSpace.x = (input.Position.x / ViewportSize.x) * 2.0f - 1.0f;
    clipSpace.y = 1.0f - (input.Position.y / ViewportSize.y) * 2.0f;
    output.Position = float4(clipSpace, 0.0f, 1.0f);
    output.Color = input.Color;
    output.TexCoord = input.TexCoord;
    return output;
}

float4 PSMain(PSInput input) : SV_Target
{
    float4 sceneColor = tex2D(SceneSampler, input.TexCoord) * input.Color;
    sceneColor.rgb *= TintColor;
    sceneColor.rgb *= Brightness;
    return sceneColor;
}

technique RenderTargetTransformEffect
{
    pass P0
    {
        VertexShader = compile vs_3_0 VSMain();
        PixelShader = compile ps_3_0 PSMain();
    }
}
