// Grass effect - renders one triangle per grass blade using vertex shading to position blades
float4x4 World;
float4x4 View;
float4x4 Projection;
float3 CameraPosition;
float3 LightDirection = float3(0.3, -0.8, 0.5); // Sunlight direction
texture GrassTexture;

sampler GrassSampler = sampler_state
{
    Texture = <GrassTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

// VSInput matches GrassVertex: RootPosition (float3), Offset (float2), TexCoord (float2), Rotation (float)
struct VSInput
{
    float3 RootPosition : POSITION0;
    float2 Offset       : TEXCOORD0; // x = lateral offset, y = height
    float2 TexCoord     : TEXCOORD1; // texture coordinates
    float Rotation      : TEXCOORD2; // random rotation angle (radians)
};

struct PSInput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float Brightness : TEXCOORD1; // Lighting factor
};

PSInput VSMain(VSInput input)
{
    PSInput output;

    // Calculate direction from blade root to camera (billboarding)
    float3 toCameraDir = normalize(CameraPosition - input.RootPosition);

    // Create a right vector perpendicular to the camera direction and up vector
    float3 up = float3(0, 1, 0);
    float3 right = normalize(cross(up, toCameraDir));

    // Apply random rotation around Y axis to the right vector
    float cosRot = cos(input.Rotation);
    float sinRot = sin(input.Rotation);
    float3 rotatedRight = float3(
        right.x * cosRot - right.z * sinRot,
        right.y,
        right.x * sinRot + right.z * cosRot
    );

    // Apply billboarding with rotated right vector: orient the blade offset to face a rotated direction
    float3 pos = input.RootPosition;
    pos += rotatedRight * input.Offset.x;  // Lateral offset along the rotated right vector
    pos.y += input.Offset.y;               // Height offset remains vertical

    // Calculate blade normal based on the rotated orientation
    float3 bladeNormal = normalize(rotatedRight);

    // Calculate lighting based on normal and height with more contrast
    float normalDot = dot(bladeNormal, normalize(-LightDirection));
    float normalLighting = saturate(normalDot * 0.5 + 0.5); // Softer contrast

    float heightFactor = saturate(input.Offset.y / 0.3); // Darker at base, brighter at top
    float ambientOcclusion = lerp(0.7, 1.0, heightFactor); // Subtle ground shadow

    // Combine lighting factors
    output.Brightness = normalLighting * ambientOcclusion;

    float4 worldPos = mul(float4(pos, 1.0), World);
    output.Position = mul(mul(worldPos, View), Projection);
    output.TexCoord = input.TexCoord;

    return output;
}

float4 PSMain(PSInput input) : SV_Target
{
    float4 texColor = tex2D(GrassSampler, input.TexCoord);

    // Ensure brightness has a minimum value to prevent it being too dark
    float finalBrightness = max(input.Brightness, 0.4) + 0.3; // Min 0.4, add ambient 0.3

    // Apply lighting while preserving color
    float3 litColor = texColor.rgb * finalBrightness;

    return float4(litColor, texColor.a);
}

technique Grass
{
    pass P0
    {
        VertexShader = compile vs_3_0 VSMain();
        PixelShader  = compile ps_3_0 PSMain();
    }
}
