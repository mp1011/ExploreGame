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

public class Puppet : PlaceableShape, IControllable<PuppetController>, ICollidable, IPhysicsShape
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

    public PuppetController Controller { get; private set; }

    public Puppet(WorldSegment worldSegment, float sizeScale)
    {
     
        ColliderMaker = new SphereColliderMaker(this);
        Size = new Vector3(1.0f, 2.0f, 1.0f) * sizeScale;

        var torso = AddChild(new Cylinder(new Theme(Color.Blue)));
        torso.Axis = Axis.Y;
        torso.Width= 1.0f;
        torso.Depth = 1.0f;
        torso.Height = 1.0f;
        torso.Size *= sizeScale;
        torso.SetWorldSide(Side.Top, this.GetWorldSide(Side.Top));

        var head = AddChild(new Ellipsoid(0.5f));
        head.Depth = 0.6f;
        head.Size *= sizeScale;
        head.LocalPosition = LocalPosition;
        head.SetWorldSide(Side.Bottom, this.GetWorldSide(Side.Top));

        WorldPosition = new Vector3(0f, 2f, 3f);

        LeftShoulder = AddChild(new Shoulder(this, sizeScale));
        LeftShoulder.WorldPosition = WorldPosition;
        LeftShoulder.Place().OnSideOuter(Side.Top, this);
        LeftShoulder.LocalX += 0.5f * sizeScale;
        LeftShoulder.LocalY -= 0.5f * sizeScale;
        LeftShoulder.Tag = "LeftShoulder";

        RightShoulder = AddChild(new Shoulder(this, sizeScale));
        RightShoulder.WorldPosition = WorldPosition;
        RightShoulder.Place().OnSideOuter(Side.Top, this);
        RightShoulder.LocalX -= 0.5f * sizeScale;
        RightShoulder.LocalY -= 0.5f * sizeScale;
        RightShoulder.Tag = "RightShoulder";

        LeftArm = AddChild(new BasicArm(worldSegment, this, LeftShoulder, 0.2f * sizeScale, 1.0f * sizeScale, 1.0f * sizeScale));
        RightArm = AddChild(new BasicArm(worldSegment, this, RightShoulder, 0.2f * sizeScale, 1.0f * sizeScale, 1.0f * sizeScale));
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return TriangleMaker.BuildEllipsoid(this, 8);
    }

    public IActiveObject CreateController(ServiceContainer serviceContainer)
    {
        Controller = serviceContainer.Get<PuppetController>();
        Controller.Puppet = this;
        return Controller;
        
    }
}

