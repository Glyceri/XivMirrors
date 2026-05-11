using Dalamud.Bindings.ImGui;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using TheMirrorsEdge.Memory;
using TheMirrorsEdge.Resources.Structs;

namespace TheMirrorsEdge.Resources.Textures;

public unsafe class DepthTexture : BasicTexture
{
    public override Texture2D           Texture             { get; protected init; }
    public override ShaderResourceView  ShaderResourceView  { get; protected init; }
    public          DepthStencilView    DepthStencilView    { get; }

    public static DepthTexture CloneFrom(Texture2D texture2D, DirectXData directXData)
    {
        Texture2DDescription baseDescription = texture2D.Description;

        Texture2DDescription newDescription  = new Texture2DDescription()
        { 
            Width               = baseDescription.Width,
            Height              = baseDescription.Height,
            MipLevels           = baseDescription.MipLevels,
            ArraySize           = baseDescription.ArraySize,
            Format              = baseDescription.Format,
            SampleDescription   = new SampleDescription(1, 0),
            Usage               = baseDescription.Usage,
            BindFlags           = baseDescription.BindFlags | BindFlags.ShaderResource | BindFlags.DepthStencil,
            CpuAccessFlags      = baseDescription.CpuAccessFlags,
            OptionFlags         = ResourceOptionFlags.None,
        };

        Texture2D newTexture = new Texture2D(directXData.Device, newDescription);

        directXData.Context.CopyResource(texture2D, newTexture);

        return new DepthTexture(directXData, ref newTexture);
    }
    
    public static DepthTexture CloneFrom(Texture* nativeTexture, DirectXData directXData)
    {
        Texture2D texture2D = new Texture2D((nint)nativeTexture->D3D11Texture2D);
        
        return CloneFrom(texture2D, directXData);
    }
    
    public DepthTexture(DirectXData directXData, uint width, uint height)
    {
        Texture2DDescription texture2DDescription = new Texture2DDescription()
        {
            Format              = Format.R32_Float,
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
            Format    = Format.D32_Float,
            Texture2D = new DepthStencilViewDescription.Texture2DResource
            {
                MipSlice = 0,
            }
        };

        ShaderResourceViewDescription shaderResourceViewDescription = new ShaderResourceViewDescription()
        {
            Format    = Format.R32_Float,
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

    public DepthTexture(DirectXData directXData, ref Texture2D texture2D)
    {
        Texture = texture2D;

        DepthStencilViewDescription depthStencilViewDescription = new DepthStencilViewDescription()
        {
            Dimension = DepthStencilViewDimension.Texture2D,
            Format    = Format.D32_Float,
            Texture2D = new DepthStencilViewDescription.Texture2DResource
            {
                MipSlice = 0,
            }
        };

        ShaderResourceViewDescription shaderResourceViewDescription = new ShaderResourceViewDescription()
        {
            Format    = Format.R32_Float,
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

    public MappedTexture ToMappedTexture(DirectXData directXData)
    {
        Texture2DDescription baseDescription = Texture.Description;

        Texture2DDescription newDescription  = new Texture2DDescription()
        { 
            Format              = Format.R32_Float,
            MipLevels           = 1,
            ArraySize           = 1,
            SampleDescription   = new SampleDescription(1, 0),
            BindFlags           = BindFlags.ShaderResource | BindFlags.DepthStencil,
            Height              = baseDescription.Height,
            Width               = baseDescription.Width,
        };

        Texture2D newTexture    = new Texture2D(directXData.Device, newDescription);

        directXData.Context.CopyResource(Texture, newTexture);

        return new MappedTexture(directXData, ref newTexture);
    }

    public override void Dispose()
    {
        DepthStencilView?.Dispose();
    }
}