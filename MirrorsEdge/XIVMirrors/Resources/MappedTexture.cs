using Dalamud.Bindings.ImGui;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using MirrorsEdge.XIVMirrors.Memory;
using MirrorsEdge.XIVMirrors.Resources.Struct;
using SharpDX.Direct3D11;
using System;

namespace MirrorsEdge.XIVMirrors.Resources;

/// <summary>
/// A MappedTexture holds a registered Texture2D and a corresponding ShaderResourceView.
/// </summary>
internal unsafe class MappedTexture : BasicTexture
{
    public override Texture2D           Texture             { get; }
    public override ShaderResourceView  ShaderResourceView  { get; }

    protected readonly bool     isNative;
    protected readonly Texture* nativeTexture;

    /// <summary>
    /// Register a native game texture as a mapped texture.
    /// </summary>
    /// <param name="nativeTexture">Native game texture.</param>
    public MappedTexture(DirectXData directXData, Texture* nativeTexture) : base(directXData)
    {
        isNative            = true;
        
        this.nativeTexture  = nativeTexture;

        Texture             = new Texture2D((nint)nativeTexture->D3D11Texture2D);
        ShaderResourceView  = new ShaderResourceView((nint)nativeTexture->D3D11ShaderResourceView);

        Width               = nativeTexture->AllocatedWidth;
        Height              = nativeTexture->AllocatedHeight;
    }

    /// <summary>
    /// Register an externaly created Texture2D and create a ShaderResourceView for it.
    /// </summary>
    /// <param name="directXData">The DirectXData object.</param>
    /// <param name="texture2D">The previously created Texture2D. [This object takes ownership]</param>
    public MappedTexture(DirectXData directXData, ref Texture2D texture2D) : base(directXData)
    {
        isNative            = false;
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
    public MappedTexture(DirectXData directXData, ref Texture2D texture2D, ref ShaderResourceView shaderResourceView) : base(directXData)
    {
        isNative            = false;
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
        base.Dispose();

        if (isNative)
        {
            return;
        }

        ShaderResourceView?.Dispose();
        Texture?.Dispose();
    }
}
