using Dalamud.Bindings.ImGui;
using SharpDX.Direct3D11;
using System;

namespace MirrorsEdge.XIVMirrors.Resources.Interfaces;

internal interface IRenderTarget : IDisposable
{
    public ShaderResourceView?  ShaderResourceView  { get; set; }
    public Texture2D?           Texture2D           { get; set; }
    public RenderTargetView?    RenderTargetView    { get; set; }

    public ImTextureID ImGUIHandle { get; }

    public IRenderTarget? Clone();
}
