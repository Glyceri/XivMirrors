Texture2D    DiffuseTexture : register(t0);
SamplerState DiffuseSampler : register(s0);

struct PSInput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
};

struct ColourBuffer
{
    float4 colour;
};

cbuffer ColourBufferEl : register(b0)
{
    ColourBuffer colourBuffer;
};

float4 PSMain(PSInput input) : SV_TARGET
{
    float2 texCoord     = input.TexCoord;
    
    float4 textureColour =  DiffuseTexture.Sample(DiffuseSampler, texCoord);
    
    if (textureColour.a == 0)
        discard;
    
    float4 outputColour = textureColour * colourBuffer.colour;


    
    float  val          = outputColour.r + outputColour.g + outputColour.b + outputColour.a;
    
    outputColour        = float4(val, val, val, 1.0f);
    
    return outputColour;
}
