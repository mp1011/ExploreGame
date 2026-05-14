using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Services;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes.Appliances;

public class EndTableLamp : LampBase
{
    public EndTableLamp(Room room, StateKey stateKey) : base(room, stateKey, 
        width: Measure.Inches(10),
        depth: Measure.Inches(10),
        height: Measure.Inches(20))
    {
    }

    protected override LightBulb CreateBulb(Room room, StateKey stateKey)
    {
        var bulb = new LightBulb(room, this, stateKey,
            diameter: Measure.Inches(4),
            intensity: LightIntensity.IndoorLight,
            color: Color.White);
        bulb.Place().AtParent().OnSideOuter(Side.Top);
        return bulb;
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return TriangleMaker.BuildEllipsoid(this, 16);
    }

}
