struct VS_OUT
{
    float4 pos : SV_POSITION;
    float2 uv  : TEXCOORD0;
};

VS_OUT VSMain(uint id : SV_VertexID)
{
    VS_OUT output;

    float2 vertices[3] =
    {
        float2(-1.0, -1.0),
        float2(-1.0, 3.0),
        float2(3.0, -1.0)
    };

    float2 uvs[3] =
    {
        float2(0.0, 1.0),
        float2(0.0, -1.0),
        float2(2.0, 1.0)
    };

    output.pos = float4(vertices[id], 0.0, 1.0);
    output.uv  = uvs[id];

    return output;
}
