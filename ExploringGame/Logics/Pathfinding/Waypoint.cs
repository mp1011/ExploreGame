using ExploringGame.GameDebug;
using ExploringGame.GeometryBuilder.Shapes;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace ExploringGame.Logics.Pathfinding;

public class Waypoint
{
    public Room Room { get; }
    public Vector3 Position { get; }
    public List<Waypoint> Neighbors { get; } = new();
    public DebugMarker DebugMarker { get; set; }

    public bool IsTargeted
    {
        get => DebugMarker?.IsTargeted ?? false;
        set => DebugMarker.IsTargeted = value;
    }

    public Waypoint(Room room)
    {
        Room = room;
        Position = room.Position;
    }

    public void AddNeighbor(Waypoint neighbor)
    {
        if (!Neighbors.Contains(neighbor))
        {
            Neighbors.Add(neighbor);
        }
    }
}
