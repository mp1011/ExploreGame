using ExploringGame.Extensions;
using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Logics.ShapeControllers;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes.Furniture;


public class Door : PlaceableShape, IPlaceableObject, IControllable<DoorController>
{
    private float _yGap = Measure.Inches(0.2f);

    public static float StandardWidth => Measure.Inches(30.5f);

    public Angle ClosedAngle { get; }
    public Angle OpenAngle { get; }
    public HAlign HingePosition { get; }

    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public override IColliderMaker ColliderMaker => new DoorColliderMaker(this);
    public override CollisionGroup CollisionGroup => CollisionGroup.Environment;
    public override CollisionGroup CollidesWithGroups => CollisionGroup.Player | CollisionGroup.SolidEntity;
    public bool Open { get; set; }

    public StateKey StateKey { get; }

    public Door(Shape parent, Side wallSide, HAlign hingePosition, DoorDirection doorDirection, StateKey stateKey)
    {

        Angle doorOpen, doorClose;

        var openMod = doorDirection == DoorDirection.Pull ? 1 : -1;

        if (hingePosition == HAlign.Left)
        {
            doorClose = new Angle(wallSide).RotateClockwise(90);
            doorOpen = doorClose.RotateClockwise(90 * openMod);
        }
        else
        {
            doorClose = new Angle(wallSide).RotateCounterClockwise(90);
            doorOpen = doorClose.RotateCounterClockwise(90 * openMod);
        }

        StateKey = stateKey;
        HingePosition = hingePosition;
        OpenAngle = doorOpen;
        ClosedAngle = doorClose;

        parent.AddChild(this);

        // default door angle is west
        Width = StandardWidth;
        Depth = Measure.Inches(1.0f);

        Height = parent.Height - _yGap * 2;

        MainTexture = new TextureInfo(Key: TextureKey.Ceiling);
        Rotation = Rotation.YawFromDegrees(doorClose.Degrees);
    }
    

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return BuildCuboid();
    }

    public DoorController Controller { get; private set; }
    public IActiveObject CreateController(ServiceContainer serviceContainer)
    {
        var controller = serviceContainer.Get<DoorController>();
        controller.Shape = this;
        Controller = controller;
        return controller;
    }

    public void SetHingePosition(Vector3 newHingePosition)
    {
        var hingeX = HingePosition == HAlign.Left ? GetSide(Side.West) : GetSide(Side.East);
        var currentHingePosition = Position.SetX(hingeX);

        var delta = newHingePosition - currentHingePosition;
        Position = Position + delta;
    }
}
