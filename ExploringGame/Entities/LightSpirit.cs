using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.Logics;
using ExploringGame.Logics.Controllers;
using ExploringGame.Services;

using Microsoft.Xna.Framework;
using ExploringGame.Logics.Collision;
using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.GameDebug;
using System;

namespace ExploringGame.Entities;

/// <summary>
/// Primary shape for the Light Spirit. Not rendered, used only to track position.
/// </summary>

public class LightSpirit : PlaceableShape, IControllable, ICollidable
{
    public int Health { get; set; } = 0;
    public LightSpiritPhase Phase { get; set; } = LightSpiritPhase.Absent;
    public LightSpiritSphere Sphere { get; private set; }

    public override CollisionGroup CollisionGroup => CollisionGroup.SolidEntity;
    public override CollisionGroup CollidesWithGroups => CollisionGroup.Environment | CollisionGroup.Doors;
    public override ViewFrom ViewFrom => GameDebug.Debug.LightSpiritVisible ? ViewFrom.Outside : ViewFrom.None;

    public override IColliderMaker ColliderMaker => new SphereColliderMaker(this);

    public LightSpirit()
    {
        // Set size so collider is created
        Size = new Vector3(0.2f, 0.2f, 0.2f);
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
        if (Debug.LightSpiritVisible)
            return TriangleMaker.BuildEllipsoid(this);
        else
            return Array.Empty<Triangle>();
    }
}
