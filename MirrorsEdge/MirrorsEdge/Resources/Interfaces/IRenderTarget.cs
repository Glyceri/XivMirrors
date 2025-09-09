using SharpDX.Direct3D11;

namespace MirrorsEdge.mirrorsedge.Resources.Interfaces;

internal interface IRenderTarget
{
    public ShaderResourceView?  ShaderResourceView  { get; }
    public Texture2D?           Texture2D           { get; }
    public RenderTargetView?    RenderTargetView    { get; }
}
