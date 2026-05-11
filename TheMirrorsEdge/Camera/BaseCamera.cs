using TheMirrorsEdge.Camera.Interfaces;
using XIVCamera = FFXIVClientStructs.FFXIV.Client.Game.Camera;

namespace TheMirrorsEdge.Camera;

public abstract class BaseCamera : ICamera
{
    private readonly nint Address;
    
    public BaseCamera(nint address) 
        => Address = address;
    
    public unsafe XIVCamera* Camera 
        => (XIVCamera*)Address;

    public abstract void Dispose();
}