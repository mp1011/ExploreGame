using ExploringGame.GameDebug;
using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Services;
using System;

namespace ExploringGame.Logics.Pathfinding;

public class Waypoint : PlaceableShape
{
    public bool IsTargeted { get; set; }

    public Waypoint(Room room)
    {
        Room = room;
        LocalPosition = room.LocalPosition;
        Width = 0.2f;
        Height = 0.2f;
        Depth = 0.2f;
    }

    public override ViewFrom ViewFrom => Debug.WaypointsVisible ? ViewFrom.Outside : ViewFrom.None;

    public override CollisionGroup CollisionGroup => CollisionGroup.None;

    public override CollisionGroup CollidesWithGroups => CollisionGroup.None;

    public override IColliderMaker ColliderMaker => new BoundingBoxColliderMaker(this);

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        if (Debug.WaypointsVisible)
            return BuildCuboid();
        else 
            return Array.Empty<Triangle>();
    }

    public override string ToString() => $"Waypoint ({Room})";
}
