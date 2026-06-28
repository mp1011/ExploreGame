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
using ExploringGame.Story;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System.Net.Quic;

namespace ExploringGame.Entities;

public class Puppet : PlaceableShape, IControllable, ICollidable, IPhysicsShape
{
    public override CollisionGroup CollisionGroup => CollisionGroup.MovingObjects;

    public override CollisionGroup CollidesWithGroups => CollisionGroup.Player | CollisionGroup.Environment;

    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public override Theme Theme { get; } = new Theme(Color.Blue);

    public override IColliderMaker ColliderMaker { get; }


    private bool _active = true;
    public bool Active
    {
        get => _active;
        set
        {
            _active = value;
            LeftShoulder.Active= value;
            RightShoulder.Active = value;
            LeftArm.UpperArm.Active = value;
            LeftArm.LowerArm.Active = value;
            RightArm.UpperArm.Active = value;
            RightArm.LowerArm.Active = value;
        }
    }

    public Shoulder LeftShoulder { get; }
    public Shoulder RightShoulder { get; }
    public BasicArm LeftArm { get; }
    public BasicArm RightArm { get; }   

    public Puppet(WorldSegment worldSegment)
    {
     
        ColliderMaker = new SphereColliderMaker(this);
        Size = new Vector3(1.0f, 2.0f, 1.0f);

        var torso = AddChild(new Cylinder(new Theme(Color.Blue)));
        torso.Axis = Axis.Y;
        torso.Width= 1.0f;
        torso.Depth = 1.0f;
        torso.Height = 1.0f;
        torso.SetWorldSide(Side.Top, this.GetWorldSide(Side.Top));

        var head = AddChild(new Ellipsoid(0.5f));
        head.Depth = 0.6f;
        head.LocalPosition = LocalPosition;
        head.SetWorldSide(Side.Bottom, this.GetWorldSide(Side.Top));

        WorldPosition = new Vector3(0f, 2f, 3f);

        LeftShoulder = AddChild(new Shoulder(this));
        LeftShoulder.WorldPosition = WorldPosition;
        LeftShoulder.Place().OnSideOuter(Side.Top, this);
        LeftShoulder.LocalX += 0.5f;
        LeftShoulder.LocalY -= 0.5f;
        LeftShoulder.Tag = "LeftShoulder";

        RightShoulder = AddChild(new Shoulder(this));
        RightShoulder.WorldPosition = WorldPosition;
        RightShoulder.Place().OnSideOuter(Side.Top, this);
        RightShoulder.LocalX -= 0.5f;
        RightShoulder.LocalY -= 0.5f;
        RightShoulder.Tag = "RightShoulder";

        // todo, need cleaner way for dependency between moving object and parts
        // X = 2f;
        //    Y = 1f;

        LeftArm = AddChild(new BasicArm(worldSegment, this, LeftShoulder, 0.2f, 1.0f, 1.0f));
        RightArm = AddChild(new BasicArm(worldSegment, this, RightShoulder, 0.2f, 1.0f, 1.0f));
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

