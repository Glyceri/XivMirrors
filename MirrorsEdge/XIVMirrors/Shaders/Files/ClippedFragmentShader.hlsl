struct ClippedShaderOutput
{
    float4 position : SV_POSITION;
    float2 texcoord : TEXCOORD;
};

Texture2D    flatTexture    : register(t0);
SamplerState textureSampler : register(s0);

float4 PSMain(ClippedShaderOutput clippedShaderOutput) : SV_Target
{
    float2 textureCoordinate = clippedShaderOutput.texcoord;
    
    float4 textureColour = flatTexture.Sample(textureSampler, textureCoordinate);
        
    textureColour.a = 1.0; // YUPP c:

    return textureColour;
}
