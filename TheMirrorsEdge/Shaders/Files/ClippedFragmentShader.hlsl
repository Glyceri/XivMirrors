struct MirrorShaderOutput
{
    float4 pos      : SV_POSITION;
    float2 uv       : TEXCOORD0;
};

Texture2D    flatTexture    : register(t0);
SamplerState textureSampler : register(s0);

float4 PSMain(MirrorShaderOutput clippedShaderOutput) : SV_Target
{
    float2 textureCoordinate = clippedShaderOutput.uv;
    
    float4 textureColour = flatTexture.Sample(textureSampler, textureCoordinate);
    
    return textureColour;
}
