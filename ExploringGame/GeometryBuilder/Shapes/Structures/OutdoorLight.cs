using ExploringGame.GeometryBuilder.Shapes.Appliances;
using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes.Structures;

public class OutdoorLight : LampBase
{
    public readonly float BulbSize = Measure.Inches(4);

    public OutdoorLight(Room room, StateKey stateKey) 
        : base(room, stateKey, width: Measure.Inches(6), depth: Measure.Inches(6), height: Measure.Inches(12))
    {
    }

    public override Theme Theme => new BasicFurnitureTheme();

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return BuildCuboid();
    }

    protected override LightBulb CreateBulb(Room room, StateKey stateKey)
    {
        var bulb = new LightBulb(Room, this, StateKey, BulbSize, LightIntensity.Bright, Color.White);
        bulb.Place().AtParent();
        return bulb;
    }
}
