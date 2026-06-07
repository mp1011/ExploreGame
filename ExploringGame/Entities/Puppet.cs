using ExploringGame.Entities.EntityParts;
using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.SimpleShapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Logics.Collision;
using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Logics.Controllers.PuppetControllers;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System.Net.Quic;

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
        Size = new Vector3(1.0f, 2.0f, 1.0f);

        var torso = AddChild(new Cylinder(new Theme(Color.Blue)));
        torso.Axis = Axis.Y;
        torso.Width= 1.0f;
        torso.Depth = 1.0f;
        torso.Height = 1.0f;
        torso.SetSide(Side.Top, GetSide(Side.Top));

        var head = AddChild(new Ellipsoid(0.5f));
        head.Position = Position;
        head.SetSide(Side.Bottom, GetSide(Side.Top));

        var leftArm = worldSegment.AddChild(new BasicArm(worldSegment, this, 0.2f, 1.0f, 1.0f));
        leftArm.UpperArm.Position = Position;
        leftArm.LowerArm.Position = Position;

        leftArm.UpperArm.SetSide(Side.Bottom, GetSide(Side.Top));
        leftArm.LowerArm.SetSide(Side.Bottom, leftArm.UpperArm.GetSide(Side.Top));
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

