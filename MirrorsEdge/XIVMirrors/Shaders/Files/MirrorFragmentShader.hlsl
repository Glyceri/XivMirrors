struct MirrorShaderOutput
{
    float4 position : SV_POSITION;
    float2 texcoord : TEXCOORD;
};

Texture2D       flatTexture     : register(t0);
SamplerState    textureSampler  : register(s0);

float4 PSMain(MirrorShaderOutput mirrorShaderOutput) : SV_Target
{
    float2 textureCoordinate = mirrorShaderOutput.texcoord;
    
    float4 colour            = flatTexture.Sample(textureSampler, textureCoordinate);
    
    colour.a = 1.0; // YUPP c:
    
    colour += float4(1, 0, 0, 0);
    
    return colour;
}
