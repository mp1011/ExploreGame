using ExploringGame.Logics;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace ExploringGame.GeometryBuilder.Shapes.Furniture;

public class Door : PlaceableShape, IActiveObject
{
    private float _yGap = Measure.Inches(0.2f);

    private float _closedDegrees, _openDegrees;
    private float _openSpeed = 1.0f;

    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public Vector3 Hinge { get; set; }

    public Angle Angle
    {
        get => new Angle(Rotation.YawDegrees);
        set => Rotation = Rotation.YawFromDegrees(value.Degrees, Rotation.Pitch, Rotation.Roll);
    }
    
    public bool Open { get; set; }

    public Door(Shape parent, float closedDegrees, float openDegrees)
    {
        _openDegrees = openDegrees;
        _closedDegrees = closedDegrees;

        parent.AddChild(this);

        Width = Measure.Inches(30.5f);
        Depth = Measure.Inches(1.0f);
        Height = parent.Height - _yGap * 2;

        MainTexture = new TextureInfo(Key: TextureKey.Ceiling);
        Rotation = Rotation.YawFromDegrees(190);
    }

    public void Update(GameTime gameTime)
    {
        var k = Keyboard.GetState();
        if (k.IsKeyDown(Keys.O))
            Open = true;
        else if (k.IsKeyDown(Keys.C))
            Open = false;

        var targetDegrees = Open ? _openDegrees : _closedDegrees;
        AdjustAngle(targetDegrees);

        PlaceDoor();
    }

    private void AdjustAngle(float targetDegrees)
    {
        if (Angle.Degrees == targetDegrees)
            return;

        Angle = Angle.RotateTowards(targetDegrees, _openSpeed);
    }

    private void PlaceDoor()
    {
        Vector3 d = new Vector3(Width/2.0f, 0, 0);
        d = Vector3.Transform(d, Rotation.AsMatrix());
        Position = Hinge - d;
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return BuildCuboid();
    }
}
