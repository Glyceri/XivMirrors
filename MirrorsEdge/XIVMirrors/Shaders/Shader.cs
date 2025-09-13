using SharpDX.Direct3D11;
using System;

namespace MirrorsEdge.XIVMirrors.Shaders;

internal class Shader : IDisposable
{
    public readonly VertexShader VertexShader;
    public readonly PixelShader  FragmentShader;
    public readonly InputLayout  InputLayout;
    public readonly SamplerState SamplerState;

    public Shader(ShaderFactory factory, string vertexFile, string fragmentFile, InputElement[] inputElements)
    {
        bool failure = false;

        failure |= !factory.GetVertexShader(vertexFile, inputElements, out VertexShader!, out InputLayout!, out SamplerState!);
        failure |= !factory.GetFragmentShader(fragmentFile, out FragmentShader!);

        if (failure)
        {
            Dispose();

            throw new Exception("Shaders failed to initialize");
        }
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
