using ExploringGame.Config;
using ExploringGame.Entities;
using ExploringGame.Logics;
using ExploringGame.Logics.ShapeControllers;
using ExploringGame.Rendering;
using System;

namespace ExploringGame.GeometryBuilder.Shapes.WorldSegments;

public class WorldSegment : Shape, IControllable<SegmentTransitionController>
{
    public override ViewFrom ViewFrom => ViewFrom.None;

    public virtual WorldSegmentTransition[] Transitions { get; }

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
            serviceContainer.Get<TransitionShapesRegistrar>(),
            serviceContainer.Get<RenderBuffers>());
    }
}
