using Dalamud.Bindings.ImGui;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using TheMirrorsEdge.Memory;
using TheMirrorsEdge.Resources.Structs;

namespace TheMirrorsEdge.Resources.Textures;

public class RenderTexture : BasicTexture
{
    public override Texture2D           Texture             { get; protected init; }
    public override ShaderResourceView  ShaderResourceView  { get; protected init; }

    public readonly RenderTargetView    RenderTargetView;

    public RenderTexture(DirectXData directXData, Texture2D reference)
    {
        Texture2DDescription desc = new Texture2DDescription()
        {
            Width               = reference.Description.Width,
            Height              = reference.Description.Height,
            MipLevels           = 1,
            ArraySize           = 1,
            Format              = reference.Description.Format,
            SampleDescription   = new SampleDescription(1, 0),
            Usage               = ResourceUsage.Default,
            BindFlags           = BindFlags.RenderTarget | BindFlags.ShaderResource,
            CpuAccessFlags      = CpuAccessFlags.None,
            OptionFlags         = ResourceOptionFlags.None
        };

        Texture = new Texture2D(directXData.Device, desc);

        ShaderResourceViewDescription srvDesc = new ShaderResourceViewDescription()
        {
            Dimension   = ShaderResourceViewDimension.Texture2D,
            Format      = reference.Description.Format,
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

    public MappedTexture ToMappedTexture(DirectXData directXData)
    {
        Texture2DDescription baseDescription = Texture.Description;

        Texture2DDescription newDescription  = new Texture2DDescription()
        { 
            Width               = baseDescription.Width,
            Height              = baseDescription.Height,
            MipLevels           = 1,
            ArraySize           = 1,
            Format              = baseDescription.Format,
            SampleDescription   = new SampleDescription(1, 0),
            Usage               = ResourceUsage.Default,
            BindFlags           = BindFlags.RenderTarget | BindFlags.ShaderResource,
            CpuAccessFlags      = CpuAccessFlags.None,
            OptionFlags         = ResourceOptionFlags.None
        };

        Texture2D newTexture = new Texture2D(directXData.Device, newDescription);

        directXData.Context.CopyResource(Texture, newTexture);

        return new MappedTexture(directXData, ref newTexture);
    }

    public override void Dispose()
    {
        ShaderResourceView?.Dispose();
        Texture?.Dispose();
        RenderTargetView?.Dispose();
    }
}
