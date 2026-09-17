Texture2D    inputTexture : register(t0);
SamplerState samplerState : register(s0);

struct PS_IN
{
    float4 pos : SV_POSITION;
    float2 uv  : TEXCOORD0;
};

float4 PSMain(PS_IN input) : SV_TARGET
{
    float2 textureCoordinate = input.uv;
    float4 textureColour     = inputTexture.Sample(samplerState, textureCoordinate);
    
    textureColour.a          = 1.0;

    return textureColour;
}
