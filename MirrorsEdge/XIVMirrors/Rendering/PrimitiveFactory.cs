using SharpDX;
using PrimitiveDeclaration = (MirrorsEdge.XIVMirrors.Rendering.Vertex[] vertices, ushort[] indices);

namespace MirrorsEdge.XIVMirrors.Rendering;

internal static class PrimitiveFactory
{
    public static PrimitiveDeclaration Quad()
    {
        Vertex[] vertices =
        [
            new Vertex(new Vector3(-1, 1, 0),   new Vector2(0, 0)),
            new Vertex(new Vector3(1, 1, 0),    new Vector2(1, 0)),
            new Vertex(new Vector3(-1, -1, 0),  new Vector2(0, 1)),
            new Vertex(new Vector3(1, -1, 0),   new Vector2(1, 1))
        ];

        ushort[] indices =
        [
            0, 1, 2,
            2, 1, 3
        ];

        return (vertices, indices);
    }

    public static PrimitiveDeclaration Cube()
    {
        Vertex[] vertices =
        {
            // Back face
            new Vertex(new Vector3(-1, -1, -1), new Vector2(0, 1)),
            new Vertex(new Vector3(-1,  1, -1), new Vector2(0, 0)),
            new Vertex(new Vector3( 1,  1, -1), new Vector2(1, 0)),
            new Vertex(new Vector3( 1, -1, -1), new Vector2(1, 1)),

            // Front face
            new Vertex(new Vector3(-1, -1, 1), new Vector2(0, 1)),
            new Vertex(new Vector3( 1, -1, 1), new Vector2(1, 1)),
            new Vertex(new Vector3( 1,  1, 1), new Vector2(1, 0)),
            new Vertex(new Vector3(-1,  1, 1), new Vector2(0, 0)),

            // Left face
            new Vertex(new Vector3(-1, -1, 1), new Vector2(0, 1)),
            new Vertex(new Vector3(-1,  1, 1), new Vector2(0, 0)),
            new Vertex(new Vector3(-1,  1, -1), new Vector2(1, 0)),
            new Vertex(new Vector3(-1, -1, -1), new Vector2(1, 1)),

            // Right face
            new Vertex(new Vector3(1, -1, -1), new Vector2(0, 1)),
            new Vertex(new Vector3(1,  1, -1), new Vector2(0, 0)),
            new Vertex(new Vector3(1,  1,  1), new Vector2(1, 0)),
            new Vertex(new Vector3(1, -1,  1), new Vector2(1, 1)),

            // Top face
            new Vertex(new Vector3(-1, 1, -1), new Vector2(0, 1)),
            new Vertex(new Vector3(-1, 1,  1), new Vector2(0, 0)),
            new Vertex(new Vector3( 1, 1,  1), new Vector2(1, 0)),
            new Vertex(new Vector3( 1, 1, -1), new Vector2(1, 1)),

            // Bottom face
            new Vertex(new Vector3(-1, -1, -1), new Vector2(0, 1)),
            new Vertex(new Vector3( 1, -1, -1), new Vector2(1, 1)),
            new Vertex(new Vector3( 1, -1,  1), new Vector2(1, 0)),
            new Vertex(new Vector3(-1, -1,  1), new Vector2(0, 0)),
        };

        ushort[] indices =
        {
            0,  1,  2,  0,  2,  3,      // Back
            4,  5,  6,  4,  6,  7,      // Front
            8,  9,  10, 8,  10, 11,     // Left
            12, 13, 14, 12, 14, 15,     // Right
            16, 17, 18, 16, 18, 19,     // Top
            20, 21, 22, 20, 22, 23      // Bottom
        };

        return (vertices, indices);
    }
}
