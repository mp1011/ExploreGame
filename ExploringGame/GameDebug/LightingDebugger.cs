using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.Logics;
using System;

namespace ExploringGame.GameDebug;

public static class LightingDebugger
{
    public static Shape WatchShape;
    public static ILightSource WatchLight;

    public static void Check(ILightSource lightSource, IRoom targetRoom)
    {
        if (WatchShape == null || WatchLight == null)
            return;

        if (lightSource != WatchLight)
            return;

        if (targetRoom.LightingGroup != WatchShape.LightingGroup)
            return;

        Console.WriteLine("!");
    }
}
