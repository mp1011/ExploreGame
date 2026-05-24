// Grass effect - renders one triangle per grass blade using vertex shading to position blades
float4x4 World;
float4x4 View;
float4x4 Projection;
float3 CameraPosition;

float3 LightPosition1;
float3 LightIntensity1;
float3 LightPosition2;
float3 LightIntensity2;

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

// VSInput matches GrassVertex: RootPosition (float3), Offset (float2), TexCoord (float2), Rotation (float), Color (float4)
struct VSInput
{
    float3 RootPosition : POSITION0;
    float2 Offset : TEXCOORD0; // x = lateral offset, y = height
    float2 TexCoord : TEXCOORD1; // texture coordinates
    float Rotation : TEXCOORD2; // random rotation angle (radians)
    float4 Color : COLOR0; // vertex color
};

struct PSInput
{
    float4 Position : SV_POSITION;
    float3 WorldPos : TEXCOORD1;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;        // Vertex color
    float3 Normal : TEXCOORD2;
};

float NormalBasedLight(PSInput input)
{
    float normRatio1 = 0.0f;
    float normRatio2 = 0.0f;
    
    float3 normal = normalize(input.Normal);
    
    float3 lightVector1 = LightPosition1 - input.WorldPos;
    float distance1 = length(lightVector1);
    float3 lightDir1 = lightVector1 / distance1;
    float NdotL1 = saturate(dot(normal, lightDir1));
    float attenuation1 = saturate(1.0f - (distance1 / 32.0f));
    normRatio1 = NdotL1 * attenuation1 * LightIntensity1;
    
    float3 lightVector2 = LightPosition2 - input.WorldPos;
    float distance2 = length(lightVector2);    
    float3 lightDir2 = lightVector2 / distance2;
    float NdotL2 = saturate(dot(normal, lightDir2));
    float attenuation2 = saturate(1.0f - (distance2 / 32.0f));
    normRatio2 = NdotL2 * attenuation2 * LightIntensity2;
    
    return max(normRatio1, normRatio2);
}

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

    // Calculate blade normal: perpendicular to the blade surface (cross product of blade width and height)
    float3 bladeNormal = normalize(cross(rotatedRight, up));
   
    float heightFactor = saturate(input.Offset.y / 0.3); // Darker at base, brighter at top
    float ambientOcclusion = lerp(0.7, 1.0, heightFactor); // Subtle ground shadow
    
    float4 worldPos = mul(float4(pos, 1.0), World);
    output.Position = mul(mul(worldPos, View), Projection);
    output.TexCoord = input.TexCoord;
    output.Color = input.Color;
    output.Normal = bladeNormal;
    output.WorldPos = worldPos;
    
    return output;
}

float4 PSMain(PSInput input) : SV_Target
{
    float4 texColor = tex2D(GrassSampler, input.TexCoord);
    float brightness = NormalBasedLight(input);
    
    // desired formula, but input.Color is always black
   // return float4(texColor.rgb * input.Color.rgb * brightness, texColor.a);
      
   return float4(texColor.rgb * float3(50.0/255.0, 110.0/255.0, 40.0/255.0) * brightness, texColor.a);   
}

technique Grass
{
    pass P0
    {
        VertexShader = compile vs_3_0 VSMain();
        PixelShader  = compile ps_3_0 PSMain();
    }
}
