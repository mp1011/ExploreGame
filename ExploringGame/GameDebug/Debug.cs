using ExploringGame.GeometryBuilder;
using ExploringGame.Story;
using System;

namespace ExploringGame.GameDebug;

public static class Debug
{
    public static string Watch1 { get; set; }
    public static string Watch2 { get; set; }

    public static Shape WatchShape { get; set; }

    public static bool NoPhysics = false;
    public static bool NoNPCPhysics = false;

    public static bool FlyMode = false;
    public static bool NoDepthStencil = false;
    public static bool LightSpiritVisible = true;
    public static bool WaypointsVisible = false;

    public static bool SavePolygonImages = false;

    public static bool UseDebugScene = true;
    public static bool NoScene = false;

    public static MovingEntityDebugger MovingEntityDebugger;

    public static Action<SceneManager> SceneManagerDebugInit = null;

    public static void Message(bool condition, string message)
    {
        if(condition)
            Console.WriteLine(message);
    }
}
