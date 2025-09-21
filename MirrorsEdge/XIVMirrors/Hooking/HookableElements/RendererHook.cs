using Dalamud.Hooking;
using MirrorsEdge.XIVMirrors.Hooking.Enum;
using MirrorsEdge.XIVMirrors.Memory;
using MirrorsEdge.XIVMirrors.Services;
using System;
using System.Collections.Generic;

namespace MirrorsEdge.XIVMirrors.Hooking.HookableElements;

internal unsafe class RendererHook : HookableElement
{
    public  delegate void RenderPassDelegate(RenderPass renderPass);
    private delegate int  OMPresentDelegate(nint swapChain, uint syncInterval, uint flags);
    
    private readonly List<RenderPassDelegate>  renderPasses         = [];
    private readonly Hook<OMPresentDelegate>?  OMPresentHook;

    public RendererHook(DalamudServices dalamudServices, MirrorServices mirrorServices, DirectXData directXData) : base(dalamudServices, mirrorServices)
    {
        nint swapChainVTable                = GetVTable(directXData.SwapChain.NativePointer);

        nint vtablePresentAddress           = GetVTableAddress(swapChainVTable, 8);

        OMPresentHook                       = DalamudServices.Hooking.HookFromAddress<OMPresentDelegate>(vtablePresentAddress, ProperPresentDetour);
    }

    public override void Init()
    {
        OMPresentHook?.Enable();
    }

    private int ProperPresentDetour(nint swapChain, uint syncInterval, uint flags)
    {
        try
        {
            foreach (RenderPassDelegate renderPass in renderPasses)
            {
                renderPass?.Invoke(RenderPass.Pre);
            }

            int returner = OMPresentHook!.Original(swapChain, syncInterval, flags);

            foreach (RenderPassDelegate renderPass in renderPasses)
            {
                renderPass?.Invoke(RenderPass.Post);
            }

            return returner;
        }
        catch(Exception e)
        {
            MirrorServices.MirrorLog.LogException(e);

            return OMPresentHook!.Original(swapChain, syncInterval, flags);
        }
    }

    public void RegisterRenderPassListener(RenderPassDelegate renderDelegate)
    {
        _ = renderPasses.Remove(renderDelegate);

        renderPasses.Add(renderDelegate);
    }

    public void DeregisterRenderPassListener(RenderPassDelegate renderDelegate)
    {
        _ = renderPasses.Remove(renderDelegate);
    }

    public override void OnDispose()
    {
        OMPresentHook?.Dispose();

        renderPasses.Clear();
    }
}

/*
[0]	6ED3F979	(CMTUseCountedObject<CDXGISwapChain>::QueryInterface)
[1]	6ED3F84D	(CMTUseCountedObject<CDXGISwapChain>::AddRef)
[2]	6ED3F77D	(CMTUseCountedObject<CDXGISwapChain>::Release)
[3]	6ED6A6D7	(CDXGISwapChain::SetPrivateData)
[4]	6ED6A904	(CDXGISwapChain::SetPrivateDataInterface)
[5]	6ED72BC9	(CDXGISwapChain::GetPrivateData)
[6]	6ED6DCDD	(CDXGISwapChain::GetParent)
[7]	6ED69BF4	(CDXGISwapChain::GetDevice)
[8]	6ED3FAAD	(CDXGISwapChain::Present)
[9]	6ED40209	(CDXGISwapChain::GetBuffer)
[10]	6ED47C1C	(CDXGISwapChain::SetFullscreenState)
[11]	6ED48CD9	(CDXGISwapChain::GetFullscreenState)
[12]	6ED40CB1	(CDXGISwapChain::GetDesc)
[13]	6ED48A3B	(CDXGISwapChain::ResizeBuffers)
[14]	6ED6F153	(CDXGISwapChain::ResizeTarget)
[15]	6ED47BA5	(CDXGISwapChain::GetContainingOutput)
[16]	6ED6D9B5	(CDXGISwapChain::GetFrameStatistics)
[17]	6ED327B5	(CDXGISwapChain::GetLastPresentCount)
[18]	6ED43400	(CDXGISwapChain::GetDesc1)
[19]	6ED6D9D0	(CDXGISwapChain::GetFullscreenDesc)
[20]	6ED6DA90	(CDXGISwapChain::GetHwnd)
[21]	6ED6D79F	(CDXGISwapChain::GetCoreWindow)
[22]	6ED6E352	(?Present1@?QIDXGISwapChain2@@CDXGISwapChain@@UAGJIIPBUDXGI_PRESENT_PARAMETERS@@@Z)
[23]	6ED6E240	(CDXGISwapChain::IsTemporaryMonoSupported)
[24]	6ED44146	(CDXGISwapChain::GetRestrictToOutput)
[25]	6ED6F766	(CDXGISwapChain::SetBackgroundColor)
[26]	6ED6D6B9	(CDXGISwapChain::GetBackgroundColor)
[27]	6ED4417B	(CDXGISwapChain::SetRotation)
[28]	6ED6DDE3	(CDXGISwapChain::GetRotation)
[29]	6ED6FF85	(CDXGISwapChain::SetSourceSize)
[30]	6ED6DF4F	(CDXGISwapChain::GetSourceSize)
[31]	6ED6FCBD	(CDXGISwapChain::SetMaximumFrameLatency)
[32]	6ED6DBE5	(CDXGISwapChain::GetMaximumFrameLatency)
[33]	6ED6D8CD	(CDXGISwapChain::GetFrameLatencyWaitableObject)
[34]	6ED6FB45	(CDXGISwapChain::SetMatrixTransform)
[35]	6ED6DAD0	(CDXGISwapChain::GetMatrixTransform)
[36]	6ED6C155	(CDXGISwapChain::CheckMultiplaneOverlaySupportInternal)
[37]	6ED6E82D	(CDXGISwapChain::PresentMultiplaneOverlayInternal)
[38]	6ED4397A	(CMTUseCountedObject<CDXGISwapChain>::`vector deleting destructor')
[39]	6ED4EAE0	(CSwapBuffer::AddRef)
[40]	6ED46C81	(CMTUseCountedObject<CDXGISwapChain>::LUCBeginLayerDestruction)
 */

