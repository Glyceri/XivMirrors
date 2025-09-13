namespace MirrorsEdge.XIVMirrors.Rendering;

internal static class PrimitiveFactory
{
    public static Vertex[] Quad()
    {
        Vertex topLeft      = new Vertex(new SharpDX.Vector3(-1, 1, 0),     new SharpDX.Vector2(0, 0));
        Vertex topRight     = new Vertex(new SharpDX.Vector3(1, 1, 0),      new SharpDX.Vector2(1, 0));
        Vertex bottomLeft   = new Vertex(new SharpDX.Vector3(-1, -1, 0),    new SharpDX.Vector2(0, 1));
        Vertex bottomRight  = new Vertex(new SharpDX.Vector3(1, -1, 0),     new SharpDX.Vector2(1, 1));

        return
        [
            topLeft, 
            topRight, 
            bottomLeft, 
            bottomLeft, 
            topRight, 
            bottomRight
        ];
    }
}
