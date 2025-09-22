using MirrorsEdge.XIVMirrors.Memory;
using MirrorsEdge.XIVMirrors.Resources;
using MirrorsEdge.XIVMirrors.Services;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using System;
using System.Diagnostics.CodeAnalysis;

namespace MirrorsEdge.XIVMirrors.Shaders;

internal class ShaderFactory
{
    private readonly MirrorServices MirrorServices;
    private readonly ResourceLoader ResourceLoader;
    private readonly DirectXData    DirectXData;

    private readonly SamplerStateDescription SamplerDescription = new SamplerStateDescription
    {
        Filter              = Filter.MinMagMipLinear,
        AddressU            = TextureAddressMode.Clamp,
        AddressV            = TextureAddressMode.Clamp,
        AddressW            = TextureAddressMode.Clamp,
        ComparisonFunction  = Comparison.Never,
        BorderColor         = new Color4(0, 0, 0, 0),
        MipLodBias          = 0.0f,
        MaximumAnisotropy   = 1,
        MinimumLod          = 0,
        MaximumLod          = float.MaxValue
    };

    public ShaderFactory(MirrorServices mirrorServices, ResourceLoader resourceLoader, DirectXData directXData)
    {
        MirrorServices  = mirrorServices;
        ResourceLoader  = resourceLoader;
        DirectXData     = directXData;
    }

    public bool GetVertexShader(string resourceName, InputElement[] inputElements, [NotNullWhen(true)] out VertexShader? vertexShader, out InputLayout? inputLayout, [NotNullWhen(true)] out SamplerState? samplerState)
    {
        vertexShader = null;
        inputLayout  = null;
        samplerState = null;

        try
        {
            byte[] data  = LoadShaderBytes(resourceName);

            using CompilationResult? vertexShaderResult = ShaderBytecode.Compile(data, "VSMain", "vs_5_0");

            if (vertexShaderResult == null)
            {
                throw new Exception("Vertex shader result is NULL");
            }

            vertexShader = new VertexShader(DirectXData.Device, vertexShaderResult);

            if (inputElements.Length > 0)
            {
                inputLayout = new InputLayout(DirectXData.Device, ShaderSignature.GetInputSignature(vertexShaderResult), inputElements);
            }

            samplerState = new SamplerState(DirectXData.Device, SamplerDescription);

            return true;
        }
        catch (Exception ex)
        {
            MirrorServices.MirrorLog.LogException(ex);
        }

        return false;
    }

    public bool GetFragmentShader(string resourceName, [NotNullWhen(true)] out PixelShader? fragmentShader)
    {
        fragmentShader = null;

        try
        {
            byte[] data = LoadShaderBytes(resourceName);

            using CompilationResult fragmentShaderResult = ShaderBytecode.Compile(data, "PSMain", "ps_5_0");

            fragmentShader = new PixelShader(DirectXData.Device, fragmentShaderResult);

            return true;
        }
        catch (Exception ex)
        {
            MirrorServices.MirrorLog.LogException(ex);
        }

        return false;
    }

    private byte[] LoadShaderBytes(string resourceName)
    {
        if (!resourceName.EndsWith(".hlsl"))
        {
            throw new ArgumentException($"The file {resourceName} is not of type .hlsl.");
        }

        return ResourceLoader.GetEmbeddedResourceBytes(resourceName);
    }
}
