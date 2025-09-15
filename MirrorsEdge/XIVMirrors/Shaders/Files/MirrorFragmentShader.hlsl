struct MirrorShaderOutput
{
    float4 position : SV_POSITION;
    float2 texcoord : TEXCOORD;
};

//struct ScaledResolution
//{
//    uint AssignedWidth;
//    uint AssignedHeight;
//    uint ActualWidth;
//    uint ActualHeight;
//};

//cbuffer ResolutionBuffer : register(b0)
//{
//    ScaledResolution scaledResolution;
//};

Texture2D       flatTexture     : register(t0);
Texture2D       nightTexture    : register(t1);
SamplerState    textureSampler  : register(s0);
SamplerState    nightSampler    : register(s1);


float4 PSMain(MirrorShaderOutput mirrorShaderOutput) : SV_Target
{
    //float scalerWidth   = (float) scaledResolution.ActualWidth  / (float) scaledResolution.AssignedWidth;
    //float scalerHeight  = (float) scaledResolution.ActualHeight / (float) scaledResolution.AssignedHeight;
    
    //float2 scaler = float2(scalerWidth, scalerHeight);
    
    float2 textureCoordinate = mirrorShaderOutput.texcoord;// * scaler;
    
    float4 backBufferColour  = flatTexture.Sample(textureSampler, textureCoordinate);
    float4 nightSkyColour    = nightTexture.Sample(nightSampler, textureCoordinate);
    
    if (nightSkyColour.r <0.05)
    {
        discard;
        //backBufferColour.rgb = float3(0, 1, 0);
    }
    
    nightSkyColour.a = 1.0; // YUPP c:
   
    return nightSkyColour;
}
