using Dalamud.Bindings.ImGui;
using FFXIVClientStructs.FFXIV.Client.Graphics.Environment;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Common.Math;
using MirrorsEdge.XIVMirrors.Services;
using System;
using static FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Object;
using Object = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Object;

namespace MirrorsEdge.XIVMirrors.Windowing.Windows;

internal unsafe class WorldWindow : MirrorWindow
{
    protected override System.Numerics.Vector2 MinSize { get; } = new System.Numerics.Vector2(350, 136);
    protected override System.Numerics.Vector2 MaxSize { get; } = new System.Numerics.Vector2(2000, 2000);
    protected override System.Numerics.Vector2 DefaultSize { get; } = new System.Numerics.Vector2(800, 400);

    int totalCount = 0;

    public WorldWindow(WindowHandler windowHandler, DalamudServices dalamudServices, MirrorServices mirrorServices) : base(windowHandler, dalamudServices, mirrorServices, "World Window", ImGuiWindowFlags.None)
    {
        Open();

        //float* yPos = &;

        //*yPos = 2;
    }

    protected override void OnDraw()
    {
        totalCount = 0;

        try
        {
            //Handle(&EnvManager.Instance()->EnvScene->EnvSpaces, 0);

        }
        catch (Exception ex)
        {
            MirrorServices.MirrorLog.LogException(ex);
        }
    }

    void DrawToScreen(Object* obj, out System.Numerics.Vector2 screenCoords)
    {
        screenCoords = Vector2.Zero;

        if (obj == null)
        {
            return;
        }

        if (DalamudServices.GameGui.WorldToScreen(obj->Position, out screenCoords))
        {
            // So, while WorldToScreen will return false if the point is off of game client screen, to
            // to avoid performance issues, we have to manually determine if creating a window would
            // produce a new viewport, and skip rendering it if so
            var objectText = $"{obj->Position}";

            var screenPos = ImGui.GetMainViewport().Pos;
            var screenSize = ImGui.GetMainViewport().Size;

            var windowSize = ImGui.CalcTextSize(objectText);

            // Add some extra safety padding
            windowSize.X += ImGui.GetStyle().WindowPadding.X + 10;
            windowSize.Y += ImGui.GetStyle().WindowPadding.Y + 10;

            if (screenCoords.X + windowSize.X > screenPos.X + screenSize.X ||
                screenCoords.Y + windowSize.Y > screenPos.Y + screenSize.Y)
                return;

            ImGui.SetNextWindowPos(new Vector2(screenCoords.X, screenCoords.Y));

            ImGui.SetNextWindowBgAlpha(100);

            if (ImGui.Begin(
                    $"Actor{totalCount}##ActorWindow{totalCount}",
                    ImGuiWindowFlags.NoDecoration |
                    ImGuiWindowFlags.AlwaysAutoResize |
                    ImGuiWindowFlags.NoSavedSettings |
                    ImGuiWindowFlags.NoMove |
                    ImGuiWindowFlags.NoMouseInputs |
                    ImGuiWindowFlags.NoDocking |
                    ImGuiWindowFlags.NoFocusOnAppearing |
                    ImGuiWindowFlags.NoNav))
            {
                ImGui.Text(objectText);
                ImGui.End();
            }
        }
    }

    void DrawObj(Object* obj, int depth)
    {
        totalCount++;

        DrawToScreen(obj, out System.Numerics.Vector2 screenCoords);

        try
        {
            string line = "";

            for (int i = 0; i < depth; i++)
            {
                line += "    ";
            }

            line += obj->Position + ", " + screenCoords;

            ImGui.TextColored(depth == 0 ? new Vector4(1, 0, 1, 1) : new Vector4(1, 1, 1, 1), line);
        }
        catch (Exception e)
        {
            MirrorServices.MirrorLog.Log(e.Message);
        }
    }

    void Handle(Object* obj, int depth = 0)
    {
        if (obj == null)
        {
            return;
        }


        DrawObj(obj, depth);

        try
        {
            SiblingEnumerator enumerator = obj->ChildObjects;

            while (enumerator.MoveNext())
            {
                try
                {
                    Handle(enumerator.Current, depth + 1);
                }
                catch (Exception e)
                {
                    MirrorServices.MirrorLog.Log(e.Message);
                }
            }
        }
        catch (Exception e)
        {
            MirrorServices.MirrorLog.Log(e.Message);
        }
    }
}
