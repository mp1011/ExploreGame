using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Services;
using System;
using System.Collections.Generic;

namespace ExploringGame.Logics.Pathfinding;

public class Waypoint : PlaceableShape
{
    public Room Room { get; }
    public List<Waypoint> Neighbors { get; } = new();

    public bool IsTargeted { get; set; }

    public Waypoint(Room room)
    {
        Room = room;
        Position = room.Position;
        Width = 0.2f;
        Height = 0.2f;
        Depth = 0.2f;
    }

    public void AddNeighbor(Waypoint neighbor)
    {
        if (!Neighbors.Contains(neighbor))
        {
            Neighbors.Add(neighbor);
        }
    }

    public override ViewFrom ViewFrom => ViewFrom.None;

    public override CollisionGroup CollisionGroup => CollisionGroup.None;

    public override CollisionGroup CollidesWithGroups => CollisionGroup.None;

    public override IColliderMaker ColliderMaker => new BoundingBoxColliderMaker(this);

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return Array.Empty<Triangle>();
    }

    public override string ToString() => $"Waypoint ({Room})";
}
