using MirrorsEdge.XIVMirrors.Memory;
using MirrorsEdge.XIVMirrors.Services;
using SharpDX.Direct3D11;
using System;

namespace MirrorsEdge.XIVMirrors.Shaders;

internal class Shader : IDisposable
{
    protected readonly DirectXData    DirectXData;
    protected readonly MirrorServices MirrorServices;

    private readonly   string         VertexFileName;
    private readonly   string         FragmentFileName;

    protected readonly VertexShader?  VertexShader;
    protected readonly PixelShader?   FragmentShader;
    private   readonly InputLayout?   InputLayout;
    private   readonly SamplerState?  SamplerState;

    private readonly bool failure;

    protected int lastBoundSlot = 0;

    public Shader(DirectXData data, MirrorServices mirrorServices, ShaderFactory factory, string vertexFile, string fragmentFile, InputElement[] inputElements)
    {
        DirectXData      = data;
        MirrorServices   = mirrorServices;

        VertexFileName   = vertexFile;
        FragmentFileName = fragmentFile;

        failure = false;

        failure |= !factory.GetVertexShader(vertexFile, inputElements, out VertexShader!, out InputLayout, out SamplerState!);
        failure |= !factory.GetFragmentShader(fragmentFile, out FragmentShader!);

        if (failure)
        {
            Dispose();

            throw new Exception($"Shaders failed to initialize. [{VertexFileName}, {FragmentFileName}]");
        }
    }

    public void Bind(int slot = 0)
    {
        lastBoundSlot = 0;

        if (failure)
        {
            MirrorServices.MirrorLog.LogWarning($"You are binding a shader that FAILED to initialize. DO NOT DO THIS!\n[{VertexFileName}, {FragmentFileName}].");

            return;
        }

        if (VertexShader == null)
        {
            MirrorServices.MirrorLog.LogWarning($"Vertex shader is NULL. This cannot be bound, so we quit.");

            return;
        }

        if (FragmentShader == null)
        {
            MirrorServices.MirrorLog.LogWarning($"Fragment shader is NULL. This cannot be bound, so we quit.");

            return;
        }

        if (SamplerState == null)
        {
            MirrorServices.MirrorLog.LogWarning($"SamplerState is NULL. This cannot be bound, so we quit.");

            return;
        }

        DirectXData.Context.VertexShader.Set(VertexShader);

        DirectXData.Context.PixelShader.Set(FragmentShader);
        DirectXData.Context.PixelShader.SetSampler(slot, SamplerState);

        DirectXData.Context.InputAssembler.InputLayout = InputLayout;

        MirrorServices.MirrorLog.LogVerbose($"The shaders [{VertexFileName}, {FragmentFileName}] have been successfully bound.");
    }

    public void Release()
    {
        DirectXData.Context.VertexShader.Set(null);

        DirectXData.Context.PixelShader.Set(null);

        DirectXData.Context.InputAssembler.InputLayout = null;
    }

    public void Dispose()
    {
        try
        {
            VertexShader?.Dispose();
        }
        catch { }

        try
        {
            FragmentShader?.Dispose();
        }
        catch { }

        try
        {
            InputLayout?.Dispose();
        }
        catch { }

        try
        {
            SamplerState?.Dispose();
        }
        catch { }
    }
}
