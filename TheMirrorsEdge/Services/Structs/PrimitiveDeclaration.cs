using TheMirrorsEdge.Resources.Structs;

namespace TheMirrorsEdge.Services.Structs;

public struct PrimitiveDeclaration(Vertex[] vertices, ushort[] indices)
{
    public readonly Vertex[] Vertices = vertices;
    public readonly ushort[] Indices  = indices;
}