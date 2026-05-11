using TheMirrorsEdge.Services.Interfaces;
using TheMirrorsEdge.Users.Interfaces;

namespace TheMirrorsEdge.Services.Wrappers;

public class UserList : IUserList
{
    public IMirrorsUser?[] Users 
        { get; }  = new IMirrorsUser[IUserList.UserArraySize];
    
    public IMirrorsUser? LocalPlayer
        => Users[0];
}