using ExploringGame.Logics;
using ExploringGame.Logics.Collision.ColliderMakers;
using ExploringGame.Logics.ShapeControllers;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes.Furniture;

public enum HingePosition
{
    Left,
    Right
};

public class Door : PlaceableShape, IPlaceableObject, IControllable
{
    private float _yGap = Measure.Inches(0.2f);

    public Angle ClosedAngle { get; }
    public Angle OpenAngle { get; }
    public HingePosition HingePosition { get; }

    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public override IColliderMaker ColliderMaker => new DoorColliderMaker(this);
    
    public bool Open { get; set; }

    public Door(Shape parent, Angle closedDegrees, Angle openDegrees, HingePosition hingeSide)
    {
        HingePosition = hingeSide;
        OpenAngle = openDegrees;
        ClosedAngle = closedDegrees;

        parent.AddChild(this);

        // default door angle is west
        Width = Measure.Inches(30.5f);
        Depth = Measure.Inches(1.0f);

        Height = parent.Height - _yGap * 2;

        MainTexture = new TextureInfo(Key: TextureKey.Ceiling);
        Rotation = Rotation.YawFromDegrees(closedDegrees.Degrees);
    }
    

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return BuildCuboid();
    }

    public IActiveObject CreateController(ServiceContainer serviceContainer)
    {
        var controller = serviceContainer.Get<DoorController>();
        controller.Shape = this;
        return controller;
    }
}
