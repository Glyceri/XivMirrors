using FFXIVClientStructs.FFXIV.Client.Game.Character;
using TheMirrorsEdge.Users.Interfaces;

namespace TheMirrorsEdge.Users;

public unsafe class MirrorsUser : IMirrorsUser
{
    public nint         Address         { get; }
    public string       Name            { get; }
    public ulong        ContentId       { get; }
    public ushort       Homeworld       { get; }
    public ulong        ObjectId        { get; }
    public ushort       ObjectIndex     { get; }
    public BattleChara* BattleChara     { get; }
    public bool         IsLocalPlayer   { get; }
    
    public MirrorsUser(BattleChara* battleChara)
    {
        BattleChara     = battleChara;
        
        Address         = (nint)battleChara;
        
        ContentId       = battleChara->ContentId;
        Homeworld       = battleChara->HomeWorld;
        ObjectId        = battleChara->GetGameObjectId().ObjectId;
        ObjectIndex     = battleChara->ObjectIndex;
        IsLocalPlayer   = ObjectIndex == 0;
        
        Name            = battleChara->NameString;
    }

    public void Dispose()
        { }
}