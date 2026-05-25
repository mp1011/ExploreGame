using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Logics.ShapeControllers;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System;

namespace ExploringGame.GeometryBuilder.Shapes.Appliances;

public class LightBulb : ShapePart, IControllable<LightController<LightBulb>>, IOnOff, ILightSource
{
    public override Theme Theme { get; } = new Theme(Color.White);

    public LightBulb(IRoom room, Shape parent, StateKey key, float diameter, float intensity, Color color)
    {
        parent.AddChild(this);

        Intensity = intensity;
        Color = color; 

        Room = room;
        StateKey = key;

        Width = diameter;
        Height = diameter;
        Depth = diameter;
    }

    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public LightController<LightBulb> Controller { get; private set; }

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

    public StateKey StateKey { get; }

    public float Intensity { get; set; }
    public Color Color { get; set; }

    public Vector3 LightPosition => Parent.Position + Position;

    public IRoom Room { get; }

    public event EventHandler<LightStateChangedEventArgs> StateChanged;

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        //hack
        if (Width < Measure.Inches(1))
            return BuildCuboid();

        return TriangleMaker.BuildEllipsoid(this, 8);
    }

    public IActiveObject CreateController(ServiceContainer serviceContainer)
    {
        var controller = serviceContainer.Get<LightController<LightBulb>>();
        controller.Shape = this;
        Controller = controller;
        On = _on; // Apply the stored state
        return controller;
    }

    public override string ToString()
    {
        return $"LightBulb ({Parent})";
    }
}
