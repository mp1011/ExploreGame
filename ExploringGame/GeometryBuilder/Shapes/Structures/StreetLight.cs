using ExploringGame.GeometryBuilder.Shapes.Appliances;
using ExploringGame.LevelControl;
using ExploringGame.Logics;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.GeometryBuilder.Shapes.Structures;

public class StreetLight : LampBase
{
    public static readonly float PoleDiameter = Measure.Feet(1);
    public static readonly float PoleHeight = Measure.Feet(30);
    public static readonly float BulbDiameter = Measure.Feet(2);

    public override Theme Theme => new BasicFurnitureTheme(Color.DarkGray);

    public StreetLight(Room room) : base(room, StateKey.StreetLightsOn, PoleDiameter, PoleDiameter, PoleHeight)
    {
        this.Place().AtParent().OnFloor();
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        return TriangleMaker.BuildCylinder(this, 16, Axis.Y);
    }

    protected override LightBulb CreateBulb(Room room, StateKey stateKey)
    {
        var bulb = new LightBulb(Room, this, StateKey, BulbDiameter, LightIntensity.Bright, Color.LightYellow);
        bulb.Place().AtParent().OnSideOuter(Side.Top);
        return bulb;
    }
}
