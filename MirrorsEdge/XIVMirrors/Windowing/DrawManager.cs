using Dalamud.Interface.ImGuiBackend.Delegates;
using MirrorsEdge.XIVMirrors.Services;
using System;
using System.Reflection;


namespace MirrorsEdge.XIVMirrors.Windowing;

internal class DrawManager
{
    private readonly DalamudServices DalamudServices;
    private readonly MirrorServices  MirrorServices;

    private object? originalHandler;

    public DrawManager(DalamudServices dalamudServices, MirrorServices mirrorServices)
    {
        DalamudServices = dalamudServices;
        MirrorServices  = mirrorServices;
    }

    // This aparently isnt Cara's most cursed shit anymore
    // but it definitely is mine.
    public bool SetExclusiveDraw(Action action)
    {
        if (originalHandler != null)
        {
            return false;
        }

        try
        {
            Assembly? dalamudAssembly = DalamudServices.DalamudPlugin.GetType().Assembly;

            if (dalamudAssembly == null)
            {
                return false;
            }

            Type? service1T = dalamudAssembly.GetType("Dalamud.Service`1");

            if (service1T == null)
            {
                return false;                
            }

            Type? interfaceManagerT = dalamudAssembly.GetType("Dalamud.Interface.Internal.InterfaceManager");

            if (interfaceManagerT == null)
            {
                return false;
            }

            Type serviceInterfaceManager = service1T.MakeGenericType(interfaceManagerT);

            MethodInfo? getter = serviceInterfaceManager.GetMethod("Get", BindingFlags.Static | BindingFlags.Public);

            if (getter == null)
            {
                return false;
            }

            object? interfaceManager = getter.Invoke(null, null);

            if (interfaceManager == null)
            {
                return false;
            }

            FieldInfo? ef = interfaceManagerT.GetField("Draw", BindingFlags.Instance | BindingFlags.NonPublic);

            if (ef == null)
            {
                return false;
            }

            object? handler = ef.GetValue(interfaceManager);
            
            if (handler == null)
            {
                return false;
            }

            originalHandler = handler;

            ef.SetValue(interfaceManager, new ImGuiBuildUiDelegate(action));

            return true;
        }
        catch (Exception ex)
        {
            MirrorServices.MirrorLog.LogFatal(ex);
            MirrorServices.MirrorLog.LogFatal("Its cooked...");
        }

        return false;
    }

    public bool FreeExclusiveDraw()
    {
        if (originalHandler == null)
        {
            return true;
        }

        try
        {
            Assembly? dalamudAssembly = DalamudServices.DalamudPlugin.GetType().Assembly;

            if (dalamudAssembly == null)
            {
                return false;
            }

            Type? service1T = dalamudAssembly.GetType("Dalamud.Service`1");

            if (service1T == null)
            {
                return false;
            }

            Type? interfaceManagerT = dalamudAssembly.GetType("Dalamud.Interface.Internal.InterfaceManager");

            if (interfaceManagerT == null)
            {
                return false;
            }

            Type serviceInterfaceManager = service1T.MakeGenericType(interfaceManagerT);

            MethodInfo? getter = serviceInterfaceManager.GetMethod("Get", BindingFlags.Static | BindingFlags.Public);

            if (getter == null)
            {
                return false;
            }

            object? interfaceManager = getter.Invoke(null, null);

            if (interfaceManager == null)
            {
                return false;
            }

            FieldInfo? ef = interfaceManagerT.GetField("Draw", BindingFlags.Instance | BindingFlags.NonPublic);

            if (ef == null)
            {
                return false;
            }

            ef.SetValue(interfaceManager, originalHandler);

            originalHandler = null;

            return true;
        }
        catch (Exception ex)
        {
            MirrorServices.MirrorLog.LogFatal(ex);
            MirrorServices.MirrorLog.LogFatal("Its cooked...");
        }

        return false;
    }
}
