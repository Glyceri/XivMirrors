struct MirrorShaderOutput
{
    float4 position : SV_POSITION;
    float2 texcoord : TEXCOORD;
};

Texture2D<float> depthTextureNoTransparency   : register(t0);
Texture2D<float> depthTextureWithTransparency : register(t1);
Texture2D        backBuffer                   : register(t2);
Texture2D        backBufferNoUI               : register(t3);
Texture2D        modelMap                     : register(t4);
Texture2D<float> modelDepthMap                : register(t5);

SamplerState     textureSampler               : register(s0);

float LinearizeDepth(float z)
{
    return (2.0f * 0.1f) / (1.0 + 0.1f - z * (0.0 - 0.1f));
}

// This was for dithering, which looked SICK btw :yaya:
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
    float4 backBufferNoUIColour     = backBufferNoUI.Sample(textureSampler, texCoord);
    float4 modelMapColour           = modelMap.Sample(textureSampler, texCoord);

    float vfxAlpha                  = backBufferNoUIColour.a;
    float combinedAlpha             = backBufferColour.a;
    
    //return backBufferColour;
    
    if (backBufferNoUIColour.r != backBufferColour.r)
    {
        return float4(0, 1, 0, 1);
    }
    else
    {
        return float4(0, 0, 1, 1);
    }
    
    float uiAlpha = 0.0f;

    if (vfxAlpha < 1.0f)
    {
        uiAlpha = (combinedAlpha - vfxAlpha) / (1.0f - vfxAlpha);
    }
    
    return float4(uiAlpha, uiAlpha, uiAlpha, 1);
}
