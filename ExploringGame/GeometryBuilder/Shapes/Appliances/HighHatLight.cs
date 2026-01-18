using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Logics.ShapeControllers;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes.Appliances;

public class HighHatLight : Shape, ICutoutShape, IControllable<LightController>, IOnOff
{
    public override Theme Theme => new Theme(Color.White);
    public override ViewFrom ViewFrom => ViewFrom.Inside;

    public Side ParentCutoutSide => Side.Top;

    Triangle[] ICutoutShape.Build() => BuildInternal(QualityLevel.Basic);

    public HighHatLight(Room room, float x, float z)
    {
        X = x;
        Y = room.Y;
        Z = z;
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
        return controller;
    }
}
