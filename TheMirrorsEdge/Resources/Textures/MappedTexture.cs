using Dalamud.Bindings.ImGui;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using TheMirrorsEdge.Memory;
using TheMirrorsEdge.Resources.Structs;

namespace TheMirrorsEdge.Resources.Textures;

/// <summary>
/// A MappedTexture holds a registered Texture2D and a corresponding ShaderResourceView.
/// </summary>
public unsafe class MappedTexture : BasicTexture
{
    public override Texture2D           Texture             { get; protected init; }
    public override ShaderResourceView  ShaderResourceView  { get; protected init; }

    private readonly bool     isNativeBuffer;
    private readonly bool     isNative;
    private readonly bool     isNativeSRV;
    private readonly Texture* nativeTexture;
    private readonly nint     nativeTextureNint;

    public static MappedTexture CloneFrom(Texture2D texture2D, DirectXData directXData, bool isNative = false)
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
            BindFlags           = baseDescription.BindFlags | BindFlags.ShaderResource,
            CpuAccessFlags      = baseDescription.CpuAccessFlags,
            OptionFlags         = ResourceOptionFlags.None,
        };

        Texture2D newTexture = new Texture2D(directXData.Device, newDescription);

        directXData.Context.CopyResource(texture2D, newTexture);

        return new MappedTexture(directXData, ref newTexture, isNative);
    }
    
    public static MappedTexture CloneFrom(Texture* nativeTexture, DirectXData directXData)
    {
        Texture2D texture2D = new Texture2D((nint)nativeTexture->D3D11Texture2D);
        
        return CloneFrom(texture2D, directXData, true);
    }
    
    /// <summary>
    /// Register a native game texture as a mapped texture.
    /// </summary>
    /// <param name="nativeTexture">Native game texture.</param>
    public MappedTexture(Texture* nativeTexture)
    {
        isNative            = true;
        isNativeSRV         = false;
        
        this.nativeTexture  = nativeTexture;
        nativeTextureNint   = (nint)nativeTexture->D3D11Texture2D;

        Texture             = new Texture2D((nint)nativeTexture->D3D11Texture2D);
        ShaderResourceView  = new ShaderResourceView((nint)nativeTexture->D3D11ShaderResourceView);

        Width               = nativeTexture->AllocatedWidth;
        Height              = nativeTexture->AllocatedHeight;
    }
    
    public MappedTexture(DirectXData directXData, uint width, uint height) 
    {
        isNative                = false;
        isNativeSRV             = false;
        nativeTexture           = null;

        Texture2DDescription newDescription = new Texture2DDescription()
        { 
            Width               = (int)width,
            Height              = (int)height,
            MipLevels           = 1,
            ArraySize           = 1,
            Format              = Format.R16G16B16A16_Float,
            SampleDescription   = new SampleDescription(1, 0),
            Usage               = ResourceUsage.Default,
            BindFlags           = BindFlags.RenderTarget | BindFlags.ShaderResource,
            CpuAccessFlags      = CpuAccessFlags.None,
            OptionFlags         = ResourceOptionFlags.None
        };

        Texture                 = new Texture2D(directXData.Device, newDescription);
        ShaderResourceView      = new ShaderResourceView(directXData.Device, Texture);

        Width                   = width;
        Height                  = height;
    }

    /// <summary>
    /// Register an externaly created Texture2D and create a ShaderResourceView for it.
    /// </summary>
    /// <param name="directXData">The DirectXData object.</param>
    /// <param name="texture2D">The previously created Texture2D. [This object takes ownership]</param>
    public MappedTexture(DirectXData directXData, ref Texture2D texture2D, bool isNative = false)
    {
        isNativeBuffer      = isNative;
        isNativeSRV         = false;
        nativeTexture       = null;

        Texture             = texture2D;
        ShaderResourceView  = new ShaderResourceView(directXData.Device, texture2D);

        Width               = (uint)texture2D.Description.Width;
        Height              = (uint)texture2D.Description.Height;
    }

    /// <summary>
    /// Register an externally created Texture2D and ShaderResourceView.
    /// </summary>
    /// <param name="texture2D">The externally created Texture2D. [This object takes ownership]</param>
    /// <param name="shaderResourceView">The externally created ShaderResourceView. [This object takes ownership]</param>
    public MappedTexture(ref Texture2D texture2D, ref ShaderResourceView shaderResourceView, bool isNativeTex2D = false, bool isNativeSRV = false)
    {
        isNative            = isNativeTex2D;
        isNativeSRV         = isNativeSRV;
        nativeTexture       = null;

        Texture             = texture2D;
        ShaderResourceView  = shaderResourceView;

        Width               = (uint)texture2D.Description.Width;
        Height              = (uint)texture2D.Description.Height;
    }

    public override uint ActualWidth   
        => GetActualWidth();

    public override uint ActualHeight  
        => GetActualHeight();

    public override nint TextureHandle 
        => Texture.NativePointer;

    public override ImTextureID Handle 
        => new ImTextureID(ShaderResourceView.NativePointer);

    public override ScaledResolution ScaledResolution 
        => new ScaledResolution((int)Width, (int)Height, (int)ActualWidth, (int)ActualHeight);

    public bool IsValid =>
        GetValidStatus();

    public RenderTexture CreateRenderTarget(DirectXData data)
        => new RenderTexture(data, Texture);
    

    private uint GetActualWidth()
    {
        if (isNative)
        {
            return nativeTexture->ActualWidth;
        }

        return Width;
    }

    private uint GetActualHeight()
    {
        if (isNative)
        {
            return nativeTexture->ActualHeight;
        }

        return Height;
    }

    private bool GetValidStatus()
    {
        if (!isNative)
        {
            return true;
        }

        if (nativeTexture == null)
        {
            return false;
        }

        if ((nint)nativeTexture->D3D11Texture2D != nativeTextureNint)
        {
            return false;
        }

        if (nativeTexture->D3D11Texture2D == null)
        {
            return false;
        }    

        if (nativeTexture->D3D11ShaderResourceView == null)
        {
            return false;
        }

        if (nativeTexture->AllocatedWidth != Width)
        {
            return false;
        }

        if (nativeTexture->AllocatedHeight != Height)
        {
            return false;
        }

        return true;
    }

    public override void Dispose()
    {
        if (!isNativeSRV)
        {
            ShaderResourceView?.Dispose();
        }
        
        if (!isNative && !isNativeBuffer)
        {
            Texture?.Dispose();
        }
    }
}