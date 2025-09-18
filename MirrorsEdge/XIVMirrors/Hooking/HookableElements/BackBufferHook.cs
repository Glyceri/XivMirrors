using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using MirrorsEdge.XIVMirrors.Hooking.Enum;
using MirrorsEdge.XIVMirrors.Memory;
using MirrorsEdge.XIVMirrors.Mirrors;
using MirrorsEdge.XIVMirrors.Rendering;
using MirrorsEdge.XIVMirrors.Resources;
using MirrorsEdge.XIVMirrors.Resources.Struct;
using MirrorsEdge.XIVMirrors.Services;
using MirrorsEdge.XIVMirrors.Shaders;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System;
using System.Diagnostics.CodeAnalysis;
using static FFXIVClientStructs.FFXIV.Client.UI.Misc.GroupPoseModule;

namespace MirrorsEdge.XIVMirrors.Hooking.HookableElements;

internal unsafe class BackBufferHook : HookableElement
{
    private readonly DirectXData    DirectXData;
    private readonly RendererHook   RendererHook;
    private readonly ScreenHook     ScreenHook;
    private readonly ShaderHandler  ShaderHandler;

    private readonly Mirror Mirror;

    public MappedTexture? ThatOneSecretTexture;

    private MappedTexture?  backBufferWithUI;
    private MappedTexture?  backBufferNoUI;
    private MappedTexture?  nonTransparentDepthBuffer;
    private MappedTexture?  transparentDepthBuffer;

    private RenderTarget?   rtBackBufferWithUI;
    private RenderTarget?   rtBackBufferNoUI;
    private RenderTarget?   rtNonTransparentDepthBuffer;
    private RenderTarget?   rtTransparentDepthBuffer;

    public  RenderTarget?   BackBufferWithUI              => rtBackBufferWithUI;
    public  RenderTarget?   BackBufferNoUI                => rtBackBufferNoUI;
    public  RenderTarget?   DepthBufferNoTransparency     => rtNonTransparentDepthBuffer;
    public  RenderTarget?   DepthBufferWithTransparency   => rtTransparentDepthBuffer;

    private readonly SharpDX.Direct3D11.Buffer VertexBuffer;
    private readonly SharpDX.Direct3D11.Buffer IndexBuffer;

    struct Vertex
    {
        public Vector3 Position;
        public Vector2 TexCoord;
    }

    // Fullscreen quad in NDC (-1..1 space)
    Vertex[] vertices = new[]
    {
            new Vertex { Position = new Vector3(-1f, -1f, 0f), TexCoord = new Vector2(0f, 1f) },
            new Vertex { Position = new Vector3(-1f,  1f, 0f), TexCoord = new Vector2(0f, 0f) },
            new Vertex { Position = new Vector3( 1f,  1f, 0f), TexCoord = new Vector2(1f, 0f) },
            new Vertex { Position = new Vector3( 1f, -1f, 0f), TexCoord = new Vector2(1f, 1f) },
        };

    uint[] indices = new uint[]
    {
        0, 1, 2,
        0, 2, 3
    };

    public BackBufferHook(DalamudServices dalamudServices, MirrorServices mirrorServices, DirectXData directXData, RendererHook rendererHook, ScreenHook screenHook, ShaderHandler shaderHandler) : base(dalamudServices, mirrorServices)
    {
        DirectXData     = directXData;
        RendererHook    = rendererHook;
        ScreenHook      = screenHook;
        ShaderHandler   = shaderHandler;

        Mirror = new Mirror(directXData);

        ScreenHook.RegisterScreenSizeChangeCallback(OnScreenSizeChanged);

        VertexBuffer = SharpDX.Direct3D11.Buffer.Create(DirectXData.Device, BindFlags.VertexBuffer, vertices);
        IndexBuffer  = SharpDX.Direct3D11.Buffer.Create(DirectXData.Device, BindFlags.IndexBuffer, indices);


        if (((MyDevice*)DirectXData.KernelDevice)->someTexture != null)
        {
            ThatOneSecretTexture = new MappedTexture(DirectXData, ((MyDevice*)DirectXData.KernelDevice)->someTexture);
        }
    }

    public override void Init()
    {
        DalamudServices.Framework.RunOnFrameworkThread(() => RendererHook.RegisterRenderPassListener(OnRenderPass));
    }

    private void OnScreenSizeChanged(int newWidth, int newHeight)
    {
        MirrorServices.MirrorLog.LogVerbose("Screen size changed");

        DisposeOldBuffers();
    }

    private void OnRenderPass(RenderPass renderPass)
    {
        if (renderPass == RenderPass.Post)
        {
            return;
        }

        return;

        try
        {
            if (!SetupBuffers())
            {
                return;
            }

            if (!MirrorServices.Configuration.DebugClearAlpha)
            {
                //return;
            }

            CleanCutout(ref nonTransparentDepthBuffer, rtNonTransparentDepthBuffer);
            CleanCutout(ref transparentDepthBuffer, rtTransparentDepthBuffer);

            //CleanAlpha(ref backBufferWithUI, rtBackBufferWithUI);
            //CleanAlpha(ref backBufferNoUI,   rtBackBufferNoUI);

           
        }
        catch(Exception e)
        {
            MirrorServices.MirrorLog.LogError(e, "Fat chance your game just crashed.");
        }
    }

