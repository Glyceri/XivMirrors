using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Maths;
using System;

namespace MirrorsEdge.MirrorsEdge.Memory;

internal unsafe class RenderTarget : IDisposable
{
    public readonly ID3D11Texture2D*            Texture;
    public readonly ID3D11RenderTargetView*     RenderTargetView;
    public readonly ID3D11ShaderResourceView*   ShaderResourceView;

    public readonly uint Width;
    public readonly uint Height;

    private bool _disposed = false;

    public ID3D11Texture2D* ReplacableTexture;

    public RenderTarget(ID3D11Device* device, uint width, uint height)
    {
        Width = width;
        Height = height;

        Format format = Format.FormatB8G8R8A8Unorm;

        Texture2DDesc desc = new Texture2DDesc
        (
            format:         format,
            width:          width,
            height:         height,
            mipLevels:      1,
            sampleDesc:     new SampleDesc(count: 1, quality: 0),
            usage:          Usage.Default,
            cPUAccessFlags: 0,
            arraySize:      1,
            bindFlags:      (uint)(BindFlag.ShaderResource | BindFlag.RenderTarget),
            miscFlags:      (uint)ResourceMiscFlag.Shared
        );

        ID3D11Texture2D* texture = null;

        int result = device->CreateTexture2D(ref desc, null, ref texture);
    
        if (result != 0)
        {
            throw new Exception($"Failed to initialize: {result}");
        }

        RenderTargetViewDesc renderTargetViewDescription = new RenderTargetViewDesc
        (
            format: format,
            viewDimension: RtvDimension.Texture2D,
            texture2D: new Tex2DRtv
            (
                mipSlice: 0
            )
        );

        ID3D11RenderTargetView* renderTargetView = null;

        int renderTargetResult = device->CreateRenderTargetView((ID3D11Resource*)texture, ref renderTargetViewDescription, ref renderTargetView);

        if (renderTargetResult != 0)
        {
            throw new Exception($"Failed to initialize renderTargetResult: {result}");
        }

        ShaderResourceViewDesc shaderResourceViewDescription = new ShaderResourceViewDesc
        (
            format: format,
            viewDimension: Silk.NET.Core.Native.D3DSrvDimension.D3DSrvDimensionTexture2D,
            texture2D: new Tex2DSrv
            (
                mostDetailedMip: 0,
                mipLevels: 1
            )
        );

        ID3D11ShaderResourceView* shaderResourceView = null;

        int shaderTargetResult = device->CreateShaderResourceView((ID3D11Resource*)texture, ref shaderResourceViewDescription, ref shaderResourceView);

        if (shaderTargetResult != 0)
        {
            throw new Exception($"Failed to initialize renderTargetResult: {shaderTargetResult}");
        }

        Texture             = texture;
        RenderTargetView    = renderTargetView;
        ShaderResourceView  = shaderResourceView;
    }

    public float AspectRatio => (float)Height / Width;

    public Matrix4X4<float> AspectRatioTransform()
    {
        return Matrix4X4.CreateScale(1, AspectRatio, 1);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _ = Texture->Release();
        _ = RenderTargetView->Release();
        _ = ShaderResourceView->Release();
    }
}
