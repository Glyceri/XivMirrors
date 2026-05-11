using System;
using System.Runtime.CompilerServices;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using TheMirrorsEdge.Services;
using TheMirrorsEdge.Services.Interfaces;
using TheMirrorsEdge.Users;
using TheMirrorsEdge.Users.Interfaces;

namespace TheMirrorsEdge.Hooking.Elements;

public unsafe class CharacterManagerHook : HookableElement
{
    private readonly Hook<BattleChara.Delegates.OnInitialize>  OnInitializeBattleCharaHook;
    private readonly Hook<BattleChara.Delegates.Terminate>     OnTerminateBattleCharaHook;
    private readonly Hook<BattleChara.Delegates.Dtor>          OnDestroyBattleCharaHook;
    
    public CharacterManagerHook(DalamudServices dalamudServices, MirrorServices mirrorServices) 
        : base(dalamudServices, mirrorServices)
    {
        OnInitializeBattleCharaHook = DalamudServices.Hooking.HookFromAddress<BattleChara.Delegates.OnInitialize>   ((nint)BattleChara.StaticVirtualTablePointer->OnInitialize,     InitializeBattleChara);
        OnTerminateBattleCharaHook  = DalamudServices.Hooking.HookFromAddress<BattleChara.Delegates.Terminate>      ((nint)BattleChara.StaticVirtualTablePointer->Terminate,        TerminateBattleChara);
        OnDestroyBattleCharaHook    = DalamudServices.Hooking.HookFromAddress<BattleChara.Delegates.Dtor>           ((nint)BattleChara.StaticVirtualTablePointer->Dtor,             DestroyBattleChara);
    }

    public override void Init()
    {
        OnInitializeBattleCharaHook.Enable();
        OnTerminateBattleCharaHook.Enable();
        OnDestroyBattleCharaHook.Enable();
        
        FloodInitialList();
    }

    public override void Dispose()
    {
        OnInitializeBattleCharaHook.Dispose();
        OnTerminateBattleCharaHook.Dispose();
        OnDestroyBattleCharaHook.Dispose();
    }
    
    private void FloodInitialList()
    {
        for (int i = 0; i < IUserList.UserArraySize; i++)
        {
            BattleChara* bChara = CharacterManager.Instance()->BattleCharas[i];

            if (bChara == null)
            {
                continue;
            }

            ObjectKind charaKind = bChara->GetObjectKind();

            if (charaKind != ObjectKind.Pc)
            {
                continue;
            }

            HandleAsCreated(bChara);
        }
    }
    
    private void InitializeBattleChara(BattleChara* bChara)
    {
        try
        {
            OnInitializeBattleCharaHook!.OriginalDisposeSafe(bChara);
        }
        catch (Exception e)
        {
            MirrorServices.MirrorLog.LogException(e);
        }

        _ = DalamudServices.Framework.Run(() => 
        {
            HandleAsCreated(bChara);
        });
    }

    private void TerminateBattleChara(BattleChara* bChara)
    {
        HandleAsDeleted(bChara);

        try
        {
            OnTerminateBattleCharaHook!.OriginalDisposeSafe(bChara);
        }
        catch (Exception e)
        {
            MirrorServices.MirrorLog.LogException(e);
        }
    }

    private GameObject* DestroyBattleChara(BattleChara* bChara, byte freeMemory)
    {
        HandleAsDeleted(bChara);

        try
        {
            return OnDestroyBattleCharaHook!.OriginalDisposeSafe(bChara, freeMemory);
        }
        catch (Exception e)
        {
            MirrorServices.MirrorLog.LogException(e);
        }

        return null;
    }
    
    private void HandleAsCreated(BattleChara* newBattleChara)
    {
        if (newBattleChara == null)
        {
            return;
        }
        
        ObjectKind actualObjectKind = newBattleChara->ObjectKind;

        if (actualObjectKind != ObjectKind.Pc)
        {
            return;
        }
        
        int actualIndex = CreateActualIndex(newBattleChara->ObjectIndex);
        
        if (actualIndex < 0 || actualIndex >= IUserList.UserArraySize)
        {
            return;
        }
        
        MirrorsUser mirrorsUser = new MirrorsUser(newBattleChara);
        
        MirrorServices.UserList.Users[actualIndex] = mirrorsUser;
    }
    
    private void HandleAsDeleted(BattleChara* battleChara)
    {
        if (battleChara == null)
        {
            return;
        }
        
        ObjectKind actualObjectKind = battleChara->ObjectKind;

        if (actualObjectKind != ObjectKind.Pc)
        {
            return;
        }
        
        for (int i = 0; i < IUserList.UserArraySize; i++)
        {
            IMirrorsUser? user = MirrorServices.UserList.Users[i];
            
            if (user == null)
            {
                continue;
            }
            
            if (user.Address != (nint)battleChara)
            {
                continue;
            }
            
            user.Dispose();
            
            MirrorServices.UserList.Users[i] = null;
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int CreateActualIndex(ushort index)
        => (int)MathF.Floor(index * 0.5f);
    
}