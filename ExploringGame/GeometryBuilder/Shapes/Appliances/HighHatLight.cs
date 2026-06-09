using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Logics.ShapeControllers;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System;

namespace ExploringGame.GeometryBuilder.Shapes.Appliances;

public class HighHatLight : Shape, ICutoutShape, IControllable<LightController<HighHatLight>>, IOnOff, ILightSource
{

    public override Theme Theme => new Theme(Color.White);
    public override ViewFrom ViewFrom => ViewFrom.Inside;

    public Side ParentCutoutSide => Side.Top;

    public float Intensity { get; set; } = LightIntensity.IndoorLight;

    public Color Color { get; set; } = Color.White;

    public IRoom Room { get; }

    public Vector3 LightPosition => LocalPosition + new Vector3(0, -Height / 2f, 0);

    public event EventHandler<LightStateChangedEventArgs> StateChanged;

    Triangle[] ICutoutShape.Build() => BuildInternal(QualityLevel.Basic);

    public HighHatLight(Room room, float x, float z, bool initialState=false)
    {
        Room = room;
        _on = initialState;
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

    public LightController<HighHatLight> Controller { get; private set; }


    private bool _on;
    public bool On 
    { 
        get => Controller?.On ?? _on; 
        set 
        {
            var oldValue = On;
            _on = value;

            if (Controller != null)
                Controller.On = value;

            if (oldValue != value)
                StateChanged?.Invoke(this, new LightStateChangedEventArgs(value));
        } 
    }

    public StateKey StateKey => StateKey.None;

    public IActiveObject CreateController(ServiceContainer serviceContainer)
    {
        var controller = serviceContainer.Get<LightController<HighHatLight>>();
        controller.Shape = this;
        Controller = controller;
        On = _on; // Apply the stored state
        On = true; //temporary
        return controller;
    }
}
