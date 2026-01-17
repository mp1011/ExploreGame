using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Logics.ShapeControllers;
using ExploringGame.Texture;

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
    
    public bool Open { get; set; }

    public StateKey StateKey { get; }

    public Door(Shape parent, Angle closedDegrees, Angle openDegrees, HAlign hingeSide, StateKey stateKey)
    {
        StateKey = stateKey;
        HingePosition = hingeSide;
        OpenAngle = openDegrees;
        ClosedAngle = closedDegrees;

        parent.AddChild(this);

        // default door angle is west
        Width = StandardWidth;
        Depth = Measure.Inches(1.0f);

        Height = parent.Height - _yGap * 2;

        MainTexture = new TextureInfo(Key: TextureKey.Ceiling);
        Rotation = Rotation.YawFromDegrees(closedDegrees.Degrees);
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
}
