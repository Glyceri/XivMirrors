using MirrorsEdge.mirrorsedge.Resources.Interfaces;
using SharpDX.Direct3D11;

namespace MirrorsEdge.mirrorsedge.Resources;

internal class RenderTarget : IRenderTarget
{
    public ShaderResourceView?  ShaderResourceView  { get; private set; }
    public Texture2D?           Texture2D           { get; private set; }
    public RenderTargetView?    RenderTargetView    { get; private set; }
}
