Texture2D    inputTexture : register(t0);
SamplerState samplerState : register(s0);

struct PS_IN
{
    float4 pos : SV_POSITION;
    float2 uv  : TEXCOORD0;
};

float4 PSMain(PS_IN input) : SV_TARGET
{
    float4 colour = inputTexture.Sample(samplerState, input.uv);

    colour.a = 1 - colour.a;
    
    return colour;
}
