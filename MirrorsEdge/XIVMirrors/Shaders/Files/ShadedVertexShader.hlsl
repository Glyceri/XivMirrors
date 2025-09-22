struct VSInput
{
    float3 Position : POSITION;
    float2 TexCoord : TEXCOORD;
};

struct PSInput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
};

cbuffer TransformBuffer : register(b0)
{
    matrix ModelMatrix;
    matrix ViewMatrix;
    matrix ProjectionMatrix;
    matrix ViewProjMatrix;
    matrix InvViewMatrix;
    matrix InvProjectionMatrix;
    float  NearPlane;
    float  FarPlane;
    float2 PADDING;
};

PSInput VSMain(VSInput input)
{
    PSInput output;
    
    output.Position = mul(float4(input.Position, 1.0f), ViewProjMatrix);
    output.TexCoord = input.TexCoord;
    
    return output;
}
