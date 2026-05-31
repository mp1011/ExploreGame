using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Logics.Collision;
using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Logics.Controllers;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.Entities;

public class Puppet : PlaceableShape, IControllable, ICollidable
{
    public override CollisionGroup CollisionGroup => CollisionGroup.MovingObjects;

    public override CollisionGroup CollidesWithGroups => CollisionGroup.Player | CollisionGroup.Environment;

    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public override Theme Theme { get; } = new Theme(Color.Blue);

    public override IColliderMaker ColliderMaker { get; } 

    public Puppet(WorldSegment worldSegment)
    {
        ColliderMaker = new SphereColliderMaker(this);
        Size = new Vector3(1.0f, 1.0f, 1.0f);
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return TriangleMaker.BuildEllipsoid(this, 8);
    }

    public IActiveObject CreateController(ServiceContainer serviceContainer)
    {
        var controller = serviceContainer.Get<PuppetController>();
        controller.Puppet = this;
        return controller;
        
    }
}

