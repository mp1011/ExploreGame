using ExploringGame.Logics;
using ExploringGame.Logics.ShapeControllers;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes.Furniture;

public class Door : PlaceableShape, IPlaceableObject, IControllable
{
    private float _yGap = Measure.Inches(0.2f);

    public Angle ClosedDegrees { get; }
    public Angle OpenDegrees { get; }
    public float OpenSpeed { get; } = 2.0f;

    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public Vector3 Hinge { get; set; }

    public Angle Angle
    {
        get => new Angle(Rotation.YawDegrees);
        set => Rotation = Rotation.YawFromDegrees(value.Degrees, Rotation.Pitch, Rotation.Roll);
    }
    
    public bool Open { get; set; }

    public Door(Shape parent, Angle closedDegrees, Angle openDegrees)
    {
        OpenDegrees = openDegrees;
        ClosedDegrees = closedDegrees;

        parent.AddChild(this);

        Width = Measure.Inches(30.5f);
        Depth = Measure.Inches(1.0f);
        Height = parent.Height - _yGap * 2;

        MainTexture = new TextureInfo(Key: TextureKey.Ceiling);
        Rotation = Rotation.YawFromDegrees(190);
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
