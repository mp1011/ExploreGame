using ExploringGame.Logics;
using Microsoft.Xna.Framework;
using System;

namespace ExploringGame.GeometryBuilder.Shapes;

public class OutdoorAmbientLight : Shape, ILightSource
{
    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public float Intensity { get; set; } = LightIntensity.VeryDim;
    public Color Color { get; set; } = Color.LightBlue;

    public Vector3 LightPosition => Position;

    public bool On { get; set; } = true;

    public Room Room { get; }

    public OutdoorAmbientLight(Room room)
    {
        Room = room;
        room.AddChild(this);
        Position = room.Position;
        Size = Vector3.One;
        Y += 100;
    }

    public event EventHandler<LightStateChangedEventArgs> StateChanged;

    protected override void BeforeBuild()
    {
        StateChanged?.Invoke(this, new LightStateChangedEventArgs(true));
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return BuildCuboid();
    }
}
