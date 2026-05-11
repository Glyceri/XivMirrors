using System;
using SharpDX.Direct3D11;
using TheMirrorsEdge.Services;

namespace TheMirrorsEdge.Shaders;

public class Shader : IDisposable
{
    protected readonly MirrorServices MirrorServices;

    private readonly   string         VertexFileName;
    private readonly   string         FragmentFileName;

    protected readonly VertexShader?  VertexShader;
    protected readonly PixelShader?   FragmentShader;
    private   readonly InputLayout?   InputLayout;
    private   readonly SamplerState?  SamplerState;

    private readonly bool failure;

    protected int lastBoundSlot = 0;

    public Shader(MirrorServices mirrorServices, ShaderFactory factory, string vertexFile, string fragmentFile, InputElement[] inputElements)
    {
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

        MirrorServices.DirectXData.Context.VertexShader.Set(VertexShader);

        MirrorServices.DirectXData.Context.PixelShader.Set(FragmentShader);
        MirrorServices.DirectXData.Context.PixelShader.SetSampler(slot, SamplerState);

        MirrorServices.DirectXData.Context.InputAssembler.InputLayout = InputLayout;
        
        MirrorServices.MirrorLog.LogVerbose($"The shaders [{VertexFileName}, {FragmentFileName}] have been successfully bound.");
    }

    public void Release()
    {
        MirrorServices.DirectXData.Context.VertexShader.Set(null);

        MirrorServices.DirectXData.Context.PixelShader.Set(null);

        MirrorServices.DirectXData.Context.InputAssembler.InputLayout = null;
    }

    protected virtual void OnDispose() { }

    public void Dispose()
    {
        OnDispose();
        
        try
        {
            VertexShader?.Dispose();
        }
        catch(Exception e)
        {
            MirrorServices.MirrorLog.LogException(e);
        }

        try
        {
            FragmentShader?.Dispose();
        }
        catch(Exception e)
        {
            MirrorServices.MirrorLog.LogException(e);
        }

        try
        {
            InputLayout?.Dispose();
        }
        catch(Exception e)
        {
            MirrorServices.MirrorLog.LogException(e);
        }

        try
        {
            SamplerState?.Dispose();
        }
        catch(Exception e)
        {
            MirrorServices.MirrorLog.LogException(e);
        }
    }
}
