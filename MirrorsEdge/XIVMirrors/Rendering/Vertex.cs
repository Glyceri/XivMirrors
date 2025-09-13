using SharpDX;

namespace MirrorsEdge.XIVMirrors.Rendering;

internal readonly struct Vertex
{
    public readonly Vector3 Position;
    public readonly Vector2 UV;

    public Vertex(Vector3 position, Vector2 uv)
    {
        Position = position;
        UV       = uv;
    }
}
