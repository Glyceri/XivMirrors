using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using MirrorsEdge.XIVMirrors.Hooking.HookableElements.Base;
using MirrorsEdge.XIVMirrors.Memory;
using MirrorsEdge.XIVMirrors.Rendering;
using MirrorsEdge.XIVMirrors.Resources;
using MirrorsEdge.XIVMirrors.Services;
using MirrorsEdge.XIVMirrors.Shaders;
using SharpDX.Direct3D11;

namespace MirrorsEdge.XIVMirrors.Hooking.HookableElements;

internal unsafe class TransparentBackBufferHook : BufferBuilder
{
    private MappedTexture? transparentStrengthMap;
    private MappedTexture? transparentDiffuseMap;
    private MappedTexture? transparentDiffuseLightMap;
    private MappedTexture? transparentSpecularLightMap;
    private MappedTexture? transparentRealTimeLightMap;

    private RenderTarget?  rtTransparentStrengthMap;
    private RenderTarget?  rtTransparentDiffuseMap;
    private RenderTarget?  rtTransparentDiffuseLightMap;
    private RenderTarget?  rtTransparentSpecularLightMap;
    private RenderTarget?  rtTransparentRealTimeLightMap;



    private MappedTexture? transparentStrengthMapCopy;
    private MappedTexture? transparentDiffuseMapCopy;
    private MappedTexture? transparentDiffuseLightMapCopy;
    private MappedTexture? transparentSpecularLightMapCopy;
    private MappedTexture? transparentRealTimeLightMapCopy;

    private RenderTarget?  finalMappedTransparentMap;

    public TransparentBackBufferHook(DalamudServices dalamudServices, MirrorServices mirrorServices, DirectXData directXData, RendererHook rendererHook, ScreenHook screenHook, ShaderHandler shaderHandler) : base (dalamudServices, mirrorServices, directXData, rendererHook, screenHook, shaderHandler)
    {
        
    }

    public MappedTexture? TransparentStrengthMap
        => transparentStrengthMap;

    public MappedTexture? TransparentDiffuseMap
        => transparentDiffuseMap;

    public MappedTexture? TransparentDiffuseLightMap
        => transparentDiffuseLightMap;

    public MappedTexture? TransparentSpecularLightMap
        => transparentSpecularLightMap;

    public MappedTexture? TransparentRealTimeLightMap
        => transparentRealTimeLightMap;



    public RenderTarget? RtTransparentStrengthMap
        => rtTransparentStrengthMap;

    public RenderTarget? RtTransparentDiffuseMap
        => rtTransparentDiffuseMap;

    public RenderTarget? RtTransparentDiffuseLightMap
        => rtTransparentDiffuseLightMap;

    public RenderTarget? RtTransparentSpecularLightMap
        => rtTransparentSpecularLightMap;

    public RenderTarget? RtTransparentRealTimeLightMap
        => rtTransparentRealTimeLightMap;



    public RenderTarget? FinalRenderTarget
        => finalMappedTransparentMap;

    protected override void OnPreRenderPass()
    {

    }

    protected override bool SetupBuffers()
    {
        MyRenderTargetManager* renderTargetManager = (MyRenderTargetManager*)RenderTargetManager.Instance();

        if (renderTargetManager == null)
        {
            return false;
        }

        bool failed = false;

        failed |= !OverrideMappedTexture(ref transparentStrengthMap,        ref rtTransparentStrengthMap,       renderTargetManager->TransparentStrengthMap);
        failed |= !OverrideMappedTexture(ref transparentDiffuseMap,         ref rtTransparentDiffuseMap,        renderTargetManager->TransparentDiffuseMap);
        failed |= !OverrideMappedTexture(ref transparentDiffuseLightMap,    ref rtTransparentDiffuseLightMap,   renderTargetManager->TransparentDiffuseLightMap);
        failed |= !OverrideMappedTexture(ref transparentSpecularLightMap,   ref rtTransparentSpecularLightMap,  renderTargetManager->TransparentSpecularDiffuseLightMap);
        failed |= !OverrideMappedTexture(ref transparentRealTimeLightMap,   ref rtTransparentRealTimeLightMap,  renderTargetManager->TransparentRealTimeLightMap);

        return !failed;
    }

    protected override void HandleShaders()
    {
        RunPastImageMappedShader(ShaderHandler.AlphaShader, ref transparentStrengthMap,      rtTransparentStrengthMap);
        RunPastImageMappedShader(ShaderHandler.AlphaShader, ref transparentDiffuseMap,       rtTransparentDiffuseMap);
        RunPastImageMappedShader(ShaderHandler.AlphaShader, ref transparentDiffuseLightMap,  rtTransparentDiffuseLightMap);
        RunPastImageMappedShader(ShaderHandler.AlphaShader, ref transparentSpecularLightMap, rtTransparentSpecularLightMap);
        RunPastImageMappedShader(ShaderHandler.AlphaShader, ref transparentRealTimeLightMap, rtTransparentRealTimeLightMap);

        HandleFullMapShader();
    }