    private void CleanAlpha(ref MappedTexture? mappedTexture, RenderTarget? renderTarget)
    {
        if (mappedTexture == null)
        {
            return;
        }

        if (!mappedTexture.IsValid)
        {
            mappedTexture?.Dispose();

            mappedTexture = null;

            return;
        }

        if (renderTarget == null)
        {
            return;
        }

        using var backBufferTex = DirectXData.SwapChain.GetBackBuffer<Texture2D>(0);
        using var backBufferRTV = new RenderTargetView(DirectXData.Device, backBufferTex);

        DirectXData.Context.OutputMerger.SetRenderTargets(backBufferRTV);

        //Viewport viewport = new Viewport(0, 0, (int)mappedTexture.Width, (int)mappedTexture.Height);
        Viewport viewport = new Viewport(0, 0, (int)mappedTexture.Width / 2, (int)mappedTexture.Height / 2);

        DirectXData.Context.Rasterizer.SetViewport(viewport);

        DirectXData.Context.VertexShader.Set(ShaderHandler.AlphaShader.VertexShader);
        DirectXData.Context.PixelShader.Set(ShaderHandler.AlphaShader.FragmentShader);

        DirectXData.Context.PixelShader.SetShaderResource(0, mappedTexture.ShaderResourceView);
        DirectXData.Context.PixelShader.SetSampler(0, ShaderHandler.AlphaShader.SamplerState);

        //DirectXData.Context.OutputMerger.SetRenderTargets(renderTarget.RenderTargetView);

        DirectXData.Context.ClearRenderTargetView(renderTarget.RenderTargetView, new RawColor4(1, 0, 1, 1f));

        BlendStateDescription blendDesc = new BlendStateDescription();

        blendDesc.RenderTarget[0].IsBlendEnabled            = false;
        blendDesc.RenderTarget[0].RenderTargetWriteMask     = ColorWriteMaskFlags.All;

        DirectXData.Context.OutputMerger.SetBlendState(new BlendState(DirectXData.Device, blendDesc));

        DirectXData.Context.InputAssembler.InputLayout       = null;
        DirectXData.Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

        DirectXData.Context.Draw(3, 0);

        DirectXData.Context.PixelShader.SetShaderResource(0, null);

        DirectXData.Context.OutputMerger.ResetTargets();
    }

    private void CleanCutout(ref MappedTexture? mappedTexture, RenderTarget? renderTarget)
    {
        if (mappedTexture == null)
        {
            return;
        }

        if (!mappedTexture.IsValid)
        {
            mappedTexture?.Dispose();

            mappedTexture = null;

            return;
        }

        if (renderTarget == null)
        {
            return;
        }


        if (RendererHook.ProbablyReshadeBackbuffer == null)
        {
            return;
        }

        using Texture2D backBuffer = DirectXData.SwapChain.GetBackBuffer<Texture2D>(0);
        using RenderTargetView backBufferRTV = new RenderTargetView(DirectXData.Device, backBuffer);

        DirectXData.Context.OutputMerger.SetRenderTargets(RendererHook.ProbablyReshadeBackbuffer, backBufferRTV);


        //Viewport viewport = new Viewport(0, 0, (int)mappedTexture.Width, (int)mappedTexture.Height);
        Viewport viewport = new Viewport(0, 0, (int)mappedTexture.Width / 2, (int)mappedTexture.Height / 2);

        DirectXData.Context.Rasterizer.SetViewport(viewport);

        DirectXData.Context.VertexShader.Set(ShaderHandler.ClippedShader.VertexShader);
        DirectXData.Context.PixelShader.Set(ShaderHandler.ClippedShader.FragmentShader);

        mappedTexture.UpdateConstantBuffer(DirectXData);

        DirectXData.Context.VertexShader.SetConstantBuffer(0, mappedTexture.ConstantBuffer);
        DirectXData.Context.PixelShader.SetShaderResource(0, mappedTexture.ShaderResourceView);

        DirectXData.Context.PixelShader.SetSampler(0, ShaderHandler.ClippedShader.SamplerState);

        //DirectXData.Context.OutputMerger.SetRenderTargets(renderTarget.RenderTargetView);

        //DirectXData.Context.ClearRenderTargetView(currentRTV[0], new RawColor4(1, 0, 1, 1f));

        // Depth state
        var depthStencilDesc = new DepthStencilStateDescription
        {
           IsDepthEnabled = true,
            DepthWriteMask = DepthWriteMask.All,
            DepthComparison = Comparison.GreaterEqual // or GreaterEqual if the game uses reversed-Z
        };
        using var depthState = new DepthStencilState(DirectXData.Device, depthStencilDesc);
        DirectXData.Context.OutputMerger.SetDepthStencilState(depthState);

        var blendDesc = new BlendStateDescription()
        {
            AlphaToCoverageEnable = false,
            IndependentBlendEnable = false,
        };

        blendDesc.RenderTarget[0] = new RenderTargetBlendDescription()
        {
            IsBlendEnabled = true,
            SourceBlend = BlendOption.SourceAlpha,
            DestinationBlend = BlendOption.InverseSourceAlpha,
            BlendOperation = BlendOperation.Add,
            SourceAlphaBlend = BlendOption.One,
            DestinationAlphaBlend = BlendOption.Zero,
            AlphaBlendOperation = BlendOperation.Add,
            RenderTargetWriteMask = ColorWriteMaskFlags.All
        };

        using var blendState = new BlendState(DirectXData.Device, blendDesc);

        DirectXData.Context.OutputMerger.SetBlendState(blendState);

        DirectXData.Context.InputAssembler.InputLayout = ShaderHandler.ClippedShader.InputLayout;
        DirectXData.Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

        DirectXData.Context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(VertexBuffer, Utilities.SizeOf<Vertex>(), 0));
        DirectXData.Context.InputAssembler.SetIndexBuffer(IndexBuffer, Format.R32_UInt, 0);
        DirectXData.Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

