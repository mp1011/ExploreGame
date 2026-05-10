using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Logics.ShapeControllers;
using ExploringGame.Services;
using Microsoft.Xna.Framework;
using System;

namespace ExploringGame.GeometryBuilder.Shapes.Appliances;

public class LightBulb : ShapePart, IControllable<LightController<LightBulb>>, IOnOff, ILightSource
{
    public LightBulb(Room room, Shape parent, StateKey key, float diameter, float intensity, Color color)
    {
        parent.AddChild(this);

        Intensity = intensity;
        Color = color; 

        Room = room;
        StateKey = key;

        Width = Measure.Inches(diameter);
        Height = Measure.Inches(diameter);
        Depth = Measure.Inches(diameter);
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

    public Room Room { get; }

    public event EventHandler<LightStateChangedEventArgs> StateChanged;

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return TriangleMaker.BuildEllipsoid(this);
    }

    public IActiveObject CreateController(ServiceContainer serviceContainer)
    {
        var controller = serviceContainer.Get<LightController<LightBulb>>();
        controller.Shape = this;
        Controller = controller;
        On = _on; // Apply the stored state
        return controller;
    }
}