/*
[0]	6C5F62A6	(CContext::ID3D11DeviceContext2_QueryInterface_Thk)
[1]	6C5F628C	(CContext::ID3D11DeviceContext2_AddRef_Thk)
[2]	6C5F1B7D	(CContext::ID3D11DeviceContext2_Release_Thk)
[3]	6C64F652	(CContext::ID3D11DeviceContext2_GetDevice_)
[4]	6C64F67F	(CContext::ID3D11DeviceContext2_GetPrivateData_)
[5]	6C64F6D9	(CContext::ID3D11DeviceContext2_SetPrivateData_)
[6]	6C64F6B9	(CContext::ID3D11DeviceContext2_SetPrivateDataInterface_)
[7]	6C5F2BBC	(CContext::ID3D11DeviceContext2_SetConstantBuffers_<1,0>)
[8]	6C5F22C0	(CContext::ID3D11DeviceContext2_SetShaderResources_<1,4>)
[9]	6C5F3265	(CContext::ID3D11DeviceContext2_SetShader_<1,4>)
[10]	6C5F32F6	(CContext::ID3D11DeviceContext2_SetSamplers_<1,4>)
[11]	6C5F33C5	(CContext::ID3D11DeviceContext2_SetShader_<1,0>)
[12]	6C5F2D28	(CContext::ID3D11DeviceContext2_DrawIndexed_<1>)
[13]	6C5F3677	(CContext::ID3D11DeviceContext2_Draw_<1>)
[14]	6C5F1A57	(CContext::ID3D11DeviceContext2_Map_<1>)
[15]	6C5F1A79	(CContext::ID3D11DeviceContext2_Unmap_<1>)
[16]	6C5F2892	(CContext::ID3D11DeviceContext2_SetConstantBuffers_<1,4>)
[17]	6C5F3456	(CContext::ID3D11DeviceContext2_IASetInputLayout_<1>)
[18]	6C5F1FFB	(CContext::ID3D11DeviceContext2_IASetVertexBuffers_<1>)
[19]	6C5F1F72	(CContext::ID3D11DeviceContext2_IASetIndexBuffer_<1>)
[20]	6C5F6A08	(CContext::ID3D11DeviceContext2_DrawIndexedInstanced_<1>)
[21]	6C61938A	(CContext::ID3D11DeviceContext2_DrawInstanced_<1>)
[22]	6C632B40	(CContext::ID3D11DeviceContext2_SetConstantBuffers_<1,3>)
[23]	6C5F3DC2	(CContext::ID3D11DeviceContext2_SetShader_<1,3>)
[24]	6C5F2399	(CContext::ID3D11DeviceContext2_IASetPrimitiveTopology_<1>)
[25]	6C5F3CF5	(CContext::ID3D11DeviceContext2_SetShaderResources_<1,0>)
[26]	6C5F35DC	(CContext::ID3D11DeviceContext2_SetSamplers_<1,0>)
[27]	6C60C4E9	(CContext::ID3D11DeviceContext2_Begin_<1>)
[28]	6C5F6A35	(CContext::ID3D11DeviceContext2_End_<1>)
[29]	6C5F735D	(CContext::ID3D11DeviceContext2_GetData_<1>)
[30]	6C5F37EE	(CContext::ID3D11DeviceContext2_SetPredication_<1>)
[31]	6C5F3D1E	(CContext::ID3D11DeviceContext2_SetShaderResources_<1,3>)
[32]	6C5F35F8	(CContext::ID3D11DeviceContext2_SetSamplers_<1,3>)
[33]	6C5F23E8	(CContext::ID3D11DeviceContext2_OMSetRenderTargets_<1>)
[34]	6C5F538E	(CContext::ID3D11DeviceContext2_OMSetRenderTargetsAndUnorderedAccessViews_<1>)
[35]	6C5F34BC	(CContext::ID3D11DeviceContext2_OMSetBlendState_<1>)
[36]	6C5F3568	(CContext::ID3D11DeviceContext2_OMSetDepthStencilState_<1>)
[37]	6C5F41A9	(CContext::ID3D11DeviceContext2_SOSetTargets_<1>)
[38]	6C618838	(CContext::ID3D11DeviceContext2_DrawAuto_<1>)
[39]	6C6188EA	(CContext::ID3D11DeviceContext2_DrawIndexedInstancedIndirect_<1>)
[40]	6C618F86	(CContext::ID3D11DeviceContext2_DrawInstancedIndirect_<1>)
[41]	6C61850F	(CContext::ID3D11DeviceContext2_Dispatch_<1>)
[42]	6C6181C7	(CContext::ID3D11DeviceContext2_DispatchIndirect_<1>)
[43]	6C5F1B12	(CContext::ID3D11DeviceContext2_RSSetState_<1>)
[44]	6C5F1CE5	(CContext::ID3D11DeviceContext2_RSSetViewports_<1>)
[45]	6C5F277A	(CContext::ID3D11DeviceContext2_RSSetScissorRects_<1>)
[46]	6C5F6A60	(CContext::ID3D11DeviceContext2_CopySubresourceRegion_<1>)
[47]	6C612046	(CContext::ID3D11DeviceContext2_CopyResource_<1>)
[48]	6C5F1B97	(CContext::ID3D11DeviceContext2_UpdateSubresource_<1>)
[49]	6C612341	(CContext::ID3D11DeviceContext2_CopyStructureCount_<1>)
[50]	6C5F5945	(CContext::ID3D11DeviceContext2_ClearRenderTargetView_<1>)
[51]	6C610F81	(CContext::ID3D11DeviceContext2_ClearUnorderedAccessViewUint_<1>)
[52]	6C610A5C	(CContext::ID3D11DeviceContext2_ClearUnorderedAccessViewFloat_<1>)
[53]	6C5FA896	(CContext::ID3D11DeviceContext2_ClearDepthStencilView_<1>)
[54]	6C61D8F4	(CContext::ID3D11DeviceContext2_GenerateMips_<1>)
[55]	6C63507F	(CContext::ID3D11DeviceContext2_SetResourceMinLOD_<1>)
[56]	6C61E1AD	(CContext::ID3D11DeviceContext2_GetResourceMinLOD_<1>)
[57]	6C62A863	(CContext::ID3D11DeviceContext2_ResolveSubresource_<1>)
[58]	6C6198ED	(CContext::ID3D11DeviceContext2_ExecuteCommandList_<1>)
[59]	6C5F3D47	(CContext::ID3D11DeviceContext2_SetShaderResources_<1,1>)
[60]	6C5F3E3D	(CContext::ID3D11DeviceContext2_SetShader_<1,1>)
[61]	6C5F3614	(CContext::ID3D11DeviceContext2_SetSamplers_<1,1>)
[62]	6C63153B	(CContext::ID3D11DeviceContext2_SetConstantBuffers_<1,1>)
[63]	6C5F3D70	(CContext::ID3D11DeviceContext2_SetShaderResources_<1,2>)
[64]	6C5F3EB8	(CContext::ID3D11DeviceContext2_SetShader_<1,2>)
[65]	6C5F3635	(CContext::ID3D11DeviceContext2_SetSamplers_<1,2>)
[66]	6C6316B8	(CContext::ID3D11DeviceContext2_SetConstantBuffers_<1,2>)
[67]	6C5F3D99	(CContext::ID3D11DeviceContext2_SetShaderResources_<1,5>)
[68]	6C5F3FB0	(CContext::ID3D11DeviceContext2_CSSetUnorderedAccessViews_<1>)
[69]	6C5F3F33	(CContext::ID3D11DeviceContext2_SetShader_<1,5>)
[70]	6C5F3656	(CContext::ID3D11DeviceContext2_SetSamplers_<1,5>)
[71]	6C631835	(CContext::ID3D11DeviceContext2_SetConstantBuffers_<1,5>)
[72]	6C645CC3	(CContext::ID3D11DeviceContext2_VSGetConstantBuffers_<1>)
[73]	6C627412	(CContext::ID3D11DeviceContext2_PSGetShaderResources_<1>)
[74]	6C6275ED	(CContext::ID3D11DeviceContext2_PSGetShader_<1>)
[75]	6C627125	(CContext::ID3D11DeviceContext2_PSGetSamplers_<1>)
[76]	6C646318	(CContext::ID3D11DeviceContext2_VSGetShader_<1>)
[77]	6C627033	(CContext::ID3D11DeviceContext2_PSGetConstantBuffers_<1>)
[78]	6C61EF84	(CContext::ID3D11DeviceContext2_IAGetInputLayout_<1>)
[79]	6C61F09C	(CContext::ID3D11DeviceContext2_IAGetVertexBuffers_<1>)
[80]	6C61EE2C	(CContext::ID3D11DeviceContext2_IAGetIndexBuffer_<1>)
[81]	6C61D2AF	(CContext::ID3D11DeviceContext2_GSGetConstantBuffers_<1>)
[82]	6C61D869	(CContext::ID3D11DeviceContext2_GSGetShader_<1>)
[83]	6C61F080	(CContext::ID3D11DeviceContext2_IAGetPrimitiveTopology_<1>)
[84]	6C64613D	(CContext::ID3D11DeviceContext2_VSGetShaderResources_<1>)
[85]	6C645FA2	(CContext::ID3D11DeviceContext2_VSGetSamplers_<1>)
[86]	6C5FD919	(CContext::ID3D11DeviceContext2_GetPredication_<1>)
[87]	6C61D68E	(CContext::ID3D11DeviceContext2_GSGetShaderResources_<1>)
[88]	6C61D3A1	(CContext::ID3D11DeviceContext2_GSGetSamplers_<1>)
[89]	6C5F670E	(CContext::ID3D11DeviceContext2_OMGetRenderTargets_<1>)
[90]	6C62088C	(CContext::ID3D11DeviceContext2_OMGetRenderTargetsAndUnorderedAccessViews_<1>)
[91]	6C62039C	(CContext::ID3D11DeviceContext2_OMGetBlendState_<1>)
[92]	6C62053E	(CContext::ID3D11DeviceContext2_OMGetDepthStencilState_<1>)
[93]	6C62B170	(CContext::ID3D11DeviceContext2_SOGetTargets_<1>)
[94]	6C62798D	(CContext::ID3D11DeviceContext2_RSGetState_<1>)
[95]	6C5FA41D	(CContext::ID3D11DeviceContext2_RSGetViewports_<1>)
[96]	6C627806	(CContext::ID3D11DeviceContext2_RSGetScissorRects_<1>)
[97]	6C61E950	(CContext::ID3D11DeviceContext2_HSGetShaderResources_<1>)
[98]	6C61EC61	(CContext::ID3D11DeviceContext2_HSGetShader_<1>)
[99]	6C61E8EB	(CContext::ID3D11DeviceContext2_HSGetSamplers_<1>)
[100]	6C61E60C	(CContext::ID3D11DeviceContext2_HSGetConstantBuffers_<1>)
[101]	6C616E8A	(CContext::ID3D11DeviceContext2_DSGetShaderResources_<1>)
[102]	6C617065	(CContext::ID3D11DeviceContext2_DSGetShader_<1>)
[103]	6C616CEF	(CContext::ID3D11DeviceContext2_DSGetSamplers_<1>)
[104]	6C616A10	(CContext::ID3D11DeviceContext2_DSGetConstantBuffers_<1>)
[105]	6C60CD5A	(CContext::ID3D11DeviceContext2_CSGetShaderResources_<1>)
[106]	6C60D2BE	(CContext::ID3D11DeviceContext2_CSGetUnorderedAccessViews_<1>)
[107]	6C60CFA9	(CContext::ID3D11DeviceContext2_CSGetShader_<1>)
[108]	6C60CCF5	(CContext::ID3D11DeviceContext2_CSGetSamplers_<1>)
[109]	6C60CB4C	(CContext::ID3D11DeviceContext2_CSGetConstantBuffers_<1>)
[110]	6C5FA518	(CContext::ID3D11DeviceContext2_ClearState_<1>)
[111]	6C5F7A84	(CContext::ID3D11DeviceContext2_Flush_AppEntered)
[112]	6C64F6A2	(CContext::ID3D11DeviceContext2_GetType_)
[113]	6C61DC03	(CContext::ID3D11DeviceContext2_GetContextFlags_<1>)
[114]	6C61CAB1	(CContext::ID3D11DeviceContext2_FinishCommandList_<1>)
[115]	6C5F2ED9	(CContext::ID3D11DeviceContext2_CopySubresourceRegion1_<1>)
[116]	6C5F6E28	(CContext::ID3D11DeviceContext2_UpdateSubresource1_<1>)
[117]	6C5F5ED6	(CContext::ID3D11DeviceContext2_DiscardResource_<1>)
[118]	6C5F7055	(CContext::ID3D11DeviceContext2_DiscardView_<1>)
[119]	6C5F3A43	(CContext::ID3D11DeviceContext2_SetConstantBuffers1_<1,0>)
[120]	6C5F3C55	(CContext::ID3D11DeviceContext2_SetConstantBuffers1_<1,1>)
[121]	6C5F3C7D	(CContext::ID3D11DeviceContext2_SetConstantBuffers1_<1,2>)
[122]	6C5F3CA5	(CContext::ID3D11DeviceContext2_SetConstantBuffers1_<1,3>)
[123]	6C5F3830	(CContext::ID3D11DeviceContext2_SetConstantBuffers1_<1,4>)
[124]	6C5F3CCD	(CContext::ID3D11DeviceContext2_SetConstantBuffers1_<1,5>)
[125]	6C645BF7	(CContext::ID3D11DeviceContext2_VSGetConstantBuffers1_<1>)
[126]	6C61E540	(CContext::ID3D11DeviceContext2_HSGetConstantBuffers1_<1>)
[127]	6C616944	(CContext::ID3D11DeviceContext2_DSGetConstantBuffers1_<1>)
[128]	6C61D148	(CContext::ID3D11DeviceContext2_GSGetConstantBuffers1_<1>)
[129]	6C626DB1	(CContext::ID3D11DeviceContext2_PSGetConstantBuffers1_<1>)
[130]	6C60C714	(CContext::ID3D11DeviceContext2_CSGetConstantBuffers1_<1>)
[131]	6C5F1214	(CContext::ID3D11DeviceContext2_SwapDeviceContextState_<1>)
[132]	6C5F6B1B	(CContext::ID3D11DeviceContext2_ClearView_<1>)
[133]	6C5F2DDC	(CContext::ID3D11DeviceContext2_DiscardView1_<1>)
[134]	6C64130C	(CContext::ID3D11DeviceContext2_UpdateTileMappings_<1>)
[135]	6C612D6B	(CContext::ID3D11DeviceContext2_CopyTileMappings_<1>)
[136]	6C615CBD	(CContext::ID3D11DeviceContext2_CopyTiles_<1>)
[137]	6C645368	(CContext::ID3D11DeviceContext2_UpdateTiles_<1>)
[138]	6C628E89	(CContext::ID3D11DeviceContext2_ResizeTilePool_<1>)
[139]	6C63F155	(CContext::ID3D11DeviceContext2_TiledResourceBarrier_<1>)
[140]	6C62004F	(CContext::ID3D11DeviceContext2_IsAnnotationEnabled_<1>)
[141]	6C634C76	(CContext::ID3D11DeviceContext2_SetMarkerInt_<1>)
[142]	6C60C3E1	(CContext::ID3D11DeviceContext2_BeginEventInt_<1>)
[143]	6C619693	(CContext::ID3D11DeviceContext2_EndEvent_<1>)
*/
