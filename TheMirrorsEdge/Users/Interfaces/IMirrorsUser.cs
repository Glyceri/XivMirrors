using System;

namespace TheMirrorsEdge.Users.Interfaces;

public interface IMirrorsUser : IMirrorCharacter, IDisposable
{
    bool IsLocalPlayer { get; }
}