using SharpDX;

namespace MirrorsEdge.XIVMirrors.Rendering;

internal static class PrimitiveFactory
{
    public static (Vertex[] vertices, ushort[] indices) Quad()
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

    public static (Vertex[] vertices, ushort[] indices) Cube()
    {
        Vertex[] vertices =
        [
            new Vertex(new Vector3(-1, -1, -1),   new Vector2(0, 1)),
            new Vertex(new Vector3(-1, 1, -1),    new Vector2(0, 0)),
            new Vertex(new Vector3(1, 1, -1),     new Vector2(1, 0)),
            new Vertex(new Vector3(1, -1, -1),    new Vector2(1, 1)),

            new Vertex(new Vector3(-1, -1, 0),    new Vector2(0, 1)),
            new Vertex(new Vector3(-1, 1, 0),     new Vector2(0, 0)),
            new Vertex(new Vector3(1, 1, 0),      new Vector2(1, 0)),
            new Vertex(new Vector3(1, -1, 0),     new Vector2(1, 1))
        ];

        ushort[] indices =
        {
            0, 1, 2,  0, 2, 3,  // back
            4, 6, 5,  4, 7, 6,  // front
            4, 5, 1,  4, 1, 0,  // left
            3, 2, 6,  3, 6, 7,  // right
            1, 5, 6,  1, 6, 2,  // top
            4, 0, 3,  4, 3, 7   // bottom
        };

        return (vertices, indices);
    }
}
