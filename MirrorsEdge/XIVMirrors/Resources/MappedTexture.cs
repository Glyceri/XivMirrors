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
internal unsafe readonly struct MappedTexture : IDisposable
{
    public readonly ShaderResourceView  ShaderResourceView;
    public readonly Texture2D           Texture;

    private readonly bool     isNative;
    private readonly Texture* nativeTexture;

    public readonly uint Width;
    public readonly uint Height;

    /// <summary>
    /// Register a native game texture as a mapped texture.
    /// </summary>
    /// <param name="nativeTexture">Native game texture.</param>
    public MappedTexture(Texture* nativeTexture)
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
    public MappedTexture(DirectXData directXData, ref Texture2D texture2D)
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
    public MappedTexture(ref Texture2D texture2D, ref ShaderResourceView shaderResourceView)
    {
        isNative            = false;
        nativeTexture       = null;

        Texture             = texture2D;
        ShaderResourceView  = shaderResourceView;

        Width               = (uint)texture2D.Description.Width;
        Height              = (uint)texture2D.Description.Height;
    }

    public uint ActualWidth   
        => GetActualWidth();

    public uint ActualHeight  
        => GetActualHeight();

    public nint TextureHandle 
        => Texture.NativePointer;

    public ImTextureID Handle 
        => new ImTextureID(ShaderResourceView.NativePointer);

    public ScaledResolution ScaledResolution 
        => new ScaledResolution(Width, Height, ActualWidth, ActualHeight);

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

        return true;
    }

    public void Dispose()
    {
        if (isNative)
        {
            return;
        }

        ShaderResourceView?.Dispose();
        Texture?.Dispose();
    }
}
