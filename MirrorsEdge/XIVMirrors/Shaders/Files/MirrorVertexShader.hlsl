struct MirrorShaderOutput
{
    float4 position : SV_POSITION;
    float2 texcoord : TEXCOORD;
};

MirrorShaderOutput VSMain(uint id : SV_VertexID)
{
    MirrorShaderOutput output;

    float2 vertices[4] =
    {
        float2(-1.0, -1.0), // bottom-left
        float2(-1.0, 1.0),  // top-left
        float2(1.0, -1.0),  // bottom-right
        float2(1.0, 1.0)    // top-right
    };

    float2 uvs[4] =
    {
        float2(0.0, 1.0),   // bottom-left
        float2(0.0, 0.0),   // top-left
        float2(1.0, 1.0),   // bottom-right
        float2(1.0, 0.0)    // top-right
    };

    uint indices[6] =
    {
        0, 1, 2,    // first triangle
        2, 1, 3     // second triangle
    };

    uint vertexIndex = indices[id]; // remap using indices

    output.position  = float4(vertices[vertexIndex], 0.0, 1.0);
    output.texcoord  = uvs[vertexIndex];

    return output;
}

