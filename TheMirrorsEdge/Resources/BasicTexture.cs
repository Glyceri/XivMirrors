using System;
using Dalamud.Bindings.ImGui;
using SharpDX.Direct3D11;
using TheMirrorsEdge.Resources.Structs;

namespace TheMirrorsEdge.Resources;

public abstract class BasicTexture : IDisposable
{
    public uint Width  { get; protected init; }
    public uint Height { get; protected init; }

    public abstract uint ActualWidth  { get; }
    public abstract uint ActualHeight { get; }

    public abstract Texture2D           Texture             { get; protected init; }
    public abstract ShaderResourceView  ShaderResourceView  { get; protected init; }

    public abstract ImTextureID Handle        { get; }
    public abstract nint        TextureHandle { get; }

    public abstract ScaledResolution ScaledResolution { get; }

    public abstract void Dispose();
}