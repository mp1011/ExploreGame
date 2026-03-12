using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.Logics;
using ExploringGame.Logics.Controllers;
using ExploringGame.Services;

using Microsoft.Xna.Framework;
using ExploringGame.Logics.Collision;
using ExploringGame.Logics.Collision.ColliderMakers;

namespace ExploringGame.Entities;

/// <summary>
/// Primary shape for the Light Spirit. Not rendered, used only to track position.
/// </summary>

public class LightSpirit : PlaceableShape, IControllable, ICollidable
{
    public int Health { get; set; } = 0;
    public LightSpiritPhase Phase { get; set; } = LightSpiritPhase.Absent;
    public LightSpiritSphere Sphere { get; private set; }

    public override CollisionGroup CollisionGroup => CollisionGroup.MovingObjects;
    public override CollisionGroup CollidesWithGroups => CollisionGroup.Environment;
    public override ViewFrom ViewFrom => ViewFrom.None;

    public override IColliderMaker ColliderMaker => new SphereColliderMaker(this);

    public LightSpirit()
    {
        // Set size so collider is created
        Size = new Vector3(1.0f, 1.0f, 1.0f);
        // Create the spherical child shape
        Sphere = new LightSpiritSphere(this);
        AddChild(Sphere);
    }

    public IActiveObject CreateController(ServiceContainer serviceContainer)
    {
        var controller = serviceContainer.Get<LightSpiritController>();
        controller.LightSpirit = this;
        return controller;
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        // Primary shape has no geometry
        return System.Array.Empty<Triangle>();
    }
}
