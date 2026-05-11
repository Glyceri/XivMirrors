using TheMirrorsEdge.Users.Interfaces;

namespace TheMirrorsEdge.Services.Interfaces;

public interface IUserList
{
    public const int UserArraySize = 100;
    
    IMirrorsUser?[] Users       { get; }
    IMirrorsUser?   LocalPlayer { get; }
}