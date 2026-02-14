using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Rendering;
using ExploringGame.Services;
using Microsoft.Xna.Framework;

namespace ExploringGame.Tests.WallDecalPlacement;

/// <summary>
/// Invisible shape that exists only to host the WallDecalTestController
/// </summary>
public class WallDecalTestShape : PlaceableShape, IControllable
{
    private readonly WallWithGapWorldSegment _worldSegment;

    public WallDecalTestController Controller { get; private set; }

    public override CollisionGroup CollisionGroup => CollisionGroup.None;
    public override CollisionGroup CollidesWithGroups => CollisionGroup.None;
    public override ViewFrom ViewFrom => ViewFrom.None;

    public WallDecalTestShape(WallWithGapWorldSegment worldSegment)
    {
        _worldSegment = worldSegment;
    }

    public IActiveObject CreateController(ServiceContainer serviceContainer)
    {
        Controller = new WallDecalTestController(_worldSegment, serviceContainer.Get<PointLights>(), serviceContainer.Get<LoadedLevelData>());
        return Controller;
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        // No geometry - this is just a controller host
        return System.Array.Empty<Triangle>();
    }
}
