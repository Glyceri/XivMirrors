using Dalamud.Bindings.ImGui;
using MirrorsEdge.mirrorsedge.Memory;
using MirrorsEdge.mirrorsedge.Resources.Interfaces;
using SharpDX.Direct3D11;

namespace MirrorsEdge.mirrorsedge.Resources;

internal class RenderTarget : IRenderTarget
{
    public ShaderResourceView?  ShaderResourceView  { get; set; }
    public Texture2D?           Texture2D           { get; set; }
    public RenderTargetView?    RenderTargetView    { get; set; }

    public ImTextureID ImGUIHandle => ShaderResourceView != null ? new ImTextureID(ShaderResourceView.NativePointer).Handle : ImTextureID.Null;

    private readonly Texture2DDescription   Description;
    private readonly DirectXData            DirectXData;

    public RenderTarget(DirectXData directXData, Texture2DDescription texture2DDescription)
    {
        DirectXData         = directXData;
        Description         = texture2DDescription;

        Texture2D           = new Texture2D(directXData.Device, texture2DDescription);
        RenderTargetView    = new RenderTargetView(directXData.Device, Texture2D);
        ShaderResourceView  = new ShaderResourceView(directXData.Device, Texture2D);
    }

    public IRenderTarget? Clone()
    {
        RenderTarget target = new RenderTarget(DirectXData, Description);

        if (Texture2D == null)
        {
            return null;
        }

        if (target.Texture2D == null)
        {
            return null;
        }

        DirectXData.Context.CopyResource(Texture2D, target.Texture2D);

        return target;
    }

    public void Dispose()
    {
        Texture2D?.Dispose();
        ShaderResourceView?.Dispose();
        RenderTargetView?.Dispose();
    }
}
