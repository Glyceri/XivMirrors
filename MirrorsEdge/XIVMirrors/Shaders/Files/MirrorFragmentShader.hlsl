struct MirrorShaderOutput
{
    float4 position : SV_POSITION;
    float2 texcoord : TEXCOORD;
};

Texture2D       flatTexture     : register(t0);
Texture2D       nightTexture    : register(t1);
SamplerState    textureSampler  : register(s0);
SamplerState    nightSampler    : register(s1);

float4 PSMain(MirrorShaderOutput mirrorShaderOutput) : SV_Target
{
    float2 textureCoordinate = mirrorShaderOutput.texcoord;
    
    float4 backBufferColour  = flatTexture.Sample(textureSampler, textureCoordinate);
    float4 nightSkyColour    = nightTexture.Sample(nightSampler, textureCoordinate);
    
    //if (backBufferColour.a < 0.05)
    {
        //backBufferColour.rgba = 1 - nightSkyColour.rgba;
    }
    
    //backBufferColour.a = 1.0; // YUPP c:
   
    return float4(nightSkyColour.r, nightSkyColour.g, nightSkyColour.b, 1);
}