        DirectXData.Context.DrawIndexed(indices.Length, 0, 0);

        DirectXData.Context.PixelShader.SetShaderResource(0, null);

        DirectXData.Context.OutputMerger.ResetTargets();
        DirectXData.Context.OutputMerger.SetBlendState(null);
        DirectXData.Context.OutputMerger.SetDepthStencilState(null);
    }

    private bool OverrideMappedTexture(ref MappedTexture? mappedTexture, ref RenderTarget? renderTarget, Texture* texture)
    {
        if (mappedTexture != null && renderTarget != null)
        {
            return true;
        }

        if (CreateMappedTexture(texture, out MappedTexture? newTexture))
        {
            MirrorServices.MirrorLog.LogVerbose("Created buffers");

            mappedTexture?.Dispose();

            mappedTexture = newTexture;

            renderTarget?.Dispose();

            renderTarget = new RenderTarget(DirectXData, mappedTexture.Texture);

            return true;
        }

        return false;
    }

    private bool CreateMappedTexture(Texture* texture, [NotNullWhen(true)] out MappedTexture? mappedTexture)
    {
        mappedTexture = null;

        if (texture == null)
        {
            return false;
        }

        mappedTexture = new MappedTexture(DirectXData, texture);

        return true;
    }

    private bool SetupBuffers()
    {
        MyRenderTargetManager* renderTargetManager = (MyRenderTargetManager*)RenderTargetManager.Instance();

        if (renderTargetManager == null)
        {
            return false;
        }

        bool failed = false;

        failed |= !OverrideMappedTexture(ref backBufferWithUI,          ref rtBackBufferWithUI,             renderTargetManager->BackBuffer);
        failed |= !OverrideMappedTexture(ref backBufferNoUI,            ref rtBackBufferNoUI,               renderTargetManager->BackBufferNoUI);
        failed |= !OverrideMappedTexture(ref nonTransparentDepthBuffer, ref rtNonTransparentDepthBuffer,    renderTargetManager->DepthBufferNoTransparency);
        failed |= !OverrideMappedTexture(ref transparentDepthBuffer,    ref rtTransparentDepthBuffer,       renderTargetManager->DepthBufferTransparency);

        return !failed;
    }

    private void DisposeOldBuffers()
    {
        MirrorServices.MirrorLog.LogVerbose("Disposed buffers");

        backBufferWithUI?.Dispose();
        backBufferWithUI?.Dispose();
        nonTransparentDepthBuffer?.Dispose();
        transparentDepthBuffer?.Dispose();

        rtBackBufferWithUI?.Dispose();
        rtBackBufferWithUI?.Dispose();
        rtNonTransparentDepthBuffer?.Dispose();
        rtTransparentDepthBuffer?.Dispose();

        backBufferWithUI            = null;
        backBufferNoUI              = null;
        nonTransparentDepthBuffer   = null;
        transparentDepthBuffer      = null;

        rtBackBufferWithUI          = null;
        rtBackBufferWithUI          = null;
        rtNonTransparentDepthBuffer = null;
        rtTransparentDepthBuffer    = null;
    }

    public override void OnDispose()
    {
        ThatOneSecretTexture?.Dispose();

        VertexBuffer?.Dispose();
        IndexBuffer?.Dispose();

        DisposeOldBuffers();

        Mirror.Dispose();

        ScreenHook.DeregisterScreenSizeChangeCallback(OnScreenSizeChanged);
        RendererHook.DeregisterRenderPassListener(OnRenderPass);
    }
}
