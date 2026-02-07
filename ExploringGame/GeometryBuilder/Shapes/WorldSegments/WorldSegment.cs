using ExploringGame.Entities;
using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Logics.Pathfinding;
using ExploringGame.Logics.ShapeControllers;
using Microsoft.Xna.Framework;
using System;

namespace ExploringGame.GeometryBuilder.Shapes.WorldSegments;

public class WorldSegment : Shape, IControllable<SegmentTransitionController>
{
    public override ViewFrom ViewFrom => ViewFrom.None;

    public virtual Vector3 DefaultPlayerStart => Position;

    public virtual WorldSegmentTransition[] Transitions { get; } = Array.Empty<WorldSegmentTransition>();

    public WaypointGraph WaypointGraph { get; set; }

    public SegmentTransitionController Controller => throw new NotImplementedException();

    public WorldSegment(params Shape[] contents)
    {
        foreach (var item in contents)
            AddChild(item);
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return Array.Empty<Triangle>();
    }

    public IActiveObject CreateController(ServiceContainer serviceContainer)
    {
        return new SegmentTransitionController(this, 
            serviceContainer.Get<Player>(), 
            serviceContainer.Get<LoadedLevelData>());
    }

    public override Matrix GetWorldMatrix()
    {
        return Matrix.CreateTranslation(Vector3.Zero);
    }
}
