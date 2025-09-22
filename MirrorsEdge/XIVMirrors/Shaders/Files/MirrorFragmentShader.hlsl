struct MirrorShaderOutput
{
    float4 position : SV_POSITION;
    float2 texcoord : TEXCOORD;
};

Texture2D<float> depthTextureNoTransparency   : register(t0);
Texture2D<float> depthTextureWithTransparency : register(t1);
Texture2D        backBufferNoUI               : register(t2);
Texture2D        backBufferWithUI             : register(t3);
Texture2D        modelMap                     : register(t4);
Texture2D<float> modelDepthMap                : register(t5);

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
    
    float4 backBufferNoUIColour     = backBufferNoUI.Sample(textureSampler, texCoord);
    float4 backBufferWithUIColour   = backBufferWithUI.Sample(textureSampler, texCoord);
    float4 modelMapColour           = modelMap.Sample(textureSampler, texCoord);

    float4 finalColour = backBufferWithUIColour;
    
    if (depthModel > depthNoTransparency)
    {
        finalColour = modelMapColour;
        
        if (depthTransparency > depthModel)
        {
            float4 srcColor = finalColour;
            float alpha     = dot(srcColor.rgb, float3(0.299, 0.587, 0.114)); // luminance as alpha
            float4 dstColor = finalColour;
            
            finalColour = dstColor;
        }
    }    

    finalColour.a = 1;
    
    return finalColour;
}
