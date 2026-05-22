float4 PSMain(PSInput input) : SV_Target
{
    float3 normal = normalize(input.Normal);
    float4 sampledColor = tex2D(TextureSampler, input.TexCoord) * input.Color;
