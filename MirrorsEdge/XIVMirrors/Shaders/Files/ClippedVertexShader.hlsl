struct VSIn
{
    float3 pos : POSITION;
    float2 uv  : TEXCOORD0;
};

struct MirrorShaderOutput
{
    float4 pos : SV_POSITION;
    float2 uv : TEXCOORD0;
};

struct ScaledResolution
{
    int AssignedWidth;
    int AssignedHeight;
    int ActualWidth;
    int ActualHeight;
};

cbuffer ResolutionBuffer : register(b0)
{
    ScaledResolution scaledResolution;
};

MirrorShaderOutput VSMain(VSIn input)
{
    MirrorShaderOutput output;

    output.pos = float4(input.pos, 1.0);
    output.uv = input.uv;

    return output;
}
