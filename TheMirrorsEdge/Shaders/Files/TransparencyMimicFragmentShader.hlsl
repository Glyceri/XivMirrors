struct MirrorShaderOutput
{
    float4 position : SV_POSITION;
    float2 texcoord : TEXCOORD;
};

Texture2D           transparentStrengthMap        : register(t0);
Texture2D           transparentDiffuseMap         : register(t1);
Texture2D           transparentDiffuseLightMap    : register(t2);
Texture2D           transparentSpecularLightMap   : register(t3);
Texture2D<float>    transparentRealTimeLightMap   : register(t4); 

SamplerState textureSampler             : register(s0);

float4 PSMain(MirrorShaderOutput mirrorShaderOutput) : SV_Target
{
    float2 texCoord = mirrorShaderOutput.texcoord;
    
    float4 transparentDiffuseMapColour          = transparentDiffuseMap.Sample(textureSampler, texCoord);
    
    if (transparentDiffuseMapColour.a < 0.1f)
    {
        discard;
        
        return transparentDiffuseMapColour;
    }
    
    float4 transparentDiffuseLightMapColour     = transparentDiffuseLightMap.Sample(textureSampler, texCoord);
    
    float4 transparentStrengthMapColour         = transparentStrengthMap      .Sample(textureSampler, texCoord);
    float4 transparentSpecularLightMapColour = transparentSpecularLightMap.Sample(textureSampler, texCoord);
    
    float transparentRealTimeLightMapStrength     = transparentRealTimeLightMap.Sample(textureSampler, texCoord);
    

    float transparentDiffuseLightMapStrength    = transparentDiffuseLightMapColour.r;
    
    float biggestLight = max(transparentRealTimeLightMapStrength, transparentDiffuseLightMapStrength);
    
    float light = biggestLight;
    
    light *= transparentStrengthMapColour.g;
    
    float3 colour = transparentDiffuseMapColour.rgb * transparentStrengthMapColour.r;
    
    float3 newLightMap = float3(1 - transparentDiffuseLightMapColour.r, 1 - transparentDiffuseLightMapColour.g, 1 - transparentDiffuseLightMapColour.b);
    
    newLightMap *= transparentStrengthMapColour.g;
    
    
    
    float3 lightMap = transparentDiffuseLightMapColour.rgb * (transparentStrengthMapColour.g * transparentStrengthMapColour.b);
    
    lightMap = 1 - lightMap;
    
    lightMap *= 0.1f;
    
    //transparentDiffuseMapColour.rgb *= transparentStrengthMapColour.g;
    
    //transparentDiffuseMapColour.rgb -= 1 - (biggestLight * 0.01f);
    
    //transparentDiffuseMapColour.rgb *= (transparentStrengthMapColour.r * transparentStrengthMapColour.b);
    
    transparentDiffuseMapColour.rgb -= lightMap;
    
    //transparentDiffuseMapColour.rgb = colour;
    
    //transparentDiffuseMapColour.rgb *= light;
    
    //transparentDiffuseMapColour.rgb += transparentStrengthMapColour.b;
    
    //transparentDiffuseMapColour.rgb -= float3(light, light, light);
    transparentDiffuseMapColour.a *= 1 - transparentStrengthMapColour.b;
    
    transparentDiffuseMapColour.rgb += transparentSpecularLightMapColour.rgb;
    
    return transparentDiffuseMapColour;
    
    return float4(transparentStrengthMapColour.g, transparentStrengthMapColour.g, transparentStrengthMapColour.g, 1);

}
