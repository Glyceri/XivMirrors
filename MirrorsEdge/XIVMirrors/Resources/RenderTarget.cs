using Dalamud.Bindings.ImGui;
using MirrorsEdge.XIVMirrors.Memory;
using MirrorsEdge.XIVMirrors.Resources.Struct;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace MirrorsEdge.XIVMirrors.Resources;

internal unsafe class RenderTarget : BasicTexture
{
    public override Texture2D           Texture             { get; }
    public override ShaderResourceView  ShaderResourceView  { get; }

    public readonly RenderTargetView    RenderTargetView;

    public RenderTarget(DirectXData directXData, Texture2D reference) : base(directXData)
    {
        Texture2DDescription desc = new Texture2DDescription()
        {
            Width               = reference.Description.Width,
            Height              = reference.Description.Height,
            MipLevels           = 1,
            ArraySize           = 1,
            Format              = Format.R8G8B8A8_UNorm,
            SampleDescription   = new SharpDX.DXGI.SampleDescription(1, 0),
            Usage               = ResourceUsage.Default,
            BindFlags           = BindFlags.RenderTarget | BindFlags.ShaderResource,
            CpuAccessFlags      = CpuAccessFlags.None,
            OptionFlags         = ResourceOptionFlags.None
        };

        Texture = new Texture2D(directXData.Device, desc);

        ShaderResourceViewDescription srvDesc = new ShaderResourceViewDescription()
        {
            Dimension   = ShaderResourceViewDimension.Texture2D,
            Format      = Format.R8G8B8A8_UNorm,
            Texture2D   = new ShaderResourceViewDescription.Texture2DResource()
            {
                MipLevels       = 1,
                MostDetailedMip = 0
            }
        };

        ShaderResourceView  = new ShaderResourceView(directXData.Device, Texture, srvDesc);

        RenderTargetView    = new RenderTargetView(directXData.Device, Texture);

        Width   = (uint)Texture.Description.Width;
        Height  = (uint)Texture.Description.Height;
    }

    public override ImTextureID Handle
        => new ImTextureID(ShaderResourceView.NativePointer);

    public override uint ActualWidth 
        => Width;

    public override uint ActualHeight 
        => Height;

    public override nint TextureHandle 
        => Texture.NativePointer;

    public override ScaledResolution ScaledResolution
        => new ScaledResolution((int)Width, (int)Height, (int)ActualWidth, (int)ActualHeight);

    public override void Dispose()
    {
        base.Dispose();

        ShaderResourceView?.Dispose();
        Texture?.Dispose();
        RenderTargetView?.Dispose();
    }
}
