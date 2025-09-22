Texture2D    DiffuseTexture : register(t0);
SamplerState DiffuseSampler : register(s0);

struct PSInput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
};

float4 PSMain(PSInput input) : SV_TARGET
{
    float2 texCoord     = input.TexCoord;
    
    float4 outputColour = DiffuseTexture.Sample(DiffuseSampler, texCoord);

    return outputColour;
}
