using SharpDX;
using TheMirrorsEdge.Resources.Structs;
using TheMirrorsEdge.Services.Structs;

namespace TheMirrorsEdge.Services.ChildServices;

public class PrimitiveFactory
{
    public PrimitiveDeclaration Quad()
    {
        Vertex[] vertices =
        [
            new Vertex(new Vector3(-1, 1,  0), new Vector2(0, 0)),
            new Vertex(new Vector3(1,  1,  0), new Vector2(1, 0)),
            new Vertex(new Vector3(-1, -1, 0), new Vector2(0, 1)),
            new Vertex(new Vector3(1,  -1, 0), new Vector2(1, 1))
        ];

        ushort[] indices =
        [
            0, 1, 2,
            2, 1, 3
        ];

        return new PrimitiveDeclaration(vertices, indices);
    }
    
    public PrimitiveDeclaration Quad(Vector2 topLeft, Vector2 topRight, Vector2 botLeft, Vector2 botRight)
    {
        Vertex[] vertices =
        [
            new Vertex(new Vector3(-1, 1,  0), topLeft),
            new Vertex(new Vector3(1,  1,  0), topRight),
            new Vertex(new Vector3(-1, -1, 0), botLeft),
            new Vertex(new Vector3(1,  -1, 0), botRight)
        ];

        ushort[] indices =
        [
            0, 1, 2,
            2, 1, 3
        ];

        return new PrimitiveDeclaration(vertices, indices);
    }

    public PrimitiveDeclaration Cube()
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

        return new PrimitiveDeclaration(vertices, indices);
    }
}
