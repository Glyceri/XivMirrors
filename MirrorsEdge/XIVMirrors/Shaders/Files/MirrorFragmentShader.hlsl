struct MirrorShaderOutput
{
    float4 position : SV_POSITION;
    float2 texcoord : TEXCOORD;
};

Texture2D<float> depthTextureNoTransparency   : register(t0);
Texture2D<float> depthTextureWithTransparency : register(t1);
Texture2D        backBuffer                   : register(t2);
Texture2D        modelMap                     : register(t3);
Texture2D<float> modelDepthMap                : register(t4);

SamplerState     textureSampler               : register(s0);

float LinearizeDepth(float z)
{
    return (2.0f * 0.1f) / (1000.0 + 0.1f - z * (1000.0 - 0.1f));
}

float Random(float2 uv)
{
    return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
}

float4 PSMain(MirrorShaderOutput mirrorShaderOutput) : SV_Target
{
    float2 texCoord = mirrorShaderOutput.texcoord;

    float depthNoTransparency       = depthTextureNoTransparency.Sample(textureSampler, texCoord);
    float depthTransparency         = depthTextureWithTransparency.Sample(textureSampler, texCoord);
    float depthModel                = modelDepthMap.Sample(textureSampler, texCoord);
    
    float4 backBufferColour         = backBuffer.Sample(textureSampler, texCoord);
    float4 modelMapColour           = modelMap.Sample(textureSampler, texCoord);

    float4 finalColour              = backBufferColour;
    
    // white means object should be fully visible
    float uiOcclusion               = saturate(1.0f - (backBufferColour.a * 1.25f));
    
    if (uiOcclusion < 0.3f)
    {
        return finalColour;
    }
    
    if (depthModel > depthNoTransparency)
    {
        finalColour = modelMapColour;
    }    
    
    if (depthTransparency > depthModel)
    {
        finalColour = finalColour;
    }

    finalColour.a = 1;
    
    return finalColour;
}
