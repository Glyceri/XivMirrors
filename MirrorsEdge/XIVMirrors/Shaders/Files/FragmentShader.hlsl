Texture2D    inputTex : register(t0);
SamplerState samp     : register(s0);

struct PS_IN
{
    float4 pos : SV_POSITION;
    float2 uv  : TEXCOORD0;
};

float4 PSMain(PS_IN input) : SV_TARGET
{
    float4 color = inputTex.Sample(samp, input.uv);

    color.a = 1.0; // YUPP c:

    return color;
}
