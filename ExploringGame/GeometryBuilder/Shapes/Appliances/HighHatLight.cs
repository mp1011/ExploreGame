using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Logics.ShapeControllers;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes.Appliances;

public class HighHatLight : Shape, ICutoutShape, IControllable<LightController>, IOnOff
{
    private bool _initialState;

    public override Theme Theme => new Theme(Color.White);
    public override ViewFrom ViewFrom => ViewFrom.Inside;

    public Side ParentCutoutSide => Side.Top;

    public float Intensity => 0.6f;

    Triangle[] ICutoutShape.Build() => BuildInternal(QualityLevel.Basic);

    public Vector3 RangeMin => new Vector3(Position.X - 50f, Position.Y - Parent.Height - 0.5f, Position.Z - 50f);

    public Vector3 RangeMax => new Vector3(Position.X + 50f, Position.Y + 0.5f, Position.Z + 50f);

    public HighHatLight(Room room, float x, float z, bool initialState=false)
    {
        _initialState = initialState;
        X = room.X + x;
        Y = room.Y;
        Z = room.Z + z;
        room.AddChild(this);

        Height = 0.1f;
        Width = 0.5f;
        Depth = 0.5f;

        this.Place().OnSideOuter(Side.Top, room);
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return TriangleMaker.BuildCylinder(this, detail: 20, Axis.Y);
    }
    
    public LightController Controller { get; private set; }
    public bool On { get => Controller.On; set => Controller.On = value; }

    public StateKey StateKey => StateKey.None;

    public IActiveObject CreateController(ServiceContainer serviceContainer)
    {
        var controller = serviceContainer.Get<LightController>();
        controller.Shape = this;
        Controller = controller;
        On = _initialState;
        return controller;
    }
}