    private void HandleFullMapShader()
    {
        DisposeCopies();

        transparentStrengthMapCopy      = rtTransparentStrengthMap?.ToMappedTexture(DirectXData);
        transparentDiffuseMapCopy       = rtTransparentDiffuseMap?.ToMappedTexture(DirectXData);
        transparentDiffuseLightMapCopy  = rtTransparentDiffuseLightMap?.ToMappedTexture(DirectXData);
        transparentSpecularLightMapCopy = rtTransparentSpecularLightMap?.ToMappedTexture(DirectXData);
        transparentRealTimeLightMapCopy = rtTransparentRealTimeLightMap?.ToMappedTexture(DirectXData);

        finalMappedTransparentMap       = transparentStrengthMapCopy?.CreateRenderTarget(DirectXData);

        if (transparentStrengthMapCopy == null || transparentDiffuseMapCopy == null || transparentDiffuseLightMapCopy == null || transparentSpecularLightMapCopy == null || transparentRealTimeLightMapCopy == null || finalMappedTransparentMap == null)
        {
            return;
        }

        ShaderHandler.TransparentMimicShader.Bind(transparentDiffuseMapCopy, transparentStrengthMapCopy, transparentDiffuseLightMapCopy, transparentRealTimeLightMapCopy, transparentSpecularLightMapCopy, finalMappedTransparentMap);

        BlendStateDescription blendStateDesc                    = new BlendStateDescription();

        blendStateDesc.RenderTarget[0].IsBlendEnabled           = true;
        blendStateDesc.RenderTarget[0].SourceBlend              = BlendOption.SourceAlpha;
        blendStateDesc.RenderTarget[0].DestinationBlend         = BlendOption.InverseSourceAlpha;
        blendStateDesc.RenderTarget[0].BlendOperation           = BlendOperation.Add;
        blendStateDesc.RenderTarget[0].SourceAlphaBlend         = BlendOption.One;
        blendStateDesc.RenderTarget[0].DestinationAlphaBlend    = BlendOption.InverseSourceAlpha;
        blendStateDesc.RenderTarget[0].AlphaBlendOperation      = BlendOperation.Add;
        blendStateDesc.RenderTarget[0].RenderTargetWriteMask    = ColorWriteMaskFlags.All;

        DirectXData.Context.OutputMerger.SetBlendState(new BlendState(DirectXData.Device, blendStateDesc));

        ShaderHandler.TransparentMimicShader.Draw();

        ShaderHandler.TransparentMimicShader.UnbindTexture();
    }

    protected override void DisposeOldBuffers()
    {
        transparentStrengthMap?.Dispose();
        transparentDiffuseMap?.Dispose();
        transparentDiffuseLightMap?.Dispose();
        transparentSpecularLightMap?.Dispose();
        transparentRealTimeLightMap?.Dispose();

        rtTransparentStrengthMap?.Dispose();
        rtTransparentDiffuseMap?.Dispose();
        rtTransparentDiffuseLightMap?.Dispose();
        rtTransparentSpecularLightMap?.Dispose();
        rtTransparentRealTimeLightMap?.Dispose();



        transparentStrengthMap          = null;
        transparentDiffuseMap           = null;
        transparentDiffuseLightMap      = null;
        transparentSpecularLightMap     = null;
        transparentRealTimeLightMap     = null;

        rtTransparentStrengthMap        = null;
        rtTransparentDiffuseMap         = null;
        rtTransparentDiffuseLightMap    = null;
        rtTransparentSpecularLightMap   = null;
        rtTransparentRealTimeLightMap   = null;
    }

    private void DisposeCopies()
    {
        transparentStrengthMapCopy?.Dispose();
        transparentDiffuseMapCopy?.Dispose();
        transparentDiffuseLightMapCopy?.Dispose();
        transparentSpecularLightMapCopy?.Dispose();
        transparentRealTimeLightMapCopy?.Dispose();

        finalMappedTransparentMap?.Dispose();


        transparentStrengthMapCopy      = null;
        transparentDiffuseMapCopy       = null;
        transparentDiffuseLightMapCopy  = null;
        transparentSpecularLightMapCopy = null;
        transparentRealTimeLightMapCopy = null;

        finalMappedTransparentMap       = null;
    }

    protected override void DisposeFinal()
    {
        DisposeCopies();
    }
}
