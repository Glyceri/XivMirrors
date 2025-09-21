using Dalamud.Bindings.ImGui;
using MirrorsEdge.XIVMirrors.Memory;
using MirrorsEdge.XIVMirrors.Resources.Struct;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace MirrorsEdge.XIVMirrors.Resources;

internal unsafe class DepthTexture : BasicTexture
{
    public override Texture2D           Texture             { get; }
    public override ShaderResourceView  ShaderResourceView  { get; }
    public          DepthStencilView    DepthStencilView    { get; }

    public DepthTexture(DirectXData directXData, uint width, uint height) : base(directXData)
    {
        Texture2DDescription texture2DDescription = new Texture2DDescription()
        {
            Format              = Format.R24G8_Typeless,
            MipLevels           = 1,
            ArraySize           = 1,
            SampleDescription   = new SampleDescription(1, 0),
            BindFlags           = BindFlags.ShaderResource | BindFlags.DepthStencil,
            Height              = (int)height,
            Width               = (int)width,
        };

        Texture = new Texture2D(directXData.Device, texture2DDescription);

        DepthStencilViewDescription depthStencilViewDescription = new DepthStencilViewDescription()
        {
            Dimension = DepthStencilViewDimension.Texture2D,
            Format    = Format.D24_UNorm_S8_UInt,
            Texture2D = new DepthStencilViewDescription.Texture2DResource
            {
                MipSlice = 0,
            }
        };

        ShaderResourceViewDescription shaderResourceViewDescription = new ShaderResourceViewDescription()
        {
            Format    = Format.R24_UNorm_X8_Typeless,
            Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D,
            Texture2D = new ShaderResourceViewDescription.Texture2DResource
            {
                MipLevels = 1
            }
        };

        DepthStencilView   = new DepthStencilView(directXData.Device, Texture, depthStencilViewDescription);
        ShaderResourceView = new ShaderResourceView(directXData.Device, Texture, shaderResourceViewDescription);

        Width  = (uint)Texture.Description.Width;
        Height = (uint)Texture.Description.Height;
    }

    public DepthTexture(DirectXData directXData, ref Texture2D texture2D) : base(directXData)
    {
        Texture = texture2D;

        DepthStencilViewDescription depthStencilViewDescription = new DepthStencilViewDescription()
        {
            Dimension = DepthStencilViewDimension.Texture2D,
            Format    = Format.D24_UNorm_S8_UInt,
            Texture2D = new DepthStencilViewDescription.Texture2DResource
            {
                MipSlice = 0,
            }
        };

        ShaderResourceViewDescription shaderResourceViewDescription = new ShaderResourceViewDescription()
        {
            Format    = Format.R24_UNorm_X8_Typeless,
            Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D,
            Texture2D = new ShaderResourceViewDescription.Texture2DResource
            {
                MipLevels = 1
            }
        };

        DepthStencilView   = new DepthStencilView(directXData.Device, Texture, depthStencilViewDescription);
        ShaderResourceView = new ShaderResourceView(directXData.Device, Texture, shaderResourceViewDescription);

        Width  = (uint)Texture.Description.Width;
        Height = (uint)Texture.Description.Height;
    }

    public override uint ActualWidth
        => Width;

    public override uint ActualHeight
        => Height;

    public override nint TextureHandle
        => Texture.NativePointer;

    public override ImTextureID Handle 
        => new ImTextureID(ShaderResourceView.NativePointer);

    public override ScaledResolution ScaledResolution
        => new ScaledResolution((int)Width, (int)Height, (int)ActualWidth, (int)ActualHeight);

    public override void Dispose()
    {
        base.Dispose();

        DepthStencilView?.Dispose();
    }
}
