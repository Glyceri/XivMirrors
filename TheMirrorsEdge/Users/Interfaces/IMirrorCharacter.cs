using FFXIVClientStructs.FFXIV.Client.Game.Character;

namespace TheMirrorsEdge.Users.Interfaces;

public unsafe interface IMirrorCharacter : IMirrorsEntity
{
    string       Name          { get; }
    ulong        ContentId     { get; }
    ushort       Homeworld     { get; }
    ulong        ObjectId      { get; }
    ushort       ObjectIndex   { get; }
    BattleChara* BattleChara   { get; }
}