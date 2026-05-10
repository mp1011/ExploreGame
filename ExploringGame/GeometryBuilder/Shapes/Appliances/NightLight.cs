using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Services;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes.Appliances;

public class NightLight : LampBase
{
    public NightLight(Room room, StateKey stateKey) : base(room,  stateKey,
        width: Measure.Inches(6),
        depth: Measure.Inches(6),
        height: Measure.Inches(10))
    {
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return TriangleMaker.BuildCylinder(this, 16, Axis.Y);
    }

    protected override LightBulb CreateBulb(Room room, StateKey stateKey)
    {
        var bulb = new LightBulb(room, this, stateKey,
            diameter: Measure.Inches(2),
            intensity: LightIntensity.Dim,
            color: Color.LightBlue);

        bulb.Place().AtParent();
        return bulb;
    }
}
